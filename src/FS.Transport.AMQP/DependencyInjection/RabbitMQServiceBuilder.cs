using FS.Transport.AMQP.Configuration;
using FS.Transport.AMQP.Producer;
using FS.Transport.AMQP.Saga;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace FS.Transport.AMQP.DependencyInjection;

/// <summary>
/// Fluent API builder for configuring RabbitMQ services
/// </summary>
/// <remarks>
/// This builder provides a fluent interface for registering and configuring RabbitMQ services.
/// It supports comprehensive configuration of all RabbitMQ components including connections,
/// queues, exchanges, producers, consumers, event bus, monitoring, and health checks.
/// </remarks>
/// <example>
/// <code>
/// services.AddRabbitMQ()
///     .WithConnectionString("amqp://localhost")
///     .WithSerializer(SerializerType.Json)
///     .WithRetryPolicy(RetryPolicyType.ExponentialBackoff)
///     .WithHealthChecks()
///     .WithEventBus()
///     .WithSaga()
///     .WithMonitoring()
///     .Build();
/// </code>
/// </example>
public sealed class RabbitMQServiceBuilder
{
    private readonly IServiceCollection _services;
    private readonly RabbitMQConfiguration _configuration;
    private bool _built = false;

    /// <summary>
    /// Initializes a new instance of the RabbitMQServiceBuilder
    /// </summary>
    /// <param name="services">The service collection to register services with</param>
    internal RabbitMQServiceBuilder(IServiceCollection services)
    {
        _services = services ?? throw new ArgumentNullException(nameof(services));
        _configuration = new RabbitMQConfiguration();
    }

    /// <summary>
    /// Configures the RabbitMQ connection string
    /// </summary>
    /// <param name="connectionString">The RabbitMQ connection string</param>
    /// <returns>The builder for fluent configuration</returns>
    /// <exception cref="ArgumentException">Thrown when connection string is null or empty</exception>
    /// <example>
    /// <code>
    /// builder.WithConnectionString("amqp://guest:guest@localhost:5672/");
    /// </code>
    /// </example>
    public RabbitMQServiceBuilder WithConnectionString(string connectionString)
    {
        if (string.IsNullOrWhiteSpace(connectionString))
            throw new ArgumentException("Connection string cannot be null or empty", nameof(connectionString));

        _configuration.Connection.ConnectionString = connectionString;
        return this;
    }

    /// <summary>
    /// Configures the RabbitMQ connection with detailed settings
    /// </summary>
    /// <param name="configure">Action to configure connection settings</param>
    /// <returns>The builder for fluent configuration</returns>
    /// <exception cref="ArgumentNullException">Thrown when configure action is null</exception>
    /// <example>
    /// <code>
    /// builder.WithConnection(conn => 
    /// {
    ///     conn.HostName = "localhost";
    ///     conn.Port = 5672;
    ///     conn.UserName = "guest";
    ///     conn.Password = "guest";
    ///     conn.VirtualHost = "/";
    ///     conn.RequestedHeartbeat = TimeSpan.FromMinutes(1);
    /// });
    /// </code>
    /// </example>
    public RabbitMQServiceBuilder WithConnection(Action<ConnectionSettings> configure)
    {
        ArgumentNullException.ThrowIfNull(configure);
        configure(_configuration.Connection);
        return this;
    }

    /// <summary>
    /// Configures the message serializer
    /// </summary>
    /// <param name="serializerType">The type of serializer to use</param>
    /// <returns>The builder for fluent configuration</returns>
    /// <example>
    /// <code>
    /// builder.WithSerializer(SerializerType.Json);
    /// builder.WithSerializer(SerializerType.Binary);
    /// </code>
    /// </example>
    public RabbitMQServiceBuilder WithSerializer(SerializerType serializerType)
    {
        _configuration.Serialization.DefaultSerializer = serializerType.ToString();
        return this;
    }

    /// <summary>
    /// Configures the retry policy
    /// </summary>
    /// <param name="retryPolicyType">The type of retry policy to use</param>
    /// <returns>The builder for fluent configuration</returns>
    /// <example>
    /// <code>
    /// builder.WithRetryPolicy(RetryPolicyType.ExponentialBackoff);
    /// builder.WithRetryPolicy(RetryPolicyType.Linear);
    /// builder.WithRetryPolicy(RetryPolicyType.CircuitBreaker);
    /// </code>
    /// </example>
    public RabbitMQServiceBuilder WithRetryPolicy(RetryPolicyType retryPolicyType)
    {
        _configuration.RetryPolicy.PolicyType = retryPolicyType.ToString();
        return this;
    }

