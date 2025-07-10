using FS.Transport.AMQP.Events;

namespace FS.Transport.AMQP.Producer;

/// <summary>
/// Fluent API for MessageProducer providing a chainable interface for message publishing
/// </summary>
public class FluentProducerApi
{
    private readonly IMessageProducer _producer;
    private object? _message;
    private string? _exchange;
    private string? _routingKey;
    private readonly MessageContext _context;
    private readonly PublishOptions _options;

    internal FluentProducerApi(IMessageProducer producer)
    {
        _producer = producer ?? throw new ArgumentNullException(nameof(producer));
        _context = new MessageContext { Exchange = "", RoutingKey = "" };
        _options = new PublishOptions();
    }

    /// <summary>
    /// Sets the message to be published
    /// </summary>
    /// <typeparam name="T">Message type</typeparam>
    /// <param name="message">Message to publish</param>
    /// <returns>Fluent API instance</returns>
    public FluentProducerApi<T> Message<T>(T message) where T : class
    {
        return new FluentProducerApi<T>(_producer, message);
    }

    /// <summary>
    /// Creates a fluent API for publishing domain events
    /// </summary>
    /// <typeparam name="T">Domain event type</typeparam>
    /// <param name="domainEvent">Domain event to publish</param>
    /// <returns>Fluent domain event API</returns>
    public FluentDomainEventApi<T> DomainEvent<T>(T domainEvent) where T : class, IDomainEvent
    {
        return new FluentDomainEventApi<T>(_producer, domainEvent);
    }

    /// <summary>
    /// Creates a fluent API for publishing integration events
    /// </summary>
    /// <typeparam name="T">Integration event type</typeparam>
    /// <param name="integrationEvent">Integration event to publish</param>
    /// <returns>Fluent integration event API</returns>
    public FluentIntegrationEventApi<T> IntegrationEvent<T>(T integrationEvent) where T : class, IIntegrationEvent
    {
        return new FluentIntegrationEventApi<T>(_producer, integrationEvent);
    }

    /// <summary>
    /// Creates a fluent API for batch publishing
    /// </summary>
    /// <typeparam name="T">Message type</typeparam>
    /// <param name="messages">Messages to publish</param>
    /// <returns>Fluent batch API</returns>
    public FluentBatchApi<T> Batch<T>(IEnumerable<T> messages) where T : class
    {
        return new FluentBatchApi<T>(_producer, messages);
    }

    /// <summary>
    /// Creates a fluent API for scheduled publishing
    /// </summary>
    /// <typeparam name="T">Message type</typeparam>
    /// <param name="message">Message to schedule</param>
    /// <returns>Fluent schedule API</returns>
    public FluentScheduleApi<T> Schedule<T>(T message) where T : class
    {
        return new FluentScheduleApi<T>(_producer, message);
    }

    /// <summary>
    /// Creates a fluent API for transactional publishing
    /// </summary>
    /// <param name="transactionId">Transaction ID (optional)</param>
    /// <returns>Fluent transaction API</returns>
    public FluentTransactionApi Transaction(string? transactionId = null)
    {
        return new FluentTransactionApi(_producer, transactionId ?? Guid.NewGuid().ToString());
    }
}

/// <summary>
/// Typed fluent API for message publishing
/// </summary>
/// <typeparam name="T">Message type</typeparam>
public class FluentProducerApi<T> where T : class
{
    private readonly IMessageProducer _producer;
    private readonly T _message;
    private readonly MessageContext _context;

    internal FluentProducerApi(IMessageProducer producer, T message)
    {
        _producer = producer ?? throw new ArgumentNullException(nameof(producer));
        _message = message ?? throw new ArgumentNullException(nameof(message));
        _context = new MessageContext { Exchange = "", RoutingKey = "" };
    }

    /// <summary>
    /// Sets the target exchange
    /// </summary>
    /// <param name="exchange">Exchange name</param>
    /// <returns>Fluent API instance</returns>
    public FluentProducerApi<T> ToExchange(string exchange)
    {
        _context.Exchange = exchange ?? throw new ArgumentNullException(nameof(exchange));
        return this;
    }

    /// <summary>
    /// Sets the target queue (uses default exchange)
    /// </summary>
    /// <param name="queueName">Queue name</param>
    /// <returns>Fluent API instance</returns>
    public FluentProducerApi<T> ToQueue(string queueName)
    {
        _context.Exchange = "";
        _context.RoutingKey = queueName ?? throw new ArgumentNullException(nameof(queueName));
        return this;
    }

