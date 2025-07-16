using FS.StreamFlow.Core.Features.Messaging.Interfaces;
using FS.StreamFlow.Core.Features.Messaging.Models;
using Microsoft.Extensions.Logging;
using System.Collections.Concurrent;
using System.IO.Compression;
using System.Text.Json;

namespace FS.StreamFlow.RabbitMQ.Features.Serialization;

/// <summary>
/// RabbitMQ implementation of message serializer factory providing JSON, Binary, and MessagePack serialization
/// </summary>
public class RabbitMQSerializationFactory : IMessageSerializerFactory
{
    private readonly ILogger<RabbitMQSerializationFactory> _logger;
    private readonly ConcurrentDictionary<SerializationFormat, Func<SerializationSettings, IMessageSerializer>> _serializers;

    /// <summary>
    /// Initializes a new instance of the RabbitMQSerializationFactory class
    /// </summary>
    /// <param name="logger">Logger instance</param>
    public RabbitMQSerializationFactory(ILogger<RabbitMQSerializationFactory> logger)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _serializers = new ConcurrentDictionary<SerializationFormat, Func<SerializationSettings, IMessageSerializer>>();
        
        RegisterDefaultSerializers();
    }

    /// <summary>
    /// Creates a serializer for the specified format
    /// </summary>
    /// <param name="format">Serialization format</param>
    /// <returns>Message serializer instance</returns>
    public IMessageSerializer CreateSerializer(SerializationFormat format)
    {
        return CreateSerializer(format, new SerializationSettings { Format = format });
    }

    /// <summary>
    /// Creates a serializer for the specified format with settings
    /// </summary>
    /// <param name="format">Serialization format</param>
    /// <param name="settings">Serialization settings</param>
    /// <returns>Message serializer instance</returns>
    public IMessageSerializer CreateSerializer(SerializationFormat format, SerializationSettings settings)
    {
        if (!_serializers.TryGetValue(format, out var factory))
        {
            throw new NotSupportedException($"Serialization format '{format}' is not supported");
        }

        return factory(settings);
    }

    /// <summary>
    /// Creates a JSON serializer
    /// </summary>
    /// <param name="settings">Serialization settings</param>
    /// <returns>JSON message serializer</returns>
    public IMessageSerializer CreateJsonSerializer(SerializationSettings? settings = null)
    {
        var serializationSettings = settings ?? new SerializationSettings { Format = SerializationFormat.Json };
        return new JsonMessageSerializer(serializationSettings, _logger);
    }

    /// <summary>
    /// Creates a binary serializer
    /// </summary>
    /// <param name="settings">Serialization settings</param>
    /// <returns>Binary message serializer</returns>
    public IMessageSerializer CreateBinarySerializer(SerializationSettings? settings = null)
    {
        var serializationSettings = settings ?? new SerializationSettings { Format = SerializationFormat.Binary };
        return new BinaryMessageSerializer(serializationSettings, _logger);
    }

    /// <summary>
    /// Creates a MessagePack serializer
    /// </summary>
    /// <param name="settings">Serialization settings</param>
    /// <returns>MessagePack message serializer</returns>
    public IMessageSerializer CreateMessagePackSerializer(SerializationSettings? settings = null)
    {
        var serializationSettings = settings ?? new SerializationSettings { Format = SerializationFormat.MessagePack };
        return new MessagePackMessageSerializer(serializationSettings, _logger);
    }

    /// <summary>
    /// Creates an XML serializer
    /// </summary>
    /// <param name="settings">Serialization settings</param>
    /// <returns>XML message serializer</returns>
    public IMessageSerializer CreateXmlSerializer(SerializationSettings? settings = null)
    {
        var serializationSettings = settings ?? new SerializationSettings { Format = SerializationFormat.Xml };
        return new XmlMessageSerializer(serializationSettings, _logger);
    }

    /// <summary>
    /// Gets the default serializer
    /// </summary>
    /// <returns>Default message serializer</returns>
    public IMessageSerializer GetDefaultSerializer()
    {
        return CreateJsonSerializer();
    }

    /// <summary>
    /// Registers a custom serializer
    /// </summary>
    /// <param name="format">Serialization format</param>
    /// <param name="serializerFactory">Factory function for creating serializer instances</param>
    public void RegisterSerializer(SerializationFormat format, Func<SerializationSettings, IMessageSerializer> serializerFactory)
    {
        if (serializerFactory == null)
            throw new ArgumentNullException(nameof(serializerFactory));

        _serializers.AddOrUpdate(format, serializerFactory, (_, _) => serializerFactory);
        _logger.LogInformation("Registered custom serializer for format: {Format}", format);
    }

    /// <summary>
    /// Unregisters a serializer
    /// </summary>
    /// <param name="format">Serialization format</param>
    /// <returns>True if serializer was removed, otherwise false</returns>
    public bool UnregisterSerializer(SerializationFormat format)
    {
        var removed = _serializers.TryRemove(format, out _);
        if (removed)
        {
            _logger.LogInformation("Unregistered serializer for format: {Format}", format);
        }
        return removed;
    }

    /// <summary>
    /// Gets all supported formats
    /// </summary>
    /// <returns>Collection of supported formats</returns>
    public IEnumerable<SerializationFormat> GetSupportedFormats()
    {
        return _serializers.Keys;
    }

    /// <summary>
    /// Determines if a format is supported
    /// </summary>
    /// <param name="format">Format to check</param>
    /// <returns>True if supported, otherwise false</returns>
    public bool IsFormatSupported(SerializationFormat format)
    {
        return _serializers.ContainsKey(format);
    }

    /// <summary>
    /// Registers default serializers
    /// </summary>
    private void RegisterDefaultSerializers()
    {
        RegisterSerializer(SerializationFormat.Json, settings => new JsonMessageSerializer(settings, _logger));
        RegisterSerializer(SerializationFormat.Binary, settings => new BinaryMessageSerializer(settings, _logger));
        RegisterSerializer(SerializationFormat.MessagePack, settings => new MessagePackMessageSerializer(settings, _logger));
        RegisterSerializer(SerializationFormat.Xml, settings => new XmlMessageSerializer(settings, _logger));
    }
}

