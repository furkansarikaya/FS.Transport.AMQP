using Microsoft.Extensions.DependencyInjection;
using FS.Transport.AMQP.Configuration;
using FS.Transport.AMQP.Connection;
using FS.Transport.AMQP.Consumer;
using FS.Transport.AMQP.Producer;
using FS.Transport.AMQP.Queue;
using FS.Transport.AMQP.Exchange;
using FS.Transport.AMQP.Saga;
using FS.Transport.AMQP.EventBus;
using FS.Transport.AMQP.EventStore;
using FS.Transport.AMQP.Monitoring;
using FS.Transport.AMQP.Serialization;
using FS.Transport.AMQP.ErrorHandling;
using FS.Transport.AMQP.RetryPolicies;
using FS.Transport.AMQP.Core;
using FS.Mediator.Features.NotificationHandling.Core;
using FS.Mediator.Features.RequestHandling.Core;

namespace FS.Transport.AMQP.DependencyInjection;

/// <summary>
/// Provides service lifetime management and recommendations for RabbitMQ services.
/// This class contains mappings of service types to their recommended service lifetimes
/// for optimal performance and resource utilization.
/// </summary>
/// <remarks>
/// <para>
/// Service lifetimes in dependency injection containers determine how long service instances
/// live and how they are shared across the application. The three main lifetimes are:
/// </para>
/// <list type="bullet">
/// <item><description><strong>Singleton</strong>: One instance per application lifetime</description></item>
/// <item><description><strong>Scoped</strong>: One instance per request/scope</description></item>
/// <item><description><strong>Transient</strong>: New instance every time it's requested</description></item>
/// </list>
/// <para>
/// This class provides recommendations based on the nature of RabbitMQ services:
/// </para>
/// <list type="bullet">
/// <item><description>Connection-related services are typically <strong>Scoped</strong> or <strong>Singleton</strong></description></item>
/// <item><description>Configuration services are <strong>Singleton</strong></description></item>
/// <item><description>Message handlers are usually <strong>Scoped</strong> or <strong>Transient</strong></description></item>
/// <item><description>Factory services are <strong>Singleton</strong></description></item>
/// </list>
/// </remarks>
public static class ServiceLifetimeConstants
{
    /// <summary>
    /// Default service lifetime for RabbitMQ services when no specific recommendation exists.
    /// </summary>
    /// <remarks>
    /// Scoped lifetime is chosen as the default because it provides a good balance between
    /// performance and resource utilization for most messaging scenarios.
    /// </remarks>
    public static readonly ServiceLifetime DefaultLifetime = ServiceLifetime.Scoped;

