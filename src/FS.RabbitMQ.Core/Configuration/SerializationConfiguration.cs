namespace FS.RabbitMQ.Core.Configuration;

/// <summary>
/// Contains configuration for message serialization and deserialization.
/// </summary>
public sealed class SerializationConfiguration
{
    /// <summary>
    /// Gets or sets the default serialization format.
    /// </summary>
    /// <value>The default format for message serialization. Default is Json.</value>
    public SerializationFormat DefaultFormat { get; set; } = SerializationFormat.Json;

    /// <summary>
    /// Gets or sets a value indicating whether compression is enabled.
    /// </summary>
    /// <value><c>true</c> to enable message compression; otherwise, <c>false</c>. Default is <c>false</c>.</value>
    public bool EnableCompression { get; set; } = false;

    /// <summary>
    /// Creates a default serialization configuration.
    /// </summary>
    public static SerializationConfiguration CreateDefault() => new();

    /// <summary>
    /// Creates a production-optimized serialization configuration.
    /// </summary>
    public static SerializationConfiguration CreateProduction() => new() { EnableCompression = true };

    /// <summary>
    /// Creates a high-performance serialization configuration.
    /// </summary>
    public static SerializationConfiguration CreateHighPerformance() => 
        new() { DefaultFormat = SerializationFormat.MessagePack, EnableCompression = false };

    /// <summary>
    /// Validates the serialization configuration.
    /// </summary>
    public void Validate() { }
}