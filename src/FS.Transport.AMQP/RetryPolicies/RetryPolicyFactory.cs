using FS.Transport.AMQP.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace FS.Transport.AMQP.RetryPolicies;

/// <summary>
/// Default implementation of retry policy factory
/// </summary>
public class RetryPolicyFactory : IRetryPolicyFactory
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<RetryPolicyFactory> _logger;

    public RetryPolicyFactory(IServiceProvider serviceProvider, ILogger<RetryPolicyFactory> logger)
    {
        _serviceProvider = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>
    /// Creates a retry policy based on the specified settings
    /// </summary>
    public IRetryPolicy CreateRetryPolicy(RetryPolicySettings settings)
    {
        if (settings == null)
            throw new ArgumentNullException(nameof(settings));

        settings.Validate();
        return CreateRetryPolicy(settings.PolicyType, settings);
    }

    /// <summary>
    /// Creates a retry policy of the specified type
    /// </summary>
    public IRetryPolicy CreateRetryPolicy(string policyType, RetryPolicySettings settings)
    {
        if (string.IsNullOrWhiteSpace(policyType))
            throw new ArgumentException("Policy type cannot be null or empty", nameof(policyType));
        if (settings == null)
            throw new ArgumentNullException(nameof(settings));

        _logger.LogDebug("Creating retry policy of type: {PolicyType}", policyType);

        // Convert to RetryPolicySettings for compatibility
        var policySettings = new RetryPolicySettings
        {
            MaxRetries = settings.MaxRetries,
            InitialDelayMs = settings.InitialDelayMs,
            MaxDelayMs = settings.MaxDelayMs,
            BackoffMultiplier = settings.BackoffMultiplier,
            JitterFactor = settings.JitterFactor,
            CircuitBreakerFailureThreshold = settings.CircuitBreakerFailureThreshold,
            CircuitBreakerRecoveryTimeoutMs = settings.CircuitBreakerRecoveryTimeoutMs
        };

        return policyType.ToLowerInvariant() switch
        {
            "none" => new NoRetryPolicy(),
            "linear" => new LinearRetryPolicy(policySettings, 
                _serviceProvider.GetRequiredService<ILogger<LinearRetryPolicy>>()),
            "exponential" => new ExponentialBackoffRetryPolicy(policySettings, 
                _serviceProvider.GetRequiredService<ILogger<ExponentialBackoffRetryPolicy>>()),
            "circuitbreaker" => new CircuitBreakerRetryPolicy(policySettings, 
                _serviceProvider.GetRequiredService<ILogger<CircuitBreakerRetryPolicy>>()),
            _ => throw new ArgumentException($"Unknown retry policy type: {policyType}", nameof(policyType))
        };
    }
}