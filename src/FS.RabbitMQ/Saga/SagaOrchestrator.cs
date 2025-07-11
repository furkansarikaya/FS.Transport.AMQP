using System.Collections.Concurrent;
using System.Diagnostics;
using FS.Mediator.Features.RequestHandling.Core;

namespace FS.RabbitMQ.Saga;

/// <summary>
/// Implementation of saga orchestrator providing centralized saga management with FS.Mediator integration
/// </summary>
public class SagaOrchestrator : ISagaOrchestrator
{
    private readonly IMediator _mediator;
    private readonly SagaSettings _settings;
    private readonly ConcurrentDictionary<string, ISaga> _activeSagas;
    private readonly ConcurrentDictionary<string, Func<string, string?, ISaga>> _sagaFactories;
    private readonly Timer? _healthCheckTimer;
    private readonly Timer? _statisticsUpdateTimer;
    private readonly object _statisticsLock = new();
    private readonly DateTimeOffset _startTime;
    private SagaOrchestratorStatistics _statistics;
    private bool _disposed = false;
    private bool _running = false;
    
    /// <summary>
    /// Mediator for CQRS operations
    /// </summary>
    public IMediator Mediator => _mediator;
    
    /// <summary>
    /// Saga settings
    /// </summary>
    public SagaSettings Settings => _settings;
    
    /// <summary>
    /// Event triggered when a saga state changes
    /// </summary>
    public event EventHandler<SagaStateChangedEventArgs>? SagaStateChanged;
    
    /// <summary>
    /// Event triggered when a saga step is completed
    /// </summary>
    public event EventHandler<SagaStepCompletedEventArgs>? SagaStepCompleted;
    
    /// <summary>
    /// Event triggered when a saga step fails
    /// </summary>
    public event EventHandler<SagaStepFailedEventArgs>? SagaStepFailed;
    
    /// <summary>
    /// Event triggered when saga compensation is triggered
    /// </summary>
    public event EventHandler<SagaCompensationEventArgs>? SagaCompensationTriggered;
    
    /// <summary>
    /// Creates a new saga orchestrator
    /// </summary>
    /// <param name="mediator">Mediator for CQRS operations</param>
    /// <param name="settings">Saga settings</param>
    public SagaOrchestrator(IMediator mediator, SagaSettings? settings = null)
    {
        _mediator = mediator ?? throw new ArgumentNullException(nameof(mediator));
        _settings = settings ?? SagaSettings.CreateDefault();
        _activeSagas = new ConcurrentDictionary<string, ISaga>();
        _sagaFactories = new ConcurrentDictionary<string, Func<string, string?, ISaga>>();
        _startTime = DateTimeOffset.UtcNow;
        _statistics = new SagaOrchestratorStatistics();
        
        // Validate settings
        _settings.Validate();
        
        // Setup timers if enabled
        if (_settings.EnableHealthChecks)
        {
            _healthCheckTimer = new Timer(PerformHealthChecks, null, _settings.HealthCheckInterval, _settings.HealthCheckInterval);
        }
        
        _statisticsUpdateTimer = new Timer(UpdateStatistics, null, TimeSpan.FromMinutes(1), TimeSpan.FromMinutes(1));
    }
    
