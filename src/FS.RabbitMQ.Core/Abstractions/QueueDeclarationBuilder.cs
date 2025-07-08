namespace FS.RabbitMQ.Core.Abstractions;

/// <summary>
/// Provides a fluent API for building QueueDeclaration instances with method chaining.
/// </summary>
/// <remarks>
/// The QueueDeclarationBuilder enables comprehensive configuration of queue characteristics
/// including durability, exclusivity, message lifecycle management, and advanced features
/// like priority queues, dead letter exchanges, and resource limits.
/// 
/// Queue configuration significantly impacts system behavior:
/// - Performance characteristics (memory vs disk usage, throughput)
/// - Reliability guarantees (message persistence, dead letter handling)
/// - Resource utilization (memory limits, disk I/O patterns)
/// - Consumer behavior (exclusive access, auto-delete lifecycle)
/// 
/// Example usage:
/// <code>
/// var workQueue = QueueDeclaration
///     .Create("order.processing")
///     .AsDurable()
///     .WithDeadLetterExchange("order.failed")
///     .WithMessageTtl(TimeSpan.FromHours(24))
///     .WithMaxLength(10000)
///     .Build();
/// </code>
/// </remarks>
public sealed class QueueDeclarationBuilder
{
    private readonly string? _name;
    private bool _durable = true;
    private bool _exclusive = false;
    private bool _autoDelete = false;
    private Dictionary<string, object>? _arguments;

    /// <summary>
    /// Initializes a new instance of the QueueDeclarationBuilder class.
    /// </summary>
    /// <param name="name">The name of the queue to declare, or null for server-generated names.</param>
    /// <remarks>
    /// This constructor is internal to ensure that instances are created only through
    /// the QueueDeclaration factory methods, which provide appropriate defaults and validation.
    /// </remarks>
    internal QueueDeclarationBuilder(string? name)
    {
        _name = name;
    }

    /// <summary>
    /// Configures the queue to be durable, surviving broker restarts.
    /// </summary>
    /// <returns>The current builder instance for method chaining.</returns>
    /// <remarks>
    /// Durable queues provide persistence guarantees essential for reliable messaging:
    /// - Queue definition survives broker restarts and is recreated automatically
    /// - Messages marked as persistent will also survive restarts when queue is durable
    /// - Slight performance overhead during queue declaration due to disk persistence
    /// - Required for any production scenario where message loss is unacceptable
    /// 
    /// Durable queues work in conjunction with persistent messages to provide end-to-end durability.
    /// Even with durable queues, non-persistent messages will be lost during broker restarts.
    /// </remarks>
    public QueueDeclarationBuilder AsDurable()
    {
        _durable = true;
        return this;
    }

    /// <summary>
    /// Configures the queue to be transient, not surviving broker restarts.
    /// </summary>
    /// <returns>The current builder instance for method chaining.</returns>
    /// <remarks>
    /// Transient queues offer performance benefits at the cost of persistence:
    /// - Faster queue declaration and operations due to memory-only storage
    /// - All messages lost during broker restarts regardless of message persistence
    /// - Suitable for temporary communication channels and development environments
    /// - Often combined with exclusive and auto-delete for fully temporary queues
    /// 
    /// Use transient queues when performance is critical and message loss is acceptable,
    /// such as real-time notifications or temporary processing pipelines.
    /// </remarks>
    public QueueDeclarationBuilder AsTransient()
    {
        _durable = false;
        return this;
    }

