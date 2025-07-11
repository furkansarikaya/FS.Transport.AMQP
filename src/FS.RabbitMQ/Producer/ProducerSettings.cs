namespace FS.RabbitMQ.Producer;

/// <summary>
/// Configuration settings for message producers
/// </summary>
public class ProducerSettings
{
    /// <summary>
    /// Gets or sets the producer name
    /// </summary>
    public string Name { get; set; } = "DefaultProducer";

    /// <summary>
    /// Gets or sets whether publisher confirms are enabled
    /// </summary>
    public bool EnableConfirms { get; set; } = true;

    /// <summary>
    /// Gets or sets the timeout for publisher confirms
    /// </summary>
    public TimeSpan ConfirmTimeout { get; set; } = TimeSpan.FromSeconds(30);

    /// <summary>
    /// Gets or sets whether messages should be persistent
    /// </summary>
    public bool PersistentMessages { get; set; } = true;

    /// <summary>
    /// Gets or sets whether to enable mandatory flag by default
    /// </summary>
    public bool MandatoryByDefault { get; set; } = false;

    /// <summary>
    /// Gets or sets the maximum number of concurrent publish operations
    /// </summary>
    public int MaxConcurrentPublishes { get; set; } = 100;

    /// <summary>
    /// Gets or sets the batch size for batch operations
    /// </summary>
    public int BatchSize { get; set; } = 100;

    /// <summary>
    /// Gets or sets the timeout for batch operations
    /// </summary>
    public TimeSpan BatchTimeout { get; set; } = TimeSpan.FromSeconds(5);

    /// <summary>
    /// Gets or sets whether to use transactions
    /// </summary>
    public bool UseTransactions { get; set; } = false;

    /// <summary>
    /// Gets or sets the retry policy for failed publishes
    /// </summary>
    public RetryPolicy RetryPolicy { get; set; } = RetryPolicy.None;

    /// <summary>
    /// Gets or sets the maximum number of retry attempts
    /// </summary>
    public int MaxRetryAttempts { get; set; } = 3;

    /// <summary>
    /// Gets or sets the retry delay
    /// </summary>
    public TimeSpan RetryDelay { get; set; } = TimeSpan.FromSeconds(1);

    /// <summary>
    /// Gets or sets whether to enable deduplication
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
    /// Gets or sets custom headers to add to all messages
    /// </summary>
    public Dictionary<string, object> DefaultHeaders { get; set; } = new();

    /// <summary>
    /// Gets or sets whether to enable message compression
    /// </summary>
    public bool EnableCompression { get; set; } = false;

    /// <summary>
    /// Gets or sets the timeout for publisher confirms
    /// </summary>
    public TimeSpan ConfirmationTimeout { get; set; } = TimeSpan.FromSeconds(30);

    /// <summary>
    /// Gets or sets custom arguments for the producer
    /// </summary>
    public Dictionary<string, object> Arguments { get; set; } = new Dictionary<string, object>();

    /// <summary>
    /// Gets or sets the maximum number of retries (alias for backward compatibility)
    /// </summary>
    public int MaxRetries
    {
        get => MaxRetryAttempts;
        set => MaxRetryAttempts = value;
    }

    /// <summary>
    /// Validates the producer settings
    /// </summary>
    /// <returns>True if settings are valid, false otherwise</returns>
    public bool Validate()
    {
        if (string.IsNullOrWhiteSpace(Name))
            return false;

        if (ConfirmTimeout <= TimeSpan.Zero)
            return false;

        if (ConfirmationTimeout <= TimeSpan.Zero)
            return false;

        return true;
    }

    /// <summary>
    /// Creates default settings for high throughput scenarios
    /// </summary>
    /// <returns>ProducerSettings configured for high throughput</returns>
    public static ProducerSettings CreateHighThroughput()
    {
        return new ProducerSettings
        {
            Name = "HighThroughputProducer",
            EnableConfirms = true,
            PersistentMessages = false,
            ConfirmTimeout = TimeSpan.FromSeconds(5),
            ConfirmationTimeout = TimeSpan.FromSeconds(5),
            EnableCompression = false
        };
    }

    /// <summary>
    /// Creates default settings for reliable messaging
    /// </summary>
    /// <returns>ProducerSettings configured for reliability</returns>
    public static ProducerSettings CreateReliable()
    {
        return new ProducerSettings
        {
            Name = "ReliableProducer", 
            EnableConfirms = true,
            PersistentMessages = true,
            ConfirmTimeout = TimeSpan.FromSeconds(60),
            ConfirmationTimeout = TimeSpan.FromSeconds(60),
            EnableCompression = false
        };
    }
}

/// <summary>
/// Retry policy options for producers
/// </summary>
public enum RetryPolicy
{
    /// <summary>
    /// No retry policy
    /// </summary>
    None,

    /// <summary>
    /// Linear retry with fixed delay
    /// </summary>
    Linear,

    /// <summary>
    /// Exponential backoff retry
    /// </summary>
    ExponentialBackoff,

    /// <summary>
    /// Circuit breaker pattern
    /// </summary>
    CircuitBreaker
}