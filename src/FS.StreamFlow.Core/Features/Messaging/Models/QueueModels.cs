namespace FS.StreamFlow.Core.Features.Messaging.Models;

/// <summary>
/// Represents queue configuration settings
/// </summary>
public class QueueSettings
{
    /// <summary>
    /// Queue name
    /// </summary>
    public string Name { get; set; } = string.Empty;
    
    /// <summary>
    /// Whether the queue is durable
    /// </summary>
    public bool Durable { get; set; } = true;
    
    /// <summary>
    /// Whether the queue is exclusive
    /// </summary>
    public bool Exclusive { get; set; } = false;
    
    /// <summary>
    /// Whether the queue auto-deletes when no longer in use
    /// </summary>
    public bool AutoDelete { get; set; } = false;
    
    /// <summary>
    /// Additional arguments for queue declaration
    /// </summary>
    public Dictionary<string, object>? Arguments { get; set; }
    
    /// <summary>
    /// Queue bindings
    /// </summary>
    public List<QueueBinding> Bindings { get; set; } = new();
    
    /// <summary>
    /// Creates a new QueueSettings instance
    /// </summary>
    /// <param name="name">Queue name</param>
    /// <param name="durable">Whether the queue is durable</param>
    /// <param name="exclusive">Whether the queue is exclusive</param>
    /// <param name="autoDelete">Whether the queue auto-deletes</param>
    /// <param name="arguments">Additional arguments</param>
    public QueueSettings(
        string name, 
        bool durable = true, 
        bool exclusive = false, 
        bool autoDelete = false, 
        Dictionary<string, object>? arguments = null)
    {
        Name = name;
        Durable = durable;
        Exclusive = exclusive;
        AutoDelete = autoDelete;
        Arguments = arguments;
    }
    
    /// <summary>
    /// Default constructor
    /// </summary>
    public QueueSettings() { }
}

/// <summary>
/// Represents a queue binding
/// </summary>
public class QueueBinding
{
    /// <summary>
    /// Queue name
    /// </summary>
    public string QueueName { get; set; } = string.Empty;
    
    /// <summary>
    /// Exchange name
    /// </summary>
    public string ExchangeName { get; set; } = string.Empty;
    
    /// <summary>
    /// Routing key for the binding
    /// </summary>
    public string RoutingKey { get; set; } = string.Empty;
    
    /// <summary>
    /// Additional arguments for the binding
    /// </summary>
    public Dictionary<string, object>? Arguments { get; set; }
    
    /// <summary>
    /// Creates a new QueueBinding instance
    /// </summary>
    /// <param name="queueName">Queue name</param>
    /// <param name="exchangeName">Exchange name</param>
    /// <param name="routingKey">Routing key</param>
    /// <param name="arguments">Additional arguments</param>
    public QueueBinding(string queueName, string exchangeName, string routingKey, Dictionary<string, object>? arguments = null)
    {
        QueueName = queueName;
        ExchangeName = exchangeName;
        RoutingKey = routingKey;
        Arguments = arguments;
    }
    
    /// <summary>
    /// Default constructor
    /// </summary>
    public QueueBinding() { }
}

/// <summary>
/// Represents the result of a queue declaration
/// </summary>
public class QueueDeclareResult
{
    /// <summary>
    /// Queue name
    /// </summary>
    public string QueueName { get; set; } = string.Empty;
    
    /// <summary>
    /// Number of messages in the queue
    /// </summary>
    public uint MessageCount { get; set; }
    
    /// <summary>
    /// Number of consumers attached to the queue
    /// </summary>
    public uint ConsumerCount { get; set; }
    
    /// <summary>
    /// Whether the queue was created (true) or already existed (false)
    /// </summary>
    public bool Created { get; set; }
    
    /// <summary>
    /// Timestamp when the queue was declared
    /// </summary>
    public DateTimeOffset Timestamp { get; set; } = DateTimeOffset.UtcNow;
    
    /// <summary>
    /// Creates a new QueueDeclareResult instance
    /// </summary>
    /// <param name="queueName">Queue name</param>
    /// <param name="messageCount">Number of messages in the queue</param>
    /// <param name="consumerCount">Number of consumers attached to the queue</param>
    /// <param name="created">Whether the queue was created</param>
    public QueueDeclareResult(string queueName, uint messageCount, uint consumerCount, bool created = true)
    {
        QueueName = queueName;
        MessageCount = messageCount;
        ConsumerCount = consumerCount;
        Created = created;
    }
    
