using System.Text.Json;
using FS.Transport.AMQP.Configuration;
using FS.Transport.AMQP.Connection;
using FS.Transport.AMQP.Core.Extensions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using RabbitMQ.Client;

namespace FS.Transport.AMQP.ErrorHandling;

/// <summary>
/// Handles dead letter operations with comprehensive error information preservation
/// </summary>
public class DeadLetterHandler : IDeadLetterHandler
{
    private readonly ErrorHandlingSettings _settings;
    private readonly IConnectionManager _connectionManager;
    private readonly ILogger<DeadLetterHandler> _logger;
    private readonly DeadLetterStatistics _statistics;
    private volatile bool _infrastructureSetup;

    public DeadLetterHandler(
        IOptions<RabbitMQConfiguration> configuration,
        IConnectionManager connectionManager,
        ILogger<DeadLetterHandler> logger)
    {
        _settings = configuration?.Value?.ErrorHandling ?? throw new ArgumentNullException(nameof(configuration));
        _connectionManager = connectionManager ?? throw new ArgumentNullException(nameof(connectionManager));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _statistics = new DeadLetterStatistics();
    }

    /// <summary>
    /// Sends a message to dead letter with comprehensive error information
    /// </summary>
    public async Task<bool> SendToDeadLetterAsync(ErrorContext context, CancellationToken cancellationToken = default)
    {
        if (context == null)
            throw new ArgumentNullException(nameof(context));

        try
        {
            _statistics.TotalMessages++;
            var startTime = DateTime.UtcNow;

            using var channel = await _connectionManager.GetChannelAsync(cancellationToken);
            
            // Ensure dead letter infrastructure exists
            if (!_infrastructureSetup)
            {
                await SetupDeadLetterInfrastructureAsync(channel, cancellationToken);
            }

            // Create dead letter message with error information
            var deadLetterMessage = CreateDeadLetterMessage(context);
            var properties = CreateDeadLetterProperties(context, channel);
            
            // Publish to dead letter exchange
            channel.SafeBasicPublish(
                _settings.DeadLetterExchange,
                _settings.DeadLetterRoutingKey,
                deadLetterMessage,
                properties,
                mandatory: false
            );

            _connectionManager.ReturnChannel(channel);
            
            var duration = DateTime.UtcNow - startTime;
            _statistics.SuccessfulMessages++;
            _statistics.AverageProcessingTime = CalculateAverageTime(duration);
            
            _logger.LogInformation("Message sent to dead letter queue in {Duration}ms: {MessageId}", 
                duration.TotalMilliseconds, context.MessageId);

            return true;
        }
        catch (Exception ex)
        {
            _statistics.FailedMessages++;
            _logger.LogError(ex, "Failed to send message to dead letter queue: {ErrorContext}", context.ToString());
            return false;
        }
    }