    /// <summary>
    /// Configures retry policy with detailed settings
    /// </summary>
    /// <param name="configure">Action to configure retry policy settings</param>
    /// <returns>The builder for fluent configuration</returns>
    /// <exception cref="ArgumentNullException">Thrown when configure action is null</exception>
    /// <example>
    /// <code>
    /// builder.WithRetryPolicy(retry => 
    /// {
    ///     retry.MaxRetries = 3;
    ///     retry.InitialDelay = TimeSpan.FromSeconds(1);
    ///     retry.MaxDelay = TimeSpan.FromMinutes(5);
    ///     retry.Multiplier = 2.0;
    /// });
    /// </code>
    /// </example>
    public RabbitMQServiceBuilder WithRetryPolicy(Action<RetryPolicySettings> configure)
    {
        ArgumentNullException.ThrowIfNull(configure);
        configure(_configuration.RetryPolicy);
        return this;
    }

    /// <summary>
    /// Configures producer settings
    /// </summary>
    /// <param name="configure">Action to configure producer settings</param>
    /// <returns>The builder for fluent configuration</returns>
    /// <exception cref="ArgumentNullException">Thrown when configure action is null</exception>
    /// <example>
    /// <code>
    /// builder.WithProducer(producer => 
    /// {
    ///     producer.EnableConfirmations = true;
    ///     producer.ConfirmationTimeout = TimeSpan.FromSeconds(30);
    ///     producer.EnableMandatory = true;
    ///     producer.EnablePersistent = true;
    /// });
    /// </code>
    /// </example>
    public RabbitMQServiceBuilder WithProducer(Action<ProducerSettings> configure)
    {
        ArgumentNullException.ThrowIfNull(configure);
        configure(_configuration.Producer);
        return this;
    }

    /// <summary>
    /// Configures consumer settings
    /// </summary>
    /// <param name="configure">Action to configure consumer settings</param>
    /// <returns>The builder for fluent configuration</returns>
    /// <exception cref="ArgumentNullException">Thrown when configure action is null</exception>
    /// <example>
    /// <code>
    /// builder.WithConsumer(consumer => 
    /// {
    ///     consumer.PrefetchCount = 10;
    ///     consumer.AutoAck = false;
    ///     consumer.ConcurrentConsumers = 1;
    ///     consumer.RetryOnFailure = true;
    /// });
    /// </code>
    /// </example>
    public RabbitMQServiceBuilder WithConsumer(Action<Configuration.ConsumerSettings> configure)
    {
        ArgumentNullException.ThrowIfNull(configure);
        configure(_configuration.Consumer);
        return this;
    }

    /// <summary>
    /// Configures event bus settings
    /// </summary>
    /// <param name="configure">Action to configure event bus settings</param>
    /// <returns>The builder for fluent configuration</returns>
    /// <exception cref="ArgumentNullException">Thrown when configure action is null</exception>
    /// <example>
    /// <code>
    /// builder.WithEventBus(eventBus => 
    /// {
    ///     eventBus.DefaultExchange = "events";
    ///     eventBus.EnableEventStore = true;
    ///     eventBus.EnableDomainEvents = true;
    ///     eventBus.EnableIntegrationEvents = true;
    /// });
    /// </code>
    /// </example>
    public RabbitMQServiceBuilder WithEventBus(Action<Configuration.EventBusSettings> configure)
    {
        ArgumentNullException.ThrowIfNull(configure);
        configure(_configuration.EventBus);
        return this;
    }

    /// <summary>
    /// Enables event bus with default settings
    /// </summary>
    /// <returns>The builder for fluent configuration</returns>
    /// <example>
    /// <code>
    /// builder.WithEventBus();
    /// </code>
    /// </example>
    public RabbitMQServiceBuilder WithEventBus()
    {
        _configuration.EventBus.Enabled = true;
        return this;
    }

