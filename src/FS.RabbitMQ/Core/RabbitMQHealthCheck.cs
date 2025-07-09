using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Logging;

namespace FS.RabbitMQ.Core;

/// <summary>
/// Health check implementation for RabbitMQ connection
/// </summary>
public class RabbitMQHealthCheck : IHealthCheck
{
    private readonly IRabbitMQClientFactory _clientFactory;
    private readonly ILogger<RabbitMQHealthCheck> _logger;

    public RabbitMQHealthCheck(IRabbitMQClientFactory clientFactory, ILogger<RabbitMQHealthCheck> logger)
    {
        _clientFactory = clientFactory ?? throw new ArgumentNullException(nameof(clientFactory));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>
    /// Performs the health check by testing RabbitMQ connectivity
    /// </summary>
    /// <param name="context">Health check context</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Health check result</returns>
    public async Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
    {
        try
        {
            using var client = _clientFactory.CreateClient();
            
            // Check if connection manager is available and connected
            if (client.ConnectionManager == null)
            {
                return HealthCheckResult.Unhealthy("Connection manager is not available");
            }

            if (!client.ConnectionManager.IsConnected)
            {
                // Try to connect
                var connected = await client.ConnectionManager.ConnectAsync(cancellationToken);
                if (!connected)
                {
                    return HealthCheckResult.Unhealthy("Unable to establish connection to RabbitMQ");
                }
            }

            // Perform a lightweight operation to verify the connection is working
            using var channel = await client.ConnectionManager.GetChannelAsync();
            if (channel == null || !channel.IsOpen)
            {
                return HealthCheckResult.Unhealthy("Unable to create channel");
            }

            var data = new Dictionary<string, object>
            {
                ["status"] = client.Status.ToString(),
                ["connected"] = client.ConnectionManager.IsConnected,
                ["timestamp"] = DateTime.UtcNow
            };

            return HealthCheckResult.Healthy("RabbitMQ connection is healthy", data);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "RabbitMQ health check failed");
            
            var data = new Dictionary<string, object>
            {
                ["error"] = ex.Message,
                ["timestamp"] = DateTime.UtcNow
            };

            return HealthCheckResult.Unhealthy("RabbitMQ health check failed", ex, data);
        }
    }
}