    /// <summary>
    /// Starts a new saga
    /// </summary>
    /// <param name="command">Start saga command</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Task with start saga result</returns>
    public async Task<StartSagaResult> StartSagaAsync(StartSagaCommand command, CancellationToken cancellationToken = default)
    {
        if (_disposed)
            throw new ObjectDisposedException(nameof(SagaOrchestrator));
        
        if (!_running)
            throw new InvalidOperationException("Saga orchestrator is not running");
        
        try
        {
            // Check if saga already exists
            if (_activeSagas.ContainsKey(command.SagaId))
            {
                return StartSagaResult.CreateFailure(command.SagaId, $"Saga {command.SagaId} already exists");
            }
            
            // Create saga instance
            var saga = CreateSaga(command.SagaType, command.SagaId, command.CorrelationId);
            if (saga == null)
            {
                return StartSagaResult.CreateFailure(command.SagaId, $"Failed to create saga of type {command.SagaType}");
            }
            
            // Subscribe to saga events
            SubscribeToSagaEvents(saga);
            
            // Add to active sagas
            _activeSagas.TryAdd(command.SagaId, saga);
            
            // Start the saga
            await saga.StartAsync(command.InputData, cancellationToken).ConfigureAwait(false);
            
            // Update statistics
            lock (_statisticsLock)
            {
                _statistics.ActiveSagas = _activeSagas.Count;
            }
            
            return StartSagaResult.CreateSuccess(command.SagaId, saga.State);
        }
        catch (Exception ex)
        {
            return StartSagaResult.CreateFailure(command.SagaId, ex.Message, ex);
        }
    }
    
    /// <summary>
    /// Executes a saga step
    /// </summary>
    /// <param name="command">Execute saga step command</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Task with execute saga step result</returns>
    public async Task<ExecuteSagaStepResult> ExecuteSagaStepAsync(ExecuteSagaStepCommand command, CancellationToken cancellationToken = default)
    {
        if (_disposed)
            throw new ObjectDisposedException(nameof(SagaOrchestrator));
        
        try
        {
            var saga = await GetSagaInstanceAsync(command.SagaId, cancellationToken).ConfigureAwait(false);
            if (saga == null)
            {
                return ExecuteSagaStepResult.CreateFailure(command.SagaId, command.StepId, $"Saga {command.SagaId} not found");
            }
            
            var stepResult = await saga.ExecuteStepAsync(command.StepId, cancellationToken).ConfigureAwait(false);
            
            // Update statistics
            lock (_statisticsLock)
            {
                _statistics.TotalStepsExecuted++;
                if (!stepResult.Success)
                {
                    _statistics.TotalStepRetries++;
                }
            }
            
            return ExecuteSagaStepResult.CreateSuccess(command.SagaId, command.StepId, stepResult, stepResult.OutputData);
        }
        catch (Exception ex)
        {
            return ExecuteSagaStepResult.CreateFailure(command.SagaId, command.StepId, ex.Message, ex);
        }
    }
    
    /// <summary>
    /// Compensates a saga
    /// </summary>
    /// <param name="command">Compensate saga command</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Task with compensate saga result</returns>
    public async Task<CompensateSagaResult> CompensateSagaAsync(CompensateSagaCommand command, CancellationToken cancellationToken = default)
    {
        if (_disposed)
            throw new ObjectDisposedException(nameof(SagaOrchestrator));
        
        try
        {
            var saga = await GetSagaInstanceAsync(command.SagaId, cancellationToken).ConfigureAwait(false);
            if (saga == null)
            {
                return CompensateSagaResult.CreateFailure(command.SagaId, $"Saga {command.SagaId} not found");
            }
            
            if (!command.ForceCompensation && !saga.CanCompensate())
            {
                return CompensateSagaResult.CreateFailure(command.SagaId, $"Saga {command.SagaId} cannot be compensated");
            }
            
            var stepsToCompensate = saga.GetStepsToCompensate().Count();
            
            await saga.CompensateAsync(command.Reason, cancellationToken).ConfigureAwait(false);
            
            // Update statistics
            lock (_statisticsLock)
            {
                _statistics.CompensatedSagas++;
                _statistics.TotalStepsCompensated += stepsToCompensate;
            }
            
            return CompensateSagaResult.CreateSuccess(command.SagaId, saga.State, stepsToCompensate);
        }
        catch (Exception ex)
        {
            return CompensateSagaResult.CreateFailure(command.SagaId, ex.Message, ex);
        }
    }
    
