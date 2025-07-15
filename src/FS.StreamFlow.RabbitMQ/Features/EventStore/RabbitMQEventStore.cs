using FS.StreamFlow.Core.Features.Events.Interfaces;
using FS.StreamFlow.Core.Features.Events.Models;
using FS.StreamFlow.Core.Features.Messaging.Interfaces;
using FS.StreamFlow.RabbitMQ.Features.Connection;
using Microsoft.Extensions.Logging;
using System.Collections.Concurrent;

namespace FS.StreamFlow.RabbitMQ.Features.EventStore;

/// <summary>
/// RabbitMQ implementation of event store providing CQRS pattern implementation.
/// Supports event persistence, stream management, snapshots, and event sourcing operations.
/// </summary>
public class RabbitMQEventStore : IEventStore
{
    private readonly IConnectionManager _connectionManager;
    private readonly ILogger<RabbitMQEventStore> _logger;
    private readonly ConcurrentDictionary<string, RabbitMQEventStream> _eventStreams;
    private readonly ConcurrentDictionary<string, EventSnapshot> _snapshots;
    private readonly EventStoreStatistics _statistics;
    private readonly Timer _statisticsTimer;
    private readonly DateTimeOffset _startTime;
    private volatile bool _disposed;
    private volatile EventStoreStatus _status;

    /// <summary>
    /// Gets the current status of the event store
    /// </summary>
    public EventStoreStatus Status => _status;

    /// <summary>
    /// Gets event store statistics and metrics
    /// </summary>
    public EventStoreStatistics Statistics => _statistics;

    /// <summary>
    /// Initializes a new instance of the RabbitMQEventStore class.
    /// </summary>
    /// <param name="connectionManager">Connection manager for RabbitMQ connections</param>
    /// <param name="logger">Logger instance for diagnostics</param>
    public RabbitMQEventStore(
        IConnectionManager connectionManager,
        ILogger<RabbitMQEventStore> logger)
    {
        _connectionManager = connectionManager ?? throw new ArgumentNullException(nameof(connectionManager));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        
        _eventStreams = new ConcurrentDictionary<string, RabbitMQEventStream>();
        _snapshots = new ConcurrentDictionary<string, EventSnapshot>();
        _statistics = new EventStoreStatistics();
        _startTime = DateTimeOffset.UtcNow;
        _status = EventStoreStatus.NotInitialized;
        
        // Initialize statistics timer
        _statisticsTimer = new Timer(UpdateStatistics, null, TimeSpan.FromSeconds(30), TimeSpan.FromSeconds(30));
        
        _logger.LogInformation("RabbitMQ Event Store initialized");
    }

