using FS.RabbitMQ.Configuration;
using FS.RabbitMQ.ErrorHandling;
using FS.RabbitMQ.Producer;

namespace FS.RabbitMQ.Consumer;

/// <summary>
/// Fluent API interface for advanced consumer configuration and message consumption
/// </summary>
/// <typeparam name="T">Message type</typeparam>
public interface IFluentConsumerApi<T> where T : class
{
    /// <summary>
    /// Configures the exchange to consume from
    /// </summary>
    /// <param name="exchangeName">Exchange name</param>
    /// <returns>Fluent consumer API for method chaining</returns>
    IFluentConsumerApi<T> FromExchange(string exchangeName);
    
    /// <summary>
    /// Configures the routing key pattern for message filtering
    /// </summary>
    /// <param name="routingKey">Routing key pattern</param>
    /// <returns>Fluent consumer API for method chaining</returns>
    IFluentConsumerApi<T> WithRoutingKey(string routingKey);
    
    /// <summary>
    /// Configures consumer settings
    /// </summary>
    /// <param name="settings">Consumer settings</param>
    /// <returns>Fluent consumer API for method chaining</returns>
    IFluentConsumerApi<T> WithSettings(ConsumerSettings settings);
    
    /// <summary>
    /// Configures consumer settings using action
    /// </summary>
    /// <param name="configure">Configuration action</param>
    /// <returns>Fluent consumer API for method chaining</returns>
    IFluentConsumerApi<T> WithSettings(Action<ConsumerSettings> configure);
    
    /// <summary>
    /// Configures consumer tag
    /// </summary>
    /// <param name="consumerTag">Consumer tag</param>
    /// <returns>Fluent consumer API for method chaining</returns>
    IFluentConsumerApi<T> WithConsumerTag(string consumerTag);
    
    /// <summary>
    /// Configures whether to auto-acknowledge messages
    /// </summary>
    /// <param name="autoAck">Auto-acknowledge flag</param>
    /// <returns>Fluent consumer API for method chaining</returns>
    IFluentConsumerApi<T> WithAutoAcknowledge(bool autoAck = true);
    
    /// <summary>
    /// Configures the prefetch count (QoS)
    /// </summary>
    /// <param name="count">Prefetch count</param>
    /// <param name="global">Whether to apply globally</param>
    /// <returns>Fluent consumer API for method chaining</returns>
    IFluentConsumerApi<T> WithPrefetch(ushort count, bool global = false);
    
    /// <summary>
    /// Configures consumer priority
    /// </summary>
    /// <param name="priority">Consumer priority</param>
    /// <returns>Fluent consumer API for method chaining</returns>
    IFluentConsumerApi<T> WithPriority(int priority);
    
    /// <summary>
    /// Configures exclusive consumer
    /// </summary>
    /// <param name="exclusive">Exclusive flag</param>
    /// <returns>Fluent consumer API for method chaining</returns>
    IFluentConsumerApi<T> WithExclusive(bool exclusive = true);
    
    /// <summary>
    /// Configures maximum concurrent message processors
    /// </summary>
    /// <param name="maxConcurrent">Maximum concurrent processors</param>
    /// <returns>Fluent consumer API for method chaining</returns>
    IFluentConsumerApi<T> WithConcurrency(int maxConcurrent);
    
    /// <summary>
    /// Configures processing timeout
    /// </summary>
    /// <param name="timeout">Processing timeout</param>
    /// <returns>Fluent consumer API for method chaining</returns>
    IFluentConsumerApi<T> WithTimeout(TimeSpan timeout);
    
    /// <summary>
    /// Enables message deduplication
    /// </summary>
    /// <param name="cacheSize">Deduplication cache size</param>
    /// <param name="cacheTtl">Cache TTL</param>
    /// <returns>Fluent consumer API for method chaining</returns>
    IFluentConsumerApi<T> WithDeduplication(int cacheSize = 10000, TimeSpan? cacheTtl = null);
    
    /// <summary>
    /// Enables batch processing
    /// </summary>
    /// <param name="batchSize">Batch size</param>
    /// <param name="batchTimeout">Batch timeout</param>
    /// <returns>Fluent consumer API for method chaining</returns>
    IFluentConsumerApi<T> WithBatchProcessing(int batchSize = 100, TimeSpan? batchTimeout = null);
    