    /// <summary>
    /// Cancels a saga
    /// </summary>
    /// <param name="command">Cancel saga command</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Task with cancel saga result</returns>
    public async Task<CancelSagaResult> CancelSagaAsync(CancelSagaCommand command, CancellationToken cancellationToken = default)
    {
        if (_disposed)
            throw new ObjectDisposedException(nameof(SagaOrchestrator));
        
        try
        {
            var saga = await GetSagaInstanceAsync(command.SagaId, cancellationToken).ConfigureAwait(false);
            if (saga == null)
            {
                return CancelSagaResult.CreateFailure(command.SagaId, $"Saga {command.SagaId} not found");
            }
            
            bool compensationPerformed = false;
            
            // Compensate before cancelling if requested
            if (command.CompensateBeforeCancel && saga.CanCompensate())
            {
                await saga.CompensateAsync($"Cancelled: {command.Reason}", cancellationToken).ConfigureAwait(false);
                compensationPerformed = true;
            }
            
            await saga.CancelAsync(command.Reason, cancellationToken).ConfigureAwait(false);
            
            // Remove from active sagas
            _activeSagas.TryRemove(command.SagaId, out _);
            
            // Update statistics
            lock (_statisticsLock)
            {
                _statistics.CancelledSagas++;
                _statistics.ActiveSagas = _activeSagas.Count;
            }
            
            return CancelSagaResult.CreateSuccess(command.SagaId, saga.State, compensationPerformed);
        }
        catch (Exception ex)
        {
            return CancelSagaResult.CreateFailure(command.SagaId, ex.Message, ex);
        }
    }
    
    /// <summary>
    /// Completes a saga
    /// </summary>
    /// <param name="command">Complete saga command</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Task with complete saga result</returns>
    public async Task<CompleteSagaResult> CompleteSagaAsync(CompleteSagaCommand command, CancellationToken cancellationToken = default)
    {
        if (_disposed)
            throw new ObjectDisposedException(nameof(SagaOrchestrator));
        
        try
        {
            var saga = await GetSagaInstanceAsync(command.SagaId, cancellationToken).ConfigureAwait(false);
            if (saga == null)
            {
                return CompleteSagaResult.CreateFailure(command.SagaId, $"Saga {command.SagaId} not found");
            }
            
            await saga.CompleteAsync(command.OutputData, cancellationToken).ConfigureAwait(false);
            
            // Remove from active sagas
            _activeSagas.TryRemove(command.SagaId, out _);
            
            // Update statistics
            lock (_statisticsLock)
            {
                _statistics.CompletedSagas++;
                _statistics.ActiveSagas = _activeSagas.Count;
                
                // Update average execution time
                if (saga.StateContext.Duration.HasValue)
                {
                    var totalCompletedSagas = _statistics.CompletedSagas;
                    var currentAverage = _statistics.AverageExecutionTime;
                    _statistics.AverageExecutionTime = TimeSpan.FromTicks(
                        (currentAverage.Ticks * (totalCompletedSagas - 1) + saga.StateContext.Duration.Value.Ticks) / totalCompletedSagas);
                }
            }
            
            return CompleteSagaResult.CreateSuccess(command.SagaId, saga.State, command.OutputData, saga.StateContext.Duration);
        }
        catch (Exception ex)
        {
            return CompleteSagaResult.CreateFailure(command.SagaId, ex.Message, ex);
        }
    }
    
    /// <summary>
    /// Gets a saga by ID
    /// </summary>
    /// <param name="query">Get saga query</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Task with get saga result</returns>
    public async Task<GetSagaResult> GetSagaAsync(GetSagaQuery query, CancellationToken cancellationToken = default)
    {
        if (_disposed)
            throw new ObjectDisposedException(nameof(SagaOrchestrator));
        
        try
        {
            var saga = await GetSagaInstanceAsync(query.SagaId, cancellationToken).ConfigureAwait(false);
            if (saga == null)
            {
                return GetSagaResult.CreateFailure($"Saga {query.SagaId} not found");
            }
            
            SagaStatistics? statistics = null;
            if (query.IncludeStatistics)
            {
                statistics = saga.GetStatistics();
            }
            
            return GetSagaResult.CreateSuccess(saga.StateContext, statistics);
        }
        catch (Exception ex)
        {
            return GetSagaResult.CreateFailure(ex.Message, ex);
        }
    }
    