    /// <summary>
    /// Initializes the event store
    /// </summary>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Task representing the initialization operation</returns>
    public async Task InitializeAsync(CancellationToken cancellationToken = default)
    {
        if (_disposed)
            throw new ObjectDisposedException(nameof(RabbitMQEventStore));

        try
        {
            _status = EventStoreStatus.Initializing;
            _logger.LogInformation("Initializing RabbitMQ Event Store...");

            // Ensure connection is established
            await _connectionManager.ConnectAsync(cancellationToken);

            // Initialize event store exchanges and queues
            await InitializeInfrastructureAsync(cancellationToken);

            _status = EventStoreStatus.Ready;
            _logger.LogInformation("RabbitMQ Event Store initialized successfully");
        }
        catch (Exception ex)
        {
            _status = EventStoreStatus.Error;
            _logger.LogError(ex, "Failed to initialize RabbitMQ Event Store");
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
        if (string.IsNullOrWhiteSpace(streamId))
            throw new ArgumentException("Stream ID cannot be null or empty", nameof(streamId));

        if (events == null)
            throw new ArgumentNullException(nameof(events));

        if (_disposed)
            throw new ObjectDisposedException(nameof(RabbitMQEventStore));

        if (_status != EventStoreStatus.Ready)
            throw new InvalidOperationException($"Event store is not ready. Current status: {_status}");

        try
        {
            var eventList = events.ToList();
            if (!eventList.Any())
                return expectedVersion;

            // Get or create stream
            var stream = await GetOrCreateStreamAsync(streamId, cancellationToken);

            // Validate expected version
            if (expectedVersion >= 0 && stream.Version != expectedVersion)
            {
                throw new InvalidOperationException($"Version mismatch. Expected: {expectedVersion}, Actual: {stream.Version}");
            }

            // Save events to stream
            var newVersion = await stream.AppendEventsAsync(eventList, expectedVersion, cancellationToken);

            // Update statistics
            _statistics.TotalEvents += eventList.Count;
            _statistics.EventsWritten += eventList.Count;
            _statistics.LastEventWriteAt = DateTimeOffset.UtcNow;

            _logger.LogInformation("Saved {EventCount} events to stream {StreamId}. New version: {Version}", 
                eventList.Count, streamId, newVersion);

            return newVersion;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to save events to stream {StreamId}", streamId);
            throw;
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
        if (string.IsNullOrWhiteSpace(streamId))
            throw new ArgumentException("Stream ID cannot be null or empty", nameof(streamId));

        if (_disposed)
            throw new ObjectDisposedException(nameof(RabbitMQEventStore));

        if (_status != EventStoreStatus.Ready)
            throw new InvalidOperationException($"Event store is not ready. Current status: {_status}");

        try
        {
            var stream = await GetStreamAsync(streamId, cancellationToken);
            if (stream == null)
            {
                _logger.LogWarning("Stream not found: {StreamId}", streamId);
                return Enumerable.Empty<object>();
            }

            var events = await stream.ReadEventsAsync(fromVersion, cancellationToken: cancellationToken);
            
            // Update statistics
            _statistics.EventsRead += events.Count();
            _statistics.LastEventReadAt = DateTimeOffset.UtcNow;

            return events;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get events from stream {StreamId}", streamId);
            throw;
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
        if (string.IsNullOrWhiteSpace(streamId))
            throw new ArgumentException("Stream ID cannot be null or empty", nameof(streamId));

        if (_disposed)
            throw new ObjectDisposedException(nameof(RabbitMQEventStore));

        await Task.CompletedTask; // Make async for interface compliance
        
        _eventStreams.TryGetValue(streamId, out var stream);
        return stream;
    }

    /// <summary>
    /// Creates a new event stream
    /// </summary>
    /// <param name="streamId">Stream identifier</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Event stream</returns>
    public async Task<IEventStream> CreateStreamAsync(string streamId, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(streamId))
            throw new ArgumentException("Stream ID cannot be null or empty", nameof(streamId));

        if (_disposed)
            throw new ObjectDisposedException(nameof(RabbitMQEventStore));

        if (_status != EventStoreStatus.Ready)
            throw new InvalidOperationException($"Event store is not ready. Current status: {_status}");

        try
        {
            var stream = new RabbitMQEventStream(streamId, _connectionManager, _logger);
            await stream.InitializeAsync(cancellationToken);

            _eventStreams.TryAdd(streamId, stream);
            _statistics.TotalStreams++;
            _statistics.ActiveStreams++;

            _logger.LogInformation("Created new event stream: {StreamId}", streamId);
            return stream;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to create event stream: {StreamId}", streamId);
            throw;
        }
    }

    /// <summary>
    /// Deletes an event stream
    /// </summary>
    /// <param name="streamId">Stream identifier</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Task representing the deletion operation</returns>
    public async Task DeleteStreamAsync(string streamId, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(streamId))
            throw new ArgumentException("Stream ID cannot be null or empty", nameof(streamId));

        if (_disposed)
            throw new ObjectDisposedException(nameof(RabbitMQEventStore));

        try
        {
            if (_eventStreams.TryRemove(streamId, out var stream))
            {
                stream.Dispose();
                _statistics.ActiveStreams--;
                
                // Remove associated snapshot
                _snapshots.TryRemove(streamId, out _);
                
                _logger.LogInformation("Deleted event stream: {StreamId}", streamId);
            }
            else
            {
                _logger.LogWarning("Stream not found for deletion: {StreamId}", streamId);
            }

            await Task.CompletedTask;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to delete event stream: {StreamId}", streamId);
            throw;
        }
    }

    /// <summary>
    /// Gets all stream identifiers
    /// </summary>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Collection of stream identifiers</returns>
    public async Task<IEnumerable<string>> GetStreamIdsAsync(CancellationToken cancellationToken = default)
    {
        if (_disposed)
            throw new ObjectDisposedException(nameof(RabbitMQEventStore));

        await Task.CompletedTask; // Make async for interface compliance
        return _eventStreams.Keys.ToList();
    }

    /// <summary>
    /// Creates a snapshot of an aggregate
    /// </summary>
    /// <param name="streamId">Stream identifier</param>
    /// <param name="snapshot">Snapshot data</param>
    /// <param name="version">Version at which the snapshot was taken</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Task representing the snapshot operation</returns>
    public async Task SaveSnapshotAsync(string streamId, object snapshot, long version, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(streamId))
            throw new ArgumentException("Stream ID cannot be null or empty", nameof(streamId));

        if (snapshot == null)
            throw new ArgumentNullException(nameof(snapshot));

        if (_disposed)
            throw new ObjectDisposedException(nameof(RabbitMQEventStore));

        try
        {
            var eventSnapshot = new EventSnapshot(
                streamId,
                snapshot,
                version,
                snapshot.GetType().Name);

            _snapshots.AddOrUpdate(streamId, eventSnapshot, (key, existing) => eventSnapshot);
            _statistics.TotalSnapshots++;

            _logger.LogInformation("Saved snapshot for stream {StreamId} at version {Version}", streamId, version);
            await Task.CompletedTask;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to save snapshot for stream {StreamId}", streamId);
            throw;
        }
    }

