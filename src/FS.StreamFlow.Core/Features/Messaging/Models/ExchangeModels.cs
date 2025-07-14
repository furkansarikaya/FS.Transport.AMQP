namespace FS.StreamFlow.Core.Features.Messaging.Models;

/// <summary>
/// Represents exchange types in a messaging system
/// </summary>
public enum ExchangeType
{
    /// <summary>
    /// Direct exchange routes messages to queues based on exact routing key match
    /// </summary>
    Direct,
    
    /// <summary>
    /// Topic exchange routes messages to queues based on routing key patterns
    /// </summary>
    Topic,
    
    /// <summary>
    /// Headers exchange routes messages based on header values
    /// </summary>
    Headers,
    
    /// <summary>
    /// Fanout exchange routes messages to all bound queues
    /// </summary>
    Fanout
}

/// <summary>
/// Represents exchange configuration settings
/// </summary>
public class ExchangeSettings
{
    /// <summary>
    /// Exchange name
    /// </summary>
    public string Name { get; set; } = string.Empty;
    
    /// <summary>
    /// Exchange type
    /// </summary>
    public ExchangeType Type { get; set; } = ExchangeType.Direct;
    
    /// <summary>
    /// Whether the exchange is durable
    /// </summary>
    public bool Durable { get; set; } = true;
    
    /// <summary>
    /// Whether the exchange auto-deletes when no longer in use
    /// </summary>
    public bool AutoDelete { get; set; } = false;
    
    /// <summary>
    /// Whether the exchange is internal
    /// </summary>
    public bool Internal { get; set; } = false;
    
    /// <summary>
    /// Additional arguments for exchange declaration
    /// </summary>
    public Dictionary<string, object>? Arguments { get; set; }
    
    /// <summary>
    /// Exchange bindings
    /// </summary>
    public List<ExchangeBinding> Bindings { get; set; } = new();
    
    /// <summary>
    /// Creates a new ExchangeSettings instance
    /// </summary>
    /// <param name="name">Exchange name</param>
    /// <param name="type">Exchange type</param>
    /// <param name="durable">Whether the exchange is durable</param>
    /// <param name="autoDelete">Whether the exchange auto-deletes</param>
    /// <param name="internal">Whether the exchange is internal</param>
    /// <param name="arguments">Additional arguments</param>
    public ExchangeSettings(
        string name, 
        ExchangeType type = ExchangeType.Direct, 
        bool durable = true, 
        bool autoDelete = false, 
        bool @internal = false, 
        Dictionary<string, object>? arguments = null)
    {
        Name = name;
        Type = type;
        Durable = durable;
        AutoDelete = autoDelete;
        Internal = @internal;
        Arguments = arguments;
    }
    
    /// <summary>
    /// Default constructor
    /// </summary>
    public ExchangeSettings() { }
}

/// <summary>
/// Represents an exchange binding
/// </summary>
public class ExchangeBinding
{
    /// <summary>
    /// Source exchange name
    /// </summary>
    public string Source { get; set; } = string.Empty;
    
    /// <summary>
    /// Destination exchange name
    /// </summary>
    public string Destination { get; set; } = string.Empty;
    
    /// <summary>
    /// Routing key for the binding
    /// </summary>
    public string RoutingKey { get; set; } = string.Empty;
    
    /// <summary>
    /// Additional arguments for the binding
    /// </summary>
    public Dictionary<string, object>? Arguments { get; set; }
    
    /// <summary>
    /// Creates a new ExchangeBinding instance
    /// </summary>
    /// <param name="source">Source exchange name</param>
    /// <param name="destination">Destination exchange name</param>
    /// <param name="routingKey">Routing key</param>
    /// <param name="arguments">Additional arguments</param>
    public ExchangeBinding(string source, string destination, string routingKey, Dictionary<string, object>? arguments = null)
    {
        Source = source;
        Destination = destination;
        RoutingKey = routingKey;
        Arguments = arguments;
    }
    
    /// <summary>
    /// Default constructor
    /// </summary>
    public ExchangeBinding() { }
}