    /// <summary>
    /// Gets saga state
    /// </summary>
    /// <param name="query">Get saga state query</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Task with get saga state result</returns>
    public async Task<GetSagaStateResult> GetSagaStateAsync(GetSagaStateQuery query, CancellationToken cancellationToken = default)
    {
        if (_disposed)
            throw new ObjectDisposedException(nameof(SagaOrchestrator));
        
        try
        {
            var saga = await GetSagaInstanceAsync(query.SagaId, cancellationToken).ConfigureAwait(false);
            if (saga == null)
            {
                return GetSagaStateResult.CreateFailure($"Saga {query.SagaId} not found");
            }
            
            return GetSagaStateResult.CreateSuccess(saga.StateContext);
        }
        catch (Exception ex)
        {
            return GetSagaStateResult.CreateFailure(ex.Message, ex);
        }
    }
    
    /// <summary>
    /// Gets active sagas
    /// </summary>
    /// <param name="query">Get active sagas query</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Task with get active sagas result</returns>
    public async Task<GetActiveSagasResult> GetActiveSagasAsync(GetActiveSagasQuery query, CancellationToken cancellationToken = default)
    {
        if (_disposed)
            throw new ObjectDisposedException(nameof(SagaOrchestrator));
        
        try
        {
            var sagas = _activeSagas.Values
                .Where(s => query.SagaType == null || s.SagaType == query.SagaType)
                .Where(s => query.CorrelationId == null || s.CorrelationId == query.CorrelationId)
                .Where(s => query.State == null || s.State == query.State)
                .Select(s => s.StateContext)
                .ToList();
            
            var totalCount = sagas.Count;
            var pagedSagas = sagas
                .Skip((query.Page - 1) * query.PageSize)
                .Take(query.PageSize)
                .ToList();
            
            return GetActiveSagasResult.CreateSuccess(pagedSagas, totalCount, query.Page, query.PageSize);
        }
        catch (Exception ex)
        {
            return GetActiveSagasResult.CreateFailure(ex.Message, ex);
        }
    }
    
    /// <summary>
    /// Gets saga statistics
    /// </summary>
    /// <param name="query">Get saga statistics query</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Task with get saga statistics result</returns>
    public async Task<GetSagaStatisticsResult> GetSagaStatisticsAsync(GetSagaStatisticsQuery query, CancellationToken cancellationToken = default)
    {
        if (_disposed)
            throw new ObjectDisposedException(nameof(SagaOrchestrator));
        
        try
        {
            var saga = await GetSagaInstanceAsync(query.SagaId, cancellationToken).ConfigureAwait(false);
            if (saga == null)
            {
                return GetSagaStatisticsResult.CreateFailure($"Saga {query.SagaId} not found");
            }
            
            var statistics = saga.GetStatistics();
            
            Dictionary<string, SagaStepStatistics>? stepStatistics = null;
            if (query.IncludeStepStatistics)
            {
                stepStatistics = saga.GetAllSteps().ToDictionary(
                    step => step.StepId,
                    step => new SagaStepStatistics
                    {
                        StepId = step.StepId,
                        StepName = step.StepName,
                        StepType = step.StepType,
                        State = step.State,
                        ExecutionOrder = step.ExecutionOrder,
                        RetryAttempts = step.RetryAttempts,
                        MaxRetries = step.MaxRetries,
                        Duration = step.Duration,
                        IsCritical = step.IsCritical,
                        CanCompensate = step.CanCompensate,
                        StartedAt = step.StartedAt,
                        CompletedAt = step.CompletedAt,
                        ErrorMessage = step.ErrorMessage
                    });
            }
            
            return GetSagaStatisticsResult.CreateSuccess(statistics, stepStatistics);
        }
        catch (Exception ex)
        {
            return GetSagaStatisticsResult.CreateFailure(ex.Message, ex);
        }
    }
    
