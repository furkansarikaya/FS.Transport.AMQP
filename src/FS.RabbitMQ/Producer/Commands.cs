using FS.Mediator.Features.RequestHandling.Core;
using FS.RabbitMQ.Events;

namespace FS.RabbitMQ.Producer;

/// <summary>
/// Command to publish a message to RabbitMQ
/// </summary>
/// <typeparam name="T">Message type</typeparam>
public class PublishMessageCommand<T> : IRequest<PublishResult> where T : class
{
    /// <summary>
    /// Message to publish
    /// </summary>
    public T Message { get; set; } = default!;
    
    /// <summary>
    /// Exchange name
    /// </summary>
    public string Exchange { get; set; } = string.Empty;
    
    /// <summary>
    /// Routing key
    /// </summary>
    public string RoutingKey { get; set; } = string.Empty;
    
    /// <summary>
    /// Publish options
    /// </summary>
    public PublishOptions? Options { get; set; }
    
    /// <summary>
    /// Message context
    /// </summary>
    public MessageContext? Context { get; set; }
    
    /// <summary>
    /// Correlation ID for tracing
    /// </summary>
    public string? CorrelationId { get; set; }
    
    /// <summary>
    /// Timeout for publish operation
    /// </summary>
    public TimeSpan? Timeout { get; set; }
    
    /// <summary>
    /// Creates a publish message command
    /// </summary>
    /// <param name="message">Message to publish</param>
    /// <param name="exchange">Exchange name</param>
    /// <param name="routingKey">Routing key</param>
    /// <param name="options">Publish options</param>
    /// <param name="correlationId">Correlation ID</param>
    /// <param name="timeout">Timeout</param>
    /// <returns>Publish message command</returns>
    public static PublishMessageCommand<T> Create(T message, string exchange, string routingKey, 
        PublishOptions? options = null, string? correlationId = null, TimeSpan? timeout = null)
    {
        return new PublishMessageCommand<T>
        {
            Message = message,
            Exchange = exchange,
            RoutingKey = routingKey,
            Options = options,
            CorrelationId = correlationId,
            Timeout = timeout
        };
    }
    
    /// <summary>
    /// Creates a publish message command with context
    /// </summary>
    /// <param name="message">Message to publish</param>
    /// <param name="context">Message context</param>
    /// <param name="correlationId">Correlation ID</param>
    /// <param name="timeout">Timeout</param>
    /// <returns>Publish message command</returns>
    public static PublishMessageCommand<T> CreateWithContext(T message, MessageContext context, 
        string? correlationId = null, TimeSpan? timeout = null)
    {
        return new PublishMessageCommand<T>
        {
            Message = message,
            Context = context,
            Exchange = context.Exchange,
            RoutingKey = context.RoutingKey,
            CorrelationId = correlationId,
            Timeout = timeout
        };
    }
}

/// <summary>
/// Command to publish a batch of messages to RabbitMQ
/// </summary>
/// <typeparam name="T">Message type</typeparam>
public class PublishBatchCommand<T> : IRequest<BatchPublishResult> where T : class
{
    /// <summary>
    /// Messages with their contexts
    /// </summary>
    public IEnumerable<MessageWithContext<T>> Messages { get; set; } = Enumerable.Empty<MessageWithContext<T>>();
    
    /// <summary>
    /// Batch options
    /// </summary>
    public BatchPublishOptions? Options { get; set; }
    
    /// <summary>
    /// Correlation ID for tracing
    /// </summary>
    public string? CorrelationId { get; set; }
    
    /// <summary>
    /// Timeout for batch operation
    /// </summary>
    public TimeSpan? Timeout { get; set; }
    
    /// <summary>
    /// Creates a publish batch command
    /// </summary>
    /// <param name="messages">Messages with contexts</param>
    /// <param name="options">Batch options</param>
    /// <param name="correlationId">Correlation ID</param>
    /// <param name="timeout">Timeout</param>
    /// <returns>Publish batch command</returns>
    public static PublishBatchCommand<T> Create(IEnumerable<MessageWithContext<T>> messages, 
        BatchPublishOptions? options = null, string? correlationId = null, TimeSpan? timeout = null)
    {
        return new PublishBatchCommand<T>
        {
            Messages = messages,
            Options = options,
            CorrelationId = correlationId,
            Timeout = timeout
        };
    }
    
