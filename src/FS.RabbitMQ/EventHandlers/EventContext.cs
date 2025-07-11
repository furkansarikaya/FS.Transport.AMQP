using FS.RabbitMQ.Events;

namespace FS.RabbitMQ.EventHandlers;

/// <summary>
/// Context information for event handling operations
/// </summary>
public class EventContext
{
    /// <summary>
    /// The event being handled
    /// </summary>
    public IEvent Event { get; }
    
    /// <summary>
    /// Unique identifier for this handling operation
    /// </summary>
    public string HandlingId { get; }
    
    /// <summary>
    /// Timestamp when handling started
    /// </summary>
    public DateTime StartedAt { get; }
    
    /// <summary>
    /// Handler that is processing the event
    /// </summary>
    public string? HandlerName { get; set; }
    
    /// <summary>
    /// Number of retry attempts made
    /// </summary>
    public int AttemptCount { get; set; }
    
    /// <summary>
    /// Whether this is a retry attempt
    /// </summary>
    public bool IsRetry => AttemptCount > 1;
    
    /// <summary>
    /// Source of the event (queue, topic, etc.)
    /// </summary>
    public string? Source { get; set; }
    
    /// <summary>
    /// Routing information
    /// </summary>
    public string? RoutingKey { get; set; }
    
    /// <summary>
    /// Exchange name where event came from
    /// </summary>
    public string? Exchange { get; set; }
    
    /// <summary>
    /// Queue name where event came from
    /// </summary>
    public string? Queue { get; set; }
    
    /// <summary>
    /// Additional properties for handler context
    /// </summary>
    public IDictionary<string, object> Properties { get; }
    
    /// <summary>
    /// Cancellation token for the handling operation
    /// </summary>
    public CancellationToken CancellationToken { get; set; }

    public EventContext(IEvent @event, CancellationToken cancellationToken = default)
    {
        Event = @event ?? throw new ArgumentNullException(nameof(@event));
        HandlingId = Guid.NewGuid().ToString();
        StartedAt = DateTime.UtcNow;
        AttemptCount = 1;
        Properties = new Dictionary<string, object>();
        CancellationToken = cancellationToken;
    }

    /// <summary>
    /// Creates context from message delivery information
    /// </summary>
    /// <param name="event">Event instance</param>
    /// <param name="exchange">Exchange name</param>
    /// <param name="routingKey">Routing key</param>
    /// <param name="queue">Queue name</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Event context with message information</returns>
    public static EventContext FromMessage(IEvent @event, string? exchange, string? routingKey, string? queue, CancellationToken cancellationToken = default)
    {
        var context = new EventContext(@event, cancellationToken)
        {
            Exchange = exchange,
            RoutingKey = routingKey,
            Queue = queue,
            Source = "message"
        };
        return context;
    }

    /// <summary>
    /// Creates context for direct event handling
    /// </summary>
    /// <param name="event">Event instance</param>
    /// <param name="source">Source identifier</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Event context</returns>
    public static EventContext FromDirect(IEvent @event, string source, CancellationToken cancellationToken = default)
    {
        var context = new EventContext(@event, cancellationToken)
        {
            Source = source
        };
        return context;
    }

    /// <summary>
    /// Adds a property to the context
    /// </summary>
    /// <param name="key">Property key</param>
    /// <param name="value">Property value</param>
    /// <returns>Context for fluent configuration</returns>
    public EventContext WithProperty(string key, object value)
    {
        Properties[key] = value;
        return this;
    }

    /// <summary>
    /// Sets the handler name
    /// </summary>
    /// <param name="handlerName">Handler name</param>
    /// <returns>Context for fluent configuration</returns>
    public EventContext WithHandler(string handlerName)
    {
        HandlerName = handlerName;
        return this;
    }

    /// <summary>
    /// Increments the attempt count for retries
    /// </summary>
    /// <returns>Context for fluent configuration</returns>
    public EventContext IncrementAttempt()
    {
        AttemptCount++;
        return this;
    }

    /// <summary>
    /// Gets the elapsed time since handling started
    /// </summary>
    /// <returns>Elapsed time</returns>
    public TimeSpan GetElapsed()
    {
        return DateTime.UtcNow - StartedAt;
    }

    /// <summary>
    /// Gets a string representation of the context
    /// </summary>
    /// <returns>Context description</returns>
    public override string ToString()
    {
        var parts = new List<string>
        {
            $"Event: {Event.EventType}",
            $"HandlingId: {HandlingId}",
            $"Attempt: {AttemptCount}"
        };

        if (!string.IsNullOrEmpty(HandlerName))
            parts.Add($"Handler: {HandlerName}");
            
        if (!string.IsNullOrEmpty(Source))
            parts.Add($"Source: {Source}");

        return string.Join(", ", parts);
    }
}
