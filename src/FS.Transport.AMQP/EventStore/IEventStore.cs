namespace FS.Transport.AMQP.EventStore;

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
    /// Checks if a stream exists
    /// </summary>
    /// <param name="streamId">Stream identifier</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>True if stream exists</returns>
    Task<bool> StreamExistsAsync(string streamId, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Deletes a stream
    /// </summary>
    /// <param name="streamId">Stream identifier</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Task representing the deletion operation</returns>
    Task DeleteStreamAsync(string streamId, CancellationToken cancellationToken = default);
}

/// <summary>
/// Event store status enumeration
/// </summary>
public enum EventStoreStatus
{
    /// <summary>
    /// Event store is not initialized
    /// </summary>
    NotInitialized,
    
    /// <summary>
    /// Event store is initializing
    /// </summary>
    Initializing,
    
    /// <summary>
    /// Event store is running
    /// </summary>
    Running,
    
    /// <summary>
    /// Event store is stopping
    /// </summary>
    Stopping,
    
    /// <summary>
    /// Event store is stopped
    /// </summary>
    Stopped,
    
    /// <summary>
    /// Event store is in faulted state
    /// </summary>
    Faulted
}

/// <summary>
/// Event store statistics
/// </summary>
public class EventStoreStatistics
{
    /// <summary>
    /// Total number of events stored
    /// </summary>
    public long TotalEvents { get; set; }
    
    /// <summary>
    /// Total number of streams
    /// </summary>
    public long TotalStreams { get; set; }
    
    /// <summary>
    /// Number of save operations performed
    /// </summary>
    public long SaveOperations { get; set; }
    
    /// <summary>
    /// Number of read operations performed
    /// </summary>
    public long ReadOperations { get; set; }
    
    /// <summary>
    /// Number of failed operations
    /// </summary>
    public long FailedOperations { get; set; }
    
    /// <summary>
    /// Average save latency in milliseconds
    /// </summary>
    public double AverageSaveLatency { get; set; }
    
    /// <summary>
    /// Average read latency in milliseconds
    /// </summary>
    public double AverageReadLatency { get; set; }
    
    /// <summary>
    /// Timestamp when statistics were last updated
    /// </summary>
    public DateTimeOffset LastUpdated { get; set; }
}