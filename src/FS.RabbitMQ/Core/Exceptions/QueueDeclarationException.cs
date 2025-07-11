namespace FS.RabbitMQ.Core.Exceptions;

/// <summary>
/// Exception thrown when queue declaration fails
/// </summary>
public class QueueDeclarationException : QueueException
{
    /// <summary>
    /// Initializes a new instance of the <see cref="QueueDeclarationException"/> class with a specified message and queue name
    /// </summary>
    /// <param name="message">The message that describes the error</param>
    /// <param name="queueName">The name of the queue that failed to be declared</param>
    public QueueDeclarationException(string message, string queueName) 
        : base(message, queueName)
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="QueueDeclarationException"/> class with a specified message and inner exception
    /// </summary>
    /// <param name="message">The message that describes the error</param>
    /// <param name="innerException">The exception that is the cause of the current exception</param>
    public QueueDeclarationException(string message, Exception innerException) 
        : base(message, innerException, null)
    {
    }
}