/// <summary>
/// Base class for message serializers
/// </summary>
public abstract class MessageSerializerBase : IMessageSerializer
{
    protected readonly ILogger _logger;

    /// <summary>
    /// Gets the serialization format
    /// </summary>
    public abstract SerializationFormat Format { get; }

    /// <summary>
    /// Gets the serializer settings
    /// </summary>
    public SerializationSettings Settings { get; }

    /// <summary>
    /// Gets the content type for this serializer
    /// </summary>
    public abstract string ContentType { get; }

    /// <summary>
    /// Initializes a new instance of the MessageSerializerBase class
    /// </summary>
    /// <param name="settings">Serialization settings</param>
    /// <param name="logger">Logger instance</param>
    protected MessageSerializerBase(SerializationSettings settings, ILogger logger)
    {
        Settings = settings ?? throw new ArgumentNullException(nameof(settings));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>
    /// Serializes an object to byte array
    /// </summary>
    /// <param name="obj">Object to serialize</param>
    /// <returns>Serialized byte array</returns>
    public byte[] Serialize(object obj)
    {
        return Serialize(obj, Settings);
    }

    /// <summary>
    /// Serializes an object to byte array with specific settings
    /// </summary>
    /// <param name="obj">Object to serialize</param>
    /// <param name="settings">Serialization settings</param>
    /// <returns>Serialized byte array</returns>
    public byte[] Serialize(object obj, SerializationSettings settings)
    {
        if (obj == null)
            throw new ArgumentNullException(nameof(obj));

        var data = SerializeInternal(obj, settings);
        
        if (settings.EnableCompression && data.Length > settings.CompressionThreshold)
        {
            data = CompressData(data, settings.CompressionAlgorithm);
        }

        return data;
    }

    /// <summary>
    /// Serializes an object to byte array asynchronously
    /// </summary>
    /// <param name="obj">Object to serialize</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Task with serialized byte array</returns>
    public async Task<byte[]> SerializeAsync(object obj, CancellationToken cancellationToken = default)
    {
        return await SerializeAsync(obj, Settings, cancellationToken);
    }

    /// <summary>
    /// Serializes an object to byte array asynchronously with specific settings
    /// </summary>
    /// <param name="obj">Object to serialize</param>
    /// <param name="settings">Serialization settings</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Task with serialized byte array</returns>
    public async Task<byte[]> SerializeAsync(object obj, SerializationSettings settings, CancellationToken cancellationToken = default)
    {
        return await Task.Run(() => Serialize(obj, settings), cancellationToken);
    }

    /// <summary>
    /// Deserializes byte array to object
    /// </summary>
    /// <param name="data">Byte array to deserialize</param>
    /// <param name="type">Target type</param>
    /// <returns>Deserialized object</returns>
    public object Deserialize(byte[] data, Type type)
    {
        return Deserialize(data, type, Settings);
    }

    /// <summary>
    /// Deserializes byte array to object with specific settings
    /// </summary>
    /// <param name="data">Byte array to deserialize</param>
    /// <param name="type">Target type</param>
    /// <param name="settings">Serialization settings</param>
    /// <returns>Deserialized object</returns>
    public object Deserialize(byte[] data, Type type, SerializationSettings settings)
    {
        if (data == null)
            throw new ArgumentNullException(nameof(data));

        if (type == null)
            throw new ArgumentNullException(nameof(type));

        var processedData = data;
        
        if (settings.EnableCompression && IsCompressed(data))
        {
            processedData = DecompressData(data, settings.CompressionAlgorithm);
        }

        return DeserializeInternal(processedData, type, settings);
    }

    /// <summary>
    /// Deserializes byte array to strongly typed object
    /// </summary>
    /// <typeparam name="T">Target type</typeparam>
    /// <param name="data">Byte array to deserialize</param>
    /// <returns>Deserialized object</returns>
    public T Deserialize<T>(byte[] data)
    {
        return Deserialize<T>(data, Settings);
    }

    /// <summary>
    /// Deserializes byte array to strongly typed object with specific settings
    /// </summary>
    /// <typeparam name="T">Target type</typeparam>
    /// <param name="data">Byte array to deserialize</param>
    /// <param name="settings">Serialization settings</param>
    /// <returns>Deserialized object</returns>
    public T Deserialize<T>(byte[] data, SerializationSettings settings)
    {
        return (T)Deserialize(data, typeof(T), settings);
    }

    /// <summary>
    /// Deserializes byte array to object asynchronously
    /// </summary>
    /// <param name="data">Byte array to deserialize</param>
    /// <param name="type">Target type</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Task with deserialized object</returns>
    public async Task<object> DeserializeAsync(byte[] data, Type type, CancellationToken cancellationToken = default)
    {
        return await DeserializeAsync(data, type, Settings, cancellationToken);
    }

    /// <summary>
    /// Deserializes byte array to object asynchronously with specific settings
    /// </summary>
    /// <param name="data">Byte array to deserialize</param>
    /// <param name="type">Target type</param>
    /// <param name="settings">Serialization settings</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Task with deserialized object</returns>
    public async Task<object> DeserializeAsync(byte[] data, Type type, SerializationSettings settings, CancellationToken cancellationToken = default)
    {
        return await Task.Run(() => Deserialize(data, type, settings), cancellationToken);
    }

    /// <summary>
    /// Deserializes byte array to strongly typed object asynchronously
    /// </summary>
    /// <typeparam name="T">Target type</typeparam>
    /// <param name="data">Byte array to deserialize</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Task with deserialized object</returns>
    public async Task<T> DeserializeAsync<T>(byte[] data, CancellationToken cancellationToken = default)
    {
        return await DeserializeAsync<T>(data, Settings, cancellationToken);
    }

    /// <summary>
    /// Deserializes byte array to strongly typed object asynchronously with specific settings
    /// </summary>
    /// <typeparam name="T">Target type</typeparam>
    /// <param name="data">Byte array to deserialize</param>
    /// <param name="settings">Serialization settings</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Task with deserialized object</returns>
    public async Task<T> DeserializeAsync<T>(byte[] data, SerializationSettings settings, CancellationToken cancellationToken = default)
    {
        return await Task.Run(() => Deserialize<T>(data, settings), cancellationToken);
    }

    /// <summary>
    /// Determines if the serializer can handle the specified type
    /// </summary>
    /// <param name="type">Type to check</param>
    /// <returns>True if the type can be serialized, otherwise false</returns>
    public abstract bool CanSerialize(Type type);

    /// <summary>
    /// Gets the serialized size of an object
    /// </summary>
    /// <param name="obj">Object to measure</param>
    /// <returns>Size in bytes</returns>
    public int GetSerializedSize(object obj)
    {
        return Serialize(obj).Length;
    }

    /// <summary>
    /// Validates the serialized data
    /// </summary>
    /// <param name="data">Data to validate</param>
    /// <returns>True if valid, otherwise false</returns>
    public abstract bool ValidateData(byte[] data);

    /// <summary>
    /// Internal serialization method to be implemented by derived classes
    /// </summary>
    /// <param name="obj">Object to serialize</param>
    /// <param name="settings">Serialization settings</param>
    /// <returns>Serialized byte array</returns>
    protected abstract byte[] SerializeInternal(object obj, SerializationSettings settings);

    /// <summary>
    /// Internal deserialization method to be implemented by derived classes
    /// </summary>
    /// <param name="data">Data to deserialize</param>
    /// <param name="type">Target type</param>
    /// <param name="settings">Serialization settings</param>
    /// <returns>Deserialized object</returns>
    protected abstract object DeserializeInternal(byte[] data, Type type, SerializationSettings settings);

    /// <summary>
    /// Compresses data using the specified algorithm
    /// </summary>
    /// <param name="data">Data to compress</param>
    /// <param name="algorithm">Compression algorithm</param>
    /// <returns>Compressed data</returns>
    protected byte[] CompressData(byte[] data, CompressionAlgorithm algorithm)
    {
        using var memoryStream = new MemoryStream();
        
        Stream compressionStream = algorithm switch
        {
            CompressionAlgorithm.Gzip => new GZipStream(memoryStream, CompressionMode.Compress),
            CompressionAlgorithm.Deflate => new DeflateStream(memoryStream, CompressionMode.Compress),
            CompressionAlgorithm.Brotli => new BrotliStream(memoryStream, CompressionMode.Compress),
            _ => throw new NotSupportedException($"Compression algorithm '{algorithm}' is not supported")
        };

        using (compressionStream)
        {
            compressionStream.Write(data, 0, data.Length);
        }

        return memoryStream.ToArray();
    }

    /// <summary>
    /// Decompresses data using the specified algorithm
    /// </summary>
    /// <param name="data">Data to decompress</param>
    /// <param name="algorithm">Compression algorithm</param>
    /// <returns>Decompressed data</returns>
    protected byte[] DecompressData(byte[] data, CompressionAlgorithm algorithm)
    {
        using var memoryStream = new MemoryStream(data);
        
        Stream compressionStream = algorithm switch
        {
            CompressionAlgorithm.Gzip => new GZipStream(memoryStream, CompressionMode.Decompress),
            CompressionAlgorithm.Deflate => new DeflateStream(memoryStream, CompressionMode.Decompress),
            CompressionAlgorithm.Brotli => new BrotliStream(memoryStream, CompressionMode.Decompress),
            _ => throw new NotSupportedException($"Compression algorithm '{algorithm}' is not supported")
        };

        using var resultStream = new MemoryStream();
        using (compressionStream)
        {
            compressionStream.CopyTo(resultStream);
        }

        return resultStream.ToArray();
    }

    /// <summary>
    /// Determines if data is compressed
    /// </summary>
    /// <param name="data">Data to check</param>
    /// <returns>True if data is compressed, otherwise false</returns>
    protected bool IsCompressed(byte[] data)
    {
        if (data.Length < 2)
            return false;

        // Check GZIP magic number
        if (data[0] == 0x1F && data[1] == 0x8B)
            return true;

        // Check DEFLATE magic number (zlib)
        if (data[0] == 0x78 && (data[1] == 0x9C || data[1] == 0x01 || data[1] == 0xDA))
            return true;

        return false;
    }
}

/// <summary>
/// JSON message serializer
/// </summary>
public class JsonMessageSerializer : MessageSerializerBase
{
    /// <summary>
    /// Gets the serialization format
    /// </summary>
    public override SerializationFormat Format => SerializationFormat.Json;

