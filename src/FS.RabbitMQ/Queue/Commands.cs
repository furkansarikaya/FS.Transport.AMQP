using FS.Mediator.Features.RequestHandling.Core;
using FS.RabbitMQ.Configuration;

namespace FS.RabbitMQ.Queue;

/// <summary>
/// Command to declare a queue
/// </summary>
public class DeclareQueueCommand : IRequest<QueueDeclarationResult>
{
    /// <summary>
    /// Queue settings to use (optional, if null will use manual parameters)
    /// </summary>
    public QueueSettings? Settings { get; set; }
    
    /// <summary>
    /// Queue name
    /// </summary>
    public string Name { get; set; } = string.Empty;
    
    /// <summary>
    /// Whether queue is durable
    /// </summary>
    public bool Durable { get; set; } = true;
    
    /// <summary>
    /// Whether queue is exclusive
    /// </summary>
    public bool Exclusive { get; set; } = false;
    
    /// <summary>
    /// Whether queue auto-deletes
    /// </summary>
    public bool AutoDelete { get; set; } = false;
    
    /// <summary>
    /// Additional queue arguments
    /// </summary>
    public IDictionary<string, object>? Arguments { get; set; }
    
    /// <summary>
    /// Whether to apply bindings if defined in settings
    /// </summary>
    public bool ApplyBindings { get; set; } = true;
    
    /// <summary>
    /// Correlation ID for tracing
    /// </summary>
    public string? CorrelationId { get; set; }
    
    /// <summary>
    /// Creates a declare queue command using settings
    /// </summary>
    /// <param name="settings">Queue settings</param>
    /// <param name="applyBindings">Apply bindings</param>
    /// <param name="correlationId">Correlation ID</param>
    /// <returns>Declare queue command</returns>
    public static DeclareQueueCommand CreateFromSettings(QueueSettings settings, bool applyBindings = true, string? correlationId = null)
    {
        return new DeclareQueueCommand
        {
            Settings = settings,
            ApplyBindings = applyBindings,
            CorrelationId = correlationId
        };
    }
    
    /// <summary>
    /// Creates a declare queue command with manual parameters
    /// </summary>
    /// <param name="name">Queue name</param>
    /// <param name="durable">Durable flag</param>
    /// <param name="exclusive">Exclusive flag</param>
    /// <param name="autoDelete">Auto-delete flag</param>
    /// <param name="arguments">Queue arguments</param>
    /// <param name="correlationId">Correlation ID</param>
    /// <returns>Declare queue command</returns>
    public static DeclareQueueCommand CreateManual(string name, bool durable = true, bool exclusive = false, 
        bool autoDelete = false, IDictionary<string, object>? arguments = null, string? correlationId = null)
    {
        return new DeclareQueueCommand
        {
            Name = name,
            Durable = durable,
            Exclusive = exclusive,
            AutoDelete = autoDelete,
            Arguments = arguments,
            CorrelationId = correlationId
        };
    }
}

/// <summary>
/// Command to delete a queue
/// </summary>
public class DeleteQueueCommand : IRequest<QueueOperationResult>
{
    /// <summary>
    /// Queue name to delete
    /// </summary>
    public string Name { get; set; } = string.Empty;
    
    /// <summary>
    /// Only delete if unused
    /// </summary>
    public bool IfUnused { get; set; } = false;
    
    /// <summary>
    /// Only delete if empty
    /// </summary>
    public bool IfEmpty { get; set; } = false;
    
    /// <summary>
    /// Whether to force deletion
    /// </summary>
    public bool Force { get; set; } = false;
    
    /// <summary>
    /// Reason for deletion
    /// </summary>
    public string? Reason { get; set; }
    
    /// <summary>
    /// Correlation ID for tracing
    /// </summary>
    public string? CorrelationId { get; set; }
    
    /// <summary>
    /// Creates a delete queue command
    /// </summary>
    /// <param name="name">Queue name</param>
    /// <param name="ifUnused">If unused flag</param>
    /// <param name="ifEmpty">If empty flag</param>
    /// <param name="force">Force deletion</param>
    /// <param name="reason">Deletion reason</param>
    /// <param name="correlationId">Correlation ID</param>
    /// <returns>Delete queue command</returns>
    public static DeleteQueueCommand Create(string name, bool ifUnused = false, bool ifEmpty = false, 
        bool force = false, string? reason = null, string? correlationId = null)
    {
        return new DeleteQueueCommand
        {
            Name = name,
            IfUnused = ifUnused,
            IfEmpty = ifEmpty,
            Force = force,
            Reason = reason,
            CorrelationId = correlationId
        };
    }
}

