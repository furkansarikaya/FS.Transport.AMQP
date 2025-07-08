namespace FS.RabbitMQ.Core.Factories;

/// <summary>
/// Defines the contract for publisher factories that create and manage message publishers.
/// </summary>
/// <remarks>
/// Publisher factories are responsible for creating, configuring, and managing the lifecycle
/// of message publishers. They handle publisher registration, performance optimization,
/// and integration with delivery confirmation and retry mechanisms.
/// </remarks>
public interface IPublisherFactory : IDisposable
{
    /// <summary>
    /// Creates a new publisher for the specified message type.
    /// </summary>
    /// <typeparam name="T">The type of messages to publish.</typeparam>
    /// <param name="channel">The channel to use for message publishing.</param>
    /// <returns>A new publisher instance.</returns>
    /// <exception cref="PublisherException">Thrown when publisher creation fails.</exception>
    Task<IPublisher<T>> CreatePublisherAsync<T>(IChannel channel) where T : class;

    /// <summary>
    /// Creates a new publisher with the specified configuration.
    /// </summary>
    /// <typeparam name="T">The type of messages to publish.</typeparam>
    /// <param name="channel">The channel to use for message publishing.</param>
    /// <param name="configuration">The publisher configuration.</param>
    /// <returns>A new publisher instance.</returns>
    /// <exception cref="PublisherException">Thrown when publisher creation fails.</exception>
    Task<IPublisher<T>> CreatePublisherAsync<T>(IChannel channel, PublisherConfiguration configuration) where T : class;

    /// <summary>
    /// Creates a batch publisher for publishing multiple messages efficiently.
    /// </summary>
    /// <typeparam name="T">The type of messages to publish.</typeparam>
    /// <param name="channel">The channel to use for message publishing.</param>
    /// <returns>A new batch publisher instance.</returns>
    /// <exception cref="PublisherException">Thrown when publisher creation fails.</exception>
    Task<IBatchPublisher<T>> CreateBatchPublisherAsync<T>(IChannel channel) where T : class;

    /// <summary>
    /// Gets the current publisher statistics.
    /// </summary>
    /// <returns>Publisher statistics and health information.</returns>
    Task<PublisherInfo> GetPublisherInfoAsync();

    /// <summary>
    /// Occurs when a publisher is created.
    /// </summary>
    event EventHandler<PublisherCreatedEventArgs>? PublisherCreated;

    /// <summary>
    /// Occurs when a publisher creation fails.
    /// </summary>
    event EventHandler<PublisherFailedEventArgs>? PublisherFailed;
}