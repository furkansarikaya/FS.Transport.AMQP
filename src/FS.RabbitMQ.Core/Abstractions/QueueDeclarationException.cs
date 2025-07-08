namespace FS.RabbitMQ.Core.Abstractions;

/// <summary>
/// Exception thrown when queue declaration operations fail.
/// </summary>
/// <remarks>
/// Queue declaration exceptions occur when queues cannot be created or validated
/// due to name conflicts, parameter mismatches, or resource constraints.
/// These exceptions help identify queue-specific topology issues.
/// </remarks>
[Serializable]
public sealed class QueueDeclarationException : MessagingException
{
    /// <summary>
    /// Gets the name of the queue that failed to declare.
    /// </summary>
    /// <value>The queue name, or null if not available.</value>
    public string? QueueName { get; }

    /// <summary>
    /// Initializes a new instance of the QueueDeclarationException class.
    /// </summary>
    /// <param name="message">The error message describing the declaration failure.</param>
    /// <param name="queueName">The name of the queue that failed to declare.</param>
    /// <param name="errorCode">Optional error code for programmatic error handling.</param>
    /// <param name="context">Optional additional context information.</param>
    public QueueDeclarationException(string message, string? queueName = null, string? errorCode = null, IReadOnlyDictionary<string, object>? context = null)
        : base(message, errorCode, context)
    {
        QueueName = queueName;
    }

    /// <summary>
    /// Initializes a new instance of the QueueDeclarationException class.
    /// </summary>
    /// <param name="message">The error message describing the declaration failure.</param>
    /// <param name="innerException">The exception that caused the declaration failure.</param>
    /// <param name="queueName">The name of the queue that failed to declare.</param>
    /// <param name="errorCode">Optional error code for programmatic error handling.</param>
    /// <param name="context">Optional additional context information.</param>
    public QueueDeclarationException(string message, Exception innerException, string? queueName = null, string? errorCode = null, IReadOnlyDictionary<string, object>? context = null)
        : base(message, innerException, errorCode, context)
    {
        QueueName = queueName;
    }

    /// <summary>
    /// Initializes a new instance of the QueueDeclarationException class with serialized data.
    /// </summary>
    /// <param name="info">The SerializationInfo that holds the serialized object data.</param>
    /// <param name="context">The StreamingContext that contains contextual information.</param>
    private QueueDeclarationException(System.Runtime.Serialization.SerializationInfo info, System.Runtime.Serialization.StreamingContext context)
        : base(info, context)
    {
        QueueName = info.GetString(nameof(QueueName));
    }
}