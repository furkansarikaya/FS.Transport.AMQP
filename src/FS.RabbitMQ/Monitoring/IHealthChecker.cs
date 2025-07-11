namespace FS.RabbitMQ.Monitoring;

/// <summary>
/// Represents a health checker for monitoring the health status of RabbitMQ transport components.
/// Provides comprehensive health checking capabilities for connection, queue, exchange, and producer/consumer health.
/// </summary>
public interface IHealthChecker : IDisposable
{
    /// <summary>
    /// Gets the overall health status of the RabbitMQ transport.
    /// </summary>
    HealthStatus OverallHealth { get; }

    /// <summary>
    /// Gets the collection of individual health check results.
    /// </summary>
    IReadOnlyDictionary<string, HealthCheckResult> HealthChecks { get; }

    /// <summary>
    /// Gets a value indicating whether the health checker is currently running.
    /// </summary>
    bool IsRunning { get; }

    /// <summary>
    /// Occurs when the overall health status changes.
    /// </summary>
    event EventHandler<HealthStatusChangedEventArgs>? HealthStatusChanged;

    /// <summary>
    /// Occurs when a specific health check result changes.
    /// </summary>
    event EventHandler<HealthCheckResultChangedEventArgs>? HealthCheckResultChanged;

    /// <summary>
    /// Starts the health checker with the specified monitoring interval.
    /// </summary>
    /// <param name="interval">The interval between health checks. If null, uses default interval.</param>
    /// <param name="cancellationToken">A cancellation token that can be used to cancel the operation.</param>
    /// <returns>A task that represents the asynchronous start operation.</returns>
    Task StartAsync(TimeSpan? interval = null, CancellationToken cancellationToken = default);

    /// <summary>
    /// Stops the health checker.
    /// </summary>
    /// <param name="cancellationToken">A cancellation token that can be used to cancel the operation.</param>
    /// <returns>A task that represents the asynchronous stop operation.</returns>
    Task StopAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Performs a one-time health check on all registered components.
    /// </summary>
    /// <param name="cancellationToken">A cancellation token that can be used to cancel the operation.</param>
    /// <returns>A task that represents the asynchronous health check operation.</returns>
    Task<HealthCheckResult> CheckHealthAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Performs a health check on a specific component.
    /// </summary>
    /// <param name="componentName">The name of the component to check.</param>
    /// <param name="cancellationToken">A cancellation token that can be used to cancel the operation.</param>
    /// <returns>A task that represents the asynchronous health check operation.</returns>
    Task<HealthCheckResult> CheckHealthAsync(string componentName, CancellationToken cancellationToken = default);

    /// <summary>
    /// Registers a custom health check function.
    /// </summary>
    /// <param name="name">The name of the health check.</param>
    /// <param name="healthCheckFunc">The health check function to execute.</param>
    /// <param name="timeout">The timeout for the health check. If null, uses default timeout.</param>
    void RegisterHealthCheck(string name, Func<CancellationToken, Task<HealthCheckResult>> healthCheckFunc, TimeSpan? timeout = null);

    /// <summary>
    /// Unregisters a custom health check.
    /// </summary>
    /// <param name="name">The name of the health check to unregister.</param>
    /// <returns>True if the health check was found and removed; otherwise, false.</returns>
    bool UnregisterHealthCheck(string name);

    /// <summary>
    /// Gets health check statistics including success rate, average response time, and error count.
    /// </summary>
    /// <returns>A dictionary containing health check statistics.</returns>
    IReadOnlyDictionary<string, HealthCheckStatistics> GetHealthCheckStatistics();

    /// <summary>
    /// Resets health check statistics for all or specific components.
    /// </summary>
    /// <param name="componentName">The name of the component to reset statistics for. If null, resets all statistics.</param>
    void ResetStatistics(string? componentName = null);
}

/// <summary>
/// Represents the overall health status of the system.
/// </summary>
public enum HealthStatus
{
    /// <summary>
    /// The health status is unknown or has not been determined.
    /// </summary>
    Unknown,

    /// <summary>
    /// The system is healthy and operating normally.
    /// </summary>
    Healthy,

    /// <summary>
    /// The system is degraded but still operational.
    /// </summary>
    Degraded,

    /// <summary>
    /// The system is unhealthy and may not be operating correctly.
    /// </summary>
    Unhealthy
}

/// <summary>
/// Represents the result of a health check.
/// </summary>
public class HealthCheckResult
{
    /// <summary>
    /// Initializes a new instance of the <see cref="HealthCheckResult"/> class.
    /// </summary>
    /// <param name="status">The health status.</param>
    /// <param name="description">The description of the health check result.</param>
    /// <param name="data">Additional data associated with the health check result.</param>
    /// <param name="duration">The duration of the health check.</param>
    /// <param name="exception">The exception that occurred during the health check, if any.</param>
    public HealthCheckResult(
        HealthStatus status,
        string description,
        IReadOnlyDictionary<string, object>? data = null,
        TimeSpan? duration = null,
        Exception? exception = null)
    {
        Status = status;
        Description = description ?? throw new ArgumentNullException(nameof(description));
        Data = data ?? new Dictionary<string, object>();
        Duration = duration ?? TimeSpan.Zero;
        Exception = exception;
        Timestamp = DateTimeOffset.UtcNow;
    }

    /// <summary>
    /// Gets the health status.
    /// </summary>
    public HealthStatus Status { get; }

    /// <summary>
    /// Gets the description of the health check result.
    /// </summary>
    public string Description { get; }

    /// <summary>
    /// Gets additional data associated with the health check result.
    /// </summary>
    public IReadOnlyDictionary<string, object> Data { get; }

    /// <summary>
    /// Gets the duration of the health check.
    /// </summary>
    public TimeSpan Duration { get; }

