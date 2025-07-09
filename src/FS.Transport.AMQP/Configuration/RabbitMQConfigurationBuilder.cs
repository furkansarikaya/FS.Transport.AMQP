using Microsoft.Extensions.DependencyInjection;

namespace FS.Transport.AMQP.Configuration;

/// <summary>
/// Fluent builder for RabbitMQ configuration
/// </summary>
public class RabbitMQConfigurationBuilder
{
    private readonly IServiceCollection _services;
    private readonly RabbitMQConfiguration _configuration;

    internal RabbitMQConfigurationBuilder(IServiceCollection services)
    {
        _services = services;
        _configuration = new RabbitMQConfiguration();
    }

    /// <summary>
    /// Configures connection settings
    /// </summary>
    /// <param name="configure">Configuration action</param>
    /// <returns>Builder for fluent configuration</returns>
    public RabbitMQConfigurationBuilder WithConnection(Action<ConnectionSettings> configure)
    {
        configure(_configuration.Connection);
        return this;
    }

    /// <summary>
    /// Configures connection using a connection string
    /// </summary>
    /// <param name="connectionString">AMQP connection string</param>
    /// <returns>Builder for fluent configuration</returns>
    public RabbitMQConfigurationBuilder WithConnectionString(string connectionString)
    {
        _configuration.Connection = ConnectionStringBuilder.Parse(connectionString);
        return this;
    }

    /// <summary>
    /// Adds an exchange configuration
    /// </summary>
    /// <param name="configure">Exchange configuration action</param>
    /// <returns>Builder for fluent configuration</returns>
    public RabbitMQConfigurationBuilder AddExchange(Action<ExchangeSettings> configure)
    {
        var exchange = new ExchangeSettings();
        configure(exchange);
        _configuration.Exchanges.Add(exchange);
        return this;
    }

    /// <summary>
    /// Adds a queue configuration
    /// </summary>
    /// <param name="configure">Queue configuration action</param>
    /// <returns>Builder for fluent configuration</returns>
    public RabbitMQConfigurationBuilder AddQueue(Action<QueueSettings> configure)
    {
        var queue = new QueueSettings();
        configure(queue);
        _configuration.Queues.Add(queue);
        return this;
    }

    /// <summary>
    /// Configures retry policy settings
    /// </summary>
    /// <param name="configure">Retry policy configuration action</param>
    /// <returns>Builder for fluent configuration</returns>
    public RabbitMQConfigurationBuilder WithRetryPolicy(Action<RetryPolicySettings> configure)
    {
        configure(_configuration.RetryPolicy);
        return this;
    }

    /// <summary>
    /// Configures error handling settings
    /// </summary>
    /// <param name="configure">Error handling configuration action</param>
    /// <returns>Builder for fluent configuration</returns>
    public RabbitMQConfigurationBuilder WithErrorHandling(Action<ErrorHandlingSettings> configure)
    {
        configure(_configuration.ErrorHandling);
        return this;
    }

    /// <summary>
    /// Configures event bus settings
    /// </summary>
    /// <param name="configure">Event bus configuration action</param>
    /// <returns>Builder for fluent configuration</returns>
    public RabbitMQConfigurationBuilder WithEventBus(Action<EventBusSettings> configure)
    {
        configure(_configuration.EventBus);
        return this;
    }

    /// <summary>
    /// Configures event store settings
    /// </summary>
    /// <param name="configure">Event store configuration action</param>
    /// <returns>Builder for fluent configuration</returns>
    public RabbitMQConfigurationBuilder WithEventStore(Action<EventStoreSettings> configure)
    {
        configure(_configuration.EventStore);
        return this;
    }

    /// <summary>
    /// Configures health check settings
    /// </summary>
    /// <param name="configure">Health check configuration action</param>
    /// <returns>Builder for fluent configuration</returns>
    public RabbitMQConfigurationBuilder WithHealthCheck(Action<HealthCheckSettings> configure)
    {
        configure(_configuration.HealthCheck);
        return this;
    }

    /// <summary>
    /// Enables or disables auto-declaration of exchanges
    /// </summary>
    /// <param name="enabled">Whether to auto-declare exchanges</param>
    /// <returns>Builder for fluent configuration</returns>
    public RabbitMQConfigurationBuilder AutoDeclareExchanges(bool enabled = true)
    {
        _configuration.AutoDeclareExchanges = enabled;
        return this;
    }

    /// <summary>
    /// Enables or disables auto-declaration of queues
    /// </summary>
    /// <param name="enabled">Whether to auto-declare queues</param>
    /// <returns>Builder for fluent configuration</returns>
    public RabbitMQConfigurationBuilder AutoDeclareQueues(bool enabled = true)
    {
        _configuration.AutoDeclareQueues = enabled;
        return this;
    }

    /// <summary>
    /// Enables or disables auto-recovery
    /// </summary>
    /// <param name="enabled">Whether to enable auto-recovery</param>
    /// <returns>Builder for fluent configuration</returns>
    public RabbitMQConfigurationBuilder AutoRecovery(bool enabled = true)
    {
        _configuration.AutoRecoveryEnabled = enabled;
        return this;
    }

    /// <summary>
    /// Sets the global operation timeout
    /// </summary>
    /// <param name="timeoutMs">Timeout in milliseconds</param>
    /// <returns>Builder for fluent configuration</returns>
    public RabbitMQConfigurationBuilder WithOperationTimeout(int timeoutMs)
    {
        _configuration.OperationTimeoutMs = timeoutMs;
        return this;
    }

    /// <summary>
    /// Builds and registers the configuration
    /// </summary>
    /// <returns>Service collection for further configuration</returns>
    public IServiceCollection Build()
    {
        _configuration.Validate();
        _services.Configure<RabbitMQConfiguration>(config =>
        {
            config.Connection = _configuration.Connection;
            config.Exchanges.Clear();
            config.Exchanges.AddRange(_configuration.Exchanges);
            config.Queues.Clear();
            config.Queues.AddRange(_configuration.Queues);
            config.RetryPolicy = _configuration.RetryPolicy;
            config.ErrorHandling = _configuration.ErrorHandling;
            config.EventBus = _configuration.EventBus;
            config.EventStore = _configuration.EventStore;
            config.HealthCheck = _configuration.HealthCheck;
            config.AutoDeclareExchanges = _configuration.AutoDeclareExchanges;
            config.AutoDeclareQueues = _configuration.AutoDeclareQueues;
            config.AutoRecoveryEnabled = _configuration.AutoRecoveryEnabled;
            config.OperationTimeoutMs = _configuration.OperationTimeoutMs;
        });

        return _services;
    }

    /// <summary>
    /// Gets the current configuration (for testing or inspection)
    /// </summary>
    /// <returns>Current configuration instance</returns>
    public RabbitMQConfiguration GetConfiguration()
    {
        return _configuration;
    }
}