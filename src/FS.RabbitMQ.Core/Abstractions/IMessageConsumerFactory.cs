namespace FS.RabbitMQ.Core.Abstractions;

/// <summary>
/// Defines the contract for creating message consumers with different consumption patterns and configurations.
/// </summary>
/// <remarks>
/// The message consumer factory provides a centralized way to create various types of message consumers
/// that can handle different messaging patterns, processing strategies, and quality of service requirements.
/// It abstracts the complexity of consumer configuration and lifecycle management while providing
/// flexibility for different consumption scenarios.
/// 
/// Key design principles:
/// - Factory pattern for consistent consumer creation and configuration
/// - Support for multiple consumption patterns (push-based, pull-based, batch processing)
/// - Flexible routing and filtering options for targeted message consumption
/// - Comprehensive error handling and recovery mechanisms
/// - Integration with dependency injection and configuration systems
/// 
/// The factory automatically manages underlying RabbitMQ consumer setup, channel allocation,
/// and connection recovery, making it safe for use in long-running applications with
/// varying network conditions and load patterns.
/// </remarks>
public interface IMessageConsumerFactory
{
    /// <summary>
    /// Creates a push-based consumer that automatically receives messages as they arrive.
    /// </summary>
    /// <typeparam name="T">The type of message payload to consume.</typeparam>
    /// <param name="queueName">The name of the queue to consume from. Cannot be null or whitespace.</param>
    /// <param name="messageHandler">The handler function to process received messages. Cannot be null.</param>
    /// <param name="options">Optional configuration options for the consumer behavior.</param>
    /// <returns>A configured push-based message consumer.</returns>
    /// <exception cref="ArgumentException">Thrown when <paramref name="queueName"/> is null, empty, or whitespace.</exception>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="messageHandler"/> is null.</exception>
    /// <exception cref="ConsumerCreationException">Thrown when the consumer cannot be created due to configuration or connectivity issues.</exception>
    /// <remarks>
    /// Push-based consumers provide automatic message delivery using the RabbitMQ push model:
    /// - Messages are delivered automatically as they become available
    /// - No polling or explicit message retrieval required
    /// - Optimal for real-time processing scenarios with low latency requirements
    /// - Supports automatic acknowledgment and error handling
    /// - Includes built-in retry mechanisms and dead letter handling
    /// 
    /// Push consumers are ideal for:
    /// - Event-driven architectures requiring immediate processing
    /// - Real-time notification systems
    /// - Stream processing scenarios with continuous data flow
    /// - Microservices communication with low latency requirements
    /// 
    /// The message handler receives structured message context including headers,
    /// routing information, and acknowledgment controls for fine-grained processing control.
    /// </remarks>
    IPushConsumer<T> CreatePushConsumer<T>(
        string queueName,
        Func<MessageContext<T>, CancellationToken, Task> messageHandler,
        ConsumerOptions? options = null) where T : class;

    /// <summary>
    /// Creates a pull-based consumer that retrieves messages on demand.
    /// </summary>
    /// <typeparam name="T">The type of message payload to consume.</typeparam>
    /// <param name="queueName">The name of the queue to consume from. Cannot be null or whitespace.</param>
    /// <param name="options">Optional configuration options for the consumer behavior.</param>
    /// <returns>A configured pull-based message consumer.</returns>
    /// <exception cref="ArgumentException">Thrown when <paramref name="queueName"/> is null, empty, or whitespace.</exception>
    /// <exception cref="ConsumerCreationException">Thrown when the consumer cannot be created due to configuration or connectivity issues.</exception>
    /// <remarks>
    /// Pull-based consumers provide on-demand message retrieval using explicit polling:
    /// - Messages are retrieved only when explicitly requested
    /// - Full control over message retrieval timing and batch sizes
    /// - Optimal for batch processing scenarios with controlled resource usage
    /// - Supports sophisticated flow control and backpressure management
    /// - Enables integration with external scheduling and orchestration systems
    /// 
    /// Pull consumers are ideal for:
    /// - Batch processing jobs with scheduled execution
    /// - Resource-constrained environments requiring controlled consumption
    /// - Integration with external workflow and orchestration systems
    /// - Scenarios requiring sophisticated flow control and rate limiting
    /// 
    /// Pull consumers provide methods for single message retrieval, batch retrieval,
    /// and conditional retrieval based on queue state and message properties.
    /// </remarks>
    IPullConsumer<T> CreatePullConsumer<T>(
        string queueName,
        ConsumerOptions? options = null) where T : class;