/// <summary>
/// Command to bind a queue to an exchange
/// </summary>
public class BindQueueCommand : IRequest<QueueBindingResult>
{
    /// <summary>
    /// Queue name to bind
    /// </summary>
    public string QueueName { get; set; } = string.Empty;
    
    /// <summary>
    /// Exchange name to bind to
    /// </summary>
    public string ExchangeName { get; set; } = string.Empty;
    
    /// <summary>
    /// Routing key for binding
    /// </summary>
    public string RoutingKey { get; set; } = string.Empty;
    
    /// <summary>
    /// Binding arguments
    /// </summary>
    public IDictionary<string, object>? Arguments { get; set; }
    
    /// <summary>
    /// Binding priority
    /// </summary>
    public int? Priority { get; set; }
    
    /// <summary>
    /// Whether to create exchange if it doesn't exist
    /// </summary>
    public bool CreateExchangeIfNotExists { get; set; } = false;
    
    /// <summary>
    /// Exchange settings for creation (if needed)
    /// </summary>
    public ExchangeSettings? ExchangeSettings { get; set; }
    
    /// <summary>
    /// Correlation ID for tracing
    /// </summary>
    public string? CorrelationId { get; set; }
    
    /// <summary>
    /// Creates a bind queue command
    /// </summary>
    /// <param name="queueName">Queue name</param>
    /// <param name="exchangeName">Exchange name</param>
    /// <param name="routingKey">Routing key</param>
    /// <param name="arguments">Binding arguments</param>
    /// <param name="priority">Binding priority</param>
    /// <param name="correlationId">Correlation ID</param>
    /// <returns>Bind queue command</returns>
    public static BindQueueCommand Create(string queueName, string exchangeName, string routingKey, 
        IDictionary<string, object>? arguments = null, int? priority = null, string? correlationId = null)
    {
        return new BindQueueCommand
        {
            QueueName = queueName,
            ExchangeName = exchangeName,
            RoutingKey = routingKey,
            Arguments = arguments,
            Priority = priority,
            CorrelationId = correlationId
        };
    }
}

/// <summary>
/// Command to unbind a queue from an exchange
/// </summary>
public class UnbindQueueCommand : IRequest<QueueBindingResult>
{
    /// <summary>
    /// Queue name to unbind
    /// </summary>
    public string QueueName { get; set; } = string.Empty;
    
    /// <summary>
    /// Exchange name to unbind from
    /// </summary>
    public string ExchangeName { get; set; } = string.Empty;
    
    /// <summary>
    /// Routing key for binding
    /// </summary>
    public string RoutingKey { get; set; } = string.Empty;
    
    /// <summary>
    /// Binding arguments
    /// </summary>
    public IDictionary<string, object>? Arguments { get; set; }
    
    /// <summary>
    /// Reason for unbinding
    /// </summary>
    public string? Reason { get; set; }
    
    /// <summary>
    /// Correlation ID for tracing
    /// </summary>
    public string? CorrelationId { get; set; }
    
    /// <summary>
    /// Creates an unbind queue command
    /// </summary>
    /// <param name="queueName">Queue name</param>
    /// <param name="exchangeName">Exchange name</param>
    /// <param name="routingKey">Routing key</param>
    /// <param name="arguments">Binding arguments</param>
    /// <param name="reason">Unbinding reason</param>
    /// <param name="correlationId">Correlation ID</param>
    /// <returns>Unbind queue command</returns>
    public static UnbindQueueCommand Create(string queueName, string exchangeName, string routingKey, 
        IDictionary<string, object>? arguments = null, string? reason = null, string? correlationId = null)
    {
        return new UnbindQueueCommand
        {
            QueueName = queueName,
            ExchangeName = exchangeName,
            RoutingKey = routingKey,
            Arguments = arguments,
            Reason = reason,
            CorrelationId = correlationId
        };
    }
}

/// <summary>
/// Command to purge a queue
/// </summary>
public class PurgeQueueCommand : IRequest<QueuePurgeResult>
{
    /// <summary>
    /// Queue name to purge
    /// </summary>
    public string Name { get; set; } = string.Empty;
    
