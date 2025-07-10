namespace FS.Transport.AMQP.Saga;

/// <summary>
/// Configuration settings for saga orchestration
/// </summary>
public class SagaSettings
{
    /// <summary>
    /// Default timeout for saga execution
    /// </summary>
    public TimeSpan DefaultTimeout { get; set; } = TimeSpan.FromMinutes(30);
    
    /// <summary>
    /// Default maximum number of retries for failed steps
    /// </summary>
    public int DefaultMaxRetries { get; set; } = 3;
    
    /// <summary>
    /// Default delay between retry attempts
    /// </summary>
    public TimeSpan DefaultRetryDelay { get; set; } = TimeSpan.FromSeconds(5);
    
    /// <summary>
    /// Whether to use exponential backoff for retries
    /// </summary>
    public bool UseExponentialBackoff { get; set; } = true;
    
    /// <summary>
    /// Maximum delay for exponential backoff
    /// </summary>
    public TimeSpan MaxBackoffDelay { get; set; } = TimeSpan.FromMinutes(5);
    
    /// <summary>
    /// Whether to automatically compensate on critical step failures
    /// </summary>
    public bool AutoCompensateOnCriticalFailure { get; set; } = true;
    
    /// <summary>
    /// Whether to persist saga state automatically
    /// </summary>
    public bool AutoPersistState { get; set; } = true;
    
    /// <summary>
    /// Interval for automatic state persistence
    /// </summary>
    public TimeSpan StatePersistenceInterval { get; set; } = TimeSpan.FromSeconds(30);
    
    /// <summary>
    /// Whether to enable saga state snapshots
    /// </summary>
    public bool EnableStateSnapshots { get; set; } = true;
    
    /// <summary>
    /// Number of state snapshots to keep
    /// </summary>
    public int MaxStateSnapshots { get; set; } = 10;
    
    /// <summary>
    /// Whether to enable saga metrics collection
    /// </summary>
    public bool EnableMetrics { get; set; } = true;
    
    /// <summary>
    /// Whether to enable saga event logging
    /// </summary>
    public bool EnableEventLogging { get; set; } = true;
    
    /// <summary>
    /// Maximum number of concurrent saga executions
    /// </summary>
    public int MaxConcurrentSagas { get; set; } = 100;
    
    /// <summary>
    /// Whether to enable saga correlation tracking
    /// </summary>
    public bool EnableCorrelationTracking { get; set; } = true;
    
    /// <summary>
    /// Default correlation ID generator
    /// </summary>
    public Func<string> CorrelationIdGenerator { get; set; } = () => Guid.NewGuid().ToString();
    
    /// <summary>
    /// Whether to enable saga step validation
    /// </summary>
    public bool EnableStepValidation { get; set; } = true;
    
    /// <summary>
    /// Whether to enable saga dependency validation
    /// </summary>
    public bool EnableDependencyValidation { get; set; } = true;
    
    /// <summary>
    /// Whether to enable saga deadlock detection
    /// </summary>
    public bool EnableDeadlockDetection { get; set; } = true;
    
    /// <summary>
    /// Timeout for saga step execution
    /// </summary>
    public TimeSpan StepTimeout { get; set; } = TimeSpan.FromMinutes(10);
    
    /// <summary>
    /// Timeout for saga compensation
    /// </summary>
    public TimeSpan CompensationTimeout { get; set; } = TimeSpan.FromMinutes(15);
    
    /// <summary>
    /// Whether to enable saga circuit breaker
    /// </summary>
    public bool EnableCircuitBreaker { get; set; } = true;
    
    /// <summary>
    /// Circuit breaker failure threshold
    /// </summary>
    public int CircuitBreakerFailureThreshold { get; set; } = 5;
    
    /// <summary>
    /// Circuit breaker recovery timeout
    /// </summary>
    public TimeSpan CircuitBreakerRecoveryTimeout { get; set; } = TimeSpan.FromMinutes(5);
    
    /// <summary>
    /// Whether to enable saga state caching
    /// </summary>
    public bool EnableStateCaching { get; set; } = true;
    
    /// <summary>
    /// State cache expiration time
    /// </summary>
    public TimeSpan StateCacheExpiration { get; set; } = TimeSpan.FromMinutes(30);
    
    /// <summary>
    /// Maximum size of state cache
    /// </summary>
    public int MaxStateCacheSize { get; set; } = 1000;
    
    /// <summary>
    /// Whether to enable saga health checks
    /// </summary>
    public bool EnableHealthChecks { get; set; } = true;
    
