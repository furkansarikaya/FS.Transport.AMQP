namespace FS.RabbitMQ.Core.Abstractions;

/// <summary>
/// Contains configuration options for message serialization and deserialization.
/// </summary>
/// <remarks>
/// Serialization options control how messages are converted between their
/// object representation and wire format, affecting performance, compatibility,
/// and interoperability across different systems and technology stacks.
/// </remarks>
public sealed class SerializationOptions
{
    /// <summary>
    /// Gets or sets the default content type for serialized messages.
    /// </summary>
    /// <value>The MIME type used for message serialization. Default is "application/json".</value>
    public string ContentType { get; set; } = "application/json";

    /// <summary>
    /// Gets or sets the content encoding for compressed or encoded messages.
    /// </summary>
    /// <value>The encoding format applied to serialized messages, or null for no encoding.</value>
    public string? ContentEncoding { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether to use compression for message payloads.
    /// </summary>
    /// <value><c>true</c> to compress message payloads; otherwise, <c>false</c>. Default is <c>false</c>.</value>
    public bool UseCompression { get; set; } = false;

    /// <summary>
    /// Gets or sets the compression threshold for automatic compression.
    /// </summary>
    /// <value>The minimum message size in bytes that triggers compression. Default is 1024 bytes.</value>
    public int CompressionThreshold { get; set; } = 1024;

    /// <summary>
    /// Gets or sets a value indicating whether to include type information in serialized messages.
    /// </summary>
    /// <value><c>true</c> to include type metadata; otherwise, <c>false</c>. Default is <c>true</c>.</value>
    public bool IncludeTypeInformation { get; set; } = true;

    /// <summary>
    /// Gets or sets the custom serializer to use for message serialization.
    /// </summary>
    /// <value>A custom serializer implementation, or null to use the default serializer.</value>
    public IMessageSerializer? CustomSerializer { get; set; }

    /// <summary>
    /// Gets or sets serializer-specific settings and configuration.
    /// </summary>
    /// <value>A dictionary containing serializer-specific configuration options.</value>
    public IDictionary<string, object>? SerializerSettings { get; set; }

    /// <summary>
    /// Creates serialization options optimized for JSON messaging.
    /// </summary>
    /// <param name="useCompression">Whether to enable compression for large messages.</param>
    /// <returns>Serialization options configured for JSON messaging.</returns>
    public static SerializationOptions CreateJson(bool useCompression = false) =>
        new()
        {
            ContentType = "application/json",
            UseCompression = useCompression,
            IncludeTypeInformation = true
        };

    /// <summary>
    /// Creates serialization options optimized for high-performance binary messaging.
    /// </summary>
    /// <param name="useCompression">Whether to enable compression for large messages.</param>
    /// <returns>Serialization options configured for binary messaging.</returns>
    public static SerializationOptions CreateBinary(bool useCompression = true) =>
        new()
        {
            ContentType = "application/octet-stream",
            UseCompression = useCompression,
            CompressionThreshold = 512,
            IncludeTypeInformation = true
        };
}