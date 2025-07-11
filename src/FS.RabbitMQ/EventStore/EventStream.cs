using FS.RabbitMQ.Events;

namespace FS.RabbitMQ.EventStore;

/// <summary>
/// Implementation of event stream that stores events for a specific aggregate
/// </summary>
public class EventStream : IEventStream
{
    private readonly ConcurrentList<EventSnapshot> _events = new();
    private readonly Dictionary<string, object> _metadata = new();
    private readonly object _lock = new();
    private bool _disposed = false;
    
    /// <summary>
    /// Unique identifier for the aggregate
    /// </summary>
    public string AggregateId { get; private set; }
    
    /// <summary>
    /// Type of the aggregate
    /// </summary>
    public string AggregateType { get; private set; }
    
    /// <summary>
    /// Current version of the aggregate
    /// </summary>
    public long Version { get; private set; }
    
    /// <summary>
    /// Timestamp when the stream was created
    /// </summary>
    public DateTimeOffset CreatedAt { get; private set; }
    
    /// <summary>
    /// Timestamp when the stream was last modified
    /// </summary>
    public DateTimeOffset LastModifiedAt { get; private set; }
    
    /// <summary>
    /// Total number of events in the stream
    /// </summary>
    public long EventCount => _events.Count;
    
    /// <summary>
    /// Creates a new event stream
    /// </summary>
    /// <param name="aggregateId">Aggregate identifier</param>
    /// <param name="aggregateType">Aggregate type</param>
    public EventStream(string aggregateId, string aggregateType)
    {
        AggregateId = aggregateId ?? throw new ArgumentNullException(nameof(aggregateId));
        AggregateType = aggregateType ?? throw new ArgumentNullException(nameof(aggregateType));
        Version = 0;
        CreatedAt = DateTimeOffset.UtcNow;
        LastModifiedAt = DateTimeOffset.UtcNow;
    }
    
    /// <summary>
    /// Creates an event stream with initial events
    /// </summary>
    /// <param name="aggregateId">Aggregate identifier</param>
    /// <param name="aggregateType">Aggregate type</param>
    /// <param name="initialEvents">Initial events</param>
    public EventStream(string aggregateId, string aggregateType, IEnumerable<EventSnapshot> initialEvents) 
        : this(aggregateId, aggregateType)
    {
        if (initialEvents != null)
        {
            foreach (var eventSnapshot in initialEvents)
            {
                _events.Add(eventSnapshot);
                Version = Math.Max(Version, eventSnapshot.Version);
            }
        }
    }
    
    /// <summary>
    /// Gets all events in the stream
    /// </summary>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Collection of event snapshots</returns>
    public Task<IEnumerable<EventSnapshot>> GetAllEventsAsync(CancellationToken cancellationToken = default)
    {
        if (_disposed)
            throw new ObjectDisposedException(nameof(EventStream));
        
        lock (_lock)
        {
            return Task.FromResult<IEnumerable<EventSnapshot>>(_events.ToList());
        }
    }
    
    /// <summary>
    /// Gets events from a specific version
    /// </summary>
    /// <param name="fromVersion">Starting version (inclusive)</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Collection of event snapshots</returns>
    public Task<IEnumerable<EventSnapshot>> GetEventsFromVersionAsync(long fromVersion, CancellationToken cancellationToken = default)
    {
        if (_disposed)
            throw new ObjectDisposedException(nameof(EventStream));
        
        lock (_lock)
        {
            var events = _events.Where(e => e.Version >= fromVersion).ToList();
            return Task.FromResult<IEnumerable<EventSnapshot>>(events);
        }
    }
    
    /// <summary>
    /// Gets events within a version range
    /// </summary>
    /// <param name="fromVersion">Starting version (inclusive)</param>
    /// <param name="toVersion">Ending version (inclusive)</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Collection of event snapshots</returns>
    public Task<IEnumerable<EventSnapshot>> GetEventsInRangeAsync(long fromVersion, long toVersion, CancellationToken cancellationToken = default)
    {
        if (_disposed)
            throw new ObjectDisposedException(nameof(EventStream));
        
        lock (_lock)
        {
            var events = _events.Where(e => e.Version >= fromVersion && e.Version <= toVersion).ToList();
            return Task.FromResult<IEnumerable<EventSnapshot>>(events);
        }
    }
    
    /// <summary>
    /// Gets events with pagination
    /// </summary>
    /// <param name="offset">Number of events to skip</param>
    /// <param name="limit">Maximum number of events to return</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Collection of event snapshots</returns>
    public Task<IEnumerable<EventSnapshot>> GetEventsPaginatedAsync(int offset, int limit, CancellationToken cancellationToken = default)
    {
        if (_disposed)
            throw new ObjectDisposedException(nameof(EventStream));
        
        lock (_lock)
        {
            var events = _events.Skip(offset).Take(limit).ToList();
            return Task.FromResult<IEnumerable<EventSnapshot>>(events);
        }
    }
    
