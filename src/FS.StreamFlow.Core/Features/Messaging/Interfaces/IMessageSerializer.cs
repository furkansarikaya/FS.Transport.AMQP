using FS.StreamFlow.Core.Features.Messaging.Models;

namespace FS.StreamFlow.Core.Features.Messaging.Interfaces;

/// <summary>
/// Interface for message serialization providing support for multiple formats (JSON, Binary, MessagePack, XML) with compression and type information
/// </summary>
public interface IMessageSerializer
{
    /// <summary>
    /// Gets the serialization format
    /// </summary>
    SerializationFormat Format { get; }
    
    /// <summary>
    /// Gets the serializer settings
    /// </summary>
    SerializationSettings Settings { get; }
    
    /// <summary>
    /// Gets the content type for this serializer
    /// </summary>
    string ContentType { get; }
    
    /// <summary>
    /// Serializes an object to byte array
    /// </summary>
    /// <param name="obj">Object to serialize</param>
    /// <returns>Serialized byte array</returns>
    byte[] Serialize(object obj);
    
    /// <summary>
    /// Serializes an object to byte array with specific settings
    /// </summary>
    /// <param name="obj">Object to serialize</param>
    /// <param name="settings">Serialization settings</param>
    /// <returns>Serialized byte array</returns>
    byte[] Serialize(object obj, SerializationSettings settings);
    