    /// <summary>
    /// Configures event store settings
    /// </summary>
    /// <param name="configure">Action to configure event store settings</param>
    /// <returns>The builder for fluent configuration</returns>
    /// <exception cref="ArgumentNullException">Thrown when configure action is null</exception>
    /// <example>
    /// <code>
    /// builder.WithEventStore(eventStore => 
    /// {
    ///     eventStore.StreamPrefix = "stream-";
    ///     eventStore.SnapshotFrequency = 100;
    ///     eventStore.EnableSnapshots = true;
    /// });
    /// </code>
    /// </example>
    public RabbitMQServiceBuilder WithEventStore(Action<Configuration.EventStoreSettings> configure)
    {
        ArgumentNullException.ThrowIfNull(configure);
        configure(_configuration.EventStore);
        return this;
    }

    /// <summary>
    /// Enables event store with default settings
    /// </summary>
    /// <returns>The builder for fluent configuration</returns>
    /// <example>
    /// <code>
    /// builder.WithEventStore();
    /// </code>
    /// </example>
    public RabbitMQServiceBuilder WithEventStore()
    {
        _configuration.EventStore.Enabled = true;
        return this;
    }

    /// <summary>
    /// Configures saga orchestration settings
    /// </summary>
    /// <param name="configure">Action to configure saga settings</param>
    /// <returns>The builder for fluent configuration</returns>
    /// <exception cref="ArgumentNullException">Thrown when configure action is null</exception>
    /// <example>
    /// <code>
    /// builder.WithSaga(saga => 
    /// {
    ///     saga.EnableCompensation = true;
    ///     saga.TimeoutDuration = TimeSpan.FromMinutes(30);
    ///     saga.MaxRetries = 3;
    /// });
    /// </code>
    /// </example>
    public RabbitMQServiceBuilder WithSaga(Action<SagaSettings> configure)
    {
        ArgumentNullException.ThrowIfNull(configure);
        configure(_configuration.Saga);
        return this;
    }

    /// <summary>
    /// Enables saga orchestration with default settings
    /// </summary>
    /// <returns>The builder for fluent configuration</returns>
    /// <example>
    /// <code>
    /// builder.WithSaga();
    /// </code>
    /// </example>
    public RabbitMQServiceBuilder WithSaga()
    {
        _configuration.Saga.Enabled = true;
        return this;
    }

    /// <summary>
    /// Configures monitoring settings
    /// </summary>
    /// <param name="configure">Action to configure monitoring settings</param>
    /// <returns>The builder for fluent configuration</returns>
    /// <exception cref="ArgumentNullException">Thrown when configure action is null</exception>
    /// <example>
    /// <code>
    /// builder.WithMonitoring(monitoring => 
    /// {
    ///     monitoring.Enabled = true;
    ///     monitoring.CheckInterval = TimeSpan.FromSeconds(30);
    ///     monitoring.Timeout = TimeSpan.FromSeconds(30);
    /// });
    /// </code>
    /// </example>
    public RabbitMQServiceBuilder WithMonitoring(Action<HealthCheckSettings> configure)
    {
        ArgumentNullException.ThrowIfNull(configure);
        configure(_configuration.HealthCheck);
        return this;
    }

    /// <summary>
    /// Enables monitoring with default settings
    /// </summary>
    /// <returns>The builder for fluent configuration</returns>
    /// <example>
    /// <code>
    /// builder.WithMonitoring();
    /// </code>
    /// </example>
    public RabbitMQServiceBuilder WithMonitoring()
    {
        _configuration.Monitoring.Enabled = true;
        return this;
    }

    /// <summary>
    /// Configures health check settings
    /// </summary>
    /// <param name="configure">Action to configure health check settings</param>
    /// <returns>The builder for fluent configuration</returns>
    /// <exception cref="ArgumentNullException">Thrown when configure action is null</exception>
    /// <example>
    /// <code>
    /// builder.WithHealthChecks(health => 
    /// {
    ///     health.Enabled = true;
    ///     health.CheckInterval = TimeSpan.FromMinutes(1);
    ///     health.Timeout = TimeSpan.FromSeconds(30);
    ///     health.FailureStatus = HealthStatus.Unhealthy;
    /// });
    /// </code>
    /// </example>
    public RabbitMQServiceBuilder WithHealthChecks(Action<HealthCheckSettings> configure)
    {
        ArgumentNullException.ThrowIfNull(configure);
        configure(_configuration.HealthCheck);
        return this;
    }

