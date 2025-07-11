using System.Collections.Concurrent;
using FS.RabbitMQ.Configuration;
using FS.RabbitMQ.Connection;
using FS.RabbitMQ.Core.Exceptions;
using FS.RabbitMQ.ErrorHandling;
using FS.RabbitMQ.RetryPolicies;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using RabbitMQ.Client;
using RabbitMQ.Client.Exceptions;

namespace FS.RabbitMQ.Exchange;

/// <summary>
/// Production-ready exchange manager with auto-recovery, error handling, and comprehensive monitoring
/// </summary>
public class ExchangeManager : IExchangeManager
{
    private readonly RabbitMQConfiguration _configuration;
    private readonly IConnectionManager _connectionManager;
    private readonly IErrorHandler _errorHandler;
    private readonly IRetryPolicy _retryPolicy;
    private readonly ILogger<ExchangeManager> _logger;
    private readonly ExchangeStatistics _statistics;
    private readonly ConcurrentDictionary<string, ExchangeDeclaration> _declaredExchanges;
    private readonly SemaphoreSlim _declarationSemaphore;

    /// <summary>
    /// Occurs when an exchange is successfully declared
    /// </summary>
    public event EventHandler<ExchangeEventArgs>? ExchangeDeclared;
    
    /// <summary>
    /// Occurs when an exchange is deleted
    /// </summary>
    public event EventHandler<ExchangeEventArgs>? ExchangeDeleted;
    
    /// <summary>
    /// Occurs when exchanges are being recreated during recovery
    /// </summary>
    public event EventHandler<ExchangeEventArgs>? ExchangesRecreating;
    
    /// <summary>
    /// Occurs when exchanges have been successfully recreated during recovery
    /// </summary>
    public event EventHandler<ExchangeEventArgs>? ExchangesRecreated;

    /// <summary>
    /// Initializes a new instance of the <see cref="ExchangeManager"/> class
    /// </summary>
    /// <param name="configuration">RabbitMQ configuration settings</param>
    /// <param name="connectionManager">Connection manager for RabbitMQ connectivity</param>
    /// <param name="errorHandler">Error handler for processing failures</param>
    /// <param name="retryPolicy">Retry policy for failed operations</param>
    /// <param name="logger">Logger for exchange management activities</param>
    /// <exception cref="ArgumentNullException">
    /// Thrown when any required parameter is null
    /// </exception>
    public ExchangeManager(
        IOptions<RabbitMQConfiguration> configuration,
        IConnectionManager connectionManager,
        IErrorHandler errorHandler,
        IRetryPolicy retryPolicy,
        ILogger<ExchangeManager> logger)
    {
        _configuration = configuration?.Value ?? throw new ArgumentNullException(nameof(configuration));
        _connectionManager = connectionManager ?? throw new ArgumentNullException(nameof(connectionManager));
        _errorHandler = errorHandler ?? throw new ArgumentNullException(nameof(errorHandler));
        _retryPolicy = retryPolicy ?? throw new ArgumentNullException(nameof(retryPolicy));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _statistics = new ExchangeStatistics();
        _declaredExchanges = new ConcurrentDictionary<string, ExchangeDeclaration>();
        _declarationSemaphore = new SemaphoreSlim(1, 1);

        _logger.LogDebug("ExchangeManager initialized");
    }

    /// <summary>
    /// Declares an exchange using configuration settings with automatic retry and error handling
    /// </summary>
    /// <param name="exchangeSettings">Exchange configuration settings</param>
    /// <param name="cancellationToken">Token to monitor for cancellation requests</param>
    /// <returns>
    /// A task that represents the asynchronous declaration operation.
    /// The task result contains <c>true</c> if declaration was successful; otherwise, <c>false</c>.
    /// </returns>
    /// <exception cref="ArgumentNullException">
    /// Thrown when exchangeSettings is null
    /// </exception>
    /// <exception cref="ArgumentException">
    /// Thrown when exchange settings validation fails
    /// </exception>
    /// <exception cref="ExchangeDeclarationException">
    /// Thrown when exchange declaration fails
    /// </exception>
    public async Task<bool> DeclareAsync(ExchangeSettings exchangeSettings, CancellationToken cancellationToken = default)
    {
        if (exchangeSettings == null)
            throw new ArgumentNullException(nameof(exchangeSettings));

        exchangeSettings.Validate();

        return await DeclareAsync(
            exchangeSettings.Name,
            exchangeSettings.Type,
            exchangeSettings.Durable,
            exchangeSettings.AutoDelete,
            exchangeSettings.Arguments,
            cancellationToken);
    }

