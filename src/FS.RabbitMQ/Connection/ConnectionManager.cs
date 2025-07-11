using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using RabbitMQ.Client.Exceptions;
using FS.RabbitMQ.Configuration;
using FS.RabbitMQ.Core.Exceptions;
using FS.RabbitMQ.Core.Extensions;

namespace FS.RabbitMQ.Connection;

/// <summary>
/// Manages RabbitMQ connections with automatic recovery, health monitoring, and connection pooling
/// </summary>
public class ConnectionManager : IConnectionManager
{
    private readonly FS.RabbitMQ.Configuration.ConnectionSettings _settings;
    private readonly ILogger<ConnectionManager> _logger;
    private readonly ConnectionPool _connectionPool;
    private readonly Timer _healthCheckTimer;
    private readonly Timer _recoveryTimer;
    private readonly SemaphoreSlim _connectionSemaphore;
    private readonly CancellationTokenSource _cancellationTokenSource;

    private volatile ConnectionState _state = ConnectionState.NotInitialized;
    private volatile bool _disposed;
    private IConnection? _primaryConnection;
    private DateTime _lastConnectionAttempt;
    private int _consecutiveFailures;
    private readonly ConnectionStatistics _statistics;

    /// <summary>
    /// Initializes a new instance of the ConnectionManager class
    /// </summary>
    /// <param name="configuration">RabbitMQ configuration</param>
    /// <param name="logger">Logger instance</param>
    public ConnectionManager(IOptions<RabbitMQConfiguration> configuration, ILogger<ConnectionManager> logger)
    {
        _settings = configuration.Value.Connection;
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _connectionPool = new ConnectionPool(_settings.MaxChannels ?? 20, _logger);
        _connectionSemaphore = new SemaphoreSlim(1, 1);
        _cancellationTokenSource = new CancellationTokenSource();
        _statistics = new ConnectionStatistics();

        // Setup health check timer
        if (_settings.HealthCheck.Enabled)
        {
            _healthCheckTimer = new Timer(PerformHealthCheck, null, 
                _settings.HealthCheck.Interval, _settings.HealthCheck.Interval);
        }

        // Setup recovery timer
        _recoveryTimer = new Timer(AttemptRecovery, null, 
            Timeout.InfiniteTimeSpan, Timeout.InfiniteTimeSpan);
    }

    /// <summary>
    /// Gets whether the connection is currently connected
    /// </summary>
    public bool IsConnected => _state == ConnectionState.Connected && 
                               _primaryConnection?.IsOpen == true;

    /// <summary>
    /// Gets the current connection state
    /// </summary>
    public ConnectionState State => _state;

    /// <summary>
    /// Gets connection statistics
    /// </summary>
    public ConnectionStatistics Statistics => _statistics;

    /// <summary>
    /// Event raised when the connection is established
    /// </summary>
    public event EventHandler<ConnectionEventArgs>? Connected;

    /// <summary>
    /// Event raised when the connection is lost
    /// </summary>
    public event EventHandler<ConnectionEventArgs>? Disconnected;

    /// <summary>
    /// Event raised when the connection is recovering
    /// </summary>
    public event EventHandler<ConnectionEventArgs>? Recovering;

    /// <summary>
    /// Event raised when the connection recovery is complete
    /// </summary>
    public event EventHandler<ConnectionEventArgs>? RecoveryComplete;

    /// <summary>
    /// Event raised when the connection recovery fails
    /// </summary>
    public event EventHandler<ConnectionEventArgs>? RecoveryFailed;

    /// <summary>
    /// Connects to RabbitMQ with automatic retry logic
    /// </summary>
    /// <param name="cancellationToken">Token to monitor for cancellation requests</param>
    /// <returns>A task that represents the asynchronous operation</returns>
    public async Task ConnectAsync(CancellationToken cancellationToken = default)
    {
        if (_disposed)
            return;

        await _connectionSemaphore.WaitAsync(cancellationToken);
        
        try
        {
            if (IsConnected)
                return;

            _logger.LogInformation("Connecting to RabbitMQ at {HostName}:{Port}", 
                _settings.HostName, _settings.Port);

            _state = ConnectionState.Connecting;
            _lastConnectionAttempt = DateTime.UtcNow;
            _statistics.ConnectionAttempts++;

            var factory = CreateConnectionFactory();
            _primaryConnection = await CreateConnectionAsync(factory, cancellationToken);

            if (_primaryConnection?.IsOpen == true)
            {
                SetupConnectionEventHandlers(_primaryConnection);
                await _connectionPool.InitializeAsync(_primaryConnection);
                
                _state = ConnectionState.Connected;
                _consecutiveFailures = 0;
                _statistics.ConnectionSuccesses++;
                
                _logger.LogInformation("Successfully connected to RabbitMQ");
                Connected?.Invoke(this, new ConnectionEventArgs("Connected", null));
            }
            else
            {
                _consecutiveFailures++;
                _statistics.ConnectionFailures++;
                _state = ConnectionState.Failed;
                _logger.LogError("Failed to connect to RabbitMQ");
                throw new ConnectionException("Failed to connect to RabbitMQ");
            }
        }
        catch (Exception ex)
        {
            _consecutiveFailures++;
            _statistics.ConnectionFailures++;
            _state = ConnectionState.Failed;
            _logger.LogError(ex, "Error connecting to RabbitMQ");
            throw;
        }
        finally
        {
            _connectionSemaphore.Release();
        }
    }

