namespace FS.Transport.AMQP.Saga;

/// <summary>
/// Extension methods for saga orchestration
/// </summary>
public static class SagaExtensions
{
    /// <summary>
    /// Adds a step to the saga with builder pattern
    /// </summary>
    /// <param name="saga">Saga instance</param>
    /// <param name="stepId">Step identifier</param>
    /// <param name="stepName">Step name</param>
    /// <param name="stepType">Step type</param>
    /// <param name="executionOrder">Execution order</param>
    /// <returns>Saga instance for chaining</returns>
    public static ISaga WithStep(this ISaga saga, string stepId, string stepName, string stepType, int executionOrder)
    {
        var stepInfo = SagaStepInfo.Create(stepId, stepName, stepType, executionOrder);
        saga.AddStep(stepInfo);
        return saga;
    }
    
    /// <summary>
    /// Adds a step to the saga with configuration
    /// </summary>
    /// <param name="saga">Saga instance</param>
    /// <param name="stepId">Step identifier</param>
    /// <param name="stepName">Step name</param>
    /// <param name="stepType">Step type</param>
    /// <param name="executionOrder">Execution order</param>
    /// <param name="configure">Step configuration action</param>
    /// <returns>Saga instance for chaining</returns>
    public static ISaga WithStep(this ISaga saga, string stepId, string stepName, string stepType, int executionOrder, Action<SagaStepInfo> configure)
    {
        var stepInfo = SagaStepInfo.Create(stepId, stepName, stepType, executionOrder);
        configure(stepInfo);
        saga.AddStep(stepInfo);
        return saga;
    }
    
    /// <summary>
    /// Adds a critical step to the saga
    /// </summary>
    /// <param name="saga">Saga instance</param>
    /// <param name="stepId">Step identifier</param>
    /// <param name="stepName">Step name</param>
    /// <param name="stepType">Step type</param>
    /// <param name="executionOrder">Execution order</param>
    /// <returns>Saga instance for chaining</returns>
    public static ISaga WithCriticalStep(this ISaga saga, string stepId, string stepName, string stepType, int executionOrder)
    {
        return saga.WithStep(stepId, stepName, stepType, executionOrder, step =>
        {
            step.IsCritical = true;
            step.CanCompensate = true;
        });
    }
    
    /// <summary>
    /// Adds a non-compensable step to the saga
    /// </summary>
    /// <param name="saga">Saga instance</param>
    /// <param name="stepId">Step identifier</param>
    /// <param name="stepName">Step name</param>
    /// <param name="stepType">Step type</param>
    /// <param name="executionOrder">Execution order</param>
    /// <returns>Saga instance for chaining</returns>
    public static ISaga WithNonCompensableStep(this ISaga saga, string stepId, string stepName, string stepType, int executionOrder)
    {
        return saga.WithStep(stepId, stepName, stepType, executionOrder, step =>
        {
            step.CanCompensate = false;
        });
    }
    
    /// <summary>
    /// Adds input data to the saga
    /// </summary>
    /// <param name="saga">Saga instance</param>
    /// <param name="key">Data key</param>
    /// <param name="value">Data value</param>
    /// <returns>Saga instance for chaining</returns>
    public static ISaga WithInputData(this ISaga saga, string key, object value)
    {
        saga.StateContext.InputData[key] = value;
        return saga;
    }
    
    /// <summary>
    /// Adds multiple input data to the saga
    /// </summary>
    /// <param name="saga">Saga instance</param>
    /// <param name="data">Data dictionary</param>
    /// <returns>Saga instance for chaining</returns>
    public static ISaga WithInputData(this ISaga saga, Dictionary<string, object> data)
    {
        foreach (var kvp in data)
        {
            saga.StateContext.InputData[kvp.Key] = kvp.Value;
        }
        return saga;
    }
    
    /// <summary>
    /// Sets correlation ID for the saga
    /// </summary>
    /// <param name="saga">Saga instance</param>
    /// <param name="correlationId">Correlation ID</param>
    /// <returns>Saga instance for chaining</returns>
    public static ISaga WithCorrelationId(this ISaga saga, string correlationId)
    {
        saga.StateContext.CorrelationId = correlationId;
        return saga;
    }
    
    /// <summary>
    /// Sets who initiated the saga
    /// </summary>
    /// <param name="saga">Saga instance</param>
    /// <param name="initiatedBy">Initiator identifier</param>
    /// <returns>Saga instance for chaining</returns>
    public static ISaga WithInitiatedBy(this ISaga saga, string initiatedBy)
    {
        saga.StateContext.InitiatedBy = initiatedBy;
        return saga;
    }
    
