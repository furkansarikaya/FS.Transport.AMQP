namespace FS.RabbitMQ.Core.Abstractions;

/// <summary>
/// Defines a pull-based consumer that retrieves messages on demand through explicit requests.
/// </summary>
/// <typeparam name="T">The type of message payload being consumed.</typeparam>
/// <remarks>
/// Pull consumers implement on-demand message retrieval, giving applications full control
/// over when and how many messages are processed. This pattern is optimal for batch
/// processing scenarios, resource-constrained environments, or situations requiring
/// sophisticated flow control and scheduling.
/// 
/// Key characteristics:
/// - Explicit message retrieval with full timing control
/// - Support for single message and batch retrieval operations
/// - Fine-grained flow control and backpressure management
/// - Integration with external scheduling and orchestration systems
/// </remarks>
public interface IPullConsumer<T> : IMessageConsumer where T : class
{
    /// <summary>
    /// Retrieves a single message from the queue if available.
    /// </summary>
    /// <param name="timeout">The maximum time to wait for a message to become available.</param>
    /// <param name="cancellationToken">Token to monitor for cancellation requests.</param>
    /// <returns>
    /// A task that represents the asynchronous retrieval operation.
    /// The task result contains the message context if a message was retrieved, or null if no message was available.
    /// </returns>
    /// <exception cref="ConsumerException">Thrown when message retrieval fails due to connectivity or configuration issues.</exception>
    /// <remarks>
    /// Single message retrieval provides fine-grained control over message processing:
    /// - Returns immediately if a message is available
    /// - Waits up to the specified timeout for message availability
    /// - Returns null if no message becomes available within the timeout period
    /// - Includes comprehensive message context for processing decisions
    /// 
    /// This method is ideal for scenarios requiring individual message inspection,
    /// conditional processing, or integration with external scheduling systems.
    /// </remarks>
    Task<MessageContext<T>?> ReceiveAsync(TimeSpan timeout, CancellationToken cancellationToken = default);

    /// <summary>
    /// Retrieves multiple messages from the queue up to the specified batch size.
    /// </summary>
    /// <param name="batchSize">The maximum number of messages to retrieve in this batch. Must be positive.</param>
    /// <param name="timeout">The maximum time to wait for messages to become available.</param>
    /// <param name="cancellationToken">Token to monitor for cancellation requests.</param>
    /// <returns>
    /// A task that represents the asynchronous batch retrieval operation.
    /// The task result contains a collection of message contexts, which may be empty if no messages were available.
    /// </returns>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when <paramref name="batchSize"/> is not positive.</exception>
    /// <exception cref="ConsumerException">Thrown when batch retrieval fails due to connectivity or configuration issues.</exception>
    /// <remarks>
    /// Batch message retrieval optimizes throughput for high-volume processing:
    /// - Returns immediately with available messages up to the batch size
    /// - Waits up to the specified timeout for additional messages if batch is not full
    /// - Returns partial batches if fewer messages are available
    /// - Enables efficient bulk processing and resource utilization
    /// 
    /// Batch retrieval is ideal for ETL processes, bulk data operations,
    /// and scenarios where processing overhead benefits from batching.
    /// </remarks>
    Task<IReadOnlyList<MessageContext<T>>> ReceiveBatchAsync(
        int batchSize, 
        TimeSpan timeout, 
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Checks if messages are available in the queue without retrieving them.
    /// </summary>
    /// <param name="cancellationToken">Token to monitor for cancellation requests.</param>
    /// <returns>
    /// A task that represents the asynchronous availability check.
    /// The task result indicates whether messages are currently available for consumption.
    /// </returns>
    /// <exception cref="ConsumerException">Thrown when the availability check fails due to connectivity issues.</exception>
    /// <remarks>
    /// Message availability checking enables efficient polling strategies:
    /// - Quick check without the overhead of message retrieval
    /// - Useful for implementing intelligent polling intervals
    /// - Enables conditional processing based on queue state
    /// - Supports integration with external monitoring and alerting systems
    /// 
    /// This method is particularly useful for implementing adaptive polling
    /// strategies that adjust frequency based on message availability patterns.
    /// </remarks>
    Task<bool> HasMessagesAsync(CancellationToken cancellationToken = default);
}