namespace FS.StreamFlow.Core.Features.Messaging.Models;

/// <summary>
/// Represents the result of a publish operation
/// </summary>
public class PublishResult
{
    /// <summary>
    /// Whether the publish was successful
    /// </summary>
    public bool IsSuccess { get; set; }
    
    /// <summary>
    /// Message identifier
    /// </summary>
    public string MessageId { get; set; } = string.Empty;
    
    /// <summary>
    /// Exchange name where the message was published
    /// </summary>
    public string Exchange { get; set; } = string.Empty;
    
    /// <summary>
    /// Routing key used for publishing
    /// </summary>
    public string RoutingKey { get; set; } = string.Empty;
    
    /// <summary>
    /// Publisher confirmation tag
    /// </summary>
    public ulong? ConfirmationTag { get; set; }
    
    /// <summary>
    /// Correlation identifier for the published message
    /// </summary>
    public string? CorrelationId { get; set; }
    
    /// <summary>
    /// Sequence number for the published message
    /// </summary>
    public ulong? SequenceNumber { get; set; }
    
    /// <summary>
    /// Error message if publish failed
    /// </summary>
    public string? ErrorMessage { get; set; }
    
    /// <summary>
    /// Exception details if publish failed
    /// </summary>
    public Exception? Exception { get; set; }
    
    /// <summary>
    /// Publish timestamp
    /// </summary>
    public DateTimeOffset Timestamp { get; set; } = DateTimeOffset.UtcNow;
    
    /// <summary>
    /// Publish duration
    /// </summary>
    public TimeSpan Duration { get; set; }
    
    /// <summary>
    /// Message size in bytes
    /// </summary>
    public long MessageSize { get; set; }
    
    /// <summary>
    /// Whether the message was mandatory
    /// </summary>
    public bool IsMandatory { get; set; }
    
    /// <summary>
    /// Whether the message was persistent
    /// </summary>
    public bool IsPersistent { get; set; }
    
    /// <summary>
    /// Additional metadata
    /// </summary>
    public Dictionary<string, object>? Metadata { get; set; }
    
    /// <summary>
    /// Creates a successful publish result
    /// </summary>
    /// <param name="messageId">Message identifier</param>
    /// <param name="exchange">Exchange name</param>
    /// <param name="routingKey">Routing key</param>
    /// <param name="confirmationTag">Publisher confirmation tag</param>
    /// <returns>Successful publish result</returns>
    public static PublishResult Success(string messageId, string exchange, string routingKey, ulong? confirmationTag = null)
    {
        return new PublishResult
        {
            IsSuccess = true,
            MessageId = messageId,
            Exchange = exchange,
            RoutingKey = routingKey,
            ConfirmationTag = confirmationTag
        };
    }
    
    /// <summary>
    /// Creates a failed publish result
    /// </summary>
    /// <param name="messageId">Message identifier</param>
    /// <param name="exchange">Exchange name</param>
    /// <param name="routingKey">Routing key</param>
    /// <param name="errorMessage">Error message</param>
    /// <param name="exception">Exception details</param>
    /// <returns>Failed publish result</returns>
    public static PublishResult Failure(string messageId, string exchange, string routingKey, string errorMessage, Exception? exception = null)
    {
        return new PublishResult
        {
            IsSuccess = false,
            MessageId = messageId,
            Exchange = exchange,
            RoutingKey = routingKey,
            ErrorMessage = errorMessage,
            Exception = exception
        };
    }
}

/// <summary>
/// Represents the result of a batch publish operation
/// </summary>
public class BatchPublishResult
{
    /// <summary>
    /// Whether the batch publish was successful
    /// </summary>
    public bool IsSuccess { get; set; }
    
    /// <summary>
    /// Total number of messages in the batch
    /// </summary>
    public int TotalMessages { get; set; }
    
    /// <summary>
    /// Number of successfully published messages
    /// </summary>
    public int SuccessfulMessages { get; set; }
    
    /// <summary>
    /// Number of failed messages
    /// </summary>
    public int FailedMessages { get; set; }
    
    /// <summary>
    /// Individual publish results
    /// </summary>
    public List<PublishResult> Results { get; set; } = new();
    
    /// <summary>
    /// Failed publish results
    /// </summary>
    public List<PublishResult> Failures => Results.Where(r => !r.IsSuccess).ToList();
    
    /// <summary>
    /// Batch identifier
    /// </summary>
    public string BatchId { get; set; } = Guid.CreateVersion7().ToString();
    
    /// <summary>
    /// Batch publish timestamp
    /// </summary>
    public DateTimeOffset Timestamp { get; set; } = DateTimeOffset.UtcNow;
    
    /// <summary>
    /// Total batch processing duration
    /// </summary>
    public TimeSpan Duration { get; set; }
    
    /// <summary>
    /// Total size of all messages in bytes
    /// </summary>
    public long TotalSize { get; set; }
    
    /// <summary>
    /// Average message size
    /// </summary>
    public double AverageMessageSize => TotalMessages > 0 ? (double)TotalSize / TotalMessages : 0;
    
