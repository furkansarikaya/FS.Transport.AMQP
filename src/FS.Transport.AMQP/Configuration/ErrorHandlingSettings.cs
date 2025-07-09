namespace FS.Transport.AMQP.Configuration;

/// <summary>
/// Settings for error handling and dead letter queue behavior
/// </summary>
public class ErrorHandlingSettings
{
    /// <summary>
    /// Error handling strategy (Retry, DeadLetter, Ignore, Custom)
    /// </summary>
    public string Strategy { get; set; } = "DeadLetter";
    
    /// <summary>
    /// Dead letter exchange name
    /// </summary>
    public string DeadLetterExchange { get; set; } = "dlx";
    
    /// <summary>
    /// Dead letter queue name
    /// </summary>
    public string DeadLetterQueue { get; set; } = "dlq";
    
    /// <summary>
    /// Dead letter routing key
    /// </summary>
    public string DeadLetterRoutingKey { get; set; } = "dead-letter";
    
    /// <summary>
    /// Whether to include original message headers in dead letter
    /// </summary>
    public bool IncludeOriginalHeaders { get; set; } = true;
    
    /// <summary>
    /// Whether to include error details in dead letter headers
    /// </summary>
    public bool IncludeErrorDetails { get; set; } = true;
    
    /// <summary>
    /// Maximum error message length to include in headers
    /// </summary>
    public int MaxErrorMessageLength { get; set; } = 1000;
    
    /// <summary>
    /// Whether to log errors
    /// </summary>
    public bool LogErrors { get; set; } = true;
    
    /// <summary>
    /// Error log level (Debug, Information, Warning, Error, Critical)
    /// </summary>
    public string ErrorLogLevel { get; set; } = "Error";

    /// <summary>
    /// Validates error handling settings
    /// </summary>
    public void Validate()
    {
        var validStrategies = new[] { "Retry", "DeadLetter", "Ignore", "Custom" };
        if (!validStrategies.Contains(Strategy))
            throw new ArgumentException($"Error handling strategy must be one of: {string.Join(", ", validStrategies)}");
            
        if (string.IsNullOrWhiteSpace(DeadLetterExchange))
            throw new ArgumentException("DeadLetterExchange cannot be null or empty");
            
        if (string.IsNullOrWhiteSpace(DeadLetterQueue))
            throw new ArgumentException("DeadLetterQueue cannot be null or empty");
            
        if (MaxErrorMessageLength <= 0)
            throw new ArgumentException("MaxErrorMessageLength must be greater than 0");
            
        var validLogLevels = new[] { "Debug", "Information", "Warning", "Error", "Critical" };
        if (!validLogLevels.Contains(ErrorLogLevel))
            throw new ArgumentException($"ErrorLogLevel must be one of: {string.Join(", ", validLogLevels)}");
    }
}