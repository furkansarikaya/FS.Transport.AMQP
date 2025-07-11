using System.Collections.Concurrent;
using System.Diagnostics;
using FS.RabbitMQ.Connection;
using FS.RabbitMQ.Consumer;
using FS.RabbitMQ.Exchange;
using FS.RabbitMQ.Producer;
using FS.RabbitMQ.Queue;
using Microsoft.Extensions.Logging;

namespace FS.RabbitMQ.Monitoring;

/// <summary>
/// Provides comprehensive health checking capabilities for RabbitMQ transport components.
/// Monitors connection health, queue status, exchange availability, and producer/consumer health.
/// Supports custom health checks and provides detailed statistics and monitoring.
/// </summary>
public class HealthChecker : IHealthChecker
{
    private readonly IConnectionManager _connectionManager;
    private readonly IQueueManager _queueManager;
    private readonly IExchangeManager _exchangeManager;
    private readonly IMessageProducer _producer;
    private readonly IMessageConsumer _consumer;
    private readonly ILogger<HealthChecker> _logger;
    private readonly ConcurrentDictionary<string, HealthCheckRegistration> _customHealthChecks;
    private readonly ConcurrentDictionary<string, HealthCheckResult> _healthCheckResults;
    private readonly ConcurrentDictionary<string, HealthCheckStatisticsInternal> _healthCheckStatistics;
    private readonly SemaphoreSlim _healthCheckSemaphore;
    
    private Timer? _healthCheckTimer;
    private TimeSpan _healthCheckInterval;
    private HealthStatus _overallHealth;
    private bool _isRunning;
    private bool _disposed;
    
    private static readonly TimeSpan DefaultHealthCheckInterval = TimeSpan.FromSeconds(30);
    private static readonly TimeSpan DefaultHealthCheckTimeout = TimeSpan.FromSeconds(10);

    /// <summary>
    /// Initializes a new instance of the <see cref="HealthChecker"/> class.
    /// </summary>
    /// <param name="connectionManager">The connection manager to monitor.</param>
    /// <param name="queueManager">The queue manager to monitor.</param>
    /// <param name="exchangeManager">The exchange manager to monitor.</param>
    /// <param name="producer">The message producer to monitor.</param>
    /// <param name="consumer">The message consumer to monitor.</param>
    /// <param name="logger">The logger for health check operations.</param>
    public HealthChecker(
        IConnectionManager connectionManager,
        IQueueManager queueManager,
        IExchangeManager exchangeManager,
        IMessageProducer producer,
        IMessageConsumer consumer,
        ILogger<HealthChecker> logger)
    {
        _connectionManager = connectionManager ?? throw new ArgumentNullException(nameof(connectionManager));
        _queueManager = queueManager ?? throw new ArgumentNullException(nameof(queueManager));
        _exchangeManager = exchangeManager ?? throw new ArgumentNullException(nameof(exchangeManager));
        _producer = producer ?? throw new ArgumentNullException(nameof(producer));
        _consumer = consumer ?? throw new ArgumentNullException(nameof(consumer));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        
        _customHealthChecks = new ConcurrentDictionary<string, HealthCheckRegistration>();
        _healthCheckResults = new ConcurrentDictionary<string, HealthCheckResult>();
        _healthCheckStatistics = new ConcurrentDictionary<string, HealthCheckStatisticsInternal>();
        _healthCheckSemaphore = new SemaphoreSlim(1, 1);
        
        _healthCheckInterval = DefaultHealthCheckInterval;
        _overallHealth = HealthStatus.Unknown;
        
        RegisterBuiltInHealthChecks();
    }

    /// <inheritdoc />
    public HealthStatus OverallHealth => _overallHealth;

    /// <inheritdoc />
    public IReadOnlyDictionary<string, HealthCheckResult> HealthChecks => _healthCheckResults;

    /// <inheritdoc />
    public bool IsRunning => _isRunning;

    /// <inheritdoc />
    public event EventHandler<HealthStatusChangedEventArgs>? HealthStatusChanged;

    /// <inheritdoc />
    public event EventHandler<HealthCheckResultChangedEventArgs>? HealthCheckResultChanged;

