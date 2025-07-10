using FS.Mediator.Features.RequestHandling.Core;
using FS.Transport.AMQP.Configuration;
using FS.Transport.AMQP.Connection;
using FS.Transport.AMQP.Consumer;
using FS.Transport.AMQP.Core;
using FS.Transport.AMQP.ErrorHandling;
using FS.Transport.AMQP.EventBus;
using FS.Transport.AMQP.EventHandlers;
using FS.Transport.AMQP.EventStore;
using FS.Transport.AMQP.Exchange;
using FS.Transport.AMQP.Monitoring;
using FS.Transport.AMQP.Producer;
using FS.Transport.AMQP.Queue;
using FS.Transport.AMQP.RetryPolicies;
using FS.Transport.AMQP.Saga;
using FS.Transport.AMQP.Serialization;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Logging;
using System.Reflection;

namespace FS.Transport.AMQP.DependencyInjection;

/// <summary>
/// Centralized service registrar for RabbitMQ services
/// </summary>
/// <remarks>
/// This class provides static methods for registering different groups of RabbitMQ services.
/// It includes automatic handler discovery, service validation, and comprehensive registration
/// for all RabbitMQ components including core services, CQRS handlers, event bus, monitoring,
/// and health checks.
/// </remarks>
public static class ServiceRegistrar
{
    /// <summary>
    /// Registers core RabbitMQ services
    /// </summary>
    /// <param name="services">The service collection</param>
    /// <returns>The service collection for fluent configuration</returns>
    /// <remarks>
    /// Registers essential services including:
    /// - RabbitMQ client factory and client
    /// - Connection manager and connection pool
    /// - Queue manager and exchange manager
    /// - Message producer and consumer
    /// - Configuration services
    /// </remarks>
    /// <example>
    /// <code>
    /// ServiceRegistrar.RegisterCoreServices(services);
    /// </code>
    /// </example>
    public static IServiceCollection RegisterCoreServices(IServiceCollection services)
    {
        ArgumentNullException.ThrowIfNull(services);

        // Register factory as singleton
        services.TryAddSingleton<IRabbitMQClientFactory, RabbitMQClientFactory>();
        
        // Register client as scoped
        services.TryAddScoped<IRabbitMQClient, RabbitMQClient>();
        
        // Register connection services
        services.TryAddScoped<IConnectionManager, ConnectionManager>();
        services.TryAddScoped<ConnectionPool>();
        
        // Register queue and exchange managers
        services.TryAddScoped<IQueueManager, QueueManager>();
        services.TryAddScoped<IExchangeManager, ExchangeManager>();
        
        // Register producer and consumer services
        services.TryAddScoped<IMessageProducer, MessageProducer>();
        services.TryAddScoped<IMessageConsumer, MessageConsumer>();
        
        // Register configuration services
        services.TryAddSingleton<IRabbitMQConfiguration, RabbitMQConfiguration>();
        
        return services;
    }

    /// <summary>
    /// Registers serialization services
    /// </summary>
    /// <param name="services">The service collection</param>
    /// <returns>The service collection for fluent configuration</returns>
    /// <remarks>
    /// Registers serialization services including:
    /// - Message serializer factory
    /// - JSON message serializer
    /// - Binary message serializer
    /// - Custom converters for DateTime and TimeSpan
    /// </remarks>
    /// <example>
    /// <code>
    /// ServiceRegistrar.RegisterSerializationServices(services);
    /// </code>
    /// </example>
    public static IServiceCollection RegisterSerializationServices(IServiceCollection services)
    {
        ArgumentNullException.ThrowIfNull(services);

        // Register serializer factory as singleton
        services.TryAddSingleton<IMessageSerializerFactory, MessageSerializerFactory>();
        
        // Register individual serializers
        services.TryAddTransient<JsonMessageSerializer>();
        services.TryAddTransient<BinaryMessageSerializer>();
        
        // Register custom converters
        services.TryAddTransient<DateTimeOffsetConverter>();
        services.TryAddTransient<TimeSpanConverter>();
        
        return services;
    }

