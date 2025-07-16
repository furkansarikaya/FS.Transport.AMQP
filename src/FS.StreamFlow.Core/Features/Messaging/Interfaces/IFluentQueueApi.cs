using FS.StreamFlow.Core.Features.Messaging.Models;

namespace FS.StreamFlow.Core.Features.Messaging.Interfaces;

/// <summary>
/// Fluent API interface for queue configuration and management
/// </summary>
public interface IFluentQueueApi
{
    /// <summary>
    /// Configures whether the queue is durable (survives server restarts)
    /// </summary>
    /// <param name="durable">Whether the queue is durable</param>
    /// <returns>Fluent queue API for method chaining</returns>
    IFluentQueueApi WithDurable(bool durable = true);
    
    /// <summary>
    /// Configures whether the queue is exclusive (only used by one connection)
    /// </summary>
    /// <param name="exclusive">Whether the queue is exclusive</param>
    /// <returns>Fluent queue API for method chaining</returns>
    IFluentQueueApi WithExclusive(bool exclusive = true);
    
    /// <summary>
    /// Configures whether the queue auto-deletes when not in use
    /// </summary>
    /// <param name="autoDelete">Whether the queue auto-deletes</param>
    /// <returns>Fluent queue API for method chaining</returns>
    IFluentQueueApi WithAutoDelete(bool autoDelete = true);
    
    /// <summary>
    /// Configures queue arguments
    /// </summary>
    /// <param name="arguments">Queue arguments dictionary</param>
    /// <returns>Fluent queue API for method chaining</returns>
    IFluentQueueApi WithArguments(Dictionary<string, object> arguments);
    
    /// <summary>
    /// Adds a single argument to the queue
    /// </summary>
    /// <param name="key">Argument key</param>
    /// <param name="value">Argument value</param>
    /// <returns>Fluent queue API for method chaining</returns>
    IFluentQueueApi WithArgument(string key, object value);
    
    /// <summary>
    /// Configures dead letter exchange
    /// </summary>
    /// <param name="deadLetterExchange">Dead letter exchange name</param>
    /// <returns>Fluent queue API for method chaining</returns>
    IFluentQueueApi WithDeadLetterExchange(string deadLetterExchange);
    
    /// <summary>
    /// Configures dead letter routing key
    /// </summary>
    /// <param name="deadLetterRoutingKey">Dead letter routing key</param>
    /// <returns>Fluent queue API for method chaining</returns>
    IFluentQueueApi WithDeadLetterRoutingKey(string deadLetterRoutingKey);
    
    /// <summary>
    /// Configures message TTL (time-to-live)
    /// </summary>
    /// <param name="ttl">Message TTL</param>
    /// <returns>Fluent queue API for method chaining</returns>
    IFluentQueueApi WithMessageTtl(TimeSpan ttl);
    
    /// <summary>
    /// Configures maximum queue length
    /// </summary>
    /// <param name="maxLength">Maximum queue length</param>
    /// <returns>Fluent queue API for method chaining</returns>
    IFluentQueueApi WithMaxLength(int maxLength);
    
    /// <summary>
    /// Configures maximum queue length in bytes
    /// </summary>
    /// <param name="maxLengthBytes">Maximum queue length in bytes</param>
    /// <returns>Fluent queue API for method chaining</returns>
    IFluentQueueApi WithMaxLengthBytes(int maxLengthBytes);
    
    /// <summary>
    /// Configures queue priority
    /// </summary>
    /// <param name="priority">Queue priority</param>
    /// <returns>Fluent queue API for method chaining</returns>
    IFluentQueueApi WithPriority(int priority);
    
    /// <summary>
    /// Binds the queue to an exchange without a routing key
    /// </summary>
    /// <param name="exchange">Exchange name</param>
    /// <returns>Fluent queue API for method chaining</returns>
    IFluentQueueApi BindToExchange(string exchange);
    
    /// <summary>
    /// Binds the queue to an exchange
    /// </summary>
    /// <param name="exchange">Exchange name</param>
    /// <param name="routingKey">Routing key</param>
    /// <returns>Fluent queue API for method chaining</returns>
    IFluentQueueApi BindToExchange(string exchange, string routingKey);
    
    /// <summary>
    /// Binds the queue to an exchange with arguments
    /// </summary>
    /// <param name="exchange">Exchange name</param>
    /// <param name="routingKey">Routing key</param>
    /// <param name="arguments">Binding arguments</param>
    /// <returns>Fluent queue API for method chaining</returns>
    IFluentQueueApi BindToExchange(string exchange, string routingKey, Dictionary<string, object> arguments);
    
    /// <summary>
    /// Declares the queue with the configured settings
    /// </summary>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Queue declaration result</returns>
    Task<QueueDeclareResult?> DeclareAsync(CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Declares the queue synchronously with the configured settings
    /// </summary>
    /// <returns>Queue declaration result</returns>
    QueueDeclareResult? Declare();
    
    /// <summary>
    /// Deletes the queue
    /// </summary>
    /// <param name="ifUnused">Only delete if unused</param>
    /// <param name="ifEmpty">Only delete if empty</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Task representing the delete operation</returns>
    Task DeleteAsync(bool ifUnused = false, bool ifEmpty = false, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Purges the queue (removes all messages)
    /// </summary>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Number of messages purged</returns>
    Task<uint> PurgeAsync(CancellationToken cancellationToken = default);
} 