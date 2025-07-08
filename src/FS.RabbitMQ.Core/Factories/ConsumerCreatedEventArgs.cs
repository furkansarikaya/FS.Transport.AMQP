namespace FS.RabbitMQ.Core.Factories;

/// <summary>
/// Event arguments for consumer creation events.
/// </summary>
public sealed class ConsumerCreatedEventArgs : EventArgs
{
    /// <summary>
    /// Initializes a new instance of the <see cref="ConsumerCreatedEventArgs"/> class.
    /// </summary>
    /// <param name="consumerId">The ID of the created consumer.</param>
    /// <param name="queueName">The name of the queue being consumed.</param>
    /// <param name="messageType">The type of messages being consumed.</param>
    public ConsumerCreatedEventArgs(string consumerId, string queueName, Type messageType)
    {
        ConsumerId = consumerId ?? throw new ArgumentNullException(nameof(consumerId));
        QueueName = queueName ?? throw new ArgumentNullException(nameof(queueName));
        MessageType = messageType ?? throw new ArgumentNullException(nameof(messageType));
        CreatedAt = DateTimeOffset.UtcNow;
    }

    /// <summary>
    /// Gets the ID of the created consumer.
    /// </summary>
    public string ConsumerId { get; }

    /// <summary>
    /// Gets the name of the queue being consumed.
    /// </summary>
    public string QueueName { get; }

    /// <summary>
    /// Gets the type of messages being consumed.
    /// </summary>
    public Type MessageType { get; }

    /// <summary>
    /// Gets the time when the consumer was created.
    /// </summary>
    public DateTimeOffset CreatedAt { get; }
}