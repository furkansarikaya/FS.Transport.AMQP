using RabbitMQ.Client;

namespace FS.RabbitMQ.Connection;

/// <summary>
/// Represents a pooled RabbitMQ channel with metadata
/// </summary>
/// <param name="channel">The RabbitMQ channel</param>
/// <param name="created">When the channel was created</param>
internal class PooledChannel(IChannel channel, DateTime created)
{
    /// <summary>
    /// Gets the underlying RabbitMQ channel
    /// </summary>
    public IChannel Channel { get; }

    /// <summary>
    /// Gets the unique identifier for this pooled channel
    /// </summary>
    public string Id { get; }

    /// <summary>
    /// Gets the time when this channel was created
    /// </summary>
    public DateTimeOffset CreatedAt { get; }

    /// <summary>
    /// Gets or sets the time when this channel was last used
    /// </summary>
    public DateTimeOffset LastUsed { get; set; }

    /// <summary>
    /// Gets whether this channel has expired based on the configured TTL
    /// </summary>
    public bool IsExpired => DateTimeOffset.UtcNow - LastUsed > TimeSpan.FromMinutes(30);

    /// <summary>
    /// Gets whether the underlying channel is open and usable
    /// </summary>
    public bool IsOpen => Channel?.IsOpen == true;
}