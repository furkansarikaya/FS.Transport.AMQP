namespace FS.Transport.AMQP.EventBus;

/// <summary>
/// Event bus status enumeration
/// </summary>
public enum EventBusStatus
{
    /// <summary>
    /// Event bus is not initialized
    /// </summary>
    NotInitialized = 0,
    
    /// <summary>
    /// Event bus is starting up
    /// </summary>
    Starting = 1,
    
    /// <summary>
    /// Event bus is running and operational
    /// </summary>
    Running = 2,
    
    /// <summary>
    /// Event bus is stopping
    /// </summary>
    Stopping = 3,
    
    /// <summary>
    /// Event bus is stopped
    /// </summary>
    Stopped = 4,
    
    /// <summary>
    /// Event bus has encountered an error
    /// </summary>
    Faulted = 5
} 