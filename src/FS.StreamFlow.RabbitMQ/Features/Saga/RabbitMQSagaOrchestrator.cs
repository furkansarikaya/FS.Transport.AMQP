using FS.StreamFlow.Core.Features.Events.Interfaces;
using FS.StreamFlow.Core.Features.Events.Models;
using FS.StreamFlow.Core.Features.Messaging.Interfaces;
using FS.StreamFlow.RabbitMQ.Features.Connection;
using FS.StreamFlow.RabbitMQ.Features.ErrorHandling;
using Microsoft.Extensions.Logging;
using RabbitMQ.Client;
using System.Collections.Concurrent;
using System.Text.Json;

namespace FS.StreamFlow.RabbitMQ.Features.Saga;

/// <summary>
/// RabbitMQ implementation of saga orchestrator providing workflow management and compensation patterns
/// </summary>
public class RabbitMQSagaOrchestrator : ISagaOrchestrator
{
    private readonly IConnectionManager _connectionManager;
    private readonly ILogger<RabbitMQSagaOrchestrator> _logger;
    private readonly ConcurrentDictionary<string, ISaga> _activeSagas;
    private readonly ConcurrentDictionary<string, Func<string, ISaga>> _sagaFactories;
    private readonly Timer _timeoutTimer;
    private readonly object _lock = new();
    private volatile bool _disposed;
    private volatile bool _isStarted;

    /// <summary>
    /// Saga settings
    /// </summary>
    public SagaSettings Settings { get; }

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
    /// Initializes a new instance of the RabbitMQSagaOrchestrator class
    /// </summary>
    /// <param name="connectionManager">Connection manager</param>
    /// <param name="logger">Logger instance</param>
    /// <param name="settings">Saga settings</param>
    public RabbitMQSagaOrchestrator(
        IConnectionManager connectionManager,
        ILogger<RabbitMQSagaOrchestrator> logger,
        SagaSettings? settings = null)
    {
        _connectionManager = connectionManager ?? throw new ArgumentNullException(nameof(connectionManager));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        Settings = settings ?? new SagaSettings();
        _activeSagas = new ConcurrentDictionary<string, ISaga>();
        _sagaFactories = new ConcurrentDictionary<string, Func<string, ISaga>>();
        
        // Initialize timeout timer
        _timeoutTimer = new Timer(CheckSagaTimeouts, null, TimeSpan.Zero, Settings.TimeoutCheckInterval);
    }

    /// <summary>
    /// Starts a new saga
    /// </summary>
    /// <param name="sagaType">Type of saga to start</param>
    /// <param name="inputData">Input data for the saga</param>
    /// <param name="correlationId">Correlation ID for tracking</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Task with saga identifier</returns>
    public async Task<string> StartSagaAsync(string sagaType, Dictionary<string, object> inputData, string? correlationId = null, CancellationToken cancellationToken = default)
    {
        if (_disposed)
            throw new ObjectDisposedException(nameof(RabbitMQSagaOrchestrator));

        if (string.IsNullOrEmpty(sagaType))
            throw new ArgumentException("Saga type cannot be null or empty", nameof(sagaType));

        if (inputData == null)
            throw new ArgumentNullException(nameof(inputData));

        try
        {
            // Check if we've reached the maximum number of concurrent sagas
            if (_activeSagas.Count >= Settings.MaxConcurrentSagas)
            {
                throw new InvalidOperationException($"Maximum number of concurrent sagas ({Settings.MaxConcurrentSagas}) reached");
            }

            // Get saga factory
            if (!_sagaFactories.TryGetValue(sagaType, out var sagaFactory))
            {
                throw new InvalidOperationException($"Saga type '{sagaType}' is not registered");
            }

            // Create saga instance
            var sagaId = Guid.NewGuid().ToString();
            var saga = sagaFactory(sagaId);

            // Set correlation ID if provided
            if (!string.IsNullOrEmpty(correlationId))
            {
                saga.Context.CorrelationId = correlationId;
            }

            // Subscribe to saga events
            SubscribeToSagaEvents(saga);

            // Add saga to active sagas
            _activeSagas.TryAdd(sagaId, saga);

            // Start the saga
            await saga.StartAsync(inputData, cancellationToken);

            // Persist saga state
            await PersistSagaStateAsync(saga, cancellationToken);

            _logger.LogInformation("Started saga {SagaId} of type {SagaType} with correlation ID {CorrelationId}", 
                sagaId, sagaType, correlationId);

            return sagaId;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to start saga of type {SagaType}", sagaType);
            throw;
        }
    }

