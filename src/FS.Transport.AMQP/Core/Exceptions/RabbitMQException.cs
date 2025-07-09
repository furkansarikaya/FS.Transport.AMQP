namespace FS.Transport.AMQP.Core.Exceptions;

/// <summary>
/// Base exception class for all RabbitMQ-related exceptions
/// </summary>
public class RabbitMQException : Exception
{
    /// <summary>
    /// Error code for categorizing the exception
    /// </summary>
    public string ErrorCode { get; }
    
    /// <summary>
    /// Additional context information about the error
    /// </summary>
    public IDictionary<string, object> Context { get; }

    public RabbitMQException(string message, string errorCode = "RABBITMQ_ERROR") 
        : base(message)
    {
        ErrorCode = errorCode;
        Context = new Dictionary<string, object>();
    }

    public RabbitMQException(string message, Exception innerException, string errorCode = "RABBITMQ_ERROR") 
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