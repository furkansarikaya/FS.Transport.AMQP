using FS.Mediator.Features.RequestHandling.Core;
using FS.RabbitMQ.EventHandlers;
using FS.RabbitMQ.Events;
using FS.RabbitMQ.Producer;

namespace FS.RabbitMQ.Consumer;

/// <summary>
/// Command to start a consumer
/// </summary>
public class StartConsumerCommand : IRequest<ConsumerStatusResult>
{
    /// <summary>
    /// Consumer settings to use
    /// </summary>
    public ConsumerSettings? Settings { get; set; }
    
    /// <summary>
    /// Queue name to consume from
    /// </summary>
    public string QueueName { get; set; } = string.Empty;
    
    /// <summary>
    /// Consumer tag
    /// </summary>
    public string? ConsumerTag { get; set; }
    
    /// <summary>
    /// Correlation ID for tracing
    /// </summary>
    public string? CorrelationId { get; set; }
    
    /// <summary>
    /// Timeout for start operation
    /// </summary>
    public TimeSpan? Timeout { get; set; }
    
    /// <summary>
    /// Creates a start consumer command
    /// </summary>
    /// <param name="queueName">Queue name</param>
    /// <param name="settings">Consumer settings</param>
    /// <param name="consumerTag">Consumer tag</param>
    /// <param name="correlationId">Correlation ID</param>
    /// <param name="timeout">Timeout</param>
    /// <returns>Start consumer command</returns>
    public static StartConsumerCommand Create(string queueName, ConsumerSettings? settings = null, 
        string? consumerTag = null, string? correlationId = null, TimeSpan? timeout = null)
    {
        return new StartConsumerCommand
        {
            QueueName = queueName,
            Settings = settings,
            ConsumerTag = consumerTag,
            CorrelationId = correlationId,
            Timeout = timeout
        };
    }
}

/// <summary>
/// Command to stop a consumer
/// </summary>
public class StopConsumerCommand : IRequest<ConsumerStatusResult>
{
    /// <summary>
    /// Consumer tag to stop
    /// </summary>
    public string? ConsumerTag { get; set; }
    
    /// <summary>
    /// Whether to gracefully stop
    /// </summary>
    public bool Graceful { get; set; } = true;
    
    /// <summary>
    /// Correlation ID for tracing
    /// </summary>
    public string? CorrelationId { get; set; }
    
    /// <summary>
    /// Timeout for stop operation
    /// </summary>
    public TimeSpan? Timeout { get; set; }
    
    /// <summary>
    /// Creates a stop consumer command
    /// </summary>
    /// <param name="consumerTag">Consumer tag</param>
    /// <param name="graceful">Whether to gracefully stop</param>
    /// <param name="correlationId">Correlation ID</param>
    /// <param name="timeout">Timeout</param>
    /// <returns>Stop consumer command</returns>
    public static StopConsumerCommand Create(string? consumerTag = null, bool graceful = true, 
        string? correlationId = null, TimeSpan? timeout = null)
    {
        return new StopConsumerCommand
        {
            ConsumerTag = consumerTag,
            Graceful = graceful,
            CorrelationId = correlationId,
            Timeout = timeout
        };
    }
}

/// <summary>
/// Command to acknowledge a message
/// </summary>
public class AcknowledgeMessageCommand : IRequest<AcknowledgeResult>
{
    /// <summary>
    /// Message delivery tag
    /// </summary>
    public ulong DeliveryTag { get; set; }
    
    /// <summary>
    /// Whether to acknowledge multiple messages
    /// </summary>
    public bool Multiple { get; set; }
    
    /// <summary>
    /// Consumer tag
    /// </summary>
    public string? ConsumerTag { get; set; }
    
    /// <summary>
    /// Correlation ID for tracing
    /// </summary>
    public string? CorrelationId { get; set; }
    
    /// <summary>
    /// Creates an acknowledge message command
    /// </summary>
    /// <param name="deliveryTag">Delivery tag</param>
    /// <param name="multiple">Acknowledge multiple</param>
    /// <param name="consumerTag">Consumer tag</param>
    /// <param name="correlationId">Correlation ID</param>
    /// <returns>Acknowledge message command</returns>
    public static AcknowledgeMessageCommand Create(ulong deliveryTag, bool multiple = false, 
        string? consumerTag = null, string? correlationId = null)
    {
        return new AcknowledgeMessageCommand
        {
            DeliveryTag = deliveryTag,
            Multiple = multiple,
            ConsumerTag = consumerTag,
            CorrelationId = correlationId
        };
    }
}