    /// <summary>
    /// Recommended service lifetimes for specific service types.
    /// </summary>
    /// <remarks>
    /// This dictionary contains carefully chosen service lifetimes based on the nature of each
    /// RabbitMQ service types. It's used by the GetRecommendedLifetime method.
    /// </remarks>
    public static readonly IReadOnlyDictionary<Type, ServiceLifetime> RecommendedLifetimes = 
        new Dictionary<Type, ServiceLifetime>
        {
            // Core services
            { typeof(IRabbitMQClientFactory), ServiceLifetime.Singleton },
            { typeof(RabbitMQClientFactory), ServiceLifetime.Singleton },
            { typeof(IRabbitMQClient), ServiceLifetime.Scoped },
            { typeof(RabbitMQClient), ServiceLifetime.Scoped },

            // Configuration services
            { typeof(IRabbitMQConfiguration), ServiceLifetime.Singleton },
            { typeof(RabbitMQConfiguration), ServiceLifetime.Singleton },
            { typeof(ConnectionSettings), ServiceLifetime.Singleton },
            { typeof(ProducerSettings), ServiceLifetime.Singleton },
            { typeof(FS.Transport.AMQP.Configuration.ConsumerSettings), ServiceLifetime.Singleton },
            { typeof(FS.Transport.AMQP.Configuration.EventBusSettings), ServiceLifetime.Singleton },
            { typeof(FS.Transport.AMQP.Configuration.EventStoreSettings), ServiceLifetime.Singleton },
            { typeof(SagaSettings), ServiceLifetime.Singleton },
            { typeof(HealthCheckSettings), ServiceLifetime.Singleton },

            // Connection services
            { typeof(IConnectionManager), ServiceLifetime.Scoped },
            { typeof(ConnectionManager), ServiceLifetime.Scoped },
            { typeof(ConnectionPool), ServiceLifetime.Singleton },

            // Queue and Exchange services
            { typeof(IQueueManager), ServiceLifetime.Scoped },
            { typeof(QueueManager), ServiceLifetime.Scoped },
            { typeof(IExchangeManager), ServiceLifetime.Scoped },
            { typeof(ExchangeManager), ServiceLifetime.Scoped },

            // Messaging services
            { typeof(IMessageProducer), ServiceLifetime.Scoped },
            { typeof(MessageProducer), ServiceLifetime.Scoped },
            { typeof(IMessageConsumer), ServiceLifetime.Scoped },
            { typeof(MessageConsumer), ServiceLifetime.Scoped },

            // Serialization services
            { typeof(IMessageSerializer), ServiceLifetime.Singleton },
            { typeof(JsonMessageSerializer), ServiceLifetime.Singleton },

            // Error handling services
            { typeof(IErrorHandler), ServiceLifetime.Scoped },
            { typeof(ErrorHandler), ServiceLifetime.Scoped },
            { typeof(IDeadLetterHandler), ServiceLifetime.Scoped },
            { typeof(DeadLetterHandler), ServiceLifetime.Scoped },

            // Retry policy services
            { typeof(IRetryPolicy), ServiceLifetime.Transient },
            { typeof(IRetryPolicyFactory), ServiceLifetime.Singleton },
            { typeof(RetryPolicyFactory), ServiceLifetime.Singleton },
            { typeof(ExponentialBackoffRetryPolicy), ServiceLifetime.Transient },
            { typeof(LinearRetryPolicy), ServiceLifetime.Transient },
            { typeof(CircuitBreakerRetryPolicy), ServiceLifetime.Transient },

            // Event bus services
            { typeof(IEventBus), ServiceLifetime.Scoped },
            { typeof(EventBus.EventBus), ServiceLifetime.Scoped },
            { typeof(IEventPublisher), ServiceLifetime.Scoped },
            { typeof(EventPublisher), ServiceLifetime.Scoped },
            { typeof(IEventSubscriber), ServiceLifetime.Scoped },
            { typeof(EventSubscriber), ServiceLifetime.Scoped },

            // Event store services
            { typeof(IEventStore), ServiceLifetime.Scoped },
            { typeof(EventStore.EventStore), ServiceLifetime.Scoped },

            // Saga services
            { typeof(ISagaOrchestrator), ServiceLifetime.Scoped },
            { typeof(SagaOrchestrator), ServiceLifetime.Scoped },

            // Monitoring services
            { typeof(IHealthChecker), ServiceLifetime.Scoped },
            { typeof(HealthChecker), ServiceLifetime.Scoped },
            { typeof(IMetricsCollector), ServiceLifetime.Singleton },
            { typeof(MetricsCollector), ServiceLifetime.Singleton },

            // CQRS Handlers - These are typically transient for clean state
            { typeof(IRequestHandler<,>), ServiceLifetime.Transient },
            { typeof(INotificationHandler<>), ServiceLifetime.Transient }
        };

    /// <summary>
    /// Gets the recommended service lifetime for a given service type.
    /// </summary>
    /// <param name="serviceType">The service type to get the recommendation for</param>
    /// <returns>The recommended service lifetime</returns>
    /// <remarks>
    /// This method checks the RecommendedLifetimes dictionary first. If no specific
    /// recommendation exists, it returns the DefaultLifetime.
    /// </remarks>
    /// <example>
    /// <code>
    /// var lifetime = ServiceLifetimeConstants.GetRecommendedLifetime(typeof(IMessageProducer));
    /// // Returns ServiceLifetime.Scoped
    /// </code>
    /// </example>
    public static ServiceLifetime GetRecommendedLifetime(Type serviceType)
    {
        ArgumentNullException.ThrowIfNull(serviceType);
        
        return RecommendedLifetimes.TryGetValue(serviceType, out var lifetime) 
            ? lifetime 
            : DefaultLifetime;
    }

