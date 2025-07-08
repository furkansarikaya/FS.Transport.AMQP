using FS.RabbitMQ.Core.Abstractions;

namespace FS.RabbitMQ.Core.Configuration;

/// <summary>
/// Defines the declaration parameters for RabbitMQ queues.
/// </summary>
/// <remarks>
/// Queue declarations define the message storage and consumption endpoints within RabbitMQ.
/// Queues are the fundamental storage mechanism that holds messages until they are consumed
/// by applications. Think of queues as mailboxes that store messages for specific recipients
/// (consumers) until they are ready to process them.
/// 
/// Understanding RabbitMQ queues:
/// 
/// Queues serve as the storage and delivery mechanism for messages in RabbitMQ. They provide:
/// - Message storage with configurable persistence and durability
/// - Ordered message delivery (FIFO by default)
/// - Flow control and backpressure management
/// - Integration with routing infrastructure through bindings
/// - Consumer coordination and load balancing
/// 
/// Queue characteristics and behavior:
/// 
/// 1. Message Storage:
///    - Queues store messages until consumed or expired
///    - Can be configured for memory-only or disk-persistent storage
///    - Support message TTL (time-to-live) for automatic expiration
///    - Provide overflow handling when capacity limits are reached
/// 
/// 2. Message Ordering:
///    - Standard queues provide FIFO (first-in, first-out) delivery
///    - Priority queues support message priority-based ordering
///    - Message ordering can be affected by acknowledgment patterns
///    - Redelivered messages may affect ordering guarantees
/// 
/// 3. Consumer Management:
///    - Queues coordinate message delivery across multiple consumers
///    - Support exclusive consumer patterns for single-consumer scenarios
///    - Enable competing consumer patterns for load balancing
///    - Provide consumer cancellation and lifecycle management
/// 
/// 4. Resource Management:
///    - Configurable memory and disk usage limits
///    - Automatic message expiration and cleanup
///    - Dead letter routing for unprocessable messages
///    - Flow control to prevent resource exhaustion
/// 
/// Queue design patterns:
/// 
/// Work Queues:
/// - Multiple consumers process messages from a single queue
/// - Natural load balancing across consumers
/// - Good for scalable background job processing
/// 
/// Exclusive Queues:
/// - Single consumer has exclusive access to the queue
/// - Often used for temporary or session-specific messaging
/// - Automatically deleted when consumer disconnects
/// 
/// Priority Queues:
/// - Messages are delivered based on priority rather than arrival order
/// - Higher priority messages are delivered first
/// - Useful for systems with varying message importance
/// 
/// Lazy Queues:
/// - Messages are stored on disk immediately rather than in memory
/// - Lower memory usage but potentially higher latency
/// - Good for very large queues or memory-constrained environments
/// </remarks>
public sealed class QueueDeclaration
{
    /// <summary>
    /// Gets or sets the name of the queue.
    /// </summary>
    /// <value>The queue name used for message storage and consumer binding.</value>
    /// <remarks>
    /// Queue names should be descriptive and follow organizational naming conventions:
    /// - Use clear, meaningful names that indicate purpose and function
    /// - Consider hierarchical naming for related queues
    /// - Follow consistent naming patterns across services and applications
    /// - Avoid special characters that might cause routing or consumption issues
    /// 
    /// Common queue naming patterns:
    /// - Purpose-based: "order-processing", "email-notifications", "audit-logs"
    /// - Service-based: "user-service-commands", "payment-service-events"
    /// - Environment-prefixed: "prod.order-processing", "dev.email-notifications"
    /// - Priority-suffixed: "high-priority-orders", "background-tasks"
    /// 
    /// Queue names are case-sensitive and must be unique within a virtual host.
    /// Empty string names are allowed and result in server-generated unique names,
    /// which are useful for temporary or exclusive queues.
    /// 
    /// Naming considerations:
    /// - Descriptive names improve operational visibility and debugging
    /// - Consistent patterns enable automated management and monitoring
    /// - Avoid overly long names that complicate logging and management
    /// - Consider future maintenance and operational requirements
    /// </remarks>
    public required string Name { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether the queue is durable.
    /// </summary>
    /// <value><c>true</c> if the queue should survive broker restarts; otherwise, <c>false</c>. Default is <c>true</c>.</value>
    /// <remarks>
    /// Durability determines whether the queue persists across broker restarts:
    /// 
    /// Durable queues:
    /// - Survive broker restarts and maintain message storage capability
    /// - Essential for production systems requiring message persistence
    /// - Required for persistent message storage and delivery guarantees
    /// - Should be used for all business-critical message processing
    /// 
    /// Non-durable queues:
    /// - Lost when broker restarts, requiring recreation by applications
    /// - Suitable for temporary, session-based, or ephemeral messaging
    /// - May be appropriate for development, testing, or cache-like scenarios
    /// - Can be used for real-time data where historical messages aren't important
    /// 
    /// Durability considerations:
    /// - Business importance and criticality of messages
    /// - Recovery requirements after broker maintenance or failures
    /// - Integration with message persistence and delivery guarantee requirements
    /// - Operational complexity of queue recreation and consumer coordination
    /// 
    /// Note: Queue durability is independent of message persistence. A durable queue
    /// can store both persistent and non-persistent messages, but a non-durable queue
    /// will lose all messages (persistent or not) when the broker restarts.
    /// </remarks>
    public bool Durable { get; set; } = true;

    /// <summary>
    /// Gets or sets a value indicating whether the queue is exclusive to the declaring connection.
    /// </summary>
    /// <value><c>true</c> if the queue should be exclusive; otherwise, <c>false</c>. Default is <c>false</c>.</value>
    /// <remarks>
    /// Exclusive queues are restricted to a single connection:
    /// 
    /// Exclusive queues:
    /// - Can only be accessed by the connection that declared them
    /// - Automatically deleted when the declaring connection closes
    /// - Provide isolation and prevent access conflicts
    /// - Useful for temporary, session-specific, or private messaging
    /// 
    /// Non-exclusive queues:
    /// - Can be accessed by multiple connections and consumers
    /// - Persist beyond individual connection lifecycles
    /// - Enable shared access and load balancing across connections
    /// - Required for most production messaging scenarios
    /// 
    /// Use exclusive queues for:
    /// - Temporary queues that should be cleaned up automatically
    /// - Session-specific messaging that shouldn't be shared
    /// - Request-response patterns with private reply queues
    /// - Testing and development scenarios requiring isolation
    /// 
    /// Use non-exclusive queues for:
    /// - Shared message processing across multiple consumers
    /// - Production workloads requiring resilience to connection failures
    /// - Load balancing and scalability scenarios
    /// - Persistent message storage that outlives individual connections
    /// 
    /// Exclusive queues are typically combined with auto-delete for automatic
    /// cleanup when connections close, making them ideal for temporary messaging patterns.
    /// </remarks>
    public bool Exclusive { get; set; } = false;

    /// <summary>
    /// Gets or sets a value indicating whether the queue should be automatically deleted.
    /// </summary>
    /// <value><c>true</c> if the queue should be auto-deleted when no longer used; otherwise, <c>false</c>. Default is <c>false</c>.</value>
    /// <remarks>
    /// Auto-deletion controls automatic cleanup when the queue is no longer used:
    /// 
    /// Auto-delete enabled:
    /// - Queue is automatically removed when no consumers are subscribed
    /// - Useful for temporary queues and dynamic messaging patterns
    /// - Reduces administrative overhead for queue lifecycle management
    /// - Can lead to accidental deletion if not carefully managed
    /// 
    /// Auto-delete disabled:
    /// - Queue persists even when no consumers are active
    /// - Provides stability and predictable message storage
    /// - Requires manual cleanup when queues are no longer needed
    /// - Prevents accidental deletion during consumer maintenance or failures
    /// 
    /// Use auto-delete when:
    /// - Queues are created dynamically for temporary purposes
    /// - Automatic cleanup is more important than persistence
    /// - Queue lifecycle is tightly coupled to consumer presence
    /// - Resource management automation is desired
    /// 
    /// Avoid auto-delete when:
    /// - Queues are part of permanent messaging infrastructure
    /// - Manual control over queue lifecycle is preferred
    /// - Risk of accidental deletion during maintenance is unacceptable
    /// - Queues need to buffer messages when consumers are temporarily unavailable
    /// 
    /// Auto-delete behavior considerations:
    /// - Auto-delete only triggers when consumer count reaches zero
    /// - Timing of deletion may vary based on broker implementation
    /// - Messages in auto-deleted queues are lost permanently
    /// - Monitoring and alerting should account for auto-deletion patterns
    /// </remarks>
    public bool AutoDelete { get; set; } = false;

    /// <summary>
    /// Gets or sets additional arguments for queue configuration.
    /// </summary>
    /// <value>A dictionary of queue-specific arguments, or null if no additional arguments are needed.</value>
    /// <remarks>
    /// Queue arguments provide advanced configuration options for specialized queue behavior:
    /// - Message TTL (time-to-live) and expiration policies
    /// - Queue length limits and overflow behavior
    /// - Dead letter exchange configuration for unprocessable messages
    /// - Priority queue configuration for message ordering
    /// - Performance tuning and resource management options
    /// 
    /// Common queue arguments:
    /// 
    /// Message TTL (x-message-ttl):
    /// - Sets default TTL for messages in the queue (milliseconds)
    /// - Messages expire automatically after the specified time
    /// - Useful for time-sensitive data and automatic cleanup
    /// - Example: Arguments["x-message-ttl"] = 3600000 (1 hour)
    /// 
    /// Queue TTL (x-expires):
    /// - Automatically deletes the queue after inactivity period (milliseconds)
    /// - Alternative to auto-delete based on time rather than consumer count
    /// - Useful for temporary queues with time-based cleanup
    /// - Example: Arguments["x-expires"] = 1800000 (30 minutes)
    /// 
    /// Queue Length Limit (x-max-length):
    /// - Sets maximum number of messages the queue can hold
    /// - Oldest messages are dropped when limit is reached
    /// - Useful for preventing unbounded queue growth
    /// - Example: Arguments["x-max-length"] = 10000
    /// 
    /// Queue Size Limit (x-max-length-bytes):
    /// - Sets maximum total size of messages in the queue (bytes)
    /// - Messages are dropped when size limit is reached
    /// - Useful for memory and disk usage control
    /// - Example: Arguments["x-max-length-bytes"] = 104857600 (100MB)
    /// 
    /// Dead Letter Exchange (x-dead-letter-exchange):
    /// - Routes rejected, expired, or dropped messages to another exchange
    /// - Essential for error handling and poison message management
    /// - Enables message recovery and analysis workflows
    /// - Example: Arguments["x-dead-letter-exchange"] = "dlx"
    /// 
    /// Dead Letter Routing Key (x-dead-letter-routing-key):
    /// - Specifies routing key for messages sent to dead letter exchange
    /// - Enables selective routing of dead lettered messages
    /// - Can be used with different dead letter routing strategies
    /// - Example: Arguments["x-dead-letter-routing-key"] = "failed"
    /// 
    /// Queue Priority (x-max-priority):
    /// - Enables priority queue behavior with specified maximum priority
    /// - Messages with higher priority are delivered first
    /// - Useful for systems with varying message importance
    /// - Example: Arguments["x-max-priority"] = 10
    /// 
    /// Queue Mode (x-queue-mode):
    /// - Controls queue storage strategy (default vs lazy)
    /// - Lazy mode stores messages on disk immediately
    /// - Useful for large queues or memory-constrained environments
    /// - Example: Arguments["x-queue-mode"] = "lazy"
    /// 
    /// Overflow Behavior (x-overflow):
    /// - Controls behavior when queue length limits are reached
    /// - Options: drop-head (default), reject-publish
    /// - Affects how queue length limits are enforced
    /// - Example: Arguments["x-overflow"] = "reject-publish"
    /// 
    /// Consumer Timeout (x-consumer-timeout):
    /// - Automatically cancels consumers after inactivity period
    /// - Useful for detecting and cleaning up hung consumers
    /// - Helps maintain queue processing health
    /// - Example: Arguments["x-consumer-timeout"] = 900000 (15 minutes)
    /// 
    /// Use queue arguments when:
    /// - Default queue behavior doesn't meet specific requirements
    /// - Advanced message lifecycle management is needed
    /// - Resource usage control and limits are required
    /// - Error handling and dead letter processing is necessary
    /// - Priority-based message processing is required
    /// 
    /// Arguments are key-value pairs where keys are typically strings starting with "x-"
    /// and values can be strings, numbers, or booleans depending on the argument type.
    /// Incorrect argument types or values may cause queue declaration to fail.
    /// </remarks>
    public IDictionary<string, object>? Arguments { get; set; }

    /// <summary>
    /// Creates a standard durable queue declaration with the specified name.
    /// </summary>
    /// <param name="name">The name of the queue.</param>
    /// <param name="durable">Whether the queue should be durable. Default is true.</param>
    /// <returns>A queue declaration configured as a standard durable queue.</returns>
    /// <remarks>
    /// Standard queues provide reliable message storage for production workloads:
    /// - Durable by default for persistence across broker restarts
    /// - Non-exclusive to allow multiple consumers and connections
    /// - Not auto-deleted to provide stable message storage
    /// - No special arguments for straightforward behavior
    /// 
    /// Standard queues are ideal for:
    /// - Production message processing workloads
    /// - Reliable message storage and delivery
    /// - Load balancing across multiple consumers
    /// - Long-term message buffering and processing
    /// </remarks>
    public static QueueDeclaration CreateStandard(string name, bool durable = true)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(name);

        return new QueueDeclaration
        {
            Name = name,
            Durable = durable,
            Exclusive = false,
            AutoDelete = false
        };
    }

