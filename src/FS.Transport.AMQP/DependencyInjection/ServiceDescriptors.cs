using Microsoft.Extensions.DependencyInjection;
using System.Reflection;

namespace FS.Transport.AMQP.DependencyInjection;

/// <summary>
/// Utility class for creating and managing service descriptors for RabbitMQ services
/// </summary>
/// <remarks>
/// This class provides utility methods for creating service descriptors with appropriate
/// lifetimes, dynamic service discovery, and bulk registration operations. It helps
/// ensure consistent service registration patterns across the RabbitMQ library.
/// </remarks>
public static class ServiceDescriptors
{
    #region Service Descriptor Creation

    /// <summary>
    /// Creates a service descriptor with the recommended lifetime
    /// </summary>
    /// <param name="serviceType">The service type</param>
    /// <param name="implementationType">The implementation type</param>
    /// <returns>A service descriptor with the recommended lifetime</returns>
    /// <exception cref="ArgumentNullException">Thrown when serviceType or implementationType is null</exception>
    /// <remarks>
    /// This method creates a service descriptor using the recommended lifetime for the
    /// service type based on RabbitMQ best practices and naming conventions.
    /// </remarks>
    /// <example>
    /// <code>
    /// var descriptor = ServiceDescriptors.CreateWithRecommendedLifetime(typeof(IQueueManager), typeof(QueueManager));
    /// services.Add(descriptor);
    /// </code>
    /// </example>
    public static ServiceDescriptor CreateWithRecommendedLifetime(Type serviceType, Type implementationType)
    {
        ArgumentNullException.ThrowIfNull(serviceType);
        ArgumentNullException.ThrowIfNull(implementationType);

        var lifetime = ServiceLifetimeConstants.GetRecommendedLifetime(serviceType);
        return new ServiceDescriptor(serviceType, implementationType, lifetime);
    }

    /// <summary>
    /// Creates a service descriptor with the recommended lifetime using a factory
    /// </summary>
    /// <param name="serviceType">The service type</param>
    /// <param name="factory">The factory function</param>
    /// <returns>A service descriptor with the recommended lifetime</returns>
    /// <exception cref="ArgumentNullException">Thrown when serviceType or factory is null</exception>
    /// <remarks>
    /// This method creates a service descriptor using a factory function and the
    /// recommended lifetime for the service type.
    /// </remarks>
    /// <example>
    /// <code>
    /// var descriptor = ServiceDescriptors.CreateWithRecommendedLifetime(typeof(IQueueManager), 
    ///     provider => new QueueManager(provider.GetRequiredService&lt;IConnectionManager&gt;()));
    /// services.Add(descriptor);
    /// </code>
    /// </example>
    public static ServiceDescriptor CreateWithRecommendedLifetime(Type serviceType, Func<IServiceProvider, object> factory)
    {
        ArgumentNullException.ThrowIfNull(serviceType);
        ArgumentNullException.ThrowIfNull(factory);

        var lifetime = ServiceLifetimeConstants.GetRecommendedLifetime(serviceType);
        return new ServiceDescriptor(serviceType, factory, lifetime);
    }

    /// <summary>
    /// Creates a service descriptor with the recommended lifetime using a generic factory
    /// </summary>
    /// <typeparam name="TService">The service type</typeparam>
    /// <typeparam name="TImplementation">The implementation type</typeparam>
    /// <returns>A service descriptor with the recommended lifetime</returns>
    /// <remarks>
    /// This method creates a service descriptor using generic types and the
    /// recommended lifetime for the service type.
    /// </remarks>
    /// <example>
    /// <code>
    /// var descriptor = ServiceDescriptors.CreateWithRecommendedLifetime&lt;IQueueManager, QueueManager&gt;();
    /// services.Add(descriptor);
    /// </code>
    /// </example>
    public static ServiceDescriptor CreateWithRecommendedLifetime<TService, TImplementation>()
        where TService : class
        where TImplementation : class, TService
    {
        return CreateWithRecommendedLifetime(typeof(TService), typeof(TImplementation));
    }

