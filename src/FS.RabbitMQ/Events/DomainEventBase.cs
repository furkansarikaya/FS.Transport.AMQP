namespace FS.RabbitMQ.Events;

/// <summary>
/// Base implementation for domain events
/// </summary>
public abstract class DomainEventBase : EventBase, IDomainEvent
{
    /// <summary>
    /// Identifier of the aggregate that raised the event
    /// </summary>
    public string AggregateId { get; private set; }
    
    /// <summary>
    /// Type name of the aggregate that raised the event
    /// </summary>
    public string AggregateType { get; private set; }
    
    /// <summary>
    /// Version of the aggregate when this event was raised
    /// </summary>
    public long AggregateVersion { get; private set; }
    
    /// <summary>
    /// User or system that initiated the action leading to this event
    /// </summary>
    public string? InitiatedBy { get; set; }

    /// <summary>
    /// Constructor for domain events
    /// </summary>
    /// <param name="aggregateId">Aggregate identifier</param>
    /// <param name="aggregateType">Aggregate type name</param>
    /// <param name="aggregateVersion">Aggregate version</param>
    protected DomainEventBase(string aggregateId, string aggregateType, long aggregateVersion = -1) : base()
    {
        if (string.IsNullOrWhiteSpace(aggregateId))
            throw new ArgumentException("AggregateId cannot be null or empty", nameof(aggregateId));
        if (string.IsNullOrWhiteSpace(aggregateType))
            throw new ArgumentException("AggregateType cannot be null or empty", nameof(aggregateType));

        AggregateId = aggregateId;
        AggregateType = aggregateType;
        AggregateVersion = aggregateVersion;
        
        // Add domain-specific metadata
        WithMetadata("Domain", "true")
            .WithMetadata("AggregateId", aggregateId)
            .WithMetadata("AggregateType", aggregateType)
            .WithMetadata("AggregateVersion", aggregateVersion);
    }

    /// <summary>
    /// Constructor for event reconstruction
    /// </summary>
    /// <param name="eventId">Event identifier</param>
    /// <param name="occurredOn">When the event occurred</param>
    /// <param name="aggregateId">Aggregate identifier</param>
    /// <param name="aggregateType">Aggregate type name</param>
    /// <param name="aggregateVersion">Aggregate version</param>
    protected DomainEventBase(Guid eventId, DateTime occurredOn, string aggregateId, string aggregateType, long aggregateVersion) 
        : base(eventId, occurredOn)
    {
        AggregateId = aggregateId;
        AggregateType = aggregateType;
        AggregateVersion = aggregateVersion;
        
        // Add domain-specific metadata
        WithMetadata("Domain", "true")
            .WithMetadata("AggregateId", aggregateId)
            .WithMetadata("AggregateType", aggregateType)
            .WithMetadata("AggregateVersion", aggregateVersion);
    }

    /// <summary>
    /// Sets who initiated this domain event
    /// </summary>
    /// <param name="initiatedBy">User or system identifier</param>
    /// <returns>The event instance for fluent configuration</returns>
    public DomainEventBase WithInitiator(string initiatedBy)
    {
        InitiatedBy = initiatedBy;
        WithMetadata("InitiatedBy", initiatedBy);
        return this;
    }

    /// <summary>
    /// Gets a string representation of the domain event
    /// </summary>
    /// <returns>Event description with domain information</returns>
    public override string ToString()
    {
        return $"{EventType} [Id={Id}, AggregateId={AggregateId}, AggregateType={AggregateType}, " +
               $"AggregateVersion={AggregateVersion}, OccurredOn={OccurredOn:yyyy-MM-dd HH:mm:ss} UTC]";
    }
}