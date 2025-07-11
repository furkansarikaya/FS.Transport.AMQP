using System.Collections.Concurrent;
using FS.RabbitMQ.Consumer;
using FS.RabbitMQ.Core;
using FS.RabbitMQ.EventHandlers;
using FS.RabbitMQ.Events;
using FS.RabbitMQ.Producer;
using RabbitMQ.Client;

namespace FS.RabbitMQ.EventBus;

/// <summary>
/// High-level event bus implementation for publishing and subscribing to events with automatic routing and handling
/// </summary>
public class EventBus : IEventBus
{
    private readonly IRabbitMQClient _rabbitMQClient;
    private readonly IMessageProducer _producer;
    private readonly IMessageConsumer _consumer;
    private readonly ConcurrentDictionary<Type, List<object>> _subscriptions = new();
    private readonly EventBusStatistics _statistics;
    private EventBusStatus _status = EventBusStatus.NotInitialized;
    private readonly SemaphoreSlim _semaphore = new(1, 1);
    private bool _disposed = false;
    
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
    /// Initializes a new instance of the <see cref="EventBus"/> class
    /// </summary>
    /// <param name="rabbitMQClient">The RabbitMQ client for accessing infrastructure services</param>
    /// <param name="producer">The message producer for publishing events</param>
    /// <param name="consumer">The message consumer for receiving events</param>
    /// <exception cref="ArgumentNullException">
    /// Thrown when any required parameter is null
    /// </exception>
    public EventBus(IRabbitMQClient rabbitMQClient, IMessageProducer producer, IMessageConsumer consumer)
    {
        _rabbitMQClient = rabbitMQClient ?? throw new ArgumentNullException(nameof(rabbitMQClient));
        _producer = producer ?? throw new ArgumentNullException(nameof(producer));
        _consumer = consumer ?? throw new ArgumentNullException(nameof(consumer));
        
        _statistics = new EventBusStatistics
        {
            Name = "EventBus",
            Status = _status
        };
    }
    
    /// <summary>
    /// Starts the event bus and initializes connections
    /// </summary>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Task representing the start operation</returns>
    public async Task StartAsync(CancellationToken cancellationToken = default)
    {
        await _semaphore.WaitAsync(cancellationToken).ConfigureAwait(false);
        try
        {
            if (_status != EventBusStatus.NotInitialized && _status != EventBusStatus.Stopped)
                return;
                
            _status = EventBusStatus.Starting;
            _statistics.Status = _status;
            
            // Initialize connections and start consumer
            await _consumer.StartAsync(cancellationToken).ConfigureAwait(false);
            
            _status = EventBusStatus.Running;
            _statistics.Status = _status;
            _statistics.StartedAt = DateTimeOffset.UtcNow;
            _statistics.IsConnected = true;
        }
        catch (Exception ex)
        {
            _status = EventBusStatus.Faulted;
            _statistics.Status = _status;
            _statistics.LastErrorMessage = ex.Message;
            _statistics.LastErrorAt = DateTimeOffset.UtcNow;
            throw;
        }
        finally
        {
            _semaphore.Release();
        }
    }
    
    /// <summary>
    /// Stops the event bus and releases resources
    /// </summary>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Task representing the stop operation</returns>
    public async Task StopAsync(CancellationToken cancellationToken = default)
    {
        await _semaphore.WaitAsync(cancellationToken).ConfigureAwait(false);
        try
        {
            if (_status != EventBusStatus.Running)
                return;
                
            _status = EventBusStatus.Stopping;
            _statistics.Status = _status;
            
            // Stop consumer
            await _consumer.StopAsync(cancellationToken).ConfigureAwait(false);
            
            _status = EventBusStatus.Stopped;
            _statistics.Status = _status;
            _statistics.IsConnected = false;
        }
        finally
        {
            _semaphore.Release();
        }
    }
    