    /// <summary>
    /// Checks if saga exists
    /// </summary>
    /// <param name="query">Saga exists query</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Task with saga exists result</returns>
    public async Task<SagaExistsResult> SagaExistsAsync(SagaExistsQuery query, CancellationToken cancellationToken = default)
    {
        if (_disposed)
            throw new ObjectDisposedException(nameof(SagaOrchestrator));
        
        try
        {
            var saga = await GetSagaInstanceAsync(query.SagaId, cancellationToken).ConfigureAwait(false);
            var exists = saga != null;
            var state = saga?.State;
            
            return SagaExistsResult.CreateSuccess(query.SagaId, exists, state);
        }
        catch (Exception ex)
        {
            return SagaExistsResult.CreateFailure(query.SagaId, ex.Message, ex);
        }
    }
    
    /// <summary>
    /// Gets saga steps
    /// </summary>
    /// <param name="query">Get saga steps query</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Task with get saga steps result</returns>
    public async Task<GetSagaStepsResult> GetSagaStepsAsync(GetSagaStepsQuery query, CancellationToken cancellationToken = default)
    {
        if (_disposed)
            throw new ObjectDisposedException(nameof(SagaOrchestrator));
        
        try
        {
            var saga = await GetSagaInstanceAsync(query.SagaId, cancellationToken).ConfigureAwait(false);
            if (saga == null)
            {
                return GetSagaStepsResult.CreateFailure(query.SagaId, $"Saga {query.SagaId} not found");
            }
            
            var steps = saga.GetAllSteps()
                .Where(s => query.StateFilter == null || s.State == query.StateFilter)
                .ToList();
            
            return GetSagaStepsResult.CreateSuccess(query.SagaId, steps);
        }
        catch (Exception ex)
        {
            return GetSagaStepsResult.CreateFailure(query.SagaId, ex.Message, ex);
        }
    }
    
    /// <summary>
    /// Saves saga state
    /// </summary>
    /// <param name="command">Save saga state command</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Task with save saga state result</returns>
    public async Task<SaveSagaStateResult> SaveSagaStateAsync(SaveSagaStateCommand command, CancellationToken cancellationToken = default)
    {
        if (_disposed)
            throw new ObjectDisposedException(nameof(SagaOrchestrator));
        
        try
        {
            var saga = await GetSagaInstanceAsync(command.SagaId, cancellationToken).ConfigureAwait(false);
            if (saga == null)
            {
                return SaveSagaStateResult.CreateFailure(command.SagaId, $"Saga {command.SagaId} not found");
            }
            
            await saga.SaveStateAsync(cancellationToken).ConfigureAwait(false);
            
            return SaveSagaStateResult.CreateSuccess(command.SagaId, saga.Version, command.CreateSnapshot);
        }
        catch (Exception ex)
        {
            return SaveSagaStateResult.CreateFailure(command.SagaId, ex.Message, ex);
        }
    }
    
    /// <summary>
    /// Registers a saga type
    /// </summary>
    /// <typeparam name="TSaga">Saga type</typeparam>
    /// <param name="sagaType">Saga type identifier</param>
    /// <param name="factory">Saga factory function</param>
    public void RegisterSagaType<TSaga>(string sagaType, Func<string, string?, TSaga> factory) where TSaga : class, ISaga
    {
        if (string.IsNullOrEmpty(sagaType))
            throw new ArgumentException("Saga type cannot be null or empty", nameof(sagaType));
        
        if (factory == null)
            throw new ArgumentNullException(nameof(factory));
        
        _sagaFactories.AddOrUpdate(sagaType, factory, (key, existing) => factory);
        
        lock (_statisticsLock)
        {
            _statistics.RegisteredSagaTypes = _sagaFactories.Count;
        }
    }
    
