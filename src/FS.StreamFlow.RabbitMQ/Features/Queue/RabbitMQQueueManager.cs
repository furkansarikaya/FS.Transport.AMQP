using RabbitMQ.Client;
using FS.StreamFlow.Core.Features.Messaging.Interfaces;
using FS.StreamFlow.Core.Features.Messaging.Models;
using Microsoft.Extensions.Logging;
using System.Collections.Concurrent;
using CoreChannel = FS.StreamFlow.Core.Features.Messaging.Models.IChannel;
using RabbitChannel = RabbitMQ.Client.IChannel;

namespace FS.StreamFlow.RabbitMQ.Features.Queue;

/// <summary>
/// RabbitMQ implementation of queue manager providing comprehensive queue management with declaration, 
/// binding, deletion, and monitoring capabilities with enterprise-grade error handling and recovery
/// </summary>
public class RabbitMQQueueManager : IQueueManager
{
    private readonly IConnectionManager _connectionManager;
    private readonly ILogger<RabbitMQQueueManager> _logger;
    private readonly ConcurrentDictionary<string, QueueInfo> _declaredQueues = new();
    private readonly ConcurrentDictionary<string, QueueBinding> _queueBindings = new();
    private readonly object _lockObject = new();
    private volatile bool _disposed = false;

    /// <summary>
    /// Initializes a new instance of the RabbitMQQueueManager class
    /// </summary>
    /// <param name="connectionManager">Connection manager for RabbitMQ operations</param>
    /// <param name="logger">Logger instance for diagnostic information</param>
    /// <exception cref="ArgumentNullException">Thrown when connectionManager or logger is null</exception>
    public RabbitMQQueueManager(IConnectionManager connectionManager, ILogger<RabbitMQQueueManager> logger)
    {
        _connectionManager = connectionManager ?? throw new ArgumentNullException(nameof(connectionManager));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        
        // Subscribe to connection events for queue recovery
        _connectionManager.Recovered += OnConnectionRecovered;
        
        _logger.LogInformation("RabbitMQ Queue Manager initialized successfully");
    }

    /// <summary>
    /// Event raised when a queue is successfully declared
    /// </summary>
    public event EventHandler<QueueEventArgs>? QueueDeclared;

    /// <summary>
    /// Event raised when a queue is deleted
    /// </summary>
    public event EventHandler<QueueEventArgs>? QueueDeleted;

    /// <summary>
    /// Event raised when queues are being recreated during recovery
    /// </summary>
    public event EventHandler<QueueEventArgs>? QueuesRecreating;

    /// <summary>
    /// Event raised when queues have been successfully recreated during recovery
    /// </summary>
    public event EventHandler<QueueEventArgs>? QueuesRecreated;

    /// <summary>
    /// Declares a queue with the specified configuration
    /// </summary>
    /// <param name="queueSettings">Queue configuration settings</param>
    /// <param name="cancellationToken">Cancellation token for operation cancellation</param>
    /// <returns>Queue declaration result with message count and consumer count, or null if failed</returns>
    /// <exception cref="ArgumentNullException">Thrown when queueSettings is null</exception>
    /// <exception cref="ObjectDisposedException">Thrown when the queue manager has been disposed</exception>
    public async Task<QueueDeclareResult?> DeclareAsync(QueueSettings queueSettings, CancellationToken cancellationToken = default)
    {
        if (queueSettings == null)
            throw new ArgumentNullException(nameof(queueSettings));

        ThrowIfDisposed();

        return await DeclareAsync(
            queueSettings.Name,
            queueSettings.Durable,
            queueSettings.Exclusive,
            queueSettings.AutoDelete,
            queueSettings.Arguments,
            cancellationToken);
    }

