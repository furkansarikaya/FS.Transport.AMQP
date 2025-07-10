using FS.Transport.AMQP.Core;
using FS.Transport.AMQP.ErrorHandling;

namespace FS.Transport.AMQP.Producer;

/// <summary>
/// Configuration settings for the message producer
/// </summary>
public class ProducerSettings
{
    /// <summary>
    /// Producer name/identifier
    /// </summary>
    public string Name { get; set; } = "DefaultProducer";
    
    /// <summary>
    /// Default exchange for messages when not specified
    /// </summary>
    public string? DefaultExchange { get; set; }
    
    /// <summary>
    /// Default routing key when not specified
    /// </summary>
    public string? DefaultRoutingKey { get; set; }
    
    /// <summary>
    /// Default content type for messages
    /// </summary>
    public string DefaultContentType { get; set; } = "application/json";
    
    /// <summary>
    /// Default content encoding for messages
    /// </summary>
    public string DefaultContentEncoding { get; set; } = "utf-8";
    
    /// <summary>
    /// Default delivery mode (1 = non-persistent, 2 = persistent)
    /// </summary>
    public byte DefaultDeliveryMode { get; set; } = 2;
    
    /// <summary>
    /// Enable publish confirmations
    /// </summary>
    public bool EnableConfirmations { get; set; } = true;
    
    /// <summary>
    /// Timeout for confirmation acknowledgments
    /// </summary>
    public TimeSpan ConfirmationTimeout { get; set; } = TimeSpan.FromSeconds(30);
    
    /// <summary>
    /// Maximum number of unconfirmed messages
    /// </summary>
    public int MaxUnconfirmedMessages { get; set; } = 1000;
    
    /// <summary>
    /// Enable message batching for better performance
    /// </summary>
    public bool EnableBatching { get; set; } = true;
    
    /// <summary>
    /// Maximum batch size for message publishing
    /// </summary>
    public int MaxBatchSize { get; set; } = 100;
    
    /// <summary>
    /// Maximum time to wait before publishing a partial batch
    /// </summary>
    public TimeSpan BatchTimeout { get; set; } = TimeSpan.FromMilliseconds(100);
    
    /// <summary>
    /// Enable automatic retry on transient failures
    /// </summary>
    public bool EnableRetry { get; set; } = true;
    
    /// <summary>
    /// Default retry policy name
    /// </summary>
    public string DefaultRetryPolicyName { get; set; } = "DefaultRetryPolicy";
    
    /// <summary>
    /// Maximum number of retry attempts
    /// </summary>
    public int MaxRetries { get; set; } = 3;
    
    /// <summary>
    /// Base delay between retry attempts
    /// </summary>
    public TimeSpan RetryDelay { get; set; } = TimeSpan.FromSeconds(1);
    
    /// <summary>
    /// Maximum delay between retry attempts
    /// </summary>
    public TimeSpan MaxRetryDelay { get; set; } = TimeSpan.FromSeconds(30);
    
    /// <summary>
    /// Retry delay multiplier for exponential backoff
    /// </summary>
    public double RetryDelayMultiplier { get; set; } = 2.0;
    
    /// <summary>
    /// Enable transactional publishing
    /// </summary>
    public bool EnableTransactions { get; set; } = false;
    
    /// <summary>
    /// Transaction timeout
    /// </summary>
    public TimeSpan TransactionTimeout { get; set; } = TimeSpan.FromSeconds(30);
    
    /// <summary>
    /// Maximum number of concurrent transactions
    /// </summary>
    public int MaxConcurrentTransactions { get; set; } = 10;
    
    /// <summary>
    /// Enable message scheduling
    /// </summary>
    public bool EnableScheduling { get; set; } = true;
    
    /// <summary>
    /// Scheduling precision (how often to check for scheduled messages)
    /// </summary>
    public TimeSpan SchedulingPrecision { get; set; } = TimeSpan.FromSeconds(1);
    
