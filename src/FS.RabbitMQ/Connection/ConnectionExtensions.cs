using FS.RabbitMQ.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace FS.RabbitMQ.Connection;

/// <summary>
/// Extension methods for connection management
/// </summary>
public static class ConnectionExtensions
{
    /// <summary>
    /// Adds connection management services to the service collection
    /// </summary>
    /// <param name="services">Service collection</param>
    /// <returns>Service collection for fluent configuration</returns>
    public static IServiceCollection AddConnectionManager(this IServiceCollection services)
    {
        services.TryAddSingleton<IConnectionManager, ConnectionManager>();
        return services;
    }

    /// <summary>
    /// Configures connection settings
    /// </summary>
    /// <param name="services">Service collection</param>
    /// <param name="configureConnection">Connection configuration action</param>
    /// <returns>Service collection for fluent configuration</returns>
    public static IServiceCollection ConfigureConnection(this IServiceCollection services, 
        Action<ConnectionSettings> configureConnection)
    {
        services.Configure<RabbitMQConfiguration>(config => configureConnection(config.Connection));
        return services;
    }

    /// <summary>
    /// Enables connection auto-recovery
    /// </summary>
    /// <param name="services">Service collection</param>
    /// <param name="enabled">Whether to enable auto-recovery</param>
    /// <returns>Service collection for fluent configuration</returns>
    public static IServiceCollection EnableAutoRecovery(this IServiceCollection services, bool enabled = true)
    {
        services.Configure<RabbitMQConfiguration>(config => config.AutoRecoveryEnabled = enabled);
        return services;
    }

    /// <summary>
    /// Configures connection pooling settings
    /// </summary>
    /// <param name="services">Service collection</param>
    /// <param name="minConnections">Minimum number of connections</param>
    /// <param name="maxConnections">Maximum number of connections</param>
    /// <returns>Service collection for fluent configuration</returns>
    public static IServiceCollection ConfigureConnectionPooling(this IServiceCollection services, 
        int minConnections, 
        int maxConnections)
    {
        services.Configure<RabbitMQConfiguration>(config =>
        {
            config.Connection.MinConnections = minConnections;
            config.Connection.MaxConnections = maxConnections;
        });
        return services;
    }

    /// <summary>
    /// Configures SSL settings for secure connections
    /// </summary>
    /// <param name="services">Service collection</param>
    /// <param name="configureSsl">SSL configuration action</param>
    /// <returns>Service collection for fluent configuration</returns>
    public static IServiceCollection ConfigureSsl(this IServiceCollection services, 
        Action<SslSettings> configureSsl)
    {
        services.Configure<RabbitMQConfiguration>(config =>
        {
            config.Connection.UseSsl = true;
            configureSsl(config.Connection.Ssl);
        });
        return services;
    }

    /// <summary>
    /// Sets connection timeout
    /// </summary>
    /// <param name="services">Service collection</param>
    /// <param name="timeoutMs">Timeout in milliseconds</param>
    /// <returns>Service collection for fluent configuration</returns>
    public static IServiceCollection SetConnectionTimeout(this IServiceCollection services, int timeoutMs)
    {
        services.Configure<RabbitMQConfiguration>(config => config.Connection.ConnectionTimeoutMs = timeoutMs);
        return services;
    }

    /// <summary>
    /// Sets heartbeat interval
    /// </summary>
    /// <param name="services">Service collection</param>
    /// <param name="intervalSeconds">Heartbeat interval in seconds</param>
    /// <returns>Service collection for fluent configuration</returns>
    public static IServiceCollection SetHeartbeatInterval(this IServiceCollection services, ushort intervalSeconds)
    {
        services.Configure<RabbitMQConfiguration>(config => config.Connection.HeartbeatInterval = intervalSeconds);
        return services;
    }
}