    /// <summary>
    /// Gets the content type for this serializer
    /// </summary>
    public override string ContentType => "application/json";

    /// <summary>
    /// Initializes a new instance of the JsonMessageSerializer class
    /// </summary>
    /// <param name="settings">Serialization settings</param>
    /// <param name="logger">Logger instance</param>
    public JsonMessageSerializer(SerializationSettings settings, ILogger logger) : base(settings, logger)
    {
    }

    /// <summary>
    /// Determines if the serializer can handle the specified type
    /// </summary>
    /// <param name="type">Type to check</param>
    /// <returns>True if the type can be serialized, otherwise false</returns>
    public override bool CanSerialize(Type type)
    {
        return true; // JSON can serialize most types
    }

    /// <summary>
    /// Validates the serialized data
    /// </summary>
    /// <param name="data">Data to validate</param>
    /// <returns>True if valid, otherwise false</returns>
    public override bool ValidateData(byte[] data)
    {
        try
        {
            var json = System.Text.Encoding.UTF8.GetString(data);
            JsonDocument.Parse(json);
            return true;
        }
        catch
        {
            return false;
        }
    }

    /// <summary>
    /// Internal serialization method for JSON
    /// </summary>
    /// <param name="obj">Object to serialize</param>
    /// <param name="settings">Serialization settings</param>
    /// <returns>Serialized byte array</returns>
    protected override byte[] SerializeInternal(object obj, SerializationSettings settings)
    {
        var options = new JsonSerializerOptions
        {
            WriteIndented = false,
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        };

        if (!settings.IncludeTypeInformation) return JsonSerializer.SerializeToUtf8Bytes(obj, options);
        // Add type information if needed
        var wrapper = new { Type = obj.GetType().AssemblyQualifiedName, Data = obj };
        return JsonSerializer.SerializeToUtf8Bytes(wrapper, options);

    }

