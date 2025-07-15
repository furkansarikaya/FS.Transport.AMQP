using FS.StreamFlow.Core.Features.Messaging.Interfaces;
using FS.StreamFlow.Core.Features.Messaging.Models;
using Microsoft.Extensions.Logging;
using System.Collections.Concurrent;
using System.Timers;

namespace FS.StreamFlow.RabbitMQ.Features.HealthCheck;

/// <summary>
/// RabbitMQ implementation of health checker providing comprehensive health monitoring
/// for all RabbitMQ components including connection, channels, producers, consumers, and event bus
/// </summary>
public class RabbitMQHealthChecker : IHealthChecker
{
    private readonly IConnectionManager _connectionManager;
    private readonly ILogger<RabbitMQHealthChecker> _logger;
    private readonly ConcurrentDictionary<string, RegisteredHealthCheck> _registeredChecks = new();
    private readonly HealthCheckStatistics _statistics = new();
    private readonly System.Timers.Timer _healthCheckTimer;
    private readonly object _lockObject = new();
    private HealthStatus _currentStatus = HealthStatus.Healthy;
    private readonly DateTimeOffset _startTime = DateTimeOffset.UtcNow;
    private volatile bool _isRunning = false;
    private volatile bool _disposed = false;

    /// <summary>
    /// Initializes a new instance of the RabbitMQHealthChecker class
    /// </summary>
    /// <param name="connectionManager">Connection manager for health checking</param>
    /// <param name="logger">Logger instance for diagnostic information</param>
    /// <exception cref="ArgumentNullException">Thrown when any parameter is null</exception>
    public RabbitMQHealthChecker(
        IConnectionManager connectionManager,
        ILogger<RabbitMQHealthChecker> logger)
    {
        _connectionManager = connectionManager ?? throw new ArgumentNullException(nameof(connectionManager));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        
        // Setup health check timer (default: 30 seconds)
        _healthCheckTimer = new System.Timers.Timer(TimeSpan.FromSeconds(30).TotalMilliseconds);
        _healthCheckTimer.Elapsed += OnHealthCheckTimerElapsed;
        _healthCheckTimer.AutoReset = true;
        
        // Register default health checks
        RegisterDefaultHealthChecks();
        
        _logger.LogInformation("RabbitMQ Health Checker initialized successfully");
    }

    /// <summary>
    /// Gets the current health status
    /// </summary>
    public HealthStatus CurrentStatus => _currentStatus;

    /// <summary>
    /// Gets health check statistics
    /// </summary>
    public HealthCheckStatistics Statistics => _statistics;

    /// <summary>
    /// Event raised when health status changes
    /// </summary>
    public event EventHandler<HealthStatusChangedEventArgs>? HealthStatusChanged;

    /// <summary>
    /// Event raised when a health check completes
    /// </summary>
    public event EventHandler<HealthCheckCompletedEventArgs>? HealthCheckCompleted;

    /// <summary>
    /// Starts the health checker
    /// </summary>
    /// <param name="cancellationToken">Cancellation token for operation cancellation</param>
    /// <returns>Task representing the start operation</returns>
    /// <exception cref="ObjectDisposedException">Thrown when the health checker has been disposed</exception>
    public async Task StartAsync(CancellationToken cancellationToken = default)
    {
        ThrowIfDisposed();
        
        if (_isRunning)
            return;

        try
        {
            _logger.LogInformation("Starting RabbitMQ Health Checker");
            
            lock (_lockObject)
            {
                _isRunning = true;
                _healthCheckTimer.Start();
            }

            // Perform initial health check
            await CheckHealthAsync(cancellationToken);
            
            _logger.LogInformation("RabbitMQ Health Checker started successfully");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to start RabbitMQ Health Checker");
            throw;
        }
    }

    /// <summary>
    /// Stops the health checker
    /// </summary>
    /// <param name="cancellationToken">Cancellation token for operation cancellation</param>
    /// <returns>Task representing the stop operation</returns>
    /// <exception cref="ObjectDisposedException">Thrown when the health checker has been disposed</exception>
    public async Task StopAsync(CancellationToken cancellationToken = default)
    {
        ThrowIfDisposed();
        
        if (!_isRunning)
            return;

        try
        {
            _logger.LogInformation("Stopping RabbitMQ Health Checker");
            
            lock (_lockObject)
            {
                _isRunning = false;
                _healthCheckTimer.Stop();
            }

            _logger.LogInformation("RabbitMQ Health Checker stopped successfully");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to stop RabbitMQ Health Checker");
            throw;
        }
    }

