using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System.Reflection;
using FS.RabbitMQ.Configuration;
using FS.RabbitMQ.EventBus;
using FS.RabbitMQ.EventStore;
using FS.RabbitMQ.Saga;

namespace FS.RabbitMQ.DependencyInjection;

/// <summary>
/// Extension methods for IServiceCollection to register RabbitMQ services
/// </summary>
/// <remarks>
/// This class provides various methods for registering RabbitMQ services with different
/// configuration approaches including simple registration, fluent API, configuration-based,
/// and environment-specific registrations. It also provides compatibility with existing
/// registration methods and advanced features like automatic handler discovery.
/// </remarks>
public static class RabbitMQServiceCollectionExtensions
{
    #region Main Registration Methods

    /// <summary>
    /// Adds RabbitMQ services with fluent configuration API
    /// </summary>
    /// <param name="services">The service collection</param>
    /// <returns>RabbitMQ service builder for fluent configuration</returns>
    /// <remarks>
    /// This is the primary method for configuring RabbitMQ services using the fluent API.
    /// It provides a comprehensive builder pattern for configuring all aspects of RabbitMQ
    /// including connections, queues, exchanges, producers, consumers, event bus, monitoring,
    /// and health checks.
    /// </remarks>
    /// <example>
    /// <code>
    /// services.AddRabbitMQ()
    ///     .WithConnectionString("amqp://localhost")
    ///     .WithSerializer(SerializerType.Json)
    ///     .WithRetryPolicy(RetryPolicyType.ExponentialBackoff)
    ///     .WithHealthChecks()
    ///     .WithEventBus()
    ///     .WithMonitoring()
    ///     .ForProduction()
    ///     .Build();
    /// </code>
    /// </example>
    public static RabbitMQServiceBuilder AddRabbitMQ(this IServiceCollection services)
    {
        ArgumentNullException.ThrowIfNull(services);
        return new RabbitMQServiceBuilder(services);
    }

    /// <summary>
    /// Adds RabbitMQ services with connection string
    /// </summary>
    /// <param name="services">The service collection</param>
    /// <param name="connectionString">RabbitMQ connection string</param>
    /// <returns>The service collection for fluent configuration</returns>
    /// <exception cref="ArgumentException">Thrown when connection string is null or empty</exception>
    /// <remarks>
    /// This method provides a simple way to register RabbitMQ services with just a connection string.
    /// It uses default settings for all other configuration options and is suitable for simple
    /// scenarios where minimal configuration is needed.
    /// </remarks>
    /// <example>
    /// <code>
    /// services.AddRabbitMQ("amqp://guest:guest@localhost:5672/");
    /// </code>
    /// </example>
    public static IServiceCollection AddRabbitMQ(this IServiceCollection services, string connectionString)
    {
        if (string.IsNullOrWhiteSpace(connectionString))
            throw new ArgumentException("Connection string cannot be null or empty", nameof(connectionString));

        return services.AddRabbitMQ()
            .WithConnectionString(connectionString)
            .Build();
    }

    /// <summary>
    /// Adds RabbitMQ services with configuration action
    /// </summary>
    /// <param name="services">The service collection</param>
    /// <param name="configure">Action to configure RabbitMQ settings</param>
    /// <returns>The service collection for fluent configuration</returns>
    /// <exception cref="ArgumentNullException">Thrown when configure action is null</exception>
    /// <remarks>
    /// This method allows for detailed configuration of RabbitMQ services using a configuration action.
    /// It provides access to all configuration options while maintaining a simple API surface.
    /// </remarks>
    /// <example>
    /// <code>
    /// services.AddRabbitMQ(config =>
    /// {
    ///     config.Connection.ConnectionString = "amqp://localhost";
    ///     config.Producer.EnableConfirmations = true;
    ///     config.Consumer.PrefetchCount = 10;
    ///     config.EventBus.Enabled = true;
    ///     config.HealthCheck.Enabled = true;
    /// });
    /// </code>
    /// </example>
    public static IServiceCollection AddRabbitMQ(this IServiceCollection services, Action<RabbitMQConfiguration> configure)
    {
        ArgumentNullException.ThrowIfNull(configure);

        var configuration = new RabbitMQConfiguration();
        configure(configuration);

        services.Configure<RabbitMQConfiguration>(config =>
        {
            config.Connection = configuration.Connection;
            config.Producer = configuration.Producer;
            config.Consumer = configuration.Consumer;
            config.EventBus = configuration.EventBus;
            config.EventStore = configuration.EventStore;
            config.Saga = configuration.Saga;
            config.Monitoring = configuration.Monitoring;
            config.HealthCheck = configuration.HealthCheck;
            config.ErrorHandling = configuration.ErrorHandling;
            config.RetryPolicy = configuration.RetryPolicy;
            config.Serialization = configuration.Serialization;
        });

        return RegisterAllServices(services, configuration);
    }

