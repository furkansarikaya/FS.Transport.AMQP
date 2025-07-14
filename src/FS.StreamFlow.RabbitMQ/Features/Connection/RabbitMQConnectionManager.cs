using FS.StreamFlow.Core.Features.Messaging.Interfaces;
using FS.StreamFlow.Core.Features.Messaging.Models;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System.Collections.Concurrent;
using CoreChannel = FS.StreamFlow.Core.Features.Messaging.Models.IChannel;

namespace FS.StreamFlow.RabbitMQ.Features.Connection;

/// <summary>
/// RabbitMQ implementation of the connection manager interface.
/// Manages RabbitMQ connections with automatic recovery, health monitoring, and connection pooling.
/// </summary>
public class RabbitMQConnectionManager : IConnectionManager
{
    private readonly ConnectionSettings _settings;
    private readonly ILogger<RabbitMQConnectionManager> _logger;
    private readonly ConcurrentQueue<CoreChannel> _channelPool;
    private readonly SemaphoreSlim _connectionSemaphore;
    private readonly Timer? _healthCheckTimer;
    private readonly CancellationTokenSource _cancellationTokenSource;

    private volatile ConnectionState _state = ConnectionState.NotInitialized;
    private volatile bool _disposed;
    private IConnection? _primaryConnection;
    private readonly ConnectionStatistics _statistics;

    /// <summary>
    /// Gets the current connection state.
    /// </summary>
    public ConnectionState State => _state;

    /// <summary>
    /// Gets a value indicating whether the connection is established.
    /// </summary>
    public bool IsConnected => _state == ConnectionState.Connected;

    /// <summary>
    /// Gets connection statistics including connection count, error count, and uptime.
    /// </summary>
    public ConnectionStatistics Statistics => _statistics;

    /// <summary>
    /// Event raised when connection is established.
    /// </summary>
    public event EventHandler<ConnectionEventArgs>? Connected;

    /// <summary>
    /// Event raised when connection is lost.
    /// </summary>
    public event EventHandler<ConnectionEventArgs>? Disconnected;

    /// <summary>
    /// Event raised when connection recovery starts.
    /// </summary>
    public event EventHandler<ConnectionEventArgs>? Recovering;

    /// <summary>
    /// Event raised when connection is recovered.
    /// </summary>
    public event EventHandler<ConnectionEventArgs>? Recovered;

    /// <summary>
    /// Event raised when connection state changes.
    /// </summary>
    public event EventHandler<ConnectionStateChangedEventArgs>? StateChanged;

    /// <summary>
    /// Initializes a new instance of the <see cref="RabbitMQConnectionManager"/> class.
    /// </summary>
    /// <param name="settings">The connection settings.</param>
    /// <param name="logger">The logger instance.</param>
    public RabbitMQConnectionManager(
        IOptions<ConnectionSettings> settings,
        ILogger<RabbitMQConnectionManager> logger)
    {
        _settings = settings.Value ?? throw new ArgumentNullException(nameof(settings));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _channelPool = new ConcurrentQueue<CoreChannel>();
        _connectionSemaphore = new SemaphoreSlim(1, 1);
        _cancellationTokenSource = new CancellationTokenSource();
        _statistics = new ConnectionStatistics
        {
            CurrentState = ConnectionState.NotInitialized,
            Timestamp = DateTimeOffset.UtcNow
        };
    }

    /// <summary>
    /// Connects to the RabbitMQ server asynchronously.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task representing the asynchronous operation with success status.</returns>
    public async Task<bool> ConnectAsync(CancellationToken cancellationToken = default)
    {
        if (_disposed)
            throw new ObjectDisposedException(nameof(RabbitMQConnectionManager));

        await _connectionSemaphore.WaitAsync(cancellationToken);
        try
        {
            if (_state == ConnectionState.Connected)
                return true;

            ChangeState(ConnectionState.Connecting);
            _logger.LogInformation("Connecting to RabbitMQ server at {Host}:{Port}", 
                _settings.Host, _settings.Port);

            var factory = CreateConnectionFactory();
            _primaryConnection = await factory.CreateConnectionAsync(cancellationToken);

            // Setup connection event handlers
            _primaryConnection.ConnectionShutdownAsync += OnConnectionShutdownAsync;
            _primaryConnection.RecoverySucceededAsync += OnRecoverySucceededAsync;
            _primaryConnection.ConnectionRecoveryErrorAsync += OnConnectionRecoveryErrorAsync;

            _statistics.LastConnectedAt = DateTimeOffset.UtcNow;
            _statistics.SuccessfulConnections++;
            _statistics.TotalConnectionAttempts++;
            
            ChangeState(ConnectionState.Connected);
            Connected?.Invoke(this, new ConnectionEventArgs(ConnectionState.Connected, "primary"));
            
            _logger.LogInformation("Successfully connected to RabbitMQ server");
            return true;
        }
        catch (Exception ex)
        {
            _statistics.FailedConnections++;
            _statistics.TotalConnectionAttempts++;
            ChangeState(ConnectionState.Error);
            _logger.LogError(ex, "Failed to connect to RabbitMQ server");
            return false;
        }
        finally
        {
            _connectionSemaphore.Release();
        }
    }

