namespace FS.RabbitMQ.EventHandlers;

/// <summary>
/// Detailed information about a registered handler
/// </summary>
public class HandlerDetail
{
    /// <summary>
    /// Handler name
    /// </summary>
    public string Name { get; set; } = string.Empty;
    
    /// <summary>
    /// Event types this handler processes
    /// </summary>
    public List<string> EventTypes { get; set; } = new();
    
    /// <summary>
    /// Handler priority
    /// </summary>
    public int Priority { get; set; }
    
    /// <summary>
    /// Whether handler allows concurrent execution
    /// </summary>
    public bool AllowConcurrentExecution { get; set; }

    /// <summary>
    /// Gets a string representation of the handler detail
    /// </summary>
    /// <returns>Handler description</returns>
    public override string ToString()
    {
        return $"{Name} (Priority: {Priority}, Events: {string.Join(", ", EventTypes)}, Concurrent: {AllowConcurrentExecution})";
    }
}