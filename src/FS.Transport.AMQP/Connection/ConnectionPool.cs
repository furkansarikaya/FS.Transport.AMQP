using System.Collections.Concurrent;
using FS.Transport.AMQP.Configuration;
using FS.Transport.AMQP.Core.Extensions;
using Microsoft.Extensions.Logging;
using RabbitMQ.Client;

namespace FS.Transport.AMQP.Connection;

/// <summary>
/// Thread-safe connection pool for managing RabbitMQ channels with automatic cleanup and health monitoring
/// </summary>
public class ConnectionPool : IDisposable
{
    private readonly ConnectionSettings _settings;
    private readonly ILogger _logger;
    private readonly ConcurrentQueue<PooledChannel> _availableChannels;
    private readonly ConcurrentDictionary<int, PooledChannel> _activeChannels;
    private readonly Timer _cleanupTimer;
    private readonly SemaphoreSlim _semaphore;
    private volatile bool _disposed;
    private IConnection? _connection;

    public ConnectionPool(ConnectionSettings settings, ILogger logger)
    {
        _settings = settings ?? throw new ArgumentNullException(nameof(settings));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _availableChannels = new ConcurrentQueue<PooledChannel>();
        _activeChannels = new ConcurrentDictionary<int, PooledChannel>();
        _semaphore = new SemaphoreSlim(_settings.MaxConnections, _settings.MaxConnections);
        
        // Setup cleanup timer to run every minute
        _cleanupTimer = new Timer(CleanupExpiredChannels, null, 
            TimeSpan.FromMilliseconds(_settings.PoolCleanupIntervalMs), 
            TimeSpan.FromMilliseconds(_settings.PoolCleanupIntervalMs));
            
        _logger.LogDebug("ConnectionPool initialized with max connections: {MaxConnections}", _settings.MaxConnections);
    }

    /// <summary>
    /// Initializes the pool with a primary connection
    /// </summary>
    public async Task InitializeAsync(IConnection connection)
    {
        if (_disposed)
            throw new ObjectDisposedException(nameof(ConnectionPool));

        _connection = connection ?? throw new ArgumentNullException(nameof(connection));
        
        // Pre-create minimum number of channels
        for (int i = 0; i < _settings.MinConnections; i++)
        {
            try
            {
                var channel = CreatePooledChannel();
                _availableChannels.Enqueue(channel);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to pre-create channel {Index} during pool initialization", i);
            }
        }
        
        _logger.LogInformation("ConnectionPool initialized with {ChannelCount} pre-created channels", 
            _availableChannels.Count);
    }

    /// <summary>
    /// Gets a channel from the pool or creates a new one
    /// </summary>
    public async Task<IModel> GetChannelAsync(CancellationToken cancellationToken = default)
    {
        if (_disposed)
            throw new ObjectDisposedException(nameof(ConnectionPool));

        if (_connection == null || !_connection.IsOpen)
            throw new InvalidOperationException("Connection pool is not initialized or connection is closed");

        await _semaphore.WaitAsync(cancellationToken);
        
        try
        {
            // Try to get an available channel
            while (_availableChannels.TryDequeue(out var pooledChannel))
            {
                if (pooledChannel.Channel.IsUsable() && !pooledChannel.IsExpired)
                {
                    pooledChannel.LastUsed = DateTime.UtcNow;
                    _activeChannels.TryAdd(pooledChannel.Channel.ChannelNumber, pooledChannel);
                    
                    _logger.LogTrace("Reused pooled channel {ChannelNumber}", pooledChannel.Channel.ChannelNumber);
                    return pooledChannel.Channel;
                }
                else
                {
                    // Channel is no longer usable, dispose it
                    pooledChannel.Channel.SafeDispose();
                    _logger.LogTrace("Disposed expired pooled channel {ChannelNumber}", pooledChannel.Channel.ChannelNumber);
                }
            }
            
            // No available channels, create a new one
            var newChannel = CreatePooledChannel();
            _activeChannels.TryAdd(newChannel.Channel.ChannelNumber, newChannel);
            
            _logger.LogTrace("Created new pooled channel {ChannelNumber}", newChannel.Channel.ChannelNumber);
            return newChannel.Channel;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get channel from pool");
            throw;
        }
        finally
        {
            _semaphore.Release();
        }
    }

