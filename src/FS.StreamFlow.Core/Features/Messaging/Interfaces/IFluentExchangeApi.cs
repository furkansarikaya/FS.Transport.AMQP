namespace FS.StreamFlow.Core.Features.Messaging.Interfaces;

/// <summary>
/// Fluent API interface for exchange configuration and management
/// </summary>
public interface IFluentExchangeApi
{
    /// <summary>
    /// Configures the exchange type (e.g., direct, topic, fanout, headers)
    /// </summary>
    /// <param name="type">Exchange type</param>
    /// <returns>Fluent exchange API for method chaining</returns>
    IFluentExchangeApi WithType(string type);
    
    /// <summary>
    /// Configures the exchange as direct type
    /// </summary>
    /// <returns>Fluent exchange API for method chaining</returns>
    IFluentExchangeApi AsDirect();
    
    /// <summary>
    /// Configures the exchange as topic type
    /// </summary>
    /// <returns>Fluent exchange API for method chaining</returns>
    IFluentExchangeApi AsTopic();
    
    /// <summary>
    /// Configures the exchange as fanout type
    /// </summary>
    /// <returns>Fluent exchange API for method chaining</returns>
    IFluentExchangeApi AsFanout();
    
    /// <summary>
    /// Configures the exchange as headers type
    /// </summary>
    /// <returns>Fluent exchange API for method chaining</returns>
    IFluentExchangeApi AsHeaders();
    
    /// <summary>
    /// Configures whether the exchange is durable (survives server restarts)
    /// </summary>
    /// <param name="durable">Whether the exchange is durable</param>
    /// <returns>Fluent exchange API for method chaining</returns>
    IFluentExchangeApi WithDurable(bool durable = true);
    
    /// <summary>
    /// Configures whether the exchange auto-deletes when not in use
    /// </summary>
    /// <param name="autoDelete">Whether the exchange auto-deletes</param>
    /// <returns>Fluent exchange API for method chaining</returns>
    IFluentExchangeApi WithAutoDelete(bool autoDelete = true);
    
    /// <summary>
    /// Configures exchange arguments
    /// </summary>
    /// <param name="arguments">Exchange arguments dictionary</param>
    /// <returns>Fluent exchange API for method chaining</returns>
    IFluentExchangeApi WithArguments(Dictionary<string, object> arguments);
    
    /// <summary>
    /// Adds a single argument to the exchange
    /// </summary>
    /// <param name="key">Argument key</param>
    /// <param name="value">Argument value</param>
    /// <returns>Fluent exchange API for method chaining</returns>
    IFluentExchangeApi WithArgument(string key, object value);
    
    /// <summary>
    /// Configures exchange as an alternate exchange
    /// </summary>
    /// <param name="alternateExchange">Alternate exchange name</param>
    /// <returns>Fluent exchange API for method chaining</returns>
    IFluentExchangeApi WithAlternateExchange(string alternateExchange);
    
    /// <summary>
    /// Configures exchange as internal (cannot be published to directly)
    /// </summary>
    /// <param name="internal">Whether the exchange is internal</param>
    /// <returns>Fluent exchange API for method chaining</returns>
    IFluentExchangeApi WithInternal(bool @internal = true);
    
    /// <summary>
    /// Binds the exchange to another exchange without a routing key
    /// </summary>
    /// <param name="destinationExchange">Destination exchange name</param>
    /// <returns>Fluent exchange API for method chaining</returns>
    IFluentExchangeApi BindToExchange(string destinationExchange);
    
    /// <summary>
    /// Binds the exchange to another exchange
    /// </summary>
    /// <param name="destinationExchange">Destination exchange name</param>
    /// <param name="routingKey">Routing key</param>
    /// <returns>Fluent exchange API for method chaining</returns>
    IFluentExchangeApi BindToExchange(string destinationExchange, string routingKey);
    
    /// <summary>
    /// Binds the exchange to another exchange with arguments
    /// </summary>
    /// <param name="destinationExchange">Destination exchange name</param>
    /// <param name="routingKey">Routing key</param>
    /// <param name="arguments">Binding arguments</param>
    /// <returns>Fluent exchange API for method chaining</returns>
    IFluentExchangeApi BindToExchange(string destinationExchange, string routingKey, Dictionary<string, object> arguments);
    
    /// <summary>
    /// Declares the exchange with the configured settings
    /// </summary>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Task representing the declaration operation</returns>
    Task<bool> DeclareAsync(CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Declares the exchange synchronously with the configured settings
    /// </summary>
    /// <returns>True if declaration was successful</returns>
    bool Declare();
    
    /// <summary>
    /// Deletes the exchange
    /// </summary>
    /// <param name="ifUnused">Only delete if unused</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Task representing the delete operation</returns>
    Task<bool> DeleteAsync(bool ifUnused = false, CancellationToken cancellationToken = default);
} 