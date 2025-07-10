using FS.Transport.AMQP.Configuration;
using RabbitMQ.Client;

namespace FS.Transport.AMQP.Producer;

/// <summary>
/// Context information for message publishing including exchange, routing, and options
/// </summary>
public class MessageContext
{
    /// <summary>
    /// Exchange name where the message should be published
    /// </summary>
    public required string Exchange { get; set; }
    
    /// <summary>
    /// Routing key for message routing
    /// </summary>
    public required string RoutingKey { get; set; }
    
    /// <summary>
    /// Message properties (headers, TTL, priority, etc.)
    /// </summary>
    public IBasicProperties? Properties { get; set; }
    
    /// <summary>
    /// Custom headers to be added to the message
    /// </summary>
    public IDictionary<string, object>? Headers { get; set; }
    
    /// <summary>
    /// Message priority (0-255, higher values indicate higher priority)
    /// </summary>
    public byte? Priority { get; set; }
    
    /// <summary>
    /// Message time-to-live (TTL) in milliseconds
    /// </summary>
    public TimeSpan? TimeToLive { get; set; }
    
    /// <summary>
    /// Message expiration time (absolute time when message expires)
    /// </summary>
    public DateTimeOffset? Expiration { get; set; }
    
    /// <summary>
    /// Content type of the message (e.g., "application/json")
    /// </summary>
    public string? ContentType { get; set; }
    
    /// <summary>
    /// Content encoding of the message (e.g., "utf-8")
    /// </summary>
    public string? ContentEncoding { get; set; }
    
    /// <summary>
    /// Message correlation ID for request-response patterns
    /// </summary>
    public string? CorrelationId { get; set; }
    
    /// <summary>
    /// Reply-to queue for response messages
    /// </summary>
    public string? ReplyTo { get; set; }
    
    /// <summary>
    /// Message type identifier
    /// </summary>
    public string? MessageType { get; set; }
    
    /// <summary>
    /// Message delivery mode (1 = non-persistent, 2 = persistent)
    /// </summary>
    public byte? DeliveryMode { get; set; }
    
    /// <summary>
    /// Application-specific message identifier
    /// </summary>
    public string? MessageId { get; set; }
    
    /// <summary>
    /// User ID associated with the message
    /// </summary>
    public string? UserId { get; set; }
    
    /// <summary>
    /// Application ID that generated the message
    /// </summary>
    public string? AppId { get; set; }
    
    /// <summary>
    /// Timestamp when the message was created
    /// </summary>
    public DateTimeOffset? Timestamp { get; set; }
    
    /// <summary>
    /// Whether to wait for broker confirmation
    /// </summary>
    public bool WaitForConfirmation { get; set; } = true;
    
    /// <summary>
    /// Timeout for waiting broker confirmation
    /// </summary>
    public TimeSpan ConfirmationTimeout { get; set; } = TimeSpan.FromSeconds(30);
    
    /// <summary>
    /// Whether the message should be published as mandatory
    /// </summary>
    public bool Mandatory { get; set; } = false;
    
    /// <summary>
    /// Whether the message should be published as immediate
    /// </summary>
    public bool Immediate { get; set; } = false;
    
    /// <summary>
    /// Custom serializer to use for this message (overrides default)
    /// </summary>
    public string? SerializerType { get; set; }
    
    /// <summary>
    /// Retry policy name to use for this message
    /// </summary>
    public string? RetryPolicyName { get; set; }
    
    /// <summary>
    /// Custom retry count for this message
    /// </summary>
    public int? MaxRetries { get; set; }
    
    /// <summary>
    /// Dead letter exchange for failed messages
    /// </summary>
    public string? DeadLetterExchange { get; set; }
    
    /// <summary>
    /// Dead letter routing key for failed messages
    /// </summary>
    public string? DeadLetterRoutingKey { get; set; }
    
    /// <summary>
    /// Creates a new message context with basic information
    /// </summary>
    /// <param name="exchange">Exchange name</param>
    /// <param name="routingKey">Routing key</param>
    /// <returns>New message context</returns>
    public static MessageContext Create(string exchange, string routingKey)
    {
        return new MessageContext
        {
            Exchange = exchange,
            RoutingKey = routingKey,
            Timestamp = DateTimeOffset.UtcNow
        };
    }
    
    /// <summary>
    /// Creates a new message context with basic information and content type
    /// </summary>
    /// <param name="exchange">Exchange name</param>
    /// <param name="routingKey">Routing key</param>
    /// <param name="contentType">Content type</param>
    /// <returns>New message context</returns>
    public static MessageContext Create(string exchange, string routingKey, string contentType)
    {
        return new MessageContext
        {
            Exchange = exchange,
            RoutingKey = routingKey,
            ContentType = contentType,
            Timestamp = DateTimeOffset.UtcNow
        };
    }
    
