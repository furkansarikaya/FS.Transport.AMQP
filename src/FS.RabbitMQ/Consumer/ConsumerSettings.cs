namespace FS.RabbitMQ.Consumer;

/// <summary>
/// Configuration settings for message consumers
/// </summary>
public class ConsumerSettings
{
    /// <summary>
    /// Gets or sets the consumer name
    /// </summary>
    public string Name { get; set; } = "DefaultConsumer";

    /// <summary>
    /// Gets or sets the prefetch count for the consumer
    /// </summary>
    public ushort PrefetchCount { get; set; } = 10;

    /// <summary>
    /// Gets or sets whether to automatically acknowledge messages
    /// </summary>
    public bool AutoAck { get; set; } = false;

    /// <summary>
    /// Gets or sets the maximum number of concurrent messages to process
    /// </summary>
    public int MaxConcurrentMessages { get; set; } = 10;

    /// <summary>
    /// Gets or sets the consumer timeout
    /// </summary>
    public TimeSpan ConsumerTimeout { get; set; } = TimeSpan.FromMinutes(5);

    /// <summary>
    /// Gets or sets whether to enable dead letter handling
    /// </summary>
    public bool EnableDeadLetter { get; set; } = true;

    /// <summary>
    /// Gets or sets the maximum number of retry attempts
    /// </summary>
    public int MaxRetryAttempts { get; set; } = 3;

    /// <summary>
    /// Gets or sets the retry delay
    /// </summary>
    public TimeSpan RetryDelay { get; set; } = TimeSpan.FromSeconds(1);

    /// <summary>
    /// Gets or sets whether to enable message deduplication
    /// </summary>
    public bool EnableDeduplication { get; set; } = false;

    /// <summary>
    /// Gets or sets the deduplication cache size
    /// </summary>
    public int DeduplicationCacheSize { get; set; } = 10000;

    /// <summary>
    /// Gets or sets the deduplication cache expiry time
    /// </summary>
    public TimeSpan DeduplicationCacheExpiry { get; set; } = TimeSpan.FromMinutes(10);

    /// <summary>
    /// Gets or sets whether to enable batch processing
    /// </summary>
    public bool EnableBatchProcessing { get; set; } = false;

    /// <summary>
    /// Gets or sets the batch size for batch processing
    /// </summary>
    public int BatchSize { get; set; } = 10;

    /// <summary>
    /// Gets or sets the batch timeout
    /// </summary>
    public TimeSpan BatchTimeout { get; set; } = TimeSpan.FromSeconds(5);

    /// <summary>
    /// Gets or sets the statistics interval
    /// </summary>
    public TimeSpan StatisticsInterval { get; set; } = TimeSpan.FromSeconds(30);

    /// <summary>
    /// Gets or sets the heartbeat interval
    /// </summary>
    public TimeSpan HeartbeatInterval { get; set; } = TimeSpan.FromMinutes(1);

    /// <summary>
    /// Gets or sets whether messages should be automatically acknowledged
    /// </summary>
    public bool AutoAcknowledge { get; set; } = false;

    /// <summary>
    /// Gets or sets whether to enable global prefetch
    /// </summary>
    public bool GlobalPrefetch { get; set; } = false;

    /// <summary>
    /// Gets or sets the consumer priority
    /// </summary>
    public int Priority { get; set; } = 0;

    /// <summary>
    /// Gets or sets whether this consumer is exclusive
    /// </summary>
    public bool Exclusive { get; set; } = false;

    /// <summary>
    /// Gets or sets the processing timeout
    /// </summary>
    public TimeSpan ProcessingTimeout { get; set; } = TimeSpan.FromMinutes(5);

    /// <summary>
    /// Gets or sets the deduplication cache TTL
    /// </summary>
    public TimeSpan DeduplicationCacheTtl { get; set; } = TimeSpan.FromMinutes(30);

    /// <summary>
    /// Gets or sets the retry policy name
    /// </summary>
    public string? RetryPolicy { get; set; }

    /// <summary>
    /// Gets or sets the error handling strategy
    /// </summary>
    public string? ErrorHandling { get; set; }

    /// <summary>
    /// Gets or sets the dead letter exchange
    /// </summary>
    public string? DeadLetterExchange { get; set; }

    /// <summary>
    /// Gets or sets the dead letter routing key
    /// </summary>
    public string? DeadLetterRoutingKey { get; set; }

    /// <summary>
    /// Gets or sets whether to enable circuit breaker
    /// </summary>
    public bool EnableCircuitBreaker { get; set; } = false;

    /// <summary>
    /// Gets or sets the circuit breaker failure threshold
    /// </summary>
    public int CircuitBreakerFailureThreshold { get; set; } = 10;

    /// <summary>
    /// Gets or sets the circuit breaker recovery timeout
    /// </summary>
    public TimeSpan CircuitBreakerRecoveryTimeout { get; set; } = TimeSpan.FromMinutes(5);

    /// <summary>
    /// Gets or sets whether to enable pause/resume functionality
    /// </summary>
    public bool EnablePauseResume { get; set; } = false;

    /// <summary>
    /// Gets or sets the serializer type
    /// </summary>
    public string? Serializer { get; set; }

