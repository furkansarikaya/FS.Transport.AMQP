namespace FS.Transport.AMQP.Consumer;

/// <summary>
/// Consumer operational status
/// </summary>
public enum ConsumerStatus
{
    /// <summary>
    /// Consumer has not been initialized
    /// </summary>
    NotInitialized,
    
    /// <summary>
    /// Consumer is starting up
    /// </summary>
    Starting,
    
    /// <summary>
    /// Consumer is running and consuming messages
    /// </summary>
    Running,
    
    /// <summary>
    /// Consumer is paused
    /// </summary>
    Paused,
    
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
    Faulted,
    
    /// <summary>
    /// Consumer is reconnecting after connection loss
    /// </summary>
    Reconnecting
}

/// <summary>
/// Consumer statistics and metrics
/// </summary>
public class ConsumerStatistics
{
    /// <summary>
    /// Consumer name for identification
    /// </summary>
    public string Name { get; set; } = "";
    
    /// <summary>
    /// Current consumer status
    /// </summary>
    public ConsumerStatus Status { get; set; }
    
    /// <summary>
    /// Consumer start time
    /// </summary>
    public DateTimeOffset StartTime { get; set; }
    
    /// <summary>
    /// Last statistics update time
    /// </summary>
    public DateTimeOffset LastUpdateTime { get; set; }
    
    /// <summary>
    /// Total number of messages consumed
    /// </summary>
    public long TotalMessages { get; set; }
    
    /// <summary>
    /// Number of successfully processed messages
    /// </summary>
    public long SuccessfulMessages { get; set; }
    
    /// <summary>
    /// Number of failed messages
    /// </summary>
    public long FailedMessages { get; set; }
    
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
    /// Number of messages processed in batches
    /// </summary>
    public long BatchesProcessed { get; set; }
    
    /// <summary>
    /// Number of currently processing messages
    /// </summary>
    public long CurrentlyProcessing { get; set; }
    
    /// <summary>
    /// Number of messages in the deduplication cache
    /// </summary>
    public long DeduplicationCacheSize { get; set; }
    
    /// <summary>
    /// Number of duplicate messages detected
    /// </summary>
    public long DuplicatesDetected { get; set; }
    
    /// <summary>
    /// Average processing time per message in milliseconds
    /// </summary>
    public double AverageProcessingTime { get; set; }
    
    /// <summary>
    /// Minimum processing time in milliseconds
    /// </summary>
    public double MinProcessingTime { get; set; }
    
    /// <summary>
    /// Maximum processing time in milliseconds
    /// </summary>
    public double MaxProcessingTime { get; set; }
    
    /// <summary>
    /// Messages processed per second
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
    /// Timestamp of the last error
    /// </summary>
    public DateTimeOffset? LastErrorTime { get; set; }
    
    /// <summary>
    /// Consumer uptime
    /// </summary>
    public TimeSpan Uptime => DateTimeOffset.UtcNow - StartTime;
    
    /// <summary>
    /// Connection recovery count
    /// </summary>
    public long ConnectionRecoveries { get; set; }
    
    /// <summary>
    /// Circuit breaker state
    /// </summary>
    public string CircuitBreakerState { get; set; } = "Closed";
    
    /// <summary>
    /// Circuit breaker failure count
    /// </summary>
    public int CircuitBreakerFailureCount { get; set; }
    
    /// <summary>
    /// Custom metrics
    /// </summary>
    public IDictionary<string, object> CustomMetrics { get; set; } = new Dictionary<string, object>();
}

/// <summary>
/// Result of a consumption operation
/// </summary>
public class ConsumeResult
{
    /// <summary>
    /// Whether the consumption was successful
    /// </summary>
    public bool IsSuccess { get; set; }
    
    /// <summary>
    /// Message identifier
    /// </summary>
    public string MessageId { get; set; } = "";
    
    /// <summary>
    /// Delivery tag
    /// </summary>
    public ulong DeliveryTag { get; set; }
    
    /// <summary>
    /// Processing time in milliseconds
    /// </summary>
    public double ProcessingTime { get; set; }
    
    /// <summary>
    /// Whether the message was acknowledged
    /// </summary>
    public bool Acknowledged { get; set; }
    
    /// <summary>
    /// Whether the message was a duplicate
    /// </summary>
    public bool WasDuplicate { get; set; }
    
    /// <summary>
    /// Error that occurred during processing (if any)
    /// </summary>
    public Exception? Error { get; set; }
    
    /// <summary>
    /// Error message (if any)
    /// </summary>
    public string? ErrorMessage => Error?.Message;
    
    /// <summary>
    /// Timestamp when the consumption occurred
    /// </summary>
    public DateTimeOffset Timestamp { get; set; } = DateTimeOffset.UtcNow;
    
    /// <summary>
    /// Additional metadata
    /// </summary>
    public IDictionary<string, object> Metadata { get; set; } = new Dictionary<string, object>();
    
