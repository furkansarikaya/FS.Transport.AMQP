using FS.RabbitMQ.Core.Abstractions;

namespace FS.RabbitMQ.Core.Configuration;

/// <summary>
/// Defines configuration for circuit breaker pattern implementation to prevent cascading failures.
/// </summary>
/// <remarks>
/// The circuit breaker pattern is a crucial resilience mechanism that prevents systems from
/// repeatedly attempting operations that are likely to fail. It acts like an electrical
/// circuit breaker - when too many failures occur, it "trips" and stops allowing requests
/// through, giving the failing system time to recover.
/// 
/// Understanding the circuit breaker pattern:
/// 
/// Think of a circuit breaker like the electrical breakers in your home. When there's an
/// electrical problem (like a short circuit), the breaker automatically shuts off power
/// to prevent damage. Similarly, a software circuit breaker monitors failures and stops
/// sending requests to a failing service to prevent overwhelming it and causing more problems.
/// 
/// Circuit breaker states:
/// 
/// 1. Closed (Normal Operation):
///    - Requests flow through normally
///    - Failures are counted and monitored
///    - If failure threshold is exceeded, transitions to Open
/// 
/// 2. Open (Failure Mode):
///    - All requests fail immediately without being attempted
///    - Provides fast failure responses
///    - After timeout period, transitions to Half-Open for testing
/// 
/// 3. Half-Open (Testing Recovery):
///    - Limited number of test requests are allowed through
///    - If test requests succeed, transitions back to Closed
///    - If test requests fail, returns to Open state
/// 
/// Benefits of circuit breaker pattern:
/// - Prevents cascading failures across system boundaries
/// - Reduces resource usage during system failures
/// - Provides fast failure responses when systems are known to be down
/// - Allows automatic recovery detection and restoration
/// - Improves overall system stability and user experience
/// </remarks>
public sealed class CircuitBreakerConfiguration
{
    /// <summary>
    /// Gets or sets the failure threshold that triggers the circuit breaker to open.
    /// </summary>
    /// <value>The number of consecutive failures or failure rate that causes the circuit to open. Default is 5.</value>
    /// <remarks>
    /// The failure threshold determines how sensitive the circuit breaker is to failures:
    /// 
    /// For consecutive failure counting:
    /// - Lower thresholds (3-5): More sensitive, faster failure detection, may trip on brief issues
    /// - Higher thresholds (10-20): Less sensitive, slower detection, more tolerant of intermittent failures
    /// 
    /// For failure rate percentage:
    /// - 50-70%: Balanced approach for most scenarios
    /// - 30-50%: More sensitive for critical services
    /// - 70-90%: Less sensitive for services with expected occasional failures
    /// 
    /// Threshold considerations:
    /// - Service criticality and expected reliability
    /// - Cost of false positives (unnecessary circuit opening)
    /// - Cost of false negatives (continued attempts to failing service)
    /// - Expected patterns of failures (brief spikes vs. sustained outages)
    /// - Downstream service characteristics and recovery patterns
    /// 
    /// Start with moderate thresholds and adjust based on observed failure patterns
    /// and the impact of circuit breaker trips on user experience.
    /// </remarks>
    public int FailureThreshold { get; set; } = 5;

    /// <summary>
    /// Gets or sets the time window for measuring failure rates when using percentage-based thresholds.
    /// </summary>
    /// <value>The sliding window duration for failure rate calculations. Default is 1 minute.</value>
    /// <remarks>
    /// The measurement window affects how the circuit breaker responds to different failure patterns:
    /// 
    /// Shorter windows (30 seconds - 2 minutes):
    /// - More responsive to sudden failure spikes
    /// - Better for detecting acute service problems
    /// - May be too sensitive for services with variable performance
    /// - Good for real-time or interactive services
    /// 
    /// Longer windows (5-15 minutes):
    /// - Smoother response to intermittent failures
    /// - Better for services with expected performance variations
    /// - May be too slow for detecting critical failures
    /// - Good for batch processing or background services
    /// 
    /// Window size should consider:
    /// - Service communication patterns and request frequency
    /// - Expected failure characteristics and recovery times
    /// - Acceptable delay in failure detection and response
    /// - Balance between sensitivity and stability
    /// </remarks>
    public TimeSpan MeasurementWindow { get; set; } = TimeSpan.FromMinutes(1);