    /// <summary>
    /// Configures the queue for exclusive access by the declaring connection.
    /// </summary>
    /// <returns>The current builder instance for method chaining.</returns>
    /// <remarks>
    /// Exclusive queues provide connection-scoped isolation and automatic cleanup:
    /// - Only the connection that declared the queue can access it for publishing or consuming
    /// - Automatically deleted when the declaring connection closes (connection death)
    /// - Cannot be redeclared by other connections, even with identical parameters
    /// - Useful for private communication channels and request-response patterns
    /// 
    /// Exclusive queues are particularly valuable for:
    /// - Reply queues in RPC-style communication
    /// - Client-specific event subscriptions
    /// - Temporary processing pipelines tied to specific connections
    /// 
    /// Note: Exclusive implies a form of auto-delete tied to connection lifecycle,
    /// separate from the auto-delete setting which is based on consumer count.
    /// </remarks>
    public QueueDeclarationBuilder AsExclusive()
    {
        _exclusive = true;
        return this;
    }

    /// <summary>
    /// Configures the queue for shared access across multiple connections.
    /// </summary>
    /// <returns>The current builder instance for method chaining.</returns>
    /// <remarks>
    /// Shared queues enable multi-consumer patterns and connection independence:
    /// - Multiple connections can declare, publish to, and consume from the queue
    /// - Queue persists independently of any specific connection lifecycle
    /// - Essential for load-balanced processing with multiple worker instances
    /// - Required for persistent work queues in production environments
    /// 
    /// Shared access is the default and recommended setting for most production scenarios
    /// where queues need to outlive individual application instances and support
    /// horizontal scaling through multiple consumers.
    /// </remarks>
    public QueueDeclarationBuilder AsShared()
    {
        _exclusive = false;
        return this;
    }

    /// <summary>
    /// Configures the queue to be automatically deleted when no consumers are attached.
    /// </summary>
    /// <returns>The current builder instance for method chaining.</returns>
    /// <remarks>
    /// Auto-delete provides automatic resource cleanup based on consumer activity:
    /// - Queue is deleted when the last consumer cancels or disconnects
    /// - Useful for dynamic processing scenarios and temporary subscriptions
    /// - Messages in the queue are lost when auto-deletion occurs
    /// - Can help prevent resource leaks in systems with dynamic queue creation
    /// 
    /// Exercise caution with auto-delete in production environments:
    /// - Network interruptions may temporarily disconnect all consumers, triggering deletion
    /// - Consider implementing reconnection logic before relying on auto-delete
    /// - Combine with monitoring to detect unexpected queue deletions
    /// 
    /// Auto-delete is particularly useful for temporary subscriptions where cleanup
    /// should happen automatically when interest in the queue disappears.
    /// </remarks>
    public QueueDeclarationBuilder WithAutoDelete()
    {
        _autoDelete = true;
        return this;
    }

    /// <summary>
    /// Configures the queue to persist even when no consumers are attached.
    /// </summary>
    /// <returns>The current builder instance for method chaining.</returns>
    /// <remarks>
    /// Persistent queues provide stability and reliability for asynchronous processing:
    /// - Queue remains available even when temporarily unused
    /// - Messages accumulate safely when consumers are offline
    /// - Essential for reliable batch processing and offline message delivery
    /// - Prevents accidental data loss due to temporary connectivity issues
    /// 
    /// This is the recommended setting for production work queues where message
    /// processing might be temporarily interrupted but should resume automatically
    /// when consumers reconnect.
    /// </remarks>
    public QueueDeclarationBuilder WithoutAutoDelete()
    {
        _autoDelete = false;
        return this;
    }

