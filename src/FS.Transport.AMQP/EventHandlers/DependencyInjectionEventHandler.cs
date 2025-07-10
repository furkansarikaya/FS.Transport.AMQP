using Microsoft.Extensions.DependencyInjection;

namespace FS.Transport.AMQP.EventHandlers;

/// <summary>
/// Event handler that resolves actual handlers from DI container
/// </summary>
internal class DependencyInjectionEventHandler : IEventHandler
{
    private readonly Type _handlerType;
    private readonly IServiceProvider _serviceProvider;

    public Type[] EventTypes { get; }
    public string HandlerName { get; }
    public int Priority { get; }
    public bool AllowConcurrentExecution { get; }

    public DependencyInjectionEventHandler(Type handlerType, string handlerName, Type[] eventTypes, IServiceProvider serviceProvider)
    {
        _handlerType = handlerType;
        _serviceProvider = serviceProvider;
        HandlerName = handlerName;
        EventTypes = eventTypes;
        
        // Try to get default values from a temporary instance
        try
        {
            var tempInstance = ActivatorUtilities.CreateInstance(serviceProvider, handlerType) as IEventHandler;
            Priority = tempInstance?.Priority ?? 0;
            AllowConcurrentExecution = tempInstance?.AllowConcurrentExecution ?? true;
        }
        catch
        {
            Priority = 0;
            AllowConcurrentExecution = true;
        }
    }

    public bool CanHandle(Type eventType)
    {
        return EventTypes.Any(t => t.IsAssignableFrom(eventType));
    }

    /// <summary>
    /// Resolves the actual handler from DI container
    /// </summary>
    /// <returns>Handler instance</returns>
    public IEventHandler ResolveHandler()
    {
        return (IEventHandler)ActivatorUtilities.CreateInstance(_serviceProvider, _handlerType);
    }
}