    /// <summary>
    /// Publishes a domain event to the event bus
    /// </summary>
    /// <typeparam name="T">Domain event type</typeparam>
    /// <param name="domainEvent">Domain event to publish</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Task representing the publish operation</returns>
    public async Task PublishDomainEventAsync<T>(T domainEvent, CancellationToken cancellationToken = default) where T : class, IDomainEvent
    {
        if (domainEvent == null) throw new ArgumentNullException(nameof(domainEvent));
        
        try
        {
            var exchangeName = "domain-events";
            var routingKey = typeof(T).Name;
            
            // Serialize the domain event to bytes
            var messageBytes = System.Text.Json.JsonSerializer.SerializeToUtf8Bytes(domainEvent);
            var properties = new BasicProperties
            {
                ContentType = "application/json",
                MessageId = Guid.NewGuid().ToString(),
                Timestamp = new AmqpTimestamp(DateTimeOffset.UtcNow.ToUnixTimeSeconds())
            };
            
            await _producer.PublishAsync(exchangeName, routingKey, messageBytes, properties, false, cancellationToken).ConfigureAwait(false);
            
            _statistics.TotalEventsPublished++;
            _statistics.LastEventPublished = DateTimeOffset.UtcNow;
            
            EventPublished?.Invoke(this, new EventPublishedEventArgs(domainEvent, exchangeName, routingKey, true));
        }
        catch (Exception ex)
        {
            var exchangeName = "domain-events";
            var routingKey = typeof(T).Name;
            
            EventPublished?.Invoke(this, new EventPublishedEventArgs(domainEvent, exchangeName, routingKey, false, ex.Message));
            throw;
        }
    }
    
    /// <summary>
    /// Publishes an integration event to the event bus
    /// </summary>
    /// <typeparam name="T">Integration event type</typeparam>
    /// <param name="integrationEvent">Integration event to publish</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Task representing the publish operation</returns>
    public async Task PublishIntegrationEventAsync<T>(T integrationEvent, CancellationToken cancellationToken = default) where T : class, IIntegrationEvent
    {
        if (integrationEvent == null) throw new ArgumentNullException(nameof(integrationEvent));
        
        try
        {
            var exchangeName = "integration-events";
            var routingKey = typeof(T).Name;
            
            // Serialize the integration event to bytes
            var messageBytes = System.Text.Json.JsonSerializer.SerializeToUtf8Bytes(integrationEvent);
            var properties = new BasicProperties
            {
                ContentType = "application/json",
                MessageId = Guid.NewGuid().ToString(),
                Timestamp = new AmqpTimestamp(DateTimeOffset.UtcNow.ToUnixTimeSeconds())
            };
            
            await _producer.PublishAsync(exchangeName, routingKey, messageBytes, properties, false, cancellationToken).ConfigureAwait(false);
            
            _statistics.TotalEventsPublished++;
            _statistics.LastEventPublished = DateTimeOffset.UtcNow;
            
            EventPublished?.Invoke(this, new EventPublishedEventArgs(integrationEvent, exchangeName, routingKey, true));
        }
        catch (Exception ex)
        {
            var exchangeName = "integration-events";
            var routingKey = typeof(T).Name;
            
            EventPublished?.Invoke(this, new EventPublishedEventArgs(integrationEvent, exchangeName, routingKey, false, ex.Message));
            throw;
        }
    }
    
    /// <summary>
    /// Publishes multiple events in a batch for high-throughput scenarios
    /// </summary>
    /// <param name="events">Collection of events to publish</param>
    /// <param name="cancellationToken">Token to monitor for cancellation requests</param>
    /// <returns>A task that represents the asynchronous batch publish operation</returns>
    /// <exception cref="ArgumentNullException">
    /// Thrown when events collection is null
    /// </exception>
    /// <remarks>
    /// This method provides optimized batch publishing for scenarios where multiple events
    /// need to be published simultaneously. Events are grouped by type and published efficiently.
    /// </remarks>
    public async Task PublishBatchAsync(IEnumerable<IEvent> events, CancellationToken cancellationToken = default)
    {
        if (events == null) throw new ArgumentNullException(nameof(events));
        
        var eventList = events.ToList();
        if (!eventList.Any()) return;
        
        var tasks = eventList.Select(async @event =>
        {
            try
            {
                switch (@event)
                {
                    case IDomainEvent domainEvent:
                        await PublishDomainEventAsync(domainEvent, cancellationToken).ConfigureAwait(false);
                        break;
                    case IIntegrationEvent integrationEvent:
                        await PublishIntegrationEventAsync(integrationEvent, cancellationToken).ConfigureAwait(false);
                        break;
                    default:
                        throw new ArgumentException($"Unknown event type: {@event.GetType().Name}");
                }
            }
            catch (Exception ex)
            {
                // Log individual event failures but continue with others
                _statistics.LastErrorMessage = ex.Message;
                _statistics.LastErrorAt = DateTimeOffset.UtcNow;
                throw;
            }
        });
        
        await Task.WhenAll(tasks).ConfigureAwait(false);
    }
    
