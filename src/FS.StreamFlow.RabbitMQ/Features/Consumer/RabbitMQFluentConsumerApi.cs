using FS.StreamFlow.Core.Features.Messaging.Interfaces;
using FS.StreamFlow.Core.Features.Messaging.Models;

namespace FS.StreamFlow.RabbitMQ.Features.Consumer;

/// <summary>
/// RabbitMQ implementation of fluent consumer API for chainable message consumption configuration
/// </summary>
/// <typeparam name="T">Message type</typeparam>
public class RabbitMQFluentConsumerApi<T> : IFluentConsumerApi<T> where T : class
{
    private readonly IConsumer _consumer;
    private readonly string _queueName;
    private readonly ConsumerSettings _settings;
    private ConsumerContext? _context;
    private string? _exchangeName;
    private string? _routingKey;
    private Func<Exception, MessageContext, Task<bool>>? _errorHandler;
    private Func<T, bool>? _filter;

    /// <summary>
    /// Initializes a new instance of the RabbitMQ fluent consumer API
    /// </summary>
    /// <param name="consumer">Consumer instance</param>
    /// <param name="queueName">Queue name to consume from</param>
    public RabbitMQFluentConsumerApi(IConsumer consumer, string queueName)
    {
        _consumer = consumer ?? throw new ArgumentNullException(nameof(consumer));
        _queueName = queueName ?? throw new ArgumentNullException(nameof(queueName));
        _settings = new ConsumerSettings();
    }

    /// <summary>
    /// Configures the exchange to consume from
    /// </summary>
    /// <param name="exchangeName">Exchange name</param>
    /// <returns>Fluent consumer API for method chaining</returns>
    public IFluentConsumerApi<T> FromExchange(string exchangeName)
    {
        _exchangeName = exchangeName;
        return this;
    }

    /// <summary>
    /// Configures the routing key pattern for message filtering
    /// </summary>
    /// <param name="routingKey">Routing key pattern</param>
    /// <returns>Fluent consumer API for method chaining</returns>
    public IFluentConsumerApi<T> WithRoutingKey(string routingKey)
    {
        _routingKey = routingKey;
        return this;
    }

    /// <summary>
    /// Configures consumer settings
    /// </summary>
    /// <param name="settings">Consumer settings</param>
    /// <returns>Fluent consumer API for method chaining</returns>
    public IFluentConsumerApi<T> WithSettings(ConsumerSettings settings)
    {
        if (settings == null)
            throw new ArgumentNullException(nameof(settings));

        // Copy settings
        _settings.ConsumerId = settings.ConsumerId;
        _settings.PrefetchCount = settings.PrefetchCount;
        _settings.AutoAcknowledge = settings.AutoAcknowledge;
        _settings.ConsumerTag = settings.ConsumerTag;
        _settings.MaxConcurrentConsumers = settings.MaxConcurrentConsumers;
        _settings.RetryPolicy = settings.RetryPolicy;
        _settings.DeadLetterSettings = settings.DeadLetterSettings;
        _settings.ConsumerTimeout = settings.ConsumerTimeout;
        _settings.MessageProcessingTimeout = settings.MessageProcessingTimeout;
        
        return this;
    }

    /// <summary>
    /// Configures consumer settings using action
    /// </summary>
    /// <param name="configure">Configuration action</param>
    /// <returns>Fluent consumer API for method chaining</returns>
    public IFluentConsumerApi<T> WithSettings(Action<ConsumerSettings> configure)
    {
        if (configure == null)
            throw new ArgumentNullException(nameof(configure));

        configure(_settings);
        return this;
    }

    /// <summary>
    /// Configures consumer tag
    /// </summary>
    /// <param name="consumerTag">Consumer tag</param>
    /// <returns>Fluent consumer API for method chaining</returns>
    public IFluentConsumerApi<T> WithConsumerTag(string consumerTag)
    {
        _settings.ConsumerTag = consumerTag;
        return this;
    }

    /// <summary>
    /// Configures whether to auto-acknowledge messages
    /// </summary>
    /// <param name="autoAck">Auto-acknowledge flag</param>
    /// <returns>Fluent consumer API for method chaining</returns>
    public IFluentConsumerApi<T> WithAutoAck(bool autoAck)
    {
        _settings.AutoAcknowledge = autoAck;
        return this;
    }

