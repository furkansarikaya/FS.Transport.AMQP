using FS.Mediator.Features.RequestHandling.Core;
using System.Collections.Concurrent;

namespace FS.Transport.AMQP.EventStore;

/// <summary>
/// Event store implementation using CQRS pattern with FS.Mediator
/// </summary>
public class EventStore : IEventStore
{
    private readonly IMediator _mediator;
    private readonly ConcurrentDictionary<string, IEventStream> _streams = new();
    private readonly EventStoreStatistics _statistics = new();
    private EventStoreStatus _status = EventStoreStatus.NotInitialized;
    private bool _disposed = false;
    
    /// <summary>
    /// Gets the current status of the event store
    /// </summary>
    public EventStoreStatus Status => _status;
    
    /// <summary>
    /// Gets event store statistics and metrics
    /// </summary>
    public EventStoreStatistics Statistics => _statistics;
    
    /// <summary>
    /// Creates a new EventStore instance
    /// </summary>
    /// <param name="mediator">Mediator for CQRS operations</param>
    public EventStore(IMediator mediator)
    {
        _mediator = mediator ?? throw new ArgumentNullException(nameof(mediator));
        _statistics.LastUpdated = DateTimeOffset.UtcNow;
    }
    
    /// <summary>
    /// Initializes the event store
    /// </summary>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Task representing the initialization operation</returns>
    public Task InitializeAsync(CancellationToken cancellationToken = default)
    {
        if (_disposed)
            throw new ObjectDisposedException(nameof(EventStore));
        
        _status = EventStoreStatus.Initializing;
        
        try
        {
            // Initialize event store components here
            _status = EventStoreStatus.Running;
            _statistics.LastUpdated = DateTimeOffset.UtcNow;
            
            return Task.CompletedTask;
        }
        catch (Exception)
        {
            _status = EventStoreStatus.Faulted;
            throw;
        }
    }
    
    /// <summary>
    /// Saves events to a stream
    /// </summary>
    /// <param name="streamId">Stream identifier</param>
    /// <param name="events">Events to save</param>
    /// <param name="expectedVersion">Expected version for concurrency control</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>New version after saving events</returns>
    public async Task<long> SaveEventsAsync(string streamId, IEnumerable<object> events, long expectedVersion, CancellationToken cancellationToken = default)
    {
        if (_disposed)
            throw new ObjectDisposedException(nameof(EventStore));
        
        if (string.IsNullOrEmpty(streamId))
            throw new ArgumentException("Stream ID cannot be null or empty", nameof(streamId));
        
        if (events == null)
            throw new ArgumentNullException(nameof(events));
        
        try
        {
            _statistics.SaveOperations++;
            var startTime = DateTimeOffset.UtcNow;
            
            // Extract aggregate info from stream ID (assuming format: "AggregateType-AggregateId")
            var parts = streamId.Split('-', 2);
            var aggregateType = parts.Length > 1 ? parts[0] : "Unknown";
            var aggregateId = parts.Length > 1 ? parts[1] : streamId;
            
            // Convert objects to IEvent (this would need proper implementation)
            var eventList = events.OfType<Events.IEvent>().ToList();
            
            var command = SaveEventsCommand.Create(aggregateId, aggregateType, eventList, expectedVersion);
            var result = await _mediator.SendAsync(command, cancellationToken).ConfigureAwait(false);
            
            if (result.Success)
            {
                _statistics.TotalEvents += result.EventsSaved;
                var duration = DateTimeOffset.UtcNow - startTime;
                _statistics.AverageSaveLatency = (_statistics.AverageSaveLatency + duration.TotalMilliseconds) / 2;
                
                return result.NewVersion;
            }
            else
            {
                _statistics.FailedOperations++;
                throw new InvalidOperationException(result.ErrorMessage ?? "Save operation failed");
            }
        }
        catch (Exception)
        {
            _statistics.FailedOperations++;
            throw;
        }
        finally
        {
            _statistics.LastUpdated = DateTimeOffset.UtcNow;
        }
    }
    
    /// <summary>
    /// Gets events from a stream
    /// </summary>
    /// <param name="streamId">Stream identifier</param>
    /// <param name="fromVersion">Starting version (inclusive)</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Collection of events</returns>
    public async Task<IEnumerable<object>> GetEventsAsync(string streamId, long fromVersion = 0, CancellationToken cancellationToken = default)
    {
        if (_disposed)
            throw new ObjectDisposedException(nameof(EventStore));
        
        if (string.IsNullOrEmpty(streamId))
            throw new ArgumentException("Stream ID cannot be null or empty", nameof(streamId));
        
        try
        {
            _statistics.ReadOperations++;
            var startTime = DateTimeOffset.UtcNow;
            
            // Extract aggregate info from stream ID
            var parts = streamId.Split('-', 2);
            var aggregateType = parts.Length > 1 ? parts[0] : "Unknown";
            var aggregateId = parts.Length > 1 ? parts[1] : streamId;
            
            var query = GetEventsQuery.AllEvents(aggregateId, aggregateType);
            query.FromVersion = fromVersion;
            
            var result = await _mediator.SendAsync(query, cancellationToken).ConfigureAwait(false);
            
            if (result.Success)
            {
                var duration = DateTimeOffset.UtcNow - startTime;
                _statistics.AverageReadLatency = (_statistics.AverageReadLatency + duration.TotalMilliseconds) / 2;
                
                return result.Events.Cast<object>();
            }
            else
            {
                _statistics.FailedOperations++;
                throw new InvalidOperationException(result.ErrorMessage ?? "Get events operation failed");
            }
        }
        catch (Exception)
        {
            _statistics.FailedOperations++;
            throw;
        }
        finally
        {
            _statistics.LastUpdated = DateTimeOffset.UtcNow;
        }
    }
    
