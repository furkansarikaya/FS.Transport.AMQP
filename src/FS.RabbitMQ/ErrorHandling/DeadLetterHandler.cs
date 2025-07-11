using Microsoft.Extensions.Logging;
using RabbitMQ.Client;
using System.Text;
using System.Text.Json;
using System.Collections.Concurrent;
using FS.RabbitMQ.Connection;
using FS.RabbitMQ.Core.Extensions;

namespace FS.RabbitMQ.ErrorHandling;

/// <summary>
/// Handles dead letter messages in RabbitMQ with comprehensive error tracking and recovery options
/// </summary>
public class DeadLetterHandler : IDeadLetterHandler
{
    private readonly IConnectionManager _connectionManager;
    private readonly DeadLetterSettings _settings;
    private readonly ILogger<DeadLetterHandler> _logger;
    private readonly DeadLetterStatistics _statistics;
    private readonly ConcurrentDictionary<string, DeadLetterMessage> _deadLetterMessages;
    private readonly SemaphoreSlim _operationLock;
    private volatile bool _disposed;

    /// <summary>
    /// Initializes a new instance of the DeadLetterHandler
    /// </summary>
    /// <param name="connectionManager">Connection manager instance</param>
    /// <param name="settings">Dead letter settings</param>
    /// <param name="logger">Logger instance</param>
    public DeadLetterHandler(
        IConnectionManager connectionManager,
        DeadLetterSettings settings,
        ILogger<DeadLetterHandler> logger)
    {
        _connectionManager = connectionManager ?? throw new ArgumentNullException(nameof(connectionManager));
        _settings = settings ?? throw new ArgumentNullException(nameof(settings));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _statistics = new DeadLetterStatistics();
        _deadLetterMessages = new ConcurrentDictionary<string, DeadLetterMessage>();
        _operationLock = new SemaphoreSlim(1, 1);
    }

    /// <summary>
    /// Handles a dead letter message by sending it to the dead letter queue
    /// </summary>
    /// <param name="context">Error context containing the message and error information</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Task representing the handling operation</returns>
    public async Task<bool> HandleDeadLetterAsync(ErrorContext context, CancellationToken cancellationToken = default)
    {
        if (_disposed)
            throw new ObjectDisposedException(nameof(DeadLetterHandler));

        if (context == null)
            throw new ArgumentNullException(nameof(context));

        await _operationLock.WaitAsync(cancellationToken);
        
        try
        {
            _logger.LogWarning("Handling dead letter message: {MessageId} from {Exchange}/{RoutingKey}",
                context.MessageProperties?.MessageId, context.ExchangeName, context.RoutingKey);

            // Create dead letter message
            var deadLetterMessage = CreateDeadLetterMessage(context);
            
            // Store in memory tracking
            _deadLetterMessages[deadLetterMessage.MessageId] = deadLetterMessage;
            
            // Send to dead letter queue
            var success = await SendToDeadLetterQueueAsync(context, deadLetterMessage, cancellationToken);
            
            if (success)
            {
                _statistics.TotalDeadLetters++;
                _statistics.LastDeadLetterTime = DateTimeOffset.UtcNow;
                _logger.LogInformation("Dead letter message handled successfully: {MessageId}", deadLetterMessage.MessageId);
            }
            else
            {
                _statistics.FailedDeadLetters++;
                _logger.LogError("Failed to handle dead letter message: {MessageId}", deadLetterMessage.MessageId);
            }
            
            return success;
        }
        catch (Exception ex)
        {
            _statistics.FailedDeadLetters++;
            _logger.LogError(ex, "Error handling dead letter message");
            return false;
        }
        finally
        {
            _operationLock.Release();
        }
    }

    /// <summary>
    /// Sets up the dead letter infrastructure (exchanges, queues, bindings)
    /// </summary>
    /// <param name="channel">The RabbitMQ channel to use for setup</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Task representing the setup operation</returns>
    public async Task<bool> SetupDeadLetterInfrastructureAsync(IChannel channel, CancellationToken cancellationToken = default)
    {
        if (_disposed)
            throw new ObjectDisposedException(nameof(DeadLetterHandler));

        if (channel == null)
            throw new ArgumentNullException(nameof(channel));

        try
        {
            _logger.LogInformation("Setting up dead letter infrastructure");

            // Declare dead letter exchange
            await channel.ExchangeDeclareAsync(
                exchange: _settings.DeadLetterExchange,
                type: "direct",
                durable: true,
                autoDelete: false,
                arguments: null,
                cancellationToken: cancellationToken);

            // Declare dead letter queue
            var queueArguments = new Dictionary<string, object>();
            if (_settings.MessageTtl.HasValue)
            {
                queueArguments["x-message-ttl"] = (int)_settings.MessageTtl.Value.TotalMilliseconds;
            }

            await channel.QueueDeclareAsync(
                queue: _settings.DeadLetterQueue,
                durable: true,
                exclusive: false,
                autoDelete: false,
                arguments: queueArguments,
                cancellationToken: cancellationToken);

            // Bind dead letter queue to exchange
            await channel.QueueBindAsync(
                queue: _settings.DeadLetterQueue,
                exchange: _settings.DeadLetterExchange,
                routingKey: _settings.DeadLetterRoutingKey,
                arguments: null,
                cancellationToken: cancellationToken);

            _logger.LogInformation("Dead letter infrastructure setup completed");
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to setup dead letter infrastructure");
            return false;
        }
    }

