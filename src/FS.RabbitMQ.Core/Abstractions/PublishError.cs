namespace FS.RabbitMQ.Core.Abstractions;

/// <summary>
/// Contains detailed error information for failed publishing operations.
/// </summary>
/// <remarks>
/// Publishing errors provide comprehensive information about failure conditions,
/// enabling applications to implement appropriate error handling, retry logic,
/// and troubleshooting workflows. Error categorization helps distinguish between
/// different types of failures and implement appropriate response strategies.
/// </remarks>
public sealed record PublishError
{
    /// <summary>
    /// Gets the type of publishing error that occurred.
    /// </summary>
    /// <value>The error category that classifies the failure type.</value>
    /// <remarks>
    /// Error types enable:
    /// - Categorized error handling with type-specific strategies
    /// - Monitoring and alerting based on error patterns
    /// - Troubleshooting guidance based on error classification
    /// - Retry strategy selection based on error recoverability
    /// </remarks>
    public required PublishErrorType ErrorType { get; init; }

    /// <summary>
    /// Gets a human-readable description of the error.
    /// </summary>
    /// <value>A descriptive message explaining the error condition.</value>
    /// <remarks>
    /// Error messages provide:
    /// - Human-readable explanation for logging and debugging
    /// - Context information for troubleshooting
    /// - User-friendly error descriptions for UI scenarios
    /// - Detailed diagnostic information for support teams
    /// </remarks>
    public required string Message { get; init; }

    /// <summary>
    /// Gets the underlying exception that caused the error, if available.
    /// </summary>
    /// <value>The exception instance with detailed error information, or null if not applicable.</value>
    /// <remarks>
    /// Exception details provide:
    /// - Complete stack traces for debugging
    /// - Nested exception information for complex error scenarios
    /// - Type-specific error properties for specialized handling
    /// - Integration with existing exception handling infrastructure
    /// </remarks>
    public Exception? Exception { get; init; }

    /// <summary>
    /// Gets a value indicating whether the error condition is likely recoverable with retry.
    /// </summary>
    /// <value><c>true</c> if retrying the operation might succeed; otherwise, <c>false</c>.</value>
    /// <remarks>
    /// Recoverability assessment helps:
    /// - Implement intelligent retry strategies
    /// - Avoid unnecessary retry attempts for permanent failures
    /// - Optimize resource utilization in error scenarios
    /// - Provide appropriate user feedback for different error types
    /// 
    /// Assessment is based on error type characteristics:
    /// - Network and connectivity issues: Usually recoverable
    /// - Configuration and validation errors: Typically not recoverable
    /// - Resource exhaustion: May be recoverable after delay
    /// - Authorization failures: Usually not recoverable without intervention
    /// </remarks>
    public bool IsRecoverable { get; init; }

    /// <summary>
    /// Gets the recommended delay before retrying the operation, if applicable.
    /// </summary>
    /// <value>The suggested wait time before retry, or null if retry is not recommended.</value>
    /// <remarks>
    /// Retry delay recommendations help:
    /// - Implement appropriate backoff strategies
    /// - Avoid overwhelming systems during recovery
    /// - Optimize retry timing for different error types
    /// - Balance recovery speed with system stability
    /// 
    /// Delay recommendations consider:
    /// - Error type and severity
    /// - System load and resource availability
    /// - Historical recovery patterns
    /// - Best practices for the specific error condition
    /// </remarks>
    public TimeSpan? RecommendedRetryDelay { get; init; }

    /// <summary>
    /// Creates a new publish error with the specified details.
    /// </summary>
    /// <param name="errorType">The type of error that occurred.</param>
    /// <param name="message">A descriptive error message.</param>
    /// <param name="exception">The underlying exception, if available.</param>
    /// <param name="isRecoverable">Whether the error is likely recoverable with retry.</param>
    /// <param name="recommendedRetryDelay">The recommended delay before retrying, if applicable.</param>
    /// <returns>A new PublishError instance with the specified information.</returns>
    /// <exception cref="ArgumentException">Thrown when <paramref name="message"/> is null, empty, or whitespace.</exception>
    public static PublishError Create(
        PublishErrorType errorType,
        string message,
        Exception? exception = null,
        bool isRecoverable = true,
        TimeSpan? recommendedRetryDelay = null)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(message);

        return new PublishError
        {
            ErrorType = errorType,
            Message = message,
            Exception = exception,
            IsRecoverable = isRecoverable,
            RecommendedRetryDelay = recommendedRetryDelay
        };
    }

    /// <summary>
    /// Returns a string representation of the publish error.
    /// </summary>
    /// <returns>A formatted string describing the error condition.</returns>
    public override string ToString()
    {
        var recoverableInfo = IsRecoverable ? " (recoverable)" : " (not recoverable)";
        var retryInfo = RecommendedRetryDelay.HasValue ? $" - retry after {RecommendedRetryDelay}" : "";
        return $"{ErrorType}: {Message}{recoverableInfo}{retryInfo}";
    }
}