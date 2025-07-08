namespace FS.RabbitMQ.Core.Abstractions;

/// <summary>
/// Contains context information for retry attempt callbacks.
/// </summary>
/// <remarks>
/// Retry attempt context provides detailed information about retry execution
/// for logging, monitoring, and custom retry logic implementation.
/// </remarks>
public sealed record RetryAttemptContext
{
    /// <summary>
    /// Gets the retry context information.
    /// </summary>
    /// <value>The context containing attempt history and timing information.</value>
    public required RetryContext Context { get; init; }

    /// <summary>
    /// Gets the exception that triggered this retry attempt.
    /// </summary>
    /// <value>The exception that caused the previous attempt to fail.</value>
    public required Exception Exception { get; init; }

    /// <summary>
    /// Gets the calculated delay before this retry attempt.
    /// </summary>
    /// <value>The duration that was waited before executing this retry attempt.</value>
    public required TimeSpan Delay { get; init; }

    /// <summary>
    /// Gets additional metadata about the retry attempt.
    /// </summary>
    /// <value>A dictionary containing supplementary retry attempt information, or null if no additional data is available.</value>
    public IReadOnlyDictionary<string, object>? Metadata { get; init; }
}