    /// <summary>
    /// Gets or sets the minimum number of requests required before the circuit breaker can trip.
    /// </summary>
    /// <value>The minimum request count needed to evaluate failure rates. Default is 10.</value>
    /// <remarks>
    /// Minimum request count prevents the circuit breaker from tripping on insufficient data:
    /// 
    /// - Prevents premature tripping during low traffic periods
    /// - Ensures statistical significance for failure rate calculations
    /// - Avoids false positives from small sample sizes
    /// - Should be set based on typical request volumes
    /// 
    /// Considerations for minimum request count:
    /// - Normal traffic patterns and request volumes
    /// - Acceptable delay in failure detection during low traffic
    /// - Statistical significance requirements for failure rate calculations
    /// - Service startup patterns and warm-up periods
    /// 
    /// For high-traffic services: 50-200 requests
    /// For moderate-traffic services: 10-50 requests
    /// For low-traffic services: 5-20 requests
    /// 
    /// Set too low and the circuit may trip on insufficient data.
    /// Set too high and failure detection may be delayed during low traffic periods.
    /// </remarks>
    public int MinimumRequestCount { get; set; } = 10;

    /// <summary>
    /// Gets or sets the duration the circuit breaker stays open before attempting recovery.
    /// </summary>
    /// <value>The time to wait in the open state before transitioning to half-open. Default is 30 seconds.</value>
    /// <remarks>
    /// Open duration affects recovery timing and system behavior during outages:
    /// 
    /// Shorter open durations (10-30 seconds):
    /// - Faster recovery detection for brief outages
    /// - More frequent recovery attempts
    /// - May not give failing systems enough time to recover
    /// - Better user experience for brief failures
    /// 
    /// Longer open durations (1-10 minutes):
    /// - More time for failing systems to recover
    /// - Fewer recovery attempts reduce load on failing systems
    /// - May delay recovery detection for systems that recover quickly
    /// - Better for systems with longer recovery cycles
    /// 
    /// Duration should be based on:
    /// - Expected failure and recovery patterns of downstream services
    /// - Acceptable delay in detecting recovery
    /// - Impact of circuit breaker being open on user experience
    /// - Load characteristics and recovery capacity of downstream services
    /// 
    /// Common patterns:
    /// - Critical real-time services: 15-60 seconds
    /// - Standard services: 30 seconds - 5 minutes
    /// - Background/batch services: 2-15 minutes
    /// - Infrastructure services: 1-30 minutes
    /// </remarks>
    public TimeSpan OpenDuration { get; set; } = TimeSpan.FromSeconds(30);

    /// <summary>
    /// Gets or sets the number of test requests allowed in the half-open state.
    /// </summary>
    /// <value>The number of trial requests to determine if the service has recovered. Default is 3.</value>
    /// <remarks>
    /// Half-open test requests balance recovery detection speed with load on recovering services:
    /// 
    /// Fewer test requests (1-3):
    /// - Minimal load on recovering services
    /// - Faster decision making (succeed or fail quickly)
    /// - May not provide sufficient evidence of recovery
    /// - Good for services that recover completely or not at all
    /// 
    /// More test requests (5-10):
    /// - Better statistical confidence in recovery assessment
    /// - Can detect partial recovery or intermittent issues
    /// - More load on recovering services
    /// - Better for services that recover gradually
    /// 
    /// Test request considerations:
    /// - Recovery characteristics of downstream services
    /// - Acceptable load during recovery testing
    /// - Confidence required for recovery decisions
    /// - Cost of false recovery detection (premature circuit closing)
    /// - Cost of delayed recovery detection (extended outage)
    /// 
    /// Most services work well with 3-5 test requests, providing good balance
    /// between confidence and minimal load on recovering systems.
    /// </remarks>
    public int HalfOpenRequestCount { get; set; } = 3;

