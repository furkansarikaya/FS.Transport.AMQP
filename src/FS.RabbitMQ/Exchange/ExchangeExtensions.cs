using FS.RabbitMQ.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using RabbitMQ.Client;

namespace FS.RabbitMQ.Exchange;

/// <summary>
/// Extension methods for exchange management and configuration
/// </summary>
public static class ExchangeExtensions
{
    /// <summary>
    /// Adds exchange management services to the service collection
    /// </summary>
    /// <param name="services">Service collection</param>
    /// <returns>Service collection for fluent configuration</returns>
    public static IServiceCollection AddExchangeManager(this IServiceCollection services)
    {
        services.TryAddSingleton<IExchangeManager, ExchangeManager>();
        return services;
    }

    /// <summary>
    /// Adds an exchange configuration
    /// </summary>
    /// <param name="services">Service collection</param>
    /// <param name="configureExchange">Exchange configuration action</param>
    /// <returns>Service collection for fluent configuration</returns>
    public static IServiceCollection AddExchange(this IServiceCollection services, 
        Action<ExchangeSettings> configureExchange)
    {
        services.Configure<RabbitMQConfiguration>(config =>
        {
            var exchange = new ExchangeSettings();
            configureExchange(exchange);
            config.Exchanges.Add(exchange);
        });
        return services;
    }

    /// <summary>
    /// Adds a topic exchange configuration
    /// </summary>
    /// <param name="services">Service collection</param>
    /// <param name="name">Exchange name</param>
    /// <param name="durable">Whether exchange is durable</param>
    /// <param name="autoDelete">Whether exchange auto-deletes</param>
    /// <returns>Service collection for fluent configuration</returns>
    public static IServiceCollection AddTopicExchange(this IServiceCollection services,
        string name,
        bool durable = true,
        bool autoDelete = false)
    {
        return services.AddExchange(exchange =>
        {
            exchange.Name = name;
            exchange.Type = ExchangeType.Topic;
            exchange.Durable = durable;
            exchange.AutoDelete = autoDelete;
        });
    }

    /// <summary>
    /// Adds a direct exchange configuration
    /// </summary>
    /// <param name="services">Service collection</param>
    /// <param name="name">Exchange name</param>
    /// <param name="durable">Whether exchange is durable</param>
    /// <param name="autoDelete">Whether exchange auto-deletes</param>
    /// <returns>Service collection for fluent configuration</returns>
    public static IServiceCollection AddDirectExchange(this IServiceCollection services,
        string name,
        bool durable = true,
        bool autoDelete = false)
    {
        return services.AddExchange(exchange =>
        {
            exchange.Name = name;
            exchange.Type = ExchangeType.Direct;
            exchange.Durable = durable;
            exchange.AutoDelete = autoDelete;
        });
    }

    /// <summary>
    /// Adds a fanout exchange configuration
    /// </summary>
    /// <param name="services">Service collection</param>
    /// <param name="name">Exchange name</param>
    /// <param name="durable">Whether exchange is durable</param>
    /// <param name="autoDelete">Whether exchange auto-deletes</param>
    /// <returns>Service collection for fluent configuration</returns>
    public static IServiceCollection AddFanoutExchange(this IServiceCollection services,
        string name,
        bool durable = true,
        bool autoDelete = false)
    {
        return services.AddExchange(exchange =>
        {
            exchange.Name = name;
            exchange.Type = ExchangeType.Fanout;
            exchange.Durable = durable;
            exchange.AutoDelete = autoDelete;
        });
    }

    /// <summary>
    /// Adds a headers exchange configuration
    /// </summary>
    /// <param name="services">Service collection</param>
    /// <param name="name">Exchange name</param>
    /// <param name="durable">Whether exchange is durable</param>
    /// <param name="autoDelete">Whether exchange auto-deletes</param>
    /// <returns>Service collection for fluent configuration</returns>
    public static IServiceCollection AddHeadersExchange(this IServiceCollection services,
        string name,
        bool durable = true,
        bool autoDelete = false)
    {
        return services.AddExchange(exchange =>
        {
            exchange.Name = name;
            exchange.Type = ExchangeType.Headers;
            exchange.Durable = durable;
            exchange.AutoDelete = autoDelete;
        });
    }

    /// <summary>
    /// Adds a dead letter exchange configuration
    /// </summary>
    /// <param name="services">Service collection</param>
    /// <param name="name">Exchange name (default: "dlx")</param>
    /// <returns>Service collection for fluent configuration</returns>
    public static IServiceCollection AddDeadLetterExchange(this IServiceCollection services, string name = "dlx")
    {
        return services.AddTopicExchange(name, durable: true, autoDelete: false);
    }

    /// <summary>
    /// Configures exchange auto-declaration
    /// </summary>
    /// <param name="services">Service collection</param>
    /// <param name="enabled">Whether to auto-declare exchanges</param>
    /// <returns>Service collection for fluent configuration</returns>
    public static IServiceCollection ConfigureExchangeAutoDeclaration(this IServiceCollection services, bool enabled = true)
    {
        services.Configure<RabbitMQConfiguration>(config => config.AutoDeclareExchanges = enabled);
        return services;
    }

    /// <summary>
    /// Validates an exchange name according to RabbitMQ naming rules
    /// </summary>
    /// <param name="name">Exchange name to validate</param>
    /// <returns>True if name is valid</returns>
    public static bool IsValidExchangeName(this string name)
    {
        if (string.IsNullOrWhiteSpace(name))
            return false;

        // RabbitMQ exchange names cannot be longer than 255 characters
        if (name.Length > 255)
            return false;

        // Check for invalid characters (basic validation)
        var invalidChars = new[] { '\0', '\r', '\n' };
        return !name.Any(c => invalidChars.Contains(c));
    }

    /// <summary>
    /// Validates an exchange type
    /// </summary>
    /// <param name="type">Exchange type to validate</param>
    /// <returns>True if type is valid</returns>
    public static bool IsValidExchangeType(this string type)
    {
        if (string.IsNullOrWhiteSpace(type))
            return false;

        var validTypes = new[] { ExchangeType.Direct, ExchangeType.Topic, ExchangeType.Fanout, ExchangeType.Headers };
        return validTypes.Contains(type, StringComparer.OrdinalIgnoreCase);
    }

    /// <summary>
    /// Creates exchange settings from a fluent builder
    /// </summary>
    /// <param name="name">Exchange name</param>
    /// <returns>Exchange settings builder</returns>
    public static ExchangeSettingsBuilder CreateExchange(string name)
    {
        return new ExchangeSettingsBuilder(name);
    }

    /// <summary>
    /// Declares an exchange using fluent syntax
    /// </summary>
    /// <param name="exchangeManager">Exchange manager</param>
    /// <param name="name">Exchange name</param>
    /// <returns>Exchange declaration builder</returns>
    public static ExchangeDeclarationBuilder DeclareExchange(this IExchangeManager exchangeManager, string name)
    {
        return new ExchangeDeclarationBuilder(exchangeManager, name);
    }

    /// <summary>
    /// Gets the default routing key pattern for an exchange type
    /// </summary>
    /// <param name="exchangeType">Exchange type</param>
    /// <returns>Default routing key pattern</returns>
    public static string GetDefaultRoutingKeyPattern(this string exchangeType)
    {
        return exchangeType.ToLowerInvariant() switch
        {
            ExchangeType.Direct => "*",
            ExchangeType.Topic => "#",
            ExchangeType.Fanout => "", // Fanout ignores routing keys
            ExchangeType.Headers => "", // Headers uses header matching
            _ => "#" // Default to topic-style
        };
    }
}