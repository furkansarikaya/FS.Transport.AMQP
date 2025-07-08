using System.Net.Sockets;
using System.Security;

namespace FS.RabbitMQ.Core.Abstractions;

/// <summary>
/// Defines comprehensive error handling strategies and policies for message processing failures.
/// </summary>
/// <remarks>
/// Error handling options provide sophisticated strategies for dealing with various types
/// of processing failures, from transient network issues to permanent data corruption.
/// These options work in conjunction with retry policies to create resilient messaging
/// systems that can handle complex failure scenarios while maintaining data integrity
/// and system stability.
/// 
/// The error handling system supports:
/// - Exception-specific handling strategies
/// - Dead letter queue configuration for poison messages
/// - Circuit breaker patterns for cascading failure prevention
/// - Custom error handling logic and escalation procedures
/// - Integration with monitoring and alerting systems
/// </remarks>
public sealed class ErrorHandlingOptions
{
    /// <summary>
    /// Gets the default error handling strategy for unspecified exception types.
    /// </summary>
    /// <value>The strategy used when no specific handler is configured for an exception type.</value>
    /// <remarks>
    /// Default strategy provides:
    /// - Fallback behavior for unexpected exception types
    /// - Consistent handling when specific strategies are not defined
    /// - Safety net for new exception types introduced during system evolution
    /// - Baseline error handling behavior for all processing failures
    /// 
    /// Common default strategies include retry with exponential backoff,
    /// immediate dead letter routing, or custom escalation procedures.
    /// </remarks>
    public ErrorHandlingStrategy DefaultStrategy { get; }

    /// <summary>
    /// Gets the exception-specific error handling strategies.
    /// </summary>
    /// <value>A dictionary mapping exception types to their specific handling strategies.</value>
    /// <remarks>
    /// Exception-specific strategies enable:
    /// - Tailored handling for different types of failures
    /// - Optimization of recovery approaches based on failure characteristics
    /// - Integration with domain-specific error handling requirements
    /// - Fine-grained control over retry and escalation behavior
    /// 
    /// Strategies can be configured for specific exception types,
    /// base exception classes, or interface types to provide hierarchical
    /// error handling with inheritance-based strategy selection.
    /// </remarks>
    public IReadOnlyDictionary<Type, ErrorHandlingStrategy> ExceptionStrategies { get; }

    /// <summary>
    /// Gets the dead letter exchange configuration for handling poison messages.
    /// </summary>
    /// <value>Configuration for routing failed messages to dead letter infrastructure, or null if not configured.</value>
    /// <remarks>
    /// Dead letter configuration provides:
    /// - Isolation of poison messages that cannot be processed successfully
    /// - Preservation of failed messages for analysis and debugging
    /// - Prevention of message loss during processing failures
    /// - Integration with error analysis and remediation workflows
    /// 
    /// Dead letter exchanges are essential for maintaining system stability
    /// and providing visibility into processing failures and message quality issues.
    /// </remarks>
    public DeadLetterConfiguration? DeadLetterConfig { get; }

    /// <summary>
    /// Gets the circuit breaker configuration for preventing cascading failures.
    /// </summary>
    /// <value>Configuration for circuit breaker behavior, or null if circuit breaking is not enabled.</value>
    /// <remarks>
    /// Circuit breaker configuration enables:
    /// - Automatic protection against cascading failures
    /// - Fast failure when downstream systems are unavailable
    /// - Automatic recovery detection and service restoration
    /// - Resource protection during widespread system failures
    /// 
    /// Circuit breakers are particularly important in distributed systems
    /// where failures in one component can quickly propagate and overwhelm
    /// other components, leading to complete system failure.
    /// </remarks>
    public CircuitBreakerConfiguration? CircuitBreakerConfig { get; }

