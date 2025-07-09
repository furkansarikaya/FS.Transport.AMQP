namespace FS.Transport.AMQP.Configuration;

/// <summary>
/// Settings for queue declaration and configuration
/// </summary>
public class QueueSettings
{
    /// <summary>
    /// Queue name
    /// </summary>
    public string Name { get; set; } = string.Empty;
    
    /// <summary>
    /// Whether the queue is durable (survives broker restart)
    /// </summary>
    public bool Durable { get; set; } = true;
    
    /// <summary>
    /// Whether the queue is exclusive to this connection
    /// </summary>
    public bool Exclusive { get; set; } = false;
    
    /// <summary>
    /// Whether the queue is auto-deleted when no longer used
    /// </summary>
    public bool AutoDelete { get; set; } = false;
    
    /// <summary>
    /// Additional arguments for queue declaration
    /// </summary>
    public Dictionary<string, object> Arguments { get; set; } = new();
    
    /// <summary>
    /// Bindings to exchanges
    /// </summary>
    public List<QueueBinding> Bindings { get; set; } = new();
    
    /// <summary>
    /// Consumer settings for this queue
    /// </summary>
    public ConsumerSettings Consumer { get; set; } = new();

    /// <summary>
    /// Validates queue settings
    /// </summary>
    public void Validate()
    {
        if (string.IsNullOrWhiteSpace(Name))
            throw new ArgumentException("Queue Name cannot be null or empty");
            
        foreach (var binding in Bindings)
        {
            binding.Validate();
        }
        
        Consumer.Validate();
    }
}