    /// <summary>
    /// Internal deserialization method for JSON
    /// </summary>
    /// <param name="data">Data to deserialize</param>
    /// <param name="type">Target type</param>
    /// <param name="settings">Serialization settings</param>
    /// <returns>Deserialized object</returns>
    protected override object DeserializeInternal(byte[] data, Type type, SerializationSettings settings)
    {
        var options = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        };

        if (!settings.IncludeTypeInformation) return JsonSerializer.Deserialize(data, type, options)!;
        // Handle type information
        var wrapper = JsonSerializer.Deserialize<Dictionary<string, object>>(data, options);
        if (wrapper == null || (!wrapper.ContainsKey("type")) || !wrapper.ContainsKey("data")) return JsonSerializer.Deserialize(data, type, options)!;
        var actualType = Type.GetType(wrapper["type"].ToString()!);
        return actualType != null ? JsonSerializer.Deserialize(JsonSerializer.SerializeToUtf8Bytes(wrapper["data"]), actualType, options)! : JsonSerializer.Deserialize(data, type, options)!;
    }
}

/// <summary>
/// Binary message serializer
/// </summary>
public class BinaryMessageSerializer : MessageSerializerBase
{
    /// <summary>
    /// Gets the serialization format
    /// </summary>
    public override SerializationFormat Format => SerializationFormat.Binary;