    /// <summary>
    /// Whether to require confirmation
    /// </summary>
    public bool RequireConfirmation { get; set; } = true;
    
    /// <summary>
    /// Maximum number of messages to purge (0 = all)
    /// </summary>
    public uint? MaxMessages { get; set; }
    
    /// <summary>
    /// Reason for purging
    /// </summary>
    public string? Reason { get; set; }
    
    /// <summary>
    /// Correlation ID for tracing
    /// </summary>
    public string? CorrelationId { get; set; }
    
    /// <summary>
    /// Creates a purge queue command
    /// </summary>
    /// <param name="name">Queue name</param>
    /// <param name="requireConfirmation">Require confirmation</param>
    /// <param name="maxMessages">Maximum messages to purge</param>
    /// <param name="reason">Purge reason</param>
    /// <param name="correlationId">Correlation ID</param>
    /// <returns>Purge queue command</returns>
    public static PurgeQueueCommand Create(string name, bool requireConfirmation = true, uint? maxMessages = null, 
        string? reason = null, string? correlationId = null)
    {
        return new PurgeQueueCommand
        {
            Name = name,
            RequireConfirmation = requireConfirmation,
            MaxMessages = maxMessages,
            Reason = reason,
            CorrelationId = correlationId
        };
    }
}

/// <summary>
/// Command to declare multiple queues at once
/// </summary>
public class DeclareQueuesCommand : IRequest<QueueBatchDeclarationResult>
{
    /// <summary>
    /// Queue settings to declare
    /// </summary>
    public IEnumerable<QueueSettings> QueueSettings { get; set; } = Enumerable.Empty<QueueSettings>();
    
    /// <summary>
    /// Manual queue declarations
    /// </summary>
    public IEnumerable<QueueDeclarationInfo> ManualDeclarations { get; set; } = Enumerable.Empty<QueueDeclarationInfo>();
    
    /// <summary>
    /// Whether to stop on first failure
    /// </summary>
    public bool StopOnFailure { get; set; } = false;
    
    /// <summary>
    /// Whether to apply bindings
    /// </summary>
    public bool ApplyBindings { get; set; } = true;
    
    /// <summary>
    /// Maximum parallelism for declarations
    /// </summary>
    public int MaxParallelism { get; set; } = 5;
    
    /// <summary>
    /// Correlation ID for tracing
    /// </summary>
    public string? CorrelationId { get; set; }
    
    /// <summary>
    /// Creates a declare queues command from settings
    /// </summary>
    /// <param name="queueSettings">Queue settings</param>
    /// <param name="stopOnFailure">Stop on failure</param>
    /// <param name="applyBindings">Apply bindings</param>
    /// <param name="maxParallelism">Maximum parallelism</param>
    /// <param name="correlationId">Correlation ID</param>
    /// <returns>Declare queues command</returns>
    public static DeclareQueuesCommand CreateFromSettings(IEnumerable<QueueSettings> queueSettings, 
        bool stopOnFailure = false, bool applyBindings = true, int maxParallelism = 5, string? correlationId = null)
    {
        return new DeclareQueuesCommand
        {
            QueueSettings = queueSettings,
            StopOnFailure = stopOnFailure,
            ApplyBindings = applyBindings,
            MaxParallelism = maxParallelism,
            CorrelationId = correlationId
        };
    }
    
    /// <summary>
    /// Creates a declare queues command from manual declarations
    /// </summary>
    /// <param name="manualDeclarations">Manual declarations</param>
    /// <param name="stopOnFailure">Stop on failure</param>
    /// <param name="maxParallelism">Maximum parallelism</param>
    /// <param name="correlationId">Correlation ID</param>
    /// <returns>Declare queues command</returns>
    public static DeclareQueuesCommand CreateManual(IEnumerable<QueueDeclarationInfo> manualDeclarations, 
        bool stopOnFailure = false, int maxParallelism = 5, string? correlationId = null)
    {
        return new DeclareQueuesCommand
        {
            ManualDeclarations = manualDeclarations,
            StopOnFailure = stopOnFailure,
            MaxParallelism = maxParallelism,
            CorrelationId = correlationId
        };
    }
}