    /// <summary>
    /// Creates a publish batch command with same exchange and routing key
    /// </summary>
    /// <param name="messages">Messages to publish</param>
    /// <param name="exchange">Exchange name</param>
    /// <param name="routingKey">Routing key</param>
    /// <param name="options">Batch options</param>
    /// <param name="correlationId">Correlation ID</param>
    /// <param name="timeout">Timeout</param>
    /// <returns>Publish batch command</returns>
    public static PublishBatchCommand<T> CreateUniform(IEnumerable<T> messages, string exchange, string routingKey, 
        BatchPublishOptions? options = null, string? correlationId = null, TimeSpan? timeout = null)
    {
        var messageContexts = messages.Select(msg => new MessageWithContext<T>
        {
            Message = msg,
            Context = new MessageContext { Exchange = exchange, RoutingKey = routingKey }
        });
        
        return new PublishBatchCommand<T>
        {
            Messages = messageContexts,
            Options = options,
            CorrelationId = correlationId,
            Timeout = timeout
        };
    }
}

/// <summary>
/// Command to publish a transactional message to RabbitMQ
/// </summary>
/// <typeparam name="T">Message type</typeparam>
public class PublishTransactionalCommand<T> : IRequest<TransactionResult> where T : class
{
    /// <summary>
    /// Message to publish
    /// </summary>
    public T Message { get; set; } = default!;
    
    /// <summary>
    /// Exchange name
    /// </summary>
    public string Exchange { get; set; } = string.Empty;
    
    /// <summary>
    /// Routing key
    /// </summary>
    public string RoutingKey { get; set; } = string.Empty;
    
    /// <summary>
    /// Transaction ID
    /// </summary>
    public string TransactionId { get; set; } = string.Empty;
    
    /// <summary>
    /// Publish options
    /// </summary>
    public PublishOptions? Options { get; set; }
    
    /// <summary>
    /// Correlation ID for tracing
    /// </summary>
    public string? CorrelationId { get; set; }
    
    /// <summary>
    /// Timeout for transaction
    /// </summary>
    public TimeSpan? Timeout { get; set; }
    
    /// <summary>
    /// Creates a publish transactional command
    /// </summary>
    /// <param name="message">Message to publish</param>
    /// <param name="exchange">Exchange name</param>
    /// <param name="routingKey">Routing key</param>
    /// <param name="transactionId">Transaction ID</param>
    /// <param name="options">Publish options</param>
    /// <param name="correlationId">Correlation ID</param>
    /// <param name="timeout">Timeout</param>
    /// <returns>Publish transactional command</returns>
    public static PublishTransactionalCommand<T> Create(T message, string exchange, string routingKey, 
        string transactionId, PublishOptions? options = null, string? correlationId = null, TimeSpan? timeout = null)
    {
        return new PublishTransactionalCommand<T>
        {
            Message = message,
            Exchange = exchange,
            RoutingKey = routingKey,
            TransactionId = transactionId,
            Options = options,
            CorrelationId = correlationId,
            Timeout = timeout
        };
    }
}

/// <summary>
/// Command to schedule a message for future publishing
/// </summary>
/// <typeparam name="T">Message type</typeparam>
public class ScheduleMessageCommand<T> : IRequest<ScheduleResult> where T : class
{
    /// <summary>
    /// Message to schedule
    /// </summary>
    public T Message { get; set; } = default!;
    
    /// <summary>
    /// Exchange name
    /// </summary>
    public string Exchange { get; set; } = string.Empty;
    
    /// <summary>
    /// Routing key
    /// </summary>
    public string RoutingKey { get; set; } = string.Empty;
    
    /// <summary>
    /// Scheduled time for publishing
    /// </summary>
    public DateTimeOffset ScheduledTime { get; set; }
    
    /// <summary>
    /// Publish options
    /// </summary>
    public PublishOptions? Options { get; set; }
    
    /// <summary>
    /// Correlation ID for tracing
    /// </summary>
    public string? CorrelationId { get; set; }
    
    /// <summary>
    /// Creates a schedule message command
    /// </summary>
    /// <param name="message">Message to schedule</param>
    /// <param name="exchange">Exchange name</param>
    /// <param name="routingKey">Routing key</param>
    /// <param name="scheduledTime">Scheduled time</param>
    /// <param name="options">Publish options</param>
    /// <param name="correlationId">Correlation ID</param>
    /// <returns>Schedule message command</returns>
    public static ScheduleMessageCommand<T> Create(T message, string exchange, string routingKey, 
        DateTimeOffset scheduledTime, PublishOptions? options = null, string? correlationId = null)
    {
        return new ScheduleMessageCommand<T>
        {
            Message = message,
            Exchange = exchange,
            RoutingKey = routingKey,
            ScheduledTime = scheduledTime,
            Options = options,
            CorrelationId = correlationId
        };
    }
}

