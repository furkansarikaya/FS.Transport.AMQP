using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System.Text;

namespace FS.RabbitMQ.ErrorHandling;

/// <summary>
/// Provides context information for error handling operations
/// </summary>
public class ErrorContext
{
    /// <summary>
    /// Gets or sets the exception that occurred
    /// </summary>
    public Exception Exception { get; set; } = null!;

    /// <summary>
    /// Gets or sets the error message
    /// </summary>
    public string? Message { get; set; }

    /// <summary>
    /// Gets or sets the channel associated with the error
    /// </summary>
    public IChannel? Channel { get; set; }

    /// <summary>
    /// Gets or sets the delivery arguments for the message that caused the error
    /// </summary>
    public BasicDeliverEventArgs? DeliveryArgs { get; set; }

    /// <summary>
    /// Gets or sets the queue name where the error occurred
    /// </summary>
    public string? QueueName { get; set; }

    /// <summary>
    /// Gets or sets the exchange name where the error occurred
    /// </summary>
    public string? ExchangeName { get; set; }

    /// <summary>
    /// Gets or sets the routing key associated with the error
    /// </summary>
    public string? RoutingKey { get; set; }

    /// <summary>
    /// Gets or sets the correlation ID for the message
    /// </summary>
    public string? CorrelationId { get; set; }

    /// <summary>
    /// Gets or sets the message body that caused the error
    /// </summary>
    public ReadOnlyMemory<byte> MessageBody { get; set; }

    /// <summary>
    /// Gets or sets the message properties
    /// </summary>
    public IReadOnlyBasicProperties? MessageProperties { get; set; }

    /// <summary>
    /// Gets or sets the delivery tag of the message
    /// </summary>
    public ulong DeliveryTag { get; set; }

    /// <summary>
    /// Gets or sets the number of retry attempts made
    /// </summary>
    public int RetryCount { get; set; }

    /// <summary>
    /// Gets or sets the maximum number of retry attempts allowed
    /// </summary>
    public int MaxRetries { get; set; }

    /// <summary>
    /// Gets or sets when the error occurred
    /// </summary>
    public DateTimeOffset Timestamp { get; set; } = DateTimeOffset.UtcNow;

    /// <summary>
    /// The attempt count for retry scenarios
    /// </summary>
    public int AttemptCount { get; set; } = 1;

    /// <summary>
    /// Additional context data
    /// </summary>
    public Dictionary<string, object> AdditionalData { get; set; } = new();

    /// <summary>
    /// Gets or sets the consumer tag associated with the error
    /// </summary>
    public string? ConsumerTag { get; set; }

    /// <summary>
    /// Gets or sets whether the message was redelivered
    /// </summary>
    public bool Redelivered { get; set; }

    /// <summary>
    /// Gets the message body as a string using UTF-8 encoding
    /// </summary>
    public string GetMessageBodyAsString()
    {
        if (MessageBody.IsEmpty)
            return string.Empty;

        try
        {
            return Encoding.UTF8.GetString(MessageBody.Span);
        }
        catch
        {
            return Convert.ToBase64String(MessageBody.Span);
        }
    }

    /// <summary>
    /// Gets the message body as bytes
    /// </summary>
    public byte[] GetMessageBodyAsBytes()
    {
        return MessageBody.ToArray();
    }

    /// <summary>
    /// Adds additional context data
    /// </summary>
    /// <param name="key">The key for the data</param>
    /// <param name="value">The value to store</param>
    public void AddContextData(string key, object? value)
    {
        AdditionalData[key] = value;
    }

    /// <summary>
    /// Gets additional context data
    /// </summary>
    /// <typeparam name="T">The type of the data</typeparam>
    /// <param name="key">The key for the data</param>
    /// <returns>The data value or default if not found</returns>
    public T? GetContextData<T>(string key)
    {
        if (AdditionalData.TryGetValue(key, out var value))
        {
            try
            {
                return (T?)value;
            }
            catch
            {
                return default;
            }
        }
        return default;
    }

    /// <summary>
    /// Creates an ErrorContext from a BasicDeliverEventArgs
    /// </summary>
    /// <param name="exception">The exception that occurred</param>
    /// <param name="deliveryArgs">The delivery arguments</param>
    /// <param name="channel">The channel associated with the error</param>
    /// <returns>A new ErrorContext instance</returns>
    public static ErrorContext FromDelivery(Exception exception, BasicDeliverEventArgs deliveryArgs, IChannel channel)
    {
        if (exception == null) throw new ArgumentNullException(nameof(exception));
        if (deliveryArgs == null) throw new ArgumentNullException(nameof(deliveryArgs));
        if (channel == null) throw new ArgumentNullException(nameof(channel));

        return new ErrorContext
        {
            Exception = exception,
            Channel = channel,
            DeliveryArgs = deliveryArgs,
            QueueName = deliveryArgs.DeliveryTag.ToString(), // Queue name might not be available directly
            ExchangeName = deliveryArgs.Exchange,
            RoutingKey = deliveryArgs.RoutingKey,
            MessageBody = deliveryArgs.Body,
            MessageProperties = deliveryArgs.BasicProperties,
            DeliveryTag = deliveryArgs.DeliveryTag,
            ConsumerTag = deliveryArgs.ConsumerTag,
            Redelivered = deliveryArgs.Redelivered,
            Timestamp = DateTimeOffset.UtcNow
        };
    }

