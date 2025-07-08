namespace FS.RabbitMQ.Core.Abstractions;

/// <summary>
/// Contains the result information from a message publishing operation.
/// </summary>
/// <remarks>
/// Publishing results provide comprehensive feedback about message delivery status,
/// enabling applications to implement appropriate error handling, retry logic,
/// and delivery confirmation workflows. The result includes both success confirmation
/// and detailed error information when publishing fails.
/// 
/// Results are particularly important in scenarios requiring delivery guarantees,
/// audit trails, or complex error handling strategies. They enable applications
/// to distinguish between different types of publishing failures and respond accordingly.
/// </remarks>
public sealed record PublishResult
{
    /// <summary>
    /// Gets a value indicating whether the message was successfully published.
    /// </summary>
    /// <value><c>true</c> if the message was accepted by the broker; otherwise, <c>false</c>.</value>
    /// <remarks>
    /// Success indicates that the broker has accepted responsibility for the message
    /// according to the configured delivery guarantees:
    /// - With publisher confirmation: broker has acknowledged receipt and routing
    /// - Without confirmation: message was sent without immediate errors
    /// - Success does not guarantee final delivery to consumers (depends on queue durability)
    /// 
    /// Applications should check this property before considering the publishing
    /// operation complete and proceeding with dependent operations.
    /// </remarks>
    public bool IsSuccess { get; init; }

    /// <summary>
    /// Gets the unique identifier assigned to the published message.
    /// </summary>
    /// <value>A unique message identifier, or null if the message was not successfully published.</value>
    /// <remarks>
    /// Message IDs enable:
    /// - Tracking individual messages through the system
    /// - Implementing deduplication logic
    /// - Correlating publish results with downstream processing
    /// - Supporting audit and compliance requirements
    /// 
    /// The ID may be provided by the application or automatically generated
    /// by the publishing infrastructure, depending on configuration.
    /// </remarks>
    public string? MessageId { get; init; }

    /// <summary>
    /// Gets the exchange name where the message was published.
    /// </summary>
    /// <value>The name of the exchange that received the message.</value>
    /// <remarks>
    /// Exchange information is useful for:
    /// - Confirming correct routing configuration
    /// - Implementing retry logic with alternative exchanges
    /// - Audit logging and troubleshooting
    /// - Monitoring message flow through different exchanges
    /// </remarks>
    public required string ExchangeName { get; init; }

    /// <summary>
    /// Gets the routing key used for message routing.
    /// </summary>
    /// <value>The routing key that was applied to the message.</value>
    /// <remarks>
    /// Routing key information enables:
    /// - Verification of correct routing configuration
    /// - Implementation of routing-aware retry strategies
    /// - Detailed audit trails for message routing decisions
    /// - Troubleshooting routing and binding issues
    /// </remarks>
    public required string RoutingKey { get; init; }

    /// <summary>
    /// Gets the timestamp when the publishing operation completed.
    /// </summary>
    /// <value>The UTC timestamp of publishing completion.</value>
    /// <remarks>
    /// Completion timestamps enable:
    /// - Performance monitoring and latency analysis
    /// - Timeout detection in complex workflows
    /// - Audit trails with accurate timing information
    /// - Correlation with external system events
    /// 
    /// The timestamp represents when the broker confirmed receipt
    /// (with confirmation) or when the send operation completed (without confirmation).
    /// </remarks>
    public DateTimeOffset Timestamp { get; init; } = DateTimeOffset.UtcNow;

    /// <summary>
    /// Gets the broker confirmation sequence number, if publisher confirmation is enabled.
    /// </summary>
    /// <value>The sequence number assigned by the broker, or null if confirmation is not enabled.</value>
    /// <remarks>
    /// Confirmation sequence numbers provide:
    /// - Ordering information for published messages
    /// - Correlation between publish operations and broker confirmations
    /// - Support for batch confirmation scenarios
    /// - Detailed tracking of message flow through the broker
    /// 
    /// Sequence numbers are particularly useful in high-throughput scenarios
    /// where multiple messages are published rapidly and confirmation ordering matters.
    /// </remarks>
    public long? ConfirmationSequenceNumber { get; init; }

