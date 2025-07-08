namespace FS.RabbitMQ.Core.Abstractions;

/// <summary>
/// Contains performance and operational statistics for message consumers.
/// </summary>
/// <remarks>
/// Consumer statistics provide real-time insights into consumer performance,
/// health, and operational characteristics. These metrics are essential for
/// monitoring, capacity planning, and performance optimization.
/// </remarks>
public sealed record ConsumerStatistics
{
    /// <summary>
    /// Gets the total number of messages processed by the consumer.
    /// </summary>
    /// <value>The cumulative count of messages that have been successfully processed.</value>
    public long TotalMessagesProcessed { get; init; }

    /// <summary>
    /// Gets the total number of messages that failed processing.
    /// </summary>
    /// <value>The cumulative count of messages that could not be processed successfully.</value>
    public long TotalMessagesFailed { get; init; }

    /// <summary>
    /// Gets the current message processing rate in messages per second.
    /// </summary>
    /// <value>The recent processing throughput measured over a sliding time window.</value>
    public double MessagesPerSecond { get; init; }

    /// <summary>
    /// Gets the average message processing time.
    /// </summary>
    /// <value>The mean duration for processing individual messages.</value>
    public TimeSpan AverageProcessingTime { get; init; }

    /// <summary>
    /// Gets the timestamp when statistics collection started.
    /// </summary>
    /// <value>The UTC timestamp when the consumer began collecting statistics.</value>
    public DateTimeOffset StartTime { get; init; }

    /// <summary>
    /// Gets the timestamp of the last statistics update.
    /// </summary>
    /// <value>The UTC timestamp when these statistics were last updated.</value>
    public DateTimeOffset LastUpdated { get; init; }

    /// <summary>
    /// Gets the duration the consumer has been operational.
    /// </summary>
    /// <value>The total time the consumer has been active and collecting statistics.</value>
    public TimeSpan Uptime => LastUpdated - StartTime;

    /// <summary>
    /// Gets the success rate as a percentage of successfully processed messages.
    /// </summary>
    /// <value>The percentage of messages processed successfully (0.0 to 100.0).</value>
    public double SuccessRate
    {
        get
        {
            var total = TotalMessagesProcessed + TotalMessagesFailed;
            return total > 0 ? (double)TotalMessagesProcessed / total * 100.0 : 0.0;
        }
    }

    /// <summary>
    /// Gets the error rate as a percentage of failed messages.
    /// </summary>
    /// <value>The percentage of messages that failed processing (0.0 to 100.0).</value>
    public double ErrorRate => 100.0 - SuccessRate;

    /// <summary>
    /// Creates a new ConsumerStatistics instance with the specified values.
    /// </summary>
    /// <param name="totalMessagesProcessed">The total number of messages processed successfully.</param>
    /// <param name="totalMessagesFailed">The total number of messages that failed processing.</param>
    /// <param name="messagesPerSecond">The current processing rate in messages per second.</param>
    /// <param name="averageProcessingTime">The average time to process a message.</param>
    /// <param name="startTime">The timestamp when statistics collection started.</param>
    /// <returns>A new ConsumerStatistics instance with the specified values.</returns>
    public static ConsumerStatistics Create(
        long totalMessagesProcessed,
        long totalMessagesFailed,
        double messagesPerSecond,
        TimeSpan averageProcessingTime,
        DateTimeOffset startTime)
    {
        return new ConsumerStatistics
        {
            TotalMessagesProcessed = totalMessagesProcessed,
            TotalMessagesFailed = totalMessagesFailed,
            MessagesPerSecond = messagesPerSecond,
            AverageProcessingTime = averageProcessingTime,
            StartTime = startTime,
            LastUpdated = DateTimeOffset.UtcNow
        };
    }

    /// <summary>
    /// Returns a string representation of the consumer statistics.
    /// </summary>
    /// <returns>A formatted string containing key performance metrics.</returns>
    public override string ToString()
    {
        return $"Processed: {TotalMessagesProcessed}, Failed: {TotalMessagesFailed}, " +
               $"Rate: {MessagesPerSecond:F2} msg/s, Success: {SuccessRate:F1}%, " +
               $"Avg Time: {AverageProcessingTime.TotalMilliseconds:F0}ms, Uptime: {Uptime}";
    }
}