    /// <summary>
    /// Creates a successful consumption result
    /// </summary>
    /// <param name="messageId">Message identifier</param>
    /// <param name="deliveryTag">Delivery tag</param>
    /// <returns>Successful consumption result</returns>
    public static ConsumeResult Success(string messageId, ulong deliveryTag)
    {
        return new ConsumeResult
        {
            IsSuccess = true,
            MessageId = messageId,
            DeliveryTag = deliveryTag,
            Acknowledged = true,
            Timestamp = DateTimeOffset.UtcNow
        };
    }
    
    /// <summary>
    /// Creates a failed consumption result
    /// </summary>
    /// <param name="error">Error that occurred</param>
    /// <param name="messageId">Message identifier</param>
    /// <param name="deliveryTag">Delivery tag</param>
    /// <returns>Failed consumption result</returns>
    public static ConsumeResult Failure(Exception error, string messageId, ulong deliveryTag)
    {
        return new ConsumeResult
        {
            IsSuccess = false,
            MessageId = messageId,
            DeliveryTag = deliveryTag,
            Error = error,
            Acknowledged = false,
            Timestamp = DateTimeOffset.UtcNow
        };
    }
}

/// <summary>
/// Result of a batch consumption operation
/// </summary>
public class BatchConsumeResult
{
    /// <summary>
    /// Whether the batch consumption was successful
    /// </summary>
    public bool IsSuccess { get; set; }
    
    /// <summary>
    /// Batch identifier
    /// </summary>
    public string BatchId { get; set; } = "";
    
    /// <summary>
    /// Individual consumption results
    /// </summary>
    public List<ConsumeResult> Results { get; set; } = new();
    
    /// <summary>
    /// Number of successful messages in the batch
    /// </summary>
    public int SuccessCount { get; set; }
    
    /// <summary>
    /// Number of failed messages in the batch
    /// </summary>
    public int FailureCount { get; set; }
    
    /// <summary>
    /// Total number of messages in the batch
    /// </summary>
    public int TotalCount { get; set; }
    
    /// <summary>
    /// Total processing time for the batch in milliseconds
    /// </summary>
    public double TotalProcessingTime { get; set; }
    
    /// <summary>
    /// Average processing time per message in milliseconds
    /// </summary>
    public double AverageProcessingTime { get; set; }
    
    /// <summary>
    /// Success rate as percentage (0-100)
    /// </summary>
    public double SuccessRate => TotalCount > 0 ? (double)SuccessCount / TotalCount * 100 : 0;
    
    /// <summary>
    /// Timestamp when the batch consumption completed
    /// </summary>
    public DateTimeOffset Timestamp { get; set; } = DateTimeOffset.UtcNow;
}

// Event Arguments for Consumer Events
/// <summary>
/// Event arguments for message received event
/// </summary>
public class MessageReceivedEventArgs : EventArgs
{
    /// <summary>
    /// Message identifier
    /// </summary>
    public string MessageId { get; set; } = "";
    
    /// <summary>
    /// Queue name where the message was received
    /// </summary>
    public string QueueName { get; set; } = "";
    
    /// <summary>
    /// Exchange name where the message was published
    /// </summary>
    public string Exchange { get; set; } = "";
    
    /// <summary>
    /// Routing key used for message routing
    /// </summary>
    public string RoutingKey { get; set; } = "";
    
    /// <summary>
    /// Message type
    /// </summary>
    public string MessageType { get; set; } = "";
    
    /// <summary>
    /// Message size in bytes
    /// </summary>
    public int MessageSize { get; set; }
    
    /// <summary>
    /// Delivery tag
    /// </summary>
    public ulong DeliveryTag { get; set; }
    
    /// <summary>
    /// Whether the message was redelivered
    /// </summary>
    public bool Redelivered { get; set; }
    
    /// <summary>
    /// Consumer tag
    /// </summary>
    public string ConsumerTag { get; set; } = "";
    
    /// <summary>
    /// Timestamp when the message was received
    /// </summary>
    public DateTimeOffset Timestamp { get; set; }
}

/// <summary>
/// Event arguments for message processed event
/// </summary>
public class MessageProcessedEventArgs : EventArgs
{
    /// <summary>
    /// Message identifier
    /// </summary>
    public string MessageId { get; set; } = "";
    
    /// <summary>
    /// Queue name where the message was processed
    /// </summary>
    public string QueueName { get; set; } = "";
    
    /// <summary>
    /// Exchange name where the message was published
    /// </summary>
    public string Exchange { get; set; } = "";
    
    /// <summary>
    /// Routing key used for message routing
    /// </summary>
    public string RoutingKey { get; set; } = "";
    
    /// <summary>
    /// Message type
    /// </summary>
    public string MessageType { get; set; } = "";
    
    /// <summary>
    /// Processing time in milliseconds
    /// </summary>
    public double ProcessingTime { get; set; }
    
    /// <summary>
    /// Delivery tag
    /// </summary>
    public ulong DeliveryTag { get; set; }
    
    /// <summary>
    /// Whether the message was acknowledged
    /// </summary>
    public bool Acknowledged { get; set; }
    
    /// <summary>
    /// Whether the message was a duplicate
    /// </summary>
    public bool WasDuplicate { get; set; }
    
    /// <summary>
    /// Consumer tag
    /// </summary>
    public string ConsumerTag { get; set; } = "";
    
