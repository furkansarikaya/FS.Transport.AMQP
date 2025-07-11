using System.Text.Json;
using FS.RabbitMQ.Core;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Logging;

namespace FS.RabbitMQ.Serialization;

/// <summary>
/// Extension methods for configuring message serialization
/// </summary>
public static class SerializationExtensions
{
    /// <summary>
    /// Adds default JSON message serializer to the service collection
    /// </summary>
    /// <param name="services">Service collection</param>
    /// <param name="configureOptions">Optional JSON options configuration</param>
    /// <returns>Service collection for fluent configuration</returns>
    public static IServiceCollection AddJsonMessageSerializer(this IServiceCollection services, 
        Action<JsonSerializerOptions>? configureOptions = null)
    {
        var options = JsonMessageSerializer.CreateDefaultOptions();
        configureOptions?.Invoke(options);
        
        services.TryAddSingleton(options);
        services.TryAddSingleton<IMessageSerializer, JsonMessageSerializer>();
        
        return services;
    }

    /// <summary>
    /// Adds binary message serializer to the service collection
    /// </summary>
    /// <param name="services">Service collection</param>
    /// <param name="useCompression">Whether to enable compression</param>
    /// <returns>Service collection for fluent configuration</returns>
    public static IServiceCollection AddBinaryMessageSerializer(this IServiceCollection services, 
        bool useCompression = false)
    {
        services.TryAddSingleton<IMessageSerializer>(provider =>
        {
            var logger = provider.GetRequiredService<Microsoft.Extensions.Logging.ILogger<BinaryMessageSerializer>>();
            var loggerFactory = provider.GetRequiredService<ILoggerFactory>();
            return new BinaryMessageSerializer(logger, loggerFactory, useCompression);
        });
        
        return services;
    }

    /// <summary>
    /// Adds multiple message serializers with a factory to resolve the appropriate one
    /// </summary>
    /// <param name="services">Service collection</param>
    /// <returns>Service collection for fluent configuration</returns>
    public static IServiceCollection AddMessageSerializers(this IServiceCollection services)
    {
        // Register individual serializers
        services.AddJsonMessageSerializer();
        services.AddBinaryMessageSerializer();
        
        // Register factory
        services.TryAddSingleton<IMessageSerializerFactory, MessageSerializerFactory>();
        
        return services;
    }

    /// <summary>
    /// Determines the appropriate serializer based on content type
    /// </summary>
    /// <param name="serializers">Available serializers</param>
    /// <param name="contentType">Content type</param>
    /// <returns>Matching serializer or null if none found</returns>
    public static IMessageSerializer? GetSerializer(this IEnumerable<IMessageSerializer> serializers, string contentType)
    {
        return serializers.FirstOrDefault(s => s.CanHandle(contentType));
    }

    /// <summary>
    /// Gets the default serializer (JSON) from the collection
    /// </summary>
    /// <param name="serializers">Available serializers</param>
    /// <returns>Default serializer</returns>
    public static IMessageSerializer GetDefaultSerializer(this IEnumerable<IMessageSerializer> serializers)
    {
        return serializers.GetSerializer(Constants.ContentTypes.Json) 
               ?? serializers.First();
    }

    /// <summary>
    /// Serializes an object with automatic serializer selection
    /// </summary>
    /// <typeparam name="T">Object type</typeparam>
    /// <param name="serializers">Available serializers</param>
    /// <param name="obj">Object to serialize</param>
    /// <param name="contentType">Preferred content type</param>
    /// <returns>Serialization result with content type and data</returns>
    public static SerializationResult SerializeWithContentType<T>(this IEnumerable<IMessageSerializer> serializers, 
        T obj, 
        string? contentType = null) where T : class
    {
        contentType ??= Constants.ContentTypes.Json;
        
        var serializer = serializers.GetSerializer(contentType) ?? serializers.GetDefaultSerializer();
        var data = serializer.Serialize(obj);
        
        return new SerializationResult(data, serializer.ContentType);
    }

    /// <summary>
    /// Deserializes data with automatic serializer selection
    /// </summary>
    /// <typeparam name="T">Target type</typeparam>
    /// <param name="serializers">Available serializers</param>
    /// <param name="data">Serialized data</param>
    /// <param name="contentType">Content type</param>
    /// <returns>Deserialized object</returns>
    public static T DeserializeWithContentType<T>(this IEnumerable<IMessageSerializer> serializers, 
        byte[] data, 
        string contentType) where T : class
    {
        var serializer = serializers.GetSerializer(contentType) ?? serializers.GetDefaultSerializer();
        return serializer.Deserialize<T>(data);
    }
}