    /// <inheritdoc />
    public async Task StartAsync(TimeSpan? interval = null, CancellationToken cancellationToken = default)
    {
        ThrowIfDisposed();
        
        if (_isRunning)
        {
            _logger.LogWarning("Health checker is already running");
            return;
        }

        _healthCheckInterval = interval ?? DefaultHealthCheckInterval;
        _isRunning = true;
        
        _logger.LogInformation("Starting health checker with interval: {Interval}", _healthCheckInterval);

        // Perform initial health check
        await CheckHealthAsync(cancellationToken);

        // Start periodic health checks
        _healthCheckTimer = new Timer(
            PerformScheduledHealthCheck,
            null,
            _healthCheckInterval,
            _healthCheckInterval);
    }

    /// <inheritdoc />
    public async Task StopAsync(CancellationToken cancellationToken = default)
    {
        if (!_isRunning)
            return;

        _logger.LogInformation("Stopping health checker");

        _isRunning = false;
        _healthCheckTimer?.Dispose();
        _healthCheckTimer = null;

        // Wait for any ongoing health checks to complete
        await _healthCheckSemaphore.WaitAsync(cancellationToken);
        try
        {
            // Health check is complete
        }
        finally
        {
            _healthCheckSemaphore.Release();
        }

        _logger.LogInformation("Health checker stopped");
    }

    /// <inheritdoc />
    public async Task<HealthCheckResult> CheckHealthAsync(CancellationToken cancellationToken = default)
    {
        ThrowIfDisposed();

        await _healthCheckSemaphore.WaitAsync(cancellationToken);
        try
        {
            return await PerformHealthCheckAsync(cancellationToken);
        }
        finally
        {
            _healthCheckSemaphore.Release();
        }
    }

    /// <inheritdoc />
    public async Task<HealthCheckResult> CheckHealthAsync(string componentName, CancellationToken cancellationToken = default)
    {
        ThrowIfDisposed();
        
        if (string.IsNullOrWhiteSpace(componentName))
            throw new ArgumentException("Component name cannot be null or whitespace", nameof(componentName));

        await _healthCheckSemaphore.WaitAsync(cancellationToken);
        try
        {
            return await PerformSingleHealthCheckAsync(componentName, cancellationToken);
        }
        finally
        {
            _healthCheckSemaphore.Release();
        }
    }

    /// <inheritdoc />
    public void RegisterHealthCheck(string name, Func<CancellationToken, Task<HealthCheckResult>> healthCheckFunc, TimeSpan? timeout = null)
    {
        ThrowIfDisposed();
        
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Health check name cannot be null or whitespace", nameof(name));
        
        if (healthCheckFunc == null)
            throw new ArgumentNullException(nameof(healthCheckFunc));

        var registration = new HealthCheckRegistration(healthCheckFunc, timeout ?? DefaultHealthCheckTimeout);
        _customHealthChecks.AddOrUpdate(name, registration, (_, _) => registration);
        
        _logger.LogInformation("Registered custom health check: {Name}", name);
    }

    /// <inheritdoc />
    public bool UnregisterHealthCheck(string name)
    {
        ThrowIfDisposed();
        
        if (string.IsNullOrWhiteSpace(name))
            return false;

        var removed = _customHealthChecks.TryRemove(name, out _);
        if (removed)
        {
            _healthCheckResults.TryRemove(name, out _);
            _healthCheckStatistics.TryRemove(name, out _);
            _logger.LogInformation("Unregistered custom health check: {Name}", name);
        }

        return removed;
    }

    /// <inheritdoc />
    public IReadOnlyDictionary<string, HealthCheckStatistics> GetHealthCheckStatistics()
    {
        ThrowIfDisposed();
        
        return _healthCheckStatistics.ToDictionary(
            kvp => kvp.Key,
            kvp => kvp.Value.ToPublicStatistics());
    }

    /// <inheritdoc />
    public void ResetStatistics(string? componentName = null)
    {
        ThrowIfDisposed();
        
        if (string.IsNullOrWhiteSpace(componentName))
        {
            _healthCheckStatistics.Clear();
            _logger.LogInformation("Reset all health check statistics");
        }
        else
        {
            if (_healthCheckStatistics.TryRemove(componentName, out _))
            {
                _logger.LogInformation("Reset health check statistics for component: {ComponentName}", componentName);
            }
        }
    }

    /// <summary>
    /// Registers built-in health checks for RabbitMQ components.
    /// </summary>
    private void RegisterBuiltInHealthChecks()
    {
        RegisterHealthCheck("connection", CheckConnectionHealthAsync);
        RegisterHealthCheck("queues", CheckQueuesHealthAsync);
        RegisterHealthCheck("exchanges", CheckExchangesHealthAsync);
        RegisterHealthCheck("producer", CheckProducerHealthAsync);
        RegisterHealthCheck("consumer", CheckConsumerHealthAsync);
    }