    /// <summary>
    /// Timestamp when the message was processed
    /// </summary>
    public DateTimeOffset Timestamp { get; set; }
}

/// <summary>
/// Event arguments for message processing failed event
/// </summary>
public class MessageProcessingFailedEventArgs : EventArgs
{
    /// <summary>
    /// Message identifier
    /// </summary>
    public string MessageId { get; set; } = "";
    
    /// <summary>
    /// Queue name where the message was being processed
    /// </summary>
    public string QueueName { get; set; } = "";
    
    /// <summary>
    /// Exchange name where the message was published
    /// </summary>
    public string Exchange { get; set; } = "";
    
    /// <summary>
    /// Routing key used for message routing
    /// </summary>
    public string RoutingKey { get; set; } = "";
    
    /// <summary>
    /// Message type
    /// </summary>
    public string MessageType { get; set; } = "";
    
    /// <summary>
    /// Error that occurred during processing
    /// </summary>
    public Exception? Error { get; set; }
    
    /// <summary>
    /// Delivery tag
    /// </summary>
    public ulong DeliveryTag { get; set; }
    
    /// <summary>
    /// Whether the message was requeued
    /// </summary>
    public bool Requeued { get; set; }
    
    /// <summary>
    /// Retry attempt number
    /// </summary>
    public int RetryAttempt { get; set; }
    
    /// <summary>
    /// Consumer tag
    /// </summary>
    public string ConsumerTag { get; set; } = "";
    
    /// <summary>
    /// Timestamp when the processing failed
    /// </summary>
    public DateTimeOffset Timestamp { get; set; }
}

/// <summary>
/// Event arguments for message acknowledged event
/// </summary>
public class MessageAcknowledgedEventArgs : EventArgs
{
    /// <summary>
    /// Message identifier
    /// </summary>
    public string MessageId { get; set; } = "";
    
    /// <summary>
    /// Delivery tag
    /// </summary>
    public ulong DeliveryTag { get; set; }
    
    /// <summary>
    /// Whether multiple messages were acknowledged
    /// </summary>
    public bool Multiple { get; set; }
    
    /// <summary>
    /// Consumer tag
    /// </summary>
    public string ConsumerTag { get; set; } = "";
    
    /// <summary>
    /// Timestamp when the message was acknowledged
    /// </summary>
    public DateTimeOffset Timestamp { get; set; }
}

/// <summary>
/// Event arguments for message rejected event
/// </summary>
public class MessageRejectedEventArgs : EventArgs
{
    /// <summary>
    /// Message identifier
    /// </summary>
    public string MessageId { get; set; } = "";
    
    /// <summary>
    /// Delivery tag
    /// </summary>
    public ulong DeliveryTag { get; set; }
    
    /// <summary>
    /// Whether the message was requeued
    /// </summary>
    public bool Requeued { get; set; }
    
    /// <summary>
    /// Consumer tag
    /// </summary>
    public string ConsumerTag { get; set; } = "";
    
    /// <summary>
    /// Timestamp when the message was rejected
    /// </summary>
    public DateTimeOffset Timestamp { get; set; }
}

/// <summary>
/// Event arguments for consumer status changed event
/// </summary>
public class ConsumerStatusChangedEventArgs : EventArgs
{
    /// <summary>
    /// Previous consumer status
    /// </summary>
    public ConsumerStatus OldStatus { get; set; }
    
    /// <summary>
    /// New consumer status
    /// </summary>
    public ConsumerStatus NewStatus { get; set; }
    
    /// <summary>
    /// Reason for the status change
    /// </summary>
    public string Reason { get; set; } = "";
    
    /// <summary>
    /// Error that caused the status change (if any)
    /// </summary>
    public Exception? Error { get; set; }
    
    /// <summary>
    /// Consumer tag
    /// </summary>
    public string ConsumerTag { get; set; } = "";
    
    /// <summary>
    /// Timestamp when the status changed
    /// </summary>
    public DateTimeOffset Timestamp { get; set; }
}

/// <summary>
/// Event arguments for consumer paused event
/// </summary>
public class ConsumerPausedEventArgs : EventArgs
{
    /// <summary>
    /// Consumer tag
    /// </summary>
    public string ConsumerTag { get; set; } = "";
    
    /// <summary>
    /// Reason for pausing
    /// </summary>
    public string Reason { get; set; } = "";
    
    /// <summary>
    /// Timestamp when the consumer was paused
    /// </summary>
    public DateTimeOffset Timestamp { get; set; }
}

/// <summary>
/// Event arguments for consumer resumed event
/// </summary>
public class ConsumerResumedEventArgs : EventArgs
{
    /// <summary>
    /// Consumer tag
    /// </summary>
    public string ConsumerTag { get; set; } = "";
    
    /// <summary>
    /// Reason for resuming
    /// </summary>
    public string Reason { get; set; } = "";
    
    /// <summary>
    /// How long the consumer was paused
    /// </summary>
    public TimeSpan PausedDuration { get; set; }
    
    /// <summary>
    /// Timestamp when the consumer was resumed
    /// </summary>
    public DateTimeOffset Timestamp { get; set; }
} 