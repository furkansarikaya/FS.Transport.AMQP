using FS.StreamFlow.Core.Features.Messaging.Interfaces;
using FS.StreamFlow.Core.Features.Messaging.Models;
using FS.StreamFlow.RabbitMQ.Features.Connection;
using Microsoft.Extensions.Logging;
using System.Collections.Concurrent;
using CoreExchangeType = FS.StreamFlow.Core.Features.Messaging.Models.ExchangeType;
using CoreChannel = FS.StreamFlow.Core.Features.Messaging.Models.IChannel;
using RabbitChannel = RabbitMQ.Client.IChannel;

namespace FS.StreamFlow.RabbitMQ.Features.Exchange;

/// <summary>
/// RabbitMQ implementation of exchange management with declaration, binding, and management operations.
/// Provides comprehensive exchange management with connection recovery, statistics tracking, and error handling.
/// </summary>
public class RabbitMQExchangeManager : IExchangeManager
{
    private readonly IConnectionManager _connectionManager;
    private readonly ILogger<RabbitMQExchangeManager> _logger;
    private readonly ConcurrentDictionary<string, ExchangeSettings> _declaredExchanges;
    private readonly ConcurrentDictionary<string, ExchangeBinding> _exchangeBindings;
    private readonly ExchangeStatistics _statistics;
    private readonly Timer _statisticsTimer;
    private volatile bool _disposed;

    /// <summary>
    /// Event raised when an exchange is successfully declared
    /// </summary>
    public event EventHandler<ExchangeEventArgs>? ExchangeDeclared;

    /// <summary>
    /// Event raised when an exchange is deleted
    /// </summary>
    public event EventHandler<ExchangeEventArgs>? ExchangeDeleted;

    /// <summary>
    /// Event raised when exchanges are being recreated during recovery
    /// </summary>
    public event EventHandler<ExchangeEventArgs>? ExchangesRecreating;

    /// <summary>
    /// Event raised when exchanges have been successfully recreated during recovery
    /// </summary>
    public event EventHandler<ExchangeEventArgs>? ExchangesRecreated;

    /// <summary>
    /// Initializes a new instance of the RabbitMQExchangeManager class.
    /// </summary>
    /// <param name="connectionManager">Connection manager for RabbitMQ connections</param>
    /// <param name="logger">Logger instance for diagnostics</param>
    public RabbitMQExchangeManager(
        IConnectionManager connectionManager,
        ILogger<RabbitMQExchangeManager> logger)
    {
        _connectionManager = connectionManager ?? throw new ArgumentNullException(nameof(connectionManager));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        
        _declaredExchanges = new ConcurrentDictionary<string, ExchangeSettings>();
        _exchangeBindings = new ConcurrentDictionary<string, ExchangeBinding>();
        _statistics = new ExchangeStatistics();
        
        // Initialize statistics timer
        _statisticsTimer = new Timer(UpdateStatistics, null, TimeSpan.FromSeconds(30), TimeSpan.FromSeconds(30));
        
        // Subscribe to connection events for auto-recovery
        _connectionManager.Connected += OnConnectionRecovered;
        _connectionManager.Recovered += OnConnectionRecovered;
        
        _logger.LogInformation("RabbitMQ Exchange Manager initialized successfully");
    }

