namespace FS.RabbitMQ.Core.Factories;

/// <summary>
/// Provides detailed information about a specific consumer.
/// </summary>
public sealed class ConsumerDetails
{
    /// <summary>
    /// Gets or sets the consumer ID.
    /// </summary>
    public string ConsumerId { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the queue name being consumed.
    /// </summary>
    public string QueueName { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the message type being consumed.
    /// </summary>
    public string MessageType { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the consumer status.
    /// </summary>
    public ConsumerStatus Status { get; set; }

    /// <summary>
    /// Gets or sets the number of messages processed by this consumer.
    /// </summary>
    public long MessagesProcessed { get; set; }

    /// <summary>
    /// Gets or sets the number of messages currently being processed.
    /// </summary>
    public int CurrentlyProcessing { get; set; }

    /// <summary>
    /// Gets or sets the last processing time for this consumer.
    /// </summary>
    public DateTimeOffset? LastProcessedAt { get; set; }
}
