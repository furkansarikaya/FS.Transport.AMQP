namespace FS.RabbitMQ.Core.Exceptions;

/// <summary>
/// Exception thrown when exchange-related errors occur
/// </summary>
public class ExchangeException : RabbitMQException
{
    public string? ExchangeName { get; }

    public ExchangeException(string message, string? exchangeName = null) 
        : base(message, "EXCHANGE_ERROR")
    {
        ExchangeName = exchangeName;
        if (!string.IsNullOrEmpty(exchangeName))
        {
            WithContext("ExchangeName", exchangeName);
        }
    }

    public ExchangeException(string message, Exception innerException, string? exchangeName = null) 
        : base(message, innerException, "EXCHANGE_ERROR")
    {
        ExchangeName = exchangeName;
        if (!string.IsNullOrEmpty(exchangeName))
        {
            WithContext("ExchangeName", exchangeName);
        }
    }
}