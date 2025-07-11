using FS.Transport.AMQP.Configuration;
using FS.Transport.AMQP.Core.Exceptions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using RabbitMQ.Client.Exceptions;

namespace FS.Transport.AMQP.Connection;

/// <summary>
/// Manages RabbitMQ connections with auto-recovery, connection pooling, and health monitoring
/// </summary>
/// <remarks>
/// This class provides high-level connection management including automatic reconnection,
/// connection pooling for performance, health monitoring, and graceful error handling.
/// It implements the IConnectionManager interface for dependency injection.
/// </remarks>
public class ConnectionManager : IConnectionManager
{
    private readonly ConnectionSettings _settings;
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
    /// Gets a value indicating whether the connection to RabbitMQ is currently active
    /// </summary>
    /// <value>
    /// <c>true</c> if connected to RabbitMQ and the connection is open; otherwise, <c>false</c>
    /// </value>
    public bool IsConnected => _state == ConnectionState.Connected && 
                              _primaryConnection != null && 
                              _primaryConnection.IsOpen;
    
    /// <summary>
    /// Gets the current state of the connection
    /// </summary>
    /// <value>
    /// The current connection state (NotInitialized, Connecting, Connected, Disconnecting, Failed, Closed)
    /// </value>
    public ConnectionState State => _state;
    
    /// <summary>
    /// Gets connection statistics including connection counts, failures, and performance metrics
    /// </summary>
    /// <value>
    /// A <see cref="ConnectionStatistics"/> object containing detailed connection metrics
    /// </value>
    public ConnectionStatistics Statistics => _statistics;

    /// <summary>
    /// Occurs when a connection to RabbitMQ is successfully established
    /// </summary>
    public event EventHandler<ConnectionEventArgs>? Connected;
    
    /// <summary>
    /// Occurs when the connection to RabbitMQ is lost or closed
    /// </summary>
    public event EventHandler<ConnectionEventArgs>? Disconnected;
    
    /// <summary>
    /// Occurs when auto-recovery process begins after connection failure
    /// </summary>
    public event EventHandler<ConnectionEventArgs>? Recovering;
    
    /// <summary>
    /// Occurs when auto-recovery successfully restores the connection
    /// </summary>
    public event EventHandler<ConnectionEventArgs>? Recovered;
    
    /// <summary>
    /// Occurs when auto-recovery fails to restore the connection
    /// </summary>
    public event EventHandler<ConnectionEventArgs>? RecoveryFailed;

    /// <summary>
    /// Initializes a new instance of the <see cref="ConnectionManager"/> class
    /// </summary>
    /// <param name="configuration">RabbitMQ configuration containing connection settings</param>
    /// <param name="logger">Logger for connection management activities</param>
    /// <exception cref="ArgumentNullException">
    /// Thrown when <paramref name="configuration"/> or <paramref name="logger"/> is null
    /// </exception>
    public ConnectionManager(IOptions<RabbitMQConfiguration> configuration, ILogger<ConnectionManager> logger)
    {
        _settings = configuration?.Value?.Connection ?? throw new ArgumentNullException(nameof(configuration));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _connectionPool = new ConnectionPool(_settings, _logger);
        _connectionSemaphore = new SemaphoreSlim(1, 1);
        _cancellationTokenSource = new CancellationTokenSource();
        _statistics = new ConnectionStatistics();
        
        // Initialize timers for health check and recovery
        _healthCheckTimer = new Timer(PerformHealthCheck, null, Timeout.Infinite, Timeout.Infinite);
        _recoveryTimer = new Timer(AttemptRecovery, null, Timeout.Infinite, Timeout.Infinite);
        
        _logger.LogDebug("ConnectionManager initialized with settings: {HostName}:{Port}", 
            _settings.HostName, _settings.Port);
    }

