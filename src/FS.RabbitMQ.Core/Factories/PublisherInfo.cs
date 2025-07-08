namespace FS.RabbitMQ.Core.Factories;

/// <summary>
/// Provides information about publisher statistics and health.
/// </summary>
public sealed class PublisherInfo
{
    /// <summary>
    /// Gets or sets the number of active publishers.
    /// </summary>
    public int ActivePublishers { get; set; }

    /// <summary>
    /// Gets or sets detailed information about each publisher.
    /// </summary>
    public IList<PublisherDetails> PublisherDetails { get; set; } = new List<PublisherDetails>();

    /// <summary>
    /// Gets or sets the total number of messages published across all publishers.
    /// </summary>
    public long TotalMessagesPublished { get; set; }

    /// <summary>
    /// Gets or sets the total number of messages that were confirmed.
    /// </summary>
    public long TotalMessagesConfirmed { get; set; }

    /// <summary>
    /// Gets or sets the total number of messages that failed to publish.
    /// </summary>
    public long TotalMessagesFailed { get; set; }

    /// <summary>
    /// Gets or sets the average message publishing time across all publishers.
    /// </summary>
    public TimeSpan AveragePublishTime { get; set; }

    /// <summary>
    /// Gets or sets the time when this information was last updated.
    /// </summary>
    public DateTimeOffset LastUpdated { get; set; }
}