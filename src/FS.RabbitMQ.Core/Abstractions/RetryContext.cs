namespace FS.RabbitMQ.Core.Abstractions;

/// <summary>
/// Contains context information for retry decision making.
/// </summary>
/// <remarks>
/// Retry context provides comprehensive information about the retry state
/// and history, enabling sophisticated retry decision logic based on
/// attempt patterns, timing, and processing characteristics.
/// </remarks>
public sealed record RetryContext
{
    /// <summary>
    /// Gets the current attempt number (1-based).
    /// </summary>
    /// <value>The number of the current attempt, starting from 1 for the first attempt.</value>
    public required int AttemptNumber { get; init; }

    /// <summary>
    /// Gets the timestamp when the first attempt was made.
    /// </summary>
    /// <value>The UTC timestamp of the initial processing attempt.</value>
    public required DateTimeOffset FirstAttemptTime { get; init; }

    /// <summary>
    /// Gets the timestamp when the current attempt was made.
    /// </summary>
    /// <value>The UTC timestamp of the current attempt.</value>
    public required DateTimeOffset CurrentAttemptTime { get; init; }

    /// <summary>
    /// Gets the total duration across all retry attempts.
    /// </summary>
    /// <value>The time elapsed since the first attempt began.</value>
    public TimeSpan TotalDuration => CurrentAttemptTime - FirstAttemptTime;

    /// <summary>
    /// Gets additional context metadata for retry decisions.
    /// </summary>
    /// <value>A dictionary containing supplementary retry context information, or null if no additional data is available.</value>
    public IReadOnlyDictionary<string, object>? Metadata { get; init; }
}
