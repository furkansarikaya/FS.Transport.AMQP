using FS.RabbitMQ.Configuration;

namespace FS.RabbitMQ.Core;

/// <summary>
/// Factory interface for creating RabbitMQ client instances
/// </summary>
public interface IRabbitMQClientFactory
{
    /// <summary>
    /// Creates a new RabbitMQ client instance
    /// </summary>
    /// <returns>Configured RabbitMQ client</returns>
    IRabbitMQClient CreateClient();
    
    /// <summary>
    /// Creates a new RabbitMQ client instance with custom configuration
    /// </summary>
    /// <param name="configurationAction">Configuration customization action</param>
    /// <returns>Configured RabbitMQ client</returns>
    IRabbitMQClient CreateClient(Action<RabbitMQConfiguration> configurationAction);
}