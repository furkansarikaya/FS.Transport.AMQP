namespace FS.RabbitMQ.Core.Abstractions;

/// <summary>
/// Exception thrown when consumer operations fail during runtime.
/// </summary>
/// <remarks>
/// Consumer exceptions occur during consumer operation and indicate
/// issues with message processing, connection management, or consumer
/// lifecycle events. These exceptions help identify operational issues
/// that require attention or intervention.
/// </remarks>
[Serializable]
public sealed class ConsumerException : MessagingException
{
    /// <summary>
    /// Gets the consumer identifier where the error occurred.
    /// </summary>
    /// <value>The unique consumer ID, or null if not available.</value>
    public string? ConsumerId { get; }

    /// <summary>
    /// Gets the operation that was being performed when the error occurred.
    /// </summary>
    /// <value>The operation name or description, or null if not specified.</value>
    public string? Operation { get; }

    /// <summary>
    /// Initializes a new instance of the ConsumerException class.
    /// </summary>
    /// <param name="message">The error message describing the consumer failure.</param>
    /// <param name="consumerId">The consumer identifier where the error occurred.</param>
    /// <param name="operation">The operation that was being performed.</param>
    /// <param name="errorCode">Optional error code for programmatic error handling.</param>
    /// <param name="context">Optional additional context information.</param>
    public ConsumerException(string message, string? consumerId = null, string? operation = null, string? errorCode = null, IReadOnlyDictionary<string, object>? context = null)
        : base(message, errorCode, context)
    {
        ConsumerId = consumerId;
        Operation = operation;
    }

    /// <summary>
    /// Initializes a new instance of the ConsumerException class.
    /// </summary>
    /// <param name="message">The error message describing the consumer failure.</param>
    /// <param name="innerException">The exception that caused the consumer failure.</param>
    /// <param name="consumerId">The consumer identifier where the error occurred.</param>
    /// <param name="operation">The operation that was being performed.</param>
    /// <param name="errorCode">Optional error code for programmatic error handling.</param>
    /// <param name="context">Optional additional context information.</param>
    public ConsumerException(string message, Exception innerException, string? consumerId = null, string? operation = null, string? errorCode = null, IReadOnlyDictionary<string, object>? context = null)
        : base(message, innerException, errorCode, context)
    {
        ConsumerId = consumerId;
        Operation = operation;
    }

    /// <summary>
    /// Initializes a new instance of the ConsumerException class with serialized data.
    /// </summary>
    /// <param name="info">The SerializationInfo that holds the serialized object data.</param>
    /// <param name="context">The StreamingContext that contains contextual information.</param>
    private ConsumerException(System.Runtime.Serialization.SerializationInfo info, System.Runtime.Serialization.StreamingContext context)
        : base(info, context)
    {
        ConsumerId = info.GetString(nameof(ConsumerId));
        Operation = info.GetString(nameof(Operation));
    }
}