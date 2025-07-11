using FS.RabbitMQ.Events;
using Microsoft.Extensions.DependencyInjection;

namespace FS.RabbitMQ.EventHandlers;

/// <summary>
/// Registry for managing event handlers with discovery, registration, and retrieval capabilities
/// </summary>
public interface IEventHandlerRegistry
{
    /// <summary>
    /// Registers an event handler instance
    /// </summary>
    /// <param name="handler">Handler instance</param>
    void RegisterHandler(IEventHandler handler);
    
    /// <summary>
    /// Registers an event handler type for dependency injection
    /// </summary>
    /// <typeparam name="THandler">Handler type</typeparam>
    /// <param name="serviceLifetime">Service lifetime</param>
    void RegisterHandler<THandler>(ServiceLifetime serviceLifetime = ServiceLifetime.Scoped) where THandler : class, IEventHandler;
    
    /// <summary>
    /// Unregisters an event handler
    /// </summary>
    /// <param name="handlerName">Handler name</param>
    /// <returns>True if handler was unregistered</returns>
    bool UnregisterHandler(string handlerName);
    
    /// <summary>
    /// Gets all handlers for a specific event type
    /// </summary>
    /// <param name="eventType">Event type</param>
    /// <returns>Collection of matching handlers</returns>
    IEnumerable<IEventHandler> GetHandlersForEvent(Type eventType);
    
    /// <summary>
    /// Gets all handlers for a specific event type
    /// </summary>
    /// <typeparam name="T">Event type</typeparam>
    /// <returns>Collection of matching handlers</returns>
    IEnumerable<IEventHandler> GetHandlersForEvent<T>() where T : class, IEvent;
    
    /// <summary>
    /// Gets a handler by name
    /// </summary>
    /// <param name="handlerName">Handler name</param>
    /// <returns>Handler instance or null if not found</returns>
    IEventHandler? GetHandler(string handlerName);
    
    /// <summary>
    /// Gets all registered handlers
    /// </summary>
    /// <returns>Collection of all handlers</returns>
    IEnumerable<IEventHandler> GetAllHandlers();
    
    /// <summary>
    /// Gets handler registration statistics
    /// </summary>
    /// <returns>Registry statistics</returns>
    EventHandlerRegistryStatistics GetStatistics();
    
    /// <summary>
    /// Discovers and registers handlers from assemblies
    /// </summary>
    /// <param name="assemblies">Assemblies to scan</param>
    /// <returns>Number of handlers discovered and registered</returns>
    int DiscoverAndRegisterHandlers(params System.Reflection.Assembly[] assemblies);
}