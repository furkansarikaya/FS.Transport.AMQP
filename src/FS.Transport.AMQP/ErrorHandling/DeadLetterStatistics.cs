namespace FS.Transport.AMQP.ErrorHandling;

/// <summary>
/// Dead letter statistics for monitoring
/// </summary>
public class DeadLetterStatistics
{
    /// <summary>
    /// Total number of messages processed
    /// </summary>
    public long TotalMessages { get; set; }
    
    /// <summary>
    /// Number of messages successfully sent to dead letter
    /// </summary>
    public long SuccessfulMessages { get; set; }
    
    /// <summary>
    /// Number of messages that failed to be sent to dead letter
    /// </summary>
    public long FailedMessages { get; set; }
    
    /// <summary>
    /// Average processing time
    /// </summary>
    public TimeSpan AverageProcessingTime { get; set; }
    
    /// <summary>
    /// Success rate as percentage
    /// </summary>
    public double SuccessRate => TotalMessages > 0 ? (double)SuccessfulMessages / TotalMessages * 100 : 0;

    /// <summary>
    /// Creates a copy of the statistics
    /// </summary>
    /// <returns>Cloned statistics</returns>
    public DeadLetterStatistics Clone()
    {
        return new DeadLetterStatistics
        {
            TotalMessages = TotalMessages,
            SuccessfulMessages = SuccessfulMessages,
            FailedMessages = FailedMessages,
            AverageProcessingTime = AverageProcessingTime
        };
    }

    /// <summary>
    /// Gets a string representation of the statistics
    /// </summary>
    /// <returns>Statistics summary</returns>
    public override string ToString()
    {
        return $"Dead Letter: {TotalMessages} total, {SuccessfulMessages} successful, " +
               $"{FailedMessages} failed, Success Rate: {SuccessRate:F2}%, " +
               $"Avg Time: {AverageProcessingTime.TotalMilliseconds:F2}ms";
    }
}