    /// <summary>
    /// Declares an exchange with manual parameters and automatic retry logic
    /// </summary>
    /// <param name="name">Exchange name</param>
    /// <param name="type">Exchange type (topic, direct, fanout, headers)</param>
    /// <param name="durable">Whether the exchange survives server restarts</param>
    /// <param name="autoDelete">Whether the exchange is deleted when not in use</param>
    /// <param name="arguments">Additional exchange arguments</param>
    /// <param name="cancellationToken">Token to monitor for cancellation requests</param>
    /// <returns>
    /// A task that represents the asynchronous declaration operation.
    /// The task result contains <c>true</c> if declaration was successful; otherwise, <c>false</c>.
    /// </returns>
    /// <exception cref="ArgumentException">
    /// Thrown when exchange name is null or empty
    /// </exception>
    /// <exception cref="ExchangeDeclarationException">
    /// Thrown when exchange declaration fails
    /// </exception>
    public async Task<bool> DeclareAsync(
        string name, 
        string type = ExchangeType.Topic, 
        bool durable = true, 
        bool autoDelete = false, 
        IDictionary<string, object>? arguments = null,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Exchange name cannot be null or empty", nameof(name));

        await _declarationSemaphore.WaitAsync(cancellationToken);
        try
        {
            _statistics.TotalOperations++;
            var startTime = DateTime.UtcNow;

            var declaration = new ExchangeDeclaration(name, type, durable, autoDelete, arguments);
            
            _logger.LogInformation("Declaring exchange: {ExchangeName} (Type: {Type}, Durable: {Durable})", 
                name, type, durable);

            var success = await _retryPolicy.ExecuteAsync(async () =>
            {
                using var channel = await _connectionManager.GetChannelAsync(cancellationToken);
                
                try
                {
                    channel.ExchangeDeclare(
                        exchange: name,
                        type: type,
                        durable: durable,
                        autoDelete: autoDelete,
                        arguments: arguments);

                    _connectionManager.ReturnChannel(channel);
                    return true;
                }
                catch (Exception ex)
                {
                    _connectionManager.ReturnChannel(channel);
                    
                    _logger.LogError(ex, "Failed to declare exchange {ExchangeName}: {ErrorMessage}", name, ex.Message);
                    throw new ExchangeDeclarationException(name, ex);
                }
            }, cancellationToken);

            var duration = DateTime.UtcNow - startTime;
            
            if (success)
            {
                // Store the declaration for auto-recovery
                _declaredExchanges.AddOrUpdate(name, declaration, (key, existing) => declaration);
                
                _statistics.SuccessfulOperations++;
                _statistics.DeclaredExchanges++;
                _statistics.AverageOperationTime = CalculateAverageTime(duration);
                
                _logger.LogInformation("Exchange {ExchangeName} declared successfully in {Duration}ms", 
                    name, duration.TotalMilliseconds);
                
                ExchangeDeclared?.Invoke(this, new ExchangeEventArgs(name, ExchangeOperation.Declare, success));
            }
            else
            {
                _statistics.FailedOperations++;
                _logger.LogError("Failed to declare exchange {ExchangeName} after retries", name);
            }

            return success;
        }
        finally
        {
            _declarationSemaphore.Release();
        }
    }

