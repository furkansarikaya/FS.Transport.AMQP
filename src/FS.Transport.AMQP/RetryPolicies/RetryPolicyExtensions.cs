using System.Net.Sockets;
using FS.Transport.AMQP.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace FS.Transport.AMQP.RetryPolicies;

/// <summary>
/// Extension methods for retry policy configuration and usage
/// </summary>
public static class RetryPolicyExtensions
{
    /// <summary>
    /// Adds retry policy services to the service collection
    /// </summary>
    /// <param name="services">Service collection</param>
    /// <returns>Service collection for fluent configuration</returns>
    public static IServiceCollection AddRetryPolicies(this IServiceCollection services)
    {
        services.TryAddTransient<IRetryPolicyFactory, RetryPolicyFactory>();
        services.TryAddTransient<ExponentialBackoffRetryPolicy>();
        services.TryAddTransient<LinearRetryPolicy>();
        services.TryAddTransient<CircuitBreakerRetryPolicy>();
        
        return services;
    }

    /// <summary>
    /// Configures the default retry policy
    /// </summary>
    /// <param name="services">Service collection</param>
    /// <param name="configureRetry">Retry configuration action</param>
    /// <returns>Service collection for fluent configuration</returns>
    public static IServiceCollection ConfigureRetryPolicy(this IServiceCollection services, 
        Action<RetryPolicySettings> configureRetry)
    {
        services.Configure<RabbitMQConfiguration>(config => configureRetry(config.RetryPolicy));
        return services;
    }

    /// <summary>
    /// Sets the retry policy type
    /// </summary>
    /// <param name="services">Service collection</param>
    /// <param name="policyType">Policy type (None, Linear, Exponential, CircuitBreaker)</param>
    /// <returns>Service collection for fluent configuration</returns>
    public static IServiceCollection UseRetryPolicy(this IServiceCollection services, string policyType)
    {
        services.Configure<RabbitMQConfiguration>(config => config.RetryPolicy.PolicyType = policyType);
        return services;
    }

    /// <summary>
    /// Configures exponential backoff retry policy
    /// </summary>
    /// <param name="services">Service collection</param>
    /// <param name="maxRetries">Maximum retry attempts</param>
    /// <param name="initialDelayMs">Initial delay in milliseconds</param>
    /// <param name="maxDelayMs">Maximum delay in milliseconds</param>
    /// <param name="backoffMultiplier">Backoff multiplier</param>
    /// <returns>Service collection for fluent configuration</returns>
    public static IServiceCollection UseExponentialBackoff(this IServiceCollection services,
        int maxRetries = 3,
        int initialDelayMs = 1000,
        int maxDelayMs = 30000,
        double backoffMultiplier = 2.0)
    {
        services.Configure<RabbitMQConfiguration>(config =>
        {
            config.RetryPolicy.PolicyType = "Exponential";
            config.RetryPolicy.MaxRetries = maxRetries;
            config.RetryPolicy.InitialDelayMs = initialDelayMs;
            config.RetryPolicy.MaxDelayMs = maxDelayMs;
            config.RetryPolicy.BackoffMultiplier = backoffMultiplier;
        });
        return services;
    }

    /// <summary>
    /// Configures linear retry policy
    /// </summary>
    /// <param name="services">Service collection</param>
    /// <param name="maxRetries">Maximum retry attempts</param>
    /// <param name="fixedDelayMs">Fixed delay between retries in milliseconds</param>
    /// <returns>Service collection for fluent configuration</returns>
    public static IServiceCollection UseLinearRetry(this IServiceCollection services,
        int maxRetries = 3,
        int fixedDelayMs = 1000)
    {
        services.Configure<RabbitMQConfiguration>(config =>
        {
            config.RetryPolicy.PolicyType = "Linear";
            config.RetryPolicy.MaxRetries = maxRetries;
            config.RetryPolicy.InitialDelayMs = fixedDelayMs;
        });
        return services;
    }

