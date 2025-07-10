namespace FS.Transport.AMQP.Saga;

/// <summary>
/// Interface for saga implementation providing workflow orchestration with compensation
/// </summary>
public interface ISaga : IDisposable
{
    /// <summary>
    /// Unique identifier for the saga
    /// </summary>
    string SagaId { get; }
    
    /// <summary>
    /// Type of the saga
    /// </summary>
    string SagaType { get; }
    
    /// <summary>
    /// Current state of the saga
    /// </summary>
    SagaState State { get; }
    
    /// <summary>
    /// Current version of the saga (for optimistic concurrency)
    /// </summary>
    long Version { get; }
    
    /// <summary>
    /// Correlation ID for tracing
    /// </summary>
    string? CorrelationId { get; }
    
    /// <summary>
    /// Gets the complete saga state context
    /// </summary>
    SagaStateContext StateContext { get; }
    
    /// <summary>
    /// Event triggered when saga state changes
    /// </summary>
    event EventHandler<SagaStateChangedEventArgs>? StateChanged;
    
    /// <summary>
    /// Event triggered when a step is completed
    /// </summary>
    event EventHandler<SagaStepCompletedEventArgs>? StepCompleted;
    
    /// <summary>
    /// Event triggered when a step fails
    /// </summary>
    event EventHandler<SagaStepFailedEventArgs>? StepFailed;
    
    /// <summary>
    /// Event triggered when compensation is triggered
    /// </summary>
    event EventHandler<SagaCompensationEventArgs>? CompensationTriggered;
    