    /// <summary>
    /// Configures retry policy
    /// </summary>
    /// <param name="maxRetries">Maximum number of retries</param>
    /// <param name="initialDelay">Initial delay between retries</param>
    /// <param name="maxDelay">Maximum delay between retries</param>
    /// <param name="backoffMultiplier">Backoff multiplier</param>
    /// <returns>Fluent consumer API for method chaining</returns>
    IFluentConsumerApi<T> WithRetryPolicy(int maxRetries, TimeSpan initialDelay, TimeSpan? maxDelay = null, double backoffMultiplier = 2.0);
    
    /// <summary>
    /// Configures retry policy using settings
    /// </summary>
    /// <param name="retryPolicy">Retry policy settings</param>
    /// <returns>Fluent consumer API for method chaining</returns>
    IFluentConsumerApi<T> WithRetryPolicy(RetryPolicySettings retryPolicy);
    
    /// <summary>
    /// Configures error handling strategy
    /// </summary>
    /// <param name="strategy">Error handling strategy</param>
    /// <returns>Fluent consumer API for method chaining</returns>
    IFluentConsumerApi<T> WithErrorHandling(ErrorHandlingStrategy strategy);
    
    /// <summary>
    /// Configures dead letter exchange for failed messages
    /// </summary>
    /// <param name="exchange">Dead letter exchange</param>
    /// <param name="routingKey">Dead letter routing key</param>
    /// <returns>Fluent consumer API for method chaining</returns>
    IFluentConsumerApi<T> WithDeadLetter(string exchange, string? routingKey = null);
    
    /// <summary>
    /// Enables circuit breaker pattern
    /// </summary>
    /// <param name="failureThreshold">Failure threshold</param>
    /// <param name="recoveryTimeout">Recovery timeout</param>
    /// <returns>Fluent consumer API for method chaining</returns>
    IFluentConsumerApi<T> WithCircuitBreaker(int failureThreshold = 5, TimeSpan? recoveryTimeout = null);
    
    /// <summary>
    /// Configures serializer settings
    /// </summary>
    /// <param name="settings">Serializer settings</param>
    /// <returns>Fluent consumer API for method chaining</returns>
    IFluentConsumerApi<T> WithSerializer(SerializerSettings settings);
    
    /// <summary>
    /// Configures custom headers for message filtering
    /// </summary>
    /// <param name="headers">Custom headers</param>
    /// <returns>Fluent consumer API for method chaining</returns>
    IFluentConsumerApi<T> WithHeaders(IDictionary<string, object> headers);
    
    /// <summary>
    /// Adds a custom header for message filtering
    /// </summary>
    /// <param name="key">Header key</param>
    /// <param name="value">Header value</param>
    /// <returns>Fluent consumer API for method chaining</returns>
    IFluentConsumerApi<T> WithHeader(string key, object value);
    
    /// <summary>
    /// Configures custom consumer arguments
    /// </summary>
    /// <param name="arguments">Consumer arguments</param>
    /// <returns>Fluent consumer API for method chaining</returns>
    IFluentConsumerApi<T> WithArguments(IDictionary<string, object> arguments);
    
    /// <summary>
    /// Adds a custom consumer argument
    /// </summary>
    /// <param name="key">Argument key</param>
    /// <param name="value">Argument value</param>
    /// <returns>Fluent consumer API for method chaining</returns>
    IFluentConsumerApi<T> WithArgument(string key, object value);
    
    /// <summary>
    /// Configures high-throughput consumption settings
    /// </summary>
    /// <returns>Fluent consumer API for method chaining</returns>
    IFluentConsumerApi<T> ForHighThroughput();
    
    /// <summary>
    /// Configures low-latency consumption settings
    /// </summary>
    /// <returns>Fluent consumer API for method chaining</returns>
    IFluentConsumerApi<T> ForLowLatency();
    
    /// <summary>
    /// Configures reliable consumption settings
    /// </summary>
    /// <returns>Fluent consumer API for method chaining</returns>
    IFluentConsumerApi<T> ForReliability();
    