    /// <summary>
    /// Creates a temporary queue declaration that will be auto-deleted.
    /// </summary>
    /// <param name="name">The name of the temporary queue, or empty string for server-generated name.</param>
    /// <param name="exclusive">Whether the queue should be exclusive. Default is true for temporary queues.</param>
    /// <returns>A queue declaration configured as a temporary queue.</returns>
    /// <remarks>
    /// Temporary queues are automatically cleaned up when no longer needed:
    /// - Non-durable to avoid persistence overhead
    /// - Auto-delete for automatic cleanup
    /// - Often exclusive for private use by single connections
    /// - Suitable for short-lived messaging patterns
    /// 
    /// Temporary queues are ideal for:
    /// - Request-response patterns with private reply queues
    /// - Session-specific messaging that doesn't need persistence
    /// - Development and testing scenarios
    /// - Dynamic messaging patterns created by applications
    /// </remarks>
    public static QueueDeclaration CreateTemporary(string name = "", bool exclusive = true)
    {
        return new QueueDeclaration
        {
            Name = name,
            Durable = false,
            Exclusive = exclusive,
            AutoDelete = true
        };
    }

    /// <summary>
    /// Creates a priority queue declaration with specified maximum priority.
    /// </summary>
    /// <param name="name">The name of the priority queue.</param>
    /// <param name="maxPriority">The maximum priority level (1-255). Default is 10.</param>
    /// <param name="durable">Whether the queue should be durable. Default is true.</param>
    /// <returns>A queue declaration configured as a priority queue.</returns>
    /// <remarks>
    /// Priority queues deliver messages based on priority rather than arrival order:
    /// - Messages with higher priority are delivered first
    /// - Priority levels range from 0 (lowest) to maxPriority (highest)
    /// - Useful for systems with varying message importance
    /// - May have performance implications for very high priority ranges
    /// 
    /// Priority queues are ideal for:
    /// - Systems with critical and non-critical messages
    /// - Task processing with different urgency levels
    /// - SLA-based message processing
    /// - Alert and notification systems with priority levels
    /// 
    /// Priority considerations:
    /// - Higher max priority values may impact performance
    /// - Priority only affects delivery order, not storage
    /// - Messages with same priority are delivered in FIFO order
    /// - Priority settings should align with application needs
    /// </remarks>
    public static QueueDeclaration CreatePriority(string name, int maxPriority = 10, bool durable = true)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(name);

