using FS.RabbitMQ.Core.Abstractions;

namespace FS.RabbitMQ.Core.Configuration;

/// <summary>
/// Contains configuration for message consumption operations and consumer behavior.
/// </summary>
/// <remarks>
/// Consumer configuration defines the default behavior for all consumer types,
/// including performance characteristics, error handling policies, resource management,
/// and quality of service settings.
/// </remarks>
public sealed class ConsumerConfiguration
{
    /// <summary>
    /// Gets or sets the default maximum concurrency for message processing.
    /// </summary>
    /// <value>The maximum number of concurrent message processing operations. Default is 10.</value>
    /// <remarks>
    /// Concurrency controls how many messages can be processed simultaneously,
    /// affecting throughput and resource utilization. Higher values increase
    /// throughput but require more resources.
    /// </remarks>
    public int DefaultMaxConcurrency { get; set; } = 10;

    /// <summary>
    /// Gets or sets the default prefetch count for message consumers.
    /// </summary>
    /// <value>The number of unacknowledged messages a consumer can handle. Default is 10.</value>
    /// <remarks>
    /// Prefetch count controls how many messages the broker delivers to a consumer
    /// before waiting for acknowledgments. This affects load balancing and memory usage.
    /// </remarks>
    public ushort DefaultPrefetchCount { get; set; } = 10;

    /// <summary>
    /// Gets or sets a value indicating whether automatic acknowledgment is enabled by default.
    /// </summary>
    /// <value><c>true</c> to automatically acknowledge messages; otherwise, <c>false</c>. Default is <c>false</c>.</value>
    /// <remarks>
    /// Automatic acknowledgment trades reliability for performance by acknowledging
    /// messages immediately upon delivery rather than after processing.
    /// </remarks>
    public bool DefaultAutoAck { get; set; } = false;

    /// <summary>
    /// Gets or sets the default message processing timeout.
    /// </summary>
    /// <value>The maximum time allowed for processing a single message. Default is 5 minutes.</value>
    /// <remarks>
    /// Processing timeout prevents hung message handlers from blocking consumers
    /// indefinitely and enables detection of slow or problematic processing logic.
    /// </remarks>
    public TimeSpan DefaultProcessingTimeout { get; set; } = TimeSpan.FromMinutes(5);

    /// <summary>
    /// Gets or sets the default error handling strategy.
    /// </summary>
    /// <value>The strategy for handling message processing errors. Default is RetryThenDeadLetter.</value>
    /// <remarks>
    /// Error handling strategy determines what happens when message processing fails:
    /// - Ignore: Acknowledge failed messages and continue
    /// - Retry: Retry failed messages with backoff
    /// - DeadLetter: Route failed messages to dead letter exchange
    /// - RetryThenDeadLetter: Retry with limits, then dead letter
    /// </remarks>
    public ErrorHandlingStrategyEnum DefaultErrorHandling { get; set; } = ErrorHandlingStrategyEnum.RetryThenDeadLetter;

    /// <summary>
    /// Gets or sets the default retry policy for message processing failures.
    /// </summary>
    /// <value>The retry policy for handling processing failures.</value>
    /// <remarks>
    /// Consumer retry policy defines how the system retries failed message processing,
    /// providing resilience against transient processing errors.
    /// </remarks>
    public RetryPolicy? DefaultRetryPolicy { get; set; }

