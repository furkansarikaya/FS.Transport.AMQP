using FS.Transport.AMQP.Configuration;

namespace FS.Transport.AMQP.Queue;

/// <summary>
/// Interface for managing RabbitMQ queues with declaration, binding, consumer management, and auto-recovery capabilities
/// </summary>
public interface IQueueManager
{
    /// <summary>
    /// Declares a queue with the specified configuration
    /// </summary>
    /// <param name="queueSettings">Queue configuration</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Queue declaration result with message count and consumer count</returns>
    Task<QueueDeclareResult?> DeclareAsync(QueueSettings queueSettings, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Declares a queue with manual parameters
    /// </summary>
    /// <param name="name">Queue name</param>
    /// <param name="durable">Whether queue is durable</param>
    /// <param name="exclusive">Whether queue is exclusive</param>
    /// <param name="autoDelete">Whether queue auto-deletes</param>
    /// <param name="arguments">Additional arguments</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Queue declaration result</returns>
    Task<QueueDeclareResult?> DeclareAsync(
        string name, 
        bool durable = true, 
        bool exclusive = false, 
        bool autoDelete = false, 
        IDictionary<string, object>? arguments = null,
        CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Deletes a queue
    /// </summary>
    /// <param name="name">Queue name</param>
    /// <param name="ifUnused">Only delete if unused</param>
    /// <param name="ifEmpty">Only delete if empty</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Number of messages deleted</returns>
    Task<uint?> DeleteAsync(string name, bool ifUnused = false, bool ifEmpty = false, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Binds a queue to an exchange
    /// </summary>
    /// <param name="queueName">Queue name</param>
    /// <param name="exchangeName">Exchange name</param>
    /// <param name="routingKey">Routing key for binding</param>
    /// <param name="arguments">Additional arguments</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>True if binding was created successfully</returns>
    Task<bool> BindAsync(
        string queueName, 
        string exchangeName, 
        string routingKey, 
        IDictionary<string, object>? arguments = null,
        CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Unbinds a queue from an exchange
    /// </summary>
    /// <param name="queueName">Queue name</param>
    /// <param name="exchangeName">Exchange name</param>
    /// <param name="routingKey">Routing key for binding</param>
    /// <param name="arguments">Additional arguments</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>True if binding was removed successfully</returns>
    Task<bool> UnbindAsync(
        string queueName, 
        string exchangeName, 
        string routingKey, 
        IDictionary<string, object>? arguments = null,
        CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Checks if a queue exists
    /// </summary>
    /// <param name="name">Queue name</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>True if queue exists</returns>
    Task<bool> ExistsAsync(string name, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Gets information about a queue
    /// </summary>
    /// <param name="name">Queue name</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Queue information or null if not found</returns>
    Task<QueueInfo?> GetInfoAsync(string name, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Purges all messages from a queue
    /// </summary>
    /// <param name="name">Queue name</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Number of messages purged</returns>
    Task<uint?> PurgeAsync(string name, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Gets the message count for a queue
    /// </summary>
    /// <param name="name">Queue name</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Number of messages in queue</returns>
    Task<uint?> GetMessageCountAsync(string name, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Gets the consumer count for a queue
    /// </summary>
    /// <param name="name">Queue name</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Number of consumers connected to queue</returns>
    Task<uint?> GetConsumerCountAsync(string name, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Declares all configured queues from settings
    /// </summary>
    /// <param name="queues">Queue configurations</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>True if all queues were declared successfully</returns>
    Task<bool> DeclareAllAsync(IEnumerable<QueueSettings> queues, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Re-declares all registered queues (used for auto-recovery)
    /// </summary>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>True if all queues were re-declared successfully</returns>
    Task<bool> RedeclareAllAsync(CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Gets queue management statistics
    /// </summary>
    /// <returns>Queue statistics</returns>
    QueueStatistics GetStatistics();
    
    /// <summary>
    /// Event raised when a queue is declared
    /// </summary>
    event EventHandler<QueueEventArgs>? QueueDeclared;
    
    /// <summary>
    /// Event raised when a queue is deleted
    /// </summary>
    event EventHandler<QueueEventArgs>? QueueDeleted;
    
    /// <summary>
    /// Event raised when a queue is purged
    /// </summary>
    event EventHandler<QueueEventArgs>? QueuePurged;
    
    /// <summary>
    /// Event raised when queues are being recreated (auto-recovery)
    /// </summary>
    event EventHandler<QueueEventArgs>? QueuesRecreating;
    
    /// <summary>
    /// Event raised when queues recreation is completed
    /// </summary>
    event EventHandler<QueueEventArgs>? QueuesRecreated;
}