/// <summary>
/// Command to redeclare all queues (auto-recovery)
/// </summary>
public class RedeclareQueuesCommand : IRequest<QueueRecoveryResult>
{
    /// <summary>
    /// Recovery strategy to use
    /// </summary>
    public QueueRecoveryStrategy Strategy { get; set; } = QueueRecoveryStrategy.Automatic;
    
    /// <summary>
    /// Maximum recovery attempts
    /// </summary>
    public int? MaxAttempts { get; set; }
    
    /// <summary>
    /// Delay between recovery attempts
    /// </summary>
    public TimeSpan? DelayBetweenAttempts { get; set; }
    
    /// <summary>
    /// Whether to include bindings in recovery
    /// </summary>
    public bool IncludeBindings { get; set; } = true;
    
    /// <summary>
    /// Recovery timeout
    /// </summary>
    public TimeSpan? Timeout { get; set; }
    
    /// <summary>
    /// Correlation ID for tracing
    /// </summary>
    public string? CorrelationId { get; set; }
    
    /// <summary>
    /// Creates a redeclare queues command
    /// </summary>
    /// <param name="strategy">Recovery strategy</param>
    /// <param name="maxAttempts">Maximum attempts</param>
    /// <param name="delayBetweenAttempts">Delay between attempts</param>
    /// <param name="includeBindings">Include bindings</param>
    /// <param name="timeout">Recovery timeout</param>
    /// <param name="correlationId">Correlation ID</param>
    /// <returns>Redeclare queues command</returns>
    public static RedeclareQueuesCommand Create(QueueRecoveryStrategy strategy = QueueRecoveryStrategy.Automatic, 
        int? maxAttempts = null, TimeSpan? delayBetweenAttempts = null, bool includeBindings = true, 
        TimeSpan? timeout = null, string? correlationId = null)
    {
        return new RedeclareQueuesCommand
        {
            Strategy = strategy,
            MaxAttempts = maxAttempts,
            DelayBetweenAttempts = delayBetweenAttempts,
            IncludeBindings = includeBindings,
            Timeout = timeout,
            CorrelationId = correlationId
        };
    }
}

/// <summary>
/// Command to reset queue statistics
/// </summary>
public class ResetQueueStatisticsCommand : IRequest<QueueOperationResult>
{
    /// <summary>
    /// Statistics categories to reset
    /// </summary>
    public QueueStatisticsCategory Categories { get; set; } = QueueStatisticsCategory.All;
    
    /// <summary>
    /// Whether to require confirmation
    /// </summary>
    public bool RequireConfirmation { get; set; } = true;
    
    /// <summary>
    /// Correlation ID for tracing
    /// </summary>
    public string? CorrelationId { get; set; }
    
    /// <summary>
    /// Creates a reset queue statistics command
    /// </summary>
    /// <param name="categories">Categories to reset</param>
    /// <param name="requireConfirmation">Require confirmation</param>
    /// <param name="correlationId">Correlation ID</param>
    /// <returns>Reset queue statistics command</returns>
    public static ResetQueueStatisticsCommand Create(QueueStatisticsCategory categories = QueueStatisticsCategory.All, 
        bool requireConfirmation = true, string? correlationId = null)
    {
        return new ResetQueueStatisticsCommand
        {
            Categories = categories,
            RequireConfirmation = requireConfirmation,
            CorrelationId = correlationId
        };
    }
}

// Supporting enums and classes

/// <summary>
/// Queue recovery strategies
/// </summary>
public enum QueueRecoveryStrategy
{
    /// <summary>
    /// Automatic recovery using default settings
    /// </summary>
    Automatic,
    
    /// <summary>
    /// Conservative recovery (slower but safer)
    /// </summary>
    Conservative,
    
    /// <summary>
    /// Aggressive recovery (faster but riskier)
    /// </summary>
    Aggressive,
    
    /// <summary>
    /// Manual recovery with specific parameters
    /// </summary>
    Manual
}

/// <summary>
/// Queue statistics categories
/// </summary>
[Flags]
public enum QueueStatisticsCategory
{
    /// <summary>
    /// Operation counts
    /// </summary>
    Operations = 1,
    
    /// <summary>
    /// Performance metrics
    /// </summary>
    Performance = 2,
    
    /// <summary>
    /// Error statistics
    /// </summary>
    Errors = 4,
    
    /// <summary>
    /// Binding statistics
    /// </summary>
    Bindings = 8,
    
