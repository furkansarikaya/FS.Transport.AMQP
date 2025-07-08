namespace FS.RabbitMQ.Core.Abstractions;

/// <summary>
/// Base interface for all message consumers providing common functionality and lifecycle management.
/// </summary>
/// <remarks>
/// This base interface defines the fundamental operations that all consumer types must support,
/// including lifecycle management, state monitoring, and resource cleanup. It provides a
/// consistent foundation for different consumer patterns while allowing specialized behavior
/// in derived interfaces.
/// 
/// All consumers support:
/// - Asynchronous start/stop operations for controlled lifecycle management
/// - State monitoring for operational visibility and health checking
/// - Proper resource disposal for memory and connection management
/// - Event-driven notifications for monitoring and integration scenarios
/// </remarks>
public interface IMessageConsumer : IAsyncDisposable, IDisposable
{
    /// <summary>
    /// Gets the unique identifier for this consumer instance.
    /// </summary>
    /// <value>A unique identifier that can be used for logging, monitoring, and correlation purposes.</value>
    /// <remarks>
    /// Consumer IDs enable:
    /// - Tracking individual consumer instances in multi-consumer scenarios
    /// - Correlation of log entries and metrics across distributed systems
    /// - Debugging and troubleshooting of consumer-specific issues
    /// - Integration with monitoring and observability platforms
    /// </remarks>
    string ConsumerId { get; }

    /// <summary>
    /// Gets the name of the queue this consumer is bound to.
    /// </summary>
    /// <value>The queue name that this consumer processes messages from.</value>
    /// <remarks>
    /// Queue name information enables:
    /// - Verification of consumer configuration and routing
    /// - Monitoring and metrics segmentation by queue
    /// - Dynamic consumer management and orchestration
    /// - Audit trails and compliance reporting
    /// </remarks>
    string QueueName { get; }

    /// <summary>
    /// Gets the current state of the consumer.
    /// </summary>
    /// <value>The current operational state indicating the consumer's lifecycle phase.</value>
    /// <remarks>
    /// Consumer state provides visibility into operational status:
    /// - Stopped: Consumer is not active and not processing messages
    /// - Starting: Consumer is in the process of initialization and setup
    /// - Running: Consumer is actively processing messages
    /// - Stopping: Consumer is gracefully shutting down
    /// - Faulted: Consumer encountered an error and requires intervention
    /// 
    /// Applications can use state information for health monitoring,
    /// automatic recovery, and operational decision making.
    /// </remarks>
    ConsumerState State { get; }

    /// <summary>
    /// Gets a value indicating whether the consumer is currently active and processing messages.
    /// </summary>
    /// <value><c>true</c> if the consumer is in the Running state; otherwise, <c>false</c>.</value>
    /// <remarks>
    /// Active status provides a convenient boolean check for operational readiness,
    /// commonly used in health checks, monitoring systems, and conditional logic
    /// that depends on consumer availability.
    /// </remarks>
    bool IsActive { get; }

    /// <summary>
    /// Starts the consumer and begins message processing.
    /// </summary>
    /// <param name="cancellationToken">Token to monitor for cancellation requests during startup.</param>
    /// <returns>A task that represents the asynchronous start operation.</returns>
    /// <exception cref="InvalidOperationException">Thrown when the consumer is not in a valid state for starting.</exception>
    /// <exception cref="ConsumerException">Thrown when the consumer cannot be started due to configuration or connectivity issues.</exception>
    /// <remarks>
    /// Starting a consumer involves:
    /// - Establishing connections to the message broker
    /// - Configuring channel settings and QoS parameters
    /// - Registering message handlers and error recovery mechanisms
    /// - Transitioning to the Running state for active message processing
    /// 
    /// The operation is idempotent - calling start on an already running consumer
    /// has no effect and returns successfully.
    /// </remarks>
    Task StartAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Stops the consumer and ceases message processing gracefully.
    /// </summary>
    /// <param name="cancellationToken">Token to monitor for cancellation requests during shutdown.</param>
    /// <returns>A task that represents the asynchronous stop operation.</returns>
    /// <exception cref="ConsumerException">Thrown when the consumer cannot be stopped cleanly.</exception>
    /// <remarks>
    /// Stopping a consumer involves:
    /// - Completing processing of any in-flight messages
    /// - Gracefully closing connections and releasing resources
    /// - Transitioning to the Stopped state
    /// - Ensuring no message loss during the shutdown process
    /// 
    /// The operation is idempotent - calling stop on an already stopped consumer
    /// has no effect and returns successfully.
    /// </remarks>
    Task StopAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Event raised when the consumer state changes.
    /// </summary>
    /// <remarks>
    /// State change events enable:
    /// - Real-time monitoring of consumer lifecycle
    /// - Implementation of consumer-aware application logic
    /// - Integration with external monitoring and alerting systems
    /// - Automated recovery and failover scenarios
    /// 
    /// Event handlers should be prepared for rapid state changes during
    /// startup, shutdown, and error recovery scenarios.
    /// </remarks>
    event EventHandler<ConsumerStateChangedEventArgs> StateChanged;

    /// <summary>
    /// Event raised when an error occurs during message processing.
    /// </summary>
    /// <remarks>
    /// Error events capture exceptions that occur during message processing
    /// and provide comprehensive error information for logging, monitoring,
    /// and recovery decision making. Error events are raised for both
    /// recoverable and non-recoverable error conditions.
    /// </remarks>
    event EventHandler<ConsumerErrorEventArgs> ErrorOccurred;
}