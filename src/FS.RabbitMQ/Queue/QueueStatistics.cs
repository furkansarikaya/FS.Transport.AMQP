namespace FS.RabbitMQ.Queue;

/// <summary>
/// Statistics for queue management operations
/// </summary>
public class QueueStatistics
{
    /// <summary>
    /// Total number of operations performed
    /// </summary>
    public long TotalOperations { get; set; }
    
    /// <summary>
    /// Number of successful operations
    /// </summary>
    public long SuccessfulOperations { get; set; }
    
    /// <summary>
    /// Number of failed operations
    /// </summary>
    public long FailedOperations { get; set; }
    
    /// <summary>
    /// Number of queues declared
    /// </summary>
    public long DeclaredQueues { get; set; }
    
    /// <summary>
    /// Number of queues deleted
    /// </summary>
    public long DeletedQueues { get; set; }
    
    /// <summary>
    /// Number of queue purge operations
    /// </summary>
    public long PurgedQueues { get; set; }
    
    /// <summary>
    /// Average time per operation
    /// </summary>
    public TimeSpan AverageOperationTime { get; set; }
    
    /// <summary>
    /// Success rate as percentage
    /// </summary>
    public double SuccessRate => TotalOperations > 0 ? (double)SuccessfulOperations / TotalOperations * 100 : 0;
    
    /// <summary>
    /// Failure rate as percentage
    /// </summary>
    public double FailureRate => TotalOperations > 0 ? (double)FailedOperations / TotalOperations * 100 : 0;

    /// <summary>
    /// Creates a copy of the statistics
    /// </summary>
    /// <returns>Cloned statistics</returns>
    public QueueStatistics Clone()
    {
        return new QueueStatistics
        {
            TotalOperations = TotalOperations,
            SuccessfulOperations = SuccessfulOperations,
            FailedOperations = FailedOperations,
            DeclaredQueues = DeclaredQueues,
            DeletedQueues = DeletedQueues,
            PurgedQueues = PurgedQueues,
            AverageOperationTime = AverageOperationTime
        };
    }

    /// <summary>
    /// Resets all statistics
    /// </summary>
    public void Reset()
    {
        TotalOperations = 0;
        SuccessfulOperations = 0;
        FailedOperations = 0;
        DeclaredQueues = 0;
        DeletedQueues = 0;
        PurgedQueues = 0;
        AverageOperationTime = TimeSpan.Zero;
    }

    /// <summary>
    /// Gets a string representation of the statistics
    /// </summary>
    /// <returns>Statistics summary</returns>
    public override string ToString()
    {
        return $"Queue Stats: {TotalOperations} ops, {SuccessfulOperations} success, {FailedOperations} failed, " +
               $"Success Rate: {SuccessRate:F2}%, Avg Time: {AverageOperationTime.TotalMilliseconds:F2}ms, " +
               $"Declared: {DeclaredQueues}, Deleted: {DeletedQueues}, Purged: {PurgedQueues}";
    }
}