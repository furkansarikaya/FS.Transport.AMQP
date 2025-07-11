namespace FS.RabbitMQ.RetryPolicies;

/// <summary>
/// Event arguments for circuit breaker state changes
/// </summary>
public class CircuitBreakerStateChangedEventArgs(CircuitBreakerState previousState, CircuitBreakerState currentState, int failureCount)
    : EventArgs
{
    /// <summary>
    /// Previous circuit breaker state
    /// </summary>
    public CircuitBreakerState PreviousState { get; } = previousState;

    /// <summary>
    /// Current circuit breaker state
    /// </summary>
    public CircuitBreakerState CurrentState { get; } = currentState;

    /// <summary>
    /// Current failure count
    /// </summary>
    public int FailureCount { get; } = failureCount;

    /// <summary>
    /// Timestamp of the state change
    /// </summary>
    public DateTime Timestamp { get; } = DateTime.UtcNow;

    /// <summary>
    /// Gets a string representation of the state change
    /// </summary>
    /// <returns>State change description</returns>
    public override string ToString()
    {
        return $"Circuit breaker state changed from {PreviousState} to {CurrentState} (Failures: {FailureCount}) at {Timestamp:yyyy-MM-dd HH:mm:ss}";
    }
}