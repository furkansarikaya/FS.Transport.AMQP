using FS.Mediator.Features.RequestHandling.Core;

namespace FS.Transport.AMQP.Saga;

/// <summary>
/// Query to get saga by ID
/// </summary>
public class GetSagaQuery : IRequest<GetSagaResult>
{
    /// <summary>
    /// Saga identifier
    /// </summary>
    public string SagaId { get; set; } = string.Empty;
    
    /// <summary>
    /// Whether to include step details
    /// </summary>
    public bool IncludeSteps { get; set; } = true;
    
    /// <summary>
    /// Whether to include statistics
    /// </summary>
    public bool IncludeStatistics { get; set; } = true;
    
    /// <summary>
    /// Creates a get saga query
    /// </summary>
    /// <param name="sagaId">Saga identifier</param>
    /// <param name="includeSteps">Include steps</param>
    /// <param name="includeStatistics">Include statistics</param>
    /// <returns>Get saga query</returns>
    public static GetSagaQuery Create(string sagaId, bool includeSteps = true, bool includeStatistics = true)
    {
        return new GetSagaQuery
        {
            SagaId = sagaId,
            IncludeSteps = includeSteps,
            IncludeStatistics = includeStatistics
        };
    }
}

/// <summary>
/// Result of get saga query
/// </summary>
public class GetSagaResult
{
    /// <summary>
    /// Whether the query was successful
    /// </summary>
    public bool Success { get; set; }
    
    /// <summary>
    /// Saga state context
    /// </summary>
    public SagaStateContext? StateContext { get; set; }
    
    /// <summary>
    /// Saga statistics
    /// </summary>
    public SagaStatistics? Statistics { get; set; }
    
    /// <summary>
    /// Error message if failed
    /// </summary>
    public string? ErrorMessage { get; set; }
    
    /// <summary>
    /// Exception details if failed
    /// </summary>
    public Exception? Exception { get; set; }
    
    /// <summary>
    /// Creates a successful result
    /// </summary>
    /// <param name="stateContext">State context</param>
    /// <param name="statistics">Statistics</param>
    /// <returns>Successful result</returns>
    public static GetSagaResult CreateSuccess(SagaStateContext stateContext, SagaStatistics? statistics = null)
    {
        return new GetSagaResult
        {
            Success = true,
            StateContext = stateContext,
            Statistics = statistics
        };
    }
    
    /// <summary>
    /// Creates a failed result
    /// </summary>
    /// <param name="errorMessage">Error message</param>
    /// <param name="exception">Exception</param>
    /// <returns>Failed result</returns>
    public static GetSagaResult CreateFailure(string errorMessage, Exception? exception = null)
    {
        return new GetSagaResult
        {
            Success = false,
            ErrorMessage = errorMessage,
            Exception = exception
        };
    }
}

/// <summary>
/// Query to get saga state
/// </summary>
public class GetSagaStateQuery : IRequest<GetSagaStateResult>
{
    /// <summary>
    /// Saga identifier
    /// </summary>
    public string SagaId { get; set; } = string.Empty;
    
    /// <summary>
    /// Version of the state to retrieve (optional)
    /// </summary>
    public long? Version { get; set; }
    
    /// <summary>
    /// Creates a get saga state query
    /// </summary>
    /// <param name="sagaId">Saga identifier</param>
    /// <param name="version">Version</param>
    /// <returns>Get saga state query</returns>
    public static GetSagaStateQuery Create(string sagaId, long? version = null)
    {
        return new GetSagaStateQuery
        {
            SagaId = sagaId,
            Version = version
        };
    }
}

/// <summary>
/// Result of get saga state query
/// </summary>
public class GetSagaStateResult
{
    /// <summary>
    /// Whether the query was successful
    /// </summary>
    public bool Success { get; set; }
    
    /// <summary>
    /// Saga state context
    /// </summary>
    public SagaStateContext? StateContext { get; set; }
    
    /// <summary>
    /// Error message if failed
    /// </summary>
    public string? ErrorMessage { get; set; }
    
    /// <summary>
    /// Exception details if failed
    /// </summary>
    public Exception? Exception { get; set; }
    
    /// <summary>
    /// Creates a successful result
    /// </summary>
    /// <param name="stateContext">State context</param>
    /// <returns>Successful result</returns>
    public static GetSagaStateResult CreateSuccess(SagaStateContext stateContext)
    {
        return new GetSagaStateResult
        {
            Success = true,
            StateContext = stateContext
        };
    }
    