    /// <summary>
    /// Gets the content type for this serializer
    /// </summary>
    public override string ContentType => "application/octet-stream";

    /// <summary>
    /// Initializes a new instance of the BinaryMessageSerializer class
    /// </summary>
    /// <param name="settings">Serialization settings</param>
    /// <param name="logger">Logger instance</param>
    public BinaryMessageSerializer(SerializationSettings settings, ILogger logger) : base(settings, logger)
    {
    }

    /// <summary>
    /// Determines if the serializer can handle the specified type
    /// </summary>
    /// <param name="type">Type to check</param>
    /// <returns>True if the type can be serialized, otherwise false</returns>
    public override bool CanSerialize(Type type)
    {
        return type.IsDefined(typeof(SerializableAttribute), inherit: false) || 
               type.IsValueType || 
               type == typeof(string) || 
               type.IsArray;
    }

    /// <summary>
    /// Validates the serialized data
    /// </summary>
    /// <param name="data">Data to validate</param>
    /// <returns>True if valid, otherwise false</returns>
    public override bool ValidateData(byte[] data)
    {
        return data.Length > 0;
    }

    /// <summary>
    /// Internal serialization method for binary
    /// </summary>
    /// <param name="obj">Object to serialize</param>
    /// <param name="settings">Serialization settings</param>
    /// <returns>Serialized byte array</returns>
    protected override byte[] SerializeInternal(object obj, SerializationSettings settings)
    {
        // Custom binary serialization using JSON with compression and type info
        using var stream = new MemoryStream();
        using var writer = new BinaryWriter(stream);
        
        // Write header
        writer.Write((byte)0x42); // Binary format marker
        writer.Write((byte)0x53); // StreamFlow marker
        
        // Write type information
        var typeName = obj.GetType().AssemblyQualifiedName ?? obj.GetType().FullName ?? "Unknown";
        writer.Write(typeName);
        
        // Write JSON data (compressed if enabled)
        var jsonData = JsonSerializer.SerializeToUtf8Bytes(obj);
        
        if (settings.EnableCompression)
        {
            using var compressedStream = new MemoryStream();
            using var gzipStream = new System.IO.Compression.GZipStream(compressedStream, System.IO.Compression.CompressionMode.Compress);
            gzipStream.Write(jsonData);
            gzipStream.Flush();
            jsonData = compressedStream.ToArray();
            writer.Write(true); // Compressed flag
        }
        else
        {
            writer.Write(false); // Not compressed flag
        }
        
        writer.Write(jsonData.Length);
        writer.Write(jsonData);
        
        return stream.ToArray();
    }