    /// <summary>
    /// Configures a time-to-live for messages in the queue.
    /// </summary>
    /// <param name="ttl">The time-to-live duration for messages in the queue.</param>
    /// <returns>The current builder instance for method chaining.</returns>
    /// <exception cref="ArgumentOutOfRangeException">
    /// Thrown when <paramref name="ttl"/> is negative or exceeds the maximum allowed value.
    /// </exception>
    /// <remarks>
    /// Message TTL provides automatic cleanup of aged messages:
    /// - Messages older than the TTL are automatically removed from the queue
    /// - TTL countdown begins when the message is first enqueued
    /// - Expired messages can be redirected to dead letter exchanges if configured
    /// - Helps prevent unbounded queue growth in scenarios with slow or failing consumers
    /// 
    /// TTL is particularly useful for:
    /// - Time-sensitive notifications that become irrelevant over time
    /// - Preventing memory exhaustion in queues with inconsistent processing rates
    /// - Implementing message expiration policies for compliance requirements
    /// - Creating time-based cleanup strategies for temporary data
    /// 
    /// Consider the processing time requirements of your consumers when setting TTL values.
    /// Messages that expire too quickly may be lost before legitimate processing can occur.
    /// </remarks>
    public QueueDeclarationBuilder WithMessageTtl(TimeSpan ttl)
    {
        if (ttl < TimeSpan.Zero)
        {
            throw new ArgumentOutOfRangeException(nameof(ttl), "Message TTL cannot be negative.");
        }

        if (ttl.TotalMilliseconds > int.MaxValue)
        {
            throw new ArgumentOutOfRangeException(nameof(ttl), "Message TTL exceeds maximum allowed value.");
        }

        return WithArgument("x-message-ttl", (int)ttl.TotalMilliseconds);
    }

    /// <summary>
    /// Configures the queue to expire and be deleted after a period of inactivity.
    /// </summary>
    /// <param name="expiry">The duration of inactivity after which the queue should be deleted.</param>
    /// <returns>The current builder instance for method chaining.</returns>
    /// <exception cref="ArgumentOutOfRangeException">
    /// Thrown when <paramref name="expiry"/> is negative or exceeds the maximum allowed value.
    /// </exception>
    /// <remarks>
    /// Queue expiry provides automatic cleanup of unused queues:
    /// - Countdown begins when the queue becomes unused (no consumers, publishers, or queue operations)
    /// - Queue and all its messages are deleted when expiry time is reached
    /// - Useful for temporary queues in dynamic systems
    /// - Helps prevent resource leaks from abandoned queues
    /// 
    /// Queue expiry is valuable for:
    /// - Temporary processing queues that should clean up automatically
    /// - Development and testing scenarios with frequent queue creation
    /// - Multi-tenant systems where queues are created dynamically per user/session
    /// - Implementing automatic resource management policies
    /// 
    /// Be careful with queue expiry in production systems where temporary inactivity
    /// might be normal operational behavior rather than abandonment.
    /// </remarks>
    public QueueDeclarationBuilder WithExpiry(TimeSpan expiry)
    {
        if (expiry < TimeSpan.Zero)
        {
            throw new ArgumentOutOfRangeException(nameof(expiry), "Queue expiry cannot be negative.");
        }

        if (expiry.TotalMilliseconds > int.MaxValue)
        {
            throw new ArgumentOutOfRangeException(nameof(expiry), "Queue expiry exceeds maximum allowed value.");
        }

        return WithArgument("x-expires", (int)expiry.TotalMilliseconds);
    }

    /// <summary>
    /// Configures a maximum number of messages that can be stored in the queue.
    /// </summary>
    /// <param name="maxLength">The maximum number of messages allowed in the queue.</param>
    /// <returns>The current builder instance for method chaining.</returns>
    /// <exception cref="ArgumentOutOfRangeException">
    /// Thrown when <paramref name="maxLength"/> is negative.
    /// </exception>
    /// <remarks>
    /// Queue length limits provide resource protection and flow control:
    /// - When limit is reached, behavior depends on overflow policy (default: drop oldest messages)
    /// - Helps prevent memory exhaustion from unbounded queue growth
    /// - Useful for implementing backpressure in high-throughput systems
    /// - Can be combined with dead letter exchanges to handle overflow messages
    /// 
    /// Maximum length is essential for:
    /// - Protecting broker resources in scenarios with unbalanced producer/consumer rates
    /// - Implementing bounded queues for real-time systems with strict memory constraints
    /// - Creating sliding window behaviors for time-series data processing
    /// - Ensuring predictable memory usage patterns in production environments
    /// 
    /// Consider combining with appropriate overflow policies and dead letter exchanges
    /// to handle scenarios where the limit is reached during normal operation.
    /// </remarks>
    public QueueDeclarationBuilder WithMaxLength(int maxLength)
    {
        if (maxLength < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(maxLength), "Maximum queue length cannot be negative.");
        }

