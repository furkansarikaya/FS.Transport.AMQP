namespace FS.RabbitMQ.Core.Abstractions;

/// <summary>
/// Contains statistics about filter effectiveness and message processing patterns for filtered consumers.
/// </summary>
/// <remarks>
/// Filter statistics provide insights into how effectively filters are selecting
/// messages and the distribution of message types flowing through the consumer.
/// This information is valuable for optimizing filter logic and understanding
/// message patterns.
/// </remarks>
public sealed record FilterStatistics
{
    /// <summary>
    /// Gets the total number of messages evaluated by the filter.
    /// </summary>
    /// <value>The cumulative count of messages that have been examined by the filter logic.</value>
    public long TotalMessagesEvaluated { get; init; }

    /// <summary>
    /// Gets the number of messages accepted by the filter for processing.
    /// </summary>
    /// <value>The cumulative count of messages that passed filter criteria and were processed.</value>
    public long MessagesAccepted { get; init; }

    /// <summary>
    /// Gets the number of messages rejected by the filter.
    /// </summary>
    /// <value>The cumulative count of messages that did not pass filter criteria and were skipped.</value>
    public long MessagesRejected { get; init; }

    /// <summary>
    /// Gets the average time spent evaluating filter criteria per message.
    /// </summary>
    /// <value>The mean duration for executing filter logic on individual messages.</value>
    public TimeSpan AverageFilterTime { get; init; }

    /// <summary>
    /// Gets the timestamp when filter statistics collection started.
    /// </summary>
    /// <value>The UTC timestamp when the filter began collecting statistics.</value>
    public DateTimeOffset StartTime { get; init; }

    /// <summary>
    /// Gets the timestamp of the last statistics update.
    /// </summary>
    /// <value>The UTC timestamp when these statistics were last updated.</value>
    public DateTimeOffset LastUpdated { get; init; }

    /// <summary>
    /// Gets the filter acceptance rate as a percentage.
    /// </summary>
    /// <value>The percentage of evaluated messages that were accepted by the filter (0.0 to 100.0).</value>
    public double AcceptanceRate
    {
        get
        {
            return TotalMessagesEvaluated > 0 
                ? (double)MessagesAccepted / TotalMessagesEvaluated * 100.0 
                : 0.0;
        }
    }

    /// <summary>
    /// Gets the filter rejection rate as a percentage.
    /// </summary>
    /// <value>The percentage of evaluated messages that were rejected by the filter (0.0 to 100.0).</value>
    public double RejectionRate => 100.0 - AcceptanceRate;

    /// <summary>
    /// Gets the filter selectivity, indicating how selective the filter is.
    /// </summary>
    /// <value>A selectivity measure where values closer to 0.5 indicate balanced filtering, and values closer to 0 or 1 indicate high selectivity.</value>
    /// <remarks>
    /// Filter selectivity helps assess filter effectiveness:
    /// - Values near 0.5 indicate the filter accepts about half the messages (balanced)
    /// - Values near 0 indicate very selective filtering (few messages accepted)
    /// - Values near 1 indicate very permissive filtering (most messages accepted)
    /// 
    /// This metric helps optimize filter logic and understand message distribution patterns.
    /// </remarks>
    public double Selectivity
    {
        get
        {
            var rate = AcceptanceRate / 100.0;
            return Math.Abs(rate - 0.5);
        }
    }

    /// <summary>
    /// Creates a new FilterStatistics instance with the specified values.
    /// </summary>
    /// <param name="totalMessagesEvaluated">The total number of messages evaluated by the filter.</param>
    /// <param name="messagesAccepted">The number of messages accepted by the filter.</param>
    /// <param name="messagesRejected">The number of messages rejected by the filter.</param>
    /// <param name="averageFilterTime">The average time spent evaluating filter criteria.</param>
    /// <param name="startTime">The timestamp when statistics collection started.</param>
    /// <returns>A new FilterStatistics instance with the specified values.</returns>
    /// <exception cref="ArgumentException">Thrown when the sum of accepted and rejected messages does not equal the total evaluated.</exception>
    public static FilterStatistics Create(
        long totalMessagesEvaluated,
        long messagesAccepted,
        long messagesRejected,
        TimeSpan averageFilterTime,
        DateTimeOffset startTime)
    {
        if (messagesAccepted + messagesRejected != totalMessagesEvaluated)
        {
            throw new ArgumentException("The sum of accepted and rejected messages must equal the total evaluated messages.");
        }

        return new FilterStatistics
        {
            TotalMessagesEvaluated = totalMessagesEvaluated,
            MessagesAccepted = messagesAccepted,
            MessagesRejected = messagesRejected,
            AverageFilterTime = averageFilterTime,
            StartTime = startTime,
            LastUpdated = DateTimeOffset.UtcNow
        };
    }

    /// <summary>
    /// Returns a string representation of the filter statistics.
    /// </summary>
    /// <returns>A formatted string containing key filter performance metrics.</returns>
    public override string ToString()
    {
        return $"Evaluated: {TotalMessagesEvaluated}, Accepted: {MessagesAccepted}, Rejected: {MessagesRejected}, " +
               $"Acceptance: {AcceptanceRate:F1}%, Selectivity: {Selectivity:F3}, " +
               $"Avg Filter Time: {AverageFilterTime.TotalMicroseconds:F0}μs";
    }
}