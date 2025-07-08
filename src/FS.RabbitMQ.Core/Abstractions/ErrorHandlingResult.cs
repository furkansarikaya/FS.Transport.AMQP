namespace FS.RabbitMQ.Core.Abstractions;

/// <summary>
/// Contains the result of custom error handling operations.
/// </summary>
/// <remarks>
/// Error handling results communicate the outcome of custom error handling
/// logic and provide instructions for subsequent processing decisions.
/// </remarks>
public sealed record ErrorHandlingResult
{
    /// <summary>
    /// Gets the action that should be taken based on the error handling result.
    /// </summary>
    /// <value>The recommended action for handling the error.</value>
    public required ErrorHandlingAction Action { get; init; }

    /// <summary>
    /// Gets the delay before executing the recommended action.
    /// </summary>
    /// <value>The duration to wait before taking the specified action.</value>
    public TimeSpan Delay { get; init; } = TimeSpan.Zero;

    /// <summary>
    /// Gets additional context or reasoning for the handling decision.
    /// </summary>
    /// <value>A description of why this action was chosen, or null if not provided.</value>
    public string? Reason { get; init; }

    /// <summary>
    /// Gets additional metadata about the error handling result.
    /// </summary>
    /// <value>A dictionary containing supplementary result information.</value>
    public IReadOnlyDictionary<string, object>? Metadata { get; init; }

    /// <summary>
    /// Creates a result indicating that the message should be retried.
    /// </summary>
    /// <param name="delay">The delay before retrying.</param>
    /// <param name="reason">The reason for retrying.</param>
    /// <returns>An error handling result configured for retry.</returns>
    public static ErrorHandlingResult Retry(TimeSpan delay = default, string? reason = null) =>
        new() { Action = ErrorHandlingAction.Retry, Delay = delay, Reason = reason };

    /// <summary>
    /// Creates a result indicating that the message should be sent to the dead letter exchange.
    /// </summary>
    /// <param name="reason">The reason for dead letter routing.</param>
    /// <returns>An error handling result configured for dead letter routing.</returns>
    public static ErrorHandlingResult DeadLetter(string? reason = null) =>
        new() { Action = ErrorHandlingAction.DeadLetter, Reason = reason };

    /// <summary>
    /// Creates a result indicating that the message should be discarded.
    /// </summary>
    /// <param name="reason">The reason for discarding the message.</param>
    /// <returns>An error handling result configured for message discarding.</returns>
    public static ErrorHandlingResult Discard(string? reason = null) =>
        new() { Action = ErrorHandlingAction.Discard, Reason = reason };
}