namespace FS.StreamFlow.Core.Features.Messaging.Models;

/// <summary>
/// Represents consumer configuration settings
/// </summary>
public class ConsumerSettings
{
    /// <summary>
    /// Consumer identifier
    /// </summary>
    public string ConsumerId { get; set; } = Guid.CreateVersion7().ToString();
    
    /// <summary>
    /// Consumer tag
    /// </summary>
    public string ConsumerTag { get; set; } = string.Empty;
    
    /// <summary>
    /// Whether to auto-acknowledge messages
    /// </summary>
    public bool AutoAcknowledge { get; set; } = false;
    
    /// <summary>
    /// Whether to allow exclusive consumption
    /// </summary>
    public bool Exclusive { get; set; } = false;
    
    /// <summary>
    /// Whether to consume from multiple exchanges
    /// </summary>
    public bool NoLocal { get; set; } = false;
    
    /// <summary>
    /// Prefetch count (number of messages to prefetch)
    /// </summary>
    public ushort PrefetchCount { get; set; } = 10;
    
    /// <summary>
    /// Prefetch size (total size of messages to prefetch)
    /// </summary>
    public uint PrefetchSize { get; set; } = 0;
    
    /// <summary>
    /// Whether to apply prefetch globally
    /// </summary>
    public bool GlobalPrefetch { get; set; } = false;
    
    /// <summary>
    /// Maximum number of concurrent consumers
    /// </summary>
    public int MaxConcurrentConsumers { get; set; } = 1;
    
    /// <summary>
    /// Consumer timeout
    /// </summary>
    public TimeSpan ConsumerTimeout { get; set; } = TimeSpan.FromSeconds(30);
    
    /// <summary>
    /// Message processing timeout
    /// </summary>
    public TimeSpan MessageProcessingTimeout { get; set; } = TimeSpan.FromSeconds(60);
    
    /// <summary>
    /// Whether to enable dead letter queue
    /// </summary>
    public bool EnableDeadLetterQueue { get; set; } = true;
    
    /// <summary>
    /// Dead letter queue settings
    /// </summary>
    public DeadLetterSettings DeadLetterSettings { get; set; } = new();
    
    /// <summary>
    /// Retry policy settings
    /// </summary>
    public RetryPolicySettings RetryPolicy { get; set; } = new();
    
    /// <summary>
    /// Serialization settings
    /// </summary>
    public SerializationSettings Serialization { get; set; } = new();
    
    /// <summary>
    /// Error handling settings
    /// </summary>
    public ErrorHandlingSettings ErrorHandling { get; set; } = new();
    
    /// <summary>
    /// Additional consumer arguments
    /// </summary>
    public Dictionary<string, object>? Arguments { get; set; }
}

/// <summary>
/// Represents consumer statistics
/// </summary>
public class ConsumerStatistics
{
    /// <summary>
    /// Consumer identifier
    /// </summary>
    public string ConsumerId { get; set; } = string.Empty;
    
    /// <summary>
    /// Consumer tag
    /// </summary>
    public string ConsumerTag { get; set; } = string.Empty;
    
    /// <summary>
    /// Total number of messages consumed
    /// </summary>
    public long TotalMessagesConsumed { get; set; }
    
    /// <summary>
    /// Number of messages successfully processed
    /// </summary>
    public long SuccessfullyProcessed { get; set; }
    
    /// <summary>
    /// Number of messages that failed processing
    /// </summary>
    public long FailedProcessing { get; set; }
    
    /// <summary>
    /// Number of messages acknowledged
    /// </summary>
    public long AcknowledgedMessages { get; set; }
    
    /// <summary>
    /// Number of messages rejected
    /// </summary>
    public long RejectedMessages { get; set; }
    
    /// <summary>
    /// Number of messages requeued
    /// </summary>
    public long RequeuedMessages { get; set; }
    
    /// <summary>
    /// Total bytes consumed
    /// </summary>
    public long TotalBytesConsumed { get; set; }
    
    /// <summary>
    /// Average message size
    /// </summary>
    public double AverageMessageSize => TotalMessagesConsumed > 0 
        ? (double)TotalBytesConsumed / TotalMessagesConsumed 
        : 0;
    
    /// <summary>
    /// Processing success rate (percentage)
    /// </summary>
    public double ProcessingSuccessRate => TotalMessagesConsumed > 0 
        ? (double)SuccessfullyProcessed / TotalMessagesConsumed * 100 
        : 0;
    
    /// <summary>
    /// Messages processed per second
    /// </summary>
    public double MessagesPerSecond { get; set; }
    
    /// <summary>
    /// Average processing duration
    /// </summary>
    public TimeSpan AverageProcessingDuration { get; set; }
    
    /// <summary>
    /// Last message consumed timestamp
    /// </summary>
    public DateTimeOffset? LastConsumedAt { get; set; }
    
    /// <summary>
    /// Consumer start time
    /// </summary>
    public DateTimeOffset StartTime { get; set; } = DateTimeOffset.UtcNow;
    
    /// <summary>
    /// Statistics collection timestamp
    /// </summary>
    public DateTimeOffset Timestamp { get; set; } = DateTimeOffset.UtcNow;
}

/// <summary>
/// Represents the status of a consumer
/// </summary>
public enum ConsumerStatus
{
    /// <summary>
    /// Consumer is not initialized
    /// </summary>
    NotInitialized,
    