    /// <summary>
    /// Creates a service descriptor with the recommended lifetime using a generic factory
    /// </summary>
    /// <typeparam name="TService">The service type</typeparam>
    /// <param name="factory">The factory function</param>
    /// <returns>A service descriptor with the recommended lifetime</returns>
    /// <exception cref="ArgumentNullException">Thrown when factory is null</exception>
    /// <remarks>
    /// This method creates a service descriptor using a generic factory function and the
    /// recommended lifetime for the service type.
    /// </remarks>
    /// <example>
    /// <code>
    /// var descriptor = ServiceDescriptors.CreateWithRecommendedLifetime&lt;IQueueManager&gt;(
    ///     provider => new QueueManager(provider.GetRequiredService&lt;IConnectionManager&gt;()));
    /// services.Add(descriptor);
    /// </code>
    /// </example>
    public static ServiceDescriptor CreateWithRecommendedLifetime<TService>(Func<IServiceProvider, TService> factory)
        where TService : class
    {
        ArgumentNullException.ThrowIfNull(factory);

        var lifetime = ServiceLifetimeConstants.GetRecommendedLifetime(typeof(TService));
        return new ServiceDescriptor(typeof(TService), factory, lifetime);
    }

    /// <summary>
    /// Creates multiple service descriptors with recommended lifetimes
    /// </summary>
    /// <param name="serviceTypePairs">Pairs of service type and implementation type</param>
    /// <returns>A collection of service descriptors with recommended lifetimes</returns>
    /// <exception cref="ArgumentNullException">Thrown when serviceTypePairs is null</exception>
    /// <remarks>
    /// This method creates multiple service descriptors at once, using the recommended
    /// lifetime for each service type.
    /// </remarks>
    /// <example>
    /// <code>
    /// var descriptors = ServiceDescriptors.CreateMultipleWithRecommendedLifetimes(new[]
    /// {
    ///     (typeof(IQueueManager), typeof(QueueManager)),
    ///     (typeof(IExchangeManager), typeof(ExchangeManager))
    /// });
    /// services.Add(descriptors);
    /// </code>
    /// </example>
    public static IEnumerable<ServiceDescriptor> CreateMultipleWithRecommendedLifetimes(
        IEnumerable<(Type ServiceType, Type ImplementationType)> serviceTypePairs)
    {
        ArgumentNullException.ThrowIfNull(serviceTypePairs);

        return serviceTypePairs.Select(pair => CreateWithRecommendedLifetime(pair.ServiceType, pair.ImplementationType));
    }

    #endregion

    #region Service Discovery

    /// <summary>
    /// Discovers services in the specified assemblies that match the given criteria
    /// </summary>
    /// <param name="assemblies">Assemblies to search</param>
    /// <param name="serviceFilter">Filter for service types</param>
    /// <param name="implementationFilter">Filter for implementation types</param>
    /// <returns>A collection of discovered service descriptors</returns>
    /// <exception cref="ArgumentNullException">Thrown when assemblies is null</exception>
    /// <remarks>
    /// This method discovers services in assemblies using the provided filters and
    /// creates service descriptors with recommended lifetimes.
    /// </remarks>
    /// <example>
    /// <code>
    /// var descriptors = ServiceDescriptors.DiscoverServices(
    ///     new[] { typeof(Program).Assembly },
    ///     serviceType => serviceType.IsInterface &amp;&amp; serviceType.Name.StartsWith("I"),
    ///     implType => implType.IsClass &amp;&amp; !implType.IsAbstract);
    /// </code>
    /// </example>
    public static IEnumerable<ServiceDescriptor> DiscoverServices(
        IEnumerable<Assembly> assemblies,
        Func<Type, bool> serviceFilter,
        Func<Type, bool> implementationFilter)
    {
        ArgumentNullException.ThrowIfNull(assemblies);
        ArgumentNullException.ThrowIfNull(serviceFilter);
        ArgumentNullException.ThrowIfNull(implementationFilter);

        var descriptors = new List<ServiceDescriptor>();

        foreach (var assembly in assemblies)
        {
            var types = assembly.GetTypes();
            var serviceTypes = types.Where(serviceFilter).ToList();
            var implementationTypes = types.Where(implementationFilter).ToList();

            foreach (var serviceType in serviceTypes)
            {
                var implementationType = implementationTypes.FirstOrDefault(impl => 
                    serviceType.IsAssignableFrom(impl) && serviceType != impl);

                if (implementationType != null)
                {
                    descriptors.Add(CreateWithRecommendedLifetime(serviceType, implementationType));
                }
            }
        }

        return descriptors;
    }

