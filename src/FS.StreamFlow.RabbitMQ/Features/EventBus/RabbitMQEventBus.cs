using FS.StreamFlow.Core.Features.Events.Interfaces;
using FS.StreamFlow.Core.Features.Events.Models;
using FS.StreamFlow.Core.Features.Messaging.Interfaces;
using FS.StreamFlow.Core.Features.Messaging.Models;
using Microsoft.Extensions.Logging;
using System.Collections.Concurrent;

namespace FS.StreamFlow.RabbitMQ.Features.EventBus;

/// <summary>
/// RabbitMQ implementation of event bus providing comprehensive event-driven architecture with domain events, 
/// integration events, automatic routing, retry policies, and enterprise-grade monitoring capabilities
/// </summary>
public class RabbitMQEventBus : IEventBus
{
    private readonly IConnectionManager _connectionManager;
    private readonly IProducer _producer;
    private readonly IConsumer _consumer;
    private readonly ILogger<RabbitMQEventBus> _logger;
    private readonly ConcurrentDictionary<Type, List<IAsyncEventHandler<IEvent>>> _eventHandlers = new();
    private readonly ConcurrentDictionary<Type, List<Func<IEvent, EventContext, Task<bool>>>> _eventSubscriptions = new();
    private readonly EventBusStatistics _statistics;
    private readonly object _lockObject = new();
    private volatile EventBusStatus _status = EventBusStatus.NotInitialized;
    private volatile bool _disposed = false;

    /// <summary>
    /// Initializes a new instance of the RabbitMQEventBus class
    /// </summary>
    /// <param name="connectionManager">Connection manager for RabbitMQ operations</param>
    /// <param name="producer">Message producer for publishing events</param>
    /// <param name="consumer">Message consumer for receiving events</param>
    /// <param name="logger">Logger instance for diagnostic information</param>
    /// <exception cref="ArgumentNullException">Thrown when any parameter is null</exception>
    public RabbitMQEventBus(
        IConnectionManager connectionManager,
        IProducer producer,
        IConsumer consumer,
        ILogger<RabbitMQEventBus> logger)
    {
        _connectionManager = connectionManager ?? throw new ArgumentNullException(nameof(connectionManager));
        _producer = producer ?? throw new ArgumentNullException(nameof(producer));
        _consumer = consumer ?? throw new ArgumentNullException(nameof(consumer));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        
        _statistics = new EventBusStatistics
        {
            Name = "RabbitMQEventBus",
            Status = EventBusStatus.NotInitialized
        };
        
        _logger.LogInformation("RabbitMQ Event Bus initialized successfully");
    }

    /// <summary>
    /// Gets the current status of the event bus
    /// </summary>
    public EventBusStatus Status => _status;

    /// <summary>
    /// Gets event bus statistics and metrics
    /// </summary>
    public EventBusStatistics Statistics => _statistics;

    /// <summary>
    /// Event raised when an event is published
    /// </summary>
    public event EventHandler<EventPublishedEventArgs>? EventPublished;

    /// <summary>
    /// Event raised when an event is received
    /// </summary>
    public event EventHandler<EventReceivedEventArgs>? EventReceived;

    /// <summary>
    /// Event raised when event processing fails
    /// </summary>
    public event EventHandler<EventProcessingFailedEventArgs>? EventProcessingFailed;

