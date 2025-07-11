using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Logging;

namespace FS.RabbitMQ.Core;

/// <summary>
/// Health check implementation for RabbitMQ connectivity
/// </summary>
public class RabbitMQHealthCheck : IHealthCheck
{
    private readonly IRabbitMQClientFactory _clientFactory;
    private readonly ILogger<RabbitMQHealthCheck> _logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="RabbitMQHealthCheck"/> class
    /// </summary>
    /// <param name="clientFactory">Factory for creating RabbitMQ clients</param>
    /// <param name="logger">Logger for health check activities</param>
    /// <exception cref="ArgumentNullException">
    /// Thrown when clientFactory or logger is null
    /// </exception>
    public RabbitMQHealthCheck(IRabbitMQClientFactory clientFactory, ILogger<RabbitMQHealthCheck> logger)
    {
        _clientFactory = clientFactory ?? throw new ArgumentNullException(nameof(clientFactory));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>
    /// Performs a health check by testing RabbitMQ connectivity
    /// </summary>
    /// <param name="context">Health check context</param>
    /// <param name="cancellationToken">Token to monitor for cancellation requests</param>
    /// <returns>
    /// A task that represents the asynchronous health check operation.
    /// The task result contains the health check result.
    /// </returns>
    public async Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
    {
        try
        {
            var client = _clientFactory.CreateClient();
            
            // Test connection
            await client.InitializeAsync(cancellationToken);
            
            // Check if connection is healthy
            if (client.ConnectionManager.IsConnected)
            {
                return HealthCheckResult.Healthy("RabbitMQ connection is healthy");
            }
            else
            {
                return HealthCheckResult.Unhealthy("RabbitMQ connection is not established");
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "RabbitMQ health check failed");
            return HealthCheckResult.Unhealthy("RabbitMQ health check failed", ex);
        }
    }
}