    /// <summary>
    /// Applies configuration conditionally
    /// </summary>
    /// <param name="condition">Condition to evaluate</param>
    /// <param name="configure">Configuration action to apply if condition is true</param>
    /// <returns>Fluent consumer API for method chaining</returns>
    IFluentConsumerApi<T> When(bool condition, Action<IFluentConsumerApi<T>> configure);
    
    /// <summary>
    /// Starts consuming messages with the specified handler
    /// </summary>
    /// <param name="messageHandler">Message handler function</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Task representing the consumption operation</returns>
    Task ConsumeAsync(Func<T, MessageContext, Task<bool>> messageHandler, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Starts consuming messages with the specified handler and context
    /// </summary>
    /// <param name="messageHandler">Message handler function</param>
    /// <param name="context">Consumer context</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Task representing the consumption operation</returns>
    Task ConsumeAsync(Func<T, MessageContext, Task<bool>> messageHandler, ConsumerContext context, CancellationToken cancellationToken = default);
}

/// <summary>
/// Fluent API implementation for configuring and consuming messages with advanced settings
/// </summary>
/// <typeparam name="T">The type of messages to consume</typeparam>
/// <remarks>
/// This class provides a fluent interface for configuring message consumption with advanced features like:
/// - Automatic exchange and queue setup
/// - Retry policies with exponential backoff
/// - Dead letter queue handling
/// - Circuit breaker patterns
/// - Batch processing
/// - Message deduplication
/// - Concurrency control
/// - Timeout management
/// - Performance optimization presets
/// </remarks>
public class FluentConsumerApi<T> : IFluentConsumerApi<T> where T : class
{
    private readonly IMessageConsumer _consumer;
    private readonly string _queueName;
    private ConsumerContext _context;

    /// <summary>
    /// Initializes a new instance of the <see cref="FluentConsumerApi{T}"/> class
    /// </summary>
    /// <param name="consumer">The message consumer instance</param>
    /// <param name="queueName">The name of the queue to consume from</param>
    /// <exception cref="ArgumentNullException">
    /// Thrown when consumer or queueName is null
    /// </exception>
    public FluentConsumerApi(IMessageConsumer consumer, string queueName)
    {
        _consumer = consumer ?? throw new ArgumentNullException(nameof(consumer));
        _queueName = queueName ?? throw new ArgumentNullException(nameof(queueName));
        _context = ConsumerContext.CreateForQueue(queueName);
    }

    /// <summary>
    /// Configures the exchange to consume from
    /// </summary>
    /// <param name="exchangeName">The name of the exchange to consume from</param>
    /// <returns>The fluent consumer API for method chaining</returns>
    /// <exception cref="ArgumentNullException">
    /// Thrown when exchangeName is null
    /// </exception>
    public IFluentConsumerApi<T> FromExchange(string exchangeName)
    {
        _context.ExchangeName = exchangeName;
        return this;
    }

    /// <summary>
    /// Configures the routing key pattern for message filtering
    /// </summary>
    /// <param name="routingKey">The routing key pattern to filter messages</param>
    /// <returns>The fluent consumer API for method chaining</returns>
    /// <exception cref="ArgumentNullException">
    /// Thrown when routingKey is null
    /// </exception>
    public IFluentConsumerApi<T> WithRoutingKey(string routingKey)
    {
        _context.RoutingKey = routingKey;
        return this;
    }

    /// <summary>
    /// Configures consumer settings using a pre-configured settings object
    /// </summary>
    /// <param name="settings">The consumer settings to apply</param>
    /// <returns>The fluent consumer API for method chaining</returns>
    /// <exception cref="ArgumentNullException">
    /// Thrown when settings is null
    /// </exception>
    public IFluentConsumerApi<T> WithSettings(ConsumerSettings settings)
    {
        _context.Settings = settings ?? throw new ArgumentNullException(nameof(settings));
        return this;
    }

    /// <summary>
    /// Configures consumer settings using a configuration action
    /// </summary>
    /// <param name="configure">Action to configure the settings</param>
    /// <returns>The fluent consumer API for method chaining</returns>
    public IFluentConsumerApi<T> WithSettings(Action<ConsumerSettings> configure)
    {
        configure?.Invoke(_context.Settings);
        return this;
    }

