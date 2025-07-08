namespace FS.RabbitMQ.Core.Abstractions;

/// <summary>
/// Defines the contract for publishing messages to RabbitMQ exchanges and queues.
/// </summary>
/// <remarks>
/// The message publisher provides high-level abstractions for sending messages through
/// various publishing patterns while handling connection management, serialization,
/// and delivery confirmation automatically. It supports both fire-and-forget and
/// reliable publishing scenarios with appropriate error handling and retry mechanisms.
/// 
/// Key design principles:
/// - Async-first API design for non-blocking operations
/// - Support for both typed and untyped message publishing
/// - Flexible routing options (exchange-based and direct queue publishing)
/// - Comprehensive delivery confirmation and error handling
/// - Built-in serialization support with customizable serializers
/// - Transaction and batch publishing support for high-throughput scenarios
/// 
/// The publisher automatically manages underlying RabbitMQ channels and handles
/// connection recovery, making it safe for use in long-running applications
/// with varying network conditions.
/// </remarks>
public interface IMessagePublisher
{
    /// <summary>
    /// Publishes a message to the specified exchange with routing key.
    /// </summary>
    /// <typeparam name="T">The type of the message payload.</typeparam>
    /// <param name="message">The message payload to publish. Cannot be null.</param>
    /// <param name="exchangeName">The name of the exchange to publish to. Cannot be null or whitespace.</param>
    /// <param name="routingKey">The routing key for message routing. Cannot be null but can be empty.</param>
    /// <param name="options">Optional publishing options for customizing message properties and behavior.</param>
    /// <param name="cancellationToken">Token to monitor for cancellation requests.</param>
    /// <returns>
    /// A task that represents the asynchronous publishing operation.
    /// The task result contains confirmation details about the publish operation.
    /// </returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="message"/> is null.</exception>
    /// <exception cref="ArgumentException">Thrown when <paramref name="exchangeName"/> is null, empty, or whitespace.</exception>
    /// <exception cref="PublishException">Thrown when the message cannot be published due to broker or configuration issues.</exception>
    /// <exception cref="SerializationException">Thrown when the message cannot be serialized to the target format.</exception>
    /// <remarks>
    /// This method provides exchange-based publishing, which is the standard approach for
    /// RabbitMQ message routing. The exchange determines how messages are routed to queues
    /// based on the exchange type and routing key:
    /// 
    /// - Direct exchanges: Routing key must exactly match queue binding key
    /// - Fanout exchanges: Routing key is ignored, all bound queues receive messages
    /// - Topic exchanges: Routing key is matched against wildcard patterns in bindings
    /// - Headers exchanges: Routing is based on message headers rather than routing key
    /// 
    /// Message serialization is handled automatically using the configured message serializer.
    /// The method supports delivery confirmation when publisher confirms are enabled,
    /// providing delivery guarantees for reliable messaging scenarios.
    /// </remarks>
    Task<PublishResult> PublishAsync<T>(
        T message,
        string exchangeName,
        string routingKey,
        PublishOptions? options = null,
        CancellationToken cancellationToken = default) where T : class;

    /// <summary>
    /// Publishes a message directly to the specified queue, bypassing exchange routing.
    /// </summary>
    /// <typeparam name="T">The type of the message payload.</typeparam>
    /// <param name="message">The message payload to publish. Cannot be null.</param>
    /// <param name="queueName">The name of the queue to publish directly to. Cannot be null or whitespace.</param>
    /// <param name="options">Optional publishing options for customizing message properties and behavior.</param>
    /// <param name="cancellationToken">Token to monitor for cancellation requests.</param>
    /// <returns>
    /// A task that represents the asynchronous publishing operation.
    /// The task result contains confirmation details about the publish operation.
    /// </returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="message"/> is null.</exception>
    /// <exception cref="ArgumentException">Thrown when <paramref name="queueName"/> is null, empty, or whitespace.</exception>
    /// <exception cref="PublishException">Thrown when the message cannot be published due to broker or configuration issues.</exception>
    /// <exception cref="SerializationException">Thrown when the message cannot be serialized to the target format.</exception>
    /// <remarks>
    /// Direct queue publishing bypasses the exchange routing mechanism and sends messages
    /// directly to the specified queue using the default exchange (empty string exchange name).
    /// This approach is useful for:
    /// - Simple point-to-point messaging patterns
    /// - Direct worker queue scenarios
    /// - Bypassing complex routing logic when the target queue is known
    /// - Legacy integration scenarios requiring direct queue access
    /// 
    /// Note: The target queue must exist before publishing. This method does not create
    /// the queue automatically and will fail if the queue does not exist.
    /// </remarks>
    Task<PublishResult> PublishToQueueAsync<T>(
        T message,
        string queueName,
        PublishOptions? options = null,
        CancellationToken cancellationToken = default) where T : class;

