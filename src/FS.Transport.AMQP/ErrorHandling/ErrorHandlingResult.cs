namespace FS.Transport.AMQP.ErrorHandling;

/// <summary>
/// Result of error handling operation
/// </summary>
public class ErrorHandlingResult
{
    /// <summary>
    /// Whether the error was successfully handled
    /// </summary>
    public bool IsSuccess { get; set; }
    
    /// <summary>
    /// Action taken to handle the error
    /// </summary>
    public ErrorHandlingAction Action { get; set; }
    
    /// <summary>
    /// Additional information about the handling result
    /// </summary>
    public string? Message { get; set; }
    
    /// <summary>
    /// Whether the original message should be acknowledged
    /// </summary>
    public bool ShouldAcknowledge { get; set; }
    
    /// <summary>
    /// Whether the original message should be requeued
    /// </summary>
    public bool ShouldRequeue { get; set; }
    
    /// <summary>
    /// Whether the operation should be retried
    /// </summary>
    public bool ShouldRetry { get; set; }
    
    /// <summary>
    /// Delay before retry (if applicable)
    /// </summary>
    public TimeSpan? RetryDelay { get; set; }
    
    /// <summary>
    /// Additional context about the result
    /// </summary>
    public IDictionary<string, object> Context { get; set; } = new Dictionary<string, object>();

    /// <summary>
    /// Creates a successful result
    /// </summary>
    /// <param name="action">Action taken</param>
    /// <param name="message">Result message</param>
    /// <returns>Success result</returns>
    public static ErrorHandlingResult Success(ErrorHandlingAction action, string? message = null)
    {
        return new ErrorHandlingResult
        {
            IsSuccess = true,
            Action = action,
            Message = message,
            ShouldAcknowledge = action == ErrorHandlingAction.Acknowledged || action == ErrorHandlingAction.SentToDeadLetter
        };
    }

    /// <summary>
    /// Creates a failure result
    /// </summary>
    /// <param name="message">Failure message</param>
    /// <returns>Failure result</returns>
    public static ErrorHandlingResult Failure(string message)
    {
        return new ErrorHandlingResult
        {
            IsSuccess = false,
            Action = ErrorHandlingAction.Failed,
            Message = message,
            ShouldAcknowledge = false
        };
    }

    /// <summary>
    /// Creates a retry result
    /// </summary>
    /// <param name="delay">Retry delay</param>
    /// <param name="message">Retry message</param>
    /// <returns>Retry result</returns>
    public static ErrorHandlingResult Retry(TimeSpan delay, string? message = null)
    {
        return new ErrorHandlingResult
        {
            IsSuccess = false,
            Action = ErrorHandlingAction.Retried,
            Message = message,
            ShouldRetry = true,
            RetryDelay = delay,
            ShouldAcknowledge = false
        };
    }

    /// <summary>
    /// Creates a requeue result
    /// </summary>
    /// <param name="message">Requeue message</param>
    /// <returns>Requeue result</returns>
    public static ErrorHandlingResult Requeue(string? message = null)
    {
        return new ErrorHandlingResult
        {
            IsSuccess = true,
            Action = ErrorHandlingAction.Requeued,
            Message = message,
            ShouldRequeue = true,
            ShouldAcknowledge = false
        };
    }
}