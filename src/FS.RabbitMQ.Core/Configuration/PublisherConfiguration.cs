using FS.RabbitMQ.Core.Abstractions;

namespace FS.RabbitMQ.Core.Configuration;

/// <summary>
/// Contains configuration for message publishing operations and behavior.
/// </summary>
/// <remarks>
/// Publisher configuration defines the default behavior for message publishing,
/// including delivery guarantees, serialization settings, performance optimizations,
/// and reliability mechanisms.
/// </remarks>
public sealed class PublisherConfiguration
{
    /// <summary>
    /// Gets or sets a value indicating whether publisher confirms are enabled.
    /// </summary>
    /// <value><c>true</c> to enable publisher confirms for delivery guarantees; otherwise, <c>false</c>. Default is <c>true</c>.</value>
    /// <remarks>
    /// Publisher confirms provide delivery acknowledgments from the broker,
    /// ensuring that messages are successfully routed and persisted.
    /// </remarks>
    public bool EnablePublisherConfirms { get; set; } = true;

    /// <summary>
    /// Gets or sets the timeout for publisher confirmation waiting.
    /// </summary>
    /// <value>The maximum time to wait for publisher confirmations. Default is 30 seconds.</value>
    /// <remarks>
    /// Confirmation timeout balances reliability with performance by limiting
    /// how long the publisher waits for broker acknowledgments.
    /// </remarks>
    public TimeSpan ConfirmationTimeout { get; set; } = TimeSpan.FromSeconds(30);

    /// <summary>
    /// Gets or sets a value indicating whether mandatory publishing is enabled.
    /// </summary>
    /// <value><c>true</c> to require messages to be routed to at least one queue; otherwise, <c>false</c>. Default is <c>false</c>.</value>
    /// <remarks>
    /// Mandatory publishing ensures that messages are routed to at least one queue,
    /// providing early detection of routing failures.
    /// </remarks>
    public bool MandatoryPublishing { get; set; } = false;

    /// <summary>
    /// Gets or sets a value indicating whether immediate publishing is enabled.
    /// </summary>
    /// <value><c>true</c> to require immediate delivery to consumers; otherwise, <c>false</c>. Default is <c>false</c>.</value>
    /// <remarks>
    /// Immediate publishing requires that messages be delivered to consumers immediately,
    /// returning unroutable messages if no consumers are available.
    /// Note: This feature is deprecated in newer RabbitMQ versions.
    /// </remarks>
    public bool ImmediatePublishing { get; set; } = false;

    /// <summary>
    /// Gets or sets the default message persistence setting.
    /// </summary>
    /// <value><c>true</c> to persist messages to disk by default; otherwise, <c>false</c>. Default is <c>true</c>.</value>
    /// <remarks>
    /// Message persistence ensures that messages survive broker restarts,
    /// providing durability guarantees at the cost of performance.
    /// </remarks>
    public bool DefaultPersistence { get; set; } = true;

    /// <summary>
    /// Gets or sets the default message time-to-live.
    /// </summary>
    /// <value>The default TTL for messages, or null for no expiration. Default is null.</value>
    /// <remarks>
    /// Message TTL prevents old messages from accumulating in queues,
    /// automatically removing messages that exceed their lifetime.
    /// </remarks>
    public TimeSpan? DefaultTimeToLive { get; set; }

    /// <summary>
    /// Gets or sets the default message priority.
    /// </summary>
    /// <value>The default priority for messages (0-255), or null for no priority. Default is null.</value>
    /// <remarks>
    /// Message priority enables preferential processing of high-priority messages
    /// when queues support priority ordering.
    /// </remarks>
    public byte? DefaultPriority { get; set; }

    /// <summary>
    /// Gets or sets the retry policy for publish operations.
    /// </summary>
    /// <value>The retry policy for handling publish failures.</value>
    /// <remarks>
    /// Publish retry policy defines how the system retries failed publish operations,
    /// providing resilience against transient network and broker issues.
    /// </remarks>
    public RetryPolicy? PublishRetryPolicy { get; set; }

    /// <summary>
    /// Gets or sets the default content type for message serialization.
    /// </summary>
    /// <value>The MIME type for message content. Default is "application/json".</value>
    /// <remarks>
    /// Content type enables proper message deserialization and interoperability
    /// with different systems and serialization formats.
    /// </remarks>
    public string DefaultContentType { get; set; } = "application/json";

