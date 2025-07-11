namespace FS.RabbitMQ.Connection;

/// <summary>
/// Event arguments for connection-related events
/// </summary>
public class ConnectionEventArgs : EventArgs
{
    /// <summary>
    /// Description of the connection event
    /// </summary>
    public string Message { get; }
    
    /// <summary>
    /// Exception associated with the event (if any)
    /// </summary>
    public Exception? Exception { get; }
    
    /// <summary>
    /// Timestamp when the event occurred
    /// </summary>
    public DateTime Timestamp { get; }
    
    /// <summary>
    /// Additional context information
    /// </summary>
    public IDictionary<string, object> Context { get; }

    public ConnectionEventArgs(string message, Exception? exception = null)
    {
        Message = message ?? throw new ArgumentNullException(nameof(message));
        Exception = exception;
        Timestamp = DateTime.UtcNow;
        Context = new Dictionary<string, object>();
    }

    /// <summary>
    /// Adds context information to the event
    /// </summary>
    /// <param name="key">Context key</param>
    /// <param name="value">Context value</param>
    /// <returns>The event args instance for fluent configuration</returns>
    public ConnectionEventArgs WithContext(string key, object value)
    {
        Context[key] = value;
        return this;
    }

    /// <summary>
    /// Gets a string representation of the connection event
    /// </summary>
    /// <returns>Event description</returns>
    public override string ToString()
    {
        var result = $"[{Timestamp:yyyy-MM-dd HH:mm:ss}] {Message}";
        if (Exception != null)
        {
            result += $" - Exception: {Exception.Message}";
        }
        return result;
    }
}