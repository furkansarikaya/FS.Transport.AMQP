using FS.StreamFlow.Core.Features.Messaging.Models;

namespace FS.StreamFlow.Core.Features.Messaging.Interfaces;

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
    IFluentConsumerApi<T> WithAutoAck(bool autoAck);
    
    /// <summary>
    /// Configures prefetch count
    /// </summary>
    /// <param name="prefetchCount">Prefetch count</param>
    /// <returns>Fluent consumer API for method chaining</returns>
    IFluentConsumerApi<T> WithPrefetchCount(ushort prefetchCount);
    
    /// <summary>
    /// Configures concurrent consumers
    /// </summary>
    /// <param name="concurrency">Number of concurrent consumers</param>
    /// <returns>Fluent consumer API for method chaining</returns>
    IFluentConsumerApi<T> WithConcurrency(int concurrency);
    
    /// <summary>
    /// Configures error handling
    /// </summary>
    /// <param name="errorHandler">Error handler function</param>
    /// <returns>Fluent consumer API for method chaining</returns>
    IFluentConsumerApi<T> WithErrorHandler(Func<Exception, MessageContext, Task<bool>> errorHandler);
    
    /// <summary>
    /// Configures retry policy
    /// </summary>
    /// <param name="retryPolicy">Retry policy settings</param>
    /// <returns>Fluent consumer API for method chaining</returns>
    IFluentConsumerApi<T> WithRetryPolicy(RetryPolicySettings retryPolicy);
    
    /// <summary>
    /// Configures dead letter queue
    /// </summary>
    /// <param name="deadLetterSettings">Dead letter settings</param>
    /// <returns>Fluent consumer API for method chaining</returns>
    IFluentConsumerApi<T> WithDeadLetterQueue(DeadLetterSettings deadLetterSettings);
    
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