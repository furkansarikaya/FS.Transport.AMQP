namespace FS.RabbitMQ.Core.Abstractions;

/// <summary>
/// Exception thrown when message publishing operations fail.
/// </summary>
/// <remarks>
/// Publishing exceptions occur when messages cannot be sent to the broker
/// due to various conditions including network issues, broker configuration,
/// routing failures, or validation errors. The exception provides detailed
/// information about the failure to enable appropriate error handling.
/// </remarks>
[Serializable]
public sealed class PublishException : MessagingException
{
    /// <summary>
    /// Gets the name of the exchange where publishing was attempted.
    /// </summary>
    /// <value>The exchange name, or null if not applicable to the error.</value>
    public string? ExchangeName { get; }

    /// <summary>
    /// Gets the routing key that was used for the failed publishing attempt.
    /// </summary>
    /// <value>The routing key, or null if not applicable to the error.</value>
    public string? RoutingKey { get; }

    /// <summary>
    /// Gets the message identifier of the message that failed to publish.
    /// </summary>
    /// <value>The message ID, or null if not available.</value>
    public string? MessageId { get; }

    /// <summary>
    /// Initializes a new instance of the PublishException class.
    /// </summary>
    /// <param name="message">The error message describing the publishing failure.</param>
    /// <param name="exchangeName">The exchange name where publishing was attempted.</param>
    /// <param name="routingKey">The routing key used for the publishing attempt.</param>
    /// <param name="messageId">The message identifier, if available.</param>
    /// <param name="errorCode">Optional error code for programmatic error handling.</param>
    /// <param name="context">Optional additional context information.</param>
    public PublishException(string message, string? exchangeName = null, string? routingKey = null, string? messageId = null, string? errorCode = null, IReadOnlyDictionary<string, object>? context = null)
        : base(message, errorCode, context)
    {
        ExchangeName = exchangeName;
        RoutingKey = routingKey;
        MessageId = messageId;
    }

    /// <summary>
    /// Initializes a new instance of the PublishException class.
    /// </summary>
    /// <param name="message">The error message describing the publishing failure.</param>
    /// <param name="innerException">The exception that caused the publishing failure.</param>
    /// <param name="exchangeName">The exchange name where publishing was attempted.</param>
    /// <param name="routingKey">The routing key used for the publishing attempt.</param>
    /// <param name="messageId">The message identifier, if available.</param>
    /// <param name="errorCode">Optional error code for programmatic error handling.</param>
    /// <param name="context">Optional additional context information.</param>
    public PublishException(string message, Exception innerException, string? exchangeName = null, string? routingKey = null, string? messageId = null, string? errorCode = null, IReadOnlyDictionary<string, object>? context = null)
        : base(message, innerException, errorCode, context)
    {
        ExchangeName = exchangeName;
        RoutingKey = routingKey;
        MessageId = messageId;
    }

    /// <summary>
    /// Initializes a new instance of the PublishException class with serialized data.
    /// </summary>
    /// <param name="info">The SerializationInfo that holds the serialized object data.</param>
    /// <param name="context">The StreamingContext that contains contextual information.</param>
    private PublishException(System.Runtime.Serialization.SerializationInfo info, System.Runtime.Serialization.StreamingContext context)
        : base(info, context)
    {
        ExchangeName = info.GetString(nameof(ExchangeName));
        RoutingKey = info.GetString(nameof(RoutingKey));
        MessageId = info.GetString(nameof(MessageId));
    }
}