    /// <summary>
    /// Sets up dead letter exchange and queue infrastructure
    /// </summary>
    public async Task<bool> SetupDeadLetterInfrastructureAsync(IModel channel, CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogDebug("Setting up dead letter infrastructure");

            // Declare dead letter exchange
            channel.ExchangeDeclare(
                exchange: _settings.DeadLetterExchange,
                type: ExchangeType.Topic,
                durable: true,
                autoDelete: false,
                arguments: null
            );

            // Declare dead letter queue
            var queueArgs = new Dictionary<string, object>();
            
            channel.QueueDeclare(
                queue: _settings.DeadLetterQueue,
                durable: true,
                exclusive: false,
                autoDelete: false,
                arguments: queueArgs
            );

            // Bind queue to exchange
            channel.QueueBind(
                queue: _settings.DeadLetterQueue,
                exchange: _settings.DeadLetterExchange,
                routingKey: _settings.DeadLetterRoutingKey,
                arguments: null
            );

            _infrastructureSetup = true;
            _logger.LogInformation("Dead letter infrastructure setup completed successfully");

            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to setup dead letter infrastructure");
            return false;
        }
    }

    /// <summary>
    /// Gets dead letter statistics
    /// </summary>
    public DeadLetterStatistics GetStatistics()
    {
        return _statistics.Clone();
    }

    private byte[] CreateDeadLetterMessage(ErrorContext context)
    {
        var deadLetterInfo = new DeadLetterMessage
        {
            MessageId = context.MessageId ?? Guid.NewGuid().ToString(),
            CorrelationId = context.CorrelationId,
            OriginalTimestamp = context.OriginalTimestamp ?? DateTime.UtcNow,
            ErrorTimestamp = context.ErrorTimestamp,
            AttemptCount = context.AttemptCount,
            OriginalExchange = context.ExchangeName,
            OriginalRoutingKey = context.RoutingKey,
            OriginalQueue = context.QueueName,
            ConsumerTag = context.ConsumerTag,
            Operation = context.Operation,
            Exception = new DeadLetterException
            {
                Type = context.Exception.GetType().FullName ?? context.Exception.GetType().Name,
                Message = TruncateErrorMessage(context.Exception.Message),
                StackTrace = _settings.IncludeErrorDetails ? context.Exception.StackTrace : null,
                Source = context.Exception.Source
            },
            Headers = context.Headers,
            Properties = context.Properties,
            OriginalMessage = context.MessageBody?.ToArray()
        };

        var options = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            WriteIndented = false
        };

        return JsonSerializer.SerializeToUtf8Bytes(deadLetterInfo, options);
    }

    private IBasicProperties CreateDeadLetterProperties(ErrorContext context, IModel channel)
    {
        var properties = channel.CreateBasicProperties();
        
        properties.MessageId = context.MessageId ?? Guid.NewGuid().ToString();
        properties.CorrelationId = context.CorrelationId;
        properties.Timestamp = new AmqpTimestamp(((DateTimeOffset)context.ErrorTimestamp).ToUnixTimeSeconds());
                    properties.ContentType = FS.Transport.AMQP.Core.Constants.ContentTypes.Json;
        properties.DeliveryMode = 2; // Persistent
        
        // Add headers with error information
        properties.Headers = new Dictionary<string, object>();
        
        if (_settings.IncludeOriginalHeaders && context.Headers != null)
        {
            foreach (var header in context.Headers)
            {
                properties.Headers[$"original-{header.Key}"] = header.Value;
            }
        }
        
        // Add dead letter specific headers
                    properties.Headers[FS.Transport.AMQP.Core.Constants.Headers.DeathReason] = TruncateErrorMessage(context.Exception.Message);
            properties.Headers[FS.Transport.AMQP.Core.Constants.Headers.RetryCount] = context.AttemptCount;
            properties.Headers[FS.Transport.AMQP.Core.Constants.Headers.OriginalExchange] = context.ExchangeName ?? "";
            properties.Headers[FS.Transport.AMQP.Core.Constants.Headers.OriginalRoutingKey] = context.RoutingKey ?? "";
        properties.Headers["x-death-timestamp"] = context.ErrorTimestamp.ToString("O");
        properties.Headers["x-exception-type"] = context.Exception.GetType().Name;
        
        if (!string.IsNullOrEmpty(context.QueueName))
        {
            properties.Headers["x-original-queue"] = context.QueueName;
        }
        
        if (!string.IsNullOrEmpty(context.Operation))
        {
            properties.Headers["x-failed-operation"] = context.Operation;
        }

        return properties;
    }

    private string TruncateErrorMessage(string message)
    {
        if (string.IsNullOrEmpty(message))
            return "No error message";
            
        return message.Length > _settings.MaxErrorMessageLength 
            ? message.Substring(0, _settings.MaxErrorMessageLength) + "..." 
            : message;
    }

    private TimeSpan CalculateAverageTime(TimeSpan currentDuration)
    {
        var totalProcessed = _statistics.SuccessfulMessages + _statistics.FailedMessages;
        if (totalProcessed <= 1)
            return currentDuration;

        var currentAverage = _statistics.AverageProcessingTime;
        var newAverage = ((currentAverage.TotalMilliseconds * (totalProcessed - 1)) + currentDuration.TotalMilliseconds) / totalProcessed;
        
        return TimeSpan.FromMilliseconds(newAverage);
    }
}