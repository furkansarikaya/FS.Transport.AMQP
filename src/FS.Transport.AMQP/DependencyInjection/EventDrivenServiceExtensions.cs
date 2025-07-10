using FS.Transport.AMQP.Configuration;
using FS.Transport.AMQP.EventBus;
using FS.Transport.AMQP.EventHandlers;
using FS.Transport.AMQP.Events;
using FS.Transport.AMQP.EventStore;
using FS.Transport.AMQP.Saga;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using System.Reflection;

namespace FS.Transport.AMQP.DependencyInjection;

/// <summary>
/// Extension methods for registering event-driven services
/// </summary>
/// <remarks>
/// This class provides comprehensive extension methods for registering event-driven architecture
/// components including event bus, event store, saga orchestration, and automatic handler discovery.
/// It supports both domain events and integration events with flexible configuration options.
/// </remarks>
public static class EventDrivenServiceExtensions
{
    #region Event Bus Registration

    /// <summary>
    /// Adds event bus services to the service collection
    /// </summary>
    /// <param name="services">The service collection</param>
    /// <param name="configure">Optional action to configure event bus settings</param>
    /// <returns>The service collection for fluent configuration</returns>
    /// <remarks>
    /// This method registers the core event bus services including the event bus itself,
    /// event publisher, event subscriber, and event handler registry. It provides the
    /// foundation for event-driven architecture in your application.
    /// </remarks>
    /// <example>
    /// <code>
    /// services.AddEventBus(eventBus =>
    /// {
    ///     eventBus.DefaultExchange = "events";
    ///     eventBus.EnableEventStore = true;
    ///     eventBus.EnableDomainEvents = true;
    ///     eventBus.EnableIntegrationEvents = true;
    /// });
    /// </code>
    /// </example>
    public static IServiceCollection AddEventBus(this IServiceCollection services, Action<Configuration.EventBusSettings>? configure = null)
    {
        ArgumentNullException.ThrowIfNull(services);

        // Configure event bus settings
        services.Configure<Configuration.EventBusSettings>(settings =>
        {
            settings.Enabled = true;
            configure?.Invoke(settings);
        });

        // Register core event bus services
        services.TryAddSingleton<IEventBus, EventBus.EventBus>();
        services.TryAddScoped<IEventPublisher, EventPublisher>();
        services.TryAddScoped<IEventSubscriber, EventSubscriber>();
        
        // Register event handler services
        services.TryAddSingleton<IEventHandlerRegistry, EventHandlerRegistry>();
        services.TryAddScoped<IEventHandlerFactory, EventHandlerFactory>();
        
        return services;
    }

    /// <summary>
    /// Adds event bus services with connection string
    /// </summary>
    /// <param name="services">The service collection</param>
    /// <param name="connectionString">RabbitMQ connection string</param>
    /// <param name="configure">Optional action to configure event bus settings</param>
    /// <returns>The service collection for fluent configuration</returns>
    /// <exception cref="ArgumentException">Thrown when connection string is null or empty</exception>
    /// <remarks>
    /// This method provides a convenient way to register event bus services with a
    /// connection string. It automatically configures the underlying RabbitMQ connection
    /// for event publishing and subscribing.
    /// </remarks>
    /// <example>
    /// <code>
    /// services.AddEventBus("amqp://localhost", eventBus =>
    /// {
    ///     eventBus.DefaultExchange = "events";
    ///     eventBus.EnableEventStore = true;
    /// });
    /// </code>
    /// </example>
    public static IServiceCollection AddEventBus(this IServiceCollection services, string connectionString, Action<Configuration.EventBusSettings>? configure = null)
    {
        if (string.IsNullOrWhiteSpace(connectionString))
            throw new ArgumentException("Connection string cannot be null or empty", nameof(connectionString));

        // Configure connection for event bus
        services.Configure<RabbitMQConfiguration>(config =>
        {
            config.Connection.ConnectionString = connectionString;
            config.EventBus.Enabled = true;
        });

        return services.AddEventBus(configure);
    }

