namespace FS.RabbitMQ.Core.Abstractions;

/// <summary>
/// Exception thrown when consumer creation or initialization fails.
/// </summary>
/// <remarks>
/// Consumer creation exceptions occur when consumers cannot be properly
/// initialized due to configuration issues, resource constraints, or
/// connectivity problems. These exceptions help identify setup and
/// configuration issues that prevent normal consumer operation.
/// </remarks>
[Serializable]
public sealed class ConsumerCreationException : MessagingException
{
    /// <summary>
    /// Gets the type of consumer that failed to be created.
    /// </summary>
    /// <value>The consumer type identifier, or null if not specified.</value>
    public string? ConsumerType { get; }

    /// <summary>
    /// Gets the queue name that the consumer was attempting to bind to.
    /// </summary>
    /// <value>The queue name, or null if not applicable.</value>
    public string? QueueName { get; }

    /// <summary>
    /// Initializes a new instance of the ConsumerCreationException class.
    /// </summary>
    /// <param name="message">The error message describing the creation failure.</param>
    /// <param name="consumerType">The type of consumer that failed to be created.</param>
    /// <param name="queueName">The queue name the consumer was attempting to bind to.</param>
    /// <param name="errorCode">Optional error code for programmatic error handling.</param>
    /// <param name="context">Optional additional context information.</param>
    public ConsumerCreationException(string message, string? consumerType = null, string? queueName = null, string? errorCode = null, IReadOnlyDictionary<string, object>? context = null)
        : base(message, errorCode, context)
    {
        ConsumerType = consumerType;
        QueueName = queueName;
    }

    /// <summary>
    /// Initializes a new instance of the ConsumerCreationException class.
    /// </summary>
    /// <param name="message">The error message describing the creation failure.</param>
    /// <param name="innerException">The exception that caused the creation failure.</param>
    /// <param name="consumerType">The type of consumer that failed to be created.</param>
    /// <param name="queueName">The queue name the consumer was attempting to bind to.</param>
    /// <param name="errorCode">Optional error code for programmatic error handling.</param>
    /// <param name="context">Optional additional context information.</param>
    public ConsumerCreationException(string message, Exception innerException, string? consumerType = null, string? queueName = null, string? errorCode = null, IReadOnlyDictionary<string, object>? context = null)
        : base(message, innerException, errorCode, context)
    {
        ConsumerType = consumerType;
        QueueName = queueName;
    }

    /// <summary>
    /// Initializes a new instance of the ConsumerCreationException class with serialized data.
    /// </summary>
    /// <param name="info">The SerializationInfo that holds the serialized object data.</param>
    /// <param name="context">The StreamingContext that contains contextual information.</param>
    private ConsumerCreationException(System.Runtime.Serialization.SerializationInfo info, System.Runtime.Serialization.StreamingContext context)
        : base(info, context)
    {
        ConsumerType = info.GetString(nameof(ConsumerType));
        QueueName = info.GetString(nameof(QueueName));
    }
}