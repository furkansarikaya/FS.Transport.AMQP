using System.Text;
using RabbitMQ.Client;

namespace FS.RabbitMQ.Core.Extensions;

/// <summary>
/// Extension methods for IChannel to provide additional functionality
/// </summary>
public static class ChannelExtensions
{
    /// <summary>
    /// Safely publishes a message to RabbitMQ with error handling
    /// </summary>
    /// <param name="channel">The channel to publish on</param>
    /// <param name="exchange">Exchange name</param>
    /// <param name="routingKey">Routing key</param>
    /// <param name="mandatory">Whether the message is mandatory</param>
    /// <param name="properties">Message properties</param>
    /// <param name="body">Message body</param>
    /// <returns>Task representing the async operation</returns>
    public static async Task SafeBasicPublishAsync(
        this IChannel channel,
        string exchange,
        string routingKey,
        bool mandatory,
        BasicProperties properties,
        ReadOnlyMemory<byte> body)
    {
        try
        {
            await channel.BasicPublishAsync(exchange, routingKey, mandatory, properties, body);
        }
        catch (Exception ex)
        {
            // Wrap and rethrow with additional context
            throw new InvalidOperationException(
                $"Failed to publish message to exchange '{exchange}' with routing key '{routingKey}': {ex.Message}", 
                ex);
        }
    }

    /// <summary>
    /// Publishes a string message to RabbitMQ
    /// </summary>
    /// <param name="channel">The channel to publish on</param>
    /// <param name="exchange">Exchange name</param>
    /// <param name="routingKey">Routing key</param>
    /// <param name="message">String message to publish</param>
    /// <param name="mandatory">Whether the message is mandatory</param>
    /// <param name="properties">Message properties</param>
    /// <returns>Task representing the async operation</returns>
    public static async Task BasicPublishStringAsync(
        this IChannel channel,
        string exchange,
        string routingKey,
        string message,
        bool mandatory = false,
        BasicProperties? properties = null)
    {
        var body = Encoding.UTF8.GetBytes(message);
        var props = properties ?? new BasicProperties();
        await channel.BasicPublishAsync(exchange, routingKey, mandatory, props, body);
    }

    /// <summary>
    /// Safely acknowledges a message
    /// </summary>
    /// <param name="channel">The channel</param>
    /// <param name="deliveryTag">Delivery tag</param>
    /// <param name="multiple">Whether to acknowledge multiple messages</param>
    /// <returns>Task representing the async operation</returns>
    public static async Task SafeBasicAckAsync(this IChannel channel, ulong deliveryTag, bool multiple = false)
    {
        try
        {
            await channel.BasicAckAsync(deliveryTag, multiple);
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException(
                $"Failed to acknowledge message with delivery tag '{deliveryTag}': {ex.Message}", 
                ex);
        }
    }

    /// <summary>
    /// Safely rejects a message
    /// </summary>
    /// <param name="channel">The channel</param>
    /// <param name="deliveryTag">Delivery tag</param>
    /// <param name="requeue">Whether to requeue the message</param>
    /// <returns>Task representing the async operation</returns>
    public static async Task SafeBasicRejectAsync(this IChannel channel, ulong deliveryTag, bool requeue = false)
    {
        try
        {
            await channel.BasicRejectAsync(deliveryTag, requeue);
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException(
                $"Failed to reject message with delivery tag '{deliveryTag}': {ex.Message}", 
                ex);
        }
    }

    /// <summary>
    /// Safely sends a negative acknowledgment
    /// </summary>
    /// <param name="channel">The channel</param>
    /// <param name="deliveryTag">Delivery tag</param>
    /// <param name="multiple">Whether to nack multiple messages</param>
    /// <param name="requeue">Whether to requeue the message</param>
    /// <returns>Task representing the async operation</returns>
    public static async Task SafeBasicNackAsync(this IChannel channel, ulong deliveryTag, bool multiple = false, bool requeue = false)
    {
        try
        {
            await channel.BasicNackAsync(deliveryTag, multiple, requeue);
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException(
                $"Failed to nack message with delivery tag '{deliveryTag}': {ex.Message}", 
                ex);
        }
    }

    /// <summary>
    /// Creates basic properties with comprehensive parameter support
    /// </summary>
    /// <param name="channel">The channel</param>
    /// <param name="contentType">Content type</param>
    /// <param name="contentEncoding">Content encoding</param>
    /// <param name="headers">Headers</param>
    /// <param name="deliveryMode">Delivery mode</param>
    /// <param name="priority">Priority</param>
    /// <param name="correlationId">Correlation ID</param>
    /// <param name="replyTo">Reply to</param>
    /// <param name="expiration">Expiration</param>
    /// <param name="messageId">Message ID</param>
    /// <param name="timestamp">Timestamp</param>
    /// <param name="type">Type</param>
    /// <param name="userId">User ID</param>
    /// <param name="appId">App ID</param>
    /// <param name="clusterId">Cluster ID</param>
    /// <returns>BasicProperties instance</returns>
    public static BasicProperties CreateBasicProperties(
        this IChannel channel,
        string? contentType = null,
        string? contentEncoding = null,
        IDictionary<string, object?>? headers = null,
        DeliveryModes? deliveryMode = null,
        byte? priority = null,
        string? correlationId = null,
        string? replyTo = null,
        string? expiration = null,
        string? messageId = null,
        AmqpTimestamp? timestamp = null,
        string? type = null,
        string? userId = null,
        string? appId = null,
        string? clusterId = null)
    {
        var properties = new BasicProperties();

        if (!string.IsNullOrEmpty(contentType))
            properties.ContentType = contentType;

        if (!string.IsNullOrEmpty(contentEncoding))
            properties.ContentEncoding = contentEncoding;

        if (headers != null)
            properties.Headers = headers;

        if (deliveryMode.HasValue)
            properties.DeliveryMode = deliveryMode.Value;

        if (priority.HasValue)
            properties.Priority = priority.Value;

        if (!string.IsNullOrEmpty(correlationId))
            properties.CorrelationId = correlationId;

        if (!string.IsNullOrEmpty(replyTo))
            properties.ReplyTo = replyTo;

        if (!string.IsNullOrEmpty(expiration))
            properties.Expiration = expiration;

        if (!string.IsNullOrEmpty(messageId))
            properties.MessageId = messageId;

        if (timestamp.HasValue)
            properties.Timestamp = timestamp.Value;

        if (!string.IsNullOrEmpty(type))
            properties.Type = type;

        if (!string.IsNullOrEmpty(userId))
            properties.UserId = userId;

        if (!string.IsNullOrEmpty(appId))
            properties.AppId = appId;

        if (!string.IsNullOrEmpty(clusterId))
            properties.ClusterId = clusterId;

        return properties;
    }
}