    /// <summary>
    /// Enables health checks with default settings
    /// </summary>
    /// <returns>The builder for fluent configuration</returns>
    /// <example>
    /// <code>
    /// builder.WithHealthChecks();
    /// </code>
    /// </example>
    public RabbitMQServiceBuilder WithHealthChecks()
    {
        _configuration.HealthCheck.Enabled = true;
        return this;
    }

    /// <summary>
    /// Configures error handling settings
    /// </summary>
    /// <param name="configure">Action to configure error handling settings</param>
    /// <returns>The builder for fluent configuration</returns>
    /// <exception cref="ArgumentNullException">Thrown when configure action is null</exception>
    /// <example>
    /// <code>
    /// builder.WithErrorHandling(error => 
    /// {
    ///     error.Strategy = ErrorHandlingStrategy.DeadLetter;
    ///     error.MaxRetries = 3;
    ///     error.RetryDelay = TimeSpan.FromSeconds(5);
    /// });
    /// </code>
    /// </example>
    public RabbitMQServiceBuilder WithErrorHandling(Action<ErrorHandlingSettings> configure)
    {
        ArgumentNullException.ThrowIfNull(configure);
        configure(_configuration.ErrorHandling);
        return this;
    }

    /// <summary>
    /// Configures the builder for development environment
    /// </summary>
    /// <returns>The builder for fluent configuration</returns>
    /// <remarks>
    /// This method configures the builder with settings optimized for development,
    /// including verbose logging, extended timeouts, and development-friendly defaults.
    /// </remarks>
    /// <example>
    /// <code>
    /// builder.ForDevelopment();
    /// </code>
    /// </example>
    public RabbitMQServiceBuilder ForDevelopment()
    {
        _configuration.Connection.RequestedHeartbeat = TimeSpan.FromMinutes(10);
        _configuration.Connection.ConnectionTimeout = TimeSpan.FromMinutes(1);
        _configuration.Producer.ConfirmationTimeout = TimeSpan.FromMinutes(1);
        _configuration.Consumer.PrefetchCount = 1;
        _configuration.RetryPolicy.MaxRetries = 3;
        _configuration.HealthCheck.Enabled = true;
        _configuration.Monitoring.Enabled = true;
        return this;
    }

    /// <summary>
    /// Configures the builder for production environment
    /// </summary>
    /// <returns>The builder for fluent configuration</returns>
    /// <remarks>
    /// This method configures the builder with settings optimized for production,
    /// including performance optimizations, appropriate timeouts, and production-safe defaults.
    /// </remarks>
    /// <example>
    /// <code>
    /// builder.ForProduction();
    /// </code>
    /// </example>
    public RabbitMQServiceBuilder ForProduction()
    {
        _configuration.Connection.RequestedHeartbeat = TimeSpan.FromMinutes(1);
        _configuration.Connection.ConnectionTimeout = TimeSpan.FromSeconds(30);
        _configuration.Producer.ConfirmationTimeout = TimeSpan.FromSeconds(30);
        _configuration.Consumer.PrefetchCount = 10;
        _configuration.RetryPolicy.MaxRetries = 5;
        _configuration.HealthCheck.Enabled = true;
        _configuration.Monitoring.Enabled = true;
        _configuration.ErrorHandling.Strategy = ErrorHandling.ErrorHandlingStrategy.DeadLetter.ToString();
        return this;
    }

    /// <summary>
    /// Configures the builder for testing environment
    /// </summary>
    /// <returns>The builder for fluent configuration</returns>
    /// <remarks>
    /// This method configures the builder with settings optimized for testing,
    /// including fast timeouts, minimal retries, and test-friendly defaults.
    /// </remarks>
    /// <example>
    /// <code>
    /// builder.ForTesting();
    /// </code>
    /// </example>
    public RabbitMQServiceBuilder ForTesting()
    {
        _configuration.Connection.RequestedHeartbeat = TimeSpan.FromSeconds(30);
        _configuration.Connection.ConnectionTimeout = TimeSpan.FromSeconds(10);
        _configuration.Producer.ConfirmationTimeout = TimeSpan.FromSeconds(5);
        _configuration.Consumer.PrefetchCount = 1;
        _configuration.RetryPolicy.MaxRetries = 1;
        _configuration.HealthCheck.Enabled = false;
        _configuration.Monitoring.Enabled = false;
        return this;
    }

