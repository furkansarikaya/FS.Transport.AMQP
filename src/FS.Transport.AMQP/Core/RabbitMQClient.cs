using FS.Transport.AMQP.Configuration;
using FS.Transport.AMQP.Connection;
using FS.Transport.AMQP.Consumer;
using FS.Transport.AMQP.Core.Exceptions;
using FS.Transport.AMQP.EventBus;
using FS.Transport.AMQP.EventStore;
using FS.Transport.AMQP.Exchange;
using FS.Transport.AMQP.Monitoring;
using FS.Transport.AMQP.Producer;
using FS.Transport.AMQP.Queue;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace FS.Transport.AMQP.Core;

/// <summary>
/// Main RabbitMQ client implementation providing centralized access to all RabbitMQ operations
/// </summary>
/// <remarks>
/// This class serves as the primary entry point for all RabbitMQ operations, providing:
/// - Centralized connection management
/// - Unified access to all RabbitMQ features
/// - Automatic initialization and shutdown
/// - Health monitoring and status tracking
/// - Event-driven architecture support
/// - Comprehensive error handling and recovery
/// </remarks>
public class RabbitMQClient : IRabbitMQClient
{
    private readonly ILogger<RabbitMQClient> _logger;
    private readonly RabbitMQConfiguration _configuration;
    private readonly IServiceProvider _serviceProvider;
    private bool _disposed;
    private ClientStatus _status = ClientStatus.NotInitialized;

    /// <summary>
    /// Gets the connection manager for RabbitMQ connectivity
    /// </summary>
    /// <value>
    /// The connection manager instance responsible for managing RabbitMQ connections
    /// </value>
    public IConnectionManager ConnectionManager { get; }
    
    /// <summary>
    /// Gets the exchange manager for RabbitMQ exchange operations
    /// </summary>
    /// <value>
    /// The exchange manager instance for creating, deleting, and managing exchanges
    /// </value>
    public IExchangeManager ExchangeManager { get; }
    
    /// <summary>
    /// Gets the queue manager for RabbitMQ queue operations
    /// </summary>
    /// <value>
    /// The queue manager instance for creating, deleting, and managing queues
    /// </value>
    public IQueueManager QueueManager { get; }
    
    /// <summary>
    /// Gets the message producer for publishing messages
    /// </summary>
    /// <value>
    /// The message producer instance for sending messages to RabbitMQ
    /// </value>
    public IMessageProducer Producer { get; }
    
    /// <summary>
    /// Gets the message consumer for consuming messages
    /// </summary>
    /// <value>
    /// The message consumer instance for receiving messages from RabbitMQ
    /// </value>
    public IMessageConsumer Consumer { get; }
    
    /// <summary>
    /// Gets the event bus for event-driven architecture
    /// </summary>
    /// <value>
    /// The event bus instance for publishing and subscribing to events
    /// </value>
    public IEventBus EventBus { get; }
    
    /// <summary>
    /// Gets the event store for event sourcing
    /// </summary>
    /// <value>
    /// The event store instance for storing and retrieving events
    /// </value>
    public IEventStore EventStore { get; }
    
    /// <summary>
    /// Gets the health checker for monitoring RabbitMQ health
    /// </summary>
    /// <value>
    /// The health checker instance for monitoring connection and service health
    /// </value>
    public IHealthChecker HealthChecker { get; }
    
    /// <summary>
    /// Gets the current status of the RabbitMQ client
    /// </summary>
    /// <value>
    /// The current client status (NotInitialized, Initializing, Ready, Disconnected, Reconnecting, ShuttingDown, Shutdown, Failed)
    /// </value>
    public ClientStatus Status 
    { 
        get => _status; 
        private set 
        { 
            if (_status != value)
            {
                var oldStatus = _status;
                _status = value;
                StatusChanged?.Invoke(this, new ClientStatusChangedEventArgs(oldStatus, value));
            }
        } 
    }

    /// <summary>
    /// Occurs when the client status changes
    /// </summary>
    public event EventHandler<ClientStatusChangedEventArgs>? StatusChanged;

