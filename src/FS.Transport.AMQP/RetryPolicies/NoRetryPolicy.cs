namespace FS.Transport.AMQP.RetryPolicies;

/// <summary>
/// No-retry policy implementation that never retries
/// </summary>
public class NoRetryPolicy : IRetryPolicy
{
    public string Name => "None";
    public int MaxRetries => 0;

    public event EventHandler<RetryEventArgs>? Retrying;
    public event EventHandler<RetryEventArgs>? MaxRetriesExceeded;

    public bool ShouldRetry(Exception exception, int attemptCount) => false;

    public TimeSpan CalculateDelay(int attemptCount) => TimeSpan.Zero;

    public async Task<T> ExecuteAsync<T>(Func<Task<T>> operation, CancellationToken cancellationToken = default)
    {
        if (operation == null)
            throw new ArgumentNullException(nameof(operation));

        return await operation();
    }

    public async Task ExecuteAsync(Func<Task> operation, CancellationToken cancellationToken = default)
    {
        if (operation == null)
            throw new ArgumentNullException(nameof(operation));

        await operation();
    }

    public T Execute<T>(Func<T> operation)
    {
        if (operation == null)
            throw new ArgumentNullException(nameof(operation));

        return operation();
    }

    public void Execute(Action operation)
    {
        if (operation == null)
            throw new ArgumentNullException(nameof(operation));

        operation();
    }
}