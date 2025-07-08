namespace FS.RabbitMQ.Core.Abstractions;

/// <summary>
/// Contains the result information from a batch message publishing operation.
/// </summary>
/// <remarks>
/// Batch publishing results provide comprehensive feedback about the outcome of
/// publishing multiple messages in a single operation. Results include both
/// overall batch status and individual message outcomes, enabling fine-grained
/// error handling and retry strategies for partially successful batches.
/// 
/// Batch results are essential for high-throughput scenarios where understanding
/// the success/failure status of individual messages within a batch is important
/// for implementing appropriate recovery and retry logic.
/// </remarks>
public sealed record BatchPublishResult
{
    /// <summary>
    /// Gets a value indicating whether all messages in the batch were successfully published.
    /// </summary>
    /// <value><c>true</c> if all messages were published successfully; otherwise, <c>false</c>.</value>
    /// <remarks>
    /// Overall success indicates that the entire batch was processed without errors.
    /// When false, examine individual results to determine which specific messages
    /// failed and implement appropriate retry strategies for the failed subset.
    /// </remarks>
    public bool IsSuccess { get; init; }

    /// <summary>
    /// Gets the total number of messages that were attempted to be published.
    /// </summary>
    /// <value>The total count of messages in the original batch.</value>
    /// <remarks>
    /// Total count enables:
    /// - Validation that all messages were processed
    /// - Calculation of success/failure rates
    /// - Monitoring of batch size effectiveness
    /// - Audit trail completeness verification
    /// </remarks>
    public int TotalCount { get; init; }

    /// <summary>
    /// Gets the number of messages that were successfully published.
    /// </summary>
    /// <value>The count of messages that were accepted by the broker.</value>
    /// <remarks>
    /// Success count enables:
    /// - Partial success detection and handling
    /// - Success rate calculation for performance monitoring
    /// - Retry strategy optimization based on success patterns
    /// - Progress tracking for large batch operations
    /// </remarks>
    public int SuccessCount { get; init; }

    /// <summary>
    /// Gets the number of messages that failed to publish.
    /// </summary>
    /// <value>The count of messages that could not be published.</value>
    /// <remarks>
    /// Failure count enables:
    /// - Quick assessment of batch operation impact
    /// - Error rate monitoring and alerting
    /// - Capacity planning for error handling resources
    /// - Performance tuning of batch size and retry strategies
    /// </remarks>
    public int FailureCount { get; init; }

    /// <summary>
    /// Gets the detailed results for each message in the batch.
    /// </summary>
    /// <value>A collection containing the publish result for each message in the original order.</value>
    /// <remarks>
    /// Individual results enable:
    /// - Identification of specific failed messages for targeted retry
    /// - Detailed error analysis for troubleshooting
    /// - Selective acknowledgment in complex processing workflows
    /// - Fine-grained audit trails for compliance requirements
    /// 
    /// Results are ordered to match the original message sequence in the batch,
    /// enabling easy correlation between input messages and outcomes.
    /// </remarks>
    public required IReadOnlyList<PublishResult> Results { get; init; }

    /// <summary>
    /// Gets the timestamp when the batch publishing operation completed.
    /// </summary>
    /// <value>The UTC timestamp when the entire batch operation finished.</value>
    /// <remarks>
    /// Completion timestamp enables:
    /// - Batch operation performance monitoring
    /// - Timeout detection for large batch operations
    /// - Correlation with system performance metrics
    /// - Audit trail timing information
    /// </remarks>
    public DateTimeOffset Timestamp { get; init; } = DateTimeOffset.UtcNow;

    /// <summary>
    /// Gets the exchange name where the batch was published.
    /// </summary>
    /// <value>The name of the exchange that received the batch.</value>
    /// <remarks>
    /// Exchange information is useful for:
    /// - Batch operation monitoring and analytics
    /// - Troubleshooting routing and connectivity issues
    /// - Implementing exchange-specific retry strategies
    /// - Audit logging and compliance reporting
    /// </remarks>
    public required string ExchangeName { get; init; }

