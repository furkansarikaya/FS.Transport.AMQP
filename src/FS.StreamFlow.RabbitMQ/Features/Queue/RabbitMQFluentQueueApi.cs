using FS.StreamFlow.Core.Features.Messaging.Interfaces;
using FS.StreamFlow.Core.Features.Messaging.Models;
using Microsoft.Extensions.Logging;

namespace FS.StreamFlow.RabbitMQ.Features.Queue;

/// <summary>
/// RabbitMQ implementation of fluent queue API
/// </summary>
public class RabbitMQFluentQueueApi : IFluentQueueApi
{
    private readonly IQueueManager _queueManager;
    private readonly ILogger _logger;
    private readonly string _queueName;
    
    private bool _durable = true;
    private bool _exclusive = false;
    private bool _autoDelete = false;
    private Dictionary<string, object> _arguments = new();
    private readonly List<FluentQueueBinding> _bindings = new();
    
    public RabbitMQFluentQueueApi(
        IQueueManager queueManager,
        string queueName,
        ILogger logger)
    {
        _queueManager = queueManager ?? throw new ArgumentNullException(nameof(queueManager));
        _queueName = queueName ?? throw new ArgumentNullException(nameof(queueName));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }
    
    /// <summary>
    /// Configures whether the queue is durable (survives server restarts)
    /// </summary>
    /// <param name="durable">Whether the queue is durable</param>
    /// <returns>Fluent queue API for method chaining</returns>
    public IFluentQueueApi WithDurable(bool durable = true)
    {
        _durable = durable;
        return this;
    }
    
    /// <summary>
    /// Configures whether the queue is exclusive (only used by one connection)
    /// </summary>
    /// <param name="exclusive">Whether the queue is exclusive</param>
    /// <returns>Fluent queue API for method chaining</returns>
    public IFluentQueueApi WithExclusive(bool exclusive = true)
    {
        _exclusive = exclusive;
        return this;
    }
    
    /// <summary>
    /// Configures whether the queue auto-deletes when not in use
    /// </summary>
    /// <param name="autoDelete">Whether the queue auto-deletes</param>
    /// <returns>Fluent queue API for method chaining</returns>
    public IFluentQueueApi WithAutoDelete(bool autoDelete = true)
    {
        _autoDelete = autoDelete;
        return this;
    }
    
    /// <summary>
    /// Configures queue arguments
    /// </summary>
    /// <param name="arguments">Queue arguments dictionary</param>
    /// <returns>Fluent queue API for method chaining</returns>
    public IFluentQueueApi WithArguments(Dictionary<string, object> arguments)
    {
        _arguments = arguments ?? throw new ArgumentNullException(nameof(arguments));
        return this;
    }
    
    /// <summary>
    /// Adds a single argument to the queue
    /// </summary>
    /// <param name="key">Argument key</param>
    /// <param name="value">Argument value</param>
    /// <returns>Fluent queue API for method chaining</returns>
    public IFluentQueueApi WithArgument(string key, object value)
    {
        if (string.IsNullOrEmpty(key))
            throw new ArgumentException("Key cannot be null or empty", nameof(key));
        
        _arguments[key] = value ?? throw new ArgumentNullException(nameof(value));
        return this;
    }
    
    /// <summary>
    /// Configures dead letter exchange
    /// </summary>
    /// <param name="deadLetterExchange">Dead letter exchange name</param>
    /// <returns>Fluent queue API for method chaining</returns>
    public IFluentQueueApi WithDeadLetterExchange(string deadLetterExchange)
    {
        if (string.IsNullOrEmpty(deadLetterExchange))
            throw new ArgumentException("Dead letter exchange cannot be null or empty", nameof(deadLetterExchange));
        
        _arguments["x-dead-letter-exchange"] = deadLetterExchange;
        return this;
    }
    
    /// <summary>
    /// Configures dead letter routing key
    /// </summary>
    /// <param name="deadLetterRoutingKey">Dead letter routing key</param>
    /// <returns>Fluent queue API for method chaining</returns>
    public IFluentQueueApi WithDeadLetterRoutingKey(string deadLetterRoutingKey)
    {
        if (string.IsNullOrEmpty(deadLetterRoutingKey))
            throw new ArgumentException("Dead letter routing key cannot be null or empty", nameof(deadLetterRoutingKey));
        
        _arguments["x-dead-letter-routing-key"] = deadLetterRoutingKey;
        return this;
    }
    
    /// <summary>
    /// Configures message TTL (time-to-live)
    /// </summary>
    /// <param name="ttl">Message TTL</param>
    /// <returns>Fluent queue API for method chaining</returns>
    public IFluentQueueApi WithMessageTtl(TimeSpan ttl)
    {
        if (ttl <= TimeSpan.Zero)
            throw new ArgumentException("TTL must be greater than zero", nameof(ttl));
        
        _arguments["x-message-ttl"] = (int)ttl.TotalMilliseconds;
        return this;
    }
    
    /// <summary>
    /// Configures maximum queue length
    /// </summary>
    /// <param name="maxLength">Maximum queue length</param>
    /// <returns>Fluent queue API for method chaining</returns>
    public IFluentQueueApi WithMaxLength(int maxLength)
    {
        if (maxLength <= 0)
            throw new ArgumentException("Max length must be greater than zero", nameof(maxLength));
        
        _arguments["x-max-length"] = maxLength;
        return this;
    }
    
