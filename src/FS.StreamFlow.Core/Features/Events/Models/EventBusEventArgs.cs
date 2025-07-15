using FS.StreamFlow.Core.Features.Events.Interfaces;

namespace FS.StreamFlow.Core.Features.Events.Models;

/// <summary>
/// Event arguments for when an event is published
/// </summary>
public class EventPublishedEventArgs : EventArgs
{
    /// <summary>
    /// The event that was published
    /// </summary>
    public IEvent Event { get; }
    
    /// <summary>
    /// Exchange name where the event was published
    /// </summary>
    public string ExchangeName { get; }
    
    /// <summary>
    /// Routing key used for publishing
    /// </summary>
    public string RoutingKey { get; }
    
    /// <summary>
    /// Timestamp when the event was published
    /// </summary>
    public DateTimeOffset PublishedAt { get; }
    
    /// <summary>
    /// Whether the publish was successful
    /// </summary>
    public bool Success { get; }
    
    /// <summary>
    /// Error message if publish failed
    /// </summary>
    public string? ErrorMessage { get; }

    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="event">The event that was published</param>
    /// <param name="exchangeName">The exchange where the event was published</param>
    /// <param name="routingKey">The routing key used</param>
    /// <param name="success">True if the publish was successful</param>
    /// <param name="errorMessage">The error message if the publish failed</param>
    /// <exception cref="ArgumentNullException">Thrown when event, exchangeName, or routingKey is null</exception>
    public EventPublishedEventArgs(IEvent @event, string exchangeName, string routingKey, bool success, string? errorMessage = null)
    {
        Event = @event ?? throw new ArgumentNullException(nameof(@event));
        ExchangeName = exchangeName ?? throw new ArgumentNullException(nameof(exchangeName));
        RoutingKey = routingKey ?? throw new ArgumentNullException(nameof(routingKey));
        PublishedAt = DateTimeOffset.UtcNow;
        Success = success;
        ErrorMessage = errorMessage;
    }
}

/// <summary>
/// Event arguments for when an event is received
/// </summary>
public class EventReceivedEventArgs : EventArgs
{
    /// <summary>
    /// The event that was received
    /// </summary>
    public IEvent Event { get; }
    
    /// <summary>
    /// Exchange name where the event came from
    /// </summary>
    public string ExchangeName { get; }
    
    /// <summary>
    /// Routing key of the received event
    /// </summary>
    public string RoutingKey { get; }
    
    /// <summary>
    /// Timestamp when the event was received
    /// </summary>
    public DateTimeOffset ReceivedAt { get; }
    
    /// <summary>
    /// Consumer tag that received the event
    /// </summary>
    public string? ConsumerTag { get; }

    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="event">The event that was received</param>
    /// <param name="exchangeName">The exchange where the event came from</param>
    /// <param name="routingKey">The routing key of the received event</param>
    /// <param name="consumerTag">The consumer tag that received the event</param>
    /// <exception cref="ArgumentNullException">Thrown when event, exchangeName, or routingKey is null</exception>
    public EventReceivedEventArgs(IEvent @event, string exchangeName, string routingKey, string? consumerTag = null)
    {
        Event = @event ?? throw new ArgumentNullException(nameof(@event));
        ExchangeName = exchangeName ?? throw new ArgumentNullException(nameof(exchangeName));
        RoutingKey = routingKey ?? throw new ArgumentNullException(nameof(routingKey));
        ReceivedAt = DateTimeOffset.UtcNow;
        ConsumerTag = consumerTag;
    }
}

/// <summary>
/// Event arguments for when event processing fails
/// </summary>
public class EventProcessingFailedEventArgs : EventArgs
{
    /// <summary>
    /// The event that failed processing
    /// </summary>
    public IEvent Event { get; }
    
    /// <summary>
    /// The exception that occurred during processing
    /// </summary>
    public Exception Exception { get; }
    
    /// <summary>
    /// Exchange name where the event came from
    /// </summary>
    public string ExchangeName { get; }
    
    /// <summary>
    /// Routing key of the failed event
    /// </summary>
    public string RoutingKey { get; }
    
    /// <summary>
    /// Timestamp when the failure occurred
    /// </summary>
    public DateTimeOffset FailedAt { get; }
    
    /// <summary>
    /// Number of retry attempts made
    /// </summary>
    public int AttemptCount { get; }
    
    /// <summary>
    /// Whether the event will be retried
    /// </summary>
    public bool WillRetry { get; }

    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="event">The event that failed processing</param>
    /// <param name="exception">The exception that occurred</param>
    /// <param name="exchangeName">The exchange where the event came from</param>
    /// <param name="routingKey">The routing key of the failed event</param>
    /// <param name="attemptCount">The number of retry attempts</param>
    /// <param name="willRetry">True if the event will be retried</param>
    /// <exception cref="ArgumentNullException">Thrown when event, exception, exchangeName, or routingKey is null</exception>
    public EventProcessingFailedEventArgs(IEvent @event, Exception exception, string exchangeName, string routingKey, int attemptCount, bool willRetry)
    {
        Event = @event ?? throw new ArgumentNullException(nameof(@event));
        Exception = exception ?? throw new ArgumentNullException(nameof(exception));
        ExchangeName = exchangeName ?? throw new ArgumentNullException(nameof(exchangeName));
        RoutingKey = routingKey ?? throw new ArgumentNullException(nameof(routingKey));
        FailedAt = DateTimeOffset.UtcNow;
        AttemptCount = attemptCount;
        WillRetry = willRetry;
    }
} 