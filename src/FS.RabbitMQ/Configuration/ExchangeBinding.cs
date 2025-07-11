namespace FS.RabbitMQ.Configuration;

/// <summary>
/// Exchange to exchange binding configuration
/// </summary>
public class ExchangeBinding
{
    /// <summary>
    /// Destination exchange name
    /// </summary>
    public string DestinationExchange { get; set; } = string.Empty;
    
    /// <summary>
    /// Routing key for the binding
    /// </summary>
    public string RoutingKey { get; set; } = string.Empty;
    
    /// <summary>
    /// Additional arguments for binding
    /// </summary>
    public Dictionary<string, object> Arguments { get; set; } = new();

    /// <summary>
    /// Validates exchange binding
    /// </summary>
    public void Validate()
    {
        if (string.IsNullOrWhiteSpace(DestinationExchange))
            throw new ArgumentException("DestinationExchange cannot be null or empty");
    }
}