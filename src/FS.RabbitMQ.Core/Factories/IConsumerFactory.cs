namespace FS.RabbitMQ.Core.Factories;

/// <summary>
/// Defines the contract for consumer factories that create and manage message consumers.
/// </summary>
/// <remarks>
/// Consumer factories are responsible for creating, configuring, and managing the lifecycle
/// of message consumers. They handle consumer registration, subscription management,
/// and integration with error handling and retry mechanisms.
/// </remarks>
public interface IConsumerFactory : IDisposable
{
    /// <summary>
    /// Creates a new consumer for the specified message type.
    /// </summary>
    /// <typeparam name="T">The type of messages to consume.</typeparam>
    /// <param name="channel">The channel to use for message consumption.</param>
    /// <param name="queueName">The name of the queue to consume from.</param>
    /// <param name="messageHandler">The handler for processing messages.</param>
    /// <returns>A new consumer instance.</returns>
    /// <exception cref="ConsumerException">Thrown when consumer creation fails.</exception>
    Task<IConsumer<T>> CreateConsumerAsync<T>(
        IChannel channel, 
        string queueName, 
        IMessageHandler<T> messageHandler) where T : class;

    /// <summary>
    /// Creates a new consumer with the specified configuration.
    /// </summary>
    /// <typeparam name="T">The type of messages to consume.</typeparam>
    /// <param name="channel">The channel to use for message consumption.</param>
    /// <param name="queueName">The name of the queue to consume from.</param>
    /// <param name="messageHandler">The handler for processing messages.</param>
    /// <param name="configuration">The consumer configuration.</param>
    /// <returns>A new consumer instance.</returns>
    /// <exception cref="ConsumerException">Thrown when consumer creation fails.</exception>
    Task<IConsumer<T>> CreateConsumerAsync<T>(
        IChannel channel,
        string queueName,
        IMessageHandler<T> messageHandler,
        ConsumerConfiguration configuration) where T : class;

    /// <summary>
    /// Creates a batch consumer for processing multiple messages at once.
    /// </summary>
    /// <typeparam name="T">The type of messages to consume.</typeparam>
    /// <param name="channel">The channel to use for message consumption.</param>
    /// <param name="queueName">The name of the queue to consume from.</param>
    /// <param name="batchHandler">The handler for processing message batches.</param>
    /// <returns>A new batch consumer instance.</returns>
    /// <exception cref="ConsumerException">Thrown when consumer creation fails.</exception>
    Task<IBatchConsumer<T>> CreateBatchConsumerAsync<T>(
        IChannel channel,
        string queueName,
        IBatchMessageHandler<T> batchHandler) where T : class;

    /// <summary>
    /// Gets the current consumer statistics.
    /// </summary>
    /// <returns>Consumer statistics and health information.</returns>
    Task<ConsumerInfo> GetConsumerInfoAsync();

    /// <summary>
    /// Occurs when a consumer is created.
    /// </summary>
    event EventHandler<ConsumerCreatedEventArgs>? ConsumerCreated;

    /// <summary>
    /// Occurs when a consumer creation fails.
    /// </summary>
    event EventHandler<ConsumerFailedEventArgs>? ConsumerFailed;
}