    /// <summary>
    /// Starts the event bus and initializes connections
    /// </summary>
    /// <param name="cancellationToken">Cancellation token for operation cancellation</param>
    /// <returns>Task representing the start operation</returns>
    /// <exception cref="ObjectDisposedException">Thrown when the event bus has been disposed</exception>
    public async Task StartAsync(CancellationToken cancellationToken = default)
    {
        ThrowIfDisposed();
        
        if (_status == EventBusStatus.Running)
            return;

        try
        {
            ChangeStatus(EventBusStatus.Starting);
            _logger.LogInformation("Starting RabbitMQ Event Bus");

            // Initialize producer and consumer
            await _producer.InitializeAsync(cancellationToken);
            await _consumer.StartAsync(cancellationToken);
            
            // Subscribe to consumer events
            _consumer.MessageReceived += OnMessageReceived;
            _consumer.MessageProcessingFailed += OnMessageProcessingFailed;
            
            _statistics.StartedAt = DateTimeOffset.UtcNow;
            _statistics.IsConnected = _connectionManager.IsConnected;
            ChangeStatus(EventBusStatus.Running);
            
            _logger.LogInformation("RabbitMQ Event Bus started successfully");
        }
        catch (Exception ex)
        {
            ChangeStatus(EventBusStatus.Faulted);
            _statistics.LastErrorMessage = ex.Message;
            _statistics.LastErrorAt = DateTimeOffset.UtcNow;
            _logger.LogError(ex, "Failed to start RabbitMQ Event Bus");
            throw;
        }
    }

    /// <summary>
    /// Stops the event bus and releases resources
    /// </summary>
    /// <param name="cancellationToken">Cancellation token for operation cancellation</param>
    /// <returns>Task representing the stop operation</returns>
    /// <exception cref="ObjectDisposedException">Thrown when the event bus has been disposed</exception>
    public async Task StopAsync(CancellationToken cancellationToken = default)
    {
        ThrowIfDisposed();
        
        if (_status == EventBusStatus.Stopped)
            return;

        try
        {
            ChangeStatus(EventBusStatus.Stopping);
            _logger.LogInformation("Stopping RabbitMQ Event Bus");

            // Unsubscribe from consumer events
            _consumer.MessageReceived -= OnMessageReceived;
            _consumer.MessageProcessingFailed -= OnMessageProcessingFailed;
            
            // Stop consumer
            await _consumer.StopAsync(cancellationToken);
            
            _statistics.IsConnected = false;
            ChangeStatus(EventBusStatus.Stopped);
            
            _logger.LogInformation("RabbitMQ Event Bus stopped successfully");
        }
        catch (Exception ex)
        {
            ChangeStatus(EventBusStatus.Faulted);
            _statistics.LastErrorMessage = ex.Message;
            _statistics.LastErrorAt = DateTimeOffset.UtcNow;
            _logger.LogError(ex, "Failed to stop RabbitMQ Event Bus");
            throw;
        }
    }

    /// <summary>
    /// Publishes a domain event to the event bus
    /// </summary>
    /// <typeparam name="T">Domain event type</typeparam>
    /// <param name="domainEvent">Domain event to publish</param>
    /// <param name="cancellationToken">Cancellation token for operation cancellation</param>
    /// <returns>Task representing the publish operation</returns>
    /// <exception cref="ArgumentNullException">Thrown when domainEvent is null</exception>
    /// <exception cref="ObjectDisposedException">Thrown when the event bus has been disposed</exception>
    public async Task PublishDomainEventAsync<T>(T domainEvent, CancellationToken cancellationToken = default) where T : class, IDomainEvent
    {
        if (domainEvent == null)
            throw new ArgumentNullException(nameof(domainEvent));

        ThrowIfDisposed();
        EnsureRunning();

        var context = new EventPublishContext
        {
            EventType = domainEvent.EventType,
            EventId = domainEvent.Id.ToString(),
            CorrelationId = domainEvent.CorrelationId,
            CausationId = domainEvent.CausationId,
            Timestamp = DateTimeOffset.UtcNow,
            Version = domainEvent.Version,
            Source = $"Domain.{domainEvent.AggregateType}",
            Subject = domainEvent.AggregateId,
            Options = new PublishOptions
            {
                Exchange = "domain-events",
                RoutingKey = $"domain.{domainEvent.AggregateType.ToLowerInvariant()}.{domainEvent.EventType.ToLowerInvariant()}",
                Mandatory = true,
                Properties = new MessageProperties
                {
                    DeliveryMode = DeliveryMode.Persistent
                }
            }
        };

        await PublishEventInternalAsync(domainEvent, context, cancellationToken);
    }

