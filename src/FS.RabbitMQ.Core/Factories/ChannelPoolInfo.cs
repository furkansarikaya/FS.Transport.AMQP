namespace FS.RabbitMQ.Core.Factories;

/// <summary>
/// Provides information about channel pool statistics and health.
/// </summary>
public sealed class ChannelPoolInfo
{
    /// <summary>
    /// Gets or sets the channel pool information for each connection.
    /// </summary>
    public IList<ConnectionChannelPoolInfo> ConnectionPools { get; set; } = new List<ConnectionChannelPoolInfo>();

    /// <summary>
    /// Gets or sets the total number of active channels across all pools.
    /// </summary>
    public int TotalActiveChannels { get; set; }

    /// <summary>
    /// Gets or sets the total number of pooled channels across all pools.
    /// </summary>
    public int TotalPooledChannels { get; set; }

    /// <summary>
    /// Gets or sets the total number of channels created across all pools.
    /// </summary>
    public long TotalChannelsCreated { get; set; }

    /// <summary>
    /// Gets or sets the time when this information was last updated.
    /// </summary>
    public DateTimeOffset LastUpdated { get; set; }
}