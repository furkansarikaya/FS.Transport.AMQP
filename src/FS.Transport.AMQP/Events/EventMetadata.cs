using System.Text.Json;

namespace FS.Transport.AMQP.Events;

/// <summary>
/// Represents metadata associated with an event for enhanced tracking and processing
/// </summary>
public class EventMetadata
{
    /// <summary>
    /// Event identifier
    /// </summary>
    public Guid EventId { get; set; }
    
    /// <summary>
    /// Event type name
    /// </summary>
    public string EventType { get; set; } = string.Empty;
    
    /// <summary>
    /// Full type name including namespace
    /// </summary>
    public string? EventTypeName { get; set; }
    
    /// <summary>
    /// Assembly name where the event type is defined
    /// </summary>
    public string? AssemblyName { get; set; }
    
    /// <summary>
    /// When the event occurred (UTC)
    /// </summary>
    public DateTime OccurredOn { get; set; }
    
    /// <summary>
    /// Event version for schema evolution
    /// </summary>
    public int Version { get; set; }
    
    /// <summary>
    /// Correlation identifier for tracing
    /// </summary>
    public string? CorrelationId { get; set; }
    
    /// <summary>
    /// Causation identifier for command/event chains
    /// </summary>
    public string? CausationId { get; set; }
    
    /// <summary>
    /// Source system or service
    /// </summary>
    public string? Source { get; set; }
    
    /// <summary>
    /// Target system or service (for integration events)
    /// </summary>
    public string? Target { get; set; }
    
    /// <summary>
    /// User or system that initiated the action
    /// </summary>
    public string? InitiatedBy { get; set; }
    
    /// <summary>
    /// Aggregate information (for domain events)
    /// </summary>
    public AggregateMetadata? Aggregate { get; set; }
    
    /// <summary>
    /// Additional custom properties
    /// </summary>
    public IDictionary<string, object> Properties { get; set; } = new Dictionary<string, object>();

    /// <summary>
    /// Creates metadata from an event
    /// </summary>
    /// <param name="event">Event instance</param>
    /// <returns>Event metadata</returns>
    public static EventMetadata FromEvent(IEvent @event)
    {
        if (@event == null)
            throw new ArgumentNullException(nameof(@event));

        var metadata = new EventMetadata
        {
            EventId = @event.Id,
            EventType = @event.EventType,
            OccurredOn = @event.OccurredOn,
            Version = @event.Version,
            CorrelationId = @event.CorrelationId,
            CausationId = @event.CausationId,
            Properties = new Dictionary<string, object>(@event.Metadata)
        };

        // Extract additional metadata from event properties
        if (@event.Metadata.TryGetValue("EventTypeName", out var eventTypeName))
            metadata.EventTypeName = eventTypeName?.ToString();
            
        if (@event.Metadata.TryGetValue("AssemblyName", out var assemblyName))
            metadata.AssemblyName = assemblyName?.ToString();
            
        if (@event.Metadata.TryGetValue("Source", out var source))
            metadata.Source = source?.ToString();
            
        if (@event.Metadata.TryGetValue("Target", out var target))
            metadata.Target = target?.ToString();
            
        if (@event.Metadata.TryGetValue("InitiatedBy", out var initiatedBy))
            metadata.InitiatedBy = initiatedBy?.ToString();

        // Handle domain event specific metadata
        if (@event is IDomainEvent domainEvent)
        {
            metadata.Aggregate = new AggregateMetadata
            {
                Id = domainEvent.AggregateId,
                Type = domainEvent.AggregateType,
                Version = domainEvent.AggregateVersion
            };
        }

        return metadata;
    }

    /// <summary>
    /// Converts metadata to dictionary for message headers
    /// </summary>
    /// <returns>Dictionary of metadata properties</returns>
    public IDictionary<string, object> ToDictionary()
    {
        var dict = new Dictionary<string, object>
        {
            ["EventId"] = EventId.ToString(),
            ["EventType"] = EventType,
            ["OccurredOn"] = OccurredOn.ToString("O"), // ISO 8601 format
            ["Version"] = Version
        };

        if (!string.IsNullOrEmpty(EventTypeName))
            dict["EventTypeName"] = EventTypeName;
            
        if (!string.IsNullOrEmpty(AssemblyName))
            dict["AssemblyName"] = AssemblyName;
            
        if (!string.IsNullOrEmpty(CorrelationId))
            dict["CorrelationId"] = CorrelationId;
            
        if (!string.IsNullOrEmpty(CausationId))
            dict["CausationId"] = CausationId;
            
        if (!string.IsNullOrEmpty(Source))
            dict["Source"] = Source;
            
        if (!string.IsNullOrEmpty(Target))
            dict["Target"] = Target;
            
        if (!string.IsNullOrEmpty(InitiatedBy))
            dict["InitiatedBy"] = InitiatedBy;

        if (Aggregate != null)
        {
            dict["AggregateId"] = Aggregate.Id;
            dict["AggregateType"] = Aggregate.Type;
            dict["AggregateVersion"] = Aggregate.Version;
        }

        // Add custom properties
        foreach (var property in Properties)
        {
            if (!dict.ContainsKey(property.Key))
            {
                dict[property.Key] = property.Value;
            }
        }

        return dict;
    }