    /// <summary>
    /// Discovers RabbitMQ services in the specified assemblies
    /// </summary>
    /// <param name="assemblies">Assemblies to search</param>
    /// <returns>A collection of discovered RabbitMQ service descriptors</returns>
    /// <exception cref="ArgumentNullException">Thrown when assemblies is null</exception>
    /// <remarks>
    /// This method discovers RabbitMQ services using standard naming conventions and
    /// interface patterns. It looks for interfaces starting with 'I' and their
    /// corresponding implementations.
    /// </remarks>
    /// <example>
    /// <code>
    /// var descriptors = ServiceDescriptors.DiscoverRabbitMQServices(new[] { typeof(Program).Assembly });
    /// foreach (var descriptor in descriptors)
    /// {
    ///     services.Add(descriptor);
    /// }
    /// </code>
    /// </example>
    public static IEnumerable<ServiceDescriptor> DiscoverRabbitMQServices(IEnumerable<Assembly> assemblies)
    {
        ArgumentNullException.ThrowIfNull(assemblies);

        return DiscoverServices(
            assemblies,
            serviceType => serviceType.IsInterface && 
                          serviceType.Name.StartsWith("I") && 
                          serviceType.Namespace?.StartsWith("FS.Transport.AMQP") == true,
            implementationType => implementationType.IsClass && 
                                 !implementationType.IsAbstract && 
                                 implementationType.Namespace?.StartsWith("FS.Transport.AMQP") == true);
    }

    /// <summary>
    /// Discovers event handlers in the specified assemblies
    /// </summary>
    /// <param name="assemblies">Assemblies to search</param>
    /// <returns>A collection of discovered event handler service descriptors</returns>
    /// <exception cref="ArgumentNullException">Thrown when assemblies is null</exception>
    /// <remarks>
    /// This method discovers event handlers that implement IEventHandler or IAsyncEventHandler
    /// interfaces and creates service descriptors with scoped lifetime.
    /// </remarks>
    /// <example>
    /// <code>
    /// var descriptors = ServiceDescriptors.DiscoverEventHandlers(new[] { typeof(Program).Assembly });
    /// foreach (var descriptor in descriptors)
    /// {
    ///     services.Add(descriptor);
    /// }
    /// </code>
    /// </example>
    public static IEnumerable<ServiceDescriptor> DiscoverEventHandlers(IEnumerable<Assembly> assemblies)
    {
        ArgumentNullException.ThrowIfNull(assemblies);

        var descriptors = new List<ServiceDescriptor>();

        foreach (var assembly in assemblies)
        {
            var handlerTypes = assembly.GetTypes()
                .Where(t => t.IsClass && !t.IsAbstract)
                .Where(t => t.Name.EndsWith("Handler") || t.Name.EndsWith("EventHandler"))
                .ToList();

            foreach (var handlerType in handlerTypes)
            {
                var interfaces = handlerType.GetInterfaces()
                    .Where(i => i.IsGenericType && (
                        i.GetGenericTypeDefinition().Name.Contains("EventHandler") ||
                        i.GetGenericTypeDefinition().Name.Contains("Handler")))
                    .ToList();

                foreach (var @interface in interfaces)
                {
                    descriptors.Add(new ServiceDescriptor(@interface, handlerType, ServiceLifetime.Scoped));
                }
            }
        }

        return descriptors;
    }

