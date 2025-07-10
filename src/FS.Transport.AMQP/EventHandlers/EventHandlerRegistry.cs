using System.Collections.Concurrent;
using FS.Transport.AMQP.Events;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace FS.Transport.AMQP.EventHandlers;

/// <summary>
/// Default implementation of event handler registry
/// </summary>
public class EventHandlerRegistry : IEventHandlerRegistry
{
    private readonly ConcurrentDictionary<string, IEventHandler> _handlers;
    private readonly ConcurrentDictionary<Type, List<string>> _eventTypeToHandlers;
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<EventHandlerRegistry> _logger;
    private readonly object _lock = new();

    public EventHandlerRegistry(IServiceProvider serviceProvider, ILogger<EventHandlerRegistry> logger)
    {
        _handlers = new ConcurrentDictionary<string, IEventHandler>();
        _eventTypeToHandlers = new ConcurrentDictionary<Type, List<string>>();
        _serviceProvider = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>
    /// Registers an event handler instance
    /// </summary>
    public void RegisterHandler(IEventHandler handler)
    {
        if (handler == null)
            throw new ArgumentNullException(nameof(handler));

        lock (_lock)
        {
            var handlerName = handler.HandlerName;
            
            _handlers.AddOrUpdate(handlerName, handler, (key, existing) =>
            {
                _logger.LogWarning("Replacing existing handler {HandlerName}", handlerName);
                return handler;
            });

            // Update event type mappings
            foreach (var eventType in handler.EventTypes)
            {
                _eventTypeToHandlers.AddOrUpdate(
                    eventType,
                    new List<string> { handlerName },
                    (key, existing) =>
                    {
                        if (!existing.Contains(handlerName))
                        {
                            existing.Add(handlerName);
                        }
                        return existing;
                    });
            }

            _logger.LogInformation("Registered event handler {HandlerName} for event types: {EventTypes}", 
                handlerName, string.Join(", ", handler.EventTypes.Select(t => t.Name)));
        }
    }

    /// <summary>
    /// Registers an event handler type for dependency injection
    /// </summary>
    public void RegisterHandler<THandler>(ServiceLifetime serviceLifetime = ServiceLifetime.Scoped) where THandler : class, IEventHandler
    {
        try
        {
            // Create a temporary instance to get handler information
            var tempHandler = ActivatorUtilities.CreateInstance<THandler>(_serviceProvider);
            
            // Register the type mapping for later instantiation
            var handlerName = tempHandler.HandlerName;
            
            // Create a factory that resolves from DI container
            var factoryHandler = new DependencyInjectionEventHandler(typeof(THandler), handlerName, tempHandler.EventTypes, _serviceProvider);
            
            RegisterHandler(factoryHandler);
            
            _logger.LogInformation("Registered DI event handler type {HandlerType} with name {HandlerName}", 
                typeof(THandler).Name, handlerName);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to register event handler type {HandlerType}", typeof(THandler).Name);
            throw;
        }
    }

    /// <summary>
    /// Unregisters an event handler
    /// </summary>
    public bool UnregisterHandler(string handlerName)
    {
        if (string.IsNullOrWhiteSpace(handlerName))
            return false;

        lock (_lock)
        {
            if (!_handlers.TryRemove(handlerName, out var handler))
                return false;

            // Remove from event type mappings
            foreach (var eventType in handler.EventTypes)
            {
                if (_eventTypeToHandlers.TryGetValue(eventType, out var handlerList))
                {
                    handlerList.Remove(handlerName);
                    if (handlerList.Count == 0)
                    {
                        _eventTypeToHandlers.TryRemove(eventType, out _);
                    }
                }
            }

            _logger.LogInformation("Unregistered event handler {HandlerName}", handlerName);
            return true;
        }
    }

    /// <summary>
    /// Gets all handlers for a specific event type
    /// </summary>
    public IEnumerable<IEventHandler> GetHandlersForEvent(Type eventType)
    {
        if (eventType == null)
            return Enumerable.Empty<IEventHandler>();

        var handlerNames = new List<string>();

        // Direct type match
        if (_eventTypeToHandlers.TryGetValue(eventType, out var directHandlers))
        {
            handlerNames.AddRange(directHandlers);
        }

        // Check inheritance hierarchy and interfaces
        foreach (var kvp in _eventTypeToHandlers)
        {
            if (kvp.Key != eventType && kvp.Key.IsAssignableFrom(eventType))
            {
                handlerNames.AddRange(kvp.Value);
            }
        }

        // Get unique handler instances ordered by priority
        return handlerNames
            .Distinct()
            .Select(name => _handlers.TryGetValue(name, out var handler) ? handler : null)
            .Where(h => h != null)
            .OrderByDescending(h => h!.Priority)
            .ToList()!;
    }

    /// <summary>
    /// Gets all handlers for a specific event type
    /// </summary>
    public IEnumerable<IEventHandler> GetHandlersForEvent<T>() where T : class, IEvent
    {
        return GetHandlersForEvent(typeof(T));
    }

    /// <summary>
    /// Gets a handler by name
    /// </summary>
    public IEventHandler? GetHandler(string handlerName)
    {
        if (string.IsNullOrWhiteSpace(handlerName))
            return null;

        return _handlers.TryGetValue(handlerName, out var handler) ? handler : null;
    }

    /// <summary>
    /// Gets all registered handlers
    /// </summary>
    public IEnumerable<IEventHandler> GetAllHandlers()
    {
        return _handlers.Values.ToList();
    }

    /// <summary>
    /// Gets handler registration statistics
    /// </summary>
    public EventHandlerRegistryStatistics GetStatistics()
    {
        return new EventHandlerRegistryStatistics
        {
            TotalHandlers = _handlers.Count,
            EventTypes = _eventTypeToHandlers.Keys.Count,
            AverageHandlersPerEventType = _eventTypeToHandlers.Any() 
                ? _eventTypeToHandlers.Values.Average(list => list.Count) 
                : 0,
            HandlerDetails = _handlers.Values.Select(h => new HandlerDetail
            {
                Name = h.HandlerName,
                EventTypes = h.EventTypes.Select(t => t.Name).ToList(),
                Priority = h.Priority,
                AllowConcurrentExecution = h.AllowConcurrentExecution
            }).ToList()
        };
    }

    /// <summary>
    /// Discovers and registers handlers from assemblies
    /// </summary>
    public int DiscoverAndRegisterHandlers(params System.Reflection.Assembly[] assemblies)
    {
        if (assemblies == null || assemblies.Length == 0)
        {
            assemblies = new[] { System.Reflection.Assembly.GetEntryAssembly() ?? System.Reflection.Assembly.GetExecutingAssembly() };
        }

        var registeredCount = 0;

        foreach (var assembly in assemblies)
        {
            try
            {
                var handlerTypes = assembly.GetTypes()
                    .Where(t => t.IsClass && !t.IsAbstract)
                    .Where(t => typeof(IEventHandler).IsAssignableFrom(t))
                    .ToList();

                foreach (var handlerType in handlerTypes)
                {
                    try
                    {
                        // Use reflection to call RegisterHandler<T>
                        var method = typeof(EventHandlerRegistry).GetMethod(nameof(RegisterHandler), new[] { typeof(ServiceLifetime) });
                        var genericMethod = method?.MakeGenericMethod(handlerType);
                        genericMethod?.Invoke(this, new object[] { ServiceLifetime.Scoped });
                        
                        registeredCount++;
                    }
                    catch (Exception ex)
                    {
                        _logger.LogWarning(ex, "Failed to register handler type {HandlerType} from assembly {Assembly}", 
                            handlerType.Name, assembly.GetName().Name);
                    }
                }

                _logger.LogInformation("Discovered and registered {Count} event handlers from assembly {Assembly}", 
                    handlerTypes.Count, assembly.GetName().Name);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to scan assembly {Assembly} for event handlers", assembly.GetName().Name);
            }
        }

        return registeredCount;
    }
}