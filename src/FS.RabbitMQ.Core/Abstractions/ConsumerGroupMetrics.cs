namespace FS.RabbitMQ.Core.Abstractions;

/// <summary>
/// Contains aggregate performance metrics for a consumer group.
/// </summary>
/// <remarks>
/// Group metrics provide a consolidated view of performance across all group members,
/// enabling monitoring of overall group effectiveness and identification of
/// performance trends and optimization opportunities.
/// </remarks>
public sealed record ConsumerGroupMetrics
{
    /// <summary>
    /// Gets the total message processing rate across all group members.
    /// </summary>
    /// <value>The aggregate throughput in messages per second for the entire group.</value>
    public double TotalMessagesPerSecond { get; init; }

    /// <summary>
    /// Gets the average message processing time across all group members.
    /// </summary>
    /// <value>The mean processing duration weighted by member activity levels.</value>
    public TimeSpan AverageProcessingTime { get; init; }

    /// <summary>
    /// Gets the overall success rate across all group members.
    /// </summary>
    /// <value>The percentage of messages processed successfully across the entire group (0.0 to 100.0).</value>
    public double OverallSuccessRate { get; init; }

    /// <summary>
    /// Gets the load distribution balance score.
    /// </summary>
    /// <value>A score indicating how evenly load is distributed across group members (0.0 to 1.0, where 1.0 is perfectly balanced).</value>
    /// <remarks>
    /// Load balance calculation considers:
    /// - Variation in processing rates across members
    /// - Differences in assigned load shares
    /// - Member capacity and performance characteristics
    /// - Recent rebalancing effectiveness
    /// 
    /// Values closer to 1.0 indicate well-balanced load distribution,
    /// while lower values suggest rebalancing may be beneficial.
    /// </remarks>
    public double LoadBalanceScore { get; init; }

    /// <summary>
    /// Gets the timestamp when these metrics were calculated.
    /// </summary>
    /// <value>The UTC timestamp when the aggregate metrics were computed.</value>
    public DateTimeOffset CalculatedAt { get; init; }

    /// <summary>
    /// Gets an empty ConsumerGroupMetrics instance for initialization purposes.
    /// </summary>
    /// <value>A metrics instance with zero values and current timestamp.</value>
    public static ConsumerGroupMetrics Empty => new()
    {
        TotalMessagesPerSecond = 0,
        AverageProcessingTime = TimeSpan.Zero,
        OverallSuccessRate = 0,
        LoadBalanceScore = 0,
        CalculatedAt = DateTimeOffset.UtcNow
    };

    /// <summary>
    /// Creates aggregate metrics from individual member statistics.
    /// </summary>
    /// <param name="memberStatistics">The collection of individual member statistics.</param>
    /// <returns>Aggregated metrics for the consumer group.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="memberStatistics"/> is null.</exception>
    public static ConsumerGroupMetrics FromMemberStatistics(IEnumerable<ConsumerStatistics> memberStatistics)
    {
        ArgumentNullException.ThrowIfNull(memberStatistics);

        var stats = memberStatistics.ToList();
        if (!stats.Any())
        {
            return Empty;
        }

        var totalMessagesPerSecond = stats.Sum(s => s.MessagesPerSecond);
        var weightedProcessingTime = stats
            .Where(s => s.TotalMessagesProcessed > 0)
            .Select(s => s.AverageProcessingTime.TotalMilliseconds * s.TotalMessagesProcessed)
            .Sum() / Math.Max(1, stats.Sum(s => s.TotalMessagesProcessed));
        
        var totalProcessed = stats.Sum(s => s.TotalMessagesProcessed);
        var totalFailed = stats.Sum(s => s.TotalMessagesFailed);
        var successRate = totalProcessed + totalFailed > 0 
            ? (double)totalProcessed / (totalProcessed + totalFailed) * 100.0 
            : 0.0;

        // Calculate load balance score based on processing rate variance
        var rates = stats.Select(s => s.MessagesPerSecond).ToList();
        var avgRate = rates.Average();
        var variance = rates.Any() ? rates.Sum(r => Math.Pow(r - avgRate, 2)) / rates.Count : 0;
        var loadBalanceScore = avgRate > 0 ? Math.Max(0, 1.0 - (Math.Sqrt(variance) / avgRate)) : 1.0;

        return new ConsumerGroupMetrics
        {
            TotalMessagesPerSecond = totalMessagesPerSecond,
            AverageProcessingTime = TimeSpan.FromMilliseconds(weightedProcessingTime),
            OverallSuccessRate = successRate,
            LoadBalanceScore = Math.Min(1.0, loadBalanceScore),
            CalculatedAt = DateTimeOffset.UtcNow
        };
    }

    /// <summary>
    /// Returns a string representation of the consumer group metrics.
    /// </summary>
    /// <returns>A formatted string containing key group performance indicators.</returns>
    public override string ToString()
    {
        return $"Group: {TotalMessagesPerSecond:F2} msg/s, Success: {OverallSuccessRate:F1}%, " +
               $"Balance: {LoadBalanceScore:F3}, Avg Time: {AverageProcessingTime.TotalMilliseconds:F0}ms";
    }
}