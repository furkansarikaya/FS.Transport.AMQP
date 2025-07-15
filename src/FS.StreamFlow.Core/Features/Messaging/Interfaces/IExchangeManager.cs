using FS.StreamFlow.Core.Features.Messaging.Models;

namespace FS.StreamFlow.Core.Features.Messaging.Interfaces;

/// <summary>
/// Interface for managing messaging exchanges with declaration, binding, and management operations
/// </summary>
public interface IExchangeManager : IDisposable
{
    /// <summary>
    /// Event raised when an exchange is successfully declared
    /// </summary>
    event EventHandler<ExchangeEventArgs>? ExchangeDeclared;
    
    /// <summary>
    /// Event raised when an exchange is deleted
    /// </summary>
    event EventHandler<ExchangeEventArgs>? ExchangeDeleted;
    
    /// <summary>
    /// Event raised when exchanges are being recreated during recovery
    /// </summary>
    event EventHandler<ExchangeEventArgs>? ExchangesRecreating;
    
    /// <summary>
    /// Event raised when exchanges have been successfully recreated during recovery
    /// </summary>
    event EventHandler<ExchangeEventArgs>? ExchangesRecreated;
    
    /// <summary>
    /// Declares an exchange with the specified settings
    /// </summary>
    /// <param name="exchangeSettings">Exchange configuration settings</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Task representing the declaration operation</returns>
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
    /// <returns>Task representing the declaration operation</returns>
    Task<bool> DeclareAsync(
        string name, 
        ExchangeType type, 
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
    /// <returns>Task representing the deletion operation</returns>
    Task<bool> DeleteAsync(string name, bool ifUnused = false, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Binds an exchange to another exchange
    /// </summary>
    /// <param name="source">Source exchange name</param>
    /// <param name="destination">Destination exchange name</param>
    /// <param name="routingKey">Routing key</param>
    /// <param name="arguments">Additional arguments</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Task representing the binding operation</returns>
    Task<bool> BindAsync(
        string source, 
        string destination, 
        string routingKey, 
        IDictionary<string, object>? arguments = null,
        CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Unbinds an exchange from another exchange
    /// </summary>
    /// <param name="source">Source exchange name</param>
    /// <param name="destination">Destination exchange name</param>
    /// <param name="routingKey">Routing key</param>
    /// <param name="arguments">Additional arguments</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Task representing the unbinding operation</returns>
    Task<bool> UnbindAsync(
        string source, 
        string destination, 
        string routingKey, 
        IDictionary<string, object>? arguments = null,
        CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Gets information about an exchange
    /// </summary>
    /// <param name="name">Exchange name</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Exchange information</returns>
    Task<ExchangeInfo?> GetExchangeInfoAsync(string name, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Gets statistics for all exchanges
    /// </summary>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Exchange statistics</returns>
    Task<ExchangeStatistics> GetStatisticsAsync(CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Creates a fluent API for exchange configuration and management
    /// </summary>
    /// <param name="exchangeName">Exchange name</param>
    /// <returns>Fluent exchange API for method chaining</returns>
    IFluentExchangeApi Exchange(string exchangeName);
} 