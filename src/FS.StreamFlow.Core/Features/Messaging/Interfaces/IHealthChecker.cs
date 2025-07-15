using FS.StreamFlow.Core.Features.Messaging.Models;

namespace FS.StreamFlow.Core.Features.Messaging.Interfaces;

/// <summary>
/// Interface for health checking operations and monitoring
/// </summary>
public interface IHealthChecker : IDisposable
{
    /// <summary>
    /// Gets the current health status
    /// </summary>
    HealthStatus CurrentStatus { get; }
    
    /// <summary>
    /// Gets health check statistics
    /// </summary>
    HealthCheckStatistics Statistics { get; }
    
    /// <summary>
    /// Event raised when health status changes
    /// </summary>
    event EventHandler<HealthStatusChangedEventArgs>? HealthStatusChanged;
    
    /// <summary>
    /// Event raised when a health check completes
    /// </summary>
    event EventHandler<HealthCheckCompletedEventArgs>? HealthCheckCompleted;
    
    /// <summary>
    /// Starts the health checker
    /// </summary>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Task representing the start operation</returns>
    Task StartAsync(CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Stops the health checker
    /// </summary>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Task representing the stop operation</returns>
    Task StopAsync(CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Performs a health check
    /// </summary>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Health check result</returns>
    Task<HealthCheckResult> CheckHealthAsync(CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Performs a health check for a specific component
    /// </summary>
    /// <param name="componentName">Name of the component to check</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Health check result</returns>
    Task<HealthCheckResult> CheckHealthAsync(string componentName, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Gets the health status of all components
    /// </summary>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Dictionary of component health statuses</returns>
    Task<Dictionary<string, HealthCheckResult>> GetAllHealthStatusesAsync(CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Registers a health check for a component
    /// </summary>
    /// <param name="name">Component name</param>
    /// <param name="healthCheckFunc">Health check function</param>
    /// <param name="interval">Check interval</param>
    /// <param name="tags">Optional tags</param>
    /// <returns>Task representing the registration</returns>
    Task RegisterHealthCheckAsync(
        string name, 
        Func<CancellationToken, Task<HealthCheckResult>> healthCheckFunc, 
        TimeSpan interval, 
        IEnumerable<string>? tags = null);
    
    /// <summary>
    /// Unregisters a health check for a component
    /// </summary>
    /// <param name="name">Component name</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Task representing the unregistration</returns>
    Task UnregisterHealthCheckAsync(string name, CancellationToken cancellationToken = default);
}

/// <summary>
/// Represents health check statistics
/// </summary>
public class HealthCheckStatistics
{
    /// <summary>
    /// Total number of health checks performed
    /// </summary>
    public long TotalHealthChecks { get; set; }
    
    /// <summary>
    /// Number of healthy checks
    /// </summary>
    public long HealthyChecks { get; set; }
    
    /// <summary>
    /// Number of degraded checks
    /// </summary>
    public long DegradedChecks { get; set; }
    
    /// <summary>
    /// Number of unhealthy checks
    /// </summary>
    public long UnhealthyChecks { get; set; }
    
    /// <summary>
    /// Average health check duration
    /// </summary>
    public TimeSpan AverageCheckDuration { get; set; }
    
    /// <summary>
    /// Last health check timestamp
    /// </summary>
    public DateTimeOffset? LastHealthCheckAt { get; set; }
    
    /// <summary>
    /// Current uptime
    /// </summary>
    public TimeSpan CurrentUptime { get; set; }
    
    /// <summary>
    /// Health check success rate (percentage)
    /// </summary>
    public double HealthCheckSuccessRate => TotalHealthChecks > 0 
        ? (double)HealthyChecks / TotalHealthChecks * 100 
        : 0;
    
    /// <summary>
    /// Statistics collection timestamp
    /// </summary>
    public DateTimeOffset Timestamp { get; set; } = DateTimeOffset.UtcNow;
}

/// <summary>
/// Event arguments for health status changes
/// </summary>
public class HealthStatusChangedEventArgs : EventArgs
{
    /// <summary>
    /// Previous health status
    /// </summary>
    public HealthStatus PreviousStatus { get; }
    
    /// <summary>
    /// Current health status
    /// </summary>
    public HealthStatus CurrentStatus { get; }
    
    /// <summary>
    /// Component name (if applicable)
    /// </summary>
    public string? ComponentName { get; }
    
    /// <summary>
    /// Timestamp when the status changed
    /// </summary>
    public DateTimeOffset Timestamp { get; }
    
    /// <summary>
    /// Initializes a new instance of the HealthStatusChangedEventArgs class
    /// </summary>
    /// <param name="previousStatus">Previous health status</param>
    /// <param name="currentStatus">Current health status</param>
    /// <param name="componentName">Component name (if applicable)</param>
    public HealthStatusChangedEventArgs(
        HealthStatus previousStatus, 
        HealthStatus currentStatus, 
        string? componentName = null)
    {
        PreviousStatus = previousStatus;
        CurrentStatus = currentStatus;
        ComponentName = componentName;
        Timestamp = DateTimeOffset.UtcNow;
    }
}

/// <summary>
/// Event arguments for health check completion
/// </summary>
public class HealthCheckCompletedEventArgs : EventArgs
{
    /// <summary>
    /// Health check result
    /// </summary>
    public HealthCheckResult Result { get; }
    
    /// <summary>
    /// Component name (if applicable)
    /// </summary>
    public string? ComponentName { get; }
    
    /// <summary>
    /// Timestamp when the check completed
    /// </summary>
    public DateTimeOffset Timestamp { get; }
    
    /// <summary>
    /// Initializes a new instance of the HealthCheckCompletedEventArgs class
    /// </summary>
    /// <param name="result">Health check result</param>
    /// <param name="componentName">Component name (if applicable)</param>
    public HealthCheckCompletedEventArgs(HealthCheckResult result, string? componentName = null)
    {
        Result = result;
        ComponentName = componentName;
        Timestamp = DateTimeOffset.UtcNow;
    }
} 