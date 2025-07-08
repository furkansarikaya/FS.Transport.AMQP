namespace FS.RabbitMQ.Core.Abstractions;

/// <summary>
/// Exception thrown when message processing operations fail within a processing strategy.
/// </summary>
/// <remarks>
/// Message processing exceptions occur when custom processing strategies
/// encounter errors during message handling, transformation, or integration
/// with external systems. These exceptions provide detailed context about
/// processing failures and strategy-specific error conditions.
/// </remarks>
[Serializable]
public sealed class MessageProcessingException : MessagingException
{
    /// <summary>
    /// Gets the processing strategy identifier where the error occurred.
    /// </summary>
    /// <value>The strategy ID, or null if not available.</value>
    public string? StrategyId { get; }

    /// <summary>
    /// Gets the message identifier that was being processed.
    /// </summary>
    /// <value>The message ID, or null if not available.</value>
    public string? MessageId { get; }

    /// <summary>
    /// Gets the processing stage where the error occurred.
    /// </summary>
    /// <value>The processing stage description, or null if not specified.</value>
    public string? ProcessingStage { get; }

    /// <summary>
    /// Initializes a new instance of the MessageProcessingException class.
    /// </summary>
    /// <param name="message">The error message describing the processing failure.</param>
    /// <param name="strategyId">The processing strategy identifier.</param>
    /// <param name="messageId">The message identifier being processed.</param>
    /// <param name="processingStage">The processing stage where the error occurred.</param>
    /// <param name="errorCode">Optional error code for programmatic error handling.</param>
    /// <param name="context">Optional additional context information.</param>
    public MessageProcessingException(string message, string? strategyId = null, string? messageId = null, string? processingStage = null, string? errorCode = null, IReadOnlyDictionary<string, object>? context = null)
        : base(message, errorCode, context)
    {
        StrategyId = strategyId;
        MessageId = messageId;
        ProcessingStage = processingStage;
    }

    /// <summary>
    /// Initializes a new instance of the MessageProcessingException class.
    /// </summary>
    /// <param name="message">The error message describing the processing failure.</param>
    /// <param name="innerException">The exception that caused the processing failure.</param>
    /// <param name="strategyId">The processing strategy identifier.</param>
    /// <param name="messageId">The message identifier being processed.</param>
    /// <param name="processingStage">The processing stage where the error occurred.</param>
    /// <param name="errorCode">Optional error code for programmatic error handling.</param>
    /// <param name="context">Optional additional context information.</param>
    public MessageProcessingException(string message, Exception innerException, string? strategyId = null, string? messageId = null, string? processingStage = null, string? errorCode = null, IReadOnlyDictionary<string, object>? context = null)
        : base(message, innerException, errorCode, context)
    {
        StrategyId = strategyId;
        MessageId = messageId;
        ProcessingStage = processingStage;
    }

    /// <summary>
    /// Initializes a new instance of the MessageProcessingException class with serialized data.
    /// </summary>
    /// <param name="info">The SerializationInfo that holds the serialized object data.</param>
    /// <param name="context">The StreamingContext that contains contextual information.</param>
    private MessageProcessingException(System.Runtime.Serialization.SerializationInfo info, System.Runtime.Serialization.StreamingContext context)
        : base(info, context)
    {
        StrategyId = info.GetString(nameof(StrategyId));
        MessageId = info.GetString(nameof(MessageId));
        ProcessingStage = info.GetString(nameof(ProcessingStage));
    }
}