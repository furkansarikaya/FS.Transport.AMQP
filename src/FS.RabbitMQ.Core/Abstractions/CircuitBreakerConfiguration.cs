namespace FS.RabbitMQ.Core.Abstractions;

/// <summary>
/// Contains configuration for circuit breaker behavior to prevent cascading failures.
/// </summary>
/// <remarks>
/// Circuit breaker configuration implements the circuit breaker pattern to provide
/// fast failure and automatic recovery capabilities, protecting system resources
/// during widespread failures and enabling graceful degradation.
/// </remarks>
public sealed record CircuitBreakerConfiguration
{
    /// <summary>
    /// Gets the number of consecutive failures required to open the circuit.
    /// </summary>
    /// <value>The failure threshold for circuit activation.</value>
    public int FailureThreshold { get; init; } = 5;

    /// <summary>
    /// Gets the duration to wait before attempting to close the circuit after it opens.
    /// </summary>
    /// <value>The timeout period for circuit recovery attempts.</value>
    public TimeSpan RecoveryTimeout { get; init; } = TimeSpan.FromMinutes(1);

    /// <summary>
    /// Gets the number of successful operations required to fully close the circuit.
    /// </summary>
    /// <value>The success threshold for circuit recovery.</value>
    public int SuccessThreshold { get; init; } = 3;

    /// <summary>
    /// Gets the time window for counting failures and successes.
    /// </summary>
    /// <value>The sampling window for circuit breaker state evaluation.</value>
    public TimeSpan SamplingWindow { get; init; } = TimeSpan.FromMinutes(5);

    /// <summary>
    /// Creates a circuit breaker configuration with the specified failure threshold.
    /// </summary>
    /// <param name="failureThreshold">The number of failures required to open the circuit.</param>
    /// <param name="recoveryTimeout">The timeout before attempting recovery.</param>
    /// <returns>A circuit breaker configuration with the specified settings.</returns>
    public static CircuitBreakerConfiguration Create(int failureThreshold = 5, TimeSpan? recoveryTimeout = null) =>
        new()
        {
            FailureThreshold = failureThreshold,
            RecoveryTimeout = recoveryTimeout ?? TimeSpan.FromMinutes(1)
        };
}