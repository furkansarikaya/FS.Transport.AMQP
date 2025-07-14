namespace FS.StreamFlow.Core.Features.Events.Models;

/// <summary>
/// Event bus statistics and metrics
/// </summary>
public class EventBusStatistics
{
    /// <summary>
    /// Event bus name/identifier
    /// </summary>
    public string Name { get; set; } = "DefaultEventBus";
    
    /// <summary>
    /// Current status of the event bus
    /// </summary>
    public EventBusStatus Status { get; set; } = EventBusStatus.NotInitialized;
    
    /// <summary>
    /// When the event bus was started
    /// </summary>
    public DateTimeOffset? StartedAt { get; set; }
    
    /// <summary>
    /// Event bus uptime
    /// </summary>
    public TimeSpan Uptime => StartedAt.HasValue ? DateTimeOffset.UtcNow - StartedAt.Value : TimeSpan.Zero;
    
    /// <summary>
    /// Total number of events published
    /// </summary>
    public long TotalEventsPublished { get; set; }
    
    /// <summary>
    /// Total number of events received
    /// </summary>
    public long TotalEventsReceived { get; set; }
    
    /// <summary>
    /// Total number of events processed successfully
    /// </summary>
    public long TotalEventsProcessedSuccessfully { get; set; }
    
    /// <summary>
    /// Total number of events that failed processing
    /// </summary>
    public long TotalEventsFailedProcessing { get; set; }
    
    /// <summary>
    /// Number of active subscriptions
    /// </summary>
    public int ActiveSubscriptions { get; set; }
    
    /// <summary>
    /// Number of events currently being processed
    /// </summary>
    public int CurrentlyProcessingEvents { get; set; }
    
    /// <summary>
    /// Average event processing time in milliseconds
    /// </summary>
    public double AverageProcessingTimeMs { get; set; }
    
    /// <summary>
    /// Events per second rate
    /// </summary>
    public double EventsPerSecond { get; set; }
    
    /// <summary>
    /// Success rate percentage (0-100)
    /// </summary>
    public double SuccessRate => TotalEventsReceived > 0 
        ? (double)TotalEventsProcessedSuccessfully / TotalEventsReceived * 100 
        : 0;
    
    /// <summary>
    /// Last event published timestamp
    /// </summary>
    public DateTimeOffset? LastEventPublished { get; set; }
    
    /// <summary>
    /// Last event received timestamp
    /// </summary>
    public DateTimeOffset? LastEventReceived { get; set; }
    
    /// <summary>
    /// Memory usage in bytes
    /// </summary>
    public long MemoryUsageBytes { get; set; }
    
    /// <summary>
    /// Connection status
    /// </summary>
    public bool IsConnected { get; set; }
    
    /// <summary>
    /// Number of connection retries
    /// </summary>
    public int ConnectionRetries { get; set; }
    
    /// <summary>
    /// Last error message (if any)
    /// </summary>
    public string? LastErrorMessage { get; set; }
    
    /// <summary>
    /// Last error timestamp
    /// </summary>
    public DateTimeOffset? LastErrorAt { get; set; }
} 