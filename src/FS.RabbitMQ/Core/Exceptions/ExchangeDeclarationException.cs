namespace FS.RabbitMQ.Core.Exceptions;

/// <summary>
/// Exception thrown when exchange declaration fails
/// </summary>
public class ExchangeDeclarationException : ExchangeException
{
    /// <summary>
    /// Initializes a new instance of the <see cref="ExchangeDeclarationException"/> class with a specified message and exchange name
    /// </summary>
    /// <param name="message">The message that describes the error</param>
    /// <param name="exchangeName">The name of the exchange that failed to be declared</param>
    public ExchangeDeclarationException(string message, string exchangeName) 
        : base(message, exchangeName)
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="ExchangeDeclarationException"/> class with a specified message and inner exception
    /// </summary>
    /// <param name="message">The message that describes the error</param>
    /// <param name="innerException">The exception that is the cause of the current exception</param>
    public ExchangeDeclarationException(string message, Exception innerException) 
        : base(message, innerException, null)
    {
    }
}