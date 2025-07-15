namespace FS.StreamFlow.Core.Features.Messaging.Models;

/// <summary>
/// Event arguments for message received events
/// </summary>
public class MessageReceivedEventArgs : EventArgs
{
    /// <summary>
    /// The received message
    /// </summary>
    public object Message { get; }
    
    /// <summary>
    /// Message context
    /// </summary>
    public MessageContext Context { get; }
    
    /// <summary>
    /// Timestamp when the message was received
    /// </summary>
    public DateTimeOffset Timestamp { get; }
    
    /// <summary>
    /// Initializes a new instance of the MessageReceivedEventArgs class
    /// </summary>
    /// <param name="message">The received message</param>
    /// <param name="context">Message context</param>
    public MessageReceivedEventArgs(object message, MessageContext context)
    {
        Message = message;
        Context = context;
        Timestamp = DateTimeOffset.UtcNow;
    }
}

/// <summary>
/// Event arguments for message processed events
/// </summary>
public class MessageProcessedEventArgs : EventArgs
{
    /// <summary>
    /// The processed message
    /// </summary>
    public object Message { get; }
    
    /// <summary>
    /// Message context
    /// </summary>
    public MessageContext Context { get; }
    
    /// <summary>
    /// Processing result
    /// </summary>
    public bool ProcessingResult { get; }
    
    /// <summary>
    /// Processing duration
    /// </summary>
    public TimeSpan ProcessingDuration { get; }
    
    /// <summary>
    /// Timestamp when the message was processed
    /// </summary>
    public DateTimeOffset Timestamp { get; }
    
    /// <summary>
    /// Initializes a new instance of the MessageProcessedEventArgs class
    /// </summary>
    /// <param name="message">The processed message</param>
    /// <param name="context">Message context</param>
    /// <param name="processingResult">Processing result</param>
    /// <param name="processingDuration">Processing duration</param>
    public MessageProcessedEventArgs(object message, MessageContext context, bool processingResult, TimeSpan processingDuration)
    {
        Message = message;
        Context = context;
        ProcessingResult = processingResult;
        ProcessingDuration = processingDuration;
        Timestamp = DateTimeOffset.UtcNow;
    }
}

/// <summary>
/// Event arguments for message processing failed events
/// </summary>
public class MessageProcessingFailedEventArgs : EventArgs
{
    /// <summary>
    /// The message that failed processing
    /// </summary>
    public object Message { get; }
    
    /// <summary>
    /// Message context
    /// </summary>
    public MessageContext Context { get; }
    
    /// <summary>
    /// The exception that caused the failure
    /// </summary>
    public Exception Exception { get; }
    
    /// <summary>
    /// Attempt count
    /// </summary>
    public int AttemptCount { get; }
    
    /// <summary>
    /// Timestamp when the processing failed
    /// </summary>
    public DateTimeOffset Timestamp { get; }
    
    /// <summary>
    /// Initializes a new instance of the MessageProcessingFailedEventArgs class
    /// </summary>
    /// <param name="message">The message that failed processing</param>
    /// <param name="context">Message context</param>
    /// <param name="exception">The exception that caused the failure</param>
    /// <param name="attemptCount">Attempt count</param>
    public MessageProcessingFailedEventArgs(object message, MessageContext context, Exception exception, int attemptCount)
    {
        Message = message;
        Context = context;
        Exception = exception;
        AttemptCount = attemptCount;
        Timestamp = DateTimeOffset.UtcNow;
    }
}

/// <summary>
/// Event arguments for message acknowledged events
/// </summary>
public class MessageAcknowledgedEventArgs : EventArgs
{
    /// <summary>
    /// The acknowledged message
    /// </summary>
    public object Message { get; }
    
    /// <summary>
    /// Message context
    /// </summary>
    public MessageContext Context { get; }
    
    /// <summary>
    /// Timestamp when the message was acknowledged
    /// </summary>
    public DateTimeOffset Timestamp { get; }
    
    /// <summary>
    /// Initializes a new instance of the MessageAcknowledgedEventArgs class
    /// </summary>
    /// <param name="message">The acknowledged message</param>
    /// <param name="context">Message context</param>
    public MessageAcknowledgedEventArgs(object message, MessageContext context)
    {
        Message = message;
        Context = context;
        Timestamp = DateTimeOffset.UtcNow;
    }
}

/// <summary>
/// Event arguments for message rejected events
/// </summary>
public class MessageRejectedEventArgs : EventArgs
{
    /// <summary>
    /// The rejected message
    /// </summary>
    public object Message { get; }
    
    /// <summary>
    /// Message context
    /// </summary>
    public MessageContext Context { get; }
    
    /// <summary>
    /// Rejection reason
    /// </summary>
    public string? Reason { get; }
    