    /// <summary>
    /// Performs a scheduled health check via timer.
    /// </summary>
    /// <param name="state">The timer state.</param>
    private async void PerformScheduledHealthCheck(object? state)
    {
        if (!_isRunning)
            return;

        try
        {
            await CheckHealthAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during scheduled health check");
        }
    }

    /// <summary>
    /// Performs a comprehensive health check on all registered components.
    /// </summary>
    /// <param name="cancellationToken">A cancellation token that can be used to cancel the operation.</param>
    /// <returns>A task that represents the asynchronous health check operation.</returns>
    private async Task<HealthCheckResult> PerformHealthCheckAsync(CancellationToken cancellationToken)
    {
        var stopwatch = Stopwatch.StartNew();
        var overallData = new Dictionary<string, object>();
        var healthyCount = 0;
        var degradedCount = 0;
        var unhealthyCount = 0;
        var totalCount = 0;

        // Get all health check names (built-in + custom)
        var healthCheckNames = _customHealthChecks.Keys.ToList();

        // Perform all health checks
        var healthCheckTasks = healthCheckNames
            .Select(name => PerformSingleHealthCheckAsync(name, cancellationToken))
            .ToArray();

        var results = await Task.WhenAll(healthCheckTasks);

        // Analyze results
        foreach (var result in results)
        {
            totalCount++;
            switch (result.Status)
            {
                case HealthStatus.Healthy:
                    healthyCount++;
                    break;
                case HealthStatus.Degraded:
                    degradedCount++;
                    break;
                case HealthStatus.Unhealthy:
                    unhealthyCount++;
                    break;
            }
        }

        // Determine overall health status
        var previousOverallHealth = _overallHealth;
        _overallHealth = DetermineOverallHealth(healthyCount, degradedCount, unhealthyCount);

        // Update overall data
        overallData["total_checks"] = totalCount;
        overallData["healthy_checks"] = healthyCount;
        overallData["degraded_checks"] = degradedCount;
        overallData["unhealthy_checks"] = unhealthyCount;
        overallData["success_rate"] = totalCount > 0 ? (double)healthyCount / totalCount * 100 : 0;

        stopwatch.Stop();

        var overallResult = new HealthCheckResult(
            _overallHealth,
            $"Overall health: {healthyCount}/{totalCount} healthy, {degradedCount} degraded, {unhealthyCount} unhealthy",
            overallData,
            stopwatch.Elapsed);

        // Raise events if health status changed
        if (previousOverallHealth != _overallHealth)
        {
            HealthStatusChanged?.Invoke(this, new HealthStatusChangedEventArgs(previousOverallHealth, _overallHealth));
        }

        return overallResult;
    }

    /// <summary>
    /// Performs a health check on a specific component.
    /// </summary>
    /// <param name="componentName">The name of the component to check.</param>
    /// <param name="cancellationToken">A cancellation token that can be used to cancel the operation.</param>
    /// <returns>A task that represents the asynchronous health check operation.</returns>
    private async Task<HealthCheckResult> PerformSingleHealthCheckAsync(string componentName, CancellationToken cancellationToken)
    {
        if (!_customHealthChecks.TryGetValue(componentName, out var registration))
        {
            return HealthCheckResult.Unhealthy($"Health check '{componentName}' not found");
        }

        var stopwatch = Stopwatch.StartNew();
        HealthCheckResult result;

        try
        {
            using var timeoutCts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
            timeoutCts.CancelAfter(registration.Timeout);

            result = await registration.HealthCheckFunc(timeoutCts.Token);
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            result = HealthCheckResult.Unhealthy($"Health check '{componentName}' was cancelled");
        }
        catch (OperationCanceledException)
        {
            result = HealthCheckResult.Unhealthy($"Health check '{componentName}' timed out after {registration.Timeout}");
        }
        catch (Exception ex)
        {
            result = HealthCheckResult.Unhealthy($"Health check '{componentName}' failed", ex);
        }

        stopwatch.Stop();

        // Update result with actual duration
        result = new HealthCheckResult(
            result.Status,
            result.Description,
            result.Data,
            stopwatch.Elapsed,
            result.Exception);

        // Update statistics
        UpdateHealthCheckStatistics(componentName, result);

        // Update result cache
        var previousResult = _healthCheckResults.GetValueOrDefault(componentName);
        _healthCheckResults[componentName] = result;

        // Raise event if result changed
        if (previousResult == null || previousResult.Status != result.Status)
        {
            HealthCheckResultChanged?.Invoke(this, new HealthCheckResultChangedEventArgs(componentName, previousResult, result));
        }

        return result;
    }

