using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using RabbitMQ.Client;

namespace FS.RabbitMQ.Core.Factories;

/// <summary>
/// Provides a robust implementation of the connection factory with advanced features.
/// </summary>
/// <remarks>
/// The default connection factory implementation provides comprehensive connection management
/// including connection pooling, automatic recovery, health monitoring, and performance optimization.
/// It integrates with the configuration system and dependency injection container for seamless
/// integration with ASP.NET Core and other .NET hosting environments.
/// </remarks>
public sealed class DefaultConnectionFactory : FactoryBase<IConnection>, IConnectionFactory
{
    private readonly BrokerConfiguration _configuration;
    private readonly IConnectionPoolManager _poolManager;
    private readonly IConnectionHealthMonitor _healthMonitor;
    private readonly IRetryPolicyExecutor _retryExecutor;

    /// <summary>
    /// Initializes a new instance of the <see cref="DefaultConnectionFactory"/> class.
    /// </summary>
    /// <param name="configuration">The broker configuration.</param>
    /// <param name="poolManager">The connection pool manager.</param>
    /// <param name="healthMonitor">The connection health monitor.</param>
    /// <param name="retryExecutor">The retry policy executor.</param>
    /// <param name="logger">The logger instance.</param>
    public DefaultConnectionFactory(
        IOptions<BrokerConfiguration> configuration,
        IConnectionPoolManager poolManager,
        IConnectionHealthMonitor healthMonitor,
        IRetryPolicyExecutor retryExecutor,
        ILogger<DefaultConnectionFactory> logger)
        : base(logger)
    {
        _configuration = configuration?.Value ?? throw new ArgumentNullException(nameof(configuration));
        _poolManager = poolManager ?? throw new ArgumentNullException(nameof(poolManager));
        _healthMonitor = healthMonitor ?? throw new ArgumentNullException(nameof(healthMonitor));
        _retryExecutor = retryExecutor ?? throw new ArgumentNullException(nameof(retryExecutor));

        ValidateConfiguration();
    }

    /// <summary>
    /// Occurs when a connection is created.
    /// </summary>
    public event EventHandler<ConnectionCreatedEventArgs>? ConnectionCreated;

    /// <summary>
    /// Occurs when a connection creation fails.
    /// </summary>
    public event EventHandler<ConnectionFailedEventArgs>? ConnectionFailed;

    /// <summary>
    /// Creates a new connection to the RabbitMQ broker.
    /// </summary>
    /// <returns>A new connection instance.</returns>
    public async Task<IConnection> CreateConnectionAsync()
    {
        return await CreateConnectionAsync($"Connection-{Guid.NewGuid():N}");
    }