    /// <summary>
    /// Gets statistics about dead letter handling
    /// </summary>
    /// <returns>Dead letter statistics</returns>
    public DeadLetterStatistics GetStatistics()
    {
        var stats = new DeadLetterStatistics
        {
            TotalDeadLetters = _statistics.TotalDeadLetters,
            FailedDeadLetters = _statistics.FailedDeadLetters,
            LastDeadLetterTime = _statistics.LastDeadLetterTime,
            ActiveDeadLetters = _deadLetterMessages.Count
        };

        return stats;
    }

    /// <summary>
    /// Requeues a message from the dead letter queue back to the original queue
    /// </summary>
    /// <param name="messageId">The message ID to requeue</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Task representing the requeue operation</returns>
    public async Task<bool> RequeueFromDeadLetterAsync(string messageId, CancellationToken cancellationToken = default)
    {
        if (_disposed)
            throw new ObjectDisposedException(nameof(DeadLetterHandler));

        if (string.IsNullOrEmpty(messageId))
            throw new ArgumentException("Message ID cannot be null or empty", nameof(messageId));

        if (!_deadLetterMessages.TryGetValue(messageId, out var deadLetterMessage))
        {
            _logger.LogWarning("Dead letter message not found: {MessageId}", messageId);
            return false;
        }

        try
        {
            _logger.LogInformation("Requeuing dead letter message: {MessageId}", messageId);

            using var channel = await _connectionManager.CreateChannelAsync(cancellationToken);
            
            // Recreate the original message properties
            var properties = new BasicProperties
            {
                MessageId = deadLetterMessage.OriginalMessageId,
                CorrelationId = deadLetterMessage.CorrelationId,
                ContentType = deadLetterMessage.ContentType,
                DeliveryMode = (DeliveryModes)deadLetterMessage.DeliveryMode,
                Priority = deadLetterMessage.Priority,
                Headers = new Dictionary<string, object>()
            };

            // Add requeue tracking header
            properties.Headers["x-requeued-from-dlq"] = true;
            properties.Headers["x-requeue-count"] = (deadLetterMessage.RequeueCount + 1).ToString();
            properties.Headers["x-requeue-time"] = DateTimeOffset.UtcNow.ToString("O");

            // Publish back to original exchange
            await channel.BasicPublishAsync(
                exchange: deadLetterMessage.OriginalExchange,
                routingKey: deadLetterMessage.OriginalRoutingKey,
                mandatory: false,
                basicProperties: properties,
                body: deadLetterMessage.MessageBody);

            // Update tracking
            deadLetterMessage.RequeueCount++;
            deadLetterMessage.LastRequeueTime = DateTimeOffset.UtcNow;
            
            _logger.LogInformation("Dead letter message requeued successfully: {MessageId}", messageId);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to requeue dead letter message: {MessageId}", messageId);
            return false;
        }
    }

    /// <summary>
    /// Purges all messages from the dead letter queue
    /// </summary>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Task representing the purge operation</returns>
    public async Task<bool> PurgeDeadLetterQueueAsync(CancellationToken cancellationToken = default)
    {
        if (_disposed)
            throw new ObjectDisposedException(nameof(DeadLetterHandler));

        try
        {
            _logger.LogInformation("Purging dead letter queue: {Queue}", _settings.DeadLetterQueue);

            using var channel = await _connectionManager.CreateChannelAsync(cancellationToken);
            
            var purgeResult = await channel.QueuePurgeAsync(_settings.DeadLetterQueue, cancellationToken);
            
            // Clear in-memory tracking
            _deadLetterMessages.Clear();
            
            _logger.LogInformation("Dead letter queue purged successfully. {MessageCount} messages removed", purgeResult);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to purge dead letter queue");
            return false;
        }
    }

    /// <summary>
    /// Gets the configuration settings for dead letter handling
    /// </summary>
    /// <returns>Dead letter configuration settings</returns>
    public DeadLetterSettings GetSettings()
    {
        return _settings;
    }