/// <summary>
/// Command to reject a message
/// </summary>
public class RejectMessageCommand : IRequest<RejectResult>
{
    /// <summary>
    /// Message delivery tag
    /// </summary>
    public ulong DeliveryTag { get; set; }
    
    /// <summary>
    /// Whether to requeue the message
    /// </summary>
    public bool Requeue { get; set; }
    
    /// <summary>
    /// Consumer tag
    /// </summary>
    public string? ConsumerTag { get; set; }
    
    /// <summary>
    /// Rejection reason
    /// </summary>
    public string? Reason { get; set; }
    
    /// <summary>
    /// Correlation ID for tracing
    /// </summary>
    public string? CorrelationId { get; set; }
    
    /// <summary>
    /// Creates a reject message command
    /// </summary>
    /// <param name="deliveryTag">Delivery tag</param>
    /// <param name="requeue">Whether to requeue</param>
    /// <param name="reason">Rejection reason</param>
    /// <param name="consumerTag">Consumer tag</param>
    /// <param name="correlationId">Correlation ID</param>
    /// <returns>Reject message command</returns>
    public static RejectMessageCommand Create(ulong deliveryTag, bool requeue = false, string? reason = null, 
        string? consumerTag = null, string? correlationId = null)
    {
        return new RejectMessageCommand
        {
            DeliveryTag = deliveryTag,
            Requeue = requeue,
            Reason = reason,
            ConsumerTag = consumerTag,
            CorrelationId = correlationId
        };
    }
}

/// <summary>
/// Command to pause a consumer
/// </summary>
public class PauseConsumerCommand : IRequest<ConsumerStatusResult>
{
    /// <summary>
    /// Consumer tag to pause
    /// </summary>
    public string? ConsumerTag { get; set; }
    
    /// <summary>
    /// Pause reason
    /// </summary>
    public string? Reason { get; set; }
    
    /// <summary>
    /// Correlation ID for tracing
    /// </summary>
    public string? CorrelationId { get; set; }
    
    /// <summary>
    /// Creates a pause consumer command
    /// </summary>
    /// <param name="consumerTag">Consumer tag</param>
    /// <param name="reason">Pause reason</param>
    /// <param name="correlationId">Correlation ID</param>
    /// <returns>Pause consumer command</returns>
    public static PauseConsumerCommand Create(string? consumerTag = null, string? reason = null, 
        string? correlationId = null)
    {
        return new PauseConsumerCommand
        {
            ConsumerTag = consumerTag,
            Reason = reason,
            CorrelationId = correlationId
        };
    }
}

/// <summary>
/// Command to resume a consumer
/// </summary>
public class ResumeConsumerCommand : IRequest<ConsumerStatusResult>
{
    /// <summary>
    /// Consumer tag to resume
    /// </summary>
    public string? ConsumerTag { get; set; }
    
    /// <summary>
    /// Resume reason
    /// </summary>
    public string? Reason { get; set; }
    
    /// <summary>
    /// Correlation ID for tracing
    /// </summary>
    public string? CorrelationId { get; set; }
    
    /// <summary>
    /// Creates a resume consumer command
    /// </summary>
    /// <param name="consumerTag">Consumer tag</param>
    /// <param name="reason">Resume reason</param>
    /// <param name="correlationId">Correlation ID</param>
    /// <returns>Resume consumer command</returns>
    public static ResumeConsumerCommand Create(string? consumerTag = null, string? reason = null, 
        string? correlationId = null)
    {
        return new ResumeConsumerCommand
        {
            ConsumerTag = consumerTag,
            Reason = reason,
            CorrelationId = correlationId
        };
    }
}

/// <summary>
/// Command to consume a message from a queue
/// </summary>
/// <typeparam name="T">Message type</typeparam>
public class ConsumeMessageCommand<T> : IRequest<ConsumeResult<T>> where T : class
{
    /// <summary>
    /// Queue name to consume from
    /// </summary>
    public string QueueName { get; set; } = string.Empty;
    