    /// <summary>
    /// Health check interval
    /// </summary>
    public TimeSpan HealthCheckInterval { get; set; } = TimeSpan.FromMinutes(5);
    
    /// <summary>
    /// Creates default saga settings
    /// </summary>
    /// <returns>Default saga settings</returns>
    public static SagaSettings CreateDefault()
    {
        return new SagaSettings();
    }
    
    /// <summary>
    /// Creates saga settings optimized for high throughput
    /// </summary>
    /// <returns>High throughput saga settings</returns>
    public static SagaSettings CreateHighThroughput()
    {
        return new SagaSettings
        {
            MaxConcurrentSagas = 500,
            AutoPersistState = false,
            EnableStateSnapshots = false,
            EnableMetrics = false,
            EnableEventLogging = false,
            EnableStateCaching = true,
            StateCacheExpiration = TimeSpan.FromMinutes(10),
            MaxStateCacheSize = 5000
        };
    }
    
    /// <summary>
    /// Creates saga settings optimized for reliability
    /// </summary>
    /// <returns>Reliable saga settings</returns>
    public static SagaSettings CreateReliable()
    {
        return new SagaSettings
        {
            DefaultMaxRetries = 5,
            UseExponentialBackoff = true,
            AutoCompensateOnCriticalFailure = true,
            AutoPersistState = true,
            StatePersistenceInterval = TimeSpan.FromSeconds(10),
            EnableStateSnapshots = true,
            MaxStateSnapshots = 20,
            EnableCircuitBreaker = true,
            CircuitBreakerFailureThreshold = 3,
            EnableHealthChecks = true,
            HealthCheckInterval = TimeSpan.FromMinutes(1)
        };
    }
    
    /// <summary>
    /// Validates the saga settings
    /// </summary>
    /// <returns>Validation result</returns>
    public SagaSettingsValidationResult Validate()
    {
        var result = new SagaSettingsValidationResult();
        
        if (DefaultTimeout <= TimeSpan.Zero)
            result.AddError("DefaultTimeout must be greater than zero");
        
        if (DefaultMaxRetries < 0)
            result.AddError("DefaultMaxRetries cannot be negative");
        
        if (DefaultRetryDelay <= TimeSpan.Zero)
            result.AddError("DefaultRetryDelay must be greater than zero");
        
        if (MaxBackoffDelay <= TimeSpan.Zero)
            result.AddError("MaxBackoffDelay must be greater than zero");
        
        if (StatePersistenceInterval <= TimeSpan.Zero)
            result.AddError("StatePersistenceInterval must be greater than zero");
        
        if (MaxStateSnapshots < 0)
            result.AddError("MaxStateSnapshots cannot be negative");
        
        if (MaxConcurrentSagas <= 0)
            result.AddError("MaxConcurrentSagas must be greater than zero");
        
        if (StepTimeout <= TimeSpan.Zero)
            result.AddError("StepTimeout must be greater than zero");
        
        if (CompensationTimeout <= TimeSpan.Zero)
            result.AddError("CompensationTimeout must be greater than zero");
        
        if (CircuitBreakerFailureThreshold <= 0)
            result.AddError("CircuitBreakerFailureThreshold must be greater than zero");
        
        if (CircuitBreakerRecoveryTimeout <= TimeSpan.Zero)
            result.AddError("CircuitBreakerRecoveryTimeout must be greater than zero");
        
        if (StateCacheExpiration <= TimeSpan.Zero)
            result.AddError("StateCacheExpiration must be greater than zero");
        
        if (MaxStateCacheSize <= 0)
            result.AddError("MaxStateCacheSize must be greater than zero");
        
        if (HealthCheckInterval <= TimeSpan.Zero)
            result.AddError("HealthCheckInterval must be greater than zero");
        
        return result;
    }
}

/// <summary>
/// Result of saga settings validation
/// </summary>
public class SagaSettingsValidationResult
{
    /// <summary>
    /// List of validation errors
    /// </summary>
    public List<string> Errors { get; } = new();
    
    /// <summary>
    /// Whether the validation passed
    /// </summary>
    public bool IsValid => !Errors.Any();
    
    /// <summary>
    /// Adds a validation error
    /// </summary>
    /// <param name="error">Error message</param>
    public void AddError(string error)
    {
        Errors.Add(error);
    }
    
    /// <summary>
    /// Gets formatted error messages
    /// </summary>
    /// <returns>Formatted error string</returns>
    public string GetErrorMessages()
    {
        return string.Join("; ", Errors);
    }
}