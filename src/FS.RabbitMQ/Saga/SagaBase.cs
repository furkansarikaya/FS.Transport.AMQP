namespace FS.RabbitMQ.Saga;

/// <summary>
/// Abstract base class for saga implementations providing common functionality
/// </summary>
public abstract class SagaBase : ISaga
{
    private readonly object _stateLock = new();
    private bool _disposed = false;
    
    /// <summary>
    /// Unique identifier for the saga
    /// </summary>
    public string SagaId { get; private set; }
    
    /// <summary>
    /// Type of the saga
    /// </summary>
    public string SagaType { get; private set; }
    
    /// <summary>
    /// Current state of the saga
    /// </summary>
    public SagaState State
    {
        get
        {
            lock (_stateLock)
            {
                return StateContext.State;
            }
        }
    }
    
    /// <summary>
    /// Current version of the saga (for optimistic concurrency)
    /// </summary>
    public long Version
    {
        get
        {
            lock (_stateLock)
            {
                return StateContext.Version;
            }
        }
    }
    
    /// <summary>
    /// Correlation ID for tracing
    /// </summary>
    public string? CorrelationId
    {
        get
        {
            lock (_stateLock)
            {
                return StateContext.CorrelationId;
            }
        }
    }
    
    /// <summary>
    /// Gets the complete saga state context
    /// </summary>
    public SagaStateContext StateContext { get; private set; }
    
    /// <summary>
    /// Event triggered when saga state changes
    /// </summary>
    public event EventHandler<SagaStateChangedEventArgs>? StateChanged;
    
    /// <summary>
    /// Event triggered when a step is completed
    /// </summary>
    public event EventHandler<SagaStepCompletedEventArgs>? StepCompleted;
    
    /// <summary>
    /// Event triggered when a step fails
    /// </summary>
    public event EventHandler<SagaStepFailedEventArgs>? StepFailed;
    
    /// <summary>
    /// Event triggered when compensation is triggered
    /// </summary>
    public event EventHandler<SagaCompensationEventArgs>? CompensationTriggered;
    
    /// <summary>
    /// Creates a new saga base
    /// </summary>
    /// <param name="sagaId">Saga identifier</param>
    /// <param name="sagaType">Saga type</param>
    /// <param name="correlationId">Correlation ID</param>
    protected SagaBase(string sagaId, string sagaType, string? correlationId = null)
    {
        if (string.IsNullOrEmpty(sagaId))
            throw new ArgumentException("Saga ID cannot be null or empty", nameof(sagaId));
        if (string.IsNullOrEmpty(sagaType))
            throw new ArgumentException("Saga type cannot be null or empty", nameof(sagaType));
        
        SagaId = sagaId;
        SagaType = sagaType;
        
        StateContext = new SagaStateContext
        {
            SagaId = sagaId,
            SagaType = sagaType,
            CorrelationId = correlationId,
            CreatedAt = DateTimeOffset.UtcNow
        };
        
        // Initialize steps for the saga
        InitializeSteps();
    }
    
    /// <summary>
    /// Initializes the steps for this saga - must be implemented by derived classes
    /// </summary>
    protected abstract void InitializeSteps();
    
    /// <summary>
    /// Executes a specific step - must be implemented by derived classes
    /// </summary>
    /// <param name="stepInfo">Step to execute</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Result of step execution</returns>
    protected abstract Task<SagaStepResult> ExecuteStepInternalAsync(SagaStepInfo stepInfo, CancellationToken cancellationToken);
    
    /// <summary>
    /// Compensates a specific step - must be implemented by derived classes
    /// </summary>
    /// <param name="stepInfo">Step to compensate</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Result of step compensation</returns>
    protected abstract Task<SagaCompensationResult> CompensateStepInternalAsync(SagaStepInfo stepInfo, CancellationToken cancellationToken);
    
    /// <summary>
    /// Saves saga state - can be overridden by derived classes
    /// </summary>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Task representing the save operation</returns>
    public virtual Task SaveStateAsync(CancellationToken cancellationToken = default)
    {
        // Default implementation - derived classes should override for persistence
        return Task.CompletedTask;
    }
    
    /// <summary>
    /// Loads saga state - can be overridden by derived classes
    /// </summary>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Task representing the load operation</returns>
    public virtual Task LoadStateAsync(CancellationToken cancellationToken = default)
    {
        // Default implementation - derived classes should override for persistence
        return Task.CompletedTask;
    }
    
