namespace FS.Transport.AMQP.EventHandlers;

/// <summary>
/// Result of event handler execution
/// </summary>
public class EventHandlingResult
{
    /// <summary>
    /// Whether the handler execution was successful
    /// </summary>
    public bool IsSuccess { get; }
    
    /// <summary>
    /// Name of the handler that was executed
    /// </summary>
    public string HandlerName { get; }
    
    /// <summary>
    /// Exception that occurred (if any)
    /// </summary>
    public Exception? Exception { get; }
    
    /// <summary>
    /// Execution duration
    /// </summary>
    public TimeSpan Duration { get; }
    
    /// <summary>
    /// Additional result context
    /// </summary>
    public IDictionary<string, object> Context { get; }

    private EventHandlingResult(bool success, string handlerName, Exception? exception, TimeSpan duration)
    {
        IsSuccess = success;
        HandlerName = handlerName ?? throw new ArgumentNullException(nameof(handlerName));
        Exception = exception;
        Duration = duration;
        Context = new Dictionary<string, object>();
    }

    /// <summary>
    /// Creates a successful result
    /// </summary>
    /// <param name="handlerName">Handler name</param>
    /// <param name="duration">Execution duration</param>
    /// <returns>Success result</returns>
    public static EventHandlingResult Success(string handlerName, TimeSpan duration)
    {
        return new EventHandlingResult(true, handlerName, null, duration);
    }

    /// <summary>
    /// Creates a failure result
    /// </summary>
    /// <param name="handlerName">Handler name</param>
    /// <param name="exception">Exception that occurred</param>
    /// <param name="duration">Execution duration</param>
    /// <returns>Failure result</returns>
    public static EventHandlingResult Failure(string handlerName, Exception exception, TimeSpan duration)
    {
        return new EventHandlingResult(false, handlerName, exception, duration);
    }

    /// <summary>
    /// Adds context information
    /// </summary>
    /// <param name="key">Context key</param>
    /// <param name="value">Context value</param>
    /// <returns>Result for fluent configuration</returns>
    public EventHandlingResult WithContext(string key, object value)
    {
        Context[key] = value;
        return this;
    }

    /// <summary>
    /// Gets a string representation of the result
    /// </summary>
    /// <returns>Result description</returns>
    public override string ToString()
    {
        var status = IsSuccess ? "Success" : "Failed";
        var error = Exception != null ? $" - {Exception.Message}" : "";
        return $"Handler {HandlerName}: {status} in {Duration.TotalMilliseconds:F2}ms{error}";
    }
}