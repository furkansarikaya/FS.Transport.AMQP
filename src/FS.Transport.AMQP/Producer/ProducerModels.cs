namespace FS.Transport.AMQP.Producer;

/// <summary>
/// Producer status enumeration
/// </summary>
public enum ProducerStatus
{
    /// <summary>
    /// Producer has not been initialized
    /// </summary>
    NotInitialized,
    
    /// <summary>
    /// Producer is starting up
    /// </summary>
    Starting,
    
    /// <summary>
    /// Producer is running and ready to publish messages
    /// </summary>
    Running,
    
    /// <summary>
    /// Producer is stopping
    /// </summary>
    Stopping,
    
    /// <summary>
    /// Producer has stopped
    /// </summary>
    Stopped,
    
    /// <summary>
    /// Producer is in a faulted state due to an error
    /// </summary>
    Faulted
}

/// <summary>
/// Statistics for message producer performance and health monitoring
/// </summary>
public class ProducerStatistics
{
    /// <summary>
    /// Producer name identifier
    /// </summary>
    public string Name { get; set; } = "";
    
    /// <summary>
    /// Current producer status
    /// </summary>
    public ProducerStatus Status { get; set; }
    
    /// <summary>
    /// Time when producer was started
    /// </summary>
    public DateTimeOffset StartTime { get; set; }
    
    /// <summary>
    /// Time when producer was last updated
    /// </summary>
    public DateTimeOffset LastUpdateTime { get; set; }
    
    /// <summary>
    /// Total number of messages attempted to be published
    /// </summary>
    public long TotalMessages { get; set; }
    
    /// <summary>
    /// Number of successfully published messages
    /// </summary>
    public long SuccessfulMessages { get; set; }
    
    /// <summary>
    /// Number of failed message publish attempts
    /// </summary>
    public long FailedMessages { get; set; }
    
    /// <summary>
    /// Total number of batches processed
    /// </summary>
    public long TotalBatches { get; set; }
    
    /// <summary>
    /// Number of messages currently pending confirmation
    /// </summary>
    public long PendingConfirmations { get; set; }
    
    /// <summary>
    /// Number of scheduled messages waiting to be published
    /// </summary>
    public long ScheduledMessages { get; set; }
    
    /// <summary>
    /// Number of active transactions
    /// </summary>
    public long ActiveTransactions { get; set; }
    
    /// <summary>
    /// Average message publish latency in milliseconds
    /// </summary>
    public double AverageLatency { get; set; }
    
    /// <summary>
    /// Minimum message publish latency in milliseconds
    /// </summary>
    public double MinLatency { get; set; }
    
    /// <summary>
    /// Maximum message publish latency in milliseconds
    /// </summary>
    public double MaxLatency { get; set; }
    
    /// <summary>
    /// Messages published per second (calculated over last minute)
    /// </summary>
    public double MessagesPerSecond { get; set; }
    
    /// <summary>
    /// Success rate as percentage (0-100)
    /// </summary>
    public double SuccessRate => TotalMessages > 0 ? (double)SuccessfulMessages / TotalMessages * 100 : 0;
    
    /// <summary>
    /// Failure rate as percentage (0-100)
    /// </summary>
    public double FailureRate => TotalMessages > 0 ? (double)FailedMessages / TotalMessages * 100 : 0;
    
    /// <summary>
    /// Last error that occurred
    /// </summary>
    public Exception? LastError { get; set; }
    
    /// <summary>
    /// Time when last error occurred
    /// </summary>
    public DateTimeOffset? LastErrorTime { get; set; }
    
    /// <summary>
    /// Additional custom metrics
    /// </summary>
    public IDictionary<string, object> CustomMetrics { get; set; } = new Dictionary<string, object>();
}

/// <summary>
/// Result of a message publish operation
/// </summary>
public class PublishResult
{
    /// <summary>
    /// Whether the publish operation was successful
    /// </summary>
    public bool IsSuccess { get; set; }
    
    /// <summary>
    /// Unique identifier of the published message
    /// </summary>
    public string MessageId { get; set; } = "";
    
    /// <summary>
    /// Sequence number assigned to the message by the broker
    /// </summary>
    public ulong? SequenceNumber { get; set; }
    
    /// <summary>
    /// Time taken to publish the message in milliseconds
    /// </summary>
    public double PublishLatency { get; set; }
    
    /// <summary>
    /// Whether the message was confirmed by the broker
    /// </summary>
    public bool Confirmed { get; set; }
    
    /// <summary>
    /// Whether the message was deduplicated (already published)
    /// </summary>
    public bool Deduplicated { get; set; }
    
    /// <summary>
    /// Error that occurred during publishing (if any)
    /// </summary>
    public Exception? Error { get; set; }
    
    /// <summary>
    /// Error message (if any)
    /// </summary>
    public string? ErrorMessage => Error?.Message;
    
    /// <summary>
    /// Timestamp when the publish operation completed
    /// </summary>
    public DateTimeOffset Timestamp { get; set; } = DateTimeOffset.UtcNow;
    
