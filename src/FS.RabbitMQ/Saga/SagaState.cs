namespace FS.RabbitMQ.Saga;

/// <summary>
/// Saga execution state enumeration
/// </summary>
public enum SagaState
{
    /// <summary>
    /// Saga has not been started yet
    /// </summary>
    NotStarted,
    
    /// <summary>
    /// Saga is currently executing steps
    /// </summary>
    InProgress,
    
    /// <summary>
    /// Saga completed successfully
    /// </summary>
    Completed,
    
    /// <summary>
    /// Saga failed and is being compensated
    /// </summary>
    Compensating,
    
    /// <summary>
    /// Saga compensation completed successfully
    /// </summary>
    Compensated,
    
    /// <summary>
    /// Saga failed during compensation
    /// </summary>
    CompensationFailed,
    
    /// <summary>
    /// Saga was cancelled before completion
    /// </summary>
    Cancelled,
    
    /// <summary>
    /// Saga is in an unknown/error state
    /// </summary>
    Faulted
}

/// <summary>
/// Saga step state enumeration
/// </summary>
public enum SagaStepState
{
    /// <summary>
    /// Step has not been executed yet
    /// </summary>
    Pending,
    
    /// <summary>
    /// Step is currently executing
    /// </summary>
    Executing,
    
    /// <summary>
    /// Step completed successfully
    /// </summary>
    Completed,
    
    /// <summary>
    /// Step failed during execution
    /// </summary>
    Failed,
    
    /// <summary>
    /// Step is being compensated
    /// </summary>
    Compensating,
    
    /// <summary>
    /// Step compensation completed successfully
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
/// Represents the state and data of a saga step
/// </summary>
public class SagaStepInfo
{
    /// <summary>
    /// Unique identifier for the step
    /// </summary>
    public string StepId { get; set; } = string.Empty;
    
    /// <summary>
    /// Name of the step
    /// </summary>
    public string StepName { get; set; } = string.Empty;
    
    /// <summary>
    /// Type of the step
    /// </summary>
    public string StepType { get; set; } = string.Empty;
    
    /// <summary>
    /// Current state of the step
    /// </summary>
    public SagaStepState State { get; set; } = SagaStepState.Pending;
    
    /// <summary>
    /// Order of execution for this step
    /// </summary>
    public int ExecutionOrder { get; set; }
    
    /// <summary>
    /// Input data for the step
    /// </summary>
    public Dictionary<string, object> InputData { get; set; } = new();
    
    /// <summary>
    /// Output data from the step
    /// </summary>
    public Dictionary<string, object> OutputData { get; set; } = new();
    
    /// <summary>
    /// Error information if step failed
    /// </summary>
    public string? ErrorMessage { get; set; }
    
    /// <summary>
    /// Exception details if step failed
    /// </summary>
    public string? ExceptionDetails { get; set; }
    
    /// <summary>
    /// Number of retry attempts for this step
    /// </summary>
    public int RetryAttempts { get; set; }
    
    /// <summary>
    /// Maximum number of retries allowed
    /// </summary>
    public int MaxRetries { get; set; } = 3;
    
    /// <summary>
    /// Whether this step can be compensated
    /// </summary>
    public bool CanCompensate { get; set; } = true;
    
    /// <summary>
    /// Whether this step is critical (saga fails if this step fails)
    /// </summary>
    public bool IsCritical { get; set; } = true;
    
    /// <summary>
    /// Timestamp when step was started
    /// </summary>
    public DateTimeOffset? StartedAt { get; set; }
    
    /// <summary>
    /// Timestamp when step was completed
    /// </summary>
    public DateTimeOffset? CompletedAt { get; set; }
    
    /// <summary>
    /// Duration of step execution
    /// </summary>
    public TimeSpan? Duration => CompletedAt - StartedAt;
    
    /// <summary>
    /// Additional step metadata
    /// </summary>
    public Dictionary<string, object> Metadata { get; set; } = new();
    
    /// <summary>
    /// Creates a new saga step info
    /// </summary>
    /// <param name="stepId">Step identifier</param>
    /// <param name="stepName">Step name</param>
    /// <param name="stepType">Step type</param>
    /// <param name="executionOrder">Execution order</param>
    /// <returns>SagaStepInfo instance</returns>
    public static SagaStepInfo Create(string stepId, string stepName, string stepType, int executionOrder)
    {
        return new SagaStepInfo
        {
            StepId = stepId,
            StepName = stepName,
            StepType = stepType,
            ExecutionOrder = executionOrder
        };
    }
    
    /// <summary>
    /// Marks step as started
    /// </summary>
    public void MarkAsStarted()
    {
        State = SagaStepState.Executing;
        StartedAt = DateTimeOffset.UtcNow;
    }
    
    /// <summary>
    /// Marks step as completed
    /// </summary>
    /// <param name="outputData">Output data from step</param>
    public void MarkAsCompleted(Dictionary<string, object>? outputData = null)
    {
        State = SagaStepState.Completed;
        CompletedAt = DateTimeOffset.UtcNow;
        
        if (outputData != null)
        {
            foreach (var kvp in outputData)
            {
                OutputData[kvp.Key] = kvp.Value;
            }
        }
    }
    