    /// <summary>
    /// Creates a batch consumer that processes multiple messages together for improved efficiency.
    /// </summary>
    /// <typeparam name="T">The type of message payload to consume.</typeparam>
    /// <param name="queueName">The name of the queue to consume from. Cannot be null or whitespace.</param>
    /// <param name="batchHandler">The handler function to process batches of messages. Cannot be null.</param>
    /// <param name="batchSize">The maximum number of messages to include in each batch. Must be positive.</param>
    /// <param name="options">Optional configuration options for the consumer behavior.</param>
    /// <returns>A configured batch message consumer.</returns>
    /// <exception cref="ArgumentException">
    /// Thrown when <paramref name="queueName"/> is null, empty, or whitespace, or when <paramref name="batchSize"/> is not positive.
    /// </exception>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="batchHandler"/> is null.</exception>
    /// <exception cref="ConsumerCreationException">Thrown when the consumer cannot be created due to configuration or connectivity issues.</exception>
    /// <remarks>
    /// Batch consumers optimize throughput by processing multiple messages together:
    /// - Reduces per-message processing overhead through batching
    /// - Enables efficient resource utilization for high-volume scenarios
    /// - Supports atomic batch processing with all-or-nothing semantics
    /// - Includes intelligent batch formation based on message availability and timing
    /// - Provides comprehensive error handling for partial batch failures
    /// 
    /// Batch consumers are ideal for:
    /// - High-throughput data processing pipelines
    /// - Database bulk operations and ETL processes
    /// - Analytics and reporting scenarios with large data volumes
    /// - Integration scenarios requiring transaction-like batch processing
    /// 
    /// The batch handler receives collections of message contexts and can implement
    /// sophisticated processing logic including partial success handling and retry strategies
    /// for individual messages within failed batches.
    /// </remarks>
    IBatchConsumer<T> CreateBatchConsumer<T>(
        string queueName,
        Func<IReadOnlyList<MessageContext<T>>, CancellationToken, Task> batchHandler,
        int batchSize,
        ConsumerOptions? options = null) where T : class;

    /// <summary>
    /// Creates a competitive consumer that participates in load balancing across multiple consumer instances.
    /// </summary>
    /// <typeparam name="T">The type of message payload to consume.</typeparam>
    /// <param name="queueName">The name of the queue to consume from. Cannot be null or whitespace.</param>
    /// <param name="messageHandler">The handler function to process received messages. Cannot be null.</param>
    /// <param name="consumerGroup">The consumer group identifier for load balancing. Cannot be null or whitespace.</param>
    /// <param name="options">Optional configuration options for the consumer behavior.</param>
    /// <returns>A configured competitive message consumer.</returns>
    /// <exception cref="ArgumentException">
    /// Thrown when <paramref name="queueName"/> or <paramref name="consumerGroup"/> is null, empty, or whitespace.
    /// </exception>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="messageHandler"/> is null.</exception>
    /// <exception cref="ConsumerCreationException">Thrown when the consumer cannot be created due to configuration or connectivity issues.</exception>
    /// <remarks>
    /// Competitive consumers enable horizontal scaling through load distribution:
    /// - Messages are distributed across multiple consumer instances for parallel processing
    /// - Automatic load balancing based on consumer availability and processing capacity
    /// - Supports dynamic scaling with automatic rebalancing when consumers join or leave
    /// - Includes comprehensive failure detection and recovery mechanisms
    /// - Provides monitoring and metrics for load distribution analysis
    /// 
    /// Competitive consumers are ideal for:
    /// - Horizontally scalable microservices architectures
    /// - Auto-scaling scenarios with dynamic resource allocation
    /// - High-availability systems requiring fault tolerance
    /// - Load-balanced processing across multiple worker instances
    /// 
    /// Consumer groups enable coordinated processing where each message is delivered
    /// to exactly one consumer within the group, providing natural load distribution
    /// and failure isolation across multiple processing instances.
    /// </remarks>
    ICompetitiveConsumer<T> CreateCompetitiveConsumer<T>(
        string queueName,
        Func<MessageContext<T>, CancellationToken, Task> messageHandler,
        string consumerGroup,
        ConsumerOptions? options = null) where T : class;