    /// <summary>
    /// Maximum number of scheduled messages
    /// </summary>
    public int MaxScheduledMessages { get; set; } = 10000;
    
    /// <summary>
    /// Enable dead letter handling
    /// </summary>
    public bool EnableDeadLetter { get; set; } = true;
    
    /// <summary>
    /// Default dead letter exchange
    /// </summary>
    public string DefaultDeadLetterExchange { get; set; } = "dead.letter";
    
    /// <summary>
    /// Default dead letter routing key
    /// </summary>
    public string DefaultDeadLetterRoutingKey { get; set; } = "failed";
    
    /// <summary>
    /// Enable message compression
    /// </summary>
    public bool EnableCompression { get; set; } = false;
    
    /// <summary>
    /// Compression threshold (messages larger than this will be compressed)
    /// </summary>
    public int CompressionThreshold { get; set; } = 1024;
    
    /// <summary>
    /// Compression algorithm to use
    /// </summary>
    public CompressionAlgorithm CompressionAlgorithm { get; set; } = CompressionAlgorithm.GZip;
    
    /// <summary>
    /// Enable message deduplication
    /// </summary>
    public bool EnableDeduplication { get; set; } = false;
    
    /// <summary>
    /// Deduplication window (how long to remember message IDs)
    /// </summary>
    public TimeSpan DeduplicationWindow { get; set; } = TimeSpan.FromMinutes(5);
    
    /// <summary>
    /// Maximum number of message IDs to remember for deduplication
    /// </summary>
    public int MaxDeduplicationEntries { get; set; } = 100000;
    
    /// <summary>
    /// Enable message routing validation
    /// </summary>
    public bool EnableRoutingValidation { get; set; } = true;
    
    /// <summary>
    /// Enable message serialization validation
    /// </summary>
    public bool EnableSerializationValidation { get; set; } = true;
    
    /// <summary>
    /// Channel pool size for concurrent publishing
    /// </summary>
    public int ChannelPoolSize { get; set; } = 10;
    
    /// <summary>
    /// Maximum channel idle time before closing
    /// </summary>
    public TimeSpan ChannelIdleTimeout { get; set; } = TimeSpan.FromMinutes(5);
    
    /// <summary>
    /// Enable producer metrics collection
    /// </summary>
    public bool EnableMetrics { get; set; } = true;
    
    /// <summary>
    /// Metrics collection interval
    /// </summary>
    public TimeSpan MetricsInterval { get; set; } = TimeSpan.FromSeconds(30);
    
    /// <summary>
    /// Enable producer health checks
    /// </summary>
    public bool EnableHealthChecks { get; set; } = true;
    
    /// <summary>
    /// Health check interval
    /// </summary>
    public TimeSpan HealthCheckInterval { get; set; } = TimeSpan.FromSeconds(30);
    
    /// <summary>
    /// Producer startup timeout
    /// </summary>
    public TimeSpan StartupTimeout { get; set; } = TimeSpan.FromSeconds(30);
    
    /// <summary>
    /// Producer shutdown timeout
    /// </summary>
    public TimeSpan ShutdownTimeout { get; set; } = TimeSpan.FromSeconds(30);
    
    /// <summary>
    /// Enable graceful shutdown (wait for pending messages)
    /// </summary>
    public bool EnableGracefulShutdown { get; set; } = true;
    
    /// <summary>
    /// Custom error handling strategy
    /// </summary>
    public ErrorHandlingStrategy ErrorHandlingStrategy { get; set; } = ErrorHandlingStrategy.RetryThenDeadLetter;
    
    /// <summary>
    /// Custom serializer settings
    /// </summary>
    public IDictionary<string, object>? SerializerSettings { get; set; }
    
    /// <summary>
    /// Custom producer tags for identification
    /// </summary>
    public IDictionary<string, string>? Tags { get; set; }
    
    /// <summary>
    /// Creates default producer settings
    /// </summary>
    /// <returns>Default producer settings</returns>
    public static ProducerSettings CreateDefault()
    {
        return new ProducerSettings();
    }
    