    /// <summary>
    /// Creates a new connection to the RabbitMQ broker with the specified name.
    /// </summary>
    /// <param name="connectionName">The logical name for the connection.</param>
    /// <returns>A new connection instance.</returns>
    public async Task<IConnection> CreateConnectionAsync(string connectionName)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(connectionName);
        return await CreateConnectionAsync(_configuration, connectionName);
    }

    /// <summary>
    /// Creates a new connection using the specified broker configuration.
    /// </summary>
    /// <param name="brokerConfiguration">The broker configuration to use.</param>
    /// <returns>A new connection instance.</returns>
    public async Task<IConnection> CreateConnectionAsync(BrokerConfiguration brokerConfiguration)
    {
        ArgumentNullException.ThrowIfNull(brokerConfiguration);
        return await CreateConnectionAsync(brokerConfiguration, $"Connection-{Guid.NewGuid():N}");
    }

    /// <summary>
    /// Validates the connection configuration and tests connectivity.
    /// </summary>
    /// <returns><c>true</c> if the configuration is valid and connectivity is available; otherwise, <c>false</c>.</returns>
    public async Task<bool> ValidateConnectionAsync()
    {
        try
        {
            Logger.LogDebug("Validating connection configuration and connectivity");

            // Validate configuration
            _configuration.Validate();

            // Test connectivity by creating a temporary connection
            using var testConnection = await CreateConnectionInternalAsync(_configuration, "ValidationTest");
            
            // Perform basic health check
            var healthInfo = await _healthMonitor.CheckHealthAsync(testConnection);
            
            Logger.LogDebug("Connection validation successful. Health status: {Status}", healthInfo.Status);
            return healthInfo.Status == ConnectionHealthStatus.Healthy;
        }
        catch (Exception ex)
        {
            Logger.LogWarning(ex, "Connection validation failed");
            return false;
        }
    }

    /// <summary>
    /// Gets the current connection statistics and health information.
    /// </summary>
    /// <returns>Connection statistics and health data.</returns>
    public async Task<ConnectionHealthInfo> GetConnectionHealthAsync()
    {
        ThrowIfDisposed();

        try
        {
            var poolHealth = await _poolManager.GetHealthInfoAsync();
            return new ConnectionHealthInfo
            {
                Status = poolHealth.IsHealthy ? ConnectionHealthStatus.Healthy : ConnectionHealthStatus.Unhealthy,
                ActiveConnections = poolHealth.ActiveConnections,
                PooledConnections = poolHealth.PooledConnections,
                TotalConnectionsCreated = poolHealth.TotalConnectionsCreated,
                LastHealthCheck = DateTimeOffset.UtcNow,
                Details = poolHealth.Details
            };
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Failed to get connection health information");
            throw new ConnectionException("Failed to retrieve connection health information", ex);
        }
    }

    /// <summary>
    /// Creates the core connection instance.
    /// </summary>
    /// <param name="parameters">The parameters for connection creation.</param>
    /// <returns>A new connection instance.</returns>
    protected override IConnection CreateCore(params object[] parameters)
    {
        // This method is called by the base class Create method
        // For async operations, we use the public async methods instead
        throw new NotSupportedException("Use CreateConnectionAsync methods for asynchronous connection creation");
    }

    private async Task<IConnection> CreateConnectionAsync(BrokerConfiguration configuration, string connectionName)
    {
        ThrowIfDisposed();
        ArgumentNullException.ThrowIfNull(configuration);
        ArgumentException.ThrowIfNullOrWhiteSpace(connectionName);

        try
        {
            Logger.LogInformation("Creating new connection '{ConnectionName}' to broker", connectionName);

            var connection = await _retryExecutor.ExecuteAsync(
                async () => await CreateConnectionInternalAsync(configuration, connectionName),
                configuration.Connection.ConnectionRetryPolicy ?? RetryPolicy.CreateDefault(),
                CancellationToken.None);

            // Start health monitoring for this connection
            await _healthMonitor.StartMonitoringAsync(connection);

            // Register with pool manager
            await _poolManager.RegisterConnectionAsync(connection);

            // Raise connection created event
            ConnectionCreated?.Invoke(this, new ConnectionCreatedEventArgs(connection, connectionName));

            Logger.LogInformation("Successfully created connection '{ConnectionName}'", connectionName);
            return connection;
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Failed to create connection '{ConnectionName}'", connectionName);
            
            // Raise connection failed event
            ConnectionFailed?.Invoke(this, new ConnectionFailedEventArgs(connectionName, ex));
            
            throw new ConnectionException($"Failed to create connection '{connectionName}'", ex);
        }
    }

    private async Task<IConnection> CreateConnectionInternalAsync(BrokerConfiguration configuration, string connectionName)
    {
        // This would integrate with RabbitMQ.Client or a similar library
        // For now, we'll create a mock implementation
        var connectionBuilder = new ConnectionBuilder()
            .WithConfiguration(configuration)
            .WithName(connectionName)
            .WithLogger(Logger);

        return await connectionBuilder.BuildAsync();
    }

    private void ValidateConfiguration()
    {
        try
        {
            _configuration.Validate();
            Logger.LogDebug("Connection factory configuration validation successful");
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Connection factory configuration validation failed");
            throw new FactoryException("Invalid connection factory configuration", ex);
        }
    }

    /// <summary>
    /// Releases the resources used by the connection factory.
    /// </summary>
    /// <param name="disposing"><c>true</c> to release both managed and unmanaged resources; otherwise, <c>false</c>.</param>
    protected override void Dispose(bool disposing)
    {
        if (disposing && !IsDisposed)
        {
            Logger.LogDebug("Disposing connection factory and associated resources");
            
            try
            {
                _poolManager?.Dispose();
                _healthMonitor?.Dispose();
            }
            catch (Exception ex)
            {
                Logger.LogWarning(ex, "Error occurred while disposing connection factory resources");
            }
        }

        base.Dispose(disposing);
    }
}