    /// <summary>
    /// Gets the latest snapshot for an aggregate
    /// </summary>
    /// <param name="streamId">Stream identifier</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Snapshot data or null if no snapshot exists</returns>
    public async Task<EventSnapshot?> GetSnapshotAsync(string streamId, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(streamId))
            throw new ArgumentException("Stream ID cannot be null or empty", nameof(streamId));

        if (_disposed)
            throw new ObjectDisposedException(nameof(RabbitMQEventStore));

        await Task.CompletedTask; // Make async for interface compliance
        
        _snapshots.TryGetValue(streamId, out var snapshot);
        return snapshot;
    }

    /// <summary>
    /// Checks if a stream exists
    /// </summary>
    /// <param name="streamId">Stream identifier</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>True if the stream exists, otherwise false</returns>
    public async Task<bool> StreamExistsAsync(string streamId, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(streamId))
            throw new ArgumentException("Stream ID cannot be null or empty", nameof(streamId));

        if (_disposed)
            throw new ObjectDisposedException(nameof(RabbitMQEventStore));

        await Task.CompletedTask; // Make async for interface compliance
        return _eventStreams.ContainsKey(streamId);
    }

    /// <summary>
    /// Gets the version of a stream
    /// </summary>
    /// <param name="streamId">Stream identifier</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Current version of the stream</returns>
    public async Task<long> GetStreamVersionAsync(string streamId, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(streamId))
            throw new ArgumentException("Stream ID cannot be null or empty", nameof(streamId));

        if (_disposed)
            throw new ObjectDisposedException(nameof(RabbitMQEventStore));

        await Task.CompletedTask; // Make async for interface compliance
        
        if (_eventStreams.TryGetValue(streamId, out var stream))
        {
            return stream.Version;
        }

        return -1; // Stream not found
    }

    /// <summary>
    /// Gets or creates a stream
    /// </summary>
    /// <param name="streamId">Stream identifier</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Event stream</returns>
    private async Task<RabbitMQEventStream> GetOrCreateStreamAsync(string streamId, CancellationToken cancellationToken = default)
    {
        if (_eventStreams.TryGetValue(streamId, out var existingStream))
        {
            return existingStream;
        }

        var stream = new RabbitMQEventStream(streamId, _connectionManager, _logger);
        await stream.InitializeAsync(cancellationToken);

        _eventStreams.TryAdd(streamId, stream);
        _statistics.TotalStreams++;
        _statistics.ActiveStreams++;

        return stream;
    }

    /// <summary>
    /// Initializes the event store infrastructure
    /// </summary>
    /// <param name="cancellationToken">Cancellation token</param>
    private async Task InitializeInfrastructureAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            var channel = await _connectionManager.GetChannelAsync(cancellationToken);
            if (channel == null)
            {
                throw new InvalidOperationException("Failed to get channel for event store initialization");
            }

            try
            {
                var rabbitChannel = ((RabbitMQChannel)channel).GetNativeChannel();

                // Declare event store exchange
                await rabbitChannel.ExchangeDeclareAsync(
                    exchange: "eventstore",
                    type: "topic",
                    durable: true,
                    autoDelete: false,
                    arguments: null,
                    cancellationToken: cancellationToken);

                // Declare snapshot exchange
                await rabbitChannel.ExchangeDeclareAsync(
                    exchange: "snapshots",
                    type: "direct",
                    durable: true,
                    autoDelete: false,
                    arguments: null,
                    cancellationToken: cancellationToken);

                _logger.LogInformation("Event store infrastructure initialized successfully");
            }
            finally
            {
                await _connectionManager.ReturnChannelAsync(channel, cancellationToken);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to initialize event store infrastructure");
            throw;
        }
    }

    /// <summary>
    /// Updates event store statistics
    /// </summary>
    /// <param name="state">Timer state</param>
    private void UpdateStatistics(object? state)
    {
        if (_disposed)
            return;

        try
        {
            _statistics.Uptime = DateTimeOffset.UtcNow - _startTime;
            _statistics.TotalStreams = _eventStreams.Count;
            _statistics.ActiveStreams = _eventStreams.Count(kvp => kvp.Value.Status == EventStreamStatus.Active);
            _statistics.TotalSnapshots = _snapshots.Count;
            _statistics.Timestamp = DateTimeOffset.UtcNow;

            // Calculate rates
            var uptimeSeconds = _statistics.Uptime.TotalSeconds;
            if (uptimeSeconds > 0)
            {
                _statistics.EventsPerSecond = _statistics.EventsWritten / uptimeSeconds;
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to update event store statistics");
        }
    }

    /// <summary>
    /// Gets a fluent API for event stream operations
    /// </summary>
    /// <param name="streamId">Stream identifier</param>
    /// <returns>Fluent event stream API</returns>
    public IFluentEventStreamApi Stream(string streamId)
    {
        if (_disposed)
            throw new ObjectDisposedException(nameof(RabbitMQEventStore));
        
        if (string.IsNullOrEmpty(streamId))
            throw new ArgumentException("Stream ID cannot be null or empty", nameof(streamId));
        
        _logger.LogDebug("Creating fluent event stream API for stream {StreamId}", streamId);
        
        return new RabbitMQFluentEventStreamApi(this, streamId, _logger);
    }

    /// <summary>
    /// Disposes the event store and releases all resources
    /// </summary>
    public void Dispose()
    {
        if (_disposed)
            return;

        _disposed = true;
        _status = EventStoreStatus.ShuttingDown;

        try
        {
            _statisticsTimer?.Dispose();
            
            // Dispose all event streams
            foreach (var stream in _eventStreams.Values)
            {
                stream.Dispose();
            }
            _eventStreams.Clear();
            
            _snapshots.Clear();
            _status = EventStoreStatus.ShutDown;
            
            _logger.LogInformation("RabbitMQ Event Store disposed successfully");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while disposing RabbitMQ Event Store");
        }
    }
}