    /// <summary>
    /// Publishes an integration event to the event bus
    /// </summary>
    /// <typeparam name="T">Integration event type</typeparam>
    /// <param name="integrationEvent">Integration event to publish</param>
    /// <param name="cancellationToken">Cancellation token for operation cancellation</param>
    /// <returns>Task representing the publish operation</returns>
    /// <exception cref="ArgumentNullException">Thrown when integrationEvent is null</exception>
    /// <exception cref="ObjectDisposedException">Thrown when the event bus has been disposed</exception>
    public async Task PublishIntegrationEventAsync<T>(T integrationEvent, CancellationToken cancellationToken = default) where T : class, IIntegrationEvent
    {
        if (integrationEvent == null)
            throw new ArgumentNullException(nameof(integrationEvent));

        ThrowIfDisposed();
        EnsureRunning();

        var context = new EventPublishContext
        {
            EventType = integrationEvent.EventType,
            EventId = integrationEvent.Id.ToString(),
            CorrelationId = integrationEvent.CorrelationId,
            CausationId = integrationEvent.CausationId,
            Timestamp = DateTimeOffset.UtcNow,
            Version = integrationEvent.Version,
            Source = integrationEvent.Source,
            Subject = integrationEvent.Target,
            Options = new PublishOptions
            {
                Exchange = "integration-events",
                RoutingKey = integrationEvent.RoutingKey,
                Mandatory = true,
                Properties = new MessageProperties
                {
                    DeliveryMode = DeliveryMode.Persistent,
                    Expiration = integrationEvent.TimeToLive?.TotalMilliseconds.ToString()
                }
            }
        };

        await PublishEventInternalAsync(integrationEvent, context, cancellationToken);
    }

