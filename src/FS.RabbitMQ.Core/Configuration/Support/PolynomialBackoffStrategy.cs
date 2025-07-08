namespace FS.RabbitMQ.Core.Configuration.Support;

/// <summary>
/// Polynomial backoff strategy implementation.
/// </summary>
internal sealed class PolynomialBackoffStrategy(double power) : BackoffStrategy
{
    public override TimeSpan CalculateDelay(int attemptNumber, TimeSpan baseDelay)
    {
        var multiplier = Math.Pow(attemptNumber, power);
        return TimeSpan.FromMilliseconds(baseDelay.TotalMilliseconds * multiplier);
    }

    public override string ToString() => $"polynomial(^{power})";
}