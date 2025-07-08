namespace FS.RabbitMQ.Core.Abstractions;

/// <summary>
/// Contains configuration options for message publishing operations.
/// </summary>
/// <remarks>
/// Publishing options provide fine-grained control over message properties, delivery behavior,
/// and performance characteristics. These options allow applications to customize publishing
/// behavior based on specific requirements such as durability guarantees, priority handling,
/// expiration policies, and delivery confirmation requirements.
/// 
/// The options are designed to cover the most common messaging scenarios while providing
/// extensibility for advanced use cases through custom properties and headers.
/// Many options correspond directly to AMQP message properties and RabbitMQ-specific features.
/// </remarks>
public sealed class PublishOptions
{
    /// <summary>
    /// Gets or sets a value indicating whether the message should be persisted to disk.
    /// </summary>
    /// <value>
    /// <c>true</c> if the message should survive broker restarts; <c>false</c> for in-memory only storage.
    /// Default is <c>true</c> for reliability.
    /// </value>
    /// <remarks>
    /// Message persistence works in conjunction with durable queues and exchanges to provide
    /// end-to-end durability guarantees:
    /// - Persistent messages in durable queues survive broker restarts
    /// - Non-persistent messages are faster but lost during broker restarts
    /// - Persistence has performance implications due to disk I/O requirements
    /// 
    /// Use persistent messages for:
    /// - Critical business data that cannot be lost
    /// - Financial transactions and audit trails
    /// - Long-running workflows and state management
    /// - Compliance scenarios requiring message retention
    /// 
    /// Use non-persistent messages for:
    /// - Real-time notifications and status updates
    /// - High-frequency telemetry and monitoring data
    /// - Temporary coordination messages
    /// - Performance-critical scenarios where speed is prioritized over durability
    /// </remarks>
    public bool Persistent { get; set; } = true;

    /// <summary>
    /// Gets or sets the priority level for the message.
    /// </summary>
    /// <value>
    /// An optional priority value between 0 and 255, where higher values indicate higher priority.
    /// <c>null</c> indicates no specific priority (treated as priority 0).
    /// </value>
    /// <remarks>
    /// Message priority enables preferential processing of important messages:
    /// - Requires the target queue to be declared with priority support (x-max-priority argument)
    /// - Higher priority messages are delivered before lower priority messages
    /// - Priority 0 is the default for messages without explicit priority
    /// - Maximum priority value depends on queue configuration (typically 0-10 or 0-255)
    /// 
    /// Priority queues are useful for:
    /// - Processing urgent messages before routine ones
    /// - Implementing service level agreements with different response times
    /// - Managing system resources during high load periods
    /// - Handling both interactive and batch processing workloads
    /// 
    /// Note: Priority handling adds overhead to queue operations and should be used
    /// judiciously in high-throughput scenarios.
    /// </remarks>
    public byte? Priority { get; set; }

    /// <summary>
    /// Gets or sets the message expiration time.
    /// </summary>
    /// <value>
    /// The time duration after which the message should be considered expired and removed.
    /// <c>null</c> indicates no expiration (message persists until consumed).
    /// </value>
    /// <remarks>
    /// Message expiration provides automatic cleanup of aged messages:
    /// - Expiration countdown begins when the message is published
    /// - Expired messages are automatically removed from queues
    /// - Expired messages can be redirected to dead letter exchanges if configured
    /// - Helps prevent unbounded queue growth and memory exhaustion
    /// 
    /// Use message expiration for:
    /// - Time-sensitive notifications that become irrelevant
    /// - Implementing timeout behaviors in request-response patterns
    /// - Preventing stale data processing in batch operations
    /// - Compliance with data retention policies
    /// 
    /// Consider the processing time requirements of consumers when setting expiration values.
    /// Messages that expire too quickly may be lost before legitimate processing can occur.
    /// </remarks>
    public TimeSpan? Expiration { get; set; }

