namespace FS.RabbitMQ.Core.Abstractions;

/// <summary>
/// Defines the available backoff strategies for retry delay calculation.
/// </summary>
/// <remarks>
/// Backoff strategies determine how retry delays are calculated and provide
/// different characteristics for handling various types of failures and
/// system recovery patterns. The choice of strategy significantly impacts
/// retry behavior and system performance under failure conditions.
/// </remarks>
public sealed class BackoffStrategy
{
    private readonly Func<int, TimeSpan, TimeSpan> _delayCalculator;
    private readonly string _name;

    /// <summary>
    /// Initializes a new instance of the BackoffStrategy class.
    /// </summary>
    /// <param name="name">The descriptive name of the strategy.</param>
    /// <param name="delayCalculator">The function that calculates delays based on attempt number and base delay.</param>
    private BackoffStrategy(string name, Func<int, TimeSpan, TimeSpan> delayCalculator)
    {
        _name = name;
        _delayCalculator = delayCalculator;
    }

    /// <summary>
    /// Gets a backoff strategy that performs immediate retries with no delay.
    /// </summary>
    public static BackoffStrategy Immediate { get; } = new("Immediate", (_, _) => TimeSpan.Zero);

    /// <summary>
    /// Gets a backoff strategy that uses a fixed delay for all retry attempts.
    /// </summary>
    public static BackoffStrategy Linear { get; } = new("Linear", (_, baseDelay) => baseDelay);

    /// <summary>
    /// Gets a backoff strategy that doubles the delay with each retry attempt.
    /// </summary>
    public static BackoffStrategy Exponential { get; } = new("Exponential", 
        (attemptNumber, baseDelay) => TimeSpan.FromMilliseconds(baseDelay.TotalMilliseconds * Math.Pow(2, attemptNumber - 1)));

    /// <summary>
    /// Creates a polynomial backoff strategy with configurable growth power.
    /// </summary>
    /// <param name="power">The polynomial power for delay calculation.</param>
    /// <returns>A backoff strategy that uses polynomial delay growth.</returns>
    public static BackoffStrategy CreatePolynomial(double power) =>
        new($"Polynomial(power={power})", 
            (attemptNumber, baseDelay) => TimeSpan.FromMilliseconds(baseDelay.TotalMilliseconds * Math.Pow(attemptNumber, power)));

    /// <summary>
    /// Creates a custom backoff strategy with user-defined delay calculation.
    /// </summary>
    /// <param name="delayCalculator">The function that calculates delays based on attempt number.</param>
    /// <returns>A backoff strategy that uses the custom calculation function.</returns>
    public static BackoffStrategy CreateCustom(Func<int, TimeSpan> delayCalculator) =>
        new("Custom", (attemptNumber, _) => delayCalculator(attemptNumber));

    /// <summary>
    /// Calculates the delay for the specified attempt using this strategy.
    /// </summary>
    /// <param name="attemptNumber">The current attempt number (1-based).</param>
    /// <param name="baseDelay">The base delay for calculations.</param>
    /// <returns>The calculated delay duration.</returns>
    public TimeSpan CalculateDelay(int attemptNumber, TimeSpan baseDelay) =>
        _delayCalculator(attemptNumber, baseDelay);

    /// <summary>
    /// Returns a string representation of the backoff strategy.
    /// </summary>
    /// <returns>The name of the backoff strategy.</returns>
    public override string ToString() => _name;
}