    /// <summary>
    /// Declares an exchange with the specified settings
    /// </summary>
    /// <param name="exchangeSettings">Exchange configuration settings</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Task representing the declaration operation</returns>
    public async Task<bool> DeclareAsync(ExchangeSettings exchangeSettings, CancellationToken cancellationToken = default)
    {
        if (exchangeSettings == null)
            throw new ArgumentNullException(nameof(exchangeSettings));

        if (string.IsNullOrWhiteSpace(exchangeSettings.Name))
            throw new ArgumentException("Exchange name cannot be null or empty", nameof(exchangeSettings));

        return await DeclareAsync(
            exchangeSettings.Name,
            exchangeSettings.Type,
            exchangeSettings.Durable,
            exchangeSettings.AutoDelete,
            exchangeSettings.Arguments,
            cancellationToken);
    }

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
    public async Task<bool> DeclareAsync(
        string name,
        CoreExchangeType type,
        bool durable = true,
        bool autoDelete = false,
        IDictionary<string, object>? arguments = null,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Exchange name cannot be null or empty", nameof(name));

        if (_disposed)
            throw new ObjectDisposedException(nameof(RabbitMQExchangeManager));

        try
        {
            var channel = await _connectionManager.GetChannelAsync(cancellationToken);
            if (channel == null)
            {
                _logger.LogWarning("Failed to get channel for exchange declaration: {ExchangeName}", name);
                _statistics.FailedRoutings++;
                return false;
            }

            try
            {
                // Get the underlying RabbitMQ channel
                var rabbitChannel = GetRabbitChannel(channel);
                var exchangeType = MapExchangeType(type);
                var args = arguments?.ToDictionary(kvp => kvp.Key, kvp => kvp.Value);

                await rabbitChannel.ExchangeDeclareAsync(
                    exchange: name,
                    type: exchangeType,
                    durable: durable,
                    autoDelete: autoDelete,
                    arguments: args,
                    cancellationToken: cancellationToken).ConfigureAwait(false);

                // Store exchange settings for recovery
                var exchangeSettings = new ExchangeSettings
                {
                    Name = name,
                    Type = type,
                    Durable = durable,
                    AutoDelete = autoDelete,
                    Arguments = args
                };

                _declaredExchanges.TryAdd(name, exchangeSettings);
                _statistics.TotalExchanges++;

                _logger.LogInformation("Exchange declared successfully: {ExchangeName} ({ExchangeType})", name, type);
                
                // Raise event
                ExchangeDeclared?.Invoke(this, new ExchangeEventArgs(name, type));
                
                return true;
            }
            finally
            {
                await _connectionManager.ReturnChannelAsync(channel, cancellationToken);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to declare exchange: {ExchangeName}", name);
            _statistics.FailedRoutings++;
            return false;
        }
    }

    /// <summary>
    /// Deletes an exchange
    /// </summary>
    /// <param name="name">Exchange name</param>
    /// <param name="ifUnused">Only delete if unused</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Task representing the deletion operation</returns>
    public async Task<bool> DeleteAsync(string name, bool ifUnused = false, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Exchange name cannot be null or empty", nameof(name));

        if (_disposed)
            throw new ObjectDisposedException(nameof(RabbitMQExchangeManager));

        try
        {
            var channel = await _connectionManager.GetChannelAsync(cancellationToken);
            if (channel == null)
            {
                _logger.LogWarning("Failed to get channel for exchange deletion: {ExchangeName}", name);
                return false;
            }

            try
            {
                var rabbitChannel = GetRabbitChannel(channel);

                await rabbitChannel.ExchangeDeleteAsync(
                    exchange: name,
                    ifUnused: ifUnused,
                    cancellationToken: cancellationToken);

                // Remove from declared exchanges
                if (_declaredExchanges.TryRemove(name, out _))
                {
                    _statistics.TotalExchanges--;
                }

                _logger.LogInformation("Exchange deleted successfully: {ExchangeName}", name);
                
                // Raise event - we don't know the type, so use Direct as default
                ExchangeDeleted?.Invoke(this, new ExchangeEventArgs(name, CoreExchangeType.Direct));
                
                return true;
            }
            finally
            {
                await _connectionManager.ReturnChannelAsync(channel, cancellationToken);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to delete exchange: {ExchangeName}", name);
            return false;
        }
    }

    /// <summary>
    /// Binds an exchange to another exchange without routing key
    /// </summary>
    /// <param name="source">Source exchange name</param>
    /// <param name="destination">Destination exchange name</param>
    /// <param name="arguments">Additional arguments</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Task representing the binding operation</returns>
    public async Task<bool> BindAsync(
        string source,
        string destination,
        IDictionary<string, object>? arguments = null,
        CancellationToken cancellationToken = default)
    {
        return await BindAsync(source, destination, string.Empty, arguments, cancellationToken);
    }

    /// <summary>
    /// Binds an exchange to another exchange
    /// </summary>
    /// <param name="source">Source exchange name</param>
    /// <param name="destination">Destination exchange name</param>
    /// <param name="routingKey">Routing key</param>
    /// <param name="arguments">Additional arguments</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Task representing the binding operation</returns>
    public async Task<bool> BindAsync(
        string source,
        string destination,
        string routingKey,
        IDictionary<string, object>? arguments = null,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(source))
            throw new ArgumentException("Source exchange name cannot be null or empty", nameof(source));

        if (string.IsNullOrWhiteSpace(destination))
            throw new ArgumentException("Destination exchange name cannot be null or empty", nameof(destination));

        if (_disposed)
            throw new ObjectDisposedException(nameof(RabbitMQExchangeManager));

        try
        {
            var channel = await _connectionManager.GetChannelAsync(cancellationToken);
            if (channel == null)
            {
                _logger.LogWarning("Failed to get channel for exchange binding: {Source} -> {Destination}", source, destination);
                return false;
            }

            try
            {
                var rabbitChannel = GetRabbitChannel(channel);
                var args = arguments?.ToDictionary(kvp => kvp.Key, kvp => kvp.Value);

                await rabbitChannel.ExchangeBindAsync(
                    destination: destination,
                    source: source,
                    routingKey: routingKey,
                    arguments: args,
                    cancellationToken: cancellationToken);

                // Store binding for recovery
                var bindingKey = $"{source}|{destination}|{routingKey}";
                var binding = new ExchangeBinding
                {
                    Source = source,
                    Destination = destination,
                    RoutingKey = routingKey,
                    Arguments = args
                };

                _exchangeBindings.TryAdd(bindingKey, binding);

                _logger.LogInformation("Exchange binding created successfully: {Source} -> {Destination} (Key: {RoutingKey})", 
                    source, destination, routingKey);
                
                return true;
            }
            finally
            {
                await _connectionManager.ReturnChannelAsync(channel, cancellationToken);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to bind exchange: {Source} -> {Destination} (Key: {RoutingKey})", 
                source, destination, routingKey);
            return false;
        }
    }

    /// <summary>
    /// Unbinds an exchange from another exchange without routing key
    /// </summary>
    /// <param name="source">Source exchange name</param>
    /// <param name="destination">Destination exchange name</param>
    /// <param name="arguments">Additional arguments</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Task representing the unbinding operation</returns>
    public async Task<bool> UnbindAsync(
        string source,
        string destination,
        IDictionary<string, object>? arguments = null,
        CancellationToken cancellationToken = default)
    {
        return await UnbindAsync(source, destination, string.Empty, arguments, cancellationToken);
    }

    /// <summary>
    /// Unbinds an exchange from another exchange
    /// </summary>
    /// <param name="source">Source exchange name</param>
    /// <param name="destination">Destination exchange name</param>
    /// <param name="routingKey">Routing key</param>
    /// <param name="arguments">Additional arguments</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Task representing the unbinding operation</returns>
    public async Task<bool> UnbindAsync(
        string source,
        string destination,
        string routingKey,
        IDictionary<string, object>? arguments = null,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(source))
            throw new ArgumentException("Source exchange name cannot be null or empty", nameof(source));

        if (string.IsNullOrWhiteSpace(destination))
            throw new ArgumentException("Destination exchange name cannot be null or empty", nameof(destination));

        if (_disposed)
            throw new ObjectDisposedException(nameof(RabbitMQExchangeManager));

        try
        {
            var channel = await _connectionManager.GetChannelAsync(cancellationToken);
            if (channel == null)
            {
                _logger.LogWarning("Failed to get channel for exchange unbinding: {Source} -> {Destination}", source, destination);
                return false;
            }

            try
            {
                var rabbitChannel = GetRabbitChannel(channel);
                var args = arguments?.ToDictionary(kvp => kvp.Key, kvp => kvp.Value);

                await rabbitChannel.ExchangeUnbindAsync(
                    destination: destination,
                    source: source,
                    routingKey: routingKey,
                    arguments: args,
                    cancellationToken: cancellationToken);

                // Remove binding from storage
                var bindingKey = $"{source}|{destination}|{routingKey}";
                _exchangeBindings.TryRemove(bindingKey, out _);

                _logger.LogInformation("Exchange binding removed successfully: {Source} -> {Destination} (Key: {RoutingKey})", 
                    source, destination, routingKey);
                
                return true;
            }
            finally
            {
                await _connectionManager.ReturnChannelAsync(channel, cancellationToken);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to unbind exchange: {Source} -> {Destination} (Key: {RoutingKey})", 
                source, destination, routingKey);
            return false;
        }
    }

