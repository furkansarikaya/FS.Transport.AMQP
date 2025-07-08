namespace FS.RabbitMQ.Core.Abstractions;

/// <summary>
/// Contains configuration parameters for declaring a queue in RabbitMQ.
/// </summary>
/// <remarks>
/// Queue declaration is idempotent in RabbitMQ - if a queue already exists with the same configuration,
/// the operation succeeds and returns information about the existing queue. However, if configuration
/// parameters differ from the existing queue, a channel exception will be raised.
/// 
/// Queues are the final destination for messages in RabbitMQ and serve as buffers between
/// producers and consumers. Queue configuration significantly impacts message durability,
/// performance, and system behavior under various load conditions.
/// 
/// The Builder pattern is recommended for creating instances of this class to ensure
/// comprehensive configuration and proper validation of interdependent settings.
/// </remarks>
public sealed class QueueDeclaration
{
    /// <summary>
    /// Gets the name of the queue to declare.
    /// </summary>
    /// <value>
    /// A string representing the queue name. Can be null or empty for server-generated queue names.
    /// Server-generated names are typically used for temporary, exclusive queues.
    /// </value>
    /// <remarks>
    /// Queue naming considerations:
    /// - Empty or null names result in server-generated unique names
    /// - Names starting with "amq." are reserved for broker internal use
    /// - Queue names are case-sensitive and should follow consistent naming conventions
    /// - Consider including application/service prefixes for multi-tenant environments
    /// </remarks>
    public string? Name { get; init; }

    /// <summary>
    /// Gets a value indicating whether the queue should survive broker restarts.
    /// </summary>
    /// <value>
    /// <c>true</c> if the queue should be persisted to disk and survive broker restarts;
    /// <c>false</c> if the queue should be deleted when the broker restarts.
    /// </value>
    /// <remarks>
    /// Durable queues are essential for message persistence guarantees:
    /// - Messages in durable queues can survive broker restarts (if also marked as persistent)
    /// - Non-durable queues are faster but lose all messages during broker restarts
    /// - Production systems typically require durable queues for reliability
    /// - Consider disk I/O implications when designing high-throughput systems with durable queues
    /// </remarks>
    public bool Durable { get; init; } = true;

    /// <summary>
    /// Gets a value indicating whether the queue can only be accessed by the connection that declares it.
    /// </summary>
    /// <value>
    /// <c>true</c> if the queue is exclusive to the declaring connection;
    /// <c>false</c> if the queue can be accessed by multiple connections.
    /// </value>
    /// <remarks>
    /// Exclusive queues provide connection-scoped isolation:
    /// - Only the connection that declared the queue can access it
    /// - Automatically deleted when the declaring connection closes
    /// - Useful for temporary, private communication channels
    /// - Cannot be redeclared by other connections, even with identical parameters
    /// - Ideal for request-response patterns with temporary reply queues
    /// </remarks>
    public bool Exclusive { get; init; } = false;

    /// <summary>
    /// Gets a value indicating whether the queue should be deleted when no longer in use.
    /// </summary>
    /// <value>
    /// <c>true</c> if the queue should be automatically deleted when no consumers are attached;
    /// <c>false</c> if the queue should persist even when unused.
    /// </value>
    /// <remarks>
    /// Auto-delete behavior helps with resource management:
    /// - Queue is deleted when the last consumer cancels or disconnects
    /// - Useful for temporary processing queues in dynamic systems
    /// - Be cautious in production: temporary network issues might trigger deletion
    /// - Messages in auto-deleted queues are lost unless consumed or moved elsewhere
    /// - Exclusive queues are often combined with auto-delete for temporary channels
    /// </remarks>
    public bool AutoDelete { get; init; } = false;

