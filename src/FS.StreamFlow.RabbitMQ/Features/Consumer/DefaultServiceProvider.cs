using System;

namespace FS.StreamFlow.RabbitMQ.Features.Consumer;

/// <summary>
/// Default service provider implementation for fallback scenarios
/// </summary>
public class DefaultServiceProvider : IServiceProvider
{
    /// <summary>
    /// Gets the service object of the specified type
    /// </summary>
    /// <param name="serviceType">Type of service to get</param>
    /// <returns>Service instance or null if not found</returns>
    public object? GetService(Type serviceType)
    {
        // For basic types, try to create instances
        if (serviceType.IsClass && !serviceType.IsAbstract)
        {
            try
            {
                return Activator.CreateInstance(serviceType);
            }
            catch
            {
                return null;
            }
        }
        
        return null;
    }
} 