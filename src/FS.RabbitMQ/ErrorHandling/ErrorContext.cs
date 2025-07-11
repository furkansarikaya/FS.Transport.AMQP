using FS.RabbitMQ.Events;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace FS.RabbitMQ.ErrorHandling;

/// <summary>
/// Comprehensive context information for error handling operations
/// </summary>
public class ErrorContext
{
    /// <summary>
    /// The exception that occurred
    /// </summary>
    public Exception Exception { get; set; }
    
    /// <summary>
    /// RabbitMQ channel where the error occurred
    /// </summary>
    public IModel? Channel { get; set; }
    
    /// <summary>
    /// Basic delivery information (if from a message)
    /// </summary>
    public BasicDeliverEventArgs? DeliveryArgs { get; set; }
    
    /// <summary>
    /// Raw message body
    /// </summary>
    public ReadOnlyMemory<byte>? MessageBody { get; set; }
    
    /// <summary>
    /// Deserialized message object (if available)
    /// </summary>
    public object? Message { get; set; }
    
    /// <summary>
    /// Event that caused the error (if applicable)
    /// </summary>
    public IEvent? Event { get; set; }
    
    /// <summary>
    /// Number of retry attempts made so far
    /// </summary>
    public int AttemptCount { get; set; }
    
    /// <summary>
    /// Timestamp when the error occurred
    /// </summary>
    public DateTime ErrorTimestamp { get; set; } = DateTime.UtcNow;
    
    /// <summary>
    /// Original timestamp when the message was first received
    /// </summary>
    public DateTime? OriginalTimestamp { get; set; }
    
    /// <summary>
    /// Queue name where the message came from
    /// </summary>
    public string? QueueName { get; set; }
    
    /// <summary>
    /// Exchange name where the message came from
    /// </summary>
    public string? ExchangeName { get; set; }
    
    /// <summary>
    /// Routing key used for the message
    /// </summary>
    public string? RoutingKey { get; set; }
    
    /// <summary>
    /// Correlation ID for tracing
    /// </summary>
    public string? CorrelationId { get; set; }
    
    /// <summary>
    /// Message ID
    /// </summary>
    public string? MessageId { get; set; }
    
    /// <summary>
    /// Consumer tag that was processing the message
    /// </summary>
    public string? ConsumerTag { get; set; }
    
    /// <summary>
    /// Operation that was being performed when error occurred
    /// </summary>
    public string? Operation { get; set; }
    
    /// <summary>
    /// Additional context properties
    /// </summary>
    public IDictionary<string, object> Properties { get; set; } = new Dictionary<string, object>();
    
    /// <summary>
    /// Original message headers
    /// </summary>
    public IDictionary<string, object>? Headers { get; set; }

    public ErrorContext(Exception exception)
    {
        Exception = exception ?? throw new ArgumentNullException(nameof(exception));
    }

    /// <summary>
    /// Creates error context from a basic delivery event
    /// </summary>
    /// <param name="exception">Exception that occurred</param>
    /// <param name="deliveryArgs">Delivery event arguments</param>
    /// <param name="channel">RabbitMQ channel</param>
    /// <returns>Error context</returns>
    public static ErrorContext FromDelivery(Exception exception, BasicDeliverEventArgs deliveryArgs, IModel channel)
    {
        var context = new ErrorContext(exception)
        {
            Channel = channel,
            DeliveryArgs = deliveryArgs,
            MessageBody = deliveryArgs.Body,
            QueueName = deliveryArgs.RoutingKey, // Note: This might not always be the queue name
            ExchangeName = deliveryArgs.Exchange,
            RoutingKey = deliveryArgs.RoutingKey,
            ConsumerTag = deliveryArgs.ConsumerTag,
            Headers = deliveryArgs.BasicProperties?.Headers?.ToDictionary(kvp => kvp.Key, kvp => kvp.Value) ?? new Dictionary<string, object>()
        };

        // Extract common properties from headers
        if (deliveryArgs.BasicProperties != null)
        {
            context.MessageId = deliveryArgs.BasicProperties.MessageId;
            context.CorrelationId = deliveryArgs.BasicProperties.CorrelationId;
            
            if (deliveryArgs.BasicProperties.Timestamp.UnixTime > 0)
            {
                context.OriginalTimestamp = DateTimeOffset.FromUnixTimeSeconds(deliveryArgs.BasicProperties.Timestamp.UnixTime).DateTime;
            }
        }

        // Extract retry count from headers
        if (context.Headers?.TryGetValue("x-retry-count", out var retryCountObj) == true && 
            int.TryParse(retryCountObj?.ToString(), out var retryCount))
        {
            context.AttemptCount = retryCount;
        }

        return context;
    }

    /// <summary>
    /// Creates error context from an event
    /// </summary>
    /// <param name="exception">Exception that occurred</param>
    /// <param name="event">Event that caused the error</param>
    /// <param name="operation">Operation being performed</param>
    /// <returns>Error context</returns>
    public static ErrorContext FromEvent(Exception exception, IEvent @event, string operation)
    {
        var context = new ErrorContext(exception)
        {
            Event = @event,
            Operation = operation,
            MessageId = @event.Id.ToString(),
            CorrelationId = @event.CorrelationId,
            OriginalTimestamp = @event.OccurredOn
        };

        // Add event metadata to properties
        foreach (var kvp in @event.Metadata)
        {
            context.Properties[kvp.Key] = kvp.Value;
        }

        return context;
    }

    /// <summary>
    /// Creates error context from a general operation
    /// </summary>
    /// <param name="exception">Exception that occurred</param>
    /// <param name="operation">Operation that failed</param>
    /// <returns>Error context</returns>
    public static ErrorContext FromOperation(Exception exception, string operation)
    {
        return new ErrorContext(exception)
        {
            Operation = operation
        };
    }

    /// <summary>
    /// Adds additional context property
    /// </summary>
    /// <param name="key">Property key</param>
    /// <param name="value">Property value</param>
    /// <returns>Error context for fluent configuration</returns>
    public ErrorContext WithProperty(string key, object value)
    {
        Properties[key] = value;
        return this;
    }

    /// <summary>
    /// Sets the message object
    /// </summary>
    /// <param name="message">Deserialized message</param>
    /// <returns>Error context for fluent configuration</returns>
    public ErrorContext WithMessage(object message)
    {
        Message = message;
        return this;
    }

    /// <summary>
    /// Sets the queue name
    /// </summary>
    /// <param name="queueName">Queue name</param>
    /// <returns>Error context for fluent configuration</returns>
    public ErrorContext WithQueue(string queueName)
    {
        QueueName = queueName;
        return this;
    }

    /// <summary>
    /// Increments the attempt count
    /// </summary>
    /// <returns>Error context for fluent configuration</returns>
    public ErrorContext IncrementAttempt()
    {
        AttemptCount++;
        return this;
    }

    /// <summary>
    /// Gets a summary of the error context
    /// </summary>
    /// <returns>Error context summary</returns>
    public override string ToString()
    {
        var parts = new List<string>
        {
            $"Exception: {Exception.GetType().Name}",
            $"Message: {Exception.Message}"
        };

        if (!string.IsNullOrEmpty(Operation))
            parts.Add($"Operation: {Operation}");
            
        if (!string.IsNullOrEmpty(QueueName))
            parts.Add($"Queue: {QueueName}");
            
        if (!string.IsNullOrEmpty(MessageId))
            parts.Add($"MessageId: {MessageId}");
            
        if (AttemptCount > 0)
            parts.Add($"Attempt: {AttemptCount}");

        return string.Join(", ", parts);
    }
}