    /// <summary>
    /// Configures prefetch count
    /// </summary>
    /// <param name="prefetchCount">Prefetch count</param>
    /// <returns>Fluent consumer API for method chaining</returns>
    public IFluentConsumerApi<T> WithPrefetchCount(ushort prefetchCount)
    {
        _settings.PrefetchCount = prefetchCount;
        return this;
    }

    /// <summary>
    /// Configures concurrent consumers
    /// </summary>
    /// <param name="concurrency">Number of concurrent consumers</param>
    /// <returns>Fluent consumer API for method chaining</returns>
    public IFluentConsumerApi<T> WithConcurrency(int concurrency)
    {
        _settings.MaxConcurrentConsumers = concurrency;
        return this;
    }

    /// <summary>
    /// Configures error handling
    /// </summary>
    /// <param name="errorHandler">Error handler function</param>
    /// <returns>Fluent consumer API for method chaining</returns>
    public IFluentConsumerApi<T> WithErrorHandler(Func<Exception, MessageContext, Task<bool>> errorHandler)
    {
        _errorHandler = errorHandler;
        return this;
    }

    /// <summary>
    /// Configures retry policy
    /// </summary>
    /// <param name="retryPolicy">Retry policy settings</param>
    /// <returns>Fluent consumer API for method chaining</returns>
    public IFluentConsumerApi<T> WithRetryPolicy(RetryPolicySettings retryPolicy)
    {
        _settings.RetryPolicy = retryPolicy;
        return this;
    }

    /// <summary>
    /// Configures dead letter queue
    /// </summary>
    /// <param name="deadLetterSettings">Dead letter settings</param>
    /// <returns>Fluent consumer API for method chaining</returns>
    public IFluentConsumerApi<T> WithDeadLetterQueue(DeadLetterSettings deadLetterSettings)
    {
        _settings.DeadLetterSettings = deadLetterSettings;
        return this;
    }

    /// <summary>
    /// Starts consuming messages with the specified handler
    /// </summary>
    /// <param name="messageHandler">Message handler function</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Task representing the consumption operation</returns>
    public async Task ConsumeAsync(Func<T, MessageContext, Task<bool>> messageHandler, CancellationToken cancellationToken = default)
    {
        if (messageHandler == null)
            throw new ArgumentNullException(nameof(messageHandler));

        // Create a wrapper that handles filtering and error handling
        var wrappedHandler = CreateWrappedHandler(messageHandler);
        
        // Apply settings to the consumer if necessary
        if (_settings.PrefetchCount > 0 || _settings.MaxConcurrentConsumers > 1 || !string.IsNullOrEmpty(_settings.ConsumerTag))
        {
            await _consumer.ConsumeAsync(_queueName, wrappedHandler, cancellationToken);
        }
        else
        {
            await _consumer.ConsumeAsync(_queueName, wrappedHandler, cancellationToken);
        }
    }

    /// <summary>
    /// Starts consuming messages with the specified handler and context
    /// </summary>
    /// <param name="messageHandler">Message handler function</param>
    /// <param name="context">Consumer context</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Task representing the consumption operation</returns>
    public async Task ConsumeAsync(Func<T, MessageContext, Task<bool>> messageHandler, ConsumerContext context, CancellationToken cancellationToken = default)
    {
        if (messageHandler == null)
            throw new ArgumentNullException(nameof(messageHandler));

        _context = context;
        
        // Create a wrapper that handles filtering and error handling
        var wrappedHandler = CreateWrappedHandler(messageHandler);
        
        await _consumer.ConsumeAsync(_queueName, wrappedHandler, context, cancellationToken);
    }

    /// <summary>
    /// Creates a wrapped message handler that applies filtering and error handling
    /// </summary>
    /// <param name="originalHandler">Original message handler</param>
    /// <returns>Wrapped message handler</returns>
    private Func<T, MessageContext, Task<bool>> CreateWrappedHandler(Func<T, MessageContext, Task<bool>> originalHandler)
    {
        return async (message, context) =>
        {
            try
            {
                // Apply filter if configured
                if (_filter != null && !_filter(message))
                {
                    // Message filtered out, acknowledge it
                    return true;
                }

                // Process the message
                return await originalHandler(message, context);
            }
            catch (Exception ex)
            {
                // Apply error handler if configured
                if (_errorHandler != null)
                {
                    return await _errorHandler(ex, context);
                }

                // Re-throw if no error handler
                throw;
            }
        };
    }
} 