/// <summary>
/// RabbitMQ implementation of an event stream
/// </summary>
public class RabbitMQEventStream : IEventStream
{
    private readonly string _streamId;
    private readonly IConnectionManager _connectionManager;
    private readonly ILogger _logger;
    private readonly ConcurrentQueue<StoredEvent> _events;
    private long _version;
    private volatile EventStreamStatus _status;
    private volatile bool _disposed;

    /// <summary>
    /// Stream identifier
    /// </summary>
    public string StreamId => _streamId;

    /// <summary>
    /// Current version of the stream
    /// </summary>
    public long Version => _version;

    /// <summary>
    /// Gets the current status of the stream
    /// </summary>
    public EventStreamStatus Status => _status;

    /// <summary>
    /// Initializes a new instance of the RabbitMQEventStream class
    /// </summary>
    /// <param name="streamId">Stream identifier</param>
    /// <param name="connectionManager">Connection manager</param>
    /// <param name="logger">Logger instance</param>
    public RabbitMQEventStream(string streamId, IConnectionManager connectionManager, ILogger logger)
    {
        _streamId = streamId ?? throw new ArgumentNullException(nameof(streamId));
        _connectionManager = connectionManager ?? throw new ArgumentNullException(nameof(connectionManager));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        
        _events = new ConcurrentQueue<StoredEvent>();
        _version = -1;
        _status = EventStreamStatus.Creating;
    }

