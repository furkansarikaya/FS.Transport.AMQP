namespace FS.RabbitMQ.Core.Abstractions;

/// <summary>
/// Defines the contract for custom message serialization implementations.
/// </summary>
/// <remarks>
/// Custom serializers enable integration with specialized serialization frameworks,
/// optimization for specific data types, and implementation of custom encoding
/// and compression strategies.
/// </remarks>
public interface IMessageSerializer
{
    /// <summary>
    /// Serializes an object to bytes for message transmission.
    /// </summary>
    /// <param name="obj">The object to serialize.</param>
    /// <param name="contentType">The target content type for serialization.</param>
    /// <returns>The serialized byte array representing the object.</returns>
    byte[] Serialize(object obj, string contentType);

    /// <summary>
    /// Deserializes bytes to an object of the specified type.
    /// </summary>
    /// <param name="data">The serialized data to deserialize.</param>
    /// <param name="targetType">The type to deserialize to.</param>
    /// <param name="contentType">The content type of the serialized data.</param>
    /// <returns>The deserialized object instance.</returns>
    object Deserialize(byte[] data, Type targetType, string contentType);

    /// <summary>
    /// Gets the supported content types for this serializer.
    /// </summary>
    /// <value>A collection of MIME types that this serializer can handle.</value>
    IEnumerable<string> SupportedContentTypes { get; }
}