    /// <summary>
    /// Gets or sets the default message encoding.
    /// </summary>
    /// <value>The text encoding for message content. Default is "utf-8".</value>
    /// <remarks>
    /// Content encoding ensures proper text handling across different systems
    /// and character sets.
    /// </remarks>
    public string DefaultContentEncoding { get; set; } = "utf-8";

    /// <summary>
    /// Creates a publisher configuration suitable for development environments.
    /// </summary>
    /// <returns>A publisher configuration with development-friendly defaults.</returns>
    public static PublisherConfiguration CreateDevelopment()
    {
        return new PublisherConfiguration
        {
            EnablePublisherConfirms = true,
            ConfirmationTimeout = TimeSpan.FromSeconds(10),
            MandatoryPublishing = false,
            ImmediatePublishing = false,
            DefaultPersistence = false, // Better performance for dev
            PublishRetryPolicy = RetryPolicy.CreateImmediate(maxAttempts: 3)
        };
    }

    /// <summary>
    /// Creates a publisher configuration suitable for production environments.
    /// </summary>
    /// <returns>A publisher configuration with production-ready defaults.</returns>
    public static PublisherConfiguration CreateProduction()
    {
        return new PublisherConfiguration
        {
            EnablePublisherConfirms = true,
            ConfirmationTimeout = TimeSpan.FromSeconds(30),
            MandatoryPublishing = true,
            ImmediatePublishing = false,
            DefaultPersistence = true,
            PublishRetryPolicy = RetryPolicy.CreateExponentialBackoff(
                baseDelay: TimeSpan.FromSeconds(1),
                maxDelay: TimeSpan.FromMinutes(2),
                maxAttempts: 5)
        };
    }

    /// <summary>
    /// Creates a publisher configuration optimized for high-throughput scenarios.
    /// </summary>
    /// <returns>A publisher configuration optimized for performance.</returns>
    public static PublisherConfiguration CreateHighThroughput()
    {
        return new PublisherConfiguration
        {
            EnablePublisherConfirms = false, // Better performance
            ConfirmationTimeout = TimeSpan.FromSeconds(5),
            MandatoryPublishing = false,
            ImmediatePublishing = false,
            DefaultPersistence = false, // Better performance
            PublishRetryPolicy = RetryPolicy.CreateLinear(
                delay: TimeSpan.FromMilliseconds(100),
                maxAttempts: 3)
        };
    }

    /// <summary>
    /// Validates the publisher configuration for consistency and reasonableness.
    /// </summary>
    /// <exception cref="BrokerConfigurationException">Thrown when the configuration is invalid.</exception>
    public void Validate()
    {
        if (ConfirmationTimeout <= TimeSpan.Zero)
        {
            throw new BrokerConfigurationException(
                "Confirmation timeout must be positive.",
                configurationSection: nameof(PublisherConfiguration),
                parameterName: nameof(ConfirmationTimeout),
                parameterValue: ConfirmationTimeout);
        }

        if (DefaultTimeToLive.HasValue && DefaultTimeToLive.Value <= TimeSpan.Zero)
        {
            throw new BrokerConfigurationException(
                "Default time-to-live must be positive when specified.",
                configurationSection: nameof(PublisherConfiguration),
                parameterName: nameof(DefaultTimeToLive),
                parameterValue: DefaultTimeToLive);
        }

        if (string.IsNullOrWhiteSpace(DefaultContentType))
        {
            throw new BrokerConfigurationException(
                "Default content type cannot be null or empty.",
                configurationSection: nameof(PublisherConfiguration),
                parameterName: nameof(DefaultContentType),
                parameterValue: DefaultContentType);
        }

        if (string.IsNullOrWhiteSpace(DefaultContentEncoding))
        {
            throw new BrokerConfigurationException(
                "Default content encoding cannot be null or empty.",
                configurationSection: nameof(PublisherConfiguration),
                parameterName: nameof(DefaultContentEncoding),
                parameterValue: DefaultContentEncoding);
        }
    }

    /// <summary>
    /// Returns a string representation of the publisher configuration.
    /// </summary>
    /// <returns>A formatted string describing the publisher settings.</returns>
    public override string ToString()
    {
        var confirms = EnablePublisherConfirms ? "confirms" : "no-confirms";
        var mandatory = MandatoryPublishing ? "mandatory" : "optional";
        var persistence = DefaultPersistence ? "persistent" : "transient";
        return $"Publisher[{confirms}, {mandatory}, {persistence}]";
    }
}