    /// <summary>
    /// Adds event bus services with detailed configuration
    /// </summary>
    /// <param name="services">The service collection</param>
    /// <param name="connectionString">RabbitMQ connection string</param>
    /// <param name="exchangeName">Default exchange name for events</param>
    /// <param name="configure">Optional action to configure event bus settings</param>
    /// <returns>The service collection for fluent configuration</returns>
    /// <exception cref="ArgumentException">Thrown when connection string or exchange name is null or empty</exception>
    /// <remarks>
    /// This method provides detailed configuration for event bus services including
    /// connection string and default exchange name. It's useful when you need more
    /// control over the event bus configuration.
    /// </remarks>
    /// <example>
    /// <code>
    /// services.AddEventBus("amqp://localhost", "domain-events", eventBus =>
    /// {
    ///     eventBus.EnableEventStore = true;
    ///     eventBus.EnableDomainEvents = true;
    /// });
    /// </code>
    /// </example>
    public static IServiceCollection AddEventBus(
        this IServiceCollection services, 
        string connectionString, 
        string exchangeName, 
        Action<Configuration.EventBusSettings>? configure = null)
    {
        if (string.IsNullOrWhiteSpace(connectionString))
            throw new ArgumentException("Connection string cannot be null or empty", nameof(connectionString));
        
        if (string.IsNullOrWhiteSpace(exchangeName))
            throw new ArgumentException("Exchange name cannot be null or empty", nameof(exchangeName));

        return services.AddEventBus(connectionString, eventBus =>
        {
            eventBus.DefaultExchange = exchangeName;
            configure?.Invoke(eventBus);
        });
    }

    #endregion

    #region Event Store Registration

    /// <summary>
    /// Adds event store services to the service collection
    /// </summary>
    /// <param name="services">The service collection</param>
    /// <param name="configure">Optional action to configure event store settings</param>
    /// <returns>The service collection for fluent configuration</returns>
    /// <remarks>
    /// This method registers event store services for event sourcing capabilities.
    /// It includes event store implementation, event streams, and event snapshots.
    /// </remarks>
    /// <example>
    /// <code>
    /// services.AddEventStore(eventStore =>
    /// {
    ///     eventStore.StreamPrefix = "stream-";
    ///     eventStore.SnapshotFrequency = 100;
    ///     eventStore.EnableSnapshots = true;
    /// });
    /// </code>
    /// </example>
    public static IServiceCollection AddEventStore(this IServiceCollection services, Action<Configuration.EventStoreSettings>? configure = null)
    {
        ArgumentNullException.ThrowIfNull(services);

        // Configure event store settings
        services.Configure<Configuration.EventStoreSettings>(settings =>
        {
            settings.Enabled = true;
            configure?.Invoke(settings);
        });

        // Register event store services
        services.TryAddScoped<IEventStore, EventStore.EventStore>();
        services.TryAddScoped<IEventStream, EventStream>();
        
        return services;
    }

    /// <summary>
    /// Adds event store services with connection string
    /// </summary>
    /// <param name="services">The service collection</param>
    /// <param name="connectionString">RabbitMQ connection string</param>
    /// <param name="configure">Optional action to configure event store settings</param>
    /// <returns>The service collection for fluent configuration</returns>
    /// <exception cref="ArgumentException">Thrown when connection string is null or empty</exception>
    /// <remarks>
    /// This method provides a convenient way to register event store services with a
    /// connection string. It automatically configures the underlying RabbitMQ connection
    /// for event persistence and retrieval.
    /// </remarks>
    /// <example>
    /// <code>
    /// services.AddEventStore("amqp://localhost", eventStore =>
    /// {
    ///     eventStore.StreamPrefix = "stream-";
    ///     eventStore.EnableSnapshots = true;
    /// });
    /// </code>
    /// </example>
    public static IServiceCollection AddEventStore(this IServiceCollection services, string connectionString, Action<Configuration.EventStoreSettings>? configure = null)
    {
        if (string.IsNullOrWhiteSpace(connectionString))
            throw new ArgumentException("Connection string cannot be null or empty", nameof(connectionString));

        // Configure connection for event store
        services.Configure<RabbitMQConfiguration>(config =>
        {
            config.Connection.ConnectionString = connectionString;
            config.EventStore.Enabled = true;
        });

        return services.AddEventStore(configure);
    }

