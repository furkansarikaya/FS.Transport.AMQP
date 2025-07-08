namespace FS.RabbitMQ.Core.Abstractions;

/// <summary>
/// Defines retry behavior and policies for handling transient failures in message processing.
/// </summary>
/// <remarks>
/// Retry policies provide sophisticated strategies for handling various types of processing
/// failures, from simple immediate retries to complex exponential backoff algorithms.
/// These policies are essential for building resilient messaging systems that can handle
/// transient network issues, resource constraints, and temporary service unavailability.
/// 
/// The policy system supports:
/// - Multiple retry strategies with different backoff algorithms
/// - Conditional retry based on exception types and contexts
/// - Maximum attempt limits to prevent infinite retry loops
/// - Dead letter handling for messages that exceed retry limits
/// - Integration with monitoring and observability systems
/// </remarks>
public sealed class RetryPolicy
{
    /// <summary>
    /// Gets the maximum number of retry attempts allowed.
    /// </summary>
    /// <value>The maximum retry attempts, including the initial attempt. Must be at least 1.</value>
    /// <remarks>
    /// Maximum attempts provide:
    /// - Protection against infinite retry loops
    /// - Predictable resource utilization and timeout behavior
    /// - Clear bounds for retry-related costs and latency
    /// - Integration with SLA and performance requirements
    /// 
    /// The count includes the initial processing attempt, so a value of 3
    /// means 1 initial attempt plus 2 retry attempts.
    /// </remarks>
    public int MaxAttempts { get; }

    /// <summary>
    /// Gets the base delay used for calculating retry intervals.
    /// </summary>
    /// <value>The base duration used as input to the backoff strategy calculation.</value>
    /// <remarks>
    /// Base delay serves as:
    /// - Starting point for exponential and polynomial backoff calculations
    /// - Fixed delay for linear and immediate retry strategies
    /// - Minimum delay floor for jittered backoff algorithms
    /// - Tuning parameter for optimizing retry timing
    /// 
    /// Optimal base delay depends on the nature of failures being handled
    /// and the recovery characteristics of downstream systems.
    /// </remarks>
    public TimeSpan BaseDelay { get; }

    /// <summary>
    /// Gets the maximum delay allowed between retry attempts.
    /// </summary>
    /// <value>The maximum duration to wait between retries, regardless of backoff calculation.</value>
    /// <remarks>
    /// Maximum delay provides:
    /// - Upper bound on retry intervals to prevent excessive waiting
    /// - Protection against runaway backoff calculations
    /// - Integration with timeout and SLA requirements
    /// - Predictable worst-case retry behavior
    /// 
    /// This limit is particularly important for exponential backoff strategies
    /// that can quickly grow to impractical delay intervals.
    /// </remarks>
    public TimeSpan MaxDelay { get; }

    /// <summary>
    /// Gets the backoff strategy used for calculating retry delays.
    /// </summary>
    /// <value>The algorithm used to determine the delay between successive retry attempts.</value>
    /// <remarks>
    /// Backoff strategies determine retry timing characteristics:
    /// - Immediate: No delay between retries (for fast recovery scenarios)
    /// - Linear: Fixed delay intervals (for predictable retry timing)
    /// - Exponential: Increasing delays (for systems that need recovery time)
    /// - Polynomial: Configurable growth rate (balance between linear and exponential)
    /// - Custom: Application-specific algorithms (for specialized requirements)
    /// 
    /// Strategy selection depends on failure patterns and system characteristics.
    /// </remarks>
    public BackoffStrategy Strategy { get; }

    /// <summary>
    /// Gets the jitter configuration for randomizing retry delays.
    /// </summary>
    /// <value>The jitter settings used to add randomness to calculated delays, or null for no jitter.</value>
    /// <remarks>
    /// Jitter helps prevent:
    /// - Thundering herd problems when multiple clients retry simultaneously
    /// - Synchronized retry patterns that can overwhelm recovering systems
    /// - Predictable load patterns that may cause recurring failures
    /// - Resource contention during system recovery periods
    /// 
    /// Jitter is particularly important in distributed systems with many
    /// concurrent consumers that may fail and retry simultaneously.
    /// </remarks>
    public JitterConfiguration? Jitter { get; }