    /// <summary>
    /// Creates a new saga instance
    /// </summary>
    /// <param name="sagaType">Saga type</param>
    /// <param name="sagaId">Saga identifier</param>
    /// <param name="correlationId">Correlation ID</param>
    /// <returns>Saga instance</returns>
    public ISaga? CreateSaga(string sagaType, string sagaId, string? correlationId = null)
    {
        if (!_sagaFactories.TryGetValue(sagaType, out var factory))
        {
            return null;
        }
        
        return factory(sagaId, correlationId);
    }
    
    /// <summary>
    /// Gets a saga instance by ID
    /// </summary>
    /// <param name="sagaId">Saga identifier</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Saga instance or null if not found</returns>
    public async Task<ISaga?> GetSagaInstanceAsync(string sagaId, CancellationToken cancellationToken = default)
    {
        _activeSagas.TryGetValue(sagaId, out var saga);
        return saga;
    }
    
    /// <summary>
    /// Removes a saga instance from cache
    /// </summary>
    /// <param name="sagaId">Saga identifier</param>
    /// <returns>True if removed</returns>
    public bool RemoveSagaInstance(string sagaId)
    {
        var removed = _activeSagas.TryRemove(sagaId, out var saga);
        
        if (removed && saga != null)
        {
            UnsubscribeFromSagaEvents(saga);
            saga.Dispose();
            
            lock (_statisticsLock)
            {
                _statistics.ActiveSagas = _activeSagas.Count;
            }
        }
        
        return removed;
    }
    
    /// <summary>
    /// Gets all active saga instances
    /// </summary>
    /// <returns>Collection of active saga instances</returns>
    public IEnumerable<ISaga> GetActiveSagaInstances()
    {
        return _activeSagas.Values.ToList();
    }
    
    /// <summary>
    /// Gets orchestrator statistics
    /// </summary>
    /// <returns>Orchestrator statistics</returns>
    public SagaOrchestratorStatistics GetStatistics()
    {
        lock (_statisticsLock)
        {
            _statistics.Uptime = DateTimeOffset.UtcNow - _startTime;
            _statistics.MemoryUsage = GC.GetTotalMemory(false);
            _statistics.CollectedAt = DateTimeOffset.UtcNow;
            
            return new SagaOrchestratorStatistics
            {
                ActiveSagas = _statistics.ActiveSagas,
                CompletedSagas = _statistics.CompletedSagas,
                FailedSagas = _statistics.FailedSagas,
                CompensatedSagas = _statistics.CompensatedSagas,
                CancelledSagas = _statistics.CancelledSagas,
                AverageExecutionTime = _statistics.AverageExecutionTime,
                TotalStepsExecuted = _statistics.TotalStepsExecuted,
                TotalStepsCompensated = _statistics.TotalStepsCompensated,
                TotalStepRetries = _statistics.TotalStepRetries,
                Uptime = _statistics.Uptime,
                RegisteredSagaTypes = _statistics.RegisteredSagaTypes,
                MemoryUsage = _statistics.MemoryUsage,
                CollectedAt = _statistics.CollectedAt
            };
        }
    }
    
    /// <summary>
    /// Starts the orchestrator
    /// </summary>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Task representing the start operation</returns>
    public async Task StartAsync(CancellationToken cancellationToken = default)
    {
        if (_disposed)
            throw new ObjectDisposedException(nameof(SagaOrchestrator));
        
        if (_running)
            return;
        
        _running = true;
    }
    
    /// <summary>
    /// Stops the orchestrator
    /// </summary>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Task representing the stop operation</returns>
    public async Task StopAsync(CancellationToken cancellationToken = default)
    {
        if (_disposed || !_running)
            return;
        
        _running = false;
        
        // Cancel all active sagas
        var tasks = _activeSagas.Values.Select(saga => 
            saga.CancelAsync("Orchestrator stopping", cancellationToken)).ToArray();
        
        await Task.WhenAll(tasks).ConfigureAwait(false);
        
        // Clear active sagas
        foreach (var saga in _activeSagas.Values)
        {
            UnsubscribeFromSagaEvents(saga);
            saga.Dispose();
        }
        _activeSagas.Clear();
    }
    