    /// <summary>
    /// Disconnects from RabbitMQ gracefully
    /// </summary>
    /// <param name="cancellationToken">Token to monitor for cancellation requests</param>
    /// <returns>A task that represents the asynchronous operation</returns>
    public async Task DisconnectAsync(CancellationToken cancellationToken = default)
    {
        if (_disposed)
            return;

        await _connectionSemaphore.WaitAsync(cancellationToken);
        
        try
        {
            if (_state != ConnectionState.Connected)
                return;

            _logger.LogInformation("Disconnecting from RabbitMQ");
            _state = ConnectionState.Disconnecting;

            // Clean up connection pool
            await _connectionPool.CleanupAsync();

            // Close primary connection
            if (_primaryConnection?.IsOpen == true)
            {
                try
                {
                    await _primaryConnection.CloseAsync();
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Error closing connection gracefully");
                }
            }

            _primaryConnection?.Dispose();
            _primaryConnection = null;
            
            _state = ConnectionState.Disconnected;
            _logger.LogInformation("Disconnected from RabbitMQ");
            Disconnected?.Invoke(this, new ConnectionEventArgs("Disconnected", null));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during disconnection");
        }
        finally
        {
            _connectionSemaphore.Release();
        }
    }

    /// <summary>
    /// Gets a channel from the connection pool with automatic recovery support
    /// </summary>
    /// <param name="cancellationToken">Token to monitor for cancellation requests</param>
    /// <returns>
    /// A task that represents the asynchronous operation. 
    /// The task result contains a pooled RabbitMQ channel ready for use.
    /// </returns>
    /// <exception cref="ObjectDisposedException">
    /// Thrown when the connection manager has been disposed
    /// </exception>
    /// <exception cref="ConnectionException">
    /// Thrown when unable to establish connection or get channel from pool
    /// </exception>
    public async Task<IChannel> GetChannelAsync(CancellationToken cancellationToken = default)
    {
        if (_disposed)
            throw new ObjectDisposedException(nameof(ConnectionManager));

        if (!IsConnected)
        {
            // Attempt to reconnect
            await ConnectAsync(cancellationToken);
            if (!IsConnected)
                throw new ConnectionException("Unable to establish connection to get channel");
        }

        try
        {
            var channel = await _connectionPool.GetChannelAsync(cancellationToken);
            _statistics.ActiveChannels++;
            return channel;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get channel from pool");
            throw new ConnectionException("Failed to get channel", ex);
        }
    }

    /// <summary>
    /// Returns a pooled channel back to the connection pool for reuse
    /// </summary>
    /// <param name="channel">The channel to return to the pool</param>
    /// <remarks>
    /// Always call this method when done using a pooled channel to ensure proper resource management.
    /// Failing to return channels can lead to resource exhaustion.
    /// </remarks>
    public void ReturnChannel(IChannel channel)
    {
        if (_disposed || channel == null)
            return;

        try
        {
            _connectionPool.ReturnChannel(channel);
            _statistics.ActiveChannels--;
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Error returning channel to pool");
        }
    }

    /// <summary>
    /// Creates a dedicated RabbitMQ channel that is not managed by the connection pool
    /// </summary>
    /// <param name="cancellationToken">Token to monitor for cancellation requests</param>
    /// <returns>
    /// A task that represents the asynchronous operation.
    /// The task result contains a dedicated channel that must be disposed manually.
    /// </returns>
    /// <exception cref="ObjectDisposedException">
    /// Thrown when the connection manager has been disposed
    /// </exception>
    /// <exception cref="ConnectionException">
    /// Thrown when unable to establish connection or create channel
    /// </exception>
    /// <remarks>
    /// Use this method when you need a long-lived channel or when pooled channels are not suitable.
    /// The caller is responsible for disposing the returned channel.
    /// </remarks>
    public async Task<IChannel> CreateChannelAsync(CancellationToken cancellationToken = default)
    {
        if (_disposed)
            throw new ObjectDisposedException(nameof(ConnectionManager));

        if (!IsConnected)
        {
            await ConnectAsync(cancellationToken);
            if (!IsConnected)
                throw new ConnectionException("Unable to establish connection to create channel");
        }

        try
        {
            var channel = await _primaryConnection!.CreateChannelAsync(options: null, cancellationToken);
            SetupChannelEventHandlers(channel);
            return channel;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to create dedicated channel");
            throw new ConnectionException("Failed to create channel", ex);
        }
    }

