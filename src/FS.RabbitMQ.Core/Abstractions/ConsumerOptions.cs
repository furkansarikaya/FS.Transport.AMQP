namespace FS.RabbitMQ.Core.Abstractions;

/// <summary>
/// Contains configuration options for message consumer creation and behavior.
/// </summary>
/// <remarks>
/// Consumer options provide comprehensive control over consumer behavior,
/// performance characteristics, and quality of service parameters. These options
/// enable fine-tuning of consumer operation for different use cases ranging from
/// high-throughput batch processing to low-latency real-time scenarios.
/// 
/// Options are designed to be composable and provide sensible defaults for
/// common scenarios while allowing detailed customization for specialized requirements.
/// </remarks>
public sealed class ConsumerOptions
{
    /// <summary>
    /// Gets or sets the prefetch count for message retrieval optimization.
    /// </summary>
    /// <value>
    /// The number of messages the consumer will fetch in advance for processing.
    /// Default is 10 for balanced throughput and memory usage.
    /// </value>
    /// <remarks>
    /// Prefetch count affects:
    /// - Memory usage: Higher values consume more memory but improve throughput
    /// - Latency: Lower values reduce latency but may decrease overall throughput
    /// - Load balancing: Lower values provide better distribution across consumers
    /// - Backpressure: Higher values may mask downstream processing issues
    /// 
    /// Optimal values depend on message size, processing time, and system resources.
    /// Consider starting with default values and adjusting based on performance monitoring.
    /// </remarks>
    public ushort PrefetchCount { get; set; } = 10;

    /// <summary>
    /// Gets or sets a value indicating whether messages should be automatically acknowledged after successful processing.
    /// </summary>
    /// <value>
    /// <c>true</c> if messages should be acknowledged automatically upon successful handler completion;
    /// <c>false</c> if manual acknowledgment is required. Default is <c>true</c>.
    /// </value>
    /// <remarks>
    /// Automatic acknowledgment provides:
    /// - Simplified programming model with reduced boilerplate code
    /// - Automatic error handling and message lifecycle management
    /// - Reduced risk of programming errors in acknowledgment handling
    /// - Built-in retry and dead letter handling for failed processing
    /// 
    /// Manual acknowledgment enables:
    /// - Fine-grained control over message lifecycle and error handling
    /// - Implementation of complex processing patterns with conditional acknowledgment
    /// - Custom retry strategies and error recovery mechanisms
    /// - Integration with external transaction systems
    /// </remarks>
    public bool AutoAcknowledge { get; set; } = true;

    /// <summary>
    /// Gets or sets a value indicating whether the consumer should be exclusive to its queue.
    /// </summary>
    /// <value>
    /// <c>true</c> if this consumer should have exclusive access to the queue;
    /// <c>false</c> if multiple consumers can process messages from the same queue. Default is <c>false</c>.
    /// </value>
    /// <remarks>
    /// Exclusive consumers provide:
    /// - Guaranteed message ordering within a single consumer
    /// - Elimination of load balancing complexity
    /// - Simplified debugging and monitoring with single consumer instance
    /// - Predictable processing patterns for order-sensitive scenarios
    /// 
    /// Non-exclusive consumers enable:
    /// - Horizontal scaling with multiple consumer instances
    /// - Load balancing and fault tolerance across multiple workers
    /// - Higher throughput through parallel processing
    /// - Better resource utilization in distributed systems
    /// </remarks>
    public bool Exclusive { get; set; } = false;

    /// <summary>
    /// Gets or sets the consumer priority for message delivery when multiple consumers compete for messages.
    /// </summary>
    /// <value>
    /// The priority value where higher numbers indicate higher priority.
    /// Default is 0 (normal priority).
    /// </value>
    /// <remarks>
    /// Consumer priority affects:
    /// - Message delivery order when multiple consumers are available
    /// - Resource allocation during high load scenarios
    /// - Implementation of tiered processing architectures
    /// - Quality of service differentiation across consumer types
    /// 
    /// Priority-based delivery is useful for:
    /// - Implementing fast/slow lane processing architectures
    /// - Ensuring critical consumers receive messages first
    /// - Load shedding strategies during system stress
    /// - SLA-aware message processing with different service levels
    /// </remarks>
    public int Priority { get; set; } = 0;

