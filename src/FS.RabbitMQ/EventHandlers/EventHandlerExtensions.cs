using FS.RabbitMQ.Events;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace FS.RabbitMQ.EventHandlers;

/// <summary>
/// Extension methods for event handler configuration and utilities
/// </summary>
public static class EventHandlerExtensions
{
    /// <summary>
    /// Adds event handler services to the service collection
    /// </summary>
    /// <param name="services">Service collection</param>
    /// <returns>Service collection for fluent configuration</returns>
    public static IServiceCollection AddEventHandlers(this IServiceCollection services)
    {
        services.TryAddSingleton<IEventHandlerRegistry, EventHandlerRegistry>();
        services.TryAddSingleton<IEventHandlerFactory, EventHandlerFactory>();
        
        return services;
    }

    /// <summary>
    /// Registers an event handler
    /// </summary>
    /// <typeparam name="THandler">Handler type</typeparam>
    /// <param name="services">Service collection</param>
    /// <param name="serviceLifetime">Service lifetime</param>
    /// <returns>Service collection for fluent configuration</returns>
    public static IServiceCollection AddEventHandler<THandler>(this IServiceCollection services, 
        ServiceLifetime serviceLifetime = ServiceLifetime.Scoped) 
        where THandler : class, IEventHandler
    {
        // Register the handler type in DI container
        services.Add(new ServiceDescriptor(typeof(THandler), typeof(THandler), serviceLifetime));
        
        // Register with the registry (will be done during app startup)
        services.Configure<EventHandlerRegistrationOptions>(options =>
        {
            options.HandlerTypes.Add(typeof(THandler));
        });
        
        return services;
    }

    /// <summary>
    /// Registers multiple event handlers from an assembly
    /// </summary>
    /// <param name="services">Service collection</param>
    /// <param name="assembly">Assembly to scan</param>
    /// <param name="serviceLifetime">Service lifetime for handlers</param>
    /// <returns>Service collection for fluent configuration</returns>
    public static IServiceCollection AddEventHandlersFromAssembly(this IServiceCollection services, 
        System.Reflection.Assembly assembly, 
        ServiceLifetime serviceLifetime = ServiceLifetime.Scoped)
    {
        var handlerTypes = assembly.GetTypes()
            .Where(t => t.IsClass && !t.IsAbstract)
            .Where(t => typeof(IEventHandler).IsAssignableFrom(t))
            .ToList();

        foreach (var handlerType in handlerTypes)
        {
            // Register the handler type in DI container
            services.Add(new ServiceDescriptor(handlerType, handlerType, serviceLifetime));
            
            // Add to registration options
            services.Configure<EventHandlerRegistrationOptions>(options =>
            {
                options.HandlerTypes.Add(handlerType);
            });
        }

        return services;
    }

    /// <summary>
    /// Registers event handlers from the calling assembly
    /// </summary>
    /// <param name="services">Service collection</param>
    /// <param name="serviceLifetime">Service lifetime for handlers</param>
    /// <returns>Service collection for fluent configuration</returns>
    public static IServiceCollection AddEventHandlersFromCallingAssembly(this IServiceCollection services, 
        ServiceLifetime serviceLifetime = ServiceLifetime.Scoped)
    {
        var callingAssembly = System.Reflection.Assembly.GetCallingAssembly();
        return services.AddEventHandlersFromAssembly(callingAssembly, serviceLifetime);
    }

    /// <summary>
    /// Configures event handler execution settings
    /// </summary>
    /// <param name="services">Service collection</param>
    /// <param name="maxConcurrentHandlers">Maximum concurrent handlers</param>
    /// <param name="handlerTimeout">Handler execution timeout</param>
    /// <returns>Service collection for fluent configuration</returns>
    public static IServiceCollection ConfigureEventHandlerExecution(this IServiceCollection services,
        int maxConcurrentHandlers = 0, // 0 = use processor count * 2
        TimeSpan? handlerTimeout = null)
    {
        services.Configure<EventHandlerExecutionOptions>(options =>
        {
            options.MaxConcurrentHandlers = maxConcurrentHandlers > 0 ? maxConcurrentHandlers : Environment.ProcessorCount * 2;
            options.HandlerTimeout = handlerTimeout ?? TimeSpan.FromSeconds(30);
        });
        
        return services;
    }

    /// <summary>
    /// Executes all handlers for an event with the factory
    /// </summary>
    /// <param name="factory">Event handler factory</param>
    /// <param name="event">Event to handle</param>
    /// <param name="source">Source identifier</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Collection of handling results</returns>
    public static async Task<IEnumerable<EventHandlingResult>> HandleEventAsync(this IEventHandlerFactory factory, 
        IEvent @event, 
        string source = "direct", 
        CancellationToken cancellationToken = default)
    {
        var context = EventContext.FromDirect(@event, source, cancellationToken);
        return await factory.ExecuteHandlersAsync(@event, context, cancellationToken);
    }

