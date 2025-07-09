using FS.RabbitMQ.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using RabbitMQ.Client.Exceptions;

namespace FS.RabbitMQ.Core;

/// <summary>
/// Factory for creating RabbitMQ client instances with proper dependency injection
/// </summary>
public class RabbitMQClientFactory(IServiceProvider serviceProvider, ILogger<RabbitMQClientFactory> logger)
    : IRabbitMQClientFactory
{
    private readonly IServiceProvider _serviceProvider = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));
    private readonly ILogger<RabbitMQClientFactory> _logger = logger ?? throw new ArgumentNullException(nameof(logger));

    /// <summary>
    /// Creates a new RabbitMQ client instance
    /// </summary>
    /// <returns>Configured RabbitMQ client</returns>
    public IRabbitMQClient CreateClient()
    {
        try
        {
            _logger.LogDebug("Creating new RabbitMQ client instance");
            return _serviceProvider.GetRequiredService<IRabbitMQClient>();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to create RabbitMQ client instance");
            throw new RabbitMQClientException("Failed to create RabbitMQ client", ex);
        }
    }

    /// <summary>
    /// Creates a new RabbitMQ client instance with custom configuration
    /// </summary>
    /// <param name="configurationAction">Configuration customization action</param>
    /// <returns>Configured RabbitMQ client</returns>
    public IRabbitMQClient CreateClient(Action<RabbitMQConfiguration> configurationAction)
    {
        if (configurationAction == null)
            throw new ArgumentNullException(nameof(configurationAction));

        try
        {
            // Create a scoped service provider with custom configuration
            var scope = _serviceProvider.CreateScope();
            var client = scope.ServiceProvider.GetRequiredService<IRabbitMQClient>();
            
            _logger.LogDebug("Created RabbitMQ client instance with custom configuration");
            return client;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to create RabbitMQ client with custom configuration");
            throw new RabbitMQClientException("Failed to create RabbitMQ client with custom configuration", ex);
        }
    }
}