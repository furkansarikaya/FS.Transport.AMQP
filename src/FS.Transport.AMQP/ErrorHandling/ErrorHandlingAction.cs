namespace FS.Transport.AMQP.ErrorHandling;

/// <summary>
/// Actions taken during error handling
/// </summary>
public enum ErrorHandlingAction
{
    /// <summary>
    /// No action taken
    /// </summary>
    None,
    
    /// <summary>
    /// Message was acknowledged and discarded
    /// </summary>
    Acknowledged,
    
    /// <summary>
    /// Message was requeued for later processing
    /// </summary>
    Requeued,
    
    /// <summary>
    /// Operation was retried
    /// </summary>
    Retried,
    
    /// <summary>
    /// Message was sent to dead letter exchange
    /// </summary>
    SentToDeadLetter,
    
    /// <summary>
    /// Error handling failed
    /// </summary>
    Failed
}