    /// <summary>
    /// Message handler function
    /// </summary>
    public Func<T, MessageContext, Task<bool>>? Handler { get; set; }
    
    /// <summary>
    /// Consumer context
    /// </summary>
    public ConsumerContext? Context { get; set; }
    
    /// <summary>
    /// Correlation ID for tracing
    /// </summary>
    public string? CorrelationId { get; set; }
    
    /// <summary>
    /// Cancellation token
    /// </summary>
    public CancellationToken CancellationToken { get; set; } = default;
    
    /// <summary>
    /// Creates a consume message command
    /// </summary>
    /// <param name="queueName">Queue name</param>
    /// <param name="handler">Message handler</param>
    /// <param name="context">Consumer context</param>
    /// <param name="correlationId">Correlation ID</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Consume message command</returns>
    public static ConsumeMessageCommand<T> Create(string queueName, 
        Func<T, MessageContext, Task<bool>> handler, ConsumerContext? context = null, 
        string? correlationId = null, CancellationToken cancellationToken = default)
    {
        return new ConsumeMessageCommand<T>
        {
            QueueName = queueName,
            Handler = handler,
            Context = context,
            CorrelationId = correlationId,
            CancellationToken = cancellationToken
        };
    }
}

/// <summary>
/// Command to consume events from a queue
/// </summary>
/// <typeparam name="T">Event type</typeparam>
public class ConsumeEventCommand<T> : IRequest<ConsumeResult<T>> where T : class, IEvent
{
    /// <summary>
    /// Exchange name
    /// </summary>
    public string Exchange { get; set; } = string.Empty;
    
    /// <summary>
    /// Routing key pattern
    /// </summary>
    public string RoutingKey { get; set; } = string.Empty;
    
    /// <summary>
    /// Event handler
    /// </summary>
    public IAsyncEventHandler<T>? EventHandler { get; set; }
    
    /// <summary>
    /// Event handler function
    /// </summary>
    public Func<T, EventContext, Task<bool>>? Handler { get; set; }
    
    /// <summary>
    /// Correlation ID for tracing
    /// </summary>
    public string? CorrelationId { get; set; }
    
    /// <summary>
    /// Cancellation token
    /// </summary>
    public CancellationToken CancellationToken { get; set; } = default;
    
    /// <summary>
    /// Creates a consume event command with event handler
    /// </summary>
    /// <param name="exchange">Exchange name</param>
    /// <param name="routingKey">Routing key</param>
    /// <param name="eventHandler">Event handler</param>
    /// <param name="correlationId">Correlation ID</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Consume event command</returns>
    public static ConsumeEventCommand<T> Create(string exchange, string routingKey, 
        IAsyncEventHandler<T> eventHandler, string? correlationId = null, 
        CancellationToken cancellationToken = default)
    {
        return new ConsumeEventCommand<T>
        {
            Exchange = exchange,
            RoutingKey = routingKey,
            EventHandler = eventHandler,
            CorrelationId = correlationId,
            CancellationToken = cancellationToken
        };
    }
    
    /// <summary>
    /// Creates a consume event command with handler function
    /// </summary>
    /// <param name="exchange">Exchange name</param>
    /// <param name="routingKey">Routing key</param>
    /// <param name="handler">Handler function</param>
    /// <param name="correlationId">Correlation ID</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Consume event command</returns>
    public static ConsumeEventCommand<T> CreateWithHandler(string exchange, string routingKey, 
        Func<T, EventContext, Task<bool>> handler, string? correlationId = null, 
        CancellationToken cancellationToken = default)
    {
        return new ConsumeEventCommand<T>
        {
            Exchange = exchange,
            RoutingKey = routingKey,
            Handler = handler,
            CorrelationId = correlationId,
            CancellationToken = cancellationToken
        };
    }
}

/// <summary>
/// Command to consume domain events
/// </summary>
/// <typeparam name="T">Domain event type</typeparam>
public class ConsumeDomainEventCommand<T> : IRequest<ConsumeResult<T>> where T : class, IDomainEvent
{
    /// <summary>
    /// Routing key pattern
    /// </summary>
    public string RoutingKey { get; set; } = string.Empty;
    
