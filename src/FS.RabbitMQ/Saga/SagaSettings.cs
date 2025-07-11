namespace FS.RabbitMQ.Saga;

/// <summary>
/// Configuration settings for saga orchestration
/// </summary>
public class SagaSettings
{
    /// <summary>
    /// Whether saga orchestration is enabled
    /// </summary>
    public bool Enabled { get; set; } = true;
    
    /// <summary>
    /// Whether to enable compensation for failed sagas
    /// </summary>
    public bool EnableCompensation { get; set; } = true;
    
    /// <summary>
    /// Timeout duration for saga execution
    /// </summary>
    public TimeSpan TimeoutDuration { get; set; } = TimeSpan.FromMinutes(30);
    
    /// <summary>
    /// Maximum number of retries for failed steps
    /// </summary>
    public int MaxRetries { get; set; } = 3;
    
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
            MaxConcurrentSagas = 10,
            AutoPersistState = true,
            EnableStateSnapshots = true,
            EnableMetrics = true,
            EnableEventLogging = true,
            EnableStateCaching = false,
            DefaultMaxRetries = 5,
            DefaultRetryDelay = TimeSpan.FromSeconds(10),
            UseExponentialBackoff = true,
            MaxBackoffDelay = TimeSpan.FromMinutes(10)
        };
    }
    
    /// <summary>
    /// Creates saga settings optimized for development
    /// </summary>
    /// <returns>Development saga settings</returns>
    public static SagaSettings CreateDevelopment()
    {
        return new SagaSettings
        {
            MaxConcurrentSagas = 5,
            AutoPersistState = false,
            EnableStateSnapshots = false,
            EnableMetrics = false,
            EnableEventLogging = true,
            EnableStateCaching = false,
            DefaultMaxRetries = 1,
            DefaultRetryDelay = TimeSpan.FromSeconds(1),
            UseExponentialBackoff = false
        };
    }
    
    /// <summary>
    /// Validates saga settings
    /// </summary>
    public void Validate()
    {
        if (TimeoutDuration <= TimeSpan.Zero)
            throw new ArgumentException("TimeoutDuration must be greater than zero");
            
        if (MaxRetries < 0)
            throw new ArgumentException("MaxRetries cannot be negative");
            
        if (DefaultTimeout <= TimeSpan.Zero)
            throw new ArgumentException("DefaultTimeout must be greater than zero");
            
        if (DefaultMaxRetries < 0)
            throw new ArgumentException("DefaultMaxRetries cannot be negative");
            
        if (DefaultRetryDelay < TimeSpan.Zero)
            throw new ArgumentException("DefaultRetryDelay cannot be negative");
            
        if (MaxBackoffDelay <= TimeSpan.Zero)
            throw new ArgumentException("MaxBackoffDelay must be greater than zero");
            
        if (StatePersistenceInterval <= TimeSpan.Zero)
            throw new ArgumentException("StatePersistenceInterval must be greater than zero");
            
        if (MaxStateSnapshots < 0)
            throw new ArgumentException("MaxStateSnapshots cannot be negative");
            
        if (MaxConcurrentSagas <= 0)
            throw new ArgumentException("MaxConcurrentSagas must be greater than zero");
            
        if (StepTimeout <= TimeSpan.Zero)
            throw new ArgumentException("StepTimeout must be greater than zero");
            
        if (CompensationTimeout <= TimeSpan.Zero)
            throw new ArgumentException("CompensationTimeout must be greater than zero");
            
        if (CircuitBreakerFailureThreshold <= 0)
            throw new ArgumentException("CircuitBreakerFailureThreshold must be greater than zero");
            
        if (CircuitBreakerRecoveryTimeout <= TimeSpan.Zero)
            throw new ArgumentException("CircuitBreakerRecoveryTimeout must be greater than zero");
            
        if (StateCacheExpiration <= TimeSpan.Zero)
            throw new ArgumentException("StateCacheExpiration must be greater than zero");
            
        if (MaxStateCacheSize <= 0)
            throw new ArgumentException("MaxStateCacheSize must be greater than zero");
            
        if (HealthCheckInterval <= TimeSpan.Zero)
            throw new ArgumentException("HealthCheckInterval must be greater than zero");
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