using FS.RabbitMQ.Configuration;
using FS.RabbitMQ.Core;
using FS.RabbitMQ.ErrorHandling;

namespace FS.RabbitMQ.Consumer;

/// <summary>
/// Configuration settings for message consumer with comprehensive options for consumption behavior, error handling, and performance tuning
/// </summary>
public class ConsumerSettings
{
    /// <summary>
    /// Consumer name for identification and monitoring
    /// </summary>
    public string Name { get; set; } = "DefaultConsumer";
    
    /// <summary>
    /// Number of concurrent consumers
    /// </summary>
    public int ConcurrentConsumers { get; set; } = Environment.ProcessorCount;
    
    /// <summary>
    /// Consumer tag for RabbitMQ identification
    /// </summary>
    public string? ConsumerTag { get; set; }
    
    /// <summary>
    /// Whether to automatically acknowledge messages after processing
    /// </summary>
    public bool AutoAcknowledge { get; set; } = false;
    
    /// <summary>
    /// Quality of Service - Maximum number of unacknowledged messages
    /// </summary>
    public ushort PrefetchCount { get; set; } = 10;
    
    /// <summary>
    /// Whether to apply prefetch count globally for the connection
    /// </summary>
    public bool GlobalPrefetch { get; set; } = false;
    
    /// <summary>
    /// Consumer priority for message distribution (higher values get more messages)
    /// </summary>
    public int Priority { get; set; } = 0;
    
    /// <summary>
    /// Whether this consumer should be exclusive to the queue
    /// </summary>
    public bool Exclusive { get; set; } = false;
    
    /// <summary>
    /// Custom consumer arguments
    /// </summary>
    public IDictionary<string, object> Arguments { get; set; } = new Dictionary<string, object>();
    
    /// <summary>
    /// Maximum number of concurrent message processors
    /// </summary>
    public int MaxConcurrentMessages { get; set; } = Environment.ProcessorCount;
    
    /// <summary>
    /// Timeout for message processing before considering it failed
    /// </summary>
    public TimeSpan ProcessingTimeout { get; set; } = TimeSpan.FromMinutes(5);
    
    /// <summary>
    /// Whether to enable message deduplication
    /// </summary>
    public bool EnableDeduplication { get; set; } = false;
    
    /// <summary>
    /// Deduplication cache size
    /// </summary>
    public int DeduplicationCacheSize { get; set; } = 10000;
    
    /// <summary>
    /// Deduplication cache TTL
    /// </summary>
    public TimeSpan DeduplicationCacheTtl { get; set; } = TimeSpan.FromHours(1);
    
    /// <summary>
    /// Whether to enable message compression
    /// </summary>
    public bool EnableCompression { get; set; } = false;
    
    /// <summary>
    /// Compression algorithm to use
    /// </summary>
    public CompressionAlgorithm CompressionAlgorithm { get; set; } = CompressionAlgorithm.GZip;
    
    /// <summary>
    /// Whether to enable message encryption
    /// </summary>
    public bool EnableEncryption { get; set; } = false;
    
    /// <summary>
    /// Encryption key for message decryption
    /// </summary>
    public string? EncryptionKey { get; set; }
    
    /// <summary>
    /// Retry policy settings
    /// </summary>
    public RetryPolicySettings RetryPolicy { get; set; } = new();
    
    /// <summary>
    /// Error handling strategy
    /// </summary>
    public ErrorHandlingStrategy ErrorHandling { get; set; } = ErrorHandlingStrategy.RetryThenDeadLetter;
    
    /// <summary>
    /// Dead letter exchange for failed messages
    /// </summary>
    public string? DeadLetterExchange { get; set; }
    
    /// <summary>
    /// Dead letter routing key
    /// </summary>
    public string? DeadLetterRoutingKey { get; set; }
    
    /// <summary>
    /// Whether to enable consumer heartbeat
    /// </summary>
    public bool EnableHeartbeat { get; set; } = true;
    
    /// <summary>
    /// Heartbeat interval
    /// </summary>
    public TimeSpan HeartbeatInterval { get; set; } = TimeSpan.FromSeconds(30);
    
    /// <summary>
    /// Whether to enable consumer statistics
    /// </summary>
    public bool EnableStatistics { get; set; } = true;
    
