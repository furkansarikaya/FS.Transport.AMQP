using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using FS.Transport.AMQP.Core;
using FS.Transport.AMQP.Core.Exceptions;
using Microsoft.Extensions.Logging;

namespace FS.Transport.AMQP.Serialization;

/// <summary>
/// JSON message serializer using System.Text.Json with high performance and modern features
/// </summary>
public class JsonMessageSerializer : IMessageSerializer
{
    private readonly JsonSerializerOptions _options;
    private readonly ILogger<JsonMessageSerializer> _logger;
    
    /// <summary>
    /// Content type handled by this serializer
    /// </summary>
    public string ContentType => Constants.ContentTypes.Json;

    public JsonMessageSerializer(ILogger<JsonMessageSerializer> logger, JsonSerializerOptions? options = null)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _options = options ?? CreateDefaultOptions();
    }

    /// <summary>
    /// Creates default JSON serializer options optimized for messaging
    /// </summary>
    /// <returns>Configured JsonSerializerOptions</returns>
    public static JsonSerializerOptions CreateDefaultOptions()
    {
        return new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            WriteIndented = false, // Compact for message size
            DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
            PropertyNameCaseInsensitive = true,
            AllowTrailingCommas = true,
            ReadCommentHandling = JsonCommentHandling.Skip,
            NumberHandling = JsonNumberHandling.AllowReadingFromString,
            Converters =
            {
                new JsonStringEnumConverter(JsonNamingPolicy.CamelCase),
                new DateTimeOffsetConverter(),
                new TimeSpanConverter()
            }
        };
    }

    /// <summary>
    /// Serializes an object to JSON byte array
    /// </summary>
    /// <typeparam name="T">Object type</typeparam>
    /// <param name="obj">Object to serialize</param>
    /// <returns>UTF-8 encoded JSON byte array</returns>
    public byte[] Serialize<T>(T obj) where T : class
    {
        if (obj == null)
            throw new ArgumentNullException(nameof(obj));

        try
        {
            _logger.LogTrace("Serializing object of type {Type}", typeof(T).Name);
            return JsonSerializer.SerializeToUtf8Bytes(obj, _options);
        }
        catch (JsonException ex)
        {
            _logger.LogError(ex, "JSON serialization failed for type {Type}", typeof(T).Name);
            throw new SerializationException($"Failed to serialize object of type {typeof(T).Name}", ex, typeof(T));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error during serialization of type {Type}", typeof(T).Name);
            throw new SerializationException($"Unexpected error during serialization of type {typeof(T).Name}", ex, typeof(T));
        }
    }

    /// <summary>
    /// Serializes an object to JSON byte array asynchronously
    /// </summary>
    /// <typeparam name="T">Object type</typeparam>
    /// <param name="obj">Object to serialize</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>UTF-8 encoded JSON byte array</returns>
    public async Task<byte[]> SerializeAsync<T>(T obj, CancellationToken cancellationToken = default) where T : class
    {
        if (obj == null)
            throw new ArgumentNullException(nameof(obj));

        try
        {
            _logger.LogTrace("Async serializing object of type {Type}", typeof(T).Name);
            
            using var stream = new MemoryStream();
            await JsonSerializer.SerializeAsync(stream, obj, _options, cancellationToken);
            return stream.ToArray();
        }
        catch (JsonException ex)
        {
            _logger.LogError(ex, "Async JSON serialization failed for type {Type}", typeof(T).Name);
            throw new SerializationException($"Failed to serialize object of type {typeof(T).Name}", ex, typeof(T));
        }
        catch (OperationCanceledException)
        {
            _logger.LogDebug("Serialization cancelled for type {Type}", typeof(T).Name);
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error during async serialization of type {Type}", typeof(T).Name);
            throw new SerializationException($"Unexpected error during serialization of type {typeof(T).Name}", ex, typeof(T));
        }
    }

    /// <summary>
    /// Deserializes JSON byte array to specified type
    /// </summary>
    /// <typeparam name="T">Target type</typeparam>
    /// <param name="data">JSON byte array</param>
    /// <returns>Deserialized object</returns>
    public T Deserialize<T>(byte[] data) where T : class
    {
        if (data == null || data.Length == 0)
            throw new ArgumentException("Data cannot be null or empty", nameof(data));

        try
        {
            _logger.LogTrace("Deserializing {DataSize} bytes to type {Type}", data.Length, typeof(T).Name);
            
            var result = JsonSerializer.Deserialize<T>(data, _options);
            if (result == null)
            {
                throw new SerializationException($"Deserialization resulted in null for type {typeof(T).Name}", typeof(T));
            }
            
            return result;
        }
        catch (JsonException ex)
        {
            _logger.LogError(ex, "JSON deserialization failed for type {Type}", typeof(T).Name);
            throw new SerializationException($"Failed to deserialize to type {typeof(T).Name}", ex, typeof(T));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error during deserialization to type {Type}", typeof(T).Name);
            throw new SerializationException($"Unexpected error during deserialization to type {typeof(T).Name}", ex, typeof(T));
        }
    }

    /// <summary>
    /// Deserializes JSON byte array to specified type
    /// </summary>
    /// <param name="data">JSON byte array</param>
    /// <param name="type">Target type</param>
    /// <returns>Deserialized object</returns>
    public object Deserialize(byte[] data, Type type)
    {
        if (data == null || data.Length == 0)
            throw new ArgumentException("Data cannot be null or empty", nameof(data));
        if (type == null)
            throw new ArgumentNullException(nameof(type));

        try
        {
            _logger.LogTrace("Deserializing {DataSize} bytes to type {Type}", data.Length, type.Name);
            
            var result = JsonSerializer.Deserialize(data, type, _options);
            if (result == null && !type.IsValueType)
            {
                throw new SerializationException($"Deserialization resulted in null for type {type.Name}", type);
            }
            
            return result!;
        }
        catch (JsonException ex)
        {
            _logger.LogError(ex, "JSON deserialization failed for type {Type}", type.Name);
            throw new SerializationException($"Failed to deserialize to type {type.Name}", ex, type);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error during deserialization to type {Type}", type.Name);
            throw new SerializationException($"Unexpected error during deserialization to type {type.Name}", ex, type);
        }
    }

    /// <summary>
    /// Deserializes JSON byte array to specified type asynchronously
    /// </summary>
    /// <typeparam name="T">Target type</typeparam>
    /// <param name="data">JSON byte array</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Deserialized object</returns>
    public async Task<T> DeserializeAsync<T>(byte[] data, CancellationToken cancellationToken = default) where T : class
    {
        if (data == null || data.Length == 0)
            throw new ArgumentException("Data cannot be null or empty", nameof(data));

        try
        {
            _logger.LogTrace("Async deserializing {DataSize} bytes to type {Type}", data.Length, typeof(T).Name);
            
            using var stream = new MemoryStream(data);
            var result = await JsonSerializer.DeserializeAsync<T>(stream, _options, cancellationToken);
            
            if (result == null)
            {
                throw new SerializationException($"Async deserialization resulted in null for type {typeof(T).Name}", typeof(T));
            }
            
            return result;
        }
        catch (JsonException ex)
        {
            _logger.LogError(ex, "Async JSON deserialization failed for type {Type}", typeof(T).Name);
            throw new SerializationException($"Failed to deserialize to type {typeof(T).Name}", ex, typeof(T));
        }
        catch (OperationCanceledException)
        {
            _logger.LogDebug("Deserialization cancelled for type {Type}", typeof(T).Name);
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error during async deserialization to type {Type}", typeof(T).Name);
            throw new SerializationException($"Unexpected error during deserialization to type {typeof(T).Name}", ex, typeof(T));
        }
    }

    /// <summary>
    /// Deserializes JSON byte array to specified type asynchronously
    /// </summary>
    /// <param name="data">JSON byte array</param>
    /// <param name="type">Target type</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Deserialized object</returns>
    public async Task<object> DeserializeAsync(byte[] data, Type type, CancellationToken cancellationToken = default)
    {
        if (data == null || data.Length == 0)
            throw new ArgumentException("Data cannot be null or empty", nameof(data));
        if (type == null)
            throw new ArgumentNullException(nameof(type));

        try
        {
            _logger.LogTrace("Async deserializing {DataSize} bytes to type {Type}", data.Length, type.Name);
            
            using var stream = new MemoryStream(data);
            var result = await JsonSerializer.DeserializeAsync(stream, type, _options, cancellationToken);
            
            if (result == null && !type.IsValueType)
            {
                throw new SerializationException($"Async deserialization resulted in null for type {type.Name}", type);
            }
            
            return result!;
        }
        catch (JsonException ex)
        {
            _logger.LogError(ex, "Async JSON deserialization failed for type {Type}", type.Name);
            throw new SerializationException($"Failed to deserialize to type {type.Name}", ex, type);
        }
        catch (OperationCanceledException)
        {
            _logger.LogDebug("Deserialization cancelled for type {Type}", type.Name);
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error during async deserialization to type {Type}", type.Name);
            throw new SerializationException($"Unexpected error during deserialization to type {type.Name}", ex, type);
        }
    }

    /// <summary>
    /// Checks if this serializer can handle the specified content type
    /// </summary>
    /// <param name="contentType">Content type to check</param>
    /// <returns>True if can handle JSON content types</returns>
    public bool CanHandle(string contentType)
    {
        if (string.IsNullOrWhiteSpace(contentType))
            return false;

        return contentType.Equals(Constants.ContentTypes.Json, StringComparison.OrdinalIgnoreCase) ||
               contentType.StartsWith("application/json", StringComparison.OrdinalIgnoreCase) ||
               contentType.StartsWith("text/json", StringComparison.OrdinalIgnoreCase);
    }

    /// <summary>
    /// Gets estimated serialized size for an object
    /// </summary>
    /// <typeparam name="T">Object type</typeparam>
    /// <param name="obj">Object to estimate</param>
    /// <returns>Estimated size in bytes</returns>
    public int GetEstimatedSize<T>(T obj) where T : class
    {
        if (obj == null)
            return 0;

        try
        {
            // For performance, we do a quick estimation based on string length
            var jsonString = JsonSerializer.Serialize(obj, _options);
            return Encoding.UTF8.GetByteCount(jsonString);
        }
        catch
        {
            // Fallback estimation based on type
            return EstimateSizeByType(typeof(T));
        }
    }

    private static int EstimateSizeByType(Type type)
    {
        // Simple heuristic for size estimation
        if (type == typeof(string))
            return 100; // Average string size
        if (type.IsValueType)
            return 50; // Value types are generally smaller
        return type.IsClass ? 200 : // Reference types are generally larger
            100; // Default estimate
    }
}
