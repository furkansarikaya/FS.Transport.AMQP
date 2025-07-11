namespace FS.RabbitMQ.Queue;

/// <summary>
/// Represents a declared queue with its configuration and bindings for auto-recovery purposes
/// </summary>
public class QueueDeclaration
{
    /// <summary>
    /// Queue name
    /// </summary>
    public string Name { get; }
    
    /// <summary>
    /// Whether the queue is durable
    /// </summary>
    public bool Durable { get; }
    
    /// <summary>
    /// Whether the queue is exclusive
    /// </summary>
    public bool Exclusive { get; }
    
    /// <summary>
    /// Whether the queue auto-deletes
    /// </summary>
    public bool AutoDelete { get; }
    
    /// <summary>
    /// Additional arguments for the queue
    /// </summary>
    public IDictionary<string, object> Arguments { get; }
    
    /// <summary>
    /// Queue bindings to exchanges
    /// </summary>
    public List<QueueBindingInfo> Bindings { get; }
    
    /// <summary>
    /// Timestamp when the queue was declared
    /// </summary>
    public DateTime DeclaredAt { get; }

    public QueueDeclaration(
        string name, 
        bool durable, 
        bool exclusive, 
        bool autoDelete, 
        IDictionary<string, object>? arguments = null)
    {
        Name = name ?? throw new ArgumentNullException(nameof(name));
        Durable = durable;
        Exclusive = exclusive;
        AutoDelete = autoDelete;
        Arguments = arguments?.ToDictionary(kvp => kvp.Key, kvp => kvp.Value) ?? new Dictionary<string, object>();
        Bindings = new List<QueueBindingInfo>();
        DeclaredAt = DateTime.UtcNow;
    }

    /// <summary>
    /// Adds a binding to this queue declaration
    /// </summary>
    /// <param name="exchange">Exchange name</param>
    /// <param name="routingKey">Routing key</param>
    /// <param name="arguments">Binding arguments</param>
    public void AddBinding(string exchange, string routingKey, IDictionary<string, object>? arguments = null)
    {
        var binding = new QueueBindingInfo
        {
            Exchange = exchange,
            RoutingKey = routingKey,
            Arguments = arguments?.ToDictionary(kvp => kvp.Key, kvp => kvp.Value) ?? new Dictionary<string, object>()
        };

        // Avoid duplicate bindings
        if (!Bindings.Any(b => b.Exchange == exchange && b.RoutingKey == routingKey))
        {
            Bindings.Add(binding);
        }
    }

    /// <summary>
    /// Creates a copy of the queue declaration
    /// </summary>
    /// <returns>Cloned queue declaration</returns>
    public QueueDeclaration Clone()
    {
        var clone = new QueueDeclaration(Name, Durable, Exclusive, AutoDelete, Arguments);
        foreach (var binding in Bindings)
        {
            clone.AddBinding(binding.Exchange, binding.RoutingKey, binding.Arguments);
        }
        return clone;
    }

    /// <summary>
    /// Determines equality based on queue name and configuration
    /// </summary>
    /// <param name="obj">Object to compare</param>
    /// <returns>True if declarations are equivalent</returns>
    public override bool Equals(object? obj)
    {
        if (obj is not QueueDeclaration other)
            return false;

        return Name == other.Name &&
               Durable == other.Durable &&
               Exclusive == other.Exclusive &&
               AutoDelete == other.AutoDelete;
    }

    /// <summary>
    /// Gets hash code based on queue configuration
    /// </summary>
    /// <returns>Hash code</returns>
    public override int GetHashCode()
    {
        return HashCode.Combine(Name, Durable, Exclusive, AutoDelete);
    }

    /// <summary>
    /// Gets a string representation of the queue declaration
    /// </summary>
    /// <returns>Queue description</returns>
    public override string ToString()
    {
        return $"Queue '{Name}' (Durable: {Durable}, Exclusive: {Exclusive}, AutoDelete: {AutoDelete}, Bindings: {Bindings.Count})";
    }
}