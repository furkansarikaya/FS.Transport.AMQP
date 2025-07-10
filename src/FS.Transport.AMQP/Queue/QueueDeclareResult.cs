namespace FS.Transport.AMQP.Queue;

/// <summary>
/// Result of queue declaration operation
/// </summary>
public class QueueDeclareResult
{
    /// <summary>
    /// Queue name
    /// </summary>
    public string QueueName { get; set; } = string.Empty;
    
    /// <summary>
    /// Number of messages in the queue
    /// </summary>
    public uint MessageCount { get; set; }
    
    /// <summary>
    /// Number of consumers connected to the queue
    /// </summary>
    public uint ConsumerCount { get; set; }
    
    /// <summary>
    /// Whether the queue was newly created or already existed
    /// </summary>
    public bool WasCreated { get; set; }

    /// <summary>
    /// Gets a string representation of the declaration result
    /// </summary>
    /// <returns>Result description</returns>
    public override string ToString()
    {
        var status = WasCreated ? "Created" : "Existed";
        return $"Queue '{QueueName}' {status} - Messages: {MessageCount}, Consumers: {ConsumerCount}";
    }
}