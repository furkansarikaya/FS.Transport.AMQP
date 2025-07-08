namespace FS.RabbitMQ.Core.Abstractions;

/// <summary>
/// Defines the types of message acknowledgment operations.
/// </summary>
/// <remarks>
/// Acknowledgment types provide semantic meaning to message processing outcomes
/// and enable appropriate handling of different processing scenarios.
/// </remarks>
public enum AcknowledgmentType
{
    /// <summary>
    /// Message was processed successfully and should be removed from the queue.
    /// </summary>
    Acknowledged = 0,

    /// <summary>
    /// Message processing failed but the message should be requeued for retry.
    /// </summary>
    NegativeAcknowledgedWithRequeue = 1,

    /// <summary>
    /// Message processing failed and the message should be discarded or sent to dead letter exchange.
    /// </summary>
    NegativeAcknowledgedWithoutRequeue = 2,

    /// <summary>
    /// Message was rejected and should be handled according to rejection policies.
    /// </summary>
    Rejected = 3
}