    /// <summary>
    /// Configures a custom consumer tag for identification
    /// </summary>
    /// <param name="consumerTag">The consumer tag to use</param>
    /// <returns>The fluent consumer API for method chaining</returns>
    public IFluentConsumerApi<T> WithConsumerTag(string consumerTag)
    {
        _context.ConsumerTag = consumerTag;
        return this;
    }

    /// <summary>
    /// Configures automatic message acknowledgment
    /// </summary>
    /// <param name="autoAck">Whether to automatically acknowledge messages</param>
    /// <returns>The fluent consumer API for method chaining</returns>
    /// <remarks>
    /// When auto-acknowledge is enabled, messages are acknowledged immediately after delivery.
    /// Disable this for manual acknowledgment control and better error handling.
    /// </remarks>
    public IFluentConsumerApi<T> WithAutoAcknowledge(bool autoAck = true)
    {
        _context.AutoAcknowledge = autoAck;
        _context.Settings.AutoAcknowledge = autoAck;
        return this;
    }

    /// <summary>
    /// Configures the prefetch count (QoS) for message delivery
    /// </summary>
    /// <param name="count">Number of messages to prefetch</param>
    /// <param name="global">Whether to apply prefetch globally</param>
    /// <returns>The fluent consumer API for method chaining</returns>
    /// <remarks>
    /// Prefetch controls how many messages RabbitMQ will deliver before waiting for acknowledgments.
    /// Higher values improve throughput but increase memory usage.
    /// </remarks>
    public IFluentConsumerApi<T> WithPrefetch(ushort count, bool global = false)
    {
        _context.Settings.PrefetchCount = count;
        _context.Settings.GlobalPrefetch = global;
        return this;
    }

    /// <summary>
    /// Configures consumer priority for message delivery ordering
    /// </summary>
    /// <param name="priority">The consumer priority (higher values get priority)</param>
    /// <returns>The fluent consumer API for method chaining</returns>
    /// <remarks>
    /// Consumers with higher priority receive messages before lower priority consumers.
    /// </remarks>
    public IFluentConsumerApi<T> WithPriority(int priority)
    {
        _context.Priority = priority;
        _context.Settings.Priority = priority;
        return this;
    }

    /// <summary>
    /// Configures exclusive consumer access to the queue
    /// </summary>
    /// <param name="exclusive">Whether this consumer should have exclusive access</param>
    /// <returns>The fluent consumer API for method chaining</returns>
    /// <remarks>
    /// Exclusive consumers prevent other consumers from accessing the same queue.
    /// </remarks>
    public IFluentConsumerApi<T> WithExclusive(bool exclusive = true)
    {
        _context.Exclusive = exclusive;
        _context.Settings.Exclusive = exclusive;
        return this;
    }

    /// <summary>
    /// Configures maximum concurrent message processors
    /// </summary>
    /// <param name="maxConcurrent">Maximum number of concurrent message processors</param>
    /// <returns>The fluent consumer API for method chaining</returns>
    /// <remarks>
    /// Controls how many messages can be processed simultaneously.
    /// Higher values improve throughput but increase resource usage.
    /// </remarks>
    public IFluentConsumerApi<T> WithConcurrency(int maxConcurrent)
    {
        _context.MaxConcurrentMessages = maxConcurrent;
        _context.Settings.MaxConcurrentMessages = maxConcurrent;
        return this;
    }

    /// <summary>
    /// Configures processing timeout for message handling
    /// </summary>
    /// <param name="timeout">Maximum time allowed for message processing</param>
    /// <returns>The fluent consumer API for method chaining</returns>
    /// <remarks>
    /// Messages that take longer than the timeout will be considered failed.
    /// </remarks>
    public IFluentConsumerApi<T> WithTimeout(TimeSpan timeout)
    {
        _context.ProcessingTimeout = timeout;
        _context.Settings.ProcessingTimeout = timeout;
        return this;
    }

