using FS.RabbitMQ.Core.Abstractions;

namespace FS.RabbitMQ.Core.Configuration;

/// <summary>
/// Contains configuration for consumer health monitoring and checks.
/// </summary>
public sealed class ConsumerHealthCheckConfiguration
{
    /// <summary>
    /// Gets or sets a value indicating whether health checks are enabled.
    /// </summary>
    /// <value><c>true</c> to enable health checks; otherwise, <c>false</c>. Default is <c>true</c>.</value>
    public bool Enabled { get; set; } = true;

    /// <summary>
    /// Gets or sets the interval between health checks.
    /// </summary>
    /// <value>The interval between health check executions. Default is 30 seconds.</value>
    public TimeSpan CheckInterval { get; set; } = TimeSpan.FromSeconds(30);

    /// <summary>
    /// Gets or sets the timeout for health check operations.
    /// </summary>
    /// <value>The maximum time for a health check to complete. Default is 10 seconds.</value>
    public TimeSpan CheckTimeout { get; set; } = TimeSpan.FromSeconds(10);

    /// <summary>
    /// Creates a default health check configuration.
    /// </summary>
    /// <returns>A health check configuration with default settings.</returns>
    public static ConsumerHealthCheckConfiguration CreateDefault()
    {
        return new ConsumerHealthCheckConfiguration
        {
            Enabled = true,
            CheckInterval = TimeSpan.FromSeconds(30),
            CheckTimeout = TimeSpan.FromSeconds(10)
        };
    }

    /// <summary>
    /// Validates the health check configuration.
    /// </summary>
    /// <exception cref="BrokerConfigurationException">Thrown when the configuration is invalid.</exception>
    public void Validate()
    {
        if (CheckInterval <= TimeSpan.Zero)
        {
            throw new BrokerConfigurationException(
                "Health check interval must be positive.",
                configurationSection: nameof(ConsumerHealthCheckConfiguration),
                parameterName: nameof(CheckInterval),
                parameterValue: CheckInterval);
        }

        if (CheckTimeout <= TimeSpan.Zero)
        {
            throw new BrokerConfigurationException(
                "Health check timeout must be positive.",
                configurationSection: nameof(ConsumerHealthCheckConfiguration),
                parameterName: nameof(CheckTimeout),
                parameterValue: CheckTimeout);
        }
    }
}