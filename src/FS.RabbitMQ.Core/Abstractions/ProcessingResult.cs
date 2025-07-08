namespace FS.RabbitMQ.Core.Abstractions;

/// <summary>
/// Contains the result of a message processing operation.
/// </summary>
/// <remarks>
/// Processing results communicate the outcome of message processing
/// and provide guidance for subsequent operations including acknowledgment,
/// retry, and error handling decisions.
/// </remarks>
public sealed record ProcessingResult
{
    /// <summary>
    /// Gets a value indicating whether the message was processed successfully.
    /// </summary>
    /// <value><c>true</c> if processing completed successfully; otherwise, <c>false</c>.</value>
    public bool IsSuccess { get; init; }

    /// <summary>
    /// Gets the recommended action to take based on the processing result.
    /// </summary>
    /// <value>The action that should be performed following message processing.</value>
    public ProcessingAction Action { get; init; } = ProcessingAction.Acknowledge;

    /// <summary>
    /// Gets additional context or reasoning for the processing result.
    /// </summary>
    /// <value>A description of the processing outcome, or null if not provided.</value>
    public string? Message { get; init; }

    /// <summary>
    /// Gets additional metadata about the processing operation.
    /// </summary>
    /// <value>A dictionary containing supplementary processing information.</value>
    public IReadOnlyDictionary<string, object>? Metadata { get; init; }

    /// <summary>
    /// Gets the processing duration for performance monitoring.
    /// </summary>
    /// <value>The time taken to process the message.</value>
    public TimeSpan ProcessingDuration { get; init; }

    /// <summary>
    /// Creates a successful processing result.
    /// </summary>
    /// <param name="message">Optional message describing the successful processing.</param>
    /// <param name="processingDuration">The duration of the processing operation.</param>
    /// <param name="metadata">Optional metadata about the processing operation.</param>
    /// <returns>A processing result indicating success.</returns>
    public static ProcessingResult Success(string? message = null, TimeSpan processingDuration = default, IReadOnlyDictionary<string, object>? metadata = null) =>
        new()
        {
            IsSuccess = true,
            Action = ProcessingAction.Acknowledge,
            Message = message,
            ProcessingDuration = processingDuration,
            Metadata = metadata
        };

    /// <summary>
    /// Creates a failed processing result with retry recommendation.
    /// </summary>
    /// <param name="message">Optional message describing the processing failure.</param>
    /// <param name="processingDuration">The duration of the failed processing operation.</param>
    /// <param name="metadata">Optional metadata about the processing failure.</param>
    /// <returns>A processing result indicating failure with retry.</returns>
    public static ProcessingResult FailureWithRetry(string? message = null, TimeSpan processingDuration = default, IReadOnlyDictionary<string, object>? metadata = null) =>
        new()
        {
            IsSuccess = false,
            Action = ProcessingAction.Retry,
            Message = message,
            ProcessingDuration = processingDuration,
            Metadata = metadata
        };

    /// <summary>
    /// Creates a failed processing result with dead letter recommendation.
    /// </summary>
    /// <param name="message">Optional message describing the processing failure.</param>
    /// <param name="processingDuration">The duration of the failed processing operation.</param>
    /// <param name="metadata">Optional metadata about the processing failure.</param>
    /// <returns>A processing result indicating failure with dead letter routing.</returns>
    public static ProcessingResult FailureWithDeadLetter(string? message = null, TimeSpan processingDuration = default, IReadOnlyDictionary<string, object>? metadata = null) =>
        new()
        {
            IsSuccess = false,
            Action = ProcessingAction.DeadLetter,
            Message = message,
            ProcessingDuration = processingDuration,
            Metadata = metadata
        };
}