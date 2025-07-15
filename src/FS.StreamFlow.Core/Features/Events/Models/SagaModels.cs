namespace FS.StreamFlow.Core.Features.Events.Models;

/// <summary>
/// Represents the state of a saga
/// </summary>
public enum SagaState
{
    /// <summary>
    /// Saga is not started
    /// </summary>
    NotStarted,
    
    /// <summary>
    /// Saga is running
    /// </summary>
    Running,
    
    /// <summary>
    /// Saga is waiting for external events
    /// </summary>
    Waiting,
    
    /// <summary>
    /// Saga is being compensated
    /// </summary>
    Compensating,
    
    /// <summary>
    /// Saga has completed successfully
    /// </summary>
    Completed,
    
    /// <summary>
    /// Saga has been aborted
    /// </summary>
    Aborted,
    
    /// <summary>
    /// Saga has failed
    /// </summary>
    Failed,
    
    /// <summary>
    /// Saga has timed out
    /// </summary>
    TimedOut
}

/// <summary>
/// Represents saga context containing state and execution information
/// </summary>
public class SagaContext
{
    /// <summary>
    /// Saga identifier
    /// </summary>
    public string SagaId { get; set; } = string.Empty;
    
    /// <summary>
    /// Saga type
    /// </summary>
    public string SagaType { get; set; } = string.Empty;
    
    /// <summary>
    /// Current saga state
    /// </summary>
    public SagaState State { get; set; } = SagaState.NotStarted;
    
    /// <summary>
    /// Current saga version
    /// </summary>
    public long Version { get; set; } = 0;
    
    /// <summary>
    /// Correlation ID for tracking
    /// </summary>
    public string? CorrelationId { get; set; }
    
    /// <summary>
    /// Saga data
    /// </summary>
    public Dictionary<string, object> Data { get; set; } = new();
    
    /// <summary>
    /// Saga metadata
    /// </summary>
    public Dictionary<string, object> Metadata { get; set; } = new();
    
    /// <summary>
    /// Saga creation timestamp
    /// </summary>
    public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;
    
    /// <summary>
    /// Saga last update timestamp
    /// </summary>
    public DateTimeOffset UpdatedAt { get; set; } = DateTimeOffset.UtcNow;
    
    /// <summary>
    /// Saga timeout
    /// </summary>
    public TimeSpan? Timeout { get; set; }
    
    /// <summary>
    /// Saga expiration time
    /// </summary>
    public DateTimeOffset? ExpiresAt { get; set; }
    
    /// <summary>
    /// Completed saga steps
    /// </summary>
    public List<SagaStepInfo> CompletedSteps { get; set; } = new();
    
    /// <summary>
    /// Failed saga steps
    /// </summary>
    public List<SagaStepInfo> FailedSteps { get; set; } = new();
    
    /// <summary>
    /// Current step being executed
    /// </summary>
    public SagaStepInfo? CurrentStep { get; set; }
    
    /// <summary>
    /// Saga result data
    /// </summary>
    public Dictionary<string, object>? Result { get; set; }
    
    /// <summary>
    /// Saga error information
    /// </summary>
    public SagaErrorInfo? Error { get; set; }
}

/// <summary>
/// Represents information about a saga step
/// </summary>
public class SagaStepInfo
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
    /// Step execution status
    /// </summary>
    public SagaStepStatus Status { get; set; } = SagaStepStatus.NotStarted;
    
    /// <summary>
    /// Step input data
    /// </summary>
    public Dictionary<string, object>? InputData { get; set; }
    
    /// <summary>
    /// Step output data
    /// </summary>
    public Dictionary<string, object>? OutputData { get; set; }
    
    /// <summary>
    /// Step execution start time
    /// </summary>
    public DateTimeOffset? StartedAt { get; set; }
    
    /// <summary>
    /// Step execution end time
    /// </summary>
    public DateTimeOffset? CompletedAt { get; set; }
    
    /// <summary>
    /// Step execution duration
    /// </summary>
    public TimeSpan? Duration => CompletedAt - StartedAt;
    
    /// <summary>
    /// Step retry count
    /// </summary>
    public int RetryCount { get; set; } = 0;
    
    /// <summary>
    /// Step error information
    /// </summary>
    public SagaErrorInfo? Error { get; set; }
    
    /// <summary>
    /// Whether the step can be compensated
    /// </summary>
    public bool CanCompensate { get; set; } = true;
    
    /// <summary>
    /// Step compensation information
    /// </summary>
    public SagaCompensationInfo? Compensation { get; set; }
}

/// <summary>
/// Represents the status of a saga step
/// </summary>
public enum SagaStepStatus
{
    /// <summary>
    /// Step is not started
    /// </summary>
    NotStarted,
    
    /// <summary>
    /// Step is running
    /// </summary>
    Running,
    
    /// <summary>
    /// Step has completed successfully
    /// </summary>
    Completed,
    
    /// <summary>
    /// Step has failed
    /// </summary>
    Failed,
    
