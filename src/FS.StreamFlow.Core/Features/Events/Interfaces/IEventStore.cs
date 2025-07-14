using FS.StreamFlow.Core.Features.Events.Models;

namespace FS.StreamFlow.Core.Features.Events.Interfaces;

/// <summary>
/// Interface for event store operations providing CQRS pattern implementation
/// </summary>
public interface IEventStore : IDisposable
{
    /// <summary>
    /// Gets the current status of the event store
    /// </summary>
    EventStoreStatus Status { get; }
    
    /// <summary>
    /// Gets event store statistics and metrics
    /// </summary>
    EventStoreStatistics Statistics { get; }
    
    /// <summary>
    /// Initializes the event store
    /// </summary>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Task representing the initialization operation</returns>
    Task InitializeAsync(CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Saves events to a stream
    /// </summary>
    /// <param name="streamId">Stream identifier</param>
    /// <param name="events">Events to save</param>
    /// <param name="expectedVersion">Expected version for concurrency control</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>New version after saving events</returns>
    Task<long> SaveEventsAsync(string streamId, IEnumerable<object> events, long expectedVersion, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Gets events from a stream
    /// </summary>
    /// <param name="streamId">Stream identifier</param>
    /// <param name="fromVersion">Starting version (inclusive)</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Collection of events</returns>
    Task<IEnumerable<object>> GetEventsAsync(string streamId, long fromVersion = 0, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Gets an event stream
    /// </summary>
    /// <param name="streamId">Stream identifier</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Event stream</returns>
    Task<IEventStream?> GetStreamAsync(string streamId, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Creates a new event stream
    /// </summary>
    /// <param name="streamId">Stream identifier</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Event stream</returns>
    Task<IEventStream> CreateStreamAsync(string streamId, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Deletes an event stream
    /// </summary>
    /// <param name="streamId">Stream identifier</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Task representing the deletion operation</returns>
    Task DeleteStreamAsync(string streamId, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Gets all stream identifiers
    /// </summary>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Collection of stream identifiers</returns>
    Task<IEnumerable<string>> GetStreamIdsAsync(CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Creates a snapshot of an aggregate
    /// </summary>
    /// <param name="streamId">Stream identifier</param>
    /// <param name="snapshot">Snapshot data</param>
    /// <param name="version">Version at which the snapshot was taken</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Task representing the snapshot operation</returns>
    Task SaveSnapshotAsync(string streamId, object snapshot, long version, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Gets the latest snapshot for an aggregate
    /// </summary>
    /// <param name="streamId">Stream identifier</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Snapshot data or null if no snapshot exists</returns>
    Task<EventSnapshot?> GetSnapshotAsync(string streamId, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Checks if a stream exists
    /// </summary>
    /// <param name="streamId">Stream identifier</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>True if the stream exists, otherwise false</returns>
    Task<bool> StreamExistsAsync(string streamId, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Gets the version of a stream
    /// </summary>
    /// <param name="streamId">Stream identifier</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Current version of the stream</returns>
    Task<long> GetStreamVersionAsync(string streamId, CancellationToken cancellationToken = default);
}

/// <summary>
/// Interface for event stream operations
/// </summary>
public interface IEventStream : IDisposable
{
    /// <summary>
    /// Stream identifier
    /// </summary>
    string StreamId { get; }
    
    /// <summary>
    /// Current version of the stream
    /// </summary>
    long Version { get; }
    
    /// <summary>
    /// Gets the current status of the stream
    /// </summary>
    EventStreamStatus Status { get; }
    
    /// <summary>
    /// Appends events to the stream
    /// </summary>
    /// <param name="events">Events to append</param>
    /// <param name="expectedVersion">Expected version for concurrency control</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>New version after appending events</returns>
    Task<long> AppendEventsAsync(IEnumerable<object> events, long expectedVersion, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Reads events from the stream
    /// </summary>
    /// <param name="fromVersion">Starting version (inclusive)</param>
    /// <param name="maxCount">Maximum number of events to read</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Collection of events</returns>
    Task<IEnumerable<object>> ReadEventsAsync(long fromVersion = 0, int maxCount = int.MaxValue, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Reads events from the stream in reverse order
    /// </summary>
    /// <param name="fromVersion">Starting version (inclusive)</param>
    /// <param name="maxCount">Maximum number of events to read</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Collection of events in reverse order</returns>
    Task<IEnumerable<object>> ReadEventsBackwardAsync(long fromVersion = -1, int maxCount = int.MaxValue, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Truncates the stream at the specified version
    /// </summary>
    /// <param name="version">Version to truncate at</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Task representing the truncation operation</returns>
    Task TruncateAsync(long version, CancellationToken cancellationToken = default);
} 