    /// <summary>
    /// All statistics categories
    /// </summary>
    All = Operations | Performance | Errors | Bindings
}

/// <summary>
/// Queue declaration information for manual declarations
/// </summary>
public class QueueDeclarationInfo
{
    /// <summary>
    /// Queue name
    /// </summary>
    public string Name { get; set; } = string.Empty;
    
    /// <summary>
    /// Whether queue is durable
    /// </summary>
    public bool Durable { get; set; } = true;
    
    /// <summary>
    /// Whether queue is exclusive
    /// </summary>
    public bool Exclusive { get; set; } = false;
    
    /// <summary>
    /// Whether queue auto-deletes
    /// </summary>
    public bool AutoDelete { get; set; } = false;
    
    /// <summary>
    /// Queue arguments
    /// </summary>
    public IDictionary<string, object>? Arguments { get; set; }
    
    /// <summary>
    /// Bindings to apply after declaration
    /// </summary>
    public IEnumerable<QueueBindingInfo>? Bindings { get; set; }
    
    /// <summary>
    /// Creates a basic queue declaration info
    /// </summary>
    /// <param name="name">Queue name</param>
    /// <param name="durable">Durable flag</param>
    /// <param name="exclusive">Exclusive flag</param>
    /// <param name="autoDelete">Auto-delete flag</param>
    /// <returns>Queue declaration info</returns>
    public static QueueDeclarationInfo Create(string name, bool durable = true, bool exclusive = false, bool autoDelete = false)
    {
        return new QueueDeclarationInfo
        {
            Name = name,
            Durable = durable,
            Exclusive = exclusive,
            AutoDelete = autoDelete
        };
    }
    
    /// <summary>
    /// Creates a quorum queue declaration info
    /// </summary>
    /// <param name="name">Queue name</param>
    /// <returns>Quorum queue declaration info</returns>
    public static QueueDeclarationInfo CreateQuorum(string name)
    {
        return new QueueDeclarationInfo
        {
            Name = name,
            Durable = true,
            Exclusive = false,
            AutoDelete = false,
            Arguments = new Dictionary<string, object> { { "x-queue-type", "quorum" } }
        };
    }
    
    /// <summary>
    /// Creates a priority queue declaration info
    /// </summary>
    /// <param name="name">Queue name</param>
    /// <param name="maxPriority">Maximum priority</param>
    /// <returns>Priority queue declaration info</returns>
    public static QueueDeclarationInfo CreatePriority(string name, int maxPriority = 10)
    {
        return new QueueDeclarationInfo
        {
            Name = name,
            Durable = true,
            Exclusive = false,
            AutoDelete = false,
            Arguments = new Dictionary<string, object> { { "x-max-priority", maxPriority } }
        };
    }
}

// Result models

/// <summary>
/// Result of queue declaration operation
/// </summary>
public class QueueDeclarationResult
{
    /// <summary>
    /// Whether the operation was successful
    /// </summary>
    public bool Success { get; set; }
    
    /// <summary>
    /// Queue name
    /// </summary>
    public string QueueName { get; set; } = string.Empty;
    
    /// <summary>
    /// Queue declaration result from RabbitMQ
    /// </summary>
    public QueueDeclareResult? DeclareResult { get; set; }
    
    /// <summary>
    /// Number of bindings applied
    /// </summary>
    public int BindingsApplied { get; set; }
    
    /// <summary>
    /// Operation duration
    /// </summary>
    public TimeSpan Duration { get; set; }
    
    /// <summary>
    /// Error message if not successful
    /// </summary>
    public string? ErrorMessage { get; set; }
    
    /// <summary>
    /// Exception if one occurred
    /// </summary>
    public Exception? Exception { get; set; }
    
    /// <summary>
    /// Operation timestamp
    /// </summary>
    public DateTimeOffset Timestamp { get; set; } = DateTimeOffset.UtcNow;
    
    /// <summary>
    /// Additional metadata
    /// </summary>
    public Dictionary<string, object> Metadata { get; set; } = new();
    
