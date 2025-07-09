using System.Reflection;
using System.Text.Json;
using FS.Transport.AMQP.Core;

namespace FS.Transport.AMQP.Events;

/// <summary>
/// Extension methods for working with events
/// </summary>
public static class EventExtensions
{
    /// <summary>
    /// Checks if an event is a domain event
    /// </summary>
    /// <param name="event">Event to check</param>
    /// <returns>True if it's a domain event</returns>
    public static bool IsDomainEvent(this IEvent @event)
    {
        return @event is IDomainEvent;
    }

    /// <summary>
    /// Checks if an event is an integration event
    /// </summary>
    /// <param name="event">Event to check</param>
    /// <returns>True if it's an integration event</returns>
    public static bool IsIntegrationEvent(this IEvent @event)
    {
        return @event is IIntegrationEvent;
    }

    /// <summary>
    /// Gets the routing key for an event
    /// </summary>
    /// <param name="event">Event instance</param>
    /// <param name="defaultPrefix">Default prefix if not an integration event</param>
    /// <returns>Routing key</returns>
    public static string GetRoutingKey(this IEvent @event, string defaultPrefix = "events")
    {
        return @event switch
        {
            IIntegrationEvent integrationEvent => integrationEvent.RoutingKey,
            IDomainEvent domainEvent => $"{defaultPrefix}.domain.{domainEvent.AggregateType.ToLowerInvariant()}.{@event.EventType.ToLowerInvariant()}",
            _ => $"{defaultPrefix}.{@event.EventType.ToLowerInvariant()}"
        };
    }

    /// <summary>
    /// Gets the exchange name for an event based on its type
    /// </summary>
    /// <param name="event">Event instance</param>
    /// <param name="domainExchange">Exchange for domain events</param>
    /// <param name="integrationExchange">Exchange for integration events</param>
    /// <returns>Exchange name</returns>
    public static string GetExchangeName(this IEvent @event, 
        string domainExchange = Constants.Exchanges.DomainEvents, 
        string integrationExchange = Constants.Exchanges.IntegrationEvents)
    {
        return @event switch
        {
            IIntegrationEvent => integrationExchange,
            IDomainEvent => domainExchange,
            _ => domainExchange
        };
    }

    /// <summary>
    /// Creates a message headers dictionary from event metadata
    /// </summary>
    /// <param name="event">Event instance</param>
    /// <returns>Headers dictionary</returns>
    public static IDictionary<string, object> ToHeaders(this IEvent @event)
    {
        var metadata = EventMetadata.FromEvent(@event);
        return metadata.ToDictionary();
    }

    /// <summary>
    /// Extracts event metadata from message headers
    /// </summary>
    /// <param name="headers">Message headers</param>
    /// <returns>Event metadata</returns>
    public static EventMetadata ToEventMetadata(this IDictionary<string, object> headers)
    {
        return EventMetadata.FromDictionary(headers);
    }

