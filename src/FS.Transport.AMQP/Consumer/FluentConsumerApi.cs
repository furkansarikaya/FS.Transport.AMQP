using FS.Transport.AMQP.Configuration;
using FS.Transport.AMQP.ErrorHandling;

namespace FS.Transport.AMQP.Consumer;

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
    Task ConsumeAsync(Func<T, FS.Transport.AMQP.Producer.MessageContext, Task<bool>> messageHandler, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Starts consuming messages with the specified handler and context
    /// </summary>
    /// <param name="messageHandler">Message handler function</param>
    /// <param name="context">Consumer context</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Task representing the consumption operation</returns>
    Task ConsumeAsync(Func<T, FS.Transport.AMQP.Producer.MessageContext, Task<bool>> messageHandler, ConsumerContext context, CancellationToken cancellationToken = default);
}

/// <summary>
/// Implementation of fluent consumer API
/// </summary>
/// <typeparam name="T">Message type</typeparam>
public class FluentConsumerApi<T> : IFluentConsumerApi<T> where T : class
{
    private readonly IMessageConsumer _consumer;
    private readonly string _queueName;
    private ConsumerContext _context;

    public FluentConsumerApi(IMessageConsumer consumer, string queueName)
    {
        _consumer = consumer ?? throw new ArgumentNullException(nameof(consumer));
        _queueName = queueName ?? throw new ArgumentNullException(nameof(queueName));
        _context = ConsumerContext.CreateForQueue(queueName);
    }

    public IFluentConsumerApi<T> FromExchange(string exchangeName)
    {
        _context.ExchangeName = exchangeName;
        return this;
    }

    public IFluentConsumerApi<T> WithRoutingKey(string routingKey)
    {
        _context.RoutingKey = routingKey;
        return this;
    }

    public IFluentConsumerApi<T> WithSettings(ConsumerSettings settings)
    {
        _context.Settings = settings ?? throw new ArgumentNullException(nameof(settings));
        return this;
    }

    public IFluentConsumerApi<T> WithSettings(Action<ConsumerSettings> configure)
    {
        configure?.Invoke(_context.Settings);
        return this;
    }

    public IFluentConsumerApi<T> WithConsumerTag(string consumerTag)
    {
        _context.ConsumerTag = consumerTag;
        return this;
    }

    public IFluentConsumerApi<T> WithAutoAcknowledge(bool autoAck = true)
    {
        _context.AutoAcknowledge = autoAck;
        _context.Settings.AutoAcknowledge = autoAck;
        return this;
    }

    public IFluentConsumerApi<T> WithPrefetch(ushort count, bool global = false)
    {
        _context.Settings.PrefetchCount = count;
        _context.Settings.GlobalPrefetch = global;
        return this;
    }

    public IFluentConsumerApi<T> WithPriority(int priority)
    {
        _context.Priority = priority;
        _context.Settings.Priority = priority;
        return this;
    }

    public IFluentConsumerApi<T> WithExclusive(bool exclusive = true)
    {
        _context.Exclusive = exclusive;
        _context.Settings.Exclusive = exclusive;
        return this;
    }

    public IFluentConsumerApi<T> WithConcurrency(int maxConcurrent)
    {
        _context.MaxConcurrentMessages = maxConcurrent;
        _context.Settings.MaxConcurrentMessages = maxConcurrent;
        return this;
    }

    public IFluentConsumerApi<T> WithTimeout(TimeSpan timeout)
    {
        _context.ProcessingTimeout = timeout;
        _context.Settings.ProcessingTimeout = timeout;
        return this;
    }

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
        _context.Settings.RetryPolicy = retryPolicy;
        
        return this;
    }

    public IFluentConsumerApi<T> WithRetryPolicy(RetryPolicySettings retryPolicy)
    {
        _context.RetryPolicy = retryPolicy ?? throw new ArgumentNullException(nameof(retryPolicy));
        _context.Settings.RetryPolicy = retryPolicy;
        return this;
    }

    public IFluentConsumerApi<T> WithErrorHandling(ErrorHandlingStrategy strategy)
    {
        _context.ErrorHandling = strategy;
        _context.Settings.ErrorHandling = strategy;
        return this;
    }

    public IFluentConsumerApi<T> WithDeadLetter(string exchange, string? routingKey = null)
    {
        _context.DeadLetterExchange = exchange;
        _context.DeadLetterRoutingKey = routingKey;
        _context.Settings.DeadLetterExchange = exchange;
        _context.Settings.DeadLetterRoutingKey = routingKey;
        return this;
    }

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

    public IFluentConsumerApi<T> WithSerializer(SerializerSettings settings)
    {
        _context.Serializer = settings ?? throw new ArgumentNullException(nameof(settings));
        _context.Settings.Serializer = settings;
        return this;
    }

    public IFluentConsumerApi<T> WithHeaders(IDictionary<string, object> headers)
    {
        _context.Headers = headers;
        return this;
    }

    public IFluentConsumerApi<T> WithHeader(string key, object value)
    {
        _context.Headers ??= new Dictionary<string, object>();
        _context.Headers[key] = value;
        return this;
    }

    public IFluentConsumerApi<T> WithArguments(IDictionary<string, object> arguments)
    {
        _context.Arguments = arguments ?? throw new ArgumentNullException(nameof(arguments));
        _context.Settings.Arguments = arguments;
        return this;
    }

    public IFluentConsumerApi<T> WithArgument(string key, object value)
    {
        _context.Arguments[key] = value;
        _context.Settings.Arguments[key] = value;
        return this;
    }

    public IFluentConsumerApi<T> ForHighThroughput()
    {
        _context = ConsumerContext.CreateHighThroughput(_queueName);
        return this;
    }

    public IFluentConsumerApi<T> ForLowLatency()
    {
        _context = ConsumerContext.CreateLowLatency(_queueName);
        return this;
    }

    public IFluentConsumerApi<T> ForReliability()
    {
        _context = ConsumerContext.CreateReliable(_queueName);
        return this;
    }

    public IFluentConsumerApi<T> When(bool condition, Action<IFluentConsumerApi<T>> configure)
    {
        if (condition)
        {
            configure?.Invoke(this);
        }
        return this;
    }

    public Task ConsumeAsync(Func<T, FS.Transport.AMQP.Producer.MessageContext, Task<bool>> messageHandler, CancellationToken cancellationToken = default)
    {
        return _consumer.ConsumeAsync(_queueName, messageHandler, _context, cancellationToken);
    }

    public Task ConsumeAsync(Func<T, FS.Transport.AMQP.Producer.MessageContext, Task<bool>> messageHandler, ConsumerContext context, CancellationToken cancellationToken = default)
    {
        return _consumer.ConsumeAsync(_queueName, messageHandler, context ?? _context, cancellationToken);
    }
} 