    /// <summary>
    /// Establishes connection with retry logic and auto-recovery setup
    /// </summary>
    public async Task<bool> ConnectAsync(CancellationToken cancellationToken = default)
    {
        if (_disposed)
            throw new ObjectDisposedException(nameof(ConnectionManager));

        if (IsConnected)
        {
            _logger.LogDebug("Already connected to RabbitMQ");
            return true;
        }

        await _connectionSemaphore.WaitAsync(cancellationToken);
        try
        {
            _state = ConnectionState.Connecting;
            _lastConnectionAttempt = DateTime.UtcNow;
            
            _logger.LogInformation("Attempting to connect to RabbitMQ at {HostName}:{Port}", 
                _settings.HostName, _settings.Port);

            var factory = CreateConnectionFactory();
            
            // Attempt connection with timeout
            using var timeoutCts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
            timeoutCts.CancelAfter(_settings.ConnectionTimeoutMs);
            
            try
            {
                _primaryConnection = await CreateConnectionAsync(factory, timeoutCts.Token);
                
                // Setup connection event handlers
                SetupConnectionEventHandlers(_primaryConnection);
                
                _state = ConnectionState.Connected;
                _consecutiveFailures = 0;
                _statistics.TotalConnections++;
                _statistics.LastConnected = DateTime.UtcNow;
                
                // Initialize connection pool
                await _connectionPool.InitializeAsync(_primaryConnection);
                
                // Start health check timer
                _healthCheckTimer.Change(TimeSpan.FromSeconds(30), TimeSpan.FromSeconds(30));
                
                _logger.LogInformation("Successfully connected to RabbitMQ");
                Connected?.Invoke(this, new ConnectionEventArgs("Connection established", null));
                
                return true;
            }
            catch (OperationCanceledException) when (timeoutCts.Token.IsCancellationRequested)
            {
                throw new ConnectionTimeoutException(TimeSpan.FromMilliseconds(_settings.ConnectionTimeoutMs));
            }
        }
        catch (Exception ex)
        {
            _consecutiveFailures++;
            _statistics.FailedConnections++;
            _state = ConnectionState.Failed;
            
            _logger.LogError(ex, "Failed to connect to RabbitMQ (attempt {ConsecutiveFailures})", _consecutiveFailures);
            
            // Start recovery timer if auto-recovery is enabled
            if (_settings.AutoRecoveryEnabled && !_disposed)
            {
                var delay = CalculateRecoveryDelay();
                _recoveryTimer.Change(delay, Timeout.InfiniteTimeSpan);
                _logger.LogInformation("Auto-recovery scheduled in {Delay} seconds", delay.TotalSeconds);
            }
            
            throw new ConnectionException($"Failed to connect to RabbitMQ: {ex.Message}", ex);
        }
        finally
        {
            _connectionSemaphore.Release();
        }
    }