    /// <summary>
    /// Gets completed steps from the saga
    /// </summary>
    /// <param name="saga">Saga instance</param>
    /// <returns>Collection of completed steps</returns>
    public static IEnumerable<SagaStepInfo> GetCompletedSteps(this ISaga saga)
    {
        return saga.GetAllSteps().Where(s => s.State == SagaStepState.Completed);
    }
    
    /// <summary>
    /// Gets failed steps from the saga
    /// </summary>
    /// <param name="saga">Saga instance</param>
    /// <returns>Collection of failed steps</returns>
    public static IEnumerable<SagaStepInfo> GetFailedSteps(this ISaga saga)
    {
        return saga.GetAllSteps().Where(s => s.State == SagaStepState.Failed);
    }
    
    /// <summary>
    /// Gets pending steps from the saga
    /// </summary>
    /// <param name="saga">Saga instance</param>
    /// <returns>Collection of pending steps</returns>
    public static IEnumerable<SagaStepInfo> GetPendingSteps(this ISaga saga)
    {
        return saga.GetAllSteps().Where(s => s.State == SagaStepState.Pending);
    }
    
    /// <summary>
    /// Gets compensated steps from the saga
    /// </summary>
    /// <param name="saga">Saga instance</param>
    /// <returns>Collection of compensated steps</returns>
    public static IEnumerable<SagaStepInfo> GetCompensatedSteps(this ISaga saga)
    {
        return saga.GetAllSteps().Where(s => s.State == SagaStepState.Compensated);
    }
    
    /// <summary>
    /// Checks if the saga is completed
    /// </summary>
    /// <param name="saga">Saga instance</param>
    /// <returns>True if saga is completed</returns>
    public static bool IsCompleted(this ISaga saga)
    {
        return saga.State == SagaState.Completed;
    }
    
    /// <summary>
    /// Checks if the saga has failed
    /// </summary>
    /// <param name="saga">Saga instance</param>
    /// <returns>True if saga has failed</returns>
    public static bool HasFailed(this ISaga saga)
    {
        return saga.State == SagaState.Faulted || saga.State == SagaState.CompensationFailed;
    }
    
    /// <summary>
    /// Checks if the saga is in progress
    /// </summary>
    /// <param name="saga">Saga instance</param>
    /// <returns>True if saga is in progress</returns>
    public static bool IsInProgress(this ISaga saga)
    {
        return saga.State == SagaState.InProgress;
    }
    
    /// <summary>
    /// Checks if the saga is compensating
    /// </summary>
    /// <param name="saga">Saga instance</param>
    /// <returns>True if saga is compensating</returns>
    public static bool IsCompensating(this ISaga saga)
    {
        return saga.State == SagaState.Compensating;
    }
    
    /// <summary>
    /// Checks if the saga is compensated
    /// </summary>
    /// <param name="saga">Saga instance</param>
    /// <returns>True if saga is compensated</returns>
    public static bool IsCompensated(this ISaga saga)
    {
        return saga.State == SagaState.Compensated;
    }
    
    /// <summary>
    /// Gets the total execution time of the saga
    /// </summary>
    /// <param name="saga">Saga instance</param>
    /// <returns>Total execution time</returns>
    public static TimeSpan? GetTotalExecutionTime(this ISaga saga)
    {
        return saga.StateContext.Duration;
    }
    
    /// <summary>
    /// Gets the progress percentage of the saga
    /// </summary>
    /// <param name="saga">Saga instance</param>
    /// <returns>Progress percentage (0-100)</returns>
    public static double GetProgressPercentage(this ISaga saga)
    {
        return saga.StateContext.CompletionPercentage;
    }
    
    /// <summary>
    /// Gets the next step to execute
    /// </summary>
    /// <param name="saga">Saga instance</param>
    /// <returns>Next step or null if none</returns>
    public static SagaStepInfo? GetNextStep(this ISaga saga)
    {
        return saga.StateContext.GetNextStep();
    }
    
    /// <summary>
    /// Gets steps that need compensation
    /// </summary>
    /// <param name="saga">Saga instance</param>
    /// <returns>Collection of steps needing compensation</returns>
    public static IEnumerable<SagaStepInfo> GetStepsToCompensate(this ISaga saga)
    {
        return saga.StateContext.GetStepsToCompensate();
    }
    
