using FS.RabbitMQ.EventHandlers;
using FS.RabbitMQ.Events;
using FS.RabbitMQ.Producer;

namespace FS.RabbitMQ.Consumer;

/// <summary>
/// Interface for high-performance message consumer with enterprise features including automatic acknowledgment, retry policies, error handling, and monitoring
/// </summary>
public interface IMessageConsumer : IDisposable
{
    /// <summary>
    /// Gets the current consumer status
    /// </summary>
    ConsumerStatus Status { get; }
    
    /// <summary>
    /// Gets consumer configuration settings
    /// </summary>
    ConsumerSettings Settings { get; }
    
    /// <summary>
    /// Gets consumer statistics and metrics
    /// </summary>
    ConsumerStatistics Statistics { get; }
    
    /// <summary>
    /// Starts the consumer and begins consuming messages
    /// </summary>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Task representing the start operation</returns>
    Task StartAsync(CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Stops the consumer and releases resources
    /// </summary>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Task representing the stop operation</returns>
    Task StopAsync(CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Consumes messages from a queue with automatic deserialization and processing
    /// </summary>
    /// <typeparam name="T">Message type</typeparam>
    /// <param name="queueName">Queue name to consume from</param>
    /// <param name="messageHandler">Handler function for processing messages</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Task representing the consumption operation</returns>
    Task ConsumeAsync<T>(string queueName, Func<T, MessageContext, Task<bool>> messageHandler, CancellationToken cancellationToken = default) where T : class;
    
    /// <summary>
    /// Consumes messages from a queue with consumer context and options
    /// </summary>
    /// <typeparam name="T">Message type</typeparam>
    /// <param name="queueName">Queue name to consume from</param>
    /// <param name="messageHandler">Handler function for processing messages</param>
    /// <param name="context">Consumer context with settings and options</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Task representing the consumption operation</returns>
    Task ConsumeAsync<T>(string queueName, Func<T, MessageContext, Task<bool>> messageHandler, ConsumerContext context, CancellationToken cancellationToken = default) where T : class;
    
    /// <summary>
    /// Consumes events from an exchange with automatic event handler resolution
    /// </summary>
    /// <typeparam name="T">Event type</typeparam>
    /// <param name="exchangeName">Exchange name to consume from</param>
    /// <param name="routingKey">Routing key pattern</param>
    /// <param name="eventHandler">Event handler for processing events</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Task representing the consumption operation</returns>
    Task ConsumeEventAsync<T>(string exchangeName, string routingKey, IAsyncEventHandler<T> eventHandler, CancellationToken cancellationToken = default) where T : class, IEvent;
    
    /// <summary>
    /// Consumes events from an exchange with inline event handler
    /// </summary>
    /// <typeparam name="T">Event type</typeparam>
    /// <param name="exchangeName">Exchange name to consume from</param>
    /// <param name="routingKey">Routing key pattern</param>
    /// <param name="eventHandler">Inline event handler function</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Task representing the consumption operation</returns>
    Task ConsumeEventAsync<T>(string exchangeName, string routingKey, Func<T, EventContext, Task<bool>> eventHandler, CancellationToken cancellationToken = default) where T : class, IEvent;
    
    /// <summary>
    /// Consumes domain events with automatic aggregate handling
    /// </summary>
    /// <typeparam name="T">Domain event type</typeparam>
    /// <param name="aggregateType">Aggregate type to consume events for</param>
    /// <param name="eventHandler">Domain event handler</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Task representing the consumption operation</returns>
    Task ConsumeDomainEventAsync<T>(string aggregateType, Func<T, EventContext, Task<bool>> eventHandler, CancellationToken cancellationToken = default) where T : class, IDomainEvent;
    
    /// <summary>
    /// Consumes integration events with service-to-service communication
    /// </summary>
    /// <typeparam name="T">Integration event type</typeparam>
    /// <param name="serviceName">Service name to consume events from</param>
    /// <param name="eventHandler">Integration event handler</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Task representing the consumption operation</returns>
    Task ConsumeIntegrationEventAsync<T>(string serviceName, Func<T, EventContext, Task<bool>> eventHandler, CancellationToken cancellationToken = default) where T : class, IIntegrationEvent;
    
    /// <summary>
    /// Creates a fluent API for advanced consumer configuration
    /// </summary>
    /// <typeparam name="T">Message type</typeparam>
    /// <param name="queueName">Queue name to consume from</param>
    /// <returns>Fluent consumer API</returns>
    IFluentConsumerApi<T> Fluent<T>(string queueName) where T : class;
    
    /// <summary>
    /// Acknowledges a message manually
    /// </summary>
    /// <param name="deliveryTag">Delivery tag of the message</param>
    /// <param name="multiple">Whether to acknowledge multiple messages</param>
    /// <returns>Task representing the acknowledgment operation</returns>
    Task AcknowledgeAsync(ulong deliveryTag, bool multiple = false);
    
    /// <summary>
    /// Rejects a message and optionally requeues it
    /// </summary>
    /// <param name="deliveryTag">Delivery tag of the message</param>
    /// <param name="requeue">Whether to requeue the message</param>
    /// <returns>Task representing the rejection operation</returns>
    Task RejectAsync(ulong deliveryTag, bool requeue = false);
    
    /// <summary>
    /// Requeues a message for later processing
    /// </summary>
    /// <param name="deliveryTag">Delivery tag of the message</param>
    /// <returns>Task representing the requeue operation</returns>
    Task RequeueAsync(ulong deliveryTag);
    
    /// <summary>
    /// Pauses message consumption
    /// </summary>
    /// <returns>Task representing the pause operation</returns>
    Task PauseAsync();
    
    /// <summary>
    /// Resumes message consumption
    /// </summary>
    /// <returns>Task representing the resume operation</returns>
    Task ResumeAsync();
    
    /// <summary>
    /// Gets the current number of unprocessed messages in the queue
    /// </summary>
    /// <param name="queueName">Queue name</param>
    /// <returns>Number of unprocessed messages</returns>
    Task<uint> GetMessageCountAsync(string queueName);
    
    /// <summary>
    /// Gets the current number of consumers for a queue
    /// </summary>
    /// <param name="queueName">Queue name</param>
    /// <returns>Number of consumers</returns>
    Task<uint> GetConsumerCountAsync(string queueName);
    
    /// <summary>
    /// Event raised when a message is received
    /// </summary>
    event EventHandler<MessageReceivedEventArgs>? MessageReceived;
    
    /// <summary>
    /// Event raised when a message is processed successfully
    /// </summary>
    event EventHandler<MessageProcessedEventArgs>? MessageProcessed;
    
    /// <summary>
    /// Event raised when message processing fails
    /// </summary>
    event EventHandler<MessageProcessingFailedEventArgs>? MessageProcessingFailed;
    
    /// <summary>
    /// Event raised when a message is acknowledged
    /// </summary>
    event EventHandler<MessageAcknowledgedEventArgs>? MessageAcknowledged;
    
    /// <summary>
    /// Event raised when a message is rejected
    /// </summary>
    event EventHandler<MessageRejectedEventArgs>? MessageRejected;
    
    /// <summary>
    /// Event raised when consumer status changes
    /// </summary>
    event EventHandler<ConsumerStatusChangedEventArgs>? StatusChanged;
    
    /// <summary>
    /// Event raised when consumer is paused
    /// </summary>
    event EventHandler<ConsumerPausedEventArgs>? ConsumerPaused;
    
    /// <summary>
    /// Event raised when consumer is resumed
    /// </summary>
    event EventHandler<ConsumerResumedEventArgs>? ConsumerResumed;
}