    /// <summary>
    /// Declares a queue with manual parameters
    /// </summary>
    /// <param name="name">Queue name</param>
    /// <param name="durable">Whether queue is durable (survives server restarts)</param>
    /// <param name="exclusive">Whether queue is exclusive (only one connection can use it)</param>
    /// <param name="autoDelete">Whether queue auto-deletes when last consumer disconnects</param>
    /// <param name="arguments">Additional queue arguments for advanced features</param>
    /// <param name="cancellationToken">Cancellation token for operation cancellation</param>
    /// <returns>Queue declaration result with message count and consumer count, or null if failed</returns>
    /// <exception cref="ArgumentException">Thrown when name is null or empty</exception>
    /// <exception cref="ObjectDisposedException">Thrown when the queue manager has been disposed</exception>
    public async Task<QueueDeclareResult?> DeclareAsync(
        string name,
        bool durable = true,
        bool exclusive = false,
        bool autoDelete = false,
        IDictionary<string, object>? arguments = null,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Queue name cannot be null or empty", nameof(name));

        ThrowIfDisposed();

        try
        {
            var channel = await _connectionManager.GetChannelAsync(cancellationToken);
            if (channel == null)
            {
                _logger.LogWarning("Failed to get channel for queue declaration: {QueueName}", name);
                return null;
            }

            try
            {
                var rabbitChannel = ExtractRabbitChannel(channel);
                if (rabbitChannel == null)
                {
                    _logger.LogWarning("Failed to extract RabbitMQ channel for queue declaration: {QueueName}", name);
                    return null;
                }

                // Declare the queue
                var queueDeclareOk = await rabbitChannel.QueueDeclareAsync(
                    queue: name,
                    durable: durable,
                    exclusive: exclusive,
                    autoDelete: autoDelete,
                    arguments: arguments,
                    cancellationToken: cancellationToken);

                // Store queue information
                var queueInfo = new QueueInfo
                {
                    Name = name,
                    Durable = durable,
                    Exclusive = exclusive,
                    AutoDelete = autoDelete,
                    Arguments = arguments?.ToDictionary(kvp => kvp.Key, kvp => kvp.Value),
                    MessageCount = queueDeclareOk.MessageCount,
                    ConsumerCount = queueDeclareOk.ConsumerCount,
                    CreatedAt = DateTimeOffset.UtcNow,
                    LastUsedAt = DateTimeOffset.UtcNow,
                    State = QueueState.Active
                };

                _declaredQueues.AddOrUpdate(name, queueInfo, (_, _) => queueInfo);

                var result = new QueueDeclareResult(
                    queueName: name,
                    messageCount: queueDeclareOk.MessageCount,
                    consumerCount: queueDeclareOk.ConsumerCount,
                    created: true);

                _logger.LogInformation("Queue declared successfully: {QueueName} (Messages: {MessageCount}, Consumers: {ConsumerCount})",
                    name, queueDeclareOk.MessageCount, queueDeclareOk.ConsumerCount);

                // Raise event
                QueueDeclared?.Invoke(this, new QueueEventArgs(name, QueueState.Active));

                return result;
            }
            finally
            {
                await _connectionManager.ReturnChannelAsync(channel, cancellationToken);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to declare queue: {QueueName}", name);
            QueueDeclared?.Invoke(this, new QueueEventArgs(name, QueueState.Error, ex.Message, ex));
            return null;
        }
    }

    /// <summary>
    /// Deletes a queue
    /// </summary>
    /// <param name="name">Queue name to delete</param>
    /// <param name="ifUnused">Only delete if queue has no consumers</param>
    /// <param name="ifEmpty">Only delete if queue has no messages</param>
    /// <param name="cancellationToken">Cancellation token for operation cancellation</param>
    /// <returns>Number of messages deleted, or null if operation failed</returns>
    /// <exception cref="ArgumentException">Thrown when name is null or empty</exception>
    /// <exception cref="ObjectDisposedException">Thrown when the queue manager has been disposed</exception>
    public async Task<uint?> DeleteAsync(string name, bool ifUnused = false, bool ifEmpty = false, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Queue name cannot be null or empty", nameof(name));

        ThrowIfDisposed();

        try
        {
            var channel = await _connectionManager.GetChannelAsync(cancellationToken);
            if (channel == null)
            {
                _logger.LogWarning("Failed to get channel for queue deletion: {QueueName}", name);
                return null;
            }

            try
            {
                var rabbitChannel = ExtractRabbitChannel(channel);
                if (rabbitChannel == null)
                {
                    _logger.LogWarning("Failed to extract RabbitMQ channel for queue deletion: {QueueName}", name);
                    return null;
                }

                // Update queue state
                if (_declaredQueues.TryGetValue(name, out var queueInfo))
                {
                    queueInfo.State = QueueState.Deleting;
                }

                // Delete the queue
                var deleteOk = await rabbitChannel.QueueDeleteAsync(
                    queue: name,
                    ifUnused: ifUnused,
                    ifEmpty: ifEmpty,
                    cancellationToken: cancellationToken);

                // Remove from tracking
                _declaredQueues.TryRemove(name, out _);

                _logger.LogInformation("Queue deleted successfully: {QueueName} (Messages deleted: {MessageCount})",
                    name, deleteOk);

                // Raise event
                QueueDeleted?.Invoke(this, new QueueEventArgs(name, QueueState.Deleting));

                return deleteOk;
            }
            finally
            {
                await _connectionManager.ReturnChannelAsync(channel, cancellationToken);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to delete queue: {QueueName}", name);
            QueueDeleted?.Invoke(this, new QueueEventArgs(name, QueueState.Error, ex.Message, ex));
            return null;
        }
    }

    /// <summary>
    /// Binds a queue to an exchange
    /// </summary>
    /// <param name="queueName">Queue name to bind</param>
    /// <param name="exchangeName">Exchange name to bind to</param>
    /// <param name="routingKey">Routing key for message routing</param>
    /// <param name="arguments">Additional binding arguments</param>
    /// <param name="cancellationToken">Cancellation token for operation cancellation</param>
    /// <returns>True if binding was successful, otherwise false</returns>
    /// <exception cref="ArgumentException">Thrown when queueName or exchangeName is null or empty</exception>
    /// <exception cref="ObjectDisposedException">Thrown when the queue manager has been disposed</exception>
    public async Task<bool> BindAsync(
        string queueName,
        string exchangeName,
        string routingKey,
        IDictionary<string, object>? arguments = null,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(queueName))
            throw new ArgumentException("Queue name cannot be null or empty", nameof(queueName));
        if (string.IsNullOrWhiteSpace(exchangeName))
            throw new ArgumentException("Exchange name cannot be null or empty", nameof(exchangeName));

        ThrowIfDisposed();

        try
        {
            var channel = await _connectionManager.GetChannelAsync(cancellationToken);
            if (channel == null)
            {
                _logger.LogWarning("Failed to get channel for queue binding: {QueueName} -> {ExchangeName}", queueName, exchangeName);
                return false;
            }

            try
            {
                var rabbitChannel = ExtractRabbitChannel(channel);
                if (rabbitChannel == null)
                {
                    _logger.LogWarning("Failed to extract RabbitMQ channel for queue binding: {QueueName} -> {ExchangeName}", queueName, exchangeName);
                    return false;
                }

                // Bind the queue to exchange
                await rabbitChannel.QueueBindAsync(
                    queue: queueName,
                    exchange: exchangeName,
                    routingKey: routingKey ?? string.Empty,
                    arguments: arguments,
                    cancellationToken: cancellationToken);

                // Store binding information
                var bindingKey = $"{queueName}:{exchangeName}:{routingKey}";
                var binding = new QueueBinding
                {
                    QueueName = queueName,
                    ExchangeName = exchangeName,
                    RoutingKey = routingKey ?? string.Empty,
                    Arguments = arguments?.ToDictionary(kvp => kvp.Key, kvp => kvp.Value)
                };

                _queueBindings.AddOrUpdate(bindingKey, binding, (_, _) => binding);

                _logger.LogInformation("Queue bound successfully: {QueueName} -> {ExchangeName} (Routing Key: {RoutingKey})",
                    queueName, exchangeName, routingKey);

                return true;
            }
            finally
            {
                await _connectionManager.ReturnChannelAsync(channel, cancellationToken);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to bind queue: {QueueName} -> {ExchangeName} (Routing Key: {RoutingKey})",
                queueName, exchangeName, routingKey);
            return false;
        }
    }

    /// <summary>
    /// Unbinds a queue from an exchange
    /// </summary>
    /// <param name="queueName">Queue name to unbind</param>
    /// <param name="exchangeName">Exchange name to unbind from</param>
    /// <param name="routingKey">Routing key used for the binding</param>
    /// <param name="arguments">Additional unbinding arguments</param>
    /// <param name="cancellationToken">Cancellation token for operation cancellation</param>
    /// <returns>True if unbinding was successful, otherwise false</returns>
    /// <exception cref="ArgumentException">Thrown when queueName or exchangeName is null or empty</exception>
    /// <exception cref="ObjectDisposedException">Thrown when the queue manager has been disposed</exception>
    public async Task<bool> UnbindAsync(
        string queueName,
        string exchangeName,
        string routingKey,
        IDictionary<string, object>? arguments = null,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(queueName))
            throw new ArgumentException("Queue name cannot be null or empty", nameof(queueName));
        if (string.IsNullOrWhiteSpace(exchangeName))
            throw new ArgumentException("Exchange name cannot be null or empty", nameof(exchangeName));

        ThrowIfDisposed();

        try
        {
            var channel = await _connectionManager.GetChannelAsync(cancellationToken);
            if (channel == null)
            {
                _logger.LogWarning("Failed to get channel for queue unbinding: {QueueName} -> {ExchangeName}", queueName, exchangeName);
                return false;
            }

            try
            {
                var rabbitChannel = ExtractRabbitChannel(channel);
                if (rabbitChannel == null)
                {
                    _logger.LogWarning("Failed to extract RabbitMQ channel for queue unbinding: {QueueName} -> {ExchangeName}", queueName, exchangeName);
                    return false;
                }

                // Unbind the queue from exchange
                await rabbitChannel.QueueUnbindAsync(
                    queue: queueName,
                    exchange: exchangeName,
                    routingKey: routingKey ?? string.Empty,
                    arguments: arguments,
                    cancellationToken: cancellationToken);

                // Remove binding information
                var bindingKey = $"{queueName}:{exchangeName}:{routingKey}";
                _queueBindings.TryRemove(bindingKey, out _);

                _logger.LogInformation("Queue unbound successfully: {QueueName} -> {ExchangeName} (Routing Key: {RoutingKey})",
                    queueName, exchangeName, routingKey);

                return true;
            }
            finally
            {
                await _connectionManager.ReturnChannelAsync(channel, cancellationToken);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to unbind queue: {QueueName} -> {ExchangeName} (Routing Key: {RoutingKey})",
                queueName, exchangeName, routingKey);
            return false;
        }
    }

