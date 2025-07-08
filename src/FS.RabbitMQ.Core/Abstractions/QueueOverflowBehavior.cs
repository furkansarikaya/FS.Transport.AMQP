namespace FS.RabbitMQ.Core.Abstractions;

/// <summary>
/// Defines the behavior when a queue reaches its configured maximum length or size limits.
/// </summary>
/// <remarks>
/// Overflow behavior is critical for system stability and message delivery guarantees.
/// Each option provides different trade-offs between message preservation, system responsiveness,
/// and resource utilization under high load conditions.
/// </remarks>
public enum QueueOverflowBehavior
{
    /// <summary>
    /// Removes the oldest messages from the queue to make room for new messages.
    /// This maintains queue size limits while preserving the most recent messages.
    /// </summary>
    /// <remarks>
    /// Drop-head behavior implements a sliding window pattern:
    /// - Maintains FIFO ordering for message processing
    /// - Ensures queue size never exceeds configured limits
    /// - Older messages are sacrificed to accommodate newer ones
    /// - Useful for real-time data streams where recent data is more valuable
    /// 
    /// Consider this option when:
    /// - Message freshness is more important than message completeness
    /// - System stability requires strict resource limits
    /// - Processing naturally favors recent messages over historical ones
    /// </remarks>
    DropHead,

    /// <summary>
    /// Rejects new messages when the queue reaches capacity, applying backpressure to publishers.
    /// This preserves all messages currently in the queue but may impact publisher performance.
    /// </summary>
    /// <remarks>
    /// Reject-publish behavior provides strong message preservation guarantees:
    /// - No messages are lost once successfully enqueued
    /// - Publishers receive immediate feedback about capacity issues
    /// - May cause publisher blocking or error handling requirements
    /// - Enables upstream flow control and load shedding
    /// 
    /// Consider this option when:
    /// - Message preservation is critical for business requirements
    /// - Publishers can handle rejection and implement retry logic
    /// - Downstream processing will eventually catch up to clear the backlog
    /// - You prefer explicit backpressure over silent message loss
    /// </remarks>
    RejectPublish,

    /// <summary>
    /// Rejects new messages and routes them to the configured dead letter exchange.
    /// This combines backpressure with alternative message handling paths.
    /// </summary>
    /// <remarks>
    /// Reject-publish with dead letter exchange provides sophisticated overflow handling:
    /// - Preserves messages in the primary queue while handling overflow
    /// - Enables separate processing logic for overflow messages
    /// - Maintains audit trail of capacity-related message rejections
    /// - Allows for delayed processing or alternative routing of overflow messages
    /// 
    /// Consider this option when:
    /// - You need both capacity limits and comprehensive message tracking
    /// - Overflow messages require different processing logic
    /// - Compliance requirements mandate retention of all messages
    /// - You want to implement sophisticated retry or escalation patterns
    /// 
    /// Requires: A dead letter exchange must be configured for this option to work.
    /// </remarks>
    RejectPublishDlx
}