    /// <summary>
    /// Gets the exception that occurred during the health check, if any.
    /// </summary>
    public Exception? Exception { get; }

    /// <summary>
    /// Gets the timestamp when the health check was performed.
    /// </summary>
    public DateTimeOffset Timestamp { get; }

    /// <summary>
    /// Creates a healthy result.
    /// </summary>
    /// <param name="description">The description of the healthy result.</param>
    /// <param name="data">Additional data associated with the result.</param>
    /// <param name="duration">The duration of the health check.</param>
    /// <returns>A healthy health check result.</returns>
    public static HealthCheckResult Healthy(string description, IReadOnlyDictionary<string, object>? data = null, TimeSpan? duration = null)
        => new(HealthStatus.Healthy, description, data, duration);

    /// <summary>
    /// Creates a degraded result.
    /// </summary>
    /// <param name="description">The description of the degraded result.</param>
    /// <param name="data">Additional data associated with the result.</param>
    /// <param name="duration">The duration of the health check.</param>
    /// <returns>A degraded health check result.</returns>
    public static HealthCheckResult Degraded(string description, IReadOnlyDictionary<string, object>? data = null, TimeSpan? duration = null)
        => new(HealthStatus.Degraded, description, data, duration);

    /// <summary>
    /// Creates an unhealthy result.
    /// </summary>
    /// <param name="description">The description of the unhealthy result.</param>
    /// <param name="exception">The exception that caused the unhealthy result.</param>
    /// <param name="data">Additional data associated with the result.</param>
    /// <param name="duration">The duration of the health check.</param>
    /// <returns>An unhealthy health check result.</returns>
    public static HealthCheckResult Unhealthy(string description, Exception? exception = null, IReadOnlyDictionary<string, object>? data = null, TimeSpan? duration = null)
        => new(HealthStatus.Unhealthy, description, data, duration, exception);
}

/// <summary>
/// Represents statistics for a health check.
/// </summary>
public class HealthCheckStatistics
{
    /// <summary>
    /// Initializes a new instance of the <see cref="HealthCheckStatistics"/> class.
    /// </summary>
    /// <param name="totalChecks">The total number of health checks performed.</param>
    /// <param name="successfulChecks">The number of successful health checks.</param>
    /// <param name="averageResponseTime">The average response time for health checks.</param>
    /// <param name="lastCheckTime">The timestamp of the last health check.</param>
    /// <param name="lastResult">The result of the last health check.</param>
    public HealthCheckStatistics(
        long totalChecks,
        long successfulChecks,
        TimeSpan averageResponseTime,
        DateTimeOffset? lastCheckTime,
        HealthCheckResult? lastResult)
    {
        TotalChecks = totalChecks;
        SuccessfulChecks = successfulChecks;
        AverageResponseTime = averageResponseTime;
        LastCheckTime = lastCheckTime;
        LastResult = lastResult;
    }

    /// <summary>
    /// Gets the total number of health checks performed.
    /// </summary>
    public long TotalChecks { get; }

    /// <summary>
    /// Gets the number of successful health checks.
    /// </summary>
    public long SuccessfulChecks { get; }

    /// <summary>
    /// Gets the success rate as a percentage.
    /// </summary>
    public double SuccessRate => TotalChecks > 0 ? (double)SuccessfulChecks / TotalChecks * 100 : 0;

    /// <summary>
    /// Gets the average response time for health checks.
    /// </summary>
    public TimeSpan AverageResponseTime { get; }

    /// <summary>
    /// Gets the timestamp of the last health check.
    /// </summary>
    public DateTimeOffset? LastCheckTime { get; }

    /// <summary>
    /// Gets the result of the last health check.
    /// </summary>
    public HealthCheckResult? LastResult { get; }
}

/// <summary>
/// Provides data for the health status changed event.
/// </summary>
public class HealthStatusChangedEventArgs : EventArgs
{
    /// <summary>
    /// Initializes a new instance of the <see cref="HealthStatusChangedEventArgs"/> class.
    /// </summary>
    /// <param name="previousStatus">The previous health status.</param>
    /// <param name="currentStatus">The current health status.</param>
    public HealthStatusChangedEventArgs(HealthStatus previousStatus, HealthStatus currentStatus)
    {
        PreviousStatus = previousStatus;
        CurrentStatus = currentStatus;
    }

    /// <summary>
    /// Gets the previous health status.
    /// </summary>
    public HealthStatus PreviousStatus { get; }

    /// <summary>
    /// Gets the current health status.
    /// </summary>
    public HealthStatus CurrentStatus { get; }
}

/// <summary>
/// Provides data for the health check result changed event.
/// </summary>
public class HealthCheckResultChangedEventArgs : EventArgs
{
    /// <summary>
    /// Initializes a new instance of the <see cref="HealthCheckResultChangedEventArgs"/> class.
    /// </summary>
    /// <param name="componentName">The name of the component.</param>
    /// <param name="previousResult">The previous health check result.</param>
    /// <param name="currentResult">The current health check result.</param>
    public HealthCheckResultChangedEventArgs(string componentName, HealthCheckResult? previousResult, HealthCheckResult currentResult)
    {
        ComponentName = componentName ?? throw new ArgumentNullException(nameof(componentName));
        PreviousResult = previousResult;
        CurrentResult = currentResult ?? throw new ArgumentNullException(nameof(currentResult));
    }

    /// <summary>
    /// Gets the name of the component.
    /// </summary>
    public string ComponentName { get; }

    /// <summary>
    /// Gets the previous health check result.
    /// </summary>
    public HealthCheckResult? PreviousResult { get; }

    /// <summary>
    /// Gets the current health check result.
    /// </summary>
    public HealthCheckResult CurrentResult { get; }
}