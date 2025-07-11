namespace FS.RabbitMQ.Events;

/// <summary>
/// Base implementation for all events providing common functionality
/// </summary>
public abstract class EventBase : IEvent
{
    /// <summary>
    /// Unique identifier for the event
    /// </summary>
    public Guid Id { get; private set; }

    /// <summary>
    /// Timestamp when the event occurred (UTC)
    /// </summary>
    public DateTime OccurredOn { get; private set; }

    /// <summary>
    /// Event version for schema evolution
    /// </summary>
    public virtual int Version { get; protected set; } = 1;

    /// <summary>
    /// Event type name derived from the class name
    /// </summary>
    public virtual string EventType => GetType().Name;

    /// <summary>
    /// Correlation identifier for tracing related events
    /// </summary>
    public string? CorrelationId { get; set; }

    /// <summary>
    /// Causation identifier linking to the command or event that caused this event
    /// </summary>
    public string? CausationId { get; set; }

    /// <summary>
    /// Additional metadata associated with the event
    /// </summary>
    public IDictionary<string, object> Metadata { get; private set; }

    /// <summary>
    /// Protected constructor for derived classes
    /// </summary>
    protected EventBase()
    {
        Id = Guid.NewGuid();
        OccurredOn = DateTime.UtcNow;
        Metadata = new Dictionary<string, object>();

        // Add common metadata
        Metadata["EventTypeName"] = GetType().FullName ?? GetType().Name;
        Metadata["AssemblyName"] = GetType().Assembly.GetName().Name ?? "Unknown";
    }

    /// <summary>
    /// Constructor with specific event ID and timestamp (for reconstruction)
    /// </summary>
    /// <param name="eventId">Event identifier</param>
    /// <param name="occurredOn">When the event occurred</param>
    protected EventBase(Guid eventId, DateTime occurredOn)
    {
        Id = eventId;
        OccurredOn = occurredOn;
        Metadata = new Dictionary<string, object>();

        // Add common metadata
        Metadata["EventTypeName"] = GetType().FullName ?? GetType().Name;
        Metadata["AssemblyName"] = GetType().Assembly.GetName().Name ?? "Unknown";
    }

    /// <summary>
    /// Adds metadata to the event
    /// </summary>
    /// <param name="key">Metadata key</param>
    /// <param name="value">Metadata value</param>
    /// <returns>The event instance for fluent configuration</returns>
    public EventBase WithMetadata(string key, object value)
    {
        if (string.IsNullOrWhiteSpace(key))
            throw new ArgumentException("Metadata key cannot be null or empty", nameof(key));

        Metadata[key] = value;
        return this;
    }

    /// <summary>
    /// Sets the correlation ID for this event
    /// </summary>
    /// <param name="correlationId">Correlation identifier</param>
    /// <returns>The event instance for fluent configuration</returns>
    public EventBase WithCorrelationId(string correlationId)
    {
        CorrelationId = correlationId;
        return this;
    }

    /// <summary>
    /// Sets the causation ID for this event
    /// </summary>
    /// <param name="causationId">Causation identifier</param>
    /// <returns>The event instance for fluent configuration</returns>
    public EventBase WithCausationId(string causationId)
    {
        CausationId = causationId;
        return this;
    }

    /// <summary>
    /// Creates a copy of this event with updated metadata
    /// </summary>
    /// <returns>New event instance with copied data</returns>
    public virtual EventBase Clone()
    {
        var clone = (EventBase)MemberwiseClone();
        clone.Metadata = new Dictionary<string, object>(Metadata);
        return clone;
    }

    /// <summary>
    /// Gets a string representation of the event
    /// </summary>
    /// <returns>Event description</returns>
    public override string ToString()
    {
        return $"{EventType} [Id={Id}, OccurredOn={OccurredOn:yyyy-MM-dd HH:mm:ss} UTC, Version={Version}]";
    }

    /// <summary>
    /// Determines equality based on event ID
    /// </summary>
    /// <param name="obj">Object to compare</param>
    /// <returns>True if events have the same ID</returns>
    public override bool Equals(object? obj)
    {
        if (obj is not EventBase other)
            return false;

        return Id == other.Id;
    }

    /// <summary>
    /// Gets hash code based on event ID
    /// </summary>
    /// <returns>Hash code</returns>
    public override int GetHashCode()
    {
        return Id.GetHashCode();
    }
}