    /// <summary>
    /// Gets or sets arguments to be passed during consumer registration.
    /// </summary>
    /// <value>
    /// A dictionary containing consumer-specific arguments, or null if no special arguments are needed.
    /// </value>
    /// <remarks>
    /// Consumer arguments enable:
    /// - Broker-specific feature configuration and tuning
    /// - Plugin-specific settings and behavior customization
    /// - Integration with external systems and monitoring tools
    /// - Advanced queue and consumer behavior configuration
    /// 
    /// Common consumer arguments include:
    /// - "x-cancel-on-ha-failover": Cancel consumer during HA failover events
    /// - "x-priority": Consumer priority (alternative to Priority property)
    /// - Plugin-specific configuration for custom behavior
    /// 
    /// Arguments are broker and plugin specific; verify support in target environment.
    /// </remarks>
    public IDictionary<string, object>? ConsumerArguments { get; set; }

    /// <summary>
    /// Gets or sets the retry policy for handling processing failures.
    /// </summary>
    /// <value>
    /// The retry policy configuration, or null to use default retry behavior.
    /// </value>
    /// <remarks>
    /// Retry policies provide:
    /// - Automatic recovery from transient processing failures
    /// - Configurable backoff strategies (exponential, linear, custom)
    /// - Maximum retry limits to prevent infinite retry loops
    /// - Dead letter handling for messages exceeding retry limits
    /// 
    /// Retry policies are essential for:
    /// - Handling network and connectivity issues
    /// - Managing temporary resource constraints
    /// - Implementing resilient processing patterns
    /// - Maintaining system availability during partial failures
    /// </remarks>
    public RetryPolicy? RetryPolicy { get; set; }

    /// <summary>
    /// Gets or sets the timeout for individual message processing operations.
    /// </summary>
    /// <value>
    /// The maximum duration allowed for processing a single message.
    /// Default is 30 seconds.
    /// </value>
    /// <remarks>
    /// Processing timeout provides:
    /// - Protection against hung or slow processing operations
    /// - Predictable resource utilization and capacity planning
    /// - SLA enforcement for message processing requirements
    /// - Detection of performance regressions and bottlenecks
    /// 
    /// Timeout considerations:
    /// - Should accommodate normal processing time plus safety margin
    /// - Must account for downstream system latency and variability
    /// - Balance between responsiveness and processing completion
    /// - Consider network latency and external system dependencies
    /// </remarks>
    public TimeSpan ProcessingTimeout { get; set; } = TimeSpan.FromSeconds(30);

    /// <summary>
    /// Gets or sets the maximum degree of parallelism for concurrent message processing.
    /// </summary>
    /// <value>
    /// The maximum number of messages that can be processed concurrently.
    /// Default is 1 (sequential processing).
    /// </value>
    /// <remarks>
    /// Concurrency level affects:
    /// - Processing throughput and resource utilization
    /// - Memory usage and system load characteristics
    /// - Message ordering guarantees (higher concurrency reduces ordering)
    /// - Error isolation and recovery complexity
    /// 
    /// Concurrency considerations:
    /// - Higher values increase throughput but reduce message ordering
    /// - Should not exceed available CPU cores or downstream capacity
    /// - Monitor memory usage and system performance under load
    /// - Consider thread safety requirements in message processing logic
    /// </remarks>
    public int MaxConcurrency { get; set; } = 1;

    /// <summary>
    /// Gets or sets error handling options for processing failures.
    /// </summary>
    /// <value>
    /// The error handling configuration, or null to use default error handling behavior.
    /// </value>
    /// <remarks>
    /// Error handling options provide:
    /// - Strategies for different types of processing exceptions
    /// - Dead letter exchange configuration for unprocessable messages
    /// - Error logging and monitoring integration
    /// - Conditional recovery and escalation policies
    /// 
    /// Error handling is crucial for:
    /// - Maintaining system stability during partial failures
    /// - Implementing poison message detection and isolation
    /// - Providing visibility into processing issues and trends
    /// - Enabling automated recovery and alerting mechanisms
    /// </remarks>
    public ErrorHandlingOptions? ErrorHandling { get; set; }

