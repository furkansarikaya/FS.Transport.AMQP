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
        if (serviceType is not { IsClass: true, IsAbstract: false }) return null;
        try
        {
            return Activator.CreateInstance(serviceType);
        }
        catch
        {
            return null;
        }
    }
} 