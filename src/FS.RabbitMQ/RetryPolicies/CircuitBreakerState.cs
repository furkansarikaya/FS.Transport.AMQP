namespace FS.RabbitMQ.RetryPolicies;

/// <summary>
/// Circuit breaker states
/// </summary>
public enum CircuitBreakerState
{
    /// <summary>
    /// Circuit is closed - operations are allowed
    /// </summary>
    Closed,
    
    /// <summary>
    /// Circuit is open - operations are rejected
    /// </summary>
    Open,
    
    /// <summary>
    /// Circuit is half-open - limited operations are allowed for testing
    /// </summary>
    HalfOpen
}