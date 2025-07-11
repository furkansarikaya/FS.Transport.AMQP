namespace FS.Transport.AMQP.Core.Exceptions;

/// <summary>
/// Exception thrown when queue-related errors occur
/// </summary>
public class QueueException : RabbitMQException
{
    /// <summary>
    /// Gets the name of the queue that caused the exception
    /// </summary>
    /// <value>The queue name, or null if not specified</value>
    public string? QueueName { get; }

    /// <summary>
    /// Initializes a new instance of the <see cref="QueueException"/> class with a specified message and queue name
    /// </summary>
    /// <param name="message">The message that describes the error</param>
    /// <param name="queueName">The name of the queue that caused the exception</param>
    public QueueException(string message, string? queueName) 
        : base(message, "QUEUE_ERROR")
    {
        QueueName = queueName;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="QueueException"/> class with a specified message, inner exception, and queue name
    /// </summary>
    /// <param name="message">The message that describes the error</param>
    /// <param name="innerException">The exception that is the cause of the current exception</param>
    /// <param name="queueName">The name of the queue that caused the exception</param>
    public QueueException(string message, Exception innerException, string? queueName) 
        : base(message, innerException, "QUEUE_ERROR")
    {
        QueueName = queueName;
    }
}