    /// <summary>
    /// Starts the saga execution
    /// </summary>
    /// <param name="inputData">Input data for the saga</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Task representing the start operation</returns>
    public virtual async Task StartAsync(Dictionary<string, object> inputData, CancellationToken cancellationToken = default)
    {
        if (_disposed)
            throw new ObjectDisposedException(nameof(SagaBase));
        
        lock (_stateLock)
        {
            if (StateContext.State != SagaState.NotStarted)
                throw new InvalidOperationException($"Saga {SagaId} is already started (State: {StateContext.State})");
            
            StateContext.InputData = inputData ?? new Dictionary<string, object>();
            ChangeState(SagaState.InProgress);
            StateContext.MarkAsStarted();
        }
        
        await SaveStateAsync(cancellationToken).ConfigureAwait(false);
    }
    
    /// <summary>
    /// Executes the next step in the saga
    /// </summary>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>True if there are more steps to execute</returns>
    public virtual async Task<bool> ExecuteNextStepAsync(CancellationToken cancellationToken = default)
    {
        if (_disposed)
            throw new ObjectDisposedException(nameof(SagaBase));
        
        SagaStepInfo? nextStep;
        
        lock (_stateLock)
        {
            if (StateContext.State != SagaState.InProgress)
                return false;
            
            nextStep = StateContext.GetNextStep();
            if (nextStep == null)
            {
                // No more steps - complete the saga
                ChangeState(SagaState.Completed);
                StateContext.MarkAsCompleted();
                return false;
            }
            
            StateContext.CurrentStep = nextStep;
            StateContext.CurrentStepIndex = StateContext.Steps.IndexOf(nextStep);
        }
        
        try
        {
            var result = await ExecuteStepAsync(nextStep.StepId, cancellationToken).ConfigureAwait(false);
            return result.Success && StateContext.GetNextStep() != null;
        }
        catch (Exception ex)
        {
            await HandleStepFailureAsync(nextStep, ex, cancellationToken).ConfigureAwait(false);
            return false;
        }
    }
    
    /// <summary>
    /// Executes a specific step by ID
    /// </summary>
    /// <param name="stepId">Step identifier</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Result of step execution</returns>
    public virtual async Task<SagaStepResult> ExecuteStepAsync(string stepId, CancellationToken cancellationToken = default)
    {
        if (_disposed)
            throw new ObjectDisposedException(nameof(SagaBase));
        
        var stepInfo = GetStep(stepId);
        if (stepInfo == null)
            return SagaStepResult.CreateFailure($"Step {stepId} not found");
        
        lock (_stateLock)
        {
            stepInfo.MarkAsStarted();
        }
        
        try
        {
            var result = await ExecuteStepInternalAsync(stepInfo, cancellationToken).ConfigureAwait(false);
            
            lock (_stateLock)
            {
                if (result.Success)
                {
                    stepInfo.MarkAsCompleted(result.OutputData);
                    OnStepCompleted(stepInfo, result.OutputData);
                }
                else
                {
                    stepInfo.MarkAsFailed(result.ErrorMessage ?? "Step execution failed", result.Exception?.ToString());
                    OnStepFailed(stepInfo, result.Exception ?? new Exception(result.ErrorMessage ?? "Unknown error"));
                }
            }
            
            await SaveStateAsync(cancellationToken).ConfigureAwait(false);
            return result;
        }
        catch (Exception ex)
        {
            lock (_stateLock)
            {
                stepInfo.MarkAsFailed(ex.Message, ex.ToString());
            }
            
            OnStepFailed(stepInfo, ex);
            await SaveStateAsync(cancellationToken).ConfigureAwait(false);
            
            return SagaStepResult.CreateFailure(ex.Message, ex);
        }
    }
    
    /// <summary>
    /// Compensates the saga by undoing completed steps
    /// </summary>
    /// <param name="reason">Reason for compensation</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Task representing the compensation operation</returns>
    public virtual async Task CompensateAsync(string reason, CancellationToken cancellationToken = default)
    {
        if (_disposed)
            throw new ObjectDisposedException(nameof(SagaBase));
        
        List<SagaStepInfo> stepsToCompensate;
        
        lock (_stateLock)
        {
            if (!CanCompensate())
                return;
            
            ChangeState(SagaState.Compensating);
            StateContext.MarkAsCompensating(reason);
            
            stepsToCompensate = StateContext.GetStepsToCompensate().ToList();
        }
        
        OnCompensationTriggered(reason, stepsToCompensate);
        
        foreach (var step in stepsToCompensate)
        {
            try
            {
                await CompensateStepAsync(step.StepId, cancellationToken).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                lock (_stateLock)
                {
                    ChangeState(SagaState.CompensationFailed);
                    StateContext.LastError = $"Compensation failed for step {step.StepId}: {ex.Message}";
                }
                
                await SaveStateAsync(cancellationToken).ConfigureAwait(false);
                throw;
            }
        }
        
        lock (_stateLock)
        {
            ChangeState(SagaState.Compensated);
            StateContext.MarkAsCompensated();
        }
        
        await SaveStateAsync(cancellationToken).ConfigureAwait(false);
    }
    
