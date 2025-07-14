namespace FS.StreamFlow.Core.Features.Messaging.Models;

/// <summary>
/// Context information for message publishing
/// </summary>
public class StreamFlowMessageContext
{
    /// <summary>
    /// Gets or sets the exchange to publish to
    /// </summary>
    public string Exchange { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the routing key
    /// </summary>
    public string RoutingKey { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the message body
    /// </summary>
    public ReadOnlyMemory<byte> Body { get; set; }
    
    /// <summary>
    /// Gets or sets the message object
    /// </summary>
    public object? Message { get; set; }
    
    /// <summary>
    /// Gets or sets the message properties
    /// </summary>
    public MessageProperties Properties { get; set; } = new();

    /// <summary>
    /// Gets or sets whether the message is mandatory
    /// </summary>
    public bool Mandatory { get; set; }

    /// <summary>
    /// Gets or sets the message ID
    /// </summary>
    public string MessageId { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the correlation ID
    /// </summary>
    public string CorrelationId { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets when the message was created
    /// </summary>
    public DateTimeOffset Timestamp { get; set; } = DateTimeOffset.UtcNow;

    /// <summary>
    /// Gets or sets additional context data
    /// </summary>
    public Dictionary<string, object?> AdditionalData { get; set; } = new();

    /// <summary>
    /// Gets or sets the message headers
    /// </summary>
    public Dictionary<string, object>? Headers { get; set; }

    /// <summary>
    /// Gets or sets the message type
    /// </summary>
    public string? MessageType { get; set; }

    /// <summary>
    /// Gets or sets the message priority (0-255)
    /// </summary>
    public byte? Priority { get; set; }

    /// <summary>
    /// Gets or sets the time-to-live for the message in milliseconds
    /// </summary>
    public TimeSpan? TimeToLive { get; set; }

    /// <summary>
    /// Gets or sets the message expiration
    /// </summary>
    public string? Expiration { get; set; }

    /// <summary>
    /// Gets or sets whether to wait for publisher confirmation
    /// </summary>
    public bool WaitForConfirmation { get; set; }

    /// <summary>
    /// Gets or sets the confirmation timeout
    /// </summary>
    public TimeSpan? ConfirmationTimeout { get; set; }

    /// <summary>
    /// Gets or sets the reply-to queue
    /// </summary>
    public string? ReplyTo { get; set; }

    /// <summary>
    /// Gets or sets the content type
    /// </summary>
    public string? ContentType { get; set; }

    /// <summary>
    /// Gets or sets the content encoding
    /// </summary>
    public string? ContentEncoding { get; set; }

    /// <summary>
    /// Gets or sets the retry policy name
    /// </summary>
    public string? RetryPolicyName { get; set; }

    /// <summary>
    /// Gets or sets the maximum number of retries
    /// </summary>
    public int? MaxRetries { get; set; }

    /// <summary>
    /// Gets or sets the dead letter exchange
    /// </summary>
    public string? DeadLetterExchange { get; set; }

    /// <summary>
    /// Gets or sets the dead letter routing key
    /// </summary>
    public string? DeadLetterRoutingKey { get; set; }
}