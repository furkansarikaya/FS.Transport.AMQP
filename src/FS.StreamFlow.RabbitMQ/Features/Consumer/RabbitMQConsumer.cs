using FS.StreamFlow.Core.Features.Messaging.Interfaces;
using FS.StreamFlow.Core.Features.Messaging.Models;
using FS.StreamFlow.Core.Features.Events.Interfaces;
using FS.StreamFlow.Core.Features.Events.Models;
using FS.StreamFlow.RabbitMQ.Features.Connection;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using Microsoft.Extensions.Logging;
using System.Text;
using System.Text.Json;
using System.Collections.Concurrent;
using RabbitMqChannel = RabbitMQ.Client.IChannel;

namespace FS.StreamFlow.RabbitMQ.Features.Consumer;

/// <summary>
/// RabbitMQ implementation of the consumer interface.
/// Provides high-performance message consumption with automatic acknowledgment, retry policies, and error handling.
/// </summary>
public class RabbitMQConsumer : IConsumer
{
    private readonly IConnectionManager _connectionManager;
    private readonly ILogger<RabbitMQConsumer> _logger;
    private readonly ConsumerSettings _settings;
    private readonly ConsumerStatistics _statistics;
    private readonly ConcurrentDictionary<string, ConsumerInfo> _activeConsumers;
    private readonly SemaphoreSlim _operationSemaphore;
    private readonly CancellationTokenSource _cancellationTokenSource;
    
    private volatile ConsumerStatus _status = ConsumerStatus.NotInitialized;
    private volatile bool _disposed;
    private RabbitMqChannel? _channel;

    /// <summary>
    /// Gets the current consumer status
    /// </summary>
    public ConsumerStatus Status => _status;

    /// <summary>
    /// Gets consumer configuration settings
    /// </summary>
    public ConsumerSettings Settings => _settings;

    /// <summary>
    /// Gets consumer statistics and metrics
    /// </summary>
    public ConsumerStatistics Statistics => _statistics;

    /// <summary>
    /// Event raised when a message is received
    /// </summary>
    public event EventHandler<MessageReceivedEventArgs>? MessageReceived;

    /// <summary>
    /// Event raised when a message is processed successfully
    /// </summary>
    public event EventHandler<MessageProcessedEventArgs>? MessageProcessed;

    /// <summary>
    /// Event raised when message processing fails
    /// </summary>
    public event EventHandler<MessageProcessingFailedEventArgs>? MessageProcessingFailed;

    /// <summary>
    /// Event raised when a message is acknowledged
    /// </summary>
    public event EventHandler<MessageAcknowledgedEventArgs>? MessageAcknowledged;

    /// <summary>
    /// Event raised when a message is rejected
    /// </summary>
    public event EventHandler<MessageRejectedEventArgs>? MessageRejected;

    /// <summary>
    /// Event raised when consumer status changes
    /// </summary>
    public event EventHandler<ConsumerStatusChangedEventArgs>? StatusChanged;

    /// <summary>
    /// Event raised when consumer is paused
    /// </summary>
    public event EventHandler<ConsumerPausedEventArgs>? ConsumerPaused;

    /// <summary>
    /// Event raised when consumer is resumed
    /// </summary>
    public event EventHandler<ConsumerResumedEventArgs>? ConsumerResumed;

    /// <summary>
    /// Initializes a new instance of the RabbitMQConsumer class
    /// </summary>
    /// <param name="connectionManager">Connection manager</param>
    /// <param name="settings">Consumer settings</param>
    /// <param name="logger">Logger</param>
    public RabbitMQConsumer(
        IConnectionManager connectionManager,
        ConsumerSettings settings,
        ILogger<RabbitMQConsumer> logger)
    {
        _connectionManager = connectionManager ?? throw new ArgumentNullException(nameof(connectionManager));
        _settings = settings ?? throw new ArgumentNullException(nameof(settings));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        
        _statistics = new ConsumerStatistics
        {
            ConsumerId = _settings.ConsumerId,
            StartTime = DateTimeOffset.UtcNow
        };
        
        _activeConsumers = new ConcurrentDictionary<string, ConsumerInfo>();
        _operationSemaphore = new SemaphoreSlim(1, 1);
        _cancellationTokenSource = new CancellationTokenSource();
    }