    /// <summary>
    /// Registers error handling services
    /// </summary>
    /// <param name="services">The service collection</param>
    /// <returns>The service collection for fluent configuration</returns>
    /// <remarks>
    /// Registers error handling services including:
    /// - Error handler implementation
    /// - Dead letter handler
    /// - Retry policy factory and implementations
    /// - Circuit breaker services
    /// </remarks>
    /// <example>
    /// <code>
    /// ServiceRegistrar.RegisterErrorHandlingServices(services);
    /// </code>
    /// </example>
    public static IServiceCollection RegisterErrorHandlingServices(IServiceCollection services)
    {
        ArgumentNullException.ThrowIfNull(services);

        // Register error handling services
        services.TryAddScoped<IErrorHandler, ErrorHandler>();
        services.TryAddScoped<IDeadLetterHandler, DeadLetterHandler>();
        
        // Register retry policy factory and implementations
        services.TryAddSingleton<IRetryPolicyFactory, RetryPolicyFactory>();
        services.TryAddTransient<NoRetryPolicy>();
        services.TryAddTransient<LinearRetryPolicy>();
        services.TryAddTransient<ExponentialBackoffRetryPolicy>();
        services.TryAddTransient<CircuitBreakerRetryPolicy>();
        
        return services;
    }

    /// <summary>
    /// Registers event bus services
    /// </summary>
    /// <param name="services">The service collection</param>
    /// <returns>The service collection for fluent configuration</returns>
    /// <remarks>
    /// Registers event bus services including:
    /// - Event bus implementation
    /// - Event publisher and subscriber
    /// - Event handler registry and factory
    /// - Domain and integration event handlers
    /// </remarks>
    /// <example>
    /// <code>
    /// ServiceRegistrar.RegisterEventBusServices(services);
    /// </code>
    /// </example>
    public static IServiceCollection RegisterEventBusServices(IServiceCollection services)
    {
        ArgumentNullException.ThrowIfNull(services);

        // Register event bus services
        services.TryAddSingleton<IEventBus, EventBus.EventBus>();
        services.TryAddScoped<IEventPublisher, EventPublisher>();
        services.TryAddScoped<IEventSubscriber, EventSubscriber>();
        
        // Register event handler services
        services.TryAddSingleton<IEventHandlerRegistry, EventHandlerRegistry>();
        services.TryAddScoped<IEventHandlerFactory, EventHandlerFactory>();
        
        return services;
    }

    /// <summary>
    /// Registers event store services
    /// </summary>
    /// <param name="services">The service collection</param>
    /// <returns>The service collection for fluent configuration</returns>
    /// <remarks>
    /// Registers event store services including:
    /// - Event store implementation
    /// - Event stream management
    /// - Event snapshot services
    /// - Event sourcing capabilities
    /// </remarks>
    /// <example>
    /// <code>
    /// ServiceRegistrar.RegisterEventStoreServices(services);
    /// </code>
    /// </example>
    public static IServiceCollection RegisterEventStoreServices(IServiceCollection services)
    {
        ArgumentNullException.ThrowIfNull(services);

        // Register event store services
        services.TryAddScoped<IEventStore, EventStore.EventStore>();
        services.TryAddScoped<IEventStream, EventStream>();
        
        return services;
    }

    /// <summary>
    /// Registers saga orchestration services
    /// </summary>
    /// <param name="services">The service collection</param>
    /// <returns>The service collection for fluent configuration</returns>
    /// <remarks>
    /// Registers saga services including:
    /// - Saga orchestrator implementation
    /// - Saga state management
    /// - Saga timeout and compensation handling
    /// - Saga persistence and recovery
    /// </remarks>
    /// <example>
    /// <code>
    /// ServiceRegistrar.RegisterSagaServices(services);
    /// </code>
    /// </example>
    public static IServiceCollection RegisterSagaServices(IServiceCollection services)
    {
        ArgumentNullException.ThrowIfNull(services);

        // Register saga services
        services.TryAddScoped<ISagaOrchestrator, SagaOrchestrator>();
        
        return services;
    }

    /// <summary>
    /// Registers monitoring services
    /// </summary>
    /// <param name="services">The service collection</param>
    /// <returns>The service collection for fluent configuration</returns>
    /// <remarks>
    /// Registers monitoring services including:
    /// - Metrics collector
    /// - Performance monitoring
    /// - Health checker
    /// - Logging extensions
    /// </remarks>
    /// <example>
    /// <code>
    /// ServiceRegistrar.RegisterMonitoringServices(services);
    /// </code>
    /// </example>
    public static IServiceCollection RegisterMonitoringServices(IServiceCollection services)
    {
        ArgumentNullException.ThrowIfNull(services);

        // Register monitoring services
        services.TryAddSingleton<MetricsCollector>();
        services.TryAddScoped<IHealthChecker, HealthChecker>();
        
        return services;
    }

