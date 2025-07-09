namespace FS.RabbitMQ.Events;

/// <summary>
/// Base implementation for integration events
/// </summary>
public abstract class IntegrationEventBase : EventBase, IIntegrationEvent
{
    /// <summary>
    /// Source service or bounded context that published this event
    /// </summary>
    public string Source { get; private set; }
    
    /// <summary>
    /// Routing key pattern for message distribution
    /// </summary>
    public string RoutingKey { get; private set; }
    
    /// <summary>
    /// Target audience for this integration event (optional)
    /// </summary>
    public string? Target { get; set; }
    
    /// <summary>
    /// Event schema version for external compatibility
    /// </summary>
    public string SchemaVersion { get; private set; }
    
    /// <summary>
    /// Time to live for this event (optional)
    /// </summary>
    public TimeSpan? TimeToLive { get; set; }

    /// <summary>
    /// Constructor for integration events
    /// </summary>
    /// <param name="source">Source service or context</param>
    /// <param name="routingKey">Routing key for distribution</param>
    /// <param name="schemaVersion">Schema version</param>
    protected IntegrationEventBase(string source, string routingKey, string schemaVersion = "1.0") : base()
    {
        if (string.IsNullOrWhiteSpace(source))
            throw new ArgumentException("Source cannot be null or empty", nameof(source));
        if (string.IsNullOrWhiteSpace(routingKey))
            throw new ArgumentException("RoutingKey cannot be null or empty", nameof(routingKey));
        if (string.IsNullOrWhiteSpace(schemaVersion))
            throw new ArgumentException("SchemaVersion cannot be null or empty", nameof(schemaVersion));

        Source = source;
        RoutingKey = routingKey;
        SchemaVersion = schemaVersion;
        
        // Add integration-specific metadata
        WithMetadata("Integration", "true")
            .WithMetadata("Source", source)
            .WithMetadata("RoutingKey", routingKey)
            .WithMetadata("SchemaVersion", schemaVersion);
    }

    /// <summary>
    /// Constructor for event reconstruction
    /// </summary>
    /// <param name="eventId">Event identifier</param>
    /// <param name="occurredOn">When the event occurred</param>
    /// <param name="source">Source service or context</param>
    /// <param name="routingKey">Routing key for distribution</param>
    /// <param name="schemaVersion">Schema version</param>
    protected IntegrationEventBase(Guid eventId, DateTime occurredOn, string source, string routingKey, string schemaVersion) 
        : base(eventId, occurredOn)
    {
        Source = source;
        RoutingKey = routingKey;
        SchemaVersion = schemaVersion;
        
        // Add integration-specific metadata
        WithMetadata("Integration", "true")
            .WithMetadata("Source", source)
            .WithMetadata("RoutingKey", routingKey)
            .WithMetadata("SchemaVersion", schemaVersion);
    }

    /// <summary>
    /// Sets the target audience for this integration event
    /// </summary>
    /// <param name="target">Target service or context</param>
    /// <returns>The event instance for fluent configuration</returns>
    public IntegrationEventBase WithTarget(string target)
    {
        Target = target;
        WithMetadata("Target", target);
        return this;
    }

    /// <summary>
    /// Sets the time to live for this integration event
    /// </summary>
    /// <param name="timeToLive">Time to live duration</param>
    /// <returns>The event instance for fluent configuration</returns>
    public IntegrationEventBase WithTimeToLive(TimeSpan timeToLive)
    {
        if (timeToLive <= TimeSpan.Zero)
            throw new ArgumentException("TimeToLive must be greater than zero", nameof(timeToLive));
            
        TimeToLive = timeToLive;
        WithMetadata("TimeToLive", timeToLive.ToString());
        return this;
    }

    /// <summary>
    /// Sets a custom routing key pattern
    /// </summary>
    /// <param name="routingKey">New routing key</param>
    /// <returns>The event instance for fluent configuration</returns>
    public IntegrationEventBase WithRoutingKey(string routingKey)
    {
        if (string.IsNullOrWhiteSpace(routingKey))
            throw new ArgumentException("RoutingKey cannot be null or empty", nameof(routingKey));
            
        RoutingKey = routingKey;
        WithMetadata("RoutingKey", routingKey);
        return this;
    }

    /// <summary>
    /// Gets a string representation of the integration event
    /// </summary>
    /// <returns>Event description with integration information</returns>
    public override string ToString()
    {
        return $"{EventType} [Id={Id}, Source={Source}, RoutingKey={RoutingKey}, " +
               $"SchemaVersion={SchemaVersion}, OccurredOn={OccurredOn:yyyy-MM-dd HH:mm:ss} UTC]";
    }
}