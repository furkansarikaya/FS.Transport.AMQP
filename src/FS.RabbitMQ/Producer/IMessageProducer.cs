using FS.RabbitMQ.Events;

namespace FS.RabbitMQ.Producer;

/// <summary>
/// Interface for high-performance message producer with comprehensive features including retry policies, error handling, batching, confirmations, and monitoring
/// </summary>
public interface IMessageProducer : IDisposable
{
    /// <summary>
    /// Gets the current producer status
    /// </summary>
    ProducerStatus Status { get; }
    
    /// <summary>
    /// Gets producer configuration settings
    /// </summary>
    ProducerSettings Settings { get; }
    
    /// <summary>
    /// Gets producer statistics and metrics
    /// </summary>
    ProducerStatistics Statistics { get; }
    
    /// <summary>
    /// Publishes a message to the specified exchange with routing key
    /// </summary>
    /// <typeparam name="T">Message type</typeparam>
    /// <param name="message">Message to publish</param>
    /// <param name="exchange">Exchange name</param>
    /// <param name="routingKey">Routing key</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Publish result containing confirmation details</returns>
    Task<PublishResult> PublishAsync<T>(T message, string exchange, string routingKey, CancellationToken cancellationToken = default) where T : class;
    
    /// <summary>
    /// Publishes a message with full context and options
    /// </summary>
    /// <typeparam name="T">Message type</typeparam>
    /// <param name="message">Message to publish</param>
    /// <param name="context">Message context with exchange, routing key, and options</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Publish result containing confirmation details</returns>
    Task<PublishResult> PublishAsync<T>(T message, MessageContext context, CancellationToken cancellationToken = default) where T : class;
    
    /// <summary>
    /// Publishes a message with custom options
    /// </summary>
    /// <typeparam name="T">Message type</typeparam>
    /// <param name="message">Message to publish</param>
    /// <param name="exchange">Exchange name</param>
    /// <param name="routingKey">Routing key</param>
    /// <param name="options">Publish options</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Publish result containing confirmation details</returns>
    Task<PublishResult> PublishAsync<T>(T message, string exchange, string routingKey, PublishOptions options, CancellationToken cancellationToken = default) where T : class;
    
    /// <summary>
    /// Publishes an event (domain or integration event)
    /// </summary>
    /// <typeparam name="T">Event type</typeparam>
    /// <param name="event">Event to publish</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Publish result containing confirmation details</returns>
    Task<PublishResult> PublishEventAsync<T>(T @event, CancellationToken cancellationToken = default) where T : class, IEvent;
    
    /// <summary>
    /// Publishes an event with custom context
    /// </summary>
    /// <typeparam name="T">Event type</typeparam>
    /// <param name="event">Event to publish</param>
    /// <param name="context">Event context with routing information</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Publish result containing confirmation details</returns>
    Task<PublishResult> PublishEventAsync<T>(T @event, EventPublishContext context, CancellationToken cancellationToken = default) where T : class, IEvent;
    
    /// <summary>
    /// Publishes multiple messages in a batch for better performance
    /// </summary>
    /// <typeparam name="T">Message type</typeparam>
    /// <param name="messages">Messages to publish</param>
    /// <param name="exchange">Exchange name</param>
    /// <param name="routingKeySelector">Function to select routing key for each message</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Batch publish result with individual message results</returns>
    Task<BatchPublishResult> PublishBatchAsync<T>(IEnumerable<T> messages, string exchange, Func<T, string> routingKeySelector, CancellationToken cancellationToken = default) where T : class;
    
    /// <summary>
    /// Publishes multiple messages with individual contexts
    /// </summary>
    /// <typeparam name="T">Message type</typeparam>
    /// <param name="messageContexts">Messages with their contexts</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Batch publish result with individual message results</returns>
    Task<BatchPublishResult> PublishBatchAsync<T>(IEnumerable<MessageWithContext<T>> messageContexts, CancellationToken cancellationToken = default) where T : class;
    
