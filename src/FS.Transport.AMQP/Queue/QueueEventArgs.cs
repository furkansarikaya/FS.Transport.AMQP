namespace FS.Transport.AMQP.Queue;

/// <summary>
/// Event arguments for queue-related events
/// </summary>
public class QueueEventArgs : EventArgs
{
    /// <summary>
    /// Queue name
    /// </summary>
    public string QueueName { get; }
    
    /// <summary>
    /// Operation that was performed
    /// </summary>
    public QueueOperation Operation { get; }
    
    /// <summary>
    /// Whether the operation was successful
    /// </summary>
    public bool Success { get; }
    
    /// <summary>
    /// Queue declaration result (if applicable)
    /// </summary>
    public QueueDeclareResult? Result { get; }
    
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

    public QueueEventArgs(string queueName, QueueOperation operation, bool success, QueueDeclareResult? result = null, string? errorMessage = null)
    {
        QueueName = queueName ?? throw new ArgumentNullException(nameof(queueName));
        Operation = operation;
        Success = success;
        Result = result;
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
    public QueueEventArgs WithContext(string key, object value)
    {
        Context[key] = value;
        return this;
    }

    /// <summary>
    /// Gets a string representation of the queue event
    /// </summary>
    /// <returns>Event description</returns>
    public override string ToString()
    {
        var status = Success ? "Success" : "Failed";
        var error = !string.IsNullOrEmpty(ErrorMessage) ? $" - {ErrorMessage}" : "";
        var result = Result != null ? $" ({Result.MessageCount} msgs, {Result.ConsumerCount} consumers)" : "";
        return $"[{Timestamp:yyyy-MM-dd HH:mm:ss}] Queue '{QueueName}' {Operation}: {status}{result}{error}";
    }
}