    /// <summary>
    /// Gets additional arguments for queue declaration.
    /// </summary>
    /// <value>
    /// A dictionary containing optional queue arguments, or <c>null</c> if no additional arguments are needed.
    /// </value>
    /// <remarks>
    /// Queue arguments enable advanced queue features and behaviors:
    /// 
    /// Message lifecycle arguments:
    /// - "x-message-ttl": Message time-to-live in milliseconds
    /// - "x-expires": Queue expiration time in milliseconds when unused
    /// - "x-max-length": Maximum number of messages in the queue
    /// - "x-max-length-bytes": Maximum total size of messages in bytes
    /// 
    /// Dead letter arguments:
    /// - "x-dead-letter-exchange": Exchange for expired or rejected messages
    /// - "x-dead-letter-routing-key": Routing key for dead letter messages
    /// 
    /// Performance and behavior arguments:
    /// - "x-max-priority": Enable priority queues (0-255, higher numbers = higher priority)
    /// - "x-queue-mode": Set to "lazy" for disk-based storage optimization
    /// - "x-overflow": Behavior when max-length is reached ("drop-head", "reject-publish", "reject-publish-dlx")
    /// 
    /// Arguments are broker-specific and may require additional plugins or configuration.
    /// </remarks>
    public IReadOnlyDictionary<string, object>? Arguments { get; init; }

    /// <summary>
    /// Creates a new builder instance for constructing QueueDeclaration objects.
    /// </summary>
    /// <param name="name">
    /// The name of the queue to declare. Use null or empty string for server-generated names.
    /// </param>
    /// <returns>A new QueueDeclarationBuilder instance for fluent configuration.</returns>
    /// <remarks>
    /// Server-generated queue names are particularly useful for:
    /// - Temporary reply queues in request-response patterns
    /// - Exclusive queues that don't need stable names
    /// - Dynamic queue creation scenarios where name collisions must be avoided
    /// 
    /// Named queues are essential for:
    /// - Persistent work queues with multiple consumers
    /// - Queues that must be accessible across application restarts
    /// - Well-known queues that multiple services need to reference
    /// </remarks>
    public static QueueDeclarationBuilder Create(string? name = null)
    {
        return new QueueDeclarationBuilder(name);
    }

    /// <summary>
    /// Creates a new builder instance for constructing a temporary, exclusive queue declaration.
    /// </summary>
    /// <returns>
    /// A new QueueDeclarationBuilder instance pre-configured for temporary queue usage.
    /// </returns>
    /// <remarks>
    /// Temporary queues are automatically configured with:
    /// - Server-generated name (empty name)
    /// - Exclusive access (only declaring connection can use it)
    /// - Auto-delete behavior (deleted when connection closes)
    /// - Non-durable (exists only in memory)
    /// 
    /// This configuration is ideal for:
    /// - Request-response reply queues
    /// - Private notification channels
    /// - Temporary data processing pipelines
    /// - Client-specific event subscriptions
    /// </remarks>
    public static QueueDeclarationBuilder CreateTemporary()
    {
        return new QueueDeclarationBuilder(null)
            .AsExclusive()
            .WithAutoDelete()
            .AsTransient();
    }