    /// <summary>
    /// Configures maximum queue length in bytes
    /// </summary>
    /// <param name="maxLengthBytes">Maximum queue length in bytes</param>
    /// <returns>Fluent queue API for method chaining</returns>
    public IFluentQueueApi WithMaxLengthBytes(int maxLengthBytes)
    {
        if (maxLengthBytes <= 0)
            throw new ArgumentException("Max length bytes must be greater than zero", nameof(maxLengthBytes));
        
        _arguments["x-max-length-bytes"] = maxLengthBytes;
        return this;
    }
    
    /// <summary>
    /// Configures queue priority
    /// </summary>
    /// <param name="priority">Queue priority</param>
    /// <returns>Fluent queue API for method chaining</returns>
    public IFluentQueueApi WithPriority(int priority)
    {
        if (priority < 0 || priority > 255)
            throw new ArgumentException("Priority must be between 0 and 255", nameof(priority));
        
        _arguments["x-max-priority"] = priority;
        return this;
    }
    
    /// <summary>
    /// Binds the queue to an exchange without routing key
    /// </summary>
    /// <param name="exchange">Exchange name</param>
    /// <returns>Fluent queue API for method chaining</returns>
    public IFluentQueueApi BindToExchange(string exchange)
    {
        return BindToExchange(exchange, string.Empty, []);
    }
    
    /// <summary>
    /// Binds the queue to an exchange
    /// </summary>
    /// <param name="exchange">Exchange name</param>
    /// <param name="routingKey">Routing key</param>
    /// <returns>Fluent queue API for method chaining</returns>
    public IFluentQueueApi BindToExchange(string exchange, string routingKey)
    {
        return BindToExchange(exchange, routingKey, []);
    }
    
    /// <summary>
    /// Binds the queue to an exchange with arguments
    /// </summary>
    /// <param name="exchange">Exchange name</param>
    /// <param name="routingKey">Routing key</param>
    /// <param name="arguments">Binding arguments</param>
    /// <returns>Fluent queue API for method chaining</returns>
    public IFluentQueueApi BindToExchange(string exchange, string routingKey, Dictionary<string, object> arguments)
    {
        if (string.IsNullOrEmpty(exchange))
            throw new ArgumentException("Exchange cannot be null or empty", nameof(exchange));
        
        _bindings.Add(new FluentQueueBinding
        {
            Exchange = exchange,
            RoutingKey = routingKey,
            Arguments = arguments ?? []
        });
        
        return this;
    }
    
    /// <summary>
    /// Declares the queue with the configured settings
    /// </summary>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Queue declaration result</returns>
    public async Task<QueueDeclareResult?> DeclareAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogDebug("Declaring queue {QueueName} with durable={Durable}, exclusive={Exclusive}, autoDelete={AutoDelete}",
                _queueName, _durable, _exclusive, _autoDelete);
            
            // Declare the queue
            var result = await _queueManager.DeclareAsync(
                _queueName,
                _durable,
                _exclusive,
                _autoDelete,
                _arguments,
                cancellationToken);
            
            // Bind to exchanges if configured
            foreach (var binding in _bindings)
            {
                _logger.LogDebug("Binding queue {QueueName} to exchange {Exchange} with routing key {RoutingKey}",
                    _queueName, binding.Exchange, binding.RoutingKey);
                
                await _queueManager.BindAsync(
                    _queueName,
                    binding.Exchange,
                    binding.RoutingKey,
                    binding.Arguments,
                    cancellationToken);
            }
            
            _logger.LogInformation("Successfully declared queue {QueueName}", _queueName);
            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to declare queue {QueueName}", _queueName);
            throw;
        }
    }
    
    /// <summary>
    /// Declares the queue synchronously with the configured settings
    /// </summary>
    /// <returns>Queue declaration result</returns>
    public QueueDeclareResult? Declare()
    {
        return DeclareAsync().GetAwaiter().GetResult();
    }
    
    /// <summary>
    /// Deletes the queue
    /// </summary>
    /// <param name="ifUnused">Only delete if unused</param>
    /// <param name="ifEmpty">Only delete if empty</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Task representing the delete operation</returns>
    public async Task DeleteAsync(bool ifUnused = false, bool ifEmpty = false, CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogDebug("Deleting queue {QueueName} with ifUnused={IfUnused}, ifEmpty={IfEmpty}",
                _queueName, ifUnused, ifEmpty);
            
            await _queueManager.DeleteAsync(_queueName, ifUnused, ifEmpty, cancellationToken);
            
            _logger.LogInformation("Successfully deleted queue {QueueName}", _queueName);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to delete queue {QueueName}", _queueName);
            throw;
        }
    }
    
    /// <summary>
    /// Purges the queue (removes all messages)
    /// </summary>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Number of messages purged</returns>
    public async Task<uint> PurgeAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogDebug("Purging queue {QueueName}", _queueName);
            
            var result = await _queueManager.PurgeAsync(_queueName, cancellationToken);
            
            _logger.LogInformation("Successfully purged {MessageCount} messages from queue {QueueName}",
                result, _queueName);
            
            return result ?? 0;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to purge queue {QueueName}", _queueName);
            throw;
        }
    }
}

/// <summary>
/// Internal class for queue binding information
/// </summary>
internal class FluentQueueBinding
{
    public string Exchange { get; set; } = string.Empty;
    public string RoutingKey { get; set; } = string.Empty;
    public Dictionary<string, object> Arguments { get; set; } = new();
} 