    /// <summary>
    /// Gets or sets the message type identifier.
    /// </summary>
    /// <value>
    /// A string identifying the message type for routing and processing logic.
    /// Can be null if type identification is not required.
    /// </value>
    /// <remarks>
    /// Message type identification enables polymorphic message handling:
    /// - Consumers can implement different processing logic based on message type
    /// - Useful for implementing command/event pattern variations
    /// - Enables versioning and evolution of message schemas
    /// - Supports content-based routing and filtering
    /// 
    /// Common type identification patterns:
    /// - Fully qualified type names: "MyApp.Commands.CreateOrderCommand"
    /// - Simple type names: "CreateOrder", "OrderCreated"
    /// - Versioned types: "CreateOrder.v2", "OrderCreated.v1.1"
    /// - Category-based types: "command.create-order", "event.order-created"
    /// 
    /// Consistent type naming conventions across applications improve
    /// interoperability and simplify message routing logic.
    /// </remarks>
    public string? MessageType { get; set; }

    /// <summary>
    /// Gets or sets the reply-to queue name for request-response patterns.
    /// </summary>
    /// <value>
    /// The name of the queue where response messages should be sent.
    /// Can be null if response is not expected or handled differently.
    /// </value>
    /// <remarks>
    /// The reply-to property enables asynchronous request-response communication:
    /// - Specifies where response messages should be delivered
    /// - Typically used with correlation ID for response matching
    /// - Can reference temporary queues for private responses
    /// - Enables decoupled RPC-style communication patterns
    /// 
    /// Reply-to patterns are common in:
    /// - Service-to-service communication requiring responses
    /// - Command processing with result notification
    /// - Distributed computing scenarios with result aggregation
    /// - Integration patterns requiring acknowledgment or status updates
    /// 
    /// When using temporary reply queues, ensure proper cleanup to prevent
    /// resource leaks in long-running applications.
    /// </remarks>
    public string? ReplyTo { get; set; }

    /// <summary>
    /// Gets or sets the correlation identifier for message tracking.
    /// </summary>
    /// <value>
    /// A unique identifier for correlating related messages.
    /// Can be null if correlation is not required.
    /// </value>
    /// <remarks>
    /// Correlation IDs enable message tracking and conversation management:
    /// - Link request and response messages in RPC patterns
    /// - Track related messages across multiple processing steps
    /// - Enable distributed tracing and observability
    /// - Support conversation-based processing logic
    /// 
    /// Correlation ID usage patterns:
    /// - UUID/GUID for unique message identification
    /// - Business identifiers like order numbers or customer IDs
    /// - Composite identifiers for complex correlation scenarios
    /// - Hierarchical identifiers for nested conversation tracking
    /// 
    /// Consistent correlation ID strategies across applications improve
    /// debugging capabilities and enable comprehensive audit trails.
    /// </remarks>
    public string? CorrelationId { get; set; }

    /// <summary>
    /// Gets or sets the unique message identifier.
    /// </summary>
    /// <value>
    /// A unique identifier for the message instance.
    /// Can be null if unique identification is not required.
    /// </value>
    /// <remarks>
    /// Message IDs provide unique identification for individual message instances:
    /// - Enable deduplication logic to prevent duplicate processing
    /// - Support audit trails and message tracking systems
    /// - Facilitate debugging and troubleshooting scenarios
    /// - Enable idempotency patterns for reliable message processing
    /// 
    /// Message ID best practices:
    /// - Use globally unique identifiers (UUIDs) for distributed systems
    /// - Include timestamp or sequence information for ordering
    /// - Consider using deterministic IDs for deduplication scenarios
    /// - Ensure ID uniqueness across all publishers in the system
    /// 
    /// Automatic message ID generation can be configured when IDs are not
    /// explicitly provided, ensuring all messages have unique identification.
    /// </remarks>
    public string? MessageId { get; set; }

