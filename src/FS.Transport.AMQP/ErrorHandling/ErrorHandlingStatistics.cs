namespace FS.Transport.AMQP.ErrorHandling;

/// <summary>
/// Error handling statistics for monitoring and metrics
/// </summary>
public class ErrorHandlingStatistics
{
    /// <summary>
    /// Total number of errors encountered
    /// </summary>
    public long TotalErrors { get; set; }
    
    /// <summary>
    /// Number of errors successfully handled
    /// </summary>
    public long SuccessfullyHandled { get; set; }
    
    /// <summary>
    /// Number of errors that failed to be handled
    /// </summary>
    public long FailedToHandle { get; set; }
    
    /// <summary>
    /// Average time to handle an error
    /// </summary>
    public TimeSpan AverageHandlingTime { get; set; }
    
    /// <summary>
    /// Success rate as percentage
    /// </summary>
    public double SuccessRate => TotalErrors > 0 ? (double)SuccessfullyHandled / TotalErrors * 100 : 0;

    /// <summary>
    /// Creates a copy of the statistics
    /// </summary>
    /// <returns>Cloned statistics</returns>
    public ErrorHandlingStatistics Clone()
    {
        return new ErrorHandlingStatistics
        {
            TotalErrors = TotalErrors,
            SuccessfullyHandled = SuccessfullyHandled,
            FailedToHandle = FailedToHandle,
            AverageHandlingTime = AverageHandlingTime
        };
    }

    /// <summary>
    /// Gets a string representation of the statistics
    /// </summary>
    /// <returns>Statistics summary</returns>
    public override string ToString()
    {
        return $"Errors: {TotalErrors}, Handled: {SuccessfullyHandled}, Failed: {FailedToHandle}, " +
               $"Success Rate: {SuccessRate:F2}%, Avg Time: {AverageHandlingTime.TotalMilliseconds:F2}ms";
    }
}