    /// <summary>
    /// Starts the saga execution
    /// </summary>
    /// <param name="inputData">Input data for the saga</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Task representing the start operation</returns>
    Task StartAsync(Dictionary<string, object> inputData, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Executes the next step in the saga
    /// </summary>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>True if there are more steps to execute</returns>
    Task<bool> ExecuteNextStepAsync(CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Executes a specific step by ID
    /// </summary>
    /// <param name="stepId">Step identifier</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Result of step execution</returns>
    Task<SagaStepResult> ExecuteStepAsync(string stepId, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Compensates the saga by undoing completed steps
    /// </summary>
    /// <param name="reason">Reason for compensation</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Task representing the compensation operation</returns>
    Task CompensateAsync(string reason, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Compensates a specific step by ID
    /// </summary>
    /// <param name="stepId">Step identifier</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Result of step compensation</returns>
    Task<SagaCompensationResult> CompensateStepAsync(string stepId, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Cancels the saga execution
    /// </summary>
    /// <param name="reason">Reason for cancellation</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Task representing the cancellation operation</returns>
    Task CancelAsync(string reason, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Completes the saga
    /// </summary>
    /// <param name="outputData">Output data from the saga</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Task representing the completion operation</returns>
    Task CompleteAsync(Dictionary<string, object>? outputData = null, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Adds a step to the saga workflow
    /// </summary>
    /// <param name="stepInfo">Step information</param>
    void AddStep(SagaStepInfo stepInfo);
    
    /// <summary>
    /// Gets a step by ID
    /// </summary>
    /// <param name="stepId">Step identifier</param>
    /// <returns>Step information or null if not found</returns>
    SagaStepInfo? GetStep(string stepId);
    
    /// <summary>
    /// Gets all steps in the saga
    /// </summary>
    /// <returns>Collection of step information</returns>
    IEnumerable<SagaStepInfo> GetAllSteps();
    
    /// <summary>
    /// Checks if the saga can be compensated
    /// </summary>
    /// <returns>True if saga can be compensated</returns>
    bool CanCompensate();
    
    /// <summary>
    /// Saves the current saga state
    /// </summary>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Task representing the save operation</returns>
    Task SaveStateAsync(CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Loads saga state from storage
    /// </summary>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Task representing the load operation</returns>
    Task LoadStateAsync(CancellationToken cancellationToken = default);
}

/// <summary>
/// Event arguments for saga state changes
/// </summary>
public class SagaStateChangedEventArgs : EventArgs
{
    /// <summary>
    /// Saga identifier
    /// </summary>
    public string SagaId { get; set; } = string.Empty;
    
    /// <summary>
    /// Previous state
    /// </summary>
    public SagaState PreviousState { get; set; }
    
    /// <summary>
    /// New state
    /// </summary>
    public SagaState NewState { get; set; }
    
    /// <summary>
    /// Timestamp of the state change
    /// </summary>
    public DateTimeOffset Timestamp { get; set; } = DateTimeOffset.UtcNow;
    
    /// <summary>
    /// Additional context data
    /// </summary>
    public Dictionary<string, object> Context { get; set; } = new();
}

/// <summary>
/// Event arguments for saga step completion
/// </summary>
public class SagaStepCompletedEventArgs : EventArgs
{
    /// <summary>
    /// Saga identifier
    /// </summary>
    public string SagaId { get; set; } = string.Empty;
    
    /// <summary>
    /// Step that was completed
    /// </summary>
    public SagaStepInfo StepInfo { get; set; } = null!;
    
    /// <summary>
    /// Output data from the step
    /// </summary>
    public Dictionary<string, object> OutputData { get; set; } = new();
    
    /// <summary>
    /// Timestamp of completion
    /// </summary>
    public DateTimeOffset Timestamp { get; set; } = DateTimeOffset.UtcNow;
}

/// <summary>
/// Event arguments for saga step failure
/// </summary>
public class SagaStepFailedEventArgs : EventArgs
{
    /// <summary>
    /// Saga identifier
    /// </summary>
    public string SagaId { get; set; } = string.Empty;
    
    /// <summary>
    /// Step that failed
    /// </summary>
    public SagaStepInfo StepInfo { get; set; } = null!;
    
    /// <summary>
    /// Exception that caused the failure
    /// </summary>
    public Exception Exception { get; set; } = null!;
    
    /// <summary>
    /// Whether compensation will be triggered
    /// </summary>
    public bool WillCompensate { get; set; }
    
    /// <summary>
    /// Timestamp of failure
    /// </summary>
    public DateTimeOffset Timestamp { get; set; } = DateTimeOffset.UtcNow;
}

/// <summary>
/// Event arguments for saga compensation
/// </summary>
public class SagaCompensationEventArgs : EventArgs
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
    /// Steps that will be compensated
    /// </summary>
    public List<SagaStepInfo> StepsToCompensate { get; set; } = new();
    
    /// <summary>
    /// Timestamp when compensation started
    /// </summary>
    public DateTimeOffset Timestamp { get; set; } = DateTimeOffset.UtcNow;
}

/// <summary>
/// Result of saga step execution
/// </summary>
public class SagaStepResult
{
    /// <summary>
    /// Whether the step execution was successful
    /// </summary>
    public bool Success { get; set; }
    
    /// <summary>
    /// Output data from the step
    /// </summary>
    public Dictionary<string, object> OutputData { get; set; } = new();
    
    /// <summary>
    /// Error message if step failed
    /// </summary>
    public string? ErrorMessage { get; set; }
    
    /// <summary>
    /// Exception details if step failed
    /// </summary>
    public Exception? Exception { get; set; }
    
    /// <summary>
    /// Whether the step should be retried
    /// </summary>
    public bool ShouldRetry { get; set; }
    
    /// <summary>
    /// Creates a successful step result
    /// </summary>
    /// <param name="outputData">Output data</param>
    /// <returns>Successful step result</returns>
    public static SagaStepResult CreateSuccess(Dictionary<string, object>? outputData = null)
    {
        return new SagaStepResult
        {
            Success = true,
            OutputData = outputData ?? new Dictionary<string, object>()
        };
    }
    
    /// <summary>
    /// Creates a failed step result
    /// </summary>
    /// <param name="errorMessage">Error message</param>
    /// <param name="exception">Exception</param>
    /// <param name="shouldRetry">Whether to retry</param>
    /// <returns>Failed step result</returns>
    public static SagaStepResult CreateFailure(string errorMessage, Exception? exception = null, bool shouldRetry = false)
    {
        return new SagaStepResult
        {
            Success = false,
            ErrorMessage = errorMessage,
            Exception = exception,
            ShouldRetry = shouldRetry
        };
    }
}

/// <summary>
/// Result of saga step compensation
/// </summary>
public class SagaCompensationResult
{
    /// <summary>
    /// Whether the compensation was successful
    /// </summary>
    public bool Success { get; set; }
    
    /// <summary>
    /// Error message if compensation failed
    /// </summary>
    public string? ErrorMessage { get; set; }
    
    /// <summary>
    /// Exception details if compensation failed
    /// </summary>
    public Exception? Exception { get; set; }
    
    /// <summary>
    /// Creates a successful compensation result
    /// </summary>
    /// <returns>Successful compensation result</returns>
    public static SagaCompensationResult CreateSuccess()
    {
        return new SagaCompensationResult { Success = true };
    }
    
    /// <summary>
    /// Creates a failed compensation result
    /// </summary>
    /// <param name="errorMessage">Error message</param>
    /// <param name="exception">Exception</param>
    /// <returns>Failed compensation result</returns>
    public static SagaCompensationResult CreateFailure(string errorMessage, Exception? exception = null)
    {
        return new SagaCompensationResult
        {
            Success = false,
            ErrorMessage = errorMessage,
            Exception = exception
        };
    }
}