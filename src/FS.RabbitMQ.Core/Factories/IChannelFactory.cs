using RabbitMQ.Client;

namespace FS.RabbitMQ.Core.Factories;

/// <summary>
/// Defines the contract for channel factories that create and manage RabbitMQ channels.
/// </summary>
/// <remarks>
/// Channel factories manage the creation, lifecycle, and pooling of RabbitMQ channels.
/// They ensure thread-safety, proper resource management, and optimal performance
/// through channel pooling and reuse strategies.
/// </remarks>
public interface IChannelFactory : IDisposable
{
    /// <summary>
    /// Creates a new channel from the specified connection.
    /// </summary>
    /// <param name="connection">The connection to create the channel from.</param>
    /// <returns>A new channel instance.</returns>
    /// <exception cref="ChannelException">Thrown when channel creation fails.</exception>
    Task<IChannel> CreateChannelAsync(IConnection connection);

    /// <summary>
    /// Creates a new channel with the specified configuration.
    /// </summary>
    /// <param name="connection">The connection to create the channel from.</param>
    /// <param name="configuration">The channel configuration.</param>
    /// <returns>A new channel instance.</returns>
    /// <exception cref="ChannelException">Thrown when channel creation fails.</exception>
    Task<IChannel> CreateChannelAsync(IConnection connection, ChannelConfiguration configuration);

    /// <summary>
    /// Gets or creates a pooled channel for the specified connection.
    /// </summary>
    /// <param name="connection">The connection to get a channel for.</param>
    /// <returns>A pooled channel instance.</returns>
    /// <exception cref="ChannelException">Thrown when channel retrieval fails.</exception>
    Task<IChannel> GetPooledChannelAsync(IConnection connection);

    /// <summary>
    /// Returns a channel to the pool for reuse.
    /// </summary>
    /// <param name="channel">The channel to return to the pool.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    Task ReturnChannelToPoolAsync(IChannel channel);

    /// <summary>
    /// Gets the current channel pool statistics.
    /// </summary>
    /// <returns>Channel pool statistics and health information.</returns>
    Task<ChannelPoolInfo> GetChannelPoolInfoAsync();

    /// <summary>
    /// Occurs when a channel is created.
    /// </summary>
    event EventHandler<ChannelCreatedEventArgs>? ChannelCreated;

    /// <summary>
    /// Occurs when a channel creation fails.
    /// </summary>
    event EventHandler<ChannelFailedEventArgs>? ChannelFailed;
}