    /// <summary>
    /// Step is being compensated
    /// </summary>
    Compensating,
    
    /// <summary>
    /// Step has been compensated
    /// </summary>
    Compensated,
    
    /// <summary>
    /// Step compensation failed
    /// </summary>
    CompensationFailed,
    
    /// <summary>
    /// Step was skipped
    /// </summary>
    Skipped
}

/// <summary>
/// Represents saga error information
/// </summary>
public class SagaErrorInfo
{
    /// <summary>
    /// Error message
    /// </summary>
    public string Message { get; set; } = string.Empty;
    
    /// <summary>
    /// Error type
    /// </summary>
    public string ErrorType { get; set; } = string.Empty;
    
    /// <summary>
    /// Error stack trace
    /// </summary>
    public string? StackTrace { get; set; }
    
    /// <summary>
    /// Error occurrence timestamp
    /// </summary>
    public DateTimeOffset OccurredAt { get; set; } = DateTimeOffset.UtcNow;
    
    /// <summary>
    /// Error context data
    /// </summary>
    public Dictionary<string, object>? Context { get; set; }
    
    /// <summary>
    /// Whether the error is recoverable
    /// </summary>
    public bool IsRecoverable { get; set; } = true;
    
    /// <summary>
    /// Recovery attempts made
    /// </summary>
    public int RecoveryAttempts { get; set; } = 0;
}

/// <summary>
/// Represents saga compensation information
/// </summary>
public class SagaCompensationInfo
{
    /// <summary>
    /// Compensation action identifier
    /// </summary>
    public string CompensationId { get; set; } = string.Empty;
    
    /// <summary>
    /// Compensation action name
    /// </summary>
    public string CompensationName { get; set; } = string.Empty;
    
    /// <summary>
    /// Compensation execution status
    /// </summary>
    public SagaStepStatus Status { get; set; } = SagaStepStatus.NotStarted;
    
    /// <summary>
    /// Compensation input data
    /// </summary>
    public Dictionary<string, object>? InputData { get; set; }
    
    /// <summary>
    /// Compensation output data
    /// </summary>
    public Dictionary<string, object>? OutputData { get; set; }
    
    /// <summary>
    /// Compensation execution start time
    /// </summary>
    public DateTimeOffset? StartedAt { get; set; }
    
    /// <summary>
    /// Compensation execution end time
    /// </summary>
    public DateTimeOffset? CompletedAt { get; set; }
    
    /// <summary>
    /// Compensation execution duration
    /// </summary>
    public TimeSpan? Duration => CompletedAt - StartedAt;
    
    /// <summary>
    /// Compensation retry count
    /// </summary>
    public int RetryCount { get; set; } = 0;
    
    /// <summary>
    /// Compensation error information
    /// </summary>
    public SagaErrorInfo? Error { get; set; }
}

/// <summary>
/// Represents saga settings
/// </summary>
public class SagaSettings
{
    /// <summary>
    /// Default saga timeout
    /// </summary>
    public TimeSpan DefaultTimeout { get; set; } = TimeSpan.FromMinutes(30);
    
    /// <summary>
    /// Maximum retry attempts per step
    /// </summary>
    public int MaxRetryAttempts { get; set; } = 3;
    
    /// <summary>
    /// Retry delay between attempts
    /// </summary>
    public TimeSpan RetryDelay { get; set; } = TimeSpan.FromSeconds(1);
    
    /// <summary>
    /// Whether to use exponential backoff for retries
    /// </summary>
    public bool UseExponentialBackoff { get; set; } = true;
    
    /// <summary>
    /// Whether to enable saga persistence
    /// </summary>
    public bool EnablePersistence { get; set; } = true;
    
    /// <summary>
    /// Saga persistence interval
    /// </summary>
    public TimeSpan PersistenceInterval { get; set; } = TimeSpan.FromSeconds(30);
    
    /// <summary>
    /// Whether to enable saga recovery
    /// </summary>
    public bool EnableRecovery { get; set; } = true;
    
    /// <summary>
    /// Recovery check interval
    /// </summary>
    public TimeSpan RecoveryInterval { get; set; } = TimeSpan.FromMinutes(5);
    
    /// <summary>
    /// Whether to enable saga timeout handling
    /// </summary>
    public bool EnableTimeoutHandling { get; set; } = true;
    
    /// <summary>
    /// Timeout check interval
    /// </summary>
    public TimeSpan TimeoutCheckInterval { get; set; } = TimeSpan.FromMinutes(1);
    
    /// <summary>
    /// Maximum number of concurrent sagas
    /// </summary>
    public int MaxConcurrentSagas { get; set; } = 1000;
    
    /// <summary>
    /// Whether to enable saga metrics
    /// </summary>
    public bool EnableMetrics { get; set; } = true;
    
    /// <summary>
    /// Additional saga configuration
    /// </summary>
    public Dictionary<string, object>? AdditionalSettings { get; set; }
}