    /// <summary>
    /// Publishes multiple messages in a single batch operation for improved throughput.
    /// </summary>
    /// <typeparam name="T">The type of the message payloads.</typeparam>
    /// <param name="messages">The collection of messages to publish. Cannot be null or empty.</param>
    /// <param name="exchangeName">The name of the exchange to publish to. Cannot be null or whitespace.</param>
    /// <param name="routingKeySelector">A function to determine the routing key for each message. Cannot be null.</param>
    /// <param name="options">Optional publishing options applied to all messages in the batch.</param>
    /// <param name="cancellationToken">Token to monitor for cancellation requests.</param>
    /// <returns>
    /// A task that represents the asynchronous batch publishing operation.
    /// The task result contains confirmation details for each message in the batch.
    /// </returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="messages"/> or <paramref name="routingKeySelector"/> is null.</exception>
    /// <exception cref="ArgumentException">Thrown when <paramref name="exchangeName"/> is null, empty, or whitespace, or when <paramref name="messages"/> is empty.</exception>
    /// <exception cref="PublishException">Thrown when one or more messages cannot be published.</exception>
    /// <exception cref="SerializationException">Thrown when one or more messages cannot be serialized.</exception>
    /// <remarks>
    /// Batch publishing provides significant performance improvements for high-throughput scenarios
    /// by reducing network round-trips and allowing the broker to process multiple messages
    /// more efficiently. Key benefits include:
    /// 
    /// - Reduced network overhead through message batching
    /// - Improved broker-side processing efficiency
    /// - Lower latency for high-volume publishing scenarios
    /// - Atomic confirmation for the entire batch when using publisher confirms
    /// 
    /// The routing key selector function is called for each message, allowing dynamic
    /// routing key assignment based on message content. This enables sophisticated
    /// routing patterns while maintaining batch publishing efficiency.
    /// 
    /// All messages in the batch are published to the same exchange but can have
    /// different routing keys determined by the selector function.
    /// </remarks>
    Task<BatchPublishResult> PublishBatchAsync<T>(
        IEnumerable<T> messages,
        string exchangeName,
        Func<T, string> routingKeySelector,
        PublishOptions? options = null,
        CancellationToken cancellationToken = default) where T : class;

    /// <summary>
    /// Publishes a message with request-response semantics, waiting for a correlated response.
    /// </summary>
    /// <typeparam name="TRequest">The type of the request message payload.</typeparam>
    /// <typeparam name="TResponse">The type of the expected response message payload.</typeparam>
    /// <param name="request">The request message payload to publish. Cannot be null.</param>
    /// <param name="exchangeName">The name of the exchange to publish the request to. Cannot be null or whitespace.</param>
    /// <param name="routingKey">The routing key for request routing. Cannot be null but can be empty.</param>
    /// <param name="timeout">The maximum time to wait for a response. Must be positive.</param>
    /// <param name="options">Optional publishing options for customizing request properties and behavior.</param>
    /// <param name="cancellationToken">Token to monitor for cancellation requests.</param>
    /// <returns>
    /// A task that represents the asynchronous request-response operation.
    /// The task result contains the correlated response message.
    /// </returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="request"/> is null.</exception>
    /// <exception cref="ArgumentException">
    /// Thrown when <paramref name="exchangeName"/> is null, empty, or whitespace, or when <paramref name="timeout"/> is not positive.
    /// </exception>
    /// <exception cref="TimeoutException">Thrown when no response is received within the specified timeout period.</exception>
    /// <exception cref="PublishException">Thrown when the request cannot be published.</exception>
    /// <exception cref="SerializationException">Thrown when the request or response cannot be serialized/deserialized.</exception>
    /// <remarks>
    /// Request-response messaging implements the RPC (Remote Procedure Call) pattern over
    /// RabbitMQ, providing synchronous-style interaction over asynchronous messaging infrastructure.
    /// The implementation includes:
    /// 
    /// - Automatic creation of temporary, exclusive reply queues
    /// - Correlation ID generation and matching for response correlation
    /// - Timeout handling for requests that don't receive responses
    /// - Proper cleanup of temporary resources after completion
    /// 
    /// This pattern is useful for:
    /// - Synchronous-style APIs over messaging infrastructure
    /// - Service-to-service communication requiring responses
    /// - Command-query scenarios where immediate feedback is needed
    /// - Integration with synchronous systems through messaging
    /// 
    /// Consider using asynchronous publish-subscribe patterns for better scalability
    /// in high-throughput scenarios where immediate responses are not required.
    /// </remarks>
    Task<TResponse> RequestAsync<TRequest, TResponse>(
        TRequest request,
        string exchangeName,
        string routingKey,
        TimeSpan timeout,
        PublishOptions? options = null,
        CancellationToken cancellationToken = default)
        where TRequest : class
        where TResponse : class;