    /// <summary>
    /// Gets or sets the strategy for counting failures and determining when to trip the circuit.
    /// </summary>
    /// <value>The failure counting strategy used to evaluate circuit breaker conditions. Default is ConsecutiveFailures.</value>
    /// <remarks>
    /// Different failure counting strategies respond to different failure patterns:
    /// 
    /// Consecutive Failures:
    /// - Counts failures in a row, resets on any success
    /// - Responsive to sustained failure conditions
    /// - Tolerant of intermittent failures mixed with successes
    /// - Good for services with binary failure patterns (working or completely broken)
    /// 
    /// Failure Rate (percentage over time window):
    /// - Counts percentage of failures within a sliding time window
    /// - More sophisticated analysis of failure patterns
    /// - Can detect degraded performance and partial failures
    /// - Good for services that may have varying levels of success/failure
    /// 
    /// Failure Count (absolute number within time window):
    /// - Counts total failures within a time window regardless of successes
    /// - Useful for detecting absolute failure volume thresholds
    /// - Good for scenarios where any significant failure count indicates problems
    /// 
    /// Choose the strategy that best matches your service failure characteristics:
    /// - Binary services (fully working or completely broken): Consecutive failures
    /// - Services with gradual degradation: Failure rate
    /// - Services where failure volume matters: Failure count
    /// </remarks>
    public CircuitBreakerStrategy Strategy { get; set; } = CircuitBreakerStrategy.ConsecutiveFailures;

    /// <summary>
    /// Gets or sets the success threshold for transitioning from half-open back to closed state.
    /// </summary>
    /// <value>The number of consecutive successful requests needed to close the circuit. Default is equal to HalfOpenRequestCount.</value>
    /// <remarks>
    /// Success threshold determines how confident the circuit breaker needs to be about recovery:
    /// 
    /// Lower thresholds (1-2 successes):
    /// - Faster recovery and circuit closing
    /// - More optimistic about service recovery
    /// - Risk of premature closing if service isn't fully recovered
    /// - Good for services that typically recover completely
    /// 
    /// Higher thresholds (all test requests or more):
    /// - More conservative recovery assessment
    /// - Higher confidence in service recovery before reopening
    /// - Slower recovery but more stable circuit behavior
    /// - Good for services with gradual or unreliable recovery patterns
    /// 
    /// Common configurations:
    /// - Require all test requests to succeed (most conservative)
    /// - Require majority of test requests to succeed (balanced)
    /// - Require just a few successes (most optimistic)
    /// 
    /// The success threshold should align with your tolerance for false recovery
    /// detection and the cost of prematurely closing the circuit.
    /// </remarks>
    public int SuccessThreshold { get; set; } = 3;

    /// <summary>
    /// Gets or sets additional circuit breaker properties for advanced scenarios.
    /// </summary>
    /// <value>A dictionary of advanced configuration properties, or null if no additional configuration is needed.</value>
    /// <remarks>
    /// Advanced properties enable customization for specialized circuit breaker scenarios:
    /// - Custom metrics and monitoring integration
    /// - Integration with external health monitoring systems
    /// - Advanced failure detection algorithms
    /// - Performance tuning parameters
    /// 
    /// Common advanced properties:
    /// - Custom failure detection logic
    /// - Integration with service mesh circuit breakers
    /// - Metric collection and reporting configuration
    /// - Advanced state transition logging and monitoring
    /// </remarks>
    public IDictionary<string, object>? Properties { get; set; }

    /// <summary>
    /// Creates a circuit breaker configuration optimized for fast failure detection.
    /// </summary>
    /// <param name="failureThreshold">The number of failures that trigger the circuit to open. Default is 3.</param>
    /// <param name="openDuration">The time to stay open before testing recovery. Default is 15 seconds.</param>
    /// <returns>A circuit breaker configuration for fast failure detection.</returns>
    /// <remarks>
    /// Fast failure detection is optimized for scenarios where:
    /// - Quick detection of failures is critical for user experience
    /// - Services tend to fail completely rather than degrade gradually
    /// - Fast feedback is more important than tolerance for brief failures
    /// - Real-time or interactive applications require immediate failure response
    /// 
    /// This configuration uses:
    /// - Lower failure thresholds for faster detection
    /// - Shorter open durations for quicker recovery testing
    /// - Fewer test requests for faster recovery decisions
    /// 
    /// Trade-offs:
    /// - May be too sensitive for services with occasional transient failures
    /// - Provides faster user feedback during outages
    /// - May not give failing services enough recovery time
    /// </remarks>
    public static CircuitBreakerConfiguration CreateFastFailure(
        int failureThreshold = 3,
        TimeSpan? openDuration = null)
    {
        return new CircuitBreakerConfiguration
        {
            FailureThreshold = failureThreshold,
            MeasurementWindow = TimeSpan.FromSeconds(30),
            MinimumRequestCount = 5,
            OpenDuration = openDuration ?? TimeSpan.FromSeconds(15),
            HalfOpenRequestCount = 2,
            Strategy = CircuitBreakerStrategy.ConsecutiveFailures,
            SuccessThreshold = 2
        };
    }

