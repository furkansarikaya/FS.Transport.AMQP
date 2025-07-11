namespace FS.Transport.AMQP.Core.Exceptions;

/// <summary>
/// Base exception class for all RabbitMQ-related exceptions
/// </summary>
public class RabbitMQException : Exception
{
    /// <summary>
    /// Gets the error code associated with this exception
    /// </summary>
    /// <value>A string representing the error code</value>
    public string ErrorCode { get; }
    
    /// <summary>
    /// Additional context information about the error
    /// </summary>
    public IDictionary<string, object> Context { get; }

    /// <summary>
    /// Initializes a new instance of the <see cref="RabbitMQException"/> class with a specified error message
    /// </summary>
    /// <param name="message">The message that describes the error</param>
    public RabbitMQException(string message) 
        : base(message)
    {
        ErrorCode = "RABBITMQ_ERROR";
        Context = new Dictionary<string, object>();
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="RabbitMQException"/> class with a specified error message and error code
    /// </summary>
    /// <param name="message">The message that describes the error</param>
    /// <param name="errorCode">The error code associated with this exception</param>
    public RabbitMQException(string message, string errorCode) 
        : base(message)
    {
        ErrorCode = errorCode;
        Context = new Dictionary<string, object>();
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="RabbitMQException"/> class with a specified error message, inner exception, and error code
    /// </summary>
    /// <param name="message">The message that describes the error</param>
    /// <param name="innerException">The exception that is the cause of the current exception</param>
    /// <param name="errorCode">The error code associated with this exception</param>
    public RabbitMQException(string message, Exception innerException, string errorCode) 
        : base(message, innerException)
    {
        ErrorCode = errorCode;
        Context = new Dictionary<string, object>();
    }

    /// <summary>
    /// Adds context information to the exception
    /// </summary>
    /// <param name="key">Context key</param>
    /// <param name="value">Context value</param>
    /// <returns>The exception instance for fluent configuration</returns>
    public RabbitMQException WithContext(string key, object value)
    {
        Context[key] = value;
        return this;
    }
}