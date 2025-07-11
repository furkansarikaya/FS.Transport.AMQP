using RabbitMQ.Client;

namespace FS.RabbitMQ.Connection;

/// <summary>
/// Interface for managing RabbitMQ connections and channels
/// </summary>
public interface IConnectionManager : IDisposable
{
    /// <summary>
    /// Gets whether the connection is currently connected
    /// </summary>
    bool IsConnected { get; }

    /// <summary>
    /// Gets the current connection state
    /// </summary>
    ConnectionState State { get; }

    /// <summary>
    /// Gets connection statistics
    /// </summary>
    ConnectionStatistics Statistics { get; }

    /// <summary>
    /// Event raised when the connection is established
    /// </summary>
    event EventHandler<ConnectionEventArgs>? Connected;

    /// <summary>
    /// Event raised when the connection is lost
    /// </summary>
    event EventHandler<ConnectionEventArgs>? Disconnected;

    /// <summary>
    /// Event raised when the connection is recovering
    /// </summary>
    event EventHandler<ConnectionEventArgs>? Recovering;

    /// <summary>
    /// Event raised when the connection recovery is complete
    /// </summary>
    event EventHandler<ConnectionEventArgs>? RecoveryComplete;

    /// <summary>
    /// Event raised when the connection recovery fails
    /// </summary>
    event EventHandler<ConnectionEventArgs>? RecoveryFailed;

    /// <summary>
    /// Gets a channel from the pool
    /// </summary>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>A channel from the pool</returns>
    Task<IChannel> GetChannelAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Returns a channel to the pool
    /// </summary>
    /// <param name="channel">The channel to return</param>
    void ReturnChannel(IChannel channel);

    /// <summary>
    /// Creates a new channel (not from pool)
    /// </summary>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>A new channel</returns>
    Task<IChannel> CreateChannelAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Connects to RabbitMQ
    /// </summary>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Task representing the connection operation</returns>
    Task ConnectAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Disconnects from RabbitMQ
    /// </summary>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Task representing the disconnection operation</returns>
    Task DisconnectAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Performs health check on the connection
    /// </summary>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Task representing the health check operation</returns>
    Task<bool> HealthCheckAsync(CancellationToken cancellationToken = default);
}