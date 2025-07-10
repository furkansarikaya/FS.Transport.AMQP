namespace FS.Transport.AMQP.EventHandlers;

/// <summary>
/// Statistics for event handler execution
/// </summary>
public class EventHandlerExecutionStatistics
{
    /// <summary>
    /// Total number of events processed
    /// </summary>
    public long TotalEvents { get; set; }
    
    /// <summary>
    /// Number of successfully processed events
    /// </summary>
    public long SuccessfulEvents { get; set; }
    
    /// <summary>
    /// Number of failed events
    /// </summary>
    public long FailedEvents { get; set; }
    
    /// <summary>
    /// Total number of handler executions
    /// </summary>
    public long TotalHandlerExecutions { get; set; }
    
    /// <summary>
    /// Number of successful handler executions
    /// </summary>
    public long SuccessfulHandlerExecutions { get; set; }
    
    /// <summary>
    /// Number of failed handler executions
    /// </summary>
    public long FailedHandlerExecutions { get; set; }
    
    /// <summary>
    /// Average event processing time
    /// </summary>
    public TimeSpan AverageExecutionTime { get; set; }
    
    /// <summary>
    /// Average handler execution time
    /// </summary>
    public TimeSpan AverageHandlerExecutionTime { get; set; }
    
    /// <summary>
    /// Event success rate as percentage
    /// </summary>
    public double EventSuccessRate => TotalEvents > 0 ? (double)SuccessfulEvents / TotalEvents * 100 : 0;
    
    /// <summary>
    /// Handler success rate as percentage
    /// </summary>
    public double HandlerSuccessRate => TotalHandlerExecutions > 0 ? (double)SuccessfulHandlerExecutions / TotalHandlerExecutions * 100 : 0;

    /// <summary>
    /// Creates a copy of the statistics
    /// </summary>
    /// <returns>Cloned statistics</returns>
    public EventHandlerExecutionStatistics Clone()
    {
        return new EventHandlerExecutionStatistics
        {
            TotalEvents = TotalEvents,
            SuccessfulEvents = SuccessfulEvents,
            FailedEvents = FailedEvents,
            TotalHandlerExecutions = TotalHandlerExecutions,
            SuccessfulHandlerExecutions = SuccessfulHandlerExecutions,
            FailedHandlerExecutions = FailedHandlerExecutions,
            AverageExecutionTime = AverageExecutionTime,
            AverageHandlerExecutionTime = AverageHandlerExecutionTime
        };
    }

    /// <summary>
    /// Gets a string representation of the statistics
    /// </summary>
    /// <returns>Statistics summary</returns>
    public override string ToString()
    {
        return $"Event Handler Stats: {TotalEvents} events ({EventSuccessRate:F2}% success), " +
               $"{TotalHandlerExecutions} handler executions ({HandlerSuccessRate:F2}% success), " +
               $"Avg Event Time: {AverageExecutionTime.TotalMilliseconds:F2}ms, " +
               $"Avg Handler Time: {AverageHandlerExecutionTime.TotalMilliseconds:F2}ms";
    }
}