namespace FS.RabbitMQ.Configuration;

/// <summary>
/// Settings for retry policies and circuit breaker behavior
/// </summary>
public class RetryPolicySettings
{
    /// <summary>
    /// Type of retry policy (None, Linear, Exponential, CircuitBreaker)
    /// </summary>
    public string Type { get; set; } = "Exponential";
    
    /// <summary>
    /// Maximum number of retry attempts
    /// </summary>
    public int MaxRetries { get; set; } = 3;
    
    /// <summary>
    /// Initial delay between retries in milliseconds
    /// </summary>
    public int InitialDelayMs { get; set; } = 1000;
    
    /// <summary>
    /// Maximum delay between retries in milliseconds
    /// </summary>
    public int MaxDelayMs { get; set; } = 30000;
    
    /// <summary>
    /// Exponential backoff multiplier (for exponential policy)
    /// </summary>
    public double BackoffMultiplier { get; set; } = 2.0;
    
    /// <summary>
    /// Jitter factor to add randomness (0.0 to 1.0)
    /// </summary>
    public double JitterFactor { get; set; } = 0.1;
    
    /// <summary>
    /// Circuit breaker failure threshold
    /// </summary>
    public int CircuitBreakerFailureThreshold { get; set; } = 5;
    
    /// <summary>
    /// Circuit breaker recovery timeout in milliseconds
    /// </summary>
    public int CircuitBreakerRecoveryTimeoutMs { get; set; } = 60000;

    /// <summary>
    /// Validates retry policy settings
    /// </summary>
    public void Validate()
    {
        var validTypes = new[] { "None", "Linear", "Exponential", "CircuitBreaker" };
        if (!validTypes.Contains(Type))
            throw new ArgumentException($"Retry policy type must be one of: {string.Join(", ", validTypes)}");
            
        if (MaxRetries < 0)
            throw new ArgumentException("MaxRetries cannot be negative");
            
        if (InitialDelayMs <= 0)
            throw new ArgumentException("InitialDelayMs must be greater than 0");
            
        if (MaxDelayMs <= 0)
            throw new ArgumentException("MaxDelayMs must be greater than 0");
            
        if (InitialDelayMs > MaxDelayMs)
            throw new ArgumentException("InitialDelayMs cannot be greater than MaxDelayMs");
            
        if (BackoffMultiplier <= 1.0)
            throw new ArgumentException("BackoffMultiplier must be greater than 1.0");
            
        if (JitterFactor < 0.0 || JitterFactor > 1.0)
            throw new ArgumentException("JitterFactor must be between 0.0 and 1.0");
            
        if (CircuitBreakerFailureThreshold <= 0)
            throw new ArgumentException("CircuitBreakerFailureThreshold must be greater than 0");
            
        if (CircuitBreakerRecoveryTimeoutMs <= 0)
            throw new ArgumentException("CircuitBreakerRecoveryTimeoutMs must be greater than 0");
    }
}
