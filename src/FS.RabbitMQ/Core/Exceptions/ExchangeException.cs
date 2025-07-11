namespace FS.RabbitMQ.Core.Exceptions;

/// <summary>
/// Exception thrown when exchange-related errors occur
/// </summary>
public class ExchangeException : RabbitMQException
{
    /// <summary>
    /// Gets the name of the exchange that caused the exception
    /// </summary>
    /// <value>The exchange name, or null if not specified</value>
    public string? ExchangeName { get; }

    /// <summary>
    /// Initializes a new instance of the <see cref="ExchangeException"/> class with a specified message and exchange name
    /// </summary>
    /// <param name="message">The message that describes the error</param>
    /// <param name="exchangeName">The name of the exchange that caused the exception</param>
    public ExchangeException(string message, string? exchangeName) 
        : base(message, "EXCHANGE_ERROR")
    {
        ExchangeName = exchangeName;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="ExchangeException"/> class with a specified message, inner exception, and exchange name
    /// </summary>
    /// <param name="message">The message that describes the error</param>
    /// <param name="innerException">The exception that is the cause of the current exception</param>
    /// <param name="exchangeName">The name of the exchange that caused the exception</param>
    public ExchangeException(string message, Exception innerException, string? exchangeName) 
        : base(message, innerException, "EXCHANGE_ERROR")
    {
        ExchangeName = exchangeName;
    }
}