    /// <summary>
    /// Gets information about an exchange
    /// </summary>
    /// <param name="name">Exchange name</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Exchange information</returns>
    public Task<ExchangeInfo?> GetExchangeInfoAsync(string name, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Exchange name cannot be null or empty", nameof(name));

        if (_disposed)
            throw new ObjectDisposedException(nameof(RabbitMQExchangeManager));

        try
        {
            // Check if exchange is in our declared exchanges
            if (_declaredExchanges.TryGetValue(name, out var exchangeSettings))
            {
                return Task.FromResult(new ExchangeInfo
                {
                    Name = name,
                    Type = exchangeSettings.Type,
                    Durable = exchangeSettings.Durable,
                    AutoDelete = exchangeSettings.AutoDelete,
                    Arguments = exchangeSettings.Arguments ?? new Dictionary<string, object>(),
                    Bindings = GetExchangeBindings(name)
                });
            }

            _logger.LogWarning("Exchange not found in declared exchanges: {ExchangeName}", name);
            return Task.FromResult<ExchangeInfo?>(null);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get exchange info: {ExchangeName}", name);
            return Task.FromResult<ExchangeInfo?>(null);
        }
    }

    /// <summary>
    /// Gets statistics for all exchanges
    /// </summary>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Exchange statistics</returns>
    public async Task<ExchangeStatistics> GetStatisticsAsync(CancellationToken cancellationToken = default)
    {
        if (_disposed)
            throw new ObjectDisposedException(nameof(RabbitMQExchangeManager));

        await Task.CompletedTask; // Make async for interface compliance
        return _statistics;
    }

