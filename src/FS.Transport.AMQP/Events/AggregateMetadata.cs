namespace FS.Transport.AMQP.Events;

/// <summary>
/// Aggregate-specific metadata for domain events
/// </summary>
public class AggregateMetadata
{
    /// <summary>
    /// Aggregate identifier
    /// </summary>
    public string Id { get; set; } = string.Empty;
    
    /// <summary>
    /// Aggregate type name
    /// </summary>
    public string Type { get; set; } = string.Empty;
    
    /// <summary>
    /// Aggregate version when the event was raised
    /// </summary>
    public long Version { get; set; } = -1;

    /// <summary>
    /// Gets a string representation of the aggregate metadata
    /// </summary>
    /// <returns>Aggregate description</returns>
    public override string ToString()
    {
        return $"{Type}:{Id}@{Version}";
    }
}
