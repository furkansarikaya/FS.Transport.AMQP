using RabbitMQ.Client;

namespace FS.RabbitMQ.ErrorHandling;

/// <summary>
/// Interface for handling dead letter messages in RabbitMQ
/// </summary>
public interface IDeadLetterHandler
{
    /// <summary>
    /// Handles a dead letter message
    /// </summary>
    /// <param name="context">Error context containing the message and error information</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Task representing the handling operation</returns>
    Task<bool> HandleDeadLetterAsync(ErrorContext context, CancellationToken cancellationToken = default);

    /// <summary>
    /// Sets up the dead letter infrastructure (exchanges, queues, bindings)
    /// </summary>
    /// <param name="channel">The RabbitMQ channel to use for setup</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Task representing the setup operation</returns>
    Task<bool> SetupDeadLetterInfrastructureAsync(IChannel channel, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets statistics about dead letter handling
    /// </summary>
    /// <returns>Dead letter statistics</returns>
    DeadLetterStatistics GetStatistics();

    /// <summary>
    /// Requeues a message from the dead letter queue back to the original queue
    /// </summary>
    /// <param name="messageId">The message ID to requeue</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Task representing the requeue operation</returns>
    Task<bool> RequeueFromDeadLetterAsync(string messageId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Purges all messages from the dead letter queue
    /// </summary>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Task representing the purge operation</returns>
    Task<bool> PurgeDeadLetterQueueAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets the configuration settings for dead letter handling
    /// </summary>
    /// <returns>Dead letter configuration settings</returns>
    DeadLetterSettings GetSettings();
}