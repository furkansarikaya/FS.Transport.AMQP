namespace FS.Transport.AMQP.RetryPolicies;

/// <summary>
/// No-retry policy implementation that never retries failed operations
/// </summary>
public class NoRetryPolicy : IRetryPolicy
{
    public string Name => "NoRetry";
    public int MaxRetries => 0;

#pragma warning disable CS0067 // Event is never used - by design for NoRetryPolicy
    public event EventHandler<RetryEventArgs>? Retrying;
    public event EventHandler<RetryEventArgs>? MaxRetriesExceeded;
#pragma warning restore CS0067

    public bool ShouldRetry(Exception exception, int attemptCount) => false;

    public TimeSpan CalculateDelay(int attemptCount) => TimeSpan.Zero;

    public async Task<T> ExecuteAsync<T>(Func<Task<T>> operation, CancellationToken cancellationToken = default)
    {
        try
        {
            return await operation().ConfigureAwait(false);
        }
        catch (Exception)
        {
            throw; // No retry, just rethrow
        }
    }

    public async Task ExecuteAsync(Func<Task> operation, CancellationToken cancellationToken = default)
    {
        try
        {
            await operation().ConfigureAwait(false);
        }
        catch (Exception)
        {
            throw; // No retry, just rethrow
        }
    }

    public T Execute<T>(Func<T> operation)
    {
        try
        {
            return operation();
        }
        catch (Exception)
        {
            throw; // No retry, just rethrow
        }
    }

    public void Execute(Action operation)
    {
        try
        {
            operation();
        }
        catch (Exception)
        {
            throw; // No retry, just rethrow
        }
    }
}