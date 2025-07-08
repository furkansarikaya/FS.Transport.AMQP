using System.Collections.Concurrent;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using RabbitMQ.Client;

namespace FS.RabbitMQ.Core.Factories;

/// <summary>
/// Provides a comprehensive implementation of the channel factory with pooling and lifecycle management.
/// </summary>
/// <remarks>
/// The default channel factory provides advanced channel management including pooling,
/// automatic cleanup, health monitoring, and performance optimization. It manages
/// channel lifecycle events and ensures proper resource disposal.
/// </remarks>
public sealed class DefaultChannelFactory : FactoryBase<IChannel>, IChannelFactory
{
    private readonly ChannelConfiguration _configuration;
    private readonly IChannelPoolManager _poolManager;
    private readonly IChannelHealthMonitor _healthMonitor;
    private readonly ConcurrentDictionary<string, ChannelPool> _connectionChannelPools;
    private readonly SemaphoreSlim _poolCreationSemaphore;

    /// <summary>
    /// Initializes a new instance of the <see cref="DefaultChannelFactory"/> class.
    /// </summary>
    /// <param name="configuration">The channel configuration.</param>
    /// <param name="poolManager">The channel pool manager.</param>
    /// <param name="healthMonitor">The channel health monitor.</param>
    /// <param name="logger">The logger instance.</param>
    public DefaultChannelFactory(
        IOptions<ChannelConfiguration> configuration,
        IChannelPoolManager poolManager,
        IChannelHealthMonitor healthMonitor,
        ILogger<DefaultChannelFactory> logger)
        : base(logger)
    {
        _configuration = configuration?.Value ?? throw new ArgumentNullException(nameof(configuration));
        _poolManager = poolManager ?? throw new ArgumentNullException(nameof(poolManager));
        _healthMonitor = healthMonitor ?? throw new ArgumentNullException(nameof(healthMonitor));
        _connectionChannelPools = new ConcurrentDictionary<string, ChannelPool>();
        _poolCreationSemaphore = new SemaphoreSlim(1, 1);

        ValidateConfiguration();
    }

    /// <summary>
    /// Occurs when a channel is created.
    /// </summary>
    public event EventHandler<ChannelCreatedEventArgs>? ChannelCreated;

    /// <summary>
    /// Occurs when a channel creation fails.
    /// </summary>
    public event EventHandler<ChannelFailedEventArgs>? ChannelFailed;

    /// <summary>
    /// Creates a new channel from the specified connection.
    /// </summary>
    /// <param name="connection">The connection to create the channel from.</param>
    /// <returns>A new channel instance.</returns>
    public async Task<IChannel> CreateChannelAsync(IConnection connection)
    {
        return await CreateChannelAsync(connection, _configuration);
    }

    /// <summary>
    /// Creates a new channel with the specified configuration.
    /// </summary>
    /// <param name="connection">The connection to create the channel from.</param>
    /// <param name="configuration">The channel configuration.</param>
    /// <returns>A new channel instance.</returns>
    public async Task<IChannel> CreateChannelAsync(IConnection connection, ChannelConfiguration configuration)
    {
        ThrowIfDisposed();
        ArgumentNullException.ThrowIfNull(connection);
        ArgumentNullException.ThrowIfNull(configuration);

        try
        {
            Logger.LogDebug("Creating new channel for connection {ConnectionId}", connection.Id);

            var channel = await CreateChannelInternalAsync(connection, configuration);

            // Start health monitoring
            await _healthMonitor.StartMonitoringAsync(channel);

            // Raise channel created event
            ChannelCreated?.Invoke(this, new ChannelCreatedEventArgs(channel, connection.Id));

            Logger.LogDebug("Successfully created channel {ChannelId} for connection {ConnectionId}", 
                channel.Id, connection.Id);

            return channel;
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Failed to create channel for connection {ConnectionId}", connection.Id);
            
            // Raise channel failed event
            ChannelFailed?.Invoke(this, new ChannelFailedEventArgs(connection.Id, ex));
            
            throw new ChannelException($"Failed to create channel for connection {connection.Id}", ex);
        }
    }

    /// <summary>
    /// Gets or creates a pooled channel for the specified connection.
    /// </summary>
    /// <param name="connection">The connection to get a channel for.</param>
    /// <returns>A pooled channel instance.</returns>
    public async Task<IChannel> GetPooledChannelAsync(IConnection connection)
    {
        ThrowIfDisposed();
        ArgumentNullException.ThrowIfNull(connection);

        try
        {
            var pool = await GetOrCreateChannelPoolAsync(connection);
            var channel = await pool.GetChannelAsync();

            Logger.LogTrace("Retrieved pooled channel {ChannelId} for connection {ConnectionId}", 
                channel.Id, connection.Id);

            return channel;
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Failed to get pooled channel for connection {ConnectionId}", connection.Id);
            throw new ChannelException($"Failed to get pooled channel for connection {connection.Id}", ex);
        }
    }

