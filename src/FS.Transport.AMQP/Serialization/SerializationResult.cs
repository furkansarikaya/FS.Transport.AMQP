namespace FS.Transport.AMQP.Serialization;

/// <summary>
/// Result of serialization operation including content type
/// </summary>
public class SerializationResult
{
    /// <summary>
    /// Serialized data
    /// </summary>
    public byte[] Data { get; }
    
    /// <summary>
    /// Content type used for serialization
    /// </summary>
    public string ContentType { get; }
    
    /// <summary>
    /// Size of serialized data in bytes
    /// </summary>
    public int Size => Data.Length;

    public SerializationResult(byte[] data, string contentType)
    {
        Data = data ?? throw new ArgumentNullException(nameof(data));
        ContentType = contentType ?? throw new ArgumentNullException(nameof(contentType));
    }
}