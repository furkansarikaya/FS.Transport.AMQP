namespace FS.RabbitMQ.Core.Abstractions;

/// <summary>
/// Defines error handling strategies for different types of processing failures.
/// </summary>
/// <remarks>
/// Error handling strategies provide specific actions to take when processing
/// failures occur, enabling sophisticated error recovery and failure isolation
/// patterns. Strategies can be combined and customized to handle complex
/// failure scenarios and business requirements.
/// </remarks>
public sealed class ErrorHandlingStrategy
{
    /// <summary>
    /// Gets the type of action to take when this strategy is applied.
    /// </summary>
    /// <value>The error handling action that defines the strategy behavior.</value>
    public ErrorHandlingAction Action { get; }

    /// <summary>
    /// Gets the retry policy to use if the action is retry-based.
    /// </summary>
    /// <value>The retry policy configuration, or null if the action doesn't involve retries.</value>
    public RetryPolicy? RetryPolicy { get; }

    /// <summary>
    /// Gets the custom error handler function for advanced scenarios.
    /// </summary>
    /// <value>A custom error handling function, or null if using standard actions.</value>
    public Func<ErrorContext, Task<ErrorHandlingResult>>? CustomHandler { get; }

    /// <summary>
    /// Gets additional metadata for the error handling strategy.
    /// </summary>
    /// <value>A dictionary containing strategy-specific configuration and metadata.</value>
    public IReadOnlyDictionary<string, object>? Metadata { get; }

    /// <summary>
    /// Initializes a new instance of the ErrorHandlingStrategy class.
    /// </summary>
    /// <param name="action">The error handling action to take.</param>
    /// <param name="retryPolicy">The retry policy for retry-based actions.</param>
    /// <param name="customHandler">A custom error handling function.</param>
    /// <param name="metadata">Additional strategy metadata.</param>
    private ErrorHandlingStrategy(
        ErrorHandlingAction action,
        RetryPolicy? retryPolicy = null,
        Func<ErrorContext, Task<ErrorHandlingResult>>? customHandler = null,
        IReadOnlyDictionary<string, object>? metadata = null)
    {
        Action = action;
        RetryPolicy = retryPolicy;
        CustomHandler = customHandler;
        Metadata = metadata;
    }

    /// <summary>
    /// Creates a strategy that immediately sends failed messages to the dead letter exchange.
    /// </summary>
    /// <param name="reason">Optional reason for dead letter routing.</param>
    /// <returns>An error handling strategy configured for immediate dead letter routing.</returns>
    /// <remarks>
    /// Dead letter strategy is appropriate for:
    /// - Permanent failures that will not recover with retry (data validation errors)
    /// - Poison messages that cause consumer failures
    /// - Security violations and authentication failures
    /// - Messages that exceed configured retry limits
    /// 
    /// Dead letter routing preserves failed messages for analysis and debugging
    /// while preventing them from blocking normal message processing.
    /// </remarks>
    public static ErrorHandlingStrategy CreateDeadLetter(string? reason = null)
    {
        var metadata = reason != null
            ? new Dictionary<string, object> { ["reason"] = reason }
            : null;

        return new ErrorHandlingStrategy(ErrorHandlingAction.DeadLetter, metadata: metadata);
    }

    /// <summary>
    /// Creates a strategy that discards failed messages without further processing.
    /// </summary>
    /// <param name="reason">Optional reason for message discarding.</param>
    /// <returns>An error handling strategy configured to discard failed messages.</returns>
    /// <remarks>
    /// Discard strategy should be used with caution and is appropriate for:
    /// - Non-critical messages where loss is acceptable
    /// - High-volume scenarios where individual message failures don't impact business outcomes
    /// - Test and development environments
    /// - Messages that have already been processed elsewhere
    /// 
    /// Warning: Discarded messages are permanently lost and cannot be recovered.
    /// Ensure that message loss is acceptable before using this strategy.
    /// </remarks>
    public static ErrorHandlingStrategy CreateDiscard(string? reason = null)
    {
        var metadata = reason != null
            ? new Dictionary<string, object> { ["reason"] = reason }
            : null;

        return new ErrorHandlingStrategy(ErrorHandlingAction.Discard, metadata: metadata);
    }

