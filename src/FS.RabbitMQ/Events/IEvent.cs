namespace FS.RabbitMQ.Events;

/// <summary>
/// Base interface for all events in the system
/// </summary>
public interface IEvent
{
    /// <summary>
    /// Unique identifier for the event
    /// </summary>
    Guid Id { get; }
    
    /// <summary>
    /// Timestamp when the event occurred (UTC)
    /// </summary>
    DateTime OccurredOn { get; }
    
    /// <summary>
    /// Event version for schema evolution and compatibility
    /// </summary>
    int Version { get; }
    
    /// <summary>
    /// Event type name for routing and handler resolution
    /// </summary>
    string EventType { get; }
    
    /// <summary>
    /// Correlation identifier for tracing related events
    /// </summary>
    string? CorrelationId { get; }
    
    /// <summary>
    /// Causation identifier linking to the command or event that caused this event
    /// </summary>
    string? CausationId { get; }
    
    /// <summary>
    /// Additional metadata associated with the event
    /// </summary>
    IDictionary<string, object> Metadata { get; }
}