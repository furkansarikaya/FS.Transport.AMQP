namespace FS.RabbitMQ.Core.Abstractions;

/// <summary>
/// Defines the contract for custom message processing strategies used by custom consumers.
/// </summary>
/// <typeparam name="T">The type of message payload being processed.</typeparam>
/// <remarks>
/// Message processing strategies provide a plugin architecture for implementing
/// sophisticated message processing patterns that go beyond the standard consumer
/// types. Strategies encapsulate the complete message processing pipeline including
/// message handling, acknowledgment, error recovery, and resource management.
/// 
/// Custom strategies enable:
/// - Integration with external processing frameworks and libraries
/// - Implementation of domain-specific processing patterns
/// - Advanced message routing and transformation logic
/// - Custom error handling and recovery mechanisms
/// - Specialized performance optimization and resource management
/// 
/// Strategy implementations should be designed to be stateless and thread-safe
/// to support concurrent message processing and dynamic strategy updates.
/// </remarks>
public interface IMessageProcessingStrategy<T> where T : class
{
    /// <summary>
    /// Gets the unique identifier for this processing strategy.
    /// </summary>
    /// <value>A unique name or identifier for the processing strategy.</value>
    /// <remarks>
    /// Strategy identifiers enable:
    /// - Strategy registration and discovery in plugin systems
    /// - Monitoring and metrics segmentation by strategy type
    /// - Dynamic strategy selection and configuration
    /// - Debugging and troubleshooting of processing behavior
    /// 
    /// Identifiers should be unique within the application domain
    /// and descriptive of the strategy's purpose and behavior.
    /// </remarks>
    string StrategyId { get; }

    /// <summary>
    /// Gets the supported message types for this processing strategy.
    /// </summary>
    /// <value>A collection of types that this strategy can process.</value>
    /// <remarks>
    /// Type support information enables:
    /// - Runtime validation of strategy compatibility with message types
    /// - Dynamic strategy selection based on message characteristics
    /// - Prevention of mismatched strategy and message type combinations
    /// - Support for polymorphic processing patterns
    /// 
    /// Strategies can support specific types, base classes, or interfaces
    /// to provide flexible message processing capabilities.
    /// </remarks>
    IEnumerable<Type> SupportedMessageTypes { get; }

    /// <summary>
    /// Initializes the processing strategy with the provided configuration.
    /// </summary>
    /// <param name="configuration">The strategy-specific configuration options.</param>
    /// <param name="cancellationToken">Token to monitor for cancellation requests.</param>
    /// <returns>A task that represents the asynchronous initialization operation.</returns>
    /// <exception cref="StrategyConfigurationException">Thrown when the configuration is invalid or incomplete.</exception>
    /// <remarks>
    /// Initialization provides:
    /// - Setup of strategy-specific resources and connections
    /// - Validation of configuration parameters and dependencies
    /// - Preparation of processing infrastructure and state
    /// - Integration with external systems and frameworks
    /// 
    /// Strategies should validate all required configuration during initialization
    /// and fail fast if the configuration is invalid or required resources are unavailable.
    /// </remarks>
    Task InitializeAsync(IReadOnlyDictionary<string, object>? configuration, CancellationToken cancellationToken = default);

    /// <summary>
    /// Processes a message using the custom strategy logic.
    /// </summary>
    /// <param name="context">The message context containing the message and processing information.</param>
    /// <param name="cancellationToken">Token to monitor for cancellation requests.</param>
    /// <returns>A task that represents the asynchronous processing operation with the processing result.</returns>
    /// <exception cref="MessageProcessingException">Thrown when message processing fails due to strategy-specific issues.</exception>
    /// <remarks>
    /// Message processing encapsulates:
    /// - Custom business logic and message transformation
    /// - Integration with external systems and services
    /// - Error handling and recovery mechanisms
    /// - Performance monitoring and metrics collection
    /// 
    /// Strategies are responsible for all aspects of message processing
    /// including acknowledgment decisions, error handling, and resource cleanup.
    /// The result indicates the outcome and any recommended follow-up actions.
    /// </remarks>
    Task<ProcessingResult> ProcessMessageAsync(MessageContext<T> context, CancellationToken cancellationToken = default);

    /// <summary>
    /// Handles errors that occur during message processing.
    /// </summary>
    /// <param name="context">The message context for the failed message.</param>
    /// <param name="exception">The exception that occurred during processing.</param>
    /// <param name="cancellationToken">Token to monitor for cancellation requests.</param>
    /// <returns>A task that represents the asynchronous error handling operation with the handling result.</returns>
    /// <remarks>
    /// Error handling provides:
    /// - Strategy-specific error analysis and classification
    /// - Custom retry and recovery logic implementation
    /// - Integration with external error handling systems
    /// - Specialized dead letter and poison message handling
    /// 
    /// Error handlers can implement sophisticated retry strategies,
    /// custom escalation procedures, and integration with external
    /// error tracking and analysis systems.
    /// </remarks>
    Task<ErrorHandlingResult> HandleErrorAsync(MessageContext<T> context, Exception exception, CancellationToken cancellationToken = default);

    /// <summary>
    /// Determines whether the strategy can process the specified message type.
    /// </summary>
    /// <param name="messageType">The message type to check for compatibility.</param>
    /// <returns><c>true</c> if the strategy can process the specified type; otherwise, <c>false</c>.</returns>
    /// <remarks>
    /// Type compatibility checking enables:
    /// - Runtime validation of strategy and message type combinations
    /// - Dynamic strategy selection based on message characteristics
    /// - Prevention of runtime type errors and processing failures
    /// - Support for polymorphic and generic processing patterns
    /// 
    /// Implementations should check for exact type matches, base class
    /// compatibility, and interface implementations as appropriate.
    /// </remarks>
    bool CanProcessMessageType(Type messageType);

    /// <summary>
    /// Releases resources and performs cleanup when the strategy is no longer needed.
    /// </summary>
    /// <param name="cancellationToken">Token to monitor for cancellation requests.</param>
    /// <returns>A task that represents the asynchronous cleanup operation.</returns>
    /// <remarks>
    /// Cleanup operations include:
    /// - Releasing external connections and resources
    /// - Completing in-flight operations and transactions
    /// - Saving state and configuration changes
    /// - Notifying external systems of strategy shutdown
    /// 
    /// Strategies should implement graceful shutdown to ensure
    /// data integrity and proper resource cleanup.
    /// </remarks>
    Task CleanupAsync(CancellationToken cancellationToken = default);
}