    /// <summary>
    /// Schedules a message for delayed delivery at the specified future time.
    /// </summary>
    /// <typeparam name="T">The type of the message payload.</typeparam>
    /// <param name="message">The message payload to schedule. Cannot be null.</param>
    /// <param name="exchangeName">The name of the exchange to publish to when the delay expires. Cannot be null or whitespace.</param>
    /// <param name="routingKey">The routing key for message routing when delivered. Cannot be null but can be empty.</param>
    /// <param name="deliveryTime">The UTC time when the message should be delivered. Must be in the future.</param>
    /// <param name="options">Optional publishing options for customizing message properties and behavior.</param>
    /// <param name="cancellationToken">Token to monitor for cancellation requests.</param>
    /// <returns>
    /// A task that represents the asynchronous scheduling operation.
    /// The task result contains confirmation details about the scheduling operation.
    /// </returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="message"/> is null.</exception>
    /// <exception cref="ArgumentException">
    /// Thrown when <paramref name="exchangeName"/> is null, empty, or whitespace, or when <paramref name="deliveryTime"/> is not in the future.
    /// </exception>
    /// <exception cref="PublishException">Thrown when the message cannot be scheduled due to broker or configuration issues.</exception>
    /// <exception cref="SerializationException">Thrown when the message cannot be serialized.</exception>
    /// <remarks>
    /// Delayed message delivery enables time-based messaging patterns such as:
    /// - Reminder and notification systems
    /// - Scheduled batch processing tasks
    /// - Timeout and deadline handling in workflows
    /// - Rate limiting and throttling mechanisms
    /// - Retry scheduling with exponential backoff
    /// 
    /// Implementation may use:
    /// - RabbitMQ delayed message plugin for native delayed delivery
    /// - TTL and dead letter exchange combinations for delay simulation
    /// - External scheduling services for complex timing requirements
    /// 
    /// The exact delivery time is best-effort and may be slightly delayed due to
    /// broker load, network conditions, or implementation characteristics.
    /// </remarks>
    Task<PublishResult> ScheduleAsync<T>(
        T message,
        string exchangeName,
        string routingKey,
        DateTimeOffset deliveryTime,
        PublishOptions? options = null,
        CancellationToken cancellationToken = default) where T : class;

    /// <summary>
    /// Publishes a message with transaction semantics, ensuring atomic delivery.
    /// </summary>
    /// <typeparam name="T">The type of the message payload.</typeparam>
    /// <param name="message">The message payload to publish. Cannot be null.</param>
    /// <param name="exchangeName">The name of the exchange to publish to. Cannot be null or whitespace.</param>
    /// <param name="routingKey">The routing key for message routing. Cannot be null but can be empty.</param>
    /// <param name="transactionScope">The transaction scope for atomic publishing operations.</param>
    /// <param name="options">Optional publishing options for customizing message properties and behavior.</param>
    /// <param name="cancellationToken">Token to monitor for cancellation requests.</param>
    /// <returns>
    /// A task that represents the asynchronous transactional publishing operation.
    /// The task result contains confirmation details about the publish operation.
    /// </returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="message"/> or <paramref name="transactionScope"/> is null.</exception>
    /// <exception cref="ArgumentException">Thrown when <paramref name="exchangeName"/> is null, empty, or whitespace.</exception>
    /// <exception cref="PublishException">Thrown when the message cannot be published or transaction fails.</exception>
    /// <exception cref="SerializationException">Thrown when the message cannot be serialized.</exception>
    /// <remarks>
    /// Transactional publishing provides ACID properties for message delivery,
    /// ensuring that messages are either successfully delivered or completely rolled back.
    /// This is particularly important for:
    /// 
    /// - Financial transactions requiring strict consistency
    /// - Multi-step workflows where partial completion is problematic
    /// - Integration scenarios requiring atomic operations across multiple systems
    /// - Compliance requirements mandating transactional guarantees
    /// 
    /// Note: Transactions have significant performance overhead compared to regular
    /// publishing. Consider using publisher confirms for most scenarios unless
    /// strict transactional semantics are required.
    /// 
    /// The transaction scope manages the underlying RabbitMQ transaction boundaries
    /// and can span multiple publish operations for atomic multi-message delivery.
    /// </remarks>
    Task<PublishResult> PublishTransactionalAsync<T>(
        T message,
        string exchangeName,
        string routingKey,
        ITransactionScope transactionScope,
        PublishOptions? options = null,
        CancellationToken cancellationToken = default) where T : class;
}