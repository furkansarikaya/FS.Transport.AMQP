using FS.StreamFlow.Core.Features.Events.Interfaces;
using FS.StreamFlow.Core.Features.Messaging.Interfaces;
using Microsoft.Extensions.Logging;

namespace FS.StreamFlow.RabbitMQ.Features.EventStore;

/// <summary>
/// RabbitMQ implementation of fluent event stream API
/// </summary>
public class RabbitMQFluentEventStreamApi : IFluentEventStreamApi
{
    private readonly IEventStore _eventStore;
    private readonly ILogger _logger;
    private readonly string _streamId;
    
    private List<object> _events = new();
    private long _expectedVersion = -1;
    private int _maxCount = int.MaxValue;
    private long _fromVersion = 0;
    private object? _snapshot;
    private long _snapshotVersion = -1;
    
    public RabbitMQFluentEventStreamApi(
        IEventStore eventStore,
        string streamId,
        ILogger logger)
    {
        _eventStore = eventStore ?? throw new ArgumentNullException(nameof(eventStore));
        _streamId = streamId ?? throw new ArgumentNullException(nameof(streamId));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }
    
    /// <summary>
    /// Appends events to the stream
    /// </summary>
    /// <param name="events">Events to append</param>
    /// <returns>Fluent event stream API for method chaining</returns>
    public IFluentEventStreamApi AppendEvents(IEnumerable<object> events)
    {
        if (events == null)
            throw new ArgumentNullException(nameof(events));
        
        _events.AddRange(events);
        return this;
    }
    
    /// <summary>
    /// Appends a single event to the stream
    /// </summary>
    /// <param name="event">Event to append</param>
    /// <returns>Fluent event stream API for method chaining</returns>
    public IFluentEventStreamApi AppendEvent(object @event)
    {
        if (@event == null)
            throw new ArgumentNullException(nameof(@event));
        
        _events.Add(@event);
        return this;
    }
    
    /// <summary>
    /// Appends events with expected version for concurrency control
    /// </summary>
    /// <param name="events">Events to append</param>
    /// <param name="expectedVersion">Expected version for concurrency control</param>
    /// <returns>Fluent event stream API for method chaining</returns>
    public IFluentEventStreamApi AppendEventsWithExpectedVersion(IEnumerable<object> events, long expectedVersion)
    {
        if (events == null)
            throw new ArgumentNullException(nameof(events));
        
        _events.AddRange(events);
        _expectedVersion = expectedVersion;
        return this;
    }
    
    /// <summary>
    /// Configures the maximum number of events to read
    /// </summary>
    /// <param name="maxCount">Maximum number of events to read</param>
    /// <returns>Fluent event stream API for method chaining</returns>
    public IFluentEventStreamApi WithMaxCount(int maxCount)
    {
        if (maxCount <= 0)
            throw new ArgumentException("Max count must be greater than 0", nameof(maxCount));
        
        _maxCount = maxCount;
        return this;
    }
    
    /// <summary>
    /// Configures the starting version for reading events
    /// </summary>
    /// <param name="fromVersion">Starting version (inclusive)</param>
    /// <returns>Fluent event stream API for method chaining</returns>
    public IFluentEventStreamApi FromVersion(long fromVersion)
    {
        if (fromVersion < 0)
            throw new ArgumentException("From version must be non-negative", nameof(fromVersion));
        
        _fromVersion = fromVersion;
        return this;
    }
    
    /// <summary>
    /// Configures snapshot settings for the stream
    /// </summary>
    /// <param name="snapshot">Snapshot data</param>
    /// <param name="version">Version at which the snapshot was taken</param>
    /// <returns>Fluent event stream API for method chaining</returns>
    public IFluentEventStreamApi WithSnapshot(object snapshot, long version)
    {
        if (snapshot == null)
            throw new ArgumentNullException(nameof(snapshot));
        
        if (version < 0)
            throw new ArgumentException("Version must be non-negative", nameof(version));
        
        _snapshot = snapshot;
        _snapshotVersion = version;
        return this;
    }
    
    /// <summary>
    /// Saves events to the stream with the configured settings
    /// </summary>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Task representing the save operation with new version</returns>
    public async Task<long> SaveAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogDebug("Saving {EventCount} events to stream {StreamId} with expected version {ExpectedVersion}",
                _events.Count, _streamId, _expectedVersion);
            
            var version = await _eventStore.SaveEventsAsync(_streamId, _events, _expectedVersion, cancellationToken);
            
            _logger.LogInformation("Successfully saved {EventCount} events to stream {StreamId}, new version: {Version}",
                _events.Count, _streamId, version);
            
