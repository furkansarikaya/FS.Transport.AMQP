using FS.RabbitMQ.Core;

namespace FS.RabbitMQ.Configuration;

/// <summary>
/// Settings for message serialization and deserialization
/// </summary>
public class SerializerSettings
{
    /// <summary>
    /// Default serializer type to use
    /// </summary>
    public string DefaultSerializer { get; set; } = "Json";
    
    /// <summary>
    /// Serializer type (Json, MessagePack, ProtoBuf, etc.)
    /// </summary>
    public string SerializerType { get; set; } = "Json";
    
    /// <summary>
    /// Whether to enable compression
    /// </summary>
    public bool EnableCompression { get; set; } = false;
    
    /// <summary>
    /// Compression algorithm to use
    /// </summary>
    public CompressionAlgorithm CompressionAlgorithm { get; set; } = CompressionAlgorithm.GZip;
    
    /// <summary>
    /// Whether to enable encryption
    /// </summary>
    public bool EnableEncryption { get; set; } = false;
    
    /// <summary>
    /// Encryption key for message encryption/decryption
    /// </summary>
    public string? EncryptionKey { get; set; }
    
    /// <summary>
    /// Whether to include type information in serialized data
    /// </summary>
    public bool IncludeTypeInformation { get; set; } = true;
    
    /// <summary>
    /// Whether to use camel case property names
    /// </summary>
    public bool UseCamelCase { get; set; } = true;
    
    /// <summary>
    /// Whether to ignore null values during serialization
    /// </summary>
    public bool IgnoreNullValues { get; set; } = true;
    
    /// <summary>
    /// Date/time format for serialization
    /// </summary>
    public string DateTimeFormat { get; set; } = "yyyy-MM-ddTHH:mm:ss.fffZ";
    
    /// <summary>
    /// Custom converters for specific types
    /// </summary>
    public IDictionary<string, object> CustomConverters { get; set; } = new Dictionary<string, object>();
    
    /// <summary>
    /// Validates the serializer settings
    /// </summary>
    /// <exception cref="ArgumentException">Thrown when settings are invalid</exception>
    public void Validate()
    {
        var validTypes = new[] { "Json", "MessagePack", "ProtoBuf", "Avro", "Xml" };
        if (!validTypes.Contains(SerializerType))
            throw new ArgumentException($"Serializer type must be one of: {string.Join(", ", validTypes)}");
            
        if (!validTypes.Contains(DefaultSerializer))
            throw new ArgumentException($"Default serializer must be one of: {string.Join(", ", validTypes)}");
            
        if (EnableEncryption && string.IsNullOrWhiteSpace(EncryptionKey))
            throw new ArgumentException("EncryptionKey is required when encryption is enabled", nameof(EncryptionKey));
            
        if (string.IsNullOrWhiteSpace(DateTimeFormat))
            throw new ArgumentException("DateTimeFormat cannot be empty", nameof(DateTimeFormat));
    }
    
    /// <summary>
    /// Creates a copy of the serializer settings
    /// </summary>
    /// <returns>New instance with copied values</returns>
    public SerializerSettings Clone()
    {
        return new SerializerSettings
        {
            DefaultSerializer = DefaultSerializer,
            SerializerType = SerializerType,
            EnableCompression = EnableCompression,
            CompressionAlgorithm = CompressionAlgorithm,
            EnableEncryption = EnableEncryption,
            EncryptionKey = EncryptionKey,
            IncludeTypeInformation = IncludeTypeInformation,
            UseCamelCase = UseCamelCase,
            IgnoreNullValues = IgnoreNullValues,
            DateTimeFormat = DateTimeFormat,
            CustomConverters = new Dictionary<string, object>(CustomConverters)
        };
    }
    
    /// <summary>
    /// Creates default JSON serializer settings
    /// </summary>
    /// <returns>JSON serializer settings</returns>
    public static SerializerSettings CreateJson()
    {
        return new SerializerSettings
        {
            DefaultSerializer = "Json",
            SerializerType = "Json",
            UseCamelCase = true,
            IgnoreNullValues = true,
            IncludeTypeInformation = false
        };
    }
    
    /// <summary>
    /// Creates default MessagePack serializer settings
    /// </summary>
    /// <returns>MessagePack serializer settings</returns>
    public static SerializerSettings CreateMessagePack()
    {
        return new SerializerSettings
        {
            DefaultSerializer = "MessagePack",
            SerializerType = "MessagePack",
            EnableCompression = true,
            CompressionAlgorithm = CompressionAlgorithm.LZ4,
            IncludeTypeInformation = true
        };
    }
    
    /// <summary>
    /// Creates default ProtoBuf serializer settings
    /// </summary>
    /// <returns>ProtoBuf serializer settings</returns>
    public static SerializerSettings CreateProtoBuf()
    {
        return new SerializerSettings
        {
            DefaultSerializer = "ProtoBuf",
            SerializerType = "ProtoBuf",
            EnableCompression = true,
            CompressionAlgorithm = CompressionAlgorithm.GZip,
            IncludeTypeInformation = false
        };
    }
} 