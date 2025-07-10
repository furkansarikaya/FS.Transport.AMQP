namespace FS.Transport.AMQP.Queue;

/// <summary>
/// Queue use cases for determining optimal configuration
/// </summary>
public enum QueueUseCase
{
    /// <summary>
    /// High throughput scenarios - use quorum queues
    /// </summary>
    HighThroughput,
    
    /// <summary>
    /// Low latency scenarios - use classic queues
    /// </summary>
    LowLatency,
    
    /// <summary>
    /// Message ordering scenarios - use single active consumer
    /// </summary>
    MessageOrdering,
    
    /// <summary>
    /// Temporary work queues - use auto-delete
    /// </summary>
    TemporaryWorkQueue,
    
    /// <summary>
    /// Priority-based processing - use priority queues
    /// </summary>
    PriorityProcessing
}