namespace FS.RabbitMQ.Core.Abstractions;

/// <summary>
/// Provides data for the ConsumerGroupRebalanced event.
/// </summary>
/// <remarks>
/// Group rebalancing events provide detailed information about load distribution
/// changes, member status updates, and coordination activities within consumer groups.
/// These events are essential for monitoring group health and understanding
/// the impact of membership changes on message processing.
/// </remarks>
public sealed class ConsumerGroupRebalancedEventArgs : EventArgs
{
    /// <summary>
    /// Gets the consumer group identifier for the rebalancing event.
    /// </summary>
    /// <value>The unique identifier of the group that underwent rebalancing.</value>
    public string GroupId { get; }

    /// <summary>
    /// Gets the reason that triggered the rebalancing operation.
    /// </summary>
    /// <value>A description of what caused the group to rebalance.</value>
    public string Reason { get; }

    /// <summary>
    /// Gets the group membership before the rebalancing operation.
    /// </summary>
    /// <value>The list of group members and their status before rebalancing.</value>
    public IReadOnlyList<ConsumerGroupMember> PreviousMembers { get; }

    /// <summary>
    /// Gets the group membership after the rebalancing operation.
    /// </summary>
    /// <value>The list of group members and their status after rebalancing.</value>
    public IReadOnlyList<ConsumerGroupMember> CurrentMembers { get; }

    /// <summary>
    /// Gets the timestamp when the rebalancing operation started.
    /// </summary>
    /// <value>The UTC timestamp when rebalancing began.</value>
    public DateTimeOffset StartTime { get; }

    /// <summary>
    /// Gets the timestamp when the rebalancing operation completed.
    /// </summary>
    /// <value>The UTC timestamp when rebalancing finished.</value>
    public DateTimeOffset CompletionTime { get; }

    /// <summary>
    /// Gets a value indicating whether the rebalancing operation completed successfully.
    /// </summary>
    /// <value><c>true</c> if rebalancing completed without errors; otherwise, <c>false</c>.</value>
    public bool IsSuccessful { get; }

    /// <summary>
    /// Gets error information if the rebalancing operation failed.
    /// </summary>
    /// <value>Details about rebalancing failures, or null if the operation was successful.</value>
    public string? ErrorMessage { get; }

    /// <summary>
    /// Initializes a new instance of the ConsumerGroupRebalancedEventArgs class.
    /// </summary>
    /// <param name="groupId">The consumer group identifier.</param>
    /// <param name="reason">The reason for the rebalancing operation.</param>
    /// <param name="previousMembers">The group membership before rebalancing.</param>
    /// <param name="currentMembers">The group membership after rebalancing.</param>
    /// <param name="startTime">The timestamp when rebalancing started.</param>
    /// <param name="completionTime">The timestamp when rebalancing completed.</param>
    /// <param name="isSuccessful">Whether the rebalancing was successful.</param>
    /// <param name="errorMessage">Error details if rebalancing failed.</param>
    /// <exception cref="ArgumentException">Thrown when <paramref name="groupId"/> or <paramref name="reason"/> is null, empty, or whitespace.</exception>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="previousMembers"/> or <paramref name="currentMembers"/> is null.</exception>
    public ConsumerGroupRebalancedEventArgs(
        string groupId,
        string reason,
        IReadOnlyList<ConsumerGroupMember> previousMembers,
        IReadOnlyList<ConsumerGroupMember> currentMembers,
        DateTimeOffset startTime,
        DateTimeOffset completionTime,
        bool isSuccessful,
        string? errorMessage = null)
    {
        GroupId = !string.IsNullOrWhiteSpace(groupId) 
            ? groupId 
            : throw new ArgumentException("Group ID cannot be null or empty.", nameof(groupId));
        Reason = !string.IsNullOrWhiteSpace(reason) 
            ? reason 
            : throw new ArgumentException("Reason cannot be null or empty.", nameof(reason));
        PreviousMembers = previousMembers ?? throw new ArgumentNullException(nameof(previousMembers));
        CurrentMembers = currentMembers ?? throw new ArgumentNullException(nameof(currentMembers));
        StartTime = startTime;
        CompletionTime = completionTime;
        IsSuccessful = isSuccessful;
        ErrorMessage = errorMessage;
    }

    /// <summary>
    /// Gets the duration of the rebalancing operation.
    /// </summary>
    /// <value>The time elapsed from start to completion of the rebalancing operation.</value>
    public TimeSpan Duration => CompletionTime - StartTime;

    /// <summary>
    /// Gets the change in group membership count.
    /// </summary>
    /// <value>The difference between current and previous member counts (positive for growth, negative for shrinkage).</value>
    public int MembershipChange => CurrentMembers.Count - PreviousMembers.Count;

    /// <summary>
    /// Gets the members that joined the group during this rebalancing.
    /// </summary>
    /// <value>A collection of members present in the current membership but not in the previous membership.</value>
    public IEnumerable<ConsumerGroupMember> NewMembers =>
        CurrentMembers.Where(current => !PreviousMembers.Any(previous => previous.ConsumerId == current.ConsumerId));

    /// <summary>
    /// Gets the members that left the group during this rebalancing.
    /// </summary>
    /// <value>A collection of members present in the previous membership but not in the current membership.</value>
    public IEnumerable<ConsumerGroupMember> RemovedMembers =>
        PreviousMembers.Where(previous => !CurrentMembers.Any(current => current.ConsumerId == previous.ConsumerId));

    /// <summary>
    /// Returns a string representation of the consumer group rebalancing event.
    /// </summary>
    /// <returns>A formatted string describing the rebalancing operation and its outcome.</returns>
    public override string ToString()
    {
        var outcome = IsSuccessful ? "successful" : $"failed ({ErrorMessage})";
        var membershipInfo = MembershipChange != 0 ? $" ({MembershipChange:+#;-#;0} members)" : "";
        return $"Group '{GroupId}' rebalanced: {Reason}, {outcome} in {Duration.TotalMilliseconds:F0}ms{membershipInfo}";
    }
}