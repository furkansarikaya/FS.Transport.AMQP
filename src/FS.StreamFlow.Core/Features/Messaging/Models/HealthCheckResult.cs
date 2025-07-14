namespace FS.StreamFlow.Core.Features.Messaging.Models;

/// <summary>
/// Represents the result of a health check operation
/// </summary>
public class HealthCheckResult
{
    /// <summary>
    /// Gets or sets the health check status
    /// </summary>
    public HealthStatus Status { get; set; }
    
    /// <summary>
    /// Gets or sets the description of the health check result
    /// </summary>
    public string Description { get; set; } = string.Empty;
    
    /// <summary>
    /// Gets or sets the timestamp when the health check was performed
    /// </summary>
    public DateTimeOffset Timestamp { get; set; } = DateTimeOffset.UtcNow;
    
    /// <summary>
    /// Gets or sets the duration of the health check
    /// </summary>
    public TimeSpan Duration { get; set; }
    
    /// <summary>
    /// Gets or sets additional data related to the health check
    /// </summary>
    public Dictionary<string, object>? Data { get; set; }
    
    /// <summary>
    /// Gets or sets the exception if the health check failed
    /// </summary>
    public Exception? Exception { get; set; }
    
    /// <summary>
    /// Gets or sets the tags associated with the health check
    /// </summary>
    public IEnumerable<string>? Tags { get; set; }
    
    /// <summary>
    /// Creates a healthy result
    /// </summary>
    /// <param name="description">Description of the health check</param>
    /// <param name="data">Additional data</param>
    /// <returns>Healthy health check result</returns>
    public static HealthCheckResult Healthy(string? description = null, Dictionary<string, object>? data = null)
    {
        return new HealthCheckResult
        {
            Status = HealthStatus.Healthy,
            Description = description ?? "Health check passed",
            Data = data
        };
    }
    
    /// <summary>
    /// Creates a degraded result
    /// </summary>
    /// <param name="description">Description of the health check</param>
    /// <param name="exception">Exception that caused the degradation</param>
    /// <param name="data">Additional data</param>
    /// <returns>Degraded health check result</returns>
    public static HealthCheckResult Degraded(string? description = null, Exception? exception = null, Dictionary<string, object>? data = null)
    {
        return new HealthCheckResult
        {
            Status = HealthStatus.Degraded,
            Description = description ?? "Health check degraded",
            Exception = exception,
            Data = data
        };
    }
    
    /// <summary>
    /// Creates an unhealthy result
    /// </summary>
    /// <param name="description">Description of the health check</param>
    /// <param name="exception">Exception that caused the failure</param>
    /// <param name="data">Additional data</param>
    /// <returns>Unhealthy health check result</returns>
    public static HealthCheckResult Unhealthy(string? description = null, Exception? exception = null, Dictionary<string, object>? data = null)
    {
        return new HealthCheckResult
        {
            Status = HealthStatus.Unhealthy,
            Description = description ?? "Health check failed",
            Exception = exception,
            Data = data
        };
    }
}

/// <summary>
/// Represents the status of a health check
/// </summary>
public enum HealthStatus
{
    /// <summary>
    /// Health check passed
    /// </summary>
    Healthy,
    
    /// <summary>
    /// Health check passed but with warnings
    /// </summary>
    Degraded,
    
    /// <summary>
    /// Health check failed
    /// </summary>
    Unhealthy
} 