    /// <summary>
    /// Gets or sets the default consumer priority.
    /// </summary>
    /// <value>The priority for consumers when competing for messages, or null for no priority. Default is null.</value>
    /// <remarks>
    /// Consumer priority enables preferential message delivery to high-priority consumers
    /// when multiple consumers compete for messages from the same queue.
    /// </remarks>
    public int? DefaultConsumerPriority { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether exclusive consumers are used by default.
    /// </summary>
    /// <value><c>true</c> to create exclusive consumers; otherwise, <c>false</c>. Default is <c>false</c>.</value>
    /// <remarks>
    /// Exclusive consumers ensure that only one consumer can access a queue at a time,
    /// useful for scenarios requiring strict ordering or single-threaded processing.
    /// </remarks>
    public bool DefaultExclusive { get; set; } = false;

    /// <summary>
    /// Gets or sets the consumer health check configuration.
    /// </summary>
    /// <value>The health check settings for monitoring consumer health.</value>
    /// <remarks>
    /// Health check configuration enables monitoring of consumer status and
    /// automatic detection of problematic or unhealthy consumers.
    /// </remarks>
    public ConsumerHealthCheckConfiguration? HealthCheck { get; set; }

    /// <summary>
    /// Gets or sets the default consumer cancellation timeout.
    /// </summary>
    /// <value>The maximum time to wait for consumer shutdown. Default is 30 seconds.</value>
    /// <remarks>
    /// Cancellation timeout ensures that consumer shutdown operations complete
    /// within a reasonable time, preventing hung shutdown processes.
    /// </remarks>
    public TimeSpan DefaultCancellationTimeout { get; set; } = TimeSpan.FromSeconds(30);

    /// <summary>
    /// Creates a consumer configuration suitable for development environments.
    /// </summary>
    /// <returns>A consumer configuration with development-friendly defaults.</returns>
    public static ConsumerConfiguration CreateDevelopment()
    {
        return new ConsumerConfiguration
        {
            DefaultMaxConcurrency = 5,
            DefaultPrefetchCount = 5,
            DefaultAutoAck = false,
            DefaultProcessingTimeout = TimeSpan.FromMinutes(10), // Longer for debugging
            DefaultErrorHandling = ErrorHandlingStrategyEnum.Retry,
            DefaultRetryPolicy = RetryPolicy.CreateImmediate(maxAttempts: 3),
            DefaultCancellationTimeout = TimeSpan.FromSeconds(10)
        };
    }

    /// <summary>
    /// Creates a consumer configuration suitable for production environments.
    /// </summary>
    /// <returns>A consumer configuration with production-ready defaults.</returns>
    public static ConsumerConfiguration CreateProduction()
    {
        return new ConsumerConfiguration
        {
            DefaultMaxConcurrency = 10,
            DefaultPrefetchCount = 10,
            DefaultAutoAck = false,
            DefaultProcessingTimeout = TimeSpan.FromMinutes(5),
            DefaultErrorHandling = ErrorHandlingStrategyEnum.RetryThenDeadLetter,
            DefaultRetryPolicy = RetryPolicy.CreateExponentialBackoff(
                baseDelay: TimeSpan.FromSeconds(2),
                maxDelay: TimeSpan.FromMinutes(10),
                maxAttempts: 5),
            HealthCheck = ConsumerHealthCheckConfiguration.CreateDefault(),
            DefaultCancellationTimeout = TimeSpan.FromSeconds(30)
        };
    }

    /// <summary>
    /// Creates a consumer configuration optimized for high-throughput scenarios.
    /// </summary>
    /// <returns>A consumer configuration optimized for performance.</returns>
    public static ConsumerConfiguration CreateHighThroughput()
    {
        return new ConsumerConfiguration
        {
            DefaultMaxConcurrency = 50,
            DefaultPrefetchCount = 100,
            DefaultAutoAck = false, // Still maintain reliability
            DefaultProcessingTimeout = TimeSpan.FromMinutes(1),
            DefaultErrorHandling = ErrorHandlingStrategyEnum.DeadLetter, // Fast failure
            DefaultRetryPolicy = RetryPolicy.CreateLinear(
                delay: TimeSpan.FromMilliseconds(100),
                maxAttempts: 2),
            DefaultCancellationTimeout = TimeSpan.FromSeconds(10)
        };
    }

    /// <summary>
    /// Validates the consumer configuration for consistency and reasonableness.
    /// </summary>
    /// <exception cref="BrokerConfigurationException">Thrown when the configuration is invalid.</exception>
    public void Validate()
    {
        if (DefaultMaxConcurrency <= 0)
        {
            throw new BrokerConfigurationException(
                "Default maximum concurrency must be positive.",
                configurationSection: nameof(ConsumerConfiguration),
                parameterName: nameof(DefaultMaxConcurrency),
                parameterValue: DefaultMaxConcurrency);
        }

        if (DefaultPrefetchCount == 0)
        {
            throw new BrokerConfigurationException(
                "Default prefetch count must be positive.",
                configurationSection: nameof(ConsumerConfiguration),
                parameterName: nameof(DefaultPrefetchCount),
                parameterValue: DefaultPrefetchCount);
        }

        if (DefaultProcessingTimeout <= TimeSpan.Zero)
        {
            throw new BrokerConfigurationException(
                "Default processing timeout must be positive.",
                configurationSection: nameof(ConsumerConfiguration),
                parameterName: nameof(DefaultProcessingTimeout),
                parameterValue: DefaultProcessingTimeout);
        }

        if (DefaultCancellationTimeout <= TimeSpan.Zero)
        {
            throw new BrokerConfigurationException(
                "Default cancellation timeout must be positive.",
                configurationSection: nameof(ConsumerConfiguration),
                parameterName: nameof(DefaultCancellationTimeout),
                parameterValue: DefaultCancellationTimeout);
        }

        if (DefaultConsumerPriority is < 0)
        {
            throw new BrokerConfigurationException(
                "Default consumer priority cannot be negative.",
                configurationSection: nameof(ConsumerConfiguration),
                parameterName: nameof(DefaultConsumerPriority),
                parameterValue: DefaultConsumerPriority);
        }

        HealthCheck?.Validate();
    }

    /// <summary>
    /// Returns a string representation of the consumer configuration.
    /// </summary>
    /// <returns>A formatted string describing the consumer settings.</returns>
    public override string ToString()
    {
        var autoAck = DefaultAutoAck ? "auto-ack" : "manual-ack";
        var exclusive = DefaultExclusive ? "exclusive" : "shared";
        return $"Consumer[concurrency: {DefaultMaxConcurrency}, prefetch: {DefaultPrefetchCount}, {autoAck}, {exclusive}]";
    }
}