    /// <summary>
    /// Creates a copy of this message context
    /// </summary>
    /// <returns>Copy of message context</returns>
    public MessageContext Clone()
    {
        return new MessageContext
        {
            Exchange = Exchange,
            RoutingKey = RoutingKey,
            Properties = Properties,
            Headers = Headers?.ToDictionary(kvp => kvp.Key, kvp => kvp.Value),
            Priority = Priority,
            TimeToLive = TimeToLive,
            Expiration = Expiration,
            ContentType = ContentType,
            ContentEncoding = ContentEncoding,
            CorrelationId = CorrelationId,
            ReplyTo = ReplyTo,
            MessageType = MessageType,
            DeliveryMode = DeliveryMode,
            MessageId = MessageId,
            UserId = UserId,
            AppId = AppId,
            Timestamp = Timestamp,
            WaitForConfirmation = WaitForConfirmation,
            Mandatory = Mandatory,
            Immediate = Immediate,
            SerializerType = SerializerType,
            RetryPolicyName = RetryPolicyName,
            MaxRetries = MaxRetries,
            DeadLetterExchange = DeadLetterExchange,
            DeadLetterRoutingKey = DeadLetterRoutingKey
        };
    }
}

/// <summary>
/// Event context information for event publishing
/// </summary>
public class EventContext : MessageContext
{
    /// <summary>
    /// Event type name
    /// </summary>
    public string? EventType { get; set; }
    
    /// <summary>
    /// Event version for backward compatibility
    /// </summary>
    public string? EventVersion { get; set; }
    
    /// <summary>
    /// Aggregate ID associated with the event
    /// </summary>
    public string? AggregateId { get; set; }
    
    /// <summary>
    /// Aggregate type associated with the event
    /// </summary>
    public string? AggregateType { get; set; }
    
    /// <summary>
    /// Event sequence number within the aggregate
    /// </summary>
    public long? EventSequence { get; set; }
    
    /// <summary>
    /// Causation ID for tracking event causality
    /// </summary>
    public string? CausationId { get; set; }
    
    /// <summary>
    /// Whether this is a domain event or integration event
    /// </summary>
    public bool IsDomainEvent { get; set; } = false;
    
    /// <summary>
    /// Creates a new event context for domain events
    /// </summary>
    /// <param name="aggregateId">Aggregate ID</param>
    /// <param name="aggregateType">Aggregate type</param>
    /// <param name="eventType">Event type</param>
    /// <returns>New event context</returns>
    public static EventContext CreateDomainEvent(string aggregateId, string aggregateType, string eventType)
    {
        return new EventContext
        {
            Exchange = "domain.events",
            RoutingKey = $"{aggregateType}.{eventType}",
            AggregateId = aggregateId,
            AggregateType = aggregateType,
            EventType = eventType,
            IsDomainEvent = true,
            Timestamp = DateTimeOffset.UtcNow
        };
    }
    
    /// <summary>
    /// Creates a new event context for integration events
    /// </summary>
    /// <param name="eventType">Event type</param>
    /// <param name="routingKey">Routing key</param>
    /// <returns>New event context</returns>
    public static EventContext CreateIntegrationEvent(string eventType, string routingKey)
    {
        return new EventContext
        {
            Exchange = "integration.events",
            RoutingKey = routingKey,
            EventType = eventType,
            IsDomainEvent = false,
            Timestamp = DateTimeOffset.UtcNow
        };
    }
}

/// <summary>
/// Message with its context for batch operations
/// </summary>
/// <typeparam name="T">Message type</typeparam>
public class MessageWithContext<T> where T : class
{
    /// <summary>
    /// The message to publish
    /// </summary>
    public required T Message { get; set; }
    
    /// <summary>
    /// Context information for the message
    /// </summary>
    public required MessageContext Context { get; set; }
    
    /// <summary>
    /// Creates a new message with context
    /// </summary>
    /// <param name="message">Message to publish</param>
    /// <param name="context">Message context</param>
    /// <returns>New message with context</returns>
    public static MessageWithContext<T> Create(T message, MessageContext context)
    {
        return new MessageWithContext<T>
        {
            Message = message,
            Context = context
        };
    }
    
    /// <summary>
    /// Creates a new message with context using exchange and routing key
    /// </summary>
    /// <param name="message">Message to publish</param>
    /// <param name="exchange">Exchange name</param>
    /// <param name="routingKey">Routing key</param>
    /// <returns>New message with context</returns>
    public static MessageWithContext<T> Create(T message, string exchange, string routingKey)
    {
        return new MessageWithContext<T>
        {
            Message = message,
            Context = MessageContext.Create(exchange, routingKey)
        };
    }
}