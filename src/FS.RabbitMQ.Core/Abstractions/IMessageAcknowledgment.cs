namespace FS.RabbitMQ.Core.Abstractions;

/// <summary>
/// Defines the contract for message acknowledgment and lifecycle management operations.
/// </summary>
/// <remarks>
/// Message acknowledgment provides fine-grained control over message delivery guarantees
/// and enables implementation of sophisticated error handling and retry strategies.
/// Proper acknowledgment handling is essential for reliable message processing and
/// preventing message loss or duplication in distributed systems.
/// </remarks>
public interface IMessageAcknowledgment
{
    /// <summary>
    /// Gets a value indicating whether the message has been acknowledged.
    /// </summary>
    /// <value><c>true</c> if any acknowledgment operation has been performed; otherwise, <c>false</c>.</value>
    /// <remarks>
    /// Acknowledgment status helps prevent duplicate acknowledgment operations
    /// and enables conditional processing logic based on message state.
    /// </remarks>
    bool IsAcknowledged { get; }

    /// <summary>
    /// Gets the type of acknowledgment that was performed.
    /// </summary>
    /// <value>The acknowledgment type, or null if no acknowledgment has been performed.</value>
    /// <remarks>
    /// Acknowledgment type information enables:
    /// - Audit trails and monitoring of message processing outcomes
    /// - Metrics collection segmented by acknowledgment type
    /// - Debugging and analysis of processing patterns
    /// - Implementation of acknowledgment-aware business logic
    /// </remarks>
    AcknowledgmentType? AcknowledgmentType { get; }

    /// <summary>
    /// Positively acknowledges the message, indicating successful processing.
    /// </summary>
    /// <param name="cancellationToken">Token to monitor for cancellation requests.</param>
    /// <returns>A task that represents the asynchronous acknowledgment operation.</returns>
    /// <exception cref="InvalidOperationException">Thrown when the message has already been acknowledged.</exception>
    /// <exception cref="MessagingException">Thrown when the acknowledgment operation fails due to connectivity or broker issues.</exception>
    /// <remarks>
    /// Positive acknowledgment:
    /// - Confirms that the message was processed successfully
    /// - Removes the message from the queue permanently
    /// - Indicates that no further processing is needed
    /// - Completes the message delivery lifecycle
    /// 
    /// Use positive acknowledgment when message processing completes successfully
    /// and the message should not be redelivered to any consumer.
    /// </remarks>
    Task AckAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Negatively acknowledges the message with optional requeue for retry processing.
    /// </summary>
    /// <param name="requeue">Whether the message should be requeued for potential redelivery to consumers.</param>
    /// <param name="cancellationToken">Token to monitor for cancellation requests.</param>
    /// <returns>A task that represents the asynchronous negative acknowledgment operation.</returns>
    /// <exception cref="InvalidOperationException">Thrown when the message has already been acknowledged.</exception>
    /// <exception cref="MessagingException">Thrown when the acknowledgment operation fails due to connectivity or broker issues.</exception>
    /// <remarks>
    /// Negative acknowledgment with requeue:
    /// - Indicates that processing failed but retry may be successful
    /// - Returns the message to the queue for redelivery
    /// - Increments the delivery count for the message
    /// - Enables automatic retry with backoff strategies
    /// 
    /// Negative acknowledgment without requeue:
    /// - Indicates that processing failed and retry is not desired
    /// - Removes the message from the queue (may route to dead letter exchange)
    /// - Prevents infinite retry loops for unprocessable messages
    /// - Suitable for poison messages or permanent processing failures
    /// </remarks>
    Task NackAsync(bool requeue = true, CancellationToken cancellationToken = default);

    /// <summary>
    /// Rejects the message and optionally routes it to a dead letter exchange.
    /// </summary>
    /// <param name="requeue">Whether the message should be requeued for potential redelivery.</param>
    /// <param name="cancellationToken">Token to monitor for cancellation requests.</param>
    /// <returns>A task that represents the asynchronous rejection operation.</returns>
    /// <exception cref="InvalidOperationException">Thrown when the message has already been acknowledged.</exception>
    /// <exception cref="MessagingException">Thrown when the rejection operation fails due to connectivity or broker issues.</exception>
    /// <remarks>
    /// Message rejection provides:
    /// - Explicit indication that the message cannot be processed
    /// - Routing to dead letter exchanges for error handling
    /// - Support for poison message isolation and analysis
    /// - Integration with error handling and alerting systems
    /// 
    /// Rejection is typically used for messages that are malformed,
    /// violate business rules, or exceed retry limits.
    /// </remarks>
    Task RejectAsync(bool requeue = false, CancellationToken cancellationToken = default);
}