    /// <summary>
    /// Creates a filtered consumer that only processes messages matching specific criteria.
    /// </summary>
    /// <typeparam name="T">The type of message payload to consume.</typeparam>
    /// <param name="queueName">The name of the queue to consume from. Cannot be null or whitespace.</param>
    /// <param name="messageHandler">The handler function to process filtered messages. Cannot be null.</param>
    /// <param name="filter">The filter function to determine which messages to process. Cannot be null.</param>
    /// <param name="options">Optional configuration options for the consumer behavior.</param>
    /// <returns>A configured filtered message consumer.</returns>
    /// <exception cref="ArgumentException">Thrown when <paramref name="queueName"/> is null, empty, or whitespace.</exception>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="messageHandler"/> or <paramref name="filter"/> is null.</exception>
    /// <exception cref="ConsumerCreationException">Thrown when the consumer cannot be created due to configuration or connectivity issues.</exception>
    /// <remarks>
    /// Filtered consumers provide selective message processing based on runtime criteria:
    /// - Messages are evaluated against filter criteria before processing
    /// - Non-matching messages are automatically acknowledged without processing
    /// - Supports complex filtering logic based on message content, headers, and metadata
    /// - Enables content-based routing and selective processing patterns
    /// - Includes comprehensive metrics for filter effectiveness analysis
    /// 
    /// Filtered consumers are ideal for:
    /// - Multi-tenant systems requiring tenant-specific processing
    /// - Content-based routing scenarios with complex selection criteria
    /// - Selective processing based on message priorities or categories
    /// - A/B testing scenarios with conditional message handling
    /// 
    /// Filter functions receive full message context and can implement sophisticated
    /// selection logic including header-based filtering, content inspection,
    /// and integration with external configuration or policy systems.
    /// </remarks>
    IFilteredConsumer<T> CreateFilteredConsumer<T>(
        string queueName,
        Func<MessageContext<T>, CancellationToken, Task> messageHandler,
        Func<MessageContext<T>, bool> filter,
        ConsumerOptions? options = null) where T : class;

    /// <summary>
    /// Creates a transactional consumer that processes messages within transaction scopes.
    /// </summary>
    /// <typeparam name="T">The type of message payload to consume.</typeparam>
    /// <param name="queueName">The name of the queue to consume from. Cannot be null or whitespace.</param>
    /// <param name="messageHandler">The handler function to process messages transactionally. Cannot be null.</param>
    /// <param name="options">Optional configuration options for the consumer behavior.</param>
    /// <returns>A configured transactional message consumer.</returns>
    /// <exception cref="ArgumentException">Thrown when <paramref name="queueName"/> is null, empty, or whitespace.</exception>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="messageHandler"/> is null.</exception>
    /// <exception cref="ConsumerCreationException">Thrown when the consumer cannot be created due to configuration or connectivity issues.</exception>
    /// <remarks>
    /// Transactional consumers provide ACID properties for message processing:
    /// - Message acknowledgment and handler execution are wrapped in transactions
    /// - All-or-nothing semantics ensure consistent state across message processing
    /// - Automatic rollback and redelivery on processing failures
    /// - Integration with external transactional resources (databases, message brokers)
    /// - Comprehensive error handling with transaction-aware retry mechanisms
    /// 
    /// Transactional consumers are ideal for:
    /// - Financial systems requiring strict consistency guarantees
    /// - Multi-step workflows where partial completion is problematic
    /// - Integration scenarios requiring atomic operations across multiple systems
    /// - Compliance scenarios requiring transactional audit trails
    /// 
    /// Transaction scopes are automatically managed, providing the message handler
    /// with a transactional context that can enlist additional resources
    /// for coordinated transaction processing across multiple systems.
    /// </remarks>
    ITransactionalConsumer<T> CreateTransactionalConsumer<T>(
        string queueName,
        Func<MessageContext<T>, ITransactionScope, CancellationToken, Task> messageHandler,
        ConsumerOptions? options = null) where T : class;

    /// <summary>
    /// Creates a consumer with a custom processing strategy for specialized scenarios.
    /// </summary>
    /// <typeparam name="T">The type of message payload to consume.</typeparam>
    /// <param name="queueName">The name of the queue to consume from. Cannot be null or whitespace.</param>
    /// <param name="processingStrategy">The custom processing strategy to use. Cannot be null.</param>
    /// <param name="options">Optional configuration options for the consumer behavior.</param>
    /// <returns>A configured custom message consumer.</returns>
    /// <exception cref="ArgumentException">Thrown when <paramref name="queueName"/> is null, empty, or whitespace.</exception>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="processingStrategy"/> is null.</exception>
    /// <exception cref="ConsumerCreationException">Thrown when the consumer cannot be created due to configuration or connectivity issues.</exception>
    /// <remarks>
    /// Custom consumers enable implementation of specialized processing patterns:
    /// - Full control over message lifecycle and processing flow
    /// - Integration with custom frameworks and processing engines
    /// - Support for advanced patterns not covered by standard consumer types
    /// - Extensibility for domain-specific processing requirements
    /// - Comprehensive hooks for monitoring, metrics, and observability
    /// 
    /// Custom consumers are ideal for:
    /// - Integration with existing processing frameworks and libraries
    /// - Implementation of specialized messaging patterns
    /// - Research and development scenarios requiring experimental approaches
    /// - Legacy system integration with unique processing requirements
    /// 
    /// Processing strategies implement the IMessageProcessingStrategy interface
    /// and provide complete control over message handling, acknowledgment,
    /// error recovery, and integration with external systems and frameworks.
    /// </remarks>
    ICustomConsumer<T> CreateCustomConsumer<T>(
        string queueName,
        IMessageProcessingStrategy<T> processingStrategy,
        ConsumerOptions? options = null) where T : class;
}