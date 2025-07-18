using FS.StreamFlow.Core.Features.Events.Interfaces;
using FS.StreamFlow.Core.Features.Events.Models;

namespace FS.StreamFlow.RabbitMQ.Features.EventBus;

/// <summary>
/// RabbitMQ implementation of fluent event bus API for chainable event publishing configuration.
/// Provides a fluent interface for configuring event metadata, properties, and publishing options.
/// Uses fanout exchanges for event distribution without routing keys.
/// </summary>
/// <typeparam name="T">Event type that implements IEvent</typeparam>
public class RabbitMQFluentEventBusApi<T> : IFluentEventBusApi<T> where T : class, IEvent
{
    private readonly IEventBus _eventBus;
    private readonly EventMetadata _metadata;
    private readonly Dictionary<string, object> _properties;

    /// <summary>
    /// Initializes a new instance of the RabbitMQ fluent event bus API
    /// </summary>
    /// <param name="eventBus">Event bus instance</param>
    public RabbitMQFluentEventBusApi(IEventBus eventBus)
    {
        _eventBus = eventBus ?? throw new ArgumentNullException(nameof(eventBus));
        _metadata = new EventMetadata();
        _properties = new Dictionary<string, object>();
    }

    /// <summary>
    /// Configures event metadata
    /// </summary>
    /// <param name="configure">Metadata configuration action</param>
    /// <returns>Fluent event bus API for method chaining</returns>
    public IFluentEventBusApi<T> WithMetadata(Action<EventMetadata> configure)
    {
        if (configure == null)
            throw new ArgumentNullException(nameof(configure));

        configure(_metadata);
        return this;
    }

    /// <summary>
    /// Configures correlation ID for event tracking
    /// </summary>
    /// <param name="correlationId">Correlation ID</param>
    /// <returns>Fluent event bus API for method chaining</returns>
    public IFluentEventBusApi<T> WithCorrelationId(string correlationId)
    {
        _metadata.CorrelationId = correlationId;
        return this;
    }

    /// <summary>
    /// Configures causation ID for event tracking
    /// </summary>
    /// <param name="causationId">Causation ID</param>
    /// <returns>Fluent event bus API for method chaining</returns>
    public IFluentEventBusApi<T> WithCausationId(string causationId)
    {
        _metadata.CausationId = causationId;
        return this;
    }

    /// <summary>
    /// Configures event source
    /// </summary>
    /// <param name="source">Event source</param>
    /// <returns>Fluent event bus API for method chaining</returns>
    public IFluentEventBusApi<T> WithSource(string source)
    {
        _metadata.Source = source;
        return this;
    }

    /// <summary>
    /// Configures event version
    /// </summary>
    /// <param name="version">Event version</param>
    /// <returns>Fluent event bus API for method chaining</returns>
    public IFluentEventBusApi<T> WithVersion(string version)
    {
        _metadata.Version = int.Parse(version);
        return this;
    }

    /// <summary>
    /// Configures aggregate ID (for domain events)
    /// </summary>
    /// <param name="aggregateId">Aggregate ID</param>
    /// <returns>Fluent event bus API for method chaining</returns>
    public IFluentEventBusApi<T> WithAggregateId(string aggregateId)
    {
        _metadata.Aggregate ??= new AggregateMetadata();
        _metadata.Aggregate.Id = aggregateId;
        return this;
    }

    /// <summary>
    /// Configures aggregate type (for domain events).
    /// This will be used to create the exchange name as "domain.{aggregateType}" for fanout exchanges.
    /// </summary>
    /// <param name="aggregateType">Aggregate type (e.g., "Order", "Customer")</param>
    /// <returns>Fluent event bus API for method chaining</returns>
    public IFluentEventBusApi<T> WithAggregateType(string aggregateType)
    {
        _metadata.Aggregate ??= new AggregateMetadata();
        _metadata.Aggregate.Type = aggregateType;
        return this;
    }

    /// <summary>
    /// Configures event priority
    /// </summary>
    /// <param name="priority">Event priority</param>
    /// <returns>Fluent event bus API for method chaining</returns>
    public IFluentEventBusApi<T> WithPriority(int priority)
    {
        _properties["priority"] = priority;
        return this;
    }

    /// <summary>
    /// Configures event time to live
    /// </summary>
    /// <param name="ttl">Time to live</param>
    /// <returns>Fluent event bus API for method chaining</returns>
    public IFluentEventBusApi<T> WithTtl(TimeSpan ttl)
    {
        _properties["ttl"] = ttl;
        return this;
    }

