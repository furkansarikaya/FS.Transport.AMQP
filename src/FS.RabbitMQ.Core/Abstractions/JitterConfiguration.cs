namespace FS.RabbitMQ.Core.Abstractions;

/// <summary>
/// Contains configuration for adding randomness to retry delays to prevent thundering herd problems.
/// </summary>
/// <remarks>
/// Jitter configuration helps distribute retry attempts across time to prevent
/// synchronized retry patterns that can overwhelm recovering systems. This is
/// particularly important in distributed systems where many clients may
/// experience failures simultaneously and attempt retry at the same time.
/// </remarks>
public sealed record JitterConfiguration
{
    /// <summary>
    /// Gets the type of jitter to apply to calculated delays.
    /// </summary>
    /// <value>The jitter algorithm used for randomizing delays.</value>
    public JitterType Type { get; init; }

    /// <summary>
    /// Gets the amount of jitter to apply as a percentage of the calculated delay.
    /// </summary>
    /// <value>The jitter amount as a percentage (0.0 to 1.0), where 0.5 means ±50% of the calculated delay.</value>
    public double Amount { get; init; } = 0.1; // Default 10% jitter

    /// <summary>
    /// Creates a jitter configuration with the specified type and amount.
    /// </summary>
    /// <param name="type">The type of jitter to apply.</param>
    /// <param name="amount">The jitter amount as a percentage (0.0 to 1.0).</param>
    /// <returns>A jitter configuration with the specified settings.</returns>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when <paramref name="amount"/> is outside the valid range.</exception>
    public static JitterConfiguration Create(JitterType type, double amount = 0.1)
    {
        if (amount < 0.0 || amount > 1.0)
            throw new ArgumentOutOfRangeException(nameof(amount), "Jitter amount must be between 0.0 and 1.0.");

        return new JitterConfiguration { Type = type, Amount = amount };
    }

    /// <summary>
    /// Applies jitter to the specified delay using the configured jitter algorithm.
    /// </summary>
    /// <param name="delay">The calculated delay to apply jitter to.</param>
    /// <returns>The delay with jitter applied.</returns>
    public TimeSpan ApplyJitter(TimeSpan delay)
    {
        if (Amount == 0.0 || delay == TimeSpan.Zero)
            return delay;

        var random = Random.Shared;
        var jitterAmount = delay.TotalMilliseconds * Amount;

        var jitteredMilliseconds = Type switch
        {
            JitterType.Full => random.NextDouble() * delay.TotalMilliseconds * 2 * Amount,
            JitterType.Equal => delay.TotalMilliseconds + (random.NextDouble() - 0.5) * 2 * jitterAmount,
            JitterType.Decorrelated => Math.Max(0, random.NextDouble() * (delay.TotalMilliseconds + jitterAmount)),
            _ => delay.TotalMilliseconds
        };

        return TimeSpan.FromMilliseconds(Math.Max(0, jitteredMilliseconds));
    }

    /// <summary>
    /// Returns a string representation of the jitter configuration.
    /// </summary>
    /// <returns>A formatted string describing the jitter settings.</returns>
    public override string ToString() => $"{Type} jitter ({Amount:P0})";
}