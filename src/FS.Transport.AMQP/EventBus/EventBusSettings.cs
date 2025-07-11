using FS.Transport.AMQP.Configuration;
using FS.Transport.AMQP.Core;
using RabbitMQ.Client;

namespace FS.Transport.AMQP.EventBus;

/// <summary>
/// Configuration settings for the event bus
/// </summary>
public class EventBusSettings
{
    /// <summary>
    /// Event bus name/identifier
    /// </summary>
    public string Name { get; set; } = "DefaultEventBus";
    
    /// <summary>
    /// Whether the event bus is enabled
    /// </summary>
    public bool Enabled { get; set; } = true;
    
    /// <summary>
    /// Default exchange name for events
    /// </summary>
    public string DefaultExchange { get; set; } = "events";
    
    /// <summary>
    /// Whether to enable event store functionality
    /// </summary>
    public bool EnableEventStore { get; set; } = true;
    
    /// <summary>
    /// Whether to enable domain events
    /// </summary>
    public bool EnableDomainEvents { get; set; } = true;
    
    /// <summary>
    /// Whether to enable integration events
    /// </summary>
    public bool EnableIntegrationEvents { get; set; } = true;
    
    /// <summary>
    /// Maximum number of concurrent event processing
    /// </summary>
    public int MaxConcurrentEvents { get; set; } = 10;
    
    /// <summary>
    /// Event processing timeout in milliseconds
    /// </summary>
    public int ProcessingTimeoutMs { get; set; } = 30000; // 30 seconds
    
    /// <summary>
    /// Whether to automatically acknowledge messages after successful processing
    /// </summary>
    public bool AutoAck { get; set; } = true;
    
    /// <summary>
    /// Whether to use message prefetch count
    /// </summary>
    public bool UsePrefetch { get; set; } = true;
    
    /// <summary>
    /// Message prefetch count
    /// </summary>
    public ushort PrefetchCount { get; set; } = 10;
    
    /// <summary>
    /// Whether to enable dead letter handling
    /// </summary>
    public bool EnableDeadLetterHandling { get; set; } = true;
    
    /// <summary>
    /// Dead letter exchange name
    /// </summary>
    public string DeadLetterExchange { get; set; } = "dead-letter-events";
    
    /// <summary>
    /// Dead letter routing key
    /// </summary>
    public string DeadLetterRoutingKey { get; set; } = "dead-letter";
    
    /// <summary>
    /// Maximum number of retry attempts for failed events
    /// </summary>
    public int MaxRetryAttempts { get; set; } = 3;
    
    /// <summary>
    /// Retry delay in milliseconds
    /// </summary>
    public int RetryDelayMs { get; set; } = 1000;
    
    /// <summary>
    /// Whether to use exponential backoff for retries
    /// </summary>
    public bool UseExponentialBackoff { get; set; } = true;
    
    /// <summary>
    /// Domain events exchange name
    /// </summary>
    public string DomainEventsExchange { get; set; } = "domain-events";
    
    /// <summary>
    /// Integration events exchange name
    /// </summary>
    public string IntegrationEventsExchange { get; set; } = "integration-events";
    
    /// <summary>
    /// Whether domain events exchange should be durable
    /// </summary>
    public bool DomainEventsExchangeDurable { get; set; } = true;
    
    /// <summary>
    /// Whether integration events exchange should be durable
    /// </summary>
    public bool IntegrationEventsExchangeDurable { get; set; } = true;
    
    /// <summary>
    /// Exchange type for domain events
    /// </summary>
    public string DomainEventsExchangeType { get; set; } = ExchangeType.Topic;
    
    /// <summary>
    /// Exchange type for integration events
    /// </summary>
    public string IntegrationEventsExchangeType { get; set; } = ExchangeType.Topic;
    
    /// <summary>
    /// Whether queues should be durable
    /// </summary>
    public bool QueuesDurable { get; set; } = true;
    
    /// <summary>
    /// Whether queues should be exclusive
    /// </summary>
    public bool QueuesExclusive { get; set; } = false;
    