    /// <summary>
    /// Creates producer settings optimized for high throughput
    /// </summary>
    /// <returns>High throughput producer settings</returns>
    public static ProducerSettings CreateHighThroughput()
    {
        return new ProducerSettings
        {
            Name = "HighThroughputProducer",
            EnableBatching = true,
            MaxBatchSize = 1000,
            BatchTimeout = TimeSpan.FromMilliseconds(50),
            EnableConfirmations = true,
            MaxUnconfirmedMessages = 10000,
            ChannelPoolSize = 20,
            EnableCompression = true,
            CompressionThreshold = 512,
            EnableDeduplication = false,
            EnableHealthChecks = true,
            MetricsInterval = TimeSpan.FromSeconds(10)
        };
    }
    
    /// <summary>
    /// Creates producer settings optimized for reliability
    /// </summary>
    /// <returns>Reliable producer settings</returns>
    public static ProducerSettings CreateReliable()
    {
        return new ProducerSettings
        {
            Name = "ReliableProducer",
            EnableConfirmations = true,
            ConfirmationTimeout = TimeSpan.FromSeconds(60),
            MaxUnconfirmedMessages = 100,
            EnableRetry = true,
            MaxRetries = 5,
            RetryDelay = TimeSpan.FromSeconds(2),
            MaxRetryDelay = TimeSpan.FromMinutes(1),
            EnableTransactions = false,
            EnableDeadLetter = true,
            EnableDeduplication = true,
            EnableRoutingValidation = true,
            EnableSerializationValidation = true,
            EnableGracefulShutdown = true
        };
    }
    
    /// <summary>
    /// Creates producer settings optimized for low latency
    /// </summary>
    /// <returns>Low latency producer settings</returns>
    public static ProducerSettings CreateLowLatency()
    {
        return new ProducerSettings
        {
            Name = "LowLatencyProducer",
            EnableBatching = false,
            EnableConfirmations = false,
            EnableRetry = false,
            EnableTransactions = false,
            EnableCompression = false,
            EnableDeduplication = false,
            EnableRoutingValidation = false,
            EnableSerializationValidation = false,
            ChannelPoolSize = 5,
            MetricsInterval = TimeSpan.FromSeconds(60),
            HealthCheckInterval = TimeSpan.FromSeconds(60)
        };
    }
    
    /// <summary>
    /// Validates the producer settings
    /// </summary>
    /// <returns>Validation result</returns>
    public SettingsValidationResult Validate()
    {
        var result = new SettingsValidationResult();
        
        if (string.IsNullOrWhiteSpace(Name))
        {
            result.Errors.Add("Producer name cannot be null or empty");
        }
        
        if (MaxBatchSize <= 0)
        {
            result.Errors.Add("MaxBatchSize must be greater than zero");
        }
        
        if (BatchTimeout <= TimeSpan.Zero)
        {
            result.Errors.Add("BatchTimeout must be greater than zero");
        }
        
        if (ConfirmationTimeout <= TimeSpan.Zero)
        {
            result.Errors.Add("ConfirmationTimeout must be greater than zero");
        }
        
        if (MaxUnconfirmedMessages <= 0)
        {
            result.Errors.Add("MaxUnconfirmedMessages must be greater than zero");
        }
        
        if (MaxRetries < 0)
        {
            result.Errors.Add("MaxRetries cannot be negative");
        }
        
        if (RetryDelay <= TimeSpan.Zero)
        {
            result.Errors.Add("RetryDelay must be greater than zero");
        }
        
        if (MaxRetryDelay <= TimeSpan.Zero)
        {
            result.Errors.Add("MaxRetryDelay must be greater than zero");
        }
        
        if (RetryDelayMultiplier <= 0)
        {
            result.Errors.Add("RetryDelayMultiplier must be greater than zero");
        }
        
        if (ChannelPoolSize <= 0)
        {
            result.Errors.Add("ChannelPoolSize must be greater than zero");
        }
        
        if (CompressionThreshold < 0)
        {
            result.Errors.Add("CompressionThreshold cannot be negative");
        }
        
        if (EnableDeduplication && DeduplicationWindow <= TimeSpan.Zero)
        {
            result.Errors.Add("DeduplicationWindow must be greater than zero when deduplication is enabled");
        }
        
        if (EnableDeduplication && MaxDeduplicationEntries <= 0)
        {
            result.Errors.Add("MaxDeduplicationEntries must be greater than zero when deduplication is enabled");
        }
        
        result.IsValid = result.Errors.Count == 0;
        return result;
    }
    