    /// <summary>
    /// Creates an event from its metadata and payload
    /// </summary>
    /// <typeparam name="T">Event type</typeparam>
    /// <param name="metadata">Event metadata</param>
    /// <param name="payload">Event payload (JSON)</param>
    /// <returns>Reconstructed event</returns>
    public static T ReconstructEvent<T>(this EventMetadata metadata, string payload) where T : class, IEvent
    {
        if (string.IsNullOrWhiteSpace(payload))
            throw new ArgumentException("Payload cannot be null or empty", nameof(payload));

        try
        {
            var options = new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                PropertyNameCaseInsensitive = true
            };

            var @event = JsonSerializer.Deserialize<T>(payload, options);
            if (@event == null)
                throw new InvalidOperationException($"Failed to deserialize event of type {typeof(T).Name}");

            // Apply metadata to the reconstructed event
            if (@event is EventBase eventBase)
            {
                // Update properties that might have been overridden during deserialization
                var idField = typeof(EventBase).GetField("<Id>k__BackingField", BindingFlags.NonPublic | BindingFlags.Instance);
                var occurredOnField = typeof(EventBase).GetField("<OccurredOn>k__BackingField", BindingFlags.NonPublic | BindingFlags.Instance);
                
                idField?.SetValue(eventBase, metadata.EventId);
                occurredOnField?.SetValue(eventBase, metadata.OccurredOn);
                
                if (!string.IsNullOrEmpty(metadata.CorrelationId))
                    eventBase.CorrelationId = metadata.CorrelationId;
                    
                if (!string.IsNullOrEmpty(metadata.CausationId))
                    eventBase.CausationId = metadata.CausationId;

                // Restore metadata
                foreach (var property in metadata.Properties)
                {
                    eventBase.Metadata[property.Key] = property.Value;
                }
            }

            return @event;
        }
        catch (JsonException ex)
        {
            throw new InvalidOperationException($"Failed to reconstruct event of type {typeof(T).Name}", ex);
        }
    }

    /// <summary>
    /// Serializes an event to JSON with metadata
    /// </summary>
    /// <param name="event">Event to serialize</param>
    /// <returns>JSON representation</returns>
    public static string ToJson(this IEvent @event)
    {
        var options = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            WriteIndented = false
        };

        return JsonSerializer.Serialize(@event, @event.GetType(), options);
    }

    /// <summary>
    /// Creates a copy of an event with a new correlation ID
    /// </summary>
    /// <typeparam name="T">Event type</typeparam>
    /// <param name="event">Original event</param>
    /// <param name="correlationId">New correlation ID</param>
    /// <returns>Event copy with new correlation ID</returns>
    public static T WithNewCorrelationId<T>(this T @event, string correlationId) where T : EventBase
    {
        var copy = @event.Clone();
        copy.CorrelationId = correlationId;
        return (T)copy;
    }

    /// <summary>
    /// Creates a copy of an event with a new causation ID
    /// </summary>
    /// <typeparam name="T">Event type</typeparam>
    /// <param name="event">Original event</param>
    /// <param name="causationId">New causation ID</param>
    /// <returns>Event copy with new causation ID</returns>
    public static T WithNewCausationId<T>(this T @event, string causationId) where T : EventBase
    {
        var copy = @event.Clone();
        copy.CausationId = causationId;
        return (T)copy;
    }

    /// <summary>
    /// Checks if an event has expired based on its TTL
    /// </summary>
    /// <param name="event">Event to check</param>
    /// <returns>True if the event has expired</returns>
    public static bool HasExpired(this IEvent @event)
    {
        if (@event is IIntegrationEvent integrationEvent && integrationEvent.TimeToLive.HasValue)
        {
            return DateTime.UtcNow > @event.OccurredOn.Add(integrationEvent.TimeToLive.Value);
        }

        return false;
    }

    /// <summary>
    /// Gets the age of an event (time since it occurred)
    /// </summary>
    /// <param name="event">Event instance</param>
    /// <returns>Time span since the event occurred</returns>
    public static TimeSpan GetAge(this IEvent @event)
    {
        return DateTime.UtcNow - @event.OccurredOn;
    }

    /// <summary>
    /// Determines if two events are causally related
    /// </summary>
    /// <param name="event1">First event</param>
    /// <param name="event2">Second event</param>
    /// <returns>True if events are causally related</returns>
    public static bool IsCausallyRelatedTo(this IEvent event1, IEvent event2)
    {
        return !string.IsNullOrEmpty(event1.CorrelationId) && 
               event1.CorrelationId == event2.CorrelationId;
    }

    /// <summary>
    /// Validates that an event has all required properties
    /// </summary>
    /// <param name="event">Event to validate</param>
    /// <returns>Validation result</returns>
    public static EventValidationResult Validate(this IEvent @event)
    {
        var result = new EventValidationResult();

        if (@event.Id == Guid.Empty)
            result.AddError("Event ID cannot be empty");

        if (string.IsNullOrWhiteSpace(@event.EventType))
            result.AddError("Event type cannot be null or empty");

        if (@event.OccurredOn == default)
            result.AddError("OccurredOn must be set");

        if (@event.OccurredOn > DateTime.UtcNow.AddMinutes(5))
            result.AddWarning("Event occurred in the future");

        if (@event.Version <= 0)
            result.AddError("Event version must be greater than 0");

        // Domain event specific validation
        if (@event is IDomainEvent domainEvent)
        {
            if (string.IsNullOrWhiteSpace(domainEvent.AggregateId))
                result.AddError("Domain event must have an aggregate ID");

            if (string.IsNullOrWhiteSpace(domainEvent.AggregateType))
                result.AddError("Domain event must have an aggregate type");
        }

        // Integration event specific validation
        if (@event is IIntegrationEvent integrationEvent)
        {
            if (string.IsNullOrWhiteSpace(integrationEvent.Source))
                result.AddError("Integration event must have a source");

            if (string.IsNullOrWhiteSpace(integrationEvent.RoutingKey))
                result.AddError("Integration event must have a routing key");

            if (string.IsNullOrWhiteSpace(integrationEvent.SchemaVersion))
                result.AddError("Integration event must have a schema version");
        }

        return result;
    }
}