    /// <summary>
    /// Maps StreamFlow exchange type to RabbitMQ exchange type
    /// </summary>
    /// <param name="type">StreamFlow exchange type</param>
    /// <returns>RabbitMQ exchange type string</returns>
    private static string MapExchangeType(CoreExchangeType type)
    {
        return type switch
        {
            CoreExchangeType.Direct => "direct",
            CoreExchangeType.Topic => "topic",
            CoreExchangeType.Headers => "headers",
            CoreExchangeType.Fanout => "fanout",
            _ => throw new ArgumentException($"Unsupported exchange type: {type}", nameof(type))
        };
    }

    /// <summary>
    /// Gets the underlying RabbitMQ channel from the abstraction
    /// </summary>
    /// <param name="channel">The abstracted channel</param>
    /// <returns>RabbitMQ channel</returns>
    private static RabbitChannel GetRabbitChannel(CoreChannel channel)
    {
        // Try to get the underlying RabbitMQ channel
        if (channel is RabbitMQChannel rabbitChannel)
        {
            return rabbitChannel.GetNativeChannel();
        }
        
        throw new InvalidOperationException("Channel is not a RabbitMQ channel");
    }

    /// <summary>
    /// Gets exchange bindings for a specific exchange
    /// </summary>
    /// <param name="exchangeName">Exchange name</param>
    /// <returns>List of exchange bindings</returns>
    private List<ExchangeBinding> GetExchangeBindings(string exchangeName)
    {
        var bindings = new List<ExchangeBinding>();
        
        foreach (var binding in _exchangeBindings.Values)
        {
            if (binding.Source == exchangeName || binding.Destination == exchangeName)
            {
                bindings.Add(binding);
            }
        }
        
        return bindings;
    }

