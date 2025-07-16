namespace FS.StreamFlow.Core.Features.Messaging.Models;

/// <summary>
/// Represents producer configuration settings
/// </summary>
public class ProducerSettings
{
    /// <summary>
    /// Producer identifier
    /// </summary>
    public string ProducerId { get; set; } = Guid.CreateVersion7().ToString();
    
    /// <summary>
    /// Whether to enable publisher confirmations
    /// </summary>
    public bool EnablePublisherConfirms { get; set; } = true;
    
    /// <summary>
    /// Whether to use publisher confirmations (alias for EnablePublisherConfirms)
    /// </summary>
    public bool UsePublisherConfirms => EnablePublisherConfirms;
    
    /// <summary>
    /// Whether to enable mandatory publishing
    /// </summary>
    public bool EnableMandatoryPublishing { get; set; } = false;
    
    /// <summary>
    /// Maximum number of concurrent publishes
    /// </summary>
    public int MaxConcurrentPublishes { get; set; } = 100;
    
    /// <summary>
    /// Timeout for publish operations
    /// </summary>
    public TimeSpan PublishTimeout { get; set; } = TimeSpan.FromSeconds(30);
    
    /// <summary>
    /// Timeout for publish confirmations
    /// </summary>
    public TimeSpan ConfirmationTimeout { get; set; } = TimeSpan.FromSeconds(10);
    
    /// <summary>
    /// Whether to use transactions
    /// </summary>
    public bool UseTransactions { get; set; } = false;
    
    /// <summary>
    /// Retry policy settings
    /// </summary>
    public RetryPolicySettings RetryPolicy { get; set; } = new();
    
    /// <summary>
    /// Serialization settings
    /// </summary>
    public SerializationSettings Serialization { get; set; } = new();
    
    /// <summary>
    /// Default exchange name
    /// </summary>
    public string DefaultExchange { get; set; } = string.Empty;
    
    /// <summary>
    /// Default routing key
    /// </summary>
    public string DefaultRoutingKey { get; set; } = string.Empty;
    
    /// <summary>
    /// Default message properties
    /// </summary>
    public MessageProperties DefaultProperties { get; set; } = new();
}

/// <summary>
/// Represents producer statistics
/// </summary>
public class ProducerStatistics
{
    /// <summary>
    /// Producer identifier
    /// </summary>
    public string ProducerId { get; set; } = string.Empty;
    
    /// <summary>
    /// Total number of messages published
    /// </summary>
    public long TotalMessagesPublished { get; set; }
    
    /// <summary>
    /// Number of messages successfully published
    /// </summary>
    public long SuccessfulPublishes { get; set; }
    
    /// <summary>
    /// Number of failed publishes
    /// </summary>
    public long FailedPublishes { get; set; }
    
    /// <summary>
    /// Number of messages pending confirmation
    /// </summary>
    public long PendingConfirmations { get; set; }
    
    /// <summary>
    /// Number of confirmed messages
    /// </summary>
    public long ConfirmedMessages { get; set; }
    
    /// <summary>
    /// Number of rejected messages
    /// </summary>
    public long RejectedMessages { get; set; }
    
    /// <summary>
    /// Total bytes published
    /// </summary>
    public long TotalBytesPublished { get; set; }
    
    /// <summary>
    /// Average message size
    /// </summary>
    public double AverageMessageSize => TotalMessagesPublished > 0 
        ? (double)TotalBytesPublished / TotalMessagesPublished 
        : 0;
    
    /// <summary>
    /// Publish success rate (percentage)
    /// </summary>
    public double PublishSuccessRate => TotalMessagesPublished > 0 
        ? (double)SuccessfulPublishes / TotalMessagesPublished * 100 
        : 0;
    
    /// <summary>
    /// Messages published per second
    /// </summary>
    public double MessagesPerSecond { get; set; }
    
    /// <summary>
    /// Average publish duration
    /// </summary>
    public TimeSpan AveragePublishDuration { get; set; }
    
    /// <summary>
    /// Last publish timestamp
    /// </summary>
    public DateTimeOffset? LastPublishAt { get; set; }
    
    /// <summary>
    /// Producer start time
    /// </summary>
    public DateTimeOffset StartTime { get; set; } = DateTimeOffset.UtcNow;
    
    /// <summary>
    /// Producer initialization time (alias for StartTime)
    /// </summary>
    public DateTimeOffset InitializedAt => StartTime;
    
    /// <summary>
    /// Messages published (alias for TotalMessagesPublished)
    /// </summary>
    public long MessagesPublished => TotalMessagesPublished;
    
    /// <summary>
    /// Bytes published (alias for TotalBytesPublished)
    /// </summary>
    public long BytesPublished => TotalBytesPublished;
    
    /// <summary>
    /// Publish failures (alias for FailedPublishes)
    /// </summary>
    public long PublishFailures => FailedPublishes;
    