    /// <summary>
    /// Registers health check services
    /// </summary>
    /// <param name="services">The service collection</param>
    /// <param name="name">The health check name</param>
    /// <param name="failureStatus">The failure status to report</param>
    /// <param name="tags">Tags for the health check</param>
    /// <returns>The service collection for fluent configuration</returns>
    /// <remarks>
    /// Health check services include:
    /// - RabbitMQ connection health check
    /// - Queue health monitoring
    /// - Exchange health monitoring
    /// - Producer and consumer health checks
    /// </remarks>
    /// <example>
    /// <code>
    /// ServiceRegistrar.RegisterHealthCheckServices(services, "rabbitmq", HealthStatus.Unhealthy, new[] { "rabbitmq", "messaging" });
    /// </code>
    /// </example>
    public static IServiceCollection RegisterHealthCheckServices(
        IServiceCollection services, 
        string name = "rabbitmq", 
        Microsoft.Extensions.Diagnostics.HealthChecks.HealthStatus failureStatus = Microsoft.Extensions.Diagnostics.HealthChecks.HealthStatus.Unhealthy,
        string[]? tags = null)
    {
        ArgumentNullException.ThrowIfNull(services);

        tags ??= new[] { "rabbitmq", "messaging" };

        services.AddHealthChecks()
            .AddCheck<RabbitMQHealthCheck>(name, failureStatus, tags: tags);

        return services;
    }

    /// <summary>
    /// Registers CQRS command and query handlers
    /// </summary>
    /// <param name="services">The service collection</param>
    /// <returns>The service collection for fluent configuration</returns>
    /// <remarks>
    /// Registers all CQRS handlers including:
    /// - Queue command and query handlers
    /// - Exchange command and query handlers
    /// - Connection command and query handlers
    /// - Consumer and producer handlers
    /// </remarks>
    /// <example>
    /// <code>
    /// ServiceRegistrar.RegisterCQRSHandlers(services);
    /// </code>
    /// </example>
    public static IServiceCollection RegisterCQRSHandlers(IServiceCollection services)
    {
        ArgumentNullException.ThrowIfNull(services);

        // Register Queue Command Handlers
        services.TryAddScoped<IRequestHandler<DeclareQueueCommand, QueueDeclarationResult>, DeclareQueueCommandHandler>();
        services.TryAddScoped<IRequestHandler<DeleteQueueCommand, QueueOperationResult>, DeleteQueueCommandHandler>();
        services.TryAddScoped<IRequestHandler<BindQueueCommand, QueueBindingResult>, BindQueueCommandHandler>();
        services.TryAddScoped<IRequestHandler<UnbindQueueCommand, QueueBindingResult>, UnbindQueueCommandHandler>();
        services.TryAddScoped<IRequestHandler<PurgeQueueCommand, QueuePurgeResult>, PurgeQueueCommandHandler>();
        services.TryAddScoped<IRequestHandler<DeclareQueuesCommand, QueueBatchDeclarationResult>, DeclareQueuesCommandHandler>();
        services.TryAddScoped<IRequestHandler<RedeclareQueuesCommand, QueueRecoveryResult>, RedeclareQueuesCommandHandler>();
        services.TryAddScoped<IRequestHandler<ResetQueueStatisticsCommand, QueueOperationResult>, ResetQueueStatisticsCommandHandler>();
        
        // Register Queue Query Handlers
        services.TryAddScoped<IRequestHandler<GetQueueInfoQuery, QueueInfoResult>, GetQueueInfoQueryHandler>();
        services.TryAddScoped<IRequestHandler<GetQueueStatisticsQuery, QueueStatisticsResult>, GetQueueStatisticsQueryHandler>();
        services.TryAddScoped<IRequestHandler<GetQueueMessageCountQuery, QueueMessageCountResult>, GetQueueMessageCountQueryHandler>();
        services.TryAddScoped<IRequestHandler<GetQueueConsumerCountQuery, QueueConsumerCountResult>, GetQueueConsumerCountQueryHandler>();
        services.TryAddScoped<IRequestHandler<QueueExistsQuery, QueueExistsResult>, QueueExistsQueryHandler>();
        services.TryAddScoped<IRequestHandler<GetQueueBindingsQuery, QueueBindingsResult>, GetQueueBindingsQueryHandler>();
        services.TryAddScoped<IRequestHandler<GetQueueHealthQuery, QueueHealthResult>, GetQueueHealthQueryHandler>();
        services.TryAddScoped<IRequestHandler<GetQueuePerformanceQuery, QueuePerformanceResult>, GetQueuePerformanceQueryHandler>();
        services.TryAddScoped<IRequestHandler<ListQueuesQuery, QueueListResult>, ListQueuesQueryHandler>();
        
        // Register Exchange Command Handlers
        services.TryAddScoped<IRequestHandler<DeclareExchangeCommand, ExchangeDeclarationResult>, DeclareExchangeCommandHandler>();
        services.TryAddScoped<IRequestHandler<DeleteExchangeCommand, ExchangeOperationResult>, DeleteExchangeCommandHandler>();
        services.TryAddScoped<IRequestHandler<BindExchangeCommand, ExchangeBindingResult>, BindExchangeCommandHandler>();
        services.TryAddScoped<IRequestHandler<UnbindExchangeCommand, ExchangeBindingResult>, UnbindExchangeCommandHandler>();
        services.TryAddScoped<IRequestHandler<DeclareExchangesCommand, ExchangeBatchDeclarationResult>, DeclareExchangesCommandHandler>();
        services.TryAddScoped<IRequestHandler<RedeclareExchangesCommand, ExchangeRecoveryResult>, RedeclareExchangesCommandHandler>();
        services.TryAddScoped<IRequestHandler<ResetExchangeStatisticsCommand, ExchangeOperationResult>, ResetExchangeStatisticsCommandHandler>();
        
        // Register Exchange Query Handlers
        services.TryAddScoped<IRequestHandler<GetExchangeInfoQuery, ExchangeInfoResult>, GetExchangeInfoQueryHandler>();
        services.TryAddScoped<IRequestHandler<GetExchangeStatisticsQuery, ExchangeStatisticsResult>, GetExchangeStatisticsQueryHandler>();
        services.TryAddScoped<IRequestHandler<ExchangeExistsQuery, ExchangeExistsResult>, ExchangeExistsQueryHandler>();
        services.TryAddScoped<IRequestHandler<GetExchangeBindingsQuery, ExchangeBindingsResult>, GetExchangeBindingsQueryHandler>();
        services.TryAddScoped<IRequestHandler<GetExchangeHealthQuery, ExchangeHealthResult>, GetExchangeHealthQueryHandler>();
        services.TryAddScoped<IRequestHandler<GetExchangePerformanceQuery, ExchangePerformanceResult>, GetExchangePerformanceQueryHandler>();
        services.TryAddScoped<IRequestHandler<ListExchangesQuery, ExchangeListResult>, ListExchangesQueryHandler>();
        services.TryAddScoped<IRequestHandler<GetExchangeTopologyQuery, ExchangeTopologyResult>, GetExchangeTopologyQueryHandler>();
        
        return services;
    }