/// <summary>
/// Event arguments for saga state changes
/// </summary>
public class SagaStateChangedEventArgs : EventArgs
{
    /// <summary>
    /// Saga identifier
    /// </summary>
    public string SagaId { get; }
    
    /// <summary>
    /// Previous saga state
    /// </summary>
    public SagaState PreviousState { get; }
    
    /// <summary>
    /// Current saga state
    /// </summary>
    public SagaState CurrentState { get; }
    
    /// <summary>
    /// Saga context
    /// </summary>
    public SagaContext Context { get; }
    
    /// <summary>
    /// Timestamp when the state changed
    /// </summary>
    public DateTimeOffset Timestamp { get; }
    
    /// <summary>
    /// Initializes a new instance of the SagaStateChangedEventArgs class
    /// </summary>
    /// <param name="sagaId">Saga identifier</param>
    /// <param name="previousState">Previous saga state</param>
    /// <param name="currentState">Current saga state</param>
    /// <param name="context">Saga context</param>
    public SagaStateChangedEventArgs(string sagaId, SagaState previousState, SagaState currentState, SagaContext context)
    {
        SagaId = sagaId;
        PreviousState = previousState;
        CurrentState = currentState;
        Context = context;
        Timestamp = DateTimeOffset.UtcNow;
    }
}

/// <summary>
/// Event arguments for saga step completion
/// </summary>
public class SagaStepCompletedEventArgs : EventArgs
{
    /// <summary>
    /// Saga identifier
    /// </summary>
    public string SagaId { get; }
    
    /// <summary>
    /// Step information
    /// </summary>
    public SagaStepInfo StepInfo { get; }
    
    /// <summary>
    /// Saga context
    /// </summary>
    public SagaContext Context { get; }
    
    /// <summary>
    /// Timestamp when the step completed
    /// </summary>
    public DateTimeOffset Timestamp { get; }
    
    /// <summary>
    /// Initializes a new instance of the SagaStepCompletedEventArgs class
    /// </summary>
    /// <param name="sagaId">Saga identifier</param>
    /// <param name="stepInfo">Step information</param>
    /// <param name="context">Saga context</param>
    public SagaStepCompletedEventArgs(string sagaId, SagaStepInfo stepInfo, SagaContext context)
    {
        SagaId = sagaId;
        StepInfo = stepInfo;
        Context = context;
        Timestamp = DateTimeOffset.UtcNow;
    }
}

/// <summary>
/// Event arguments for saga step failures
/// </summary>
public class SagaStepFailedEventArgs : EventArgs
{
    /// <summary>
    /// Saga identifier
    /// </summary>
    public string SagaId { get; }
    
    /// <summary>
    /// Step information
    /// </summary>
    public SagaStepInfo StepInfo { get; }
    
    /// <summary>
    /// Saga context
    /// </summary>
    public SagaContext Context { get; }
    
    /// <summary>
    /// Error that caused the failure
    /// </summary>
    public Exception Exception { get; }
    
    /// <summary>
    /// Timestamp when the step failed
    /// </summary>
    public DateTimeOffset Timestamp { get; }
    
    /// <summary>
    /// Initializes a new instance of the SagaStepFailedEventArgs class
    /// </summary>
    /// <param name="sagaId">Saga identifier</param>
    /// <param name="stepInfo">Step information</param>
    /// <param name="context">Saga context</param>
    /// <param name="exception">Error that caused the failure</param>
    public SagaStepFailedEventArgs(string sagaId, SagaStepInfo stepInfo, SagaContext context, Exception exception)
    {
        SagaId = sagaId;
        StepInfo = stepInfo;
        Context = context;
        Exception = exception;
        Timestamp = DateTimeOffset.UtcNow;
    }
}

/// <summary>
/// Event arguments for saga compensation
/// </summary>
public class SagaCompensationEventArgs : EventArgs
{
    /// <summary>
    /// Saga identifier
    /// </summary>
    public string SagaId { get; }
    
    /// <summary>
    /// Compensation information
    /// </summary>
    public SagaCompensationInfo CompensationInfo { get; }
    
    /// <summary>
    /// Saga context
    /// </summary>
    public SagaContext Context { get; }
    
    /// <summary>
    /// Reason for compensation
    /// </summary>
    public string Reason { get; }
    
    /// <summary>
    /// Timestamp when the compensation was triggered
    /// </summary>
    public DateTimeOffset Timestamp { get; }
    
    /// <summary>
    /// Initializes a new instance of the SagaCompensationEventArgs class
    /// </summary>
    /// <param name="sagaId">Saga identifier</param>
    /// <param name="compensationInfo">Compensation information</param>
    /// <param name="context">Saga context</param>
    /// <param name="reason">Reason for compensation</param>
    public SagaCompensationEventArgs(string sagaId, SagaCompensationInfo compensationInfo, SagaContext context, string reason)
    {
        SagaId = sagaId;
        CompensationInfo = compensationInfo;
        Context = context;
        Reason = reason;
        Timestamp = DateTimeOffset.UtcNow;
    }
} 