    /// <summary>
    /// Performs a health check
    /// </summary>
    /// <param name="cancellationToken">Cancellation token for operation cancellation</param>
    /// <returns>Health check result</returns>
    /// <exception cref="ObjectDisposedException">Thrown when the health checker has been disposed</exception>
    public async Task<HealthCheckResult> CheckHealthAsync(CancellationToken cancellationToken = default)
    {
        ThrowIfDisposed();
        
        var startTime = DateTimeOffset.UtcNow;
        
        try
        {
            _logger.LogDebug("Performing overall health check");
            
            var healthCheckTasks = _registeredChecks.Values
                .Select(check => PerformHealthCheck(check, cancellationToken))
                .ToArray();

            var results = await Task.WhenAll(healthCheckTasks);
            
            // Determine overall health status
            var overallStatus = DetermineOverallStatus(results);
            var duration = DateTimeOffset.UtcNow - startTime;
            
            var overallResult = new HealthCheckResult
            {
                Status = overallStatus,
                Description = $"Overall health check completed with {results.Length} checks",
                Duration = duration,
                Data = results.ToDictionary(r => r.Tags?.FirstOrDefault() ?? "unknown", r => (object)r.Status)
            };

            await UpdateHealthStatus(overallStatus);
            await UpdateStatistics(overallResult);
            
            HealthCheckCompleted?.Invoke(this, new HealthCheckCompletedEventArgs(overallResult));
            
            _logger.LogDebug("Overall health check completed: {Status} in {Duration}ms", 
                overallStatus, duration.TotalMilliseconds);
            
            return overallResult;
        }
        catch (Exception ex)
        {
            var duration = DateTimeOffset.UtcNow - startTime;
            var errorResult = HealthCheckResult.Unhealthy(
                "Health check failed with exception", 
                ex, 
                new Dictionary<string, object> { { "duration", duration.TotalMilliseconds } });
            
            await UpdateHealthStatus(HealthStatus.Unhealthy);
            await UpdateStatistics(errorResult);
            
            _logger.LogError(ex, "Health check failed");
            return errorResult;
        }
    }

    /// <summary>
    /// Performs a health check for a specific component
    /// </summary>
    /// <param name="componentName">Name of the component to check</param>
    /// <param name="cancellationToken">Cancellation token for operation cancellation</param>
    /// <returns>Health check result</returns>
    /// <exception cref="ArgumentException">Thrown when componentName is null or empty</exception>
    /// <exception cref="ObjectDisposedException">Thrown when the health checker has been disposed</exception>
    public async Task<HealthCheckResult> CheckHealthAsync(string componentName, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(componentName))
            throw new ArgumentException("Component name cannot be null or empty", nameof(componentName));

        ThrowIfDisposed();
        
        if (!_registeredChecks.TryGetValue(componentName, out var healthCheck))
        {
            return HealthCheckResult.Unhealthy(
                $"Component '{componentName}' is not registered",
                null,
                new Dictionary<string, object> { { "component", componentName } });
        }