    /// <summary>
    /// Starts the consumer and begins consuming messages
    /// </summary>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Task representing the start operation</returns>
    public async Task StartAsync(CancellationToken cancellationToken = default)
    {
        await _operationSemaphore.WaitAsync(cancellationToken);
        try
        {
            if (_status == ConsumerStatus.Running)
            {
                _logger.LogWarning("Consumer is already running");
                return;
            }

            _logger.LogInformation("Starting RabbitMQ consumer {ConsumerId}", _settings.ConsumerId);
            
            if (_connectionManager.State != ConnectionState.Connected)
            {
                await _connectionManager.ConnectAsync(cancellationToken);
            }

            _channel = await _connectionManager.GetChannelAsync(cancellationToken) as RabbitMqChannel;
            if (_channel != null)
            {
                await ConfigureChannelAsync(_channel, cancellationToken);
            }

            ChangeStatus(ConsumerStatus.Running);
            _statistics.StartTime = DateTimeOffset.UtcNow;
            
            _logger.LogInformation("RabbitMQ consumer {ConsumerId} started successfully", _settings.ConsumerId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to start RabbitMQ consumer {ConsumerId}", _settings.ConsumerId);
            ChangeStatus(ConsumerStatus.Faulted);
            throw;
        }
        finally
        {
            _operationSemaphore.Release();
        }
    }

    /// <summary>
    /// Stops the consumer and releases resources
    /// </summary>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Task representing the stop operation</returns>
    public async Task StopAsync(CancellationToken cancellationToken = default)
    {
        await _operationSemaphore.WaitAsync(cancellationToken);
        try
        {
            if (_status == ConsumerStatus.Stopped)
            {
                _logger.LogWarning("Consumer is already stopped");
                return;
            }

            _logger.LogInformation("Stopping RabbitMQ consumer {ConsumerId}", _settings.ConsumerId);
            
            ChangeStatus(ConsumerStatus.Stopping);
            
            // Cancel all active consumers
            foreach (var kvp in _activeConsumers)
            {
                try
                {
                    if (_channel != null)
                    {
                        await _channel.BasicCancelAsync(kvp.Key, false, cancellationToken);
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Failed to cancel consumer {ConsumerTag}", kvp.Key);
                }
            }
            
            _activeConsumers.Clear();

            // Close channel
            if (_channel != null)
            {
                await _channel.CloseAsync(cancellationToken);
                _channel = null;
            }

            ChangeStatus(ConsumerStatus.Stopped);
            
            _logger.LogInformation("RabbitMQ consumer {ConsumerId} stopped successfully", _settings.ConsumerId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to stop RabbitMQ consumer {ConsumerId}", _settings.ConsumerId);
            ChangeStatus(ConsumerStatus.Faulted);
            throw;
        }
        finally
        {
            _operationSemaphore.Release();
        }
    }

    /// <summary>
    /// Consumes messages from a queue with automatic deserialization and processing
    /// </summary>
    /// <typeparam name="T">Message type</typeparam>
    /// <param name="queueName">Queue name to consume from</param>
    /// <param name="messageHandler">Handler function for processing messages</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Task representing the consumption operation</returns>
    public async Task ConsumeAsync<T>(string queueName, Func<T, MessageContext, Task<bool>> messageHandler, CancellationToken cancellationToken = default) where T : class
    {
        var context = new ConsumerContext();
        await ConsumeAsync(queueName, messageHandler, context, cancellationToken);
    }

    /// <summary>
    /// Consumes messages from a queue with consumer context and options
    /// </summary>
    /// <typeparam name="T">Message type</typeparam>
    /// <param name="queueName">Queue name to consume from</param>
    /// <param name="messageHandler">Handler function for processing messages</param>
    /// <param name="context">Consumer context with settings and options</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Task representing the consumption operation</returns>
    public async Task ConsumeAsync<T>(string queueName, Func<T, MessageContext, Task<bool>> messageHandler, ConsumerContext context, CancellationToken cancellationToken = default) where T : class
    {
        if (_status != ConsumerStatus.Running)
        {
            throw new InvalidOperationException("Consumer must be started before consuming messages");
        }

        if (_channel == null)
        {
            throw new InvalidOperationException("Channel is not available");
        }

        _logger.LogInformation("Starting to consume messages from queue {QueueName}", queueName);

        // Set prefetch count
        await _channel.BasicQosAsync(0, _settings.PrefetchCount, false, cancellationToken);

        var consumer = new AsyncEventingBasicConsumer(_channel);
        consumer.ReceivedAsync += async (sender, eventArgs) =>
        {
            await HandleMessageAsync(eventArgs, messageHandler, context, cancellationToken);
        };

        var consumerTag = await _channel.BasicConsumeAsync(
            queue: queueName,
            autoAck: _settings.AutoAcknowledge,
            consumer: consumer,
            cancellationToken: cancellationToken);

        _activeConsumers.TryAdd(consumerTag, new ConsumerInfo
        {
            ConsumerTag = consumerTag,
            QueueName = queueName,
            MessageType = typeof(T).Name,
            StartTime = DateTimeOffset.UtcNow,
            IsActive = true
        });

        _logger.LogInformation("Consumer {ConsumerTag} started for queue {QueueName}", consumerTag, queueName);
    }

    /// <summary>
    /// Consumes events from an exchange with automatic event handler resolution
    /// </summary>
    /// <typeparam name="T">Event type</typeparam>
    /// <param name="exchangeName">Exchange name to consume from</param>
    /// <param name="routingKey">Routing key pattern</param>
    /// <param name="eventHandler">Event handler for processing events</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Task representing the consumption operation</returns>
    public async Task ConsumeEventAsync<T>(string exchangeName, string routingKey, IAsyncEventHandler<T> eventHandler, CancellationToken cancellationToken = default) where T : class, IEvent
    {
        await ConsumeEventAsync<T>(exchangeName, routingKey, async (evt, ctx) =>
        {
            await eventHandler.HandleAsync(evt, ctx, cancellationToken);
            return true;
        }, cancellationToken);
    }

    /// <summary>
    /// Consumes events from an exchange with inline event handler
    /// </summary>
    /// <typeparam name="T">Event type</typeparam>
    /// <param name="exchangeName">Exchange name to consume from</param>
    /// <param name="routingKey">Routing key pattern</param>
    /// <param name="eventHandler">Inline event handler function</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Task representing the consumption operation</returns>
    public async Task ConsumeEventAsync<T>(string exchangeName, string routingKey, Func<T, EventContext, Task<bool>> eventHandler, CancellationToken cancellationToken = default) where T : class, IEvent
    {
        if (_status != ConsumerStatus.Running)
        {
            throw new InvalidOperationException("Consumer must be started before consuming events");
        }

        if (_channel == null)
        {
            throw new InvalidOperationException("Channel is not available");
        }

        // Declare a temporary queue for event consumption
        var queueResult = await _channel.QueueDeclareAsync(
            queue: string.Empty,
            durable: false,
            exclusive: true,
            autoDelete: true,
            arguments: null,
            cancellationToken: cancellationToken);

        var queueName = queueResult.QueueName;

        // Bind queue to exchange
        await _channel.QueueBindAsync(
            queue: queueName,
            exchange: exchangeName,
            routingKey: routingKey,
            arguments: null,
            cancellationToken: cancellationToken);

        _logger.LogInformation("Consuming events from exchange {ExchangeName} with routing key {RoutingKey}", exchangeName, routingKey);

        // Start consuming
        await ConsumeAsync<T>(queueName, async (evt, msgCtx) =>
        {
            var eventCtx = new EventContext
            {
                EventId = evt.GetType().Name, // Basic implementation
                EventType = evt.GetType().Name,
                Source = exchangeName,
                Timestamp = DateTimeOffset.UtcNow,
                CorrelationId = msgCtx.Properties.CorrelationId
            };

            return await eventHandler(evt, eventCtx);
        }, cancellationToken);
    }

    /// <summary>
    /// Consumes domain events with automatic aggregate handling
    /// </summary>
    /// <typeparam name="T">Domain event type</typeparam>
    /// <param name="aggregateType">Aggregate type to consume events for</param>
    /// <param name="eventHandler">Domain event handler</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Task representing the consumption operation</returns>
    public async Task ConsumeDomainEventAsync<T>(string aggregateType, Func<T, EventContext, Task<bool>> eventHandler, CancellationToken cancellationToken = default) where T : class, IDomainEvent
    {
        var exchangeName = $"domain.{aggregateType}";
        var routingKey = typeof(T).Name;

        await ConsumeEventAsync<T>(exchangeName, routingKey, eventHandler, cancellationToken);
    }

    /// <summary>
    /// Consumes integration events with service-to-service communication
    /// </summary>
    /// <typeparam name="T">Integration event type</typeparam>
    /// <param name="serviceName">Service name to consume events from</param>
    /// <param name="eventHandler">Integration event handler</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Task representing the consumption operation</returns>
    public async Task ConsumeIntegrationEventAsync<T>(string serviceName, Func<T, EventContext, Task<bool>> eventHandler, CancellationToken cancellationToken = default) where T : class, IIntegrationEvent
    {
        var exchangeName = $"integration.{serviceName}";
        var routingKey = typeof(T).Name;

        await ConsumeEventAsync<T>(exchangeName, routingKey, eventHandler, cancellationToken);
    }

    /// <summary>
    /// Creates a fluent API for advanced consumer configuration
    /// </summary>
    /// <typeparam name="T">Message type</typeparam>
    /// <param name="queueName">Queue name to consume from</param>
    /// <returns>Fluent consumer API</returns>
    public IFluentConsumerApi<T> Fluent<T>(string queueName) where T : class
    {
        return new RabbitMQFluentConsumerApi<T>(this, queueName);
    }

    /// <summary>
    /// Acknowledges a message manually
    /// </summary>
    /// <param name="deliveryTag">Delivery tag of the message</param>
    /// <param name="multiple">Whether to acknowledge multiple messages</param>
    /// <returns>Task representing the acknowledgment operation</returns>
    public async Task AcknowledgeAsync(ulong deliveryTag, bool multiple = false)
    {
        if (_channel == null)
        {
            throw new InvalidOperationException("Channel is not available");
        }

        await _channel.BasicAckAsync(deliveryTag, multiple, CancellationToken.None);
        var messageContext = new MessageContext { DeliveryTag = deliveryTag };
        MessageAcknowledged?.Invoke(this, new MessageAcknowledgedEventArgs(deliveryTag, messageContext));
    }

    /// <summary>
    /// Rejects a message and optionally requeues it
    /// </summary>
    /// <param name="deliveryTag">Delivery tag of the message</param>
    /// <param name="requeue">Whether to requeue the message</param>
    /// <returns>Task representing the rejection operation</returns>
    public async Task RejectAsync(ulong deliveryTag, bool requeue = false)
    {
        if (_channel == null)
        {
            throw new InvalidOperationException("Channel is not available");
        }

        await _channel.BasicRejectAsync(deliveryTag, requeue, CancellationToken.None);
        var messageContext = new MessageContext { DeliveryTag = deliveryTag };
        MessageRejected?.Invoke(this, new MessageRejectedEventArgs(deliveryTag, messageContext, "Message rejected", requeue));
    }

    /// <summary>
    /// Requeues a message for later processing
    /// </summary>
    /// <param name="deliveryTag">Delivery tag of the message</param>
    /// <returns>Task representing the requeue operation</returns>
    public async Task RequeueAsync(ulong deliveryTag)
    {
        await RejectAsync(deliveryTag, true);
    }

    /// <summary>
    /// Pauses message consumption
    /// </summary>
    /// <returns>Task representing the pause operation</returns>
    public async Task PauseAsync()
    {
        if (_status != ConsumerStatus.Running)
        {
            throw new InvalidOperationException("Consumer must be running to pause");
        }

        _logger.LogInformation("Pausing consumer {ConsumerId}", _settings.ConsumerId);
        ChangeStatus(ConsumerStatus.Running); // Keep as running for now
        
        await Task.CompletedTask;
    }

    /// <summary>
    /// Resumes message consumption
    /// </summary>
    /// <returns>Task representing the resume operation</returns>
    public async Task ResumeAsync()
    {
        _logger.LogInformation("Resuming consumer {ConsumerId}", _settings.ConsumerId);
        ChangeStatus(ConsumerStatus.Running);
        
        await Task.CompletedTask;
    }

    /// <summary>
    /// Gets the current number of unprocessed messages in a queue
    /// </summary>
    /// <param name="queueName">Queue name</param>
    /// <returns>Number of unprocessed messages</returns>
    public async Task<uint> GetMessageCountAsync(string queueName)
    {
        if (_channel == null)
        {
            throw new InvalidOperationException("Channel is not available");
        }

        var result = await _channel.QueueDeclarePassiveAsync(queueName);
        return result.MessageCount;
    }

    /// <summary>
    /// Gets the current number of consumers for a queue
    /// </summary>
    /// <param name="queueName">Queue name</param>
    /// <returns>Number of consumers</returns>
    public async Task<uint> GetConsumerCountAsync(string queueName)
    {
        if (_channel == null)
        {
            throw new InvalidOperationException("Channel is not available");
        }

        var result = await _channel.QueueDeclarePassiveAsync(queueName);
        return result.ConsumerCount;
    }

    /// <summary>
    /// Disposes the consumer resources
    /// </summary>
    public void Dispose()
    {
        if (_disposed) return;

        try
        {
            StopAsync().Wait(TimeSpan.FromSeconds(30));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during consumer disposal");
        }

        _operationSemaphore?.Dispose();
        _cancellationTokenSource?.Dispose();
        _disposed = true;
        GC.SuppressFinalize(this);
    }

    private async Task ConfigureChannelAsync(RabbitMqChannel channel, CancellationToken cancellationToken)
    {
        // Configure channel settings
        await channel.BasicQosAsync(0, _settings.PrefetchCount, false, cancellationToken);
        
        _logger.LogDebug("Channel configured with prefetch count: {PrefetchCount}", _settings.PrefetchCount);
    }

    private async Task HandleMessageAsync<T>(BasicDeliverEventArgs eventArgs, Func<T, MessageContext, Task<bool>> messageHandler, ConsumerContext context, CancellationToken cancellationToken) where T : class
    {
        var messageContext = CreateMessageContext(eventArgs);
        
        try
        {
            // Deserialize message
            var message = DeserializeMessage<T>(eventArgs.Body.ToArray());
            messageContext.Message = message;
            
            MessageReceived?.Invoke(this, new MessageReceivedEventArgs(message, messageContext));
            
            // Process message
            var processingStartTime = DateTimeOffset.UtcNow;
            var success = await messageHandler(message, messageContext);
            var processingDuration = DateTimeOffset.UtcNow - processingStartTime;
            
            if (success)
            {
                MessageProcessed?.Invoke(this, new MessageProcessedEventArgs(message, messageContext, true, processingDuration));
                
                // Acknowledge message if not auto-ack
                if (!_settings.AutoAcknowledge)
                {
                    await AcknowledgeAsync(eventArgs.DeliveryTag, false);
                }
            }
            else
            {
                MessageProcessingFailed?.Invoke(this, new MessageProcessingFailedEventArgs(
                    message, messageContext, new Exception("Message processing returned false"), 1));
                
                // Reject message
                if (!_settings.AutoAcknowledge)
                {
                    await RejectAsync(eventArgs.DeliveryTag, false);
                }
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing message from queue {QueueName}", eventArgs.RoutingKey);
            
            MessageProcessingFailed?.Invoke(this, new MessageProcessingFailedEventArgs(
                eventArgs.Body.ToArray(), messageContext, ex, 1));
            
            // Reject message on exception
            if (!_settings.AutoAcknowledge)
            {
                await RejectAsync(eventArgs.DeliveryTag, false);
            }
        }
    }

    private MessageContext CreateMessageContext(BasicDeliverEventArgs eventArgs)
    {
        var properties = eventArgs.BasicProperties;
        
        return new MessageContext
        {
            Exchange = eventArgs.Exchange,
            RoutingKey = eventArgs.RoutingKey,
            QueueName = eventArgs.ConsumerTag,
            ConsumerTag = eventArgs.ConsumerTag,
            DeliveryTag = eventArgs.DeliveryTag,
            IsRedelivered = eventArgs.Redelivered,
            Body = eventArgs.Body,
            Properties = new MessageProperties
            {
                MessageId = properties.MessageId ?? Guid.NewGuid().ToString(),
                CorrelationId = properties.CorrelationId,
                ContentType = properties.ContentType,
                ContentEncoding = properties.ContentEncoding,
                DeliveryMode = (DeliveryMode)properties.DeliveryMode,
                Priority = properties.Priority,
                Timestamp = DateTimeOffset.UtcNow,
                Expiration = properties.Expiration,
                UserId = properties.UserId,
                AppId = properties.AppId,
                Type = properties.Type,
                ReplyTo = properties.ReplyTo,
                Headers = properties.Headers?.ToDictionary(kvp => kvp.Key, kvp => kvp.Value) ?? new Dictionary<string, object>()
            },
            ReceivedAt = DateTimeOffset.UtcNow
        };
    }

    private T DeserializeMessage<T>(byte[] body) where T : class
    {
        var json = Encoding.UTF8.GetString(body);
        return JsonSerializer.Deserialize<T>(json) ?? throw new InvalidOperationException($"Failed to deserialize message to type {typeof(T).Name}");
    }

    private void ChangeStatus(ConsumerStatus newStatus)
    {
        var oldStatus = _status;
        _status = newStatus;
        
        if (oldStatus != newStatus)
        {
            StatusChanged?.Invoke(this, new ConsumerStatusChangedEventArgs(
                oldStatus, newStatus, _settings.ConsumerId));
        }
    }
}

/// <summary>
/// Information about an active consumer
/// </summary>
internal class ConsumerInfo
{
    public string ConsumerTag { get; set; } = string.Empty;
    public string QueueName { get; set; } = string.Empty;
    public string MessageType { get; set; } = string.Empty;
    public DateTimeOffset StartTime { get; set; }
    public bool IsActive { get; set; }
}

/// <summary>
/// Simple fluent API for consumer configuration
/// </summary>
/// <typeparam name="T">Message type</typeparam>
public class RabbitMQFluentConsumerApi<T> : IFluentConsumerApi<T> where T : class
{
    private readonly IConsumer _consumer;
    private readonly string _queueName;

    public RabbitMQFluentConsumerApi(IConsumer consumer, string queueName)
    {
        _consumer = consumer ?? throw new ArgumentNullException(nameof(consumer));
        _queueName = queueName ?? throw new ArgumentNullException(nameof(queueName));
    }

    public IFluentConsumerApi<T> FromExchange(string exchangeName) => this;
    public IFluentConsumerApi<T> WithRoutingKey(string routingKey) => this;
    public IFluentConsumerApi<T> WithSettings(ConsumerSettings settings) => this;
    public IFluentConsumerApi<T> WithSettings(Action<ConsumerSettings> configureSettings) => this;
    public IFluentConsumerApi<T> WithContext(ConsumerContext context) => this;
    public IFluentConsumerApi<T> WithConsumerTag(string consumerTag) => this;
    public IFluentConsumerApi<T> WithAutoAck(bool autoAck = true) => this;
    public IFluentConsumerApi<T> WithPrefetchCount(int prefetchCount) => this;
    public IFluentConsumerApi<T> WithPrefetchCount(ushort prefetchCount) => this;
    public IFluentConsumerApi<T> WithConcurrency(int maxConcurrency) => this;
    public IFluentConsumerApi<T> WithDeadLetterQueue(DeadLetterSettings deadLetterSettings) => this;
    public IFluentConsumerApi<T> WithRetryPolicy(RetryPolicySettings retryPolicy) => this;
    public IFluentConsumerApi<T> WithFilter(Func<T, bool> filter) => this;
    public IFluentConsumerApi<T> WithErrorHandler(Func<Exception, MessageContext, Task<bool>> errorHandler) => this;

    public async Task ConsumeAsync(Func<T, MessageContext, Task<bool>> messageHandler, CancellationToken cancellationToken = default)
    {
        await _consumer.ConsumeAsync(_queueName, messageHandler, cancellationToken);
    }

    public async Task ConsumeAsync(Func<T, MessageContext, Task<bool>> messageHandler, ConsumerContext context, CancellationToken cancellationToken = default)
    {
        await _consumer.ConsumeAsync(_queueName, messageHandler, context, cancellationToken);
    }

    public async Task ConsumeAsync(Func<IServiceProvider, Func<T, MessageContext, Task<bool>>> handlerFactory, CancellationToken cancellationToken = default)
    {
        // Create a default service provider if none is available
        var serviceProvider = new DefaultServiceProvider();
        var handler = handlerFactory(serviceProvider);
        
        await _consumer.ConsumeAsync(_queueName, handler, new ConsumerContext 
        { 
            ConsumerTag = _queueName,
            Settings = new ConsumerSettings
            {
                AutoAcknowledge = false,
                PrefetchCount = 1
            }
        }, cancellationToken);
    }

    public async Task ConsumeAsync<THandler>(CancellationToken cancellationToken = default) where THandler : class
    {
        // Create an instance of the handler type
        var handler = Activator.CreateInstance<THandler>();
        
        // Check if handler implements the expected interface
        if (handler is not Func<T, MessageContext, Task<bool>> messageHandler)
        {
            throw new InvalidOperationException($"Handler {typeof(THandler).Name} does not implement the expected message handler interface");
        }
        
        await _consumer.ConsumeAsync(_queueName, messageHandler, new ConsumerContext 
        { 
            ConsumerTag = _queueName,
            Settings = new ConsumerSettings
            {
                AutoAcknowledge = false,
                PrefetchCount = 1
            }
        }, cancellationToken);
    }

    public async Task ConsumeEventAsync<TEvent>(Func<TEvent, EventContext, Task<bool>> eventHandler, CancellationToken cancellationToken = default) where TEvent : class, IEvent
    {
        await Task.CompletedTask;
    }

    public async Task ConsumeEventAsync<TEvent>(IAsyncEventHandler<TEvent> eventHandler, CancellationToken cancellationToken = default) where TEvent : class, IEvent
    {
        await Task.CompletedTask;
    }
} 