namespace FS.Transport.AMQP.Serialization;

/// <summary>
/// Factory for creating and managing message serializers
/// </summary>
public interface IMessageSerializerFactory
{
    /// <summary>
    /// Gets a serializer for the specified content type
    /// </summary>
    /// <param name="contentType">Content type</param>
    /// <returns>Appropriate serializer</returns>
    IMessageSerializer GetSerializer(string contentType);
    
    /// <summary>
    /// Gets the default serializer
    /// </summary>
    /// <returns>Default serializer</returns>
    IMessageSerializer GetDefaultSerializer();
    
    /// <summary>
    /// Gets all available serializers
    /// </summary>
    /// <returns>Collection of serializers</returns>
    IEnumerable<IMessageSerializer> GetAllSerializers();
}
