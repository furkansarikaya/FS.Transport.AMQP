namespace FS.RabbitMQ.Core.Configuration.Support;

/// <summary>
/// Custom backoff strategy implementation.
/// </summary>
internal sealed class CustomBackoffStrategy(Func<int, TimeSpan> calculator) : BackoffStrategy
{
    private readonly Func<int, TimeSpan> _calculator = calculator ?? throw new ArgumentNullException(nameof(calculator));

    public override TimeSpan CalculateDelay(int attemptNumber, TimeSpan baseDelay) => _calculator(attemptNumber);
    public override string ToString() => "custom";
}