    /// <summary>
    /// Configures circuit breaker retry policy
    /// </summary>
    /// <param name="services">Service collection</param>
    /// <param name="failureThreshold">Failure threshold before opening circuit</param>
    /// <param name="recoveryTimeoutMs">Recovery timeout in milliseconds</param>
    /// <param name="maxRetries">Maximum retry attempts</param>
    /// <returns>Service collection for fluent configuration</returns>
    public static IServiceCollection UseCircuitBreaker(this IServiceCollection services,
        int failureThreshold = 5,
        int recoveryTimeoutMs = 60000,
        int maxRetries = 3)
    {
        services.Configure<RabbitMQConfiguration>(config =>
        {
            config.RetryPolicy.PolicyType = "CircuitBreaker";
            config.RetryPolicy.CircuitBreakerFailureThreshold = failureThreshold;
            config.RetryPolicy.CircuitBreakerRecoveryTimeoutMs = recoveryTimeoutMs;
            config.RetryPolicy.MaxRetries = maxRetries;
        });
        return services;
    }

    /// <summary>
    /// Executes an operation with retry logic using the specified policy
    /// </summary>
    /// <typeparam name="T">Return type</typeparam>
    /// <param name="retryPolicy">Retry policy to use</param>
    /// <param name="operation">Operation to execute</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Operation result</returns>
    public static async Task<T> ExecuteWithRetryAsync<T>(this IRetryPolicy retryPolicy, 
        Func<Task<T>> operation, 
        CancellationToken cancellationToken = default)
    {
        return await retryPolicy.ExecuteAsync(operation, cancellationToken);
    }

    /// <summary>
    /// Executes an operation with retry logic using the specified policy
    /// </summary>
    /// <param name="retryPolicy">Retry policy to use</param>
    /// <param name="operation">Operation to execute</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Task representing the operation</returns>
    public static async Task ExecuteWithRetryAsync(this IRetryPolicy retryPolicy, 
        Func<Task> operation, 
        CancellationToken cancellationToken = default)
    {
        await retryPolicy.ExecuteAsync(operation, cancellationToken);
    }

    /// <summary>
    /// Creates a retry policy based on settings
    /// </summary>
    /// <param name="settings">Retry settings</param>
    /// <param name="serviceProvider">Service provider for dependency resolution</param>
    /// <returns>Configured retry policy</returns>
    public static IRetryPolicy CreateRetryPolicy(this RetryPolicySettings settings, IServiceProvider serviceProvider)
    {
        var factory = serviceProvider.GetRequiredService<IRetryPolicyFactory>();
        return factory.CreateRetryPolicy(settings);
    }

    /// <summary>
    /// Determines if an exception should be retried based on its type
    /// </summary>
    /// <param name="exception">Exception to check</param>
    /// <param name="settings">Retry settings with custom exception configuration</param>
    /// <returns>True if the exception should be retried</returns>
    public static bool IsRetryable(this Exception exception, RetryPolicySettings settings)
    {
        var exceptionTypeName = exception.GetType().FullName ?? exception.GetType().Name;
        
        // Check explicit non-retryable exceptions
        if (settings.NonRetryableExceptions.Contains(exceptionTypeName))
            return false;
            
        // Check explicit retryable exceptions
        if (settings.RetryableExceptions.Contains(exceptionTypeName))
            return true;
            
        // Use default logic
        return IsDefaultRetryableException(exception);
    }

    private static bool IsDefaultRetryableException(Exception exception)
    {
        return exception switch
        {
            TimeoutException => true,
            SocketException => true,
            HttpRequestException => true,
            TaskCanceledException => false,
            OperationCanceledException => false,
            RabbitMQ.Client.Exceptions.BrokerUnreachableException => true,
            RabbitMQ.Client.Exceptions.ConnectFailureException => true,
            RabbitMQ.Client.Exceptions.AlreadyClosedException => true,
            ArgumentException => false,
            InvalidOperationException => false,
            _ => true
        };
    }
}