namespace FS.RabbitMQ.Core.Abstractions;

/// <summary>
/// Contains context information for error handling operations.
/// </summary>
/// <remarks>
/// Error context provides comprehensive information about the failure
/// and processing state, enabling sophisticated error handling decisions
/// and custom recovery logic implementation.
/// </remarks>
public sealed record ErrorContext
{
    /// <summary>
    /// Gets the exception that occurred during processing.
    /// </summary>
    /// <value>The exception instance containing detailed error information.</value>
    public required Exception Exception { get; init; }

    /// <summary>
    /// Gets the message context that was being processed when the error occurred.
    /// </summary>
    /// <value>The message context containing message and routing information.</value>
    public required object MessageContext { get; init; }

    /// <summary>
    /// Gets the current attempt number for this message.
    /// </summary>
    /// <value>The number of processing attempts made for this message (1-based).</value>
    public required int AttemptNumber { get; init; }

    /// <summary>
    /// Gets the timestamp when the error occurred.
    /// </summary>
    /// <value>The UTC timestamp when the processing failure was detected.</value>
    public DateTimeOffset Timestamp { get; init; } = DateTimeOffset.UtcNow;

    /// <summary>
    /// Gets additional metadata about the error context.
    /// </summary>
    /// <value>A dictionary containing supplementary error context information.</value>
    public IReadOnlyDictionary<string, object>? Metadata { get; init; }
}