    /// <summary>
    /// Automatically discovers and registers all CQRS handlers from the current assembly
    /// </summary>
    /// <param name="services">The service collection</param>
    /// <param name="assemblies">Assemblies to scan for handlers (defaults to current assembly)</param>
    /// <returns>The service collection for fluent configuration</returns>
    /// <remarks>
    /// This method uses reflection to automatically discover all command and query handlers
    /// that implement IRequestHandler interface and registers them with appropriate lifetimes.
    /// </remarks>
    /// <example>
    /// <code>
    /// ServiceRegistrar.RegisterHandlersFromAssembly(services, typeof(Program).Assembly);
    /// </code>
    /// </example>
    public static IServiceCollection RegisterHandlersFromAssembly(IServiceCollection services, params Assembly[] assemblies)
    {
        ArgumentNullException.ThrowIfNull(services);

        if (assemblies.Length == 0)
        {
            assemblies = new[] { Assembly.GetExecutingAssembly() };
        }

        foreach (var assembly in assemblies)
        {
            var handlerTypes = assembly.GetTypes()
                .Where(t => t.IsClass && !t.IsAbstract)
                .Where(t => t.GetInterfaces().Any(i => i.IsGenericType && i.GetGenericTypeDefinition() == typeof(IRequestHandler<,>)))
                .ToList();

            foreach (var handlerType in handlerTypes)
            {
                var interfaces = handlerType.GetInterfaces()
                    .Where(i => i.IsGenericType && i.GetGenericTypeDefinition() == typeof(IRequestHandler<,>))
                    .ToList();

                foreach (var @interface in interfaces)
                {
                    services.TryAddScoped(@interface, handlerType);
                }
            }
        }

        return services;
    }

