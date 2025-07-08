namespace FS.RabbitMQ.Core.Configuration.Support;

/// <summary>
/// Immediate backoff strategy implementation.
/// </summary>
internal sealed class ImmediateBackoffStrategy : BackoffStrategy
{
    public override TimeSpan CalculateDelay(int attemptNumber, TimeSpan baseDelay) => TimeSpan.Zero;
    public override string ToString() => "immediate";
}