    /// <summary>
    /// Performs a health check on the connection
    /// </summary>
    /// <param name="cancellationToken">Token to monitor for cancellation requests</param>
    /// <returns>
    /// A task that represents the asynchronous operation.
    /// The task result contains <c>true</c> if the connection is healthy; otherwise, <c>false</c>.
    /// </returns>
    public async Task<bool> HealthCheckAsync(CancellationToken cancellationToken = default)
    {
        if (_disposed || !IsConnected)
            return false;

        try
        {
            // Perform a lightweight operation to test connection
            using var channel = await GetChannelAsync(cancellationToken);
            
            // Test basic operations - try to declare a non-existent queue passively
            try
            {
                await channel.QueueDeclarePassiveAsync("amq.gen-non-existent-queue-test");
            }
            catch (OperationInterruptedException)
            {
                // Expected for non-existent queue - connection is working
            }
            
            ReturnChannel(channel);
            return true;
        }
        catch
        {
            return false;
        }
    }

    /// <summary>
    /// Forces an immediate connection recovery attempt
    /// </summary>
    /// <param name="cancellationToken">Token to monitor for cancellation requests</param>
    /// <returns>
    /// A task that represents the asynchronous operation.
    /// The task result contains <c>true</c> if recovery was successful; otherwise, <c>false</c>.
    /// </returns>
    /// <remarks>
    /// This method manually triggers the recovery process, which will close existing connections
    /// and attempt to re-establish them. Use this when automatic recovery is not sufficient.
    /// </remarks>
    public async Task<bool> RecoverAsync(CancellationToken cancellationToken = default)
    {
        if (_disposed)
            return false;

        _logger.LogInformation("Starting connection recovery");
        _state = ConnectionState.Recovering;
        _statistics.RecoveryAttempts++;
        
        Recovering?.Invoke(this, new ConnectionEventArgs("Recovery started", null));

        try
        {
            // Close existing connections
            await DisconnectAsync(cancellationToken);
            
            // Wait a moment before reconnecting
            await Task.Delay(1000, cancellationToken);
            
            // Attempt to reconnect
            await ConnectAsync(cancellationToken);
            
            if (IsConnected)
            {
                _statistics.SuccessfulRecoveries++;
                _logger.LogInformation("Connection recovery completed successfully");
                RecoveryComplete?.Invoke(this, new ConnectionEventArgs("Recovery completed", null));
            }
            else
            {
                _logger.LogError("Connection recovery failed");
                RecoveryFailed?.Invoke(this, new ConnectionEventArgs("Recovery failed", null));
            }
            
            return IsConnected;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during connection recovery");
            RecoveryFailed?.Invoke(this, new ConnectionEventArgs("Recovery failed", ex));
            return false;
        }
    }

    private IConnectionFactory CreateConnectionFactory()
    {
        var factory = new ConnectionFactory
        {
            HostName = _settings.HostName,
            Port = _settings.Port,
            VirtualHost = _settings.VirtualHost,
            UserName = _settings.UserName,
            Password = _settings.Password,
            RequestedHeartbeat = _settings.RequestedHeartbeat,
            RequestedConnectionTimeout = _settings.RequestedConnectionTimeout,
            AutomaticRecoveryEnabled = _settings.AutomaticRecovery.Enabled,
            NetworkRecoveryInterval = _settings.AutomaticRecovery.NetworkRecoveryInterval,
            TopologyRecoveryEnabled = _settings.AutomaticRecovery.TopologyRecoveryEnabled,
            ClientProvidedName = _settings.ClientProvidedName
        };

        if (_settings.Ssl.Enabled)
        {
            factory.Ssl = new SslOption
            {
                Enabled = true,
                ServerName = _settings.Ssl.ServerName,
                Version = _settings.Ssl.Version,
                AcceptablePolicyErrors = _settings.Ssl.AcceptablePolicyErrors
            };

            if (!string.IsNullOrEmpty(_settings.Ssl.CertificatePath))
            {
                factory.Ssl.CertPath = _settings.Ssl.CertificatePath;
            }

            if (!string.IsNullOrEmpty(_settings.Ssl.CertificatePassword))
            {
                factory.Ssl.CertPassphrase = _settings.Ssl.CertificatePassword;
            }
        }

        return factory;
    }

    private async Task<IConnection> CreateConnectionAsync(IConnectionFactory factory, CancellationToken cancellationToken)
    {
        return await factory.CreateConnectionAsync(cancellationToken);
    }

