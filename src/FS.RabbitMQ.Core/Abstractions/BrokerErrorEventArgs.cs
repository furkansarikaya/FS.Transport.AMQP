using System.Net.Sockets;

namespace FS.RabbitMQ.Core.Abstractions;

/// <summary>
/// Provides data for the ErrorOccurred event.
/// </summary>
/// <remarks>
/// Broker error events capture exceptional conditions that occur during message broker operations
/// but cannot be attributed to specific user operations. These events are essential for:
/// - Implementing comprehensive error monitoring and alerting
/// - Diagnosing systemic issues with broker connectivity or configuration
/// - Providing detailed error information for support and debugging
/// - Implementing error-based fallback or recovery strategies
/// 
/// Error events typically indicate infrastructure-level issues rather than application-level
/// exceptions, making them particularly important for operational monitoring.
/// </remarks>
public sealed class BrokerErrorEventArgs : EventArgs
{
    /// <summary>
    /// Gets the exception that caused the error.
    /// </summary>
    /// <value>The exception instance containing error details, stack trace, and type information.</value>
    /// <remarks>
    /// The exception provides the most detailed information about the error condition:
    /// - Exception type indicates the category of error (network, protocol, configuration)
    /// - Message provides human-readable error description
    /// - Stack trace enables detailed debugging and issue resolution
    /// - Inner exceptions may provide additional context for nested error conditions
    /// 
    /// Applications should log the complete exception information for diagnostic purposes
    /// while presenting appropriate user-friendly messages in user interfaces.
    /// </remarks>
    public Exception Exception { get; }

    /// <summary>
    /// Gets the operational context in which the error occurred.
    /// </summary>
    /// <value>A description of the broker operation or context when the error happened.</value>
    /// <remarks>
    /// Operational context helps identify the specific broker functionality that encountered the error:
    /// - "Connection Management": Errors during connection establishment or maintenance
    /// - "Channel Operations": Errors during channel creation or management
    /// - "Queue Declaration": Errors during queue setup or validation
    /// - "Exchange Declaration": Errors during exchange setup or validation
    /// - "Message Publishing": Errors during message routing or delivery
    /// - "Message Consumption": Errors during message retrieval or acknowledgment
    /// - "Background Recovery": Errors during automatic connection or channel recovery
    /// 
    /// Context information enables targeted troubleshooting and helps correlate errors
    /// with specific application operations or infrastructure components.
    /// </remarks>
    public string OperationContext { get; }

    /// <summary>
    /// Gets the timestamp when the error occurred.
    /// </summary>
    /// <value>The UTC timestamp when the error was detected.</value>
    /// <remarks>
    /// Accurate timestamps are essential for:
    /// - Correlating errors with external events (network issues, broker restarts)
    /// - Analyzing error patterns and frequency over time
    /// - Implementing time-based error handling strategies
    /// - Generating accurate audit trails and compliance reports
    /// </remarks>
    public DateTimeOffset Timestamp { get; }

    /// <summary>
    /// Gets the severity level of the error.
    /// </summary>
    /// <value>The severity classification of the error condition.</value>
    /// <remarks>
    /// Error severity helps applications and operators prioritize response actions:
    /// - Critical: System is unusable, immediate intervention required
    /// - High: Major functionality impacted, urgent attention needed
    /// - Medium: Some functionality impacted, attention needed during business hours
    /// - Low: Minor issues, can be addressed during routine maintenance
    /// - Informational: Notable events that don't require action but should be logged
    /// 
    /// Severity levels enable appropriate alerting, escalation procedures, and
    /// automated response strategies based on the impact of the error condition.
    /// </remarks>
    public ErrorSeverity Severity { get; }