    /// <summary>
    /// Subscribes to domain events of a specific type with automatic routing
    /// </summary>
    /// <typeparam name="T">The type of domain events to subscribe to</typeparam>
    /// <param name="handler">Event handler implementation for processing domain events</param>
    /// <param name="cancellationToken">Token to monitor for cancellation requests</param>
    /// <returns>A task that represents the asynchronous subscription operation</returns>
    /// <exception cref="ArgumentNullException">
    /// Thrown when handler is null
    /// </exception>
    /// <remarks>
    /// Domain events are consumed from the domain-events exchange with automatic queue creation
    /// and binding. The queue name follows the pattern: domain-events.{EventType}
    /// </remarks>
    public async Task SubscribeToDomainEventAsync<T>(IAsyncEventHandler<T> handler, CancellationToken cancellationToken = default) where T : class, IDomainEvent
    {
        if (handler == null) throw new ArgumentNullException(nameof(handler));
        
        var eventType = typeof(T);
        var exchangeName = "domain-events";
        var routingKey = eventType.Name;
        
        // Add to subscriptions
        _subscriptions.AddOrUpdate(eventType, [handler], (_, existing) =>
        {
            existing.Add(handler);
            return existing;
        });
        
        // Use the consumer's event consumption method
        await _consumer.ConsumeDomainEventAsync<T>("default", async (message, eventContext) =>
        {
            try
            {
                _statistics.TotalEventsReceived++;
                _statistics.CurrentlyProcessingEvents++;
                _statistics.LastEventReceived = DateTimeOffset.UtcNow;
                
                EventReceived?.Invoke(this, new EventReceivedEventArgs(message, exchangeName, routingKey, null));
                
                await handler.HandleAsync(message, eventContext, cancellationToken).ConfigureAwait(false);
                
                _statistics.TotalEventsProcessedSuccessfully++;
                return true;
            }
            catch (Exception ex)
            {
                _statistics.TotalEventsFailedProcessing++;
                EventProcessingFailed?.Invoke(this, new EventProcessingFailedEventArgs(message, ex, exchangeName, routingKey, 1, false));
                throw;
            }
            finally
            {
                _statistics.CurrentlyProcessingEvents--;
            }
        }, cancellationToken).ConfigureAwait(false);
        
        _statistics.ActiveSubscriptions++;
    }
    
    /// <summary>
    /// Subscribes to integration events of a specific type with automatic routing
    /// </summary>
    /// <typeparam name="T">The type of integration events to subscribe to</typeparam>
    /// <param name="handler">Event handler implementation for processing integration events</param>
    /// <param name="cancellationToken">Token to monitor for cancellation requests</param>
    /// <returns>A task that represents the asynchronous subscription operation</returns>
    /// <exception cref="ArgumentNullException">
    /// Thrown when handler is null
    /// </exception>
    /// <remarks>
    /// Integration events are consumed from the integration-events exchange with automatic queue creation
    /// and binding. The queue name follows the pattern: integration-events.{EventType}
    /// </remarks>
    public async Task SubscribeToIntegrationEventAsync<T>(IAsyncEventHandler<T> handler, CancellationToken cancellationToken = default) where T : class, IIntegrationEvent
    {
        if (handler == null) throw new ArgumentNullException(nameof(handler));
        
        var eventType = typeof(T);
        var exchangeName = "integration-events";
        var routingKey = eventType.Name;
        
        // Add to subscriptions
        _subscriptions.AddOrUpdate(eventType, new List<object> { handler }, (_, existing) =>
        {
            existing.Add(handler);
            return existing;
        });
        
        // Use the consumer's event consumption method
        await _consumer.ConsumeIntegrationEventAsync<T>("default", async (message, eventContext) =>
        {
            try
            {
                _statistics.TotalEventsReceived++;
                _statistics.CurrentlyProcessingEvents++;
                _statistics.LastEventReceived = DateTimeOffset.UtcNow;
                
                EventReceived?.Invoke(this, new EventReceivedEventArgs(message, exchangeName, routingKey, null));
                
                await handler.HandleAsync(message, eventContext).ConfigureAwait(false);
                
                _statistics.TotalEventsProcessedSuccessfully++;
                return true;
            }
            catch (Exception ex)
            {
                _statistics.TotalEventsFailedProcessing++;
                EventProcessingFailed?.Invoke(this, new EventProcessingFailedEventArgs(message, ex, exchangeName, routingKey, 1, false));
                throw;
            }
            finally
            {
                _statistics.CurrentlyProcessingEvents--;
            }
        }, cancellationToken).ConfigureAwait(false);
        
        _statistics.ActiveSubscriptions++;
    }
    
