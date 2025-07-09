namespace FS.RabbitMQ.Configuration;

/// <summary>
/// Settings for health check functionality
/// </summary>
public class HealthCheckSettings
{
    /// <summary>
    /// Whether health checks are enabled
    /// </summary>
    public bool Enabled { get; set; } = true;
    
    /// <summary>
    /// Health check interval in milliseconds
    /// </summary>
    public int IntervalMs { get; set; } = 30000; // 30 seconds
    
    /// <summary>
    /// Health check timeout in milliseconds
    /// </summary>
    public int TimeoutMs { get; set; } = 5000; // 5 seconds
    
    /// <summary>
    /// Number of consecutive failures before marking as unhealthy
    /// </summary>
    public int FailureThreshold { get; set; } = 3;
    
    /// <summary>
    /// Number of consecutive successes before marking as healthy
    /// </summary>
    public int SuccessThreshold { get; set; } = 1;
    
    /// <summary>
    /// Whether to include detailed metrics in health check results
    /// </summary>
    public bool IncludeMetrics { get; set; } = true;
    
    /// <summary>
    /// Whether to test queue operations during health check
    /// </summary>
    public bool TestQueueOperations { get; set; } = false;
    
    /// <summary>
    /// Test queue name for health check operations
    /// </summary>
    public string TestQueueName { get; set; } = "health-check-test";

    /// <summary>
    /// Validates health check settings
    /// </summary>
    public void Validate()
    {
        if (IntervalMs <= 0)
            throw new ArgumentException("IntervalMs must be greater than 0");
            
        if (TimeoutMs <= 0)
            throw new ArgumentException("TimeoutMs must be greater than 0");
            
        if (FailureThreshold <= 0)
            throw new ArgumentException("FailureThreshold must be greater than 0");
            
        if (SuccessThreshold <= 0)
            throw new ArgumentException("SuccessThreshold must be greater than 0");
            
        if (TestQueueOperations && string.IsNullOrWhiteSpace(TestQueueName))
            throw new ArgumentException("TestQueueName cannot be null or empty when TestQueueOperations is enabled");
    }
}
