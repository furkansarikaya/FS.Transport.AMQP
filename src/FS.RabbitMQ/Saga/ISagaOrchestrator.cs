using FS.Mediator.Features.RequestHandling.Core;

namespace FS.RabbitMQ.Saga;

/// <summary>
/// Interface for saga orchestration providing centralized saga management
/// </summary>
public interface ISagaOrchestrator : IDisposable
{
    /// <summary>
    /// Mediator for CQRS operations
    /// </summary>
    IMediator Mediator { get; }
    
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
    /// <param name="command">Start saga command</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Task with start saga result</returns>
    Task<StartSagaResult> StartSagaAsync(StartSagaCommand command, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Executes a saga step
    /// </summary>
    /// <param name="command">Execute saga step command</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Task with execute saga step result</returns>
    Task<ExecuteSagaStepResult> ExecuteSagaStepAsync(ExecuteSagaStepCommand command, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Compensates a saga
    /// </summary>
    /// <param name="command">Compensate saga command</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Task with compensate saga result</returns>
    Task<CompensateSagaResult> CompensateSagaAsync(CompensateSagaCommand command, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Cancels a saga
    /// </summary>
    /// <param name="command">Cancel saga command</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Task with cancel saga result</returns>
    Task<CancelSagaResult> CancelSagaAsync(CancelSagaCommand command, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Completes a saga
    /// </summary>
    /// <param name="command">Complete saga command</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Task with complete saga result</returns>
    Task<CompleteSagaResult> CompleteSagaAsync(CompleteSagaCommand command, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Gets a saga by ID
    /// </summary>
    /// <param name="query">Get saga query</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Task with get saga result</returns>
    Task<GetSagaResult> GetSagaAsync(GetSagaQuery query, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Gets saga state
    /// </summary>
    /// <param name="query">Get saga state query</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Task with get saga state result</returns>
    Task<GetSagaStateResult> GetSagaStateAsync(GetSagaStateQuery query, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Gets active sagas
    /// </summary>
    /// <param name="query">Get active sagas query</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Task with get active sagas result</returns>
    Task<GetActiveSagasResult> GetActiveSagasAsync(GetActiveSagasQuery query, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Gets saga statistics
    /// </summary>
    /// <param name="query">Get saga statistics query</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Task with get saga statistics result</returns>
    Task<GetSagaStatisticsResult> GetSagaStatisticsAsync(GetSagaStatisticsQuery query, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Checks if saga exists
    /// </summary>
    /// <param name="query">Saga exists query</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Task with saga exists result</returns>
    Task<SagaExistsResult> SagaExistsAsync(SagaExistsQuery query, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Gets saga steps
    /// </summary>
    /// <param name="query">Get saga steps query</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Task with get saga steps result</returns>
    Task<GetSagaStepsResult> GetSagaStepsAsync(GetSagaStepsQuery query, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Saves saga state
    /// </summary>
    /// <param name="command">Save saga state command</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Task with save saga state result</returns>
    Task<SaveSagaStateResult> SaveSagaStateAsync(SaveSagaStateCommand command, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Registers a saga type
    /// </summary>
    /// <typeparam name="TSaga">Saga type</typeparam>
    /// <param name="sagaType">Saga type identifier</param>
    /// <param name="factory">Saga factory function</param>
    void RegisterSagaType<TSaga>(string sagaType, Func<string, string?, TSaga> factory) where TSaga : class, ISaga;
    
    /// <summary>
    /// Creates a new saga instance
    /// </summary>
    /// <param name="sagaType">Saga type</param>
    /// <param name="sagaId">Saga identifier</param>
    /// <param name="correlationId">Correlation ID</param>
    /// <returns>Saga instance</returns>
    ISaga? CreateSaga(string sagaType, string sagaId, string? correlationId = null);
    
    /// <summary>
    /// Gets a saga instance by ID
    /// </summary>
    /// <param name="sagaId">Saga identifier</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Saga instance or null if not found</returns>
    Task<ISaga?> GetSagaInstanceAsync(string sagaId, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Removes a saga instance from cache
    /// </summary>
    /// <param name="sagaId">Saga identifier</param>
    /// <returns>True if removed</returns>
    bool RemoveSagaInstance(string sagaId);
    
    /// <summary>
    /// Gets all active saga instances
    /// </summary>
    /// <returns>Collection of active saga instances</returns>
    IEnumerable<ISaga> GetActiveSagaInstances();
    
    /// <summary>
    /// Gets orchestrator statistics
    /// </summary>
    /// <returns>Orchestrator statistics</returns>
    SagaOrchestratorStatistics GetStatistics();
    
    /// <summary>
    /// Starts the orchestrator
    /// </summary>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Task representing the start operation</returns>
    Task StartAsync(CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Stops the orchestrator
    /// </summary>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Task representing the stop operation</returns>
    Task StopAsync(CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Gets the health status of the orchestrator
    /// </summary>
    /// <returns>Health status</returns>
    SagaOrchestratorHealth GetHealth();
}

/// <summary>
/// Statistics for saga orchestrator
/// </summary>
public class SagaOrchestratorStatistics
{
    /// <summary>
    /// Total number of active sagas
    /// </summary>
    public int ActiveSagas { get; set; }
    
    /// <summary>
    /// Total number of completed sagas
    /// </summary>
    public long CompletedSagas { get; set; }
    
    /// <summary>
    /// Total number of failed sagas
    /// </summary>
    public long FailedSagas { get; set; }
    
    /// <summary>
    /// Total number of compensated sagas
    /// </summary>
    public long CompensatedSagas { get; set; }
    
    /// <summary>
    /// Total number of cancelled sagas
    /// </summary>
    public long CancelledSagas { get; set; }
    
    /// <summary>
    /// Average saga execution time
    /// </summary>
    public TimeSpan AverageExecutionTime { get; set; }
    
    /// <summary>
    /// Total number of steps executed
    /// </summary>
    public long TotalStepsExecuted { get; set; }
    
    /// <summary>
    /// Total number of steps compensated
    /// </summary>
    public long TotalStepsCompensated { get; set; }
    
    /// <summary>
    /// Total number of step retries
    /// </summary>
    public long TotalStepRetries { get; set; }
    
    /// <summary>
    /// Uptime of the orchestrator
    /// </summary>
    public TimeSpan Uptime { get; set; }
    
    /// <summary>
    /// Number of registered saga types
    /// </summary>
    public int RegisteredSagaTypes { get; set; }
    
    /// <summary>
    /// Memory usage in bytes
    /// </summary>
    public long MemoryUsage { get; set; }
    
    /// <summary>
    /// When statistics were collected
    /// </summary>
    public DateTimeOffset CollectedAt { get; set; } = DateTimeOffset.UtcNow;
}

/// <summary>
/// Health status for saga orchestrator
/// </summary>
public class SagaOrchestratorHealth
{
    /// <summary>
    /// Overall health status
    /// </summary>
    public HealthStatus Status { get; set; }
    
    /// <summary>
    /// Health description
    /// </summary>
    public string Description { get; set; } = string.Empty;
    
    /// <summary>
    /// List of health checks and their results
    /// </summary>
    public Dictionary<string, HealthCheckResult> Checks { get; set; } = new();
    
    /// <summary>
    /// When health was checked
    /// </summary>
    public DateTimeOffset CheckedAt { get; set; } = DateTimeOffset.UtcNow;
    
    /// <summary>
    /// Creates a healthy status
    /// </summary>
    /// <param name="description">Description</param>
    /// <returns>Healthy status</returns>
    public static SagaOrchestratorHealth Healthy(string description = "All systems operational")
    {
        return new SagaOrchestratorHealth
        {
            Status = HealthStatus.Healthy,
            Description = description
        };
    }
    
    /// <summary>
    /// Creates a degraded status
    /// </summary>
    /// <param name="description">Description</param>
    /// <returns>Degraded status</returns>
    public static SagaOrchestratorHealth Degraded(string description = "Some issues detected")
    {
        return new SagaOrchestratorHealth
        {
            Status = HealthStatus.Degraded,
            Description = description
        };
    }
    
    /// <summary>
    /// Creates an unhealthy status
    /// </summary>
    /// <param name="description">Description</param>
    /// <returns>Unhealthy status</returns>
    public static SagaOrchestratorHealth Unhealthy(string description = "Critical issues detected")
    {
        return new SagaOrchestratorHealth
        {
            Status = HealthStatus.Unhealthy,
            Description = description
        };
    }
}

/// <summary>
/// Health status enumeration
/// </summary>
public enum HealthStatus
{
    /// <summary>
    /// System is healthy
    /// </summary>
    Healthy,
    
    /// <summary>
    /// System is degraded but functional
    /// </summary>
    Degraded,
    
    /// <summary>
    /// System is unhealthy
    /// </summary>
    Unhealthy
}

/// <summary>
/// Health check result
/// </summary>
public class HealthCheckResult
{
    /// <summary>
    /// Health status
    /// </summary>
    public HealthStatus Status { get; set; }
    
    /// <summary>
    /// Check description
    /// </summary>
    public string Description { get; set; } = string.Empty;
    
    /// <summary>
    /// Exception if check failed
    /// </summary>
    public Exception? Exception { get; set; }
    
    /// <summary>
    /// Check duration
    /// </summary>
    public TimeSpan Duration { get; set; }
    
    /// <summary>
    /// Additional data
    /// </summary>
    public Dictionary<string, object> Data { get; set; } = new();
    
    /// <summary>
    /// Creates a healthy result
    /// </summary>
    /// <param name="description">Description</param>
    /// <param name="duration">Duration</param>
    /// <param name="data">Additional data</param>
    /// <returns>Healthy result</returns>
    public static HealthCheckResult Healthy(string description, TimeSpan duration, Dictionary<string, object>? data = null)
    {
        return new HealthCheckResult
        {
            Status = HealthStatus.Healthy,
            Description = description,
            Duration = duration,
            Data = data ?? new Dictionary<string, object>()
        };
    }
    
    /// <summary>
    /// Creates a degraded result
    /// </summary>
    /// <param name="description">Description</param>
    /// <param name="duration">Duration</param>
    /// <param name="data">Additional data</param>
    /// <returns>Degraded result</returns>
    public static HealthCheckResult Degraded(string description, TimeSpan duration, Dictionary<string, object>? data = null)
    {
        return new HealthCheckResult
        {
            Status = HealthStatus.Degraded,
            Description = description,
            Duration = duration,
            Data = data ?? new Dictionary<string, object>()
        };
    }
    
    /// <summary>
    /// Creates an unhealthy result
    /// </summary>
    /// <param name="description">Description</param>
    /// <param name="exception">Exception</param>
    /// <param name="duration">Duration</param>
    /// <param name="data">Additional data</param>
    /// <returns>Unhealthy result</returns>
    public static HealthCheckResult Unhealthy(string description, Exception? exception = null, TimeSpan duration = default, Dictionary<string, object>? data = null)
    {
        return new HealthCheckResult
        {
            Status = HealthStatus.Unhealthy,
            Description = description,
            Exception = exception,
            Duration = duration,
            Data = data ?? new Dictionary<string, object>()
        };
    }
}