    /// <summary>
    /// Handles connection recovery by redeclaring exchanges and bindings
    /// </summary>
    /// <param name="sender">Event sender</param>
    /// <param name="e">Connection event args</param>
    private async void OnConnectionRecovered(object? sender, ConnectionEventArgs e)
    {
        if (_disposed)
            return;

        try
        {
            _logger.LogInformation("Connection recovered, redeclaring exchanges and bindings...");
            
            // Raise recreating event
            ExchangesRecreating?.Invoke(this, new ExchangeEventArgs("all", CoreExchangeType.Direct));

            // Redeclare exchanges
            foreach (var exchange in _declaredExchanges.Values)
            {
                await DeclareAsync(exchange);
            }

            // Recreate bindings
            foreach (var binding in _exchangeBindings.Values)
            {
                await BindAsync(binding.Source, binding.Destination, binding.RoutingKey, binding.Arguments);
            }

            _logger.LogInformation("All exchanges and bindings recreated successfully");
            
            // Raise recreated event
            ExchangesRecreated?.Invoke(this, new ExchangeEventArgs("all", CoreExchangeType.Direct));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to recreate exchanges and bindings after connection recovery");
        }
    }

    /// <summary>
    /// Updates exchange statistics
    /// </summary>
    /// <param name="state">Timer state</param>
    private void UpdateStatistics(object? state)
    {
        if (_disposed)
            return;

        try
        {
            _statistics.TotalExchanges = _declaredExchanges.Count;
            _statistics.DurableExchanges = _declaredExchanges.Values.Count(e => e.Durable);
            _statistics.AutoDeleteExchanges = _declaredExchanges.Values.Count(e => e.AutoDelete);
            _statistics.Timestamp = DateTimeOffset.UtcNow;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to update exchange statistics");
        }
    }

    /// <summary>
    /// Creates a fluent API for exchange configuration and management
    /// </summary>
    /// <param name="exchangeName">Exchange name</param>
    /// <returns>Fluent exchange API for method chaining</returns>
    /// <exception cref="ArgumentException">Thrown when exchangeName is null or empty</exception>
    /// <exception cref="ObjectDisposedException">Thrown when the exchange manager has been disposed</exception>
    public IFluentExchangeApi Exchange(string exchangeName)
    {
        if (_disposed)
            throw new ObjectDisposedException(nameof(RabbitMQExchangeManager));
        
        if (string.IsNullOrEmpty(exchangeName))
            throw new ArgumentException("Exchange name cannot be null or empty", nameof(exchangeName));
        
        _logger.LogDebug("Creating fluent exchange API for exchange {ExchangeName}", exchangeName);
        
        return new RabbitMQFluentExchangeApi(this, exchangeName, _logger);
    }

    /// <summary>
    /// Disposes the exchange manager and releases all resources
    /// </summary>
    public void Dispose()
    {
        if (_disposed)
            return;

        _disposed = true;
        
        try
        {
            _statisticsTimer?.Dispose();
            
            // Unsubscribe from connection events
            _connectionManager.Connected -= OnConnectionRecovered;
            _connectionManager.Recovered -= OnConnectionRecovered;
            
            _logger.LogInformation("RabbitMQ Exchange Manager disposed successfully");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while disposing RabbitMQ Exchange Manager");
        }
    }
} 