    /// <summary>
    /// Adds RabbitMQ services from IConfiguration
    /// </summary>
    /// <param name="services">The service collection</param>
    /// <param name="configuration">Configuration instance</param>
    /// <param name="sectionKey">Configuration section key (default: "RabbitMQ")</param>
    /// <returns>The service collection for fluent configuration</returns>
    /// <exception cref="ArgumentNullException">Thrown when configuration is null</exception>
    /// <remarks>
    /// This method registers RabbitMQ services using configuration from appsettings.json or other
    /// configuration sources. It supports the standard .NET configuration system and allows
    /// for environment-specific configurations.
    /// </remarks>
    /// <example>
    /// <code>
    /// services.AddRabbitMQ(configuration, "MessageBroker:RabbitMQ");
    /// </code>
    /// </example>
    public static IServiceCollection AddRabbitMQ(this IServiceCollection services, IConfiguration configuration, string sectionKey = "RabbitMQ")
    {
        ArgumentNullException.ThrowIfNull(configuration);

        return services.AddRabbitMQ()
            .FromConfiguration(configuration, sectionKey)
            .Build();
    }

    #endregion

    #region Environment-Specific Registration Methods

    /// <summary>
    /// Adds RabbitMQ services configured for development environment
    /// </summary>
    /// <param name="services">The service collection</param>
    /// <param name="connectionString">RabbitMQ connection string</param>
    /// <returns>The service collection for fluent configuration</returns>
    /// <remarks>
    /// This method configures RabbitMQ services with development-friendly settings including
    /// verbose logging, extended timeouts, single consumer prefetch, and enabled monitoring.
    /// </remarks>
    /// <example>
    /// <code>
    /// services.AddRabbitMQForDevelopment("amqp://localhost");
    /// </code>
    /// </example>
    public static IServiceCollection AddRabbitMQForDevelopment(this IServiceCollection services, string connectionString)
    {
        return services.AddRabbitMQ()
            .WithConnectionString(connectionString)
            .ForDevelopment()
            .WithHealthChecks()
            .WithMonitoring()
            .WithEventBus()
            .Build();
    }

    /// <summary>
    /// Adds RabbitMQ services configured for production environment
    /// </summary>
    /// <param name="services">The service collection</param>
    /// <param name="connectionString">RabbitMQ connection string</param>
    /// <returns>The service collection for fluent configuration</returns>
    /// <remarks>
    /// This method configures RabbitMQ services with production-optimized settings including
    /// performance optimizations, appropriate timeouts, multiple consumer prefetch, circuit breaker
    /// retry policy, and comprehensive monitoring and health checks.
    /// </remarks>
    /// <example>
    /// <code>
    /// services.AddRabbitMQForProduction("amqp://production-server");
    /// </code>
    /// </example>
    public static IServiceCollection AddRabbitMQForProduction(this IServiceCollection services, string connectionString)
    {
        return services.AddRabbitMQ()
            .WithConnectionString(connectionString)
            .ForProduction()
            .WithHealthChecks()
            .WithMonitoring()
            .WithEventBus()
            .WithEventStore()
            .WithSaga()
            .WithRetryPolicy(RetryPolicyType.ExponentialBackoff)
            .Build();
    }

    /// <summary>
    /// Adds RabbitMQ services configured for testing environment
    /// </summary>
    /// <param name="services">The service collection</param>
    /// <param name="connectionString">RabbitMQ connection string</param>
    /// <returns>The service collection for fluent configuration</returns>
    /// <remarks>
    /// This method configures RabbitMQ services with testing-friendly settings including
    /// fast timeouts, minimal retries, single consumer prefetch, and disabled monitoring
    /// to improve test performance.
    /// </remarks>
    /// <example>
    /// <code>
    /// services.AddRabbitMQForTesting("amqp://localhost");
    /// </code>
    /// </example>
    public static IServiceCollection AddRabbitMQForTesting(this IServiceCollection services, string connectionString)
    {
        return services.AddRabbitMQ()
            .WithConnectionString(connectionString)
            .ForTesting()
            .Build();
    }

