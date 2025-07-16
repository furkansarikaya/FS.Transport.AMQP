using FS.StreamFlow.Core.Features.Events.Interfaces;
using FS.StreamFlow.Core.Features.Messaging.Interfaces;
using FS.StreamFlow.Core.Features.Messaging.Models;
using FS.StreamFlow.RabbitMQ.Features.Connection;
using FS.StreamFlow.RabbitMQ.Features.Producer;
using FS.StreamFlow.RabbitMQ.Features.Consumer;
using FS.StreamFlow.RabbitMQ.Features.Queue;
using FS.StreamFlow.RabbitMQ.Features.EventBus;
using FS.StreamFlow.RabbitMQ.Features.Exchange;
using FS.StreamFlow.RabbitMQ.Features.EventStore;
using FS.StreamFlow.RabbitMQ.Features.HealthCheck;
using FS.StreamFlow.RabbitMQ.Features.Saga;
using FS.StreamFlow.RabbitMQ.Features.RetryPolicies;
using FS.StreamFlow.RabbitMQ.Features.Serialization;
using FS.StreamFlow.RabbitMQ.Features.ErrorHandling;
using FS.StreamFlow.RabbitMQ.Features.Metrics;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace FS.StreamFlow.RabbitMQ.DependencyInjection;

/// <summary>
/// Extension methods for configuring RabbitMQ services in the dependency injection container.
/// Provides comprehensive registration of all RabbitMQ-related services and configurations.
/// </summary>
public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Adds RabbitMQ StreamFlow services to the service collection with default configuration.
    /// </summary>
    /// <param name="services">The service collection to add services to.</param>
    /// <returns>The service collection for method chaining.</returns>
    public static IServiceCollection AddRabbitMQStreamFlow(this IServiceCollection services)
    {
        return services.AddRabbitMQStreamFlow(_ => { });
    }

    /// <summary>
    /// Adds RabbitMQ StreamFlow services to the service collection with custom configuration.
    /// </summary>
    /// <param name="services">The service collection to add services to.</param>
    /// <param name="configure">Action to configure the RabbitMQ client options.</param>
    /// <returns>The service collection for method chaining.</returns>
    public static IServiceCollection AddRabbitMQStreamFlow(
        this IServiceCollection services,
        Action<RabbitMQStreamFlowOptions> configure)
    {
        if (services == null)
            throw new ArgumentNullException(nameof(services));

        if (configure == null)
            throw new ArgumentNullException(nameof(configure));

        // Configure options
        services.Configure(configure);

        // Register core services
        RegisterCoreServices(services);

        // Register RabbitMQ-specific implementations
        RegisterRabbitMQImplementations(services);

        // Register feature services
        RegisterFeatureServices(services);

        return services;
    }

    /// <summary>
    /// Adds RabbitMQ StreamFlow services to the service collection with configuration from IConfiguration.
    /// </summary>
    /// <param name="services">The service collection to add services to.</param>
    /// <param name="configuration">The configuration to bind options from.</param>
    /// <param name="sectionName">The configuration section name. Defaults to "RabbitMQ".</param>
    /// <returns>The service collection for method chaining.</returns>
    public static IServiceCollection AddRabbitMQStreamFlow(
        this IServiceCollection services,
        IConfiguration configuration,
        string sectionName = "RabbitMQ")
    {
        if (services == null)
            throw new ArgumentNullException(nameof(services));

        if (configuration == null)
            throw new ArgumentNullException(nameof(configuration));

        var section = configuration.GetSection(sectionName);
        if (!section.Exists())
        {
            throw new InvalidOperationException($"Configuration section '{sectionName}' not found");
        }

        services.Configure<RabbitMQStreamFlowOptions>(options => section.Bind(options));

        // Register core services
        RegisterCoreServices(services);

        // Register RabbitMQ-specific implementations
        RegisterRabbitMQImplementations(services);

        // Register feature services
        RegisterFeatureServices(services);

        return services;
    }

    /// <summary>
    /// Adds RabbitMQ StreamFlow services with both configuration and custom action.
    /// </summary>
    /// <param name="services">The service collection to add services to.</param>
    /// <param name="configuration">The configuration to bind options from.</param>
    /// <param name="configure">Action to further configure the RabbitMQ client options.</param>
    /// <param name="sectionName">The configuration section name. Defaults to "RabbitMQ".</param>
    /// <returns>The service collection for method chaining.</returns>
    public static IServiceCollection AddRabbitMQStreamFlow(
        this IServiceCollection services,
        IConfiguration configuration,
        Action<RabbitMQStreamFlowOptions> configure,
        string sectionName = "RabbitMQ")
    {
        if (services == null)
            throw new ArgumentNullException(nameof(services));

        if (configuration == null)
            throw new ArgumentNullException(nameof(configuration));

        if (configure == null)
            throw new ArgumentNullException(nameof(configure));

        var section = configuration.GetSection(sectionName);
        if (!section.Exists())
        {
            throw new InvalidOperationException($"Configuration section '{sectionName}' not found");
        }

        services.Configure<RabbitMQStreamFlowOptions>(options => section.Bind(options));
        services.PostConfigure(configure);

        // Register core services
        RegisterCoreServices(services);

        // Register RabbitMQ-specific implementations
        RegisterRabbitMQImplementations(services);

        // Register feature services
        RegisterFeatureServices(services);

        return services;
    }

    private static void RegisterCoreServices(IServiceCollection services)
    {
        // Register the main StreamFlow client
        services.TryAddSingleton<IStreamFlowClient>(provider =>
        {
            var connectionManager = provider.GetRequiredService<IConnectionManager>();
            var producer = provider.GetRequiredService<IProducer>();
            var consumer = provider.GetRequiredService<IConsumer>();
            var queueManager = provider.GetRequiredService<IQueueManager>();
            var exchangeManager = provider.GetRequiredService<IExchangeManager>();
            var eventBus = provider.GetRequiredService<IEventBus>();
            var eventStore = provider.GetRequiredService<IEventStore>();
            var healthChecker = provider.GetRequiredService<IHealthChecker>();
            var sagaOrchestrator = provider.GetRequiredService<ISagaOrchestrator>();
            var retryPolicyFactory = provider.GetRequiredService<IRetryPolicyFactory>();
            var serializerFactory = provider.GetRequiredService<IMessageSerializerFactory>();
            var errorHandler = provider.GetRequiredService<IErrorHandler>();
            var deadLetterHandler = provider.GetRequiredService<IDeadLetterHandler>();
            var metricsCollector = provider.GetRequiredService<IMetricsCollector>();
            var logger = provider.GetRequiredService<ILogger<RabbitMQStreamFlowClient>>();
            var configuration = provider.GetRequiredService<IOptions<ClientConfiguration>>();
            
            return new RabbitMQStreamFlowClient(connectionManager, producer, consumer, queueManager, exchangeManager, eventBus, eventStore, healthChecker, sagaOrchestrator, retryPolicyFactory, serializerFactory, errorHandler, deadLetterHandler, metricsCollector, logger, configuration);
        });

        // Register options
        services.TryAddSingleton<IOptions<ClientConfiguration>>(provider =>
        {
            var rabbitMQOptions = provider.GetRequiredService<IOptions<RabbitMQStreamFlowOptions>>();
            return Options.Create(rabbitMQOptions.Value.ClientConfiguration);
        });

        services.TryAddSingleton<IOptions<ConnectionSettings>>(provider =>
        {
            var rabbitMQOptions = provider.GetRequiredService<IOptions<RabbitMQStreamFlowOptions>>();
            return Options.Create(rabbitMQOptions.Value.ConnectionSettings);
        });
        
        services.TryAddSingleton<IOptions<ConsumerSettings>>(provider =>
        {
            var rabbitMQOptions = provider.GetRequiredService<IOptions<RabbitMQStreamFlowOptions>>();
            return Options.Create(rabbitMQOptions.Value.ConsumerSettings);
        });

        services.TryAddSingleton<IOptions<ProducerSettings>>(provider =>
        {
            var rabbitMQOptions = provider.GetRequiredService<IOptions<RabbitMQStreamFlowOptions>>();
            return Options.Create(rabbitMQOptions.Value.ProducerSettings);
        });
    }

    private static void RegisterRabbitMQImplementations(IServiceCollection services)
    {
        // Connection management
        services.TryAddSingleton<IConnectionManager, RabbitMQConnectionManager>();

        // Message production
        services.TryAddSingleton<IProducer, RabbitMQProducer>();

        // Exchange management
        services.TryAddSingleton<IExchangeManager, RabbitMQExchangeManager>();

        services.TryAddSingleton<IQueueManager, RabbitMQQueueManager>();

        services.TryAddSingleton<IConsumer, RabbitMQConsumer>();

        services.TryAddSingleton<IEventBus, RabbitMQEventBus>();

        // Event store
        services.TryAddSingleton<IEventStore, RabbitMQEventStore>();

        services.TryAddSingleton<IHealthChecker, RabbitMQHealthChecker>();

        // Saga orchestrator
        services.TryAddSingleton<ISagaOrchestrator, RabbitMQSagaOrchestrator>();

        // Retry policy factory
        services.TryAddSingleton<IRetryPolicyFactory, RabbitMQRetryPolicyFactory>();

        // Serialization factory
        services.TryAddSingleton<IMessageSerializerFactory, RabbitMQSerializationFactory>();

        // Error handling
        services.TryAddSingleton<IErrorHandler, RabbitMQErrorHandler>();
        services.TryAddSingleton<IDeadLetterHandler, RabbitMQDeadLetterHandler>();

        // Metrics collector
        services.TryAddSingleton<IMetricsCollector, RabbitMQMetricsCollector>();
    }

    private static void RegisterFeatureServices(IServiceCollection services)
    {
        // Add RabbitMQ feature-specific services
        // These services provide RabbitMQ-specific implementations for various features
        
        // RabbitMQ feature service for initialization and management
        services.TryAddSingleton<IRabbitMQFeatureService, RabbitMQFeatureService>();
        
        // Additional RabbitMQ-specific services can be added here as needed
        // For example: custom message converters, specialized handlers, etc.
    }
}

