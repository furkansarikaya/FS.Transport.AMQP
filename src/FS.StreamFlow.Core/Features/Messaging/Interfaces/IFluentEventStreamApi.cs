namespace FS.StreamFlow.Core.Features.Messaging.Interfaces;

/// <summary>
/// Fluent API interface for event stream operations
/// </summary>
public interface IFluentEventStreamApi
{
    /// <summary>
    /// Appends events to the stream
    /// </summary>
    /// <param name="events">Events to append</param>
    /// <returns>Fluent event stream API for method chaining</returns>
    IFluentEventStreamApi AppendEvents(IEnumerable<object> events);
    
    /// <summary>
    /// Appends a single event to the stream
    /// </summary>
    /// <param name="event">Event to append</param>
    /// <returns>Fluent event stream API for method chaining</returns>
    IFluentEventStreamApi AppendEvent(object @event);
    
    /// <summary>
    /// Appends events with expected version for concurrency control
    /// </summary>
    /// <param name="events">Events to append</param>
    /// <param name="expectedVersion">Expected version for concurrency control</param>
    /// <returns>Fluent event stream API for method chaining</returns>
    IFluentEventStreamApi AppendEventsWithExpectedVersion(IEnumerable<object> events, long expectedVersion);
    
    /// <summary>
    /// Configures the maximum number of events to read
    /// </summary>
    /// <param name="maxCount">Maximum number of events to read</param>
    /// <returns>Fluent event stream API for method chaining</returns>
    IFluentEventStreamApi WithMaxCount(int maxCount);
    
    /// <summary>
    /// Configures the starting version for reading events
    /// </summary>
    /// <param name="fromVersion">Starting version (inclusive)</param>
    /// <returns>Fluent event stream API for method chaining</returns>
    IFluentEventStreamApi FromVersion(long fromVersion);
    
    /// <summary>
    /// Configures snapshot settings for the stream
    /// </summary>
    /// <param name="snapshot">Snapshot data</param>
    /// <param name="version">Version at which the snapshot was taken</param>
    /// <returns>Fluent event stream API for method chaining</returns>
    IFluentEventStreamApi WithSnapshot(object snapshot, long version);
    
    /// <summary>
    /// Saves events to the stream with the configured settings
    /// </summary>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Task representing the save operation with new version</returns>
    Task<long> SaveAsync(CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Reads events from the stream with the configured settings
    /// </summary>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Task representing the read operation with events</returns>
    Task<IEnumerable<object>> ReadAsync(CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Reads events from the stream in reverse order with the configured settings
    /// </summary>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Task representing the read operation with events in reverse order</returns>
    Task<IEnumerable<object>> ReadBackwardAsync(CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Creates or gets the event stream
    /// </summary>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Task representing the stream creation/retrieval operation</returns>
    Task<bool> CreateAsync(CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Deletes the event stream
    /// </summary>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Task representing the delete operation</returns>
    Task<bool> DeleteAsync(CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Truncates the stream at the specified version
    /// </summary>
    /// <param name="version">Version to truncate at</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Task representing the truncation operation</returns>
    Task<bool> TruncateAsync(long version, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Gets the current version of the stream
    /// </summary>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Task representing the version retrieval operation</returns>
    Task<long> GetVersionAsync(CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Checks if the stream exists
    /// </summary>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Task representing the existence check operation</returns>
    Task<bool> ExistsAsync(CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Saves snapshot to the stream
    /// </summary>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Task representing the snapshot save operation</returns>
    Task<bool> SaveSnapshotAsync(CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Gets the latest snapshot for the stream
    /// </summary>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Task representing the snapshot retrieval operation</returns>
    Task<object?> GetSnapshotAsync(CancellationToken cancellationToken = default);
} 