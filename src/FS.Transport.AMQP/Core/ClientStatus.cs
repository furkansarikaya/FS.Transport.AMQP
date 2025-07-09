namespace FS.Transport.AMQP.Core;

/// <summary>
/// Enumeration of client status values
/// </summary>
public enum ClientStatus
{
    /// <summary>
    /// Client is not yet initialized
    /// </summary>
    NotInitialized,
    
    /// <summary>
    /// Client is currently initializing
    /// </summary>
    Initializing,
    
    /// <summary>
    /// Client is ready and operational
    /// </summary>
    Ready,
    
    /// <summary>
    /// Client is disconnected but may reconnect
    /// </summary>
    Disconnected,
    
    /// <summary>
    /// Client is attempting to reconnect
    /// </summary>
    Reconnecting,
    
    /// <summary>
    /// Client is shutting down
    /// </summary>
    ShuttingDown,
    
    /// <summary>
    /// Client has been shut down
    /// </summary>
    Shutdown,
    
    /// <summary>
    /// Client is in a failed state
    /// </summary>
    Failed
}