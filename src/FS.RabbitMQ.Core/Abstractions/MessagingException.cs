namespace FS.RabbitMQ.Core.Abstractions;

/// <summary>
/// The base exception for all RabbitMQ messaging-related errors.
/// </summary>
/// <remarks>
/// This base class provides common functionality for all messaging exceptions
/// and enables catch blocks to handle all messaging-related errors uniformly
/// when needed. It includes additional context properties that are useful
/// for troubleshooting and monitoring messaging operations.
/// 
/// All specific messaging exceptions inherit from this base class to maintain
/// a consistent exception hierarchy and provide comprehensive error information.
/// </remarks>
[Serializable]
public abstract class MessagingException : Exception
{
    /// <summary>
    /// Gets the error code associated with this exception.
    /// </summary>
    /// <value>A string identifier for the specific error condition, or null if not applicable.</value>
    /// <remarks>
    /// Error codes enable:
    /// - Programmatic error handling based on specific error conditions
    /// - Consistent error identification across different exception types
    /// - Integration with monitoring and alerting systems
    /// - Localization and user-friendly error message mapping
    /// </remarks>
    public string? ErrorCode { get; }

    /// <summary>
    /// Gets additional context information about the error.
    /// </summary>
    /// <value>A dictionary containing supplementary error information, or null if not available.</value>
    /// <remarks>
    /// Context information may include:
    /// - Connection identifiers for network-related errors
    /// - Queue or exchange names for topology-related errors
    /// - Message identifiers for message-specific errors
    /// - Broker information for connectivity issues
    /// 
    /// This information is valuable for debugging, monitoring, and creating
    /// comprehensive audit trails for messaging operations.
    /// </remarks>
    public IReadOnlyDictionary<string, object>? Context { get; }

    /// <summary>
    /// Initializes a new instance of the MessagingException class.
    /// </summary>
    /// <param name="message">The error message that explains the reason for the exception.</param>
    /// <param name="errorCode">Optional error code for programmatic error handling.</param>
    /// <param name="context">Optional additional context information about the error.</param>
    protected MessagingException(string message, string? errorCode = null, IReadOnlyDictionary<string, object>? context = null)
        : base(message)
    {
        ErrorCode = errorCode;
        Context = context;
    }

    /// <summary>
    /// Initializes a new instance of the MessagingException class.
    /// </summary>
    /// <param name="message">The error message that explains the reason for the exception.</param>
    /// <param name="innerException">The exception that is the cause of the current exception.</param>
    /// <param name="errorCode">Optional error code for programmatic error handling.</param>
    /// <param name="context">Optional additional context information about the error.</param>
    protected MessagingException(string message, Exception innerException, string? errorCode = null, IReadOnlyDictionary<string, object>? context = null)
        : base(message, innerException)
    {
        ErrorCode = errorCode;
        Context = context;
    }

    /// <summary>
    /// Initializes a new instance of the MessagingException class with serialized data.
    /// </summary>
    /// <param name="info">The SerializationInfo that holds the serialized object data about the exception being thrown.</param>
    /// <param name="context">The StreamingContext that contains contextual information about the source or destination.</param>
#pragma warning disable SYSLIB0051
    protected MessagingException(System.Runtime.Serialization.SerializationInfo info, System.Runtime.Serialization.StreamingContext context)
        : base(info, context)
    {
        ErrorCode = info.GetString(nameof(ErrorCode));
        // Note: Context dictionary serialization would need custom implementation if required
    }
#pragma warning restore SYSLIB0051
}