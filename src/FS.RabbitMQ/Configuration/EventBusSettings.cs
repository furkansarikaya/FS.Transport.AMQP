namespace FS.RabbitMQ.Configuration;

/// <summary>
/// Settings for event bus functionality
/// </summary>
public class EventBusSettings
{
    /// <summary>
    /// Default exchange for domain events
    /// </summary>
    public string DomainEventsExchange { get; set; } = "domain-events";
    
    /// <summary>
    /// Default exchange for integration events
    /// </summary>
    public string IntegrationEventsExchange { get; set; } = "integration-events";
    
    /// <summary>
    /// Default routing key prefix for domain events
    /// </summary>
    public string DomainEventRoutingKeyPrefix { get; set; } = "domain";
    
    /// <summary>
    /// Default routing key prefix for integration events
    /// </summary>
    public string IntegrationEventRoutingKeyPrefix { get; set; } = "integration";
    
    /// <summary>
    /// Whether to include event metadata in message headers
    /// </summary>
    public bool IncludeEventMetadata { get; set; } = true;
    
    /// <summary>
    /// Default event serialization format (Json, Binary)
    /// </summary>
    public string DefaultSerializationFormat { get; set; } = "Json";
    
    /// <summary>
    /// Whether to validate event schemas
    /// </summary>
    public bool ValidateEventSchemas { get; set; } = false;
    
    /// <summary>
    /// Event handler timeout in milliseconds
    /// </summary>
    public int EventHandlerTimeoutMs { get; set; } = 30000;
    
    /// <summary>
    /// Maximum concurrent event handlers per event type
    /// </summary>
    public int MaxConcurrentHandlers { get; set; } = 10;
    
    /// <summary>
    /// Whether to use distributed tracing for events
    /// </summary>
    public bool EnableDistributedTracing { get; set; } = true;

    /// <summary>
    /// Validates event bus settings
    /// </summary>
    public void Validate()
    {
        if (string.IsNullOrWhiteSpace(DomainEventsExchange))
            throw new ArgumentException("DomainEventsExchange cannot be null or empty");
            
        if (string.IsNullOrWhiteSpace(IntegrationEventsExchange))
            throw new ArgumentException("IntegrationEventsExchange cannot be null or empty");
            
        if (string.IsNullOrWhiteSpace(DomainEventRoutingKeyPrefix))
            throw new ArgumentException("DomainEventRoutingKeyPrefix cannot be null or empty");
            
        if (string.IsNullOrWhiteSpace(IntegrationEventRoutingKeyPrefix))
            throw new ArgumentException("IntegrationEventRoutingKeyPrefix cannot be null or empty");
            
        var validFormats = new[] { "Json", "Binary" };
        if (!validFormats.Contains(DefaultSerializationFormat))
            throw new ArgumentException($"DefaultSerializationFormat must be one of: {string.Join(", ", validFormats)}");
            
        if (EventHandlerTimeoutMs <= 0)
            throw new ArgumentException("EventHandlerTimeoutMs must be greater than 0");
            
        if (MaxConcurrentHandlers <= 0)
            throw new ArgumentException("MaxConcurrentHandlers must be greater than 0");
    }
}