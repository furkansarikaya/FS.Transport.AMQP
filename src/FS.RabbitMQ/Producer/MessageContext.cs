using RabbitMQ.Client;

namespace FS.RabbitMQ.Producer;

/// <summary>
/// Context information for message publishing
/// </summary>
public class MessageContext
{
    /// <summary>
    /// Gets or sets the exchange to publish to
    /// </summary>
    public string Exchange { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the routing key
    /// </summary>
    public string RoutingKey { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the message body
    /// </summary>
    public ReadOnlyMemory<byte> Body { get; set; }

    /// <summary>
    /// Gets or sets the message properties
    /// </summary>
    public IReadOnlyBasicProperties? Properties { get; set; }

    /// <summary>
    /// Gets or sets whether the message is mandatory
    /// </summary>
    public bool Mandatory { get; set; }

    /// <summary>
    /// Gets or sets the message ID
    /// </summary>
    public string MessageId { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the correlation ID
    /// </summary>
    public string CorrelationId { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets when the message was created
    /// </summary>
    public DateTimeOffset Timestamp { get; set; } = DateTimeOffset.UtcNow;

    /// <summary>
    /// Gets or sets additional context data
    /// </summary>
    public Dictionary<string, object?> AdditionalData { get; set; } = new();

    /// <summary>
    /// Gets or sets the message headers
    /// </summary>
    public Dictionary<string, object>? Headers { get; set; }

    /// <summary>
    /// Gets or sets the message type
    /// </summary>
    public string? MessageType { get; set; }

    /// <summary>
    /// Gets or sets the message priority (0-255)
    /// </summary>
    public byte? Priority { get; set; }

    /// <summary>
    /// Gets or sets the time-to-live for the message in milliseconds
    /// </summary>
    public TimeSpan? TimeToLive { get; set; }

    /// <summary>
    /// Gets or sets the message expiration
    /// </summary>
    public string? Expiration { get; set; }

    /// <summary>
    /// Gets or sets whether to wait for publisher confirmation
    /// </summary>
    public bool WaitForConfirmation { get; set; }

    /// <summary>
    /// Gets or sets the confirmation timeout
    /// </summary>
    public TimeSpan? ConfirmationTimeout { get; set; }

    /// <summary>
    /// Gets or sets the delivery mode (persistent or transient)
    /// </summary>
    public DeliveryModes? DeliveryMode { get; set; }

    /// <summary>
    /// Gets or sets the reply-to queue
    /// </summary>
    public string? ReplyTo { get; set; }

    /// <summary>
    /// Gets or sets the content type
    /// </summary>
    public string? ContentType { get; set; }

    /// <summary>
    /// Gets or sets the content encoding
    /// </summary>
    public string? ContentEncoding { get; set; }

    /// <summary>
    /// Gets or sets the retry policy name
    /// </summary>
    public string? RetryPolicyName { get; set; }

    /// <summary>
    /// Gets or sets the maximum number of retries
    /// </summary>
    public int? MaxRetries { get; set; }

    /// <summary>
    /// Gets or sets the dead letter exchange
    /// </summary>
    public string? DeadLetterExchange { get; set; }

    /// <summary>
    /// Gets or sets the dead letter routing key
    /// </summary>
    public string? DeadLetterRoutingKey { get; set; }

    /// <summary>
    /// Creates a MessageContext from basic information
    /// </summary>
    /// <param name="exchange">Exchange name</param>
    /// <param name="routingKey">Routing key</param>
    /// <param name="body">Message body</param>
    /// <param name="properties">Message properties</param>
    /// <param name="mandatory">Whether message is mandatory</param>
    /// <returns>A new MessageContext instance</returns>
    public static MessageContext Create(
        string exchange,
        string routingKey,
        ReadOnlyMemory<byte> body,
        IBasicProperties? properties = null,
        bool mandatory = false)
    {
        return new MessageContext
        {
            Exchange = exchange,
            RoutingKey = routingKey,
            Body = body,
            Properties = properties,
            Mandatory = mandatory,
            MessageId = properties?.MessageId ?? Guid.NewGuid().ToString(),
            CorrelationId = properties?.CorrelationId ?? string.Empty
        };
    }

    /// <summary>
    /// Creates a MessageContext from basic information without body
    /// </summary>
    /// <param name="exchange">Exchange name</param>
    /// <param name="routingKey">Routing key</param>
    /// <param name="properties">Message properties</param>
    /// <param name="mandatory">Whether message is mandatory</param>
    /// <returns>A new MessageContext instance</returns>
    public static MessageContext Create(
        string exchange,
        string routingKey,
        IBasicProperties? properties = null,
        bool mandatory = false)
    {
        return new MessageContext
        {
            Exchange = exchange,
            RoutingKey = routingKey,
            Body = ReadOnlyMemory<byte>.Empty,
            Properties = properties,
            Mandatory = mandatory,
            MessageId = properties?.MessageId ?? Guid.NewGuid().ToString(),
            CorrelationId = properties?.CorrelationId ?? string.Empty
        };
    }
}

/// <summary>
/// Event context information for event publishing
/// </summary>
public class EventPublishContext : MessageContext
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
    /// Gets or sets the message priority (0-255)
    /// </summary>
    public new byte? Priority { get; set; }

    /// <summary>
    /// Gets or sets the time-to-live for the message
    /// </summary>
    public new TimeSpan? TimeToLive { get; set; }

    /// <summary>
    /// Gets or sets the delivery mode (persistent or transient)
    /// </summary>
    public new DeliveryModes? DeliveryMode { get; set; }

    /// <summary>
    /// Gets or sets the maximum number of retries
    /// </summary>
    public new int? MaxRetries { get; set; }

    /// <summary>
    /// Gets or sets whether to wait for publisher confirmation
    /// </summary>
    public new bool WaitForConfirmation { get; set; }

    /// <summary>
    /// Gets or sets the retry policy name
    /// </summary>
    public new string? RetryPolicyName { get; set; }
    
    /// <summary>
    /// Creates a new event context for domain events
    /// </summary>
    /// <param name="aggregateId">Aggregate ID</param>
    /// <param name="aggregateType">Aggregate type</param>
    /// <param name="eventType">Event type</param>
    /// <returns>New event context</returns>
    public static EventPublishContext CreateDomainEvent(string aggregateId, string aggregateType, string eventType)
    {
        return new EventPublishContext
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
    public static EventPublishContext CreateIntegrationEvent(string eventType, string routingKey)
    {
        return new EventPublishContext
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