    /// <summary>
    /// Executes a saga step
    /// </summary>
    /// <param name="sagaId">Saga identifier</param>
    /// <param name="stepData">Step data</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Task with step execution result</returns>
    public async Task<bool> ExecuteSagaStepAsync(string sagaId, Dictionary<string, object> stepData, CancellationToken cancellationToken = default)
    {
        if (_disposed)
            throw new ObjectDisposedException(nameof(RabbitMQSagaOrchestrator));

        if (string.IsNullOrEmpty(sagaId))
            throw new ArgumentException("Saga ID cannot be null or empty", nameof(sagaId));

        try
        {
            if (!_activeSagas.TryGetValue(sagaId, out var saga))
            {
                _logger.LogWarning("Saga {SagaId} not found in active sagas", sagaId);
                return false;
            }

            await saga.ExecuteNextStepAsync(cancellationToken);
            await PersistSagaStateAsync(saga, cancellationToken);

            _logger.LogInformation("Executed step for saga {SagaId}", sagaId);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to execute step for saga {SagaId}", sagaId);
            return false;
        }
    }

    /// <summary>
    /// Compensates a saga
    /// </summary>
    /// <param name="sagaId">Saga identifier</param>
    /// <param name="reason">Reason for compensation</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Task with compensation result</returns>
    public async Task<bool> CompensateSagaAsync(string sagaId, string reason, CancellationToken cancellationToken = default)
    {
        if (_disposed)
            throw new ObjectDisposedException(nameof(RabbitMQSagaOrchestrator));

        if (string.IsNullOrEmpty(sagaId))
            throw new ArgumentException("Saga ID cannot be null or empty", nameof(sagaId));

        try
        {
            if (!_activeSagas.TryGetValue(sagaId, out var saga))
            {
                _logger.LogWarning("Saga {SagaId} not found in active sagas", sagaId);
                return false;
            }

            await saga.CompensateAsync(reason, cancellationToken);
            await PersistSagaStateAsync(saga, cancellationToken);

            _logger.LogInformation("Compensated saga {SagaId} with reason: {Reason}", sagaId, reason);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to compensate saga {SagaId}", sagaId);
            return false;
        }
    }

    /// <summary>
    /// Completes a saga
    /// </summary>
    /// <param name="sagaId">Saga identifier</param>
    /// <param name="result">Final result data</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Task with completion result</returns>
    public async Task<bool> CompleteSagaAsync(string sagaId, Dictionary<string, object>? result = null, CancellationToken cancellationToken = default)
    {
        if (_disposed)
            throw new ObjectDisposedException(nameof(RabbitMQSagaOrchestrator));

        if (string.IsNullOrEmpty(sagaId))
            throw new ArgumentException("Saga ID cannot be null or empty", nameof(sagaId));

        try
        {
            if (!_activeSagas.TryGetValue(sagaId, out var saga))
            {
                _logger.LogWarning("Saga {SagaId} not found in active sagas", sagaId);
                return false;
            }

            await saga.CompleteAsync(result, cancellationToken);
            await PersistSagaStateAsync(saga, cancellationToken);

            // Remove from active sagas
            _activeSagas.TryRemove(sagaId, out _);

            _logger.LogInformation("Completed saga {SagaId}", sagaId);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to complete saga {SagaId}", sagaId);
            return false;
        }
    }

    /// <summary>
    /// Aborts a saga
    /// </summary>
    /// <param name="sagaId">Saga identifier</param>
    /// <param name="error">Error that caused the abort</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Task with abort result</returns>
    public async Task<bool> AbortSagaAsync(string sagaId, Exception error, CancellationToken cancellationToken = default)
    {
        if (_disposed)
            throw new ObjectDisposedException(nameof(RabbitMQSagaOrchestrator));

        if (string.IsNullOrEmpty(sagaId))
            throw new ArgumentException("Saga ID cannot be null or empty", nameof(sagaId));

        try
        {
            if (!_activeSagas.TryGetValue(sagaId, out var saga))
            {
                _logger.LogWarning("Saga {SagaId} not found in active sagas", sagaId);
                return false;
            }

            await saga.AbortAsync(error, cancellationToken);
            await PersistSagaStateAsync(saga, cancellationToken);

            // Remove from active sagas
            _activeSagas.TryRemove(sagaId, out _);

            _logger.LogInformation("Aborted saga {SagaId} due to error: {Error}", sagaId, error.Message);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to abort saga {SagaId}", sagaId);
            return false;
        }
    }

