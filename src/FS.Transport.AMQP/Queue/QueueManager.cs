using System.Collections.Concurrent;
using FS.Transport.AMQP.Configuration;
using FS.Transport.AMQP.Connection;
using FS.Transport.AMQP.Core.Exceptions;
using FS.Transport.AMQP.ErrorHandling;
using FS.Transport.AMQP.RetryPolicies;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using RabbitMQ.Client.Exceptions;

namespace FS.Transport.AMQP.Queue;

/// <summary>
/// Production-ready queue manager with auto-recovery, error handling, and comprehensive monitoring
/// </summary>
public class QueueManager : IQueueManager
{
    private readonly RabbitMQConfiguration _configuration;
    private readonly IConnectionManager _connectionManager;
    private readonly IErrorHandler _errorHandler;
    private readonly IRetryPolicy _retryPolicy;
    private readonly ILogger<QueueManager> _logger;
    private readonly QueueStatistics _statistics;
    private readonly ConcurrentDictionary<string, QueueDeclaration> _declaredQueues;
    private readonly SemaphoreSlim _declarationSemaphore;

    public event EventHandler<QueueEventArgs>? QueueDeclared;
    public event EventHandler<QueueEventArgs>? QueueDeleted;
    public event EventHandler<QueueEventArgs>? QueuePurged;
    public event EventHandler<QueueEventArgs>? QueuesRecreating;
    public event EventHandler<QueueEventArgs>? QueuesRecreated;

    public QueueManager(
        IOptions<RabbitMQConfiguration> configuration,
        IConnectionManager connectionManager,
        IErrorHandler errorHandler,
        IRetryPolicy retryPolicy,
        ILogger<QueueManager> logger)
    {
        _configuration = configuration?.Value ?? throw new ArgumentNullException(nameof(configuration));
        _connectionManager = connectionManager ?? throw new ArgumentNullException(nameof(connectionManager));
        _errorHandler = errorHandler ?? throw new ArgumentNullException(nameof(errorHandler));
        _retryPolicy = retryPolicy ?? throw new ArgumentNullException(nameof(retryPolicy));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _statistics = new QueueStatistics();
        _declaredQueues = new ConcurrentDictionary<string, QueueDeclaration>();
        _declarationSemaphore = new SemaphoreSlim(1, 1);

        _logger.LogDebug("QueueManager initialized");
    }

    /// <summary>
    /// Declares a queue using configuration settings
    /// </summary>
    public async Task<QueueDeclareResult?> DeclareAsync(QueueSettings queueSettings, CancellationToken cancellationToken = default)
    {
        if (queueSettings == null)
            throw new ArgumentNullException(nameof(queueSettings));

        queueSettings.Validate();

        var result = await DeclareAsync(
            queueSettings.Name,
            queueSettings.Durable,
            queueSettings.Exclusive,
            queueSettings.AutoDelete,
            queueSettings.Arguments,
            cancellationToken);

        if (result != null && queueSettings.Bindings.Any())
        {
            // Process bindings for this queue
            foreach (var binding in queueSettings.Bindings)
            {
                await BindAsync(
                    queueSettings.Name,
                    binding.Exchange,
                    binding.RoutingKey,
                    binding.Arguments,
                    cancellationToken);
            }
        }

        return result;
    }

    /// <summary>
    /// Declares a queue with manual parameters
    /// </summary>
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