    /// <summary>
    /// Publishes a message synchronously (blocking)
    /// </summary>
    /// <typeparam name="T">Message type</typeparam>
    /// <param name="message">Message to publish</param>
    /// <param name="exchange">Exchange name</param>
    /// <param name="routingKey">Routing key</param>
    /// <returns>Publish result containing confirmation details</returns>
    PublishResult Publish<T>(T message, string exchange, string routingKey) where T : class;
    
    /// <summary>
    /// Publishes a message synchronously with context
    /// </summary>
    /// <typeparam name="T">Message type</typeparam>
    /// <param name="message">Message to publish</param>
    /// <param name="context">Message context</param>
    /// <returns>Publish result containing confirmation details</returns>
    PublishResult Publish<T>(T message, MessageContext context) where T : class;
    
    /// <summary>
    /// Publishes a message with transactional support
    /// </summary>
    /// <typeparam name="T">Message type</typeparam>
    /// <param name="message">Message to publish</param>
    /// <param name="exchange">Exchange name</param>
    /// <param name="routingKey">Routing key</param>
    /// <param name="transactionId">Transaction identifier</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Publish result containing confirmation details</returns>
    Task<PublishResult> PublishTransactionalAsync<T>(T message, string exchange, string routingKey, string transactionId, CancellationToken cancellationToken = default) where T : class;
    
    /// <summary>
    /// Commits a transaction
    /// </summary>
    /// <param name="transactionId">Transaction identifier</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>True if transaction was committed successfully</returns>
    Task<bool> CommitTransactionAsync(string transactionId, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Rolls back a transaction
    /// </summary>
    /// <param name="transactionId">Transaction identifier</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>True if transaction was rolled back successfully</returns>
    Task<bool> RollbackTransactionAsync(string transactionId, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Schedules a message to be published at a specific time
    /// </summary>
    /// <typeparam name="T">Message type</typeparam>
    /// <param name="message">Message to schedule</param>
    /// <param name="exchange">Exchange name</param>
    /// <param name="routingKey">Routing key</param>
    /// <param name="scheduleTime">Time to publish the message</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Schedule result with scheduled message identifier</returns>
    Task<ScheduleResult> ScheduleAsync<T>(T message, string exchange, string routingKey, DateTimeOffset scheduleTime, CancellationToken cancellationToken = default) where T : class;
    
    /// <summary>
    /// Cancels a scheduled message
    /// </summary>
    /// <param name="scheduleId">Schedule identifier returned from ScheduleAsync</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>True if message was cancelled successfully</returns>
    Task<bool> CancelScheduledAsync(string scheduleId, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Waits for all pending publish confirmations to be received
    /// </summary>
    /// <param name="timeout">Maximum time to wait</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>True if all confirmations were received within timeout</returns>
    Task<bool> WaitForConfirmationsAsync(TimeSpan timeout, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Flushes all pending messages and waits for confirmations
    /// </summary>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>True if all messages were flushed and confirmed</returns>
    Task<bool> FlushAsync(CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Starts the producer (enables message publishing)
    /// </summary>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Task representing the start operation</returns>
    Task StartAsync(CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Stops the producer (disables message publishing)
    /// </summary>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Task representing the stop operation</returns>
    Task StopAsync(CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Event raised when a message is published successfully
    /// </summary>
    event EventHandler<MessagePublishedEventArgs>? MessagePublished;
    
    /// <summary>
    /// Event raised when a message publish fails
    /// </summary>
    event EventHandler<MessagePublishFailedEventArgs>? MessagePublishFailed;
    
    /// <summary>
    /// Event raised when a message is confirmed by the broker
    /// </summary>
    event EventHandler<MessageConfirmedEventArgs>? MessageConfirmed;
    
    /// <summary>
    /// Event raised when a message is rejected by the broker
    /// </summary>
    event EventHandler<MessageRejectedEventArgs>? MessageRejected;
    
    /// <summary>
    /// Event raised when producer status changes
    /// </summary>
    event EventHandler<ProducerStatusChangedEventArgs>? StatusChanged;
    
    /// <summary>
    /// Event raised when a batch publish operation completes
    /// </summary>
    event EventHandler<BatchPublishCompletedEventArgs>? BatchPublishCompleted;
}