        try
        {
            _logger.LogDebug("Performing health check for component: {ComponentName}", componentName);
            
            var result = await PerformHealthCheck(healthCheck, cancellationToken);
            
            HealthCheckCompleted?.Invoke(this, new HealthCheckCompletedEventArgs(result, componentName));
            
            _logger.LogDebug("Health check for component '{ComponentName}' completed: {Status}", 
                componentName, result.Status);
            
            return result;
        }
        catch (Exception ex)
        {
            var errorResult = HealthCheckResult.Unhealthy(
                $"Health check for component '{componentName}' failed with exception",
                ex,
                new Dictionary<string, object> { { "component", componentName } });
            
            _logger.LogError(ex, "Health check for component '{ComponentName}' failed", componentName);
            return errorResult;
        }
    }

    /// <summary>
    /// Gets the health status of all components
    /// </summary>
    /// <param name="cancellationToken">Cancellation token for operation cancellation</param>
    /// <returns>Dictionary of component health statuses</returns>
    /// <exception cref="ObjectDisposedException">Thrown when the health checker has been disposed</exception>
    public async Task<Dictionary<string, HealthCheckResult>> GetAllHealthStatusesAsync(CancellationToken cancellationToken = default)
    {
        ThrowIfDisposed();
        
        try
        {
            _logger.LogDebug("Getting all health statuses");
            
            var healthCheckTasks = _registeredChecks.Select(kvp => 
                PerformHealthCheck(kvp.Value, cancellationToken)
                    .ContinueWith(task => new { Name = kvp.Key, Result = task.Result }, cancellationToken))
                .ToArray();

            var results = await Task.WhenAll(healthCheckTasks);
            
            var statusDictionary = results.ToDictionary(r => r.Name, r => r.Result);
            
            _logger.LogDebug("Retrieved health statuses for {Count} components", statusDictionary.Count);
            
            return statusDictionary;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get all health statuses");
            throw;
        }
    }

    /// <summary>
    /// Registers a health check for a component
    /// </summary>
    /// <param name="name">Component name</param>
    /// <param name="healthCheckFunc">Health check function</param>
    /// <param name="interval">Check interval</param>
    /// <param name="tags">Optional tags</param>
    /// <returns>Task representing the registration</returns>
    /// <exception cref="ArgumentException">Thrown when name is null or empty</exception>
    /// <exception cref="ArgumentNullException">Thrown when healthCheckFunc is null</exception>
    /// <exception cref="ObjectDisposedException">Thrown when the health checker has been disposed</exception>
    public async Task RegisterHealthCheckAsync(
        string name, 
        Func<CancellationToken, Task<HealthCheckResult>> healthCheckFunc, 
        TimeSpan interval, 
        IEnumerable<string>? tags = null)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Name cannot be null or empty", nameof(name));

        if (healthCheckFunc == null)
            throw new ArgumentNullException(nameof(healthCheckFunc));

        ThrowIfDisposed();
        
        try
        {
            var registeredCheck = new RegisteredHealthCheck
            {
                Name = name,
                HealthCheckFunc = healthCheckFunc,
                Interval = interval,
                Tags = tags?.ToArray() ?? Array.Empty<string>(),
                LastCheck = DateTimeOffset.UtcNow,
                LastResult = null
            };

            _registeredChecks.AddOrUpdate(name, registeredCheck, (_, _) => registeredCheck);
            
            _logger.LogInformation("Health check registered for component: {ComponentName}", name);
            
            await Task.CompletedTask;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to register health check for component: {ComponentName}", name);
            throw;
        }
    }

    /// <summary>
    /// Unregisters a health check for a component
    /// </summary>
    /// <param name="name">Component name</param>
    /// <param name="cancellationToken">Cancellation token for operation cancellation</param>
    /// <returns>Task representing the unregistration</returns>
    /// <exception cref="ArgumentException">Thrown when name is null or empty</exception>
    /// <exception cref="ObjectDisposedException">Thrown when the health checker has been disposed</exception>
    public async Task UnregisterHealthCheckAsync(string name, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Name cannot be null or empty", nameof(name));

        ThrowIfDisposed();
        
        try
        {
            _registeredChecks.TryRemove(name, out _);
            
            _logger.LogInformation("Health check unregistered for component: {ComponentName}", name);
            
            await Task.CompletedTask;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to unregister health check for component: {ComponentName}", name);
            throw;
        }
    }

    /// <summary>
    /// Registers default health checks for RabbitMQ components
    /// </summary>
    private void RegisterDefaultHealthChecks()
    {
        // Connection health check
        RegisterHealthCheckAsync(
            "connection",
            async cancellationToken => await CheckConnectionHealth(cancellationToken),
            TimeSpan.FromSeconds(15),
            new[] { "connection", "core" });

        // Channel health check
        RegisterHealthCheckAsync(
            "channel",
            async cancellationToken => await CheckChannelHealth(cancellationToken),
            TimeSpan.FromSeconds(30),
            new[] { "channel", "core" });

        _logger.LogDebug("Default health checks registered");
    }

    /// <summary>
    /// Checks connection health
    /// </summary>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Health check result</returns>
    private async Task<HealthCheckResult> CheckConnectionHealth(CancellationToken cancellationToken)
    {
        try
        {
            var startTime = DateTimeOffset.UtcNow;
            
            if (!_connectionManager.IsConnected)
            {
                return HealthCheckResult.Unhealthy(
                    "Connection is not available",
                    null,
                    new Dictionary<string, object> 
                    { 
                        { "state", _connectionManager.State.ToString() },
                        { "isConnected", false }
                    });
            }

            var healthResult = await _connectionManager.HealthCheckAsync(cancellationToken);
            var duration = DateTimeOffset.UtcNow - startTime;
            
            return new HealthCheckResult
            {
                Status = healthResult.Status,
                Description = healthResult.Description,
                Duration = duration,
                Data = new Dictionary<string, object>
                {
                    { "state", _connectionManager.State.ToString() },
                    { "isConnected", _connectionManager.IsConnected },
                    { "statistics", _connectionManager.Statistics }
                },
                Tags = new[] { "connection" }
            };
        }
        catch (Exception ex)
        {
            return HealthCheckResult.Unhealthy(
                "Connection health check failed with exception",
                ex,
                new Dictionary<string, object> { { "component", "connection" } });
        }
    }

    /// <summary>
    /// Checks channel health
    /// </summary>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Health check result</returns>
    private async Task<HealthCheckResult> CheckChannelHealth(CancellationToken cancellationToken)
    {
        try
        {
            var startTime = DateTimeOffset.UtcNow;
            
            if (!_connectionManager.IsConnected)
            {
                return HealthCheckResult.Degraded(
                    "Cannot check channel health - connection unavailable",
                    null,
                    new Dictionary<string, object> { { "reason", "connection_unavailable" } });
            }

            var channel = await _connectionManager.GetChannelAsync(cancellationToken);
            var isChannelHealthy = await channel.HealthCheckAsync(cancellationToken);
            await _connectionManager.ReturnChannelAsync(channel, cancellationToken);
            
            var duration = DateTimeOffset.UtcNow - startTime;
            
            return new HealthCheckResult
            {
                Status = isChannelHealthy ? HealthStatus.Healthy : HealthStatus.Degraded,
                Description = isChannelHealthy ? "Channel is healthy" : "Channel health check failed",
                Duration = duration,
                Data = new Dictionary<string, object>
                {
                    { "channelId", channel.Id },
                    { "channelState", channel.State.ToString() },
                    { "isOpen", channel.IsOpen }
                },
                Tags = new[] { "channel" }
            };
        }
        catch (Exception ex)
        {
            return HealthCheckResult.Unhealthy(
                "Channel health check failed with exception",
                ex,
                new Dictionary<string, object> { { "component", "channel" } });
        }
    }

    /// <summary>
    /// Performs health check for a registered component
    /// </summary>
    /// <param name="healthCheck">Registered health check</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Health check result</returns>
    private async Task<HealthCheckResult> PerformHealthCheck(RegisteredHealthCheck healthCheck, CancellationToken cancellationToken)
    {
        var startTime = DateTimeOffset.UtcNow;
        
        try
        {
            var result = await healthCheck.HealthCheckFunc(cancellationToken);
            
            result.Tags = healthCheck.Tags;
            result.Duration = DateTimeOffset.UtcNow - startTime;
            
            healthCheck.LastCheck = DateTimeOffset.UtcNow;
            healthCheck.LastResult = result;
            
            return result;
        }
        catch (Exception ex)
        {
            var errorResult = HealthCheckResult.Unhealthy(
                $"Health check for '{healthCheck.Name}' failed with exception",
                ex,
                new Dictionary<string, object> 
                { 
                    { "component", healthCheck.Name },
                    { "duration", (DateTimeOffset.UtcNow - startTime).TotalMilliseconds }
                });
            
            healthCheck.LastCheck = DateTimeOffset.UtcNow;
            healthCheck.LastResult = errorResult;
            
            return errorResult;
        }
    }

    /// <summary>
    /// Determines overall health status from individual check results
    /// </summary>
    /// <param name="results">Individual health check results</param>
    /// <returns>Overall health status</returns>
    private static HealthStatus DetermineOverallStatus(HealthCheckResult[] results)
    {
        if (results.Any(r => r.Status == HealthStatus.Unhealthy))
            return HealthStatus.Unhealthy;
        
        if (results.Any(r => r.Status == HealthStatus.Degraded))
            return HealthStatus.Degraded;
        
        return HealthStatus.Healthy;
    }

    /// <summary>
    /// Updates the current health status
    /// </summary>
    /// <param name="newStatus">New health status</param>
    private async Task UpdateHealthStatus(HealthStatus newStatus)
    {
        var previousStatus = _currentStatus;
        
        if (previousStatus != newStatus)
        {
            _currentStatus = newStatus;
            HealthStatusChanged?.Invoke(this, new HealthStatusChangedEventArgs(previousStatus, newStatus));
            
            _logger.LogInformation("Health status changed from {PreviousStatus} to {NewStatus}", 
                previousStatus, newStatus);
        }
        
        await Task.CompletedTask;
    }

    /// <summary>
    /// Updates health check statistics
    /// </summary>
    /// <param name="result">Health check result</param>
    private async Task UpdateStatistics(HealthCheckResult result)
    {
        _statistics.TotalHealthChecks++;
        _statistics.LastHealthCheckAt = DateTimeOffset.UtcNow;
        _statistics.CurrentUptime = DateTimeOffset.UtcNow - _startTime;
        
        // Update running average for check duration
        var totalDuration = _statistics.AverageCheckDuration.TotalMilliseconds * (_statistics.TotalHealthChecks - 1);
        _statistics.AverageCheckDuration = TimeSpan.FromMilliseconds((totalDuration + result.Duration.TotalMilliseconds) / _statistics.TotalHealthChecks);
        
        // Update status counts
        switch (result.Status)
        {
            case HealthStatus.Healthy:
                _statistics.HealthyChecks++;
                break;
            case HealthStatus.Degraded:
                _statistics.DegradedChecks++;
                break;
            case HealthStatus.Unhealthy:
                _statistics.UnhealthyChecks++;
                break;
        }
        
        _statistics.Timestamp = DateTimeOffset.UtcNow;
        
        await Task.CompletedTask;
    }

    /// <summary>
    /// Handles health check timer elapsed event
    /// </summary>
    /// <param name="sender">Timer sender</param>
    /// <param name="e">Elapsed event args</param>
    private async void OnHealthCheckTimerElapsed(object? sender, ElapsedEventArgs e)
    {
        if (!_isRunning || _disposed)
            return;

        try
        {
            await CheckHealthAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during periodic health check");
        }
    }

    /// <summary>
    /// Throws ObjectDisposedException if the health checker has been disposed
    /// </summary>
    /// <exception cref="ObjectDisposedException">Thrown when the health checker has been disposed</exception>
    private void ThrowIfDisposed()
    {
        if (_disposed)
            throw new ObjectDisposedException(nameof(RabbitMQHealthChecker));
    }

    /// <summary>
    /// Releases all resources used by the RabbitMQHealthChecker
    /// </summary>
    public void Dispose()
    {
        if (_disposed)
            return;

        lock (_lockObject)
        {
            if (_disposed)
                return;

            _disposed = true;

            try
            {
                _isRunning = false;
                _healthCheckTimer?.Stop();
                _healthCheckTimer?.Dispose();
                
                _registeredChecks.Clear();
                
                _logger.LogInformation("RabbitMQ Health Checker disposed successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during RabbitMQ Health Checker disposal");
            }
        }
    }
}

/// <summary>
/// Represents a registered health check
/// </summary>
internal class RegisteredHealthCheck
{
    public string Name { get; set; } = string.Empty;
    public Func<CancellationToken, Task<HealthCheckResult>> HealthCheckFunc { get; set; } = null!;
    public TimeSpan Interval { get; set; }
    public string[] Tags { get; set; } = Array.Empty<string>();
    public DateTimeOffset LastCheck { get; set; }
    public HealthCheckResult? LastResult { get; set; }
} 