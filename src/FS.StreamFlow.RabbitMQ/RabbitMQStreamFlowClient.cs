using FS.StreamFlow.Core.Features.Messaging.Interfaces;
using FS.StreamFlow.Core.Features.Messaging.Models;
using FS.StreamFlow.Core.Features.Events.Interfaces;
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
    private readonly IConsumer _consumer;
    private readonly IQueueManager _queueManager;
    private readonly IExchangeManager _exchangeManager;
    private readonly IEventBus _eventBus;
    private readonly IEventStore _eventStore;
    private readonly IHealthChecker _healthChecker;
    private readonly ISagaOrchestrator _sagaOrchestrator;
    private readonly IRetryPolicyFactory _retryPolicyFactory;
    private readonly IMessageSerializerFactory _serializerFactory;
    private readonly IErrorHandler _errorHandler;
    private readonly IDeadLetterHandler _deadLetterHandler;
    private readonly IMetricsCollector _metricsCollector;
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
    public IConsumer Consumer => _consumer;

    /// <summary>
    /// Gets the exchange manager interface.
    /// </summary>
    public IExchangeManager ExchangeManager => _exchangeManager;

    /// <summary>
    /// Gets the queue manager interface.
    /// </summary>
    public IQueueManager QueueManager => _queueManager;

    /// <summary>
    /// Gets the health checker interface.
    /// </summary>
    public IHealthChecker HealthChecker => _healthChecker;

    /// <summary>
    /// Gets the event bus interface.
    /// </summary>
    public IEventBus EventBus => _eventBus;

    /// <summary>
    /// Gets the event store interface.
    /// </summary>
    public IEventStore EventStore => _eventStore;

    /// <summary>
    /// Gets saga orchestrator for managing long-running workflow processes and compensation patterns
    /// </summary>
    public ISagaOrchestrator SagaOrchestrator => _sagaOrchestrator;

    /// <summary>
    /// Gets retry policy factory for creating and managing retry strategies
    /// </summary>
    public IRetryPolicyFactory RetryPolicyFactory => _retryPolicyFactory;

    /// <summary>
    /// Gets message serializer factory for creating serializers for different formats
    /// </summary>
    public IMessageSerializerFactory SerializerFactory => _serializerFactory;

    /// <summary>
    /// Gets error handler for comprehensive error management and dead letter queue support
    /// </summary>
    public IErrorHandler ErrorHandler => _errorHandler;

    /// <summary>
    /// Gets dead letter handler for managing failed messages and reprocessing
    /// </summary>
    public IDeadLetterHandler DeadLetterHandler => _deadLetterHandler;

    /// <summary>
    /// Gets metrics collector for monitoring and observability
    /// </summary>
    public IMetricsCollector MetricsCollector => _metricsCollector;

    /// <summary>
    /// Occurs when the client status changes.
    /// </summary>
    public event EventHandler<ClientStatusChangedEventArgs>? StatusChanged;

    /// <summary>
    /// Initializes a new instance of the RabbitMQStreamFlowClient class.
    /// </summary>
    /// <param name="connectionManager">The connection manager.</param>
    /// <param name="producer">The producer.</param>
    /// <param name="consumer">The consumer.</param>
    /// <param name="queueManager">The queue manager.</param>
    /// <param name="exchangeManager">The exchange manager.</param>
    /// <param name="eventBus">The event bus.</param>
    /// <param name="eventStore">The event store.</param>
    /// <param name="healthChecker">The health checker.</param>
    /// <param name="sagaOrchestrator">The saga orchestrator.</param>
    /// <param name="retryPolicyFactory">The retry policy factory.</param>
    /// <param name="serializerFactory">The serializer factory.</param>
    /// <param name="errorHandler">The error handler.</param>
    /// <param name="deadLetterHandler">The dead letter handler.</param>
    /// <param name="metricsCollector">The metrics collector.</param>
    /// <param name="logger">The logger.</param>
    /// <param name="configuration">The client configuration.</param>
    public RabbitMQStreamFlowClient(
        IConnectionManager connectionManager,
        IProducer producer,
        IConsumer consumer,
        IQueueManager queueManager,
        IExchangeManager exchangeManager,
        IEventBus eventBus,
        IEventStore eventStore,
        IHealthChecker healthChecker,
        ISagaOrchestrator sagaOrchestrator,
        IRetryPolicyFactory retryPolicyFactory,
        IMessageSerializerFactory serializerFactory,
        IErrorHandler errorHandler,
        IDeadLetterHandler deadLetterHandler,
        IMetricsCollector metricsCollector,
        ILogger<RabbitMQStreamFlowClient> logger,
        IOptions<ClientConfiguration> configuration)
    {
        _connectionManager = connectionManager ?? throw new ArgumentNullException(nameof(connectionManager));
        _producer = producer ?? throw new ArgumentNullException(nameof(producer));
        _consumer = consumer ?? throw new ArgumentNullException(nameof(consumer));
        _queueManager = queueManager ?? throw new ArgumentNullException(nameof(queueManager));
        _exchangeManager = exchangeManager ?? throw new ArgumentNullException(nameof(exchangeManager));
        _eventBus = eventBus ?? throw new ArgumentNullException(nameof(eventBus));
        _eventStore = eventStore ?? throw new ArgumentNullException(nameof(eventStore));
        _healthChecker = healthChecker ?? throw new ArgumentNullException(nameof(healthChecker));
        _sagaOrchestrator = sagaOrchestrator ?? throw new ArgumentNullException(nameof(sagaOrchestrator));
        _retryPolicyFactory = retryPolicyFactory ?? throw new ArgumentNullException(nameof(retryPolicyFactory));
        _serializerFactory = serializerFactory ?? throw new ArgumentNullException(nameof(serializerFactory));
        _errorHandler = errorHandler ?? throw new ArgumentNullException(nameof(errorHandler));
        _deadLetterHandler = deadLetterHandler ?? throw new ArgumentNullException(nameof(deadLetterHandler));
        _metricsCollector = metricsCollector ?? throw new ArgumentNullException(nameof(metricsCollector));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _configuration = configuration.Value ?? throw new ArgumentNullException(nameof(configuration));

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