    /// <summary>
    /// Success rate (percentage)
    /// </summary>
    public double SuccessRate => TotalMessages > 0 ? (double)SuccessfulMessages / TotalMessages * 100 : 0;
    
    /// <summary>
    /// Error message if batch failed
    /// </summary>
    public string? ErrorMessage { get; set; }
    
    /// <summary>
    /// Exception details if batch failed
    /// </summary>
    public Exception? Exception { get; set; }
    
    /// <summary>
    /// Additional metadata
    /// </summary>
    public Dictionary<string, object>? Metadata { get; set; }
}

/// <summary>
/// Represents event publishing context
/// </summary>
public class EventPublishContext
{
    /// <summary>
    /// Event type
    /// </summary>
    public string EventType { get; set; } = string.Empty;
    
    /// <summary>
    /// Event identifier
    /// </summary>
    public string EventId { get; set; } = Guid.CreateVersion7().ToString();
    
    /// <summary>
    /// Correlation identifier
    /// </summary>
    public string? CorrelationId { get; set; }
    
    /// <summary>
    /// Causation identifier
    /// </summary>
    public string? CausationId { get; set; }
    
    /// <summary>
    /// Event timestamp
    /// </summary>
    public DateTimeOffset Timestamp { get; set; } = DateTimeOffset.UtcNow;
    
    /// <summary>
    /// Event version
    /// </summary>
    public int Version { get; set; } = 1;
    
    /// <summary>
    /// Event source
    /// </summary>
    public string? Source { get; set; }
    
    /// <summary>
    /// Event subject
    /// </summary>
    public string? Subject { get; set; }
    
    /// <summary>
    /// Event metadata
    /// </summary>
    public Dictionary<string, object>? Metadata { get; set; }
    
    /// <summary>
    /// Publishing options
    /// </summary>
    public PublishOptions Options { get; set; } = new();
    
    /// <summary>
    /// Exchange name (from publish options)
    /// </summary>
    public string Exchange => Options.Exchange;
    
    /// <summary>
    /// Routing key (from publish options)
    /// </summary>
    public string RoutingKey => Options.RoutingKey;
    
    /// <summary>
    /// Creates a new EventPublishContext instance
    /// </summary>
    public EventPublishContext()
    {
        Metadata = new Dictionary<string, object>();
    }
}

/// <summary>
/// Represents publishing options
/// </summary>
public class PublishOptions
{
    /// <summary>
    /// Exchange name
    /// </summary>
    public string Exchange { get; set; } = string.Empty;
    
    /// <summary>
    /// Routing key
    /// </summary>
    public string RoutingKey { get; set; } = string.Empty;
    
    /// <summary>
    /// Whether the message is mandatory
    /// </summary>
    public bool Mandatory { get; set; } = false;
    
    /// <summary>
    /// Whether to wait for publisher confirmation
    /// </summary>
    public bool WaitForConfirmation { get; set; } = true;
    
    /// <summary>
    /// Publish timeout
    /// </summary>
    public TimeSpan Timeout { get; set; } = TimeSpan.FromSeconds(30);
    
    /// <summary>
    /// Message properties
    /// </summary>
    public MessageProperties Properties { get; set; } = new();
    
    /// <summary>
    /// Additional headers
    /// </summary>
    public Dictionary<string, object>? Headers { get; set; }
    
    /// <summary>
    /// Creates a new PublishOptions instance
    /// </summary>
    public PublishOptions()
    {
        Headers = new Dictionary<string, object>();
    }
}

/// <summary>
/// Represents basic message properties (provider-agnostic version)
/// </summary>
public class BasicProperties
{
    /// <summary>
    /// Content type
    /// </summary>
    public string? ContentType { get; set; }
    
    /// <summary>
    /// Content encoding
    /// </summary>
    public string? ContentEncoding { get; set; }
    
    /// <summary>
    /// Headers
    /// </summary>
    public Dictionary<string, object>? Headers { get; set; }
    
    /// <summary>
    /// Delivery mode
    /// </summary>
    public DeliveryMode DeliveryMode { get; set; } = DeliveryMode.Persistent;
    
    /// <summary>
    /// Priority
    /// </summary>
    public byte Priority { get; set; } = 0;
    
    /// <summary>
    /// Correlation ID
    /// </summary>
    public string? CorrelationId { get; set; }
    
    /// <summary>
    /// Reply to
    /// </summary>
    public string? ReplyTo { get; set; }
    
    /// <summary>
    /// Expiration
    /// </summary>
    public string? Expiration { get; set; }
    
    /// <summary>
    /// Message ID
    /// </summary>
    public string? MessageId { get; set; }
    
    /// <summary>
    /// Timestamp
    /// </summary>
    public DateTimeOffset? Timestamp { get; set; }
    
    /// <summary>
    /// Type
    /// </summary>
    public string? Type { get; set; }
    
    /// <summary>
    /// User ID
    /// </summary>
    public string? UserId { get; set; }
    
    /// <summary>
    /// App ID
    /// </summary>
    public string? AppId { get; set; }
    
    /// <summary>
    /// Creates a new BasicProperties instance
    /// </summary>
    public BasicProperties()
    {
        Headers = new Dictionary<string, object>();
    }
} 