    /// <summary>
    /// Executes handlers for an event from message context
    /// </summary>
    /// <param name="factory">Event handler factory</param>
    /// <param name="event">Event to handle</param>
    /// <param name="exchange">Exchange name</param>
    /// <param name="routingKey">Routing key</param>
    /// <param name="queue">Queue name</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Collection of handling results</returns>
    public static async Task<IEnumerable<EventHandlingResult>> HandleMessageEventAsync(this IEventHandlerFactory factory,
        IEvent @event,
        string? exchange,
        string? routingKey,
        string? queue,
        CancellationToken cancellationToken = default)
    {
        var context = EventContext.FromMessage(@event, exchange, routingKey, queue, cancellationToken);
        return await factory.ExecuteHandlersAsync(@event, context, cancellationToken);
    }

    /// <summary>
    /// Determines if any handlers are registered for an event type
    /// </summary>
    /// <typeparam name="T">Event type</typeparam>
    /// <param name="registry">Event handler registry</param>
    /// <returns>True if handlers are registered</returns>
    public static bool HasHandlersFor<T>(this IEventHandlerRegistry registry) where T : class, IEvent
    {
        return registry.GetHandlersForEvent<T>().Any();
    }

    /// <summary>
    /// Determines if any handlers are registered for an event type
    /// </summary>
    /// <param name="registry">Event handler registry</param>
    /// <param name="eventType">Event type</param>
    /// <returns>True if handlers are registered</returns>
    public static bool HasHandlersFor(this IEventHandlerRegistry registry, Type eventType)
    {
        return registry.GetHandlersForEvent(eventType).Any();
    }

    /// <summary>
    /// Gets handler names for an event type
    /// </summary>
    /// <typeparam name="T">Event type</typeparam>
    /// <param name="registry">Event handler registry</param>
    /// <returns>Collection of handler names</returns>
    public static IEnumerable<string> GetHandlerNamesFor<T>(this IEventHandlerRegistry registry) where T : class, IEvent
    {
        return registry.GetHandlersForEvent<T>().Select(h => h.HandlerName);
    }

    /// <summary>
    /// Registers handlers based on naming conventions
    /// </summary>
    /// <param name="registry">Event handler registry</param>
    /// <param name="assemblies">Assemblies to scan</param>
    /// <param name="handlerSuffix">Handler class name suffix</param>
    /// <returns>Number of handlers registered</returns>
    public static int RegisterHandlersByConvention(this IEventHandlerRegistry registry, 
        string handlerSuffix = "Handler", 
        params System.Reflection.Assembly[] assemblies)
    {
        if (assemblies.Length == 0)
        {
            assemblies = [System.Reflection.Assembly.GetCallingAssembly()];
        }

        var registeredCount = 0;

        foreach (var assembly in assemblies)
        {
            var handlerTypes = assembly.GetTypes()
                .Where(t => t is { IsClass: true, IsAbstract: false })
                .Where(t => t.Name.EndsWith(handlerSuffix, StringComparison.OrdinalIgnoreCase))
                .Where(t => typeof(IEventHandler).IsAssignableFrom(t))
                .ToList();

            foreach (var handlerType in handlerTypes)
            {
                try
                {
                    // Use reflection to call the generic RegisterHandler<T> method
                    var method = typeof(EventHandlerRegistry).GetMethod("RegisterHandler", [typeof(ServiceLifetime)]);
                    if (method == null) continue;
                    var genericMethod = method.MakeGenericMethod(handlerType);
                    genericMethod.Invoke(registry, [ServiceLifetime.Scoped]);
                    registeredCount++;
                }
                catch
                {
                    // Continue with other handlers if one fails
                }
            }
        }

        return registeredCount;
    }

    /// <summary>
    /// Validates that an event handler is properly configured
    /// </summary>
    /// <param name="handler">Handler to validate</param>
    /// <returns>Validation result</returns>
    public static EventHandlerValidationResult Validate(this IEventHandler handler)
    {
        var result = new EventHandlerValidationResult();

        if (handler == null)
        {
            result.AddError("Handler cannot be null");
            return result;
        }

        if (string.IsNullOrWhiteSpace(handler.HandlerName))
            result.AddError("Handler name cannot be null or empty");

        if (handler.EventTypes == null || !handler.EventTypes.Any())
            result.AddError("Handler must specify at least one event type");

        if (handler.EventTypes?.Any(t => !typeof(IEvent).IsAssignableFrom(t)) == true)
            result.AddError("All event types must implement IEvent");

        // Check that handler implements appropriate interfaces
        var handlerType = handler.GetType();
        var hasAsyncInterface = false;
        var hasSyncInterface = false;

        foreach (var eventType in handler.EventTypes ?? Array.Empty<Type>())
        {
            var asyncInterface = typeof(IAsyncEventHandler<>).MakeGenericType(eventType);
            var syncInterface = typeof(IEventHandler<>).MakeGenericType(eventType);

            if (asyncInterface.IsAssignableFrom(handlerType))
                hasAsyncInterface = true;

            if (syncInterface.IsAssignableFrom(handlerType))
                hasSyncInterface = true;
        }

        if (!hasAsyncInterface && !hasSyncInterface)
            result.AddError("Handler must implement IEventHandler<T> or IAsyncEventHandler<T>");

        return result;
    }
}