namespace FS.RabbitMQ.Configuration;

/// <summary>
/// Queue to exchange binding configuration
/// </summary>
public class QueueBinding
{
    /// <summary>
    /// Exchange name to bind to
    /// </summary>
    public string Exchange { get; set; } = string.Empty;
    
    /// <summary>
    /// Routing key for the binding
    /// </summary>
    public string RoutingKey { get; set; } = string.Empty;
    
    /// <summary>
    /// Additional arguments for binding
    /// </summary>
    public Dictionary<string, object> Arguments { get; set; } = new();

    /// <summary>
    /// Validates queue binding
    /// </summary>
    public void Validate()
    {
        if (string.IsNullOrWhiteSpace(Exchange))
            throw new ArgumentException("Exchange cannot be null or empty");
    }
}