    /// <summary>
    /// Initializes the event stream
    /// </summary>
    /// <param name="cancellationToken">Cancellation token</param>
    public async Task InitializeAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            // Initialize stream-specific infrastructure if needed
            _status = EventStreamStatus.Active;
            _logger.LogInformation("Event stream initialized: {StreamId}", _streamId);
            await Task.CompletedTask;
        }
        catch (Exception ex)
        {
            _status = EventStreamStatus.Error;
            _logger.LogError(ex, "Failed to initialize event stream: {StreamId}", _streamId);
            throw;
        }
    }

    /// <summary>
    /// Appends events to the stream
    /// </summary>
    /// <param name="events">Events to append</param>
    /// <param name="expectedVersion">Expected version for concurrency control</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>New version after appending events</returns>
    public async Task<long> AppendEventsAsync(IEnumerable<object> events, long expectedVersion, CancellationToken cancellationToken = default)
    {
        if (_disposed)
            throw new ObjectDisposedException(nameof(RabbitMQEventStream));

        if (_status != EventStreamStatus.Active)
            throw new InvalidOperationException($"Stream is not active. Status: {_status}");

        try
        {
            var eventList = events.ToList();
            var currentVersion = _version;
            
            // Validate expected version
            if (expectedVersion >= 0 && currentVersion != expectedVersion)
            {
                throw new InvalidOperationException($"Version mismatch. Expected: {expectedVersion}, Actual: {currentVersion}");
            }

            // Append events
            foreach (var eventData in eventList)
            {
                var storedEvent = new StoredEvent(
                    _streamId,
                    eventData.GetType().Name,
                    eventData,
                    ++currentVersion);

                _events.Enqueue(storedEvent);
            }

            _version = currentVersion;
            
            _logger.LogDebug("Appended {EventCount} events to stream {StreamId}. New version: {Version}", 
                eventList.Count, _streamId, _version);

            await Task.CompletedTask;
            return _version;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to append events to stream {StreamId}", _streamId);
            throw;
        }
    }

    /// <summary>
    /// Reads events from the stream
    /// </summary>
    /// <param name="fromVersion">Starting version (inclusive)</param>
    /// <param name="maxCount">Maximum number of events to read</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Collection of events</returns>
    public async Task<IEnumerable<object>> ReadEventsAsync(long fromVersion = 0, int maxCount = int.MaxValue, CancellationToken cancellationToken = default)
    {
        if (_disposed)
            throw new ObjectDisposedException(nameof(RabbitMQEventStream));

        await Task.CompletedTask; // Make async for interface compliance
        
        return _events
            .Where(e => e.Version >= fromVersion)
            .Take(maxCount)
            .Select(e => e.Data)
            .ToList();
    }

    /// <summary>
    /// Reads events from the stream in reverse order
    /// </summary>
    /// <param name="fromVersion">Starting version (inclusive)</param>
    /// <param name="maxCount">Maximum number of events to read</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Collection of events in reverse order</returns>
    public async Task<IEnumerable<object>> ReadEventsBackwardAsync(long fromVersion = -1, int maxCount = int.MaxValue, CancellationToken cancellationToken = default)
    {
        if (_disposed)
            throw new ObjectDisposedException(nameof(RabbitMQEventStream));

        await Task.CompletedTask; // Make async for interface compliance
        
        var events = _events.ToList();
        var startVersion = fromVersion == -1 ? _version : fromVersion;
        
        return events
            .Where(e => e.Version <= startVersion)
            .OrderByDescending(e => e.Version)
            .Take(maxCount)
            .Select(e => e.Data)
            .ToList();
    }

    /// <summary>
    /// Truncates the stream at the specified version
    /// </summary>
    /// <param name="version">Version to truncate at</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Task representing the truncation operation</returns>
    public async Task TruncateAsync(long version, CancellationToken cancellationToken = default)
    {
        if (_disposed)
            throw new ObjectDisposedException(nameof(RabbitMQEventStream));

        try
        {
            var newEvents = new ConcurrentQueue<StoredEvent>();
            
            // Keep only events up to the specified version
            foreach (var storedEvent in _events.Where(e => e.Version <= version))
            {
                newEvents.Enqueue(storedEvent);
            }

            // Replace the events queue
            _events.Clear();
            foreach (var e in newEvents)
            {
                _events.Enqueue(e);
            }

            _version = version;
            
            _logger.LogInformation("Truncated stream {StreamId} at version {Version}", _streamId, version);
            await Task.CompletedTask;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to truncate stream {StreamId} at version {Version}", _streamId, version);
            throw;
        }
    }

    /// <summary>
    /// Disposes the event stream
    /// </summary>
    public void Dispose()
    {
        if (_disposed)
            return;

        _disposed = true;
        _status = EventStreamStatus.Deleted;
        
        // Clear events
        while (_events.TryDequeue(out _)) { }
        
        _logger.LogDebug("Event stream disposed: {StreamId}", _streamId);
    }
} 