    /// <summary>
    /// Creates metadata from dictionary (from message headers)
    /// </summary>
    /// <param name="dictionary">Dictionary of metadata</param>
    /// <returns>Event metadata</returns>
    public static EventMetadata FromDictionary(IDictionary<string, object> dictionary)
    {
        if (dictionary == null)
            throw new ArgumentNullException(nameof(dictionary));

        var metadata = new EventMetadata();

        if (dictionary.TryGetValue("EventId", out var eventId) && Guid.TryParse(eventId?.ToString(), out var id))
            metadata.EventId = id;
            
        if (dictionary.TryGetValue("EventType", out var eventType))
            metadata.EventType = eventType?.ToString() ?? string.Empty;
            
        if (dictionary.TryGetValue("EventTypeName", out var eventTypeName))
            metadata.EventTypeName = eventTypeName?.ToString();
            
        if (dictionary.TryGetValue("AssemblyName", out var assemblyName))
            metadata.AssemblyName = assemblyName?.ToString();
            
        if (dictionary.TryGetValue("OccurredOn", out var occurredOn) && DateTime.TryParse(occurredOn?.ToString(), out var occuredDate))
            metadata.OccurredOn = occuredDate.ToUniversalTime();
            
        if (dictionary.TryGetValue("Version", out var version) && int.TryParse(version?.ToString(), out var ver))
            metadata.Version = ver;
            
        if (dictionary.TryGetValue("CorrelationId", out var correlationId))
            metadata.CorrelationId = correlationId?.ToString();
            
        if (dictionary.TryGetValue("CausationId", out var causationId))
            metadata.CausationId = causationId?.ToString();
            
        if (dictionary.TryGetValue("Source", out var source))
            metadata.Source = source?.ToString();
            
        if (dictionary.TryGetValue("Target", out var target))
            metadata.Target = target?.ToString();
            
        if (dictionary.TryGetValue("InitiatedBy", out var initiatedBy))
            metadata.InitiatedBy = initiatedBy?.ToString();

        // Check for aggregate metadata
        if (dictionary.TryGetValue("AggregateId", out var aggregateId) && 
            dictionary.TryGetValue("AggregateType", out var aggregateType))
        {
            metadata.Aggregate = new AggregateMetadata
            {
                Id = aggregateId?.ToString() ?? string.Empty,
                Type = aggregateType?.ToString() ?? string.Empty
            };
            
            if (dictionary.TryGetValue("AggregateVersion", out var aggregateVersion) && 
                long.TryParse(aggregateVersion?.ToString(), out var aggVer))
            {
                metadata.Aggregate.Version = aggVer;
            }
        }

        // Add remaining properties
        var standardKeys = new HashSet<string>
        {
            "EventId", "EventType", "EventTypeName", "AssemblyName", "OccurredOn", "Version",
            "CorrelationId", "CausationId", "Source", "Target", "InitiatedBy",
            "AggregateId", "AggregateType", "AggregateVersion"
        };

        foreach (var kvp in dictionary)
        {
            if (!standardKeys.Contains(kvp.Key))
            {
                metadata.Properties[kvp.Key] = kvp.Value;
            }
        }

        return metadata;
    }

    /// <summary>
    /// Serializes metadata to JSON
    /// </summary>
    /// <returns>JSON representation</returns>
    public string ToJson()
    {
        return JsonSerializer.Serialize(this, new JsonSerializerOptions 
        { 
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            WriteIndented = false
        });
    }

    /// <summary>
    /// Deserializes metadata from JSON
    /// </summary>
    /// <param name="json">JSON string</param>
    /// <returns>Event metadata</returns>
    public static EventMetadata FromJson(string json)
    {
        if (string.IsNullOrWhiteSpace(json))
            throw new ArgumentException("JSON cannot be null or empty", nameof(json));

        return JsonSerializer.Deserialize<EventMetadata>(json, new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            PropertyNameCaseInsensitive = true
        }) ?? new EventMetadata();
    }
}