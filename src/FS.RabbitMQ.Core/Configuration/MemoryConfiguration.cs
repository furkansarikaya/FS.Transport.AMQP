using FS.RabbitMQ.Core.Abstractions;

namespace FS.RabbitMQ.Core.Configuration;

/// <summary>
/// Contains configuration for memory management optimization.
/// </summary>
public sealed class MemoryConfiguration
{
    /// <summary>
    /// Gets or sets the buffer pool size for message handling.
    /// </summary>
    public int BufferPoolSize { get; set; } = 1024;

    /// <summary>
    /// Gets or sets the maximum message size in bytes.
    /// </summary>
    public long MaxMessageSize { get; set; } = 10 * 1024 * 1024; // 10MB

    /// <summary>
    /// Creates a default memory configuration.
    /// </summary>
    public static MemoryConfiguration CreateDefault() => new();

    /// <summary>
    /// Creates a high-throughput memory configuration.
    /// </summary>
    public static MemoryConfiguration CreateHighThroughput() => 
        new() { BufferPoolSize = 4096, MaxMessageSize = 50 * 1024 * 1024 };

    /// <summary>
    /// Validates the memory configuration.
    /// </summary>
    public void Validate()
    {
        if (BufferPoolSize <= 0)
            throw new BrokerConfigurationException("Buffer pool size must be positive.");
        if (MaxMessageSize <= 0)
            throw new BrokerConfigurationException("Maximum message size must be positive.");
    }
}