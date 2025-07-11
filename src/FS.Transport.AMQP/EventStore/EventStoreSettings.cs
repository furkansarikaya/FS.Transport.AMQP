namespace FS.Transport.AMQP.EventStore;

/// <summary>
/// Configuration settings for event store functionality
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
    /// Snapshot frequency (how many events before creating a snapshot)
    /// </summary>
    public int SnapshotFrequency { get; set; } = 100;
    
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
    /// Connection string for event store database
    /// </summary>
    public string? ConnectionString { get; set; }
    
    /// <summary>
    /// Event retention period in days (0 = no retention)
    /// </summary>
    public int RetentionDays { get; set; } = 0;
    
    /// <summary>
    /// Maximum batch size for event store operations
    /// </summary>
    public int MaxBatchSize { get; set; } = 1000;
    
    /// <summary>
    /// Timeout for event store operations in milliseconds
    /// </summary>
    public int OperationTimeoutMs { get; set; } = 30000;
    
    /// <summary>
    /// Whether to enable event store compression
    /// </summary>
    public bool EnableCompression { get; set; } = false;
    
    /// <summary>
    /// Whether to enable event store encryption
    /// </summary>
    public bool EnableEncryption { get; set; } = false;
    
    /// <summary>
    /// Encryption key for event store
    /// </summary>
    public string? EncryptionKey { get; set; }
    
    /// <summary>
    /// Validates the event store settings
    /// </summary>
    /// <returns>True if settings are valid</returns>
    public bool IsValid()
    {
        if (string.IsNullOrWhiteSpace(StreamPrefix))
            return false;
            
        if (string.IsNullOrWhiteSpace(Exchange))
            return false;
            
        if (string.IsNullOrWhiteSpace(Queue))
            return false;
            
        if (SnapshotFrequency <= 0)
            return false;
            
        if (MaxBatchSize <= 0)
            return false;
            
        if (OperationTimeoutMs <= 0)
            return false;
            
        if (EnableEncryption && string.IsNullOrWhiteSpace(EncryptionKey))
            return false;
            
        return true;
    }
    
    /// <summary>
    /// Validates the event store settings and throws an exception if invalid
    /// </summary>
    public void Validate()
    {
        if (!IsValid())
            throw new InvalidOperationException("EventStoreSettings validation failed");
    }
}