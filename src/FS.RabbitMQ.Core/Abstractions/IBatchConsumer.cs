namespace FS.RabbitMQ.Core.Abstractions;

/// <summary>
/// Defines a batch consumer that processes multiple messages together for improved efficiency.
/// </summary>
/// <typeparam name="T">The type of message payload being consumed.</typeparam>
/// <remarks>
/// Batch consumers optimize throughput by collecting and processing multiple messages
/// as a single unit. This pattern reduces per-message overhead and enables efficient
/// resource utilization for high-volume scenarios while maintaining processing guarantees.
/// 
/// Key characteristics:
/// - Automatic batch formation based on size and timing criteria
/// - Atomic batch processing with all-or-nothing semantics
/// - Intelligent batch optimization based on message flow patterns
/// - Comprehensive error handling for partial batch failures
/// </remarks>
public interface IBatchConsumer<T> : IMessageConsumer where T : class
{
    /// <summary>
    /// Gets the configured batch size for this consumer.
    /// </summary>
    /// <value>The maximum number of messages processed in each batch.</value>
    /// <remarks>
    /// Batch size affects performance and resource utilization:
    /// - Larger batches improve throughput but increase latency
    /// - Smaller batches reduce latency but may decrease overall throughput
    /// - Optimal batch size depends on message characteristics and processing requirements
    /// - Batch size can be tuned based on performance monitoring and analysis
    /// </remarks>
    int BatchSize { get; }

    /// <summary>
    /// Gets the current batch timeout setting.
    /// </summary>
    /// <value>The maximum time to wait for a batch to reach the configured size before processing partial batches.</value>
    /// <remarks>
    /// Batch timeout prevents indefinite waiting for full batches:
    /// - Ensures timely processing even when message flow is irregular
    /// - Balances throughput optimization with latency requirements
    /// - Enables processing of partial batches during low-traffic periods
    /// - Can be adjusted based on SLA requirements and traffic patterns
    /// </remarks>
    TimeSpan BatchTimeout { get; }

    /// <summary>
    /// Updates the batch processing configuration dynamically.
    /// </summary>
    /// <param name="newBatchSize">The new batch size to use. Must be positive.</param>
    /// <param name="newBatchTimeout">The new batch timeout to use. Must be positive.</param>
    /// <param name="cancellationToken">Token to monitor for cancellation requests.</param>
    /// <returns>A task that represents the asynchronous configuration update.</returns>
    /// <exception cref="ArgumentOutOfRangeException">
    /// Thrown when <paramref name="newBatchSize"/> is not positive or <paramref name="newBatchTimeout"/> is not positive.
    /// </exception>
    /// <exception cref="ConsumerException">Thrown when the configuration cannot be updated due to consumer state or connectivity issues.</exception>
    /// <remarks>
    /// Dynamic configuration updates enable runtime optimization:
    /// - Adjust batch parameters based on observed performance characteristics
    /// - Respond to changing traffic patterns and system load
    /// - Implement adaptive batch sizing algorithms
    /// - Support A/B testing of different batch configurations
    /// 
    /// Configuration changes take effect for subsequent batches and do not
    /// affect batches currently being formed or processed.
    /// </remarks>
    Task UpdateBatchConfigurationAsync(
        int newBatchSize, 
        TimeSpan newBatchTimeout, 
        CancellationToken cancellationToken = default);
}