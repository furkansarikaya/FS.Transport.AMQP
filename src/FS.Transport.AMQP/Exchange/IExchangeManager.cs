using FS.Transport.AMQP.Configuration;
using RabbitMQ.Client;

namespace FS.Transport.AMQP.Exchange;

/// <summary>
/// Interface for managing RabbitMQ exchanges with declaration, binding, and auto-recovery capabilities
/// </summary>
public interface IExchangeManager
{
    /// <summary>
    /// Declares an exchange with the specified configuration
    /// </summary>
    /// <param name="exchangeSettings">Exchange configuration</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>True if exchange was declared successfully</returns>
    Task<bool> DeclareAsync(ExchangeSettings exchangeSettings, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Declares an exchange with manual parameters
    /// </summary>
    /// <param name="name">Exchange name</param>
    /// <param name="type">Exchange type</param>
    /// <param name="durable">Whether exchange is durable</param>
    /// <param name="autoDelete">Whether exchange auto-deletes</param>
    /// <param name="arguments">Additional arguments</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>True if exchange was declared successfully</returns>
    Task<bool> DeclareAsync(
        string name, 
        string type = ExchangeType.Topic, 
        bool durable = true, 
        bool autoDelete = false, 
        IDictionary<string, object>? arguments = null,
        CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Deletes an exchange
    /// </summary>
    /// <param name="name">Exchange name</param>
    /// <param name="ifUnused">Only delete if unused</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>True if exchange was deleted successfully</returns>
    Task<bool> DeleteAsync(string name, bool ifUnused = false, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Binds one exchange to another
    /// </summary>
    /// <param name="destination">Destination exchange name</param>
    /// <param name="source">Source exchange name</param>
    /// <param name="routingKey">Routing key for binding</param>
    /// <param name="arguments">Additional arguments</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>True if binding was created successfully</returns>
    Task<bool> BindAsync(
        string destination, 
        string source, 
        string routingKey, 
        IDictionary<string, object>? arguments = null,
        CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Unbinds one exchange from another
    /// </summary>
    /// <param name="destination">Destination exchange name</param>
    /// <param name="source">Source exchange name</param>
    /// <param name="routingKey">Routing key for binding</param>
    /// <param name="arguments">Additional arguments</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>True if binding was removed successfully</returns>
    Task<bool> UnbindAsync(
        string destination, 
        string source, 
        string routingKey, 
        IDictionary<string, object>? arguments = null,
        CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Checks if an exchange exists
    /// </summary>
    /// <param name="name">Exchange name</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>True if exchange exists</returns>
    Task<bool> ExistsAsync(string name, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Gets information about an exchange
    /// </summary>
    /// <param name="name">Exchange name</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Exchange information or null if not found</returns>
    Task<ExchangeInfo?> GetInfoAsync(string name, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Declares all configured exchanges from settings
    /// </summary>
    /// <param name="exchanges">Exchange configurations</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>True if all exchanges were declared successfully</returns>
    Task<bool> DeclareAllAsync(IEnumerable<ExchangeSettings> exchanges, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Re-declares all registered exchanges (used for auto-recovery)
    /// </summary>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>True if all exchanges were re-declared successfully</returns>
    Task<bool> RedeclareAllAsync(CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Gets exchange management statistics
    /// </summary>
    /// <returns>Exchange statistics</returns>
    ExchangeStatistics GetStatistics();
    
    /// <summary>
    /// Event raised when an exchange is declared
    /// </summary>
    event EventHandler<ExchangeEventArgs>? ExchangeDeclared;
    
    /// <summary>
    /// Event raised when an exchange is deleted
    /// </summary>
    event EventHandler<ExchangeEventArgs>? ExchangeDeleted;
    
    /// <summary>
    /// Event raised when exchanges are being recreated (auto-recovery)
    /// </summary>
    event EventHandler<ExchangeEventArgs>? ExchangesRecreating;
    
    /// <summary>
    /// Event raised when exchanges recreation is completed
    /// </summary>
    event EventHandler<ExchangeEventArgs>? ExchangesRecreated;
}