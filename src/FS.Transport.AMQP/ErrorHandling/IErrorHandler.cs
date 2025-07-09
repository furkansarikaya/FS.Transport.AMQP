using FS.Transport.AMQP.RetryPolicies;

namespace FS.Transport.AMQP.ErrorHandling;

/// <summary>
/// Interface for handling errors during message processing with comprehensive strategies and retry logic
/// </summary>
public interface IErrorHandler
{
    /// <summary>
    /// Error handling strategy used by this handler
    /// </summary>
    ErrorHandlingStrategy Strategy { get; }
    
    /// <summary>
    /// Retry policy used for error recovery
    /// </summary>
    IRetryPolicy RetryPolicy { get; }
    
    /// <summary>
    /// Handles an error that occurred during message processing
    /// </summary>
    /// <param name="context">Error context containing all relevant information</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Result indicating how the error was handled</returns>
    Task<ErrorHandlingResult> HandleErrorAsync(ErrorContext context, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Handles an error synchronously
    /// </summary>
    /// <param name="context">Error context containing all relevant information</param>
    /// <returns>Result indicating how the error was handled</returns>
    ErrorHandlingResult HandleError(ErrorContext context);
    
    /// <summary>
    /// Determines if an error can be handled by this handler
    /// </summary>
    /// <param name="exception">Exception to check</param>
    /// <param name="context">Error context</param>
    /// <returns>True if the error can be handled</returns>
    bool CanHandle(Exception exception, ErrorContext context);
    
    /// <summary>
    /// Gets error handling statistics
    /// </summary>
    /// <returns>Error handling statistics</returns>
    ErrorHandlingStatistics GetStatistics();
    
    /// <summary>
    /// Event raised when an error is being handled
    /// </summary>
    event EventHandler<ErrorHandlingEventArgs>? ErrorHandling;
    
    /// <summary>
    /// Event raised when an error has been handled
    /// </summary>
    event EventHandler<ErrorHandlingEventArgs>? ErrorHandled;
    
    /// <summary>
    /// Event raised when an error cannot be handled and is being sent to dead letter
    /// </summary>
    event EventHandler<ErrorHandlingEventArgs>? ErrorSentToDeadLetter;
}