    /// <summary>
    /// Configures custom event properties
    /// </summary>
    /// <param name="properties">Event properties</param>
    /// <returns>Fluent event bus API for method chaining</returns>
    public IFluentEventBusApi<T> WithProperties(Dictionary<string, object> properties)
    {
        if (properties == null)
            throw new ArgumentNullException(nameof(properties));

        foreach (var kvp in properties)
        {
            _properties[kvp.Key] = kvp.Value;
        }
        return this;
    }

    /// <summary>
    /// Adds a custom event property
    /// </summary>
    /// <param name="key">Property key</param>
    /// <param name="value">Property value</param>
    /// <returns>Fluent event bus API for method chaining</returns>
    public IFluentEventBusApi<T> WithProperty(string key, object value)
    {
        if (string.IsNullOrEmpty(key))
            throw new ArgumentNullException(nameof(key));

        _properties[key] = value;
        return this;
    }

    /// <summary>
    /// Publishes the event with the configured settings.
    /// Applies all fluent API metadata to the event before publishing.
    /// Uses fanout exchanges: domain events go to "domain.{aggregateType}" exchanges,
    /// integration events go to exchanges named after their ExchangeName property.
    /// </summary>
    /// <param name="eventData">Event data to publish</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Task representing the publish operation</returns>
    /// <exception cref="ArgumentNullException">Thrown when eventData is null</exception>
    /// <exception cref="InvalidOperationException">Thrown when event doesn't implement IDomainEvent or IIntegrationEvent</exception>
    public async Task PublishAsync(T eventData, CancellationToken cancellationToken = default)
    {
        if (eventData == null)
            throw new ArgumentNullException(nameof(eventData));

        // Apply fluent API metadata to the event
        ApplyMetadataToEvent(eventData);

        // Publish based on event type
        if (eventData is IDomainEvent domainEvent)
        {
            await _eventBus.PublishDomainEventAsync(domainEvent, cancellationToken);
        }
        else if (eventData is IIntegrationEvent integrationEvent)
        {
            await _eventBus.PublishIntegrationEventAsync(integrationEvent, cancellationToken);
        }
        else
        {
            throw new InvalidOperationException("Event must implement IDomainEvent or IIntegrationEvent");
        }
    }

    /// <summary>
    /// Applies the configured metadata to the event
    /// </summary>
    /// <param name="eventData">Event to apply metadata to</param>
    private void ApplyMetadataToEvent(T eventData)
    {
        // Apply common metadata
        if (!string.IsNullOrEmpty(_metadata.CorrelationId))
            eventData.CorrelationId = _metadata.CorrelationId;
        
        if (!string.IsNullOrEmpty(_metadata.CausationId))
            eventData.CausationId = _metadata.CausationId;
        
        if (!string.IsNullOrEmpty(_metadata.Source))
            eventData.Source = _metadata.Source;
        
        if (_metadata.Version.HasValue)
            eventData.Version = _metadata.Version.Value.ToString();

        // Apply domain event specific metadata
        if (eventData is IDomainEvent domainEvent)
        {
            if (_metadata.Aggregate != null)
            {
                if (!string.IsNullOrEmpty(_metadata.Aggregate.Id))
                    domainEvent.AggregateId = _metadata.Aggregate.Id;
                
                if (!string.IsNullOrEmpty(_metadata.Aggregate.Type))
                    domainEvent.AggregateType = _metadata.Aggregate.Type;
            }
        }

        // Apply integration event specific metadata
        if (eventData is IIntegrationEvent integrationEvent)
        {
            // Apply TTL if specified
            if (_properties.TryGetValue("ttl", out var ttlValue) && ttlValue is TimeSpan ttl)
                integrationEvent.TimeToLive = ttl;
        }

        // Apply custom properties if the event supports them
        if (_properties.Count > 0 && eventData is IEvent eventWithProperties)
        {
            eventWithProperties.Properties ??= new Dictionary<string, object>();
            foreach (var kvp in _properties)
            {
                if (kvp.Key != "ttl") // TTL is handled separately
                    eventWithProperties.Properties[kvp.Key] = kvp.Value;
            }
        }
    }

    /// <summary>
    /// Publishes the event synchronously with the configured settings
    /// </summary>
    /// <param name="eventData">Event data to publish</param>
    /// <returns>Event publishing result</returns>
    public bool Publish(T eventData)
    {
        try
        {
            PublishAsync(eventData).GetAwaiter().GetResult();
            return true;
        }
        catch
        {
            return false;
        }
    }
} 