    /// <summary>
    /// Purges all messages from a queue
    /// </summary>
    /// <param name="queueName">Queue name to purge</param>
    /// <param name="cancellationToken">Cancellation token for operation cancellation</param>
    /// <returns>Number of messages purged, or null if operation failed</returns>
    /// <exception cref="ArgumentException">Thrown when queueName is null or empty</exception>
    /// <exception cref="ObjectDisposedException">Thrown when the queue manager has been disposed</exception>
    public async Task<uint?> PurgeAsync(string queueName, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(queueName))
            throw new ArgumentException("Queue name cannot be null or empty", nameof(queueName));

        ThrowIfDisposed();

        try
        {
            var channel = await _connectionManager.GetChannelAsync(cancellationToken);
            if (channel == null)
            {
                _logger.LogWarning("Failed to get channel for queue purging: {QueueName}", queueName);
                return null;
            }

            try
            {
                var rabbitChannel = ExtractRabbitChannel(channel);
                if (rabbitChannel == null)
                {
                    _logger.LogWarning("Failed to extract RabbitMQ channel for queue purging: {QueueName}", queueName);
                    return null;
                }

                // Purge the queue
                var purgeOk = await rabbitChannel.QueuePurgeAsync(
                    queue: queueName,
                    cancellationToken: cancellationToken);

                _logger.LogInformation("Queue purged successfully: {QueueName} (Messages purged: {MessageCount})",
                    queueName, purgeOk);

                return purgeOk;
            }
            finally
            {
                await _connectionManager.ReturnChannelAsync(channel, cancellationToken);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to purge queue: {QueueName}", queueName);
            return null;
        }
    }

