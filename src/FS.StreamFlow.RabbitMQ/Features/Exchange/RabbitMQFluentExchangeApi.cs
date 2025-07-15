using FS.StreamFlow.Core.Features.Messaging.Interfaces;
using FS.StreamFlow.Core.Features.Messaging.Models;
using Microsoft.Extensions.Logging;

namespace FS.StreamFlow.RabbitMQ.Features.Exchange;

/// <summary>
/// RabbitMQ implementation of fluent exchange API
/// </summary>
public class RabbitMQFluentExchangeApi : IFluentExchangeApi
{
    private readonly IExchangeManager _exchangeManager;
    private readonly ILogger _logger;
    private readonly string _exchangeName;
    
    private ExchangeType _type = ExchangeType.Topic;
    private bool _durable = true;
    private bool _autoDelete = false;
    private bool _internal = false;
    private Dictionary<string, object> _arguments = new();
    private readonly List<FluentExchangeBinding> _bindings = new();
    
    public RabbitMQFluentExchangeApi(
        IExchangeManager exchangeManager,
        string exchangeName,
        ILogger logger)
    {
        _exchangeManager = exchangeManager ?? throw new ArgumentNullException(nameof(exchangeManager));
        _exchangeName = exchangeName ?? throw new ArgumentNullException(nameof(exchangeName));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }
    
    /// <summary>
    /// Configures the exchange type (e.g., direct, topic, fanout, headers)
    /// </summary>
    /// <param name="type">Exchange type</param>
    /// <returns>Fluent exchange API for method chaining</returns>
    public IFluentExchangeApi WithType(string type)
    {
        if (string.IsNullOrEmpty(type))
            throw new ArgumentException("Exchange type cannot be null or empty", nameof(type));
        
        _type = type.ToLower() switch
        {
            "direct" => ExchangeType.Direct,
            "topic" => ExchangeType.Topic,
            "fanout" => ExchangeType.Fanout,
            "headers" => ExchangeType.Headers,
            _ => throw new ArgumentException($"Unknown exchange type: {type}", nameof(type))
        };
        return this;
    }
    
    /// <summary>
    /// Configures the exchange as direct type
    /// </summary>
    /// <returns>Fluent exchange API for method chaining</returns>
    public IFluentExchangeApi AsDirect()
    {
        _type = ExchangeType.Direct;
        return this;
    }
    
    /// <summary>
    /// Configures the exchange as topic type
    /// </summary>
    /// <returns>Fluent exchange API for method chaining</returns>
    public IFluentExchangeApi AsTopic()
    {
        _type = ExchangeType.Topic;
        return this;
    }
    
    /// <summary>
    /// Configures the exchange as fanout type
    /// </summary>
    /// <returns>Fluent exchange API for method chaining</returns>
    public IFluentExchangeApi AsFanout()
    {
        _type = ExchangeType.Fanout;
        return this;
    }
    
    /// <summary>
    /// Configures the exchange as headers type
    /// </summary>
    /// <returns>Fluent exchange API for method chaining</returns>
    public IFluentExchangeApi AsHeaders()
    {
        _type = ExchangeType.Headers;
        return this;
    }
    
    /// <summary>
    /// Configures whether the exchange is durable (survives server restarts)
    /// </summary>
    /// <param name="durable">Whether the exchange is durable</param>
    /// <returns>Fluent exchange API for method chaining</returns>
    public IFluentExchangeApi WithDurable(bool durable = true)
    {
        _durable = durable;
        return this;
    }
    
    /// <summary>
    /// Configures whether the exchange auto-deletes when not in use
    /// </summary>
    /// <param name="autoDelete">Whether the exchange auto-deletes</param>
    /// <returns>Fluent exchange API for method chaining</returns>
    public IFluentExchangeApi WithAutoDelete(bool autoDelete = true)
    {
        _autoDelete = autoDelete;
        return this;
    }
    
    /// <summary>
    /// Configures exchange arguments
    /// </summary>
    /// <param name="arguments">Exchange arguments dictionary</param>
    /// <returns>Fluent exchange API for method chaining</returns>
    public IFluentExchangeApi WithArguments(Dictionary<string, object> arguments)
    {
        _arguments = arguments ?? throw new ArgumentNullException(nameof(arguments));
        return this;
    }
    
    /// <summary>
    /// Adds a single argument to the exchange
    /// </summary>
    /// <param name="key">Argument key</param>
    /// <param name="value">Argument value</param>
    /// <returns>Fluent exchange API for method chaining</returns>
    public IFluentExchangeApi WithArgument(string key, object value)
    {
        if (string.IsNullOrEmpty(key))
            throw new ArgumentException("Key cannot be null or empty", nameof(key));
        
        _arguments[key] = value ?? throw new ArgumentNullException(nameof(value));
        return this;
    }
    
