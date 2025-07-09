namespace FS.Transport.AMQP.ErrorHandling;

/// <summary>
/// Error handling strategies
/// </summary>
public enum ErrorHandlingStrategy
{
    /// <summary>
    /// Ignore the error and acknowledge the message
    /// </summary>
    Ignore,
    
    /// <summary>
    /// Retry the operation with the configured retry policy
    /// </summary>
    Retry,
    
    /// <summary>
    /// Send the message to a dead letter exchange/queue
    /// </summary>
    DeadLetter,
    
    /// <summary>
    /// Requeue the message for later processing
    /// </summary>
    Requeue,
    
    /// <summary>
    /// Use a custom error handling implementation
    /// </summary>
    Custom
}