    /// <summary>
    /// Event handler function
    /// </summary>
    public Func<T, EventContext, Task<bool>>? Handler { get; set; }
    
    /// <summary>
    /// Correlation ID for tracing
    /// </summary>
    public string? CorrelationId { get; set; }
    
    /// <summary>
    /// Cancellation token
    /// </summary>
    public CancellationToken CancellationToken { get; set; } = default;
    
    /// <summary>
    /// Creates a consume domain event command
    /// </summary>
    /// <param name="routingKey">Routing key</param>
    /// <param name="handler">Event handler</param>
    /// <param name="correlationId">Correlation ID</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Consume domain event command</returns>
    public static ConsumeDomainEventCommand<T> Create(string routingKey, 
        Func<T, EventContext, Task<bool>> handler, string? correlationId = null, 
        CancellationToken cancellationToken = default)
    {
        return new ConsumeDomainEventCommand<T>
        {
            RoutingKey = routingKey,
            Handler = handler,
            CorrelationId = correlationId,
            CancellationToken = cancellationToken
        };
    }
}

/// <summary>
/// Command to consume integration events
/// </summary>
/// <typeparam name="T">Integration event type</typeparam>
public class ConsumeIntegrationEventCommand<T> : IRequest<ConsumeResult<T>> where T : class, IIntegrationEvent
{
    /// <summary>
    /// Routing key pattern
    /// </summary>
    public string RoutingKey { get; set; } = string.Empty;
    
    /// <summary>
    /// Event handler function
    /// </summary>
    public Func<T, EventContext, Task<bool>>? Handler { get; set; }
    
    /// <summary>
    /// Correlation ID for tracing
    /// </summary>
    public string? CorrelationId { get; set; }
    
    /// <summary>
    /// Cancellation token
    /// </summary>
    public CancellationToken CancellationToken { get; set; } = default;
    
    /// <summary>
    /// Creates a consume integration event command
    /// </summary>
    /// <param name="routingKey">Routing key</param>
    /// <param name="handler">Event handler</param>
    /// <param name="correlationId">Correlation ID</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Consume integration event command</returns>
    public static ConsumeIntegrationEventCommand<T> Create(string routingKey, 
        Func<T, EventContext, Task<bool>> handler, string? correlationId = null, 
        CancellationToken cancellationToken = default)
    {
        return new ConsumeIntegrationEventCommand<T>
        {
            RoutingKey = routingKey,
            Handler = handler,
            CorrelationId = correlationId,
            CancellationToken = cancellationToken
        };
    }
}

/// <summary>
/// Command to requeue a message
/// </summary>
public class RequeueMessageCommand : IRequest<RequeueResult>
{
    /// <summary>
    /// Message delivery tag
    /// </summary>
    public ulong DeliveryTag { get; set; }
    
    /// <summary>
    /// Requeue priority
    /// </summary>
    public byte? Priority { get; set; }
    
    /// <summary>
    /// Delay before requeue
    /// </summary>
    public TimeSpan? Delay { get; set; }
    
    /// <summary>
    /// Consumer tag
    /// </summary>
    public string? ConsumerTag { get; set; }
    
    /// <summary>
    /// Correlation ID for tracing
    /// </summary>
    public string? CorrelationId { get; set; }
    
    /// <summary>
    /// Creates a requeue message command
    /// </summary>
    /// <param name="deliveryTag">Delivery tag</param>
    /// <param name="priority">Requeue priority</param>
    /// <param name="delay">Requeue delay</param>
    /// <param name="consumerTag">Consumer tag</param>
    /// <param name="correlationId">Correlation ID</param>
    /// <returns>Requeue message command</returns>
    public static RequeueMessageCommand Create(ulong deliveryTag, byte? priority = null, 
        TimeSpan? delay = null, string? consumerTag = null, string? correlationId = null)
    {
        return new RequeueMessageCommand
        {
            DeliveryTag = deliveryTag,
            Priority = priority,
            Delay = delay,
            ConsumerTag = consumerTag,
            CorrelationId = correlationId
        };
    }
}

/// <summary>
/// Command to update consumer settings
/// </summary>
public class UpdateConsumerSettingsCommand : IRequest<UpdateConsumerSettingsResult>
{
    /// <summary>
    /// Consumer tag
    /// </summary>
    public string? ConsumerTag { get; set; }
    