    /// <summary>
    /// Creates a failed result
    /// </summary>
    /// <param name="errorMessage">Error message</param>
    /// <param name="exception">Exception</param>
    /// <returns>Failed result</returns>
    public static GetSagaStateResult CreateFailure(string errorMessage, Exception? exception = null)
    {
        return new GetSagaStateResult
        {
            Success = false,
            ErrorMessage = errorMessage,
            Exception = exception
        };
    }
}

/// <summary>
/// Query to get active sagas
/// </summary>
public class GetActiveSagasQuery : IRequest<GetActiveSagasResult>
{
    /// <summary>
    /// Saga type filter (optional)
    /// </summary>
    public string? SagaType { get; set; }
    
    /// <summary>
    /// Correlation ID filter (optional)
    /// </summary>
    public string? CorrelationId { get; set; }
    
    /// <summary>
    /// State filter (optional)
    /// </summary>
    public SagaState? State { get; set; }
    
    /// <summary>
    /// Page number for pagination
    /// </summary>
    public int Page { get; set; } = 1;
    
    /// <summary>
    /// Page size for pagination
    /// </summary>
    public int PageSize { get; set; } = 50;
    
    /// <summary>
    /// Whether to include step details
    /// </summary>
    public bool IncludeSteps { get; set; } = false;
    
    /// <summary>
    /// Creates a get active sagas query
    /// </summary>
    /// <param name="sagaType">Saga type filter</param>
    /// <param name="correlationId">Correlation ID filter</param>
    /// <param name="state">State filter</param>
    /// <param name="page">Page number</param>
    /// <param name="pageSize">Page size</param>
    /// <param name="includeSteps">Include steps</param>
    /// <returns>Get active sagas query</returns>
    public static GetActiveSagasQuery Create(string? sagaType = null, string? correlationId = null, SagaState? state = null, 
        int page = 1, int pageSize = 50, bool includeSteps = false)
    {
        return new GetActiveSagasQuery
        {
            SagaType = sagaType,
            CorrelationId = correlationId,
            State = state,
            Page = page,
            PageSize = pageSize,
            IncludeSteps = includeSteps
        };
    }
}

/// <summary>
/// Result of get active sagas query
/// </summary>
public class GetActiveSagasResult
{
    /// <summary>
    /// Whether the query was successful
    /// </summary>
    public bool Success { get; set; }
    
    /// <summary>
    /// List of saga state contexts
    /// </summary>
    public List<SagaStateContext> Sagas { get; set; } = new();
    
    /// <summary>
    /// Total number of sagas matching the criteria
    /// </summary>
    public int TotalCount { get; set; }
    
    /// <summary>
    /// Current page number
    /// </summary>
    public int Page { get; set; }
    
    /// <summary>
    /// Page size
    /// </summary>
    public int PageSize { get; set; }
    
    /// <summary>
    /// Total number of pages
    /// </summary>
    public int TotalPages => (int)Math.Ceiling((double)TotalCount / PageSize);
    
    /// <summary>
    /// Error message if failed
    /// </summary>
    public string? ErrorMessage { get; set; }
    
    /// <summary>
    /// Exception details if failed
    /// </summary>
    public Exception? Exception { get; set; }
    
    /// <summary>
    /// Creates a successful result
    /// </summary>
    /// <param name="sagas">List of sagas</param>
    /// <param name="totalCount">Total count</param>
    /// <param name="page">Page number</param>
    /// <param name="pageSize">Page size</param>
    /// <returns>Successful result</returns>
    public static GetActiveSagasResult CreateSuccess(List<SagaStateContext> sagas, int totalCount, int page, int pageSize)
    {
        return new GetActiveSagasResult
        {
            Success = true,
            Sagas = sagas,
            TotalCount = totalCount,
            Page = page,
            PageSize = pageSize
        };
    }
    
    /// <summary>
    /// Creates a failed result
    /// </summary>
    /// <param name="errorMessage">Error message</param>
    /// <param name="exception">Exception</param>
    /// <returns>Failed result</returns>
    public static GetActiveSagasResult CreateFailure(string errorMessage, Exception? exception = null)
    {
        return new GetActiveSagasResult
        {
            Success = false,
            ErrorMessage = errorMessage,
            Exception = exception
        };
    }
}

/// <summary>
/// Query to get saga statistics
/// </summary>
public class GetSagaStatisticsQuery : IRequest<GetSagaStatisticsResult>
{
    /// <summary>
    /// Saga identifier
    /// </summary>
    public string SagaId { get; set; } = string.Empty;
    
    /// <summary>
    /// Whether to include step-level statistics
    /// </summary>
    public bool IncludeStepStatistics { get; set; } = true;
    
