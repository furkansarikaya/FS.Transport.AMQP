namespace FS.RabbitMQ.Connection;

/// <summary>
/// Represents the state of a RabbitMQ connection
/// </summary>
public enum ConnectionState
{
    /// <summary>
    /// Connection has not been initialized
    /// </summary>
    NotInitialized,

    /// <summary>
    /// Connection is being established
    /// </summary>
    Connecting,

    /// <summary>
    /// Connection is active and ready for use
    /// </summary>
    Connected,

    /// <summary>
    /// Connection is being closed
    /// </summary>
    Disconnecting,

    /// <summary>
    /// Connection has been closed
    /// </summary>
    Disconnected,

    /// <summary>
    /// Connection is being recovered
    /// </summary>
    Recovering,

    /// <summary>
    /// Connection has failed
    /// </summary>
    Failed
}