    /// <summary>
    /// Gracefully disconnects from RabbitMQ
    /// </summary>
    public async Task DisconnectAsync(CancellationToken cancellationToken = default)
    {
        if (_disposed || _state == ConnectionState.Closed)
            return;

        await _connectionSemaphore.WaitAsync(cancellationToken);
        try
        {
            _logger.LogInformation("Disconnecting from RabbitMQ");
            
            // Stop timers
            _healthCheckTimer.Change(Timeout.Infinite, Timeout.Infinite);
            _recoveryTimer.Change(Timeout.Infinite, Timeout.Infinite);
            
            // Close connection pool
            await _connectionPool.CloseAsync();
            
            // Close primary connection
            if (_primaryConnection != null)
            {
                try
                {
                    if (_primaryConnection.IsOpen)
                    {
                        _primaryConnection.Close();
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Error during connection close");
                }
                finally
                {
                    _primaryConnection?.Dispose();
                    _primaryConnection = null;
                }
            }
            
            _state = ConnectionState.Closed;
            _statistics.LastDisconnected = DateTime.UtcNow;
            
            _logger.LogInformation("Disconnected from RabbitMQ");
            Disconnected?.Invoke(this, new ConnectionEventArgs("Connection closed", null));
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
    public async Task<IModel> GetChannelAsync(CancellationToken cancellationToken = default)
    {
        if (_disposed)
            throw new ObjectDisposedException(nameof(ConnectionManager));

        if (!IsConnected)
        {
            // Attempt to reconnect
            var connected = await ConnectAsync(cancellationToken);
            if (!connected)
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
    public void ReturnChannel(IModel channel)
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
    public async Task<IModel> CreateChannelAsync(CancellationToken cancellationToken = default)
    {
        if (_disposed)
            throw new ObjectDisposedException(nameof(ConnectionManager));

        if (!IsConnected)
        {
            var connected = await ConnectAsync(cancellationToken);
            if (!connected)
                throw new ConnectionException("Unable to establish connection to create channel");
        }

        try
        {
            var channel = _primaryConnection!.CreateModel();
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
    /// Performs a lightweight test to verify the connection health
    /// </summary>
    /// <param name="cancellationToken">Token to monitor for cancellation requests</param>
    /// <returns>
    /// A task that represents the asynchronous operation.
    /// The task result contains <c>true</c> if the connection is healthy; otherwise, <c>false</c>.
    /// </returns>
    /// <remarks>
    /// This method performs a non-destructive test operation to verify connectivity.
    /// It can be used for health checks and monitoring purposes.
    /// </remarks>
    public async Task<bool> TestConnectionAsync(CancellationToken cancellationToken = default)
    {
        if (_disposed || !IsConnected)
            return false;

        try
        {
            // Perform a lightweight operation to test connection
            using var channel = await GetChannelAsync(cancellationToken);
            
            // Test basic operations
            channel.QueueDeclarePassive("amq.gen-non-existent-queue-test");
            
            ReturnChannel(channel);
            return true;
        }
        catch (OperationInterruptedException)
        {
            // Expected for non-existent queue - connection is working
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
            var success = await ConnectAsync(cancellationToken);
            
            if (success)
            {
                _statistics.SuccessfulRecoveries++;
                _logger.LogInformation("Connection recovery completed successfully");
                Recovered?.Invoke(this, new ConnectionEventArgs("Recovery completed", null));
            }
            else
            {
                _logger.LogError("Connection recovery failed");
                RecoveryFailed?.Invoke(this, new ConnectionEventArgs("Recovery failed", null));
            }
            
            return success;
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
            UserName = _settings.UserName,
            Password = _settings.Password,
            VirtualHost = _settings.VirtualHost,
            RequestedHeartbeat = TimeSpan.FromSeconds(_settings.HeartbeatInterval),
            RequestedChannelMax = _settings.RequestedChannelMax,
            RequestedFrameMax = _settings.RequestedFrameMax,
            AutomaticRecoveryEnabled = false, // We handle recovery ourselves
            TopologyRecoveryEnabled = false,  // We handle topology recovery ourselves
            NetworkRecoveryInterval = TimeSpan.FromSeconds(10),
            ClientProvidedName = _settings.ConnectionName ?? $"FS.Transport.AMQP-{Environment.MachineName}",
            ClientProperties = _settings.ClientProperties.ToDictionary(kvp => kvp.Key, kvp => kvp.Value)
        };

        // Configure SSL if enabled
        if (!_settings.UseSsl) return factory;
        factory.Ssl = new SslOption
        {
            Enabled = true,
            ServerName = _settings.Ssl.ServerName ?? _settings.HostName,
            AcceptablePolicyErrors = _settings.Ssl.AcceptInvalidCertificates 
                ? System.Net.Security.SslPolicyErrors.RemoteCertificateNotAvailable
                  | System.Net.Security.SslPolicyErrors.RemoteCertificateNameMismatch
                  | System.Net.Security.SslPolicyErrors.RemoteCertificateChainErrors
                : System.Net.Security.SslPolicyErrors.None
        };

        if (string.IsNullOrEmpty(_settings.Ssl.CertificatePath)) return factory;
        factory.Ssl.CertPath = _settings.Ssl.CertificatePath;
        factory.Ssl.CertPassphrase = _settings.Ssl.CertificatePassword;

        return factory;
    }

    private async Task<IConnection> CreateConnectionAsync(IConnectionFactory factory, CancellationToken cancellationToken)
    {
        return await Task.Run(() => factory.CreateConnection(), cancellationToken);
    }

    private void SetupConnectionEventHandlers(IConnection connection)
    {
        connection.ConnectionShutdown += OnConnectionShutdown;
        connection.ConnectionBlocked += OnConnectionBlocked;
        connection.ConnectionUnblocked += OnConnectionUnblocked;
        connection.CallbackException += OnCallbackException;
    }

    private void SetupChannelEventHandlers(IModel channel)
    {
        channel.ModelShutdown += OnChannelShutdown;
        channel.CallbackException += OnChannelCallbackException;
    }

    private void OnConnectionShutdown(object? sender, ShutdownEventArgs e)
    {
        if (_disposed)
            return;

        _logger.LogWarning("Connection shutdown: {Reason}", e.ReplyText);
        
        if (_state == ConnectionState.Connected)
        {
            _state = ConnectionState.Disconnected;
            _statistics.LastDisconnected = DateTime.UtcNow;
            
            Disconnected?.Invoke(this, new ConnectionEventArgs($"Connection shutdown: {e.ReplyText}", null));
            
            // Start recovery if auto-recovery is enabled
            if (_settings.AutoRecoveryEnabled)
            {
                var delay = CalculateRecoveryDelay();
                _recoveryTimer.Change(delay, Timeout.InfiniteTimeSpan);
            }
        }
    }

    private void OnConnectionBlocked(object? sender, ConnectionBlockedEventArgs e)
    {
        _logger.LogWarning("Connection blocked: {Reason}", e.Reason);
    }

    private void OnConnectionUnblocked(object? sender, EventArgs e)
    {
        _logger.LogInformation("Connection unblocked");
    }

    private void OnCallbackException(object? sender, CallbackExceptionEventArgs e)
    {
        _logger.LogError(e.Exception, "Connection callback exception: {Detail}", e.Detail);
    }

    private void OnChannelShutdown(object? sender, ShutdownEventArgs e)
    {
        _logger.LogDebug("Channel shutdown: {Reason}", e.ReplyText);
    }

    private void OnChannelCallbackException(object? sender, CallbackExceptionEventArgs e)
    {
        _logger.LogError(e.Exception, "Channel callback exception: {Detail}", e.Detail);
    }

    private TimeSpan CalculateRecoveryDelay()
    {
        // Exponential backoff with jitter
        var baseDelay = Math.Min(1000 * Math.Pow(2, _consecutiveFailures - 1), 30000); // Max 30 seconds
        var jitter = Random.Shared.NextDouble() * 0.1 * baseDelay; // 10% jitter
        return TimeSpan.FromMilliseconds(baseDelay + jitter);
    }

    private void PerformHealthCheck(object? state)
    {
        if (_disposed || !IsConnected)
            return;

        Task.Run(async () =>
        {
            try
            {
                var isHealthy = await TestConnectionAsync(_cancellationTokenSource.Token);
                if (!isHealthy)
                {
                    _logger.LogWarning("Health check failed - connection appears unhealthy");
                    if (_settings.AutoRecoveryEnabled)
                    {
                        _ = Task.Run(() => RecoverAsync(_cancellationTokenSource.Token));
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during health check");
            }
        });
    }

    private void AttemptRecovery(object? state)
    {
        if (_disposed)
            return;

        Task.Run(async () =>
        {
            try
            {
                await RecoverAsync(_cancellationTokenSource.Token);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Auto-recovery attempt failed");
                
                // Schedule next recovery attempt
                if (_settings.AutoRecoveryEnabled && _consecutiveFailures < 10)
                {
                    var delay = CalculateRecoveryDelay();
                    _recoveryTimer.Change(delay, Timeout.InfiniteTimeSpan);
                }
            }
        });
    }

    /// <summary>
    /// Releases all resources used by the <see cref="ConnectionManager"/>
    /// </summary>
    /// <remarks>
    /// This method closes all connections, stops timers, and releases managed and unmanaged resources.
    /// After disposal, the connection manager cannot be reused.
    /// </remarks>
    public void Dispose()
    {
        if (_disposed)
            return;

        _disposed = true;
        
        try
        {
            _cancellationTokenSource.Cancel();
            
            DisconnectAsync().GetAwaiter().GetResult();
            
            _healthCheckTimer?.Dispose();
            _recoveryTimer?.Dispose();
            _connectionPool?.Dispose();
            _connectionSemaphore?.Dispose();
            _cancellationTokenSource?.Dispose();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during ConnectionManager disposal");
        }
    }
}