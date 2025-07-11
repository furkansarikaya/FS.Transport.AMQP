namespace FS.RabbitMQ.Exchange;

/// <summary>
/// Event arguments for exchange-related events
/// </summary>
public class ExchangeEventArgs : EventArgs
{
    /// <summary>
    /// Exchange name
    /// </summary>
    public string ExchangeName { get; }
    
    /// <summary>
    /// Operation that was performed
    /// </summary>
    public ExchangeOperation Operation { get; }
    
    /// <summary>
    /// Whether the operation was successful
    /// </summary>
    public bool Success { get; }
    
    /// <summary>
    /// Optional error message
    /// </summary>
    public string? ErrorMessage { get; }
    
    /// <summary>
    /// Timestamp of the event
    /// </summary>
    public DateTime Timestamp { get; }
    
    /// <summary>
    /// Additional context information
    /// </summary>
    public IDictionary<string, object> Context { get; }

    public ExchangeEventArgs(string exchangeName, ExchangeOperation operation, bool success, string? errorMessage = null)
    {
        ExchangeName = exchangeName ?? throw new ArgumentNullException(nameof(exchangeName));
        Operation = operation;
        Success = success;
        ErrorMessage = errorMessage;
        Timestamp = DateTime.UtcNow;
        Context = new Dictionary<string, object>();
    }

    /// <summary>
    /// Adds context information to the event
    /// </summary>
    /// <param name="key">Context key</param>
    /// <param name="value">Context value</param>
    /// <returns>Event args for fluent configuration</returns>
    public ExchangeEventArgs WithContext(string key, object value)
    {
        Context[key] = value;
        return this;
    }

    /// <summary>
    /// Gets a string representation of the exchange event
    /// </summary>
    /// <returns>Event description</returns>
    public override string ToString()
    {
        var status = Success ? "Success" : "Failed";
        var error = !string.IsNullOrEmpty(ErrorMessage) ? $" - {ErrorMessage}" : "";
        return $"[{Timestamp:yyyy-MM-dd HH:mm:ss}] Exchange '{ExchangeName}' {Operation}: {status}{error}";
    }
}