    /// <summary>
    /// Creates a success result
    /// </summary>
    /// <param name="queueName">Queue name</param>
    /// <param name="declareResult">Declaration result</param>
    /// <param name="bindingsApplied">Bindings applied count</param>
    /// <param name="duration">Operation duration</param>
    /// <returns>Success result</returns>
    public static QueueDeclarationResult CreateSuccess(string queueName, QueueDeclareResult? declareResult, 
        int bindingsApplied, TimeSpan duration)
    {
        return new QueueDeclarationResult
        {
            Success = true,
            QueueName = queueName,
            DeclareResult = declareResult,
            BindingsApplied = bindingsApplied,
            Duration = duration
        };
    }
    
    /// <summary>
    /// Creates a failure result
    /// </summary>
    /// <param name="queueName">Queue name</param>
    /// <param name="errorMessage">Error message</param>
    /// <param name="exception">Exception</param>
    /// <returns>Failure result</returns>
    public static QueueDeclarationResult CreateFailure(string queueName, string errorMessage, Exception? exception = null)
    {
        return new QueueDeclarationResult
        {
            Success = false,
            QueueName = queueName,
            ErrorMessage = errorMessage,
            Exception = exception
        };
    }
}

/// <summary>
/// Result of queue operation
/// </summary>
public class QueueOperationResult
{
    /// <summary>
    /// Whether the operation was successful
    /// </summary>
    public bool Success { get; set; }
    
    /// <summary>
    /// Queue name
    /// </summary>
    public string QueueName { get; set; } = string.Empty;
    
    /// <summary>
    /// Operation performed
    /// </summary>
    public string Operation { get; set; } = string.Empty;
    
    /// <summary>
    /// Operation result data
    /// </summary>
    public Dictionary<string, object> ResultData { get; set; } = new();
    
    /// <summary>
    /// Error message if not successful
    /// </summary>
    public string? ErrorMessage { get; set; }
    
    /// <summary>
    /// Exception if one occurred
    /// </summary>
    public Exception? Exception { get; set; }
    
    /// <summary>
    /// Operation timestamp
    /// </summary>
    public DateTimeOffset Timestamp { get; set; } = DateTimeOffset.UtcNow;
    
    /// <summary>
    /// Creates a success result
    /// </summary>
    /// <param name="queueName">Queue name</param>
    /// <param name="operation">Operation performed</param>
    /// <param name="resultData">Result data</param>
    /// <returns>Success result</returns>
    public static QueueOperationResult CreateSuccess(string queueName, string operation, Dictionary<string, object>? resultData = null)
    {
        return new QueueOperationResult
        {
            Success = true,
            QueueName = queueName,
            Operation = operation,
            ResultData = resultData ?? new Dictionary<string, object>()
        };
    }
    
    /// <summary>
    /// Creates a failure result
    /// </summary>
    /// <param name="queueName">Queue name</param>
    /// <param name="operation">Operation performed</param>
    /// <param name="errorMessage">Error message</param>
    /// <param name="exception">Exception</param>
    /// <returns>Failure result</returns>
    public static QueueOperationResult CreateFailure(string queueName, string operation, string errorMessage, Exception? exception = null)
    {
        return new QueueOperationResult
        {
            Success = false,
            QueueName = queueName,
            Operation = operation,
            ErrorMessage = errorMessage,
            Exception = exception
        };
    }
}

/// <summary>
/// Result of queue binding operation
/// </summary>
public class QueueBindingResult
{
    /// <summary>
    /// Whether the operation was successful
    /// </summary>
    public bool Success { get; set; }
    
    /// <summary>
    /// Queue name
    /// </summary>
    public string QueueName { get; set; } = string.Empty;
    
    /// <summary>
    /// Exchange name
    /// </summary>
    public string ExchangeName { get; set; } = string.Empty;
    
    /// <summary>
    /// Routing key
    /// </summary>
    public string RoutingKey { get; set; } = string.Empty;
    
    /// <summary>
    /// Operation performed (bind/unbind)
    /// </summary>
    public string Operation { get; set; } = string.Empty;
    
    /// <summary>
    /// Whether exchange was created
    /// </summary>
    public bool ExchangeCreated { get; set; }
    
    /// <summary>
    /// Error message if not successful
    /// </summary>
    public string? ErrorMessage { get; set; }
    
    /// <summary>
    /// Exception if one occurred
    /// </summary>
    public Exception? Exception { get; set; }
    
    /// <summary>
    /// Operation timestamp
    /// </summary>
    public DateTimeOffset Timestamp { get; set; } = DateTimeOffset.UtcNow;
    