    /// <summary>
    /// Gets information about a queue
    /// </summary>
    /// <param name="queueName">Queue name to get information for</param>
    /// <param name="cancellationToken">Cancellation token for operation cancellation</param>
    /// <returns>Queue information, or null if queue doesn't exist or operation failed</returns>
    /// <exception cref="ArgumentException">Thrown when queueName is null or empty</exception>
    /// <exception cref="ObjectDisposedException">Thrown when the queue manager has been disposed</exception>
    public async Task<QueueInfo?> GetQueueInfoAsync(string queueName, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(queueName))
            throw new ArgumentException("Queue name cannot be null or empty", nameof(queueName));

        ThrowIfDisposed();

        try
        {
            var channel = await _connectionManager.GetChannelAsync(cancellationToken);
            if (channel == null)
            {
                _logger.LogWarning("Failed to get channel for queue info: {QueueName}", queueName);
                return null;
            }

            try
            {
                var rabbitChannel = ExtractRabbitChannel(channel);
                if (rabbitChannel == null)
                {
                    _logger.LogWarning("Failed to extract RabbitMQ channel for queue info: {QueueName}", queueName);
                    return null;
                }

                // Get queue info using passive declare
                var queueDeclareOk = await rabbitChannel.QueueDeclareAsync(
                    queue: queueName,
                    durable: true,
                    exclusive: false,
                    autoDelete: false,
                    arguments: null,
                    cancellationToken: cancellationToken);

                // Get cached queue info if available
                if (_declaredQueues.TryGetValue(queueName, out var cachedInfo))
                {
                    cachedInfo.MessageCount = queueDeclareOk.MessageCount;
                    cachedInfo.ConsumerCount = queueDeclareOk.ConsumerCount;
                    cachedInfo.LastUsedAt = DateTimeOffset.UtcNow;
                    
                    // Get bindings for this queue
                    var queueBindings = _queueBindings.Values
                        .Where(b => b.QueueName == queueName)
                        .ToList();
                    
                    cachedInfo.Bindings = queueBindings;
                    
                    return cachedInfo;
                }

                // Create new queue info
                var queueInfo = new QueueInfo
                {
                    Name = queueName,
                    MessageCount = queueDeclareOk.MessageCount,
                    ConsumerCount = queueDeclareOk.ConsumerCount,
                    CreatedAt = DateTimeOffset.UtcNow,
                    LastUsedAt = DateTimeOffset.UtcNow,
                    State = QueueState.Active,
                    Bindings = _queueBindings.Values.Where(b => b.QueueName == queueName).ToList()
                };

                return queueInfo;
            }
            finally
            {
                await _connectionManager.ReturnChannelAsync(channel, cancellationToken);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get queue info: {QueueName}", queueName);
            return null;
        }
    }