    /// <summary>
    /// Discovers CQRS handlers in the specified assemblies
    /// </summary>
    /// <param name="assemblies">Assemblies to search</param>
    /// <returns>A collection of discovered CQRS handler service descriptors</returns>
    /// <exception cref="ArgumentNullException">Thrown when assemblies is null</exception>
    /// <remarks>
    /// This method discovers CQRS command and query handlers that implement IRequestHandler
    /// interface and creates service descriptors with scoped lifetime.
    /// </remarks>
    /// <example>
    /// <code>
    /// var descriptors = ServiceDescriptors.DiscoverCQRSHandlers(new[] { typeof(Program).Assembly });
    /// foreach (var descriptor in descriptors)
    /// {
    ///     services.Add(descriptor);
    /// }
    /// </code>
    /// </example>
    public static IEnumerable<ServiceDescriptor> DiscoverCQRSHandlers(IEnumerable<Assembly> assemblies)
    {
        ArgumentNullException.ThrowIfNull(assemblies);

        var descriptors = new List<ServiceDescriptor>();

        foreach (var assembly in assemblies)
        {
            var handlerTypes = assembly.GetTypes()
                .Where(t => t.IsClass && !t.IsAbstract)
                .Where(t => t.Name.EndsWith("Handler") || t.Name.EndsWith("CommandHandler") || t.Name.EndsWith("QueryHandler"))
                .ToList();

            foreach (var handlerType in handlerTypes)
            {
                var interfaces = handlerType.GetInterfaces()
                    .Where(i => i.IsGenericType && i.GetGenericTypeDefinition().Name.Contains("RequestHandler"))
                    .ToList();

                foreach (var @interface in interfaces)
                {
                    descriptors.Add(new ServiceDescriptor(@interface, handlerType, ServiceLifetime.Scoped));
                }
            }
        }

        return descriptors;
    }

    #endregion

    #region Service Descriptor Validation

    /// <summary>
    /// Validates that services use recommended lifetimes
    /// </summary>
    /// <param name="descriptors">Service descriptors to validate</param>
    /// <returns>A list of validation warnings</returns>
    /// <exception cref="ArgumentNullException">Thrown when descriptors is null</exception>
    /// <remarks>
    /// This method checks each service descriptor against the recommended lifetimes
    /// defined in ServiceLifetimeConstants and returns warnings for any mismatches.
    /// </remarks>
    /// <example>
    /// <code>
    /// var warnings = ServiceDescriptors.ValidateLifetimes(services);
    /// if (warnings.Any())
    /// {
    ///     foreach (var warning in warnings)
    ///     {
    ///         Console.WriteLine($"Warning: {warning}");
    ///     }
    /// }
    /// </code>
    /// </example>
    public static IList<string> ValidateLifetimes(IEnumerable<ServiceDescriptor> descriptors)
    {
        ArgumentNullException.ThrowIfNull(descriptors);

        var warnings = new List<string>();
        
        foreach (var descriptor in descriptors)
        {
            if (!ServiceLifetimeConstants.ValidateLifetime(descriptor.ServiceType, descriptor.Lifetime))
            {
                var recommendedLifetime = ServiceLifetimeConstants.GetRecommendedLifetime(descriptor.ServiceType);
                warnings.Add($"Service {descriptor.ServiceType.Name} is registered with {descriptor.Lifetime} lifetime, but {recommendedLifetime} is recommended");
            }
        }

        return warnings;
    }

    /// <summary>
    /// Validates circular dependencies in service descriptors
    /// </summary>
    /// <param name="descriptors">Service descriptors to validate</param>
    /// <returns>A list of validation warnings</returns>
    /// <exception cref="ArgumentNullException">Thrown when descriptors is null</exception>
    /// <remarks>
    /// This method analyzes service descriptors for potential circular dependencies
    /// and returns warnings for any that are found.
    /// </remarks>
    /// <example>
    /// <code>
    /// var warnings = ServiceDescriptors.ValidateCircularDependencies(services);
    /// if (warnings.Any())
    /// {
    ///     foreach (var warning in warnings)
    ///     {
    ///         Console.WriteLine($"Warning: {warning}");
    ///     }
    /// }
    /// </code>
    /// </example>
    public static IList<string> ValidateCircularDependencies(IEnumerable<ServiceDescriptor> descriptors)
    {
        ArgumentNullException.ThrowIfNull(descriptors);

        var warnings = new List<string>();
        var serviceTypes = descriptors.Select(d => d.ServiceType).ToHashSet();

        foreach (var descriptor in descriptors)
        {
            if (descriptor.ImplementationType != null)
            {
                var dependencies = GetDependencies(descriptor.ImplementationType);
                var circularDeps = dependencies.Where(dep => serviceTypes.Contains(dep.ParameterType)).ToList();

                if (circularDeps.Any())
                {
                    var depNames = string.Join(", ", circularDeps.Select(d => d.ParameterType.Name));
                    warnings.Add($"Service {descriptor.ServiceType.Name} may have circular dependencies: {depNames}");
                }
            }
        }

        return warnings;
    }