    #endregion

    #region Saga Orchestration Registration

    /// <summary>
    /// Adds saga orchestration services to the service collection
    /// </summary>
    /// <param name="services">The service collection</param>
    /// <param name="configure">Optional action to configure saga settings</param>
    /// <returns>The service collection for fluent configuration</returns>
    /// <remarks>
    /// This method registers saga orchestration services for managing distributed transactions
    /// and long-running business processes. It includes saga orchestrator, state management,
    /// and compensation handling.
    /// </remarks>
    /// <example>
    /// <code>
    /// services.AddSagaOrchestration(saga =>
    /// {
    ///     saga.EnableCompensation = true;
    ///     saga.TimeoutDuration = TimeSpan.FromMinutes(30);
    ///     saga.MaxRetries = 3;
    /// });
    /// </code>
    /// </example>
    public static IServiceCollection AddSagaOrchestration(this IServiceCollection services, Action<SagaSettings>? configure = null)
    {
        ArgumentNullException.ThrowIfNull(services);

        // Configure saga settings
        services.Configure<SagaSettings>(settings =>
        {
            settings.Enabled = true;
            configure?.Invoke(settings);
        });

        // Register saga services
        services.TryAddScoped<ISagaOrchestrator, SagaOrchestrator>();
        
        return services;
    }

    /// <summary>
    /// Adds saga orchestration services with connection string
    /// </summary>
    /// <param name="services">The service collection</param>
    /// <param name="connectionString">RabbitMQ connection string</param>
    /// <param name="configure">Optional action to configure saga settings</param>
    /// <returns>The service collection for fluent configuration</returns>
    /// <exception cref="ArgumentException">Thrown when connection string is null or empty</exception>
    /// <remarks>
    /// This method provides a convenient way to register saga orchestration services with a
    /// connection string. It automatically configures the underlying RabbitMQ connection
    /// for saga state management and event coordination.
    /// </remarks>
    /// <example>
    /// <code>
    /// services.AddSagaOrchestration("amqp://localhost", saga =>
    /// {
    ///     saga.EnableCompensation = true;
    ///     saga.TimeoutDuration = TimeSpan.FromMinutes(30);
    /// });
    /// </code>
    /// </example>
    public static IServiceCollection AddSagaOrchestration(this IServiceCollection services, string connectionString, Action<SagaSettings>? configure = null)
    {
        if (string.IsNullOrWhiteSpace(connectionString))
            throw new ArgumentException("Connection string cannot be null or empty", nameof(connectionString));

        // Configure connection for saga orchestration
        services.Configure<RabbitMQConfiguration>(config =>
        {
            config.Connection.ConnectionString = connectionString;
            config.Saga.Enabled = true;
        });

        return services.AddSagaOrchestration(configure);
    }

    #endregion

    #region Event Handler Registration

