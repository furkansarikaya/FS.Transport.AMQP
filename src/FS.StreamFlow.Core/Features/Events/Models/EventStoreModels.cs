namespace FS.StreamFlow.Core.Features.Events.Models;

/// <summary>
/// Represents the status of an event store
/// </summary>
public enum EventStoreStatus
{
    /// <summary>
    /// Event store is not initialized
    /// </summary>
    NotInitialized,
    
    /// <summary>
    /// Event store is initializing
    /// </summary>
    Initializing,
    
    /// <summary>
    /// Event store is ready for operations
    /// </summary>
    Ready,
    
    /// <summary>
    /// Event store is shutting down
    /// </summary>
    ShuttingDown,
    
    /// <summary>
    /// Event store is shut down
    /// </summary>
    ShutDown,
    
    /// <summary>
    /// Event store is in an error state
    /// </summary>
    Error
}

/// <summary>
/// Represents the status of an event stream
/// </summary>
public enum EventStreamStatus
{
    /// <summary>
    /// Stream is active and ready for operations
    /// </summary>
    Active,
    
    /// <summary>
    /// Stream is being created
    /// </summary>
    Creating,
    
    /// <summary>
    /// Stream is being deleted
    /// </summary>
    Deleting,
    
    /// <summary>
    /// Stream is deleted
    /// </summary>
    Deleted,
    
    /// <summary>
    /// Stream is in an error state
    /// </summary>
    Error
}

/// <summary>
/// Represents event store statistics
/// </summary>
public class EventStoreStatistics
{
    /// <summary>
    /// Total number of streams
    /// </summary>
    public long TotalStreams { get; set; }
    
    /// <summary>
    /// Total number of events stored
    /// </summary>
    public long TotalEvents { get; set; }
    
    /// <summary>
    /// Total number of snapshots stored
    /// </summary>
    public long TotalSnapshots { get; set; }
    
    /// <summary>
    /// Total size of stored data in bytes
    /// </summary>
    public long TotalDataSize { get; set; }
    
    /// <summary>
    /// Number of active streams
    /// </summary>
    public long ActiveStreams { get; set; }
    
    /// <summary>
    /// Number of events written in the last period
    /// </summary>
    public long EventsWritten { get; set; }
    
    /// <summary>
    /// Number of events read in the last period
    /// </summary>
    public long EventsRead { get; set; }
    
    /// <summary>
    /// Average events per stream
    /// </summary>
    public double AverageEventsPerStream => TotalStreams > 0 ? (double)TotalEvents / TotalStreams : 0;
    
    /// <summary>
    /// Average event size in bytes
    /// </summary>
    public double AverageEventSize => TotalEvents > 0 ? (double)TotalDataSize / TotalEvents : 0;
    
    /// <summary>
    /// Events per second (write rate)
    /// </summary>
    public double EventsPerSecond { get; set; }
    
    /// <summary>
    /// Last event write timestamp
    /// </summary>
    public DateTimeOffset? LastEventWriteAt { get; set; }
    
    /// <summary>
    /// Last event read timestamp
    /// </summary>
    public DateTimeOffset? LastEventReadAt { get; set; }
    
    /// <summary>
    /// Event store uptime
    /// </summary>
    public TimeSpan Uptime { get; set; }
    
    /// <summary>
    /// Statistics collection timestamp
    /// </summary>
    public DateTimeOffset Timestamp { get; set; } = DateTimeOffset.UtcNow;
}

/// <summary>
/// Represents an event snapshot
/// </summary>
public class EventSnapshot
{
    /// <summary>
    /// Stream identifier
    /// </summary>
    public string StreamId { get; set; } = string.Empty;
    
    /// <summary>
    /// Snapshot data
    /// </summary>
    public object Data { get; set; } = new();
    
    /// <summary>
    /// Version at which the snapshot was taken
    /// </summary>
    public long Version { get; set; }
    
    /// <summary>
    /// Snapshot type
    /// </summary>
    public string Type { get; set; } = string.Empty;
    
    /// <summary>
    /// Snapshot creation timestamp
    /// </summary>
    public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;
    
