using RabbitMQ.Client;

namespace FS.Transport.AMQP.Connection;

/// <summary>
/// Interface for managing RabbitMQ connections with automatic reconnection, pooling, and health monitoring
/// </summary>
public interface IConnectionManager : IDisposable
{
    /// <summary>
    /// Gets the current connection status
    /// </summary>
    bool IsConnected { get; }
    
    /// <summary>
    /// Gets the current connection state
    /// </summary>
    ConnectionState State { get; }
    
    /// <summary>
    /// Gets connection statistics and metrics
    /// </summary>
    ConnectionStatistics Statistics { get; }
    
    /// <summary>
    /// Establishes connection to RabbitMQ server with retry logic and auto-recovery
    /// </summary>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>True if connection was established successfully</returns>
    Task<bool> ConnectAsync(CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Disconnects from RabbitMQ server gracefully, closing all channels and connections
    /// </summary>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Task representing the disconnection operation</returns>
    Task DisconnectAsync(CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Gets a channel from the connection pool with automatic recovery
    /// </summary>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>RabbitMQ channel</returns>
    Task<IModel> GetChannelAsync(CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Returns a channel to the pool for reuse
    /// </summary>
    /// <param name="channel">Channel to return</param>
    void ReturnChannel(IModel channel);
    
    /// <summary>
    /// Creates a new dedicated channel (not pooled)
    /// </summary>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Dedicated RabbitMQ channel</returns>
    Task<IModel> CreateChannelAsync(CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Tests the connection health by performing a lightweight operation
    /// </summary>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>True if connection is healthy</returns>
    Task<bool> TestConnectionAsync(CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Forces a connection recovery attempt
    /// </summary>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>True if recovery was successful</returns>
    Task<bool> RecoverAsync(CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Event raised when connection is established
    /// </summary>
    event EventHandler<ConnectionEventArgs>? Connected;
    
    /// <summary>
    /// Event raised when connection is lost
    /// </summary>
    event EventHandler<ConnectionEventArgs>? Disconnected;
    
    /// <summary>
    /// Event raised when connection recovery starts
    /// </summary>
    event EventHandler<ConnectionEventArgs>? Recovering;
    
    /// <summary>
    /// Event raised when connection recovery completes successfully
    /// </summary>
    event EventHandler<ConnectionEventArgs>? Recovered;
    
    /// <summary>
    /// Event raised when connection recovery fails
    /// </summary>
    event EventHandler<ConnectionEventArgs>? RecoveryFailed;
}