using FS.StreamFlow.Core.Features.Messaging.Interfaces;
using FS.StreamFlow.Core.Features.Messaging.Models;
using FS.StreamFlow.Core.Features.Events.Interfaces;
using FS.StreamFlow.RabbitMQ.Features.Connection;
using FS.StreamFlow.RabbitMQ.Features.Producer;
using FS.StreamFlow.RabbitMQ.Features.Consumer;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace FS.StreamFlow.RabbitMQ;

/// <summary>
/// RabbitMQ implementation of the StreamFlow client interface.
/// Provides provider-specific client operations for RabbitMQ messaging.
/// </summary>
public class RabbitMQStreamFlowClient : IStreamFlowClient
{
    private readonly IConnectionManager _connectionManager;
    private readonly IProducer _producer;
    private readonly ILogger<RabbitMQStreamFlowClient> _logger;
    private readonly ClientConfiguration _configuration;
    private volatile bool _disposed;

    /// <summary>
    /// Gets the current client status.
    /// </summary>
    public ClientStatus Status => _connectionManager.State == ConnectionState.Connected 
        ? ClientStatus.Connected 
        : ClientStatus.Disconnected;

    /// <summary>
    /// Gets the connection statistics.
    /// </summary>
    public ConnectionStatistics ConnectionStatistics => _connectionManager.Statistics;

    /// <summary>
    /// Gets the connection manager interface.
    /// </summary>
    public IConnectionManager ConnectionManager => _connectionManager;

    /// <summary>
    /// Gets the producer interface.
    /// </summary>
    public IProducer Producer => _producer;

    /// <summary>
    /// Gets the consumer interface.
    /// </summary>
    public IConsumer Consumer { get; private set; } = null!;

    /// <summary>
    /// Gets the exchange manager interface.
    /// </summary>
    public IExchangeManager ExchangeManager => throw new NotImplementedException("ExchangeManager implementation in progress");

    /// <summary>
    /// Gets the queue manager interface.
    /// </summary>
    public IQueueManager QueueManager => throw new NotImplementedException("QueueManager not implemented yet");

    /// <summary>
    /// Gets the health checker interface.
    /// </summary>
    public IHealthChecker HealthChecker => throw new NotImplementedException("HealthChecker not implemented yet");

    /// <summary>
    /// Gets the event bus interface.
    /// </summary>
    public IEventBus EventBus => throw new NotImplementedException("EventBus not implemented yet");

    /// <summary>
    /// Gets the event store interface.
    /// </summary>
    public IEventStore EventStore => throw new NotImplementedException("EventStore not implemented yet");

    /// <summary>
    /// Occurs when the client status changes.
    /// </summary>
    public event EventHandler<ClientStatusChangedEventArgs>? StatusChanged;

    /// <summary>
    /// Initializes a new instance of the RabbitMQStreamFlowClient class.
    /// </summary>
    /// <param name="connectionManager">The connection manager.</param>
    /// <param name="producer">The producer.</param>
    /// <param name="logger">The logger.</param>
    /// <param name="configuration">The client configuration.</param>
    /// <param name="loggerFactory">The logger factory.</param>
    public RabbitMQStreamFlowClient(
        IConnectionManager connectionManager,
        IProducer producer,
        ILogger<RabbitMQStreamFlowClient> logger,
        IOptions<ClientConfiguration> configuration,
        ILoggerFactory loggerFactory)
    {
        _connectionManager = connectionManager ?? throw new ArgumentNullException(nameof(connectionManager));
        _producer = producer ?? throw new ArgumentNullException(nameof(producer));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _configuration = configuration.Value ?? throw new ArgumentNullException(nameof(configuration));

        // Initialize Consumer
        var consumerSettings = new ConsumerSettings { ConsumerId = "default-consumer" };
        var consumerLogger = loggerFactory.CreateLogger<RabbitMQConsumer>();
        Consumer = new RabbitMQConsumer(connectionManager, consumerSettings, consumerLogger);

        // Subscribe to connection events
        _connectionManager.Connected += OnConnectionStateChanged;
        _connectionManager.Disconnected += OnConnectionStateChanged;
    }

    /// <summary>
    /// Initializes the client asynchronously.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task representing the initialization operation.</returns>
    public async Task InitializeAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("Initializing RabbitMQ StreamFlow client");
            
            await _connectionManager.ConnectAsync(cancellationToken);
            await _producer.InitializeAsync(cancellationToken);
            await Consumer.StartAsync(cancellationToken);
            
            _logger.LogInformation("RabbitMQ StreamFlow client initialized successfully");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to initialize RabbitMQ StreamFlow client");
            throw;
        }
    }

    /// <summary>
    /// Shuts down the client gracefully.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task representing the shutdown operation.</returns>
    public async Task ShutdownAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("Shutting down RabbitMQ StreamFlow client");
            
            _producer.Dispose();
            await _connectionManager.DisconnectAsync(cancellationToken);
            
            _logger.LogInformation("RabbitMQ StreamFlow client shut down successfully");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during RabbitMQ StreamFlow client shutdown");
            throw;
        }
    }

    /// <summary>
    /// Disposes the client resources.
    /// </summary>
    public void Dispose()
    {
        if (_disposed) return;

        // Unsubscribe from events
        _connectionManager.Connected -= OnConnectionStateChanged;
        _connectionManager.Disconnected -= OnConnectionStateChanged;

        // Dispose resources
        _producer?.Dispose();
        _connectionManager?.Dispose();

        _disposed = true;
        GC.SuppressFinalize(this);
    }

    /// <summary>
    /// Handles connection state changes.
    /// </summary>
    /// <param name="sender">The sender.</param>
    /// <param name="e">The event arguments.</param>
    private void OnConnectionStateChanged(object? sender, ConnectionEventArgs e)
    {
        var newStatus = e.State == ConnectionState.Connected ? ClientStatus.Connected : ClientStatus.Disconnected;
        var previousStatus = Status;
        
        if (newStatus != previousStatus)
        {
            StatusChanged?.Invoke(this, new ClientStatusChangedEventArgs(previousStatus, newStatus, "RabbitMQ Client"));
        }
    }
} 