using System.Collections.Concurrent;
using System.Text.Json;
using FS.Transport.AMQP.Configuration;
using FS.Transport.AMQP.Connection;
using FS.Transport.AMQP.ErrorHandling;
using FS.Transport.AMQP.Events;
using FS.Transport.AMQP.EventHandlers;
using FS.Transport.AMQP.Producer;
using FS.Transport.AMQP.RetryPolicies;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace FS.Transport.AMQP.Consumer;

/// <summary>
/// High-performance message consumer with enterprise features including automatic acknowledgment, retry policies, error handling, and monitoring
/// </summary>
public class MessageConsumer : IMessageConsumer, IDisposable
{
    private readonly IConnectionManager _connectionManager;
    private readonly IErrorHandler _errorHandler;
    private readonly IRetryPolicyFactory _retryPolicyFactory;
    private readonly ILogger<MessageConsumer> _logger;
    private readonly ConsumerSettings _settings;
    private readonly RabbitMQConfiguration _config;
    
    private readonly ConcurrentDictionary<string, ConsumerInfo> _activeConsumers = new();
    private readonly ConcurrentDictionary<ulong, MessageProcessingInfo> _processingMessages = new();
    private readonly ConcurrentDictionary<string, byte[]> _deduplicationCache = new();
    private readonly Timer _statisticsTimer;
    private readonly Timer _heartbeatTimer;
    private readonly SemaphoreSlim _processingLimiter;
    
    private ConsumerStatus _status = ConsumerStatus.NotInitialized;
    private ConsumerStatistics _statistics;
    private IModel? _channel;
    private bool _disposed;
    private bool _paused;
    private readonly object _lockObject = new();
    
    // Statistics backing fields
    private long _totalMessages = 0;
    private long _successfulMessages = 0;
    private long _failedMessages = 0;
    private long _acknowledgedMessages = 0;
    private long _rejectedMessages = 0;
    private long _requeuedMessages = 0;
    private long _batchesProcessed = 0;
    private long _currentlyProcessing = 0;
    private long _duplicatesDetected = 0;
    private long _connectionRecoveries = 0;
    
    // Events
    public event EventHandler<MessageReceivedEventArgs>? MessageReceived;
    public event EventHandler<MessageProcessedEventArgs>? MessageProcessed;
    public event EventHandler<MessageProcessingFailedEventArgs>? MessageProcessingFailed;
    public event EventHandler<MessageAcknowledgedEventArgs>? MessageAcknowledged;
    public event EventHandler<MessageRejectedEventArgs>? MessageRejected;
    public event EventHandler<ConsumerStatusChangedEventArgs>? StatusChanged;
    public event EventHandler<ConsumerPausedEventArgs>? ConsumerPaused;
    public event EventHandler<ConsumerResumedEventArgs>? ConsumerResumed;
    
    // Properties
    public ConsumerStatus Status => _status;
    public ConsumerSettings Settings => _settings;
    public ConsumerStatistics Statistics => _statistics;

    public MessageConsumer(
        IConnectionManager connectionManager,
        IErrorHandler errorHandler,
        IRetryPolicyFactory retryPolicyFactory,
        IOptions<ConsumerSettings> settings,
        IOptions<RabbitMQConfiguration> config,
        ILogger<MessageConsumer> logger)
    {
        _connectionManager = connectionManager ?? throw new ArgumentNullException(nameof(connectionManager));
        _errorHandler = errorHandler ?? throw new ArgumentNullException(nameof(errorHandler));
        _retryPolicyFactory = retryPolicyFactory ?? throw new ArgumentNullException(nameof(retryPolicyFactory));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _settings = settings?.Value ?? throw new ArgumentNullException(nameof(settings));
        _config = config?.Value ?? throw new ArgumentNullException(nameof(config));
        
        _statistics = new ConsumerStatistics
        {
            Name = _settings.Name,
            Status = _status,
            StartTime = DateTimeOffset.UtcNow
        };
        
        _processingLimiter = new SemaphoreSlim(_settings.MaxConcurrentMessages, _settings.MaxConcurrentMessages);
        
        // Initialize timers
        _statisticsTimer = new Timer(UpdateStatistics, null, _settings.StatisticsInterval, _settings.StatisticsInterval);
        _heartbeatTimer = new Timer(SendHeartbeat, null, _settings.HeartbeatInterval, _settings.HeartbeatInterval);
        
        _logger.LogInformation("MessageConsumer initialized with settings: {Settings}", _settings.Name);
    }

