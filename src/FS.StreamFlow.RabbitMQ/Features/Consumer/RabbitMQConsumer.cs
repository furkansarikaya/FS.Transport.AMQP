using FS.StreamFlow.Core.Features.Messaging.Interfaces;
using FS.StreamFlow.Core.Features.Messaging.Models;
using FS.StreamFlow.Core.Features.Events.Interfaces;
using FS.StreamFlow.Core.Features.Events.Models;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using Microsoft.Extensions.Logging;
using System.Collections.Concurrent;
using FS.StreamFlow.RabbitMQ.Features.Connection;
using Microsoft.Extensions.Options;
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
    private readonly ClientConfiguration _clientConfiguration;
    private readonly ConsumerStatistics _statistics;
    private readonly ConcurrentDictionary<string, ConsumerInfo> _activeConsumers;
    private readonly SemaphoreSlim _operationSemaphore;
    private readonly CancellationTokenSource _cancellationTokenSource;
    private readonly IMessageSerializerFactory _serializerFactory;
    
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
    /// Gets client configuration settings
    /// </summary>
    public ClientConfiguration ClientConfiguration => _clientConfiguration;

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
    /// <param name="clientConfiguration">Client configuration</param>
    /// <param name="logger">Logger</param>
    /// <param name="serializerFactory">Message serializer factory</param>
    public RabbitMQConsumer(
        IConnectionManager connectionManager,
        IOptions<ConsumerSettings> settings,
        IOptions<ClientConfiguration> clientConfiguration,
        ILogger<RabbitMQConsumer> logger, 
        IMessageSerializerFactory serializerFactory)
    {
        _connectionManager = connectionManager ?? throw new ArgumentNullException(nameof(connectionManager));
        _settings = settings.Value ?? throw new ArgumentNullException(nameof(settings));
        _clientConfiguration = clientConfiguration.Value ?? throw new ArgumentNullException(nameof(clientConfiguration));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _serializerFactory = serializerFactory ?? throw new ArgumentNullException(nameof(serializerFactory));

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

            var abstractChannel = await _connectionManager.GetChannelAsync(cancellationToken);
            _channel = ExtractRabbitMqChannel(abstractChannel);
            
            if (_channel == null)
            {
                throw new InvalidOperationException(
                    "Failed to obtain RabbitMQ channel from connection manager. " +
                    "Ensure RabbitMQ provider is properly configured.");
            }

            await ConfigureChannelAsync(_channel, cancellationToken);
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
    /// Consumes events from a fanout exchange with automatic queue creation and event handler resolution.
    /// Creates a durable queue and binds it to the specified exchange for reliable event processing.
    /// </summary>
    /// <typeparam name="T">The type of event to consume, must implement IEvent</typeparam>
    /// <param name="exchangeName">The name of the RabbitMQ fanout exchange to consume events from</param>
    /// <param name="queueName">The name of the queue to create and bind to the exchange</param>
    /// <param name="eventHandler">The event handler instance to process received events</param>
    /// <param name="cancellationToken">Token to cancel the consumption operation</param>
    /// <returns>A task representing the asynchronous consumption operation</returns>
    /// <exception cref="ArgumentNullException">Thrown when any parameter is null</exception>
    /// <exception cref="InvalidOperationException">Thrown when the consumer is not properly initialized</exception>
    public async Task ConsumeEventAsync<T>(string exchangeName, string queueName, IAsyncEventHandler<T> eventHandler, CancellationToken cancellationToken = default) where T : class, IEvent
    {
        await ConsumeEventAsync<T>(exchangeName, queueName, async (evt, ctx) =>
        {
            _logger.LogDebug("ConsumeEventAsync -> Received event {EventType} with correlationId: {CorrelationId}", evt.GetType().Name, evt.CorrelationId);
            await eventHandler.HandleAsync(evt, ctx, cancellationToken);
            _logger.LogDebug("ConsumeEventAsync -> Processed event {EventType} with correlationId: {CorrelationId}", evt.GetType().Name, evt.CorrelationId);
            return true;
        }, cancellationToken);
    }

    /// <summary>
    /// Consumes events from a fanout exchange with automatic queue creation and inline event handler processing.
    /// This method automatically declares the exchange as fanout type, creates a durable queue, and binds them together.
    /// </summary>
    /// <typeparam name="T">The type of event to consume, must implement IEvent</typeparam>
    /// <param name="exchangeName">The name of the RabbitMQ fanout exchange to consume events from</param>
    /// <param name="queueName">The name of the queue to create and bind to the exchange</param>
    /// <param name="eventHandler">The inline handler function to process received events, returns true if processing succeeded</param>
    /// <param name="cancellationToken">Token to cancel the consumption operation</param>
    /// <returns>A task representing the asynchronous consumption operation</returns>
    /// <exception cref="ArgumentNullException">Thrown when any parameter is null</exception>
    /// <exception cref="InvalidOperationException">Thrown when the consumer is not running or channel is unavailable</exception>
    public async Task ConsumeEventAsync<T>(string exchangeName, string queueName, Func<T, EventContext, Task<bool>> eventHandler, CancellationToken cancellationToken = default) where T : class, IEvent
    {
        if (_status != ConsumerStatus.Running)
        {
            throw new InvalidOperationException("Consumer must be started before consuming events");
        }

        if (_channel == null)
        {
            throw new InvalidOperationException("Channel is not available");
        }

        // Auto-declare exchange if it doesn't exist (fanout type for events)
        await _channel.ExchangeDeclareAsync(
            exchange: exchangeName,
            type: "fanout", // Use fanout exchange for events - no routing key needed
            durable: true,
            autoDelete: false,
            arguments: null,
            cancellationToken: cancellationToken);

        // Create a durable queue (use the provided queueName)
        await _channel.QueueDeclareAsync(
            queue: queueName,
            durable: true,
            exclusive: false,
            autoDelete: false,
            arguments: null,
            cancellationToken: cancellationToken);

        // Bind queue to exchange (no routing key needed for fanout)
        await _channel.QueueBindAsync(
            queue: queueName,
            exchange: exchangeName,
            routingKey: string.Empty, // Empty routing key for fanout exchange
            arguments: null,
            cancellationToken: cancellationToken);

        _logger.LogInformation("Consuming events from exchange {ExchangeName} with queue {QueueName}", exchangeName, queueName);

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
    /// Consumes integration events from a specified exchange with automatic queue management and fanout distribution.
    /// This method provides backward compatibility but is deprecated in favor of the more flexible ConsumeEventAsync method.
    /// </summary>
    /// <typeparam name="T">The type of integration event to consume, must implement IIntegrationEvent</typeparam>
    /// <param name="exchangeName">The name of the RabbitMQ exchange to consume events from</param>
    /// <param name="eventHandler">The handler function to process received integration events</param>
    /// <param name="cancellationToken">Token to cancel the consumption operation</param>
    /// <returns>A task representing the asynchronous consumption operation</returns>
    /// <exception cref="ArgumentNullException">Thrown when exchangeName or eventHandler is null</exception>
    /// <exception cref="InvalidOperationException">Thrown when the consumer is not properly initialized</exception>
    [Obsolete("This method is deprecated. Use ConsumeEventAsync<T>(string exchangeName, string queueName, Func<T, EventContext, Task<bool>> eventHandler, CancellationToken cancellationToken) instead for better control over queue naming and configuration.", false)]
    public async Task ConsumeIntegrationEventAsync<T>(string exchangeName, Func<T, EventContext, Task<bool>> eventHandler, CancellationToken cancellationToken = default) where T : class, IIntegrationEvent
    {
        await ConsumeEventAsync<T>(exchangeName, typeof(T).Name, eventHandler, cancellationToken);
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
    /// Creates a fluent API for queue-based consumer configuration
    /// </summary>
    /// <typeparam name="T">Message type</typeparam>
    /// <param name="queueName">Queue name to consume from</param>
    /// <returns>Fluent consumer API</returns>
    public IFluentConsumerApi<T> Queue<T>(string queueName) where T : class
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
        
        // Pause all active consumers by closing the channel
        if (_channel != null)
        {
            await _channel.CloseAsync();
            _channel = null;
        }
        
        ChangeStatus(ConsumerStatus.Stopped);
        ConsumerPaused?.Invoke(this, new ConsumerPausedEventArgs(_settings.ConsumerId, "Consumer paused"));
    }

    /// <summary>
    /// Resumes message consumption
    /// </summary>
    /// <returns>Task representing the resume operation</returns>
    public async Task ResumeAsync()
    {
        _logger.LogInformation("Resuming consumer {ConsumerId}", _settings.ConsumerId);
        
        // Resume by restarting the consumer
        if (_status == ConsumerStatus.Stopped)
        {
            await StartAsync();
        }
        
        ChangeStatus(ConsumerStatus.Running);
        ConsumerResumed?.Invoke(this, new ConsumerResumedEventArgs(_settings.ConsumerId, "Consumer resumed"));
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
            var message = await DeserializeMessage<T>(eventArgs.Body.ToArray(), cancellationToken);
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
                MessageId = properties.MessageId ?? Guid.CreateVersion7().ToString(),
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

    private async Task<T> DeserializeMessage<T>(byte[] body, CancellationToken cancellationToken) where T : class
    {
        try
        {
            _logger.LogDebug("Deserializing message to type: {TargetType}, Body length: {BodyLength}",
                typeof(T).FullName, body.Length);

            var bodyAsString = System.Text.Encoding.UTF8.GetString(body);
            _logger.LogDebug("Message body: {MessageBody}", bodyAsString);

            var serializer = _serializerFactory.CreateSerializer(SerializationFormat.Json, _clientConfiguration.Serialization);
            var result = await serializer.DeserializeAsync<T>(body, cancellationToken);

            if (result == null)
            {
                _logger.LogError("Deserialization returned null for type {TargetType}", typeof(T).FullName);
                throw new InvalidOperationException($"Failed to deserialize message to type {typeof(T).Name}");
            }

            _logger.LogDebug("Successfully deserialized to type: {ActualType}", result.GetType().FullName);
            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to deserialize message to type {TargetType}", typeof(T).FullName);
            throw;
        }
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
    
    /// <summary>
    /// Extracts the RabbitMQ channel from the abstract channel
    /// </summary>
    /// <param name="abstractChannel">Abstract channel</param>
    /// <returns></returns>
    private RabbitMqChannel? ExtractRabbitMqChannel(FS.StreamFlow.Core.Features.Messaging.Models.IChannel abstractChannel)
    {
        try
        {
            switch (abstractChannel)
            {
                // StreamFlow'un RabbitMQ implementation'ı ise
                case RabbitMQChannel streamFlowChannel:
                    return streamFlowChannel.GetNativeChannel();
                case RabbitMqChannel directChannel:
                    return directChannel;
                default:
                    _logger.LogError("Channel is not a RabbitMQ channel: {ChannelType}", 
                        abstractChannel?.GetType().Name ?? "null");
                    return null;
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to extract RabbitMQ channel");
            return null;
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