    /// <summary>
    /// Gets saga by identifier
    /// </summary>
    /// <param name="sagaId">Saga identifier</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Saga instance or null if not found</returns>
    public async Task<ISaga?> GetSagaAsync(string sagaId, CancellationToken cancellationToken = default)
    {
        if (_disposed)
            throw new ObjectDisposedException(nameof(RabbitMQSagaOrchestrator));

        if (string.IsNullOrEmpty(sagaId))
            return null;

        // First check active sagas
        if (_activeSagas.TryGetValue(sagaId, out var saga))
        {
            return saga;
        }

        // Try to restore from persistence
        try
        {
            saga = await RestoreSagaFromPersistenceAsync(sagaId, cancellationToken);
            if (saga != null)
            {
                SubscribeToSagaEvents(saga);
                _activeSagas.TryAdd(sagaId, saga);
            }
            return saga;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to restore saga {SagaId} from persistence", sagaId);
            return null;
        }
    }

    /// <summary>
    /// Gets all active sagas
    /// </summary>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Collection of active sagas</returns>
    public async Task<IEnumerable<ISaga>> GetActiveSagasAsync(CancellationToken cancellationToken = default)
    {
        if (_disposed)
            throw new ObjectDisposedException(nameof(RabbitMQSagaOrchestrator));

        await Task.CompletedTask;
        return _activeSagas.Values.ToList();
    }