    /// <summary>
    /// Deletes an exchange with optional condition checking
    /// </summary>
    /// <param name="name">Exchange name to delete</param>
    /// <param name="ifUnused">Only delete if the exchange is unused</param>
    /// <param name="cancellationToken">Token to monitor for cancellation requests</param>
    /// <returns>
    /// A task that represents the asynchronous delete operation.
    /// The task result contains <c>true</c> if deletion was successful; otherwise, <c>false</c>.
    /// </returns>
    /// <exception cref="ArgumentException">
    /// Thrown when exchange name is null or empty
    /// </exception>
    /// <exception cref="ExchangeException">
    /// Thrown when exchange deletion fails
    /// </exception>
    public async Task<bool> DeleteAsync(string name, bool ifUnused = false, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Exchange name cannot be null or empty", nameof(name));

        _statistics.TotalOperations++;
        var startTime = DateTime.UtcNow;

        try
        {
            _logger.LogInformation("Deleting exchange: {ExchangeName} (IfUnused: {IfUnused})", name, ifUnused);

            var success = await _retryPolicy.ExecuteAsync(async () =>
            {
                using var channel = await _connectionManager.GetChannelAsync(cancellationToken);
                
                try
                {
                    channel.ExchangeDelete(name, ifUnused);
                    _connectionManager.ReturnChannel(channel);
                    return true;
                }
                catch (Exception ex)
                {
                    _connectionManager.ReturnChannel(channel);
                    
                    _logger.LogError(ex, "Failed to delete exchange {ExchangeName}: {ErrorMessage}", name, ex.Message);
                    throw new ExchangeException($"Failed to delete exchange '{name}'", ex, name);
                }
            }, cancellationToken);

            var duration = DateTime.UtcNow - startTime;
            
            if (success)
            {
                // Remove from declared exchanges
                _declaredExchanges.TryRemove(name, out _);
                
                _statistics.SuccessfulOperations++;
                _statistics.DeletedExchanges++;
                _statistics.AverageOperationTime = CalculateAverageTime(duration);
                
                _logger.LogInformation("Exchange {ExchangeName} deleted successfully in {Duration}ms", 
                    name, duration.TotalMilliseconds);
                
                ExchangeDeleted?.Invoke(this, new ExchangeEventArgs(name, ExchangeOperation.Delete, success));
            }
            else
            {
                _statistics.FailedOperations++;
                _logger.LogError("Failed to delete exchange {ExchangeName} after retries", name);
            }

            return success;
        }
        catch (Exception ex)
        {
            _statistics.FailedOperations++;
            var duration = DateTime.UtcNow - startTime;
            _statistics.AverageOperationTime = CalculateAverageTime(duration);
            
            var context = ErrorContext.FromOperation(ex, $"DeleteExchange:{name}");
            await _errorHandler.HandleErrorAsync(context, cancellationToken);
            
            return false;
        }
    }

    /// <summary>
    /// Binds an exchange to another exchange with routing key
    /// </summary>
    /// <param name="destination">Destination exchange name</param>
    /// <param name="source">Source exchange name</param>
    /// <param name="routingKey">Routing key for the binding</param>
    /// <param name="arguments">Additional binding arguments</param>
    /// <param name="cancellationToken">Token to monitor for cancellation requests</param>
    /// <returns>
    /// A task that represents the asynchronous bind operation.
    /// The task result contains <c>true</c> if binding was successful; otherwise, <c>false</c>.
    /// </returns>
    /// <exception cref="ArgumentException">
    /// Thrown when destination or source exchange name is null or empty
    /// </exception>
    /// <exception cref="ExchangeException">
    /// Thrown when binding fails
    /// </exception>
    public async Task<bool> BindAsync(
        string destination, 
        string source, 
        string routingKey, 
        IDictionary<string, object>? arguments = null,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(destination))
            throw new ArgumentException("Destination exchange cannot be null or empty", nameof(destination));
        if (string.IsNullOrWhiteSpace(source))
            throw new ArgumentException("Source exchange cannot be null or empty", nameof(source));
        if (routingKey == null)
            throw new ArgumentNullException(nameof(routingKey));

        _statistics.TotalOperations++;
        var startTime = DateTime.UtcNow;

