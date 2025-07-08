namespace FS.RabbitMQ.Core.Factories;

/// <summary>
/// Provides information about consumer statistics and health.
/// </summary>
public sealed class ConsumerInfo
{
    /// <summary>
    /// Gets or sets the number of active consumers.
    /// </summary>
    public int ActiveConsumers { get; set; }

    /// <summary>
    /// Gets or sets detailed information about each consumer.
    /// </summary>
    public IList<ConsumerDetails> ConsumerDetails { get; set; } = new List<ConsumerDetails>();

    /// <summary>
    /// Gets or sets the total number of messages processed across all consumers.
    /// </summary>
    public long TotalMessagesProcessed { get; set; }

    /// <summary>
    /// Gets or sets the total number of messages that were processed successfully.
    /// </summary>
    public long TotalMessagesSucceeded { get; set; }

    /// <summary>
    /// Gets or sets the total number of messages that failed processing.
    /// </summary>
    public long TotalMessagesFailed { get; set; }

    /// <summary>
    /// Gets or sets the average message processing time across all consumers.
    /// </summary>
    public TimeSpan AverageProcessingTime { get; set; }

    /// <summary>
    /// Gets or sets the time when this information was last updated.
    /// </summary>
    public DateTimeOffset LastUpdated { get; set; }
}