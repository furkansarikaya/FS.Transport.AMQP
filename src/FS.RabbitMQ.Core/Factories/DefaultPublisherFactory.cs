using System.Collections.Concurrent;
using FS.RabbitMQ.Core.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using RabbitMQ.Client;

namespace FS.RabbitMQ.Core.Factories;

/// <summary>
/// Provides a comprehensive implementation of the publisher factory with advanced management features.
/// </summary>
/// <remarks>
/// The default publisher factory provides sophisticated publisher management including
/// lifecycle tracking, performance monitoring, delivery confirmation handling,
/// and integration with retry and reliability mechanisms.
/// </remarks>
public sealed class DefaultPublisherFactory : FactoryBase<object>, IPublisherFactory
{
    private readonly PublisherConfiguration _configuration;
    private readonly ISerializationProvider _serializationProvider;
    private readonly IDeliveryConfirmationProvider _confirmationProvider;
    private readonly ConcurrentDictionary<string, IDisposable> _activePublishers;
    private readonly IPublisherMetrics _metrics;

    /// <summary>
    /// Initializes a new instance of the <see cref="DefaultPublisherFactory"/> class.
    /// </summary>
    /// <param name="configuration">The publisher configuration.</param>
    /// <param name="serializationProvider">The serialization provider.</param>
    /// <param name="confirmationProvider">The delivery confirmation provider.</param>
    /// <param name="metrics">The publisher metrics collector.</param>
    /// <param name="logger">The logger instance.</param>
    public DefaultPublisherFactory(
        IOptions<PublisherConfiguration> configuration,
        ISerializationProvider serializationProvider,
        IDeliveryConfirmationProvider confirmationProvider,
        IPublisherMetrics metrics,
        ILogger<DefaultPublisherFactory> logger)
        : base(logger)
    {
        _configuration = configuration?.Value ?? throw new ArgumentNullException(nameof(configuration));
        _serializationProvider = serializationProvider ?? throw new ArgumentNullException(nameof(serializationProvider));
        _confirmationProvider = confirmationProvider ?? throw new ArgumentNullException(nameof(confirmationProvider));
        _metrics = metrics ?? throw new ArgumentNullException(nameof(metrics));
        _activePublishers = new ConcurrentDictionary<string, IDisposable>();

        ValidateConfiguration();
    }

    /// <summary>
    /// Occurs when a publisher is created.
    /// </summary>
    public event EventHandler<PublisherCreatedEventArgs>? PublisherCreated;

    /// <summary>
    /// Occurs when a publisher creation fails.
    /// </summary>
    public event EventHandler<PublisherFailedEventArgs>? PublisherFailed;

    /// <summary>
    /// Creates a new publisher for the specified message type.
    /// </summary>
    /// <typeparam name="T">The type of messages to publish.</typeparam>
    /// <param name="channel">The channel to use for message publishing.</param>
    /// <returns>A new publisher instance.</returns>
    public async Task<IPublisher<T>> CreatePublisherAsync<T>(IChannel channel) where T : class
    {
        return await CreatePublisherAsync<T>(channel, _configuration);
    }

    /// <summary>
    /// Creates a new publisher with the specified configuration.
    /// </summary>
    /// <typeparam name="T">The type of messages to publish.</typeparam>
    /// <param name="channel">The channel to use for message publishing.</param>
    /// <param name="configuration">The publisher configuration.</param>
    /// <returns>A new publisher instance.</returns>
    public async Task<IPublisher<T>> CreatePublisherAsync<T>(IChannel channel, PublisherConfiguration configuration) where T : class
    {
        ThrowIfDisposed();
        ArgumentNullException.ThrowIfNull(channel);
        ArgumentNullException.ThrowIfNull(configuration);

        try
        {
            Logger.LogDebug("Creating publisher for message type {MessageType}", typeof(T).Name);

            var publisherId = $"publisher-{Guid.NewGuid():N}";
            
            var publisher = new DefaultPublisher<T>(
                publisherId,
                channel,
                configuration,
                _serializationProvider,
                _confirmationProvider,
                _metrics,
                Logger);

            // Track the publisher
            _activePublishers.TryAdd(publisherId, publisher);

            // Initialize the publisher
            await publisher.InitializeAsync();

            // Raise publisher created event
            PublisherCreated?.Invoke(this, new PublisherCreatedEventArgs(publisherId, typeof(T)));

            Logger.LogInformation("Successfully created publisher {PublisherId} for message type {MessageType}", 
                publisherId, typeof(T).Name);

            return publisher;
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Failed to create publisher for message type {MessageType}", typeof(T).Name);
            
            // Raise publisher failed event
            PublisherFailed?.Invoke(this, new PublisherFailedEventArgs(typeof(T), ex));
            
            throw new PublisherException($"Failed to create publisher for message type {typeof(T).Name}", ex);
        }
    }

