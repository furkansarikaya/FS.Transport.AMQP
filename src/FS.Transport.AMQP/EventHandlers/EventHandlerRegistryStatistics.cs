namespace FS.Transport.AMQP.EventHandlers;

/// <summary>
/// Statistics for event handler registry
/// </summary>
public class EventHandlerRegistryStatistics
{
    /// <summary>
    /// Total number of registered handlers
    /// </summary>
    public int TotalHandlers { get; set; }
    
    /// <summary>
    /// Number of distinct event types handled
    /// </summary>
    public int EventTypes { get; set; }
    
    /// <summary>
    /// Average number of handlers per event type
    /// </summary>
    public double AverageHandlersPerEventType { get; set; }
    
    /// <summary>
    /// Detailed information about each handler
    /// </summary>
    public List<HandlerDetail> HandlerDetails { get; set; } = new();

    /// <summary>
    /// Gets a string representation of the statistics
    /// </summary>
    /// <returns>Statistics summary</returns>
    public override string ToString()
    {
        return $"Handler Registry: {TotalHandlers} handlers, {EventTypes} event types, " +
               $"Avg {AverageHandlersPerEventType:F2} handlers per event type";
    }
}