    /// <summary>
    /// Creates a strategy that retries failed messages with exponential backoff.
    /// </summary>
    /// <param name="maxAttempts">The maximum number of retry attempts.</param>
    /// <param name="baseDelay">The base delay for retry calculations.</param>
    /// <param name="maxDelay">The maximum delay between retries.</param>
    /// <returns>An error handling strategy configured for retry with exponential backoff.</returns>
    /// <remarks>
    /// Retry with backoff is appropriate for:
    /// - Transient network and connectivity failures
    /// - Temporary resource unavailability
    /// - Rate limiting and throttling scenarios
    /// - Downstream service temporary failures
    /// 
    /// Exponential backoff reduces load on failing systems over time,
    /// increasing the likelihood of successful recovery.
    /// </remarks>
    public static ErrorHandlingStrategy CreateRetryWithBackoff(
        int maxAttempts = 3,
        TimeSpan? baseDelay = null,
        TimeSpan? maxDelay = null)
    {
        var retryPolicy = RetryPolicy.CreateExponentialBackoff(
            baseDelay ?? TimeSpan.FromSeconds(1),
            maxDelay ?? TimeSpan.FromMinutes(5),
            maxAttempts);

        return new ErrorHandlingStrategy(ErrorHandlingAction.Retry, retryPolicy);
    }

    /// <summary>
    /// Creates a strategy that uses a custom retry policy.
    /// </summary>
    /// <param name="retryPolicy">The custom retry policy to use.</param>
    /// <returns>An error handling strategy configured with the specified retry policy.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="retryPolicy"/> is null.</exception>
    /// <remarks>
    /// Custom retry policies enable:
    /// - Application-specific retry algorithms
    /// - Integration with external timing and scheduling systems
    /// - Complex retry logic based on multiple factors
    /// - Adaptive retry behavior based on runtime conditions
    /// </remarks>
    public static ErrorHandlingStrategy CreateCustomRetry(RetryPolicy retryPolicy)
    {
        ArgumentNullException.ThrowIfNull(retryPolicy);
        return new ErrorHandlingStrategy(ErrorHandlingAction.Retry, retryPolicy);
    }

    /// <summary>
    /// Creates a strategy that uses custom error handling logic.
    /// </summary>
    /// <param name="customHandler">The custom error handling function.</param>
    /// <param name="metadata">Optional metadata for the custom strategy.</param>
    /// <returns>An error handling strategy configured with custom handling logic.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="customHandler"/> is null.</exception>
    /// <remarks>
    /// Custom error handling enables:
    /// - Integration with external error handling systems
    /// - Business-specific error recovery logic
    /// - Complex decision trees based on error context
    /// - Dynamic strategy selection based on runtime conditions
    /// 
    /// Custom handlers receive full error context and can implement
    /// any combination of retry, dead letter, discard, or escalation logic.
    /// </remarks>
    public static ErrorHandlingStrategy CreateCustom(
        Func<ErrorContext, Task<ErrorHandlingResult>> customHandler,
        IReadOnlyDictionary<string, object>? metadata = null)
    {
        ArgumentNullException.ThrowIfNull(customHandler);
        return new ErrorHandlingStrategy(ErrorHandlingAction.Custom, customHandler: customHandler, metadata: metadata);
    }

    /// <summary>
    /// Returns a string representation of the error handling strategy.
    /// </summary>
    /// <returns>A formatted string describing the strategy configuration.</returns>
    public override string ToString()
    {
        return Action switch
        {
            ErrorHandlingAction.Retry => $"Retry[{RetryPolicy}]",
            ErrorHandlingAction.DeadLetter => "DeadLetter",
            ErrorHandlingAction.Discard => "Discard",
            ErrorHandlingAction.Custom => "Custom",
            _ => Action.ToString()
        };
    }
}