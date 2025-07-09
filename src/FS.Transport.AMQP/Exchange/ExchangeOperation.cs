namespace FS.Transport.AMQP.Exchange;

/// <summary>
/// Exchange operations that can trigger events
/// </summary>
public enum ExchangeOperation
{
    /// <summary>
    /// Exchange declaration operation
    /// </summary>
    Declare,
    
    /// <summary>
    /// Exchange deletion operation
    /// </summary>
    Delete,
    
    /// <summary>
    /// Exchange binding operation
    /// </summary>
    Bind,
    
    /// <summary>
    /// Exchange unbinding operation
    /// </summary>
    Unbind,
    
    /// <summary>
    /// Exchange redeclaration (auto-recovery)
    /// </summary>
    Redeclare
}