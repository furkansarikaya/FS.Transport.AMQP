using RabbitMQ.Client;

namespace FS.StreamFlow.RabbitMQ.Features.ErrorHandling;

/// <summary>
/// Extension methods for IChannel to provide sync wrappers for async methods
/// </summary>
public static class RabbitMQChannelExtensions
{
    /// <summary>
    /// Synchronous wrapper for ExchangeDeclareAsync
    /// </summary>
    public static void ExchangeDeclare(this IChannel channel, string exchange, string type, bool durable = false, bool autoDelete = false, IDictionary<string, object>? arguments = null)
    {
        channel.ExchangeDeclareAsync(exchange, type, durable, autoDelete, arguments).GetAwaiter().GetResult();
    }

    /// <summary>
    /// Synchronous wrapper for QueueDeclareAsync
    /// </summary>
    public static QueueDeclareOk QueueDeclare(this IChannel channel, string queue = "", bool durable = false, bool exclusive = false, bool autoDelete = false, IDictionary<string, object>? arguments = null)
    {
        return channel.QueueDeclareAsync(queue, durable, exclusive, autoDelete, arguments).GetAwaiter().GetResult();
    }

    /// <summary>
    /// Synchronous wrapper for QueueDeclarePassiveAsync
    /// </summary>
    public static QueueDeclareOk QueueDeclarePassive(this IChannel channel, string queue)
    {
        return channel.QueueDeclarePassiveAsync(queue).GetAwaiter().GetResult();
    }

    /// <summary>
    /// Synchronous wrapper for QueueBindAsync
    /// </summary>
    public static void QueueBind(this IChannel channel, string queue, string exchange, string routingKey, IDictionary<string, object>? arguments = null)
    {
        channel.QueueBindAsync(queue, exchange, routingKey, arguments).GetAwaiter().GetResult();
    }

    /// <summary>
    /// Synchronous wrapper for QueuePurgeAsync
    /// </summary>
    public static uint QueuePurge(this IChannel channel, string queue)
    {
        return channel.QueuePurgeAsync(queue).GetAwaiter().GetResult();
    }

    /// <summary>
    /// Synchronous wrapper for BasicPublishAsync without properties
    /// </summary>
    public static void BasicPublish(this IChannel channel, string exchange, string routingKey, bool mandatory = false, ReadOnlyMemory<byte> body = default)
    {
        channel.BasicPublishAsync(exchange, routingKey, mandatory, body).GetAwaiter().GetResult();
    }

    /// <summary>
    /// Synchronous wrapper for BasicGetAsync
    /// </summary>
    public static BasicGetResult? BasicGet(this IChannel channel, string queue, bool autoAck)
    {
        return channel.BasicGetAsync(queue, autoAck).GetAwaiter().GetResult();
    }

    /// <summary>
    /// Synchronous wrapper for BasicAckAsync
    /// </summary>
    public static void BasicAck(this IChannel channel, ulong deliveryTag, bool multiple)
    {
        channel.BasicAckAsync(deliveryTag, multiple).GetAwaiter().GetResult();
    }

    /// <summary>
    /// Synchronous wrapper for BasicNackAsync
    /// </summary>
    public static void BasicNack(this IChannel channel, ulong deliveryTag, bool multiple, bool requeue)
    {
        channel.BasicNackAsync(deliveryTag, multiple, requeue).GetAwaiter().GetResult();
    }

    /// <summary>
    /// Creates basic properties for the channel
    /// </summary>
    public static IBasicProperties CreateBasicProperties(this IChannel channel)
    {
        return new BasicProperties();
    }
} 