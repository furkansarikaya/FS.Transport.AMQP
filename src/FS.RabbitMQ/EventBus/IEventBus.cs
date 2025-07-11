using FS.RabbitMQ.EventHandlers;
using FS.RabbitMQ.Events;

namespace FS.RabbitMQ.EventBus;

/// <summary>
/// High-level event bus interface for publishing and subscribing to events with automatic routing and handling
/// </summary>
public interface IEventBus : IDisposable
{
    /// <summary>
    /// Gets the current status of the event bus
    /// </summary>
    EventBusStatus Status { get; }
    
    /// <summary>
    /// Gets event bus statistics and metrics
    /// </summary>
    EventBusStatistics Statistics { get; }
    
    /// <summary>
    /// Starts the event bus and initializes connections
    /// </summary>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Task representing the start operation</returns>
    Task StartAsync(CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Stops the event bus and releases resources
    /// </summary>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Task representing the stop operation</returns>
    Task StopAsync(CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Publishes a domain event to the event bus
    /// </summary>
    /// <typeparam name="T">Domain event type</typeparam>
    /// <param name="domainEvent">Domain event to publish</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Task representing the publish operation</returns>
    Task PublishDomainEventAsync<T>(T domainEvent, CancellationToken cancellationToken = default) where T : class, IDomainEvent;
    
    /// <summary>
    /// Publishes an integration event to the event bus
    /// </summary>
    /// <typeparam name="T">Integration event type</typeparam>
    /// <param name="integrationEvent">Integration event to publish</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Task representing the publish operation</returns>
    Task PublishIntegrationEventAsync<T>(T integrationEvent, CancellationToken cancellationToken = default) where T : class, IIntegrationEvent;
    
    /// <summary>
    /// Publishes multiple events in a batch
    /// </summary>
    /// <param name="events">Events to publish</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Task representing the batch publish operation</returns>
    Task PublishBatchAsync(IEnumerable<IEvent> events, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Subscribes to domain events of a specific type
    /// </summary>
    /// <typeparam name="T">Domain event type</typeparam>
    /// <param name="handler">Event handler</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Task representing the subscription operation</returns>
    Task SubscribeToDomainEventAsync<T>(IAsyncEventHandler<T> handler, CancellationToken cancellationToken = default) where T : class, IDomainEvent;
    
    /// <summary>
    /// Subscribes to integration events of a specific type
    /// </summary>
    /// <typeparam name="T">Integration event type</typeparam>
    /// <param name="handler">Event handler</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Task representing the subscription operation</returns>
    Task SubscribeToIntegrationEventAsync<T>(IAsyncEventHandler<T> handler, CancellationToken cancellationToken = default) where T : class, IIntegrationEvent;
    
    /// <summary>
    /// Subscribes to events with a custom handler function
    /// </summary>
    /// <typeparam name="T">Event type</typeparam>
    /// <param name="handler">Handler function</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Task representing the subscription operation</returns>
    Task SubscribeAsync<T>(Func<T, EventContext, Task<bool>> handler, CancellationToken cancellationToken = default) where T : class, IEvent;
    
    /// <summary>
    /// Unsubscribes from events of a specific type
    /// </summary>
    /// <typeparam name="T">Event type</typeparam>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Task representing the unsubscription operation</returns>
    Task UnsubscribeAsync<T>(CancellationToken cancellationToken = default) where T : class, IEvent;
    
    /// <summary>
    /// Event raised when an event is published
    /// </summary>
    event EventHandler<EventPublishedEventArgs>? EventPublished;
    
    /// <summary>
    /// Event raised when an event is received
    /// </summary>
    event EventHandler<EventReceivedEventArgs>? EventReceived;
    
    /// <summary>
    /// Event raised when event processing fails
    /// </summary>
    event EventHandler<EventProcessingFailedEventArgs>? EventProcessingFailed;
}