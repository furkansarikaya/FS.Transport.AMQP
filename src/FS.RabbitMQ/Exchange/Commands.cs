using FS.Mediator.Features.RequestHandling.Core;
using FS.RabbitMQ.Configuration;
using RabbitMQ.Client;

namespace FS.RabbitMQ.Exchange;

/// <summary>
/// Command to declare an exchange
/// </summary>
public class DeclareExchangeCommand : IRequest<ExchangeDeclarationResult>
{
    /// <summary>
    /// Exchange settings to use (optional, if null will use manual parameters)
    /// </summary>
    public ExchangeSettings? Settings { get; set; }
    
    /// <summary>
    /// Exchange name
    /// </summary>
    public string Name { get; set; } = string.Empty;
    
    /// <summary>
    /// Exchange type
    /// </summary>
    public string Type { get; set; } = ExchangeType.Topic;
    
    /// <summary>
    /// Whether exchange is durable
    /// </summary>
    public bool Durable { get; set; } = true;
    
    /// <summary>
    /// Whether exchange auto-deletes
    /// </summary>
    public bool AutoDelete { get; set; } = false;
    
    /// <summary>
    /// Whether exchange is internal
    /// </summary>
    public bool Internal { get; set; } = false;
    
    /// <summary>
    /// Additional exchange arguments
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
    /// Creates a declare exchange command using settings
    /// </summary>
    public static DeclareExchangeCommand CreateFromSettings(ExchangeSettings settings, bool applyBindings = true, string? correlationId = null)
    {
        return new DeclareExchangeCommand
        {
            Settings = settings,
            ApplyBindings = applyBindings,
            CorrelationId = correlationId
        };
    }
    
    /// <summary>
    /// Creates a declare exchange command with manual parameters
    /// </summary>
    public static DeclareExchangeCommand CreateManual(string name, string type = ExchangeType.Topic, bool durable = true, 
        bool autoDelete = false, bool @internal = false, IDictionary<string, object>? arguments = null, string? correlationId = null)
    {
        return new DeclareExchangeCommand
        {
            Name = name,
            Type = type,
            Durable = durable,
            AutoDelete = autoDelete,
            Internal = @internal,
            Arguments = arguments,
            CorrelationId = correlationId
        };
    }
    
    /// <summary>
    /// Creates a topic exchange command
    /// </summary>
    public static DeclareExchangeCommand CreateTopic(string name, bool durable = true, bool autoDelete = false, string? correlationId = null)
    {
        return CreateManual(name, ExchangeType.Topic, durable, autoDelete, false, null, correlationId);
    }
    
    /// <summary>
    /// Creates a direct exchange command
    /// </summary>
    public static DeclareExchangeCommand CreateDirect(string name, bool durable = true, bool autoDelete = false, string? correlationId = null)
    {
        return CreateManual(name, ExchangeType.Direct, durable, autoDelete, false, null, correlationId);
    }
    
    /// <summary>
    /// Creates a fanout exchange command
    /// </summary>
    public static DeclareExchangeCommand CreateFanout(string name, bool durable = true, bool autoDelete = false, string? correlationId = null)
    {
        return CreateManual(name, ExchangeType.Fanout, durable, autoDelete, false, null, correlationId);
    }
    
    /// <summary>
    /// Creates a headers exchange command
    /// </summary>
    public static DeclareExchangeCommand CreateHeaders(string name, bool durable = true, bool autoDelete = false, string? correlationId = null)
    {
        return CreateManual(name, ExchangeType.Headers, durable, autoDelete, false, null, correlationId);
    }
}

/// <summary>
/// Command to delete an exchange
/// </summary>
public class DeleteExchangeCommand : IRequest<ExchangeOperationResult>
{
    /// <summary>
    /// Exchange name to delete
    /// </summary>
    public string Name { get; set; } = string.Empty;
    
    /// <summary>
    /// Only delete if unused
    /// </summary>
    public bool IfUnused { get; set; } = false;
    
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
    /// Creates a delete exchange command
    /// </summary>
    public static DeleteExchangeCommand Create(string name, bool ifUnused = false, bool force = false, string? reason = null, string? correlationId = null)
    {
        return new DeleteExchangeCommand
        {
            Name = name,
            IfUnused = ifUnused,
            Force = force,
            Reason = reason,
            CorrelationId = correlationId
        };
    }
}