        try
        {
            _logger.LogInformation("Binding exchange {Source} to {Destination} with routing key: {RoutingKey}", 
                source, destination, routingKey);

            var success = await _retryPolicy.ExecuteAsync(async () =>
            {
                using var channel = await _connectionManager.GetChannelAsync(cancellationToken);
                
                try
                {
                    channel.ExchangeBind(destination, source, routingKey, arguments);
                    _connectionManager.ReturnChannel(channel);
                    return true;
                }
                catch (Exception ex)
                {
                    _connectionManager.ReturnChannel(channel);
                    
                    _logger.LogError(ex, "Failed to bind exchange {Source} to {Destination}: {ErrorMessage}", 
                        source, destination, ex.Message);
                    throw new ExchangeException($"Failed to bind exchange '{source}' to '{destination}'", ex, source);
                }
            }, cancellationToken);

            var duration = DateTime.UtcNow - startTime;
            
            if (success)
            {
                _statistics.SuccessfulOperations++;
                _statistics.AverageOperationTime = CalculateAverageTime(duration);
                
                _logger.LogInformation("Exchange binding completed successfully in {Duration}ms", duration.TotalMilliseconds);
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
            
            var context = ErrorContext.FromOperation(ex, $"BindExchange:{source}->{destination}");
            await _errorHandler.HandleErrorAsync(context, cancellationToken);
            
            return false;
        }
    }

    /// <summary>
    /// Unbinds an exchange from another exchange
    /// </summary>
    /// <param name="destination">Destination exchange name</param>
    /// <param name="source">Source exchange name</param>
    /// <param name="routingKey">Routing key for the binding</param>
    /// <param name="arguments">Additional binding arguments</param>
    /// <param name="cancellationToken">Token to monitor for cancellation requests</param>
    /// <returns>
    /// A task that represents the asynchronous unbind operation.
    /// The task result contains <c>true</c> if unbinding was successful; otherwise, <c>false</c>.
    /// </returns>
    /// <exception cref="ArgumentException">
    /// Thrown when destination or source exchange name is null or empty
    /// </exception>
    /// <exception cref="ExchangeException">
    /// Thrown when unbinding fails
    /// </exception>
    public async Task<bool> UnbindAsync(
        string destination, 
        string source, 
        string routingKey, 
        IDictionary<string, object>? arguments = null,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(destination))
            throw new ArgumentException("Destination exchange cannot be null or empty", nameof(destination));
        if (string.IsNullOrWhiteSpace(source))
            throw new ArgumentException("Source exchange cannot be null or empty", nameof(source));
        if (routingKey == null)
            throw new ArgumentNullException(nameof(routingKey));

        _statistics.TotalOperations++;
        var startTime = DateTime.UtcNow;

        try
        {
            _logger.LogInformation("Unbinding exchange {Source} from {Destination} with routing key: {RoutingKey}", 
                source, destination, routingKey);

            var success = await _retryPolicy.ExecuteAsync(async () =>
            {
                using var channel = await _connectionManager.GetChannelAsync(cancellationToken);
                
                try
                {
                    channel.ExchangeUnbind(destination, source, routingKey, arguments);
                    _connectionManager.ReturnChannel(channel);
                    return true;
                }
                catch (Exception ex)
                {
                    _connectionManager.ReturnChannel(channel);
                    
                    _logger.LogError(ex, "Failed to unbind exchange {Source} from {Destination}: {ErrorMessage}", 
                        source, destination, ex.Message);
                    throw new ExchangeException($"Failed to unbind exchange '{source}' from '{destination}'", ex, source);
                }
            }, cancellationToken);

            var duration = DateTime.UtcNow - startTime;
            
            if (success)
            {
                _statistics.SuccessfulOperations++;
                _statistics.AverageOperationTime = CalculateAverageTime(duration);
                
                _logger.LogInformation("Exchange unbinding completed successfully in {Duration}ms", duration.TotalMilliseconds);
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
            
            var context = ErrorContext.FromOperation(ex, $"UnbindExchange:{source}->{destination}");
            await _errorHandler.HandleErrorAsync(context, cancellationToken);
            
            return false;
        }
    }

