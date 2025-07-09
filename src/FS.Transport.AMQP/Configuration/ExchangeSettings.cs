namespace FS.Transport.AMQP.Configuration;

/// <summary>
/// Settings for exchange declaration and configuration
/// </summary>
public class ExchangeSettings
{
    /// <summary>
    /// Exchange name
    /// </summary>
    public string Name { get; set; } = string.Empty;
    
    /// <summary>
    /// Exchange type (direct, topic, fanout, headers)
    /// </summary>
    public string Type { get; set; } = "topic";
    
    /// <summary>
    /// Whether the exchange is durable (survives broker restart)
    /// </summary>
    public bool Durable { get; set; } = true;
    
    /// <summary>
    /// Whether the exchange is auto-deleted when no longer used
    /// </summary>
    public bool AutoDelete { get; set; } = false;
    
    /// <summary>
    /// Additional arguments for exchange declaration
    /// </summary>
    public Dictionary<string, object> Arguments { get; set; } = new();
    
    /// <summary>
    /// Bindings to other exchanges
    /// </summary>
    public List<ExchangeBinding> Bindings { get; set; } = new();

    /// <summary>
    /// Validates exchange settings
    /// </summary>
    public void Validate()
    {
        if (string.IsNullOrWhiteSpace(Name))
            throw new ArgumentException("Exchange Name cannot be null or empty");
            
        if (string.IsNullOrWhiteSpace(Type))
            throw new ArgumentException("Exchange Type cannot be null or empty");
            
        var validTypes = new[] { "direct", "topic", "fanout", "headers" };
        if (!validTypes.Contains(Type.ToLowerInvariant()))
            throw new ArgumentException($"Exchange Type must be one of: {string.Join(", ", validTypes)}");
            
        foreach (var binding in Bindings)
        {
            binding.Validate();
        }
    }
}