    /// <summary>
    /// New consumer settings
    /// </summary>
    public ConsumerSettings Settings { get; set; } = new();
    
    /// <summary>
    /// Correlation ID for tracing
    /// </summary>
    public string? CorrelationId { get; set; }
    
    /// <summary>
    /// Creates an update consumer settings command
    /// </summary>
    /// <param name="settings">New settings</param>
    /// <param name="consumerTag">Consumer tag</param>
    /// <param name="correlationId">Correlation ID</param>
    /// <returns>Update consumer settings command</returns>
    public static UpdateConsumerSettingsCommand Create(ConsumerSettings settings, 
        string? consumerTag = null, string? correlationId = null)
    {
        return new UpdateConsumerSettingsCommand
        {
            Settings = settings,
            ConsumerTag = consumerTag,
            CorrelationId = correlationId
        };
    }
}

// Results

/// <summary>
/// Result of consumer status command
/// </summary>
public class ConsumerStatusResult
{
    /// <summary>
    /// Whether the operation was successful
    /// </summary>
    public bool Success { get; set; }
    
    /// <summary>
    /// Current consumer status
    /// </summary>
    public ConsumerStatus Status { get; set; }
    
    /// <summary>
    /// Consumer tag
    /// </summary>
    public string? ConsumerTag { get; set; }
    
    /// <summary>
    /// Error message if not successful
    /// </summary>
    public string? ErrorMessage { get; set; }
    
    /// <summary>
    /// Exception if one occurred
    /// </summary>
    public Exception? Exception { get; set; }
    
    /// <summary>
    /// Operation timestamp
    /// </summary>
    public DateTimeOffset Timestamp { get; set; } = DateTimeOffset.UtcNow;
    
    /// <summary>
    /// Duration of the operation
    /// </summary>
    public TimeSpan Duration { get; set; }
    
    /// <summary>
    /// Creates a success result
    /// </summary>
    /// <param name="status">Consumer status</param>
    /// <param name="consumerTag">Consumer tag</param>
    /// <param name="duration">Operation duration</param>
    /// <returns>Success result</returns>
    public static ConsumerStatusResult CreateSuccess(ConsumerStatus status, string? consumerTag = null, 
        TimeSpan duration = default)
    {
        return new ConsumerStatusResult
        {
            Success = true,
            Status = status,
            ConsumerTag = consumerTag,
            Duration = duration
        };
    }
    
    /// <summary>
    /// Creates a failure result
    /// </summary>
    /// <param name="status">Consumer status</param>
    /// <param name="errorMessage">Error message</param>
    /// <param name="exception">Exception</param>
    /// <param name="consumerTag">Consumer tag</param>
    /// <returns>Failure result</returns>
    public static ConsumerStatusResult CreateFailure(ConsumerStatus status, string errorMessage, 
        Exception? exception = null, string? consumerTag = null)
    {
        return new ConsumerStatusResult
        {
            Success = false,
            Status = status,
            ErrorMessage = errorMessage,
            Exception = exception,
            ConsumerTag = consumerTag
        };
    }
}

/// <summary>
/// Result of acknowledge message command
/// </summary>
public class AcknowledgeResult
{
    /// <summary>
    /// Whether the acknowledgment was successful
    /// </summary>
    public bool Success { get; set; }
    
    /// <summary>
    /// Delivery tag that was acknowledged
    /// </summary>
    public ulong DeliveryTag { get; set; }
    
    /// <summary>
    /// Whether multiple messages were acknowledged
    /// </summary>
    public bool Multiple { get; set; }
    
    /// <summary>
    /// Error message if not successful
    /// </summary>
    public string? ErrorMessage { get; set; }
    
    /// <summary>
    /// Exception if one occurred
    /// </summary>
    public Exception? Exception { get; set; }
    
    /// <summary>
    /// Acknowledgment timestamp
    /// </summary>
    public DateTimeOffset Timestamp { get; set; } = DateTimeOffset.UtcNow;
    
    /// <summary>
    /// Creates a success result
    /// </summary>
    /// <param name="deliveryTag">Delivery tag</param>
    /// <param name="multiple">Multiple acknowledgment</param>
    /// <returns>Success result</returns>
    public static AcknowledgeResult CreateSuccess(ulong deliveryTag, bool multiple = false)
    {
        return new AcknowledgeResult
        {
            Success = true,
            DeliveryTag = deliveryTag,
            Multiple = multiple
        };
    }
    
