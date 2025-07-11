namespace FS.RabbitMQ.Core;

/// <summary>
/// Compression algorithms supported by the transport
/// </summary>
public enum CompressionAlgorithm
{
    /// <summary>
    /// No compression
    /// </summary>
    None,
    
    /// <summary>
    /// GZip compression
    /// </summary>
    GZip,
    
    /// <summary>
    /// LZ4 compression (fast)
    /// </summary>
    LZ4,
    
    /// <summary>
    /// Deflate compression
    /// </summary>
    Deflate,
    
    /// <summary>
    /// Brotli compression
    /// </summary>
    Brotli
} 