    /// <summary>
    /// Disconnects from the RabbitMQ server asynchronously.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    public async Task DisconnectAsync(CancellationToken cancellationToken = default)
    {
        if (_disposed || _state == ConnectionState.Disconnected)
            return;

        await _connectionSemaphore.WaitAsync(cancellationToken);
        try
        {
            ChangeState(ConnectionState.Disconnected);
            _logger.LogInformation("Disconnecting from RabbitMQ server");

            if (_primaryConnection != null)
            {
                _primaryConnection.ConnectionShutdownAsync -= OnConnectionShutdownAsync;
                _primaryConnection.RecoverySucceededAsync -= OnRecoverySucceededAsync;
                _primaryConnection.ConnectionRecoveryErrorAsync -= OnConnectionRecoveryErrorAsync;
                
                await _primaryConnection.CloseAsync(cancellationToken);
                _primaryConnection = null;
            }

            _statistics.LastDisconnectedAt = DateTimeOffset.UtcNow;
            _statistics.TotalDisconnections++;
            Disconnected?.Invoke(this, new ConnectionEventArgs(ConnectionState.Disconnected, "primary"));
            
            _logger.LogInformation("Successfully disconnected from RabbitMQ server");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during disconnection");
            throw;
        }
        finally
        {
            _connectionSemaphore.Release();
        }
    }

    /// <summary>
    /// Performs a health check on the connection asynchronously.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task representing the asynchronous operation with health check result.</returns>
    public async Task<HealthCheckResult> HealthCheckAsync(CancellationToken cancellationToken = default)
    {
        if (_disposed)
            return HealthCheckResult.Unhealthy("Connection manager is disposed");

        try
        {
            if (_primaryConnection == null || !_primaryConnection.IsOpen)
                return HealthCheckResult.Unhealthy("Connection is not established");

            // Try to create a temporary channel to verify connection health
            using var channel = await GetChannelAsync(cancellationToken);
            return HealthCheckResult.Healthy("Connection is healthy");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Health check failed");
            return HealthCheckResult.Unhealthy("Health check failed", ex);
        }
    }

    /// <summary>
    /// Gets a channel for messaging operations.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task representing the asynchronous operation with a channel.</returns>
    public async Task<CoreChannel> GetChannelAsync(CancellationToken cancellationToken = default)
    {
        if (_disposed)
            throw new ObjectDisposedException(nameof(RabbitMQConnectionManager));

        if (_primaryConnection == null || !_primaryConnection.IsOpen)
            throw new InvalidOperationException("Connection is not established");

        // Try to get a channel from the pool first
        if (_channelPool.TryDequeue(out var pooledChannel) && pooledChannel.IsOpen)
        {
            return pooledChannel;
        }

        try
        {
            var channel = await _primaryConnection.CreateChannelAsync(cancellationToken: cancellationToken);
            _statistics.TotalChannelsCreated++;
            _statistics.ActiveChannels++;
            return new RabbitMQChannel(channel, _logger);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to create channel");
            throw;
        }
    }

    /// <summary>
    /// Returns a channel to the pool for reuse.
    /// </summary>
    /// <param name="channel">The channel to return.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    public async Task ReturnChannelAsync(CoreChannel channel, CancellationToken cancellationToken = default)
    {
        if (channel == null || !channel.IsOpen)
            return;

        try
        {
            _channelPool.Enqueue(channel);
            _statistics.ActiveChannels--;
            await Task.CompletedTask;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to return channel to pool");
            channel.Dispose();
        }
    }

    /// <summary>
    /// Releases all resources used by the connection manager.
    /// </summary>
    public void Dispose()
    {
        if (_disposed)
            return;

        _disposed = true;
        
        try
        {
            _cancellationTokenSource.Cancel();
            _healthCheckTimer?.Dispose();
            
            DisconnectAsync().GetAwaiter().GetResult();
            
            // Clear channel pool
            while (_channelPool.TryDequeue(out var channel))
            {
                channel?.Dispose();
            }
            
            _connectionSemaphore?.Dispose();
            _cancellationTokenSource?.Dispose();
            
            _logger.LogInformation("RabbitMQ connection manager disposed");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during disposal");
        }
    }

    private ConnectionFactory CreateConnectionFactory()
    {
        return new ConnectionFactory
        {
            HostName = _settings.Host,
            Port = _settings.Port,
            UserName = _settings.Username,
            Password = _settings.Password,
            VirtualHost = _settings.VirtualHost,
            RequestedConnectionTimeout = _settings.ConnectionTimeout,
            RequestedHeartbeat = TimeSpan.FromSeconds(60),
            AutomaticRecoveryEnabled = true,
            NetworkRecoveryInterval = TimeSpan.FromSeconds(10)
        };
    }

    private void ChangeState(ConnectionState newState)
    {
        var previousState = _state;
        _state = newState;
        _statistics.CurrentState = newState;
        
        if (previousState != newState)
        {
            StateChanged?.Invoke(this, new ConnectionStateChangedEventArgs(
                previousState, newState, "primary"));
        }
    }

    private async Task OnConnectionShutdownAsync(object? sender, ShutdownEventArgs e)
    {
        _logger.LogWarning("Connection shutdown: {Reason}", e.ReplyText);
        ChangeState(ConnectionState.Disconnected);
        Disconnected?.Invoke(this, new ConnectionEventArgs(ConnectionState.Disconnected, "primary"));
        await Task.CompletedTask;
    }

    private async Task OnConnectionRecoveryErrorAsync(object? sender, ConnectionRecoveryErrorEventArgs e)
    {
        _logger.LogError(e.Exception, "Connection recovery error");
        ChangeState(ConnectionState.Error);
        Recovering?.Invoke(this, new ConnectionEventArgs(ConnectionState.Recovering, "primary"));
        await Task.CompletedTask;
    }

    private async Task OnRecoverySucceededAsync(object? sender, AsyncEventArgs e)
    {
        _logger.LogInformation("Connection recovery succeeded");
        ChangeState(ConnectionState.Connected);
        Recovered?.Invoke(this, new ConnectionEventArgs(ConnectionState.Connected, "primary"));
        await Task.CompletedTask;
    }
} 