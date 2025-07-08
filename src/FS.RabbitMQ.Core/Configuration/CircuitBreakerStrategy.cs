namespace FS.RabbitMQ.Core.Configuration;

/// <summary>
/// Defines the strategies for counting failures and determining when to trip the circuit breaker.
/// </summary>
/// <remarks>
/// Different strategies provide different approaches to failure detection and are
/// suitable for different service characteristics and failure patterns.
/// </remarks>
public enum CircuitBreakerStrategy
{
    /// <summary>
    /// Counts consecutive failures without any intervening successes.
    /// Circuit opens when the consecutive failure count reaches the threshold.
    /// </summary>
    /// <remarks>
    /// Best for services that tend to fail completely or recover completely,
    /// with clear binary working/not-working states.
    /// </remarks>
    ConsecutiveFailures,

    /// <summary>
    /// Calculates the percentage of failures within a sliding time window.
    /// Circuit opens when the failure rate exceeds the threshold percentage.
    /// </summary>
    /// <remarks>
    /// Best for services that may have gradual degradation or mixed success/failure patterns.
    /// Provides more nuanced failure detection than consecutive counting.
    /// </remarks>
    FailureRate,

    /// <summary>
    /// Counts the absolute number of failures within a sliding time window.
    /// Circuit opens when the total failure count exceeds the threshold.
    /// </summary>
    /// <remarks>
    /// Best for scenarios where the absolute volume of failures indicates a problem,
    /// regardless of the success rate. Useful for high-volume services.
    /// </remarks>
    FailureCount
}