            return version;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to save events to stream {StreamId}", _streamId);
            throw;
        }
    }
    
    /// <summary>
    /// Reads events from the stream with the configured settings
    /// </summary>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Task representing the read operation with events</returns>
    public async Task<IEnumerable<object>> ReadAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogDebug("Reading events from stream {StreamId} from version {FromVersion} with max count {MaxCount}",
                _streamId, _fromVersion, _maxCount);
            
            var events = await _eventStore.GetEventsAsync(_streamId, _fromVersion, cancellationToken);
            
            if (_maxCount != int.MaxValue)
            {
                events = events.Take(_maxCount);
            }
            
            var eventList = events.ToList();
            _logger.LogInformation("Successfully read {EventCount} events from stream {StreamId}",
                eventList.Count, _streamId);
            
            return eventList;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to read events from stream {StreamId}", _streamId);
            throw;
        }
    }
    
    /// <summary>
    /// Reads events from the stream in reverse order with the configured settings
    /// </summary>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Task representing the read operation with events in reverse order</returns>
    public async Task<IEnumerable<object>> ReadBackwardAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogDebug("Reading events backward from stream {StreamId} from version {FromVersion} with max count {MaxCount}",
                _streamId, _fromVersion, _maxCount);
            
            var stream = await _eventStore.GetStreamAsync(_streamId, cancellationToken);
            if (stream == null)
            {
                return Enumerable.Empty<object>();
            }
            
            var events = await stream.ReadEventsBackwardAsync(_fromVersion, _maxCount, cancellationToken);
            
            var eventList = events.ToList();
            _logger.LogInformation("Successfully read {EventCount} events backward from stream {StreamId}",
                eventList.Count, _streamId);
            
            return eventList;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to read events backward from stream {StreamId}", _streamId);
            throw;
        }
    }
    
    /// <summary>
    /// Creates or gets the event stream
    /// </summary>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Task representing the stream creation/retrieval operation</returns>
    public async Task<bool> CreateAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogDebug("Creating stream {StreamId}", _streamId);
            
            var stream = await _eventStore.CreateStreamAsync(_streamId, cancellationToken);
            
            _logger.LogInformation("Successfully created stream {StreamId}", _streamId);
            return stream != null;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to create stream {StreamId}", _streamId);
            throw;
        }
    }
    
    /// <summary>
    /// Deletes the event stream
    /// </summary>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Task representing the delete operation</returns>
    public async Task<bool> DeleteAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogDebug("Deleting stream {StreamId}", _streamId);
            
            await _eventStore.DeleteStreamAsync(_streamId, cancellationToken);
            
            _logger.LogInformation("Successfully deleted stream {StreamId}", _streamId);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to delete stream {StreamId}", _streamId);
            throw;
        }
    }
    
    /// <summary>
    /// Truncates the stream at the specified version
    /// </summary>
    /// <param name="version">Version to truncate at</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Task representing the truncation operation</returns>
    public async Task<bool> TruncateAsync(long version, CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogDebug("Truncating stream {StreamId} at version {Version}", _streamId, version);
            
            var stream = await _eventStore.GetStreamAsync(_streamId, cancellationToken);
            if (stream == null)
            {
                return false;
            }
            
            await stream.TruncateAsync(version, cancellationToken);
            
            _logger.LogInformation("Successfully truncated stream {StreamId} at version {Version}", _streamId, version);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to truncate stream {StreamId} at version {Version}", _streamId, version);
            throw;
        }
    }
    
    /// <summary>
    /// Gets the current version of the stream
    /// </summary>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Task representing the version retrieval operation</returns>
    public async Task<long> GetVersionAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogDebug("Getting version for stream {StreamId}", _streamId);
            
            var version = await _eventStore.GetStreamVersionAsync(_streamId, cancellationToken);
            
            _logger.LogDebug("Stream {StreamId} version: {Version}", _streamId, version);
            return version;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get version for stream {StreamId}", _streamId);
            throw;
        }
    }
    
    /// <summary>
    /// Checks if the stream exists
    /// </summary>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Task representing the existence check operation</returns>
    public async Task<bool> ExistsAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogDebug("Checking if stream {StreamId} exists", _streamId);
            
            var exists = await _eventStore.StreamExistsAsync(_streamId, cancellationToken);
            
            _logger.LogDebug("Stream {StreamId} exists: {Exists}", _streamId, exists);
            return exists;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to check if stream {StreamId} exists", _streamId);
            throw;
        }
    }
    
    /// <summary>
    /// Saves snapshot to the stream
    /// </summary>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Task representing the snapshot save operation</returns>
    public async Task<bool> SaveSnapshotAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            if (_snapshot == null)
            {
                throw new InvalidOperationException("Snapshot not configured. Use WithSnapshot() before calling SaveSnapshotAsync().");
            }
            
            _logger.LogDebug("Saving snapshot for stream {StreamId} at version {Version}", _streamId, _snapshotVersion);
            
            await _eventStore.SaveSnapshotAsync(_streamId, _snapshot, _snapshotVersion, cancellationToken);
            
            _logger.LogInformation("Successfully saved snapshot for stream {StreamId} at version {Version}", _streamId, _snapshotVersion);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to save snapshot for stream {StreamId} at version {Version}", _streamId, _snapshotVersion);
            throw;
        }
    }
    
    /// <summary>
    /// Gets the latest snapshot for the stream
    /// </summary>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Task representing the snapshot retrieval operation</returns>
    public async Task<object?> GetSnapshotAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogDebug("Getting snapshot for stream {StreamId}", _streamId);
            
            var snapshot = await _eventStore.GetSnapshotAsync(_streamId, cancellationToken);
            
            _logger.LogDebug("Got snapshot for stream {StreamId}: {HasSnapshot}", _streamId, snapshot != null);
            return snapshot?.Data;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get snapshot for stream {StreamId}", _streamId);
            throw;
        }
    }
} 