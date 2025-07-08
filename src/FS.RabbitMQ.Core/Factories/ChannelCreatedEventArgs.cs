using RabbitMQ.Client;

namespace FS.RabbitMQ.Core.Factories;

/// <summary>
/// Event arguments for channel creation events.
/// </summary>
public sealed class ChannelCreatedEventArgs : EventArgs
{
    /// <summary>
    /// Initializes a new instance of the <see cref="ChannelCreatedEventArgs"/> class.
    /// </summary>
    /// <param name="channel">The created channel.</param>
    /// <param name="connectionId">The ID of the connection.</param>
    public ChannelCreatedEventArgs(IChannel channel, string connectionId)
    {
        Channel = channel ?? throw new ArgumentNullException(nameof(channel));
        ConnectionId = connectionId ?? throw new ArgumentNullException(nameof(connectionId));
        CreatedAt = DateTimeOffset.UtcNow;
    }

    /// <summary>
    /// Gets the created channel.
    /// </summary>
    public IChannel Channel { get; }

    /// <summary>
    /// Gets the ID of the connection.
    /// </summary>
    public string ConnectionId { get; }

    /// <summary>
    /// Gets the time when the channel was created.
    /// </summary>
    public DateTimeOffset CreatedAt { get; }
}