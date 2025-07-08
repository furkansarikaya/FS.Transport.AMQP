namespace FS.RabbitMQ.Core.Abstractions;

/// <summary>
/// Represents the operational state of a message consumer.
/// </summary>
/// <remarks>
/// Consumer states follow a specific lifecycle pattern that provides visibility
/// into operational status and enables appropriate management actions. State
/// transitions are designed to be predictable and support both manual and
/// automated consumer lifecycle management.
/// </remarks>
public enum ConsumerState
{
    /// <summary>
    /// The consumer is not active and is not processing messages.
    /// This is the initial state when a consumer is created and the final state after stopping.
    /// </summary>
    /// <remarks>
    /// In the Stopped state:
    /// - No message processing is occurring
    /// - Consumer resources are minimal or deallocated
    /// - Consumer can be started to begin message processing
    /// - Configuration changes can be safely applied
    /// </remarks>
    Stopped = 0,

    /// <summary>
    /// The consumer is in the process of starting up and initializing.
    /// Temporary state during consumer activation.
    /// </summary>
    /// <remarks>
    /// In the Starting state:
    /// - Consumer is establishing connections and initializing resources
    /// - No message processing is occurring yet
    /// - State will transition to Running upon successful startup
    /// - State may transition to Faulted if startup fails
    /// </remarks>
    Starting = 1,

    /// <summary>
    /// The consumer is active and processing messages normally.
    /// This is the primary operational state for message consumption.
    /// </summary>
    /// <remarks>
    /// In the Running state:
    /// - Consumer is actively receiving and processing messages
    /// - All consumer features and capabilities are available
    /// - Consumer can be stopped or paused from this state
    /// - Performance monitoring and metrics are actively collected
    /// </remarks>
    Running = 2,

    /// <summary>
    /// The consumer is in the process of stopping and completing final operations.
    /// Temporary state during graceful shutdown.
    /// </summary>
    /// <remarks>
    /// In the Stopping state:
    /// - Consumer is completing in-flight message processing
    /// - No new messages are being accepted for processing
    /// - Consumer is releasing resources and closing connections
    /// - State will transition to Stopped upon completion
    /// </remarks>
    Stopping = 3,

    /// <summary>
    /// The consumer has temporarily suspended message processing while maintaining connections.
    /// Used for temporary suspension without full shutdown overhead.
    /// </summary>
    /// <remarks>
    /// In the Paused state:
    /// - Message processing is suspended but connections remain active
    /// - Messages continue to accumulate in the queue
    /// - Consumer can be quickly resumed to Running state
    /// - Useful for maintenance windows and backpressure scenarios
    /// </remarks>
    Paused = 4,

    /// <summary>
    /// The consumer is in an error state and requires intervention.
    /// Indicates that automatic recovery has failed or is not possible.
    /// </summary>
    /// <remarks>
    /// In the Faulted state:
    /// - Consumer has encountered an unrecoverable error condition
    /// - Message processing has stopped due to the error
    /// - Manual intervention or configuration changes may be required
    /// - Consumer typically needs to be stopped and restarted
    /// 
    /// Common causes of faulted state:
    /// - Persistent connection failures beyond retry limits
    /// - Configuration errors preventing proper operation
    /// - Resource exhaustion or system-level failures
    /// - Authentication or authorization failures
    /// </remarks>
    Faulted = 5
}