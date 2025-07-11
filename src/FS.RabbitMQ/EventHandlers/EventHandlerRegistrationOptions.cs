namespace FS.RabbitMQ.EventHandlers;

/// <summary>
/// Options for event handler registration
/// </summary>
public class EventHandlerRegistrationOptions
{
    /// <summary>
    /// Handler types to register
    /// </summary>
    public List<Type> HandlerTypes { get; set; } = new();
}