    /// <summary>
    /// Sets the routing key
    /// </summary>
    /// <param name="routingKey">Routing key</param>
    /// <returns>Fluent API instance</returns>
    public FluentProducerApi<T> WithRoutingKey(string routingKey)
    {
        _context.RoutingKey = routingKey ?? throw new ArgumentNullException(nameof(routingKey));
        return this;
    }

    /// <summary>
    /// Sets message priority
    /// </summary>
    /// <param name="priority">Priority (0-255)</param>
    /// <returns>Fluent API instance</returns>
    public FluentProducerApi<T> WithPriority(byte priority)
    {
        _context.Priority = priority;
        return this;
    }

    /// <summary>
    /// Sets high priority (255)
    /// </summary>
    /// <returns>Fluent API instance</returns>
    public FluentProducerApi<T> WithHighPriority()
    {
        _context.Priority = 255;
        return this;
    }

    /// <summary>
    /// Sets message time-to-live
    /// </summary>
    /// <param name="ttl">Time-to-live</param>
    /// <returns>Fluent API instance</returns>
    public FluentProducerApi<T> WithTtl(TimeSpan ttl)
    {
        _context.TimeToLive = ttl;
        return this;
    }

    /// <summary>
    /// Sets message expiration time
    /// </summary>
    /// <param name="expiresAt">Expiration time</param>
    /// <returns>Fluent API instance</returns>
    public FluentProducerApi<T> ExpiresAt(DateTimeOffset expiresAt)
    {
        _context.Expiration = expiresAt;
        return this;
    }

    /// <summary>
    /// Enables confirmation waiting
    /// </summary>
    /// <param name="timeout">Confirmation timeout (optional)</param>
    /// <returns>Fluent API instance</returns>
    public FluentProducerApi<T> WithConfirmation(TimeSpan? timeout = null)
    {
        _context.WaitForConfirmation = true;
        if (timeout.HasValue)
            _context.ConfirmationTimeout = timeout.Value;
        return this;
    }

    /// <summary>
    /// Sets message as mandatory
    /// </summary>
    /// <returns>Fluent API instance</returns>
    public FluentProducerApi<T> AsMandatory()
    {
        _context.Mandatory = true;
        return this;
    }

    /// <summary>
    /// Sets message as persistent
    /// </summary>
    /// <returns>Fluent API instance</returns>
    public FluentProducerApi<T> AsPersistent()
    {
        _context.DeliveryMode = 2;
        return this;
    }

    /// <summary>
    /// Sets message as non-persistent
    /// </summary>
    /// <returns>Fluent API instance</returns>
    public FluentProducerApi<T> AsTransient()
    {
        _context.DeliveryMode = 1;
        return this;
    }

    /// <summary>
    /// Sets correlation ID for request-response patterns
    /// </summary>
    /// <param name="correlationId">Correlation ID</param>
    /// <returns>Fluent API instance</returns>
    public FluentProducerApi<T> WithCorrelationId(string correlationId)
    {
        _context.CorrelationId = correlationId;
        return this;
    }

    /// <summary>
    /// Sets reply-to queue for request-response patterns
    /// </summary>
    /// <param name="replyQueue">Reply queue name</param>
    /// <returns>Fluent API instance</returns>
    public FluentProducerApi<T> ReplyTo(string replyQueue)
    {
        _context.ReplyTo = replyQueue;
        return this;
    }

    /// <summary>
    /// Adds a custom header
    /// </summary>
    /// <param name="key">Header key</param>
    /// <param name="value">Header value</param>
    /// <returns>Fluent API instance</returns>
    public FluentProducerApi<T> WithHeader(string key, object value)
    {
        _context.Headers ??= new Dictionary<string, object>();
        _context.Headers[key] = value;
        return this;
    }

    /// <summary>
    /// Adds multiple headers
    /// </summary>
    /// <param name="headers">Headers to add</param>
    /// <returns>Fluent API instance</returns>
    public FluentProducerApi<T> WithHeaders(IDictionary<string, object> headers)
    {
        _context.Headers ??= new Dictionary<string, object>();
        foreach (var header in headers)
        {
            _context.Headers[header.Key] = header.Value;
        }
        return this;
    }

    /// <summary>
    /// Sets content type
    /// </summary>
    /// <param name="contentType">Content type</param>
    /// <returns>Fluent API instance</returns>
    public FluentProducerApi<T> WithContentType(string contentType)
    {
        _context.ContentType = contentType;
        return this;
    }

