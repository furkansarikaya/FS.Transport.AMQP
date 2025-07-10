using FS.Mediator.Features.RequestHandling.Core;

namespace FS.Transport.AMQP.Saga;

/// <summary>
/// Command to start a saga
/// </summary>
public class StartSagaCommand : IRequest<StartSagaResult>
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
    /// Input data for the saga
    /// </summary>
    public Dictionary<string, object> InputData { get; set; } = new();
    
    /// <summary>
    /// Correlation ID for tracing
    /// </summary>
    public string? CorrelationId { get; set; }
    
    /// <summary>
    /// Who initiated the saga
    /// </summary>
    public string? InitiatedBy { get; set; }
    
    /// <summary>
    /// Execution timeout
    /// </summary>
    public TimeSpan? Timeout { get; set; }
    
    /// <summary>
    /// Creates a start saga command
    /// </summary>
    /// <param name="sagaId">Saga identifier</param>
    /// <param name="sagaType">Saga type</param>
    /// <param name="inputData">Input data</param>
    /// <param name="correlationId">Correlation ID</param>
    /// <param name="initiatedBy">Initiator</param>
    /// <param name="timeout">Timeout</param>
    /// <returns>Start saga command</returns>
    public static StartSagaCommand Create(string sagaId, string sagaType, Dictionary<string, object> inputData, 
        string? correlationId = null, string? initiatedBy = null, TimeSpan? timeout = null)
    {
        return new StartSagaCommand
        {
            SagaId = sagaId,
            SagaType = sagaType,
            InputData = inputData,
            CorrelationId = correlationId,
            InitiatedBy = initiatedBy,
            Timeout = timeout
        };
    }
}

/// <summary>
/// Result of start saga command
/// </summary>
public class StartSagaResult
{
    /// <summary>
    /// Whether the command was successful
    /// </summary>
    public bool Success { get; set; }
    
    /// <summary>
    /// Saga identifier
    /// </summary>
    public string SagaId { get; set; } = string.Empty;
    