    /// <summary>
    /// Snapshot metadata
    /// </summary>
    public Dictionary<string, object>? Metadata { get; set; }
    
    /// <summary>
    /// Creates a new EventSnapshot instance
    /// </summary>
    /// <param name="streamId">Stream identifier</param>
    /// <param name="data">Snapshot data</param>
    /// <param name="version">Version at which the snapshot was taken</param>
    /// <param name="type">Snapshot type</param>
    /// <param name="metadata">Snapshot metadata</param>
    public EventSnapshot(string streamId, object data, long version, string type, Dictionary<string, object>? metadata = null)
    {
        StreamId = streamId;
        Data = data;
        Version = version;
        Type = type;
        Metadata = metadata;
    }
    
    /// <summary>
    /// Default constructor
    /// </summary>
    public EventSnapshot() { }
}

/// <summary>
/// Represents an event with metadata
/// </summary>
public class StoredEvent
{
    /// <summary>
    /// Event identifier
    /// </summary>
    public string EventId { get; set; } = Guid.NewGuid().ToString();
    
    /// <summary>
    /// Stream identifier
    /// </summary>
    public string StreamId { get; set; } = string.Empty;
    
    /// <summary>
    /// Event version in the stream
    /// </summary>
    public long Version { get; set; }
    
    /// <summary>
    /// Event type
    /// </summary>
    public string EventType { get; set; } = string.Empty;
    
    /// <summary>
    /// Event data
    /// </summary>
    public object Data { get; set; } = new();
    
    /// <summary>
    /// Event metadata
    /// </summary>
    public Dictionary<string, object>? Metadata { get; set; }
    
    /// <summary>
    /// Event creation timestamp
    /// </summary>
    public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;
    
    /// <summary>
    /// Causation identifier (event that caused this event)
    /// </summary>
    public string? CausationId { get; set; }
    
    /// <summary>
    /// Correlation identifier (transaction or process identifier)
    /// </summary>
    public string? CorrelationId { get; set; }
    
    /// <summary>
    /// Creates a new StoredEvent instance
    /// </summary>
    /// <param name="streamId">Stream identifier</param>
    /// <param name="eventType">Event type</param>
    /// <param name="data">Event data</param>
    /// <param name="version">Event version</param>
    /// <param name="metadata">Event metadata</param>
    /// <param name="causationId">Causation identifier</param>
    /// <param name="correlationId">Correlation identifier</param>
    public StoredEvent(
        string streamId, 
        string eventType, 
        object data, 
        long version, 
        Dictionary<string, object>? metadata = null, 
        string? causationId = null, 
        string? correlationId = null)
    {
        StreamId = streamId;
        EventType = eventType;
        Data = data;
        Version = version;
        Metadata = metadata;
        CausationId = causationId;
        CorrelationId = correlationId;
    }
    
    /// <summary>
    /// Default constructor
    /// </summary>
    public StoredEvent() { }
}

/// <summary>
/// Represents event stream information
/// </summary>
public class EventStreamInfo
{
    /// <summary>
    /// Stream identifier
    /// </summary>
    public string StreamId { get; set; } = string.Empty;
    
    /// <summary>
    /// Current version of the stream
    /// </summary>
    public long Version { get; set; }
    
    /// <summary>
    /// Stream status
    /// </summary>
    public EventStreamStatus Status { get; set; }
    
    /// <summary>
    /// Number of events in the stream
    /// </summary>
    public long EventCount { get; set; }
    
    /// <summary>
    /// Stream creation timestamp
    /// </summary>
    public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;
    
    /// <summary>
    /// Last event timestamp
    /// </summary>
    public DateTimeOffset? LastEventAt { get; set; }
    
    /// <summary>
    /// Stream metadata
    /// </summary>
    public Dictionary<string, object>? Metadata { get; set; }
    
    /// <summary>
    /// Stream size in bytes
    /// </summary>
    public long SizeInBytes { get; set; }
    
    /// <summary>
    /// Whether the stream has snapshots
    /// </summary>
    public bool HasSnapshots { get; set; }
    
    /// <summary>
    /// Last snapshot version
    /// </summary>
    public long? LastSnapshotVersion { get; set; }
} 