    /// <summary>
    /// Gets or sets the timestamp when the message was created.
    /// </summary>
    /// <value>
    /// The UTC timestamp of message creation.
    /// <c>null</c> indicates the timestamp will be set automatically during publishing.
    /// </value>
    /// <remarks>
    /// Message timestamps enable time-based processing and analysis:
    /// - Track message age for performance monitoring
    /// - Implement time-based routing and filtering logic
    /// - Support audit and compliance requirements
    /// - Enable temporal analysis of message patterns
    /// 
    /// Timestamp considerations:
    /// - Use UTC time to avoid timezone confusion in distributed systems
    /// - Consider clock synchronization across distributed publishers
    /// - Distinguish between creation time, publish time, and processing time
    /// - Account for network latency in time-sensitive processing
    /// 
    /// Automatic timestamp assignment ensures all messages have timing information
    /// even when not explicitly provided by the publisher.
    /// </remarks>
    public DateTimeOffset? Timestamp { get; set; }

    /// <summary>
    /// Gets or sets the application identifier that published the message.
    /// </summary>
    /// <value>
    /// A string identifying the application or service that published the message.
    /// Can be null if application identification is not required.
    /// </value>
    /// <remarks>
    /// Application identification enables:
    /// - Source tracking for debugging and audit purposes
    /// - Security and authorization based on publisher identity
    /// - Routing decisions based on message origin
    /// - Monitoring and analytics segmented by application
    /// 
    /// Application ID patterns:
    /// - Service names: "order-service", "payment-processor"
    /// - Application instances: "order-service-pod-123"
    /// - Versioned applications: "order-service.v2.1"
    /// - Environment-specific identifiers: "order-service.production"
    /// 
    /// Consistent application identification improves system observability
    /// and enables effective troubleshooting in distributed environments.
    /// </remarks>
    public string? AppId { get; set; }

    /// <summary>
    /// Gets or sets the user identifier associated with the message.
    /// </summary>
    /// <value>
    /// A string identifying the user or principal that initiated the message.
    /// Can be null if user identification is not required.
    /// </value>
    /// <remarks>
    /// User identification supports:
    /// - Security and authorization in multi-tenant systems
    /// - Audit trails linking messages to specific users
    /// - User-based routing and processing logic
    /// - Compliance with data protection and privacy regulations
    /// 
    /// User ID considerations:
    /// - Use consistent user identification across all applications
    /// - Consider privacy implications of including user information
    /// - Implement appropriate access controls for user-specific messages
    /// - Support both authenticated and anonymous user scenarios
    /// 
    /// User identification is particularly important in systems requiring
    /// detailed audit trails or user-specific message processing.
    /// </remarks>
    public string? UserId { get; set; }

    /// <summary>
    /// Gets or sets the content type of the message payload.
    /// </summary>
    /// <value>
    /// A MIME type string describing the format of the message payload.
    /// Common values include "application/json", "application/xml", "text/plain".
    /// Can be null if content type detection is handled elsewhere.
    /// </value>
    /// <remarks>
    /// Content type specification enables:
    /// - Automatic serialization and deserialization selection
    /// - Content-based routing and processing decisions
    /// - Interoperability between different technology stacks
    /// - Validation of message format before processing
    /// 
    /// Standard content types:
    /// - "application/json": JSON-formatted data
    /// - "application/xml": XML-formatted data
    /// - "text/plain": Plain text content
    /// - "application/octet-stream": Binary data
    /// - Custom types: "application/vnd.myapp.command+json"
    /// 
    /// Consistent content type usage improves system interoperability
    /// and enables automatic content handling based on format.
    /// </remarks>
    public string? ContentType { get; set; }

