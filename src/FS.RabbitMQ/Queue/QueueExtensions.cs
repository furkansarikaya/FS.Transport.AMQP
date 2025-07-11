using FS.RabbitMQ.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace FS.RabbitMQ.Queue;

/// <summary>
/// Extension methods for queue management and configuration
/// </summary>
public static class QueueExtensions
{
    /// <summary>
    /// Adds queue management services to the service collection
    /// </summary>
    /// <param name="services">Service collection</param>
    /// <returns>Service collection for fluent configuration</returns>
    public static IServiceCollection AddQueueManager(this IServiceCollection services)
    {
        services.TryAddSingleton<IQueueManager, QueueManager>();
        return services;
    }

    /// <summary>
    /// Adds a queue configuration
    /// </summary>
    /// <param name="services">Service collection</param>
    /// <param name="configureQueue">Queue configuration action</param>
    /// <returns>Service collection for fluent configuration</returns>
    public static IServiceCollection AddQueue(this IServiceCollection services, 
        Action<QueueSettings> configureQueue)
    {
        services.Configure<RabbitMQConfiguration>(config =>
        {
            var queue = new QueueSettings();
            configureQueue(queue);
            config.Queues.Add(queue);
        });
        return services;
    }

    /// <summary>
    /// Adds a standard durable queue configuration
    /// </summary>
    /// <param name="services">Service collection</param>
    /// <param name="name">Queue name</param>
    /// <param name="exchangeName">Exchange to bind to</param>
    /// <param name="routingKey">Routing key for binding</param>
    /// <param name="durable">Whether queue is durable</param>
    /// <returns>Service collection for fluent configuration</returns>
    public static IServiceCollection AddDurableQueue(this IServiceCollection services,
        string name,
        string exchangeName,
        string routingKey = "",
        bool durable = true)
    {
        return services.AddQueue(queue =>
        {
            queue.Name = name;
            queue.Durable = durable;
            queue.Exclusive = false;
            queue.AutoDelete = false;
            
            if (!string.IsNullOrEmpty(exchangeName))
            {
                queue.Bindings.Add(new QueueBinding
                {
                    Exchange = exchangeName,
                    RoutingKey = routingKey
                });
            }
        });
    }

    /// <summary>
    /// Adds a temporary queue configuration (auto-delete, non-durable)
    /// </summary>
    /// <param name="services">Service collection</param>
    /// <param name="name">Queue name</param>
    /// <param name="exchangeName">Exchange to bind to</param>
    /// <param name="routingKey">Routing key for binding</param>
    /// <returns>Service collection for fluent configuration</returns>
    public static IServiceCollection AddTemporaryQueue(this IServiceCollection services,
        string name,
        string exchangeName,
        string routingKey = "")
    {
        return services.AddQueue(queue =>
        {
            queue.Name = name;
            queue.Durable = false;
            queue.Exclusive = false;
            queue.AutoDelete = true;
            
            if (!string.IsNullOrEmpty(exchangeName))
            {
                queue.Bindings.Add(new QueueBinding
                {
                    Exchange = exchangeName,
                    RoutingKey = routingKey
                });
            }
        });
    }

    /// <summary>
    /// Adds a priority queue configuration
    /// </summary>
    /// <param name="services">Service collection</param>
    /// <param name="name">Queue name</param>
    /// <param name="maxPriority">Maximum priority level (1-255)</param>
    /// <param name="exchangeName">Exchange to bind to</param>
    /// <param name="routingKey">Routing key for binding</param>
    /// <returns>Service collection for fluent configuration</returns>
    public static IServiceCollection AddPriorityQueue(this IServiceCollection services,
        string name,
        int maxPriority,
        string exchangeName,
        string routingKey = "")
    {
        if (maxPriority < 1 || maxPriority > 255)
            throw new ArgumentException("Priority must be between 1 and 255", nameof(maxPriority));

        return services.AddQueue(queue =>
        {
            queue.Name = name;
            queue.Durable = true;
            queue.Arguments["x-max-priority"] = maxPriority;
            
            if (!string.IsNullOrEmpty(exchangeName))
            {
                queue.Bindings.Add(new QueueBinding
                {
                    Exchange = exchangeName,
                    RoutingKey = routingKey
                });
            }
        });
    }

    /// <summary>
    /// Adds a quorum queue configuration (RabbitMQ 3.8+)
    /// </summary>
    /// <param name="services">Service collection</param>
    /// <param name="name">Queue name</param>
    /// <param name="exchangeName">Exchange to bind to</param>
    /// <param name="routingKey">Routing key for binding</param>
    /// <returns>Service collection for fluent configuration</returns>
    public static IServiceCollection AddQuorumQueue(this IServiceCollection services,
        string name,
        string exchangeName,
        string routingKey = "")
    {
        return services.AddQueue(queue =>
        {
            queue.Name = name;
            queue.Durable = true;
            queue.Arguments["x-queue-type"] = "quorum";
            
            if (!string.IsNullOrEmpty(exchangeName))
            {
                queue.Bindings.Add(new QueueBinding
                {
                    Exchange = exchangeName,
                    RoutingKey = routingKey
                });
            }
        });
    }

