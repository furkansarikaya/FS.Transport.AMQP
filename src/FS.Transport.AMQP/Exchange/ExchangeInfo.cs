namespace FS.Transport.AMQP.Exchange;

/// <summary>
/// Information about an existing exchange
/// </summary>
public class ExchangeInfo
{
    /// <summary>
    /// Exchange name
    /// </summary>
    public string Name { get; set; } = string.Empty;
    
    /// <summary>
    /// Exchange type
    /// </summary>
    public string Type { get; set; } = string.Empty;
    
    /// <summary>
    /// Whether the exchange is durable
    /// </summary>
    public bool Durable { get; set; }
    
    /// <summary>
    /// Whether the exchange auto-deletes
    /// </summary>
    public bool AutoDelete { get; set; }
    
    /// <summary>
    /// Additional arguments for the exchange
    /// </summary>
    public IDictionary<string, object> Arguments { get; set; } = new Dictionary<string, object>();
    
    /// <summary>
    /// Number of bindings (if available)
    /// </summary>
    public int? BindingCount { get; set; }
    
    /// <summary>
    /// Whether the exchange is internal (cannot be published to directly)
    /// </summary>
    public bool Internal { get; set; }

    /// <summary>
    /// Gets a string representation of the exchange info
    /// </summary>
    /// <returns>Exchange description</returns>
    public override string ToString()
    {
        return $"Exchange '{Name}' (Type: {Type}, Durable: {Durable}, AutoDelete: {AutoDelete}, Internal: {Internal})";
    }
}