    /// <summary>
    /// Gets the predicate function that determines whether an exception should trigger a retry.
    /// </summary>
    /// <value>A function that evaluates exceptions and contexts to determine retry eligibility, or null to retry all exceptions.</value>
    /// <remarks>
    /// Retry predicates enable:
    /// - Selective retry based on exception types and characteristics
    /// - Context-aware retry decisions based on processing state
    /// - Integration with business logic and error classification
    /// - Prevention of retry for permanent failure conditions
    /// 
    /// Common retry criteria include:
    /// - Network and connectivity exceptions (typically retryable)
    /// - Timeout and resource exhaustion (often retryable)
    /// - Authentication and authorization failures (usually not retryable)
    /// - Data validation errors (typically not retryable)
    /// </remarks>
    public Func<Exception, RetryContext, bool>? ShouldRetry { get; }

    /// <summary>
    /// Gets the callback function invoked before each retry attempt.
    /// </summary>
    /// <value>A function called before executing retry attempts, or null for no callback.</value>
    /// <remarks>
    /// Retry callbacks enable:
    /// - Logging and monitoring of retry attempts and patterns
    /// - Custom logic execution before retry (cleanup, state reset)
    /// - Integration with external monitoring and alerting systems
    /// - Adaptive behavior based on retry attempt characteristics
    /// 
    /// Callbacks are useful for implementing sophisticated retry strategies
    /// and providing visibility into retry behavior and effectiveness.
    /// </remarks>
    public Action<RetryAttemptContext>? OnRetry { get; }

    /// <summary>
    /// Initializes a new instance of the RetryPolicy class.
    /// </summary>
    /// <param name="maxAttempts">The maximum number of retry attempts.</param>
    /// <param name="baseDelay">The base delay for retry calculations.</param>
    /// <param name="maxDelay">The maximum delay between retries.</param>
    /// <param name="strategy">The backoff strategy to use.</param>
    /// <param name="jitter">The jitter configuration for randomizing delays.</param>
    /// <param name="shouldRetry">The predicate for determining retry eligibility.</param>
    /// <param name="onRetry">The callback function for retry attempts.</param>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when <paramref name="maxAttempts"/> is less than 1.</exception>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when <paramref name="baseDelay"/> or <paramref name="maxDelay"/> is negative.</exception>
    /// <exception cref="ArgumentException">Thrown when <paramref name="maxDelay"/> is less than <paramref name="baseDelay"/>.</exception>
    private RetryPolicy(
        int maxAttempts,
        TimeSpan baseDelay,
        TimeSpan maxDelay,
        BackoffStrategy strategy,
        JitterConfiguration? jitter = null,
        Func<Exception, RetryContext, bool>? shouldRetry = null,
        Action<RetryAttemptContext>? onRetry = null)
    {
        if (maxAttempts < 1)
            throw new ArgumentOutOfRangeException(nameof(maxAttempts), "Maximum attempts must be at least 1.");

        if (baseDelay < TimeSpan.Zero)
            throw new ArgumentOutOfRangeException(nameof(baseDelay), "Base delay cannot be negative.");

        if (maxDelay < TimeSpan.Zero)
            throw new ArgumentOutOfRangeException(nameof(maxDelay), "Maximum delay cannot be negative.");

        if (maxDelay < baseDelay)
            throw new ArgumentException("Maximum delay cannot be less than base delay.");

        MaxAttempts = maxAttempts;
        BaseDelay = baseDelay;
        MaxDelay = maxDelay;
        Strategy = strategy;
        Jitter = jitter;
        ShouldRetry = shouldRetry;
        OnRetry = onRetry;
    }

    /// <summary>
    /// Creates a retry policy with immediate retry (no delay between attempts).
    /// </summary>
    /// <param name="maxAttempts">The maximum number of retry attempts.</param>
    /// <param name="shouldRetry">Optional predicate for determining retry eligibility.</param>
    /// <param name="onRetry">Optional callback for retry attempts.</param>
    /// <returns>A retry policy configured for immediate retries.</returns>
    /// <remarks>
    /// Immediate retry is suitable for:
    /// - Transient failures that resolve quickly (network blips, temporary locks)
    /// - High-frequency processing where delay is unacceptable
    /// - Testing and development scenarios
    /// - Fast fail-fast recovery patterns
    /// 
    /// Use with caution in production as it provides no backpressure relief
    /// for systems under stress or experiencing resource constraints.
    /// </remarks>
    public static RetryPolicy CreateImmediate(
        int maxAttempts = 3,
        Func<Exception, RetryContext, bool>? shouldRetry = null,
        Action<RetryAttemptContext>? onRetry = null)
    {
        return new RetryPolicy(
            maxAttempts,
            TimeSpan.Zero,
            TimeSpan.Zero,
            BackoffStrategy.Immediate,
            jitter: null,
            shouldRetry,
            onRetry);
    }