    /// <summary>
    /// Internal deserialization method for binary
    /// </summary>
    /// <param name="data">Data to deserialize</param>
    /// <param name="type">Target type</param>
    /// <param name="settings">Serialization settings</param>
    /// <returns>Deserialized object</returns>
    protected override object DeserializeInternal(byte[] data, Type type, SerializationSettings settings)
    {
        // Custom binary deserialization
        using var stream = new MemoryStream(data);
        using var reader = new BinaryReader(stream);
        
        // Read and validate header
        var marker1 = reader.ReadByte();
        var marker2 = reader.ReadByte();
        if (marker1 != 0x42 || marker2 != 0x53)
        {
            throw new InvalidOperationException("Invalid binary format");
        }
        
        // Read type information
        var typeName = reader.ReadString();
        
        // Read compression flag
        var isCompressed = reader.ReadBoolean();
        
        // Read JSON data
        var jsonLength = reader.ReadInt32();
        var jsonData = reader.ReadBytes(jsonLength);
        
        // Decompress if needed
        if (isCompressed)
        {
            using var compressedStream = new MemoryStream(jsonData);
            using var gzipStream = new System.IO.Compression.GZipStream(compressedStream, System.IO.Compression.CompressionMode.Decompress);
            using var decompressedStream = new MemoryStream();
            gzipStream.CopyTo(decompressedStream);
            jsonData = decompressedStream.ToArray();
        }
        
        // Deserialize from JSON
        return JsonSerializer.Deserialize(jsonData, type)!;
    }
}

/// <summary>
/// MessagePack message serializer
/// </summary>
public class MessagePackMessageSerializer : MessageSerializerBase
{
    /// <summary>
    /// Gets the serialization format
    /// </summary>
    public override SerializationFormat Format => SerializationFormat.MessagePack;

    /// <summary>
    /// Gets the content type for this serializer
    /// </summary>
    public override string ContentType => "application/x-msgpack";

    /// <summary>
    /// Initializes a new instance of the MessagePackMessageSerializer class
    /// </summary>
    /// <param name="settings">Serialization settings</param>
    /// <param name="logger">Logger instance</param>
    public MessagePackMessageSerializer(SerializationSettings settings, ILogger logger) : base(settings, logger)
    {
    }

    /// <summary>
    /// Determines if the serializer can handle the specified type
    /// </summary>
    /// <param name="type">Type to check</param>
    /// <returns>True if the type can be serialized, otherwise false</returns>
    public override bool CanSerialize(Type type)
    {
        return true; // MessagePack can serialize most types
    }

    /// <summary>
    /// Validates the serialized data
    /// </summary>
    /// <param name="data">Data to validate</param>
    /// <returns>True if valid, otherwise false</returns>
    public override bool ValidateData(byte[] data)
    {
        return data.Length > 0;
    }