    /// <summary>
    /// Gets performance statistics for service descriptors
    /// </summary>
    /// <param name="descriptors">Service descriptors to analyze</param>
    /// <returns>Performance statistics</returns>
    /// <exception cref="ArgumentNullException">Thrown when descriptors is null</exception>
    /// <remarks>
    /// This method analyzes service descriptors and returns statistics about their
    /// performance characteristics including lifetime distribution and dependency complexity.
    /// </remarks>
    /// <example>
    /// <code>
    /// var stats = ServiceDescriptors.GetPerformanceStatistics(services);
    /// Console.WriteLine($"Singleton services: {stats.SingletonCount}");
    /// Console.WriteLine($"Scoped services: {stats.ScopedCount}");
    /// Console.WriteLine($"Transient services: {stats.TransientCount}");
    /// </code>
    /// </example>
    public static ServicePerformanceStatistics GetPerformanceStatistics(IEnumerable<ServiceDescriptor> descriptors)
    {
        ArgumentNullException.ThrowIfNull(descriptors);

        var descriptorList = descriptors.ToList();
        var singletonCount = descriptorList.Count(d => d.Lifetime == ServiceLifetime.Singleton);
        var scopedCount = descriptorList.Count(d => d.Lifetime == ServiceLifetime.Scoped);
        var transientCount = descriptorList.Count(d => d.Lifetime == ServiceLifetime.Transient);

        var avgDependencyCount = descriptorList
            .Where(d => d.ImplementationType != null)
            .Select(d => GetDependencies(d.ImplementationType!).Count)
            .DefaultIfEmpty(0)
            .Average();

        return new ServicePerformanceStatistics
        {
            TotalServices = descriptorList.Count,
            SingletonCount = singletonCount,
            ScopedCount = scopedCount,
            TransientCount = transientCount,
            SingletonPercentage = (double)singletonCount / descriptorList.Count * 100,
            ScopedPercentage = (double)scopedCount / descriptorList.Count * 100,
            TransientPercentage = (double)transientCount / descriptorList.Count * 100,
            AverageDependencyCount = avgDependencyCount,
            SingletonToScopedRatio = scopedCount > 0 ? (double)singletonCount / scopedCount : 0
        };
    }

    #endregion

    #region Bulk Operations

    /// <summary>
    /// Adds multiple service descriptors to the service collection
    /// </summary>
    /// <param name="services">The service collection</param>
    /// <param name="descriptors">Service descriptors to add</param>
    /// <returns>The service collection for fluent configuration</returns>
    /// <exception cref="ArgumentNullException">Thrown when services or descriptors is null</exception>
    /// <remarks>
    /// This method adds multiple service descriptors to the service collection in a single operation.
    /// It's more efficient than adding descriptors one by one.
    /// </remarks>
    /// <example>
    /// <code>
    /// var descriptors = ServiceDescriptors.DiscoverRabbitMQServices(new[] { typeof(Program).Assembly });
    /// services.AddRange(descriptors);
    /// </code>
    /// </example>
    public static IServiceCollection AddRange(this IServiceCollection services, IEnumerable<ServiceDescriptor> descriptors)
    {
        ArgumentNullException.ThrowIfNull(services);
        ArgumentNullException.ThrowIfNull(descriptors);

        foreach (var descriptor in descriptors)
        {
            services.Add(descriptor);
        }

        return services;
    }