    /// <summary>
    /// Statistics collection interval
    /// </summary>
    public TimeSpan StatisticsInterval { get; set; } = TimeSpan.FromSeconds(30);
    
    /// <summary>
    /// Whether to enable consumer metrics
    /// </summary>
    public bool EnableMetrics { get; set; } = true;
    
    /// <summary>
    /// Metrics collection interval
    /// </summary>
    public TimeSpan MetricsInterval { get; set; } = TimeSpan.FromSeconds(10);
    
    /// <summary>
    /// Whether to enable batch processing
    /// </summary>
    public bool EnableBatchProcessing { get; set; } = false;
    
    /// <summary>
    /// Batch size for batch processing
    /// </summary>
    public int BatchSize { get; set; } = 100;
    
    /// <summary>
    /// Batch timeout for batch processing
    /// </summary>
    public TimeSpan BatchTimeout { get; set; } = TimeSpan.FromSeconds(5);
    
    /// <summary>
    /// Whether to enable circuit breaker pattern
    /// </summary>
    public bool EnableCircuitBreaker { get; set; } = true;
    
    /// <summary>
    /// Circuit breaker failure threshold
    /// </summary>
    public int CircuitBreakerFailureThreshold { get; set; } = 5;
    
    /// <summary>
    /// Circuit breaker recovery timeout
    /// </summary>
    public TimeSpan CircuitBreakerRecoveryTimeout { get; set; } = TimeSpan.FromMinutes(1);
    
    /// <summary>
    /// Whether to enable consumer pause/resume functionality
    /// </summary>
    public bool EnablePauseResume { get; set; } = true;
    
    /// <summary>
    /// Custom serializer settings
    /// </summary>
    public SerializerSettings Serializer { get; set; } = new();
    
    /// <summary>
    /// Connection recovery settings
    /// </summary>
    public ConnectionRecoverySettings ConnectionRecovery { get; set; } = new();
    
    /// <summary>
    /// Validates the consumer settings
    /// </summary>
    /// <exception cref="ArgumentException">Thrown when settings are invalid</exception>
    public void Validate()
    {
        if (string.IsNullOrWhiteSpace(Name))
            throw new ArgumentException("Consumer name cannot be empty", nameof(Name));
            
        if (PrefetchCount == 0)
            throw new ArgumentException("PrefetchCount must be greater than 0", nameof(PrefetchCount));
            
        if (MaxConcurrentMessages <= 0)
            throw new ArgumentException("MaxConcurrentMessages must be greater than 0", nameof(MaxConcurrentMessages));
            
        if (ProcessingTimeout <= TimeSpan.Zero)
            throw new ArgumentException("ProcessingTimeout must be greater than zero", nameof(ProcessingTimeout));
            
        if (EnableDeduplication && DeduplicationCacheSize <= 0)
            throw new ArgumentException("DeduplicationCacheSize must be greater than 0 when deduplication is enabled", nameof(DeduplicationCacheSize));
            
        if (EnableEncryption && string.IsNullOrWhiteSpace(EncryptionKey))
            throw new ArgumentException("EncryptionKey is required when encryption is enabled", nameof(EncryptionKey));
            
        if (EnableBatchProcessing && BatchSize <= 0)
            throw new ArgumentException("BatchSize must be greater than 0 when batch processing is enabled", nameof(BatchSize));
            
        if (EnableCircuitBreaker && CircuitBreakerFailureThreshold <= 0)
            throw new ArgumentException("CircuitBreakerFailureThreshold must be greater than 0 when circuit breaker is enabled", nameof(CircuitBreakerFailureThreshold));
            
        // Validate nested settings
        RetryPolicy.Validate();
        Serializer.Validate();
        ConnectionRecovery.Validate();
    }
    