    /// <summary>
    /// Returns a channel to the pool for reuse.
    /// </summary>
    /// <param name="channel">The channel to return to the pool.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    public async Task ReturnChannelToPoolAsync(IChannel channel)
    {
        ThrowIfDisposed();
        ArgumentNullException.ThrowIfNull(channel);

        try
        {
            if (_connectionChannelPools.TryGetValue(channel.Connection.Id, out var pool))
            {
                await pool.ReturnChannelAsync(channel);
                Logger.LogTrace("Returned channel {ChannelId} to pool", channel.Id);
            }
            else
            {
                Logger.LogWarning("No pool found for channel {ChannelId}, disposing channel", channel.Id);
                await channel.DisposeAsync();
            }
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Failed to return channel {ChannelId} to pool", channel.Id);
            // Don't throw here, as this is a cleanup operation
        }
    }

    /// <summary>
    /// Gets the current channel pool statistics.
    /// </summary>
    /// <returns>Channel pool statistics and health information.</returns>
    public async Task<ChannelPoolInfo> GetChannelPoolInfoAsync()
    {
        ThrowIfDisposed();

        try
        {
            var poolInfos = new List<ConnectionChannelPoolInfo>();

            foreach (var kvp in _connectionChannelPools)
            {
                var poolInfo = await kvp.Value.GetPoolInfoAsync();
                poolInfos.Add(new ConnectionChannelPoolInfo
                {
                    ConnectionId = kvp.Key,
                    ActiveChannels = poolInfo.ActiveChannels,
                    PooledChannels = poolInfo.PooledChannels,
                    TotalChannelsCreated = poolInfo.TotalChannelsCreated,
                    PoolUtilization = poolInfo.PoolUtilization
                });
            }

            return new ChannelPoolInfo
            {
                ConnectionPools = poolInfos,
                TotalActiveChannels = poolInfos.Sum(p => p.ActiveChannels),
                TotalPooledChannels = poolInfos.Sum(p => p.PooledChannels),
                TotalChannelsCreated = poolInfos.Sum(p => p.TotalChannelsCreated),
                LastUpdated = DateTimeOffset.UtcNow
            };
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Failed to get channel pool information");
            throw new ChannelException("Failed to retrieve channel pool information", ex);
        }
    }

    /// <summary>
    /// Creates the core channel instance.
    /// </summary>
    /// <param name="parameters">The parameters for channel creation.</param>
    /// <returns>A new channel instance.</returns>
    protected override IChannel CreateCore(params object[] parameters)
    {
        throw new NotSupportedException("Use CreateChannelAsync methods for asynchronous channel creation");
    }

    private async Task<IChannel> CreateChannelInternalAsync(IConnection connection, ChannelConfiguration configuration)
    {
        var channelBuilder = new ChannelBuilder()
            .WithConnection(connection)
            .WithConfiguration(configuration)
            .WithLogger(Logger);

        return await channelBuilder.BuildAsync();
    }

    private async Task<ChannelPool> GetOrCreateChannelPoolAsync(IConnection connection)
    {
        var connectionId = connection.Id;

        if (_connectionChannelPools.TryGetValue(connectionId, out var existingPool))
        {
            return existingPool;
        }

        await _poolCreationSemaphore.WaitAsync();
        try
        {
            // Double-checked locking pattern
            if (_connectionChannelPools.TryGetValue(connectionId, out existingPool))
            {
                return existingPool;
            }

            Logger.LogDebug("Creating new channel pool for connection {ConnectionId}", connectionId);

            var newPool = new ChannelPool(connection, _configuration, this, Logger);
            _connectionChannelPools.TryAdd(connectionId, newPool);

            // Subscribe to connection events for cleanup
            connection.Closed += async (_, _) => await RemoveChannelPoolAsync(connectionId);

            Logger.LogDebug("Created new channel pool for connection {ConnectionId}", connectionId);
            return newPool;
        }
        finally
        {
            _poolCreationSemaphore.Release();
        }
    }

    private async Task RemoveChannelPoolAsync(string connectionId)
    {
        if (_connectionChannelPools.TryRemove(connectionId, out var pool))
        {
            Logger.LogDebug("Removing channel pool for connection {ConnectionId}", connectionId);
            await pool.DisposeAsync();
            Logger.LogDebug("Removed channel pool for connection {ConnectionId}", connectionId);
        }
    }

    private void ValidateConfiguration()
    {
        try
        {
            _configuration.Validate();
            Logger.LogDebug("Channel factory configuration validation successful");
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Channel factory configuration validation failed");
            throw new FactoryException("Invalid channel factory configuration", ex);
        }
    }

    /// <summary>
    /// Releases the resources used by the channel factory.
    /// </summary>
    /// <param name="disposing"><c>true</c> to release both managed and unmanaged resources; otherwise, <c>false</c>.</param>
    protected override void Dispose(bool disposing)
    {
        if (disposing && !IsDisposed)
        {
            Logger.LogDebug("Disposing channel factory and associated resources");

            try
            {
                // Dispose all channel pools
                var disposeTasks = _connectionChannelPools.Values.Select(pool => pool.DisposeAsync().AsTask());
                Task.WaitAll(disposeTasks.ToArray(), TimeSpan.FromSeconds(30));

                _connectionChannelPools.Clear();
                _poolCreationSemaphore?.Dispose();
                _healthMonitor?.Dispose();
            }
            catch (Exception ex)
            {
                Logger.LogWarning(ex, "Error occurred while disposing channel factory resources");
            }
        }

        base.Dispose(disposing);
    }
}