    /// <summary>
    /// Adds multiple service descriptors to the service collection, skipping duplicates
    /// </summary>
    /// <param name="services">The service collection</param>
    /// <param name="descriptors">Service descriptors to add</param>
    /// <returns>The service collection for fluent configuration</returns>
    /// <exception cref="ArgumentNullException">Thrown when services or descriptors is null</exception>
    /// <remarks>
    /// This method adds multiple service descriptors to the service collection, but skips
    /// any descriptors for service types that are already registered.
    /// </remarks>
    /// <example>
    /// <code>
    /// var descriptors = ServiceDescriptors.DiscoverRabbitMQServices(new[] { typeof(Program).Assembly });
    /// services.TryAddRange(descriptors);
    /// </code>
    /// </example>
    public static IServiceCollection TryAddRange(this IServiceCollection services, IEnumerable<ServiceDescriptor> descriptors)
    {
        ArgumentNullException.ThrowIfNull(services);
        ArgumentNullException.ThrowIfNull(descriptors);

        var existingServiceTypes = services.Select(s => s.ServiceType).ToHashSet();

        foreach (var descriptor in descriptors)
        {
            if (!existingServiceTypes.Contains(descriptor.ServiceType))
            {
                services.Add(descriptor);
                existingServiceTypes.Add(descriptor.ServiceType);
            }
        }

        return services;
    }

    #endregion

    #region Private Helper Methods

    /// <summary>
    /// Gets the dependencies for a given type
    /// </summary>
    /// <param name="type">The type to analyze</param>
    /// <returns>A list of constructor dependencies</returns>
    private static IList<ParameterInfo> GetDependencies(Type type)
    {
        var constructors = type.GetConstructors(BindingFlags.Public | BindingFlags.Instance);
        var primaryConstructor = constructors.OrderByDescending(c => c.GetParameters().Length).FirstOrDefault();
        
        return primaryConstructor?.GetParameters().ToList() ?? new List<ParameterInfo>();
    }

    #endregion
}

/// <summary>
/// Performance statistics for service descriptors
/// </summary>
/// <remarks>
/// This class contains performance-related statistics for a collection of service descriptors,
/// including lifetime distribution and dependency complexity metrics.
/// </remarks>
public class ServicePerformanceStatistics
{
    /// <summary>
    /// Total number of services
    /// </summary>
    public int TotalServices { get; set; }

    /// <summary>
    /// Number of singleton services
    /// </summary>
    public int SingletonCount { get; set; }

    /// <summary>
    /// Number of scoped services
    /// </summary>
    public int ScopedCount { get; set; }

    /// <summary>
    /// Number of transient services
    /// </summary>
    public int TransientCount { get; set; }

    /// <summary>
    /// Percentage of singleton services
    /// </summary>
    public double SingletonPercentage { get; set; }

    /// <summary>
    /// Percentage of scoped services
    /// </summary>
    public double ScopedPercentage { get; set; }

    /// <summary>
    /// Percentage of transient services
    /// </summary>
    public double TransientPercentage { get; set; }

    /// <summary>
    /// Average number of dependencies per service
    /// </summary>
    public double AverageDependencyCount { get; set; }

    /// <summary>
    /// Ratio of singleton to scoped services
    /// </summary>
    public double SingletonToScopedRatio { get; set; }

    /// <summary>
    /// Whether the service registration is considered healthy
    /// </summary>
    public bool IsHealthy => 
        SingletonPercentage >= 20 && 
        SingletonPercentage <= 60 &&
        TransientPercentage <= 10 &&
        AverageDependencyCount <= 5;

    /// <summary>
    /// Performance recommendations based on the statistics
    /// </summary>
    public IList<string> GetRecommendations()
    {
        var recommendations = new List<string>();

        if (SingletonPercentage < 20)
        {
            recommendations.Add("Consider registering more services as singletons for better performance");
        }

        if (SingletonPercentage > 60)
        {
            recommendations.Add("High number of singleton services may indicate excessive global state");
        }

        if (TransientPercentage > 10)
        {
            recommendations.Add("High number of transient services may impact performance due to excessive object creation");
        }

        if (AverageDependencyCount > 5)
        {
            recommendations.Add("High average dependency count may indicate complex service graphs and potential circular dependencies");
        }

        if (SingletonToScopedRatio < 0.1)
        {
            recommendations.Add("Very low singleton to scoped ratio may indicate missing performance optimization opportunities");
        }

        if (SingletonToScopedRatio > 2.0)
        {
            recommendations.Add("High singleton to scoped ratio may indicate excessive singleton usage");
        }

        return recommendations;
    }
}