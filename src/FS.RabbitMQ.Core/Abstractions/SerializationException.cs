namespace FS.RabbitMQ.Core.Abstractions;

/// <summary>
/// Exception thrown when message serialization or deserialization operations fail.
/// </summary>
/// <remarks>
/// Serialization exceptions occur when messages cannot be converted to or from
/// their wire format due to data type issues, format compatibility problems,
/// or serializer configuration errors. These exceptions help identify data-related
/// issues that prevent proper message handling.
/// </remarks>
[Serializable]
public sealed class SerializationException : MessagingException
{
    /// <summary>
    /// Gets the type of the object that failed serialization.
    /// </summary>
    /// <value>The type information, or null if not available.</value>
    public Type? ObjectType { get; }

    /// <summary>
    /// Gets the serialization format that was being used.
    /// </summary>
    /// <value>The format identifier (e.g., "JSON", "XML", "Binary"), or null if not specified.</value>
    public string? SerializationFormat { get; }

    /// <summary>
    /// Gets a value indicating whether the error occurred during serialization or deserialization.
    /// </summary>
    /// <value><c>true</c> for serialization errors; <c>false</c> for deserialization errors.</value>
    public bool IsSerializationError { get; }

    /// <summary>
    /// Initializes a new instance of the SerializationException class.
    /// </summary>
    /// <param name="message">The error message describing the serialization failure.</param>
    /// <param name="isSerializationError">True if this is a serialization error, false for deserialization.</param>
    /// <param name="objectType">The type of object that failed serialization.</param>
    /// <param name="serializationFormat">The serialization format being used.</param>
    /// <param name="errorCode">Optional error code for programmatic error handling.</param>
    /// <param name="context">Optional additional context information.</param>
    public SerializationException(string message, bool isSerializationError, Type? objectType = null, string? serializationFormat = null, string? errorCode = null, IReadOnlyDictionary<string, object>? context = null)
        : base(message, errorCode, context)
    {
        IsSerializationError = isSerializationError;
        ObjectType = objectType;
        SerializationFormat = serializationFormat;
    }

    /// <summary>
    /// Initializes a new instance of the SerializationException class.
    /// </summary>
    /// <param name="message">The error message describing the serialization failure.</param>
    /// <param name="innerException">The exception that caused the serialization failure.</param>
    /// <param name="isSerializationError">True if this is a serialization error, false for deserialization.</param>
    /// <param name="objectType">The type of object that failed serialization.</param>
    /// <param name="serializationFormat">The serialization format being used.</param>
    /// <param name="errorCode">Optional error code for programmatic error handling.</param>
    /// <param name="context">Optional additional context information.</param>
    public SerializationException(string message, Exception innerException, bool isSerializationError, Type? objectType = null, string? serializationFormat = null, string? errorCode = null, IReadOnlyDictionary<string, object>? context = null)
        : base(message, innerException, errorCode, context)
    {
        IsSerializationError = isSerializationError;
        ObjectType = objectType;
        SerializationFormat = serializationFormat;
    }

    /// <summary>
    /// Initializes a new instance of the SerializationException class with serialized data.
    /// </summary>
    /// <param name="info">The SerializationInfo that holds the serialized object data.</param>
    /// <param name="context">The StreamingContext that contains contextual information.</param>
    private SerializationException(System.Runtime.Serialization.SerializationInfo info, System.Runtime.Serialization.StreamingContext context)
        : base(info, context)
    {
        IsSerializationError = info.GetBoolean(nameof(IsSerializationError));
        SerializationFormat = info.GetString(nameof(SerializationFormat));
        // Note: Type serialization would need special handling
    }
}