    /// <summary>
    /// Creates a failure result
    /// </summary>
    /// <param name="deliveryTag">Delivery tag</param>
    /// <param name="errorMessage">Error message</param>
    /// <param name="exception">Exception</param>
    /// <returns>Failure result</returns>
    public static AcknowledgeResult CreateFailure(ulong deliveryTag, string errorMessage, Exception? exception = null)
    {
        return new AcknowledgeResult
        {
            Success = false,
            DeliveryTag = deliveryTag,
            ErrorMessage = errorMessage,
            Exception = exception
        };
    }
}

/// <summary>
/// Result of reject message command
/// </summary>
public class RejectResult
{
    /// <summary>
    /// Whether the rejection was successful
    /// </summary>
    public bool Success { get; set; }
    
    /// <summary>
    /// Delivery tag that was rejected
    /// </summary>
    public ulong DeliveryTag { get; set; }
    
    /// <summary>
    /// Whether the message was requeued
    /// </summary>
    public bool Requeued { get; set; }
    
    /// <summary>
    /// Rejection reason
    /// </summary>
    public string? Reason { get; set; }
    
    /// <summary>
    /// Error message if not successful
    /// </summary>
    public string? ErrorMessage { get; set; }
    
    /// <summary>
    /// Exception if one occurred
    /// </summary>
    public Exception? Exception { get; set; }
    
    /// <summary>
    /// Rejection timestamp
    /// </summary>
    public DateTimeOffset Timestamp { get; set; } = DateTimeOffset.UtcNow;
    
    /// <summary>
    /// Creates a success result
    /// </summary>
    /// <param name="deliveryTag">Delivery tag</param>
    /// <param name="requeued">Whether requeued</param>
    /// <param name="reason">Rejection reason</param>
    /// <returns>Success result</returns>
    public static RejectResult CreateSuccess(ulong deliveryTag, bool requeued = false, string? reason = null)
    {
        return new RejectResult
        {
            Success = true,
            DeliveryTag = deliveryTag,
            Requeued = requeued,
            Reason = reason
        };
    }
    
    /// <summary>
    /// Creates a failure result
    /// </summary>
    /// <param name="deliveryTag">Delivery tag</param>
    /// <param name="errorMessage">Error message</param>
    /// <param name="exception">Exception</param>
    /// <returns>Failure result</returns>
    public static RejectResult CreateFailure(ulong deliveryTag, string errorMessage, Exception? exception = null)
    {
        return new RejectResult
        {
            Success = false,
            DeliveryTag = deliveryTag,
            ErrorMessage = errorMessage,
            Exception = exception
        };
    }
}

/// <summary>
/// Result of consume message command
/// </summary>
/// <typeparam name="T">Message type</typeparam>
public class ConsumeResult<T> where T : class
{
    /// <summary>
    /// Whether the consume operation was successful
    /// </summary>
    public bool Success { get; set; }
    
    /// <summary>
    /// Consumed message
    /// </summary>
    public T? Message { get; set; }
    
    /// <summary>
    /// Message context
    /// </summary>
    public MessageContext? Context { get; set; }
    
    /// <summary>
    /// Consumer tag
    /// </summary>
    public string? ConsumerTag { get; set; }
    
    /// <summary>
    /// Error message if not successful
    /// </summary>
    public string? ErrorMessage { get; set; }
    
    /// <summary>
    /// Exception if one occurred
    /// </summary>
    public Exception? Exception { get; set; }
    
    /// <summary>
    /// Consume timestamp
    /// </summary>
    public DateTimeOffset Timestamp { get; set; } = DateTimeOffset.UtcNow;
    
    /// <summary>
    /// Creates a success result
    /// </summary>
    /// <param name="message">Consumed message</param>
    /// <param name="context">Message context</param>
    /// <param name="consumerTag">Consumer tag</param>
    /// <returns>Success result</returns>
    public static ConsumeResult<T> CreateSuccess(T message, MessageContext context, string? consumerTag = null)
    {
        return new ConsumeResult<T>
        {
            Success = true,
            Message = message,
            Context = context,
            ConsumerTag = consumerTag
        };
    }
    
