using System.Net.Sockets;

namespace FS.RabbitMQ.Core.Abstractions;

/// <summary>
/// Provides data for the ConsumerErrorOccurred event.
/// </summary>
/// <remarks>
/// Consumer error events capture detailed information about exceptions and error
/// conditions that occur during message processing, enabling comprehensive error
/// handling, logging, and recovery strategies.
/// </remarks>
public sealed class ConsumerErrorEventArgs : EventArgs
{
    /// <summary>
    /// Gets the consumer identifier where the error occurred.
    /// </summary>
    /// <value>The unique identifier of the consumer that experienced the error.</value>
    public string ConsumerId { get; }

    /// <summary>
    /// Gets the exception that caused the error.
    /// </summary>
    /// <value>The exception instance containing detailed error information.</value>
    public Exception Exception { get; }

    /// <summary>
    /// Gets the message context associated with the error, if available.
    /// </summary>
    /// <value>The message context that was being processed when the error occurred, or null if not applicable.</value>
    public object? MessageContext { get; }

    /// <summary>
    /// Gets the operational context where the error occurred.
    /// </summary>
    /// <value>A description of the consumer operation that was executing when the error happened.</value>
    public string OperationContext { get; }

    /// <summary>
    /// Gets the timestamp when the error occurred.
    /// </summary>
    /// <value>The UTC timestamp when the error was detected.</value>
    public DateTimeOffset Timestamp { get; }

    /// <summary>
    /// Gets the severity level of the error.
    /// </summary>
    /// <value>The severity classification of the error condition.</value>
    public ErrorSeverity Severity { get; }

    /// <summary>
    /// Gets additional metadata about the error condition, if available.
    /// </summary>
    /// <value>A dictionary containing supplementary error information, or null if no additional data is available.</value>
    public IReadOnlyDictionary<string, object>? Metadata { get; }

    /// <summary>
    /// Initializes a new instance of the ConsumerErrorEventArgs class.
    /// </summary>
    /// <param name="consumerId">The unique identifier of the consumer.</param>
    /// <param name="exception">The exception that caused the error.</param>
    /// <param name="operationContext">The operational context where the error occurred.</param>
    /// <param name="severity">The severity level of the error.</param>
    /// <param name="messageContext">Optional message context associated with the error.</param>
    /// <param name="metadata">Optional additional metadata about the error.</param>
    /// <exception cref="ArgumentException">Thrown when <paramref name="consumerId"/> or <paramref name="operationContext"/> is null, empty, or whitespace.</exception>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="exception"/> is null.</exception>
    public ConsumerErrorEventArgs(
        string consumerId, 
        Exception exception, 
        string operationContext, 
        ErrorSeverity severity = ErrorSeverity.Medium,
        object? messageContext = null, 
        IReadOnlyDictionary<string, object>? metadata = null)
    {
        ConsumerId = !string.IsNullOrWhiteSpace(consumerId) 
            ? consumerId 
            : throw new ArgumentException("Consumer ID cannot be null or empty.", nameof(consumerId));
        Exception = exception ?? throw new ArgumentNullException(nameof(exception));
        OperationContext = !string.IsNullOrWhiteSpace(operationContext) 
            ? operationContext 
            : throw new ArgumentException("Operation context cannot be null or empty.", nameof(operationContext));
        Severity = severity;
        MessageContext = messageContext;
        Metadata = metadata;
        Timestamp = DateTimeOffset.UtcNow;
    }

    /// <summary>
    /// Gets a value indicating whether the error is likely recoverable through automatic retry mechanisms.
    /// </summary>
    /// <value><c>true</c> if the error condition might be temporary and recoverable; otherwise, <c>false</c>.</value>
    public bool IsRecoverable
    {
        get
        {
            return Exception switch
            {
                // Network and connectivity issues are often recoverable
                SocketException or TimeoutException or HttpRequestException => true,
                // Protocol and configuration errors are typically not recoverable
                ArgumentException or InvalidOperationException or UnauthorizedAccessException => false,
                _ => Severity <= ErrorSeverity.Medium
            };

            // For unknown exception types, err on the side of caution based on severity
        }
    }

    /// <summary>
    /// Gets a value indicating whether the error requires immediate attention from operators.
    /// </summary>
    /// <value><c>true</c> if the error requires urgent attention; otherwise, <c>false</c>.</value>
    public bool RequiresImmediateAttention => Severity >= ErrorSeverity.High;

    /// <summary>
    /// Returns a string representation of the consumer error event.
    /// </summary>
    /// <returns>A formatted string describing the error with context and timestamp.</returns>
    public override string ToString()
    {
        var metadataInfo = Metadata?.Any() == true ? $" (with {Metadata.Count} metadata items)" : "";
        return $"[{Severity}] Consumer {ConsumerId} error in {OperationContext}: {Exception.Message} at {Timestamp:yyyy-MM-dd HH:mm:ss} UTC{metadataInfo}";
    }
}