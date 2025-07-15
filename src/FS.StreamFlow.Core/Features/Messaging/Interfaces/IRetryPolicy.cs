using FS.StreamFlow.Core.Features.Messaging.Models;

namespace FS.StreamFlow.Core.Features.Messaging.Interfaces;

/// <summary>
/// Interface for retry policy providing configurable retry strategies with exponential backoff, jitter, and circuit breaker patterns
/// </summary>
public interface IRetryPolicy
{
    /// <summary>
    /// Gets the retry policy name
    /// </summary>
    string Name { get; }
    
    /// <summary>
    /// Gets the maximum number of retry attempts
    /// </summary>
    int MaxRetryAttempts { get; }
    
    /// <summary>
    /// Gets the retry policy settings
    /// </summary>
    RetryPolicySettings Settings { get; }
    
    /// <summary>
    /// Gets the current retry attempt count
    /// </summary>
    int CurrentAttempt { get; }
    
    /// <summary>
    /// Determines if a retry should be performed based on the exception and attempt count
    /// </summary>
    /// <param name="exception">Exception that occurred</param>
    /// <param name="attemptNumber">Current attempt number</param>
    /// <returns>True if retry should be performed, otherwise false</returns>
    bool ShouldRetry(Exception exception, int attemptNumber);
    
    /// <summary>
    /// Calculates the delay before the next retry attempt
    /// </summary>
    /// <param name="attemptNumber">Current attempt number</param>
    /// <returns>Delay before next retry</returns>
    TimeSpan CalculateDelay(int attemptNumber);
    