    /// <summary>
    /// Gets or sets whether to enable heartbeat
    /// </summary>
    public bool EnableHeartbeat { get; set; } = true;

    /// <summary>
    /// Gets or sets whether to enable statistics
    /// </summary>
    public bool EnableStatistics { get; set; } = true;

    /// <summary>
    /// Gets or sets whether to enable metrics
    /// </summary>
    public bool EnableMetrics { get; set; } = true;

    /// <summary>
    /// Gets or sets the metrics interval
    /// </summary>
    public TimeSpan MetricsInterval { get; set; } = TimeSpan.FromMinutes(1);

    /// <summary>
    /// Gets or sets whether to enable compression
    /// </summary>
    public bool EnableCompression { get; set; } = false;

    /// <summary>
    /// Gets or sets the compression algorithm
    /// </summary>
    public string? CompressionAlgorithm { get; set; }

    /// <summary>
    /// Gets or sets whether to enable encryption
    /// </summary>
    public bool EnableEncryption { get; set; } = false;

    /// <summary>
    /// Gets or sets the encryption key
    /// </summary>
    public string? EncryptionKey { get; set; }

    /// <summary>
    /// Gets or sets connection recovery settings
    /// </summary>
    public string? ConnectionRecovery { get; set; }

    /// <summary>
    /// Gets or sets the consumer tag
    /// </summary>
    public string? ConsumerTag { get; set; }

    /// <summary>
    /// Gets or sets the number of concurrent consumers
    /// </summary>
    public int ConcurrentConsumers { get; set; } = 1;

    /// <summary>
    /// Gets or sets custom arguments for the consumer
    /// </summary>
    public Dictionary<string, object> Arguments { get; set; } = new Dictionary<string, object>();

    /// <summary>
    /// Validates the consumer settings
    /// </summary>
    /// <returns>True if settings are valid, false otherwise</returns>
    public bool Validate()
    {
        if (string.IsNullOrWhiteSpace(Name))
            return false;

        if (PrefetchCount <= 0)
            return false;

        if (MaxConcurrentMessages <= 0)
            return false;

        if (ProcessingTimeout <= TimeSpan.Zero)
            return false;

        return true;
    }

    /// <summary>
    /// Creates a copy of the current settings
    /// </summary>
    /// <returns>A new ConsumerSettings instance with the same values</returns>
    public ConsumerSettings Clone()
    {
        return new ConsumerSettings
        {
            Name = Name,
            PrefetchCount = PrefetchCount,
            AutoAck = AutoAck,
            MaxConcurrentMessages = MaxConcurrentMessages,
            ConsumerTimeout = ConsumerTimeout,
            AutoAcknowledge = AutoAcknowledge,
            GlobalPrefetch = GlobalPrefetch,
            Priority = Priority,
            Exclusive = Exclusive,
            ProcessingTimeout = ProcessingTimeout,
            DeduplicationCacheTtl = DeduplicationCacheTtl,
            RetryPolicy = RetryPolicy,
            ErrorHandling = ErrorHandling,
            DeadLetterExchange = DeadLetterExchange,
            DeadLetterRoutingKey = DeadLetterRoutingKey,
            EnableCircuitBreaker = EnableCircuitBreaker,
            CircuitBreakerFailureThreshold = CircuitBreakerFailureThreshold,
            CircuitBreakerRecoveryTimeout = CircuitBreakerRecoveryTimeout,
            EnablePauseResume = EnablePauseResume,
            Serializer = Serializer,
            EnableHeartbeat = EnableHeartbeat,
            EnableStatistics = EnableStatistics,
            EnableMetrics = EnableMetrics,
            MetricsInterval = MetricsInterval,
            EnableCompression = EnableCompression,
            CompressionAlgorithm = CompressionAlgorithm,
            EnableEncryption = EnableEncryption,
            EncryptionKey = EncryptionKey,
            ConnectionRecovery = ConnectionRecovery,
            ConsumerTag = ConsumerTag,
            ConcurrentConsumers = ConcurrentConsumers,
            Arguments = new Dictionary<string, object>(Arguments)
        };
    }

    /// <summary>
    /// Creates settings optimized for high throughput
    /// </summary>
    public static ConsumerSettings CreateHighThroughput()
    {
        return new ConsumerSettings
        {
            Name = "HighThroughputConsumer",
            PrefetchCount = 100,
            MaxConcurrentMessages = 50,
            AutoAcknowledge = true,
            EnableMetrics = true,
            EnableStatistics = true
        };
    }

    /// <summary>
    /// Creates settings optimized for low latency
    /// </summary>
    public static ConsumerSettings CreateLowLatency()
    {
        return new ConsumerSettings
        {
            Name = "LowLatencyConsumer",
            PrefetchCount = 1,
            MaxConcurrentMessages = 1,
            AutoAcknowledge = false,
            EnableMetrics = true,
            EnableStatistics = true
        };
    }

    /// <summary>
    /// Creates settings optimized for reliability
    /// </summary>
    public static ConsumerSettings CreateReliable()
    {
        return new ConsumerSettings
        {
            Name = "ReliableConsumer",
            PrefetchCount = 10,
            MaxConcurrentMessages = 5,
            AutoAcknowledge = false,
            EnableMetrics = true,
            EnableStatistics = true,
            EnableCircuitBreaker = true
        };
    }
}