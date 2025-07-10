namespace FS.Transport.AMQP.Configuration;

/// <summary>
/// Consumer settings for queue consumption
/// </summary>
public class ConsumerSettings
{
    /// <summary>
    /// Number of concurrent consumers
    /// </summary>
    public int ConcurrentConsumers { get; set; } = Environment.ProcessorCount;
    
    /// <summary>
    /// Consumer tag for identification
    /// </summary>
    public string? ConsumerTag { get; set; }
    
    /// <summary>
    /// Whether to auto-acknowledge messages
    /// </summary>
    public bool AutoAck { get; set; } = false;
    
    /// <summary>
    /// Prefetch count (QoS)
    /// </summary>
    public ushort PrefetchCount { get; set; } = 10;
    
    /// <summary>
    /// Whether to apply prefetch globally
    /// </summary>
    public bool GlobalPrefetch { get; set; } = false;
    
    /// <summary>
    /// Consumer priority
    /// </summary>
    public int Priority { get; set; } = 0;
    
    /// <summary>
    /// Consumer arguments
    /// </summary>
    public Dictionary<string, object> Arguments { get; set; } = new();

    /// <summary>
    /// Validates consumer settings
    /// </summary>
    public void Validate()
    {
        if (ConcurrentConsumers <= 0)
            throw new ArgumentException("ConcurrentConsumers must be greater than 0");
            
        if (PrefetchCount == 0)
            throw new ArgumentException("PrefetchCount must be greater than 0");
    }
}