/// <summary>
/// Command to bind an exchange to another exchange
/// </summary>
public class BindExchangeCommand : IRequest<ExchangeBindingResult>
{
    /// <summary>
    /// Destination exchange name
    /// </summary>
    public string DestinationExchange { get; set; } = string.Empty;
    
    /// <summary>
    /// Source exchange name
    /// </summary>
    public string SourceExchange { get; set; } = string.Empty;
    
    /// <summary>
    /// Routing key for binding
    /// </summary>
    public string RoutingKey { get; set; } = string.Empty;
    
    /// <summary>
    /// Binding arguments
    /// </summary>
    public IDictionary<string, object>? Arguments { get; set; }
    
    /// <summary>
    /// Whether to create source exchange if it doesn't exist
    /// </summary>
    public bool CreateSourceIfNotExists { get; set; } = false;
    
    /// <summary>
    /// Whether to create destination exchange if it doesn't exist
    /// </summary>
    public bool CreateDestinationIfNotExists { get; set; } = false;
    
    /// <summary>
    /// Source exchange settings for creation (if needed)
    /// </summary>
    public ExchangeSettings? SourceExchangeSettings { get; set; }
    
    /// <summary>
    /// Destination exchange settings for creation (if needed)
    /// </summary>
    public ExchangeSettings? DestinationExchangeSettings { get; set; }
    
    /// <summary>
    /// Correlation ID for tracing
    /// </summary>
    public string? CorrelationId { get; set; }
    
    /// <summary>
    /// Creates a bind exchange command
    /// </summary>
    public static BindExchangeCommand Create(string destinationExchange, string sourceExchange, string routingKey, 
        IDictionary<string, object>? arguments = null, string? correlationId = null)
    {
        return new BindExchangeCommand
        {
            DestinationExchange = destinationExchange,
            SourceExchange = sourceExchange,
            RoutingKey = routingKey,
            Arguments = arguments,
            CorrelationId = correlationId
        };
    }
}

/// <summary>
/// Command to unbind an exchange from another exchange
/// </summary>
public class UnbindExchangeCommand : IRequest<ExchangeBindingResult>
{
    /// <summary>
    /// Destination exchange name
    /// </summary>
    public string DestinationExchange { get; set; } = string.Empty;
    
    /// <summary>
    /// Source exchange name
    /// </summary>
    public string SourceExchange { get; set; } = string.Empty;
    
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
    /// Creates an unbind exchange command
    /// </summary>
    public static UnbindExchangeCommand Create(string destinationExchange, string sourceExchange, string routingKey, 
        IDictionary<string, object>? arguments = null, string? reason = null, string? correlationId = null)
    {
        return new UnbindExchangeCommand
        {
            DestinationExchange = destinationExchange,
            SourceExchange = sourceExchange,
            RoutingKey = routingKey,
            Arguments = arguments,
            Reason = reason,
            CorrelationId = correlationId
        };
    }
}

/// <summary>
/// Command to declare multiple exchanges at once
/// </summary>
public class DeclareExchangesCommand : IRequest<ExchangeBatchDeclarationResult>
{
    /// <summary>
    /// Exchange settings to declare
    /// </summary>
    public IEnumerable<ExchangeSettings> ExchangeSettings { get; set; } = Enumerable.Empty<ExchangeSettings>();
    
    /// <summary>
    /// Manual exchange declarations
    /// </summary>
    public IEnumerable<ExchangeDeclarationInfo> ManualDeclarations { get; set; } = Enumerable.Empty<ExchangeDeclarationInfo>();
    
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
    /// Creates a declare exchanges command from settings
    /// </summary>
    public static DeclareExchangesCommand CreateFromSettings(IEnumerable<ExchangeSettings> exchangeSettings, 
        bool stopOnFailure = false, bool applyBindings = true, int maxParallelism = 5, string? correlationId = null)
    {
        return new DeclareExchangesCommand
        {
            ExchangeSettings = exchangeSettings,
            StopOnFailure = stopOnFailure,
            ApplyBindings = applyBindings,
            MaxParallelism = maxParallelism,
            CorrelationId = correlationId
        };
    }
    
    /// <summary>
    /// Creates a declare exchanges command from manual declarations
    /// </summary>
    public static DeclareExchangesCommand CreateManual(IEnumerable<ExchangeDeclarationInfo> manualDeclarations, 
        bool stopOnFailure = false, int maxParallelism = 5, string? correlationId = null)
    {
        return new DeclareExchangesCommand
        {
            ManualDeclarations = manualDeclarations,
            StopOnFailure = stopOnFailure,
            MaxParallelism = maxParallelism,
            CorrelationId = correlationId
        };
    }
}