/// <summary>
/// Command to start the producer
/// </summary>
public class StartProducerCommand : IRequest<ProducerStatusResult>
{
    /// <summary>
    /// Producer settings to use
    /// </summary>
    public ProducerSettings? Settings { get; set; }
    
    /// <summary>
    /// Correlation ID for tracing
    /// </summary>
    public string? CorrelationId { get; set; }
    
    /// <summary>
    /// Timeout for start operation
    /// </summary>
    public TimeSpan? Timeout { get; set; }
    
    /// <summary>
    /// Creates a start producer command
    /// </summary>
    /// <param name="settings">Producer settings</param>
    /// <param name="correlationId">Correlation ID</param>
    /// <param name="timeout">Timeout</param>
    /// <returns>Start producer command</returns>
    public static StartProducerCommand Create(ProducerSettings? settings = null, 
        string? correlationId = null, TimeSpan? timeout = null)
    {
        return new StartProducerCommand
        {
            Settings = settings,
            CorrelationId = correlationId,
            Timeout = timeout
        };
    }
}

/// <summary>
/// Command to stop the producer
/// </summary>
public class StopProducerCommand : IRequest<ProducerStatusResult>
{
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
    /// Creates a stop producer command
    /// </summary>
    /// <param name="graceful">Whether to gracefully stop</param>
    /// <param name="correlationId">Correlation ID</param>
    /// <param name="timeout">Timeout</param>
    /// <returns>Stop producer command</returns>
    public static StopProducerCommand Create(bool graceful = true, 
        string? correlationId = null, TimeSpan? timeout = null)
    {
        return new StopProducerCommand
        {
            Graceful = graceful,
            CorrelationId = correlationId,
            Timeout = timeout
        };
    }
}

/// <summary>
/// Command to cancel a scheduled message
/// </summary>
public class CancelScheduledMessageCommand : IRequest<CancelScheduledResult>
{
    /// <summary>
    /// Schedule ID to cancel
    /// </summary>
    public string ScheduleId { get; set; } = string.Empty;
    
    /// <summary>
    /// Correlation ID for tracing
    /// </summary>
    public string? CorrelationId { get; set; }
    
    /// <summary>
    /// Creates a cancel scheduled message command
    /// </summary>
    /// <param name="scheduleId">Schedule ID</param>
    /// <param name="correlationId">Correlation ID</param>
    /// <returns>Cancel scheduled message command</returns>
    public static CancelScheduledMessageCommand Create(string scheduleId, string? correlationId = null)
    {
        return new CancelScheduledMessageCommand
        {
            ScheduleId = scheduleId,
            CorrelationId = correlationId
        };
    }
}

/// <summary>
/// Command to flush all pending messages
/// </summary>
public class FlushMessagesCommand : IRequest<FlushResult>
{
    /// <summary>
    /// Correlation ID for tracing
    /// </summary>
    public string? CorrelationId { get; set; }
    
    /// <summary>
    /// Timeout for flush operation
    /// </summary>
    public TimeSpan? Timeout { get; set; }
    
    /// <summary>
    /// Creates a flush messages command
    /// </summary>
    /// <param name="correlationId">Correlation ID</param>
    /// <param name="timeout">Timeout</param>
    /// <returns>Flush messages command</returns>
    public static FlushMessagesCommand Create(string? correlationId = null, TimeSpan? timeout = null)
    {
        return new FlushMessagesCommand
        {
            CorrelationId = correlationId,
            Timeout = timeout
        };
    }
}

/// <summary>
/// Command to publish a domain event
/// </summary>
/// <typeparam name="T">Domain event type</typeparam>
public class PublishDomainEventCommand<T> : IRequest<PublishResult> where T : class, IDomainEvent
{
    /// <summary>
    /// Domain event to publish
    /// </summary>
    public T DomainEvent { get; set; } = default!;
    
    /// <summary>
    /// Publish options
    /// </summary>
    public PublishOptions? Options { get; set; }
    
