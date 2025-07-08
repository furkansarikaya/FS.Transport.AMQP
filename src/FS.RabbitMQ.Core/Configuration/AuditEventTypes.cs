namespace FS.RabbitMQ.Core.Configuration;

/// <summary>
/// Defines types of events that can be audited.
/// </summary>
[Flags]
public enum AuditEventTypes
{
    /// <summary>
    /// No events audited.
    /// </summary>
    None = 0,

    /// <summary>
    /// Authentication and authorization events.
    /// </summary>
    SecurityEvents = 1,

    /// <summary>
    /// Connection and disconnection events.
    /// </summary>
    ConnectionEvents = 2,

    /// <summary>
    /// Message publishing events.
    /// </summary>
    PublishEvents = 4,

    /// <summary>
    /// Message consumption events.
    /// </summary>
    ConsumeEvents = 8,

    /// <summary>
    /// Topology management events.
    /// </summary>
    TopologyEvents = 16,

    /// <summary>
    /// All events audited.
    /// </summary>
    All = SecurityEvents | ConnectionEvents | PublishEvents | ConsumeEvents | TopologyEvents
}