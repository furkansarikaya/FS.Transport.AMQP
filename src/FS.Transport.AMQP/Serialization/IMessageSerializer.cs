namespace FS.Transport.AMQP.Serialization;

/// <summary>
/// Interface for message serialization and deserialization
/// </summary>
public interface IMessageSerializer
{
    /// <summary>
    /// Content type this serializer handles
    /// </summary>
    string ContentType { get; }
    
    /// <summary>
    /// Serializes an object to byte array
    /// </summary>
    /// <typeparam name="T">Object type</typeparam>
    /// <param name="obj">Object to serialize</param>
    /// <returns>Serialized byte array</returns>
    byte[] Serialize<T>(T obj) where T : class;
    
    /// <summary>
    /// Serializes an object to byte array asynchronously
    /// </summary>
    /// <typeparam name="T">Object type</typeparam>
    /// <param name="obj">Object to serialize</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Serialized byte array</returns>
    Task<byte[]> SerializeAsync<T>(T obj, CancellationToken cancellationToken = default) where T : class;
    
    /// <summary>
    /// Deserializes byte array to specified type
    /// </summary>
    /// <typeparam name="T">Target type</typeparam>
    /// <param name="data">Serialized data</param>
    /// <returns>Deserialized object</returns>
    T Deserialize<T>(byte[] data) where T : class;
    
    /// <summary>
    /// Deserializes byte array to specified type
    /// </summary>
    /// <param name="data">Serialized data</param>
    /// <param name="type">Target type</param>
    /// <returns>Deserialized object</returns>
    object Deserialize(byte[] data, Type type);
    
    /// <summary>
    /// Deserializes byte array to specified type asynchronously
    /// </summary>
    /// <typeparam name="T">Target type</typeparam>
    /// <param name="data">Serialized data</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Deserialized object</returns>
    Task<T> DeserializeAsync<T>(byte[] data, CancellationToken cancellationToken = default) where T : class;
    
    /// <summary>
    /// Deserializes byte array to specified type asynchronously
    /// </summary>
    /// <param name="data">Serialized data</param>
    /// <param name="type">Target type</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Deserialized object</returns>
    Task<object> DeserializeAsync(byte[] data, Type type, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Checks if this serializer can handle the specified content type
    /// </summary>
    /// <param name="contentType">Content type to check</param>
    /// <returns>True if can handle, false otherwise</returns>
    bool CanHandle(string contentType);
    
    /// <summary>
    /// Gets estimated serialized size for an object (for optimization)
    /// </summary>
    /// <typeparam name="T">Object type</typeparam>
    /// <param name="obj">Object to estimate</param>
    /// <returns>Estimated size in bytes</returns>
    int GetEstimatedSize<T>(T obj) where T : class;
}