    /// <summary>
    /// Creates a success result
    /// </summary>
    /// <param name="queueName">Queue name</param>
    /// <param name="exchangeName">Exchange name</param>
    /// <param name="routingKey">Routing key</param>
    /// <param name="operation">Operation performed</param>
    /// <param name="exchangeCreated">Exchange created flag</param>
    /// <returns>Success result</returns>
    public static QueueBindingResult CreateSuccess(string queueName, string exchangeName, string routingKey, 
        string operation, bool exchangeCreated = false)
    {
        return new QueueBindingResult
        {
            Success = true,
            QueueName = queueName,
            ExchangeName = exchangeName,
            RoutingKey = routingKey,
            Operation = operation,
            ExchangeCreated = exchangeCreated
        };
    }
    
    /// <summary>
    /// Creates a failure result
    /// </summary>
    /// <param name="queueName">Queue name</param>
    /// <param name="exchangeName">Exchange name</param>
    /// <param name="routingKey">Routing key</param>
    /// <param name="operation">Operation performed</param>
    /// <param name="errorMessage">Error message</param>
    /// <param name="exception">Exception</param>
    /// <returns>Failure result</returns>
    public static QueueBindingResult CreateFailure(string queueName, string exchangeName, string routingKey, 
        string operation, string errorMessage, Exception? exception = null)
    {
        return new QueueBindingResult
        {
            Success = false,
            QueueName = queueName,
            ExchangeName = exchangeName,
            RoutingKey = routingKey,
            Operation = operation,
            ErrorMessage = errorMessage,
            Exception = exception
        };
    }
}

/// <summary>
/// Result of queue purge operation
/// </summary>
public class QueuePurgeResult
{
    /// <summary>
    /// Whether the operation was successful
    /// </summary>
    public bool Success { get; set; }
    
    /// <summary>
    /// Queue name
    /// </summary>
    public string QueueName { get; set; } = string.Empty;
    
    /// <summary>
    /// Number of messages purged
    /// </summary>
    public uint MessagesPurged { get; set; }
    
    /// <summary>
    /// Operation duration
    /// </summary>
    public TimeSpan Duration { get; set; }
    
    /// <summary>
    /// Error message if not successful
    /// </summary>
    public string? ErrorMessage { get; set; }
    
    /// <summary>
    /// Exception if one occurred
    /// </summary>
    public Exception? Exception { get; set; }
    
    /// <summary>
    /// Operation timestamp
    /// </summary>
    public DateTimeOffset Timestamp { get; set; } = DateTimeOffset.UtcNow;
    
    /// <summary>
    /// Creates a success result
    /// </summary>
    /// <param name="queueName">Queue name</param>
    /// <param name="messagesPurged">Messages purged count</param>
    /// <param name="duration">Operation duration</param>
    /// <returns>Success result</returns>
    public static QueuePurgeResult CreateSuccess(string queueName, uint messagesPurged, TimeSpan duration)
    {
        return new QueuePurgeResult
        {
            Success = true,
            QueueName = queueName,
            MessagesPurged = messagesPurged,
            Duration = duration
        };
    }
    
    /// <summary>
    /// Creates a failure result
    /// </summary>
    /// <param name="queueName">Queue name</param>
    /// <param name="errorMessage">Error message</param>
    /// <param name="exception">Exception</param>
    /// <returns>Failure result</returns>
    public static QueuePurgeResult CreateFailure(string queueName, string errorMessage, Exception? exception = null)
    {
        return new QueuePurgeResult
        {
            Success = false,
            QueueName = queueName,
            ErrorMessage = errorMessage,
            Exception = exception
        };
    }
}

/// <summary>
/// Result of batch queue declaration operation
/// </summary>
public class QueueBatchDeclarationResult
{
    /// <summary>
    /// Whether all operations were successful
    /// </summary>
    public bool Success { get; set; }
    
    /// <summary>
    /// Individual declaration results
    /// </summary>
    public IEnumerable<QueueDeclarationResult> Results { get; set; } = Enumerable.Empty<QueueDeclarationResult>();
    
    /// <summary>
    /// Total queues processed
    /// </summary>
    public int TotalQueues { get; set; }
    
    /// <summary>
    /// Number of successful declarations
    /// </summary>
    public int SuccessfulDeclarations { get; set; }
    
