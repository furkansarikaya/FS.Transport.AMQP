namespace FS.RabbitMQ.Core.Factories;

/// <summary>
/// Provides detailed information about a specific publisher.
/// </summary>
public sealed class PublisherDetails
{
    /// <summary>
    /// Gets or sets the publisher ID.
    /// </summary>
    public string PublisherId { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the message type being published.
    /// </summary>
    public string MessageType { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the publisher status.
    /// </summary>
    public PublisherStatus Status { get; set; }

    /// <summary>
    /// Gets or sets the number of messages published by this publisher.
    /// </summary>
    public long MessagesPublished { get; set; }

    /// <summary>
    /// Gets or sets the number of messages currently being published.
    /// </summary>
    public int CurrentlyPublishing { get; set; }

    /// <summary>
    /// Gets or sets the last publishing time for this publisher.
    /// </summary>
    public DateTimeOffset? LastPublishedAt { get; set; }

    /// <summary>
    /// Gets or sets the success rate percentage.
    /// </summary>
    public double SuccessRate { get; set; }
}