    /// <summary>
    /// Configures services from an IConfiguration instance
    /// </summary>
    /// <param name="configuration">The configuration instance</param>
    /// <param name="sectionKey">The configuration section key (default: "RabbitMQ")</param>
    /// <returns>The builder for fluent configuration</returns>
    /// <exception cref="ArgumentNullException">Thrown when configuration is null</exception>
    /// <example>
    /// <code>
    /// builder.FromConfiguration(configuration, "MessageBroker:RabbitMQ");
    /// </code>
    /// </example>
    public RabbitMQServiceBuilder FromConfiguration(IConfiguration configuration, string sectionKey = "RabbitMQ")
    {
        ArgumentNullException.ThrowIfNull(configuration);
        
        var section = configuration.GetSection(sectionKey);
        section.Bind(_configuration);
        
        return this;
    }

    /// <summary>
    /// Builds and registers all configured RabbitMQ services
    /// </summary>
    /// <returns>The service collection for further configuration</returns>
    /// <exception cref="InvalidOperationException">Thrown when the builder has already been built</exception>
    /// <remarks>
    /// This method finalizes the configuration and registers all services with the DI container.
    /// Once called, the builder cannot be used again.
    /// </remarks>
    /// <example>
    /// <code>
    /// var services = builder.Build();
    /// </code>
    /// </example>
    public IServiceCollection Build()
    {
        if (_built)
            throw new InvalidOperationException("Builder has already been built and cannot be reused.");

        _built = true;

        // Register configuration
        _services.Configure<RabbitMQConfiguration>(config =>
        {
            config.Connection = _configuration.Connection;
            config.Producer = _configuration.Producer;
            config.Consumer = _configuration.Consumer;
            config.EventBus = _configuration.EventBus;
            config.EventStore = _configuration.EventStore;
            config.Saga = _configuration.Saga;
            config.Monitoring = _configuration.Monitoring;
            config.HealthCheck = _configuration.HealthCheck;
            config.ErrorHandling = _configuration.ErrorHandling;
            config.RetryPolicy = _configuration.RetryPolicy;
            config.Serialization = _configuration.Serialization;
        });

        // Register core services
        ServiceRegistrar.RegisterCoreServices(_services);
        ServiceRegistrar.RegisterSerializationServices(_services);
        ServiceRegistrar.RegisterErrorHandlingServices(_services);
        ServiceRegistrar.RegisterCQRSHandlers(_services);

        // Register optional services based on configuration
        if (_configuration.EventBus.Enabled)
        {
            ServiceRegistrar.RegisterEventBusServices(_services);
        }

        if (_configuration.EventStore.Enabled)
        {
            ServiceRegistrar.RegisterEventStoreServices(_services);
        }

        if (_configuration.Saga.Enabled)
        {
            ServiceRegistrar.RegisterSagaServices(_services);
        }

        if (_configuration.Monitoring.Enabled)
        {
            ServiceRegistrar.RegisterMonitoringServices(_services);
        }

        if (_configuration.HealthCheck.Enabled)
        {
            ServiceRegistrar.RegisterHealthCheckServices(_services);
        }

        return _services;
    }
}

/// <summary>
/// Enumeration of available serializer types
/// </summary>
public enum SerializerType
{
    /// <summary>
    /// JSON serialization using System.Text.Json
    /// </summary>
    Json,
    
    /// <summary>
    /// Binary serialization
    /// </summary>
    Binary
}

/// <summary>
/// Enumeration of available retry policy types
/// </summary>
public enum RetryPolicyType
{
    /// <summary>
    /// No retry policy
    /// </summary>
    None,
    
    /// <summary>
    /// Linear retry with fixed delays
    /// </summary>
    Linear,
    
    /// <summary>
    /// Exponential backoff retry
    /// </summary>
    ExponentialBackoff,
    
    /// <summary>
    /// Circuit breaker pattern
    /// </summary>
    CircuitBreaker
} 