    /// <summary>
    /// Enables message deduplication to prevent duplicate processing
    /// </summary>
    /// <param name="cacheSize">Size of the deduplication cache</param>
    /// <param name="cacheTtl">Time-to-live for cache entries</param>
    /// <returns>The fluent consumer API for method chaining</returns>
    /// <remarks>
    /// Deduplication uses message IDs to detect and skip duplicate messages.
    /// Larger cache sizes provide better deduplication but use more memory.
    /// </remarks>
    public IFluentConsumerApi<T> WithDeduplication(int cacheSize = 10000, TimeSpan? cacheTtl = null)
    {
        _context.EnableDeduplication = true;
        _context.DeduplicationCacheSize = cacheSize;
        _context.Settings.EnableDeduplication = true;
        _context.Settings.DeduplicationCacheSize = cacheSize;
        
        if (cacheTtl.HasValue)
        {
            _context.Settings.DeduplicationCacheTtl = cacheTtl.Value;
        }
        
        return this;
    }

    /// <summary>
    /// Enables batch processing for improved throughput
    /// </summary>
    /// <param name="batchSize">Number of messages to process in each batch</param>
    /// <param name="batchTimeout">Maximum time to wait for a full batch</param>
    /// <returns>The fluent consumer API for method chaining</returns>
    /// <remarks>
    /// Batch processing groups messages together for more efficient processing.
    /// The batch is processed when either the size limit or timeout is reached.
    /// </remarks>
    public IFluentConsumerApi<T> WithBatchProcessing(int batchSize = 100, TimeSpan? batchTimeout = null)
    {
        _context.EnableBatchProcessing = true;
        _context.BatchSize = batchSize;
        _context.Settings.EnableBatchProcessing = true;
        _context.Settings.BatchSize = batchSize;
        
        if (batchTimeout.HasValue)
        {
            _context.BatchTimeout = batchTimeout.Value;
            _context.Settings.BatchTimeout = batchTimeout.Value;
        }
        
        return this;
    }

    /// <summary>
    /// Configures retry policy for failed message processing
    /// </summary>
    /// <param name="maxRetries">Maximum number of retry attempts</param>
    /// <param name="initialDelay">Initial delay between retries</param>
    /// <param name="maxDelay">Maximum delay between retries</param>
    /// <param name="backoffMultiplier">Multiplier for exponential backoff</param>
    /// <returns>The fluent consumer API for method chaining</returns>
    /// <remarks>
    /// Retry policy uses exponential backoff to space out retry attempts.
    /// Failed messages are retried according to the configured policy.
    /// </remarks>
    public IFluentConsumerApi<T> WithRetryPolicy(int maxRetries, TimeSpan initialDelay, TimeSpan? maxDelay = null, double backoffMultiplier = 2.0)
    {
        var retryPolicy = new RetryPolicySettings
        {
            MaxRetries = maxRetries,
            InitialDelayMs = (int)initialDelay.TotalMilliseconds,
            MaxDelayMs = (int)(maxDelay ?? TimeSpan.FromMinutes(5)).TotalMilliseconds,
            BackoffMultiplier = backoffMultiplier
        };
        
        _context.RetryPolicy = retryPolicy;
        _context.Settings.RetryPolicy = retryPolicy.PolicyType; // Convert to string
        
        return this;
    }

    /// <summary>
    /// Configures retry policy using a pre-configured settings object
    /// </summary>
    /// <param name="retryPolicy">The retry policy settings to apply</param>
    /// <returns>The fluent consumer API for method chaining</returns>
    /// <exception cref="ArgumentNullException">
    /// Thrown when retryPolicy is null
    /// </exception>
    public IFluentConsumerApi<T> WithRetryPolicy(RetryPolicySettings retryPolicy)
    {
        _context.RetryPolicy = retryPolicy ?? throw new ArgumentNullException(nameof(retryPolicy));
        _context.Settings.RetryPolicy = retryPolicy.PolicyType; // Convert to string
        return this;
    }

    /// <summary>
    /// Configures error handling strategy for failed messages
    /// </summary>
    /// <param name="strategy">The error handling strategy to use</param>
    /// <returns>The fluent consumer API for method chaining</returns>
    /// <remarks>
    /// Error handling strategy determines what happens to messages that fail processing.
    /// Options include retry, dead letter, or discard.
    /// </remarks>
    public IFluentConsumerApi<T> WithErrorHandling(ErrorHandlingStrategy strategy)
    {
        _context.ErrorHandling = strategy;
        _context.Settings.ErrorHandling = strategy.ToString(); // Convert to string
        return this;
    }

