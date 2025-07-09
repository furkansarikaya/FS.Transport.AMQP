namespace FS.Transport.AMQP.RetryPolicies;

/// <summary>
/// Event arguments for retry-related events
/// </summary>
public class RetryEventArgs(int attemptCount, Exception exception, TimeSpan delay, bool isFinalAttempt)
    : EventArgs
{
    /// <summary>
    /// Current attempt number
    /// </summary>
    public int AttemptCount { get; } = attemptCount;

    /// <summary>
    /// Exception that triggered the retry
    /// </summary>
    public Exception Exception { get; } = exception ?? throw new ArgumentNullException(nameof(exception));

    /// <summary>
    /// Delay before next retry
    /// </summary>
    public TimeSpan Delay { get; } = delay;

    /// <summary>
    /// Whether this is the final retry attempt
    /// </summary>
    public bool IsFinalAttempt { get; } = isFinalAttempt;

    /// <summary>
    /// Additional context information
    /// </summary>
    public IDictionary<string, object> Context { get; } = new Dictionary<string, object>();

    /// <summary>
    /// Adds context information to the retry event
    /// </summary>
    /// <param name="key">Context key</param>
    /// <param name="value">Context value</param>
    /// <returns>The event args instance for fluent configuration</returns>
    public RetryEventArgs WithContext(string key, object value)
    {
        Context[key] = value;
        return this;
    }
}