    /// <summary>
    /// Gets additional metadata about the error condition, if available.
    /// </summary>
    /// <value>A dictionary containing supplementary information about the error, or null if no additional data is available.</value>
    /// <remarks>
    /// Error metadata may include:
    /// - Connection identifiers for network-related errors
    /// - Channel numbers for channel-specific issues
    /// - Queue or exchange names for topology-related problems
    /// - Message identifiers for message-processing errors
    /// - Broker version information for compatibility issues
    /// - Network endpoint details for connectivity problems
    /// 
    /// Metadata provides context that can be crucial for diagnostic and troubleshooting
    /// processes, especially in complex distributed messaging scenarios.
    /// </remarks>
    public IReadOnlyDictionary<string, object>? Metadata { get; }

    /// <summary>
    /// Initializes a new instance of the BrokerErrorEventArgs class.
    /// </summary>
    /// <param name="exception">The exception that caused the error. Cannot be null.</param>
    /// <param name="operationContext">The operational context where the error occurred. Cannot be null or whitespace.</param>
    /// <param name="severity">The severity level of the error.</param>
    /// <param name="metadata">Optional additional metadata about the error condition.</param>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="exception"/> is null.</exception>
    /// <exception cref="ArgumentException">Thrown when <paramref name="operationContext"/> is null, empty, or whitespace.</exception>
    public BrokerErrorEventArgs(Exception exception, string operationContext, ErrorSeverity severity = ErrorSeverity.Medium, IReadOnlyDictionary<string, object>? metadata = null)
    {
        Exception = exception ?? throw new ArgumentNullException(nameof(exception));
        OperationContext = !string.IsNullOrWhiteSpace(operationContext) 
            ? operationContext 
            : throw new ArgumentException("Operation context cannot be null or empty.", nameof(operationContext));
        Severity = severity;
        Metadata = metadata;
        Timestamp = DateTimeOffset.UtcNow;
    }

    /// <summary>
    /// Gets a value indicating whether the error is likely recoverable through automatic retry mechanisms.
    /// </summary>
    /// <value><c>true</c> if the error condition might be temporary and recoverable; otherwise, <c>false</c>.</value>
    /// <remarks>
    /// Recoverability assessment is based on exception type and error characteristics:
    /// - Network connectivity issues: Often recoverable with retry
    /// - Temporary broker unavailability: Usually recoverable
    /// - Configuration errors: Typically not recoverable without manual intervention
    /// - Authentication failures: Not recoverable without credential updates
    /// - Protocol violations: Usually not recoverable automatically
    /// 
    /// This property helps applications decide whether to implement automatic retry
    /// logic or escalate for manual intervention.
    /// </remarks>
    public bool IsRecoverable
    {
        get
        {
            // Network and connectivity issues are often recoverable
            if (Exception is SocketException or TimeoutException or HttpRequestException)
                return true;

            // Protocol and configuration errors are typically not recoverable
            if (Exception is ArgumentException or InvalidOperationException or UnauthorizedAccessException)
                return false;

            // For unknown exception types, err on the side of caution
            return Severity <= ErrorSeverity.Medium;
        }
    }

    /// <summary>
    /// Gets a value indicating whether the error requires immediate attention from operators.
    /// </summary>
    /// <value><c>true</c> if the error requires urgent attention; otherwise, <c>false</c>.</value>
    /// <remarks>
    /// Criticality is determined by severity level and error characteristics:
    /// - Critical and High severity errors always require immediate attention
    /// - Medium severity errors may require attention based on context
    /// - Low and Informational errors typically don't require immediate response
    /// 
    /// This property is useful for implementing alerting and escalation policies
    /// that ensure appropriate response times for different error conditions.
    /// </remarks>
    public bool RequiresImmediateAttention => Severity >= ErrorSeverity.High;

    /// <summary>
    /// Returns a string representation of the broker error event.
    /// </summary>
    /// <returns>A formatted string describing the error with context and timestamp.</returns>
    public override string ToString()
    {
        var metadataInfo = Metadata?.Any() == true ? $" (with {Metadata.Count} metadata items)" : "";
        return $"[{Severity}] Error in {OperationContext}: {Exception.Message} at {Timestamp:yyyy-MM-dd HH:mm:ss} UTC{metadataInfo}";
    }
}