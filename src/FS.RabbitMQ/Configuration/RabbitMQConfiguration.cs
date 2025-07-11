using FS.RabbitMQ.Consumer;
using FS.RabbitMQ.EventBus;
using FS.RabbitMQ.EventStore;
using FS.RabbitMQ.Producer;
using FS.RabbitMQ.Saga;

namespace FS.RabbitMQ.Configuration;

/// <summary>
/// Main configuration class for RabbitMQ client with comprehensive settings
/// </summary>
public class RabbitMQConfiguration : IRabbitMQConfiguration
{
    /// <summary>
    /// Connection settings for RabbitMQ server
    /// </summary>
    public ConnectionSettings Connection { get; set; } = new();
    
    /// <summary>
    /// Producer specific settings
    /// </summary>
    public ProducerSettings Producer { get; set; } = new();
    
    /// <summary>
    /// Consumer specific settings
    /// </summary>
    public ConsumerSettings Consumer { get; set; } = new();
    
    /// <summary>
    /// Serialization settings
    /// </summary>
    public SerializerSettings Serialization { get; set; } = new();
    
    /// <summary>
    /// Exchange configurations to be auto-declared
    /// </summary>
    public List<ExchangeSettings> Exchanges { get; set; } = new();
    
    /// <summary>
    /// Queue configurations to be auto-declared
    /// </summary>
    public List<QueueSettings> Queues { get; set; } = new();
    
    /// <summary>
    /// Default retry policy settings
    /// </summary>
    public RetryPolicySettings RetryPolicy { get; set; } = new();
    
    /// <summary>
    /// Error handling settings
    /// </summary>
    public ErrorHandlingSettings ErrorHandling { get; set; } = new();
    
    /// <summary>
    /// Event bus specific settings
    /// </summary>
    public EventBusSettings EventBus { get; set; } = new();
    
    /// <summary>
    /// Event store specific settings
    /// </summary>
    public EventStoreSettings EventStore { get; set; } = new();
    
    /// <summary>
    /// Saga orchestration settings
    /// </summary>
    public SagaSettings Saga { get; set; } = new();
    
    /// <summary>
    /// Health check settings
    /// </summary>
    public HealthCheckSettings HealthCheck { get; set; } = new();
    
    /// <summary>
    /// Monitoring settings
    /// </summary>
    public HealthCheckSettings Monitoring { get; set; } = new();
    
    /// <summary>
    /// Whether to automatically declare configured exchanges on startup
    /// </summary>
    public bool AutoDeclareExchanges { get; set; } = true;
    
    /// <summary>
    /// Whether to automatically declare configured queues on startup
    /// </summary>
    public bool AutoDeclareQueues { get; set; } = true;
    
    /// <summary>
    /// Whether to enable auto-recovery for connections and channels
    /// </summary>
    public bool AutoRecoveryEnabled { get; set; } = true;
    
    /// <summary>
    /// Global timeout for operations in milliseconds
    /// </summary>
    public int OperationTimeoutMs { get; set; } = 30000; // 30 seconds

    // Interface implementations with readonly access
    IReadOnlyList<ExchangeSettings> IRabbitMQConfiguration.Exchanges => Exchanges.AsReadOnly();
    IReadOnlyList<QueueSettings> IRabbitMQConfiguration.Queues => Queues.AsReadOnly();

    /// <summary>
    /// Validates the configuration and throws if invalid
    /// </summary>
    public void Validate()
    {
        Connection.Validate();
        Producer.Validate();
        Consumer.Validate();
        Serialization.Validate();
        
        if (OperationTimeoutMs <= 0)
            throw new ArgumentException("OperationTimeoutMs must be greater than 0");
            
        foreach (var exchange in Exchanges)
        {
            exchange.Validate();
        }
        
        foreach (var queue in Queues)
        {
            queue.Validate();
        }
        
        RetryPolicy.Validate();
        ErrorHandling.Validate();
        EventBus.Validate();
        EventStore.Validate();
        Saga.Validate();
        HealthCheck.Validate();
        Monitoring.Validate();
    }
}