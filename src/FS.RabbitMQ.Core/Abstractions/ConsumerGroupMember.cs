namespace FS.RabbitMQ.Core.Abstractions;

/// <summary>
/// Contains information about an individual consumer within a consumer group.
/// </summary>
/// <remarks>
/// Consumer group member information provides detailed visibility into individual
/// consumer status, performance, and participation in group coordination activities.
/// This information is essential for debugging load distribution issues and
/// monitoring individual consumer health within the group context.
/// </remarks>
public sealed record ConsumerGroupMember
{
    /// <summary>
    /// Gets the unique identifier of the consumer.
    /// </summary>
    /// <value>The consumer ID that uniquely identifies this member within the group.</value>
    public required string ConsumerId { get; init; }

    /// <summary>
    /// Gets the current status of the consumer member.
    /// </summary>
    /// <value>The operational status indicating the member's participation level in the group.</value>
    public ConsumerMemberStatus Status { get; init; }

    /// <summary>
    /// Gets the timestamp when the member joined the group.
    /// </summary>
    /// <value>The UTC timestamp when this consumer became a member of the group.</value>
    public DateTimeOffset JoinedAt { get; init; }

    /// <summary>
    /// Gets the timestamp of the member's last activity or heartbeat.
    /// </summary>
    /// <value>The UTC timestamp of the most recent activity from this member.</value>
    public DateTimeOffset LastActivity { get; init; }

    /// <summary>
    /// Gets the current load share assigned to this member.
    /// </summary>
    /// <value>The percentage of total group load currently assigned to this member (0.0 to 100.0).</value>
    /// <remarks>
    /// Load share indicates the portion of messages this member is expected to process:
    /// - Equal distribution would show approximately 100/memberCount for each member
    /// - Unequal distribution may occur due to member capacity differences or rebalancing
    /// - Values summing to 100% across all active members indicate healthy distribution
    /// - Significant deviations may indicate rebalancing needs or member issues
    /// </remarks>
    public double LoadSharePercentage { get; init; }

    /// <summary>
    /// Gets performance statistics for this group member.
    /// </summary>
    /// <value>Individual performance metrics for monitoring and analysis.</value>
    public ConsumerStatistics Statistics { get; init; } = ConsumerStatistics.Create(0, 0, 0, TimeSpan.Zero, DateTimeOffset.UtcNow);

    /// <summary>
    /// Gets additional metadata about the group member.
    /// </summary>
    /// <value>A dictionary containing supplementary member information, or null if no additional data is available.</value>
    /// <remarks>
    /// Member metadata may include:
    /// - Host and deployment information
    /// - Consumer configuration details
    /// - Resource utilization metrics
    /// - Application-specific identifiers
    /// 
    /// This information is valuable for capacity planning, debugging,
    /// and understanding the deployment characteristics of group members.
    /// </remarks>
    public IReadOnlyDictionary<string, object>? Metadata { get; init; }

    /// <summary>
    /// Gets the duration the member has been part of the group.
    /// </summary>
    /// <value>The time elapsed since the member joined the group.</value>
    public TimeSpan MembershipDuration => DateTimeOffset.UtcNow - JoinedAt;

    /// <summary>
    /// Gets the duration since the member's last activity.
    /// </summary>
    /// <value>The time elapsed since the last recorded activity from this member.</value>
    public TimeSpan TimeSinceLastActivity => DateTimeOffset.UtcNow - LastActivity;

    /// <summary>
    /// Gets a value indicating whether the member appears to be healthy and responsive.
    /// </summary>
    /// <value><c>true</c> if the member is active and has recent activity; otherwise, <c>false</c>.</value>
    /// <remarks>
    /// Member health assessment considers:
    /// - Current operational status
    /// - Recency of last activity or heartbeat
    /// - Performance metrics and error rates
    /// - Participation in group coordination activities
    /// 
    /// Unhealthy members may require investigation or intervention
    /// to restore proper group operation and load distribution.
    /// </remarks>
    public bool IsHealthy => Status == ConsumerMemberStatus.Active && TimeSinceLastActivity < TimeSpan.FromMinutes(2);

    /// <summary>
    /// Returns a string representation of the consumer group member.
    /// </summary>
    /// <returns>A formatted string describing the member status and performance.</returns>
    public override string ToString()
    {
        var healthStatus = IsHealthy ? "healthy" : "unhealthy";
        return $"Member {ConsumerId}: {Status}, {LoadSharePercentage:F1}% load, {healthStatus}, " +
               $"joined {MembershipDuration.TotalMinutes:F0}m ago";
    }
}