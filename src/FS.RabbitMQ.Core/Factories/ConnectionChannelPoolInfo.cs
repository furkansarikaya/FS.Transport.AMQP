namespace FS.RabbitMQ.Core.Factories;

/// <summary>
/// Provides information about a specific connection's channel pool.
/// </summary>
public sealed class ConnectionChannelPoolInfo
{
    /// <summary>
    /// Gets or sets the connection ID.
    /// </summary>
    public string ConnectionId { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the number of active channels.
    /// </summary>
    public int ActiveChannels { get; set; }

    /// <summary>
    /// Gets or sets the number of pooled channels.
    /// </summary>
    public int PooledChannels { get; set; }

    /// <summary>
    /// Gets or sets the total number of channels created for this connection.
    /// </summary>
    public long TotalChannelsCreated { get; set; }

    /// <summary>
    /// Gets or sets the pool utilization percentage.
    /// </summary>
    public double PoolUtilization { get; set; }
}