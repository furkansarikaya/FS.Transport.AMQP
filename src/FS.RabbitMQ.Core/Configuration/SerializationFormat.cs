namespace FS.RabbitMQ.Core.Configuration;

/// <summary>
/// Defines supported serialization formats.
/// </summary>
public enum SerializationFormat
{
    /// <summary>
    /// JSON serialization format.
    /// </summary>
    Json,

    /// <summary>
    /// MessagePack binary serialization format.
    /// </summary>
    MessagePack,

    /// <summary>
    /// Protocol Buffers serialization format.
    /// </summary>
    ProtocolBuffers,

    /// <summary>
    /// Custom serialization format.
    /// </summary>
    Custom
}