    /// <summary>
    /// Compensates a specific step by ID
    /// </summary>
    /// <param name="stepId">Step identifier</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Result of step compensation</returns>
    public virtual async Task<SagaCompensationResult> CompensateStepAsync(string stepId, CancellationToken cancellationToken = default)
    {
        if (_disposed)
            throw new ObjectDisposedException(nameof(SagaBase));
        
        var stepInfo = GetStep(stepId);
        if (stepInfo == null)
            return SagaCompensationResult.CreateFailure($"Step {stepId} not found");
        
        if (stepInfo.State != SagaStepState.Completed || !stepInfo.CanCompensate)
            return SagaCompensationResult.CreateFailure($"Step {stepId} cannot be compensated");
        
        lock (_stateLock)
        {
            stepInfo.State = SagaStepState.Compensating;
        }
        
        try
        {
            var result = await CompensateStepInternalAsync(stepInfo, cancellationToken).ConfigureAwait(false);
            
            lock (_stateLock)
            {
                if (result.Success)
                {
                    stepInfo.State = SagaStepState.Compensated;
                }
                else
                {
                    stepInfo.State = SagaStepState.CompensationFailed;
                    stepInfo.ErrorMessage = result.ErrorMessage;
                    stepInfo.ExceptionDetails = result.Exception?.ToString();
                }
            }
            
            await SaveStateAsync(cancellationToken).ConfigureAwait(false);
            return result;
        }
        catch (Exception ex)
        {
            lock (_stateLock)
            {
                stepInfo.State = SagaStepState.CompensationFailed;
                stepInfo.ErrorMessage = ex.Message;
                stepInfo.ExceptionDetails = ex.ToString();
            }
            
            await SaveStateAsync(cancellationToken).ConfigureAwait(false);
            return SagaCompensationResult.CreateFailure(ex.Message, ex);
        }
    }
    
    /// <summary>
    /// Cancels the saga execution
    /// </summary>
    /// <param name="reason">Reason for cancellation</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Task representing the cancellation operation</returns>
    public virtual async Task CancelAsync(string reason, CancellationToken cancellationToken = default)
    {
        if (_disposed)
            throw new ObjectDisposedException(nameof(SagaBase));
        
        lock (_stateLock)
        {
            if (StateContext.State == SagaState.Completed || StateContext.State == SagaState.Cancelled)
                return;
            
            ChangeState(SagaState.Cancelled);
            StateContext.LastError = reason;
            StateContext.CompletedAt = DateTimeOffset.UtcNow;
            StateContext.Version++;
        }
        
        await SaveStateAsync(cancellationToken).ConfigureAwait(false);
    }
    
    /// <summary>
    /// Completes the saga
    /// </summary>
    /// <param name="outputData">Output data from the saga</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Task representing the completion operation</returns>
    public virtual async Task CompleteAsync(Dictionary<string, object>? outputData = null, CancellationToken cancellationToken = default)
    {
        if (_disposed)
            throw new ObjectDisposedException(nameof(SagaBase));
        
        lock (_stateLock)
        {
            if (StateContext.State != SagaState.InProgress)
                throw new InvalidOperationException($"Cannot complete saga in state {StateContext.State}");
            
            ChangeState(SagaState.Completed);
            StateContext.MarkAsCompleted();
            
            if (outputData != null)
            {
                foreach (var kvp in outputData)
                {
                    StateContext.OutputData[kvp.Key] = kvp.Value;
                }
            }
        }
        
        await SaveStateAsync(cancellationToken).ConfigureAwait(false);
    }
    
    /// <summary>
    /// Adds a step to the saga workflow
    /// </summary>
    /// <param name="stepInfo">Step information</param>
    public virtual void AddStep(SagaStepInfo stepInfo)
    {
        if (stepInfo == null)
            throw new ArgumentNullException(nameof(stepInfo));
        
        lock (_stateLock)
        {
            StateContext.AddStep(stepInfo);
        }
    }
    
    /// <summary>
    /// Gets a step by ID
    /// </summary>
    /// <param name="stepId">Step identifier</param>
    /// <returns>Step information or null if not found</returns>
    public virtual SagaStepInfo? GetStep(string stepId)
    {
        lock (_stateLock)
        {
            return StateContext.Steps.FirstOrDefault(s => s.StepId == stepId);
        }
    }
    
