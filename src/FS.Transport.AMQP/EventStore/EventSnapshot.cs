using FS.Transport.AMQP.Events;

namespace FS.Transport.AMQP.EventStore;

/// <summary>
/// Represents a snapshot of an event stored in the event store
/// </summary>
public class EventSnapshot
{
    /// <summary>
    /// Unique identifier for the event
    /// </summary>
    public string EventId { get; set; } = string.Empty;
    
    /// <summary>
    /// Aggregate identifier this event belongs to
    /// </summary>
    public string AggregateId { get; set; } = string.Empty;
    
    /// <summary>
    /// Type of the aggregate
    /// </summary>
    public string AggregateType { get; set; } = string.Empty;
    
    /// <summary>
    /// Version number of the aggregate after this event
    /// </summary>
    public long Version { get; set; }
    
    /// <summary>
    /// Sequence number of the event in the aggregate stream
    /// </summary>
    public long SequenceNumber { get; set; }
    
    /// <summary>
    /// Type of the event
    /// </summary>
    public string EventType { get; set; } = string.Empty;
    
    /// <summary>
    /// Serialized event data
    /// </summary>
    public string EventData { get; set; } = string.Empty;
    
    /// <summary>
    /// Serialized event metadata
    /// </summary>
    public string? MetaData { get; set; }
    
    /// <summary>
    /// Timestamp when the event was created
    /// </summary>
    public DateTimeOffset CreatedAt { get; set; }
    
    /// <summary>
    /// Timestamp when the event was stored
    /// </summary>
    public DateTimeOffset StoredAt { get; set; }
    
    /// <summary>
    /// Correlation ID for tracing
    /// </summary>
    public string? CorrelationId { get; set; }
    
    /// <summary>
    /// Causation ID for event sourcing
    /// </summary>
    public string? CausationId { get; set; }
    
    /// <summary>
    /// User or system that caused this event
    /// </summary>
    public string? CausedBy { get; set; }
    
    /// <summary>
    /// Additional event properties
    /// </summary>
    public Dictionary<string, object> Properties { get; set; } = new();
    
    /// <summary>
    /// Whether this event has been processed
    /// </summary>
    public bool IsProcessed { get; set; }
    
    /// <summary>
    /// Number of times this event has been processed (for retry scenarios)
    /// </summary>
    public int ProcessedCount { get; set; }
    
    /// <summary>
    /// Timestamp when the event was last processed
    /// </summary>
    public DateTimeOffset? LastProcessedAt { get; set; }
    
    /// <summary>
    /// Creates an EventSnapshot from an IEvent
    /// </summary>
    /// <param name="event">Event to create snapshot from</param>
    /// <param name="eventData">Serialized event data</param>
    /// <param name="aggregateId">Aggregate identifier</param>
    /// <param name="aggregateType">Aggregate type</param>
    /// <param name="version">Aggregate version</param>
    /// <param name="sequenceNumber">Sequence number</param>
    /// <returns>EventSnapshot instance</returns>
    public static EventSnapshot FromEvent(IEvent @event, string eventData, string aggregateId, string aggregateType, long version, long sequenceNumber)
    {
        if (@event == null)
            throw new ArgumentNullException(nameof(@event));
        if (string.IsNullOrEmpty(eventData))
            throw new ArgumentException("Event data cannot be null or empty", nameof(eventData));
        if (string.IsNullOrEmpty(aggregateId))
            throw new ArgumentException("Aggregate ID cannot be null or empty", nameof(aggregateId));
        if (string.IsNullOrEmpty(aggregateType))
            throw new ArgumentException("Aggregate type cannot be null or empty", nameof(aggregateType));
        
        var snapshot = new EventSnapshot
        {
            EventId = @event.Id.ToString(),
            AggregateId = aggregateId,
            AggregateType = aggregateType,
            Version = version,
            SequenceNumber = sequenceNumber,
            EventType = @event.EventType,
            EventData = eventData,
            CreatedAt = @event.OccurredOn,
            StoredAt = DateTimeOffset.UtcNow,
            CorrelationId = @event.CorrelationId,
            CausationId = @event.CausationId
        };
        
        // Copy metadata if available
        if (@event.Metadata != null)
        {
            foreach (var kvp in @event.Metadata)
            {
                snapshot.Properties[kvp.Key] = kvp.Value;
            }
        }
        
        return snapshot;
    }
}