    /// <summary>
    /// Statistics collection timestamp
    /// </summary>
    public DateTimeOffset Timestamp { get; set; } = DateTimeOffset.UtcNow;
}

/// <summary>
/// Represents message properties
/// </summary>
public class MessageProperties
{
    /// <summary>
    /// Message content type
    /// </summary>
    public string? ContentType { get; set; }
    
    /// <summary>
    /// Message content encoding
    /// </summary>
    public string? ContentEncoding { get; set; }
    
    /// <summary>
    /// Message headers
    /// </summary>
    public Dictionary<string, object>? Headers { get; set; }
    
    /// <summary>
    /// Message delivery mode
    /// </summary>
    public DeliveryMode DeliveryMode { get; set; } = DeliveryMode.Persistent;
    
    /// <summary>
    /// Message priority
    /// </summary>
    public byte Priority { get; set; } = 0;
    
    /// <summary>
    /// Correlation ID
    /// </summary>
    public string? CorrelationId { get; set; }
    
    /// <summary>
    /// Reply-to address
    /// </summary>
    public string? ReplyTo { get; set; }
    
    /// <summary>
    /// Message expiration
    /// </summary>
    public string? Expiration { get; set; }
    
    /// <summary>
    /// Message ID
    /// </summary>
    public string? MessageId { get; set; }
    
    /// <summary>
    /// Message timestamp
    /// </summary>
    public DateTimeOffset? Timestamp { get; set; }
    
    /// <summary>
    /// Message type
    /// </summary>
    public string? Type { get; set; }
    
    /// <summary>
    /// User ID
    /// </summary>
    public string? UserId { get; set; }
    
    /// <summary>
    /// Application ID
    /// </summary>
    public string? AppId { get; set; }
    
    /// <summary>
    /// Creates a new MessageProperties instance with default values
    /// </summary>
    public MessageProperties()
    {
        Headers = new Dictionary<string, object>();
    }
}

/// <summary>
/// Represents message delivery mode
/// </summary>
public enum DeliveryMode
{
    /// <summary>
    /// Non-persistent message (faster but may be lost)
    /// </summary>
    NonPersistent = 1,
    
    /// <summary>
    /// Persistent message (slower but durable)
    /// </summary>
    Persistent = 2
}

/// <summary>
/// Represents retry policy settings
/// </summary>
public class RetryPolicySettings
{
    /// <summary>
    /// Maximum number of retry attempts
    /// </summary>
    public int MaxRetryAttempts { get; set; } = 3;
    
    /// <summary>
    /// Initial retry delay
    /// </summary>
    public TimeSpan InitialRetryDelay { get; set; } = TimeSpan.FromSeconds(1);
    
    /// <summary>
    /// Maximum retry delay
    /// </summary>
    public TimeSpan MaxRetryDelay { get; set; } = TimeSpan.FromSeconds(30);
    
    /// <summary>
    /// Retry delay multiplier
    /// </summary>
    public double RetryDelayMultiplier { get; set; } = 2.0;
    
    /// <summary>
    /// Whether to use exponential backoff
    /// </summary>
    public bool UseExponentialBackoff { get; set; } = true;
    
    /// <summary>
    /// Whether to use jitter
    /// </summary>
    public bool UseJitter { get; set; } = true;
}

/// <summary>
/// Represents serialization settings
/// </summary>
public class SerializationSettings
{
    /// <summary>
    /// Serialization format
    /// </summary>
    public SerializationFormat Format { get; set; } = SerializationFormat.Json;
    
    /// <summary>
    /// Whether to compress messages
    /// </summary>
    public bool EnableCompression { get; set; } = false;
    
    /// <summary>
    /// Compression algorithm
    /// </summary>
    public CompressionAlgorithm CompressionAlgorithm { get; set; } = CompressionAlgorithm.Gzip;
    
    /// <summary>
    /// Compression threshold in bytes
    /// </summary>
    public int CompressionThreshold { get; set; } = 1024;
    
    /// <summary>
    /// Whether to include type information
    /// </summary>
    public bool IncludeTypeInformation { get; set; } = true;
}

/// <summary>
/// Represents serialization format
/// </summary>
public enum SerializationFormat
{
    /// <summary>
    /// JSON serialization
    /// </summary>
    Json,
    
    /// <summary>
    /// Binary serialization
    /// </summary>
    Binary,
    
    /// <summary>
    /// MessagePack serialization
    /// </summary>
    MessagePack,
    
    /// <summary>
    /// XML serialization
    /// </summary>
    Xml
}

/// <summary>
/// Represents compression algorithm
/// </summary>
public enum CompressionAlgorithm
{
    /// <summary>
    /// No compression
    /// </summary>
    None,
    
    /// <summary>
    /// GZIP compression
    /// </summary>
    Gzip,
    
    /// <summary>
    /// Deflate compression
    /// </summary>
    Deflate,
    
    /// <summary>
    /// Brotli compression
    /// </summary>
    Brotli
} 