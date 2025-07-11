namespace FS.RabbitMQ.Core.Exceptions;

/// <summary>
/// Exception thrown when serialization or deserialization errors occur
/// </summary>
public class SerializationException : RabbitMQException
{
    /// <summary>
    /// Gets the type of the object that failed to serialize or deserialize
    /// </summary>
    /// <value>The object type, or null if not specified</value>
    public Type? ObjectType { get; }

    /// <summary>
    /// Initializes a new instance of the <see cref="SerializationException"/> class with a specified message and object type
    /// </summary>
    /// <param name="message">The message that describes the error</param>
    /// <param name="objectType">The type of the object that failed to serialize or deserialize</param>
    public SerializationException(string message, Type? objectType) 
        : base(message, "SERIALIZATION_ERROR")
    {
        ObjectType = objectType;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="SerializationException"/> class with a specified message, inner exception, and object type
    /// </summary>
    /// <param name="message">The message that describes the error</param>
    /// <param name="innerException">The exception that is the cause of the current exception</param>
    /// <param name="objectType">The type of the object that failed to serialize or deserialize</param>
    public SerializationException(string message, Exception innerException, Type? objectType) 
        : base(message, innerException, "SERIALIZATION_ERROR")
    {
        ObjectType = objectType;
    }
}