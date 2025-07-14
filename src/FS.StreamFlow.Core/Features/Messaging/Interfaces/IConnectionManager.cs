using FS.StreamFlow.Core.Features.Messaging.Models;

namespace FS.StreamFlow.Core.Features.Messaging.Interfaces;

/// <summary>
/// Interface for managing messaging connections with auto-recovery, pooling, and health monitoring
/// </summary>
public interface IConnectionManager : IDisposable
{
    /// <summary>
    /// Gets the current connection state
    /// </summary>
    ConnectionState State { get; }
    
    /// <summary>
    /// Gets connection statistics and metrics
    /// </summary>
    ConnectionStatistics Statistics { get; }
    
    /// <summary>
    /// Gets a value indicating whether the connection is available
    /// </summary>
    bool IsConnected { get; }
    
    /// <summary>
    /// Event raised when connection state changes
    /// </summary>
    event EventHandler<ConnectionStateChangedEventArgs>? StateChanged;
    
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
    /// Event raised when connection recovery completes
    /// </summary>
    event EventHandler<ConnectionEventArgs>? Recovered;
    
    /// <summary>
    /// Establishes connection to the messaging provider
    /// </summary>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Task representing the connection operation</returns>
    Task<bool> ConnectAsync(CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Disconnects from the messaging provider
    /// </summary>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Task representing the disconnection operation</returns>
    Task DisconnectAsync(CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Gets a channel for messaging operations
    /// </summary>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Channel for messaging operations</returns>
    Task<IChannel> GetChannelAsync(CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Returns a channel to the pool
    /// </summary>
    /// <param name="channel">Channel to return</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Task representing the return operation</returns>
    Task ReturnChannelAsync(IChannel channel, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Performs a health check on the connection
    /// </summary>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Health check result</returns>
    Task<HealthCheckResult> HealthCheckAsync(CancellationToken cancellationToken = default);
} 