    /// <summary>
    /// Creates a retry policy with fixed linear delays between attempts.
    /// </summary>
    /// <param name="delay">The fixed delay between retry attempts.</param>
    /// <param name="maxAttempts">The maximum number of retry attempts.</param>
    /// <param name="shouldRetry">Optional predicate for determining retry eligibility.</param>
    /// <param name="onRetry">Optional callback for retry attempts.</param>
    /// <returns>A retry policy configured for linear backoff.</returns>
    /// <remarks>
    /// Linear retry is suitable for:
    /// - Systems with predictable recovery times
    /// - Scenarios requiring consistent retry intervals
    /// - Resource contention that benefits from steady backpressure
    /// - Integration with external systems with known recovery characteristics
    /// 
    /// Linear retry provides predictable timing and resource utilization
    /// while giving failing systems consistent recovery time between attempts.
    /// </remarks>
    public static RetryPolicy CreateLinear(
        TimeSpan delay,
        int maxAttempts = 3,
        Func<Exception, RetryContext, bool>? shouldRetry = null,
        Action<RetryAttemptContext>? onRetry = null)
    {
        return new RetryPolicy(
            maxAttempts,
            delay,
            delay,
            BackoffStrategy.Linear,
            jitter: null,
            shouldRetry,
            onRetry);
    }

    /// <summary>
    /// Creates a retry policy with exponential backoff delays.
    /// </summary>
    /// <param name="baseDelay">The base delay for the first retry attempt.</param>
    /// <param name="maxDelay">The maximum delay between retry attempts.</param>
    /// <param name="maxAttempts">The maximum number of retry attempts.</param>
    /// <param name="jitter">Optional jitter configuration for randomizing delays.</param>
    /// <param name="shouldRetry">Optional predicate for determining retry eligibility.</param>
    /// <param name="onRetry">Optional callback for retry attempts.</param>
    /// <returns>A retry policy configured for exponential backoff.</returns>
    /// <remarks>
    /// Exponential backoff is suitable for:
    /// - Systems that need increasing recovery time under stress
    /// - Network and connectivity failures with variable recovery times
    /// - Resource exhaustion scenarios requiring progressive backpressure
    /// - Integration with external systems that benefit from reduced retry frequency
    /// 
    /// Exponential backoff reduces load on failing systems over time,
    /// increasing the likelihood of successful recovery while preventing
    /// retry traffic from interfering with recovery efforts.
    /// </remarks>
    public static RetryPolicy CreateExponentialBackoff(
        TimeSpan? baseDelay = null,
        TimeSpan? maxDelay = null,
        int maxAttempts = 3,
        JitterConfiguration? jitter = null,
        Func<Exception, RetryContext, bool>? shouldRetry = null,
        Action<RetryAttemptContext>? onRetry = null)
    {
        var effectiveBaseDelay = baseDelay ?? TimeSpan.FromSeconds(1);
        var effectiveMaxDelay = maxDelay ?? TimeSpan.FromMinutes(5);

        return new RetryPolicy(
            maxAttempts,
            effectiveBaseDelay,
            effectiveMaxDelay,
            BackoffStrategy.Exponential,
            jitter,
            shouldRetry,
            onRetry);
    }

    /// <summary>
    /// Creates a retry policy with polynomial backoff delays.
    /// </summary>
    /// <param name="baseDelay">The base delay for retry calculations.</param>
    /// <param name="maxDelay">The maximum delay between retry attempts.</param>
    /// <param name="power">The polynomial power for backoff calculation (default is 2.0 for quadratic).</param>
    /// <param name="maxAttempts">The maximum number of retry attempts.</param>
    /// <param name="jitter">Optional jitter configuration for randomizing delays.</param>
    /// <param name="shouldRetry">Optional predicate for determining retry eligibility.</param>
    /// <param name="onRetry">Optional callback for retry attempts.</param>
    /// <returns>A retry policy configured for polynomial backoff.</returns>
    /// <remarks>
    /// Polynomial backoff provides:
    /// - Configurable growth rate between linear and exponential
    /// - Fine-tuned control over retry timing characteristics
    /// - Balance between recovery time and retry frequency
    /// - Customization for specific system recovery patterns
    /// 
    /// Power values:
    /// - 1.0 = Linear backoff
    /// - 2.0 = Quadratic backoff (default)
    /// - Values > 2.0 = Super-quadratic growth (approaching exponential)
    /// - Values between 1.0 and 2.0 = Sub-quadratic growth
    /// </remarks>
    public static RetryPolicy CreatePolynomial(
        TimeSpan baseDelay,
        TimeSpan maxDelay,
        double power = 2.0,
        int maxAttempts = 3,
        JitterConfiguration? jitter = null,
        Func<Exception, RetryContext, bool>? shouldRetry = null,
        Action<RetryAttemptContext>? onRetry = null)
    {
        return new RetryPolicy(
            maxAttempts,
            baseDelay,
            maxDelay,
            BackoffStrategy.CreatePolynomial(power),
            jitter,
            shouldRetry,
            onRetry);
    }

