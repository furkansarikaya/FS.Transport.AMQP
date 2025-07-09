using FS.Transport.AMQP.Connection;
using FS.Transport.AMQP.Consumer;
using FS.Transport.AMQP.EventBus;
using FS.Transport.AMQP.EventStore;
using FS.Transport.AMQP.Exchange;
using FS.Transport.AMQP.Monitoring;
using FS.Transport.AMQP.Producer;
using FS.Transport.AMQP.Queue;

namespace FS.Transport.AMQP.Core;

/// <summary>
/// Main RabbitMQ client interface providing high-level operations for messaging, event-driven architecture, and connection management
/// </summary>
public interface IRabbitMQClient : IDisposable
{
    /// <summary>
    /// Gets connection manager for managing RabbitMQ connections with auto-reconnection and health monitoring
    /// </summary>
    IConnectionManager ConnectionManager { get; }
    
    /// <summary>
    /// Gets exchange manager for exchange declaration, binding, and management operations
    /// </summary>
    IExchangeManager ExchangeManager { get; }
    
    /// <summary>
    /// Gets queue manager for queue declaration, binding, and management operations
    /// </summary>
    IQueueManager QueueManager { get; }
    
    /// <summary>
    /// Gets message producer for publishing messages with retry policies and error handling
    /// </summary>
    IMessageProducer Producer { get; }
    
    /// <summary>
    /// Gets message consumer for consuming messages with automatic acknowledgment and error handling
    /// </summary>
    IMessageConsumer Consumer { get; }
    
    /// <summary>
    /// Gets event bus for event-driven messaging, domain events, and integration events
    /// </summary>
    IEventBus EventBus { get; }
    
    /// <summary>
    /// Gets event store for event sourcing operations and aggregate reconstruction
    /// </summary>
    IEventStore EventStore { get; }
    
    /// <summary>
    /// Gets health checker for monitoring connection health and service availability
    /// </summary>
    IHealthChecker HealthChecker { get; }
    
    /// <summary>
    /// Gets the current client status and connection state
    /// </summary>
    ClientStatus Status { get; }
    
    /// <summary>
    /// Initializes the client and establishes initial connections
    /// </summary>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Task representing the initialization operation</returns>
    Task InitializeAsync(CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Shuts down the client gracefully, closing all connections and releasing resources
    /// </summary>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Task representing the shutdown operation</returns>
    Task ShutdownAsync(CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Event raised when client status changes
    /// </summary>
    event EventHandler<ClientStatusChangedEventArgs>? StatusChanged;
}