using FS.StreamFlow.Core.Features.Messaging.Models;

namespace FS.StreamFlow.RabbitMQ.Features.Producer;

/// <summary>
/// Event arguments for message published events.
/// </summary>
public class MessagePublishedEventArgs : EventArgs
{
    /// <summary>
    /// Gets the publish result.
    /// </summary>
    public PublishResult Result { get; }

    /// <summary>
    /// Initializes a new instance of the <see cref="MessagePublishedEventArgs"/> class.
    /// </summary>
    /// <param name="result">The publish result.</param>
    public MessagePublishedEventArgs(PublishResult result)
    {
        Result = result ?? throw new ArgumentNullException(nameof(result));
    }
}

/// <summary>
/// Event arguments for message publish failed events.
/// </summary>
public class MessagePublishFailedEventArgs : EventArgs
{
    /// <summary>
    /// Gets the message identifier.
    /// </summary>
    public string MessageId { get; }

    /// <summary>
    /// Gets the exchange name.
    /// </summary>
    public string Exchange { get; }

    /// <summary>
    /// Gets the routing key.
    /// </summary>
    public string RoutingKey { get; }

    /// <summary>
    /// Gets the exception that caused the failure.
    /// </summary>
    public Exception Exception { get; }

    /// <summary>
    /// Initializes a new instance of the <see cref="MessagePublishFailedEventArgs"/> class.
    /// </summary>
    /// <param name="messageId">The message identifier.</param>
    /// <param name="exchange">The exchange name.</param>
    /// <param name="routingKey">The routing key.</param>
    /// <param name="exception">The exception that caused the failure.</param>
    public MessagePublishFailedEventArgs(string messageId, string exchange, string routingKey, Exception exception)
    {
        MessageId = messageId ?? throw new ArgumentNullException(nameof(messageId));
        Exchange = exchange ?? throw new ArgumentNullException(nameof(exchange));
        RoutingKey = routingKey ?? throw new ArgumentNullException(nameof(routingKey));
        Exception = exception ?? throw new ArgumentNullException(nameof(exception));
    }
}

/// <summary>
/// Represents a message to be published.
/// </summary>
public class PublishMessage
{
    /// <summary>
    /// Gets or sets the exchange name.
    /// </summary>
    public string Exchange { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the routing key.
    /// </summary>
    public string RoutingKey { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the message content.
    /// </summary>
    public object Message { get; set; } = null!;

    /// <summary>
    /// Gets or sets the message properties.
    /// </summary>
    public MessageProperties? Properties { get; set; }
}

/// <summary>
/// Represents a publish failure.
/// </summary>
public class PublishFailure
{
    /// <summary>
    /// Gets or sets the message identifier.
    /// </summary>
    public string MessageId { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the exchange name.
    /// </summary>
    public string Exchange { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the routing key.
    /// </summary>
    public string RoutingKey { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the exception that caused the failure.
    /// </summary>
    public Exception Exception { get; set; } = null!;

    /// <summary>
    /// Gets or sets the failure timestamp.
    /// </summary>
    public DateTimeOffset Timestamp { get; set; } = DateTimeOffset.UtcNow;
} 