    /// <summary>
    /// Configures exchange as an alternate exchange
    /// </summary>
    /// <param name="alternateExchange">Alternate exchange name</param>
    /// <returns>Fluent exchange API for method chaining</returns>
    public IFluentExchangeApi WithAlternateExchange(string alternateExchange)
    {
        if (string.IsNullOrEmpty(alternateExchange))
            throw new ArgumentException("Alternate exchange cannot be null or empty", nameof(alternateExchange));
        
        _arguments["alternate-exchange"] = alternateExchange;
        return this;
    }
    
    /// <summary>
    /// Configures exchange as internal (cannot be published to directly)
    /// </summary>
    /// <param name="internal">Whether the exchange is internal</param>
    /// <returns>Fluent exchange API for method chaining</returns>
    public IFluentExchangeApi WithInternal(bool @internal = true)
    {
        _internal = @internal;
        return this;
    }
    
    /// <summary>
    /// Binds the exchange to another exchange
    /// </summary>
    /// <param name="destinationExchange">Destination exchange name</param>
    /// <param name="routingKey">Routing key</param>
    /// <returns>Fluent exchange API for method chaining</returns>
    public IFluentExchangeApi BindToExchange(string destinationExchange, string routingKey)
    {
        return BindToExchange(destinationExchange, routingKey, new Dictionary<string, object>());
    }
    
    /// <summary>
    /// Binds the exchange to another exchange with arguments
    /// </summary>
    /// <param name="destinationExchange">Destination exchange name</param>
    /// <param name="routingKey">Routing key</param>
    /// <param name="arguments">Binding arguments</param>
    /// <returns>Fluent exchange API for method chaining</returns>
    public IFluentExchangeApi BindToExchange(string destinationExchange, string routingKey, Dictionary<string, object> arguments)
    {
        if (string.IsNullOrEmpty(destinationExchange))
            throw new ArgumentException("Destination exchange cannot be null or empty", nameof(destinationExchange));
        
        if (string.IsNullOrEmpty(routingKey))
            throw new ArgumentException("Routing key cannot be null or empty", nameof(routingKey));
        
        _bindings.Add(new FluentExchangeBinding
        {
            DestinationExchange = destinationExchange,
            RoutingKey = routingKey,
            Arguments = arguments ?? new Dictionary<string, object>()
        });
        
        return this;
    }
    
    /// <summary>
    /// Declares the exchange with the configured settings
    /// </summary>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Task representing the declaration operation</returns>
    public async Task<bool> DeclareAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogDebug("Declaring exchange {ExchangeName} with type={Type}, durable={Durable}, autoDelete={AutoDelete}, internal={Internal}",
                _exchangeName, _type, _durable, _autoDelete, _internal);
            
            // Declare the exchange
            var result = await _exchangeManager.DeclareAsync(
                new ExchangeSettings
                {
                    Name = _exchangeName,
                    Type = _type,
                    Durable = _durable,
                    AutoDelete = _autoDelete,
                    Arguments = _arguments
                },
                cancellationToken);
            
            // Bind to other exchanges if configured
            foreach (var binding in _bindings)
            {
                _logger.LogDebug("Binding exchange {ExchangeName} to exchange {DestinationExchange} with routing key {RoutingKey}",
                    _exchangeName, binding.DestinationExchange, binding.RoutingKey);
                
                await _exchangeManager.BindAsync(
                    binding.DestinationExchange,
                    _exchangeName,
                    binding.RoutingKey,
                    binding.Arguments,
                    cancellationToken);
            }
            
            _logger.LogInformation("Successfully declared exchange {ExchangeName}", _exchangeName);
            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to declare exchange {ExchangeName}", _exchangeName);
            throw;
        }
    }
    
    /// <summary>
    /// Declares the exchange synchronously with the configured settings
    /// </summary>
    /// <returns>True if declaration was successful</returns>
    public bool Declare()
    {
        return DeclareAsync().GetAwaiter().GetResult();
    }
    
    /// <summary>
    /// Deletes the exchange
    /// </summary>
    /// <param name="ifUnused">Only delete if unused</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Task representing the delete operation</returns>
    public async Task<bool> DeleteAsync(bool ifUnused = false, CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogDebug("Deleting exchange {ExchangeName} with ifUnused={IfUnused}",
                _exchangeName, ifUnused);
            
            var result = await _exchangeManager.DeleteAsync(_exchangeName, ifUnused, cancellationToken);
            
            _logger.LogInformation("Successfully deleted exchange {ExchangeName}", _exchangeName);
            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to delete exchange {ExchangeName}", _exchangeName);
            throw;
        }
    }
}

/// <summary>
/// Internal class for exchange binding information
/// </summary>
internal class FluentExchangeBinding
{
    public string DestinationExchange { get; set; } = string.Empty;
    public string RoutingKey { get; set; } = string.Empty;
    public Dictionary<string, object> Arguments { get; set; } = new();
} 