    /// <summary>
    /// Gets an event stream
    /// </summary>
    /// <param name="streamId">Stream identifier</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Event stream</returns>
    public async Task<IEventStream?> GetStreamAsync(string streamId, CancellationToken cancellationToken = default)
    {
        if (_disposed)
            throw new ObjectDisposedException(nameof(EventStore));
        
        if (string.IsNullOrEmpty(streamId))
            throw new ArgumentException("Stream ID cannot be null or empty", nameof(streamId));
        
        try
        {
            _statistics.ReadOperations++;
            
            // Check cache first
            if (_streams.TryGetValue(streamId, out var cachedStream))
            {
                return cachedStream;
            }
            
            // Extract aggregate info from stream ID
            var parts = streamId.Split('-', 2);
            var aggregateType = parts.Length > 1 ? parts[0] : "Unknown";
            var aggregateId = parts.Length > 1 ? parts[1] : streamId;
            
            var query = GetEventStreamQuery.Create(aggregateId, aggregateType);
            var result = await _mediator.SendAsync(query, cancellationToken).ConfigureAwait(false);
            
            if (result.Success && result.StreamExists)
            {
                _streams.TryAdd(streamId, result.EventStream!);
                return result.EventStream;
            }
            
            return null;
        }
        catch (Exception)
        {
            _statistics.FailedOperations++;
            throw;
        }
        finally
        {
            _statistics.LastUpdated = DateTimeOffset.UtcNow;
        }
    }
    
    /// <summary>
    /// Checks if a stream exists
    /// </summary>
    /// <param name="streamId">Stream identifier</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>True if stream exists</returns>
    public async Task<bool> StreamExistsAsync(string streamId, CancellationToken cancellationToken = default)
    {
        if (_disposed)
            throw new ObjectDisposedException(nameof(EventStore));
        
        if (string.IsNullOrEmpty(streamId))
            throw new ArgumentException("Stream ID cannot be null or empty", nameof(streamId));
        
        try
        {
            _statistics.ReadOperations++;
            
            // Check cache first
            if (_streams.ContainsKey(streamId))
            {
                return true;
            }
            
            // Extract aggregate info from stream ID
            var parts = streamId.Split('-', 2);
            var aggregateType = parts.Length > 1 ? parts[0] : "Unknown";
            var aggregateId = parts.Length > 1 ? parts[1] : streamId;
            
            var query = StreamExistsQuery.Create(aggregateId, aggregateType);
            var result = await _mediator.SendAsync(query, cancellationToken).ConfigureAwait(false);
            
            return result.Success && result.Exists;
        }
        catch (Exception)
        {
            _statistics.FailedOperations++;
            throw;
        }
        finally
        {
            _statistics.LastUpdated = DateTimeOffset.UtcNow;
        }
    }
    
    /// <summary>
    /// Deletes a stream
    /// </summary>
    /// <param name="streamId">Stream identifier</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Task representing the deletion operation</returns>
    public async Task DeleteStreamAsync(string streamId, CancellationToken cancellationToken = default)
    {
        if (_disposed)
            throw new ObjectDisposedException(nameof(EventStore));
        
        if (string.IsNullOrEmpty(streamId))
            throw new ArgumentException("Stream ID cannot be null or empty", nameof(streamId));
        
        try
        {
            // Extract aggregate info from stream ID
            var parts = streamId.Split('-', 2);
            var aggregateType = parts.Length > 1 ? parts[0] : "Unknown";
            var aggregateId = parts.Length > 1 ? parts[1] : streamId;
            
            var command = DeleteEventsCommand.DeleteStream(aggregateId, aggregateType);
            var result = await _mediator.SendAsync(command, cancellationToken).ConfigureAwait(false);
            
            if (result.Success)
            {
                _streams.TryRemove(streamId, out _);
                if (_statistics.TotalStreams > 0)
                {
                    _statistics.TotalStreams--;
                }
            }
            else
            {
                _statistics.FailedOperations++;
                throw new InvalidOperationException(result.ErrorMessage ?? "Delete operation failed");
            }
        }
        catch (Exception)
        {
            _statistics.FailedOperations++;
            throw;
        }
        finally
        {
            _statistics.LastUpdated = DateTimeOffset.UtcNow;
        }
    }
    
    /// <summary>
    /// Releases all resources used by the EventStore
    /// </summary>
    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }
    
    /// <summary>
    /// Releases resources used by the EventStore
    /// </summary>
    /// <param name="disposing">Whether disposing is called from Dispose() method</param>
    protected virtual void Dispose(bool disposing)
    {
        if (!_disposed && disposing)
        {
            _status = EventStoreStatus.Stopping;
            
            foreach (var stream in _streams.Values)
            {
                stream?.Dispose();
            }
            
            _streams.Clear();
            _status = EventStoreStatus.Stopped;
            _disposed = true;
        }
    }
}