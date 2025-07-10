namespace FS.Transport.AMQP.Configuration;

/// <summary>
/// Settings for event store functionality
/// </summary>
public class EventStoreSettings
{
    /// <summary>
    /// Whether the event store is enabled
    /// </summary>
    public bool Enabled { get; set; } = true;
    
    /// <summary>
    /// Stream prefix for event store streams
    /// </summary>
    public string StreamPrefix { get; set; } = "events";
    
    /// <summary>
    /// Whether to enable snapshots
    /// </summary>
    public bool EnableSnapshots { get; set; } = true;
    
    /// <summary>
    /// Exchange name for event store
    /// </summary>
    public string Exchange { get; set; } = "event-store";
    
    /// <summary>
    /// Queue name for event store persistence
    /// </summary>
    public string Queue { get; set; } = "event-store";
    
    /// <summary>
    /// Routing key for event store messages
    /// </summary>
    public string RoutingKey { get; set; } = "events";
    
    /// <summary>
    /// Event serialization format (Json, Binary)
    /// </summary>
    public string SerializationFormat { get; set; } = "Json";
    
    /// <summary>
    /// Whether to compress event payloads
    /// </summary>
    public bool CompressEvents { get; set; } = false;
    
    /// <summary>
    /// Snapshot frequency (every N events)
    /// </summary>
    public int SnapshotFrequency { get; set; } = 100;
    
    /// <summary>
    /// Whether to enable event versioning
    /// </summary>
    public bool EnableVersioning { get; set; } = true;
    
    /// <summary>
    /// Event retention period in days (0 for no retention)
    /// </summary>
    public int RetentionDays { get; set; } = 0;
    
    /// <summary>
    /// Batch size for event retrieval
    /// </summary>
    public int BatchSize { get; set; } = 100;

    /// <summary>
    /// Validates event store settings
    /// </summary>
    public void Validate()
    {
        if (string.IsNullOrWhiteSpace(StreamPrefix))
            throw new ArgumentException("StreamPrefix cannot be null or empty");
            
        if (string.IsNullOrWhiteSpace(Exchange))
            throw new ArgumentException("Exchange cannot be null or empty");
            
        if (string.IsNullOrWhiteSpace(Queue))
            throw new ArgumentException("Queue cannot be null or empty");
            
        if (string.IsNullOrWhiteSpace(RoutingKey))
            throw new ArgumentException("RoutingKey cannot be null or empty");
            
        var validFormats = new[] { "Json", "Binary" };
        if (!validFormats.Contains(SerializationFormat))
            throw new ArgumentException($"SerializationFormat must be one of: {string.Join(", ", validFormats)}");
            
        if (SnapshotFrequency <= 0)
            throw new ArgumentException("SnapshotFrequency must be greater than 0");
            
        if (RetentionDays < 0)
            throw new ArgumentException("RetentionDays cannot be negative");
            
        if (BatchSize <= 0)
            throw new ArgumentException("BatchSize must be greater than 0");
    }
}