    /// <summary>
    /// Correlation ID for tracing
    /// </summary>
    public string? CorrelationId { get; set; }
    
    /// <summary>
    /// Timeout for publish operation
    /// </summary>
    public TimeSpan? Timeout { get; set; }
    
    /// <summary>
    /// Creates a publish domain event command
    /// </summary>
    /// <param name="domainEvent">Domain event to publish</param>
    /// <param name="options">Publish options</param>
    /// <param name="correlationId">Correlation ID</param>
    /// <param name="timeout">Timeout</param>
    /// <returns>Publish domain event command</returns>
    public static PublishDomainEventCommand<T> Create(T domainEvent, PublishOptions? options = null, 
        string? correlationId = null, TimeSpan? timeout = null)
    {
        return new PublishDomainEventCommand<T>
        {
            DomainEvent = domainEvent,
            Options = options,
            CorrelationId = correlationId,
            Timeout = timeout
        };
    }
}

/// <summary>
/// Command to publish an integration event
/// </summary>
/// <typeparam name="T">Integration event type</typeparam>
public class PublishIntegrationEventCommand<T> : IRequest<PublishResult> where T : class, IIntegrationEvent
{
    /// <summary>
    /// Integration event to publish
    /// </summary>
    public T IntegrationEvent { get; set; } = default!;
    
    /// <summary>
    /// Publish options
    /// </summary>
    public PublishOptions? Options { get; set; }
    
    /// <summary>
    /// Correlation ID for tracing
    /// </summary>
    public string? CorrelationId { get; set; }
    
    /// <summary>
    /// Timeout for publish operation
    /// </summary>
    public TimeSpan? Timeout { get; set; }
    
    /// <summary>
    /// Creates a publish integration event command
    /// </summary>
    /// <param name="integrationEvent">Integration event to publish</param>
    /// <param name="options">Publish options</param>
    /// <param name="correlationId">Correlation ID</param>
    /// <param name="timeout">Timeout</param>
    /// <returns>Publish integration event command</returns>
    public static PublishIntegrationEventCommand<T> Create(T integrationEvent, PublishOptions? options = null, 
        string? correlationId = null, TimeSpan? timeout = null)
    {
        return new PublishIntegrationEventCommand<T>
        {
            IntegrationEvent = integrationEvent,
            Options = options,
            CorrelationId = correlationId,
            Timeout = timeout
        };
    }
}

// Results

/// <summary>
/// Result of producer status command
/// </summary>
public class ProducerStatusResult
{
    /// <summary>
    /// Whether the operation was successful
    /// </summary>
    public bool Success { get; set; }
    
    /// <summary>
    /// Current producer status
    /// </summary>
    public ProducerStatus Status { get; set; }
    
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
    /// <param name="status">Producer status</param>
    /// <param name="duration">Operation duration</param>
    /// <returns>Success result</returns>
    public static ProducerStatusResult CreateSuccess(ProducerStatus status, TimeSpan duration)
    {
        return new ProducerStatusResult
        {
            Success = true,
            Status = status,
            Duration = duration
        };
    }
    
    /// <summary>
    /// Creates a failure result
    /// </summary>
    /// <param name="status">Producer status</param>
    /// <param name="errorMessage">Error message</param>
    /// <param name="exception">Exception</param>
    /// <returns>Failure result</returns>
    public static ProducerStatusResult CreateFailure(ProducerStatus status, string errorMessage, Exception? exception = null)
    {
        return new ProducerStatusResult
        {
            Success = false,
            Status = status,
            ErrorMessage = errorMessage,
            Exception = exception
        };
    }
}

/// <summary>
/// Result of transaction command
/// </summary>
public class TransactionResult
{
    /// <summary>
    /// Whether the transaction was successful
    /// </summary>
    public bool Success { get; set; }
    
    /// <summary>
    /// Transaction ID
    /// </summary>
    public string TransactionId { get; set; } = string.Empty;
    
    /// <summary>
    /// Error message if not successful
    /// </summary>
    public string? ErrorMessage { get; set; }
    
    /// <summary>
    /// Exception if one occurred
    /// </summary>
    public Exception? Exception { get; set; }
    
    /// <summary>
    /// Transaction timestamp
    /// </summary>
    public DateTimeOffset Timestamp { get; set; } = DateTimeOffset.UtcNow;
    
    /// <summary>
    /// Transaction duration
    /// </summary>
    public TimeSpan Duration { get; set; }
    
