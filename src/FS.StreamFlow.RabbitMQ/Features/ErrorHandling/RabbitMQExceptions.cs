using System.Runtime.Serialization;

namespace FS.StreamFlow.RabbitMQ.Features.ErrorHandling;

/// <summary>
/// Base exception for RabbitMQ client errors
/// </summary>
[Serializable]
public class RabbitMQClientException : Exception
{
    /// <summary>
    /// Initializes a new instance of the RabbitMQClientException class
    /// </summary>
    public RabbitMQClientException() { }

    /// <summary>
    /// Initializes a new instance of the RabbitMQClientException class with a specified error message
    /// </summary>
    /// <param name="message">The message that describes the error</param>
    public RabbitMQClientException(string message) : base(message) { }

    /// <summary>
    /// Initializes a new instance of the RabbitMQClientException class with a specified error message and inner exception
    /// </summary>
    /// <param name="message">The message that describes the error</param>
    /// <param name="innerException">The exception that is the cause of the current exception</param>
    public RabbitMQClientException(string message, Exception innerException) : base(message, innerException) { }

    /// <summary>
    /// Initializes a new instance of the RabbitMQClientException class with serialized data
    /// </summary>
    /// <param name="info">The SerializationInfo that holds the serialized object data about the exception being thrown</param>
    /// <param name="context">The StreamingContext that contains contextual information about the source or destination</param>
    protected RabbitMQClientException(SerializationInfo info, StreamingContext context) : base(info, context) { }
}

/// <summary>
/// Exception thrown when attempting to use a closed channel or connection
/// </summary>
[Serializable]
public class AlreadyClosedException : RabbitMQClientException
{
    /// <summary>
    /// Initializes a new instance of the AlreadyClosedException class
    /// </summary>
    public AlreadyClosedException() { }

    /// <summary>
    /// Initializes a new instance of the AlreadyClosedException class with a specified error message
    /// </summary>
    /// <param name="message">The message that describes the error</param>
    public AlreadyClosedException(string message) : base(message) { }

    /// <summary>
    /// Initializes a new instance of the AlreadyClosedException class with a specified error message and inner exception
    /// </summary>
    /// <param name="message">The message that describes the error</param>
    /// <param name="innerException">The exception that is the cause of the current exception</param>
    public AlreadyClosedException(string message, Exception innerException) : base(message, innerException) { }

    /// <summary>
    /// Initializes a new instance of the AlreadyClosedException class with serialized data
    /// </summary>
    /// <param name="info">The SerializationInfo that holds the serialized object data about the exception being thrown</param>
    /// <param name="context">The StreamingContext that contains contextual information about the source or destination</param>
    protected AlreadyClosedException(SerializationInfo info, StreamingContext context) : base(info, context) { }
}

/// <summary>
/// Exception thrown when RabbitMQ broker is unreachable
/// </summary>
[Serializable]
public class BrokerUnreachableException : RabbitMQClientException
{
    /// <summary>
    /// Initializes a new instance of the BrokerUnreachableException class
    /// </summary>
    public BrokerUnreachableException() { }

    /// <summary>
    /// Initializes a new instance of the BrokerUnreachableException class with a specified error message
    /// </summary>
    /// <param name="message">The message that describes the error</param>
    public BrokerUnreachableException(string message) : base(message) { }

    /// <summary>
    /// Initializes a new instance of the BrokerUnreachableException class with a specified error message and inner exception
    /// </summary>
    /// <param name="message">The message that describes the error</param>
    /// <param name="innerException">The exception that is the cause of the current exception</param>
    public BrokerUnreachableException(string message, Exception innerException) : base(message, innerException) { }

    /// <summary>
    /// Initializes a new instance of the BrokerUnreachableException class with serialized data
    /// </summary>
    /// <param name="info">The SerializationInfo that holds the serialized object data about the exception being thrown</param>
    /// <param name="context">The StreamingContext that contains contextual information about the source or destination</param>
    protected BrokerUnreachableException(SerializationInfo info, StreamingContext context) : base(info, context) { }
}

/// <summary>
/// Exception thrown when connection to RabbitMQ fails
/// </summary>
[Serializable]
public class ConnectFailureException : RabbitMQClientException
{
    /// <summary>
    /// Initializes a new instance of the ConnectFailureException class
    /// </summary>
    public ConnectFailureException() { }

    /// <summary>
    /// Initializes a new instance of the ConnectFailureException class with a specified error message
    /// </summary>
    /// <param name="message">The message that describes the error</param>
    public ConnectFailureException(string message) : base(message) { }

    /// <summary>
    /// Initializes a new instance of the ConnectFailureException class with a specified error message and inner exception
    /// </summary>
    /// <param name="message">The message that describes the error</param>
    /// <param name="innerException">The exception that is the cause of the current exception</param>
    public ConnectFailureException(string message, Exception innerException) : base(message, innerException) { }

    /// <summary>
    /// Initializes a new instance of the ConnectFailureException class with serialized data
    /// </summary>
    /// <param name="info">The SerializationInfo that holds the serialized object data about the exception being thrown</param>
    /// <param name="context">The StreamingContext that contains contextual information about the source or destination</param>
    protected ConnectFailureException(SerializationInfo info, StreamingContext context) : base(info, context) { }
}

/// <summary>
/// Exception thrown when an operation is interrupted
/// </summary>
[Serializable]
public class OperationInterruptedException : RabbitMQClientException
{
    /// <summary>
    /// Initializes a new instance of the OperationInterruptedException class
    /// </summary>
    public OperationInterruptedException() { }

    /// <summary>
    /// Initializes a new instance of the OperationInterruptedException class with a specified error message
    /// </summary>
    /// <param name="message">The message that describes the error</param>
    public OperationInterruptedException(string message) : base(message) { }

    /// <summary>
    /// Initializes a new instance of the OperationInterruptedException class with a specified error message and inner exception
    /// </summary>
    /// <param name="message">The message that describes the error</param>
    /// <param name="innerException">The exception that is the cause of the current exception</param>
    public OperationInterruptedException(string message, Exception innerException) : base(message, innerException) { }

    /// <summary>
    /// Initializes a new instance of the OperationInterruptedException class with serialized data
    /// </summary>
    /// <param name="info">The SerializationInfo that holds the serialized object data about the exception being thrown</param>
    /// <param name="context">The StreamingContext that contains contextual information about the source or destination</param>
    protected OperationInterruptedException(SerializationInfo info, StreamingContext context) : base(info, context) { }
}

/// <summary>
/// Exception thrown when message serialization fails
/// </summary>
[Serializable]
public class SerializationException : Exception
{
    /// <summary>
    /// Initializes a new instance of the SerializationException class
    /// </summary>
    public SerializationException() { }

    /// <summary>
    /// Initializes a new instance of the SerializationException class with a specified error message
    /// </summary>
    /// <param name="message">The message that describes the error</param>
    public SerializationException(string message) : base(message) { }

    /// <summary>
    /// Initializes a new instance of the SerializationException class with a specified error message and inner exception
    /// </summary>
    /// <param name="message">The message that describes the error</param>
    /// <param name="innerException">The exception that is the cause of the current exception</param>
    public SerializationException(string message, Exception innerException) : base(message, innerException) { }

    /// <summary>
    /// Initializes a new instance of the SerializationException class with serialized data
    /// </summary>
    /// <param name="info">The SerializationInfo that holds the serialized object data about the exception being thrown</param>
    /// <param name="context">The StreamingContext that contains contextual information about the source or destination</param>
    protected SerializationException(SerializationInfo info, StreamingContext context) : base(info, context) { }
} 