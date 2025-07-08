namespace FS.RabbitMQ.Core.Abstractions;

/// <summary>
/// Defines a custom consumer with user-defined processing strategies for specialized scenarios.
/// </summary>
/// <typeparam name="T">The type of message payload being consumed.</typeparam>
/// <remarks>
/// Custom consumers provide maximum flexibility by allowing complete control over
/// message processing flow, acknowledgment strategies, and error handling mechanisms.
/// This enables integration with specialized frameworks and implementation of
/// advanced processing patterns not covered by standard consumer types.
/// 
/// Key characteristics:
/// - Complete control over message lifecycle and processing flow
/// - Integration with custom frameworks and processing engines
/// - Support for experimental and domain-specific processing patterns
/// - Extensibility for specialized requirements and use cases
/// </remarks>
public interface ICustomConsumer<T> : IMessageConsumer where T : class
{
    /// <summary>
    /// Gets the processing strategy used by this custom consumer.
    /// </summary>
    /// <value>The strategy instance that defines custom processing behavior.</value>
    /// <remarks>
    /// Processing strategy provides:
    /// - Complete control over message handling and acknowledgment
    /// - Integration with external frameworks and systems
    /// - Implementation of specialized processing patterns
    /// - Extensibility for domain-specific requirements
    /// 
    /// Strategies can be swapped or updated to implement different
    /// processing behaviors within the same consumer infrastructure.
    /// </remarks>
    IMessageProcessingStrategy<T> ProcessingStrategy { get; }

    /// <summary>
    /// Updates the processing strategy used by this consumer.
    /// </summary>
    /// <param name="newStrategy">The new processing strategy to use. Cannot be null.</param>
    /// <param name="cancellationToken">Token to monitor for cancellation requests.</param>
    /// <returns>A task that represents the asynchronous strategy update operation.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="newStrategy"/> is null.</exception>
    /// <exception cref="ConsumerException">Thrown when the strategy cannot be updated due to consumer state issues.</exception>
    /// <remarks>
    /// Dynamic strategy updates enable:
    /// - Runtime switching between different processing approaches
    /// - A/B testing of processing strategies and algorithms
    /// - Response to changing requirements and system conditions
    /// - Implementation of adaptive processing based on external factors
    /// 
    /// Strategy changes take effect immediately for newly received messages
    /// and do not affect messages currently being processed.
    /// </remarks>
    Task UpdateProcessingStrategyAsync(
        IMessageProcessingStrategy<T> newStrategy, 
        CancellationToken cancellationToken = default);
}