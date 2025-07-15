using FS.StreamFlow.Core.Features.Events.Models;

namespace FS.StreamFlow.Core.Features.Events.Interfaces;

/// <summary>
/// Fluent API interface for advanced event publishing configuration
/// </summary>
/// <typeparam name="T">Event type</typeparam>
public interface IFluentEventBusApi<T> where T : class, IEvent
{
    /// <summary>
    /// Configures event metadata
    /// </summary>
    /// <param name="configure">Metadata configuration action</param>
    /// <returns>Fluent event bus API for method chaining</returns>
    IFluentEventBusApi<T> WithMetadata(Action<EventMetadata> configure);
    
    /// <summary>
    /// Configures correlation ID for event tracking
    /// </summary>
    /// <param name="correlationId">Correlation ID</param>
    /// <returns>Fluent event bus API for method chaining</returns>
    IFluentEventBusApi<T> WithCorrelationId(string correlationId);
    
    /// <summary>
    /// Configures causation ID for event tracking
    /// </summary>
    /// <param name="causationId">Causation ID</param>
    /// <returns>Fluent event bus API for method chaining</returns>
    IFluentEventBusApi<T> WithCausationId(string causationId);
    
    /// <summary>
    /// Configures event source
    /// </summary>
    /// <param name="source">Event source</param>
    /// <returns>Fluent event bus API for method chaining</returns>
    IFluentEventBusApi<T> WithSource(string source);
    
    /// <summary>
    /// Configures event version
    /// </summary>
    /// <param name="version">Event version</param>
    /// <returns>Fluent event bus API for method chaining</returns>
    IFluentEventBusApi<T> WithVersion(string version);
    
    /// <summary>
    /// Configures aggregate ID (for domain events)
    /// </summary>
    /// <param name="aggregateId">Aggregate ID</param>
    /// <returns>Fluent event bus API for method chaining</returns>
    IFluentEventBusApi<T> WithAggregateId(string aggregateId);
    
    /// <summary>
    /// Configures aggregate type (for domain events)
    /// </summary>
    /// <param name="aggregateType">Aggregate type</param>
    /// <returns>Fluent event bus API for method chaining</returns>
    IFluentEventBusApi<T> WithAggregateType(string aggregateType);
    
    /// <summary>
    /// Configures event priority
    /// </summary>
    /// <param name="priority">Event priority</param>
    /// <returns>Fluent event bus API for method chaining</returns>
    IFluentEventBusApi<T> WithPriority(int priority);
    
    /// <summary>
    /// Configures event time to live
    /// </summary>
    /// <param name="ttl">Time to live</param>
    /// <returns>Fluent event bus API for method chaining</returns>
    IFluentEventBusApi<T> WithTtl(TimeSpan ttl);
    
    /// <summary>
    /// Configures custom event properties
    /// </summary>
    /// <param name="properties">Event properties</param>
    /// <returns>Fluent event bus API for method chaining</returns>
    IFluentEventBusApi<T> WithProperties(Dictionary<string, object> properties);
    
    /// <summary>
    /// Adds a custom event property
    /// </summary>
    /// <param name="key">Property key</param>
    /// <param name="value">Property value</param>
    /// <returns>Fluent event bus API for method chaining</returns>
    IFluentEventBusApi<T> WithProperty(string key, object value);
    
    /// <summary>
    /// Publishes the event with the configured settings
    /// </summary>
    /// <param name="eventData">Event data to publish</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Task representing the publish operation</returns>
    Task PublishAsync(T eventData, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Publishes the event synchronously with the configured settings
    /// </summary>
    /// <param name="eventData">Event data to publish</param>
    /// <returns>Event publishing result</returns>
    bool Publish(T eventData);
} 