    /// <summary>
    /// Gets statistics about the saga execution
    /// </summary>
    /// <param name="saga">Saga instance</param>
    /// <returns>Saga statistics</returns>
    public static SagaStatistics GetStatistics(this ISaga saga)
    {
        var steps = saga.GetAllSteps().ToList();
        
        return new SagaStatistics
        {
            SagaId = saga.SagaId,
            SagaType = saga.SagaType,
            State = saga.State,
            TotalSteps = steps.Count,
            CompletedSteps = steps.Count(s => s.State == SagaStepState.Completed),
            FailedSteps = steps.Count(s => s.State == SagaStepState.Failed),
            PendingSteps = steps.Count(s => s.State == SagaStepState.Pending),
            CompensatedSteps = steps.Count(s => s.State == SagaStepState.Compensated),
            TotalRetries = steps.Sum(s => s.RetryAttempts),
            ExecutionTime = saga.StateContext.Duration,
            CompletionPercentage = saga.StateContext.CompletionPercentage,
            CreatedAt = saga.StateContext.CreatedAt,
            StartedAt = saga.StateContext.StartedAt,
            CompletedAt = saga.StateContext.CompletedAt,
            LastError = saga.StateContext.LastError,
            CompensationAttempts = saga.StateContext.CompensationAttempts
        };
    }
    
    /// <summary>
    /// Executes the saga asynchronously with error handling
    /// </summary>
    /// <param name="saga">Saga instance</param>
    /// <param name="inputData">Input data</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Task representing the saga execution</returns>
    public static async Task ExecuteAsync(this ISaga saga, Dictionary<string, object> inputData, CancellationToken cancellationToken = default)
    {
        await saga.StartAsync(inputData, cancellationToken).ConfigureAwait(false);
        
        while (saga.IsInProgress())
        {
            var hasMoreSteps = await saga.ExecuteNextStepAsync(cancellationToken).ConfigureAwait(false);
            if (!hasMoreSteps)
                break;
        }
        
        if (saga.IsInProgress())
        {
            await saga.CompleteAsync(cancellationToken: cancellationToken).ConfigureAwait(false);
        }
    }
    
    /// <summary>
    /// Executes the saga asynchronously with timeout
    /// </summary>
    /// <param name="saga">Saga instance</param>
    /// <param name="inputData">Input data</param>
    /// <param name="timeout">Execution timeout</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Task representing the saga execution</returns>
    public static async Task ExecuteAsync(this ISaga saga, Dictionary<string, object> inputData, TimeSpan timeout, CancellationToken cancellationToken = default)
    {
        using var cts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
        cts.CancelAfter(timeout);
        
        try
        {
            await saga.ExecuteAsync(inputData, cts.Token).ConfigureAwait(false);
        }
        catch (OperationCanceledException) when (cts.Token.IsCancellationRequested && !cancellationToken.IsCancellationRequested)
        {
            await saga.CancelAsync("Execution timeout", cancellationToken).ConfigureAwait(false);
            throw new TimeoutException($"Saga {saga.SagaId} execution timed out after {timeout}");
        }
    }
}

/// <summary>
/// Statistics about saga execution
/// </summary>
public class SagaStatistics
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
    /// Current state of the saga
    /// </summary>
    public SagaState State { get; set; }
    
    /// <summary>
    /// Total number of steps
    /// </summary>
    public int TotalSteps { get; set; }
    
    /// <summary>
    /// Number of completed steps
    /// </summary>
    public int CompletedSteps { get; set; }
    
    /// <summary>
    /// Number of failed steps
    /// </summary>
    public int FailedSteps { get; set; }
    
    /// <summary>
    /// Number of pending steps
    /// </summary>
    public int PendingSteps { get; set; }
    
    /// <summary>
    /// Number of compensated steps
    /// </summary>
    public int CompensatedSteps { get; set; }
    
    /// <summary>
    /// Total number of retries across all steps
    /// </summary>
    public int TotalRetries { get; set; }
    
    /// <summary>
    /// Total execution time
    /// </summary>
    public TimeSpan? ExecutionTime { get; set; }
    
    /// <summary>
    /// Completion percentage
    /// </summary>
    public double CompletionPercentage { get; set; }
    
    /// <summary>
    /// When the saga was created
    /// </summary>
    public DateTimeOffset CreatedAt { get; set; }
    
    /// <summary>
    /// When the saga was started
    /// </summary>
    public DateTimeOffset? StartedAt { get; set; }
    
    /// <summary>
    /// When the saga was completed
    /// </summary>
    public DateTimeOffset? CompletedAt { get; set; }
    
    /// <summary>
    /// Last error message
    /// </summary>
    public string? LastError { get; set; }
    
    /// <summary>
    /// Number of compensation attempts
    /// </summary>
    public int CompensationAttempts { get; set; }
}