/// <summary>
/// Command to redeclare all exchanges (auto-recovery)
/// </summary>
public class RedeclareExchangesCommand : IRequest<ExchangeRecoveryResult>
{
    /// <summary>
    /// Recovery strategy to use
    /// </summary>
    public ExchangeRecoveryStrategy Strategy { get; set; } = ExchangeRecoveryStrategy.Automatic;
    
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
    /// Creates a redeclare exchanges command
    /// </summary>
    public static RedeclareExchangesCommand Create(ExchangeRecoveryStrategy strategy = ExchangeRecoveryStrategy.Automatic, 
        int? maxAttempts = null, TimeSpan? delayBetweenAttempts = null, bool includeBindings = true, 
        TimeSpan? timeout = null, string? correlationId = null)
    {
        return new RedeclareExchangesCommand
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
/// Command to reset exchange statistics
/// </summary>
public class ResetExchangeStatisticsCommand : IRequest<ExchangeOperationResult>
{
    /// <summary>
    /// Statistics categories to reset
    /// </summary>
    public ExchangeStatisticsCategory Categories { get; set; } = ExchangeStatisticsCategory.All;
    
    /// <summary>
    /// Whether to require confirmation
    /// </summary>
    public bool RequireConfirmation { get; set; } = true;
    
    /// <summary>
    /// Correlation ID for tracing
    /// </summary>
    public string? CorrelationId { get; set; }
    
    /// <summary>
    /// Creates a reset exchange statistics command
    /// </summary>
    public static ResetExchangeStatisticsCommand Create(ExchangeStatisticsCategory categories = ExchangeStatisticsCategory.All, 
        bool requireConfirmation = true, string? correlationId = null)
    {
        return new ResetExchangeStatisticsCommand
        {
            Categories = categories,
            RequireConfirmation = requireConfirmation,
            CorrelationId = correlationId
        };
    }
}

// Supporting enums and classes

/// <summary>
/// Exchange recovery strategies
/// </summary>
public enum ExchangeRecoveryStrategy
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
/// Exchange statistics categories
/// </summary>
[Flags]
public enum ExchangeStatisticsCategory
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
/// Exchange declaration information for manual declarations
/// </summary>
public class ExchangeDeclarationInfo
{
    /// <summary>
    /// Exchange name
    /// </summary>
    public string Name { get; set; } = string.Empty;
    
    /// <summary>
    /// Exchange type
    /// </summary>
    public string Type { get; set; } = ExchangeType.Topic;
    
    /// <summary>
    /// Whether exchange is durable
    /// </summary>
    public bool Durable { get; set; } = true;
    
    /// <summary>
    /// Whether exchange auto-deletes
    /// </summary>
    public bool AutoDelete { get; set; } = false;
    
    /// <summary>
    /// Whether exchange is internal
    /// </summary>
    public bool Internal { get; set; } = false;
    
    /// <summary>
    /// Exchange arguments
    /// </summary>
    public IDictionary<string, object>? Arguments { get; set; }
    
    /// <summary>
    /// Bindings to apply after declaration
    /// </summary>
    public IEnumerable<ExchangeBindingInfo>? Bindings { get; set; }
    
    /// <summary>
    /// Creates a basic exchange declaration info
    /// </summary>
    public static ExchangeDeclarationInfo Create(string name, string type = ExchangeType.Topic, bool durable = true, 
        bool autoDelete = false, bool @internal = false)
    {
        return new ExchangeDeclarationInfo
        {
            Name = name,
            Type = type,
            Durable = durable,
            AutoDelete = autoDelete,
            Internal = @internal
        };
    }
    
    /// <summary>
    /// Creates a topic exchange declaration info
    /// </summary>
    public static ExchangeDeclarationInfo CreateTopic(string name, bool durable = true, bool autoDelete = false)
    {
        return Create(name, ExchangeType.Topic, durable, autoDelete);
    }
    
    /// <summary>
    /// Creates a direct exchange declaration info
    /// </summary>
    public static ExchangeDeclarationInfo CreateDirect(string name, bool durable = true, bool autoDelete = false)
    {
        return Create(name, ExchangeType.Direct, durable, autoDelete);
    }
    
    /// <summary>
    /// Creates a fanout exchange declaration info
    /// </summary>
    public static ExchangeDeclarationInfo CreateFanout(string name, bool durable = true, bool autoDelete = false)
    {
        return Create(name, ExchangeType.Fanout, durable, autoDelete);
    }
    
    /// <summary>
    /// Creates a headers exchange declaration info
    /// </summary>
    public static ExchangeDeclarationInfo CreateHeaders(string name, bool durable = true, bool autoDelete = false)
    {
        return Create(name, ExchangeType.Headers, durable, autoDelete);
    }
    
    /// <summary>
    /// Creates a delayed exchange declaration info
    /// </summary>
    public static ExchangeDeclarationInfo CreateDelayed(string name, string baseType = ExchangeType.Topic, bool durable = true, bool autoDelete = false)
    {
        return new ExchangeDeclarationInfo
        {
            Name = name,
            Type = "x-delayed-message",
            Durable = durable,
            AutoDelete = autoDelete,
            Internal = false,
            Arguments = new Dictionary<string, object> { { "x-delayed-type", baseType } }
        };
    }
}

/// <summary>
/// Exchange binding information
/// </summary>
public class ExchangeBindingInfo
{
    /// <summary>
    /// Destination exchange name
    /// </summary>
    public string DestinationExchange { get; set; } = string.Empty;
    
    /// <summary>
    /// Source exchange name
    /// </summary>
    public string SourceExchange { get; set; } = string.Empty;
    
    /// <summary>
    /// Routing key
    /// </summary>
    public string RoutingKey { get; set; } = string.Empty;
    
    /// <summary>
    /// Binding arguments
    /// </summary>
    public IDictionary<string, object>? Arguments { get; set; }
    
    /// <summary>
    /// Binding timestamp
    /// </summary>
    public DateTimeOffset BoundAt { get; set; } = DateTimeOffset.UtcNow;
    
    /// <summary>
    /// Creates a basic exchange binding info
    /// </summary>
    public static ExchangeBindingInfo Create(string destinationExchange, string sourceExchange, string routingKey, 
        IDictionary<string, object>? arguments = null)
    {
        return new ExchangeBindingInfo
        {
            DestinationExchange = destinationExchange,
            SourceExchange = sourceExchange,
            RoutingKey = routingKey,
            Arguments = arguments
        };
    }
}

// Result models

/// <summary>
/// Result of exchange declaration operation
/// </summary>
public class ExchangeDeclarationResult
{
    /// <summary>
    /// Whether the operation was successful
    /// </summary>
    public bool Success { get; set; }
    
    /// <summary>
    /// Exchange name
    /// </summary>
    public string ExchangeName { get; set; } = string.Empty;
    
    /// <summary>
    /// Exchange type
    /// </summary>
    public string ExchangeType { get; set; } = string.Empty;
    
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
    public static ExchangeDeclarationResult CreateSuccess(string exchangeName, string exchangeType, int bindingsApplied, TimeSpan duration)
    {
        return new ExchangeDeclarationResult
        {
            Success = true,
            ExchangeName = exchangeName,
            ExchangeType = exchangeType,
            BindingsApplied = bindingsApplied,
            Duration = duration
        };
    }
    
    /// <summary>
    /// Creates a failure result
    /// </summary>
    public static ExchangeDeclarationResult CreateFailure(string exchangeName, string errorMessage, Exception? exception = null)
    {
        return new ExchangeDeclarationResult
        {
            Success = false,
            ExchangeName = exchangeName,
            ErrorMessage = errorMessage,
            Exception = exception
        };
    }
}

/// <summary>
/// Result of exchange operation
/// </summary>
public class ExchangeOperationResult
{
    /// <summary>
    /// Whether the operation was successful
    /// </summary>
    public bool Success { get; set; }
    
    /// <summary>
    /// Exchange name
    /// </summary>
    public string ExchangeName { get; set; } = string.Empty;
    
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
    public static ExchangeOperationResult CreateSuccess(string exchangeName, string operation, Dictionary<string, object>? resultData = null)
    {
        return new ExchangeOperationResult
        {
            Success = true,
            ExchangeName = exchangeName,
            Operation = operation,
            ResultData = resultData ?? new Dictionary<string, object>()
        };
    }
    
    /// <summary>
    /// Creates a failure result
    /// </summary>
    public static ExchangeOperationResult CreateFailure(string exchangeName, string operation, string errorMessage, Exception? exception = null)
    {
        return new ExchangeOperationResult
        {
            Success = false,
            ExchangeName = exchangeName,
            Operation = operation,
            ErrorMessage = errorMessage,
            Exception = exception
        };
    }
}

/// <summary>
/// Result of exchange binding operation
/// </summary>
public class ExchangeBindingResult
{
    /// <summary>
    /// Whether the operation was successful
    /// </summary>
    public bool Success { get; set; }
    
    /// <summary>
    /// Destination exchange name
    /// </summary>
    public string DestinationExchange { get; set; } = string.Empty;
    
    /// <summary>
    /// Source exchange name
    /// </summary>
    public string SourceExchange { get; set; } = string.Empty;
    
    /// <summary>
    /// Routing key
    /// </summary>
    public string RoutingKey { get; set; } = string.Empty;
    
    /// <summary>
    /// Operation performed (bind/unbind)
    /// </summary>
    public string Operation { get; set; } = string.Empty;
    
    /// <summary>
    /// Whether source exchange was created
    /// </summary>
    public bool SourceExchangeCreated { get; set; }
    
    /// <summary>
    /// Whether destination exchange was created
    /// </summary>
    public bool DestinationExchangeCreated { get; set; }
    
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
    public static ExchangeBindingResult CreateSuccess(string destinationExchange, string sourceExchange, string routingKey, 
        string operation, bool sourceExchangeCreated = false, bool destinationExchangeCreated = false)
    {
        return new ExchangeBindingResult
        {
            Success = true,
            DestinationExchange = destinationExchange,
            SourceExchange = sourceExchange,
            RoutingKey = routingKey,
            Operation = operation,
            SourceExchangeCreated = sourceExchangeCreated,
            DestinationExchangeCreated = destinationExchangeCreated
        };
    }
    
    /// <summary>
    /// Creates a failure result
    /// </summary>
    public static ExchangeBindingResult CreateFailure(string destinationExchange, string sourceExchange, string routingKey, 
        string operation, string errorMessage, Exception? exception = null)
    {
        return new ExchangeBindingResult
        {
            Success = false,
            DestinationExchange = destinationExchange,
            SourceExchange = sourceExchange,
            RoutingKey = routingKey,
            Operation = operation,
            ErrorMessage = errorMessage,
            Exception = exception
        };
    }
}

/// <summary>
/// Result of batch exchange declaration operation
/// </summary>
public class ExchangeBatchDeclarationResult
{
    /// <summary>
    /// Whether all operations were successful
    /// </summary>
    public bool Success { get; set; }
    
    /// <summary>
    /// Individual declaration results
    /// </summary>
    public IEnumerable<ExchangeDeclarationResult> Results { get; set; } = Enumerable.Empty<ExchangeDeclarationResult>();
    
    /// <summary>
    /// Total exchanges processed
    /// </summary>
    public int TotalExchanges { get; set; }
    
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
    public static ExchangeBatchDeclarationResult Create(IEnumerable<ExchangeDeclarationResult> results, TimeSpan duration)
    {
        var resultList = results.ToList();
        var successfulCount = resultList.Count(r => r.Success);
        var failedCount = resultList.Count - successfulCount;
        
        return new ExchangeBatchDeclarationResult
        {
            Success = failedCount == 0,
            Results = resultList,
            TotalExchanges = resultList.Count,
            SuccessfulDeclarations = successfulCount,
            FailedDeclarations = failedCount,
            Duration = duration
        };
    }
}

/// <summary>
/// Result of exchange recovery operation
/// </summary>
public class ExchangeRecoveryResult
{
    /// <summary>
    /// Whether recovery was successful
    /// </summary>
    public bool Success { get; set; }
    
    /// <summary>
    /// Recovery strategy used
    /// </summary>
    public ExchangeRecoveryStrategy Strategy { get; set; }
    
    /// <summary>
    /// Number of exchanges recovered
    /// </summary>
    public int ExchangesRecovered { get; set; }
    
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
    public static ExchangeRecoveryResult CreateSuccess(ExchangeRecoveryStrategy strategy, int exchangesRecovered, 
        int bindingsRecovered, int recoveryAttempts, TimeSpan duration)
    {
        return new ExchangeRecoveryResult
        {
            Success = true,
            Strategy = strategy,
            ExchangesRecovered = exchangesRecovered,
            BindingsRecovered = bindingsRecovered,
            RecoveryAttempts = recoveryAttempts,
            Duration = duration
        };
    }
    
    /// <summary>
    /// Creates a failure result
    /// </summary>
    public static ExchangeRecoveryResult CreateFailure(ExchangeRecoveryStrategy strategy, int recoveryAttempts, 
        TimeSpan duration, string errorMessage, Exception? exception = null)
    {
        return new ExchangeRecoveryResult
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