    /// <summary>
    /// Checks if an exchange exists
    /// </summary>
    /// <param name="name">Exchange name to check</param>
    /// <param name="cancellationToken">Token to monitor for cancellation requests</param>
    /// <returns>
    /// A task that represents the asynchronous check operation.
    /// The task result contains <c>true</c> if the exchange exists; otherwise, <c>false</c>.
    /// </returns>
    /// <exception cref="ArgumentException">
    /// Thrown when exchange name is null or empty
    /// </exception>
    public async Task<bool> ExistsAsync(string name, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Exchange name cannot be null or empty", nameof(name));

        try
        {
            using var channel = await _connectionManager.GetChannelAsync(cancellationToken);
            
            try
            {
                // Try to declare passively - this will throw if exchange doesn't exist
                channel.ExchangeDeclarePassive(name);
                _connectionManager.ReturnChannel(channel);
                return true;
            }
            catch (OperationInterruptedException ex) when (ex.ShutdownReason.ReplyCode == 404)
            {
                // Exchange not found
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
            _logger.LogError(ex, "Error checking if exchange {ExchangeName} exists: {ErrorMessage}", name, ex.Message);
            return false;
        }
    }

    /// <summary>
    /// Gets detailed information about an exchange
    /// </summary>
    /// <param name="name">Exchange name</param>
    /// <param name="cancellationToken">Token to monitor for cancellation requests</param>
    /// <returns>
    /// A task that represents the asynchronous get operation.
    /// The task result contains exchange information if found; otherwise, <c>null</c>.
    /// </returns>
    /// <exception cref="ArgumentException">
    /// Thrown when exchange name is null or empty
    /// </exception>
    public async Task<ExchangeInfo?> GetInfoAsync(string name, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Exchange name cannot be null or empty", nameof(name));

        try
        {
            var exists = await ExistsAsync(name, cancellationToken);
            if (!exists)
                return null;

            // Check if we have declaration information
            if (_declaredExchanges.TryGetValue(name, out var declaration))
            {
                return new ExchangeInfo
                {
                    Name = name,
                    Type = declaration.Type,
                    Durable = declaration.Durable,
                    AutoDelete = declaration.AutoDelete,
                    Arguments = declaration.Arguments
                };
            }

            // Return basic info if exchange exists but we don't have declaration details
            return new ExchangeInfo
            {
                Name = name,
                Type = "unknown",
                Durable = true, // Most exchanges are durable
                AutoDelete = false,
                Arguments = new Dictionary<string, object>()
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting info for exchange {ExchangeName}: {ErrorMessage}", name, ex.Message);
            return null;
        }
    }

    /// <summary>
    /// Declares all exchanges from a collection with batch processing
    /// </summary>
    /// <param name="exchanges">Collection of exchanges to declare</param>
    /// <param name="cancellationToken">Token to monitor for cancellation requests</param>
    /// <returns>
    /// A task that represents the asynchronous batch declaration operation.
    /// The task result contains <c>true</c> if all declarations were successful; otherwise, <c>false</c>.
    /// </returns>
    /// <exception cref="ArgumentNullException">
    /// Thrown when exchanges collection is null
    /// </exception>
    public async Task<bool> DeclareAllAsync(IEnumerable<ExchangeSettings> exchanges, CancellationToken cancellationToken = default)
    {
        if (exchanges == null)
            throw new ArgumentNullException(nameof(exchanges));

        var exchangeList = exchanges.ToList();
        if (!exchangeList.Any())
        {
            _logger.LogDebug("No exchanges to declare");
            return true;
        }

        _logger.LogInformation("Declaring {ExchangeCount} exchanges", exchangeList.Count);

        var allSuccessful = true;
        foreach (var exchangeSettings in exchangeList)
        {
            try
            {
                var success = await DeclareAsync(exchangeSettings, cancellationToken);
                if (!success)
                {
                    allSuccessful = false;
                    _logger.LogWarning("Failed to declare exchange: {ExchangeName}", exchangeSettings.Name);
                }

                // Process bindings for this exchange
                foreach (var binding in exchangeSettings.Bindings)
                {
                    var bindingSuccess = await BindAsync(
                        binding.DestinationExchange,
                        exchangeSettings.Name,
                        binding.RoutingKey,
                        binding.Arguments,
                        cancellationToken);

                    if (!bindingSuccess)
                    {
                        allSuccessful = false;
                        _logger.LogWarning("Failed to bind exchange {Source} to {Destination}", 
                            exchangeSettings.Name, binding.DestinationExchange);
                    }
                }
            }
            catch (Exception ex)
            {
                allSuccessful = false;
                _logger.LogError(ex, "Error declaring exchange {ExchangeName}: {ErrorMessage}", 
                    exchangeSettings.Name, ex.Message);
            }
        }

        _logger.LogInformation("Exchange declaration completed. Success: {AllSuccessful}", allSuccessful);
        return allSuccessful;
    }

    /// <summary>
    /// Redeclares all previously declared exchanges (used for recovery)
    /// </summary>
    /// <param name="cancellationToken">Token to monitor for cancellation requests</param>
    /// <returns>
    /// A task that represents the asynchronous redeclaration operation.
    /// The task result contains <c>true</c> if all redeclarations were successful; otherwise, <c>false</c>.
    /// </returns>
    /// <remarks>
    /// This method is typically used during connection recovery to restore exchange topology.
    /// </remarks>
    public async Task<bool> RedeclareAllAsync(CancellationToken cancellationToken = default)
    {
        var exchanges = _declaredExchanges.Values.ToList();
        if (!exchanges.Any())
        {
            _logger.LogDebug("No exchanges to redeclare");
            return true;
        }

        _logger.LogInformation("Redeclaring {ExchangeCount} exchanges for auto-recovery", exchanges.Count);
        
        ExchangesRecreating?.Invoke(this, new ExchangeEventArgs("", ExchangeOperation.Redeclare, true));

        var allSuccessful = true;
        foreach (var declaration in exchanges)
        {
            try
            {
                var success = await DeclareAsync(
                    declaration.Name,
                    declaration.Type,
                    declaration.Durable,
                    declaration.AutoDelete,
                    declaration.Arguments,
                    cancellationToken);

                if (!success)
                {
                    allSuccessful = false;
                    _logger.LogWarning("Failed to redeclare exchange: {ExchangeName}", declaration.Name);
                }
            }
            catch (Exception ex)
            {
                allSuccessful = false;
                _logger.LogError(ex, "Error redeclaring exchange {ExchangeName}: {ErrorMessage}", 
                    declaration.Name, ex.Message);
            }
        }

        ExchangesRecreated?.Invoke(this, new ExchangeEventArgs("", ExchangeOperation.Redeclare, allSuccessful));
        
        _logger.LogInformation("Exchange redeclaration completed. Success: {AllSuccessful}", allSuccessful);
        return allSuccessful;
    }

    /// <summary>
    /// Gets exchange manager statistics and performance metrics
    /// </summary>
    /// <returns>
    /// Exchange statistics containing operation counts, success rates, and performance metrics
    /// </returns>
    public ExchangeStatistics GetStatistics()
    {
        return _statistics.Clone();
    }

    /// <summary>
    /// Gets all bindings for a specific exchange
    /// </summary>
    /// <param name="exchangeName">Exchange name to get bindings for</param>
    /// <param name="cancellationToken">Token to monitor for cancellation requests</param>
    /// <returns>
    /// A task that represents the asynchronous get operation.
    /// The task result contains a collection of exchange binding information.
    /// </returns>
    /// <exception cref="ArgumentException">
    /// Thrown when exchangeName is null or empty
    /// </exception>
    public async Task<IEnumerable<ExchangeBindingInfo>> GetBindingsAsync(string exchangeName, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(exchangeName))
            throw new ArgumentException("Exchange name cannot be null or empty", nameof(exchangeName));

        try
        {
            // For now, return empty list as we don't track bindings in ExchangeDeclaration
            // In a production environment, you would use RabbitMQ Management API
            // to get actual bindings from the server
            return new List<ExchangeBindingInfo>();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting bindings for exchange {ExchangeName}: {ErrorMessage}", exchangeName, ex.Message);
            return new List<ExchangeBindingInfo>();
        }
    }

    /// <summary>
    /// Lists all exchanges with optional filtering and detailed information
    /// </summary>
    /// <param name="filter">Optional filter string for exchange names</param>
    /// <param name="includeDetails">Whether to include detailed exchange information</param>
    /// <param name="cancellationToken">Token to monitor for cancellation requests</param>
    /// <returns>
    /// A task that represents the asynchronous list operation.
    /// The task result contains a collection of exchange information.
    /// </returns>
    public async Task<IEnumerable<ExchangeInfo>> ListExchangesAsync(string? filter = null, bool includeDetails = false, CancellationToken cancellationToken = default)
    {
        try
        {
            var exchangeInfos = new List<ExchangeInfo>();

            // Get all locally tracked exchanges
            var trackedExchanges = _declaredExchanges.Keys.ToList();

            if (!string.IsNullOrEmpty(filter))
            {
                trackedExchanges = trackedExchanges.Where(name => name.Contains(filter, StringComparison.OrdinalIgnoreCase)).ToList();
            }

            foreach (var exchangeName in trackedExchanges)
            {
                try
                {
                    var exchangeInfo = await GetInfoAsync(exchangeName, cancellationToken);
                    if (exchangeInfo != null)
                    {
                        exchangeInfos.Add(exchangeInfo);
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Could not get info for exchange {ExchangeName}", exchangeName);
                    
                    // Still add basic info if we can't get detailed info
                    if (_declaredExchanges.TryGetValue(exchangeName, out var declaration))
                    {
                        exchangeInfos.Add(new ExchangeInfo
                        {
                            Name = exchangeName,
                            Type = declaration.Type,
                            Durable = declaration.Durable,
                            AutoDelete = declaration.AutoDelete,
                            Arguments = declaration.Arguments
                        });
                    }
                }
            }

            return exchangeInfos;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error listing exchanges: {ErrorMessage}", ex.Message);
            return new List<ExchangeInfo>();
        }
    }

    /// <summary>
    /// Gets the complete topology information for exchanges
    /// </summary>
    /// <param name="exchangeName">Optional specific exchange name to get topology for</param>
    /// <param name="cancellationToken">Token to monitor for cancellation requests</param>
    /// <returns>
    /// A task that represents the asynchronous get operation.
    /// The task result contains complete exchange topology information.
    /// </returns>
    /// <remarks>
    /// This method provides a comprehensive view of the exchange topology including
    /// all exchanges, their bindings, and relationships.
    /// </remarks>
    public async Task<ExchangeTopology> GetTopologyAsync(string? exchangeName = null, CancellationToken cancellationToken = default)
    {
        try
        {
            var topology = new ExchangeTopology();

            if (!string.IsNullOrEmpty(exchangeName))
            {
                // Get topology for specific exchange
                var exchangeInfo = await GetInfoAsync(exchangeName, cancellationToken);
                if (exchangeInfo != null)
                {
                    var exchangeNode = new ExchangeTopologyNode
                    {
                        Name = exchangeInfo.Name,
                        Type = exchangeInfo.Type,
                        Durable = exchangeInfo.Durable,
                        Internal = false,
                        Depth = 0
                    };
                    
                    topology.ExchangeNodes = new List<ExchangeTopologyNode> { exchangeNode };
                    topology.RootExchange = exchangeInfo.Name;
                }
            }
            else
            {
                // Get topology for all exchanges
                var exchanges = await ListExchangesAsync(null, true, cancellationToken);
                var exchangeNodes = exchanges.Select(exchange => new ExchangeTopologyNode
                {
                    Name = exchange.Name,
                    Type = exchange.Type,
                    Durable = exchange.Durable,
                    Internal = false,
                    Depth = 0
                }).ToList();

                topology.ExchangeNodes = exchangeNodes;
                topology.RootExchange = exchangeNodes.FirstOrDefault()?.Name ?? "";
            }

            topology.Timestamp = DateTimeOffset.UtcNow;
            return topology;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting exchange topology: {ErrorMessage}", ex.Message);
            return new ExchangeTopology { Timestamp = DateTimeOffset.UtcNow };
        }
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