    /// <summary>
    /// Sets message as JSON
    /// </summary>
    /// <returns>Fluent API instance</returns>
    public FluentProducerApi<T> AsJson()
    {
        _context.ContentType = "application/json";
        _context.ContentEncoding = "utf-8";
        return this;
    }

    /// <summary>
    /// Enables compression
    /// </summary>
    /// <returns>Fluent API instance</returns>
    public FluentProducerApi<T> WithCompression()
    {
        _context.Headers ??= new Dictionary<string, object>();
        _context.Headers["compression"] = "gzip";
        return this;
    }

    /// <summary>
    /// Enables deduplication
    /// </summary>
    /// <returns>Fluent API instance</returns>
    public FluentProducerApi<T> WithDeduplication()
    {
        _context.Headers ??= new Dictionary<string, object>();
        _context.Headers["deduplication"] = "enabled";
        return this;
    }

    /// <summary>
    /// Sets retry policy
    /// </summary>
    /// <param name="policyName">Retry policy name</param>
    /// <returns>Fluent API instance</returns>
    public FluentProducerApi<T> WithRetryPolicy(string policyName)
    {
        _context.RetryPolicyName = policyName;
        return this;
    }

    /// <summary>
    /// Sets maximum retry attempts
    /// </summary>
    /// <param name="maxRetries">Maximum retry attempts</param>
    /// <returns>Fluent API instance</returns>
    public FluentProducerApi<T> WithMaxRetries(int maxRetries)
    {
        _context.MaxRetries = maxRetries;
        return this;
    }

    /// <summary>
    /// Sets dead letter configuration
    /// </summary>
    /// <param name="exchange">Dead letter exchange</param>
    /// <param name="routingKey">Dead letter routing key</param>
    /// <returns>Fluent API instance</returns>
    public FluentProducerApi<T> WithDeadLetter(string exchange, string routingKey)
    {
        _context.DeadLetterExchange = exchange;
        _context.DeadLetterRoutingKey = routingKey;
        return this;
    }

    /// <summary>
    /// Conditionally applies configuration based on a condition
    /// </summary>
    /// <param name="condition">Condition to check</param>
    /// <param name="configure">Configuration action</param>
    /// <returns>Fluent API instance</returns>
    public FluentProducerApi<T> When(bool condition, Func<FluentProducerApi<T>, FluentProducerApi<T>> configure)
    {
        return condition ? configure(this) : this;
    }

    /// <summary>
    /// Conditionally applies configuration based on a condition
    /// </summary>
    /// <param name="condition">Condition function</param>
    /// <param name="configure">Configuration action</param>
    /// <returns>Fluent API instance</returns>
    public FluentProducerApi<T> When(Func<bool> condition, Func<FluentProducerApi<T>, FluentProducerApi<T>> configure)
    {
        return condition() ? configure(this) : this;
    }

    /// <summary>
    /// Publishes the message asynchronously
    /// </summary>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Publish result</returns>
    public Task<PublishResult> PublishAsync(CancellationToken cancellationToken = default)
    {
        ValidateConfiguration();
        return _producer.PublishAsync(_message, _context, cancellationToken);
    }

    /// <summary>
    /// Publishes the message synchronously
    /// </summary>
    /// <returns>Publish result</returns>
    public PublishResult Publish()
    {
        ValidateConfiguration();
        return _producer.Publish(_message, _context);
    }

    private void ValidateConfiguration()
    {
        if (string.IsNullOrEmpty(_context.Exchange) && string.IsNullOrEmpty(_context.RoutingKey))
        {
            throw new InvalidOperationException("Either exchange or routing key (for default exchange) must be specified");
        }
    }
}

/// <summary>
/// Extension methods to create fluent API
/// </summary>
public static class FluentProducerExtensions
{
    /// <summary>
    /// Creates a fluent API for the producer
    /// </summary>
    /// <param name="producer">Producer instance</param>
    /// <returns>Fluent API instance</returns>
    public static FluentProducerApi Fluent(this IMessageProducer producer)
    {
        return new FluentProducerApi(producer);
    }

    /// <summary>
    /// Creates a fluent API for publishing a specific message
    /// </summary>
    /// <typeparam name="T">Message type</typeparam>
    /// <param name="producer">Producer instance</param>
    /// <param name="message">Message to publish</param>
    /// <returns>Fluent API instance</returns>
    public static FluentProducerApi<T> Fluent<T>(this IMessageProducer producer, T message) where T : class
    {
        return new FluentProducerApi(producer).Message(message);
    }
} 