    /// <summary>
    /// Configures dead letter exchange for failed messages
    /// </summary>
    /// <param name="exchange">The dead letter exchange name</param>
    /// <param name="routingKey">The routing key for dead letter messages</param>
    /// <returns>The fluent consumer API for method chaining</returns>
    /// <remarks>
    /// Failed messages are sent to the dead letter exchange for manual inspection or reprocessing.
    /// </remarks>
    public IFluentConsumerApi<T> WithDeadLetter(string exchange, string? routingKey = null)
    {
        _context.DeadLetterExchange = exchange;
        _context.DeadLetterRoutingKey = routingKey;
        _context.Settings.DeadLetterExchange = exchange;
        _context.Settings.DeadLetterRoutingKey = routingKey;
        return this;
    }

    /// <summary>
    /// Enables circuit breaker pattern for fault tolerance
    /// </summary>
    /// <param name="failureThreshold">Number of consecutive failures before opening circuit</param>
    /// <param name="recoveryTimeout">Time to wait before attempting recovery</param>
    /// <returns>The fluent consumer API for method chaining</returns>
    /// <remarks>
    /// Circuit breaker prevents cascading failures by temporarily stopping processing
    /// when failure rates exceed the threshold.
    /// </remarks>
    public IFluentConsumerApi<T> WithCircuitBreaker(int failureThreshold = 5, TimeSpan? recoveryTimeout = null)
    {
        _context.EnableCircuitBreaker = true;
        _context.CircuitBreakerFailureThreshold = failureThreshold;
        _context.Settings.EnableCircuitBreaker = true;
        _context.Settings.CircuitBreakerFailureThreshold = failureThreshold;
        
        if (recoveryTimeout.HasValue)
        {
            _context.CircuitBreakerRecoveryTimeout = recoveryTimeout.Value;
            _context.Settings.CircuitBreakerRecoveryTimeout = recoveryTimeout.Value;
        }
        
        return this;
    }

    /// <summary>
    /// Configures serializer settings for message deserialization
    /// </summary>
    /// <param name="settings">The serializer settings to use</param>
    /// <returns>The fluent consumer API for method chaining</returns>
    /// <exception cref="ArgumentNullException">
    /// Thrown when settings is null
    /// </exception>
    public IFluentConsumerApi<T> WithSerializer(SerializerSettings settings)
    {
        _context.Serializer = settings ?? throw new ArgumentNullException(nameof(settings));
        _context.Settings.Serializer = settings.SerializerType; // Convert to string
        return this;
    }

    /// <summary>
    /// Configures custom headers for message filtering
    /// </summary>
    /// <param name="headers">Dictionary of custom headers</param>
    /// <returns>The fluent consumer API for method chaining</returns>
    /// <exception cref="ArgumentNullException">
    /// Thrown when headers is null
    /// </exception>
    public IFluentConsumerApi<T> WithHeaders(IDictionary<string, object> headers)
    {
        _context.Headers = headers;
        return this;
    }

    /// <summary>
    /// Adds a custom header for message filtering
    /// </summary>
    /// <param name="key">The header key</param>
    /// <param name="value">The header value</param>
    /// <returns>The fluent consumer API for method chaining</returns>
    public IFluentConsumerApi<T> WithHeader(string key, object value)
    {
        _context.Headers ??= new Dictionary<string, object>();
        _context.Headers[key] = value;
        return this;
    }

    /// <summary>
    /// Configures custom consumer arguments
    /// </summary>
    /// <param name="arguments">Dictionary of consumer arguments</param>
    /// <returns>The fluent consumer API for method chaining</returns>
    /// <exception cref="ArgumentNullException">
    /// Thrown when arguments is null
    /// </exception>
    public IFluentConsumerApi<T> WithArguments(IDictionary<string, object> arguments)
    {
        _context.Arguments = (arguments ?? throw new ArgumentNullException(nameof(arguments)))!;
        _context.Settings.Arguments = new Dictionary<string, object>(arguments); // Convert to Dictionary
        return this;
    }

