namespace FS.RabbitMQ.RetryPolicies;

/// <summary>
/// Interface for retry policies that determine how and when operations should be retried
/// </summary>
public interface IRetryPolicy
{
    /// <summary>
    /// Policy name for identification
    /// </summary>
    string Name { get; }
    
    /// <summary>
    /// Maximum number of retry attempts
    /// </summary>
    int MaxRetries { get; }
    
    /// <summary>
    /// Determines if an operation should be retried based on the exception and attempt count
    /// </summary>
    /// <param name="exception">Exception that occurred</param>
    /// <param name="attemptCount">Number of attempts made so far</param>
    /// <returns>True if operation should be retried</returns>
    bool ShouldRetry(Exception exception, int attemptCount);
    
    /// <summary>
    /// Calculates the delay before the next retry attempt
    /// </summary>
    /// <param name="attemptCount">Number of attempts made so far</param>
    /// <returns>Delay before next retry</returns>
    TimeSpan CalculateDelay(int attemptCount);
    
    /// <summary>
    /// Executes an operation with retry logic
    /// </summary>
    /// <typeparam name="T">Return type</typeparam>
    /// <param name="operation">Operation to execute</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Operation result</returns>
    Task<T> ExecuteAsync<T>(Func<Task<T>> operation, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Executes an operation with retry logic
    /// </summary>
    /// <param name="operation">Operation to execute</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Task representing the operation</returns>
    Task ExecuteAsync(Func<Task> operation, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Executes a synchronous operation with retry logic
    /// </summary>
    /// <typeparam name="T">Return type</typeparam>
    /// <param name="operation">Operation to execute</param>
    /// <returns>Operation result</returns>
    T Execute<T>(Func<T> operation);
    
    /// <summary>
    /// Executes a synchronous operation with retry logic
    /// </summary>
    /// <param name="operation">Operation to execute</param>
    void Execute(Action operation);
    
    /// <summary>
    /// Event raised when a retry is about to occur
    /// </summary>
    event EventHandler<RetryEventArgs>? Retrying;
    
    /// <summary>
    /// Event raised when maximum retries are exceeded
    /// </summary>
    event EventHandler<RetryEventArgs>? MaxRetriesExceeded;
}
