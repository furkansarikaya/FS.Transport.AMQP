namespace FS.RabbitMQ.Core.Abstractions;

/// <summary>
/// Defines the overall health status of a consumer group.
/// </summary>
/// <remarks>
/// Group health provides a high-level assessment of the consumer group's
/// operational effectiveness and ability to process messages reliably.
/// Health status helps identify when intervention may be needed to maintain
/// proper group operation and message processing capabilities.
/// </remarks>
public enum GroupHealth
{
    /// <summary>
    /// The group is operating normally with all members healthy and load properly distributed.
    /// This is the desired operational state for consumer groups.
    /// </summary>
    /// <remarks>
    /// Healthy groups have:
    /// - All or most members in active status
    /// - Balanced load distribution across members
    /// - No recent rebalancing issues or failures
    /// - Good performance metrics across all members
    /// </remarks>
    Healthy = 0,

    /// <summary>
    /// The group is experiencing minor issues but continues to process messages.
    /// Some degradation in performance or member availability may be present.
    /// </summary>
    /// <remarks>
    /// Degraded groups may have:
    /// - Some members in non-active states
    /// - Uneven load distribution requiring rebalancing
    /// - Recent member failures with ongoing recovery
    /// - Performance metrics below optimal levels
    /// </remarks>
    Degraded = 1,

    /// <summary>
    /// The group has significant issues affecting message processing capabilities.
    /// Immediate attention may be required to restore proper operation.
    /// </summary>
    /// <remarks>
    /// Unhealthy groups typically have:
    /// - Multiple members in error or disconnected states
    /// - Severe load imbalances or processing bottlenecks
    /// - Recent rebalancing failures or coordination issues
    /// - Poor performance metrics indicating systemic problems
    /// </remarks>
    Unhealthy = 2,

    /// <summary>
    /// The group is non-functional and unable to process messages effectively.
    /// Critical intervention is required to restore service.
    /// </summary>
    /// <remarks>
    /// Critical groups have:
    /// - Majority of members in error or disconnected states
    /// - Complete failure of load balancing or coordination
    /// - Unable to process messages or maintain group membership
    /// - Systemic failures requiring immediate resolution
    /// </remarks>
    Critical = 3
}