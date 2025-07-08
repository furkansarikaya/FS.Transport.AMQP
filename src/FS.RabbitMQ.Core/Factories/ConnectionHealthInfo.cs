namespace FS.RabbitMQ.Core.Factories;

/// <summary>
/// Provides information about connection health and statistics.
/// </summary>
public sealed class ConnectionHealthInfo
{
    /// <summary>
    /// Gets or sets the overall health status of connections.
    /// </summary>
    public ConnectionHealthStatus Status { get; set; }

    /// <summary>
    /// Gets or sets the number of active connections.
    /// </summary>
    public int ActiveConnections { get; set; }

    /// <summary>
    /// Gets or sets the number of pooled connections.
    /// </summary>
    public int PooledConnections { get; set; }

    /// <summary>
    /// Gets or sets the total number of connections created.
    /// </summary>
    public long TotalConnectionsCreated { get; set; }

    /// <summary>
    /// Gets or sets the time of the last health check.
    /// </summary>
    public DateTimeOffset LastHealthCheck { get; set; }

    /// <summary>
    /// Gets or sets additional health check details.
    /// </summary>
    public IDictionary<string, object>? Details { get; set; }
}