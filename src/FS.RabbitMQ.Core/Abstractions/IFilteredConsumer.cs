namespace FS.RabbitMQ.Core.Abstractions;

/// <summary>
/// Defines a filtered consumer that only processes messages matching specific criteria.
/// </summary>
/// <typeparam name="T">The type of message payload being consumed.</typeparam>
/// <remarks>
/// Filtered consumers provide selective message processing based on runtime criteria,
/// enabling efficient content-based routing and conditional processing without requiring
/// complex broker-side routing configurations.
/// 
/// Key characteristics:
/// - Runtime message filtering based on content, headers, and metadata
/// - Automatic acknowledgment of non-matching messages
/// - Support for dynamic filter updates and A/B testing scenarios
/// - Comprehensive metrics for filter effectiveness and message distribution
/// </remarks>
public interface IFilteredConsumer<T> : IMessageConsumer where T : class
{
    /// <summary>
    /// Gets statistics about filter effectiveness and message processing patterns.
    /// </summary>
    /// <value>Current filter statistics for monitoring and optimization purposes.</value>
    /// <remarks>
    /// Filter statistics provide insights into:
    /// - Total messages evaluated by the filter
    /// - Number of messages accepted and rejected by filter criteria
    /// - Filter effectiveness and selectivity metrics
    /// - Performance impact of filtering operations
    /// 
    /// These statistics help optimize filter logic and understand
    /// message distribution patterns for capacity planning.
    /// </remarks>
    FilterStatistics FilterStatistics { get; }

    /// <summary>
    /// Updates the filter criteria used for message selection.
    /// </summary>
    /// <param name="newFilter">The new filter function to use for message selection. Cannot be null.</param>
    /// <param name="cancellationToken">Token to monitor for cancellation requests.</param>
    /// <returns>A task that represents the asynchronous filter update operation.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="newFilter"/> is null.</exception>
    /// <exception cref="ConsumerException">Thrown when the filter cannot be updated due to consumer state issues.</exception>
    /// <remarks>
    /// Dynamic filter updates enable:
    /// - Runtime adjustment of message selection criteria
    /// - A/B testing of different filtering strategies
    /// - Response to changing business requirements and conditions
    /// - Implementation of time-based or event-driven filtering logic
    /// 
    /// Filter updates take effect immediately for newly received messages
    /// and do not affect messages currently being processed.
    /// </remarks>
    Task UpdateFilterAsync(
        Func<MessageContext<T>, bool> newFilter, 
        CancellationToken cancellationToken = default);
}