    /// <summary>
    /// Creates a retry policy with a custom delay calculation function.
    /// </summary>
    /// <param name="delayCalculator">The function that calculates delay based on attempt number.</param>
    /// <param name="maxDelay">The maximum delay between retry attempts.</param>
    /// <param name="maxAttempts">The maximum number of retry attempts.</param>
    /// <param name="jitter">Optional jitter configuration for randomizing delays.</param>
    /// <param name="shouldRetry">Optional predicate for determining retry eligibility.</param>
    /// <param name="onRetry">Optional callback for retry attempts.</param>
    /// <returns>A retry policy configured with custom delay calculation.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="delayCalculator"/> is null.</exception>
    /// <remarks>
    /// Custom retry policies enable:
    /// - Application-specific backoff algorithms
    /// - Integration with external timing and scheduling systems
    /// - Complex delay calculations based on multiple factors
    /// - Adaptive retry behavior based on runtime conditions
    /// 
    /// The delay calculator receives the current attempt number (1-based)
    /// and should return the desired delay before the next attempt.
    /// The returned delay will be capped by the maxDelay parameter.
    /// </remarks>
    public static RetryPolicy CreateCustom(
        Func<int, TimeSpan> delayCalculator,
        TimeSpan maxDelay,
        int maxAttempts = 3,
        JitterConfiguration? jitter = null,
        Func<Exception, RetryContext, bool>? shouldRetry = null,
        Action<RetryAttemptContext>? onRetry = null)
    {
        ArgumentNullException.ThrowIfNull(delayCalculator);

        return new RetryPolicy(
            maxAttempts,
            TimeSpan.Zero, // Base delay not used for custom calculations
            maxDelay,
            BackoffStrategy.CreateCustom(delayCalculator),
            jitter,
            shouldRetry,
            onRetry);
    }

    /// <summary>
    /// Calculates the delay for a specific retry attempt.
    /// </summary>
    /// <param name="attemptNumber">The current attempt number (1-based).</param>
    /// <returns>The calculated delay duration before the next retry attempt.</returns>
    /// <remarks>
    /// Delay calculation incorporates:
    /// - Base backoff strategy algorithm
    /// - Jitter randomization if configured
    /// - Maximum delay capping
    /// - Custom calculation functions for advanced strategies
    /// 
    /// The calculated delay provides the foundation for retry timing
    /// and is used by the retry execution engine to schedule attempts.
    /// </remarks>
    public TimeSpan CalculateDelay(int attemptNumber)
    {
        var baseCalculatedDelay = Strategy.CalculateDelay(attemptNumber, BaseDelay);
        var cappedDelay = baseCalculatedDelay > MaxDelay ? MaxDelay : baseCalculatedDelay;
        
        return Jitter?.ApplyJitter(cappedDelay) ?? cappedDelay;
    }

    /// <summary>
    /// Determines whether a retry should be attempted for the given exception and context.
    /// </summary>
    /// <param name="exception">The exception that occurred during processing.</param>
    /// <param name="context">The retry context containing attempt history and metadata.</param>
    /// <returns><c>true</c> if a retry should be attempted; otherwise, <c>false</c>.</returns>
    /// <remarks>
    /// Retry decision logic considers:
    /// - Maximum attempt limits and current attempt count
    /// - Exception type and characteristics
    /// - Custom retry predicate evaluation
    /// - Context-specific retry conditions
    /// 
    /// This method provides the primary decision point for retry execution
    /// and integrates all configured retry criteria and constraints.
    /// </remarks>
    public bool ShouldRetryException(Exception exception, RetryContext context)
    {
        if (context.AttemptNumber >= MaxAttempts)
            return false;

        return ShouldRetry?.Invoke(exception, context) ?? true;
    }

    /// <summary>
    /// Returns a string representation of the retry policy configuration.
    /// </summary>
    /// <returns>A formatted string describing the retry policy settings.</returns>
    public override string ToString()
    {
        var jitterInfo = Jitter != null ? $", jitter: {Jitter}" : "";
        return $"RetryPolicy[{Strategy}, maxAttempts: {MaxAttempts}, baseDelay: {BaseDelay}, maxDelay: {MaxDelay}{jitterInfo}]";
    }
}