    /// <summary>
    /// Creates a get saga statistics query
    /// </summary>
    /// <param name="sagaId">Saga identifier</param>
    /// <param name="includeStepStatistics">Include step statistics</param>
    /// <returns>Get saga statistics query</returns>
    public static GetSagaStatisticsQuery Create(string sagaId, bool includeStepStatistics = true)
    {
        return new GetSagaStatisticsQuery
        {
            SagaId = sagaId,
            IncludeStepStatistics = includeStepStatistics
        };
    }
}

/// <summary>
/// Result of get saga statistics query
/// </summary>
public class GetSagaStatisticsResult
{
    /// <summary>
    /// Whether the query was successful
    /// </summary>
    public bool Success { get; set; }
    
    /// <summary>
    /// Saga statistics
    /// </summary>
    public SagaStatistics? Statistics { get; set; }
    
    /// <summary>
    /// Step-level statistics
    /// </summary>
    public Dictionary<string, SagaStepStatistics> StepStatistics { get; set; } = new();
    
    /// <summary>
    /// Error message if failed
    /// </summary>
    public string? ErrorMessage { get; set; }
    
    /// <summary>
    /// Exception details if failed
    /// </summary>
    public Exception? Exception { get; set; }
    
    /// <summary>
    /// Creates a successful result
    /// </summary>
    /// <param name="statistics">Statistics</param>
    /// <param name="stepStatistics">Step statistics</param>
    /// <returns>Successful result</returns>
    public static GetSagaStatisticsResult CreateSuccess(SagaStatistics statistics, Dictionary<string, SagaStepStatistics>? stepStatistics = null)
    {
        return new GetSagaStatisticsResult
        {
            Success = true,
            Statistics = statistics,
            StepStatistics = stepStatistics ?? new Dictionary<string, SagaStepStatistics>()
        };
    }
    
    /// <summary>
    /// Creates a failed result
    /// </summary>
    /// <param name="errorMessage">Error message</param>
    /// <param name="exception">Exception</param>
    /// <returns>Failed result</returns>
    public static GetSagaStatisticsResult CreateFailure(string errorMessage, Exception? exception = null)
    {
        return new GetSagaStatisticsResult
        {
            Success = false,
            ErrorMessage = errorMessage,
            Exception = exception
        };
    }
}

/// <summary>
/// Query to check if saga exists
/// </summary>
public class SagaExistsQuery : IRequest<SagaExistsResult>
{
    /// <summary>
    /// Saga identifier
    /// </summary>
    public string SagaId { get; set; } = string.Empty;
    
    /// <summary>
    /// Creates a saga exists query
    /// </summary>
    /// <param name="sagaId">Saga identifier</param>
    /// <returns>Saga exists query</returns>
    public static SagaExistsQuery Create(string sagaId)
    {
        return new SagaExistsQuery
        {
            SagaId = sagaId
        };
    }
}

/// <summary>
/// Result of saga exists query
/// </summary>
public class SagaExistsResult
{
    /// <summary>
    /// Whether the query was successful
    /// </summary>
    public bool Success { get; set; }
    
    /// <summary>
    /// Whether the saga exists
    /// </summary>
    public bool Exists { get; set; }
    
    /// <summary>
    /// Saga identifier
    /// </summary>
    public string SagaId { get; set; } = string.Empty;
    
    /// <summary>
    /// Current state of the saga (if exists)
    /// </summary>
    public SagaState? State { get; set; }
    
    /// <summary>
    /// Error message if failed
    /// </summary>
    public string? ErrorMessage { get; set; }
    
    /// <summary>
    /// Exception details if failed
    /// </summary>
    public Exception? Exception { get; set; }
    
    /// <summary>
    /// Creates a successful result
    /// </summary>
    /// <param name="sagaId">Saga identifier</param>
    /// <param name="exists">Whether saga exists</param>
    /// <param name="state">Saga state</param>
    /// <returns>Successful result</returns>
    public static SagaExistsResult CreateSuccess(string sagaId, bool exists, SagaState? state = null)
    {
        return new SagaExistsResult
        {
            Success = true,
            SagaId = sagaId,
            Exists = exists,
            State = state
        };
    }
    
    /// <summary>
    /// Creates a failed result
    /// </summary>
    /// <param name="sagaId">Saga identifier</param>
    /// <param name="errorMessage">Error message</param>
    /// <param name="exception">Exception</param>
    /// <returns>Failed result</returns>
    public static SagaExistsResult CreateFailure(string sagaId, string errorMessage, Exception? exception = null)
    {
        return new SagaExistsResult
        {
            Success = false,
            SagaId = sagaId,
            ErrorMessage = errorMessage,
            Exception = exception
        };
    }
}