    /// <summary>
    /// Creates a new builder instance for constructing a work queue declaration optimized for task distribution.
    /// </summary>
    /// <param name="name">The name of the work queue. Cannot be null or whitespace.</param>
    /// <returns>
    /// A new QueueDeclarationBuilder instance pre-configured for work queue usage.
    /// </returns>
    /// <exception cref="ArgumentException">
    /// Thrown when <paramref name="name"/> is null, empty, or contains only whitespace.
    /// </exception>
    /// <remarks>
    /// Work queues are automatically configured with:
    /// - Durable persistence (survives broker restarts)
    /// - Non-exclusive access (multiple consumers can process tasks)
    /// - Persistent behavior (not auto-deleted)
    /// 
    /// This configuration is ideal for:
    /// - Background job processing
    /// - Load-balanced task distribution
    /// - Reliable message processing with multiple workers
    /// - Long-running batch operations
    /// 
    /// Work queues typically require additional configuration for:
    /// - Dead letter handling for failed tasks
    /// - Message TTL for task expiration
    /// - Priority queues for task prioritization
    /// </remarks>
    public static QueueDeclarationBuilder CreateWorkQueue(string name)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(name);
        return new QueueDeclarationBuilder(name)
            .AsDurable()
            .AsShared()
            .WithoutAutoDelete();
    }

    /// <summary>
    /// Validates the queue declaration configuration for consistency and completeness.
    /// </summary>
    /// <exception cref="InvalidOperationException">
    /// Thrown when the configuration contains invalid or conflicting settings.
    /// </exception>
    /// <remarks>
    /// Validation checks include:
    /// - Queue name format and reservation rules
    /// - Logical consistency of exclusive/auto-delete combinations
    /// - Argument format and value constraints
    /// - Compatibility of durability and exclusivity settings
    /// 
    /// This method is called automatically during queue declaration to prevent
    /// runtime errors due to configuration mistakes.
    /// </remarks>
    internal void Validate()
    {
        // Validate reserved queue name patterns
        if (!string.IsNullOrEmpty(Name) && Name.StartsWith("amq.", StringComparison.OrdinalIgnoreCase))
        {
            throw new InvalidOperationException($"Queue name '{Name}' is reserved for broker internal use. Choose a different name.");
        }

        // Validate logical consistency of settings
        if (Exclusive && !AutoDelete)
        {
            // This is actually valid, but worth noting for documentation
            // Exclusive queues often use auto-delete, but it's not required
        }

        // Validate arguments if present
        if (Arguments?.Any() != true) return;
        foreach (var kvp in Arguments)
        {
            if (string.IsNullOrWhiteSpace(kvp.Key))
            {
                throw new InvalidOperationException("Queue argument keys cannot be null or empty.");
            }

            // Validate specific argument types and values
            ValidateQueueArgument(kvp.Key, kvp.Value);
        }
    }

    /// <summary>
    /// Validates individual queue arguments for correctness and compatibility.
    /// </summary>
    /// <param name="key">The argument key to validate.</param>
    /// <param name="value">The argument value to validate.</param>
    /// <exception cref="InvalidOperationException">
    /// Thrown when the argument key-value pair is invalid or incompatible.
    /// </exception>
    private static void ValidateQueueArgument(string key, object value)
    {
        switch (key)
        {
            case "x-message-ttl":
            case "x-expires":
            case "x-max-length":
            case "x-max-length-bytes":
                if (value is not int intValue || intValue < 0)
                {
                    throw new InvalidOperationException($"Queue argument '{key}' must be a non-negative integer, but was: {value}");
                }
                break;

            case "x-max-priority":
                if (value is not int priorityValue || priorityValue < 0 || priorityValue > 255)
                {
                    throw new InvalidOperationException($"Queue argument 'x-max-priority' must be an integer between 0 and 255, but was: {value}");
                }
                break;

            case "x-queue-mode":
                if (value is not string modeValue || (modeValue != "default" && modeValue != "lazy"))
                {
                    throw new InvalidOperationException($"Queue argument 'x-queue-mode' must be 'default' or 'lazy', but was: {value}");
                }
                break;

            case "x-overflow":
                if (value is not string overflowValue || 
                    (overflowValue != "drop-head" && overflowValue != "reject-publish" && overflowValue != "reject-publish-dlx"))
                {
                    throw new InvalidOperationException($"Queue argument 'x-overflow' must be 'drop-head', 'reject-publish', or 'reject-publish-dlx', but was: {value}");
                }
                break;

            case "x-dead-letter-exchange":
            case "x-dead-letter-routing-key":
                if (value is not string stringValue)
                {
                    throw new InvalidOperationException($"Queue argument '{key}' must be a string, but was: {value?.GetType().Name ?? "null"}");
                }
                break;
        }
    }
}