/// <summary>
/// Configuration options for RabbitMQ StreamFlow services.
/// </summary>
public class RabbitMQStreamFlowOptions
{
    /// <summary>
    /// Gets or sets the client configuration.
    /// </summary>
    public ClientConfiguration ClientConfiguration { get; set; } = new();

    /// <summary>
    /// Gets or sets the connection settings.
    /// </summary>
    public ConnectionSettings ConnectionSettings { get; set; } = new();

    /// <summary>
    /// Gets or sets the producer settings.
    /// </summary>
    public ProducerSettings ProducerSettings { get; set; } = new();

    /// <summary>
    /// Gets or sets the consumer settings.
    /// </summary>
    public ConsumerSettings ConsumerSettings { get; set; } = new();

    /// <summary>
    /// Gets or sets additional feature-specific settings.
    /// </summary>
    public Dictionary<string, object> FeatureSettings { get; set; } = new();
}

/// <summary>
/// Interface for RabbitMQ feature services.
/// </summary>
public interface IRabbitMQFeatureService
{
    /// <summary>
    /// Initializes the feature service.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task representing the initialization operation.</returns>
    Task InitializeAsync(CancellationToken cancellationToken = default);
}

/// <summary>
/// Default implementation of RabbitMQ feature service.
/// </summary>
public class RabbitMQFeatureService : IRabbitMQFeatureService
{
    private readonly ILogger<RabbitMQFeatureService> _logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="RabbitMQFeatureService"/> class.
    /// </summary>
    /// <param name="logger">The logger instance.</param>
    public RabbitMQFeatureService(ILogger<RabbitMQFeatureService> logger)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>
    /// Initializes the feature service.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task representing the initialization operation.</returns>
    public async Task InitializeAsync(CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Initializing RabbitMQ feature service...");
        await Task.CompletedTask;
        _logger.LogInformation("RabbitMQ feature service initialized successfully");
    }
} 