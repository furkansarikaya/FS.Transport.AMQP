using FS.Transport.AMQP.Configuration;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace FS.Transport.AMQP.Core.Extensions;

/// <summary>
/// Extension methods for IServiceCollection to register RabbitMQ services
/// </summary>
public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Adds RabbitMQ services to the service collection with default configuration
    /// </summary>
    /// <param name="services">Service collection</param>
    /// <param name="configuration">Configuration section for RabbitMQ</param>
    /// <returns>Service collection for fluent configuration</returns>
    public static IServiceCollection AddRabbitMQ(this IServiceCollection services, IConfiguration configuration)
    {
        ArgumentNullException.ThrowIfNull(services);
        ArgumentNullException.ThrowIfNull(configuration);

        // Register configuration
        services.Configure<RabbitMQConfiguration>(configuration.GetSection("RabbitMQ"));

        // Register core services
        return services.AddRabbitMQCore();
    }

    /// <summary>
    /// Adds RabbitMQ services to the service collection with custom configuration
    /// </summary>
    /// <param name="services">Service collection</param>
    /// <param name="configureOptions">Configuration action</param>
    /// <returns>Service collection for fluent configuration</returns>
    public static IServiceCollection AddRabbitMQ(this IServiceCollection services, Action<RabbitMQConfiguration> configureOptions)
    {
        ArgumentNullException.ThrowIfNull(services);
        ArgumentNullException.ThrowIfNull(configureOptions);

        // Register configuration
        services.Configure(configureOptions);

        // Register core services
        return services.AddRabbitMQCore();
    }

    /// <summary>
    /// Adds core RabbitMQ services to the service collection
    /// </summary>
    /// <param name="services">Service collection</param>
    /// <returns>Service collection for fluent configuration</returns>
    private static IServiceCollection AddRabbitMQCore(this IServiceCollection services)
    {
        // Register factory as singleton
        services.TryAddSingleton<IRabbitMQClientFactory, RabbitMQClientFactory>();
        
        // Register client as scoped (one per scope/request)
        services.TryAddScoped<IRabbitMQClient, RabbitMQClient>();

        return services;
    }

    /// <summary>
    /// Adds RabbitMQ health checks to the service collection
    /// </summary>
    /// <param name="services">Service collection</param>
    /// <param name="name">Health check name</param>
    /// <param name="tags">Health check tags</param>
    /// <returns>Service collection for fluent configuration</returns>
    public static IServiceCollection AddRabbitMQHealthChecks(this IServiceCollection services, 
        string name = "rabbitmq", 
        params string[] tags)
    {
        services.AddHealthChecks()
            .AddCheck<RabbitMQHealthCheck>(name, tags: tags);

        return services;
    }
}