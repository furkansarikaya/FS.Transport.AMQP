namespace FS.RabbitMQ.Core.Exceptions;

/// <summary>
/// Exception thrown when exchange declaration fails
/// </summary>
public class ExchangeDeclarationException : ExchangeException
{
    public ExchangeDeclarationException(string exchangeName, string reason) 
        : base($"Failed to declare exchange '{exchangeName}': {reason}", exchangeName)
    {
    }

    public ExchangeDeclarationException(string exchangeName, Exception innerException) 
        : base($"Failed to declare exchange '{exchangeName}'", innerException, exchangeName)
    {
    }
}