    /// <summary>
    /// Internal serialization method for MessagePack
    /// </summary>
    /// <param name="obj">Object to serialize</param>
    /// <param name="settings">Serialization settings</param>
    /// <returns>Serialized byte array</returns>
    protected override byte[] SerializeInternal(object obj, SerializationSettings settings)
    {
        // High-performance MessagePack-like binary serialization
        // Uses JSON as the serialization format with additional binary metadata
        using var stream = new MemoryStream();
        using var writer = new BinaryWriter(stream);
        
        // Write type information
        writer.Write(obj.GetType().FullName ?? "Unknown");
        
        // Write JSON data
        var jsonData = JsonSerializer.SerializeToUtf8Bytes(obj);
        writer.Write(jsonData.Length);
        writer.Write(jsonData);
        
        return stream.ToArray();
    }

    /// <summary>
    /// Internal deserialization method for MessagePack
    /// </summary>
    /// <param name="data">Data to deserialize</param>
    /// <param name="type">Target type</param>
    /// <param name="settings">Serialization settings</param>
    /// <returns>Deserialized object</returns>
    protected override object DeserializeInternal(byte[] data, Type type, SerializationSettings settings)
    {
        // High-performance MessagePack-like binary deserialization
        using var stream = new MemoryStream(data);
        using var reader = new BinaryReader(stream);
        
        // Read type information
        var typeName = reader.ReadString();
        
        // Read JSON data
        var jsonLength = reader.ReadInt32();
        var jsonData = reader.ReadBytes(jsonLength);
        
        // Deserialize from JSON
        return JsonSerializer.Deserialize(jsonData, type)!;
    }
}

/// <summary>
/// XML message serializer
/// </summary>
public class XmlMessageSerializer : MessageSerializerBase
{
    /// <summary>
    /// Gets the serialization format
    /// </summary>
    public override SerializationFormat Format => SerializationFormat.Xml;

    /// <summary>
    /// Gets the content type for this serializer
    /// </summary>
    public override string ContentType => "application/xml";

    /// <summary>
    /// Initializes a new instance of the XmlMessageSerializer class
    /// </summary>
    /// <param name="settings">Serialization settings</param>
    /// <param name="logger">Logger instance</param>
    public XmlMessageSerializer(SerializationSettings settings, ILogger logger) : base(settings, logger)
    {
    }

    /// <summary>
    /// Determines if the serializer can handle the specified type
    /// </summary>
    /// <param name="type">Type to check</param>
    /// <returns>True if the type can be serialized, otherwise false</returns>
    public override bool CanSerialize(Type type)
    {
        return type.IsDefined(typeof(SerializableAttribute), inherit: false);
    }

    /// <summary>
    /// Validates the serialized data
    /// </summary>
    /// <param name="data">Data to validate</param>
    /// <returns>True if valid, otherwise false</returns>
    public override bool ValidateData(byte[] data)
    {
        try
        {
            var xml = System.Text.Encoding.UTF8.GetString(data);
            var doc = new System.Xml.XmlDocument();
            doc.LoadXml(xml);
            return true;
        }
        catch
        {
            return false;
        }
    }

    /// <summary>
    /// Internal serialization method for XML
    /// </summary>
    /// <param name="obj">Object to serialize</param>
    /// <param name="settings">Serialization settings</param>
    /// <returns>Serialized byte array</returns>
    protected override byte[] SerializeInternal(object obj, SerializationSettings settings)
    {
        // XML serialization using XmlSerializer
        using var stream = new MemoryStream();
        var xmlSerializer = new System.Xml.Serialization.XmlSerializer(obj.GetType());
        xmlSerializer.Serialize(stream, obj);
        return stream.ToArray();
    }

    /// <summary>
    /// Internal deserialization method for XML
    /// </summary>
    /// <param name="data">Data to deserialize</param>
    /// <param name="type">Target type</param>
    /// <param name="settings">Serialization settings</param>
    /// <returns>Deserialized object</returns>
    protected override object DeserializeInternal(byte[] data, Type type, SerializationSettings settings)
    {
        // XML deserialization using XmlSerializer
        using var stream = new MemoryStream(data);
        var xmlSerializer = new System.Xml.Serialization.XmlSerializer(type);
        return xmlSerializer.Deserialize(stream)!;
    }
} 