    /// <summary>
    /// Whether queues should auto-delete
    /// </summary>
    public bool QueuesAutoDelete { get; set; } = false;
    
    /// <summary>
    /// Queue message TTL in milliseconds (0 = no TTL)
    /// </summary>
    public int QueueMessageTtlMs { get; set; } = 0;
    
    /// <summary>
    /// Whether to enable message compression
    /// </summary>
    public bool EnableCompression { get; set; } = false;
    
    /// <summary>
    /// Compression algorithm to use
    /// </summary>
    public CompressionAlgorithm CompressionAlgorithm { get; set; } = CompressionAlgorithm.None;
    
    /// <summary>
    /// Whether to enable message encryption
    /// </summary>
    public bool EnableEncryption { get; set; } = false;
    
    /// <summary>
    /// Encryption key for message encryption
    /// </summary>
    public string? EncryptionKey { get; set; }
    
    /// <summary>
    /// Whether to enable performance monitoring
    /// </summary>
    public bool EnablePerformanceMonitoring { get; set; } = true;
    
    /// <summary>
    /// Performance monitoring interval in milliseconds
    /// </summary>
    public int PerformanceMonitoringIntervalMs { get; set; } = 10000; // 10 seconds
    
    /// <summary>
    /// Whether to enable detailed logging
    /// </summary>
    public bool EnableDetailedLogging { get; set; } = false;
    
    /// <summary>
    /// Connection settings for the event bus
    /// </summary>
    public ConnectionSettings ConnectionSettings { get; set; } = new();
    
    /// <summary>
    /// Serializer settings for event serialization
    /// </summary>
    public SerializerSettings SerializerSettings { get; set; } = new();
    
    /// <summary>
    /// Error handling settings
    /// </summary>
    public ErrorHandlingSettings ErrorHandlingSettings { get; set; } = new();
    
    /// <summary>
    /// Retry policy settings
    /// </summary>
    public RetryPolicySettings RetryPolicySettings { get; set; } = new();
    
    /// <summary>
    /// Health check settings
    /// </summary>
    public HealthCheckSettings HealthCheckSettings { get; set; } = new();
    
    /// <summary>
    /// Validates the event bus settings
    /// </summary>
    /// <returns>True if settings are valid, false otherwise</returns>
    public bool IsValid()
    {
        if (string.IsNullOrWhiteSpace(Name))
            return false;
            
        if (MaxConcurrentEvents <= 0)
            return false;
            
        if (ProcessingTimeoutMs <= 0)
            return false;
            
        if (PrefetchCount == 0)
            return false;
            
        if (string.IsNullOrWhiteSpace(DomainEventsExchange))
            return false;
            
        if (string.IsNullOrWhiteSpace(IntegrationEventsExchange))
            return false;
            
        if (EnableEncryption && string.IsNullOrWhiteSpace(EncryptionKey))
            return false;
            
        return true;
    }
    
    /// <summary>
    /// Validates the event bus settings and throws an exception if invalid
    /// </summary>
    public void Validate()
    {
        if (!IsValid())
            throw new InvalidOperationException("EventBusSettings validation failed");
    }
    
    /// <summary>
    /// Gets the processing timeout as TimeSpan
    /// </summary>
    public TimeSpan ProcessingTimeout => TimeSpan.FromMilliseconds(ProcessingTimeoutMs);
    
    /// <summary>
    /// Gets the retry delay as TimeSpan
    /// </summary>
    public TimeSpan RetryDelay => TimeSpan.FromMilliseconds(RetryDelayMs);
    
    /// <summary>
    /// Gets the performance monitoring interval as TimeSpan
    /// </summary>
    public TimeSpan PerformanceMonitoringInterval => TimeSpan.FromMilliseconds(PerformanceMonitoringIntervalMs);
    
    /// <summary>
    /// Gets the queue message TTL as TimeSpan (null if no TTL)
    /// </summary>
    public TimeSpan? QueueMessageTtl => QueueMessageTtlMs > 0 ? TimeSpan.FromMilliseconds(QueueMessageTtlMs) : null;
}