    /// <summary>
    /// Creates a circuit breaker configuration optimized for fault tolerance.
    /// </summary>
    /// <param name="failureThreshold">The number of failures that trigger the circuit to open. Default is 10.</param>
    /// <param name="openDuration">The time to stay open before testing recovery. Default is 2 minutes.</param>
    /// <returns>A circuit breaker configuration for fault tolerance.</returns>
    /// <remarks>
    /// Fault tolerance configuration is optimized for scenarios where:
    /// - Services may have occasional transient failures that don't indicate outages
    /// - Tolerance for brief failures is more important than immediate detection
    /// - Services have complex recovery patterns or longer recovery times
    /// - Background or batch processing where immediate response isn't critical
    /// 
    /// This configuration uses:
    /// - Higher failure thresholds for tolerance of transient issues
    /// - Longer open durations to give services more recovery time
    /// - More test requests for confident recovery assessment
    /// 
    /// Trade-offs:
    /// - May be slower to detect true outages
    /// - More tolerant of intermittent failures
    /// - Gives failing services more time to recover properly
    /// </remarks>
    public static CircuitBreakerConfiguration CreateFaultTolerant(
        int failureThreshold = 10,
        TimeSpan? openDuration = null)
    {
        return new CircuitBreakerConfiguration
        {
            FailureThreshold = failureThreshold,
            MeasurementWindow = TimeSpan.FromMinutes(2),
            MinimumRequestCount = 20,
            OpenDuration = openDuration ?? TimeSpan.FromMinutes(2),
            HalfOpenRequestCount = 5,
            Strategy = CircuitBreakerStrategy.FailureRate,
            SuccessThreshold = 4
        };
    }

    /// <summary>
    /// Creates a circuit breaker configuration using failure rate strategy for gradual degradation detection.
    /// </summary>
    /// <param name="failureRateThreshold">The failure rate percentage that triggers the circuit to open. Default is 50.</param>
    /// <param name="measurementWindow">The time window for measuring failure rates. Default is 1 minute.</param>
    /// <returns>A circuit breaker configuration using failure rate detection.</returns>
    /// <remarks>
    /// Failure rate configuration is optimized for scenarios where:
    /// - Services may degrade gradually rather than fail completely
    /// - Both successes and failures occur during problem periods
    /// - Statistical analysis of failure patterns is valuable
    /// - Services have complex failure modes that aren't binary
    /// 
    /// This approach:
    /// - Tracks the percentage of failures over a time window
    /// - Can detect degraded performance even when some requests succeed
    /// - Provides more nuanced failure detection than simple consecutive counting
    /// - Works well for services with variable performance characteristics
    /// 
    /// Use this strategy when services exhibit partial failures or performance
    /// degradation rather than complete outages.
    /// </remarks>
    public static CircuitBreakerConfiguration CreateFailureRateBased(
        int failureRateThreshold = 50,
        TimeSpan? measurementWindow = null)
    {
        return new CircuitBreakerConfiguration
        {
            FailureThreshold = failureRateThreshold, // Interpreted as percentage for failure rate strategy
            MeasurementWindow = measurementWindow ?? TimeSpan.FromMinutes(1),
            MinimumRequestCount = 10,
            OpenDuration = TimeSpan.FromSeconds(45),
            HalfOpenRequestCount = 5,
            Strategy = CircuitBreakerStrategy.FailureRate,
            SuccessThreshold = 4
        };
    }