    /// <summary>
    /// Gets additional metadata about the batch publishing operation.
    /// </summary>
    /// <value>A dictionary containing supplementary information, or null if no additional data is available.</value>
    /// <remarks>
    /// Batch metadata may include:
    /// - Performance metrics (throughput, latency percentiles)
    /// - Resource utilization information
    /// - Serialization and compression statistics
    /// - Broker-specific batch processing information
    /// 
    /// Metadata provides insights for optimizing batch size, timing,
    /// and configuration for improved performance and reliability.
    /// </remarks>
    public IReadOnlyDictionary<string, object>? Metadata { get; init; }

    /// <summary>
    /// Creates a batch publish result from individual message results.
    /// </summary>
    /// <param name="exchangeName">The name of the exchange where the batch was published.</param>
    /// <param name="results">The individual publish results for each message in the batch.</param>
    /// <param name="metadata">Optional additional metadata about the batch operation.</param>
    /// <returns>A BatchPublishResult summarizing the batch operation outcome.</returns>
    /// <exception cref="ArgumentException">Thrown when <paramref name="exchangeName"/> is null, empty, or whitespace.</exception>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="results"/> is null.</exception>
    /// <exception cref="ArgumentException">Thrown when <paramref name="results"/> is empty.</exception>
    public static BatchPublishResult Create(
        string exchangeName,
        IReadOnlyList<PublishResult> results,
        IReadOnlyDictionary<string, object>? metadata = null)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(exchangeName);
        ArgumentNullException.ThrowIfNull(results);

        if (results.Count == 0)
        {
            throw new ArgumentException("Batch results cannot be empty.", nameof(results));
        }

        var successCount = results.Count(r => r.IsSuccess);
        var failureCount = results.Count - successCount;

        return new BatchPublishResult
        {
            IsSuccess = failureCount == 0,
            TotalCount = results.Count,
            SuccessCount = successCount,
            FailureCount = failureCount,
            Results = results,
            ExchangeName = exchangeName,
            Metadata = metadata
        };
    }

    /// <summary>
    /// Gets the messages that failed to publish from the batch.
    /// </summary>
    /// <value>A collection of publish results for messages that failed.</value>
    /// <remarks>
    /// Failed results enable:
    /// - Targeted retry of only the failed messages
    /// - Detailed error analysis for troubleshooting
    /// - Selective error handling based on failure types
    /// - Efficient resource utilization by avoiding unnecessary republishing
    /// </remarks>
    public IEnumerable<PublishResult> FailedResults => Results.Where(r => !r.IsSuccess);

    /// <summary>
    /// Gets the messages that were successfully published from the batch.
    /// </summary>
    /// <value>A collection of publish results for messages that succeeded.</value>
    /// <remarks>
    /// Successful results enable:
    /// - Confirmation of partial batch success
    /// - Progress tracking for large batch operations
    /// - Audit trail construction for completed operations
    /// - Performance analysis of successful publishing patterns
    /// </remarks>
    public IEnumerable<PublishResult> SuccessfulResults => Results.Where(r => r.IsSuccess);

    /// <summary>
    /// Gets the success rate of the batch operation as a percentage.
    /// </summary>
    /// <value>The percentage of messages that were successfully published (0.0 to 100.0).</value>
    /// <remarks>
    /// Success rate provides:
    /// - Quick assessment of batch operation quality
    /// - Performance monitoring and trend analysis
    /// - Threshold-based alerting for operational issues
    /// - Input for adaptive batch size optimization algorithms
    /// </remarks>
    public double SuccessRate => TotalCount > 0 ? (double)SuccessCount / TotalCount * 100.0 : 0.0;

    /// <summary>
    /// Returns a string representation of the batch publish result.
    /// </summary>
    /// <returns>A formatted string describing the batch operation outcome.</returns>
    public override string ToString()
    {
        return $"Batch publish to '{ExchangeName}': {SuccessCount}/{TotalCount} successful ({SuccessRate:F1}%)";
    }
}