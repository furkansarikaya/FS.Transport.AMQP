namespace FS.RabbitMQ.Core.Abstractions;

/// <summary>
/// Defines the types of jitter algorithms available for randomizing retry delays.
/// </summary>
public enum JitterType
{
    /// <summary>
    /// Full jitter randomly selects a delay between 0 and the calculated delay multiplied by jitter amount.
    /// Provides maximum randomization but may result in very short delays.
    /// </summary>
    Full,

    /// <summary>
    /// Equal jitter adds or subtracts a random amount up to the jitter percentage from the calculated delay.
    /// Provides balanced randomization while maintaining the general timing characteristics.
    /// </summary>
    Equal,

    /// <summary>
    /// Decorrelated jitter uses the previous delay as input for calculating the next jittered delay.
    /// Provides smooth jitter that prevents extreme variations between consecutive attempts.
    /// </summary>
    Decorrelated
}