    /// <summary>
    /// Marks step as failed
    /// </summary>
    /// <param name="errorMessage">Error message</param>
    /// <param name="exceptionDetails">Exception details</param>
    public void MarkAsFailed(string errorMessage, string? exceptionDetails = null)
    {
        State = SagaStepState.Failed;
        CompletedAt = DateTimeOffset.UtcNow;
        ErrorMessage = errorMessage;
        ExceptionDetails = exceptionDetails;
    }
}

/// <summary>
/// Represents the complete state and context of a saga
/// </summary>
public class SagaStateContext
{
    /// <summary>
    /// Unique identifier for the saga
    /// </summary>
    public string SagaId { get; set; } = string.Empty;
    
    /// <summary>
    /// Type of the saga
    /// </summary>
    public string SagaType { get; set; } = string.Empty;
    
    /// <summary>
    /// Current state of the saga
    /// </summary>
    public SagaState State { get; set; } = SagaState.NotStarted;
    
    /// <summary>
    /// Version of the saga state (for optimistic concurrency)
    /// </summary>
    public long Version { get; set; }
    
    /// <summary>
    /// Input data for the entire saga
    /// </summary>
    public Dictionary<string, object> InputData { get; set; } = new();
    
    /// <summary>
    /// Output data from the saga
    /// </summary>
    public Dictionary<string, object> OutputData { get; set; } = new();
    
    /// <summary>
    /// All steps in the saga workflow
    /// </summary>
    public List<SagaStepInfo> Steps { get; set; } = new();
    
    /// <summary>
    /// Currently executing step
    /// </summary>
    public SagaStepInfo? CurrentStep { get; set; }
    
    /// <summary>
    /// Index of the current step
    /// </summary>
    public int CurrentStepIndex { get; set; } = -1;
    
    /// <summary>
    /// Correlation ID for tracing
    /// </summary>
    public string? CorrelationId { get; set; }
    
    /// <summary>
    /// User or system that initiated this saga
    /// </summary>
    public string? InitiatedBy { get; set; }
    
    /// <summary>
    /// Timestamp when saga was created
    /// </summary>
    public DateTimeOffset CreatedAt { get; set; }
    
    /// <summary>
    /// Timestamp when saga was started
    /// </summary>
    public DateTimeOffset? StartedAt { get; set; }
    
    /// <summary>
    /// Timestamp when saga was completed
    /// </summary>
    public DateTimeOffset? CompletedAt { get; set; }
    
    /// <summary>
    /// Total duration of saga execution
    /// </summary>
    public TimeSpan? Duration => CompletedAt - StartedAt;
    
    /// <summary>
    /// Last error that occurred in the saga
    /// </summary>
    public string? LastError { get; set; }
    
    /// <summary>
    /// Number of compensation attempts
    /// </summary>
    public int CompensationAttempts { get; set; }
    
    /// <summary>
    /// Additional saga metadata
    /// </summary>
    public Dictionary<string, object> Metadata { get; set; } = new();
    
    /// <summary>
    /// Gets the percentage of completion (0-100)
    /// </summary>
    public double CompletionPercentage
    {
        get
        {
            if (!Steps.Any()) return 0;
            
            var completedSteps = Steps.Count(s => s.State == SagaStepState.Completed);
            return (double)completedSteps / Steps.Count * 100;
        }
    }
    
    /// <summary>
    /// Gets whether the saga can be compensated
    /// </summary>
    public bool CanCompensate => Steps.Any(s => s.State == SagaStepState.Completed && s.CanCompensate);
    
    /// <summary>
    /// Gets the next step to execute
    /// </summary>
    /// <returns>Next step or null if all steps completed</returns>
    public SagaStepInfo? GetNextStep()
    {
        return Steps
            .Where(s => s.State == SagaStepState.Pending)
            .OrderBy(s => s.ExecutionOrder)
            .FirstOrDefault();
    }
    
    /// <summary>
    /// Gets steps that need compensation (in reverse order)
    /// </summary>
    /// <returns>Steps to compensate</returns>
    public IEnumerable<SagaStepInfo> GetStepsToCompensate()
    {
        return Steps
            .Where(s => s.State == SagaStepState.Completed && s.CanCompensate)
            .OrderByDescending(s => s.ExecutionOrder);
    }
    
    /// <summary>
    /// Adds a step to the saga
    /// </summary>
    /// <param name="stepInfo">Step information</param>
    public void AddStep(SagaStepInfo stepInfo)
    {
        Steps.Add(stepInfo);
        Steps.Sort((a, b) => a.ExecutionOrder.CompareTo(b.ExecutionOrder));
    }
    
    /// <summary>
    /// Marks saga as started
    /// </summary>
    public void MarkAsStarted()
    {
        State = SagaState.InProgress;
        StartedAt = DateTimeOffset.UtcNow;
        Version++;
    }
    
    /// <summary>
    /// Marks saga as completed
    /// </summary>
    public void MarkAsCompleted()
    {
        State = SagaState.Completed;
        CompletedAt = DateTimeOffset.UtcNow;
        Version++;
    }
    
    /// <summary>
    /// Marks saga as compensating
    /// </summary>
    /// <param name="error">Error that triggered compensation</param>
    public void MarkAsCompensating(string error)
    {
        State = SagaState.Compensating;
        LastError = error;
        CompensationAttempts++;
        Version++;
    }
    
    /// <summary>
    /// Marks saga as compensated
    /// </summary>
    public void MarkAsCompensated()
    {
        State = SagaState.Compensated;
        CompletedAt = DateTimeOffset.UtcNow;
        Version++;
    }
}