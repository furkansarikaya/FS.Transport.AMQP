using FS.Transport.AMQP.EventBus;
using FS.Transport.AMQP.EventStore;

namespace FS.Transport.AMQP.Configuration;

/// <summary>
/// Interface for RabbitMQ configuration providing access to all settings
/// </summary>
public interface IRabbitMQConfiguration
{
    /// <summary>
    /// Connection settings for RabbitMQ server
    /// </summary>
    ConnectionSettings Connection { get; }
    
    /// <summary>
    /// Exchange configurations to be auto-declared
    /// </summary>
    IReadOnlyList<ExchangeSettings> Exchanges { get; }
    
    /// <summary>
    /// Queue configurations to be auto-declared
    /// </summary>
    IReadOnlyList<QueueSettings> Queues { get; }
    
    /// <summary>
    /// Default retry policy settings
    /// </summary>
    RetryPolicySettings RetryPolicy { get; }
    
    /// <summary>
    /// Error handling settings
    /// </summary>
    ErrorHandlingSettings ErrorHandling { get; }
    
    /// <summary>
    /// Event bus specific settings
    /// </summary>
    EventBusSettings EventBus { get; }
    
    /// <summary>
    /// Event store specific settings
    /// </summary>
    EventStoreSettings EventStore { get; }
    
    /// <summary>
    /// Health check settings
    /// </summary>
    HealthCheckSettings HealthCheck { get; }
    
    /// <summary>
    /// Whether to automatically declare configured exchanges on startup
    /// </summary>
    bool AutoDeclareExchanges { get; }
    
    /// <summary>
    /// Whether to automatically declare configured queues on startup
    /// </summary>
    bool AutoDeclareQueues { get; }
    
    /// <summary>
    /// Whether to enable auto-recovery for connections and channels
    /// </summary>
    bool AutoRecoveryEnabled { get; }
    
    /// <summary>
    /// Global timeout for operations in milliseconds
    /// </summary>
    int OperationTimeoutMs { get; }
}