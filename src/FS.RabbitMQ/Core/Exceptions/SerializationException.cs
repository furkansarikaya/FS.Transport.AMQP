namespace FS.RabbitMQ.Core.Exceptions;

/// <summary>
/// Exception thrown when serialization/deserialization errors occur
/// </summary>
public class SerializationException : RabbitMQException
{
    public Type? ObjectType { get; }

    public SerializationException(string message, Type? objectType = null) 
        : base(message, "SERIALIZATION_ERROR")
    {
        ObjectType = objectType;
        if (objectType != null)
        {
            WithContext("ObjectType", objectType.FullName);
        }
    }

    public SerializationException(string message, Exception innerException, Type? objectType = null) 
        : base(message, innerException, "SERIALIZATION_ERROR")
    {
        ObjectType = objectType;
        if (objectType != null)
        {
            WithContext("ObjectType", objectType.FullName);
        }
    }
}