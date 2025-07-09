using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace FS.Transport.AMQP.Configuration;

/// <summary>
/// Extension methods for configuration binding and validation
/// </summary>
public static class ConfigurationExtensions
{
    /// <summary>
    /// Binds and validates RabbitMQ configuration from IConfiguration
    /// </summary>
    /// <param name="configuration">Configuration instance</param>
    /// <param name="sectionName">Configuration section name</param>
    /// <returns>Validated RabbitMQ configuration</returns>
    public static RabbitMQConfiguration GetRabbitMQConfiguration(this IConfiguration configuration, string sectionName = "RabbitMQ")
    {
        var config = new RabbitMQConfiguration();
        configuration.GetSection(sectionName).Bind(config);
        config.Validate();
        return config;
    }

    /// <summary>
    /// Binds connection settings from configuration
    /// </summary>
    /// <param name="configuration">Configuration instance</param>
    /// <param name="sectionName">Configuration section name</param>
    /// <returns>Connection settings</returns>
    public static ConnectionSettings GetConnectionSettings(this IConfiguration configuration, string sectionName = "RabbitMQ:Connection")
    {
        var settings = new ConnectionSettings();
        configuration.GetSection(sectionName).Bind(settings);
        settings.Validate();
        return settings;
    }

    /// <summary>
    /// Gets connection string from configuration or builds it from settings
    /// </summary>
    /// <param name="configuration">Configuration instance</param>
    /// <param name="connectionStringName">Connection string name</param>
    /// <param name="settingsSection">Settings section name</param>
    /// <returns>Connection string</returns>
    public static string GetRabbitMQConnectionString(this IConfiguration configuration, 
        string connectionStringName = "RabbitMQ", 
        string settingsSection = "RabbitMQ:Connection")
    {
        // First try to get from connection strings
        var connectionString = configuration.GetConnectionString(connectionStringName);
        if (!string.IsNullOrEmpty(connectionString))
        {
            return connectionString;
        }

        // If not found, build from settings
        var settings = configuration.GetConnectionSettings(settingsSection);
        var builder = new ConnectionStringBuilder(settings);
        return builder.Build(includeQueryParameters: true);
    }

    /// <summary>
    /// Configures RabbitMQ options with validation
    /// </summary>
    /// <param name="services">Service collection</param>
    /// <param name="configuration">Configuration instance</param>
    /// <param name="sectionName">Configuration section name</param>
    /// <returns>Options builder for further configuration</returns>
    public static OptionsBuilder<RabbitMQConfiguration> ConfigureRabbitMQ(this IServiceCollection services, 
        IConfiguration configuration, 
        string sectionName = "RabbitMQ")
    {
        return services.AddOptions<RabbitMQConfiguration>()
            .Bind(configuration.GetSection(sectionName))
            .ValidateDataAnnotations()
            .Validate(config =>
            {
                try
                {
                    config.Validate();
                    return true;
                }
                catch
                {
                    return false;
                }
            }, "RabbitMQ configuration validation failed");
    }

    /// <summary>
    /// Creates a fluent configuration builder for RabbitMQ settings
    /// </summary>
    /// <param name="services">Service collection</param>
    /// <returns>RabbitMQ configuration builder</returns>
    public static RabbitMQConfigurationBuilder AddRabbitMQConfiguration(this IServiceCollection services)
    {
        return new RabbitMQConfigurationBuilder(services);
    }
}