    /// <summary>
    /// Gets error information if the publishing operation failed.
    /// </summary>
    /// <value>Details about the publishing failure, or null if the operation succeeded.</value>
    /// <remarks>
    /// Error information includes:
    /// - Exception details with stack traces for debugging
    /// - Error categorization for appropriate response strategies
    /// - Retry recommendations based on error type
    /// - Context information for troubleshooting
    /// 
    /// Applications should examine error details to implement appropriate
    /// error handling, logging, and retry strategies.
    /// </remarks>
    public PublishError? Error { get; init; }

    /// <summary>
    /// Gets additional metadata about the publishing operation.
    /// </summary>
    /// <value>A dictionary containing supplementary information, or null if no additional data is available.</value>
    /// <remarks>
    /// Metadata may include:
    /// - Channel information for debugging connection issues
    /// - Serialization details for troubleshooting format problems
    /// - Performance metrics for optimization analysis
    /// - Broker-specific information for advanced scenarios
    /// 
    /// Metadata provides extensibility for capturing operation-specific
    /// information that may be valuable for monitoring, debugging, or optimization.
    /// </remarks>
    public IReadOnlyDictionary<string, object>? Metadata { get; init; }

    /// <summary>
    /// Creates a successful publish result.
    /// </summary>
    /// <param name="messageId">The unique identifier of the published message.</param>
    /// <param name="exchangeName">The name of the exchange where the message was published.</param>
    /// <param name="routingKey">The routing key used for the message.</param>
    /// <param name="confirmationSequenceNumber">Optional broker confirmation sequence number.</param>
    /// <param name="metadata">Optional additional metadata about the operation.</param>
    /// <returns>A PublishResult indicating successful publishing.</returns>
    /// <exception cref="ArgumentException">
    /// Thrown when <paramref name="exchangeName"/> or <paramref name="routingKey"/> is null.
    /// </exception>
    public static PublishResult Success(
        string? messageId,
        string exchangeName,
        string routingKey,
        long? confirmationSequenceNumber = null,
        IReadOnlyDictionary<string, object>? metadata = null)
    {
        ArgumentNullException.ThrowIfNull(exchangeName);
        ArgumentNullException.ThrowIfNull(routingKey);

        return new PublishResult
        {
            IsSuccess = true,
            MessageId = messageId,
            ExchangeName = exchangeName,
            RoutingKey = routingKey,
            ConfirmationSequenceNumber = confirmationSequenceNumber,
            Metadata = metadata,
            Error = null
        };
    }

    /// <summary>
    /// Creates a failed publish result.
    /// </summary>
    /// <param name="exchangeName">The name of the exchange where publishing was attempted.</param>
    /// <param name="routingKey">The routing key that was used.</param>
    /// <param name="error">The error information describing the failure.</param>
    /// <param name="messageId">Optional message identifier if available.</param>
    /// <param name="metadata">Optional additional metadata about the operation.</param>
    /// <returns>A PublishResult indicating publishing failure.</returns>
    /// <exception cref="ArgumentException">
    /// Thrown when <paramref name="exchangeName"/> or <paramref name="routingKey"/> is null.
    /// </exception>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="error"/> is null.</exception>
    public static PublishResult Failure(
        string exchangeName,
        string routingKey,
        PublishError error,
        string? messageId = null,
        IReadOnlyDictionary<string, object>? metadata = null)
    {
        ArgumentNullException.ThrowIfNull(exchangeName);
        ArgumentNullException.ThrowIfNull(routingKey);
        ArgumentNullException.ThrowIfNull(error);

        return new PublishResult
        {
            IsSuccess = false,
            MessageId = messageId,
            ExchangeName = exchangeName,
            RoutingKey = routingKey,
            Error = error,
            Metadata = metadata,
            ConfirmationSequenceNumber = null
        };
    }

    /// <summary>
    /// Returns a string representation of the publish result.
    /// </summary>
    /// <returns>A formatted string describing the publishing outcome.</returns>
    public override string ToString()
    {
        if (IsSuccess)
        {
            var idInfo = !string.IsNullOrEmpty(MessageId) ? $" (ID: {MessageId})" : "";
            var seqInfo = ConfirmationSequenceNumber.HasValue ? $" [Seq: {ConfirmationSequenceNumber}]" : "";
            return $"Published successfully to '{ExchangeName}' with routing key '{RoutingKey}'{idInfo}{seqInfo}";
        }
        else
        {
            return $"Failed to publish to '{ExchangeName}' with routing key '{RoutingKey}': {Error?.ErrorType.ToString() ?? "Unknown error"}";
        }
    }
}