    /// <summary>
    /// Additional metadata about the publish operation
    /// </summary>
    public IDictionary<string, object> Metadata { get; set; } = new Dictionary<string, object>();
    
    /// <summary>
    /// Creates a successful publish result
    /// </summary>
    /// <param name="messageId">Message identifier</param>
    /// <returns>Success result</returns>
    public static PublishResult Success(string messageId)
    {
        return new PublishResult
        {
            IsSuccess = true,
            MessageId = messageId,
            Timestamp = DateTimeOffset.UtcNow
        };
    }
    
    /// <summary>
    /// Creates a failed publish result
    /// </summary>
    /// <param name="error">Error that occurred</param>
    /// <param name="messageId">Message identifier</param>
    /// <returns>Failure result</returns>
    public static PublishResult Failure(Exception error, string messageId)
    {
        return new PublishResult
        {
            IsSuccess = false,
            MessageId = messageId,
            Error = error,
            Timestamp = DateTimeOffset.UtcNow
        };
    }
}

/// <summary>
/// Result of a batch publish operation
/// </summary>
public class BatchPublishResult
{
    /// <summary>
    /// Whether the entire batch was successful
    /// </summary>
    public bool IsSuccess { get; set; }
    
    /// <summary>
    /// Unique identifier for this batch
    /// </summary>
    public string BatchId { get; set; } = "";
    
    /// <summary>
    /// Individual results for each message in the batch
    /// </summary>
    public List<PublishResult> Results { get; set; } = new();
    
    /// <summary>
    /// Number of successfully published messages
    /// </summary>
    public int SuccessCount { get; set; }
    
    /// <summary>
    /// Number of failed message publish attempts
    /// </summary>
    public int FailureCount { get; set; }
    
    /// <summary>
    /// Total number of messages in the batch
    /// </summary>
    public int TotalCount { get; set; }
    
    /// <summary>
    /// Total time taken to process the batch in milliseconds
    /// </summary>
    public double TotalLatency { get; set; }
    
    /// <summary>
    /// Average latency per message in milliseconds
    /// </summary>
    public double AverageLatency { get; set; }
    
    /// <summary>
    /// Success rate as percentage (0-100)
    /// </summary>
    public double SuccessRate => TotalCount > 0 ? (double)SuccessCount / TotalCount * 100 : 0;
    
    /// <summary>
    /// Timestamp when the batch operation completed
    /// </summary>
    public DateTimeOffset Timestamp { get; set; } = DateTimeOffset.UtcNow;
}

/// <summary>
/// Options for customizing message publishing behavior
/// </summary>
public class PublishOptions
{
    /// <summary>
    /// Message priority (0-255, higher values indicate higher priority)
    /// </summary>
    public byte? Priority { get; set; }
    
    /// <summary>
    /// Message time-to-live
    /// </summary>
    public TimeSpan? TimeToLive { get; set; }
    
    /// <summary>
    /// Message expiration time
    /// </summary>
    public DateTimeOffset? ExpiresAt { get; set; }
    
    /// <summary>
    /// Correlation ID for request-response patterns
    /// </summary>
    public string? CorrelationId { get; set; }
    
    /// <summary>
    /// Reply-to queue for responses
    /// </summary>
    public string? ReplyTo { get; set; }
    
    /// <summary>
    /// Custom headers to include with the message
    /// </summary>
    public IDictionary<string, object>? Headers { get; set; }
    
    /// <summary>
    /// Whether to wait for broker confirmation
    /// </summary>
    public bool WaitForConfirmation { get; set; } = true;
    
    /// <summary>
    /// Whether the message should be persistent
    /// </summary>
    public bool Persistent { get; set; } = true;
    
    /// <summary>
    /// Whether the message is mandatory
    /// </summary>
    public bool Mandatory { get; set; } = false;
    
    /// <summary>
    /// Custom serializer to use for this message
    /// </summary>
    public string? SerializerType { get; set; }
    
    /// <summary>
    /// Retry policy to use for this message
    /// </summary>
    public string? RetryPolicy { get; set; }
    
    /// <summary>
    /// Maximum number of retries for this message
    /// </summary>
    public int? MaxRetries { get; set; }
}

/// <summary>
/// Result of a scheduled message operation
/// </summary>
public class ScheduleResult
{
    /// <summary>
    /// Whether the scheduling was successful
    /// </summary>
    public bool IsSuccess { get; set; }
    
    /// <summary>
    /// Unique identifier for the scheduled operation
    /// </summary>
    public string ScheduleId { get; set; } = "";
    
    /// <summary>
    /// Message identifier
    /// </summary>
    public string MessageId { get; set; } = "";
    
    /// <summary>
    /// When the message is scheduled to be published
    /// </summary>
    public DateTimeOffset ScheduledTime { get; set; }
    
    /// <summary>
    /// Error that occurred during scheduling (if any)
    /// </summary>
    public Exception? Error { get; set; }
    
