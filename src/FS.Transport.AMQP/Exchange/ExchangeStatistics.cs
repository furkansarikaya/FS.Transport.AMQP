namespace FS.Transport.AMQP.Exchange;

/// <summary>
/// Statistics for exchange management operations
/// </summary>
public class ExchangeStatistics
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
    /// Number of exchanges declared
    /// </summary>
    public long DeclaredExchanges { get; set; }
    
    /// <summary>
    /// Number of exchanges deleted
    /// </summary>
    public long DeletedExchanges { get; set; }
    
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
    public ExchangeStatistics Clone()
    {
        return new ExchangeStatistics
        {
            TotalOperations = TotalOperations,
            SuccessfulOperations = SuccessfulOperations,
            FailedOperations = FailedOperations,
            DeclaredExchanges = DeclaredExchanges,
            DeletedExchanges = DeletedExchanges,
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
        DeclaredExchanges = 0;
        DeletedExchanges = 0;
        AverageOperationTime = TimeSpan.Zero;
    }

    /// <summary>
    /// Gets a string representation of the statistics
    /// </summary>
    /// <returns>Statistics summary</returns>
    public override string ToString()
    {
        return $"Exchange Stats: {TotalOperations} ops, {SuccessfulOperations} success, {FailedOperations} failed, " +
               $"Success Rate: {SuccessRate:F2}%, Avg Time: {AverageOperationTime.TotalMilliseconds:F2}ms, " +
               $"Declared: {DeclaredExchanges}, Deleted: {DeletedExchanges}";
    }
}