    /// <summary>
    /// Serializes an object to byte array asynchronously
    /// </summary>
    /// <param name="obj">Object to serialize</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Task with serialized byte array</returns>
    Task<byte[]> SerializeAsync(object obj, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Serializes an object to byte array asynchronously with specific settings
    /// </summary>
    /// <param name="obj">Object to serialize</param>
    /// <param name="settings">Serialization settings</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Task with serialized byte array</returns>
    Task<byte[]> SerializeAsync(object obj, SerializationSettings settings, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Deserializes byte array to object
    /// </summary>
    /// <param name="data">Byte array to deserialize</param>
    /// <param name="type">Target type</param>
    /// <returns>Deserialized object</returns>
    object Deserialize(byte[] data, Type type);
    
    /// <summary>
    /// Deserializes byte array to object with specific settings
    /// </summary>
    /// <param name="data">Byte array to deserialize</param>
    /// <param name="type">Target type</param>
    /// <param name="settings">Serialization settings</param>
    /// <returns>Deserialized object</returns>
    object Deserialize(byte[] data, Type type, SerializationSettings settings);
    
    /// <summary>
    /// Deserializes byte array to strongly typed object
    /// </summary>
    /// <typeparam name="T">Target type</typeparam>
    /// <param name="data">Byte array to deserialize</param>
    /// <returns>Deserialized object</returns>
    T Deserialize<T>(byte[] data);
    
    /// <summary>
    /// Deserializes byte array to strongly typed object with specific settings
    /// </summary>
    /// <typeparam name="T">Target type</typeparam>
    /// <param name="data">Byte array to deserialize</param>
    /// <param name="settings">Serialization settings</param>
    /// <returns>Deserialized object</returns>
    T Deserialize<T>(byte[] data, SerializationSettings settings);
    
    /// <summary>
    /// Deserializes byte array to object asynchronously
    /// </summary>
    /// <param name="data">Byte array to deserialize</param>
    /// <param name="type">Target type</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Task with deserialized object</returns>
    Task<object> DeserializeAsync(byte[] data, Type type, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Deserializes byte array to object asynchronously with specific settings
    /// </summary>
    /// <param name="data">Byte array to deserialize</param>
    /// <param name="type">Target type</param>
    /// <param name="settings">Serialization settings</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Task with deserialized object</returns>
    Task<object> DeserializeAsync(byte[] data, Type type, SerializationSettings settings, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Deserializes byte array to strongly typed object asynchronously
    /// </summary>
    /// <typeparam name="T">Target type</typeparam>
    /// <param name="data">Byte array to deserialize</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Task with deserialized object</returns>
    Task<T> DeserializeAsync<T>(byte[] data, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Deserializes byte array to strongly typed object asynchronously with specific settings
    /// </summary>
    /// <typeparam name="T">Target type</typeparam>
    /// <param name="data">Byte array to deserialize</param>
    /// <param name="settings">Serialization settings</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Task with deserialized object</returns>
    Task<T> DeserializeAsync<T>(byte[] data, SerializationSettings settings, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Determines if the serializer can handle the specified type
    /// </summary>
    /// <param name="type">Type to check</param>
    /// <returns>True if the type can be serialized, otherwise false</returns>
    bool CanSerialize(Type type);
    
    /// <summary>
    /// Gets the serialized size of an object
    /// </summary>
    /// <param name="obj">Object to measure</param>
    /// <returns>Size in bytes</returns>
    int GetSerializedSize(object obj);
    
    /// <summary>
    /// Validates the serialized data
    /// </summary>
    /// <param name="data">Data to validate</param>
    /// <returns>True if valid, otherwise false</returns>
    bool ValidateData(byte[] data);
}

/// <summary>
/// Factory interface for creating message serializers
/// </summary>
public interface IMessageSerializerFactory
{
    /// <summary>
    /// Creates a serializer for the specified format
    /// </summary>
    /// <param name="format">Serialization format</param>
    /// <returns>Message serializer instance</returns>
    IMessageSerializer CreateSerializer(SerializationFormat format);
    
    /// <summary>
    /// Creates a serializer for the specified format with settings
    /// </summary>
    /// <param name="format">Serialization format</param>
    /// <param name="settings">Serialization settings</param>
    /// <returns>Message serializer instance</returns>
    IMessageSerializer CreateSerializer(SerializationFormat format, SerializationSettings settings);
    
    /// <summary>
    /// Creates a JSON serializer
    /// </summary>
    /// <param name="settings">Serialization settings</param>
    /// <returns>JSON message serializer</returns>
    IMessageSerializer CreateJsonSerializer(SerializationSettings? settings = null);
    
    /// <summary>
    /// Creates a binary serializer
    /// </summary>
    /// <param name="settings">Serialization settings</param>
    /// <returns>Binary message serializer</returns>
    IMessageSerializer CreateBinarySerializer(SerializationSettings? settings = null);
    
    /// <summary>
    /// Creates a MessagePack serializer
    /// </summary>
    /// <param name="settings">Serialization settings</param>
    /// <returns>MessagePack message serializer</returns>
    IMessageSerializer CreateMessagePackSerializer(SerializationSettings? settings = null);
    
    /// <summary>
    /// Creates an XML serializer
    /// </summary>
    /// <param name="settings">Serialization settings</param>
    /// <returns>XML message serializer</returns>
    IMessageSerializer CreateXmlSerializer(SerializationSettings? settings = null);
    
    /// <summary>
    /// Gets the default serializer
    /// </summary>
    /// <returns>Default message serializer</returns>
    IMessageSerializer GetDefaultSerializer();
    
    /// <summary>
    /// Registers a custom serializer
    /// </summary>
    /// <param name="format">Serialization format</param>
    /// <param name="serializerFactory">Factory function for creating serializer instances</param>
    void RegisterSerializer(SerializationFormat format, Func<SerializationSettings, IMessageSerializer> serializerFactory);
    
    /// <summary>
    /// Unregisters a serializer
    /// </summary>
    /// <param name="format">Serialization format</param>
    /// <returns>True if serializer was removed, otherwise false</returns>
    bool UnregisterSerializer(SerializationFormat format);
    
    /// <summary>
    /// Gets all supported formats
    /// </summary>
    /// <returns>Collection of supported formats</returns>
    IEnumerable<SerializationFormat> GetSupportedFormats();
    
    /// <summary>
    /// Determines if a format is supported
    /// </summary>
    /// <param name="format">Format to check</param>
    /// <returns>True if supported, otherwise false</returns>
    bool IsFormatSupported(SerializationFormat format);
}

/// <summary>
/// Result of serialization operation
/// </summary>
public class SerializationResult
{
    /// <summary>
    /// Serialized data
    /// </summary>
    public byte[] Data { get; }
    
    /// <summary>
    /// Content type
    /// </summary>
    public string ContentType { get; }
    
    /// <summary>
    /// Content encoding
    /// </summary>
    public string? ContentEncoding { get; }
    
    /// <summary>
    /// Whether data is compressed
    /// </summary>
    public bool IsCompressed { get; }
    
    /// <summary>
    /// Original size before compression
    /// </summary>
    public int OriginalSize { get; }
    
    /// <summary>
    /// Compressed size
    /// </summary>
    public int CompressedSize { get; }
    
    /// <summary>
    /// Compression ratio
    /// </summary>
    public double CompressionRatio => OriginalSize > 0 ? (double)CompressedSize / OriginalSize : 1.0;
    
    /// <summary>
    /// Serialization format used
    /// </summary>
    public SerializationFormat Format { get; }
    
    /// <summary>
    /// Serialization timestamp
    /// </summary>
    public DateTimeOffset Timestamp { get; }
    
    /// <summary>
    /// Type information
    /// </summary>
    public string? TypeInfo { get; }
    
    /// <summary>
    /// Initializes a new instance of the SerializationResult class
    /// </summary>
    /// <param name="data">Serialized data</param>
    /// <param name="contentType">Content type</param>
    /// <param name="format">Serialization format</param>
    /// <param name="originalSize">Original size</param>
    /// <param name="isCompressed">Whether data is compressed</param>
    /// <param name="contentEncoding">Content encoding</param>
    /// <param name="typeInfo">Type information</param>
    public SerializationResult(
        byte[] data, 
        string contentType, 
        SerializationFormat format, 
        int originalSize, 
        bool isCompressed = false, 
        string? contentEncoding = null, 
        string? typeInfo = null)
    {
        Data = data ?? throw new ArgumentNullException(nameof(data));
        ContentType = contentType ?? throw new ArgumentNullException(nameof(contentType));
        Format = format;
        OriginalSize = originalSize;
        IsCompressed = isCompressed;
        ContentEncoding = contentEncoding;
        TypeInfo = typeInfo;
        CompressedSize = data.Length;
        Timestamp = DateTimeOffset.UtcNow;
    }
} 