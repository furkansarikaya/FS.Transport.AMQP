using FS.RabbitMQ.Core.Abstractions;

namespace FS.RabbitMQ.Core.Configuration.Support;

/// <summary>
/// Configuration for jitter in retry delays.
/// </summary>
public sealed class JitterConfiguration
{
    /// <summary>
    /// Gets or sets the jitter type.
    /// </summary>
    public JitterType Type { get; set; } = JitterType.None;

    /// <summary>
    /// Gets or sets the jitter amount (0.0 to 1.0).
    /// </summary>
    public double Amount { get; set; } = 0.1;

    private static readonly Random _random = new();

    /// <summary>
    /// Applies jitter to the specified delay.
    /// </summary>
    /// <param name="delay">The base delay to apply jitter to.</param>
    /// <returns>The delay with applied jitter.</returns>
    public TimeSpan ApplyJitter(TimeSpan delay)
    {
        return Type switch
        {
            JitterType.None => delay,
            JitterType.Full => TimeSpan.FromMilliseconds(delay.TotalMilliseconds * _random.NextDouble()),
            JitterType.Equal => TimeSpan.FromMilliseconds(delay.TotalMilliseconds * (0.5 + _random.NextDouble() * 0.5)),
            JitterType.Decorrelated => ApplyDecorrelatedJitter(delay),
            _ => delay
        };
    }

    private TimeSpan ApplyDecorrelatedJitter(TimeSpan delay)
    {
        var jitterMs = _random.NextDouble() * Amount * delay.TotalMilliseconds;
        return TimeSpan.FromMilliseconds(delay.TotalMilliseconds + jitterMs);
    }

    /// <summary>
    /// Returns a string representation of the jitter configuration.
    /// </summary>
    public override string ToString() => $"{Type}({Amount:P0})";
}