    /// <summary>
    /// Creates a success result
    /// </summary>
    /// <param name="transactionId">Transaction ID</param>
    /// <param name="duration">Transaction duration</param>
    /// <returns>Success result</returns>
    public static TransactionResult CreateSuccess(string transactionId, TimeSpan duration)
    {
        return new TransactionResult
        {
            Success = true,
            TransactionId = transactionId,
            Duration = duration
        };
    }
    
    /// <summary>
    /// Creates a failure result
    /// </summary>
    /// <param name="transactionId">Transaction ID</param>
    /// <param name="errorMessage">Error message</param>
    /// <param name="exception">Exception</param>
    /// <returns>Failure result</returns>
    public static TransactionResult CreateFailure(string transactionId, string errorMessage, Exception? exception = null)
    {
        return new TransactionResult
        {
            Success = false,
            TransactionId = transactionId,
            ErrorMessage = errorMessage,
            Exception = exception
        };
    }
}

/// <summary>
/// Result of cancel scheduled message command
/// </summary>
public class CancelScheduledResult
{
    /// <summary>
    /// Whether the cancellation was successful
    /// </summary>
    public bool Success { get; set; }
    
    /// <summary>
    /// Schedule ID that was cancelled
    /// </summary>
    public string ScheduleId { get; set; } = string.Empty;
    
    /// <summary>
    /// Error message if not successful
    /// </summary>
    public string? ErrorMessage { get; set; }
    
    /// <summary>
    /// Exception if one occurred
    /// </summary>
    public Exception? Exception { get; set; }
    
    /// <summary>
    /// Cancellation timestamp
    /// </summary>
    public DateTimeOffset Timestamp { get; set; } = DateTimeOffset.UtcNow;
    
    /// <summary>
    /// Creates a success result
    /// </summary>
    /// <param name="scheduleId">Schedule ID</param>
    /// <returns>Success result</returns>
    public static CancelScheduledResult CreateSuccess(string scheduleId)
    {
        return new CancelScheduledResult
        {
            Success = true,
            ScheduleId = scheduleId
        };
    }
    
    /// <summary>
    /// Creates a failure result
    /// </summary>
    /// <param name="scheduleId">Schedule ID</param>
    /// <param name="errorMessage">Error message</param>
    /// <param name="exception">Exception</param>
    /// <returns>Failure result</returns>
    public static CancelScheduledResult CreateFailure(string scheduleId, string errorMessage, Exception? exception = null)
    {
        return new CancelScheduledResult
        {
            Success = false,
            ScheduleId = scheduleId,
            ErrorMessage = errorMessage,
            Exception = exception
        };
    }
}

/// <summary>
/// Result of flush messages command
/// </summary>
public class FlushResult
{
    /// <summary>
    /// Whether the flush was successful
    /// </summary>
    public bool Success { get; set; }
    
    /// <summary>
    /// Number of messages flushed
    /// </summary>
    public int MessagesFlushed { get; set; }
    
    /// <summary>
    /// Error message if not successful
    /// </summary>
    public string? ErrorMessage { get; set; }
    
    /// <summary>
    /// Exception if one occurred
    /// </summary>
    public Exception? Exception { get; set; }
    
    /// <summary>
    /// Flush timestamp
    /// </summary>
    public DateTimeOffset Timestamp { get; set; } = DateTimeOffset.UtcNow;
    
    /// <summary>
    /// Flush duration
    /// </summary>
    public TimeSpan Duration { get; set; }
    
    /// <summary>
    /// Creates a success result
    /// </summary>
    /// <param name="messagesFlushed">Number of messages flushed</param>
    /// <param name="duration">Flush duration</param>
    /// <returns>Success result</returns>
    public static FlushResult CreateSuccess(int messagesFlushed, TimeSpan duration)
    {
        return new FlushResult
        {
            Success = true,
            MessagesFlushed = messagesFlushed,
            Duration = duration
        };
    }
    
    /// <summary>
    /// Creates a failure result
    /// </summary>
    /// <param name="errorMessage">Error message</param>
    /// <param name="exception">Exception</param>
    /// <returns>Failure result</returns>
    public static FlushResult CreateFailure(string errorMessage, Exception? exception = null)
    {
        return new FlushResult
        {
            Success = false,
            ErrorMessage = errorMessage,
            Exception = exception
        };
    }
} 