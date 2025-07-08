namespace FS.RabbitMQ.Core.Abstractions;

/// <summary>
/// Contains information about a consumer group's membership, load distribution, and operational status.
/// </summary>
/// <remarks>
/// Consumer group information provides visibility into the coordination and load balancing
/// mechanisms that enable horizontal scaling through competitive consumers. This information
/// is essential for monitoring group health, debugging distribution issues, and optimizing
/// load balancing performance.
/// </remarks>
public sealed record ConsumerGroupInfo
{
    /// <summary>
    /// Gets the consumer group identifier.
    /// </summary>
    /// <value>The unique identifier that coordinates load balancing across group members.</value>
    public required string GroupId { get; init; }

    /// <summary>
    /// Gets the total number of consumers in the group.
    /// </summary>
    /// <value>The current count of active consumers participating in the group.</value>
    public int MemberCount { get; init; }

    /// <summary>
    /// Gets information about individual group members.
    /// </summary>
    /// <value>A collection containing details about each consumer in the group.</value>
    public required IReadOnlyList<ConsumerGroupMember> Members { get; init; }

    /// <summary>
    /// Gets the current load balancing strategy used by the group.
    /// </summary>
    /// <value>The strategy name or identifier describing how messages are distributed among members.</value>
    public string LoadBalancingStrategy { get; init; } = "RoundRobin";

    /// <summary>
    /// Gets the timestamp of the last group rebalancing operation.
    /// </summary>
    /// <value>The UTC timestamp when the group last performed a rebalancing operation, or null if no rebalancing has occurred.</value>
    public DateTimeOffset? LastRebalanced { get; init; }

    /// <summary>
    /// Gets the reason for the last rebalancing operation.
    /// </summary>
    /// <value>A description of what triggered the last rebalancing, or null if no rebalancing has occurred.</value>
    public string? LastRebalanceReason { get; init; }

    /// <summary>
    /// Gets the current health status of the consumer group.
    /// </summary>
    /// <value>The overall health assessment of the group based on member status and coordination effectiveness.</value>
    public GroupHealth Health { get; init; }

    /// <summary>
    /// Gets performance metrics for the consumer group.
    /// </summary>
    /// <value>Aggregate performance statistics across all group members.</value>
    public ConsumerGroupMetrics Metrics { get; init; } = ConsumerGroupMetrics.Empty;

    /// <summary>
    /// Gets additional metadata about the consumer group.
    /// </summary>
    /// <value>A dictionary containing supplementary group information, or null if no additional data is available.</value>
    public IReadOnlyDictionary<string, object>? Metadata { get; init; }

    /// <summary>
    /// Gets a value indicating whether the consumer group is currently stable.
    /// </summary>
    /// <value><c>true</c> if the group is stable and not undergoing rebalancing; otherwise, <c>false</c>.</value>
    /// <remarks>
    /// Group stability indicates:
    /// - No rebalancing operations are in progress
    /// - All members are healthy and actively participating
    /// - Message distribution is operating normally
    /// - No recent membership changes have occurred
    /// 
    /// Stability is important for understanding whether performance metrics
    /// and load distribution represent steady-state operation.
    /// </remarks>
    public bool IsStable => Health == GroupHealth.Healthy && Members.All(m => m.Status == ConsumerMemberStatus.Active);

    /// <summary>
    /// Gets the active member count excluding any members in error or disconnected states.
    /// </summary>
    /// <value>The number of members that are currently active and participating in message processing.</value>
    public int ActiveMemberCount => Members.Count(m => m.Status == ConsumerMemberStatus.Active);

    /// <summary>
    /// Creates a new ConsumerGroupInfo instance with the specified parameters.
    /// </summary>
    /// <param name="groupId">The consumer group identifier.</param>
    /// <param name="members">The collection of group members.</param>
    /// <param name="loadBalancingStrategy">The load balancing strategy in use.</param>
    /// <param name="lastRebalanced">The timestamp of the last rebalancing operation.</param>
    /// <param name="lastRebalanceReason">The reason for the last rebalancing.</param>
    /// <param name="health">The current health status of the group.</param>
    /// <param name="metrics">Performance metrics for the group.</param>
    /// <param name="metadata">Additional metadata about the group.</param>
    /// <returns>A new ConsumerGroupInfo instance.</returns>
    /// <exception cref="ArgumentException">Thrown when <paramref name="groupId"/> is null, empty, or whitespace.</exception>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="members"/> is null.</exception>
    public static ConsumerGroupInfo Create(
        string groupId,
        IReadOnlyList<ConsumerGroupMember> members,
        string loadBalancingStrategy = "RoundRobin",
        DateTimeOffset? lastRebalanced = null,
        string? lastRebalanceReason = null,
        GroupHealth health = GroupHealth.Healthy,
        ConsumerGroupMetrics? metrics = null,
        IReadOnlyDictionary<string, object>? metadata = null)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(groupId);
        ArgumentNullException.ThrowIfNull(members);

        return new ConsumerGroupInfo
        {
            GroupId = groupId,
            MemberCount = members.Count,
            Members = members,
            LoadBalancingStrategy = loadBalancingStrategy,
            LastRebalanced = lastRebalanced,
            LastRebalanceReason = lastRebalanceReason,
            Health = health,
            Metrics = metrics ?? ConsumerGroupMetrics.Empty,
            Metadata = metadata
        };
    }

    /// <summary>
    /// Returns a string representation of the consumer group information.
    /// </summary>
    /// <returns>A formatted string describing the group status and membership.</returns>
    public override string ToString()
    {
        var healthInfo = Health != GroupHealth.Healthy ? $" ({Health})" : "";
        var lastRebalanceInfo = LastRebalanced.HasValue ? $", last rebalanced {LastRebalanced:yyyy-MM-dd HH:mm:ss} UTC" : "";
        return $"Group '{GroupId}': {ActiveMemberCount}/{MemberCount} active members{healthInfo}{lastRebalanceInfo}";
    }
}