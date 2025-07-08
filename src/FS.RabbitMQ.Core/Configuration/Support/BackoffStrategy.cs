namespace FS.RabbitMQ.Core.Configuration.Support;

/// <summary>
/// Defines backoff strategies for retry operations.
/// </summary>
public abstract class BackoffStrategy
{
    /// <summary>
    /// Calculates the delay for a specific attempt number.
    /// </summary>
    /// <param name="attemptNumber">The attempt number (1-based).</param>
    /// <param name="baseDelay">The base delay for calculations.</param>
    /// <returns>The calculated delay for the attempt.</returns>
    public abstract TimeSpan CalculateDelay(int attemptNumber, TimeSpan baseDelay);

    /// <summary>
    /// Creates an immediate backoff strategy (no delay).
    /// </summary>
    public static BackoffStrategy Immediate => new ImmediateBackoffStrategy();

    /// <summary>
    /// Creates a linear backoff strategy (fixed delay).
    /// </summary>
    public static BackoffStrategy Linear => new LinearBackoffStrategy();

    /// <summary>
    /// Creates an exponential backoff strategy.
    /// </summary>
    public static BackoffStrategy Exponential => new ExponentialBackoffStrategy();

    /// <summary>
    /// Creates a polynomial backoff strategy with specified power.
    /// </summary>
    /// <param name="power">The polynomial power for delay calculation.</param>
    public static BackoffStrategy CreatePolynomial(double power) => new PolynomialBackoffStrategy(power);

    /// <summary>
    /// Creates a custom backoff strategy with specified calculation function.
    /// </summary>
    /// <param name="calculator">The function to calculate delays.</param>
    public static BackoffStrategy CreateCustom(Func<int, TimeSpan> calculator) => new CustomBackoffStrategy(calculator);

    /// <summary>
    /// Returns a string representation of the backoff strategy.
    /// </summary>
    public override abstract string ToString();
}