using FS.RabbitMQ.Core;
using FS.RabbitMQ.Core.Exceptions;
using Microsoft.Extensions.Logging;

namespace FS.RabbitMQ.Serialization;

/// <summary>
/// Binary message serializer with compression support for high-performance scenarios
/// </summary>
public class BinaryMessageSerializer : IMessageSerializer
{
    private readonly ILogger<BinaryMessageSerializer> _logger;
    private readonly bool _useCompression;
    private readonly JsonMessageSerializer _fallbackSerializer;
    
    /// <summary>
    /// Content type handled by this serializer
    /// </summary>
    public string ContentType => Constants.ContentTypes.Binary;

    public BinaryMessageSerializer(ILogger<BinaryMessageSerializer> logger, ILoggerFactory loggerFactory, bool useCompression = false)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _useCompression = useCompression;
        _fallbackSerializer = new JsonMessageSerializer(
            loggerFactory.CreateLogger<JsonMessageSerializer>()
        );
    }

    /// <summary>
    /// Serializes an object to binary byte array
    /// </summary>
    /// <typeparam name="T">Object type</typeparam>
    /// <param name="obj">Object to serialize</param>
    /// <returns>Binary byte array</returns>
    public byte[] Serialize<T>(T obj) where T : class
    {
        ArgumentNullException.ThrowIfNull(obj);

        try
        {
            _logger.LogTrace("Binary serializing object of type {Type}", typeof(T).Name);
            
            byte[] data;
            
            // Use JSON as the underlying format for better compatibility and security
            data = _fallbackSerializer.Serialize(obj);

            if (!_useCompression) return data;
            data = CompressData(data);
            _logger.LogTrace("Compressed data from {OriginalSize} to {CompressedSize} bytes", 
                data.Length, data.Length);

            return data;
        }
        catch (Exception ex) when (!(ex is SerializationException))
        {
            _logger.LogError(ex, "Binary serialization failed for type {Type}", typeof(T).Name);
            throw new SerializationException($"Failed to serialize object of type {typeof(T).Name}", ex, typeof(T));
        }
    }

    /// <summary>
    /// Serializes an object to binary byte array asynchronously
    /// </summary>
    /// <typeparam name="T">Object type</typeparam>
    /// <param name="obj">Object to serialize</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Binary byte array</returns>
    public async Task<byte[]> SerializeAsync<T>(T obj, CancellationToken cancellationToken = default) where T : class
    {
        ArgumentNullException.ThrowIfNull(obj);

        try
        {
            _logger.LogTrace("Async binary serializing object of type {Type}", typeof(T).Name);
            
            var data = await _fallbackSerializer.SerializeAsync(obj, cancellationToken);

            if (!_useCompression) return data;
            data = await CompressDataAsync(data, cancellationToken);
            _logger.LogTrace("Async compressed data from original to {CompressedSize} bytes", data.Length);

            return data;
        }
        catch (Exception ex) when (!(ex is SerializationException) && !(ex is OperationCanceledException))
        {
            _logger.LogError(ex, "Async binary serialization failed for type {Type}", typeof(T).Name);
            throw new SerializationException($"Failed to serialize object of type {typeof(T).Name}", ex, typeof(T));
        }
    }

    /// <summary>
    /// Deserializes binary byte array to specified type
    /// </summary>
    /// <typeparam name="T">Target type</typeparam>
    /// <param name="data">Binary byte array</param>
    /// <returns>Deserialized object</returns>
    public T Deserialize<T>(byte[] data) where T : class
    {
        if (data == null || data.Length == 0)
            throw new ArgumentException("Data cannot be null or empty", nameof(data));

        try
        {
            _logger.LogTrace("Binary deserializing {DataSize} bytes to type {Type}", data.Length, typeof(T).Name);
            
            var processedData = data;

            if (!_useCompression) return _fallbackSerializer.Deserialize<T>(processedData);
            processedData = DecompressData(data);
            _logger.LogTrace("Decompressed data from {CompressedSize} to {OriginalSize} bytes", 
                data.Length, processedData.Length);

            return _fallbackSerializer.Deserialize<T>(processedData);
        }
        catch (Exception ex) when (!(ex is SerializationException))
        {
            _logger.LogError(ex, "Binary deserialization failed for type {Type}", typeof(T).Name);
            throw new SerializationException($"Failed to deserialize to type {typeof(T).Name}", ex, typeof(T));
        }
    }

    /// <summary>
    /// Deserializes binary byte array to specified type
    /// </summary>
    /// <param name="data">Binary byte array</param>
    /// <param name="type">Target type</param>
    /// <returns>Deserialized object</returns>
    public object Deserialize(byte[] data, Type type)
    {
        if (data == null || data.Length == 0)
            throw new ArgumentException("Data cannot be null or empty", nameof(data));
        ArgumentNullException.ThrowIfNull(type);

        try
        {
            _logger.LogTrace("Binary deserializing {DataSize} bytes to type {Type}", data.Length, type.Name);
            
            var processedData = data;

            if (!_useCompression) return _fallbackSerializer.Deserialize(processedData, type);
            processedData = DecompressData(data);
            _logger.LogTrace("Decompressed data from {CompressedSize} to {OriginalSize} bytes", 
                data.Length, processedData.Length);

            return _fallbackSerializer.Deserialize(processedData, type);
        }
        catch (Exception ex) when (!(ex is SerializationException))
        {
            _logger.LogError(ex, "Binary deserialization failed for type {Type}", type.Name);
            throw new SerializationException($"Failed to deserialize to type {type.Name}", ex, type);
        }
    }

    /// <summary>
    /// Deserializes binary byte array to specified type asynchronously
    /// </summary>
    /// <typeparam name="T">Target type</typeparam>
    /// <param name="data">Binary byte array</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Deserialized object</returns>
    public async Task<T> DeserializeAsync<T>(byte[] data, CancellationToken cancellationToken = default) where T : class
    {
        if (data == null || data.Length == 0)
            throw new ArgumentException("Data cannot be null or empty", nameof(data));

        try
        {
            _logger.LogTrace("Async binary deserializing {DataSize} bytes to type {Type}", data.Length, typeof(T).Name);
            
            var processedData = data;

            if (!_useCompression) return await _fallbackSerializer.DeserializeAsync<T>(processedData, cancellationToken);
            processedData = await DecompressDataAsync(data, cancellationToken);
            _logger.LogTrace("Async decompressed data from {CompressedSize} to {OriginalSize} bytes", 
                data.Length, processedData.Length);

            return await _fallbackSerializer.DeserializeAsync<T>(processedData, cancellationToken);
        }
        catch (Exception ex) when (!(ex is SerializationException) && !(ex is OperationCanceledException))
        {
            _logger.LogError(ex, "Async binary deserialization failed for type {Type}", typeof(T).Name);
            throw new SerializationException($"Failed to deserialize to type {typeof(T).Name}", ex, typeof(T));
        }
    }

    /// <summary>
    /// Deserializes binary byte array to specified type asynchronously
    /// </summary>
    /// <param name="data">Binary byte array</param>
    /// <param name="type">Target type</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Deserialized object</returns>
    public async Task<object> DeserializeAsync(byte[] data, Type type, CancellationToken cancellationToken = default)
    {
        if (data == null || data.Length == 0)
            throw new ArgumentException("Data cannot be null or empty", nameof(data));
        ArgumentNullException.ThrowIfNull(type);

        try
        {
            _logger.LogTrace("Async binary deserializing {DataSize} bytes to type {Type}", data.Length, type.Name);
            
            var processedData = data;

            if (!_useCompression) return await _fallbackSerializer.DeserializeAsync(processedData, type, cancellationToken);
            processedData = await DecompressDataAsync(data, cancellationToken);
            _logger.LogTrace("Async decompressed data from {CompressedSize} to {OriginalSize} bytes", 
                data.Length, processedData.Length);

            return await _fallbackSerializer.DeserializeAsync(processedData, type, cancellationToken);
        }
        catch (Exception ex) when (ex is not SerializationException && ex is not OperationCanceledException)
        {
            _logger.LogError(ex, "Async binary deserialization failed for type {Type}", type.Name);
            throw new SerializationException($"Failed to deserialize to type {type.Name}", ex, type);
        }
    }

    /// <summary>
    /// Checks if this serializer can handle the specified content type
    /// </summary>
    /// <param name="contentType">Content type to check</param>
    /// <returns>True if can handle binary content types</returns>
    public bool CanHandle(string contentType)
    {
        if (string.IsNullOrWhiteSpace(contentType))
            return false;

        return contentType.Equals(Constants.ContentTypes.Binary, StringComparison.OrdinalIgnoreCase) ||
               contentType.StartsWith("application/octet-stream", StringComparison.OrdinalIgnoreCase) ||
               contentType.StartsWith("application/x-binary", StringComparison.OrdinalIgnoreCase);
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
            var baseSize = _fallbackSerializer.GetEstimatedSize(obj);
            
            // If compression is enabled, estimate compressed size (typically 60-80% of original)
            return _useCompression ? (int)(baseSize * 0.7) : baseSize;
        }
        catch
        {
            // Fallback estimation
            return EstimateSizeByType(typeof(T));
        }
    }

    private static int EstimateSizeByType(Type type)
    {
        // Binary is typically smaller than JSON
        if (type == typeof(string))
            return 80;
        if (type.IsValueType)
            return 40;
        return type.IsClass ? 160 : 80;
    }

    private static byte[] CompressData(byte[] data)
    {
        using var output = new MemoryStream();
        using var gzip = new System.IO.Compression.GZipStream(output, System.IO.Compression.CompressionMode.Compress);
        gzip.Write(data, 0, data.Length);
        gzip.Close();
        return output.ToArray();
    }

    private static async Task<byte[]> CompressDataAsync(byte[] data, CancellationToken cancellationToken)
    {
        using var output = new MemoryStream();
        await using var gzip = new System.IO.Compression.GZipStream(output, System.IO.Compression.CompressionMode.Compress);
        await gzip.WriteAsync(data, 0, data.Length, cancellationToken);
        await gzip.FlushAsync(cancellationToken);
        gzip.Close();
        return output.ToArray();
    }

    private static byte[] DecompressData(byte[] data)
    {
        using var input = new MemoryStream(data);
        using var gzip = new System.IO.Compression.GZipStream(input, System.IO.Compression.CompressionMode.Decompress);
        using var output = new MemoryStream();
        gzip.CopyTo(output);
        return output.ToArray();
    }

    private static async Task<byte[]> DecompressDataAsync(byte[] data, CancellationToken cancellationToken)
    {
        using var input = new MemoryStream(data);
        await using var gzip = new System.IO.Compression.GZipStream(input, System.IO.Compression.CompressionMode.Decompress);
        using var output = new MemoryStream();
        await gzip.CopyToAsync(output, cancellationToken);
        return output.ToArray();
    }
}