    /// <summary>
    /// Appends new events to the stream
    /// </summary>
    /// <param name="events">Events to append</param>
    /// <param name="expectedVersion">Expected current version for concurrency control</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>New version after appending events</returns>
    public Task<long> AppendEventsAsync(IEnumerable<IEvent> events, long expectedVersion, CancellationToken cancellationToken = default)
    {
        if (_disposed)
            throw new ObjectDisposedException(nameof(EventStream));
        
        if (events == null)
            throw new ArgumentNullException(nameof(events));
        
        lock (_lock)
        {
            if (expectedVersion != -1 && Version != expectedVersion)
                throw new InvalidOperationException($"Expected version {expectedVersion} but current version is {Version}");
            
            var eventList = events.ToList();
            var sequenceNumber = _events.Count;
            
            foreach (var evt in eventList)
            {
                sequenceNumber++;
                Version++;
                
                var eventSnapshot = EventSnapshot.FromEvent(evt, "", AggregateId, AggregateType, Version, sequenceNumber);
                _events.Add(eventSnapshot);
            }
            
            LastModifiedAt = DateTimeOffset.UtcNow;
            return Task.FromResult(Version);
        }
    }
    
    /// <summary>
    /// Checks if the stream exists
    /// </summary>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>True if stream exists</returns>
    public Task<bool> ExistsAsync(CancellationToken cancellationToken = default)
    {
        return Task.FromResult(!_disposed && EventCount > 0);
    }
    
    /// <summary>
    /// Deletes the entire stream
    /// </summary>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Task representing the deletion operation</returns>
    public Task DeleteAsync(CancellationToken cancellationToken = default)
    {
        if (_disposed)
            throw new ObjectDisposedException(nameof(EventStream));
        
        lock (_lock)
        {
            _events.Clear();
            Version = 0;
            LastModifiedAt = DateTimeOffset.UtcNow;
        }
        
        return Task.CompletedTask;
    }
    
    /// <summary>
    /// Gets stream metadata
    /// </summary>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Stream metadata</returns>
    public Task<Dictionary<string, object>> GetMetadataAsync(CancellationToken cancellationToken = default)
    {
        if (_disposed)
            throw new ObjectDisposedException(nameof(EventStream));
        
        lock (_lock)
        {
            return Task.FromResult(new Dictionary<string, object>(_metadata));
        }
    }
    
    /// <summary>
    /// Sets stream metadata
    /// </summary>
    /// <param name="metadata">Metadata to set</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Task representing the operation</returns>
    public Task SetMetadataAsync(Dictionary<string, object> metadata, CancellationToken cancellationToken = default)
    {
        if (_disposed)
            throw new ObjectDisposedException(nameof(EventStream));
        
        if (metadata == null)
            throw new ArgumentNullException(nameof(metadata));
        
        lock (_lock)
        {
            _metadata.Clear();
            foreach (var kvp in metadata)
            {
                _metadata[kvp.Key] = kvp.Value;
            }
            LastModifiedAt = DateTimeOffset.UtcNow;
        }
        
        return Task.CompletedTask;
    }
    
    /// <summary>
    /// Releases all resources used by the EventStream
    /// </summary>
    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }
    
    /// <summary>
    /// Releases resources used by the EventStream
    /// </summary>
    /// <param name="disposing">Whether disposing is called from Dispose() method</param>
    protected virtual void Dispose(bool disposing)
    {
        if (!_disposed && disposing)
        {
            lock (_lock)
            {
                _events.Clear();
                _metadata.Clear();
                _disposed = true;
            }
        }
    }
}

/// <summary>
/// Thread-safe concurrent list implementation
/// </summary>
/// <typeparam name="T">Item type</typeparam>
internal class ConcurrentList<T> : IEnumerable<T>
{
    private readonly List<T> _list = new();
    private readonly object _lock = new();
    
    public int Count
    {
        get
        {
            lock (_lock)
            {
                return _list.Count;
            }
        }
    }
    
    public void Add(T item)
    {
        lock (_lock)
        {
            _list.Add(item);
        }
    }
    
    public void Clear()
    {
        lock (_lock)
        {
            _list.Clear();
        }
    }
    
    public IEnumerable<T> Where(Func<T, bool> predicate)
    {
        lock (_lock)
        {
            return _list.Where(predicate).ToList();
        }
    }
    
    public IEnumerable<T> Skip(int count)
    {
        lock (_lock)
        {
            return _list.Skip(count).ToList();
        }
    }
    
    public IEnumerable<T> Take(int count)
    {
        lock (_lock)
        {
            return _list.Take(count).ToList();
        }
    }
    
    public List<T> ToList()
    {
        lock (_lock)
        {
            return new List<T>(_list);
        }
    }
    
    public IEnumerator<T> GetEnumerator()
    {
        lock (_lock)
        {
            return _list.ToList().GetEnumerator();
        }
    }
    
    System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }
}