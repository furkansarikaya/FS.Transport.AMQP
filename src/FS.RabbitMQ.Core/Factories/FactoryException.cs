namespace FS.RabbitMQ.Core.Factories;

/// <summary>
/// Represents an exception that occurs during factory operations.
/// </summary>
public class FactoryException : Exception
{
    /// <summary>
    /// Initializes a new instance of the <see cref="FactoryException"/> class.
    /// </summary>
    public FactoryException() : base() { }

    /// <summary>
    /// Initializes a new instance of the <see cref="FactoryException"/> class with a specified error message.
    /// </summary>
    /// <param name="message">The message that describes the error.</param>
    public FactoryException(string message) : base(message) { }

    /// <summary>
    /// Initializes a new instance of the <see cref="FactoryException"/> class with a specified error message and inner exception.
    /// </summary>
    /// <param name="message">The message that describes the error.</param>
    /// <param name="innerException">The exception that is the cause of the current exception.</param>
    public FactoryException(string message, Exception innerException) : base(message, innerException) { }
}