    /// <summary>
    /// Number of failed declarations
    /// </summary>
    public int FailedDeclarations { get; set; }
    
    /// <summary>
    /// Total operation duration
    /// </summary>
    public TimeSpan Duration { get; set; }
    
    /// <summary>
    /// Overall error message if applicable
    /// </summary>
    public string? ErrorMessage { get; set; }
    
    /// <summary>
    /// Operation timestamp
    /// </summary>
    public DateTimeOffset Timestamp { get; set; } = DateTimeOffset.UtcNow;
    
    /// <summary>
    /// Creates a batch declaration result
    /// </summary>
    /// <param name="results">Individual results</param>
    /// <param name="duration">Total duration</param>
    /// <returns>Batch declaration result</returns>
    public static QueueBatchDeclarationResult Create(IEnumerable<QueueDeclarationResult> results, TimeSpan duration)
    {
        var resultList = results.ToList();
        var successfulCount = resultList.Count(r => r.Success);
        var failedCount = resultList.Count - successfulCount;
        
        return new QueueBatchDeclarationResult
        {
            Success = failedCount == 0,
            Results = resultList,
            TotalQueues = resultList.Count,
            SuccessfulDeclarations = successfulCount,
            FailedDeclarations = failedCount,
            Duration = duration
        };
    }
}

/// <summary>
/// Result of queue recovery operation
/// </summary>
public class QueueRecoveryResult
{
    /// <summary>
    /// Whether recovery was successful
    /// </summary>
    public bool Success { get; set; }
    
    /// <summary>
    /// Recovery strategy used
    /// </summary>
    public QueueRecoveryStrategy Strategy { get; set; }
    
    /// <summary>
    /// Number of queues recovered
    /// </summary>
    public int QueuesRecovered { get; set; }
    
    /// <summary>
    /// Number of bindings recovered
    /// </summary>
    public int BindingsRecovered { get; set; }
    
    /// <summary>
    /// Recovery attempts made
    /// </summary>
    public int RecoveryAttempts { get; set; }
    
    /// <summary>
    /// Recovery duration
    /// </summary>
    public TimeSpan Duration { get; set; }
    
    /// <summary>
    /// Recovery errors
    /// </summary>
    public List<string> Errors { get; set; } = new();
    
    /// <summary>
    /// Overall error message if recovery failed
    /// </summary>
    public string? ErrorMessage { get; set; }
    
    /// <summary>
    /// Exception if one occurred
    /// </summary>
    public Exception? Exception { get; set; }
    
    /// <summary>
    /// Operation timestamp
    /// </summary>
    public DateTimeOffset Timestamp { get; set; } = DateTimeOffset.UtcNow;
    
    /// <summary>
    /// Creates a success result
    /// </summary>
    /// <param name="strategy">Recovery strategy</param>
    /// <param name="queuesRecovered">Queues recovered count</param>
    /// <param name="bindingsRecovered">Bindings recovered count</param>
    /// <param name="recoveryAttempts">Recovery attempts</param>
    /// <param name="duration">Recovery duration</param>
    /// <returns>Success result</returns>
    public static QueueRecoveryResult CreateSuccess(QueueRecoveryStrategy strategy, int queuesRecovered, 
        int bindingsRecovered, int recoveryAttempts, TimeSpan duration)
    {
        return new QueueRecoveryResult
        {
            Success = true,
            Strategy = strategy,
            QueuesRecovered = queuesRecovered,
            BindingsRecovered = bindingsRecovered,
            RecoveryAttempts = recoveryAttempts,
            Duration = duration
        };
    }
    
    /// <summary>
    /// Creates a failure result
    /// </summary>
    /// <param name="strategy">Recovery strategy</param>
    /// <param name="recoveryAttempts">Recovery attempts</param>
    /// <param name="duration">Recovery duration</param>
    /// <param name="errorMessage">Error message</param>
    /// <param name="exception">Exception</param>
    /// <returns>Failure result</returns>
    public static QueueRecoveryResult CreateFailure(QueueRecoveryStrategy strategy, int recoveryAttempts, 
        TimeSpan duration, string errorMessage, Exception? exception = null)
    {
        return new QueueRecoveryResult
        {
            Success = false,
            Strategy = strategy,
            RecoveryAttempts = recoveryAttempts,
            Duration = duration,
            ErrorMessage = errorMessage,
            Exception = exception
        };
    }
} 