namespace FS.Transport.AMQP.Core.Exceptions;

/// <summary>
/// Exception thrown when queue declaration fails
/// </summary>
public class QueueDeclarationException : QueueException
{
    public QueueDeclarationException(string queueName, string reason) 
        : base($"Failed to declare queue '{queueName}': {reason}", queueName)
    {
    }

    public QueueDeclarationException(string queueName, Exception innerException) 
        : base($"Failed to declare queue '{queueName}'", innerException, queueName)
    {
    }
}