/// <summary>
/// Query to get saga steps
/// </summary>
public class GetSagaStepsQuery : IRequest<GetSagaStepsResult>
{
    /// <summary>
    /// Saga identifier
    /// </summary>
    public string SagaId { get; set; } = string.Empty;
    
    /// <summary>
    /// State filter for steps (optional)
    /// </summary>
    public SagaStepState? StateFilter { get; set; }
    
    /// <summary>
    /// Whether to include step metadata
    /// </summary>
    public bool IncludeMetadata { get; set; } = true;
    
    /// <summary>
    /// Creates a get saga steps query
    /// </summary>
    /// <param name="sagaId">Saga identifier</param>
    /// <param name="stateFilter">State filter</param>
    /// <param name="includeMetadata">Include metadata</param>
    /// <returns>Get saga steps query</returns>
    public static GetSagaStepsQuery Create(string sagaId, SagaStepState? stateFilter = null, bool includeMetadata = true)
    {
        return new GetSagaStepsQuery
        {
            SagaId = sagaId,
            StateFilter = stateFilter,
            IncludeMetadata = includeMetadata
        };
    }
}

/// <summary>
/// Result of get saga steps query
/// </summary>
public class GetSagaStepsResult
{
    /// <summary>
    /// Whether the query was successful
    /// </summary>
    public bool Success { get; set; }
    
    /// <summary>
    /// List of saga steps
    /// </summary>
    public List<SagaStepInfo> Steps { get; set; } = new();
    
    /// <summary>
    /// Saga identifier
    /// </summary>
    public string SagaId { get; set; } = string.Empty;
    
    /// <summary>
    /// Error message if failed
    /// </summary>
    public string? ErrorMessage { get; set; }
    
    /// <summary>
    /// Exception details if failed
    /// </summary>
    public Exception? Exception { get; set; }
    
    /// <summary>
    /// Creates a successful result
    /// </summary>
    /// <param name="sagaId">Saga identifier</param>
    /// <param name="steps">List of steps</param>
    /// <returns>Successful result</returns>
    public static GetSagaStepsResult CreateSuccess(string sagaId, List<SagaStepInfo> steps)
    {
        return new GetSagaStepsResult
        {
            Success = true,
            SagaId = sagaId,
            Steps = steps
        };
    }
    
    /// <summary>
    /// Creates a failed result
    /// </summary>
    /// <param name="sagaId">Saga identifier</param>
    /// <param name="errorMessage">Error message</param>
    /// <param name="exception">Exception</param>
    /// <returns>Failed result</returns>
    public static GetSagaStepsResult CreateFailure(string sagaId, string errorMessage, Exception? exception = null)
    {
        return new GetSagaStepsResult
        {
            Success = false,
            SagaId = sagaId,
            ErrorMessage = errorMessage,
            Exception = exception
        };
    }
}

/// <summary>
/// Statistics for a saga step
/// </summary>
public class SagaStepStatistics
{
    /// <summary>
    /// Step identifier
    /// </summary>
    public string StepId { get; set; } = string.Empty;
    
    /// <summary>
    /// Step name
    /// </summary>
    public string StepName { get; set; } = string.Empty;
    
    /// <summary>
    /// Step type
    /// </summary>
    public string StepType { get; set; } = string.Empty;
    
    /// <summary>
    /// Current state
    /// </summary>
    public SagaStepState State { get; set; }
    
    /// <summary>
    /// Execution order
    /// </summary>
    public int ExecutionOrder { get; set; }
    
    /// <summary>
    /// Number of retry attempts
    /// </summary>
    public int RetryAttempts { get; set; }
    
    /// <summary>
    /// Maximum retries allowed
    /// </summary>
    public int MaxRetries { get; set; }
    
    /// <summary>
    /// Execution duration
    /// </summary>
    public TimeSpan? Duration { get; set; }
    
    /// <summary>
    /// Whether the step is critical
    /// </summary>
    public bool IsCritical { get; set; }
    
    /// <summary>
    /// Whether the step can be compensated
    /// </summary>
    public bool CanCompensate { get; set; }
    
    /// <summary>
    /// When the step was started
    /// </summary>
    public DateTimeOffset? StartedAt { get; set; }
    
    /// <summary>
    /// When the step was completed
    /// </summary>
    public DateTimeOffset? CompletedAt { get; set; }
    
    /// <summary>
    /// Error message if failed
    /// </summary>
    public string? ErrorMessage { get; set; }
} 