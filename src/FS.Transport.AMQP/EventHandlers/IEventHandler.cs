using FS.Transport.AMQP.Events;

namespace FS.Transport.AMQP.EventHandlers;

/// <summary>
/// Non-generic base interface for all event handlers
/// </summary>
public interface IEventHandler
{
    /// <summary>
    /// Event types this handler can process
    /// </summary>
    Type[] EventTypes { get; }
    
    /// <summary>
    /// Handler name for identification and logging
    /// </summary>
    string HandlerName { get; }
    
    /// <summary>
    /// Handler priority for ordering (higher values execute first)
    /// </summary>
    int Priority { get; }
    
    /// <summary>
    /// Whether this handler can handle the specified event type
    /// </summary>
    /// <param name="eventType">Event type to check</param>
    /// <returns>True if handler can process this event type</returns>
    bool CanHandle(Type eventType);
    
    /// <summary>
    /// Whether this handler should run concurrently with other handlers
    /// </summary>
    bool AllowConcurrentExecution { get; }
}

/// <summary>
/// Generic synchronous event handler interface
/// </summary>
/// <typeparam name="T">Event type</typeparam>
public interface IEventHandler<in T> : IEventHandler where T : class, IEvent
{
    /// <summary>
    /// Handles the event synchronously
    /// </summary>
    /// <param name="event">Event instance</param>
    /// <param name="context">Event handling context</param>
    void Handle(T @event, EventContext context);
}