    /// <summary>
    /// Gets the global error callback for custom error handling logic.
    /// </summary>
    /// <value>A callback function invoked for all processing errors, or null if no global handling is needed.</value>
    /// <remarks>
    /// Global error callback enables:
    /// - Cross-cutting error handling logic and monitoring
    /// - Integration with external logging and alerting systems
    /// - Custom error analysis and pattern detection
    /// - Centralized error handling for compliance and audit requirements
    /// 
    /// The callback is invoked for all errors regardless of their specific
    /// handling strategy, providing a centralized point for error observation
    /// and custom business logic implementation.
    /// </remarks>
    public Action<ErrorContext>? OnError { get; }

    /// <summary>
    /// Gets the poison message detection configuration.
    /// </summary>
    /// <value>Configuration for identifying and handling poison messages, or null if not configured.</value>
    /// <remarks>
    /// Poison message detection provides:
    /// - Automatic identification of messages that consistently fail processing
    /// - Prevention of infinite retry loops for unprocessable messages
    /// - Isolation of problematic messages for analysis and remediation
    /// - Protection of system resources from malformed or malicious messages
    /// 
    /// Poison detection typically uses delivery count thresholds,
    /// failure pattern analysis, or custom detection logic to identify
    /// messages that are unlikely to ever be processed successfully.
    /// </remarks>
    public PoisonMessageConfiguration? PoisonMessageConfig { get; }

    /// <summary>
    /// Initializes a new instance of the ErrorHandlingOptions class.
    /// </summary>
    /// <param name="defaultStrategy">The default error handling strategy.</param>
    /// <param name="exceptionStrategies">Exception-specific handling strategies.</param>
    /// <param name="deadLetterConfig">Dead letter exchange configuration.</param>
    /// <param name="circuitBreakerConfig">Circuit breaker configuration.</param>
    /// <param name="poisonMessageConfig">Poison message detection configuration.</param>
    /// <param name="onError">Global error callback function.</param>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="defaultStrategy"/> is null.</exception>
    private ErrorHandlingOptions(
        ErrorHandlingStrategy defaultStrategy,
        IReadOnlyDictionary<Type, ErrorHandlingStrategy>? exceptionStrategies = null,
        DeadLetterConfiguration? deadLetterConfig = null,
        CircuitBreakerConfiguration? circuitBreakerConfig = null,
        PoisonMessageConfiguration? poisonMessageConfig = null,
        Action<ErrorContext>? onError = null)
    {
        DefaultStrategy = defaultStrategy ?? throw new ArgumentNullException(nameof(defaultStrategy));
        ExceptionStrategies = exceptionStrategies ?? new Dictionary<Type, ErrorHandlingStrategy>();
        DeadLetterConfig = deadLetterConfig;
        CircuitBreakerConfig = circuitBreakerConfig;
        PoisonMessageConfig = poisonMessageConfig;
        OnError = onError;
    }

    /// <summary>
    /// Creates basic error handling options with retry and dead letter support.
    /// </summary>
    /// <param name="deadLetterExchange">The dead letter exchange name for failed messages.</param>
    /// <param name="maxRetryAttempts">The maximum number of retry attempts before dead letter routing.</param>
    /// <returns>Error handling options configured for basic retry and dead letter handling.</returns>
    /// <remarks>
    /// Default configuration includes:
    /// - Exponential backoff retry for transient failures
    /// - Dead letter routing for messages exceeding retry limits
    /// - Network exception handling with immediate retry
    /// - Timeout and resource exception handling with longer delays
    /// 
    /// This configuration is suitable for most standard messaging scenarios
    /// where basic retry and dead letter handling provide adequate resilience.
    /// </remarks>
    public static ErrorHandlingOptions CreateDefault(string? deadLetterExchange = null, int maxRetryAttempts = 3)
    {
        var strategies = new Dictionary<Type, ErrorHandlingStrategy>
        {
            [typeof(TimeoutException)] = ErrorHandlingStrategy.CreateRetryWithBackoff(maxRetryAttempts, TimeSpan.FromSeconds(5)),
            [typeof(SocketException)] = ErrorHandlingStrategy.CreateRetryWithBackoff(maxRetryAttempts, TimeSpan.FromSeconds(2)),
            [typeof(HttpRequestException)] = ErrorHandlingStrategy.CreateRetryWithBackoff(maxRetryAttempts, TimeSpan.FromSeconds(2)),
            [typeof(ArgumentException)] = ErrorHandlingStrategy.CreateDeadLetter(),
            [typeof(InvalidOperationException)] = ErrorHandlingStrategy.CreateDeadLetter(),
            [typeof(SerializationException)] = ErrorHandlingStrategy.CreateDeadLetter()
        };

        var deadLetterConfig = !string.IsNullOrEmpty(deadLetterExchange)
            ? DeadLetterConfiguration.Create(deadLetterExchange)
            : null;

        return new ErrorHandlingOptions(
            ErrorHandlingStrategy.CreateRetryWithBackoff(maxRetryAttempts),
            strategies,
            deadLetterConfig);
    }

