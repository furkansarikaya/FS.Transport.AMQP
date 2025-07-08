namespace FS.RabbitMQ.Core.Abstractions;

/// <summary>
/// Defines a competitive consumer that participates in load balancing across multiple consumer instances.
/// </summary>
/// <typeparam name="T">The type of message payload being consumed.</typeparam>
/// <remarks>
/// Competitive consumers enable horizontal scaling by distributing message processing
/// across multiple consumer instances. Each message is delivered to exactly one consumer
/// within the consumer group, providing natural load balancing and fault isolation.
/// 
/// Key characteristics:
/// - Automatic load distribution across multiple consumer instances
/// - Dynamic rebalancing when consumers join or leave the group
/// - Fault tolerance with automatic failover and message redistribution
/// - Comprehensive monitoring and metrics for load distribution analysis
/// </remarks>
public interface ICompetitiveConsumer<T> : IMessageConsumer where T : class
{
    /// <summary>
    /// Gets the consumer group identifier for this competitive consumer.
    /// </summary>
    /// <value>The group identifier that coordinates load balancing with other consumer instances.</value>
    /// <remarks>
    /// Consumer group coordination enables:
    /// - Load distribution across multiple consumer instances
    /// - Automatic rebalancing when group membership changes
    /// - Fault isolation and recovery within the consumer group
    /// - Monitoring and metrics segmentation by consumer group
    /// 
    /// All consumers with the same group identifier participate in
    /// coordinated message distribution and load balancing.
    /// </remarks>
    string ConsumerGroup { get; }

    /// <summary>
    /// Gets information about the current consumer group membership and load distribution.
    /// </summary>
    /// <value>Details about group membership, load distribution, and consumer health.</value>
    /// <remarks>
    /// Group information provides visibility into:
    /// - Current members of the consumer group
    /// - Load distribution across group members
    /// - Health status of individual consumers
    /// - Rebalancing events and group membership changes
    /// 
    /// This information is valuable for monitoring group performance,
    /// detecting consumer failures, and optimizing load distribution.
    /// </remarks>
    ConsumerGroupInfo GroupInfo { get; }

    /// <summary>
    /// Forces a rebalancing of message distribution within the consumer group.
    /// </summary>
    /// <param name="cancellationToken">Token to monitor for cancellation requests.</param>
    /// <returns>A task that represents the asynchronous rebalancing operation.</returns>
    /// <exception cref="ConsumerException">Thrown when rebalancing fails due to group coordination or connectivity issues.</exception>
    /// <remarks>
    /// Manual rebalancing can be useful for:
    /// - Optimizing load distribution after consumer capacity changes
    /// - Testing failover and recovery mechanisms
    /// - Implementing custom load balancing strategies
    /// - Responding to external events that affect processing capacity
    /// 
    /// Rebalancing may temporarily disrupt message processing as consumers
    /// coordinate to establish new load distribution arrangements.
    /// </remarks>
    Task RebalanceAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Event raised when consumer group membership or load distribution changes.
    /// </summary>
    /// <remarks>
    /// Rebalancing events provide notifications about:
    /// - New consumers joining the group
    /// - Existing consumers leaving the group due to failure or shutdown
    /// - Load redistribution events and their outcomes
    /// - Group coordination events and status changes
    /// 
    /// Applications can use these events to implement group-aware logic
    /// and respond to changes in processing capacity and distribution.
    /// </remarks>
    event EventHandler<ConsumerGroupRebalancedEventArgs> GroupRebalanced;
}