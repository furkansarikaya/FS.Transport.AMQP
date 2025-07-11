namespace FS.Transport.AMQP.ErrorHandling;

/// <summary>
/// Represents a message that has been sent to a dead letter queue due to processing failure
/// </summary>
public class DeadLetterMessage
{
    /// <summary>
    /// Gets or sets the unique identifier of the message
    /// </summary>
    /// <value>The message ID</value>
    public string MessageId { get; set; } = string.Empty;
    
    /// <summary>
    /// Gets or sets the correlation identifier for message correlation
    /// </summary>
    /// <value>The correlation ID</value>
    public string CorrelationId { get; set; } = string.Empty;
    
    /// <summary>
    /// Gets or sets the timestamp when the message was originally created
    /// </summary>
    /// <value>The original message timestamp</value>
    public DateTimeOffset OriginalTimestamp { get; set; }
    
    /// <summary>
    /// Gets or sets the timestamp when the error occurred
    /// </summary>
    /// <value>The error timestamp</value>
    public DateTimeOffset ErrorTimestamp { get; set; }
    
    /// <summary>
    /// Gets or sets the number of processing attempts made for this message
    /// </summary>
    /// <value>The attempt count</value>
    public int AttemptCount { get; set; }
    
    /// <summary>
    /// Gets or sets the original exchange where the message was published
    /// </summary>
    /// <value>The original exchange name</value>
    public string OriginalExchange { get; set; } = string.Empty;
    
    /// <summary>
    /// Gets or sets the original routing key used for the message
    /// </summary>
    /// <value>The original routing key</value>
    public string OriginalRoutingKey { get; set; } = string.Empty;
    
    /// <summary>
    /// Gets or sets the original queue where the message was consumed from
    /// </summary>
    /// <value>The original queue name</value>
    public string OriginalQueue { get; set; } = string.Empty;
    
    /// <summary>
    /// Gets or sets the consumer tag that was processing the message
    /// </summary>
    /// <value>The consumer tag</value>
    public string ConsumerTag { get; set; } = string.Empty;
    
    /// <summary>
    /// Gets or sets the operation that was being performed when the error occurred
    /// </summary>
    /// <value>The operation name</value>
    public string Operation { get; set; } = string.Empty;
    
    /// <summary>
    /// Gets or sets the exception details that caused the message to be dead lettered
    /// </summary>
    /// <value>The exception information</value>
    public DeadLetterException Exception { get; set; } = new();
    
    /// <summary>
    /// Gets or sets the message headers
    /// </summary>
    /// <value>Dictionary of message headers</value>
    public Dictionary<string, object> Headers { get; set; } = new();
    
    /// <summary>
    /// Gets or sets the message properties
    /// </summary>
    /// <value>Dictionary of message properties</value>
    public Dictionary<string, object> Properties { get; set; } = new();
    
    /// <summary>
    /// Gets or sets the original message body
    /// </summary>
    /// <value>The original message content</value>
    public string OriginalMessage { get; set; } = string.Empty;
}