        if (maxPriority is < 1 or > 255)
        {
            throw new ArgumentOutOfRangeException(nameof(maxPriority), 
                "Priority must be between 1 and 255.");
        }

        return new QueueDeclaration
        {
            Name = name,
            Durable = durable,
            Exclusive = false,
            AutoDelete = false,
            Arguments = new Dictionary<string, object>
            {
                ["x-max-priority"] = maxPriority
            }
        };
    }

    /// <summary>
    /// Creates a lazy queue declaration optimized for large message volumes.
    /// </summary>
    /// <param name="name">The name of the lazy queue.</param>
    /// <param name="durable">Whether the queue should be durable. Default is true.</param>
    /// <returns>A queue declaration configured as a lazy queue.</returns>
    /// <remarks>
    /// Lazy queues store messages on disk immediately rather than in memory:
    /// - Lower memory usage for large queues
    /// - Messages are written to disk as soon as they arrive
    /// - May have higher latency due to disk I/O
    /// - Better for very large queues or memory-constrained environments
    /// 
    /// Lazy queues are ideal for:
    /// - Very large message backlogs
    /// - Memory-constrained broker environments
    /// - Scenarios where message volume may be unpredictable
    /// - Systems where memory usage needs to be controlled
    /// 
    /// Lazy queue trade-offs:
    /// - Lower memory usage vs. potentially higher latency
    /// - Better scalability vs. reduced throughput for small messages
    /// - Disk I/O vs. memory pressure considerations
    /// - Suitable for batch processing vs. real-time scenarios
    /// </remarks>
    public static QueueDeclaration CreateLazy(string name, bool durable = true)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(name);

        return new QueueDeclaration
        {
            Name = name,
            Durable = durable,
            Exclusive = false,
            AutoDelete = false,
            Arguments = new Dictionary<string, object>
            {
                ["x-queue-mode"] = "lazy"
            }
        };
    }

    /// <summary>
    /// Creates a queue declaration with dead letter exchange configuration.
    /// </summary>
    /// <param name="name">The name of the queue.</param>
    /// <param name="deadLetterExchange">The name of the dead letter exchange.</param>
    /// <param name="deadLetterRoutingKey">The routing key for dead lettered messages.</param>
    /// <param name="durable">Whether the queue should be durable. Default is true.</param>
    /// <returns>A queue declaration configured with dead letter handling.</returns>
    /// <remarks>
    /// Dead letter configuration provides error handling for unprocessable messages:
    /// - Routes rejected, expired, or dropped messages to another exchange
    /// - Essential for poison message handling and error recovery
    /// - Enables message analysis and manual intervention workflows
    /// - Prevents message loss during processing failures
    /// 
    /// Dead letter scenarios:
    /// - Messages rejected by consumers (negative acknowledgment)
    /// - Messages that exceed TTL and expire
    /// - Messages dropped due to queue length limits
    /// - Messages that exceed maximum delivery attempts
    /// 
    /// Dead letter processing patterns:
    /// - Error queue for manual investigation and reprocessing
    /// - Retry queue with delayed redelivery
    /// - Alert and notification systems for error monitoring
    /// - Analytics and metrics collection for system health
    /// </remarks>
    public static QueueDeclaration CreateWithDeadLetter(
        string name, 
        string deadLetterExchange, 
        string? deadLetterRoutingKey = null,
        bool durable = true)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(name);
        ArgumentException.ThrowIfNullOrWhiteSpace(deadLetterExchange);

        var arguments = new Dictionary<string, object>
        {
            ["x-dead-letter-exchange"] = deadLetterExchange
        };

        if (!string.IsNullOrEmpty(deadLetterRoutingKey))
        {
            arguments["x-dead-letter-routing-key"] = deadLetterRoutingKey;
        }

        return new QueueDeclaration
        {
            Name = name,
            Durable = durable,
            Exclusive = false,
            AutoDelete = false,
            Arguments = arguments
        };
    }

    /// <summary>
    /// Creates a queue declaration with length limits for resource control.
    /// </summary>
    /// <param name="name">The name of the queue.</param>
    /// <param name="maxLength">The maximum number of messages in the queue.</param>
    /// <param name="maxLengthBytes">The maximum total size of messages in bytes.</param>
    /// <param name="overflow">The overflow behavior when limits are reached.</param>
    /// <param name="durable">Whether the queue should be durable. Default is true.</param>
    /// <returns>A queue declaration configured with length limits.</returns>
    /// <remarks>
    /// Length limits provide resource control and prevent unbounded queue growth:
    /// - Protects broker memory and disk resources
    /// - Prevents system instability from excessive message accumulation
    /// - Enables predictable resource usage and capacity planning
    /// - Can be combined with dead letter routing for overflow handling
    /// 
    /// Overflow behaviors:
    /// - drop-head: Remove oldest messages when limit is reached (default)
    /// - reject-publish: Reject new messages when limit is reached
    /// 
    /// Length limit considerations:
    /// - Set limits based on expected message patterns and resource availability
    /// - Consider message size variation when setting byte limits
    /// - Monitor queue length metrics to validate limit effectiveness
    /// - Plan for overflow scenarios and dead letter handling
    /// </remarks>
    public static QueueDeclaration CreateWithLengthLimit(
        string name,
        int? maxLength = null,
        long? maxLengthBytes = null,
        string overflow = "drop-head",
        bool durable = true)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(name);

        if (maxLength == null && maxLengthBytes == null)
        {
            throw new ArgumentException("At least one length limit must be specified.");
        }

        var arguments = new Dictionary<string, object>();

        if (maxLength.HasValue)
        {
            if (maxLength <= 0)
                throw new ArgumentOutOfRangeException(nameof(maxLength), "Max length must be positive.");
            arguments["x-max-length"] = maxLength.Value;
        }

        if (maxLengthBytes.HasValue)
        {
            if (maxLengthBytes <= 0)
                throw new ArgumentOutOfRangeException(nameof(maxLengthBytes), "Max length bytes must be positive.");
            arguments["x-max-length-bytes"] = maxLengthBytes.Value;
        }

        arguments["x-overflow"] = overflow;

        return new QueueDeclaration
        {
            Name = name,
            Durable = durable,
            Exclusive = false,
            AutoDelete = false,
            Arguments = arguments
        };
    }

    /// <summary>
    /// Validates the queue declaration for consistency and correctness.
    /// </summary>
    /// <exception cref="BrokerConfigurationException">Thrown when the queue declaration is invalid.</exception>
    /// <remarks>
    /// Queue validation ensures that the declaration parameters are consistent
    /// and will result in a functional queue configuration.
    /// 
    /// Validation checks include:
    /// - Queue name format and requirements
    /// - Consistency between durability, exclusivity, and auto-delete settings
    /// - Argument format and value validation
    /// - Cross-argument consistency and compatibility
    /// 
    /// Early validation helps prevent runtime errors during queue declaration
    /// and provides clear feedback about configuration issues.
    /// </remarks>
    public void Validate()
    {
        // Queue name can be empty (server-generated), but if provided must be valid
        if (Name != null && Name.Contains(" "))
        {
            throw new BrokerConfigurationException(
                "Queue name cannot contain spaces.",
                configurationSection: nameof(QueueDeclaration),
                parameterName: nameof(Name),
                parameterValue: Name);
        }

        // Validate logical consistency
        if (Exclusive && !AutoDelete)
        {
            throw new BrokerConfigurationException(
                "Exclusive queues should typically be auto-delete to prevent resource leaks.",
                configurationSection: nameof(QueueDeclaration),
                parameterName: nameof(AutoDelete));
        }

        if (!Durable && !AutoDelete && !Exclusive)
        {
            throw new BrokerConfigurationException(
                "Non-durable, non-exclusive queues should typically be auto-delete to prevent accumulation.",
                configurationSection: nameof(QueueDeclaration),
                parameterName: nameof(AutoDelete));
        }

        // Validate arguments if present
        if (Arguments?.Any() == true)
        {
            ValidateArguments();
        }
    }

    /// <summary>
    /// Validates queue arguments for correctness and consistency.
    /// </summary>
    private void ValidateArguments()
    {
        if (Arguments == null) return;

        foreach (var argument in Arguments)
        {
            if (string.IsNullOrWhiteSpace(argument.Key))
            {
                throw new BrokerConfigurationException(
                    "Queue argument keys cannot be null or empty.",
                    configurationSection: nameof(QueueDeclaration),
                    parameterName: nameof(Arguments));
            }

            // Validate specific argument types and values
            switch (argument.Key)
            {
                case "x-message-ttl":
                case "x-expires":
                case "x-consumer-timeout":
                    if (argument.Value is not int ttlValue || ttlValue <= 0)
                    {
                        throw new BrokerConfigurationException(
                            $"Argument '{argument.Key}' must be a positive integer (milliseconds).",
                            configurationSection: nameof(QueueDeclaration),
                            parameterName: nameof(Arguments));
                    }
                    break;

                case "x-max-length":
                    if (argument.Value is not int lengthValue || lengthValue <= 0)
                    {
                        throw new BrokerConfigurationException(
                            "Argument 'x-max-length' must be a positive integer.",
                            configurationSection: nameof(QueueDeclaration),
                            parameterName: nameof(Arguments));
                    }
                    break;

                case "x-max-length-bytes":
                    if (argument.Value is not long bytesValue || bytesValue <= 0)
                    {
                        throw new BrokerConfigurationException(
                            "Argument 'x-max-length-bytes' must be a positive long integer.",
                            configurationSection: nameof(QueueDeclaration),
                            parameterName: nameof(Arguments));
                    }
                    break;

                case "x-max-priority":
                    if (argument.Value is not int priorityValue || priorityValue is < 1 or > 255)
                    {
                        throw new BrokerConfigurationException(
                            "Argument 'x-max-priority' must be between 1 and 255.",
                            configurationSection: nameof(QueueDeclaration),
                            parameterName: nameof(Arguments));
                    }
                    break;

                case "x-queue-mode":
                    if (argument.Value is not string modeValue || 
                        (modeValue != "default" && modeValue != "lazy"))
                    {
                        throw new BrokerConfigurationException(
                            "Argument 'x-queue-mode' must be 'default' or 'lazy'.",
                            configurationSection: nameof(QueueDeclaration),
                            parameterName: nameof(Arguments));
                    }
                    break;

                case "x-overflow":
                    if (argument.Value is not string overflowValue || 
                        (overflowValue != "drop-head" && overflowValue != "reject-publish"))
                    {
                        throw new BrokerConfigurationException(
                            "Argument 'x-overflow' must be 'drop-head' or 'reject-publish'.",
                            configurationSection: nameof(QueueDeclaration),
                            parameterName: nameof(Arguments));
                    }
                    break;

                case "x-dead-letter-exchange":
                    if (argument.Value is not string dlxValue || string.IsNullOrWhiteSpace(dlxValue))
                    {
                        throw new BrokerConfigurationException(
                            "Argument 'x-dead-letter-exchange' must be a non-empty string.",
                            configurationSection: nameof(QueueDeclaration),
                            parameterName: nameof(Arguments));
                    }
                    break;
            }
        }
    }

    /// <summary>
    /// Returns a string representation of the queue declaration.
    /// </summary>
    /// <returns>A formatted string describing the queue configuration.</returns>
    public override string ToString()
    {
        var features = new List<string>();
        
        if (Durable) features.Add("durable");
        if (Exclusive) features.Add("exclusive");
        if (AutoDelete) features.Add("auto-delete");
        if (Arguments?.Any() == true) features.Add($"args:{Arguments.Count}");

        var featureInfo = features.Any() ? $" [{string.Join(", ", features)}]" : "";
        return $"Queue '{Name}'{featureInfo}";
    }

    /// <summary>
    /// Determines whether two queue declarations are equivalent.
    /// </summary>
    /// <param name="obj">The object to compare with this declaration.</param>
    /// <returns><c>true</c> if the declarations are equivalent; otherwise, <c>false</c>.</returns>
    public override bool Equals(object? obj)
    {
        return obj is QueueDeclaration other &&
               Name == other.Name &&
               Durable == other.Durable &&
               Exclusive == other.Exclusive &&
               AutoDelete == other.AutoDelete;
    }

    /// <summary>
    /// Returns a hash code for the queue declaration.
    /// </summary>
    /// <returns>A hash code suitable for use in hash tables and collections.</returns>
    public override int GetHashCode()
    {
        return HashCode.Combine(Name, Durable, Exclusive, AutoDelete);
    }
}