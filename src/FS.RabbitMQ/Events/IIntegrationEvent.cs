namespace FS.RabbitMQ.Events;

/// <summary>
/// Interface for integration events that cross bounded context boundaries
/// Integration events are used for communication between different services/contexts
/// </summary>
public interface IIntegrationEvent : IEvent
{
    /// <summary>
    /// Source service or bounded context that published this event
    /// </summary>
    string Source { get; }
    
    /// <summary>
    /// Routing key pattern for message distribution
    /// </summary>
    string RoutingKey { get; }
    
    /// <summary>
    /// Target audience for this integration event (optional)
    /// </summary>
    string? Target { get; }
    
    /// <summary>
    /// Event schema version for external compatibility
    /// </summary>
    string SchemaVersion { get; }
    
    /// <summary>
    /// Time to live for this event (optional)
    /// </summary>
    TimeSpan? TimeToLive { get; }
}