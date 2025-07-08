namespace FS.RabbitMQ.Core.Configuration.Support;

/// <summary>
/// Exponential backoff strategy implementation.
/// </summary>
internal sealed class ExponentialBackoffStrategy : BackoffStrategy
{
    public override TimeSpan CalculateDelay(int attemptNumber, TimeSpan baseDelay)
    {
        var multiplier = Math.Pow(2, attemptNumber - 1);
        return TimeSpan.FromMilliseconds(baseDelay.TotalMilliseconds * multiplier);
    }
    public override string ToString() => "exponential";
}