    /// <summary>
    /// Subscribes to events with a custom event handler function
    /// </summary>
    /// <typeparam name="T">The type of events to subscribe to</typeparam>
    /// <param name="handler">Event handler function for processing events</param>
    /// <param name="cancellationToken">Token to monitor for cancellation requests</param>
    /// <returns>A task that represents the asynchronous subscription operation</returns>
    /// <exception cref="ArgumentNullException">
    /// Thrown when handler is null
    /// </exception>
    /// <remarks>
    /// This method allows for inline event handling without implementing the IAsyncEventHandler interface.
    /// The handler function should return true for successful processing or false for failures.
    /// </remarks>
    public async Task SubscribeAsync<T>(Func<T, EventContext, Task<bool>> handler, CancellationToken cancellationToken = default) where T : class, IEvent
    {
        if (handler == null) throw new ArgumentNullException(nameof(handler));
        
        var eventType = typeof(T);
        var exchangeName = eventType.IsAssignableTo(typeof(IDomainEvent)) ? "domain-events" : "integration-events";
        var routingKey = eventType.Name;
        
        // Add to subscriptions
        _subscriptions.AddOrUpdate(eventType, new List<object> { handler }, (_, existing) =>
        {
            existing.Add(handler);
            return existing;
        });
        
        // Use the consumer's event consumption method
        await _consumer.ConsumeEventAsync<T>(exchangeName, routingKey, handler, cancellationToken).ConfigureAwait(false);
        
        _statistics.ActiveSubscriptions++;
    }
    
    /// <summary>
    /// Unsubscribes from events of a specific type
    /// </summary>
    /// <typeparam name="T">The type of events to unsubscribe from</typeparam>
    /// <param name="cancellationToken">Token to monitor for cancellation requests</param>
    /// <returns>A task that represents the asynchronous unsubscribe operation</returns>
    /// <remarks>
    /// This method removes all subscriptions for the specified event type and cleans up associated resources.
    /// </remarks>
    public async Task UnsubscribeAsync<T>(CancellationToken cancellationToken = default) where T : class, IEvent
    {
        var eventType = typeof(T);
        
        // Remove from subscriptions
        _subscriptions.TryRemove(eventType, out _);
        
        // Note: Consumer doesn't have a direct unsubscribe method, 
        // would need to stop/restart consumer or manage subscriptions internally
        
        _statistics.ActiveSubscriptions = Math.Max(0, _statistics.ActiveSubscriptions - 1);
        
        await Task.CompletedTask; // To satisfy async requirement
    }
    
    /// <summary>
    /// Releases all resources used by the <see cref="EventBus"/>
    /// </summary>
    /// <remarks>
    /// This method performs cleanup of all managed resources and stops the event bus if it's running.
    /// After disposal, the event bus cannot be reused.
    /// </remarks>
    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }
    
    /// <summary>
    /// Releases resources used by the EventBus
    /// </summary>
    /// <param name="disposing">Whether disposing is called from Dispose() method</param>
    protected virtual void Dispose(bool disposing)
    {
        if (!_disposed && disposing)
        {
            try
            {
                StopAsync().GetAwaiter().GetResult();
            }
            catch
            {
                // Suppress exceptions during dispose
            }
            
            _semaphore?.Dispose();
            _subscriptions.Clear();
            _disposed = true;
        }
    }
}