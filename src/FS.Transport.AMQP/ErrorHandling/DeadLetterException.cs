namespace FS.Transport.AMQP.ErrorHandling;

/// <summary>
/// Contains information about an exception that caused a message to be sent to dead letter queue
/// </summary>
public class DeadLetterException
{
    /// <summary>
    /// Gets or sets the type of the exception that occurred
    /// </summary>
    /// <value>The exception type name</value>
    public string Type { get; set; } = string.Empty;
    
    /// <summary>
    /// Gets or sets the exception message
    /// </summary>
    /// <value>The exception message</value>
    public string Message { get; set; } = string.Empty;
    
    /// <summary>
    /// Gets or sets the exception stack trace
    /// </summary>
    /// <value>The exception stack trace</value>
    public string StackTrace { get; set; } = string.Empty;
    
    /// <summary>
    /// Gets or sets the exception source
    /// </summary>
    /// <value>The exception source</value>
    public string Source { get; set; } = string.Empty;
}