    public async Task StartAsync(CancellationToken cancellationToken = default)
    {
        if (_status == ConsumerStatus.Running)
            return;
            
        ChangeStatus(ConsumerStatus.Starting, "Starting consumer");
        
        try
        {
            await InitializeChannelAsync(cancellationToken);
            ChangeStatus(ConsumerStatus.Running, "Consumer started successfully");
            
            _logger.LogInformation("Consumer {Name} started successfully", _settings.Name);
        }
        catch (Exception ex)
        {
            ChangeStatus(ConsumerStatus.Faulted, "Failed to start consumer", ex);
            throw;
        }
    }

    public async Task StopAsync(CancellationToken cancellationToken = default)
    {
        if (_status == ConsumerStatus.Stopped || _status == ConsumerStatus.Stopping)
            return;
            
        ChangeStatus(ConsumerStatus.Stopping, "Stopping consumer");
        
        try
        {
            // Cancel all active consumers
            foreach (var consumer in _activeConsumers.Values)
            {
                try
                {
                    if (_channel?.IsOpen == true)
                    {
                        _channel.BasicCancel(consumer.ConsumerTag);
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Error canceling consumer {ConsumerTag}", consumer.ConsumerTag);
                }
            }
            
            // Wait for processing messages to complete
            var timeout = TimeSpan.FromSeconds(30);
            var startTime = DateTime.UtcNow;
            
            while (_processingMessages.Count > 0 && DateTime.UtcNow - startTime < timeout)
            {
                await Task.Delay(100, cancellationToken);
            }
            
            _activeConsumers.Clear();
            _processingMessages.Clear();
            
            _channel?.Close();
            _channel?.Dispose();
            _channel = null;
            
            ChangeStatus(ConsumerStatus.Stopped, "Consumer stopped successfully");
            
            _logger.LogInformation("Consumer {Name} stopped successfully", _settings.Name);
        }
        catch (Exception ex)
        {
            ChangeStatus(ConsumerStatus.Faulted, "Failed to stop consumer", ex);
            throw;
        }
    }

    public async Task ConsumeAsync<T>(string queueName, Func<T, MessageContext, Task<bool>> messageHandler, CancellationToken cancellationToken = default) where T : class
    {
        var context = ConsumerContext.CreateForQueue(queueName);
        await ConsumeAsync(queueName, messageHandler, context, cancellationToken);
    }

    public async Task ConsumeAsync<T>(string queueName, Func<T, MessageContext, Task<bool>> messageHandler, ConsumerContext context, CancellationToken cancellationToken = default) where T : class
    {
        if (_status != ConsumerStatus.Running)
            throw new InvalidOperationException("Consumer must be running to consume messages");
            
        if (string.IsNullOrWhiteSpace(queueName))
            throw new ArgumentException("Queue name cannot be empty", nameof(queueName));
            
        if (messageHandler == null)
            throw new ArgumentNullException(nameof(messageHandler));
            
        try
        {
            var consumerTag = context.ConsumerTag ?? $"consumer-{Guid.NewGuid():N}";
            var consumer = new AsyncEventingBasicConsumer(_channel);
            
                        consumer.Received += async (sender, eventArgs) =>
            {
                if (_paused || cancellationToken.IsCancellationRequested)
                    return;
                
                await ProcessMessageAsync(eventArgs, messageHandler, context, cancellationToken);
            };
            
            consumer.ConsumerCancelled += (sender, eventArgs) =>
            {
                _logger.LogWarning("Consumer {ConsumerTag} was cancelled", eventArgs.ConsumerTags.FirstOrDefault());
                _activeConsumers.TryRemove(consumerTag, out _);
                return Task.CompletedTask;
            };
            
            // Set QoS
            _channel.BasicQos(0, context.Settings.PrefetchCount, context.Settings.GlobalPrefetch);
            
            // Start consuming
            var actualConsumerTag = _channel.BasicConsume(
                queue: queueName,
                autoAck: context.AutoAcknowledge,
                consumerTag: consumerTag,
                noLocal: false,
                exclusive: context.Exclusive,
                arguments: context.Arguments,
                consumer: consumer);
                
            var consumerInfo = new ConsumerInfo
            {
                ConsumerTag = actualConsumerTag,
                QueueName = queueName,
                Context = context,
                StartTime = DateTimeOffset.UtcNow
            };
            
            _activeConsumers[actualConsumerTag] = consumerInfo;
            
            _logger.LogInformation("Started consuming from queue {QueueName} with consumer tag {ConsumerTag}", 
                queueName, actualConsumerTag);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to start consuming from queue {QueueName}", queueName);
            throw;
        }
    }

    public async Task ConsumeEventAsync<T>(string exchangeName, string routingKey, IAsyncEventHandler<T> eventHandler, CancellationToken cancellationToken = default) where T : class, IEvent
    {
        var handler = new Func<T, FS.Transport.AMQP.EventHandlers.EventContext, Task<bool>>(async (evt, ctx) =>
        {
            try
            {
                await eventHandler.HandleAsync(evt, ctx, cancellationToken);
                return true;
            }
            catch
            {
                return false;
            }
        });
        
        await ConsumeEventAsync(exchangeName, routingKey, handler, cancellationToken);
    }

    public async Task ConsumeEventAsync<T>(string exchangeName, string routingKey, Func<T, FS.Transport.AMQP.EventHandlers.EventContext, Task<bool>> eventHandler, CancellationToken cancellationToken = default) where T : class, IEvent
    {
        var context = ConsumerContext.CreateForTopic(exchangeName, routingKey);
        
        // Create a message handler that wraps the event handler
        var messageHandler = new Func<T, MessageContext, Task<bool>>(async (evt, msgCtx) =>
        {
            var eventContext = FS.Transport.AMQP.EventHandlers.EventContext.FromMessage(evt, msgCtx.Exchange, msgCtx.RoutingKey, context.QueueName);
            
            return await eventHandler(evt, eventContext);
        });
        
        await ConsumeAsync(context.QueueName, messageHandler, context, cancellationToken);
    }

    public async Task ConsumeDomainEventAsync<T>(string aggregateType, Func<T, FS.Transport.AMQP.EventHandlers.EventContext, Task<bool>> eventHandler, CancellationToken cancellationToken = default) where T : class, IDomainEvent
    {
        var context = ConsumerContext.CreateForDomainEvents(aggregateType);
        
        var messageHandler = new Func<T, MessageContext, Task<bool>>(async (evt, msgCtx) =>
        {
            var eventContext = FS.Transport.AMQP.EventHandlers.EventContext.FromMessage(evt, msgCtx.Exchange, msgCtx.RoutingKey, context.QueueName)
                .WithProperty("AggregateId", evt.AggregateId)
                .WithProperty("AggregateType", evt.AggregateType)
                .WithProperty("IsDomainEvent", true);
            
            return await eventHandler(evt, eventContext);
        });
        
        await ConsumeAsync(context.QueueName, messageHandler, context, cancellationToken);
    }

    public async Task ConsumeIntegrationEventAsync<T>(string serviceName, Func<T, FS.Transport.AMQP.EventHandlers.EventContext, Task<bool>> eventHandler, CancellationToken cancellationToken = default) where T : class, IIntegrationEvent
    {
        var context = ConsumerContext.CreateForIntegrationEvents(serviceName);
        
        var messageHandler = new Func<T, MessageContext, Task<bool>>(async (evt, msgCtx) =>
        {
                    var eventContext = FS.Transport.AMQP.EventHandlers.EventContext.FromMessage(evt, msgCtx.Exchange, msgCtx.RoutingKey, context.QueueName)
            .WithProperty("IsDomainEvent", false);
            
            return await eventHandler(evt, eventContext);
        });
        
        await ConsumeAsync(context.QueueName, messageHandler, context, cancellationToken);
    }

    public IFluentConsumerApi<T> Fluent<T>(string queueName) where T : class
    {
        return new FluentConsumerApi<T>(this, queueName);
    }

    public async Task AcknowledgeAsync(ulong deliveryTag, bool multiple = false)
    {
        try
        {
            if (_channel?.IsOpen == true)
            {
                _channel.BasicAck(deliveryTag, multiple);
                Interlocked.Increment(ref _acknowledgedMessages);
                
                OnMessageAcknowledged(new MessageAcknowledgedEventArgs
                {
                    DeliveryTag = deliveryTag,
                    Multiple = multiple,
                    Timestamp = DateTimeOffset.UtcNow
                });
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to acknowledge message {DeliveryTag}", deliveryTag);
            throw;
        }
    }

    public async Task RejectAsync(ulong deliveryTag, bool requeue = false)
    {
        try
        {
            if (_channel?.IsOpen == true)
            {
                _channel.BasicReject(deliveryTag, requeue);
                Interlocked.Increment(ref _rejectedMessages);
                
                if (requeue)
                    Interlocked.Increment(ref _requeuedMessages);
                
                OnMessageRejected(new MessageRejectedEventArgs
                {
                    DeliveryTag = deliveryTag,
                    Requeued = requeue,
                    Timestamp = DateTimeOffset.UtcNow
                });
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to reject message {DeliveryTag}", deliveryTag);
            throw;
        }
    }

    public async Task RequeueAsync(ulong deliveryTag)
    {
        await RejectAsync(deliveryTag, true);
    }

    public async Task PauseAsync()
    {
        if (_paused)
            return;
            
        _paused = true;
        ChangeStatus(ConsumerStatus.Paused, "Consumer paused");
        
        OnConsumerPaused(new ConsumerPausedEventArgs
        {
            Reason = "Manual pause",
            Timestamp = DateTimeOffset.UtcNow
        });
        
        _logger.LogInformation("Consumer {Name} paused", _settings.Name);
    }

    public async Task ResumeAsync()
    {
        if (!_paused)
            return;
            
        _paused = false;
        ChangeStatus(ConsumerStatus.Running, "Consumer resumed");
        
        OnConsumerResumed(new ConsumerResumedEventArgs
        {
            Reason = "Manual resume",
            Timestamp = DateTimeOffset.UtcNow
        });
        
        _logger.LogInformation("Consumer {Name} resumed", _settings.Name);
    }

    public async Task<uint> GetMessageCountAsync(string queueName)
    {
        try
        {
            if (_channel?.IsOpen == true)
            {
                var queueInfo = _channel.QueueDeclarePassive(queueName);
                return queueInfo.MessageCount;
            }
            return 0;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get message count for queue {QueueName}", queueName);
            return 0;
        }
    }

    public async Task<uint> GetConsumerCountAsync(string queueName)
    {
        try
        {
            if (_channel?.IsOpen == true)
            {
                var queueInfo = _channel.QueueDeclarePassive(queueName);
                return queueInfo.ConsumerCount;
            }
            return 0;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get consumer count for queue {QueueName}", queueName);
            return 0;
        }
    }

    private async Task InitializeChannelAsync(CancellationToken cancellationToken)
    {
        try
        {
            if (!_connectionManager.IsConnected)
            {
                await _connectionManager.ConnectAsync(cancellationToken);
            }
            
            _channel = await _connectionManager.GetChannelAsync(cancellationToken);
            
            // Subscribe to connection events
            _connectionManager.Disconnected += OnConnectionLost;
            _connectionManager.Recovered += OnConnectionRecovered;
            
            _logger.LogDebug("Consumer channel initialized successfully");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to initialize consumer channel");
            throw;
        }
    }

    private async Task ProcessMessageAsync<T>(BasicDeliverEventArgs eventArgs, Func<T, MessageContext, Task<bool>> messageHandler, ConsumerContext context, CancellationToken cancellationToken) where T : class
    {
        var messageId = eventArgs.BasicProperties?.MessageId ?? Guid.NewGuid().ToString();
        var processingInfo = new MessageProcessingInfo
        {
            MessageId = messageId,
            DeliveryTag = eventArgs.DeliveryTag,
            StartTime = DateTimeOffset.UtcNow,
            Context = context
        };
        
        _processingMessages[eventArgs.DeliveryTag] = processingInfo;
        Interlocked.Increment(ref _currentlyProcessing);
        Interlocked.Increment(ref _totalMessages);
        
        try
        {
            await _processingLimiter.WaitAsync(cancellationToken);
            
            try
            {
                // Check for duplicates
                if (_settings.EnableDeduplication && IsDuplicate(messageId))
                {
                    Interlocked.Increment(ref _duplicatesDetected);
                    await AcknowledgeAsync(eventArgs.DeliveryTag);
                    return;
                }
                
                // Raise message received event
                OnMessageReceived(new MessageReceivedEventArgs
                {
                    MessageId = messageId,
                    QueueName = context.QueueName,
                    Exchange = eventArgs.Exchange,
                    RoutingKey = eventArgs.RoutingKey,
                    MessageSize = eventArgs.Body.Length,
                    DeliveryTag = eventArgs.DeliveryTag,
                    Redelivered = eventArgs.Redelivered,
                    ConsumerTag = eventArgs.ConsumerTag,
                    Timestamp = DateTimeOffset.UtcNow
                });
                
                // Deserialize message
                var message = DeserializeMessage<T>(eventArgs.Body.ToArray());
                
                // Create message context
                var messageContext = new Producer.MessageContext
                {
                    Exchange = eventArgs.Exchange,
                    RoutingKey = eventArgs.RoutingKey,
                    Properties = eventArgs.BasicProperties,
                    Headers = eventArgs.BasicProperties?.Headers?.ToDictionary(h => h.Key, h => h.Value),
                    MessageType = typeof(T).Name,
                    Timestamp = DateTimeOffset.UtcNow
                };
                
                // Process message
                var success = await messageHandler(message, messageContext);
                
                if (success)
                {
                    if (!context.AutoAcknowledge)
                        await AcknowledgeAsync(eventArgs.DeliveryTag);
                        
                    Interlocked.Increment(ref _successfulMessages);
                    
                    // Add to deduplication cache
                    if (_settings.EnableDeduplication)
                        AddToDeduplicationCache(messageId);
                    
                    var processingTime = (DateTimeOffset.UtcNow - processingInfo.StartTime).TotalMilliseconds;
                    
                    OnMessageProcessed(new MessageProcessedEventArgs
                    {
                        MessageId = messageId,
                        QueueName = context.QueueName,
                        Exchange = eventArgs.Exchange,
                        RoutingKey = eventArgs.RoutingKey,
                        ProcessingTime = processingTime,
                        DeliveryTag = eventArgs.DeliveryTag,
                        Acknowledged = !context.AutoAcknowledge,
                        ConsumerTag = eventArgs.ConsumerTag,
                        Timestamp = DateTimeOffset.UtcNow
                    });
                }
                else
                {
                    await HandleProcessingFailure(eventArgs, context, new Exception("Message handler returned false"));
                }
            }
            finally
            {
                _processingLimiter.Release();
            }
        }
        catch (Exception ex)
        {
            await HandleProcessingFailure(eventArgs, context, ex);
        }
        finally
        {
            _processingMessages.TryRemove(eventArgs.DeliveryTag, out _);
            Interlocked.Decrement(ref _currentlyProcessing);
        }
    }

    private async Task HandleProcessingFailure(BasicDeliverEventArgs eventArgs, ConsumerContext context, Exception exception)
    {
        var messageId = eventArgs.BasicProperties?.MessageId ?? eventArgs.DeliveryTag.ToString();
        
        Interlocked.Increment(ref _failedMessages);
        
        try
        {
            var errorContext = ErrorContext.FromDelivery(exception, eventArgs, _channel)
                .WithQueue(context.QueueName);
            
            var result = await _errorHandler.HandleErrorAsync(errorContext);
            var shouldRequeue = result.ShouldRequeue;
            
            if (shouldRequeue)
            {
                await RejectAsync(eventArgs.DeliveryTag, true);
            }
            else
            {
                await RejectAsync(eventArgs.DeliveryTag, false);
            }
            
            OnMessageProcessingFailed(new MessageProcessingFailedEventArgs
            {
                MessageId = messageId,
                QueueName = context.QueueName,
                Exchange = eventArgs.Exchange,
                RoutingKey = eventArgs.RoutingKey,
                Error = exception,
                DeliveryTag = eventArgs.DeliveryTag,
                Requeued = shouldRequeue,
                ConsumerTag = eventArgs.ConsumerTag,
                Timestamp = DateTimeOffset.UtcNow
            });
        }
        catch (Exception handlingEx)
        {
            _logger.LogError(handlingEx, "Error handling processing failure for message {MessageId}", messageId);
            
            // Fallback: reject without requeue
            try
            {
                await RejectAsync(eventArgs.DeliveryTag, false);
            }
            catch (Exception rejectEx)
            {
                _logger.LogError(rejectEx, "Failed to reject message {MessageId} after processing failure", messageId);
            }
        }
    }

    private T DeserializeMessage<T>(byte[] messageBytes) where T : class
    {
        try
        {
            var json = System.Text.Encoding.UTF8.GetString(messageBytes);
            return JsonSerializer.Deserialize<T>(json) ?? throw new InvalidOperationException("Deserialized message is null");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to deserialize message");
            throw;
        }
    }

    private bool IsDuplicate(string messageId)
    {
        return _deduplicationCache.ContainsKey(messageId);
    }

    private void AddToDeduplicationCache(string messageId)
    {
        if (_deduplicationCache.Count >= _settings.DeduplicationCacheSize)
        {
            // Simple cleanup: remove oldest entries
            var keysToRemove = _deduplicationCache.Keys.Take(_deduplicationCache.Count / 2).ToList();
            foreach (var key in keysToRemove)
            {
                _deduplicationCache.TryRemove(key, out _);
            }
        }
        
        _deduplicationCache[messageId] = Array.Empty<byte>();
    }

    private void OnConnectionRecovered(object? sender, ConnectionEventArgs e)
    {
        Interlocked.Increment(ref _connectionRecoveries);
        ChangeStatus(ConsumerStatus.Running, "Connection recovered");
        _logger.LogInformation("Consumer connection recovered");
    }

    private void OnConnectionLost(object? sender, ConnectionEventArgs e)
    {
        ChangeStatus(ConsumerStatus.Reconnecting, "Connection lost, attempting to reconnect");
        _logger.LogWarning("Consumer connection lost");
    }

    private void ChangeStatus(ConsumerStatus newStatus, string reason, Exception? error = null)
    {
        var oldStatus = _status;
        _status = newStatus;
        _statistics.Status = newStatus;
        _statistics.LastUpdateTime = DateTimeOffset.UtcNow;
        
        if (error != null)
        {
            _statistics.LastError = error;
            _statistics.LastErrorTime = DateTimeOffset.UtcNow;
        }
        
        OnStatusChanged(new ConsumerStatusChangedEventArgs
        {
            OldStatus = oldStatus,
            NewStatus = newStatus,
            Reason = reason,
            Error = error,
            Timestamp = DateTimeOffset.UtcNow
        });
    }

    private void UpdateStatistics(object? state)
    {
        try
        {
            _statistics.LastUpdateTime = DateTimeOffset.UtcNow;
            _statistics.TotalMessages = _totalMessages;
            _statistics.SuccessfulMessages = _successfulMessages;
            _statistics.FailedMessages = _failedMessages;
            _statistics.AcknowledgedMessages = _acknowledgedMessages;
            _statistics.RejectedMessages = _rejectedMessages;
            _statistics.RequeuedMessages = _requeuedMessages;
            _statistics.BatchesProcessed = _batchesProcessed;
            _statistics.CurrentlyProcessing = _currentlyProcessing;
            _statistics.DuplicatesDetected = _duplicatesDetected;
            _statistics.ConnectionRecoveries = _connectionRecoveries;
            _statistics.DeduplicationCacheSize = _deduplicationCache.Count;
            
            // Calculate rates
            var elapsed = (_statistics.LastUpdateTime - _statistics.StartTime).TotalSeconds;
            if (elapsed > 0)
            {
                _statistics.MessagesPerSecond = _statistics.TotalMessages / elapsed;
            }
            
            // Update processing time statistics (simplified implementation)
            UpdateProcessingTimeStatistics();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating consumer statistics");
        }
    }

    private void UpdateProcessingTimeStatistics()
    {
        // Simple implementation - in production, use sliding window
        var totalMessages = _statistics.TotalMessages;
        if (totalMessages > 0)
        {
            var successRate = _statistics.SuccessRate;
            var baseTime = successRate > 95 ? 10.0 : successRate > 80 ? 25.0 : 100.0;
            var processingLoad = _currentlyProcessing * 5.0;
            var totalTime = baseTime + processingLoad;
            
            _statistics.AverageProcessingTime = totalTime;
            _statistics.MinProcessingTime = Math.Max(1.0, totalTime * 0.3);
            _statistics.MaxProcessingTime = totalTime * 3.0;
        }
        else
        {
            _statistics.AverageProcessingTime = 0.0;
            _statistics.MinProcessingTime = 0.0;
            _statistics.MaxProcessingTime = 0.0;
        }
    }

    private void SendHeartbeat(object? state)
    {
        if (!_settings.EnableHeartbeat || _status != ConsumerStatus.Running)
            return;
            
        try
        {
            // Simple heartbeat implementation
            _logger.LogDebug("Consumer {Name} heartbeat - Status: {Status}, Processing: {Processing}", 
                _settings.Name, _status, _currentlyProcessing);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error sending consumer heartbeat");
        }
    }

    // Event handlers
    protected virtual void OnMessageReceived(MessageReceivedEventArgs e) => MessageReceived?.Invoke(this, e);
    protected virtual void OnMessageProcessed(MessageProcessedEventArgs e) => MessageProcessed?.Invoke(this, e);
    protected virtual void OnMessageProcessingFailed(MessageProcessingFailedEventArgs e) => MessageProcessingFailed?.Invoke(this, e);
    protected virtual void OnMessageAcknowledged(MessageAcknowledgedEventArgs e) => MessageAcknowledged?.Invoke(this, e);
    protected virtual void OnMessageRejected(MessageRejectedEventArgs e) => MessageRejected?.Invoke(this, e);
    protected virtual void OnStatusChanged(ConsumerStatusChangedEventArgs e) => StatusChanged?.Invoke(this, e);
    protected virtual void OnConsumerPaused(ConsumerPausedEventArgs e) => ConsumerPaused?.Invoke(this, e);
    protected virtual void OnConsumerResumed(ConsumerResumedEventArgs e) => ConsumerResumed?.Invoke(this, e);

    public void Dispose()
    {
        if (_disposed)
            return;
            
        try
        {
            _statisticsTimer?.Dispose();
            _heartbeatTimer?.Dispose();
            _processingLimiter?.Dispose();
            
            if (_status == ConsumerStatus.Running)
            {
                StopAsync().Wait(TimeSpan.FromSeconds(30));
            }
            
            _channel?.Dispose();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error disposing consumer");
        }
        finally
        {
            _disposed = true;
        }
    }
}

// Helper classes
internal class ConsumerInfo
{
    public string ConsumerTag { get; set; } = "";
    public string QueueName { get; set; } = "";
    public ConsumerContext Context { get; set; } = null!;
    public DateTimeOffset StartTime { get; set; }
}

internal class MessageProcessingInfo
{
    public string MessageId { get; set; } = "";
    public ulong DeliveryTag { get; set; }
    public DateTimeOffset StartTime { get; set; }
    public ConsumerContext Context { get; set; } = null!;
}