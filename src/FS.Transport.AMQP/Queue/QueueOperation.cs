namespace FS.Transport.AMQP.Queue;

/// <summary>
/// Queue operations that can trigger events
/// </summary>
public enum QueueOperation
{
    /// <summary>
    /// Queue declaration operation
    /// </summary>
    Declare,
    
    /// <summary>
    /// Queue deletion operation
    /// </summary>
    Delete,
    
    /// <summary>
    /// Queue binding operation
    /// </summary>
    Bind,
    
    /// <summary>
    /// Queue unbinding operation
    /// </summary>
    Unbind,
    
    /// <summary>
    /// Queue purge operation
    /// </summary>
    Purge,
    
    /// <summary>
    /// Queue redeclaration (auto-recovery)
    /// </summary>
    Redeclare
}