    /// <summary>
    /// Gets the health status of the orchestrator
    /// </summary>
    /// <returns>Health status</returns>
    public SagaOrchestratorHealth GetHealth()
    {
        var health = new SagaOrchestratorHealth();
        var stopwatch = Stopwatch.StartNew();
        
        try
        {
            // Check if orchestrator is running
            if (!_running)
            {
                health.Checks.Add("orchestrator_running", HealthCheckResult.Unhealthy("Orchestrator is not running"));
                health.Status = HealthStatus.Unhealthy;
                health.Description = "Orchestrator is not running";
                return health;
            }
            
            health.Checks.Add("orchestrator_running", HealthCheckResult.Healthy("Orchestrator is running", stopwatch.Elapsed));
            
            // Check memory usage
            var memoryUsage = GC.GetTotalMemory(false);
            var memoryLimitMB = 1024 * 1024 * 1024; // 1GB limit
            
            if (memoryUsage > memoryLimitMB)
            {
                health.Checks.Add("memory_usage", HealthCheckResult.Degraded(
                    $"High memory usage: {memoryUsage / (1024 * 1024)}MB", 
                    stopwatch.Elapsed,
                    new Dictionary<string, object> { ["MemoryUsageBytes"] = memoryUsage }));
                health.Status = HealthStatus.Degraded;
            }
            else
            {
                health.Checks.Add("memory_usage", HealthCheckResult.Healthy(
                    $"Memory usage: {memoryUsage / (1024 * 1024)}MB", 
                    stopwatch.Elapsed,
                    new Dictionary<string, object> { ["MemoryUsageBytes"] = memoryUsage }));
            }
            
            // Check active saga count
            var activeSagaCount = _activeSagas.Count;
            if (activeSagaCount > _settings.MaxConcurrentSagas)
            {
                health.Checks.Add("active_sagas", HealthCheckResult.Degraded(
                    $"High active saga count: {activeSagaCount}/{_settings.MaxConcurrentSagas}", 
                    stopwatch.Elapsed,
                    new Dictionary<string, object> { ["ActiveSagas"] = activeSagaCount }));
                health.Status = HealthStatus.Degraded;
            }
            else
            {
                health.Checks.Add("active_sagas", HealthCheckResult.Healthy(
                    $"Active sagas: {activeSagaCount}/{_settings.MaxConcurrentSagas}", 
                    stopwatch.Elapsed,
                    new Dictionary<string, object> { ["ActiveSagas"] = activeSagaCount }));
            }
            
            // Check registered saga types
            var registeredTypes = _sagaFactories.Count;
            health.Checks.Add("registered_types", HealthCheckResult.Healthy(
                $"Registered saga types: {registeredTypes}", 
                stopwatch.Elapsed,
                new Dictionary<string, object> { ["RegisteredTypes"] = registeredTypes }));
            
            // Overall status
            if (health.Status == HealthStatus.Unhealthy)
            {
                health.Description = "Critical issues detected";
            }
            else if (health.Status == HealthStatus.Degraded)
            {
                health.Description = "Some issues detected but system is functional";
            }
            else
            {
                health.Status = HealthStatus.Healthy;
                health.Description = "All systems operational";
            }
        }
        catch (Exception ex)
        {
            health.Status = HealthStatus.Unhealthy;
            health.Description = "Health check failed";
            health.Checks.Add("health_check", HealthCheckResult.Unhealthy("Health check failed", ex, stopwatch.Elapsed));
        }
        
        return health;
    }
    
    /// <summary>
    /// Subscribes to saga events
    /// </summary>
    /// <param name="saga">Saga instance</param>
    private void SubscribeToSagaEvents(ISaga saga)
    {
        saga.StateChanged += OnSagaStateChanged;
        saga.StepCompleted += OnSagaStepCompleted;
        saga.StepFailed += OnSagaStepFailed;
        saga.CompensationTriggered += OnSagaCompensationTriggered;
    }
    