    /// <summary>
    /// Returns a channel to the pool for reuse
    /// </summary>
    public void ReturnChannel(IModel channel)
    {
        if (_disposed || channel == null)
            return;

        try
        {
            if (_activeChannels.TryRemove(channel.ChannelNumber, out var pooledChannel))
            {
                if (channel.IsUsable() && !pooledChannel.IsExpired && _availableChannels.Count < _settings.MaxConnections)
                {
                    pooledChannel.LastUsed = DateTime.UtcNow;
                    _availableChannels.Enqueue(pooledChannel);
                    _logger.LogTrace("Returned channel {ChannelNumber} to pool", channel.ChannelNumber);
                }
                else
                {
                    // Channel is not reusable or pool is full, dispose it
                    channel.SafeDispose();
                    _logger.LogTrace("Disposed channel {ChannelNumber} (not reusable or pool full)", channel.ChannelNumber);
                }
            }
            else
            {
                // Channel was not from the pool (dedicated channel), just dispose it
                channel.SafeDispose();
                _logger.LogTrace("Disposed non-pooled channel {ChannelNumber}", channel.ChannelNumber);
            }
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Error returning channel {ChannelNumber} to pool", channel.ChannelNumber);
        }
    }

    /// <summary>
    /// Closes all channels and clears the pool
    /// </summary>
    public async Task CloseAsync()
    {
        if (_disposed)
            return;

        _logger.LogInformation("Closing connection pool");
        
        // Close all available channels
        while (_availableChannels.TryDequeue(out var pooledChannel))
        {
            try
            {
                pooledChannel.Channel.SafeClose();
                pooledChannel.Channel.SafeDispose();
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Error closing pooled channel {ChannelNumber}", pooledChannel.Channel.ChannelNumber);
            }
        }
        
        // Close all active channels
        foreach (var activeChannel in _activeChannels.Values)
        {
            try
            {
                activeChannel.Channel.SafeClose();
                activeChannel.Channel.SafeDispose();
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Error closing active channel {ChannelNumber}", activeChannel.Channel.ChannelNumber);
            }
        }
        
        _activeChannels.Clear();
        _logger.LogInformation("Connection pool closed");
    }

    private PooledChannel CreatePooledChannel()
    {
        if (_connection == null || !_connection.IsOpen)
            throw new InvalidOperationException("Connection is not available");

        var channel = _connection.CreateModel();
        return new PooledChannel(channel, DateTime.UtcNow);
    }

    private void CleanupExpiredChannels(object? state)
    {
        if (_disposed)
            return;

        try
        {
            var expiredChannels = new List<PooledChannel>();
            var validChannels = new List<PooledChannel>();
            
            // Check available channels
            while (_availableChannels.TryDequeue(out var pooledChannel))
            {
                if (pooledChannel.IsExpired || !pooledChannel.Channel.IsUsable())
                {
                    expiredChannels.Add(pooledChannel);
                }
                else
                {
                    validChannels.Add(pooledChannel);
                }
            }
            
            // Re-enqueue valid channels
            foreach (var validChannel in validChannels)
            {
                _availableChannels.Enqueue(validChannel);
            }
            
            // Dispose expired channels
            foreach (var expiredChannel in expiredChannels)
            {
                try
                {
                    expiredChannel.Channel.SafeDispose();
                    _logger.LogTrace("Cleaned up expired channel {ChannelNumber}", expiredChannel.Channel.ChannelNumber);
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Error disposing expired channel {ChannelNumber}", expiredChannel.Channel.ChannelNumber);
                }
            }
            
            if (expiredChannels.Count > 0)
            {
                _logger.LogDebug("Cleaned up {ExpiredCount} expired channels from pool", expiredChannels.Count);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during connection pool cleanup");
        }
    }

    public void Dispose()
    {
        if (_disposed)
            return;

        _disposed = true;
        
        try
        {
            _cleanupTimer?.Dispose();
            CloseAsync().GetAwaiter().GetResult();
            _semaphore?.Dispose();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during ConnectionPool disposal");
        }
    }
}