    /// <summary>
    /// Consumer is starting up
    /// </summary>
    Starting,
    
    /// <summary>
    /// Consumer is running and ready to consume messages
    /// </summary>
    Running,
    
    /// <summary>
    /// Consumer is stopping
    /// </summary>
    Stopping,
    
    /// <summary>
    /// Consumer has stopped
    /// </summary>
    Stopped,
    
    /// <summary>
    /// Consumer is in a faulted state due to an error
    /// </summary>
    Faulted
}

/// <summary>
/// Represents message context for consumed messages
/// </summary>
public class MessageContext
{
    /// <summary>
    /// Exchange name where the message was received from
    /// </summary>
    public string Exchange { get; set; } = string.Empty;
    
    /// <summary>
    /// Routing key used for the message
    /// </summary>
    public string RoutingKey { get; set; } = string.Empty;
    
    /// <summary>
    /// Queue name where the message was consumed from
    /// </summary>
    public string QueueName { get; set; } = string.Empty;
    
    /// <summary>
    /// Consumer tag
    /// </summary>
    public string ConsumerTag { get; set; } = string.Empty;
    
    /// <summary>
    /// Delivery tag for acknowledgment
    /// </summary>
    public ulong DeliveryTag { get; set; }
    
    /// <summary>
    /// Whether the message was redelivered
    /// </summary>
    public bool IsRedelivered { get; set; }
    
    /// <summary>
    /// Message properties
    /// </summary>
    public MessageProperties Properties { get; set; } = new();
    
    /// <summary>
    /// Raw message body
    /// </summary>
    public ReadOnlyMemory<byte> Body { get; set; }
    
    /// <summary>
    /// Deserialized message object
    /// </summary>
    public object? Message { get; set; }
    
    /// <summary>
    /// Message received timestamp
    /// </summary>
    public DateTimeOffset ReceivedAt { get; set; } = DateTimeOffset.UtcNow;
    
    /// <summary>
    /// Processing attempt count
    /// </summary>
    public int AttemptCount { get; set; } = 1;
    
    /// <summary>
    /// Additional context information
    /// </summary>
    public Dictionary<string, object>? Context { get; set; }
    
    /// <summary>
    /// Gets or sets the message ID
    /// </summary>
    public string MessageId { get; set; } = string.Empty;
    
    /// <summary>
    /// Gets or sets the dead letter exchange
    /// </summary>
    public string? DeadLetterExchange { get; set; }
    
    /// <summary>
    /// Gets or sets the dead letter routing key
    /// </summary>
    public string? DeadLetterRoutingKey { get; set; }
    
    /// <summary>
    /// Creates a new MessageContext instance
    /// </summary>
    public MessageContext()
    {
        Context = new Dictionary<string, object>();
    }
}

/// <summary>
/// Represents dead letter queue settings
/// </summary>
public class DeadLetterSettings
{
    /// <summary>
    /// Dead letter exchange name
    /// </summary>
    public string ExchangeName { get; set; } = string.Empty;
    
    /// <summary>
    /// Dead letter routing key
    /// </summary>
    public string RoutingKey { get; set; } = string.Empty;
    
    /// <summary>
    /// Whether to enable dead letter queue
    /// </summary>
    public bool Enabled { get; set; } = true;
    
    /// <summary>
    /// Maximum number of retries before sending to dead letter queue
    /// </summary>
    public int MaxRetries { get; set; } = 3;
    
    /// <summary>
    /// TTL for messages in dead letter queue
    /// </summary>
    public TimeSpan? MessageTtl { get; set; }
    
    /// <summary>
    /// Additional arguments for dead letter queue
    /// </summary>
    public Dictionary<string, object>? Arguments { get; set; }
}

/// <summary>
/// Represents error handling settings
/// </summary>
public class ErrorHandlingSettings
{
    /// <summary>
    /// Error handling strategy
    /// </summary>
    public ErrorHandlingStrategy Strategy { get; set; } = ErrorHandlingStrategy.Requeue;
    
    /// <summary>
    /// Maximum number of error retries
    /// </summary>
    public int MaxRetries { get; set; } = 3;
    
    /// <summary>
    /// Retry delay
    /// </summary>
    public TimeSpan RetryDelay { get; set; } = TimeSpan.FromSeconds(1);
    
    /// <summary>
    /// Whether to use exponential backoff for retries
    /// </summary>
    public bool UseExponentialBackoff { get; set; } = true;
    
    /// <summary>
    /// Whether to log errors
    /// </summary>
    public bool LogErrors { get; set; } = true;
    
    /// <summary>
    /// Whether to continue processing after errors
    /// </summary>
    public bool ContinueOnError { get; set; } = true;
    
    /// <summary>
    /// Custom error handler
    /// </summary>
    public Func<Exception, MessageContext, Task<bool>>? CustomErrorHandler { get; set; }
}

/// <summary>
/// Represents error handling strategy
/// </summary>
public enum ErrorHandlingStrategy
{
    /// <summary>
    /// Requeue the message for retry
    /// </summary>
    Requeue,
    
    /// <summary>
    /// Reject the message and send to dead letter queue
    /// </summary>
    Reject,
    
    /// <summary>
    /// Acknowledge the message and continue (ignore error)
    /// </summary>
    Acknowledge,
    
    /// <summary>
    /// Use custom error handler
    /// </summary>
    Custom
} 