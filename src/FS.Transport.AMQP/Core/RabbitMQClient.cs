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
public class RabbitMQClient : IRabbitMQClient
{
    private readonly ILogger<RabbitMQClient> _logger;
    private readonly RabbitMQConfiguration _configuration;
    private readonly IServiceProvider _serviceProvider;
    private bool _disposed;
    private ClientStatus _status = ClientStatus.NotInitialized;

    public IConnectionManager ConnectionManager { get; }
    public IExchangeManager ExchangeManager { get; }
    public IQueueManager QueueManager { get; }
    public IMessageProducer Producer { get; }
    public IMessageConsumer Consumer { get; }
    public IEventBus EventBus { get; }
    public IEventStore EventStore { get; }
    public IHealthChecker HealthChecker { get; }
    
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

    public event EventHandler<ClientStatusChangedEventArgs>? StatusChanged;

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