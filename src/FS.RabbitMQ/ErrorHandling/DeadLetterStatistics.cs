namespace FS.RabbitMQ.ErrorHandling;

/// <summary>
/// Statistics for dead letter handling operations
/// </summary>
public class DeadLetterStatistics
{
    /// <summary>
    /// Gets or sets the total number of messages sent to dead letter queue
    /// </summary>
    public long TotalDeadLetters { get; set; }

    /// <summary>
    /// Gets or sets the number of failed dead letter operations
    /// </summary>
    public long FailedDeadLetters { get; set; }

    /// <summary>
    /// Gets or sets the number of messages requeued from dead letter
    /// </summary>
    public long RequeuedFromDeadLetter { get; set; }

    /// <summary>
    /// Gets or sets the number of currently active dead letter messages
    /// </summary>
    public int ActiveDeadLetters { get; set; }

    /// <summary>
    /// Gets or sets when the last message was sent to dead letter
    /// </summary>
    public DateTimeOffset? LastDeadLetterTime { get; set; }

    /// <summary>
    /// Gets or sets when the last message was requeued from dead letter
    /// </summary>
    public DateTimeOffset? LastRequeueTime { get; set; }

    /// <summary>
    /// Gets the success rate for dead letter operations
    /// </summary>
    public double SuccessRate => TotalDeadLetters > 0 ? 
        (double)(TotalDeadLetters - FailedDeadLetters) / TotalDeadLetters * 100 : 0;

    /// <summary>
    /// Gets the failure rate for dead letter operations
    /// </summary>
    public double FailureRate => TotalDeadLetters > 0 ? 
        (double)FailedDeadLetters / TotalDeadLetters * 100 : 0;

    /// <summary>
    /// Gets the requeue rate for dead letter messages
    /// </summary>
    public double RequeueRate => TotalDeadLetters > 0 ? 
        (double)RequeuedFromDeadLetter / TotalDeadLetters * 100 : 0;
}