    /// <summary>
    /// Validates the circuit breaker configuration for consistency and reasonableness.
    /// </summary>
    /// <exception cref="BrokerConfigurationException">Thrown when the circuit breaker configuration is invalid.</exception>
    /// <remarks>
    /// Validation ensures that circuit breaker parameters are reasonable and will result
    /// in effective failure detection and recovery behavior.
    /// 
    /// Validation checks include:
    /// - Positive values for thresholds and counts
    /// - Reasonable time durations that aren't too short or too long
    /// - Consistency between related parameters
    /// - Strategy-specific parameter validation
    /// 
    /// Early validation helps prevent runtime issues and provides clear feedback
    /// about configuration problems during application startup.
    /// </remarks>
    public void Validate()
    {
        if (FailureThreshold <= 0)
        {
            throw new BrokerConfigurationException(
                "Failure threshold must be positive.",
                configurationSection: nameof(CircuitBreakerConfiguration),
                parameterName: nameof(FailureThreshold),
                parameterValue: FailureThreshold);
        }

        if (Strategy == CircuitBreakerStrategy.FailureRate && FailureThreshold > 100)
        {
            throw new BrokerConfigurationException(
                "Failure rate threshold cannot exceed 100%.",
                configurationSection: nameof(CircuitBreakerConfiguration),
                parameterName: nameof(FailureThreshold),
                parameterValue: FailureThreshold);
        }

        if (MeasurementWindow <= TimeSpan.Zero)
        {
            throw new BrokerConfigurationException(
                "Measurement window must be positive.",
                configurationSection: nameof(CircuitBreakerConfiguration),
                parameterName: nameof(MeasurementWindow),
                parameterValue: MeasurementWindow);
        }

        if (MeasurementWindow > TimeSpan.FromHours(1))
        {
            throw new BrokerConfigurationException(
                "Measurement window should not exceed 1 hour to ensure timely failure detection.",
                configurationSection: nameof(CircuitBreakerConfiguration),
                parameterName: nameof(MeasurementWindow),
                parameterValue: MeasurementWindow);
        }

        if (MinimumRequestCount <= 0)
        {
            throw new BrokerConfigurationException(
                "Minimum request count must be positive.",
                configurationSection: nameof(CircuitBreakerConfiguration),
                parameterName: nameof(MinimumRequestCount),
                parameterValue: MinimumRequestCount);
        }

        if (OpenDuration <= TimeSpan.Zero)
        {
            throw new BrokerConfigurationException(
                "Open duration must be positive.",
                configurationSection: nameof(CircuitBreakerConfiguration),
                parameterName: nameof(OpenDuration),
                parameterValue: OpenDuration);
        }

        if (OpenDuration > TimeSpan.FromMinutes(30))
        {
            throw new BrokerConfigurationException(
                "Open duration should not exceed 30 minutes to prevent excessive outage periods.",
                configurationSection: nameof(CircuitBreakerConfiguration),
                parameterName: nameof(OpenDuration),
                parameterValue: OpenDuration);
        }

        if (HalfOpenRequestCount <= 0)
        {
            throw new BrokerConfigurationException(
                "Half-open request count must be positive.",
                configurationSection: nameof(CircuitBreakerConfiguration),
                parameterName: nameof(HalfOpenRequestCount),
                parameterValue: HalfOpenRequestCount);
        }

        if (HalfOpenRequestCount > 20)
        {
            throw new BrokerConfigurationException(
                "Half-open request count should not exceed 20 to limit load on recovering services.",
                configurationSection: nameof(CircuitBreakerConfiguration),
                parameterName: nameof(HalfOpenRequestCount),
                parameterValue: HalfOpenRequestCount);
        }

        if (SuccessThreshold <= 0)
        {
            throw new BrokerConfigurationException(
                "Success threshold must be positive.",
                configurationSection: nameof(CircuitBreakerConfiguration),
                parameterName: nameof(SuccessThreshold),
                parameterValue: SuccessThreshold);
        }

        if (SuccessThreshold > HalfOpenRequestCount)
        {
            throw new BrokerConfigurationException(
                "Success threshold cannot exceed the number of half-open test requests.",
                configurationSection: nameof(CircuitBreakerConfiguration),
                parameterName: nameof(SuccessThreshold),
                parameterValue: SuccessThreshold);
        }
    }

    /// <summary>
    /// Returns a string representation of the circuit breaker configuration.
    /// </summary>
    /// <returns>A formatted string describing the circuit breaker settings.</returns>
    public override string ToString()
    {
        var strategyInfo = Strategy.ToString().ToLowerInvariant();
        var thresholdInfo = Strategy == CircuitBreakerStrategy.FailureRate 
            ? $"{FailureThreshold}%" 
            : FailureThreshold.ToString();
        
        return $"CircuitBreaker[{strategyInfo}, threshold: {thresholdInfo}, open: {OpenDuration.TotalSeconds}s]";
    }
}
