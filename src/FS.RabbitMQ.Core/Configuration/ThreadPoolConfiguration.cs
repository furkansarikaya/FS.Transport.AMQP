using FS.RabbitMQ.Core.Abstractions;

namespace FS.RabbitMQ.Core.Configuration;

/// <summary>
/// Contains configuration for thread pool management.
/// </summary>
public sealed class ThreadPoolConfiguration
{
    /// <summary>
    /// Gets or sets the minimum number of worker threads.
    /// </summary>
    public int MinWorkerThreads { get; set; } = Environment.ProcessorCount;

    /// <summary>
    /// Gets or sets the maximum number of worker threads.
    /// </summary>
    public int MaxWorkerThreads { get; set; } = Environment.ProcessorCount * 4;

    /// <summary>
    /// Creates a default thread pool configuration.
    /// </summary>
    public static ThreadPoolConfiguration CreateDefault() => new();

    /// <summary>
    /// Creates a high-throughput thread pool configuration.
    /// </summary>
    public static ThreadPoolConfiguration CreateHighThroughput() => 
        new() { MaxWorkerThreads = Environment.ProcessorCount * 8 };

    /// <summary>
    /// Validates the thread pool configuration.
    /// </summary>
    public void Validate()
    {
        if (MinWorkerThreads <= 0)
            throw new BrokerConfigurationException("Minimum worker threads must be positive.");
        if (MaxWorkerThreads < MinWorkerThreads)
            throw new BrokerConfigurationException("Maximum worker threads must be >= minimum worker threads.");
    }
}