using System.Collections.Concurrent;
using FS.RabbitMQ.Core.Abstractions;
using FS.RabbitMQ.Core.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using RabbitMQ.Client;

namespace FS.RabbitMQ.Core.Factories;

/// <summary>
/// Provides a comprehensive implementation of the consumer factory with advanced management features.
/// </summary>
/// <remarks>
/// The default consumer factory provides sophisticated consumer management including
/// lifecycle tracking, health monitoring, performance optimization, and integration
/// with error handling and retry mechanisms.
/// </remarks>
public sealed class DefaultConsumerFactory : FactoryBase<object>, IConsumerFactory
{
    private readonly ConsumerConfiguration _configuration;
    private readonly ISerializationProvider _serializationProvider;
    private readonly IErrorHandlingProvider _errorHandlingProvider;
    private readonly ConcurrentDictionary<string, IDisposable> _activeConsumers;
    private readonly IConsumerMetrics _metrics;

    /// <summary>
    /// Initializes a new instance of the <see cref="DefaultConsumerFactory"/> class.
    /// </summary>
    /// <param name="configuration">The consumer configuration.</param>
    /// <param name="serializationProvider">The serialization provider.</param>
    /// <param name="errorHandlingProvider">The error handling provider.</param>
    /// <param name="metrics">The consumer metrics collector.</param>
    /// <param name="logger">The logger instance.</param>
    public DefaultConsumerFactory(
        IOptions<ConsumerConfiguration> configuration,
        ISerializationProvider serializationProvider,
        IErrorHandlingProvider errorHandlingProvider,
        IConsumerMetrics metrics,
        ILogger<DefaultConsumerFactory> logger)
        : base(logger)
    {
        _configuration = configuration?.Value ?? throw new ArgumentNullException(nameof(configuration));
        _serializationProvider = serializationProvider ?? throw new ArgumentNullException(nameof(serializationProvider));
        _errorHandlingProvider = errorHandlingProvider ?? throw new ArgumentNullException(nameof(errorHandlingProvider));
        _metrics = metrics ?? throw new ArgumentNullException(nameof(metrics));
        _activeConsumers = new ConcurrentDictionary<string, IDisposable>();

        ValidateConfiguration();
    }

    /// <summary>
    /// Occurs when a consumer is created.
    /// </summary>
    public event EventHandler<ConsumerCreatedEventArgs>? ConsumerCreated;

    /// <summary>
    /// Occurs when a consumer creation fails.
    /// </summary>
    public event EventHandler<ConsumerFailedEventArgs>? ConsumerFailed;

    /// <summary>
    /// Creates a new consumer for the specified message type.
    /// </summary>
    /// <typeparam name="T">The type of messages to consume.</typeparam>
    /// <param name="channel">The channel to use for message consumption.</param>
    /// <param name="queueName">The name of the queue to consume from.</param>
    /// <param name="messageHandler">The handler for processing messages.</param>
    /// <returns>A new consumer instance.</returns>
    public async Task<IConsumer<T>> CreateConsumerAsync<T>(
        IChannel channel,
        string queueName,
        IMessageHandler<T> messageHandler) where T : class
    {
        return await CreateConsumerAsync(channel, queueName, messageHandler, _configuration);
    }

    /// <summary>
    /// Creates a new consumer with the specified configuration.
    /// </summary>
    /// <typeparam name="T">The type of messages to consume.</typeparam>
    /// <param name="channel">The channel to use for message consumption.</param>
    /// <param name="queueName">The name of the queue to consume from.</param>
    /// <param name="messageHandler">The handler for processing messages.</param>
    /// <param name="configuration">The consumer configuration.</param>
    /// <returns>A new consumer instance.</returns>
    public async Task<IConsumer<T>> CreateConsumerAsync<T>(
        IChannel channel,
        string queueName,
        IMessageHandler<T> messageHandler,
        ConsumerConfiguration configuration) where T : class
    {
        ThrowIfDisposed();
        ArgumentNullException.ThrowIfNull(channel);
        ArgumentException.ThrowIfNullOrWhiteSpace(queueName);
        ArgumentNullException.ThrowIfNull(messageHandler);
        ArgumentNullException.ThrowIfNull(configuration);

        try
        {
            Logger.LogDebug("Creating consumer for queue '{QueueName}' with message type {MessageType}", 
                queueName, typeof(T).Name);

            var consumerId = $"consumer-{Guid.NewGuid():N}";
            
            var consumer = new DefaultConsumer<T>(
                consumerId,
                channel,
                queueName,
                messageHandler,
                configuration,
                _serializationProvider,
                _errorHandlingProvider,
                _metrics,
                Logger);

            // Track the consumer
            _activeConsumers.TryAdd(consumerId, consumer);

            // Start the consumer
            await consumer.StartAsync();

            // Raise consumer created event
            ConsumerCreated?.Invoke(this, new ConsumerCreatedEventArgs(consumerId, queueName, typeof(T)));

            Logger.LogInformation("Successfully created and started consumer {ConsumerId} for queue '{QueueName}'", 
                consumerId, queueName);

            return consumer;
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Failed to create consumer for queue '{QueueName}'", queueName);
            
            // Raise consumer failed event
            ConsumerFailed?.Invoke(this, new ConsumerFailedEventArgs(queueName, typeof(T), ex));
            
            throw new ConsumerException($"Failed to create consumer for queue '{queueName}'", ex);
        }
    }