/// <summary>
/// Represents information about an exchange
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
    public ExchangeType Type { get; set; }
    
    /// <summary>
    /// Whether the exchange is durable
    /// </summary>
    public bool Durable { get; set; }
    
    /// <summary>
    /// Whether the exchange auto-deletes
    /// </summary>
    public bool AutoDelete { get; set; }
    
    /// <summary>
    /// Whether the exchange is internal
    /// </summary>
    public bool Internal { get; set; }
    
    /// <summary>
    /// Exchange arguments
    /// </summary>
    public Dictionary<string, object>? Arguments { get; set; }
    
    /// <summary>
    /// Exchange bindings
    /// </summary>
    public List<ExchangeBinding> Bindings { get; set; } = new();
    
    /// <summary>
    /// Number of messages published to this exchange
    /// </summary>
    public long MessageCount { get; set; }
    
    /// <summary>
    /// When the exchange was created
    /// </summary>
    public DateTimeOffset CreatedAt { get; set; }
    
    /// <summary>
    /// Last time the exchange was used
    /// </summary>
    public DateTimeOffset LastUsedAt { get; set; }
}

/// <summary>
/// Represents exchange statistics
/// </summary>
public class ExchangeStatistics
{
    /// <summary>
    /// Total number of exchanges
    /// </summary>
    public int TotalExchanges { get; set; }
    
    /// <summary>
    /// Number of durable exchanges
    /// </summary>
    public int DurableExchanges { get; set; }
    
    /// <summary>
    /// Number of auto-delete exchanges
    /// </summary>
    public int AutoDeleteExchanges { get; set; }
    
    /// <summary>
    /// Total messages published to all exchanges
    /// </summary>
    public long TotalMessagesPublished { get; set; }
    
    /// <summary>
    /// Total messages routed by all exchanges
    /// </summary>
    public long TotalMessagesRouted { get; set; }
    
    /// <summary>
    /// Number of failed message routings
    /// </summary>
    public long FailedRoutings { get; set; }
    
    /// <summary>
    /// Exchange statistics by type
    /// </summary>
    public Dictionary<ExchangeType, int> ExchangesByType { get; set; } = new();
    
    /// <summary>
    /// Statistics collection timestamp
    /// </summary>
    public DateTimeOffset Timestamp { get; set; } = DateTimeOffset.UtcNow;
}

/// <summary>
/// Event arguments for exchange events
/// </summary>
public class ExchangeEventArgs : EventArgs
{
    /// <summary>
    /// Exchange name
    /// </summary>
    public string ExchangeName { get; }
    
    /// <summary>
    /// Exchange type
    /// </summary>
    public ExchangeType ExchangeType { get; }
    
    /// <summary>
    /// Timestamp when the event occurred
    /// </summary>
    public DateTimeOffset Timestamp { get; }
    
    /// <summary>
    /// Error message (if applicable)
    /// </summary>
    public string? ErrorMessage { get; }
    
    /// <summary>
    /// Exception details (if applicable)
    /// </summary>
    public Exception? Exception { get; }
    
    /// <summary>
    /// Additional context information
    /// </summary>
    public Dictionary<string, object>? Context { get; }
    
    /// <summary>
    /// Initializes a new instance of the ExchangeEventArgs class
    /// </summary>
    /// <param name="exchangeName">Exchange name</param>
    /// <param name="exchangeType">Exchange type</param>
    /// <param name="errorMessage">Error message (if applicable)</param>
    /// <param name="exception">Exception details (if applicable)</param>
    /// <param name="context">Additional context information</param>
    public ExchangeEventArgs(
        string exchangeName, 
        ExchangeType exchangeType, 
        string? errorMessage = null, 
        Exception? exception = null, 
        Dictionary<string, object>? context = null)
    {
        ExchangeName = exchangeName;
        ExchangeType = exchangeType;
        Timestamp = DateTimeOffset.UtcNow;
        ErrorMessage = errorMessage;
        Exception = exception;
        Context = context;
    }
} 