    /// <summary>
    /// Creates a batch publisher for publishing multiple messages efficiently.
    /// </summary>
    /// <typeparam name="T">The type of messages to publish.</typeparam>
    /// <param name="channel">The channel to use for message publishing.</param>
    /// <returns>A new batch publisher instance.</returns>
    public async Task<IBatchPublisher<T>> CreateBatchPublisherAsync<T>(IChannel channel) where T : class
    {
        ThrowIfDisposed();
        ArgumentNullException.ThrowIfNull(channel);

        try
        {
            Logger.LogDebug("Creating batch publisher for message type {MessageType}", typeof(T).Name);

            var publisherId = $"batch-publisher-{Guid.NewGuid():N}";
            
            var publisher = new DefaultBatchPublisher<T>(
                publisherId,
                channel,
                _configuration,
                _serializationProvider,
                _confirmationProvider,
                _metrics,
                Logger);

            // Track the publisher
            _activePublishers.TryAdd(publisherId, publisher);

            // Initialize the publisher
            await publisher.InitializeAsync();

            // Raise publisher created event
            PublisherCreated?.Invoke(this, new PublisherCreatedEventArgs(publisherId, typeof(T)));

            Logger.LogInformation("Successfully created batch publisher {PublisherId} for message type {MessageType}", 
                publisherId, typeof(T).Name);

            return publisher;
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Failed to create batch publisher for message type {MessageType}", typeof(T).Name);
            
            // Raise publisher failed event
            PublisherFailed?.Invoke(this, new PublisherFailedEventArgs(typeof(T), ex));
            
            throw new PublisherException($"Failed to create batch publisher for message type {typeof(T).Name}", ex);
        }
    }

    /// <summary>
    /// Gets the current publisher statistics.
    /// </summary>
    /// <returns>Publisher statistics and health information.</returns>
    public async Task<PublisherInfo> GetPublisherInfoAsync()
    {
        ThrowIfDisposed();

        try
        {
            var publisherDetails = new List<PublisherDetails>();

            foreach (var kvp in _activePublishers)
            {
                if (kvp.Value is IPublisherInfoProvider infoProvider)
                {
                    var details = await infoProvider.GetPublisherDetailsAsync();
                    publisherDetails.Add(details);
                }
            }

            var metrics = await _metrics.GetOverallMetricsAsync();

            return new PublisherInfo
            {
                ActivePublishers = _activePublishers.Count,
                PublisherDetails = publisherDetails,
                TotalMessagesPublished = metrics.TotalMessagesPublished,
                TotalMessagesConfirmed = metrics.TotalMessagesConfirmed,
                TotalMessagesFailed = metrics.TotalMessagesFailed,
                AveragePublishTime = metrics.AveragePublishTime,
                LastUpdated = DateTimeOffset.UtcNow
            };
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Failed to get publisher information");
            throw new PublisherException("Failed to retrieve publisher information", ex);
        }
    }

    /// <summary>
    /// Creates the core object instance.
    /// </summary>
    /// <param name="parameters">The parameters for object creation.</param>
    /// <returns>A new object instance.</returns>
    protected override object CreateCore(params object[] parameters)
    {
        throw new NotSupportedException("Use specific CreatePublisherAsync methods for publisher creation");
    }

    private void ValidateConfiguration()
    {
        try
        {
            _configuration.Validate();
            Logger.LogDebug("Publisher factory configuration validation successful");
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Publisher factory configuration validation failed");
            throw new FactoryException("Invalid publisher factory configuration", ex);
        }
    }

    /// <summary>
    /// Releases the resources used by the publisher factory.
    /// </summary>
    /// <param name="disposing"><c>true</c> to release both managed and unmanaged resources; otherwise, <c>false</c>.</param>
    protected override void Dispose(bool disposing)
    {
        if (disposing && !IsDisposed)
        {
            Logger.LogDebug("Disposing publisher factory and stopping all active publishers");

            try
            {
                // Stop and dispose all active publishers
                var disposeTasks = _activePublishers.Values.Select(publisher => Task.Run(() => publisher.Dispose()));
                Task.WaitAll(disposeTasks.ToArray(), TimeSpan.FromSeconds(30));

                _activePublishers.Clear();
            }
            catch (Exception ex)
            {
                Logger.LogWarning(ex, "Error occurred while disposing publisher factory resources");
            }
        }

        base.Dispose(disposing);
    }
}