    /// <summary>
    /// Current saga state
    /// </summary>
    public SagaState State { get; set; }
    
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
    /// <param name="state">Saga state</param>
    /// <returns>Successful result</returns>
    public static StartSagaResult CreateSuccess(string sagaId, SagaState state)
    {
        return new StartSagaResult
        {
            Success = true,
            SagaId = sagaId,
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
    public static StartSagaResult CreateFailure(string sagaId, string errorMessage, Exception? exception = null)
    {
        return new StartSagaResult
        {
            Success = false,
            SagaId = sagaId,
            ErrorMessage = errorMessage,
            Exception = exception
        };
    }
}

/// <summary>
/// Command to execute a saga step
/// </summary>
public class ExecuteSagaStepCommand : IRequest<ExecuteSagaStepResult>
{
    /// <summary>
    /// Saga identifier
    /// </summary>
    public string SagaId { get; set; } = string.Empty;
    
    /// <summary>
    /// Step identifier
    /// </summary>
    public string StepId { get; set; } = string.Empty;
    
    /// <summary>
    /// Input data for the step
    /// </summary>
    public Dictionary<string, object> InputData { get; set; } = new();
    
    /// <summary>
    /// Whether to force execution even if step is already completed
    /// </summary>
    public bool ForceExecution { get; set; } = false;
    
    /// <summary>
    /// Creates an execute saga step command
    /// </summary>
    /// <param name="sagaId">Saga identifier</param>
    /// <param name="stepId">Step identifier</param>
    /// <param name="inputData">Input data</param>
    /// <param name="forceExecution">Force execution</param>
    /// <returns>Execute saga step command</returns>
    public static ExecuteSagaStepCommand Create(string sagaId, string stepId, Dictionary<string, object>? inputData = null, bool forceExecution = false)
    {
        return new ExecuteSagaStepCommand
        {
            SagaId = sagaId,
            StepId = stepId,
            InputData = inputData ?? new Dictionary<string, object>(),
            ForceExecution = forceExecution
        };
    }
}

/// <summary>
/// Result of execute saga step command
/// </summary>
public class ExecuteSagaStepResult
{
    /// <summary>
    /// Whether the command was successful
    /// </summary>
    public bool Success { get; set; }
    
    /// <summary>
    /// Saga identifier
    /// </summary>
    public string SagaId { get; set; } = string.Empty;
    
    /// <summary>
    /// Step identifier
    /// </summary>
    public string StepId { get; set; } = string.Empty;
    
    /// <summary>
    /// Step execution result
    /// </summary>
    public SagaStepResult? StepResult { get; set; }
    
    /// <summary>
    /// Output data from the step
    /// </summary>
    public Dictionary<string, object> OutputData { get; set; } = new();
    
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
    /// <param name="stepId">Step identifier</param>
    /// <param name="stepResult">Step result</param>
    /// <param name="outputData">Output data</param>
    /// <returns>Successful result</returns>
    public static ExecuteSagaStepResult CreateSuccess(string sagaId, string stepId, SagaStepResult stepResult, Dictionary<string, object>? outputData = null)
    {
        return new ExecuteSagaStepResult
        {
            Success = true,
            SagaId = sagaId,
            StepId = stepId,
            StepResult = stepResult,
            OutputData = outputData ?? new Dictionary<string, object>()
        };
    }
    
    /// <summary>
    /// Creates a failed result
    /// </summary>
    /// <param name="sagaId">Saga identifier</param>
    /// <param name="stepId">Step identifier</param>
    /// <param name="errorMessage">Error message</param>
    /// <param name="exception">Exception</param>
    /// <returns>Failed result</returns>
    public static ExecuteSagaStepResult CreateFailure(string sagaId, string stepId, string errorMessage, Exception? exception = null)
    {
        return new ExecuteSagaStepResult
        {
            Success = false,
            SagaId = sagaId,
            StepId = stepId,
            ErrorMessage = errorMessage,
            Exception = exception
        };
    }
}

/// <summary>
/// Command to compensate a saga
/// </summary>
public class CompensateSagaCommand : IRequest<CompensateSagaResult>
{
    /// <summary>
    /// Saga identifier
    /// </summary>
    public string SagaId { get; set; } = string.Empty;
    
    /// <summary>
    /// Reason for compensation
    /// </summary>
    public string Reason { get; set; } = string.Empty;
    
    /// <summary>
    /// Whether to force compensation even if saga cannot be compensated
    /// </summary>
    public bool ForceCompensation { get; set; } = false;
    
    /// <summary>
    /// Creates a compensate saga command
    /// </summary>
    /// <param name="sagaId">Saga identifier</param>
    /// <param name="reason">Reason for compensation</param>
    /// <param name="forceCompensation">Force compensation</param>
    /// <returns>Compensate saga command</returns>
    public static CompensateSagaCommand Create(string sagaId, string reason, bool forceCompensation = false)
    {
        return new CompensateSagaCommand
        {
            SagaId = sagaId,
            Reason = reason,
            ForceCompensation = forceCompensation
        };
    }
}

/// <summary>
/// Result of compensate saga command
/// </summary>
public class CompensateSagaResult
{
    /// <summary>
    /// Whether the command was successful
    /// </summary>
    public bool Success { get; set; }
    
    /// <summary>
    /// Saga identifier
    /// </summary>
    public string SagaId { get; set; } = string.Empty;
    
    /// <summary>
    /// Current saga state
    /// </summary>
    public SagaState State { get; set; }
    
    /// <summary>
    /// Number of steps compensated
    /// </summary>
    public int CompensatedSteps { get; set; }
    
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
    /// <param name="state">Saga state</param>
    /// <param name="compensatedSteps">Number of compensated steps</param>
    /// <returns>Successful result</returns>
    public static CompensateSagaResult CreateSuccess(string sagaId, SagaState state, int compensatedSteps)
    {
        return new CompensateSagaResult
        {
            Success = true,
            SagaId = sagaId,
            State = state,
            CompensatedSteps = compensatedSteps
        };
    }
    
    /// <summary>
    /// Creates a failed result
    /// </summary>
    /// <param name="sagaId">Saga identifier</param>
    /// <param name="errorMessage">Error message</param>
    /// <param name="exception">Exception</param>
    /// <returns>Failed result</returns>
    public static CompensateSagaResult CreateFailure(string sagaId, string errorMessage, Exception? exception = null)
    {
        return new CompensateSagaResult
        {
            Success = false,
            SagaId = sagaId,
            ErrorMessage = errorMessage,
            Exception = exception
        };
    }
}

/// <summary>
/// Command to cancel a saga
/// </summary>
public class CancelSagaCommand : IRequest<CancelSagaResult>
{
    /// <summary>
    /// Saga identifier
    /// </summary>
    public string SagaId { get; set; } = string.Empty;
    
    /// <summary>
    /// Reason for cancellation
    /// </summary>
    public string Reason { get; set; } = string.Empty;
    
    /// <summary>
    /// Whether to compensate before cancelling
    /// </summary>
    public bool CompensateBeforeCancel { get; set; } = true;
    
    /// <summary>
    /// Creates a cancel saga command
    /// </summary>
    /// <param name="sagaId">Saga identifier</param>
    /// <param name="reason">Reason for cancellation</param>
    /// <param name="compensateBeforeCancel">Compensate before cancel</param>
    /// <returns>Cancel saga command</returns>
    public static CancelSagaCommand Create(string sagaId, string reason, bool compensateBeforeCancel = true)
    {
        return new CancelSagaCommand
        {
            SagaId = sagaId,
            Reason = reason,
            CompensateBeforeCancel = compensateBeforeCancel
        };
    }
}

/// <summary>
/// Result of cancel saga command
/// </summary>
public class CancelSagaResult
{
    /// <summary>
    /// Whether the command was successful
    /// </summary>
    public bool Success { get; set; }
    
    /// <summary>
    /// Saga identifier
    /// </summary>
    public string SagaId { get; set; } = string.Empty;
    
    /// <summary>
    /// Current saga state
    /// </summary>
    public SagaState State { get; set; }
    
    /// <summary>
    /// Whether compensation was performed
    /// </summary>
    public bool CompensationPerformed { get; set; }
    
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
    /// <param name="state">Saga state</param>
    /// <param name="compensationPerformed">Whether compensation was performed</param>
    /// <returns>Successful result</returns>
    public static CancelSagaResult CreateSuccess(string sagaId, SagaState state, bool compensationPerformed)
    {
        return new CancelSagaResult
        {
            Success = true,
            SagaId = sagaId,
            State = state,
            CompensationPerformed = compensationPerformed
        };
    }
    
    /// <summary>
    /// Creates a failed result
    /// </summary>
    /// <param name="sagaId">Saga identifier</param>
    /// <param name="errorMessage">Error message</param>
    /// <param name="exception">Exception</param>
    /// <returns>Failed result</returns>
    public static CancelSagaResult CreateFailure(string sagaId, string errorMessage, Exception? exception = null)
    {
        return new CancelSagaResult
        {
            Success = false,
            SagaId = sagaId,
            ErrorMessage = errorMessage,
            Exception = exception
        };
    }
}

/// <summary>
/// Command to complete a saga
/// </summary>
public class CompleteSagaCommand : IRequest<CompleteSagaResult>
{
    /// <summary>
    /// Saga identifier
    /// </summary>
    public string SagaId { get; set; } = string.Empty;
    
    /// <summary>
    /// Output data from the saga
    /// </summary>
    public Dictionary<string, object> OutputData { get; set; } = new();
    
    /// <summary>
    /// Whether to force completion even if saga is not ready
    /// </summary>
    public bool ForceCompletion { get; set; } = false;
    
    /// <summary>
    /// Creates a complete saga command
    /// </summary>
    /// <param name="sagaId">Saga identifier</param>
    /// <param name="outputData">Output data</param>
    /// <param name="forceCompletion">Force completion</param>
    /// <returns>Complete saga command</returns>
    public static CompleteSagaCommand Create(string sagaId, Dictionary<string, object>? outputData = null, bool forceCompletion = false)
    {
        return new CompleteSagaCommand
        {
            SagaId = sagaId,
            OutputData = outputData ?? new Dictionary<string, object>(),
            ForceCompletion = forceCompletion
        };
    }
}

/// <summary>
/// Result of complete saga command
/// </summary>
public class CompleteSagaResult
{
    /// <summary>
    /// Whether the command was successful
    /// </summary>
    public bool Success { get; set; }
    
    /// <summary>
    /// Saga identifier
    /// </summary>
    public string SagaId { get; set; } = string.Empty;
    
    /// <summary>
    /// Current saga state
    /// </summary>
    public SagaState State { get; set; }
    
    /// <summary>
    /// Output data from the saga
    /// </summary>
    public Dictionary<string, object> OutputData { get; set; } = new();
    
    /// <summary>
    /// Total execution time
    /// </summary>
    public TimeSpan? ExecutionTime { get; set; }
    
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
    /// <param name="state">Saga state</param>
    /// <param name="outputData">Output data</param>
    /// <param name="executionTime">Execution time</param>
    /// <returns>Successful result</returns>
    public static CompleteSagaResult CreateSuccess(string sagaId, SagaState state, Dictionary<string, object>? outputData = null, TimeSpan? executionTime = null)
    {
        return new CompleteSagaResult
        {
            Success = true,
            SagaId = sagaId,
            State = state,
            OutputData = outputData ?? new Dictionary<string, object>(),
            ExecutionTime = executionTime
        };
    }
    
    /// <summary>
    /// Creates a failed result
    /// </summary>
    /// <param name="sagaId">Saga identifier</param>
    /// <param name="errorMessage">Error message</param>
    /// <param name="exception">Exception</param>
    /// <returns>Failed result</returns>
    public static CompleteSagaResult CreateFailure(string sagaId, string errorMessage, Exception? exception = null)
    {
        return new CompleteSagaResult
        {
            Success = false,
            SagaId = sagaId,
            ErrorMessage = errorMessage,
            Exception = exception
        };
    }
}

/// <summary>
/// Command to save saga state
/// </summary>
public class SaveSagaStateCommand : IRequest<SaveSagaStateResult>
{
    /// <summary>
    /// Saga identifier
    /// </summary>
    public string SagaId { get; set; } = string.Empty;
    
    /// <summary>
    /// Saga state context
    /// </summary>
    public SagaStateContext StateContext { get; set; } = null!;
    
    /// <summary>
    /// Whether to create a state snapshot
    /// </summary>
    public bool CreateSnapshot { get; set; } = true;
    
    /// <summary>
    /// Creates a save saga state command
    /// </summary>
    /// <param name="sagaId">Saga identifier</param>
    /// <param name="stateContext">State context</param>
    /// <param name="createSnapshot">Create snapshot</param>
    /// <returns>Save saga state command</returns>
    public static SaveSagaStateCommand Create(string sagaId, SagaStateContext stateContext, bool createSnapshot = true)
    {
        return new SaveSagaStateCommand
        {
            SagaId = sagaId,
            StateContext = stateContext,
            CreateSnapshot = createSnapshot
        };
    }
}

/// <summary>
/// Result of save saga state command
/// </summary>
public class SaveSagaStateResult
{
    /// <summary>
    /// Whether the command was successful
    /// </summary>
    public bool Success { get; set; }
    
    /// <summary>
    /// Saga identifier
    /// </summary>
    public string SagaId { get; set; } = string.Empty;
    
    /// <summary>
    /// New version of the saga state
    /// </summary>
    public long Version { get; set; }
    
    /// <summary>
    /// Whether a snapshot was created
    /// </summary>
    public bool SnapshotCreated { get; set; }
    
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
    /// <param name="version">Version</param>
    /// <param name="snapshotCreated">Snapshot created</param>
    /// <returns>Successful result</returns>
    public static SaveSagaStateResult CreateSuccess(string sagaId, long version, bool snapshotCreated)
    {
        return new SaveSagaStateResult
        {
            Success = true,
            SagaId = sagaId,
            Version = version,
            SnapshotCreated = snapshotCreated
        };
    }
    
    /// <summary>
    /// Creates a failed result
    /// </summary>
    /// <param name="sagaId">Saga identifier</param>
    /// <param name="errorMessage">Error message</param>
    /// <param name="exception">Exception</param>
    /// <returns>Failed result</returns>
    public static SaveSagaStateResult CreateFailure(string sagaId, string errorMessage, Exception? exception = null)
    {
        return new SaveSagaStateResult
        {
            Success = false,
            SagaId = sagaId,
            ErrorMessage = errorMessage,
            Exception = exception
        };
    }
} 