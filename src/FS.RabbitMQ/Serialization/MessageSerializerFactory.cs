namespace FS.RabbitMQ.Serialization;

/// <summary>
/// Default implementation of message serializer factory
/// </summary>
public class MessageSerializerFactory : IMessageSerializerFactory
{
    private readonly IEnumerable<IMessageSerializer> _serializers;
    private readonly IMessageSerializer _defaultSerializer;

    public MessageSerializerFactory(IEnumerable<IMessageSerializer> serializers)
    {
        _serializers = serializers ?? throw new ArgumentNullException(nameof(serializers));
        _defaultSerializer = _serializers.GetDefaultSerializer();
    }

    /// <summary>
    /// Gets a serializer for the specified content type
    /// </summary>
    /// <param name="contentType">Content type</param>
    /// <returns>Appropriate serializer</returns>
    public IMessageSerializer GetSerializer(string contentType)
    {
        return _serializers.GetSerializer(contentType) ?? _defaultSerializer;
    }

    /// <summary>
    /// Gets the default serializer
    /// </summary>
    /// <returns>Default serializer</returns>
    public IMessageSerializer GetDefaultSerializer()
    {
        return _defaultSerializer;
    }

    /// <summary>
    /// Gets all available serializers
    /// </summary>
    /// <returns>Collection of serializers</returns>
    public IEnumerable<IMessageSerializer> GetAllSerializers()
    {
        return _serializers;
    }
}