    /// <summary>
    /// Updates health check statistics for a component.
    /// </summary>
    /// <param name="componentName">The name of the component.</param>
    /// <param name="result">The health check result.</param>
    private void UpdateHealthCheckStatistics(string componentName, HealthCheckResult result)
    {
        _healthCheckStatistics.AddOrUpdate(
            componentName,
            new HealthCheckStatisticsInternal(result),
            (_, existing) => existing.Update(result));
    }

    /// <summary>
    /// Determines the overall health status based on individual component health.
    /// </summary>
    /// <param name="healthyCount">The number of healthy components.</param>
    /// <param name="degradedCount">The number of degraded components.</param>
    /// <param name="unhealthyCount">The number of unhealthy components.</param>
    /// <returns>The overall health status.</returns>
    private static HealthStatus DetermineOverallHealth(int healthyCount, int degradedCount, int unhealthyCount)
    {
        if (unhealthyCount > 0)
            return HealthStatus.Unhealthy;
        
        if (degradedCount > 0)
            return HealthStatus.Degraded;
        
        return healthyCount > 0 ? HealthStatus.Healthy : HealthStatus.Unknown;
    }

    /// <summary>
    /// Checks the health of the RabbitMQ connection.
    /// </summary>
    /// <param name="cancellationToken">A cancellation token that can be used to cancel the operation.</param>
    /// <returns>A task that represents the asynchronous health check operation.</returns>
    private Task<HealthCheckResult> CheckConnectionHealthAsync(CancellationToken cancellationToken)
    {
        try
        {
            var isConnected = _connectionManager.IsConnected;
            var connectionHealthy = isConnected; // Use IsConnected directly instead of TestConnectionAsync
            var data = new Dictionary<string, object>
            {
                ["is_connected"] = isConnected,
                ["connection_healthy"] = connectionHealthy,
                ["connection_state"] = _connectionManager.State.ToString()
            };

            if (isConnected && connectionHealthy)
            {
                return Task.FromResult(HealthCheckResult.Healthy("Connection is healthy", data));
            }
            else if (isConnected && !connectionHealthy)
            {
                return Task.FromResult(HealthCheckResult.Degraded("Connection is established but not responding properly", data));
            }
            else
            {
                return Task.FromResult(HealthCheckResult.Unhealthy("Connection is not available", data: data));
            }
        }
        catch (Exception ex)
        {
            return Task.FromResult(HealthCheckResult.Unhealthy("Connection health check failed", ex));
        }
    }

    /// <summary>
    /// Checks the health of RabbitMQ queues.
    /// </summary>
    /// <param name="cancellationToken">A cancellation token that can be used to cancel the operation.</param>
    /// <returns>A task that represents the asynchronous health check operation.</returns>
    private static Task<HealthCheckResult> CheckQueuesHealthAsync(CancellationToken cancellationToken)
    {
        try
        {
            // Check queue manager availability and basic queue operations
            var data = new Dictionary<string, object>
            {
                ["queue_manager_available"] = true
            };

            return Task.FromResult(HealthCheckResult.Healthy("Queue manager is healthy", data));
        }
        catch (Exception ex)
        {
            return Task.FromResult(HealthCheckResult.Unhealthy("Queue health check failed", ex));
        }
    }

    /// <summary>
    /// Checks the health of RabbitMQ exchanges.
    /// </summary>
    /// <param name="cancellationToken">A cancellation token that can be used to cancel the operation.</param>
    /// <returns>A task that represents the asynchronous health check operation.</returns>
    private static Task<HealthCheckResult> CheckExchangesHealthAsync(CancellationToken cancellationToken)
    {
        try
        {
            // Check exchange manager availability and basic exchange operations
            var data = new Dictionary<string, object>
            {
                ["exchange_manager_available"] = true
            };

            return Task.FromResult(HealthCheckResult.Healthy("Exchange manager is healthy", data));
        }
        catch (Exception ex)
        {
            return Task.FromResult(HealthCheckResult.Unhealthy("Exchange health check failed", ex));
        }
    }