    private DeadLetterMessage CreateDeadLetterMessage(ErrorContext context)
    {
        var messageId = context.MessageProperties?.MessageId ?? Guid.NewGuid().ToString();
        
        return new DeadLetterMessage
        {
            MessageId = Guid.NewGuid().ToString(),
            OriginalMessageId = messageId,
            OriginalExchange = context.ExchangeName ?? string.Empty,
            OriginalRoutingKey = context.RoutingKey ?? string.Empty,
            OriginalQueue = context.QueueName ?? string.Empty,
            MessageBody = context.MessageBody,
            ContentType = context.MessageProperties?.ContentType ?? "application/json",
            DeliveryMode = (byte)(context.MessageProperties?.DeliveryMode ?? DeliveryModes.Transient),
            Priority = context.MessageProperties?.Priority ?? 0,
            CorrelationId = context.MessageProperties?.CorrelationId ?? string.Empty,
            ErrorMessage = context.Exception.Message,
            ErrorType = context.Exception.GetType().Name,
            ErrorStackTrace = context.Exception.StackTrace ?? string.Empty,
            RetryCount = context.RetryCount,
            FirstErrorTime = DateTimeOffset.UtcNow,
            LastErrorTime = DateTimeOffset.UtcNow,
            RequeueCount = 0,
            Headers = ExtractHeaders(context.MessageProperties),
            AdditionalData = context.AdditionalData.ToDictionary(kvp => kvp.Key, kvp => kvp.Value)
        };
    }

    private async Task<bool> SendToDeadLetterQueueAsync(ErrorContext context, DeadLetterMessage deadLetterMessage, CancellationToken cancellationToken)
    {
        try
        {
            using var channel = await _connectionManager.CreateChannelAsync(cancellationToken);
            
            // Create properties for dead letter message
            var properties = CreateDeadLetterProperties(context, channel);
            
            // Serialize the dead letter message
            var messageBody = JsonSerializer.SerializeToUtf8Bytes(deadLetterMessage);
            
            // Publish to dead letter exchange
            await channel.BasicPublishAsync(
                exchange: _settings.DeadLetterExchange,
                routingKey: _settings.DeadLetterRoutingKey,
                body: messageBody,
                mandatory: false,
                cancellationToken: cancellationToken);

            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to send message to dead letter queue");
            return false;
        }
    }

    private IBasicProperties CreateDeadLetterProperties(ErrorContext context, IChannel channel)
    {
        var properties = new BasicProperties
        {
            MessageId = Guid.NewGuid().ToString(),
            Timestamp = new AmqpTimestamp(DateTimeOffset.UtcNow.ToUnixTimeSeconds()),
            ContentType = "application/json",
            DeliveryMode = DeliveryModes.Persistent,
            Headers = new Dictionary<string, object>
            {
                ["x-dead-letter-time"] = DateTimeOffset.UtcNow.ToString("O"),
                ["x-original-exchange"] = context.ExchangeName ?? string.Empty,
                ["x-original-routing-key"] = context.RoutingKey ?? string.Empty,
                ["x-original-queue"] = context.QueueName ?? string.Empty,
                ["x-error-type"] = context.Exception.GetType().Name,
                ["x-error-message"] = context.Exception.Message,
                ["x-retry-count"] = context.RetryCount.ToString(),
                ["x-max-retries"] = context.MaxRetries.ToString()
            }
        };

        // Add original message properties if available
        if (context.MessageProperties != null)
        {
            properties.Headers["x-original-message-id"] = context.MessageProperties.MessageId ?? string.Empty;
            properties.Headers["x-original-correlation-id"] = context.MessageProperties.CorrelationId ?? string.Empty;
            properties.Headers["x-original-content-type"] = context.MessageProperties.ContentType ?? string.Empty;
            
            if (context.MessageProperties.Headers != null)
            {
                foreach (var header in context.MessageProperties.Headers)
                {
                    properties.Headers[$"x-original-header-{header.Key}"] = header.Value;
                }
            }
        }

        return properties;
    }

    private Dictionary<string, object> ExtractHeaders(IReadOnlyBasicProperties? properties)
    {
        var headers = new Dictionary<string, object>();
        
        if (properties?.Headers != null)
        {
            foreach (var header in properties.Headers)
            {
                headers[header.Key] = header.Value;
            }
        }
        
        return headers;
    }

    /// <summary>
    /// Disposes the dead letter handler and all its resources
    /// </summary>
    public void Dispose()
    {
        if (_disposed)
            return;

        _disposed = true;
        
        try
        {
            _operationLock?.Dispose();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during disposal");
        }
    }
}