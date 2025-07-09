namespace FS.Transport.AMQP.Events;

/// <summary>
/// Interface for domain events that occur within a bounded context
/// Domain events represent something important that happened in the domain
/// </summary>
public interface IDomainEvent : IEvent
{
    /// <summary>
    /// Identifier of the aggregate that raised the event
    /// </summary>
    string AggregateId { get; }
    
    /// <summary>
    /// Type name of the aggregate that raised the event
    /// </summary>
    string AggregateType { get; }
    
    /// <summary>
    /// Version of the aggregate when this event was raised
    /// </summary>
    long AggregateVersion { get; }
    
    /// <summary>
    /// User or system that initiated the action leading to this event
    /// </summary>
    string? InitiatedBy { get; }
}