    /// <summary>
    /// Creates a copy of the consumer settings
    /// </summary>
    /// <returns>New instance with copied values</returns>
    public ConsumerSettings Clone()
    {
        return new ConsumerSettings
        {
            Name = Name,
            ConcurrentConsumers = ConcurrentConsumers,
            ConsumerTag = ConsumerTag,
            AutoAcknowledge = AutoAcknowledge,
            PrefetchCount = PrefetchCount,
            GlobalPrefetch = GlobalPrefetch,
            Priority = Priority,
            Exclusive = Exclusive,
            Arguments = new Dictionary<string, object>(Arguments),
            MaxConcurrentMessages = MaxConcurrentMessages,
            ProcessingTimeout = ProcessingTimeout,
            EnableDeduplication = EnableDeduplication,
            DeduplicationCacheSize = DeduplicationCacheSize,
            DeduplicationCacheTtl = DeduplicationCacheTtl,
            EnableCompression = EnableCompression,
            CompressionAlgorithm = CompressionAlgorithm,
            EnableEncryption = EnableEncryption,
            EncryptionKey = EncryptionKey,
            RetryPolicy = RetryPolicy.Clone(),
            ErrorHandling = ErrorHandling,
            DeadLetterExchange = DeadLetterExchange,
            DeadLetterRoutingKey = DeadLetterRoutingKey,
            EnableHeartbeat = EnableHeartbeat,
            HeartbeatInterval = HeartbeatInterval,
            EnableStatistics = EnableStatistics,
            StatisticsInterval = StatisticsInterval,
            EnableMetrics = EnableMetrics,
            MetricsInterval = MetricsInterval,
            EnableBatchProcessing = EnableBatchProcessing,
            BatchSize = BatchSize,
            BatchTimeout = BatchTimeout,
            EnableCircuitBreaker = EnableCircuitBreaker,
            CircuitBreakerFailureThreshold = CircuitBreakerFailureThreshold,
            CircuitBreakerRecoveryTimeout = CircuitBreakerRecoveryTimeout,
            EnablePauseResume = EnablePauseResume,
            Serializer = Serializer.Clone(),
            ConnectionRecovery = ConnectionRecovery.Clone()
        };
    }
    
    /// <summary>
    /// Creates default consumer settings for high-throughput scenarios
    /// </summary>
    /// <returns>High-throughput consumer settings</returns>
    public static ConsumerSettings CreateHighThroughput()
    {
        return new ConsumerSettings
        {
            Name = "HighThroughputConsumer",
            PrefetchCount = 1000,
            MaxConcurrentMessages = Environment.ProcessorCount * 4,
            ProcessingTimeout = TimeSpan.FromSeconds(30),
            EnableBatchProcessing = true,
            BatchSize = 100,
            BatchTimeout = TimeSpan.FromSeconds(1),
            EnableCircuitBreaker = true,
            CircuitBreakerFailureThreshold = 10,
            EnableStatistics = true,
            StatisticsInterval = TimeSpan.FromSeconds(10)
        };
    }
    
    /// <summary>
    /// Creates default consumer settings for low-latency scenarios
    /// </summary>
    /// <returns>Low-latency consumer settings</returns>
    public static ConsumerSettings CreateLowLatency()
    {
        return new ConsumerSettings
        {
            Name = "LowLatencyConsumer",
            PrefetchCount = 1,
            MaxConcurrentMessages = 1,
            ProcessingTimeout = TimeSpan.FromSeconds(5),
            EnableBatchProcessing = false,
            EnableCircuitBreaker = false,
            EnableStatistics = true,
            StatisticsInterval = TimeSpan.FromSeconds(5)
        };
    }
    
    /// <summary>
    /// Creates default consumer settings for reliable processing
    /// </summary>
    /// <returns>Reliable consumer settings</returns>
    public static ConsumerSettings CreateReliable()
    {
        return new ConsumerSettings
        {
            Name = "ReliableConsumer",
            AutoAcknowledge = false,
            PrefetchCount = 10,
            MaxConcurrentMessages = Environment.ProcessorCount,
            ProcessingTimeout = TimeSpan.FromMinutes(5),
            EnableDeduplication = true,
            DeduplicationCacheSize = 10000,
            RetryPolicy = new RetryPolicySettings
            {
                MaxRetries = 3,
                InitialDelay = TimeSpan.FromSeconds(1),
                MaxDelay = TimeSpan.FromMinutes(5),
                BackoffMultiplier = 2.0
            },
            ErrorHandling = ErrorHandlingStrategy.RetryThenDeadLetter,
            EnableCircuitBreaker = true,
            CircuitBreakerFailureThreshold = 5,
            EnableStatistics = true,
            EnableMetrics = true
        };
    }
}