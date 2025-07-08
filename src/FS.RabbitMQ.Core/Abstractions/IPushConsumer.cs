namespace FS.RabbitMQ.Core.Abstractions;

/// <summary>
/// Defines a push-based consumer that automatically receives and processes messages as they arrive.
/// </summary>
/// <typeparam name="T">The type of message payload being consumed.</typeparam>
/// <remarks>
/// Push consumers implement the observer pattern for message processing, where messages
/// are automatically delivered to the consumer as they become available in the queue.
/// This pattern is optimal for real-time processing scenarios where low latency
/// and immediate response to messages are important.
/// 
/// Key characteristics:
/// - Automatic message delivery without polling
/// - Event-driven processing with minimal latency
/// - Built-in backpressure handling and flow control
/// - Comprehensive error handling and recovery mechanisms
/// </remarks>
public interface IPushConsumer<T> : IMessageConsumer where T : class
{
    /// <summary>
    /// Gets statistics about message processing performance and health.
    /// </summary>
    /// <value>Current statistics for monitoring and analysis purposes.</value>
    /// <remarks>
    /// Processing statistics provide insights into:
    /// - Message throughput and processing rates
    /// - Error rates and failure patterns
    /// - Consumer health and performance characteristics
    /// - Resource utilization and capacity planning data
    /// 
    /// Statistics are updated in real-time and can be used for monitoring
    /// dashboards, alerting systems, and performance optimization.
    /// </remarks>
    ConsumerStatistics Statistics { get; }

    /// <summary>
    /// Pauses message processing while keeping the consumer connection active.
    /// </summary>
    /// <param name="cancellationToken">Token to monitor for cancellation requests.</param>
    /// <returns>A task that represents the asynchronous pause operation.</returns>
    /// <remarks>
    /// Pausing allows temporary suspension of message processing without
    /// the overhead of stopping and restarting the consumer. Useful for:
    /// - Maintenance windows where processing should be temporarily halted
    /// - Backpressure scenarios where downstream systems need recovery time
    /// - Dynamic flow control based on external system conditions
    /// - Testing and debugging scenarios requiring controlled message flow
    /// 
    /// Messages continue to accumulate in the queue during pause periods
    /// and will be processed when the consumer is resumed.
    /// </remarks>
    Task PauseAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Resumes message processing after a pause operation.
    /// </summary>
    /// <param name="cancellationToken">Token to monitor for cancellation requests.</param>
    /// <returns>A task that represents the asynchronous resume operation.</returns>
    /// <remarks>
    /// Resuming restores normal message processing after a pause operation.
    /// The consumer will immediately begin processing any messages that
    /// accumulated during the pause period, subject to configured QoS
    /// and flow control settings.
    /// </remarks>
    Task ResumeAsync(CancellationToken cancellationToken = default);
}