using FS.StreamFlow.Core.Features.Messaging.Models;

namespace FS.StreamFlow.Core.Features.Messaging.Interfaces;

/// <summary>
/// Interface for managing messaging queues with declaration, binding, consumer management, and auto-recovery capabilities
/// </summary>
public interface IQueueManager : IDisposable
{
    /// <summary>
    /// Event raised when a queue is successfully declared
    /// </summary>
    event EventHandler<QueueEventArgs>? QueueDeclared;
    
    /// <summary>
    /// Event raised when a queue is deleted
    /// </summary>
    event EventHandler<QueueEventArgs>? QueueDeleted;
    
    /// <summary>
    /// Event raised when queues are being recreated during recovery
    /// </summary>
    event EventHandler<QueueEventArgs>? QueuesRecreating;
    
    /// <summary>
    /// Event raised when queues have been successfully recreated during recovery
    /// </summary>
    event EventHandler<QueueEventArgs>? QueuesRecreated;
    
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
    /// <param name="routingKey">Routing key</param>
    /// <param name="arguments">Additional arguments</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Task representing the binding operation</returns>
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
    /// <param name="routingKey">Routing key</param>
    /// <param name="arguments">Additional arguments</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Task representing the unbinding operation</returns>
    Task<bool> UnbindAsync(
        string queueName, 
        string exchangeName, 
        string routingKey, 
        IDictionary<string, object>? arguments = null,
        CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Purges all messages from a queue
    /// </summary>
    /// <param name="queueName">Queue name</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Number of messages purged</returns>
    Task<uint?> PurgeAsync(string queueName, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Gets information about a queue
    /// </summary>
    /// <param name="queueName">Queue name</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Queue information</returns>
    Task<QueueInfo?> GetQueueInfoAsync(string queueName, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Gets statistics for all queues
    /// </summary>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Queue statistics</returns>
    Task<QueueStatistics> GetStatisticsAsync(CancellationToken cancellationToken = default);
} 