    /// <summary>
    /// Adds a custom consumer argument
    /// </summary>
    /// <param name="key">The argument key</param>
    /// <param name="value">The argument value</param>
    /// <returns>The fluent consumer API for method chaining</returns>
    public IFluentConsumerApi<T> WithArgument(string key, object value)
    {
        _context.Arguments[key] = value;
        _context.Settings.Arguments[key] = value;
        return this;
    }

    /// <summary>
    /// Configures consumer for high-throughput scenarios
    /// </summary>
    /// <returns>The fluent consumer API for method chaining</returns>
    /// <remarks>
    /// Applies optimizations for high-throughput scenarios including increased prefetch,
    /// disabled auto-acknowledgment, and optimized concurrency settings.
    /// </remarks>
    public IFluentConsumerApi<T> ForHighThroughput()
    {
        _context = ConsumerContext.CreateHighThroughput(_queueName);
        return this;
    }

    /// <summary>
    /// Configures consumer for low-latency scenarios
    /// </summary>
    /// <returns>The fluent consumer API for method chaining</returns>
    /// <remarks>
    /// Applies optimizations for low-latency scenarios including reduced prefetch,
    /// immediate acknowledgment, and optimized processing settings.
    /// </remarks>
    public IFluentConsumerApi<T> ForLowLatency()
    {
        _context = ConsumerContext.CreateLowLatency(_queueName);
        return this;
    }

    /// <summary>
    /// Configures consumer for maximum reliability
    /// </summary>
    /// <returns>The fluent consumer API for method chaining</returns>
    /// <remarks>
    /// Applies settings for maximum reliability including manual acknowledgment,
    /// retry policies, dead letter queues, and circuit breaker patterns.
    /// </remarks>
    public IFluentConsumerApi<T> ForReliability()
    {
        _context = ConsumerContext.CreateReliable(_queueName);
        return this;
    }

    /// <summary>
    /// Applies configuration conditionally based on a boolean condition
    /// </summary>
    /// <param name="condition">The condition to evaluate</param>
    /// <param name="configure">Configuration action to apply if condition is true</param>
    /// <returns>The fluent consumer API for method chaining</returns>
    /// <remarks>
    /// This method allows conditional configuration based on runtime conditions.
    /// </remarks>
    public IFluentConsumerApi<T> When(bool condition, Action<IFluentConsumerApi<T>> configure)
    {
        if (condition)
        {
            configure?.Invoke(this);
        }
        return this;
    }

    /// <summary>
    /// Starts consuming messages with the specified handler
    /// </summary>
    /// <param name="messageHandler">Function to handle consumed messages</param>
    /// <param name="cancellationToken">Token to monitor for cancellation requests</param>
    /// <returns>A task that represents the asynchronous consumption operation</returns>
    /// <exception cref="ArgumentNullException">
    /// Thrown when messageHandler is null
    /// </exception>
    /// <remarks>
    /// The message handler should return true for successful processing or false for failures.
    /// Failed messages will be handled according to the configured error handling strategy.
    /// </remarks>
    public Task ConsumeAsync(Func<T, MessageContext, Task<bool>> messageHandler, CancellationToken cancellationToken = default)
    {
        return _consumer.ConsumeAsync(_queueName, messageHandler, _context, cancellationToken);
    }

    /// <summary>
    /// Starts consuming messages with the specified handler and context
    /// </summary>
    /// <param name="messageHandler">Function to handle consumed messages</param>
    /// <param name="context">Consumer context with additional configuration</param>
    /// <param name="cancellationToken">Token to monitor for cancellation requests</param>
    /// <returns>A task that represents the asynchronous consumption operation</returns>
    /// <exception cref="ArgumentNullException">
    /// Thrown when messageHandler or context is null
    /// </exception>
    public Task ConsumeAsync(Func<T, MessageContext, Task<bool>> messageHandler, ConsumerContext context, CancellationToken cancellationToken = default)
    {
        return _consumer.ConsumeAsync(_queueName, messageHandler, context ?? _context, cancellationToken);
    }
} 