    /// <summary>
    /// Creates a batch consumer for processing multiple messages at once.
    /// </summary>
    /// <typeparam name="T">The type of messages to consume.</typeparam>
    /// <param name="channel">The channel to use for message consumption.</param>
    /// <param name="queueName">The name of the queue to consume from.</param>
    /// <param name="batchHandler">The handler for processing message batches.</param>
    /// <returns>A new batch consumer instance.</returns>
    public async Task<IBatchConsumer<T>> CreateBatchConsumerAsync<T>(
        IChannel channel,
        string queueName,
        IBatchMessageHandler<T> batchHandler) where T : class
    {
        ThrowIfDisposed();
        ArgumentNullException.ThrowIfNull(channel);
        ArgumentException.ThrowIfNullOrWhiteSpace(queueName);
        ArgumentNullException.ThrowIfNull(batchHandler);

        try
        {
            Logger.LogDebug("Creating batch consumer for queue '{QueueName}' with message type {MessageType}", 
                queueName, typeof(T).Name);

            var consumerId = $"batch-consumer-{Guid.NewGuid():N}";
            
            var consumer = new DefaultBatchConsumer<T>(
                consumerId,
                channel,
                queueName,
                batchHandler,
                _configuration,
                _serializationProvider,
                _errorHandlingProvider,
                _metrics,
                Logger);

            // Track the consumer
            _activeConsumers.TryAdd(consumerId, consumer);

            // Start the consumer
            await consumer.StartAsync();

            // Raise consumer created event
            ConsumerCreated?.Invoke(this, new ConsumerCreatedEventArgs(consumerId, queueName, typeof(T)));

            Logger.LogInformation("Successfully created and started batch consumer {ConsumerId} for queue '{QueueName}'", 
                consumerId, queueName);

            return consumer;
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Failed to create batch consumer for queue '{QueueName}'", queueName);
            
            // Raise consumer failed event
            ConsumerFailed?.Invoke(this, new ConsumerFailedEventArgs(queueName, typeof(T), ex));
            
            throw new ConsumerException($"Failed to create batch consumer for queue '{queueName}'", ex);
        }
    }

    /// <summary>
    /// Gets the current consumer statistics.
    /// </summary>
    /// <returns>Consumer statistics and health information.</returns>
    public async Task<ConsumerInfo> GetConsumerInfoAsync()
    {
        ThrowIfDisposed();

        try
        {
            var consumerDetails = new List<ConsumerDetails>();

            foreach (var kvp in _activeConsumers)
            {
                if (kvp.Value is IConsumerInfoProvider infoProvider)
                {
                    var details = await infoProvider.GetConsumerDetailsAsync();
                    consumerDetails.Add(details);
                }
            }

            var metrics = await _metrics.GetOverallMetricsAsync();

            return new ConsumerInfo
            {
                ActiveConsumers = _activeConsumers.Count,
                ConsumerDetails = consumerDetails,
                TotalMessagesProcessed = metrics.TotalMessagesProcessed,
                TotalMessagesSucceeded = metrics.TotalMessagesSucceeded,
                TotalMessagesFailed = metrics.TotalMessagesFailed,
                AverageProcessingTime = metrics.AverageProcessingTime,
                LastUpdated = DateTimeOffset.UtcNow
            };
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Failed to get consumer information");
            throw new ConsumerException("Failed to retrieve consumer information", ex);
        }
    }

    /// <summary>
    /// Creates the core object instance.
    /// </summary>
    /// <param name="parameters">The parameters for object creation.</param>
    /// <returns>A new object instance.</returns>
    protected override object CreateCore(params object[] parameters)
    {
        throw new NotSupportedException("Use specific CreateConsumerAsync methods for consumer creation");
    }

    private void ValidateConfiguration()
    {
        try
        {
            _configuration.Validate();
            Logger.LogDebug("Consumer factory configuration validation successful");
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Consumer factory configuration validation failed");
            throw new FactoryException("Invalid consumer factory configuration", ex);
        }
    }

    /// <summary>
    /// Releases the resources used by the consumer factory.
    /// </summary>
    /// <param name="disposing"><c>true</c> to release both managed and unmanaged resources; otherwise, <c>false</c>.</param>
    protected override void Dispose(bool disposing)
    {
        if (disposing && !IsDisposed)
        {
            Logger.LogDebug("Disposing consumer factory and stopping all active consumers");

            try
            {
                // Stop and dispose all active consumers
                var disposeTasks = _activeConsumers.Values.Select(consumer => Task.Run(() => consumer.Dispose()));
                Task.WaitAll(disposeTasks.ToArray(), TimeSpan.FromSeconds(30));

                _activeConsumers.Clear();
            }
            catch (Exception ex)
            {
                Logger.LogWarning(ex, "Error occurred while disposing consumer factory resources");
            }
        }

        base.Dispose(disposing);
    }
}