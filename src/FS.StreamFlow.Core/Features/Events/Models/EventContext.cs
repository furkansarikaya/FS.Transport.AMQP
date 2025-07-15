namespace FS.StreamFlow.Core.Features.Events.Models;

/// <summary>
/// Represents event context for event handling
/// </summary>
public class EventContext
{
    /// <summary>
    /// Event identifier
    /// </summary>
    public string EventId { get; set; } = Guid.NewGuid().ToString();
    
    /// <summary>
    /// Event type
    /// </summary>
    public string EventType { get; set; } = string.Empty;
    
    /// <summary>
    /// Event version
    /// </summary>
    public int Version { get; set; } = 1;
    
    /// <summary>
    /// Event timestamp
    /// </summary>
    public DateTimeOffset Timestamp { get; set; } = DateTimeOffset.UtcNow;
    
    /// <summary>
    /// Correlation identifier
    /// </summary>
    public string? CorrelationId { get; set; }
    
    /// <summary>
    /// Causation identifier
    /// </summary>
    public string? CausationId { get; set; }
    
    /// <summary>
    /// Event source
    /// </summary>
    public string? Source { get; set; }
    
    /// <summary>
    /// Event subject
    /// </summary>
    public string? Subject { get; set; }
    
    /// <summary>
    /// Event data content type
    /// </summary>
    public string? DataContentType { get; set; }
    
    /// <summary>
    /// Event data schema
    /// </summary>
    public string? DataSchema { get; set; }
    
    /// <summary>
    /// Event metadata
    /// </summary>
    public Dictionary<string, object>? Metadata { get; set; }
    
    /// <summary>
    /// Event headers
    /// </summary>
    public Dictionary<string, object>? Headers { get; set; }
    
    /// <summary>
    /// Event extensions
    /// </summary>
    public Dictionary<string, object>? Extensions { get; set; }
    
    /// <summary>
    /// Whether the event is domain event
    /// </summary>
    public bool IsDomainEvent { get; set; } = false;
    
    /// <summary>
    /// Whether the event is integration event
    /// </summary>
    public bool IsIntegrationEvent { get; set; } = false;
    
    /// <summary>
    /// Event processing attempt count
    /// </summary>
    public int AttemptCount { get; set; } = 1;
    
    /// <summary>
    /// Event processing status
    /// </summary>
    public EventProcessingStatus ProcessingStatus { get; set; } = EventProcessingStatus.Pending;
    
    /// <summary>
    /// Event processing start time
    /// </summary>
    public DateTimeOffset? ProcessingStartedAt { get; set; }
    
    /// <summary>
    /// Event processing end time
    /// </summary>
    public DateTimeOffset? ProcessingCompletedAt { get; set; }
    
    /// <summary>
    /// Event processing duration
    /// </summary>
    public TimeSpan? ProcessingDuration => ProcessingCompletedAt - ProcessingStartedAt;
    
    /// <summary>
    /// Event processing error
    /// </summary>
    public Exception? ProcessingError { get; set; }
    
    /// <summary>
    /// Creates a new EventContext instance
    /// </summary>
    public EventContext()
    {
        Metadata = new Dictionary<string, object>();
        Headers = new Dictionary<string, object>();
        Extensions = new Dictionary<string, object>();
    }
    
    /// <summary>
    /// Creates a new EventContext instance with event type
    /// </summary>
    /// <param name="eventType">Event type</param>
    /// <param name="correlationId">Correlation identifier</param>
    /// <param name="causationId">Causation identifier</param>
    public EventContext(string eventType, string? correlationId = null, string? causationId = null)
        : this()
    {
        EventType = eventType;
        CorrelationId = correlationId;
        CausationId = causationId;
    }
}

/// <summary>
/// Represents event processing status
/// </summary>
public enum EventProcessingStatus
{
    /// <summary>
    /// Event is pending processing
    /// </summary>
    Pending,
    
    /// <summary>
    /// Event is being processed
    /// </summary>
    Processing,
    
    /// <summary>
    /// Event has been processed successfully
    /// </summary>
    Processed,
    
    /// <summary>
    /// Event processing failed
    /// </summary>
    Failed,
    
    /// <summary>
    /// Event was skipped
    /// </summary>
    Skipped,
    
    /// <summary>
    /// Event is being retried
    /// </summary>
    Retrying
}
