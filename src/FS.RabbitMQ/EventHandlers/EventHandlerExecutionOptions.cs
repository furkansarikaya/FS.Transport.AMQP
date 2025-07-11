namespace FS.RabbitMQ.EventHandlers;

/// <summary>
/// Options for event handler execution
/// </summary>
public class EventHandlerExecutionOptions
{
    /// <summary>
    /// Maximum number of concurrent handler executions
    /// </summary>
    public int MaxConcurrentHandlers { get; set; } = Environment.ProcessorCount * 2;
    
    /// <summary>
    /// Timeout for individual handler execution
    /// </summary>
    public TimeSpan HandlerTimeout { get; set; } = TimeSpan.FromSeconds(30);
}