    /// <summary>
    /// Publishes multiple events in a batch
    /// </summary>
    /// <param name="events">Events to publish</param>
    /// <param name="cancellationToken">Cancellation token for operation cancellation</param>
    /// <returns>Task representing the batch publish operation</returns>
    /// <exception cref="ArgumentNullException">Thrown when events is null</exception>
    /// <exception cref="ObjectDisposedException">Thrown when the event bus has been disposed</exception>
    public async Task PublishBatchAsync(IEnumerable<IEvent> events, CancellationToken cancellationToken = default)
    {
        if (events == null)
            throw new ArgumentNullException(nameof(events));

        ThrowIfDisposed();
        EnsureRunning();

        var eventList = events.ToList();
        if (eventList.Count == 0)
            return;

        _logger.LogInformation("Publishing batch of {EventCount} events", eventList.Count);

        var tasks = eventList.Select(async eventItem =>
        {
            try
            {
                switch (eventItem)
                {
                    case IDomainEvent domainEvent:
                        await PublishDomainEventAsync(domainEvent, cancellationToken);
                        break;
                    case IIntegrationEvent integrationEvent:
                        await PublishIntegrationEventAsync(integrationEvent, cancellationToken);
                        break;
                    default:
                        _logger.LogWarning("Unknown event type: {EventType}", eventItem.GetType());
                        break;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to publish event in batch: {EventId}", eventItem.Id);
                throw;
            }
        });

        await Task.WhenAll(tasks);
        _logger.LogInformation("Successfully published batch of {EventCount} events", eventList.Count);
    }

    /// <summary>
    /// Subscribes to domain events of a specific type
    /// </summary>
    /// <typeparam name="T">Domain event type</typeparam>
    /// <param name="handler">Event handler</param>
    /// <param name="cancellationToken">Cancellation token for operation cancellation</param>
    /// <returns>Task representing the subscription operation</returns>
    /// <exception cref="ArgumentNullException">Thrown when handler is null</exception>
    /// <exception cref="ObjectDisposedException">Thrown when the event bus has been disposed</exception>
    public async Task SubscribeToDomainEventAsync<T>(IAsyncEventHandler<T> handler, CancellationToken cancellationToken = default) where T : class, IDomainEvent
    {
        if (handler == null)
            throw new ArgumentNullException(nameof(handler));

        ThrowIfDisposed();
        EnsureRunning();

        var eventType = typeof(T);
        
        _eventHandlers.AddOrUpdate(eventType, 
            new List<IAsyncEventHandler<IEvent>> { handler as IAsyncEventHandler<IEvent> }, 
            (key, existing) => { existing.Add(handler as IAsyncEventHandler<IEvent>); return existing; });

        _statistics.ActiveSubscriptions++;
        _logger.LogInformation("Subscribed to domain event: {EventType}", eventType.Name);
        
        // Start consuming from the domain event queue
        var queueName = $"domain-events-{eventType.Name}";
        await _consumer.ConsumeAsync<T>(queueName, async (evt, context) =>
        {
            try
            {
                await handler.HandleAsync(evt, new EventContext
                {
                    EventType = eventType.Name,
                    EventId = Guid.NewGuid().ToString(),
                    Timestamp = DateTimeOffset.UtcNow,
                    Source = "EventBus"
                }, CancellationToken.None);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error handling domain event {EventType}", eventType.Name);
                return false;
            }
        }, new ConsumerContext
        {
            ConsumerTag = queueName,
            Settings = new ConsumerSettings { AutoAcknowledge = false }
        }, CancellationToken.None);
    }

    /// <summary>
    /// Subscribes to integration events of a specific type
    /// </summary>
    /// <typeparam name="T">Integration event type</typeparam>
    /// <param name="handler">Event handler</param>
    /// <param name="cancellationToken">Cancellation token for operation cancellation</param>
    /// <returns>Task representing the subscription operation</returns>
    /// <exception cref="ArgumentNullException">Thrown when handler is null</exception>
    /// <exception cref="ObjectDisposedException">Thrown when the event bus has been disposed</exception>
    public async Task SubscribeToIntegrationEventAsync<T>(IAsyncEventHandler<T> handler, CancellationToken cancellationToken = default) where T : class, IIntegrationEvent
    {
        if (handler == null)
            throw new ArgumentNullException(nameof(handler));

        ThrowIfDisposed();
        EnsureRunning();

        var eventType = typeof(T);
        var routingKey = "integration.*";
        
        _eventHandlers.AddOrUpdate(eventType, 
            new List<IAsyncEventHandler<IEvent>> { handler as IAsyncEventHandler<IEvent> }, 
            (key, existing) => { existing.Add(handler as IAsyncEventHandler<IEvent>); return existing; });

        await _consumer.ConsumeIntegrationEventAsync<T>(
            "integration-service",
            async (evt, eventContext) =>
            {
                await handler.HandleAsync(evt, eventContext, cancellationToken);
                return true;
            },
            cancellationToken);

        _statistics.ActiveSubscriptions++;
        _logger.LogInformation("Subscribed to integration event: {EventType} with routing key: {RoutingKey}", eventType.Name, routingKey);
    }

    /// <summary>
    /// Subscribes to events with a custom handler function
    /// </summary>
    /// <typeparam name="T">Event type</typeparam>
    /// <param name="handler">Handler function</param>
    /// <param name="cancellationToken">Cancellation token for operation cancellation</param>
    /// <returns>Task representing the subscription operation</returns>
    /// <exception cref="ArgumentNullException">Thrown when handler is null</exception>
    /// <exception cref="ObjectDisposedException">Thrown when the event bus has been disposed</exception>
    public async Task SubscribeAsync<T>(Func<T, EventContext, Task<bool>> handler, CancellationToken cancellationToken = default) where T : class, IEvent
    {
        if (handler == null)
            throw new ArgumentNullException(nameof(handler));

        ThrowIfDisposed();
        EnsureRunning();

        var eventType = typeof(T);
        
        _eventSubscriptions.AddOrUpdate(eventType, 
            new List<Func<IEvent, EventContext, Task<bool>>> { (evt, ctx) => handler((T)evt, ctx) }, 
            (key, existing) => { existing.Add((evt, ctx) => handler((T)evt, ctx)); return existing; });

        await _consumer.ConsumeEventAsync<T>(
            "custom-events",
            "*",
            async (evt, eventContext) =>
            {
                return await handler(evt, eventContext);
            },
            cancellationToken);

        _statistics.ActiveSubscriptions++;
        _logger.LogInformation("Subscribed to event: {EventType} with custom handler", eventType.Name);
    }

    /// <summary>
    /// Unsubscribes from events of a specific type
    /// </summary>
    /// <typeparam name="T">Event type</typeparam>
    /// <param name="cancellationToken">Cancellation token for operation cancellation</param>
    /// <returns>Task representing the unsubscription operation</returns>
    /// <exception cref="ObjectDisposedException">Thrown when the event bus has been disposed</exception>
    public Task UnsubscribeAsync<T>(CancellationToken cancellationToken = default) where T : class, IEvent
    {
        ThrowIfDisposed();

        var eventType = typeof(T);
        
        _eventHandlers.TryRemove(eventType, out _);
        _eventSubscriptions.TryRemove(eventType, out _);
        
        if (_statistics.ActiveSubscriptions > 0)
            _statistics.ActiveSubscriptions--;

        // Remove event handlers for the specific type
        if (_eventHandlers.TryGetValue(eventType, out var handlers))
        {
            handlers.Clear();
        }

        _logger.LogInformation("Unsubscribed from event: {EventType}", eventType.Name);
        return Task.CompletedTask;
    }

    /// <summary>
    /// Handles message received events from the consumer
    /// </summary>
    /// <param name="sender">Event sender</param>
    /// <param name="e">Message received event arguments</param>
    private async void OnMessageReceived(object? sender, MessageReceivedEventArgs e)
    {
        try
        {
            if (e.Message is IEvent eventMessage)
            {
                var eventContext = CreateEventContext(eventMessage, e.Context, eventMessage is IDomainEvent, eventMessage is IIntegrationEvent);
                
                EventReceived?.Invoke(this, new EventReceivedEventArgs(eventMessage, e.Context.Exchange, e.Context.RoutingKey));
                
                _statistics.TotalEventsReceived++;
                _statistics.LastEventReceived = DateTimeOffset.UtcNow;
                
                await ProcessEventAsync(eventMessage, eventContext);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error handling received message");
        }
    }

    /// <summary>
    /// Handles message processing failed events from the consumer
    /// </summary>
    /// <param name="sender">Event sender</param>
    /// <param name="e">Message processing failed event arguments</param>
    private void OnMessageProcessingFailed(object? sender, MessageProcessingFailedEventArgs e)
    {
        try
        {
            if (e.Message is IEvent eventMessage)
            {
                EventProcessingFailed?.Invoke(this, new EventProcessingFailedEventArgs(
                    eventMessage, e.Exception, e.Context.Exchange, e.Context.RoutingKey, e.Context.AttemptCount, false));
                
                _statistics.TotalEventsFailedProcessing++;
                _statistics.LastErrorMessage = e.Exception.Message;
                _statistics.LastErrorAt = DateTimeOffset.UtcNow;
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error handling message processing failed event");
        }
    }

    /// <summary>
    /// Publishes an event with the specified context
    /// </summary>
    /// <param name="eventMessage">Event to publish</param>
    /// <param name="context">Event publish context</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Task representing the publish operation</returns>
    private async Task PublishEventInternalAsync(IEvent eventMessage, EventPublishContext context, CancellationToken cancellationToken)
    {
        try
        {
            var result = await _producer.PublishEventAsync(eventMessage, context, cancellationToken);
            
            _statistics.TotalEventsPublished++;
            _statistics.LastEventPublished = DateTimeOffset.UtcNow;
            
            EventPublished?.Invoke(this, new EventPublishedEventArgs(
                eventMessage, context.Exchange, context.RoutingKey, result.IsSuccess, result.ErrorMessage));
            
            if (result.IsSuccess)
            {
                _logger.LogDebug("Event published successfully: {EventId} ({EventType})", eventMessage.Id, eventMessage.EventType);
            }
            else
            {
                _logger.LogError("Failed to publish event: {EventId} ({EventType}). Error: {Error}", 
                    eventMessage.Id, eventMessage.EventType, result.ErrorMessage);
                throw new InvalidOperationException($"Failed to publish event: {result.ErrorMessage}");
            }
        }
        catch (Exception ex)
        {
            _statistics.TotalEventsFailedProcessing++;
            _statistics.LastErrorMessage = ex.Message;
            _statistics.LastErrorAt = DateTimeOffset.UtcNow;
            
            EventPublished?.Invoke(this, new EventPublishedEventArgs(
                eventMessage, context.Exchange, context.RoutingKey, false, ex.Message));
            
            _logger.LogError(ex, "Error publishing event: {EventId} ({EventType})", eventMessage.Id, eventMessage.EventType);
            throw;
        }
    }

    /// <summary>
    /// Processes an event by invoking registered handlers
    /// </summary>
    /// <param name="eventMessage">Event to process</param>
    /// <param name="eventContext">Event context</param>
    /// <returns>Task representing the processing operation</returns>
    private async Task ProcessEventAsync(IEvent eventMessage, EventContext eventContext)
    {
        var eventType = eventMessage.GetType();
        var processingStartTime = DateTimeOffset.UtcNow;
        
        try
        {
            eventContext.ProcessingStartedAt = processingStartTime;
            eventContext.ProcessingStatus = EventProcessingStatus.Processing;
            _statistics.CurrentlyProcessingEvents++;
            
            var processed = false;
            
            // Try event handlers first
            if (_eventHandlers.TryGetValue(eventType, out var handlers))
            {
                foreach (var handler in handlers)
                {
                    try
                    {
                        await handler.HandleAsync(eventMessage, eventContext);
                        processed = true;
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Event handler failed for event: {EventId} ({EventType})", 
                            eventMessage.Id, eventMessage.EventType);
                        eventContext.ProcessingError = ex;
                        throw;
                    }
                }
            }
            
            // Try event subscriptions
            if (_eventSubscriptions.TryGetValue(eventType, out var subscriptions))
            {
                foreach (var subscription in subscriptions)
                {
                    try
                    {
                        await subscription(eventMessage, eventContext);
                        processed = true;
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Event subscription failed for event: {EventId} ({EventType})", 
                            eventMessage.Id, eventMessage.EventType);
                        eventContext.ProcessingError = ex;
                        throw;
                    }
                }
            }
            
            eventContext.ProcessingCompletedAt = DateTimeOffset.UtcNow;
            eventContext.ProcessingStatus = processed ? EventProcessingStatus.Processed : EventProcessingStatus.Skipped;
            
            if (processed)
            {
                _statistics.TotalEventsProcessedSuccessfully++;
                UpdateProcessingMetrics(processingStartTime);
            }
        }
        catch (Exception ex)
        {
            eventContext.ProcessingCompletedAt = DateTimeOffset.UtcNow;
            eventContext.ProcessingStatus = EventProcessingStatus.Failed;
            eventContext.ProcessingError = ex;
            
            _statistics.TotalEventsFailedProcessing++;
            _statistics.LastErrorMessage = ex.Message;
            _statistics.LastErrorAt = DateTimeOffset.UtcNow;
            
            throw;
        }
        finally
        {
            _statistics.CurrentlyProcessingEvents--;
        }
    }

    /// <summary>
    /// Creates an event context from message context
    /// </summary>
    /// <param name="eventMessage">Event message</param>
    /// <param name="messageContext">Message context</param>
    /// <param name="isDomainEvent">Whether it's a domain event</param>
    /// <param name="isIntegrationEvent">Whether it's an integration event</param>
    /// <returns>Event context</returns>
    private static EventContext CreateEventContext(IEvent eventMessage, MessageContext messageContext, bool isDomainEvent, bool isIntegrationEvent)
    {
        return new EventContext
        {
            EventId = eventMessage.Id.ToString(),
            EventType = eventMessage.EventType,
            Version = eventMessage.Version,
            Timestamp = DateTimeOffset.UtcNow,
            CorrelationId = eventMessage.CorrelationId,
            CausationId = eventMessage.CausationId,
            Source = messageContext.Exchange,
            Subject = messageContext.RoutingKey,
            IsDomainEvent = isDomainEvent,
            IsIntegrationEvent = isIntegrationEvent,
            AttemptCount = messageContext.AttemptCount,
            Metadata = eventMessage.Metadata?.ToDictionary(kvp => kvp.Key, kvp => kvp.Value),
            Headers = messageContext.Properties.Headers?.ToDictionary(kvp => kvp.Key, kvp => kvp.Value),
            Extensions = messageContext.Context?.ToDictionary(kvp => kvp.Key, kvp => kvp.Value)
        };
    }

    /// <summary>
    /// Updates processing metrics
    /// </summary>
    /// <param name="startTime">Processing start time</param>
    private void UpdateProcessingMetrics(DateTimeOffset startTime)
    {
        var processingTime = DateTimeOffset.UtcNow - startTime;
        
        // Update average processing time (simple moving average)
        _statistics.AverageProcessingTimeMs = (_statistics.AverageProcessingTimeMs + processingTime.TotalMilliseconds) / 2;
        
        // Update events per second (simple calculation)
        if (_statistics.StartedAt.HasValue)
        {
            var totalUptime = DateTimeOffset.UtcNow - _statistics.StartedAt.Value;
            _statistics.EventsPerSecond = _statistics.TotalEventsProcessedSuccessfully / totalUptime.TotalSeconds;
        }
    }

    /// <summary>
    /// Changes the event bus status
    /// </summary>
    /// <param name="newStatus">New status</param>
    private void ChangeStatus(EventBusStatus newStatus)
    {
        _status = newStatus;
        _statistics.Status = newStatus;
        _logger.LogDebug("Event bus status changed to: {Status}", newStatus);
    }

    /// <summary>
    /// Ensures the event bus is running
    /// </summary>
    /// <exception cref="InvalidOperationException">Thrown when event bus is not running</exception>
    private void EnsureRunning()
    {
        if (_status != EventBusStatus.Running)
            throw new InvalidOperationException($"Event bus is not running. Current status: {_status}");
    }

    /// <summary>
    /// Throws ObjectDisposedException if the event bus has been disposed
    /// </summary>
    /// <exception cref="ObjectDisposedException">Thrown when the event bus has been disposed</exception>
    private void ThrowIfDisposed()
    {
        if (_disposed)
            throw new ObjectDisposedException(nameof(RabbitMQEventBus));
    }

    /// <summary>
    /// Releases all resources used by the RabbitMQEventBus
    /// </summary>
    public void Dispose()
    {
        if (_disposed)
            return;

        lock (_lockObject)
        {
            if (_disposed)
                return;

            _disposed = true;

            try
            {
                // Stop the event bus if it's running
                if (_status == EventBusStatus.Running)
                {
                    StopAsync().GetAwaiter().GetResult();
                }

                // Clear event handlers and subscriptions
                _eventHandlers.Clear();
                _eventSubscriptions.Clear();

                _logger.LogInformation("RabbitMQ Event Bus disposed successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during RabbitMQ Event Bus disposal");
            }
        }
    }
} 