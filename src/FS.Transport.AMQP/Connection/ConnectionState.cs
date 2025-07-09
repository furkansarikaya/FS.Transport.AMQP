namespace FS.Transport.AMQP.Connection;

/// <summary>
/// Enumeration of connection states
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
    /// Connection is established and healthy
    /// </summary>
    Connected,
    
    /// <summary>
    /// Connection is temporarily disconnected
    /// </summary>
    Disconnected,
    
    /// <summary>
    /// Connection is being recovered
    /// </summary>
    Recovering,
    
    /// <summary>
    /// Connection is permanently closed
    /// </summary>
    Closed,
    
    /// <summary>
    /// Connection is in a failed state
    /// </summary>
    Failed
}