    /// <summary>
    /// Gets the recommended service lifetime for a given service type.
    /// </summary>
    /// <typeparam name="T">The service type to get the recommendation for</typeparam>
    /// <returns>The recommended service lifetime</returns>
    /// <remarks>
    /// This is a generic version of GetRecommendedLifetime that provides compile-time type safety.
    /// </remarks>
    /// <example>
    /// <code>
    /// var lifetime = ServiceLifetimeConstants.GetRecommendedLifetime&lt;IMessageProducer&gt;();
    /// // Returns ServiceLifetime.Scoped
    /// </code>
    /// </example>
    public static ServiceLifetime GetRecommendedLifetime<T>()
    {
        return GetRecommendedLifetime(typeof(T));
    }

    /// <summary>
    /// Checks if a service type has a specific lifetime recommendation.
    /// </summary>
    /// <param name="serviceType">The service type to check</param>
    /// <returns>True if there's a specific recommendation, false otherwise</returns>
    public static bool HasRecommendation(Type serviceType)
    {
        ArgumentNullException.ThrowIfNull(serviceType);
        return RecommendedLifetimes.ContainsKey(serviceType);
    }

    /// <summary>
    /// Gets all service types that have specific lifetime recommendations.
    /// </summary>
    /// <returns>A collection of service types with recommendations</returns>
    public static IEnumerable<Type> GetRecommendedServiceTypes()
    {
        return RecommendedLifetimes.Keys;
    }

    /// <summary>
    /// Gets all service types that have a specific lifetime recommendation.
    /// </summary>
    /// <param name="lifetime">The lifetime to filter by</param>
    /// <returns>A collection of service types with the specified lifetime</returns>
    public static IEnumerable<Type> GetServiceTypesWithLifetime(ServiceLifetime lifetime)
    {
        return RecommendedLifetimes
            .Where(kvp => kvp.Value == lifetime)
            .Select(kvp => kvp.Key);
    }

    /// <summary>
    /// Validates that a service registration uses the recommended lifetime.
    /// </summary>
    /// <param name="serviceType">The service type being registered</param>
    /// <param name="actualLifetime">The actual lifetime being used</param>
    /// <returns>True if the lifetime matches the recommendation or no recommendation exists</returns>
    /// <remarks>
    /// This method can be used to validate service registrations during development
    /// to ensure optimal performance and resource utilization.
    /// </remarks>
    public static bool ValidateLifetime(Type serviceType, ServiceLifetime actualLifetime)
    {
        ArgumentNullException.ThrowIfNull(serviceType);
        
        if (!HasRecommendation(serviceType))
        {
            return true; // No recommendation, so any lifetime is acceptable
        }

        var recommendedLifetime = GetRecommendedLifetime(serviceType);
        return actualLifetime == recommendedLifetime;
    }

    /// <summary>
    /// Gets a description of why a particular lifetime is recommended for a service type.
    /// </summary>
    /// <param name="serviceType">The service type</param>
    /// <returns>A description of the lifetime recommendation</returns>
    public static string GetLifetimeReason(Type serviceType)
    {
        ArgumentNullException.ThrowIfNull(serviceType);
        
        if (!HasRecommendation(serviceType))
        {
            return $"No specific recommendation for {serviceType.Name}. Using default lifetime: {DefaultLifetime}";
        }

        var lifetime = GetRecommendedLifetime(serviceType);
        return lifetime switch
        {
            ServiceLifetime.Singleton => $"{serviceType.Name} is recommended as Singleton for optimal performance and resource sharing",
            ServiceLifetime.Scoped => $"{serviceType.Name} is recommended as Scoped for request-level isolation and resource management",
            ServiceLifetime.Transient => $"{serviceType.Name} is recommended as Transient for clean state and thread safety",
            _ => $"{serviceType.Name} has lifetime {lifetime}"
        };
    }
}