    /// <summary>
    /// Registers event handlers from the specified assemblies
    /// </summary>
    /// <param name="services">The service collection</param>
    /// <param name="assemblies">Assemblies to scan for event handlers</param>
    /// <returns>The service collection for fluent configuration</returns>
    /// <remarks>
    /// This method automatically discovers and registers all event handlers from the specified
    /// assemblies. It scans for all classes that implement IEventHandler or IAsyncEventHandler.
    /// </remarks>
    /// <example>
    /// <code>
    /// services.AddEventHandlersFromAssembly(typeof(Program).Assembly, typeof(Handlers.DomainEventHandler).Assembly);
    /// </code>
    /// </example>
    public static IServiceCollection AddEventHandlersFromAssembly(this IServiceCollection services, params Assembly[] assemblies)
    {
        ArgumentNullException.ThrowIfNull(services);

        if (assemblies.Length == 0)
        {
            assemblies = new[] { Assembly.GetCallingAssembly() };
        }

        foreach (var assembly in assemblies)
        {
            var handlerTypes = assembly.GetTypes()
                .Where(t => t.IsClass && !t.IsAbstract)
                .Where(t => t.GetInterfaces().Any(i => 
                    i.IsGenericType && (
                        i.GetGenericTypeDefinition() == typeof(IEventHandler<>) ||
                        i.GetGenericTypeDefinition() == typeof(IAsyncEventHandler<>))))
                .ToList();

            foreach (var handlerType in handlerTypes)
            {
                var interfaces = handlerType.GetInterfaces()
                    .Where(i => i.IsGenericType && (
                        i.GetGenericTypeDefinition() == typeof(IEventHandler<>) ||
                        i.GetGenericTypeDefinition() == typeof(IAsyncEventHandler<>)))
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
    /// Registers event handlers from the calling assembly
    /// </summary>
    /// <param name="services">The service collection</param>
    /// <returns>The service collection for fluent configuration</returns>
    /// <remarks>
    /// This method automatically discovers and registers all event handlers from the calling assembly.
    /// It's a convenience method for the common scenario of registering handlers from the current assembly.
    /// </remarks>
    /// <example>
    /// <code>
    /// services.AddEventHandlersFromCallingAssembly();
    /// </code>
    /// </example>
    public static IServiceCollection AddEventHandlersFromCallingAssembly(this IServiceCollection services)
    {
        return services.AddEventHandlersFromAssembly(Assembly.GetCallingAssembly());
    }

    /// <summary>
    /// Registers event handlers from the executing assembly
    /// </summary>
    /// <param name="services">The service collection</param>
    /// <returns>The service collection for fluent configuration</returns>
    /// <remarks>
    /// This method automatically discovers and registers all event handlers from the executing assembly.
    /// It's useful when handlers are defined in the same assembly as the DI registration.
    /// </remarks>
    /// <example>
    /// <code>
    /// services.AddEventHandlersFromExecutingAssembly();
    /// </code>
    /// </example>
    public static IServiceCollection AddEventHandlersFromExecutingAssembly(this IServiceCollection services)
    {
        return services.AddEventHandlersFromAssembly(Assembly.GetExecutingAssembly());
    }

    /// <summary>
    /// Registers a specific event handler
    /// </summary>
    /// <typeparam name="THandler">The event handler type</typeparam>
    /// <param name="services">The service collection</param>
    /// <param name="lifetime">The service lifetime (default: Scoped)</param>
    /// <returns>The service collection for fluent configuration</returns>
    /// <remarks>
    /// This method registers a specific event handler with the specified lifetime.
    /// It's useful when you need precise control over handler registration.
    /// </remarks>
    /// <example>
    /// <code>
    /// services.AddEventHandler&lt;OrderCreatedEventHandler&gt;(ServiceLifetime.Scoped);
    /// </code>
    /// </example>
    public static IServiceCollection AddEventHandler<THandler>(this IServiceCollection services, ServiceLifetime lifetime = ServiceLifetime.Scoped)
        where THandler : class
    {
        ArgumentNullException.ThrowIfNull(services);

        var handlerType = typeof(THandler);
        var interfaces = handlerType.GetInterfaces()
            .Where(i => i.IsGenericType && (
                i.GetGenericTypeDefinition() == typeof(IEventHandler<>) ||
                i.GetGenericTypeDefinition() == typeof(IAsyncEventHandler<>)))
            .ToList();

        foreach (var @interface in interfaces)
        {
            services.Add(new ServiceDescriptor(@interface, handlerType, lifetime));
        }

        return services;
    }

    /// <summary>
    /// Registers event handlers for a specific event type
    /// </summary>
    /// <typeparam name="TEvent">The event type</typeparam>
    /// <param name="services">The service collection</param>
    /// <param name="assemblies">Assemblies to scan for handlers</param>
    /// <returns>The service collection for fluent configuration</returns>
    /// <remarks>
    /// This method registers all event handlers that handle a specific event type.
    /// It's useful when you want to register handlers for a particular event across multiple assemblies.
    /// </remarks>
    /// <example>
    /// <code>
    /// services.AddEventHandlersFor&lt;OrderCreatedEvent&gt;(typeof(Program).Assembly);
    /// </code>
    /// </example>
    public static IServiceCollection AddEventHandlersFor<TEvent>(this IServiceCollection services, params Assembly[] assemblies)
        where TEvent : class, IEvent
    {
        ArgumentNullException.ThrowIfNull(services);

        if (assemblies.Length == 0)
        {
            assemblies = new[] { Assembly.GetCallingAssembly() };
        }

        var eventType = typeof(TEvent);
        var syncHandlerInterface = typeof(IEventHandler<>).MakeGenericType(eventType);
        var asyncHandlerInterface = typeof(IAsyncEventHandler<>).MakeGenericType(eventType);

        foreach (var assembly in assemblies)
        {
            var handlerTypes = assembly.GetTypes()
                .Where(t => t.IsClass && !t.IsAbstract)
                .Where(t => syncHandlerInterface.IsAssignableFrom(t) || asyncHandlerInterface.IsAssignableFrom(t))
                .ToList();

            foreach (var handlerType in handlerTypes)
            {
                if (syncHandlerInterface.IsAssignableFrom(handlerType))
                {
                    services.TryAddScoped(syncHandlerInterface, handlerType);
                }
                
                if (asyncHandlerInterface.IsAssignableFrom(handlerType))
                {
                    services.TryAddScoped(asyncHandlerInterface, handlerType);
                }
            }
        }

        return services;
    }

    #endregion

    #region Complete Event-Driven Architecture Registration

    /// <summary>
    /// Adds complete event-driven architecture services
    /// </summary>
    /// <param name="services">The service collection</param>
    /// <param name="connectionString">RabbitMQ connection string</param>
    /// <param name="configure">Optional action to configure event-driven settings</param>
    /// <returns>The service collection for fluent configuration</returns>
    /// <exception cref="ArgumentException">Thrown when connection string is null or empty</exception>
    /// <remarks>
    /// This method registers all event-driven architecture services including event bus,
    /// event store, saga orchestration, and automatic handler discovery. It's a comprehensive
    /// setup for event-driven applications.
    /// </remarks>
    /// <example>
    /// <code>
    /// services.AddEventDrivenArchitecture("amqp://localhost", config =>
    /// {
    ///     config.EventBus.DefaultExchange = "events";
    ///     config.EventStore.StreamPrefix = "stream-";
    ///     config.Saga.EnableCompensation = true;
    /// });
    /// </code>
    /// </example>
    public static IServiceCollection AddEventDrivenArchitecture(
        this IServiceCollection services, 
        string connectionString, 
        Action<EventDrivenConfiguration>? configure = null)
    {
        if (string.IsNullOrWhiteSpace(connectionString))
            throw new ArgumentException("Connection string cannot be null or empty", nameof(connectionString));

        var config = new EventDrivenConfiguration();
        configure?.Invoke(config);

        // Register event bus
        services.AddEventBus(connectionString, eventBus =>
        {
            eventBus.DefaultExchange = config.EventBus.DefaultExchange;
            eventBus.EnableEventStore = config.EventStore.Enabled;
            eventBus.EnableDomainEvents = config.EventBus.EnableDomainEvents;
            eventBus.EnableIntegrationEvents = config.EventBus.EnableIntegrationEvents;
        });

        // Register event store if enabled
        if (config.EventStore.Enabled)
        {
            services.AddEventStore(connectionString, eventStore =>
            {
                eventStore.StreamPrefix = config.EventStore.StreamPrefix;
                eventStore.SnapshotFrequency = config.EventStore.SnapshotFrequency;
                eventStore.EnableSnapshots = config.EventStore.EnableSnapshots;
            });
        }

        // Register saga orchestration if enabled
        if (config.Saga.Enabled)
        {
            services.AddSagaOrchestration(connectionString, saga =>
            {
                saga.EnableCompensation = config.Saga.EnableCompensation;
                saga.TimeoutDuration = config.Saga.TimeoutDuration;
                saga.MaxRetries = config.Saga.MaxRetries;
            });
        }

        // Register event handlers if enabled
        if (config.AutoDiscoverHandlers)
        {
            services.AddEventHandlersFromCallingAssembly();
        }

        return services;
    }

    /// <summary>
    /// Adds complete event-driven architecture services with assemblies
    /// </summary>
    /// <param name="services">The service collection</param>
    /// <param name="connectionString">RabbitMQ connection string</param>
    /// <param name="handlerAssemblies">Assemblies containing event handlers</param>
    /// <param name="configure">Optional action to configure event-driven settings</param>
    /// <returns>The service collection for fluent configuration</returns>
    /// <exception cref="ArgumentException">Thrown when connection string is null or empty</exception>
    /// <remarks>
    /// This method registers all event-driven architecture services and automatically
    /// discovers event handlers from the specified assemblies. It provides complete
    /// event-driven architecture setup with handler discovery.
    /// </remarks>
    /// <example>
    /// <code>
    /// services.AddEventDrivenArchitecture("amqp://localhost", 
    ///     new[] { typeof(Program).Assembly, typeof(Handlers.DomainEventHandler).Assembly },
    ///     config =>
    ///     {
    ///         config.EventBus.DefaultExchange = "events";
    ///         config.EventStore.StreamPrefix = "stream-";
    ///     });
    /// </code>
    /// </example>
    public static IServiceCollection AddEventDrivenArchitecture(
        this IServiceCollection services, 
        string connectionString, 
        Assembly[] handlerAssemblies,
        Action<EventDrivenConfiguration>? configure = null)
    {
        ArgumentNullException.ThrowIfNull(handlerAssemblies);

        services.AddEventDrivenArchitecture(connectionString, configure);
        services.AddEventHandlersFromAssembly(handlerAssemblies);

        return services;
    }

    #endregion

    #region Validation Methods

    /// <summary>
    /// Validates event-driven service registration
    /// </summary>
    /// <param name="services">The service collection</param>
    /// <param name="throwOnValidationFailure">Whether to throw exception on validation failure</param>
    /// <returns>The service collection for fluent configuration</returns>
    /// <exception cref="InvalidOperationException">Thrown when validation fails and throwOnValidationFailure is true</exception>
    /// <remarks>
    /// This method validates that all required event-driven services are properly registered
    /// and configured. It checks for missing dependencies and configuration issues.
    /// </remarks>
    /// <example>
    /// <code>
    /// services.AddEventBus("amqp://localhost")
    ///     .ValidateEventDrivenServices();
    /// </code>
    /// </example>
    public static IServiceCollection ValidateEventDrivenServices(this IServiceCollection services, bool throwOnValidationFailure = true)
    {
        var errors = new List<string>();

        // Check for required event bus services
        if (!services.Any(s => s.ServiceType == typeof(IEventBus)))
        {
            errors.Add("IEventBus is not registered");
        }

        if (!services.Any(s => s.ServiceType == typeof(IEventPublisher)))
        {
            errors.Add("IEventPublisher is not registered");
        }

        if (!services.Any(s => s.ServiceType == typeof(IEventSubscriber)))
        {
            errors.Add("IEventSubscriber is not registered");
        }

        if (!services.Any(s => s.ServiceType == typeof(IEventHandlerRegistry)))
        {
            errors.Add("IEventHandlerRegistry is not registered");
        }

        if (!services.Any(s => s.ServiceType == typeof(IEventHandlerFactory)))
        {
            errors.Add("IEventHandlerFactory is not registered");
        }

        if (errors.Any() && throwOnValidationFailure)
        {
            throw new InvalidOperationException($"Event-driven service validation failed: {string.Join(", ", errors)}");
        }

        return services;
    }

    #endregion
}

/// <summary>
/// Configuration for event-driven architecture
/// </summary>
/// <remarks>
/// This class provides configuration options for event-driven architecture components
/// including event bus, event store, and saga orchestration settings.
/// </remarks>
public class EventDrivenConfiguration
{
    /// <summary>
    /// Event bus configuration
    /// </summary>
    public Configuration.EventBusSettings EventBus { get; set; } = new();

    /// <summary>
    /// Event store configuration
    /// </summary>
    public Configuration.EventStoreSettings EventStore { get; set; } = new();

    /// <summary>
    /// Saga orchestration configuration
    /// </summary>
    public SagaSettings Saga { get; set; } = new();

    /// <summary>
    /// Whether to automatically discover event handlers
    /// </summary>
    public bool AutoDiscoverHandlers { get; set; } = true;
}