    /// <summary>
    /// Validates all registered RabbitMQ services
    /// </summary>
    /// <param name="services">The service collection</param>
    /// <returns>A list of validation errors, empty if all services are valid</returns>
    /// <remarks>
    /// This method validates that all required services are registered and properly configured.
    /// It checks for missing dependencies, incorrect lifetimes, and configuration issues.
    /// </remarks>
    /// <example>
    /// <code>
    /// var errors = ServiceRegistrar.ValidateServices(services);
    /// if (errors.Any())
    /// {
    ///     throw new InvalidOperationException($"Service validation failed: {string.Join(", ", errors)}");
    /// }
    /// </code>
    /// </example>
    public static IList<string> ValidateServices(IServiceCollection services)
    {
        ArgumentNullException.ThrowIfNull(services);

        var errors = new List<string>();

        // Check for required core services
        if (!services.Any(s => s.ServiceType == typeof(IRabbitMQClientFactory)))
        {
            errors.Add("IRabbitMQClientFactory is not registered");
        }

        if (!services.Any(s => s.ServiceType == typeof(IRabbitMQClient)))
        {
            errors.Add("IRabbitMQClient is not registered");
        }

        if (!services.Any(s => s.ServiceType == typeof(IConnectionManager)))
        {
            errors.Add("IConnectionManager is not registered");
        }

        if (!services.Any(s => s.ServiceType == typeof(IQueueManager)))
        {
            errors.Add("IQueueManager is not registered");
        }

        if (!services.Any(s => s.ServiceType == typeof(IExchangeManager)))
        {
            errors.Add("IExchangeManager is not registered");
        }

        if (!services.Any(s => s.ServiceType == typeof(IMessageProducer)))
        {
            errors.Add("IMessageProducer is not registered");
        }

        if (!services.Any(s => s.ServiceType == typeof(IMessageConsumer)))
        {
            errors.Add("IMessageConsumer is not registered");
        }

        // Check for serialization services
        if (!services.Any(s => s.ServiceType == typeof(IMessageSerializerFactory)))
        {
            errors.Add("IMessageSerializerFactory is not registered");
        }

        // Check for error handling services
        if (!services.Any(s => s.ServiceType == typeof(IErrorHandler)))
        {
            errors.Add("IErrorHandler is not registered");
        }

        if (!services.Any(s => s.ServiceType == typeof(IRetryPolicyFactory)))
        {
            errors.Add("IRetryPolicyFactory is not registered");
        }

        return errors;
    }

    /// <summary>
    /// Gets service lifetime recommendations for RabbitMQ services
    /// </summary>
    /// <param name="serviceType">The service type</param>
    /// <returns>The recommended service lifetime</returns>
    /// <remarks>
    /// This method provides recommendations for service lifetimes based on RabbitMQ best practices.
    /// It helps ensure optimal performance and resource utilization.
    /// </remarks>
    /// <example>
    /// <code>
    /// var lifetime = ServiceRegistrar.GetRecommendedLifetime(typeof(IRabbitMQClient));
    /// services.Add(new ServiceDescriptor(typeof(IRabbitMQClient), typeof(RabbitMQClient), lifetime));
    /// </code>
    /// </example>
    public static ServiceLifetime GetRecommendedLifetime(Type serviceType)
    {
        ArgumentNullException.ThrowIfNull(serviceType);

        // Factories should be singletons
        if (serviceType.Name.EndsWith("Factory"))
        {
            return ServiceLifetime.Singleton;
        }

        // Configuration services should be singletons
        if (serviceType.Name.EndsWith("Configuration") || serviceType.Name.EndsWith("Settings"))
        {
            return ServiceLifetime.Singleton;
        }

        // Registry services should be singletons
        if (serviceType.Name.EndsWith("Registry"))
        {
            return ServiceLifetime.Singleton;
        }

        // Connection pool should be singleton
        if (serviceType == typeof(ConnectionPool))
        {
            return ServiceLifetime.Singleton;
        }

        // Monitoring services should be singletons
        if (serviceType.Name.Contains("Monitor") || serviceType.Name.Contains("Metrics"))
        {
            return ServiceLifetime.Singleton;
        }

        // Event bus should be singleton
        if (serviceType == typeof(IEventBus))
        {
            return ServiceLifetime.Singleton;
        }

        // Default to scoped for most services
        return ServiceLifetime.Scoped;
    }
} 