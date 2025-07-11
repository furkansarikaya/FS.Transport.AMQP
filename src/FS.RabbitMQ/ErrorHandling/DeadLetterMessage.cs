namespace FS.RabbitMQ.ErrorHandling;

/// <summary>
/// Represents a message in the dead letter queue
/// </summary>
public class DeadLetterMessage
{
    /// <summary>
    /// Gets or sets the unique identifier for this dead letter message
    /// </summary>
    public string MessageId { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the original message ID
    /// </summary>
    public string OriginalMessageId { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the original exchange where the message was published
    /// </summary>
    public string OriginalExchange { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the original routing key
    /// </summary>
    public string OriginalRoutingKey { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the original queue name
    /// </summary>
    public string OriginalQueue { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the message body
    /// </summary>
    public ReadOnlyMemory<byte> MessageBody { get; set; }

    /// <summary>
    /// Gets or sets the content type of the message
    /// </summary>
    public string ContentType { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the delivery mode
    /// </summary>
    public byte DeliveryMode { get; set; }

    /// <summary>
    /// Gets or sets the message priority
    /// </summary>
    public byte Priority { get; set; }

    /// <summary>
    /// Gets or sets the correlation ID
    /// </summary>
    public string CorrelationId { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the error message that caused this to be dead lettered
    /// </summary>
    public string ErrorMessage { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the type of error that occurred
    /// </summary>
    public string ErrorType { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the stack trace of the error
    /// </summary>
    public string ErrorStackTrace { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the number of retry attempts made
    /// </summary>
    public int RetryCount { get; set; }

    /// <summary>
    /// Gets or sets when the message first encountered an error
    /// </summary>
    public DateTimeOffset FirstErrorTime { get; set; }

    /// <summary>
    /// Gets or sets when the message was last processed unsuccessfully
    /// </summary>
    public DateTimeOffset LastErrorTime { get; set; }

    /// <summary>
    /// Gets or sets when the message was sent to the dead letter queue
    /// </summary>
    public DateTimeOffset DeadLetterTime { get; set; } = DateTimeOffset.UtcNow;

    /// <summary>
    /// Gets or sets the number of times this message has been requeued from dead letter
    /// </summary>
    public int RequeueCount { get; set; }

    /// <summary>
    /// Gets or sets when the message was last requeued
    /// </summary>
    public DateTimeOffset? LastRequeueTime { get; set; }

    /// <summary>
    /// Gets or sets the original message headers
    /// </summary>
    public Dictionary<string, object> Headers { get; set; } = new();

    /// <summary>
    /// Gets or sets additional contextual data
    /// </summary>
    public Dictionary<string, object?> AdditionalData { get; set; } = new();
}