        return WithArgument("x-max-length", maxLength);
    }

    /// <summary>
    /// Configures a maximum total size for all messages in the queue.
    /// </summary>
    /// <param name="maxSizeBytes">The maximum total size of messages in bytes.</param>
    /// <returns>The current builder instance for method chaining.</returns>
    /// <exception cref="ArgumentOutOfRangeException">
    /// Thrown when <paramref name="maxSizeBytes"/> is negative.
    /// </exception>
    /// <remarks>
    /// Byte-based queue limits provide precise memory control:
    /// - Accounts for actual message payload sizes rather than just count
    /// - More accurate resource management for queues with variable message sizes
    /// - Prevents memory exhaustion from large messages even with low message counts
    /// - Works in conjunction with max-length limits (whichever is reached first applies)
    /// 
    /// Maximum size limits are particularly important for:
    /// - Queues handling variable-sized payloads (file uploads, document processing)
    /// - Systems with strict memory constraints and predictable resource requirements
    /// - Multi-tenant environments where fair resource allocation is critical
    /// - Scenarios where message size distribution is highly variable
    /// 
    /// Monitor both message count and size metrics to understand queue behavior
    /// and tune limits appropriately for your specific use cases.
    /// </remarks>
    public QueueDeclarationBuilder WithMaxSizeBytes(int maxSizeBytes)
    {
        if (maxSizeBytes < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(maxSizeBytes), "Maximum queue size in bytes cannot be negative.");
        }

        return WithArgument("x-max-length-bytes", maxSizeBytes);
    }

    /// <summary>
    /// Configures the queue to support message priorities.
    /// </summary>
    /// <param name="maxPriority">The maximum priority value supported (0 to 255, where 255 is highest priority).</param>
    /// <returns>The current builder instance for method chaining.</returns>
    /// <exception cref="ArgumentOutOfRangeException">
    /// Thrown when <paramref name="maxPriority"/> is outside the valid range of 0 to 255.
    /// </exception>
    /// <remarks>
    /// Priority queues enable message ordering based on importance:
    /// - Messages with higher priority values are delivered before lower priority messages
    /// - Priority is set on individual messages during publishing
    /// - Non-priority messages are treated as priority 0
    /// - Performance overhead increases with the range of priorities used
    /// 
    /// Priority queues are valuable for:
    /// - Processing urgent messages before routine ones
    /// - Implementing service level agreements (SLAs) with different response times
    /// - Managing system resources during high load periods
    /// - Handling both interactive and batch processing workloads
    /// 
    /// Design considerations for priority queues:
    /// - Higher max priority values increase memory overhead
    /// - Consider using a smaller range (e.g., 0-10) if extreme granularity isn't needed
    /// - Monitor queue behavior to ensure lower priority messages aren't starved
    /// - Combine with message TTL to prevent low-priority message accumulation
    /// </remarks>
    public QueueDeclarationBuilder WithPriority(byte maxPriority)
    {
        return WithArgument("x-max-priority", (int)maxPriority);
    }

    /// <summary>
    /// Configures the queue to use lazy mode for optimized disk storage.
    /// </summary>
    /// <returns>The current builder instance for method chaining.</returns>
    /// <remarks>
    /// Lazy mode optimizes memory usage by storing messages on disk:
    /// - Messages are written to disk as soon as possible instead of being kept in memory
    /// - Significantly reduces memory usage for queues with many messages
    /// - Slight performance trade-off due to disk I/O for message access
    /// - Essential for handling very large queues or systems with limited memory
    /// 
    /// Lazy queues are recommended for:
    /// - Long-running queues that accumulate many messages
    /// - Systems with memory constraints or competing memory usage
    /// - Batch processing scenarios where messages may wait for extended periods
    /// - Archival or audit queues that store messages for compliance purposes
    /// 
    /// Performance considerations:
    /// - Disk I/O characteristics become more important than memory speed
    /// - Consider using SSDs for better lazy queue performance
    /// - Monitor disk space usage as lazy queues shift resource pressure to storage
    /// - May impact message throughput in high-frequency scenarios
    /// </remarks>
    public QueueDeclarationBuilder AsLazy()
    {
        return WithArgument("x-queue-mode", "lazy");
    }

    /// <summary>
    /// Configures the queue to use normal mode for optimized memory performance.
    /// </summary>
    /// <returns>The current builder instance for method chaining.</returns>
    /// <remarks>
    /// Normal mode prioritizes memory-based message storage for performance:
    /// - Messages are kept in memory for faster access
    /// - Higher memory usage, especially for queues with many messages
    /// - Better performance for high-throughput, low-latency scenarios
    /// - Default mode for most RabbitMQ installations
    /// 
    /// Normal mode is preferred for:
    /// - High-frequency message processing with low latency requirements
    /// - Systems with abundant memory resources
    /// - Queues that typically have low message accumulation
    /// - Real-time processing scenarios where response time is critical
    /// </remarks>
    public QueueDeclarationBuilder AsNormal()
    {
        return WithArgument("x-queue-mode", "default");
    }

    /// <summary>
    /// Configures the behavior when the queue reaches its maximum length or size.
    /// </summary>
    /// <param name="overflowBehavior">The overflow behavior to apply when limits are reached.</param>
    /// <returns>The current builder instance for method chaining.</returns>
    /// <remarks>
    /// Overflow policies determine how the queue handles capacity limits:
    /// 
    /// - DropHead: Remove oldest messages to make room for new ones (FIFO eviction)
    /// - RejectPublish: Reject new messages when queue is full (backpressure to publishers)
    /// - RejectPublishDlx: Reject and send overflow messages to dead letter exchange
    /// 
    /// Each policy has different implications for message delivery guarantees,
    /// system behavior under load, and resource management strategies.
    /// </remarks>
    public QueueDeclarationBuilder WithOverflow(QueueOverflowBehavior overflowBehavior)
    {
        var overflowValue = overflowBehavior switch
        {
            QueueOverflowBehavior.DropHead => "drop-head",
            QueueOverflowBehavior.RejectPublish => "reject-publish",
            QueueOverflowBehavior.RejectPublishDlx => "reject-publish-dlx",
            _ => throw new ArgumentOutOfRangeException(nameof(overflowBehavior), "Invalid overflow behavior specified.")
        };

        return WithArgument("x-overflow", overflowValue);
    }

    /// <summary>
    /// Configures a dead letter exchange for messages that are rejected, expired, or exceed retry limits.
    /// </summary>
    /// <param name="exchangeName">The name of the dead letter exchange. Cannot be null or whitespace.</param>
    /// <param name="routingKey">
    /// Optional routing key for dead letter messages. If null, the original message routing key is used.
    /// </param>
    /// <returns>The current builder instance for method chaining.</returns>
    /// <exception cref="ArgumentException">
    /// Thrown when <paramref name="exchangeName"/> is null, empty, or contains only whitespace.
    /// </exception>
    /// <remarks>
    /// Dead letter exchanges provide comprehensive error handling for message processing failures:
    /// - Messages are republished to the dead letter exchange instead of being discarded
    /// - Triggers include: message rejection with requeue=false, message TTL expiration, queue length limits
    /// - Dead letter messages include headers with failure reason and original queue information
    /// - Essential for implementing reliable message processing with failure recovery
    /// 
    /// Dead letter exchanges enable several important patterns:
    /// - Failed message analysis and debugging
    /// - Retry logic with exponential backoff (using TTL and republishing)
    /// - Message archival for compliance and audit requirements
    /// - Separate processing pipelines for failed messages
    /// 
    /// Design considerations:
    /// - Ensure the dead letter exchange exists before declaring this queue
    /// - Consider the routing topology for dead letter messages
    /// - Monitor dead letter queues to identify systematic processing issues
    /// - Implement alerting for dead letter message accumulation
    /// </remarks>
    public QueueDeclarationBuilder WithDeadLetterExchange(string exchangeName, string? routingKey = null)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(exchangeName);

        WithArgument("x-dead-letter-exchange", exchangeName);

        if (!string.IsNullOrEmpty(routingKey))
        {
            WithArgument("x-dead-letter-routing-key", routingKey);
        }

        return this;
    }

    /// <summary>
    /// Adds a custom argument to the queue declaration.
    /// </summary>
    /// <param name="key">The argument key. Cannot be null or whitespace.</param>
    /// <param name="value">The argument value. Cannot be null.</param>
    /// <returns>The current builder instance for method chaining.</returns>
    /// <exception cref="ArgumentException">
    /// Thrown when <paramref name="key"/> is null, empty, or whitespace.
    /// </exception>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="value"/> is null.</exception>
    /// <remarks>
    /// Custom arguments enable advanced queue features and broker-specific functionality.
    /// Common use cases include plugin-specific configuration, custom queue behaviors,
    /// and integration with external systems or monitoring tools.
    /// 
    /// Always verify argument support in your target RabbitMQ environment,
    /// as custom arguments may require specific plugins or broker versions.
    /// </remarks>
    public QueueDeclarationBuilder WithArgument(string key, object value)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(key);
        ArgumentNullException.ThrowIfNull(value);

        _arguments ??= new Dictionary<string, object>();
        _arguments[key] = value;
        return this;
    }

    /// <summary>
    /// Adds multiple custom arguments to the queue declaration.
    /// </summary>
    /// <param name="arguments">A dictionary containing argument key-value pairs. Cannot be null.</param>
    /// <returns>The current builder instance for method chaining.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="arguments"/> is null.</exception>
    /// <exception cref="ArgumentException">
    /// Thrown when any argument key is null, empty, or whitespace, or when any value is null.
    /// </exception>
    /// <remarks>
    /// Bulk argument addition is useful for applying configuration templates,
    /// loading settings from external configuration sources, or implementing
    /// standardized queue configurations across multiple queue declarations.
    /// </remarks>
    public QueueDeclarationBuilder WithArguments(IReadOnlyDictionary<string, object> arguments)
    {
        ArgumentNullException.ThrowIfNull(arguments);

        // Validate all arguments before applying any changes
        foreach (var kvp in arguments)
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(kvp.Key);
            ArgumentNullException.ThrowIfNull(kvp.Value);
        }

        _arguments ??= new Dictionary<string, object>();

        foreach (var kvp in arguments)
        {
            _arguments[kvp.Key] = kvp.Value;
        }

        return this;
    }

    /// <summary>
    /// Builds and returns the configured QueueDeclaration instance.
    /// </summary>
    /// <returns>A new QueueDeclaration instance with the specified configuration.</returns>
    /// <exception cref="InvalidOperationException">
    /// Thrown when the current configuration is invalid or incomplete.
    /// </exception>
    /// <remarks>
    /// The build process performs comprehensive validation of the queue configuration,
    /// including argument consistency, logical setting combinations, and format validation.
    /// 
    /// The resulting QueueDeclaration is immutable and thread-safe, making it suitable
    /// for use across multiple threads and operations without additional synchronization.
    /// </remarks>
    public QueueDeclaration Build()
    {
        var declaration = new QueueDeclaration
        {
            Name = _name,
            Durable = _durable,
            Exclusive = _exclusive,
            AutoDelete = _autoDelete,
            Arguments = _arguments?.AsReadOnly()
        };

        // Perform validation before returning the instance
        declaration.Validate();

        return declaration;
    }
}
