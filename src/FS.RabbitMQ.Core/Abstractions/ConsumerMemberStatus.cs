namespace FS.RabbitMQ.Core.Abstractions;

/// <summary>
/// Defines the operational status of a consumer within a consumer group.
/// </summary>
/// <remarks>
/// Member status provides granular visibility into individual consumer participation
/// in group coordination and load balancing activities. Different statuses enable
/// appropriate handling of member lifecycle events and operational conditions.
/// </remarks>
public enum ConsumerMemberStatus
{
    /// <summary>
    /// The member is actively participating in message processing and group coordination.
    /// This is the normal operational status for healthy group members.
    /// </summary>
    /// <remarks>
    /// Active members:
    /// - Participate in load balancing and message distribution
    /// - Send regular heartbeats and status updates
    /// - Process messages according to their assigned load share
    /// - Respond to group coordination events and rebalancing requests
    /// </remarks>
    Active = 0,

    /// <summary>
    /// The member is in the process of joining the group and completing initialization.
    /// Temporary status during member onboarding and setup.
    /// </summary>
    /// <remarks>
    /// Joining members:
    /// - Are completing group registration and configuration
    /// - May trigger group rebalancing upon successful join
    /// - Are not yet participating in message processing
    /// - Will transition to Active status upon successful completion
    /// </remarks>
    Joining = 1,

    /// <summary>
    /// The member is in the process of leaving the group gracefully.
    /// Temporary status during planned member departure.
    /// </summary>
    /// <remarks>
    /// Leaving members:
    /// - Are completing in-flight message processing
    /// - May trigger group rebalancing as they depart
    /// - Are no longer accepting new message assignments
    /// - Will be removed from the group upon completion
    /// </remarks>
    Leaving = 2,

    /// <summary>
    /// The member has become unresponsive and is considered disconnected.
    /// Indicates potential network or consumer failure conditions.
    /// </summary>
    /// <remarks>
    /// Disconnected members:
    /// - Have stopped sending heartbeats or status updates
    /// - Are not processing their assigned message load
    /// - May trigger automatic group rebalancing
    /// - May be removed from the group after timeout periods
    /// 
    /// Disconnection can result from network issues, consumer failures,
    /// or host-level problems requiring investigation.
    /// </remarks>
    Disconnected = 3,

    /// <summary>
    /// The member is in an error state and requires intervention.
    /// Indicates that the member has encountered unrecoverable issues.
    /// </summary>
    /// <remarks>
    /// Faulted members:
    /// - Have encountered errors that prevent normal operation
    /// - Are not participating in message processing or group coordination
    /// - May require manual intervention or configuration changes
    /// - Will likely be excluded from load balancing until recovery
    /// 
    /// Common causes include configuration errors, resource exhaustion,
    /// or persistent connectivity issues.
    /// </remarks>
    Faulted = 4
}