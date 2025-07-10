namespace FS.Transport.AMQP.Queue;

/// <summary>
/// Information about a queue binding
/// </summary>
public class QueueBindingInfo
{
    /// <summary>
    /// Exchange name
    /// </summary>
    public string Exchange { get; set; } = string.Empty;
    
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
        return $"Binding to '{Exchange}' with routing key '{RoutingKey}'";
    }
}