    /// <summary>
    /// Initializes a new instance of the <see cref="RabbitMQClient"/> class
    /// </summary>
    /// <param name="connectionManager">Connection manager for RabbitMQ connectivity</param>
    /// <param name="exchangeManager">Exchange manager for exchange operations</param>
    /// <param name="queueManager">Queue manager for queue operations</param>
    /// <param name="producer">Message producer for publishing messages</param>
    /// <param name="consumer">Message consumer for consuming messages</param>
    /// <param name="eventBus">Event bus for event-driven architecture</param>
    /// <param name="eventStore">Event store for event sourcing</param>
    /// <param name="healthChecker">Health checker for monitoring</param>
    /// <param name="configuration">RabbitMQ configuration settings</param>
    /// <param name="logger">Logger for client activities</param>
    /// <param name="serviceProvider">Service provider for dependency injection</param>
    /// <exception cref="ArgumentNullException">
    /// Thrown when any required parameter is null
    /// </exception>
    public RabbitMQClient(
        IConnectionManager connectionManager,
        IExchangeManager exchangeManager,
        IQueueManager queueManager,
        IMessageProducer producer,
        IMessageConsumer consumer,
        IEventBus eventBus,
        IEventStore eventStore,
        IHealthChecker healthChecker,
        IOptions<RabbitMQConfiguration> configuration,
        ILogger<RabbitMQClient> logger,
        IServiceProvider serviceProvider)
    {
        ConnectionManager = connectionManager ?? throw new ArgumentNullException(nameof(connectionManager));
        ExchangeManager = exchangeManager ?? throw new ArgumentNullException(nameof(exchangeManager));
        QueueManager = queueManager ?? throw new ArgumentNullException(nameof(queueManager));
        Producer = producer ?? throw new ArgumentNullException(nameof(producer));
        Consumer = consumer ?? throw new ArgumentNullException(nameof(consumer));
        EventBus = eventBus ?? throw new ArgumentNullException(nameof(eventBus));
        EventStore = eventStore ?? throw new ArgumentNullException(nameof(eventStore));
        HealthChecker = healthChecker ?? throw new ArgumentNullException(nameof(healthChecker));
        _configuration = configuration?.Value ?? throw new ArgumentNullException(nameof(configuration));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _serviceProvider = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));
        
        // Subscribe to connection events
        ConnectionManager.Connected += OnConnectionEstablished;
        ConnectionManager.Disconnected += OnConnectionLost;
        ConnectionManager.Recovering += OnConnectionRecovering;
    }

    /// <summary>
    /// Initializes the RabbitMQ client and all its components
    /// </summary>
    /// <param name="cancellationToken">Token to monitor for cancellation requests</param>
    /// <returns>A task that represents the asynchronous initialization operation</returns>
    /// <exception cref="ObjectDisposedException">
    /// Thrown when the client has been disposed
    /// </exception>
    /// <exception cref="RabbitMQClientException">
    /// Thrown when client initialization fails
    /// </exception>
    /// <remarks>
    /// This method initializes the connection to RabbitMQ, auto-declares configured exchanges and queues,
    /// and prepares all components for operation. It should be called before using any other client features.
    /// </remarks>
    public async Task InitializeAsync(CancellationToken cancellationToken = default)
    {
        if (_disposed)
            throw new ObjectDisposedException(nameof(RabbitMQClient));

        if (Status != ClientStatus.NotInitialized)
        {
            _logger.LogWarning("Client is already initialized. Current status: {Status}", Status);
            return;
        }

        try
        {
            Status = ClientStatus.Initializing;
            _logger.LogInformation("Initializing RabbitMQ client...");

            // Initialize connection
            await ConnectionManager.ConnectAsync(cancellationToken);
            
            // Initialize components in order
            await InitializeExchangesAndQueues(cancellationToken);
            
            Status = ClientStatus.Ready;
            _logger.LogInformation("RabbitMQ client initialized successfully");
        }
        catch (Exception ex)
        {
            Status = ClientStatus.Failed;
            _logger.LogError(ex, "Failed to initialize RabbitMQ client");
            throw new RabbitMQClientException("Failed to initialize RabbitMQ client", ex);
        }
    }

    /// <summary>
    /// Gracefully shuts down the RabbitMQ client and all its components
    /// </summary>
    /// <param name="cancellationToken">Token to monitor for cancellation requests</param>
    /// <returns>A task that represents the asynchronous shutdown operation</returns>
    /// <exception cref="RabbitMQClientException">
    /// Thrown when client shutdown encounters an error
    /// </exception>
    /// <remarks>
    /// This method gracefully shuts down all components in reverse order of initialization,
    /// ensuring proper cleanup of resources and connections.
    /// </remarks>
    public async Task ShutdownAsync(CancellationToken cancellationToken = default)
    {
        if (_disposed || Status == ClientStatus.NotInitialized)
            return;

        try
        {
            Status = ClientStatus.ShuttingDown;
            _logger.LogInformation("Shutting down RabbitMQ client...");

            // Shutdown components in reverse order
            await Consumer.StopAsync(cancellationToken);
            await ConnectionManager.DisconnectAsync(cancellationToken);
            
            Status = ClientStatus.Shutdown;
            _logger.LogInformation("RabbitMQ client shutdown completed");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during RabbitMQ client shutdown");
            throw new RabbitMQClientException("Error during client shutdown", ex);
        }
    }

    private async Task InitializeExchangesAndQueues(CancellationToken cancellationToken)
    {
        // Auto-declare configured exchanges
        if (_configuration.AutoDeclareExchanges)
        {
            foreach (var exchangeConfig in _configuration.Exchanges)
            {
                await ExchangeManager.DeclareAsync(exchangeConfig, cancellationToken);
            }
        }

        // Auto-declare configured queues
        if (_configuration.AutoDeclareQueues)
        {
            foreach (var queueConfig in _configuration.Queues)
            {
                await QueueManager.DeclareAsync(queueConfig, cancellationToken);
            }
        }
    }

    private void OnConnectionEstablished(object? sender, ConnectionEventArgs e)
    {
        _logger.LogInformation("Connection established to RabbitMQ");
        
        // Recreate exchanges and queues after reconnection
        Task.Run(async () =>
        {
            try
            {
                await InitializeExchangesAndQueues(CancellationToken.None);
                if (Status == ClientStatus.Reconnecting)
                {
                    Status = ClientStatus.Ready;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to recreate exchanges and queues after reconnection");
                Status = ClientStatus.Failed;
            }
        });
    }

    private void OnConnectionLost(object? sender, ConnectionEventArgs e)
    {
        _logger.LogWarning("Connection lost to RabbitMQ: {Message}", e.Message);
        if (Status == ClientStatus.Ready)
        {
            Status = ClientStatus.Disconnected;
        }
    }

    private void OnConnectionRecovering(object? sender, ConnectionEventArgs e)
    {
        _logger.LogInformation("Attempting to recover RabbitMQ connection...");
        Status = ClientStatus.Reconnecting;
    }

    /// <summary>
    /// Releases all resources used by the <see cref="RabbitMQClient"/>
    /// </summary>
    /// <remarks>
    /// This method performs cleanup of all managed resources and unsubscribes from events.
    /// After disposal, the client cannot be reused.
    /// </remarks>
    public void Dispose()
    {
        if (_disposed)
            return;

        try
        {
            ShutdownAsync().GetAwaiter().GetResult();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during disposal");
        }

        // Unsubscribe from events
        ConnectionManager.Connected -= OnConnectionEstablished;
        ConnectionManager.Disconnected -= OnConnectionLost;
        ConnectionManager.Recovering -= OnConnectionRecovering;

        ConnectionManager?.Dispose();
        Producer?.Dispose();
        Consumer?.Dispose();
        EventBus?.Dispose();
        EventStore?.Dispose();
        HealthChecker?.Dispose();

        _disposed = true;
    }
}