    /// <summary>
    /// Default constructor
    /// </summary>
    public QueueDeclareResult() { }
}

/// <summary>
/// Represents information about a queue
/// </summary>
public class QueueInfo
{
    /// <summary>
    /// Queue name
    /// </summary>
    public string Name { get; set; } = string.Empty;
    
    /// <summary>
    /// Whether the queue is durable
    /// </summary>
    public bool Durable { get; set; }
    
    /// <summary>
    /// Whether the queue is exclusive
    /// </summary>
    public bool Exclusive { get; set; }
    
    /// <summary>
    /// Whether the queue auto-deletes
    /// </summary>
    public bool AutoDelete { get; set; }
    
    /// <summary>
    /// Queue arguments
    /// </summary>
    public Dictionary<string, object>? Arguments { get; set; }
    
    /// <summary>
    /// Queue bindings
    /// </summary>
    public List<QueueBinding> Bindings { get; set; } = new();
    
    /// <summary>
    /// Number of messages in the queue
    /// </summary>
    public uint MessageCount { get; set; }
    
    /// <summary>
    /// Number of consumers attached to the queue
    /// </summary>
    public uint ConsumerCount { get; set; }
    
    /// <summary>
    /// When the queue was created
    /// </summary>
    public DateTimeOffset CreatedAt { get; set; }
    
    /// <summary>
    /// Last time the queue was used
    /// </summary>
    public DateTimeOffset LastUsedAt { get; set; }
    
    /// <summary>
    /// Queue state
    /// </summary>
    public QueueState State { get; set; }
}

/// <summary>
/// Represents the state of a queue
/// </summary>
public enum QueueState
{
    /// <summary>
    /// Queue is active and ready
    /// </summary>
    Active,
    
    /// <summary>
    /// Queue is idle
    /// </summary>
    Idle,
    
    /// <summary>
    /// Queue is being deleted
    /// </summary>
    Deleting,
    
    /// <summary>
    /// Queue is in an error state
    /// </summary>
    Error
}

/// <summary>
/// Represents queue statistics
/// </summary>
public class QueueStatistics
{
    /// <summary>
    /// Total number of queues
    /// </summary>
    public int TotalQueues { get; set; }
    
    /// <summary>
    /// Number of durable queues
    /// </summary>
    public int DurableQueues { get; set; }
    
    /// <summary>
    /// Number of exclusive queues
    /// </summary>
    public int ExclusiveQueues { get; set; }
    
    /// <summary>
    /// Number of auto-delete queues
    /// </summary>
    public int AutoDeleteQueues { get; set; }
    
    /// <summary>
    /// Total messages across all queues
    /// </summary>
    public long TotalMessages { get; set; }
    
    /// <summary>
    /// Total messages consumed across all queues
    /// </summary>
    public long TotalMessagesConsumed { get; set; }
    
    /// <summary>
    /// Total number of consumers across all queues
    /// </summary>
    public long TotalConsumers { get; set; }
    
    /// <summary>
    /// Average queue depth
    /// </summary>
    public double AverageQueueDepth { get; set; }
    
    /// <summary>
    /// Queue statistics by state
    /// </summary>
    public Dictionary<QueueState, int> QueuesByState { get; set; } = new();
    
    /// <summary>
    /// Statistics collection timestamp
    /// </summary>
    public DateTimeOffset Timestamp { get; set; } = DateTimeOffset.UtcNow;
}

/// <summary>
/// Event arguments for queue events
/// </summary>
public class QueueEventArgs : EventArgs
{
    /// <summary>
    /// Queue name
    /// </summary>
    public string QueueName { get; }
    
    /// <summary>
    /// Queue state when the event occurred
    /// </summary>
    public QueueState QueueState { get; }
    
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
    /// Initializes a new instance of the QueueEventArgs class
    /// </summary>
    /// <param name="queueName">Queue name</param>
    /// <param name="queueState">Queue state</param>
    /// <param name="errorMessage">Error message (if applicable)</param>
    /// <param name="exception">Exception details (if applicable)</param>
    /// <param name="context">Additional context information</param>
    public QueueEventArgs(
        string queueName, 
        QueueState queueState, 
        string? errorMessage = null, 
        Exception? exception = null, 
        Dictionary<string, object>? context = null)
    {
        QueueName = queueName;
        QueueState = queueState;
        Timestamp = DateTimeOffset.UtcNow;
        ErrorMessage = errorMessage;
        Exception = exception;
        Context = context;
    }
} 