    /// <summary>
    /// Whether the message was requeued
    /// </summary>
    public bool Requeued { get; }
    
    /// <summary>
    /// Timestamp when the message was rejected
    /// </summary>
    public DateTimeOffset Timestamp { get; }
    
    /// <summary>
    /// Initializes a new instance of the MessageRejectedEventArgs class
    /// </summary>
    /// <param name="message">The rejected message</param>
    /// <param name="context">Message context</param>
    /// <param name="reason">Rejection reason</param>
    /// <param name="requeued">Whether the message was requeued</param>
    public MessageRejectedEventArgs(object message, MessageContext context, string? reason, bool requeued)
    {
        Message = message;
        Context = context;
        Reason = reason;
        Requeued = requeued;
        Timestamp = DateTimeOffset.UtcNow;
    }
}

/// <summary>
/// Event arguments for consumer status changed events
/// </summary>
public class ConsumerStatusChangedEventArgs : EventArgs
{
    /// <summary>
    /// Previous consumer status
    /// </summary>
    public ConsumerStatus PreviousStatus { get; }
    
    /// <summary>
    /// Current consumer status
    /// </summary>
    public ConsumerStatus CurrentStatus { get; }
    
    /// <summary>
    /// Consumer identifier
    /// </summary>
    public string ConsumerId { get; }
    
    /// <summary>
    /// Timestamp when the status changed
    /// </summary>
    public DateTimeOffset Timestamp { get; }
    
    /// <summary>
    /// Initializes a new instance of the ConsumerStatusChangedEventArgs class
    /// </summary>
    /// <param name="previousStatus">Previous consumer status</param>
    /// <param name="currentStatus">Current consumer status</param>
    /// <param name="consumerId">Consumer identifier</param>
    public ConsumerStatusChangedEventArgs(ConsumerStatus previousStatus, ConsumerStatus currentStatus, string consumerId)
    {
        PreviousStatus = previousStatus;
        CurrentStatus = currentStatus;
        ConsumerId = consumerId;
        Timestamp = DateTimeOffset.UtcNow;
    }
}

/// <summary>
/// Event arguments for consumer paused events
/// </summary>
public class ConsumerPausedEventArgs : EventArgs
{
    /// <summary>
    /// Consumer identifier
    /// </summary>
    public string ConsumerId { get; }
    
    /// <summary>
    /// Pause reason
    /// </summary>
    public string? Reason { get; }
    
    /// <summary>
    /// Timestamp when the consumer was paused
    /// </summary>
    public DateTimeOffset Timestamp { get; }
    
    /// <summary>
    /// Initializes a new instance of the ConsumerPausedEventArgs class
    /// </summary>
    /// <param name="consumerId">Consumer identifier</param>
    /// <param name="reason">Pause reason</param>
    public ConsumerPausedEventArgs(string consumerId, string? reason)
    {
        ConsumerId = consumerId;
        Reason = reason;
        Timestamp = DateTimeOffset.UtcNow;
    }
}

/// <summary>
/// Event arguments for consumer resumed events
/// </summary>
public class ConsumerResumedEventArgs : EventArgs
{
    /// <summary>
    /// Consumer identifier
    /// </summary>
    public string ConsumerId { get; }
    
    /// <summary>
    /// Resume reason
    /// </summary>
    public string? Reason { get; }
    
    /// <summary>
    /// Timestamp when the consumer was resumed
    /// </summary>
    public DateTimeOffset Timestamp { get; }
    
    /// <summary>
    /// Initializes a new instance of the ConsumerResumedEventArgs class
    /// </summary>
    /// <param name="consumerId">Consumer identifier</param>
    /// <param name="reason">Resume reason</param>
    public ConsumerResumedEventArgs(string consumerId, string? reason)
    {
        ConsumerId = consumerId;
        Reason = reason;
        Timestamp = DateTimeOffset.UtcNow;
    }
}

/// <summary>
/// Consumer context for advanced operations
/// </summary>
public class ConsumerContext
{
    /// <summary>
    /// Consumer identifier
    /// </summary>
    public string ConsumerId { get; set; } = string.Empty;
    
    /// <summary>
    /// Consumer tag
    /// </summary>
    public string ConsumerTag { get; set; } = string.Empty;
    
    /// <summary>
    /// Consumer settings
    /// </summary>
    public ConsumerSettings Settings { get; set; } = new();
    
    /// <summary>
    /// Additional context data
    /// </summary>
    public Dictionary<string, object>? Data { get; set; }
    
    /// <summary>
    /// Context creation timestamp
    /// </summary>
    public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;
    
    /// <summary>
    /// Creates a new ConsumerContext instance
    /// </summary>
    public ConsumerContext()
    {
        Data = new Dictionary<string, object>();
    }
} 