    /// <summary>
    /// Creates a failure result
    /// </summary>
    /// <param name="errorMessage">Error message</param>
    /// <param name="exception">Exception</param>
    /// <param name="consumerTag">Consumer tag</param>
    /// <returns>Failure result</returns>
    public static ConsumeResult<T> CreateFailure(string errorMessage, Exception? exception = null, string? consumerTag = null)
    {
        return new ConsumeResult<T>
        {
            Success = false,
            ErrorMessage = errorMessage,
            Exception = exception,
            ConsumerTag = consumerTag
        };
    }
}

/// <summary>
/// Result of requeue message command
/// </summary>
public class RequeueResult
{
    /// <summary>
    /// Whether the requeue was successful
    /// </summary>
    public bool Success { get; set; }
    
    /// <summary>
    /// Delivery tag that was requeued
    /// </summary>
    public ulong DeliveryTag { get; set; }
    
    /// <summary>
    /// Error message if not successful
    /// </summary>
    public string? ErrorMessage { get; set; }
    
    /// <summary>
    /// Exception if one occurred
    /// </summary>
    public Exception? Exception { get; set; }
    
    /// <summary>
    /// Requeue timestamp
    /// </summary>
    public DateTimeOffset Timestamp { get; set; } = DateTimeOffset.UtcNow;
    
    /// <summary>
    /// Creates a success result
    /// </summary>
    /// <param name="deliveryTag">Delivery tag</param>
    /// <returns>Success result</returns>
    public static RequeueResult CreateSuccess(ulong deliveryTag)
    {
        return new RequeueResult
        {
            Success = true,
            DeliveryTag = deliveryTag
        };
    }
    
    /// <summary>
    /// Creates a failure result
    /// </summary>
    /// <param name="deliveryTag">Delivery tag</param>
    /// <param name="errorMessage">Error message</param>
    /// <param name="exception">Exception</param>
    /// <returns>Failure result</returns>
    public static RequeueResult CreateFailure(ulong deliveryTag, string errorMessage, Exception? exception = null)
    {
        return new RequeueResult
        {
            Success = false,
            DeliveryTag = deliveryTag,
            ErrorMessage = errorMessage,
            Exception = exception
        };
    }
}

/// <summary>
/// Result of update consumer settings command
/// </summary>
public class UpdateConsumerSettingsResult
{
    /// <summary>
    /// Whether the update was successful
    /// </summary>
    public bool Success { get; set; }
    
    /// <summary>
    /// Consumer tag
    /// </summary>
    public string? ConsumerTag { get; set; }
    
    /// <summary>
    /// Updated settings
    /// </summary>
    public ConsumerSettings? Settings { get; set; }
    
    /// <summary>
    /// Error message if not successful
    /// </summary>
    public string? ErrorMessage { get; set; }
    
    /// <summary>
    /// Exception if one occurred
    /// </summary>
    public Exception? Exception { get; set; }
    
    /// <summary>
    /// Update timestamp
    /// </summary>
    public DateTimeOffset Timestamp { get; set; } = DateTimeOffset.UtcNow;
    
    /// <summary>
    /// Creates a success result
    /// </summary>
    /// <param name="settings">Updated settings</param>
    /// <param name="consumerTag">Consumer tag</param>
    /// <returns>Success result</returns>
    public static UpdateConsumerSettingsResult CreateSuccess(ConsumerSettings settings, string? consumerTag = null)
    {
        return new UpdateConsumerSettingsResult
        {
            Success = true,
            Settings = settings,
            ConsumerTag = consumerTag
        };
    }
    
    /// <summary>
    /// Creates a failure result
    /// </summary>
    /// <param name="errorMessage">Error message</param>
    /// <param name="exception">Exception</param>
    /// <param name="consumerTag">Consumer tag</param>
    /// <returns>Failure result</returns>
    public static UpdateConsumerSettingsResult CreateFailure(string errorMessage, Exception? exception = null, string? consumerTag = null)
    {
        return new UpdateConsumerSettingsResult
        {
            Success = false,
            ErrorMessage = errorMessage,
            Exception = exception,
            ConsumerTag = consumerTag
        };
    }
} 