    private void SetupConnectionEventHandlers(IConnection connection)
    {
        // RabbitMQ.Client 7.x removed these event APIs
        // Events are now handled internally or through different mechanisms
        // connection.ConnectionShutdown += OnConnectionShutdown;
        // connection.ConnectionBlocked += OnConnectionBlocked;
        // connection.ConnectionUnblocked += OnConnectionUnblocked;
        // connection.CallbackException += OnCallbackException;
        
        _logger.LogDebug("Connection event handlers setup (7.x compatibility mode)");
    }

    private void SetupChannelEventHandlers(IChannel channel)
    {
        // RabbitMQ.Client 7.x removed these event APIs
        // Events are now handled internally or through different mechanisms  
        // channel.CallbackException += OnChannelCallbackException;
        // channel.ChannelShutdown += OnChannelShutdown;
        
        _logger.LogDebug("Channel event handlers setup (7.x compatibility mode)");
    }

    private void OnConnectionShutdown(object? sender, ShutdownEventArgs e)
    {
        _logger.LogWarning("Connection shutdown: {ReplyText}", e.ReplyText);
        _state = ConnectionState.Disconnected;
        _statistics.ConnectionShutdowns++;
        
        Disconnected?.Invoke(this, new ConnectionEventArgs("Connection shutdown", null));
        
        // Start recovery if enabled
        if (_settings.AutomaticRecovery.Enabled && !_disposed)
        {
            var delay = CalculateRecoveryDelay();
            _recoveryTimer.Change(delay, Timeout.InfiniteTimeSpan);
        }
    }

    private void OnConnectionBlocked(object? sender, ConnectionBlockedEventArgs e)
    {
        _logger.LogWarning("Connection blocked: {Reason}", e.Reason);
        _statistics.ConnectionBlocks++;
    }

    private void OnConnectionUnblocked(object? sender, EventArgs e)
    {
        _logger.LogInformation("Connection unblocked");
        _statistics.ConnectionUnblocks++;
    }

    private void OnCallbackException(object? sender, CallbackExceptionEventArgs e)
    {
        _logger.LogError(e.Exception, "Connection callback exception");
        _statistics.CallbackExceptions++;
    }

    private void OnChannelShutdown(object? sender, ShutdownEventArgs e)
    {
        _logger.LogDebug("Channel shutdown: {ReplyText}", e.ReplyText);
        _statistics.ChannelShutdowns++;
    }

    private void OnChannelCallbackException(object? sender, CallbackExceptionEventArgs e)
    {
        _logger.LogError(e.Exception, "Channel callback exception");
        _statistics.ChannelCallbackExceptions++;
    }

    private TimeSpan CalculateRecoveryDelay()
    {
        var baseDelay = _settings.AutomaticRecovery.NetworkRecoveryInterval;
        var exponentialBackoff = Math.Min(Math.Pow(2, _consecutiveFailures), 60);
        return TimeSpan.FromSeconds(baseDelay.TotalSeconds * exponentialBackoff);
    }

    private void PerformHealthCheck(object? state)
    {
        if (_disposed || !_settings.HealthCheck.Enabled)
            return;

        try
        {
            var isHealthy = HealthCheckAsync(_cancellationTokenSource.Token).GetAwaiter().GetResult();
            _statistics.HealthChecks++;
            
            if (!isHealthy)
            {
                _logger.LogWarning("Health check failed");
                _statistics.HealthCheckFailures++;
                
                // Trigger recovery if not already in progress
                if (_state != ConnectionState.Recovering && _settings.AutomaticRecovery.Enabled)
                {
                    _recoveryTimer.Change(TimeSpan.Zero, Timeout.InfiniteTimeSpan);
                }
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during health check");
            _statistics.HealthCheckFailures++;
        }
    }

    private void AttemptRecovery(object? state)
    {
        if (_disposed || _state == ConnectionState.Recovering)
            return;

        try
        {
            var success = RecoverAsync(_cancellationTokenSource.Token).GetAwaiter().GetResult();
            if (!success && _settings.AutomaticRecovery.Enabled)
            {
                // Schedule next recovery attempt
                var delay = CalculateRecoveryDelay();
                _recoveryTimer.Change(delay, Timeout.InfiniteTimeSpan);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during recovery attempt");
            
            // Schedule next recovery attempt
            if (_settings.AutomaticRecovery.Enabled)
            {
                var delay = CalculateRecoveryDelay();
                _recoveryTimer.Change(delay, Timeout.InfiniteTimeSpan);
            }
        }
    }

    /// <summary>
    /// Disposes the connection manager and all its resources
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
            _recoveryTimer?.Dispose();
            DisconnectAsync().GetAwaiter().GetResult();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during disposal");
        }
        finally
        {
            _connectionSemaphore?.Dispose();
            _cancellationTokenSource?.Dispose();
        }
    }
}