    /// <summary>
    /// Adds RabbitMQ services with environment-specific configuration
    /// </summary>
    /// <param name="services">The service collection</param>
    /// <param name="environment">Hosting environment</param>
    /// <param name="connectionString">RabbitMQ connection string</param>
    /// <returns>The service collection for fluent configuration</returns>
    /// <exception cref="ArgumentNullException">Thrown when environment is null</exception>
    /// <remarks>
    /// This method automatically configures RabbitMQ services based on the hosting environment.
    /// It applies environment-specific optimizations and feature sets automatically.
    /// </remarks>
    /// <example>
    /// <code>
    /// services.AddRabbitMQForEnvironment(environment, "amqp://localhost");
    /// </code>
    /// </example>
    public static IServiceCollection AddRabbitMQForEnvironment(this IServiceCollection services, IHostEnvironment environment, string connectionString)
    {
        ArgumentNullException.ThrowIfNull(environment);

        if (environment.IsDevelopment())
        {
            return services.AddRabbitMQForDevelopment(connectionString);
        }
        else if (environment.IsProduction())
        {
            return services.AddRabbitMQForProduction(connectionString);
        }
        else
        {
            // Default to testing configuration for other environments
            return services.AddRabbitMQForTesting(connectionString);
        }
    }

    #endregion

    #region Component-Specific Registration Methods

    /// <summary>
    /// Adds RabbitMQ CQRS handlers to the service collection
    /// </summary>
    /// <param name="services">The service collection</param>
    /// <returns>The service collection for fluent configuration</returns>
    /// <remarks>
    /// This method registers all CQRS command and query handlers for RabbitMQ operations
    /// including queue management, exchange management, and connection management handlers.
    /// </remarks>
    /// <example>
    /// <code>
    /// services.AddRabbitMQCQRS();
    /// </code>
    /// </example>
    public static IServiceCollection AddRabbitMQCQRS(this IServiceCollection services)
    {
        return ServiceRegistrar.RegisterCQRSHandlers(services);
    }

    /// <summary>
    /// Adds RabbitMQ event bus services to the service collection
    /// </summary>
    /// <param name="services">The service collection</param>
    /// <param name="configure">Optional action to configure event bus settings</param>
    /// <returns>The service collection for fluent configuration</returns>
    /// <remarks>
    /// This method registers services required for event-driven architecture including
    /// event bus, event publishers, event subscribers, and event handlers.
    /// </remarks>
    /// <example>
    /// <code>
    /// services.AddRabbitMQEventBus(eventBus =>
    /// {
    ///     eventBus.DefaultExchange = "events";
    ///     eventBus.EnableEventStore = true;
    ///     eventBus.EnableDomainEvents = true;
    /// });
    /// </code>
    /// </example>
    public static IServiceCollection AddRabbitMQEventBus(this IServiceCollection services, Action<EventBusSettings>? configure = null)
    {
        services.Configure<EventBusSettings>(eventBus =>
        {
            eventBus.Enabled = true;
            configure?.Invoke(eventBus);
        });

        return ServiceRegistrar.RegisterEventBusServices(services);
    }

    /// <summary>
    /// Adds RabbitMQ event store services to the service collection
    /// </summary>
    /// <param name="services">The service collection</param>
    /// <param name="configure">Optional action to configure event store settings</param>
    /// <returns>The service collection for fluent configuration</returns>
    /// <remarks>
    /// This method registers services required for event sourcing including
    /// event store, event streams, and event snapshots.
    /// </remarks>
    /// <example>
    /// <code>
    /// services.AddRabbitMQEventStore(eventStore =>
    /// {
    ///     eventStore.StreamPrefix = "stream-";
    ///     eventStore.SnapshotFrequency = 100;
    ///     eventStore.EnableSnapshots = true;
    /// });
    /// </code>
    /// </example>
    public static IServiceCollection AddRabbitMQEventStore(this IServiceCollection services, Action<EventStoreSettings>? configure = null)
    {
        services.Configure<EventStoreSettings>(eventStore =>
        {
            eventStore.Enabled = true;
            configure?.Invoke(eventStore);
        });

        return ServiceRegistrar.RegisterEventStoreServices(services);
    }

    /// <summary>
    /// Adds RabbitMQ saga orchestration services to the service collection
    /// </summary>
    /// <param name="services">The service collection</param>
    /// <param name="configure">Optional action to configure saga settings</param>
    /// <returns>The service collection for fluent configuration</returns>
    /// <remarks>
    /// This method registers services required for saga orchestration including
    /// saga orchestrator, saga state management, and saga compensation handling.
    /// </remarks>
    /// <example>
    /// <code>
    /// services.AddRabbitMQSaga(saga =>
    /// {
    ///     saga.EnableCompensation = true;
    ///     saga.TimeoutDuration = TimeSpan.FromMinutes(30);
    ///     saga.MaxRetries = 3;
    /// });
    /// </code>
    /// </example>
    public static IServiceCollection AddRabbitMQSaga(this IServiceCollection services, Action<SagaSettings>? configure = null)
    {
        services.Configure<SagaSettings>(saga =>
        {
            saga.Enabled = true;
            configure?.Invoke(saga);
        });

        return ServiceRegistrar.RegisterSagaServices(services);
    }