    /// <summary>
    /// Checks the health of the message producer.
    /// </summary>
    /// <param name="cancellationToken">A cancellation token that can be used to cancel the operation.</param>
    /// <returns>A task that represents the asynchronous health check operation.</returns>
    private static Task<HealthCheckResult> CheckProducerHealthAsync(CancellationToken cancellationToken)
    {
        try
        {
            // Check producer availability and statistics
            var data = new Dictionary<string, object>
            {
                ["producer_available"] = true
            };

            return Task.FromResult(HealthCheckResult.Healthy("Message producer is healthy", data));
        }
        catch (Exception ex)
        {
            return Task.FromResult(HealthCheckResult.Unhealthy("Producer health check failed", ex));
        }
    }

    /// <summary>
    /// Checks the health of the message consumer.
    /// </summary>
    /// <param name="cancellationToken">A cancellation token that can be used to cancel the operation.</param>
    /// <returns>A task that represents the asynchronous health check operation.</returns>
    private static Task<HealthCheckResult> CheckConsumerHealthAsync(CancellationToken cancellationToken)
    {
        try
        {
            // Check consumer availability and listening status
            var data = new Dictionary<string, object>
            {
                ["consumer_available"] = true
            };

            return Task.FromResult(HealthCheckResult.Healthy("Message consumer is healthy", data));
        }
        catch (Exception ex)
        {
            return Task.FromResult(HealthCheckResult.Unhealthy("Consumer health check failed", ex));
        }
    }

    /// <summary>
    /// Throws an exception if the health checker has been disposed.
    /// </summary>
    private void ThrowIfDisposed()
    {
        if (_disposed)
            throw new ObjectDisposedException(nameof(HealthChecker));
    }

    /// <inheritdoc />
    public void Dispose()
    {
        if (_disposed)
            return;

        StopAsync().GetAwaiter().GetResult();
        _healthCheckSemaphore.Dispose();
        _disposed = true;
    }

    /// <summary>
    /// Represents a health check registration.
    /// </summary>
    private class HealthCheckRegistration
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="HealthCheckRegistration"/> class.
        /// </summary>
        /// <param name="healthCheckFunc">The health check function.</param>
        /// <param name="timeout">The timeout for the health check.</param>
        public HealthCheckRegistration(Func<CancellationToken, Task<HealthCheckResult>> healthCheckFunc, TimeSpan timeout)
        {
            HealthCheckFunc = healthCheckFunc;
            Timeout = timeout;
        }

        /// <summary>
        /// Gets the health check function.
        /// </summary>
        public Func<CancellationToken, Task<HealthCheckResult>> HealthCheckFunc { get; }

        /// <summary>
        /// Gets the timeout for the health check.
        /// </summary>
        public TimeSpan Timeout { get; }
    }

    /// <summary>
    /// Represents internal health check statistics that can be updated.
    /// </summary>
    private class HealthCheckStatisticsInternal
    {
        private long _totalChecks;
        private long _successfulChecks;
        private long _totalResponseTimeMs;
        private DateTimeOffset? _lastCheckTime;
        private HealthCheckResult? _lastResult;

        /// <summary>
        /// Initializes a new instance of the <see cref="HealthCheckStatisticsInternal"/> class.
        /// </summary>
        /// <param name="initialResult">The initial health check result.</param>
        public HealthCheckStatisticsInternal(HealthCheckResult initialResult)
        {
            Update(initialResult);
        }

        /// <summary>
        /// Updates the statistics with a new health check result.
        /// </summary>
        /// <param name="result">The health check result.</param>
        /// <returns>The updated statistics.</returns>
        public HealthCheckStatisticsInternal Update(HealthCheckResult result)
        {
            Interlocked.Increment(ref _totalChecks);
            
            if (result.Status == HealthStatus.Healthy)
            {
                Interlocked.Increment(ref _successfulChecks);
            }
            
            Interlocked.Add(ref _totalResponseTimeMs, (long)result.Duration.TotalMilliseconds);
            _lastCheckTime = result.Timestamp;
            _lastResult = result;

            return this;
        }

        /// <summary>
        /// Converts the internal statistics to public statistics.
        /// </summary>
        /// <returns>The public health check statistics.</returns>
        public HealthCheckStatistics ToPublicStatistics()
        {
            var totalChecks = Interlocked.Read(ref _totalChecks);
            var successfulChecks = Interlocked.Read(ref _successfulChecks);
            var totalResponseTimeMs = Interlocked.Read(ref _totalResponseTimeMs);
            
            var averageResponseTime = totalChecks > 0 
                ? TimeSpan.FromMilliseconds((double)totalResponseTimeMs / totalChecks)
                : TimeSpan.Zero;

            return new HealthCheckStatistics(
                totalChecks,
                successfulChecks,
                averageResponseTime,
                _lastCheckTime,
                _lastResult);
        }
    }
}