    /// <summary>
    /// Creates a copy of the producer settings
    /// </summary>
    /// <returns>Copy of producer settings</returns>
    public ProducerSettings Clone()
    {
        return new ProducerSettings
        {
            Name = Name,
            DefaultExchange = DefaultExchange,
            DefaultRoutingKey = DefaultRoutingKey,
            DefaultContentType = DefaultContentType,
            DefaultContentEncoding = DefaultContentEncoding,
            DefaultDeliveryMode = DefaultDeliveryMode,
            EnableConfirmations = EnableConfirmations,
            ConfirmationTimeout = ConfirmationTimeout,
            MaxUnconfirmedMessages = MaxUnconfirmedMessages,
            EnableBatching = EnableBatching,
            MaxBatchSize = MaxBatchSize,
            BatchTimeout = BatchTimeout,
            EnableRetry = EnableRetry,
            DefaultRetryPolicyName = DefaultRetryPolicyName,
            MaxRetries = MaxRetries,
            RetryDelay = RetryDelay,
            MaxRetryDelay = MaxRetryDelay,
            RetryDelayMultiplier = RetryDelayMultiplier,
            EnableTransactions = EnableTransactions,
            TransactionTimeout = TransactionTimeout,
            MaxConcurrentTransactions = MaxConcurrentTransactions,
            EnableScheduling = EnableScheduling,
            SchedulingPrecision = SchedulingPrecision,
            MaxScheduledMessages = MaxScheduledMessages,
            EnableDeadLetter = EnableDeadLetter,
            DefaultDeadLetterExchange = DefaultDeadLetterExchange,
            DefaultDeadLetterRoutingKey = DefaultDeadLetterRoutingKey,
            EnableCompression = EnableCompression,
            CompressionThreshold = CompressionThreshold,
            CompressionAlgorithm = CompressionAlgorithm,
            EnableDeduplication = EnableDeduplication,
            DeduplicationWindow = DeduplicationWindow,
            MaxDeduplicationEntries = MaxDeduplicationEntries,
            EnableRoutingValidation = EnableRoutingValidation,
            EnableSerializationValidation = EnableSerializationValidation,
            ChannelPoolSize = ChannelPoolSize,
            ChannelIdleTimeout = ChannelIdleTimeout,
            EnableMetrics = EnableMetrics,
            MetricsInterval = MetricsInterval,
            EnableHealthChecks = EnableHealthChecks,
            HealthCheckInterval = HealthCheckInterval,
            StartupTimeout = StartupTimeout,
            ShutdownTimeout = ShutdownTimeout,
            EnableGracefulShutdown = EnableGracefulShutdown,
            ErrorHandlingStrategy = ErrorHandlingStrategy,
            SerializerSettings = SerializerSettings?.ToDictionary(kvp => kvp.Key, kvp => kvp.Value),
            Tags = Tags?.ToDictionary(kvp => kvp.Key, kvp => kvp.Value)
        };
    }
}

/// <summary>
/// Settings validation result
/// </summary>
public class SettingsValidationResult
{
    /// <summary>
    /// Whether the settings are valid
    /// </summary>
    public bool IsValid { get; set; } = true;
    
    /// <summary>
    /// List of validation errors
    /// </summary>
    public List<string> Errors { get; set; } = new();
    
    /// <summary>
    /// List of validation warnings
    /// </summary>
    public List<string> Warnings { get; set; } = new();
}