    /// <summary>
    /// Adds a dead letter queue configuration
    /// </summary>
    /// <param name="services">Service collection</param>
    /// <param name="name">Queue name (default: "dlq")</param>
    /// <param name="exchangeName">Dead letter exchange name (default: "dlx")</param>
    /// <param name="routingKey">Routing key (default: "dead-letter")</param>
    /// <returns>Service collection for fluent configuration</returns>
    public static IServiceCollection AddDeadLetterQueue(this IServiceCollection services,
        string name = "dlq",
        string exchangeName = "dlx", 
        string routingKey = "dead-letter")
    {
        return services.AddDurableQueue(name, exchangeName, routingKey);
    }

    /// <summary>
    /// Configures queue auto-declaration
    /// </summary>
    /// <param name="services">Service collection</param>
    /// <param name="enabled">Whether to auto-declare queues</param>
    /// <returns>Service collection for fluent configuration</returns>
    public static IServiceCollection ConfigureQueueAutoDeclaration(this IServiceCollection services, bool enabled = true)
    {
        services.Configure<RabbitMQConfiguration>(config => config.AutoDeclareQueues = enabled);
        return services;
    }

    /// <summary>
    /// Validates a queue name according to RabbitMQ naming rules
    /// </summary>
    /// <param name="name">Queue name to validate</param>
    /// <returns>True if name is valid</returns>
    public static bool IsValidQueueName(this string name)
    {
        if (string.IsNullOrWhiteSpace(name))
            return false;

        // RabbitMQ queue names cannot be longer than 255 characters
        if (name.Length > 255)
            return false;

        // Check for invalid characters
        var invalidChars = new[] { '\0', '\r', '\n' };
        return !name.Any(c => invalidChars.Contains(c));
    }

    /// <summary>
    /// Creates queue settings from a fluent builder
    /// </summary>
    /// <param name="name">Queue name</param>
    /// <returns>Queue settings builder</returns>
    public static QueueSettingsBuilder CreateQueue(string name)
    {
        return new QueueSettingsBuilder(name);
    }

    /// <summary>
    /// Declares a queue using fluent syntax
    /// </summary>
    /// <param name="queueManager">Queue manager</param>
    /// <param name="name">Queue name</param>
    /// <returns>Queue declaration builder</returns>
    public static QueueDeclarationBuilder DeclareQueue(this IQueueManager queueManager, string name)
    {
        return new QueueDeclarationBuilder(queueManager, name);
    }

    /// <summary>
    /// Creates arguments for a TTL (Time To Live) queue
    /// </summary>
    /// <param name="ttl">Message TTL</param>
    /// <returns>Queue arguments dictionary</returns>
    public static Dictionary<string, object> WithTtl(TimeSpan ttl)
    {
        return new Dictionary<string, object>
        {
            ["x-message-ttl"] = (int)ttl.TotalMilliseconds
        };
    }

    /// <summary>
    /// Creates arguments for a max length queue
    /// </summary>
    /// <param name="maxLength">Maximum number of messages</param>
    /// <returns>Queue arguments dictionary</returns>
    public static Dictionary<string, object> WithMaxLength(long maxLength)
    {
        return new Dictionary<string, object>
        {
            ["x-max-length"] = maxLength
        };
    }

    /// <summary>
    /// Creates arguments for a max length bytes queue
    /// </summary>
    /// <param name="maxBytes">Maximum queue size in bytes</param>
    /// <returns>Queue arguments dictionary</returns>
    public static Dictionary<string, object> WithMaxLengthBytes(long maxBytes)
    {
        return new Dictionary<string, object>
        {
            ["x-max-length-bytes"] = maxBytes
        };
    }

    /// <summary>
    /// Creates arguments for dead letter configuration
    /// </summary>
    /// <param name="deadLetterExchange">Dead letter exchange name</param>
    /// <param name="deadLetterRoutingKey">Dead letter routing key (optional)</param>
    /// <returns>Queue arguments dictionary</returns>
    public static Dictionary<string, object> WithDeadLetter(string deadLetterExchange, string? deadLetterRoutingKey = null)
    {
        var args = new Dictionary<string, object>
        {
            ["x-dead-letter-exchange"] = deadLetterExchange
        };

        if (!string.IsNullOrEmpty(deadLetterRoutingKey))
        {
            args["x-dead-letter-routing-key"] = deadLetterRoutingKey;
        }

        return args;
    }

    /// <summary>
    /// Determines the appropriate queue type based on use case
    /// </summary>
    /// <param name="useCase">Queue use case</param>
    /// <returns>Recommended queue type arguments</returns>
    public static Dictionary<string, object> ForUseCase(QueueUseCase useCase)
    {
        return useCase switch
        {
            QueueUseCase.HighThroughput => new Dictionary<string, object> { ["x-queue-type"] = "quorum" },
            QueueUseCase.LowLatency => new Dictionary<string, object>(), // Classic queue (default)
            QueueUseCase.MessageOrdering => new Dictionary<string, object> { ["x-single-active-consumer"] = true },
            QueueUseCase.TemporaryWorkQueue => new Dictionary<string, object>(),
            QueueUseCase.PriorityProcessing => new Dictionary<string, object> { ["x-max-priority"] = 10 },
            _ => new Dictionary<string, object>()
        };
    }
}