    /// <summary>
    /// Gets statistics for all queues
    /// </summary>
    /// <param name="cancellationToken">Cancellation token for operation cancellation</param>
    /// <returns>Comprehensive queue statistics</returns>
    /// <exception cref="ObjectDisposedException">Thrown when the queue manager has been disposed</exception>
    public async Task<QueueStatistics> GetStatisticsAsync(CancellationToken cancellationToken = default)
    {
        ThrowIfDisposed();

        try
        {
            var statistics = new QueueStatistics
            {
                TotalQueues = _declaredQueues.Count,
                Timestamp = DateTimeOffset.UtcNow
            };

            var queueInfos = _declaredQueues.Values.ToList();
            
            statistics.DurableQueues = queueInfos.Count(q => q.Durable);
            statistics.ExclusiveQueues = queueInfos.Count(q => q.Exclusive);
            statistics.AutoDeleteQueues = queueInfos.Count(q => q.AutoDelete);
            statistics.TotalMessages = queueInfos.Sum(q => (long)q.MessageCount);
            statistics.TotalConsumers = queueInfos.Sum(q => (long)q.ConsumerCount);
            statistics.AverageQueueDepth = queueInfos.Count > 0 ? queueInfos.Average(q => q.MessageCount) : 0;

            // Group by state
            var stateGroups = queueInfos.GroupBy(q => q.State);
            foreach (var group in stateGroups)
            {
                statistics.QueuesByState[group.Key] = group.Count();
            }

            _logger.LogDebug("Queue statistics calculated: {TotalQueues} queues, {TotalMessages} messages, {TotalConsumers} consumers",
                statistics.TotalQueues, statistics.TotalMessages, statistics.TotalConsumers);

            return statistics;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get queue statistics");
            return new QueueStatistics { Timestamp = DateTimeOffset.UtcNow };
        }
    }