    /// <summary>
    /// Gets all steps in the saga
    /// </summary>
    /// <returns>Collection of step information</returns>
    public virtual IEnumerable<SagaStepInfo> GetAllSteps()
    {
        lock (_stateLock)
        {
            return StateContext.Steps.ToList();
        }
    }
    
    /// <summary>
    /// Checks if the saga can be compensated
    /// </summary>
    /// <returns>True if saga can be compensated</returns>
    public virtual bool CanCompensate()
    {
        lock (_stateLock)
        {
            return StateContext.CanCompensate;
        }
    }
    
    /// <summary>
    /// Handles step failure and determines if compensation should be triggered
    /// </summary>
    /// <param name="stepInfo">Failed step</param>
    /// <param name="exception">Exception that caused failure</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Task representing the failure handling</returns>
    protected virtual async Task HandleStepFailureAsync(SagaStepInfo stepInfo, Exception exception, CancellationToken cancellationToken)
    {
        lock (_stateLock)
        {
            stepInfo.MarkAsFailed(exception.Message, exception.ToString());
        }
        
        OnStepFailed(stepInfo, exception);
        
        // Check if we should retry the step
        if (stepInfo.RetryAttempts < stepInfo.MaxRetries)
        {
            stepInfo.RetryAttempts++;
            stepInfo.State = SagaStepState.Pending;
            await SaveStateAsync(cancellationToken).ConfigureAwait(false);
            return;
        }
        
        // If it's a critical step, trigger compensation
        if (stepInfo.IsCritical && CanCompensate())
        {
            await CompensateAsync($"Critical step {stepInfo.StepId} failed: {exception.Message}", cancellationToken).ConfigureAwait(false);
        }
        else
        {
            lock (_stateLock)
            {
                ChangeState(SagaState.Faulted);
                StateContext.LastError = exception.Message;
            }
            
            await SaveStateAsync(cancellationToken).ConfigureAwait(false);
        }
    }
    
    /// <summary>
    /// Changes the saga state and triggers state change event
    /// </summary>
    /// <param name="newState">New state</param>
    protected virtual void ChangeState(SagaState newState)
    {
        var previousState = StateContext.State;
        StateContext.State = newState;
        
        StateChanged?.Invoke(this, new SagaStateChangedEventArgs
        {
            SagaId = SagaId,
            PreviousState = previousState,
            NewState = newState,
            Context = new Dictionary<string, object>
            {
                ["SagaType"] = SagaType,
                ["Version"] = StateContext.Version
            }
        });
    }
    
    /// <summary>
    /// Triggers step completed event
    /// </summary>
    /// <param name="stepInfo">Completed step</param>
    /// <param name="outputData">Output data</param>
    protected virtual void OnStepCompleted(SagaStepInfo stepInfo, Dictionary<string, object> outputData)
    {
        StepCompleted?.Invoke(this, new SagaStepCompletedEventArgs
        {
            SagaId = SagaId,
            StepInfo = stepInfo,
            OutputData = outputData
        });
    }
    
    /// <summary>
    /// Triggers step failed event
    /// </summary>
    /// <param name="stepInfo">Failed step</param>
    /// <param name="exception">Exception that caused failure</param>
    protected virtual void OnStepFailed(SagaStepInfo stepInfo, Exception exception)
    {
        var willCompensate = stepInfo.IsCritical && CanCompensate();
        
        StepFailed?.Invoke(this, new SagaStepFailedEventArgs
        {
            SagaId = SagaId,
            StepInfo = stepInfo,
            Exception = exception,
            WillCompensate = willCompensate
        });
    }
    
    /// <summary>
    /// Triggers compensation triggered event
    /// </summary>
    /// <param name="reason">Reason for compensation</param>
    /// <param name="stepsToCompensate">Steps that will be compensated</param>
    protected virtual void OnCompensationTriggered(string reason, List<SagaStepInfo> stepsToCompensate)
    {
        CompensationTriggered?.Invoke(this, new SagaCompensationEventArgs
        {
            SagaId = SagaId,
            Reason = reason,
            StepsToCompensate = stepsToCompensate
        });
    }
    
    /// <summary>
    /// Releases all resources used by the saga
    /// </summary>
    public virtual void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }
    
    /// <summary>
    /// Releases resources used by the saga
    /// </summary>
    /// <param name="disposing">Whether disposing is called from Dispose() method</param>
    protected virtual void Dispose(bool disposing)
    {
        if (!_disposed && disposing)
        {
            _disposed = true;
        }
    }
}