namespace FS.RabbitMQ.Core.Factories;

/// <summary>
/// Defines the health status of connections.
/// </summary>
public enum ConnectionHealthStatus
{
    /// <summary>
    /// Connection health is unknown.
    /// </summary>
    Unknown,

    /// <summary>
    /// Connections are healthy and functioning properly.
    /// </summary>
    Healthy,

    /// <summary>
    /// Connections are experiencing issues but are still functional.
    /// </summary>
    Degraded,

    /// <summary>
    /// Connections are unhealthy and may not be functioning properly.
    /// </summary>
    Unhealthy,

    /// <summary>
    /// Connections are completely unavailable.
    /// </summary>
    Unavailable
}