    /// <summary>
    /// Creates comprehensive error handling options with advanced features.
    /// </summary>
    /// <param name="deadLetterExchange">The dead letter exchange name for failed messages.</param>
    /// <param name="circuitBreakerThreshold">The failure threshold for circuit breaker activation.</param>
    /// <param name="poisonMessageThreshold">The delivery count threshold for poison message detection.</param>
    /// <returns>Error handling options configured with comprehensive error handling features.</returns>
    /// <remarks>
    /// Comprehensive configuration includes:
    /// - Advanced retry strategies with different policies for different exception types
    /// - Circuit breaker protection against cascading failures
    /// - Poison message detection and isolation
    /// - Dead letter handling with detailed routing and analysis
    /// - Global error monitoring and alerting integration
    /// 
    /// This configuration is suitable for production systems requiring
    /// sophisticated error handling and failure isolation capabilities.
    /// </remarks>
    public static ErrorHandlingOptions CreateComprehensive(
        string deadLetterExchange = "failed.messages",
        int circuitBreakerThreshold = 5,
        int poisonMessageThreshold = 10)
    {
        var strategies = new Dictionary<Type, ErrorHandlingStrategy>
        {
            // Network and connectivity errors - aggressive retry
            [typeof(TimeoutException)] = ErrorHandlingStrategy.CreateRetryWithBackoff(5, TimeSpan.FromSeconds(2)),
            [typeof(SocketException)] = ErrorHandlingStrategy.CreateRetryWithBackoff(5, TimeSpan.FromSeconds(1)),
            [typeof(HttpRequestException)] = ErrorHandlingStrategy.CreateRetryWithBackoff(3, TimeSpan.FromSeconds(2)),
            
            // Data and validation errors - immediate dead letter
            [typeof(ArgumentException)] = ErrorHandlingStrategy.CreateDeadLetter(),
            [typeof(ArgumentNullException)] = ErrorHandlingStrategy.CreateDeadLetter(),
            [typeof(InvalidOperationException)] = ErrorHandlingStrategy.CreateDeadLetter(),
            [typeof(SerializationException)] = ErrorHandlingStrategy.CreateDeadLetter(),
            
            // Authentication and authorization - no retry
            [typeof(UnauthorizedAccessException)] = ErrorHandlingStrategy.CreateDeadLetter(),
            [typeof(SecurityException)] = ErrorHandlingStrategy.CreateDeadLetter(),
            
            // Resource and capacity errors - moderate retry with longer delays
            [typeof(OutOfMemoryException)] = ErrorHandlingStrategy.CreateRetryWithBackoff(2, TimeSpan.FromSeconds(10)),
            [typeof(TaskCanceledException)] = ErrorHandlingStrategy.CreateRetryWithBackoff(2, TimeSpan.FromSeconds(5))
        };

        return new ErrorHandlingOptions(
            ErrorHandlingStrategy.CreateRetryWithBackoff(3),
            strategies,
            DeadLetterConfiguration.Create(deadLetterExchange),
            CircuitBreakerConfiguration.Create(circuitBreakerThreshold),
            PoisonMessageConfiguration.Create(poisonMessageThreshold));
    }

