namespace FS.RabbitMQ.Core.Abstractions;

/// <summary>
/// Exception thrown when broker connection operations fail.
/// </summary>
/// <remarks>
/// Connection exceptions occur when the messaging infrastructure cannot establish
/// or maintain connections to the RabbitMQ broker due to network issues, authentication
/// failures, or broker availability problems. These exceptions help identify
/// connectivity-related issues that may require infrastructure intervention.
/// </remarks>
[Serializable]
public sealed class BrokerConnectionException : MessagingException
{
    /// <summary>
    /// Gets the broker endpoint that was being connected to.
    /// </summary>
    /// <value>The broker connection string or endpoint, or null if not available.</value>
    public string? BrokerEndpoint { get; }

    /// <summary>
    /// Gets the connection attempt count when the error occurred.
    /// </summary>
    /// <value>The number of connection attempts made, or null if not tracked.</value>
    public int? AttemptCount { get; }

    /// <summary>
    /// Gets the duration of the connection attempt before failure.
    /// </summary>
    /// <value>The time spent attempting to connect, or null if not measured.</value>
    public TimeSpan? AttemptDuration { get; }

    /// <summary>
    /// Initializes a new instance of the BrokerConnectionException class.
    /// </summary>
    /// <param name="message">The error message describing the connection failure.</param>
    /// <param name="brokerEndpoint">The broker endpoint that failed to connect.</param>
    /// <param name="attemptCount">The number of connection attempts made.</param>
    /// <param name="attemptDuration">The duration of the connection attempt.</param>
    /// <param name="errorCode">Optional error code for programmatic error handling.</param>
    /// <param name="context">Optional additional context information.</param>
    public BrokerConnectionException(string message, string? brokerEndpoint = null, int? attemptCount = null, TimeSpan? attemptDuration = null, string? errorCode = null, IReadOnlyDictionary<string, object>? context = null)
        : base(message, errorCode, context)
    {
        BrokerEndpoint = brokerEndpoint;
        AttemptCount = attemptCount;
        AttemptDuration = attemptDuration;
    }

    /// <summary>
    /// Initializes a new instance of the BrokerConnectionException class.
    /// </summary>
    /// <param name="message">The error message describing the connection failure.</param>
    /// <param name="innerException">The exception that caused the connection failure.</param>
    /// <param name="brokerEndpoint">The broker endpoint that failed to connect.</param>
    /// <param name="attemptCount">The number of connection attempts made.</param>
    /// <param name="attemptDuration">The duration of the connection attempt.</param>
    /// <param name="errorCode">Optional error code for programmatic error handling.</param>
    /// <param name="context">Optional additional context information.</param>
    public BrokerConnectionException(string message, Exception innerException, string? brokerEndpoint = null, int? attemptCount = null, TimeSpan? attemptDuration = null, string? errorCode = null, IReadOnlyDictionary<string, object>? context = null)
        : base(message, innerException, errorCode, context)
    {
        BrokerEndpoint = brokerEndpoint;
        AttemptCount = attemptCount;
        AttemptDuration = attemptDuration;
    }

    /// <summary>
    /// Initializes a new instance of the BrokerConnectionException class with serialized data.
    /// </summary>
    /// <param name="info">The SerializationInfo that holds the serialized object data.</param>
    /// <param name="context">The StreamingContext that contains contextual information.</param>
    private BrokerConnectionException(System.Runtime.Serialization.SerializationInfo info, System.Runtime.Serialization.StreamingContext context)
        : base(info, context)
    {
        BrokerEndpoint = info.GetString(nameof(BrokerEndpoint));
        AttemptCount = (int?)info.GetValue(nameof(AttemptCount), typeof(int?));
        // Note: TimeSpan serialization would need special handling
    }
}