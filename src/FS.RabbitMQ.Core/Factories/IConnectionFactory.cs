namespace FS.RabbitMQ.Core.Factories;

/// <summary>
/// Defines the contract for connection factories that create and manage RabbitMQ connections.
/// </summary>
/// <remarks>
/// Connection factories are responsible for creating, configuring, and managing the lifecycle
/// of RabbitMQ connections. They handle connection pooling, authentication, SSL/TLS configuration,
/// and connection recovery to provide reliable connectivity to RabbitMQ brokers.
/// </remarks>
public interface IConnectionFactory : IDisposable
{
    /// <summary>
    /// Creates a new connection to the RabbitMQ broker.
    /// </summary>
    /// <returns>A new connection instance.</returns>
    /// <exception cref="ConnectionException">Thrown when connection creation fails.</exception>
    Task<IConnection> CreateConnectionAsync();

    /// <summary>
    /// Creates a new connection to the RabbitMQ broker with the specified name.
    /// </summary>
    /// <param name="connectionName">The logical name for the connection.</param>
    /// <returns>A new connection instance.</returns>
    /// <exception cref="ConnectionException">Thrown when connection creation fails.</exception>
    Task<IConnection> CreateConnectionAsync(string connectionName);

    /// <summary>
    /// Creates a new connection using the specified broker configuration.
    /// </summary>
    /// <param name="brokerConfiguration">The broker configuration to use.</param>
    /// <returns>A new connection instance.</returns>
    /// <exception cref="ConnectionException">Thrown when connection creation fails.</exception>
    Task<IConnection> CreateConnectionAsync(BrokerConfiguration brokerConfiguration);

    /// <summary>
    /// Validates the connection configuration and tests connectivity.
    /// </summary>
    /// <returns><c>true</c> if the configuration is valid and connectivity is available; otherwise, <c>false</c>.</returns>
    Task<bool> ValidateConnectionAsync();

    /// <summary>
    /// Gets the current connection statistics and health information.
    /// </summary>
    /// <returns>Connection statistics and health data.</returns>
    Task<ConnectionHealthInfo> GetConnectionHealthAsync();

    /// <summary>
    /// Occurs when a connection is created.
    /// </summary>
    event EventHandler<ConnectionCreatedEventArgs>? ConnectionCreated;

    /// <summary>
    /// Occurs when a connection creation fails.
    /// </summary>
    event EventHandler<ConnectionFailedEventArgs>? ConnectionFailed;
}