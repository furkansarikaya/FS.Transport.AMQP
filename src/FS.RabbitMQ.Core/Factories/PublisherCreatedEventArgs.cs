namespace FS.RabbitMQ.Core.Factories;

/// <summary>
/// Event arguments for publisher creation events.
/// </summary>
public sealed class PublisherCreatedEventArgs : EventArgs
{
    /// <summary>
    /// Initializes a new instance of the <see cref="PublisherCreatedEventArgs"/> class.
    /// </summary>
    /// <param name="publisherId">The ID of the created publisher.</param>
    /// <param name="messageType">The type of messages being published.</param>
    public PublisherCreatedEventArgs(string publisherId, Type messageType)
    {
        PublisherId = publisherId ?? throw new ArgumentNullException(nameof(publisherId));
        MessageType = messageType ?? throw new ArgumentNullException(nameof(messageType));
        CreatedAt = DateTimeOffset.UtcNow;
    }

    /// <summary>
    /// Gets the ID of the created publisher.
    /// </summary>
    public string PublisherId { get; }

    /// <summary>
    /// Gets the type of messages being published.
    /// </summary>
    public Type MessageType { get; }

    /// <summary>
    /// Gets the time when the publisher was created.
    /// </summary>
    public DateTimeOffset CreatedAt { get; }
}
