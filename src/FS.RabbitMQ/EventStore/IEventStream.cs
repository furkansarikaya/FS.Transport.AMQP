using FS.RabbitMQ.Events;

namespace FS.RabbitMQ.EventStore;

/// <summary>
/// Represents a stream of events for a specific aggregate
/// </summary>
public interface IEventStream : IDisposable
{
    /// <summary>
    /// Unique identifier for the aggregate
    /// </summary>
    string AggregateId { get; }
    
    /// <summary>
    /// Type of the aggregate
    /// </summary>
    string AggregateType { get; }
    
    /// <summary>
    /// Current version of the aggregate
    /// </summary>
    long Version { get; }
    
    /// <summary>
    /// Timestamp when the stream was created
    /// </summary>
    DateTimeOffset CreatedAt { get; }
    
    /// <summary>
    /// Timestamp when the stream was last modified
    /// </summary>
    DateTimeOffset LastModifiedAt { get; }
    
    /// <summary>
    /// Total number of events in the stream
    /// </summary>
    long EventCount { get; }
    
    /// <summary>
    /// Gets all events in the stream
    /// </summary>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Collection of event snapshots</returns>
    Task<IEnumerable<EventSnapshot>> GetAllEventsAsync(CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Gets events from a specific version
    /// </summary>
    /// <param name="fromVersion">Starting version (inclusive)</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Collection of event snapshots</returns>
    Task<IEnumerable<EventSnapshot>> GetEventsFromVersionAsync(long fromVersion, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Gets events within a version range
    /// </summary>
    /// <param name="fromVersion">Starting version (inclusive)</param>
    /// <param name="toVersion">Ending version (inclusive)</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Collection of event snapshots</returns>
    Task<IEnumerable<EventSnapshot>> GetEventsInRangeAsync(long fromVersion, long toVersion, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Gets events with pagination
    /// </summary>
    /// <param name="offset">Number of events to skip</param>
    /// <param name="limit">Maximum number of events to return</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Collection of event snapshots</returns>
    Task<IEnumerable<EventSnapshot>> GetEventsPaginatedAsync(int offset, int limit, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Appends new events to the stream
    /// </summary>
    /// <param name="events">Events to append</param>
    /// <param name="expectedVersion">Expected current version for concurrency control</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>New version after appending events</returns>
    Task<long> AppendEventsAsync(IEnumerable<IEvent> events, long expectedVersion, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Checks if the stream exists
    /// </summary>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>True if stream exists</returns>
    Task<bool> ExistsAsync(CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Deletes the entire stream
    /// </summary>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Task representing the deletion operation</returns>
    Task DeleteAsync(CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Gets stream metadata
    /// </summary>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Stream metadata</returns>
    Task<Dictionary<string, object>> GetMetadataAsync(CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Sets stream metadata
    /// </summary>
    /// <param name="metadata">Metadata to set</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Task representing the operation</returns>
    Task SetMetadataAsync(Dictionary<string, object> metadata, CancellationToken cancellationToken = default);
}