    /// <summary>
    /// Unsubscribes from saga events
    /// </summary>
    /// <param name="saga">Saga instance</param>
    private void UnsubscribeFromSagaEvents(ISaga saga)
    {
        saga.StateChanged -= OnSagaStateChanged;
        saga.StepCompleted -= OnSagaStepCompleted;
        saga.StepFailed -= OnSagaStepFailed;
        saga.CompensationTriggered -= OnSagaCompensationTriggered;
    }
    
    /// <summary>
    /// Handles saga state change events
    /// </summary>
    /// <param name="sender">Sender</param>
    /// <param name="e">Event arguments</param>
    private void OnSagaStateChanged(object? sender, SagaStateChangedEventArgs e)
    {
        SagaStateChanged?.Invoke(this, e);
        
        // Update statistics based on state changes
        lock (_statisticsLock)
        {
            if (e.NewState == SagaState.Completed)
            {
                _statistics.CompletedSagas++;
            }
            else if (e.NewState == SagaState.Faulted || e.NewState == SagaState.CompensationFailed)
            {
                _statistics.FailedSagas++;
            }
        }
    }
    
    /// <summary>
    /// Handles saga step completed events
    /// </summary>
    /// <param name="sender">Sender</param>
    /// <param name="e">Event arguments</param>
    private void OnSagaStepCompleted(object? sender, SagaStepCompletedEventArgs e)
    {
        SagaStepCompleted?.Invoke(this, e);
    }
    
    /// <summary>
    /// Handles saga step failed events
    /// </summary>
    /// <param name="sender">Sender</param>
    /// <param name="e">Event arguments</param>
    private void OnSagaStepFailed(object? sender, SagaStepFailedEventArgs e)
    {
        SagaStepFailed?.Invoke(this, e);
    }
    
    /// <summary>
    /// Handles saga compensation triggered events
    /// </summary>
    /// <param name="sender">Sender</param>
    /// <param name="e">Event arguments</param>
    private void OnSagaCompensationTriggered(object? sender, SagaCompensationEventArgs e)
    {
        SagaCompensationTriggered?.Invoke(this, e);
    }
    
    /// <summary>
    /// Performs health checks
    /// </summary>
    /// <param name="state">Timer state</param>
    private void PerformHealthChecks(object? state)
    {
        try
        {
            var health = GetHealth();
            
            // Log health status if needed
            if (health.Status != HealthStatus.Healthy && _settings.EnableEventLogging)
            {
                // Log health issues
            }
        }
        catch
        {
            // Ignore health check errors
        }
    }
    
    /// <summary>
    /// Updates statistics
    /// </summary>
    /// <param name="state">Timer state</param>
    private void UpdateStatistics(object? state)
    {
        try
        {
            lock (_statisticsLock)
            {
                _statistics.ActiveSagas = _activeSagas.Count;
                _statistics.Uptime = DateTimeOffset.UtcNow - _startTime;
                _statistics.MemoryUsage = GC.GetTotalMemory(false);
                _statistics.RegisteredSagaTypes = _sagaFactories.Count;
                _statistics.CollectedAt = DateTimeOffset.UtcNow;
            }
        }
        catch
        {
            // Ignore statistics update errors
        }
    }
    
    /// <summary>
    /// Releases all resources used by the orchestrator
    /// </summary>
    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }
    
    /// <summary>
    /// Releases resources used by the orchestrator
    /// </summary>
    /// <param name="disposing">Whether disposing is called from Dispose() method</param>
    protected virtual void Dispose(bool disposing)
    {
        if (!_disposed && disposing)
        {
            _healthCheckTimer?.Dispose();
            _statisticsUpdateTimer?.Dispose();
            
            // Stop and dispose all active sagas
            foreach (var saga in _activeSagas.Values)
            {
                try
                {
                    UnsubscribeFromSagaEvents(saga);
                    saga.Dispose();
                }
                catch
                {
                    // Ignore disposal errors
                }
            }
            
            _activeSagas.Clear();
            _sagaFactories.Clear();
            _disposed = true;
        }
    }
}