        await _declarationSemaphore.WaitAsync(cancellationToken);
        try
        {
            _statistics.TotalOperations++;
            var startTime = DateTime.UtcNow;

            var declaration = new QueueDeclaration(name, durable, exclusive, autoDelete, arguments);
            
            _logger.LogInformation("Declaring queue: {QueueName} (Durable: {Durable}, Exclusive: {Exclusive}, AutoDelete: {AutoDelete})", 
                name, durable, exclusive, autoDelete);

            var result = await _retryPolicy.ExecuteAsync(async () =>
            {
                using var channel = await _connectionManager.GetChannelAsync(cancellationToken);
                
                try
                {
                    var queueOk = channel.QueueDeclare(
                        queue: name,
                        durable: durable,
                        exclusive: exclusive,
                        autoDelete: autoDelete,
                        arguments: arguments);

                    _connectionManager.ReturnChannel(channel);
                    
                    return new QueueDeclareResult
                    {
                        QueueName = queueOk.QueueName,
                        MessageCount = queueOk.MessageCount,
                        ConsumerCount = queueOk.ConsumerCount,
                        WasCreated = true // We can't easily determine if it was newly created
                    };
                }
                catch (Exception ex)
                {
                    _connectionManager.ReturnChannel(channel);
                    
                    _logger.LogError(ex, "Failed to declare queue {QueueName}: {ErrorMessage}", name, ex.Message);
                    throw new QueueDeclarationException(name, ex);
                }
            }, cancellationToken);

            var duration = DateTime.UtcNow - startTime;
            
            if (result != null)
            {
                // Store the declaration for auto-recovery
                _declaredQueues.AddOrUpdate(name, declaration, (key, existing) => declaration);
                
                _statistics.SuccessfulOperations++;
                _statistics.DeclaredQueues++;
                _statistics.AverageOperationTime = CalculateAverageTime(duration);
                
                _logger.LogInformation("Queue {QueueName} declared successfully in {Duration}ms - Messages: {MessageCount}, Consumers: {ConsumerCount}", 
                    name, duration.TotalMilliseconds, result.MessageCount, result.ConsumerCount);
                
                QueueDeclared?.Invoke(this, new QueueEventArgs(name, QueueOperation.Declare, true, result));
            }
            else
            {
                _statistics.FailedOperations++;
                _logger.LogError("Failed to declare queue {QueueName} after retries", name);
            }

            return result;
        }
        finally
        {
            _declarationSemaphore.Release();
        }
    }

    /// <summary>
    /// Deletes a queue
    /// </summary>
    public async Task<uint?> DeleteAsync(string name, bool ifUnused = false, bool ifEmpty = false, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Queue name cannot be null or empty", nameof(name));

        _statistics.TotalOperations++;
        var startTime = DateTime.UtcNow;

        try
        {
            _logger.LogInformation("Deleting queue: {QueueName} (IfUnused: {IfUnused}, IfEmpty: {IfEmpty})", name, ifUnused, ifEmpty);

            var messageCount = await _retryPolicy.ExecuteAsync(async () =>
            {
                using var channel = await _connectionManager.GetChannelAsync(cancellationToken);
                
                try
                {
                    var deletedMessageCount = channel.QueueDelete(name, ifUnused, ifEmpty);
                    _connectionManager.ReturnChannel(channel);
                    return deletedMessageCount;
                }
                catch (Exception ex)
                {
                    _connectionManager.ReturnChannel(channel);
                    
                    _logger.LogError(ex, "Failed to delete queue {QueueName}: {ErrorMessage}", name, ex.Message);
                    throw new QueueException($"Failed to delete queue '{name}'", ex, name);
                }
            }, cancellationToken);

            var duration = DateTime.UtcNow - startTime;
            
            // Remove from declared queues
            _declaredQueues.TryRemove(name, out _);
            
            _statistics.SuccessfulOperations++;
            _statistics.DeletedQueues++;
            _statistics.AverageOperationTime = CalculateAverageTime(duration);
            
            _logger.LogInformation("Queue {QueueName} deleted successfully in {Duration}ms - {MessageCount} messages deleted", 
                name, duration.TotalMilliseconds, messageCount);
            
            QueueDeleted?.Invoke(this, new QueueEventArgs(name, QueueOperation.Delete, true)
                .WithContext("DeletedMessages", messageCount));

            return messageCount;
        }
        catch (Exception ex)
        {
            _statistics.FailedOperations++;
            var duration = DateTime.UtcNow - startTime;
            _statistics.AverageOperationTime = CalculateAverageTime(duration);
            
            var context = ErrorContext.FromOperation(ex, $"DeleteQueue:{name}");
            await _errorHandler.HandleErrorAsync(context, cancellationToken);
            
            return null;
        }
    }

    /// <summary>
    /// Binds a queue to an exchange
    /// </summary>
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
        if (routingKey == null)
            throw new ArgumentNullException(nameof(routingKey));

        _statistics.TotalOperations++;
        var startTime = DateTime.UtcNow;

        try
        {
            _logger.LogInformation("Binding queue {QueueName} to exchange {ExchangeName} with routing key: {RoutingKey}", 
                queueName, exchangeName, routingKey);

            var success = await _retryPolicy.ExecuteAsync(async () =>
            {
                using var channel = await _connectionManager.GetChannelAsync(cancellationToken);
                
                try
                {
                    channel.QueueBind(queueName, exchangeName, routingKey, arguments);
                    _connectionManager.ReturnChannel(channel);
                    return true;
                }
                catch (Exception ex)
                {
                    _connectionManager.ReturnChannel(channel);
                    
                    _logger.LogError(ex, "Failed to bind queue {QueueName} to exchange {ExchangeName}: {ErrorMessage}", 
                        queueName, exchangeName, ex.Message);
                    throw new QueueException($"Failed to bind queue '{queueName}' to exchange '{exchangeName}'", ex, queueName);
                }
            }, cancellationToken);

            var duration = DateTime.UtcNow - startTime;
            
            if (success)
            {
                _statistics.SuccessfulOperations++;
                _statistics.AverageOperationTime = CalculateAverageTime(duration);
                
                _logger.LogInformation("Queue binding completed successfully in {Duration}ms", duration.TotalMilliseconds);
                
                // Update stored declaration with binding info
                if (_declaredQueues.TryGetValue(queueName, out var declaration))
                {
                    declaration.AddBinding(exchangeName, routingKey, arguments);
                }
            }
            else
            {
                _statistics.FailedOperations++;
            }

            return success;
        }
        catch (Exception ex)
        {
            _statistics.FailedOperations++;
            var duration = DateTime.UtcNow - startTime;
            _statistics.AverageOperationTime = CalculateAverageTime(duration);
            
            var context = ErrorContext.FromOperation(ex, $"BindQueue:{queueName}->{exchangeName}");
            await _errorHandler.HandleErrorAsync(context, cancellationToken);
            
            return false;
        }
    }

    /// <summary>
    /// Unbinds a queue from an exchange
    /// </summary>
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
        if (routingKey == null)
            throw new ArgumentNullException(nameof(routingKey));

        _statistics.TotalOperations++;
        var startTime = DateTime.UtcNow;

        try
        {
            _logger.LogInformation("Unbinding queue {QueueName} from exchange {ExchangeName} with routing key: {RoutingKey}", 
                queueName, exchangeName, routingKey);

            var success = await _retryPolicy.ExecuteAsync(async () =>
            {
                using var channel = await _connectionManager.GetChannelAsync(cancellationToken);
                
                try
                {
                    channel.QueueUnbind(queueName, exchangeName, routingKey, arguments);
                    _connectionManager.ReturnChannel(channel);
                    return true;
                }
                catch (Exception ex)
                {
                    _connectionManager.ReturnChannel(channel);
                    
                    _logger.LogError(ex, "Failed to unbind queue {QueueName} from exchange {ExchangeName}: {ErrorMessage}", 
                        queueName, exchangeName, ex.Message);
                    throw new QueueException($"Failed to unbind queue '{queueName}' from exchange '{exchangeName}'", ex, queueName);
                }
            }, cancellationToken);

            var duration = DateTime.UtcNow - startTime;
            
            if (success)
            {
                _statistics.SuccessfulOperations++;
                _statistics.AverageOperationTime = CalculateAverageTime(duration);
                
                _logger.LogInformation("Queue unbinding completed successfully in {Duration}ms", duration.TotalMilliseconds);
            }
            else
            {
                _statistics.FailedOperations++;
            }

            return success;
        }
        catch (Exception ex)
        {
            _statistics.FailedOperations++;
            var duration = DateTime.UtcNow - startTime;
            _statistics.AverageOperationTime = CalculateAverageTime(duration);
            
            var context = ErrorContext.FromOperation(ex, $"UnbindQueue:{queueName}->{exchangeName}");
            await _errorHandler.HandleErrorAsync(context, cancellationToken);
            
            return false;
        }
    }

    /// <summary>
    /// Checks if a queue exists
    /// </summary>
    public async Task<bool> ExistsAsync(string name, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Queue name cannot be null or empty", nameof(name));

        try
        {
            using var channel = await _connectionManager.GetChannelAsync(cancellationToken);
            
            try
            {
                // Try to declare passively - this will throw if queue doesn't exist
                channel.QueueDeclarePassive(name);
                _connectionManager.ReturnChannel(channel);
                return true;
            }
            catch (OperationInterruptedException ex) when (ex.ShutdownReason.ReplyCode == 404)
            {
                // Queue not found
                _connectionManager.ReturnChannel(channel);
                return false;
            }
            catch (Exception)
            {
                _connectionManager.ReturnChannel(channel);
                throw;
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error checking if queue {QueueName} exists: {ErrorMessage}", name, ex.Message);
            return false;
        }
    }

    /// <summary>
    /// Gets queue information
    /// </summary>
    public async Task<QueueInfo?> GetInfoAsync(string name, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Queue name cannot be null or empty", nameof(name));

        try
        {
            using var channel = await _connectionManager.GetChannelAsync(cancellationToken);
            
            try
            {
                var queueOk = channel.QueueDeclarePassive(name);
                _connectionManager.ReturnChannel(channel);
                
                var info = new QueueInfo
                {
                    Name = name,
                    MessageCount = queueOk.MessageCount,
                    ConsumerCount = queueOk.ConsumerCount
                };

                // Add declaration information if available
                if (_declaredQueues.TryGetValue(name, out var declaration))
                {
                    info.Durable = declaration.Durable;
                    info.Exclusive = declaration.Exclusive;
                    info.AutoDelete = declaration.AutoDelete;
                    info.Arguments = declaration.Arguments;
                }

                return info;
            }
            catch (OperationInterruptedException ex) when (ex.ShutdownReason.ReplyCode == 404)
            {
                // Queue not found
                _connectionManager.ReturnChannel(channel);
                return null;
            }
            catch (Exception)
            {
                _connectionManager.ReturnChannel(channel);
                throw;
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting info for queue {QueueName}: {ErrorMessage}", name, ex.Message);
            return null;
        }
    }

    /// <summary>
    /// Purges all messages from a queue
    /// </summary>
    public async Task<uint?> PurgeAsync(string name, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Queue name cannot be null or empty", nameof(name));

        _statistics.TotalOperations++;
        var startTime = DateTime.UtcNow;

        try
        {
            _logger.LogInformation("Purging queue: {QueueName}", name);

            var messageCount = await _retryPolicy.ExecuteAsync(async () =>
            {
                using var channel = await _connectionManager.GetChannelAsync(cancellationToken);
                
                try
                {
                    var purgedMessageCount = channel.QueuePurge(name);
                    _connectionManager.ReturnChannel(channel);
                    return purgedMessageCount;
                }
                catch (Exception ex)
                {
                    _connectionManager.ReturnChannel(channel);
                    
                    _logger.LogError(ex, "Failed to purge queue {QueueName}: {ErrorMessage}", name, ex.Message);
                    throw new QueueException($"Failed to purge queue '{name}'", ex, name);
                }
            }, cancellationToken);

            var duration = DateTime.UtcNow - startTime;
            
            _statistics.SuccessfulOperations++;
            _statistics.AverageOperationTime = CalculateAverageTime(duration);
            
            _logger.LogInformation("Queue {QueueName} purged successfully in {Duration}ms - {MessageCount} messages purged", 
                name, duration.TotalMilliseconds, messageCount);
            
            QueuePurged?.Invoke(this, new QueueEventArgs(name, QueueOperation.Purge, true)
                .WithContext("PurgedMessages", messageCount));

            return messageCount;
        }
        catch (Exception ex)
        {
            _statistics.FailedOperations++;
            var duration = DateTime.UtcNow - startTime;
            _statistics.AverageOperationTime = CalculateAverageTime(duration);
            
            var context = ErrorContext.FromOperation(ex, $"PurgeQueue:{name}");
            await _errorHandler.HandleErrorAsync(context, cancellationToken);
            
            return null;
        }
    }

    /// <summary>
    /// Gets message count for a queue
    /// </summary>
    public async Task<uint?> GetMessageCountAsync(string name, CancellationToken cancellationToken = default)
    {
        var info = await GetInfoAsync(name, cancellationToken);
        return info?.MessageCount;
    }

    /// <summary>
    /// Gets consumer count for a queue
    /// </summary>
    public async Task<uint?> GetConsumerCountAsync(string name, CancellationToken cancellationToken = default)
    {
        var info = await GetInfoAsync(name, cancellationToken);
        return info?.ConsumerCount;
    }

    /// <summary>
    /// Declares all configured queues
    /// </summary>
    public async Task<bool> DeclareAllAsync(IEnumerable<QueueSettings> queues, CancellationToken cancellationToken = default)
    {
        if (queues == null)
            throw new ArgumentNullException(nameof(queues));

        var queueList = queues.ToList();
        if (!queueList.Any())
        {
            _logger.LogDebug("No queues to declare");
            return true;
        }

        _logger.LogInformation("Declaring {QueueCount} queues", queueList.Count);

        var allSuccessful = true;
        foreach (var queueSettings in queueList)
        {
            try
            {
                var result = await DeclareAsync(queueSettings, cancellationToken);
                if (result == null)
                {
                    allSuccessful = false;
                    _logger.LogWarning("Failed to declare queue: {QueueName}", queueSettings.Name);
                }
            }
            catch (Exception ex)
            {
                allSuccessful = false;
                _logger.LogError(ex, "Error declaring queue {QueueName}: {ErrorMessage}", 
                    queueSettings.Name, ex.Message);
            }
        }

        _logger.LogInformation("Queue declaration completed. Success: {AllSuccessful}", allSuccessful);
        return allSuccessful;
    }

    /// <summary>
    /// Re-declares all registered queues (for auto-recovery)
    /// </summary>
    public async Task<bool> RedeclareAllAsync(CancellationToken cancellationToken = default)
    {
        var queues = _declaredQueues.Values.ToList();
        if (!queues.Any())
        {
            _logger.LogDebug("No queues to redeclare");
            return true;
        }

        _logger.LogInformation("Redeclaring {QueueCount} queues for auto-recovery", queues.Count);
        
        QueuesRecreating?.Invoke(this, new QueueEventArgs("", QueueOperation.Redeclare, true));

        var allSuccessful = true;
        foreach (var declaration in queues)
        {
            try
            {
                var result = await DeclareAsync(
                    declaration.Name,
                    declaration.Durable,
                    declaration.Exclusive,
                    declaration.AutoDelete,
                    declaration.Arguments,
                    cancellationToken);

                if (result == null)
                {
                    allSuccessful = false;
                    _logger.LogWarning("Failed to redeclare queue: {QueueName}", declaration.Name);
                }
                else
                {
                    // Recreate bindings
                    foreach (var binding in declaration.Bindings)
                    {
                        var bindingSuccess = await BindAsync(
                            declaration.Name,
                            binding.Exchange,
                            binding.RoutingKey,
                            binding.Arguments,
                            cancellationToken);

                        if (!bindingSuccess)
                        {
                            allSuccessful = false;
                            _logger.LogWarning("Failed to rebind queue {QueueName} to exchange {ExchangeName}", 
                                declaration.Name, binding.Exchange);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                allSuccessful = false;
                _logger.LogError(ex, "Error redeclaring queue {QueueName}: {ErrorMessage}", 
                    declaration.Name, ex.Message);
            }
        }

        QueuesRecreated?.Invoke(this, new QueueEventArgs("", QueueOperation.Redeclare, allSuccessful));
        
        _logger.LogInformation("Queue redeclaration completed. Success: {AllSuccessful}", allSuccessful);
        return allSuccessful;
    }

    /// <summary>
    /// Gets queue management statistics
    /// </summary>
    public QueueStatistics GetStatistics()
    {
        return _statistics.Clone();
    }

    private TimeSpan CalculateAverageTime(TimeSpan currentDuration)
    {
        var totalOperations = _statistics.SuccessfulOperations + _statistics.FailedOperations;
        if (totalOperations <= 1)
            return currentDuration;

        var currentAverage = _statistics.AverageOperationTime;
        var newAverage = ((currentAverage.TotalMilliseconds * (totalOperations - 1)) + currentDuration.TotalMilliseconds) / totalOperations;
        
        return TimeSpan.FromMilliseconds(newAverage);
    }
}