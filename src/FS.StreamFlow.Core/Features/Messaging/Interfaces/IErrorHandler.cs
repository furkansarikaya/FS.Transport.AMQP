using FS.StreamFlow.Core.Features.Messaging.Models;

namespace FS.StreamFlow.Core.Features.Messaging.Interfaces;

/// <summary>
/// Interface for error handling providing comprehensive error management, dead letter queue support, and retry policies
/// </summary>
public interface IErrorHandler
{
    /// <summary>
    /// Gets the error handler name
    /// </summary>
    string Name { get; }
    
    /// <summary>
    /// Gets the error handling settings
    /// </summary>
    ErrorHandlingSettings Settings { get; }
    
    /// <summary>
    /// Handles an error that occurred during message processing
    /// </summary>
    /// <param name="exception">Exception that occurred</param>
    /// <param name="context">Message context</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Task with error handling result</returns>
    Task<ErrorHandlingResult> HandleErrorAsync(Exception exception, MessageContext context, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Handles an error that occurred during message processing with retry information
    /// </summary>
    /// <param name="exception">Exception that occurred</param>
    /// <param name="context">Message context</param>
    /// <param name="attemptNumber">Current attempt number</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Task with error handling result</returns>
    Task<ErrorHandlingResult> HandleErrorAsync(Exception exception, MessageContext context, int attemptNumber, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Determines if the error can be handled by this handler
    /// </summary>
    /// <param name="exception">Exception to check</param>
    /// <returns>True if the error can be handled, otherwise false</returns>
    bool CanHandle(Exception exception);
    
    /// <summary>
    /// Gets the retry policy for a specific error
    /// </summary>
    /// <param name="exception">Exception</param>
    /// <returns>Retry policy or null if no retry should be performed</returns>
    IRetryPolicy? GetRetryPolicy(Exception exception);
    
    /// <summary>
    /// Determines if the message should be sent to dead letter queue
    /// </summary>
    /// <param name="exception">Exception that occurred</param>
    /// <param name="context">Message context</param>
    /// <param name="attemptNumber">Current attempt number</param>
    /// <returns>True if message should be sent to dead letter queue, otherwise false</returns>
    bool ShouldSendToDeadLetterQueue(Exception exception, MessageContext context, int attemptNumber);
    
    /// <summary>
    /// Event raised when an error is handled
    /// </summary>
    event EventHandler<ErrorHandledEventArgs>? ErrorHandled;
    
    /// <summary>
    /// Event raised when a message is sent to dead letter queue
    /// </summary>
    event EventHandler<DeadLetterEventArgs>? MessageSentToDeadLetterQueue;
}

/// <summary>
/// Interface for dead letter queue handling
/// </summary>
public interface IDeadLetterHandler
{
    /// <summary>
    /// Gets the dead letter handler name
    /// </summary>
    string Name { get; }
    
    /// <summary>
    /// Gets the dead letter settings
    /// </summary>
    DeadLetterSettings Settings { get; }
    
    /// <summary>
    /// Sends a message to the dead letter queue
    /// </summary>
    /// <param name="message">Message to send</param>
    /// <param name="context">Message context</param>
    /// <param name="exception">Exception that caused the dead letter</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Task with operation result</returns>
    Task<bool> SendToDeadLetterAsync(byte[] message, MessageContext context, Exception exception, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Sends a message to the dead letter queue with retry information
    /// </summary>
    /// <param name="message">Message to send</param>
    /// <param name="context">Message context</param>
    /// <param name="exception">Exception that caused the dead letter</param>
    /// <param name="attemptNumber">Number of attempts made</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Task with operation result</returns>
    Task<bool> SendToDeadLetterAsync(byte[] message, MessageContext context, Exception exception, int attemptNumber, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Processes messages from the dead letter queue
    /// </summary>
    /// <param name="messageHandler">Handler for processing dead letter messages</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Task representing the processing operation</returns>
    Task ProcessDeadLetterMessagesAsync(Func<DeadLetterMessage, Task<bool>> messageHandler, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Reprocesses a message from the dead letter queue
    /// </summary>
    /// <param name="deadLetterMessage">Dead letter message</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Task with operation result</returns>
    Task<bool> ReprocessMessageAsync(DeadLetterMessage deadLetterMessage, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Gets statistics for the dead letter queue
    /// </summary>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Task with dead letter statistics</returns>
    Task<DeadLetterStatistics> GetStatisticsAsync(CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Purges the dead letter queue
    /// </summary>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Task with number of messages purged</returns>
    Task<int> PurgeDeadLetterQueueAsync(CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Event raised when a message is sent to dead letter queue
    /// </summary>
    event EventHandler<DeadLetterEventArgs>? MessageSentToDeadLetter;
    
    /// <summary>
    /// Event raised when a message is reprocessed from dead letter queue
    /// </summary>
    event EventHandler<DeadLetterReprocessedEventArgs>? MessageReprocessed;
}

/// <summary>
/// Result of error handling operation
/// </summary>
public class ErrorHandlingResult
{
    /// <summary>
    /// Action to take as result of error handling
    /// </summary>
    public ErrorHandlingAction Action { get; }
    
    /// <summary>
    /// Whether the error was handled successfully
    /// </summary>
    public bool IsHandled { get; }
    
    /// <summary>
    /// Delay before retry (if applicable)
    /// </summary>
    public TimeSpan? RetryDelay { get; }
    
    /// <summary>
    /// Error message
    /// </summary>
    public string? ErrorMessage { get; }
    
    /// <summary>
    /// Additional context data
    /// </summary>
    public Dictionary<string, object>? Context { get; }
    
    /// <summary>
    /// Timestamp of error handling
    /// </summary>
    public DateTimeOffset Timestamp { get; }
    
    /// <summary>
    /// Initializes a new instance of the ErrorHandlingResult class
    /// </summary>
    /// <param name="action">Error handling action</param>
    /// <param name="isHandled">Whether error was handled</param>
    /// <param name="retryDelay">Retry delay</param>
    /// <param name="errorMessage">Error message</param>
    /// <param name="context">Additional context</param>
    public ErrorHandlingResult(
        ErrorHandlingAction action, 
        bool isHandled, 
        TimeSpan? retryDelay = null, 
        string? errorMessage = null, 
        Dictionary<string, object>? context = null)
    {
        Action = action;
        IsHandled = isHandled;
        RetryDelay = retryDelay;
        ErrorMessage = errorMessage;
        Context = context;
        Timestamp = DateTimeOffset.UtcNow;
    }
    
    /// <summary>
    /// Creates a successful error handling result
    /// </summary>
    /// <param name="action">Action taken</param>
    /// <param name="context">Additional context</param>
    /// <returns>Error handling result</returns>
    public static ErrorHandlingResult Success(ErrorHandlingAction action, Dictionary<string, object>? context = null)
    {
        return new ErrorHandlingResult(action, true, context: context);
    }
    
    /// <summary>
    /// Creates a retry error handling result
    /// </summary>
    /// <param name="retryDelay">Delay before retry</param>
    /// <param name="context">Additional context</param>
    /// <returns>Error handling result</returns>
    public static ErrorHandlingResult Retry(TimeSpan retryDelay, Dictionary<string, object>? context = null)
    {
        return new ErrorHandlingResult(ErrorHandlingAction.Retry, true, retryDelay, context: context);
    }
    
    /// <summary>
    /// Creates a dead letter error handling result
    /// </summary>
    /// <param name="errorMessage">Error message</param>
    /// <param name="context">Additional context</param>
    /// <returns>Error handling result</returns>
    public static ErrorHandlingResult DeadLetter(string? errorMessage = null, Dictionary<string, object>? context = null)
    {
        return new ErrorHandlingResult(ErrorHandlingAction.DeadLetter, true, errorMessage: errorMessage, context: context);
    }
    
    /// <summary>
    /// Creates a failed error handling result
    /// </summary>
    /// <param name="errorMessage">Error message</param>
    /// <param name="context">Additional context</param>
    /// <returns>Error handling result</returns>
    public static ErrorHandlingResult Failed(string errorMessage, Dictionary<string, object>? context = null)
    {
        return new ErrorHandlingResult(ErrorHandlingAction.Reject, false, errorMessage: errorMessage, context: context);
    }
}

/// <summary>
/// Error handling action
/// </summary>
public enum ErrorHandlingAction
{
    /// <summary>
    /// Retry the operation
    /// </summary>
    Retry,
    
    /// <summary>
    /// Send to dead letter queue
    /// </summary>
    DeadLetter,
    
    /// <summary>
    /// Reject the message
    /// </summary>
    Reject,
    
    /// <summary>
    /// Acknowledge the message (ignore the error)
    /// </summary>
    Acknowledge
}

/// <summary>
/// Event arguments for error handling
/// </summary>
public class ErrorHandledEventArgs : EventArgs
{
    /// <summary>
    /// Exception that was handled
    /// </summary>
    public Exception Exception { get; }
    
    /// <summary>
    /// Message context
    /// </summary>
    public MessageContext Context { get; }
    
    /// <summary>
    /// Error handling result
    /// </summary>
    public ErrorHandlingResult Result { get; }
    
    /// <summary>
    /// Handler name
    /// </summary>
    public string HandlerName { get; }
    
    /// <summary>
    /// Timestamp
    /// </summary>
    public DateTimeOffset Timestamp { get; }
    
    /// <summary>
    /// Initializes a new instance of the ErrorHandledEventArgs class
    /// </summary>
    /// <param name="exception">Exception</param>
    /// <param name="context">Message context</param>
    /// <param name="result">Error handling result</param>
    /// <param name="handlerName">Handler name</param>
    public ErrorHandledEventArgs(Exception exception, MessageContext context, ErrorHandlingResult result, string handlerName)
    {
        Exception = exception;
        Context = context;
        Result = result;
        HandlerName = handlerName;
        Timestamp = DateTimeOffset.UtcNow;
    }
}

/// <summary>
/// Event arguments for dead letter operations
/// </summary>
public class DeadLetterEventArgs : EventArgs
{
    /// <summary>
    /// Message that was sent to dead letter queue
    /// </summary>
    public byte[] Message { get; }
    
    /// <summary>
    /// Message context
    /// </summary>
    public MessageContext Context { get; }
    
    /// <summary>
    /// Exception that caused the dead letter
    /// </summary>
    public Exception Exception { get; }
    
    /// <summary>
    /// Number of attempts made
    /// </summary>
    public int AttemptNumber { get; }
    
    /// <summary>
    /// Timestamp
    /// </summary>
    public DateTimeOffset Timestamp { get; }
    
    /// <summary>
    /// Initializes a new instance of the DeadLetterEventArgs class
    /// </summary>
    /// <param name="message">Message</param>
    /// <param name="context">Message context</param>
    /// <param name="exception">Exception</param>
    /// <param name="attemptNumber">Attempt number</param>
    public DeadLetterEventArgs(byte[] message, MessageContext context, Exception exception, int attemptNumber)
    {
        Message = message;
        Context = context;
        Exception = exception;
        AttemptNumber = attemptNumber;
        Timestamp = DateTimeOffset.UtcNow;
    }
}

/// <summary>
/// Event arguments for dead letter reprocessing
/// </summary>
public class DeadLetterReprocessedEventArgs : EventArgs
{
    /// <summary>
    /// Dead letter message that was reprocessed
    /// </summary>
    public DeadLetterMessage DeadLetterMessage { get; }
    
    /// <summary>
    /// Whether reprocessing was successful
    /// </summary>
    public bool IsSuccessful { get; }
    
    /// <summary>
    /// Error message (if reprocessing failed)
    /// </summary>
    public string? ErrorMessage { get; }
    
    /// <summary>
    /// Timestamp
    /// </summary>
    public DateTimeOffset Timestamp { get; }
    
    /// <summary>
    /// Initializes a new instance of the DeadLetterReprocessedEventArgs class
    /// </summary>
    /// <param name="deadLetterMessage">Dead letter message</param>
    /// <param name="isSuccessful">Whether reprocessing was successful</param>
    /// <param name="errorMessage">Error message</param>
    public DeadLetterReprocessedEventArgs(DeadLetterMessage deadLetterMessage, bool isSuccessful, string? errorMessage = null)
    {
        DeadLetterMessage = deadLetterMessage;
        IsSuccessful = isSuccessful;
        ErrorMessage = errorMessage;
        Timestamp = DateTimeOffset.UtcNow;
    }
}

/// <summary>
/// Represents a message in the dead letter queue
/// </summary>
public class DeadLetterMessage
{
    /// <summary>
    /// Message identifier
    /// </summary>
    public string MessageId { get; set; } = Guid.CreateVersion7().ToString();
    
    /// <summary>
    /// Original message data
    /// </summary>
    public byte[] Data { get; set; } = Array.Empty<byte>();
    
    /// <summary>
    /// Message context
    /// </summary>
    public MessageContext Context { get; set; } = new();
    
    /// <summary>
    /// Exception that caused the dead letter
    /// </summary>
    public Exception Exception { get; set; } = new InvalidOperationException("Unknown error");
    
    /// <summary>
    /// Number of attempts made
    /// </summary>
    public int AttemptNumber { get; set; }
    
    /// <summary>
    /// Timestamp when message was sent to dead letter queue
    /// </summary>
    public DateTimeOffset DeadLetterTimestamp { get; set; } = DateTimeOffset.UtcNow;
    
    /// <summary>
    /// Reason for dead letter
    /// </summary>
    public string Reason { get; set; } = string.Empty;
    
    /// <summary>
    /// Additional metadata
    /// </summary>
    public Dictionary<string, object>? Metadata { get; set; }
}

/// <summary>
/// Statistics for dead letter queue
/// </summary>
public class DeadLetterStatistics
{
    /// <summary>
    /// Total number of messages in dead letter queue
    /// </summary>
    public long TotalMessages { get; set; }
    
    /// <summary>
    /// Number of messages processed from dead letter queue
    /// </summary>
    public long ProcessedMessages { get; set; }
    
    /// <summary>
    /// Number of messages successfully reprocessed
    /// </summary>
    public long ReprocessedMessages { get; set; }
    
    /// <summary>
    /// Number of messages that failed reprocessing
    /// </summary>
    public long FailedReprocessing { get; set; }
    
    /// <summary>
    /// Average message age in dead letter queue
    /// </summary>
    public TimeSpan AverageMessageAge { get; set; }
    
    /// <summary>
    /// Oldest message timestamp
    /// </summary>
    public DateTimeOffset? OldestMessageTimestamp { get; set; }
    
    /// <summary>
    /// Newest message timestamp
    /// </summary>
    public DateTimeOffset? NewestMessageTimestamp { get; set; }
    
    /// <summary>
    /// Dead letter queue size in bytes
    /// </summary>
    public long QueueSizeBytes { get; set; }
    
    /// <summary>
    /// Statistics collection timestamp
    /// </summary>
    public DateTimeOffset Timestamp { get; set; } = DateTimeOffset.UtcNow;
} 