    /// <summary>
    /// Adds RabbitMQ monitoring services to the service collection
    /// </summary>
    /// <param name="services">The service collection</param>
    /// <param name="configure">Optional action to configure monitoring settings</param>
    /// <returns>The service collection for fluent configuration</returns>
    /// <remarks>
    /// This method registers services required for monitoring including
    /// metrics collection, performance monitoring, and health checking.
    /// </remarks>
    /// <example>
    /// <code>
    /// services.AddRabbitMQMonitoring(health =>
    /// {
    ///     health.Enabled = true;
    ///     health.CheckInterval = TimeSpan.FromSeconds(30);
    ///     health.Timeout = TimeSpan.FromSeconds(30);
    /// });
    /// </code>
    /// </example>
    public static IServiceCollection AddRabbitMQMonitoring(this IServiceCollection services, Action<HealthCheckSettings>? configure = null)
    {
        services.Configure<HealthCheckSettings>(health =>
        {
            health.Enabled = true;
            configure?.Invoke(health);
        });

        return ServiceRegistrar.RegisterMonitoringServices(services);
    }

    /// <summary>
    /// Adds RabbitMQ health check services to the service collection
    /// </summary>
    /// <param name="services">The service collection</param>
    /// <param name="name">Health check name</param>
    /// <param name="tags">Health check tags</param>
    /// <returns>The service collection for fluent configuration</returns>
    /// <remarks>
    /// This method registers RabbitMQ health check services that monitor the health
    /// of RabbitMQ connections, queues, exchanges, and message processing.
    /// </remarks>
    /// <example>
    /// <code>
    /// services.AddRabbitMQHealthChecks("rabbitmq", "messaging", "infrastructure");
    /// </code>
    /// </example>
    public static IServiceCollection AddRabbitMQHealthChecks(this IServiceCollection services, string name = "rabbitmq", params string[] tags)
    {
        return ServiceRegistrar.RegisterHealthCheckServices(services, name, tags: tags);
    }

    #endregion

    #region Handler Registration Methods

    /// <summary>
    /// Registers RabbitMQ handlers from the specified assemblies
    /// </summary>
    /// <param name="services">The service collection</param>
    /// <param name="assemblies">Assemblies to scan for handlers</param>
    /// <returns>The service collection for fluent configuration</returns>
    /// <remarks>
    /// This method automatically discovers and registers all CQRS handlers from the specified
    /// assemblies. It scans for all classes that implement IRequestHandler interface.
    /// </remarks>
    /// <example>
    /// <code>
    /// services.AddRabbitMQHandlersFromAssembly(typeof(Program).Assembly, typeof(Handlers.QueueHandler).Assembly);
    /// </code>
    /// </example>
    public static IServiceCollection AddRabbitMQHandlersFromAssembly(this IServiceCollection services, params Assembly[] assemblies)
    {
        return ServiceRegistrar.RegisterHandlersFromAssembly(services, assemblies);
    }

    /// <summary>
    /// Registers RabbitMQ handlers from the calling assembly
    /// </summary>
    /// <param name="services">The service collection</param>
    /// <returns>The service collection for fluent configuration</returns>
    /// <remarks>
    /// This method automatically discovers and registers all CQRS handlers from the calling assembly.
    /// It's a convenience method for the common scenario of registering handlers from the current assembly.
    /// </remarks>
    /// <example>
    /// <code>
    /// services.AddRabbitMQHandlersFromCallingAssembly();
    /// </code>
    /// </example>
    public static IServiceCollection AddRabbitMQHandlersFromCallingAssembly(this IServiceCollection services)
    {
        return ServiceRegistrar.RegisterHandlersFromAssembly(services, Assembly.GetCallingAssembly());
    }

    /// <summary>
    /// Registers RabbitMQ handlers from the executing assembly
    /// </summary>
    /// <param name="services">The service collection</param>
    /// <returns>The service collection for fluent configuration</returns>
    /// <remarks>
    /// This method automatically discovers and registers all CQRS handlers from the executing assembly.
    /// It's useful when handlers are defined in the same assembly as the DI registration.
    /// </remarks>
    /// <example>
    /// <code>
    /// services.AddRabbitMQHandlersFromExecutingAssembly();
    /// </code>
    /// </example>
    public static IServiceCollection AddRabbitMQHandlersFromExecutingAssembly(this IServiceCollection services)
    {
        return ServiceRegistrar.RegisterHandlersFromAssembly(services, Assembly.GetExecutingAssembly());
    }

