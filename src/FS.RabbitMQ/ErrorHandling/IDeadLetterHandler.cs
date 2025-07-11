using RabbitMQ.Client;

namespace FS.RabbitMQ.ErrorHandling;

/// <summary>
/// Interface for handling dead letter operations
/// </summary>
public interface IDeadLetterHandler
{
    /// <summary>
    /// Sends a message to the dead letter exchange/queue
    /// </summary>
    /// <param name="context">Error context containing message and error information</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>True if message was successfully sent to dead letter</returns>
    Task<bool> SendToDeadLetterAsync(ErrorContext context, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Sets up dead letter exchange and queue infrastructure
    /// </summary>
    /// <param name="channel">RabbitMQ channel</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>True if setup was successful</returns>
    Task<bool> SetupDeadLetterInfrastructureAsync(IModel channel, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Gets dead letter statistics
    /// </summary>
    /// <returns>Dead letter statistics</returns>
    DeadLetterStatistics GetStatistics();
}