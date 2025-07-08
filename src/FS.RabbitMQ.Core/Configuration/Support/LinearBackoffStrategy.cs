namespace FS.RabbitMQ.Core.Configuration.Support;

/// <summary>
/// Linear backoff strategy implementation.
/// </summary>
internal sealed class LinearBackoffStrategy : BackoffStrategy
{
    public override TimeSpan CalculateDelay(int attemptNumber, TimeSpan baseDelay) => baseDelay;
    public override string ToString() => "linear";
}