    /// <summary>
    /// Handles an incoming event for saga processing
    /// </summary>
    /// <param name="event">Event to handle</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Task representing the event handling operation</returns>
    public async Task HandleEventAsync(IEvent @event, CancellationToken cancellationToken = default)
    {
        if (_disposed)
            throw new ObjectDisposedException(nameof(RabbitMQSagaOrchestrator));

        if (@event == null)
            throw new ArgumentNullException(nameof(@event));

        try
        {
            var tasks = new List<Task>();

            // Find sagas that can handle this event
            foreach (var saga in _activeSagas.Values)
            {
                if (saga.CanHandle(@event))
                {
                    tasks.Add(HandleEventForSagaAsync(saga, @event, cancellationToken));
                }
            }

            // Handle events in parallel
            if (tasks.Count > 0)
            {
                await Task.WhenAll(tasks);
                _logger.LogInformation("Handled event {EventType} for {SagaCount} sagas", @event.GetType().Name, tasks.Count);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to handle event {EventType}", @event.GetType().Name);
            throw;
        }
    }

    /// <summary>
    /// Registers a saga type with the orchestrator
    /// </summary>
    /// <param name="sagaType">Saga type name</param>
    /// <param name="sagaFactory">Factory function to create saga instances</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Task representing the registration operation</returns>
    public async Task RegisterSagaTypeAsync(string sagaType, Func<string, ISaga> sagaFactory, CancellationToken cancellationToken = default)
    {
        if (_disposed)
            throw new ObjectDisposedException(nameof(RabbitMQSagaOrchestrator));

        if (string.IsNullOrEmpty(sagaType))
            throw new ArgumentException("Saga type cannot be null or empty", nameof(sagaType));

        if (sagaFactory == null)
            throw new ArgumentNullException(nameof(sagaFactory));

        _sagaFactories.AddOrUpdate(sagaType, sagaFactory, (_, _) => sagaFactory);
        
        _logger.LogInformation("Registered saga type: {SagaType}", sagaType);
        await Task.CompletedTask;
    }

    /// <summary>
    /// Unregisters a saga type
    /// </summary>
    /// <param name="sagaType">Saga type name</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Task representing the unregistration operation</returns>
    public async Task UnregisterSagaTypeAsync(string sagaType, CancellationToken cancellationToken = default)
    {
        if (_disposed)
            throw new ObjectDisposedException(nameof(RabbitMQSagaOrchestrator));

        if (string.IsNullOrEmpty(sagaType))
            return;

        _sagaFactories.TryRemove(sagaType, out _);
        
        _logger.LogInformation("Unregistered saga type: {SagaType}", sagaType);
        await Task.CompletedTask;
    }

    /// <summary>
    /// Starts the saga orchestrator
    /// </summary>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Task representing the start operation</returns>
    public async Task StartAsync(CancellationToken cancellationToken = default)
    {
        if (_disposed)
            throw new ObjectDisposedException(nameof(RabbitMQSagaOrchestrator));

        if (_isStarted)
            return;

        try
        {
            // Initialize RabbitMQ exchanges and queues for saga management
            await InitializeSagaInfrastructureAsync(cancellationToken);

            // Start recovery process if enabled
            if (Settings.EnableRecovery)
            {
                await RecoverSagasAsync(cancellationToken);
            }

            _isStarted = true;
            _logger.LogInformation("Saga orchestrator started");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to start saga orchestrator");
            throw;
        }
    }

    /// <summary>
    /// Stops the saga orchestrator
    /// </summary>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Task representing the stop operation</returns>
    public async Task StopAsync(CancellationToken cancellationToken = default)
    {
        if (_disposed || !_isStarted)
            return;

        try
        {
            // Gracefully stop all active sagas
            var tasks = new List<Task>();
            foreach (var saga in _activeSagas.Values)
            {
                tasks.Add(PersistSagaStateAsync(saga, cancellationToken));
            }

            if (tasks.Count > 0)
            {
                await Task.WhenAll(tasks);
            }

            _isStarted = false;
            _logger.LogInformation("Saga orchestrator stopped");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to stop saga orchestrator gracefully");
            throw;
        }
    }

    /// <summary>
    /// Subscribes to saga events
    /// </summary>
    /// <param name="saga">Saga instance</param>
    private void SubscribeToSagaEvents(ISaga saga)
    {
        saga.StateChanged += (sender, args) => SagaStateChanged?.Invoke(this, args);
        saga.StepCompleted += (sender, args) => SagaStepCompleted?.Invoke(this, args);
        saga.StepFailed += (sender, args) => SagaStepFailed?.Invoke(this, args);
        saga.CompensationTriggered += (sender, args) => SagaCompensationTriggered?.Invoke(this, args);
    }

    /// <summary>
    /// Handles an event for a specific saga
    /// </summary>
    /// <param name="saga">Saga instance</param>
    /// <param name="event">Event to handle</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Task representing the operation</returns>
    private async Task HandleEventForSagaAsync(ISaga saga, IEvent @event, CancellationToken cancellationToken)
    {
        try
        {
            await saga.HandleEventAsync(@event, cancellationToken);
            await PersistSagaStateAsync(saga, cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to handle event {EventType} for saga {SagaId}", @event.GetType().Name, saga.SagaId);
            throw;
        }
    }

    /// <summary>
    /// Persists saga state to storage
    /// </summary>
    /// <param name="saga">Saga instance</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Task representing the operation</returns>
    private async Task PersistSagaStateAsync(ISaga saga, CancellationToken cancellationToken)
    {
        if (!Settings.EnablePersistence)
            return;

        try
        {
            if (!_connectionManager.IsConnected)
            {
                await _connectionManager.ConnectAsync(cancellationToken);
            }

            var channel = await _connectionManager.GetChannelAsync(cancellationToken);
            var rabbitChannel = ((RabbitMQChannel)channel).GetNativeChannel();

            var sagaState = new SagaPersistenceModel
            {
                SagaId = saga.SagaId,
                SagaType = saga.SagaType,
                State = saga.State,
                Version = saga.Version,
                CorrelationId = saga.CorrelationId,
                Context = saga.Context,
                Data = saga.GetSagaData(),
                Timestamp = DateTimeOffset.UtcNow
            };

            var messageBody = JsonSerializer.SerializeToUtf8Bytes(sagaState);
            var properties = rabbitChannel.CreateBasicProperties();
            properties.Persistent = true;
            properties.MessageId = saga.SagaId;
            properties.Type = "SagaState";

            rabbitChannel.BasicPublish(
                exchange: "saga-state-exchange",
                routingKey: $"saga.state.{saga.SagaType}",
                body: messageBody);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to persist saga state for {SagaId}", saga.SagaId);
        }
    }

    /// <summary>
    /// Restores saga from persistence
    /// </summary>
    /// <param name="sagaId">Saga identifier</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Restored saga instance or null</returns>
    private async Task<ISaga?> RestoreSagaFromPersistenceAsync(string sagaId, CancellationToken cancellationToken)
    {
        if (!Settings.EnablePersistence)
            return null;

        try
        {
            if (!_connectionManager.IsConnected)
            {
                await _connectionManager.ConnectAsync(cancellationToken);
            }

            var channel = await _connectionManager.GetChannelAsync(cancellationToken);
            var rabbitChannel = ((RabbitMQChannel)channel).GetNativeChannel();

            // This is a simplified implementation
            // In a real scenario, you'd query a persistent store
            var result = rabbitChannel.BasicGet($"saga-state-queue-{sagaId}", autoAck: true);
            if (result != null)
            {
                var sagaState = JsonSerializer.Deserialize<SagaPersistenceModel>(result.Body.ToArray());
                if (sagaState != null && _sagaFactories.TryGetValue(sagaState.SagaType, out var factory))
                {
                    var saga = factory(sagaState.SagaId);
                    await saga.RestoreAsync(sagaState.SagaId, cancellationToken);
                    return saga;
                }
            }

            return null;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to restore saga {SagaId} from persistence", sagaId);
            return null;
        }
    }

    /// <summary>
    /// Initializes RabbitMQ infrastructure for saga management
    /// </summary>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Task representing the operation</returns>
    private async Task InitializeSagaInfrastructureAsync(CancellationToken cancellationToken)
    {
        if (!_connectionManager.IsConnected)
        {
            await _connectionManager.ConnectAsync(cancellationToken);
        }

        var channel = await _connectionManager.GetChannelAsync(cancellationToken);
        var rabbitChannel = ((RabbitMQChannel)channel).GetNativeChannel();

        // Declare saga state exchange
        rabbitChannel.ExchangeDeclare("saga-state-exchange", "topic", durable: true);

        // Declare saga event exchange
        rabbitChannel.ExchangeDeclare("saga-event-exchange", "topic", durable: true);

        _logger.LogInformation("Initialized saga infrastructure");
    }

    /// <summary>
    /// Recovers sagas from persistence
    /// </summary>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Task representing the operation</returns>
    private async Task RecoverSagasAsync(CancellationToken cancellationToken)
    {
        // This is a simplified implementation
        // In a real scenario, you'd query persistent storage for incomplete sagas
        _logger.LogInformation("Saga recovery completed");
        await Task.CompletedTask;
    }

    /// <summary>
    /// Checks for saga timeouts
    /// </summary>
    /// <param name="state">Timer state</param>
    private void CheckSagaTimeouts(object? state)
    {
        if (_disposed || !_isStarted)
            return;

        try
        {
            var timedOutSagas = new List<ISaga>();
            var now = DateTimeOffset.UtcNow;

            foreach (var saga in _activeSagas.Values)
            {
                var timeout = saga.GetTimeout();
                if (timeout.HasValue && now - saga.Context.CreatedAt > timeout.Value)
                {
                    timedOutSagas.Add(saga);
                }
            }

            foreach (var saga in timedOutSagas)
            {
                _ = Task.Run(async () =>
                {
                    try
                    {
                        await saga.HandleTimeoutAsync(CancellationToken.None);
                        _activeSagas.TryRemove(saga.SagaId, out _);
                        _logger.LogInformation("Saga {SagaId} timed out and was removed", saga.SagaId);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Failed to handle timeout for saga {SagaId}", saga.SagaId);
                    }
                });
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error checking saga timeouts");
        }
    }

    /// <summary>
    /// Disposes the saga orchestrator
    /// </summary>
    public void Dispose()
    {
        if (_disposed)
            return;

        lock (_lock)
        {
            if (_disposed)
                return;

            _disposed = true;
            _timeoutTimer?.Dispose();

            // Dispose all active sagas
            foreach (var saga in _activeSagas.Values)
            {
                saga.Dispose();
            }
            _activeSagas.Clear();
            _sagaFactories.Clear();
        }
    }
}

/// <summary>
/// Saga persistence model
/// </summary>
public class SagaPersistenceModel
{
    public string SagaId { get; set; } = string.Empty;
    public string SagaType { get; set; } = string.Empty;
    public SagaState State { get; set; }
    public long Version { get; set; }
    public string? CorrelationId { get; set; }
    public SagaContext Context { get; set; } = new();
    public Dictionary<string, object> Data { get; set; } = new();
    public DateTimeOffset Timestamp { get; set; }
} 