    /// <summary>
    /// Gets or sets serialization options for message payload handling.
    /// </summary>
    /// <value>
    /// The serialization configuration, or null to use default serialization settings.
    /// </value>
    /// <remarks>
    /// Serialization options control:
    /// - Message format and encoding strategies
    /// - Compression and optimization settings
    /// - Version compatibility and schema evolution
    /// - Integration with external serialization frameworks
    /// 
    /// Serialization considerations:
    /// - Must be compatible with publisher serialization settings
    /// - Balance between performance and message size
    /// - Consider version compatibility and schema evolution requirements
    /// - Ensure security and validation of deserialized content
    /// </remarks>
    public SerializationOptions? Serialization { get; set; }

    /// <summary>
    /// Gets or sets monitoring and observability options.
    /// </summary>
    /// <value>
    /// The monitoring configuration, or null to use default monitoring behavior.
    /// </value>
    /// <remarks>
    /// Monitoring options enable:
    /// - Performance metrics collection and reporting
    /// - Distributed tracing and correlation
    /// - Health check integration and status reporting
    /// - Custom metrics and observability integration
    /// 
    /// Monitoring is essential for:
    /// - Operational visibility and troubleshooting
    /// - Performance optimization and capacity planning
    /// - SLA monitoring and alerting
    /// - Integration with APM and monitoring platforms
    /// </remarks>
    public MonitoringOptions? Monitoring { get; set; }

    /// <summary>
    /// Creates consumer options optimized for high-throughput batch processing scenarios.
    /// </summary>
    /// <param name="prefetchCount">The number of messages to prefetch for batch processing.</param>
    /// <param name="maxConcurrency">The maximum degree of parallelism for processing.</param>
    /// <returns>ConsumerOptions configured for high-throughput scenarios.</returns>
    /// <remarks>
    /// High-throughput configuration includes:
    /// - Increased prefetch count for better batching efficiency
    /// - Higher concurrency for parallel processing
    /// - Optimized timeout settings for batch operations
    /// - Performance-oriented retry and error handling policies
    /// </remarks>
    public static ConsumerOptions CreateHighThroughput(ushort prefetchCount = 100, int? maxConcurrency = null)
    {
        return new ConsumerOptions
        {
            PrefetchCount = prefetchCount,
            MaxConcurrency = maxConcurrency ?? Environment.ProcessorCount,
            ProcessingTimeout = TimeSpan.FromMinutes(5),
            AutoAcknowledge = true,
            RetryPolicy = RetryPolicy.CreateExponentialBackoff(maxAttempts: 3),
            ErrorHandling = ErrorHandlingOptions.CreateDefault()
        };
    }

    /// <summary>
    /// Creates consumer options optimized for low-latency real-time processing scenarios.
    /// </summary>
    /// <param name="prefetchCount">The number of messages to prefetch (kept low for minimal latency).</param>
    /// <returns>ConsumerOptions configured for low-latency scenarios.</returns>
    /// <remarks>
    /// Low-latency configuration includes:
    /// - Minimal prefetch count for immediate processing
    /// - Sequential processing to maintain message ordering
    /// - Short timeout settings for responsive error handling
    /// - Aggressive retry policies for quick recovery
    /// </remarks>
    public static ConsumerOptions CreateLowLatency(ushort prefetchCount = 1)
    {
        return new ConsumerOptions
        {
            PrefetchCount = prefetchCount,
            MaxConcurrency = 1,
            ProcessingTimeout = TimeSpan.FromSeconds(10),
            AutoAcknowledge = true,
            RetryPolicy = RetryPolicy.CreateImmediate(maxAttempts: 2),
            ErrorHandling = ErrorHandlingOptions.CreateDefault()
        };
    }

    /// <summary>
    /// Creates consumer options for reliable processing with comprehensive error handling.
    /// </summary>
    /// <returns>ConsumerOptions configured for maximum reliability.</returns>
    /// <remarks>
    /// Reliable configuration includes:
    /// - Manual acknowledgment for explicit control
    /// - Conservative timeout settings for thorough processing
    /// - Comprehensive retry policies with exponential backoff
    /// - Detailed error handling and dead letter configuration
    /// </remarks>
    public static ConsumerOptions CreateReliable()
    {
        return new ConsumerOptions
        {
            PrefetchCount = 5,
            MaxConcurrency = 1,
            ProcessingTimeout = TimeSpan.FromMinutes(2),
            AutoAcknowledge = false,
            RetryPolicy = RetryPolicy.CreateExponentialBackoff(maxAttempts: 5),
            ErrorHandling = ErrorHandlingOptions.CreateComprehensive()
        };
    }
}