    /// <summary>
    /// Executes an action with retry logic
    /// </summary>
    /// <param name="action">Action to execute</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Task representing the operation</returns>
    Task ExecuteAsync(Func<Task> action, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Executes an action with retry logic and returns a result
    /// </summary>
    /// <typeparam name="T">Result type</typeparam>
    /// <param name="action">Action to execute</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Task with result</returns>
    Task<T> ExecuteAsync<T>(Func<Task<T>> action, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Resets the retry policy state
    /// </summary>
    void Reset();
    
    /// <summary>
    /// Event raised when a retry attempt is made
    /// </summary>
    event EventHandler<RetryAttemptEventArgs>? RetryAttempt;
    
    /// <summary>
    /// Event raised when all retry attempts are exhausted
    /// </summary>
    event EventHandler<RetryExhaustedEventArgs>? RetryExhausted;
}

/// <summary>
/// Factory interface for creating retry policies
/// </summary>
public interface IRetryPolicyFactory
{
    /// <summary>
    /// Creates a retry policy with the specified settings
    /// </summary>
    /// <param name="settings">Retry policy settings</param>
    /// <returns>Retry policy instance</returns>
    IRetryPolicy CreateRetryPolicy(RetryPolicySettings settings);
    
    /// <summary>
    /// Creates a retry policy with the specified name and settings
    /// </summary>
    /// <param name="name">Policy name</param>
    /// <param name="settings">Retry policy settings</param>
    /// <returns>Retry policy instance</returns>
    IRetryPolicy CreateRetryPolicy(string name, RetryPolicySettings settings);
    
    /// <summary>
    /// Creates an exponential backoff retry policy
    /// </summary>
    /// <param name="maxRetryAttempts">Maximum retry attempts</param>
    /// <param name="initialDelay">Initial delay</param>
    /// <param name="maxDelay">Maximum delay</param>
    /// <param name="multiplier">Delay multiplier</param>
    /// <param name="useJitter">Whether to use jitter</param>
    /// <returns>Retry policy instance</returns>
    IRetryPolicy CreateExponentialBackoffPolicy(
        int maxRetryAttempts = 3,
        TimeSpan? initialDelay = null,
        TimeSpan? maxDelay = null,
        double multiplier = 2.0,
        bool useJitter = true);
    
    /// <summary>
    /// Creates a linear retry policy
    /// </summary>
    /// <param name="maxRetryAttempts">Maximum retry attempts</param>
    /// <param name="delay">Fixed delay between retries</param>
    /// <returns>Retry policy instance</returns>
    IRetryPolicy CreateLinearPolicy(int maxRetryAttempts = 3, TimeSpan? delay = null);
    
    /// <summary>
    /// Creates a circuit breaker retry policy
    /// </summary>
    /// <param name="maxRetryAttempts">Maximum retry attempts</param>
    /// <param name="circuitBreakerThreshold">Circuit breaker threshold</param>
    /// <param name="circuitBreakerTimeout">Circuit breaker timeout</param>
    /// <returns>Retry policy instance</returns>
    IRetryPolicy CreateCircuitBreakerPolicy(
        int maxRetryAttempts = 3,
        int circuitBreakerThreshold = 5,
        TimeSpan? circuitBreakerTimeout = null);
    
    /// <summary>
    /// Creates a no-retry policy
    /// </summary>
    /// <returns>Retry policy instance</returns>
    IRetryPolicy CreateNoRetryPolicy();
    
    /// <summary>
    /// Gets a registered retry policy by name
    /// </summary>
    /// <param name="name">Policy name</param>
    /// <returns>Retry policy instance or null if not found</returns>
    IRetryPolicy? GetRetryPolicy(string name);
    
    /// <summary>
    /// Registers a retry policy
    /// </summary>
    /// <param name="name">Policy name</param>
    /// <param name="policy">Policy instance</param>
    void RegisterRetryPolicy(string name, IRetryPolicy policy);
    
    /// <summary>
    /// Unregisters a retry policy
    /// </summary>
    /// <param name="name">Policy name</param>
    /// <returns>True if policy was removed, otherwise false</returns>
    bool UnregisterRetryPolicy(string name);
}

/// <summary>
/// Event arguments for retry attempts
/// </summary>
public class RetryAttemptEventArgs : EventArgs
{
    /// <summary>
    /// Current attempt number
    /// </summary>
    public int AttemptNumber { get; }
    
    /// <summary>
    /// Exception that caused the retry
    /// </summary>
    public Exception Exception { get; }
    
    /// <summary>
    /// Delay before next retry
    /// </summary>
    public TimeSpan Delay { get; }
    
    /// <summary>
    /// Policy name
    /// </summary>
    public string PolicyName { get; }
    
    /// <summary>
    /// Timestamp of the attempt
    /// </summary>
    public DateTimeOffset Timestamp { get; }
    
    /// <summary>
    /// Initializes a new instance of the RetryAttemptEventArgs class
    /// </summary>
    /// <param name="attemptNumber">Attempt number</param>
    /// <param name="exception">Exception</param>
    /// <param name="delay">Delay</param>
    /// <param name="policyName">Policy name</param>
    public RetryAttemptEventArgs(int attemptNumber, Exception exception, TimeSpan delay, string policyName)
    {
        AttemptNumber = attemptNumber;
        Exception = exception;
        Delay = delay;
        PolicyName = policyName;
        Timestamp = DateTimeOffset.UtcNow;
    }
}

/// <summary>
/// Event arguments for retry exhaustion
/// </summary>
public class RetryExhaustedEventArgs : EventArgs
{
    /// <summary>
    /// Final attempt number
    /// </summary>
    public int FinalAttemptNumber { get; }
    
    /// <summary>
    /// Final exception
    /// </summary>
    public Exception FinalException { get; }
    
    /// <summary>
    /// Policy name
    /// </summary>
    public string PolicyName { get; }
    
    /// <summary>
    /// Timestamp when retries were exhausted
    /// </summary>
    public DateTimeOffset Timestamp { get; }
    
    /// <summary>
    /// Total time spent retrying
    /// </summary>
    public TimeSpan TotalRetryTime { get; }
    
    /// <summary>
    /// Initializes a new instance of the RetryExhaustedEventArgs class
    /// </summary>
    /// <param name="finalAttemptNumber">Final attempt number</param>
    /// <param name="finalException">Final exception</param>
    /// <param name="policyName">Policy name</param>
    /// <param name="totalRetryTime">Total retry time</param>
    public RetryExhaustedEventArgs(int finalAttemptNumber, Exception finalException, string policyName, TimeSpan totalRetryTime)
    {
        FinalAttemptNumber = finalAttemptNumber;
        FinalException = finalException;
        PolicyName = policyName;
        TotalRetryTime = totalRetryTime;
        Timestamp = DateTimeOffset.UtcNow;
    }
} 