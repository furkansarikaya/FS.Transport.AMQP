namespace FS.RabbitMQ.Configuration;

/// <summary>
/// Settings for retry policies and circuit breaker behavior
/// </summary>
public class RetryPolicySettings
{
    /// <summary>
    /// Type of retry policy (None, Linear, Exponential, CircuitBreaker)
    /// </summary>
    public string PolicyType { get; set; } = "Exponential";
    
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
    /// Initial delay between retries as TimeSpan
    /// </summary>
    public TimeSpan InitialDelay
    {
        get => TimeSpan.FromMilliseconds(InitialDelayMs);
        set => InitialDelayMs = (int)value.TotalMilliseconds;
    }
    
    /// <summary>
    /// Maximum delay between retries as TimeSpan
    /// </summary>
    public TimeSpan MaxDelay
    {
        get => TimeSpan.FromMilliseconds(MaxDelayMs);
        set => MaxDelayMs = (int)value.TotalMilliseconds;
    }
    
    /// <summary>
    /// Exponential backoff multiplier (for exponential policy)
    /// </summary>
    public double BackoffMultiplier { get; set; } = 2.0;
    
    /// <summary>
    /// Multiplier for retry delay (alias for BackoffMultiplier)
    /// </summary>
    public double Multiplier 
    { 
        get => BackoffMultiplier; 
        set => BackoffMultiplier = value; 
    }
    
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
    /// Custom exception types that should not be retried
    /// </summary>
    public HashSet<string> NonRetryableExceptions { get; set; } = new();
    
    /// <summary>
    /// Custom exception types that should always be retried
    /// </summary>
    public HashSet<string> RetryableExceptions { get; set; } = new();

    /// <summary>
    /// Validates retry policy settings
    /// </summary>
    public void Validate()
    {
        var validTypes = new[] { "None", "Linear", "Exponential", "CircuitBreaker" };
        if (!validTypes.Contains(PolicyType))
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

    /// <summary>
    /// Creates a copy of the retry settings
    /// </summary>
    /// <returns>Cloned retry settings</returns>
    public RetryPolicySettings Clone()
    {
        return new RetryPolicySettings
        {
            PolicyType = PolicyType,
            MaxRetries = MaxRetries,
            InitialDelayMs = InitialDelayMs,
            MaxDelayMs = MaxDelayMs,
            BackoffMultiplier = BackoffMultiplier,
            JitterFactor = JitterFactor,
            CircuitBreakerFailureThreshold = CircuitBreakerFailureThreshold,
            CircuitBreakerRecoveryTimeoutMs = CircuitBreakerRecoveryTimeoutMs,
            NonRetryableExceptions = [..NonRetryableExceptions],
            RetryableExceptions = [..RetryableExceptions]
        };
    }

    /// <summary>
    /// Gets a string representation of the retry settings
    /// </summary>
    /// <returns>Settings description</returns>
    public override string ToString()
    {
        return $"RetrySettings: {PolicyType} policy, MaxRetries: {MaxRetries}, " +
               $"InitialDelay: {InitialDelayMs}ms, MaxDelay: {MaxDelayMs}ms";
    }
}
