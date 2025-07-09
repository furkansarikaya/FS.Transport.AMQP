using System.Text;
using FS.Transport.AMQP.Core.Exceptions;
using RabbitMQ.Client;

namespace FS.Transport.AMQP.Core.Extensions;

/// <summary>
/// Extension methods for RabbitMQ channels with enhanced safety and convenience
/// </summary>
public static class ChannelExtensions
{
    /// <summary>
    /// Publishes a message with enhanced error handling and retry logic
    /// </summary>
    /// <param name="channel">RabbitMQ channel</param>
    /// <param name="exchange">Exchange name</param>
    /// <param name="routingKey">Routing key</param>
    /// <param name="body">Message body</param>
    /// <param name="properties">Basic properties</param>
    /// <param name="mandatory">Mandatory flag</param>
    public static void SafeBasicPublish(this IModel channel, 
        string exchange, 
        string routingKey, 
        ReadOnlyMemory<byte> body, 
        IBasicProperties? properties = null, 
        bool mandatory = false)
    {
        if (!channel.IsUsable())
            throw new InvalidOperationException("Channel is not in a usable state");

        try
        {
            channel.BasicPublish(exchange, routingKey, mandatory, properties, body);
        }
        catch (Exception ex)
        {
            throw new RabbitMQException($"Failed to publish message to exchange '{exchange}' with routing key '{routingKey}'", ex)
                .WithContext("Exchange", exchange)
                .WithContext("RoutingKey", routingKey)
                .WithContext("MessageSize", body.Length);
        }
    }

    /// <summary>
    /// Publishes a string message with UTF-8 encoding
    /// </summary>
    /// <param name="channel">RabbitMQ channel</param>
    /// <param name="exchange">Exchange name</param>
    /// <param name="routingKey">Routing key</param>
    /// <param name="message">String message</param>
    /// <param name="properties">Basic properties</param>
    /// <param name="mandatory">Mandatory flag</param>
    public static void BasicPublishString(this IModel channel, 
        string exchange, 
        string routingKey, 
        string message, 
        IBasicProperties? properties = null, 
        bool mandatory = false)
    {
        var body = Encoding.UTF8.GetBytes(message);
        channel.SafeBasicPublish(exchange, routingKey, body, properties, mandatory);
    }

    /// <summary>
    /// Creates basic properties with common default values
    /// </summary>
    /// <param name="channel">RabbitMQ channel</param>
    /// <param name="messageId">Message ID</param>
    /// <param name="correlationId">Correlation ID</param>
    /// <param name="contentType">Content type</param>
    /// <param name="deliveryMode">Delivery mode (1=non-persistent, 2=persistent)</param>
    /// <param name="timestamp">Message timestamp</param>
    /// <returns>Configured basic properties</returns>
    public static IBasicProperties CreateBasicProperties(this IModel channel,
        string? messageId = null,
        string? correlationId = null,
        string contentType = Constants.ContentTypes.Json,
        byte deliveryMode = 2,
        DateTimeOffset? timestamp = null)
    {
        var properties = channel.CreateBasicProperties();
        
        properties.MessageId = messageId ?? Guid.NewGuid().ToString();
        properties.CorrelationId = correlationId;
        properties.ContentType = contentType;
        properties.DeliveryMode = deliveryMode;
        properties.Timestamp = new AmqpTimestamp((timestamp ?? DateTimeOffset.UtcNow).ToUnixTimeSeconds());
        
        return properties;
    }

    /// <summary>
    /// Acknowledges a message with error handling
    /// </summary>
    /// <param name="channel">RabbitMQ channel</param>
    /// <param name="deliveryTag">Delivery tag</param>
    /// <param name="multiple">Multiple acknowledgment flag</param>
    public static void SafeBasicAck(this IModel channel, ulong deliveryTag, bool multiple = false)
    {
        if (!channel.IsUsable())
            return;

        try
        {
            channel.BasicAck(deliveryTag, multiple);
        }
        catch (Exception ex)
        {
            throw new RabbitMQException($"Failed to acknowledge message with delivery tag {deliveryTag}", ex)
                .WithContext("DeliveryTag", deliveryTag)
                .WithContext("Multiple", multiple);
        }
    }

    /// <summary>
    /// Rejects a message with error handling
    /// </summary>
    /// <param name="channel">RabbitMQ channel</param>
    /// <param name="deliveryTag">Delivery tag</param>
    /// <param name="requeue">Requeue flag</param>
    public static void SafeBasicReject(this IModel channel, ulong deliveryTag, bool requeue = false)
    {
        if (!channel.IsUsable())
            return;

        try
        {
            channel.BasicReject(deliveryTag, requeue);
        }
        catch (Exception ex)
        {
            throw new RabbitMQException($"Failed to reject message with delivery tag {deliveryTag}", ex)
                .WithContext("DeliveryTag", deliveryTag)
                .WithContext("Requeue", requeue);
        }
    }

    /// <summary>
    /// Negatively acknowledges a message with error handling
    /// </summary>
    /// <param name="channel">RabbitMQ channel</param>
    /// <param name="deliveryTag">Delivery tag</param>
    /// <param name="multiple">Multiple acknowledgment flag</param>
    /// <param name="requeue">Requeue flag</param>
    public static void SafeBasicNack(this IModel channel, ulong deliveryTag, bool multiple = false, bool requeue = false)
    {
        if (!channel.IsUsable())
            return;

        try
        {
            channel.BasicNack(deliveryTag, multiple, requeue);
        }
        catch (Exception ex)
        {
            throw new RabbitMQException($"Failed to nack message with delivery tag {deliveryTag}", ex)
                .WithContext("DeliveryTag", deliveryTag)
                .WithContext("Multiple", multiple)
                .WithContext("Requeue", requeue);
        }
    }
}