    /// <summary>
    /// Creates an ErrorContext from basic information
    /// </summary>
    /// <param name="exception">The exception that occurred</param>
    /// <param name="exchangeName">The exchange name</param>
    /// <param name="routingKey">The routing key</param>
    /// <param name="messageBody">The message body</param>
    /// <param name="channel">The channel associated with the error</param>
    /// <returns>A new ErrorContext instance</returns>
    public static ErrorContext FromBasicInfo(Exception exception, string exchangeName, string routingKey, ReadOnlyMemory<byte> messageBody, IChannel? channel = null)
    {
        if (exception == null) throw new ArgumentNullException(nameof(exception));

        return new ErrorContext
        {
            Exception = exception,
            Channel = channel,
            ExchangeName = exchangeName,
            RoutingKey = routingKey,
            MessageBody = messageBody,
            Timestamp = DateTimeOffset.UtcNow
        };
    }

    /// <summary>
    /// Creates a copy of the ErrorContext for retry scenarios
    /// </summary>
    /// <returns>A new ErrorContext instance with incremented retry count</returns>
    public ErrorContext CreateRetryContext()
    {
        var retryContext = new ErrorContext
        {
            Exception = Exception,
            Channel = Channel,
            DeliveryArgs = DeliveryArgs,
            QueueName = QueueName,
            ExchangeName = ExchangeName,
            RoutingKey = RoutingKey,
            MessageBody = MessageBody,
            MessageProperties = MessageProperties,
            DeliveryTag = DeliveryTag,
            RetryCount = RetryCount + 1,
            MaxRetries = MaxRetries,
            ConsumerTag = ConsumerTag,
            Redelivered = Redelivered,
            Timestamp = DateTimeOffset.UtcNow,
            AdditionalData = new Dictionary<string, object>(AdditionalData)
        };

        retryContext.AddContextData("OriginalTimestamp", Timestamp);
        retryContext.AddContextData("RetryAttempt", RetryCount + 1);
        
        return retryContext;
    }

    /// <summary>
    /// Returns a string representation of the error context
    /// </summary>
    public override string ToString()
    {
        var sb = new StringBuilder();
        sb.AppendLine($"ErrorContext:");
        sb.AppendLine($"  Exception: {Exception.GetType().Name} - {Exception.Message}");
        sb.AppendLine($"  Exchange: {ExchangeName}");
        sb.AppendLine($"  RoutingKey: {RoutingKey}");
        sb.AppendLine($"  DeliveryTag: {DeliveryTag}");
        sb.AppendLine($"  RetryCount: {RetryCount}/{MaxRetries}");
        sb.AppendLine($"  Timestamp: {Timestamp:yyyy-MM-dd HH:mm:ss.fff}");
        sb.AppendLine($"  Redelivered: {Redelivered}");
        
        if (AdditionalData.Count > 0)
        {
            sb.AppendLine("  Additional Data:");
            foreach (var kvp in AdditionalData)
            {
                sb.AppendLine($"    {kvp.Key}: {kvp.Value}");
            }
        }
        
        return sb.ToString();
    }

    /// <summary>
    /// Creates an ErrorContext from an event handler failure
    /// </summary>
    /// <param name="exception">The exception that occurred</param>
    /// <param name="eventType">The type of event being handled</param>
    /// <param name="eventData">Additional event data</param>
    /// <returns>A new ErrorContext instance</returns>
    public static ErrorContext FromEvent(Exception exception, string eventType, object? eventData = null)
    {
        return new ErrorContext
        {
            Exception = exception,
            Message = $"Event handler for '{eventType}' failed: {exception.Message}",
            Timestamp = DateTimeOffset.UtcNow,
            AttemptCount = 1,
            AdditionalData = new Dictionary<string, object>
            {
                ["EventType"] = eventType,
                ["EventData"] = eventData ?? "null"
            }
        };
    }

    /// <summary>
    /// Creates an ErrorContext from a failed operation
    /// </summary>
    /// <param name="exception">The exception that occurred</param>
    /// <param name="operation">The operation that failed</param>
    /// <param name="channel">The channel where the error occurred</param>
    /// <returns>A new ErrorContext instance</returns>
    public static ErrorContext FromOperation(Exception exception, string operation, IChannel channel)
    {
        return new ErrorContext
        {
            Exception = exception,
            Message = $"Operation '{operation}' failed: {exception.Message}",
            Timestamp = DateTimeOffset.UtcNow,
            Channel = channel,
            AttemptCount = 1,
            AdditionalData = new Dictionary<string, object>
            {
                ["Operation"] = operation,
                ["ChannelNumber"] = channel?.ChannelNumber ?? 0
            }
        };
    }

    /// <summary>
    /// Creates an ErrorContext from a failed operation without channel information
    /// </summary>
    /// <param name="exception">The exception that occurred</param>
    /// <param name="operation">The operation that failed</param>
    /// <returns>A new ErrorContext instance</returns>
    public static ErrorContext FromOperation(Exception exception, string operation)
    {
        return new ErrorContext
        {
            Exception = exception,
            Message = $"Operation '{operation}' failed: {exception.Message}",
            Timestamp = DateTimeOffset.UtcNow,
            Channel = null,
            AttemptCount = 1,
            AdditionalData = new Dictionary<string, object>
            {
                ["Operation"] = operation
            }
        };
    }

    /// <summary>
    /// Adds a queue context to the error
    /// </summary>
    /// <param name="queueName">The queue name</param>
    /// <returns>The same ErrorContext instance for method chaining</returns>
    public ErrorContext WithQueue(string queueName)
    {
        AdditionalData["QueueName"] = queueName;
        return this;
    }
}