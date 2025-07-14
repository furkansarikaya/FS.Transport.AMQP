namespace FS.StreamFlow.Core.Features.Messaging.Models;

/// <summary>
/// Represents the state of a messaging connection
/// </summary>
public enum ConnectionState
{
    /// <summary>
    /// Connection is not initialized
    /// </summary>
    NotInitialized,
    
    /// <summary>
    /// Connection is being established
    /// </summary>
    Connecting,
    
    /// <summary>
    /// Connection is established and ready
    /// </summary>
    Connected,
    
    /// <summary>
    /// Connection is being disconnected
    /// </summary>
    Disconnecting,
    
    /// <summary>
    /// Connection is disconnected
    /// </summary>
    Disconnected,
    
    /// <summary>
    /// Connection is being recovered
    /// </summary>
    Recovering,
    
    /// <summary>
    /// Connection has failed
    /// </summary>
    Failed,
    
    /// <summary>
    /// Connection is in an error state
    /// </summary>
    Error
} 