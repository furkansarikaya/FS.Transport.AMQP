namespace FS.RabbitMQ.Core.Abstractions;

/// <summary>
/// Represents the current state of the message broker connection.
/// </summary>
/// <remarks>
/// Connection state transitions follow a specific lifecycle pattern:
/// Disconnected -> Connecting -> Connected -> (Disconnecting -> Disconnected | Reconnecting -> Connected)
/// 
/// This enumeration provides fine-grained visibility into connection lifecycle for monitoring,
/// logging, and implementing connection-aware application logic.
/// </remarks>
public enum ConnectionState
{
    /// <summary>
    /// The broker is not connected and no connection attempts are in progress.
    /// This is the initial state and the final state after a graceful disconnection.
    /// </summary>
    Disconnected = 0,

    /// <summary>
    /// A connection attempt is currently in progress.
    /// This state indicates the initial connection establishment phase.
    /// </summary>
    Connecting = 1,

    /// <summary>
    /// The broker is successfully connected and ready to handle messaging operations.
    /// All channels are operational and message publishing/consuming can proceed normally.
    /// </summary>
    Connected = 2,

    /// <summary>
    /// The broker is in the process of gracefully disconnecting.
    /// During this state, new operations may be rejected while existing operations complete.
    /// </summary>
    Disconnecting = 3,

    /// <summary>
    /// The broker is attempting to reconnect after an unexpected connection loss.
    /// This state indicates automatic recovery mechanisms are active.
    /// Message operations may be queued or temporarily suspended during this state.
    /// </summary>
    Reconnecting = 4,

    /// <summary>
    /// The connection is in an error state and cannot be recovered automatically.
    /// Manual intervention or configuration changes may be required to restore connectivity.
    /// </summary>
    Faulted = 5
}