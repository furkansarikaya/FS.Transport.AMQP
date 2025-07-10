namespace FS.Transport.AMQP.Queue;

/// <summary>
/// Information about a queue binding
/// </summary>
public class QueueBindingInfo
{
    /// <summary>
    /// Queue name
    /// </summary>
    public string QueueName { get; set; } = string.Empty;
    
    /// <summary>
    /// Exchange name
    /// </summary>
    public string Exchange { get; set; } = string.Empty;
    
    /// <summary>
    /// Exchange name (alias for Exchange property)
    /// </summary>
    public string ExchangeName 
    { 
        get => Exchange; 
        set => Exchange = value; 
    }
    
    /// <summary>
    /// Routing key for the binding
    /// </summary>
    public string RoutingKey { get; set; } = string.Empty;
    
    /// <summary>
    /// Additional arguments for the binding
    /// </summary>
    public IDictionary<string, object> Arguments { get; set; } = new Dictionary<string, object>();

    /// <summary>
    /// Gets a string representation of the binding
    /// </summary>
    /// <returns>Binding description</returns>
    public override string ToString()
    {
        return $"Queue '{QueueName}' binding to '{Exchange}' with routing key '{RoutingKey}'";
    }
}