    /// <summary>
    /// Error message (if any)
    /// </summary>
    public string? ErrorMessage => Error?.Message;
}

/// <summary>
/// Options for batch publish operations
/// </summary>
public class BatchPublishOptions
{
    /// <summary>
    /// Whether to wait for all confirmations in the batch
    /// </summary>
    public bool WaitForAllConfirmations { get; set; } = true;
    
    /// <summary>
    /// Maximum time to wait for batch confirmation
    /// </summary>
    public TimeSpan BatchTimeout { get; set; } = TimeSpan.FromSeconds(30);
    
    /// <summary>
    /// Whether to fail the entire batch if one message fails
    /// </summary>
    public bool FailOnAnyError { get; set; } = false;
    
    /// <summary>
    /// Whether to stop processing batch on first error
    /// </summary>
    public bool StopOnFirstError { get; set; } = false;
    
    /// <summary>
    /// Maximum number of concurrent publish operations
    /// </summary>
    public int MaxConcurrency { get; set; } = Environment.ProcessorCount;
    
    /// <summary>
    /// Whether to use transaction for the batch
    /// </summary>
    public bool UseTransaction { get; set; } = false;
    
    /// <summary>
    /// Retry policy for failed messages in batch
    /// </summary>
    public string? RetryPolicy { get; set; }
    
    /// <summary>
    /// Maximum retries for failed messages
    /// </summary>
    public int MaxRetries { get; set; } = 3;
    
    /// <summary>
    /// Whether to preserve message order in batch
    /// </summary>
    public bool PreserveOrder { get; set; } = false;
    
    /// <summary>
    /// Creates default batch publish options
    /// </summary>
    /// <returns>Default batch publish options</returns>
    public static BatchPublishOptions CreateDefault()
    {
        return new BatchPublishOptions();
    }
    
    /// <summary>
    /// Creates batch publish options for high throughput
    /// </summary>
    /// <returns>High throughput batch publish options</returns>
    public static BatchPublishOptions CreateHighThroughput()
    {
        return new BatchPublishOptions
        {
            WaitForAllConfirmations = false,
            FailOnAnyError = false,
            StopOnFirstError = false,
            MaxConcurrency = Environment.ProcessorCount * 2,
            UseTransaction = false,
            PreserveOrder = false
        };
    }
    
    /// <summary>
    /// Creates batch publish options for reliability
    /// </summary>
    /// <returns>Reliable batch publish options</returns>
    public static BatchPublishOptions CreateReliable()
    {
        return new BatchPublishOptions
        {
            WaitForAllConfirmations = true,
            FailOnAnyError = true,
            StopOnFirstError = false,
            MaxConcurrency = Environment.ProcessorCount,
            UseTransaction = true,
            MaxRetries = 5,
            PreserveOrder = true
        };
    }
}

// Event Arguments for Producer Events
/// <summary>
/// Event arguments for message published event
/// </summary>
public class MessagePublishedEventArgs : EventArgs
{
    public string MessageId { get; set; } = "";
    public string Exchange { get; set; } = "";
    public string RoutingKey { get; set; } = "";
    public string MessageType { get; set; } = "";
    public DateTimeOffset Timestamp { get; set; }
}

/// <summary>
/// Event arguments for message publish failed event
/// </summary>
public class MessagePublishFailedEventArgs : EventArgs
{
    public string MessageId { get; set; } = "";
    public string Exchange { get; set; } = "";
    public string RoutingKey { get; set; } = "";
    public Exception? Error { get; set; }
    public DateTimeOffset Timestamp { get; set; }
}

/// <summary>
/// Event arguments for message confirmed event
/// </summary>
public class MessageConfirmedEventArgs : EventArgs
{
    public string MessageId { get; set; } = "";
    public ulong SequenceNumber { get; set; }
    public bool Multiple { get; set; }
    public DateTimeOffset Timestamp { get; set; }
}

/// <summary>
/// Event arguments for message rejected event
/// </summary>
public class MessageRejectedEventArgs : EventArgs
{
    public string MessageId { get; set; } = "";
    public ulong SequenceNumber { get; set; }
    public bool Multiple { get; set; }
    public bool Requeue { get; set; }
    public DateTimeOffset Timestamp { get; set; }
}

/// <summary>
/// Event arguments for producer status changed event
/// </summary>
public class ProducerStatusChangedEventArgs : EventArgs
{
    public ProducerStatus OldStatus { get; set; }
    public ProducerStatus NewStatus { get; set; }
    public string Reason { get; set; } = "";
    public Exception? Error { get; set; }
    public DateTimeOffset Timestamp { get; set; }
}

/// <summary>
/// Event arguments for batch publish completed event
/// </summary>
public class BatchPublishCompletedEventArgs : EventArgs
{
    public string BatchId { get; set; } = "";
    public BatchPublishResult Result { get; set; } = new();
    public DateTimeOffset Timestamp { get; set; }
} 