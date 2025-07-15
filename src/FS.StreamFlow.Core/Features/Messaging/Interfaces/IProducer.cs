using FS.StreamFlow.Core.Features.Messaging.Models;

namespace FS.StreamFlow.Core.Features.Messaging.Interfaces;

/// <summary>
/// Interface for RabbitMQ message producer with async operations and publisher confirms
/// </summary>
public interface IProducer : IDisposable
{
    /// <summary>
    /// Gets whether the producer is ready to publish messages
    /// </summary>
    bool IsReady { get; }

    /// <summary>
    /// Gets the producer settings
    /// </summary>
    ProducerSettings Settings { get; }

    /// <summary>
    /// Gets producer statistics
    /// </summary>
    ProducerStatistics Statistics { get; }

    /// <summary>
    /// Event raised when a message is confirmed by the broker
    /// </summary>
    event Func<ulong, bool, Task>? MessageConfirmed;

    /// <summary>
    /// Event raised when publisher encounters an error
    /// </summary>
    event Func<Exception, Task>? ErrorOccurred;

    /// <summary>
    /// Initializes the producer with a dedicated channel
    /// </summary>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Task representing the initialization operation</returns>
    Task InitializeAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Publishes a message to the specified exchange
    /// </summary>
    /// <param name="exchange">The exchange to publish to</param>
    /// <param name="routingKey">The routing key</param>
    /// <param name="message">The message to publish</param>
    /// <param name="properties">Message properties</param>
    /// <param name="mandatory">Whether the message is mandatory</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Task representing the publish operation, returns true if confirmed</returns>
    Task<bool> PublishAsync(
        string exchange,
        string routingKey,
        ReadOnlyMemory<byte> message,
        IDictionary<string, object>? properties = null,
        bool mandatory = false,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Publishes a serialized object as a message
    /// </summary>
    /// <typeparam name="T">The type of the object to publish</typeparam>
    /// <param name="exchange">The exchange to publish to</param>
    /// <param name="routingKey">The routing key</param>
    /// <param name="message">The object to publish</param>
    /// <param name="properties">Message properties</param>
    /// <param name="mandatory">Whether the message is mandatory</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Task representing the publish operation, returns true if confirmed</returns>
    Task<bool> PublishAsync<T>(
        string exchange,
        string routingKey,
        T message,
        IDictionary<string, object>? properties = null,
        bool mandatory = false,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Publishes multiple messages in a batch
    /// </summary>
    /// <param name="messages">The messages to publish</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Task representing the batch publish operation</returns>
    Task<BatchPublishResult> PublishBatchAsync(
        IEnumerable<StreamFlowMessageContext> messages,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Begins a transaction for transactional publishing
    /// </summary>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Task representing the transaction start operation</returns>
    Task BeginTransactionAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Commits the current transaction
    /// </summary>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Task representing the transaction commit operation</returns>
    Task CommitTransactionAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Rolls back the current transaction
    /// </summary>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Task representing the transaction rollback operation</returns>
    Task RollbackTransactionAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Publishes an event message
    /// </summary>
    /// <typeparam name="T">Event type</typeparam>
    /// <param name="eventMessage">Event to publish</param>
    /// <param name="context">Event publish context</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Task representing the publish operation</returns>
    Task<PublishResult> PublishEventAsync<T>(T eventMessage, EventPublishContext context, CancellationToken cancellationToken = default) where T : class;

    /// <summary>
    /// Schedules a message for future delivery
    /// </summary>
    /// <typeparam name="T">Message type</typeparam>
    /// <param name="message">Message to schedule</param>
    /// <param name="delay">Delay before delivery</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Task representing the schedule operation</returns>
    Task<bool> ScheduleAsync<T>(T message, TimeSpan delay, CancellationToken cancellationToken = default) where T : class;

    /// <summary>
    /// Publishes multiple messages in a transaction
    /// </summary>
    /// <param name="messages">Messages to publish</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Task representing the transactional publish operation</returns>
    Task<BatchPublishResult> PublishTransactionalAsync(IEnumerable<MessageContext> messages, CancellationToken cancellationToken = default);

    /// <summary>
    /// Publishes a message synchronously
    /// </summary>
    /// <param name="exchange">Exchange to publish to</param>
    /// <param name="routingKey">Routing key</param>
    /// <param name="message">Message body</param>
    /// <param name="properties">Message properties</param>
    /// <param name="mandatory">Whether message is mandatory</param>
    /// <returns>True if published successfully</returns>
    bool Publish(string exchange, string routingKey, ReadOnlyMemory<byte> message, BasicProperties? properties = null, bool mandatory = false);
    
    /// <summary>
    /// Creates a fluent API for advanced message publishing configuration
    /// </summary>
    /// <typeparam name="T">Message type</typeparam>
    /// <returns>Fluent producer API for method chaining</returns>
    IFluentProducerApi<T> Message<T>() where T : class;
    
    /// <summary>
    /// Creates a fluent API for advanced message publishing configuration with pre-configured message
    /// </summary>
    /// <typeparam name="T">Message type</typeparam>
    /// <param name="message">Message to configure for publishing</param>
    /// <returns>Fluent producer API for method chaining</returns>
    IFluentProducerApi<T> Message<T>(T message) where T : class;
}