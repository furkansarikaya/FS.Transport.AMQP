namespace FS.RabbitMQ.Core.Abstractions;

/// <summary>
/// Contains the result information from a queue declaration operation.
/// </summary>
/// <remarks>
/// Queue declaration operations return metadata about the declared queue,
/// including the actual queue name (important for server-generated names),
/// current message counts, and consumer information. This information is
/// valuable for monitoring, debugging, and implementing dynamic queue management logic.
/// 
/// The result provides insight into queue state at declaration time,
/// but values may change immediately after declaration as messages are
/// published and consumed. Use this data for initial setup validation
/// rather than ongoing operational monitoring.
/// </remarks>
public sealed record QueueDeclareResult
{
    /// <summary>
    /// Gets the actual name of the declared queue.
    /// </summary>
    /// <value>The queue name assigned by the broker, which may be server-generated for anonymous queues.</value>
    /// <remarks>
    /// Queue names are critical for subsequent operations:
    /// - For named queues, this matches the requested name in the declaration
    /// - For anonymous queues (empty name in declaration), this contains the server-generated unique name
    /// - Server-generated names typically follow the pattern "amq.gen-{unique-identifier}"
    /// - Use this value for binding operations, consumer setup, and queue management
    /// 
    /// Server-generated names are guaranteed to be unique within the broker instance
    /// and are essential for implementing dynamic queue creation patterns.
    /// </remarks>
    public required string QueueName { get; init; }

    /// <summary>
    /// Gets the number of messages currently waiting in the queue.
    /// </summary>
    /// <value>The count of messages available for consumption at declaration time.</value>
    /// <remarks>
    /// Message count provides a snapshot of queue depth:
    /// - Includes all messages regardless of their properties or state
    /// - Does not include messages currently being processed by consumers
    /// - Value may change immediately after declaration due to concurrent operations
    /// - Useful for initial queue state assessment and capacity planning
    /// 
    /// For ongoing monitoring, implement separate queue inspection mechanisms
    /// rather than relying on declaration-time snapshots.
    /// </remarks>
    public uint MessageCount { get; init; }

    /// <summary>
    /// Gets the number of consumers currently attached to the queue.
    /// </summary>
    /// <value>The count of active consumers at declaration time.</value>
    /// <remarks>
    /// Consumer count indicates queue utilization and processing capacity:
    /// - Includes all consumers regardless of their prefetch settings or activity level
    /// - Zero consumers may indicate processing bottlenecks or consumer failures
    /// - Multiple consumers enable load balancing and increased throughput
    /// - Value reflects the state at declaration time and may change dynamically
    /// 
    /// Consumer count is particularly important for:
    /// - Validating that expected consumers are attached
    /// - Detecting consumer failures in critical processing pipelines
    /// - Implementing dynamic scaling decisions based on queue utilization
    /// - Debugging message processing bottlenecks
    /// </remarks>
    public uint ConsumerCount { get; init; }

    /// <summary>
    /// Creates a new QueueDeclareResult with the specified parameters.
    /// </summary>
    /// <param name="queueName">The name of the declared queue. Cannot be null or empty.</param>
    /// <param name="messageCount">The number of messages currently in the queue.</param>
    /// <param name="consumerCount">The number of consumers currently attached to the queue.</param>
    /// <returns>A new QueueDeclareResult instance with the specified values.</returns>
    /// <exception cref="ArgumentException">Thrown when <paramref name="queueName"/> is null or empty.</exception>
    /// <remarks>
    /// This factory method provides a convenient way to create QueueDeclareResult instances
    /// with validation of required parameters. It's primarily used by the internal
    /// queue declaration implementation to return results from broker operations.
    /// </remarks>
    public static QueueDeclareResult Create(string queueName, uint messageCount, uint consumerCount)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(queueName);

        return new QueueDeclareResult
        {
            QueueName = queueName,
            MessageCount = messageCount,
            ConsumerCount = consumerCount
        };
    }

    /// <summary>
    /// Gets a value indicating whether the queue currently has messages waiting for processing.
    /// </summary>
    /// <value><c>true</c> if the queue contains one or more messages; otherwise, <c>false</c>.</value>
    /// <remarks>
    /// This property provides a convenient boolean check for queue emptiness,
    /// which is commonly needed for conditional processing logic and monitoring scenarios.
    /// </remarks>
    public bool HasMessages => MessageCount > 0;

    /// <summary>
    /// Gets a value indicating whether the queue currently has consumers attached.
    /// </summary>
    /// <value><c>true</c> if the queue has one or more consumers; otherwise, <c>false</c>.</value>
    /// <remarks>
    /// This property helps identify orphaned queues that may be accumulating messages
    /// without any consumers to process them, which is often a sign of system issues
    /// or configuration problems.
    /// </remarks>
    public bool HasConsumers => ConsumerCount > 0;

    /// <summary>
    /// Gets a value indicating whether the queue appears to have a processing backlog.
    /// </summary>
    /// <value><c>true</c> if there are messages but no consumers to process them; otherwise, <c>false</c>.</value>
    /// <remarks>
    /// A backlog condition (messages present but no consumers) often indicates:
    /// - Consumer failures or disconnections
    /// - Insufficient consumer capacity for the message load
    /// - Configuration issues preventing consumer attachment
    /// - Temporary processing delays during system startup
    /// 
    /// This property is useful for automated monitoring and alerting systems
    /// that need to detect potential processing bottlenecks.
    /// </remarks>
    public bool HasBacklog => HasMessages && !HasConsumers;

    /// <summary>
    /// Returns a string representation of the queue declaration result.
    /// </summary>
    /// <returns>A formatted string containing the queue name, message count, and consumer count.</returns>
    /// <remarks>
    /// The string representation is designed for logging and debugging purposes,
    /// providing a concise overview of the queue state at declaration time.
    /// </remarks>
    public override string ToString()
    {
        return $"Queue '{QueueName}': {MessageCount} messages, {ConsumerCount} consumers";
    }
}