    /// <summary>
    /// Creates error handling options optimized for high-throughput scenarios.
    /// </summary>
    /// <param name="deadLetterExchange">The dead letter exchange name for failed messages.</param>
    /// <returns>Error handling options configured for high-throughput processing.</returns>
    /// <remarks>
    /// High-throughput configuration includes:
    /// - Minimal retry attempts to maintain processing speed
    /// - Fast failure for non-retryable errors
    /// - Optimized dead letter routing for failed messages
    /// - Reduced overhead in error handling logic
    /// 
    /// This configuration prioritizes processing speed and throughput
    /// over comprehensive error recovery, making it suitable for scenarios
    /// where message volume is high and individual message failures are acceptable.
    /// </remarks>
    public static ErrorHandlingOptions CreateHighThroughput(string deadLetterExchange = "failed.messages")
    {
        var strategies = new Dictionary<Type, ErrorHandlingStrategy>
        {
            // Minimal retry for network errors only
            [typeof(TimeoutException)] = ErrorHandlingStrategy.CreateRetryWithBackoff(2, TimeSpan.FromSeconds(1)),
            [typeof(SocketException)] = ErrorHandlingStrategy.CreateRetryWithBackoff(2, TimeSpan.FromMilliseconds(500)),
            
            // All other errors go directly to dead letter
            [typeof(Exception)] = ErrorHandlingStrategy.CreateDeadLetter()
        };

        return new ErrorHandlingOptions(
            ErrorHandlingStrategy.CreateRetryWithBackoff(1), // Single retry attempt
            strategies,
            DeadLetterConfiguration.Create(deadLetterExchange));
    }

    /// <summary>
    /// Gets the appropriate error handling strategy for the specified exception.
    /// </summary>
    /// <param name="exception">The exception to find a strategy for.</param>
    /// <returns>The error handling strategy to use for the specified exception.</returns>
    /// <remarks>
    /// Strategy selection logic:
    /// 1. Check for exact type match in exception strategies
    /// 2. Check for base type matches in inheritance hierarchy
    /// 3. Check for interface matches if the exception implements specific interfaces
    /// 4. Fall back to the default strategy
    /// 
    /// This hierarchical approach enables flexible configuration where
    /// specific exception types can have specialized handling while
    /// maintaining sensible defaults for broader exception categories.
    /// </remarks>
    public ErrorHandlingStrategy GetStrategyForException(Exception exception)
    {
        var exceptionType = exception.GetType();

        // Direct type match
        if (ExceptionStrategies.TryGetValue(exceptionType, out var strategy))
            return strategy;

        // Check inheritance hierarchy
        var currentType = exceptionType.BaseType;
        while (currentType != null)
        {
            if (ExceptionStrategies.TryGetValue(currentType, out strategy))
                return strategy;
            currentType = currentType.BaseType;
        }

        // Check interfaces
        foreach (var interfaceType in exceptionType.GetInterfaces())
        {
            if (ExceptionStrategies.TryGetValue(interfaceType, out strategy))
                return strategy;
        }

        // Fall back to default strategy
        return DefaultStrategy;
    }

    /// <summary>
    /// Returns a string representation of the error handling options.
    /// </summary>
    /// <returns>A formatted string describing the error handling configuration.</returns>
    public override string ToString()
    {
        var strategies = ExceptionStrategies.Count;
        var features = new List<string> { $"default: {DefaultStrategy}" };
        
        if (strategies > 0) features.Add($"{strategies} specific strategies");
        if (DeadLetterConfig != null) features.Add("dead letter");
        if (CircuitBreakerConfig != null) features.Add("circuit breaker");
        if (PoisonMessageConfig != null) features.Add("poison detection");
        
        return $"ErrorHandling[{string.Join(", ", features)}]";
    }
}