    /// <summary>
    /// Gets or sets the content encoding of the message payload.
    /// </summary>
    /// <value>
    /// A string describing the encoding used for the message payload.
    /// Common values include "utf-8", "gzip", "deflate".
    /// Can be null if no special encoding is applied.
    /// </value>
    /// <remarks>
    /// Content encoding enables:
    /// - Compression for reduced bandwidth usage
    /// - Character encoding specification for text content
    /// - Multiple encoding layers for complex transformations
    /// - Automatic decompression and decoding by consumers
    /// 
    /// Encoding considerations:
    /// - Balance compression benefits against CPU overhead
    /// - Ensure consumers support the specified encoding
    /// - Consider compatibility with monitoring and debugging tools
    /// - Document encoding requirements in system specifications
    /// 
    /// Proper encoding specification is essential for reliable
    /// message processing across diverse technology platforms.
    /// </remarks>
    public string? ContentEncoding { get; set; }

    /// <summary>
    /// Gets or sets additional custom headers for the message.
    /// </summary>
    /// <value>
    /// A dictionary containing custom header key-value pairs.
    /// Can be null if no custom headers are needed.
    /// </value>
    /// <remarks>
    /// Custom headers provide extensibility for application-specific metadata:
    /// - Business context information not covered by standard properties
    /// - Routing hints for complex message distribution scenarios
    /// - Processing instructions for consumer applications
    /// - Integration metadata for cross-system communication
    /// 
    /// Header best practices:
    /// - Use consistent naming conventions across applications
    /// - Keep header values lightweight to minimize overhead
    /// - Avoid sensitive information in headers (consider encryption)
    /// - Document custom headers for interoperability
    /// 
    /// Headers are particularly valuable for implementing custom routing logic,
    /// content-based filtering, and application-specific processing directives.
    /// </remarks>
    public IDictionary<string, object>? Headers { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether publisher confirmation is required.
    /// </summary>
    /// <value>
    /// <c>true</c> if the publisher should wait for broker confirmation before considering the message delivered;
    /// <c>false</c> for fire-and-forget publishing. Default is <c>true</c> for reliability.
    /// </value>
    /// <remarks>
    /// Publisher confirmation provides delivery guarantees:
    /// - Confirms that the broker has accepted responsibility for the message
    /// - Enables detection of publishing failures for retry logic
    /// - Provides stronger guarantees than fire-and-forget publishing
    /// - Adds latency overhead due to confirmation round-trip
    /// 
    /// Use confirmation for:
    /// - Critical messages that must be delivered reliably
    /// - Financial transactions and audit-critical operations
    /// - Scenarios requiring guaranteed delivery confirmation
    /// - Systems implementing comprehensive error handling
    /// 
    /// Use fire-and-forget for:
    /// - High-throughput scenarios where speed is critical
    /// - Non-critical notifications and status updates
    /// - Systems with alternative reliability mechanisms
    /// - Performance-sensitive real-time applications
    /// </remarks>
    public bool RequireConfirmation { get; set; } = true;

    /// <summary>
    /// Gets or sets a value indicating whether the message should be returned if it cannot be routed.
    /// </summary>
    /// <value>
    /// <c>true</c> if unroutable messages should be returned to the publisher;
    /// <c>false</c> if unroutable messages should be silently dropped. Default is <c>false</c>.
    /// </value>
    /// <remarks>
    /// Mandatory publishing enables detection of routing failures:
    /// - Publisher receives notification when messages cannot be routed to any queue
    /// - Useful for implementing error handling for routing failures
    /// - Adds complexity but provides stronger delivery guarantees
    /// - Works in conjunction with immediate flag for complete routing validation
    /// 
    /// Use mandatory publishing when:
    /// - Message delivery to specific queues is required
    /// - Routing failures need immediate detection and handling
    /// - System design requires explicit confirmation of message placement
    /// - Debugging routing configuration and topology issues
    /// 
    /// Note: Mandatory publishing adds overhead and complexity to error handling.
    /// Consider using it primarily during development and critical production scenarios.
    /// </remarks>
    public bool Mandatory { get; set; } = false;

    /// <summary>
    /// Gets or sets a value indicating whether the message should be returned immediately if no consumers are available.
    /// </summary>
    /// <value>
    /// <c>true</c> if messages should be returned when no immediate consumers are available;
    /// <c>false</c> if messages should be queued normally. Default is <c>false</c>.
    /// </value>
    /// <remarks>
    /// Immediate publishing provides real-time delivery validation:
    /// - Messages are returned if no consumers are currently attached to target queues
    /// - Enables detection of consumer availability issues
    /// - Useful for implementing synchronous-style messaging patterns
    /// - Adds complexity but provides immediate feedback on delivery capability
    /// 
    /// Use immediate publishing for:
    /// - Real-time systems requiring immediate processing confirmation
    /// - Load balancing scenarios where alternative routing is available
    /// - Systems implementing dynamic consumer management
    /// - Debugging consumer attachment and availability issues
    /// 
    /// Note: Immediate publishing is deprecated in newer RabbitMQ versions
    /// and may not be supported in all configurations. Consider alternative
    /// approaches for achieving similar functionality.
    /// </remarks>
    public bool Immediate { get; set; } = false;

    /// <summary>
    /// Creates a new PublishOptions instance with default values optimized for reliable messaging.
    /// </summary>
    /// <returns>A new PublishOptions instance with production-ready defaults.</returns>
    /// <remarks>
    /// Default configuration prioritizes reliability and consistency:
    /// - Persistent messages for durability
    /// - Publisher confirmation enabled for delivery guarantees
    /// - Standard mandatory and immediate settings for balanced behavior
    /// 
    /// This configuration is suitable for most production scenarios where
    /// message reliability is important and moderate performance overhead is acceptable.
    /// </remarks>
    public static PublishOptions CreateReliable()
    {
        return new PublishOptions
        {
            Persistent = true,
            RequireConfirmation = true,
            Mandatory = false,
            Immediate = false
        };
    }

    /// <summary>
    /// Creates a new PublishOptions instance optimized for high-performance scenarios.
    /// </summary>
    /// <returns>A new PublishOptions instance with performance-optimized defaults.</returns>
    /// <remarks>
    /// Performance configuration prioritizes speed and throughput:
    /// - Non-persistent messages for reduced I/O overhead
    /// - No publisher confirmation for minimal latency
    /// - Standard mandatory and immediate settings
    /// 
    /// This configuration is suitable for high-throughput scenarios where
    /// message loss tolerance is acceptable in exchange for improved performance.
    /// Consider the implications carefully before using in production systems.
    /// </remarks>
    public static PublishOptions CreateFast()
    {
        return new PublishOptions
        {
            Persistent = false,
            RequireConfirmation = false,
            Mandatory = false,
            Immediate = false
        };
    }

    /// <summary>
    /// Creates a new PublishOptions instance for request-response messaging patterns.
    /// </summary>
    /// <param name="replyTo">The queue name where responses should be sent.</param>
    /// <param name="correlationId">The correlation identifier for matching responses to requests.</param>
    /// <returns>A new PublishOptions instance configured for RPC-style messaging.</returns>
    /// <exception cref="ArgumentException">Thrown when <paramref name="replyTo"/> is null, empty, or whitespace.</exception>
    /// <remarks>
    /// RPC configuration includes:
    /// - Reply-to queue specification for response routing
    /// - Correlation ID for request-response matching
    /// - Reliable delivery settings for request integrity
    /// - Timestamp for request timing and timeout handling
    /// 
    /// This configuration simplifies implementation of request-response patterns
    /// by providing appropriate defaults for RPC-style communication.
    /// </remarks>
    public static PublishOptions CreateForRpc(string replyTo, string? correlationId = null)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(replyTo);

        return new PublishOptions
        {
            ReplyTo = replyTo,
            CorrelationId = correlationId ?? Guid.NewGuid().ToString(),
            Persistent = true,
            RequireConfirmation = true,
            Timestamp = DateTimeOffset.UtcNow
        };
    }
}