    #endregion

    #region Validation Methods

    /// <summary>
    /// Validates RabbitMQ service registration
    /// </summary>
    /// <param name="services">The service collection</param>
    /// <param name="throwOnValidationFailure">Whether to throw exception on validation failure</param>
    /// <returns>The service collection for fluent configuration</returns>
    /// <exception cref="InvalidOperationException">Thrown when validation fails and throwOnValidationFailure is true</exception>
    /// <remarks>
    /// This method validates that all required RabbitMQ services are properly registered
    /// and configured. It checks for missing dependencies and configuration issues.
    /// </remarks>
    /// <example>
    /// <code>
    /// services.AddRabbitMQ("amqp://localhost")
    ///     .ValidateRabbitMQServices();
    /// </code>
    /// </example>
    public static IServiceCollection ValidateRabbitMQServices(this IServiceCollection services, bool throwOnValidationFailure = true)
    {
        var errors = ServiceRegistrar.ValidateServices(services);

        if (errors.Any() && throwOnValidationFailure)
        {
            throw new InvalidOperationException($"RabbitMQ service validation failed: {string.Join(", ", errors)}");
        }

        return services;
    }

    #endregion

    #region Legacy Compatibility Methods

    /// <summary>
    /// Adds RabbitMQ services using the legacy Core.Extensions method
    /// </summary>
    /// <param name="services">The service collection</param>
    /// <param name="configuration">Configuration instance</param>
    /// <returns>The service collection for fluent configuration</returns>
    /// <remarks>
    /// This method provides backward compatibility with the existing Core.Extensions.AddRabbitMQ method.
    /// It delegates to the existing implementation to ensure compatibility.
    /// </remarks>
    /// <example>
    /// <code>
    /// services.AddRabbitMQLegacy(configuration);
    /// </code>
    /// </example>
    public static IServiceCollection AddRabbitMQLegacy(this IServiceCollection services, IConfiguration configuration)
    {
        return services.AddRabbitMQ(configuration);
    }

    /// <summary>
    /// Adds RabbitMQ services using the legacy Core.Extensions method with configuration action
    /// </summary>
    /// <param name="services">The service collection</param>
    /// <param name="configureOptions">Configuration action</param>
    /// <returns>The service collection for fluent configuration</returns>
    /// <remarks>
    /// This method provides backward compatibility with the existing Core.Extensions.AddRabbitMQ method.
    /// It delegates to the existing implementation to ensure compatibility.
    /// </remarks>
    /// <example>
    /// <code>
    /// services.AddRabbitMQLegacy(config => config.Connection.ConnectionString = "amqp://localhost");
    /// </code>
    /// </example>
    public static IServiceCollection AddRabbitMQLegacy(this IServiceCollection services, Action<RabbitMQConfiguration> configureOptions)
    {
        return services.AddRabbitMQ(configureOptions);
    }

    #endregion

    #region Private Helper Methods

    /// <summary>
    /// Registers all RabbitMQ services based on configuration
    /// </summary>
    /// <param name="services">The service collection</param>
    /// <param name="configuration">RabbitMQ configuration</param>
    /// <returns>The service collection for fluent configuration</returns>
    private static IServiceCollection RegisterAllServices(IServiceCollection services, RabbitMQConfiguration configuration)
    {
        // Register core services
        ServiceRegistrar.RegisterCoreServices(services);
        ServiceRegistrar.RegisterSerializationServices(services);
        ServiceRegistrar.RegisterErrorHandlingServices(services);
        ServiceRegistrar.RegisterCQRSHandlers(services);

        // Register optional services based on configuration
        if (configuration.EventBus.Enabled)
        {
            ServiceRegistrar.RegisterEventBusServices(services);
        }

        if (configuration.EventStore.Enabled)
        {
            ServiceRegistrar.RegisterEventStoreServices(services);
        }

        if (configuration.Saga.Enabled)
        {
            ServiceRegistrar.RegisterSagaServices(services);
        }

        if (configuration.Monitoring.Enabled)
        {
            ServiceRegistrar.RegisterMonitoringServices(services);
        }

        if (configuration.HealthCheck.Enabled)
        {
            ServiceRegistrar.RegisterHealthCheckServices(services);
        }

        return services;
    }

    #endregion
}