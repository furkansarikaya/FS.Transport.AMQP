namespace FS.Transport.AMQP.Exchange;

/// <summary>
/// Represents a declared exchange with its configuration for auto-recovery purposes
/// </summary>
public class ExchangeDeclaration
{
    /// <summary>
    /// Exchange name
    /// </summary>
    public string Name { get; }
    
    /// <summary>
    /// Exchange type (direct, topic, fanout, headers)
    /// </summary>
    public string Type { get; }
    
    /// <summary>
    /// Whether the exchange is durable
    /// </summary>
    public bool Durable { get; }
    
    /// <summary>
    /// Whether the exchange auto-deletes
    /// </summary>
    public bool AutoDelete { get; }
    
    /// <summary>
    /// Additional arguments for the exchange
    /// </summary>
    public IDictionary<string, object> Arguments { get; }
    
    /// <summary>
    /// Timestamp when the exchange was declared
    /// </summary>
    public DateTime DeclaredAt { get; }

    public ExchangeDeclaration(
        string name, 
        string type, 
        bool durable, 
        bool autoDelete, 
        IDictionary<string, object>? arguments = null)
    {
        Name = name ?? throw new ArgumentNullException(nameof(name));
        Type = type ?? throw new ArgumentNullException(nameof(type));
        Durable = durable;
        AutoDelete = autoDelete;
        Arguments = arguments?.ToDictionary(kvp => kvp.Key, kvp => kvp.Value) ?? new Dictionary<string, object>();
        DeclaredAt = DateTime.UtcNow;
    }

    /// <summary>
    /// Creates a copy of the exchange declaration
    /// </summary>
    /// <returns>Cloned exchange declaration</returns>
    public ExchangeDeclaration Clone()
    {
        return new ExchangeDeclaration(Name, Type, Durable, AutoDelete, Arguments);
    }

    /// <summary>
    /// Determines equality based on exchange name and configuration
    /// </summary>
    /// <param name="obj">Object to compare</param>
    /// <returns>True if declarations are equivalent</returns>
    public override bool Equals(object? obj)
    {
        if (obj is not ExchangeDeclaration other)
            return false;

        return Name == other.Name &&
               Type == other.Type &&
               Durable == other.Durable &&
               AutoDelete == other.AutoDelete;
    }

    /// <summary>
    /// Gets hash code based on exchange configuration
    /// </summary>
    /// <returns>Hash code</returns>
    public override int GetHashCode()
    {
        return HashCode.Combine(Name, Type, Durable, AutoDelete);
    }

    /// <summary>
    /// Gets a string representation of the exchange declaration
    /// </summary>
    /// <returns>Exchange description</returns>
    public override string ToString()
    {
        return $"Exchange '{Name}' (Type: {Type}, Durable: {Durable}, AutoDelete: {AutoDelete})";
    }
}