    /// <summary>
    /// Handles connection recovery by recreating declared queues and bindings
    /// </summary>
    /// <param name="sender">Event sender</param>
    /// <param name="e">Connection event arguments</param>
    private async void OnConnectionRecovered(object? sender, ConnectionEventArgs e)
    {
        if (_disposed)
            return;

        try
        {
            _logger.LogInformation("Starting queue recovery after connection recovered");

            // Raise recovery start event
            QueuesRecreating?.Invoke(this, new QueueEventArgs("*", QueueState.Active, "Starting queue recovery"));

            // Recreate all declared queues
            var queuesToRecreate = _declaredQueues.Values.ToList();
            foreach (var queueInfo in queuesToRecreate)
            {
                try
                {
                    await DeclareAsync(
                        queueInfo.Name,
                        queueInfo.Durable,
                        queueInfo.Exclusive,
                        queueInfo.AutoDelete,
                        queueInfo.Arguments);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Failed to recreate queue during recovery: {QueueName}", queueInfo.Name);
                }
            }

            // Recreate all bindings
            var bindingsToRecreate = _queueBindings.Values.ToList();
            foreach (var binding in bindingsToRecreate)
            {
                try
                {
                    await BindAsync(
                        binding.QueueName,
                        binding.ExchangeName,
                        binding.RoutingKey,
                        binding.Arguments);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Failed to recreate binding during recovery: {QueueName} -> {ExchangeName}",
                        binding.QueueName, binding.ExchangeName);
                }
            }

            _logger.LogInformation("Queue recovery completed successfully");

            // Raise recovery complete event
            QueuesRecreated?.Invoke(this, new QueueEventArgs("*", QueueState.Active, "Queue recovery completed"));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to recover queues after connection recovery");
        }
    }

    /// <summary>
    /// Extracts the RabbitMQ channel from the provider-agnostic channel wrapper
    /// </summary>
    /// <param name="channel">Provider-agnostic channel</param>
    /// <returns>RabbitMQ channel, or null if extraction failed</returns>
    private static RabbitChannel? ExtractRabbitChannel(CoreChannel channel)
    {
        return channel switch
        {
            FS.StreamFlow.RabbitMQ.Features.Connection.RabbitMQChannel rabbitChannel => rabbitChannel.GetNativeChannel(),
            _ => null
        };
    }

    /// <summary>
    /// Throws ObjectDisposedException if the queue manager has been disposed
    /// </summary>
    /// <exception cref="ObjectDisposedException">Thrown when the queue manager has been disposed</exception>
    private void ThrowIfDisposed()
    {
        if (_disposed)
            throw new ObjectDisposedException(nameof(RabbitMQQueueManager));
    }

    /// <summary>
    /// Releases all resources used by the RabbitMQQueueManager
    /// </summary>
    public void Dispose()
    {
        if (_disposed)
            return;

        lock (_lockObject)
        {
            if (_disposed)
                return;

            _disposed = true;

            // Unsubscribe from events
            if (_connectionManager != null)
            {
                _connectionManager.Recovered -= OnConnectionRecovered;
            }

            // Clear collections
            _declaredQueues.Clear();
            _queueBindings.Clear();

            _logger.LogInformation("RabbitMQ Queue Manager disposed successfully");
        }
    }
} 