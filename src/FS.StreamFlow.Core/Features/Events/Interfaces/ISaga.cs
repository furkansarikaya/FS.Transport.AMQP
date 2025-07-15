using FS.StreamFlow.Core.Features.Events.Models;

namespace FS.StreamFlow.Core.Features.Events.Interfaces;

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
    SagaContext Context { get; }
    
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
    /// <returns>Task representing the execution operation</returns>
    Task ExecuteNextStepAsync(CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Compensates the saga by rolling back completed steps
    /// </summary>
    /// <param name="reason">Reason for compensation</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Task representing the compensation operation</returns>
    Task CompensateAsync(string reason, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Completes the saga successfully
    /// </summary>
    /// <param name="result">Final result data</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Task representing the completion operation</returns>
    Task CompleteAsync(Dictionary<string, object>? result = null, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Aborts the saga due to failure
    /// </summary>
    /// <param name="error">Error that caused the abort</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Task representing the abort operation</returns>
    Task AbortAsync(Exception error, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Persists the current saga state
    /// </summary>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Task representing the persistence operation</returns>
    Task PersistAsync(CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Restores the saga from persisted state
    /// </summary>
    /// <param name="sagaId">Saga identifier</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Task representing the restoration operation</returns>
    Task RestoreAsync(string sagaId, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Handles an incoming event for the saga
    /// </summary>
    /// <param name="event">Event to handle</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Task representing the event handling operation</returns>
    Task HandleEventAsync(IEvent @event, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Gets the current saga data
    /// </summary>
    /// <returns>Saga data dictionary</returns>
    Dictionary<string, object> GetSagaData();
    
    /// <summary>
    /// Updates saga data
    /// </summary>
    /// <param name="data">Data to update</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Task representing the update operation</returns>
    Task UpdateDataAsync(Dictionary<string, object> data, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Checks if the saga can handle the given event
    /// </summary>
    /// <param name="event">Event to check</param>
    /// <returns>True if the saga can handle the event, otherwise false</returns>
    bool CanHandle(IEvent @event);
    
    /// <summary>
    /// Gets the saga timeout
    /// </summary>
    /// <returns>Timeout duration or null if no timeout</returns>
    TimeSpan? GetTimeout();
    
    /// <summary>
    /// Handles saga timeout
    /// </summary>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Task representing the timeout handling operation</returns>
    Task HandleTimeoutAsync(CancellationToken cancellationToken = default);
}

/// <summary>
/// Interface for saga orchestrator providing centralized saga management
/// </summary>
public interface ISagaOrchestrator : IDisposable
{
    /// <summary>
    /// Saga settings
    /// </summary>
    SagaSettings Settings { get; }
    
    /// <summary>
    /// Event triggered when a saga state changes
    /// </summary>
    event EventHandler<SagaStateChangedEventArgs>? SagaStateChanged;
    
    /// <summary>
    /// Event triggered when a saga step is completed
    /// </summary>
    event EventHandler<SagaStepCompletedEventArgs>? SagaStepCompleted;
    
    /// <summary>
    /// Event triggered when a saga step fails
    /// </summary>
    event EventHandler<SagaStepFailedEventArgs>? SagaStepFailed;
    
    /// <summary>
    /// Event triggered when saga compensation is triggered
    /// </summary>
    event EventHandler<SagaCompensationEventArgs>? SagaCompensationTriggered;
    
    /// <summary>
    /// Starts a new saga
    /// </summary>
    /// <param name="sagaType">Type of saga to start</param>
    /// <param name="inputData">Input data for the saga</param>
    /// <param name="correlationId">Correlation ID for tracking</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Task with saga identifier</returns>
    Task<string> StartSagaAsync(string sagaType, Dictionary<string, object> inputData, string? correlationId = null, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Executes a saga step
    /// </summary>
    /// <param name="sagaId">Saga identifier</param>
    /// <param name="stepData">Step data</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Task with step execution result</returns>
    Task<bool> ExecuteSagaStepAsync(string sagaId, Dictionary<string, object> stepData, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Compensates a saga
    /// </summary>
    /// <param name="sagaId">Saga identifier</param>
    /// <param name="reason">Reason for compensation</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Task with compensation result</returns>
    Task<bool> CompensateSagaAsync(string sagaId, string reason, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Completes a saga
    /// </summary>
    /// <param name="sagaId">Saga identifier</param>
    /// <param name="result">Final result data</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Task with completion result</returns>
    Task<bool> CompleteSagaAsync(string sagaId, Dictionary<string, object>? result = null, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Aborts a saga
    /// </summary>
    /// <param name="sagaId">Saga identifier</param>
    /// <param name="error">Error that caused the abort</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Task with abort result</returns>
    Task<bool> AbortSagaAsync(string sagaId, Exception error, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Gets saga by identifier
    /// </summary>
    /// <param name="sagaId">Saga identifier</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Saga instance or null if not found</returns>
    Task<ISaga?> GetSagaAsync(string sagaId, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Gets all active sagas
    /// </summary>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Collection of active sagas</returns>
    Task<IEnumerable<ISaga>> GetActiveSagasAsync(CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Handles an incoming event for saga processing
    /// </summary>
    /// <param name="event">Event to handle</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Task representing the event handling operation</returns>
    Task HandleEventAsync(IEvent @event, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Registers a saga type with the orchestrator
    /// </summary>
    /// <param name="sagaType">Saga type name</param>
    /// <param name="sagaFactory">Factory function to create saga instances</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Task representing the registration operation</returns>
    Task RegisterSagaTypeAsync(string sagaType, Func<string, ISaga> sagaFactory, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Unregisters a saga type
    /// </summary>
    /// <param name="sagaType">Saga type name</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Task representing the unregistration operation</returns>
    Task UnregisterSagaTypeAsync(string sagaType, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Starts the saga orchestrator
    /// </summary>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Task representing the start operation</returns>
    Task StartAsync(CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Stops the saga orchestrator
    /// </summary>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Task representing the stop operation</returns>
    Task StopAsync(CancellationToken cancellationToken = default);
} 