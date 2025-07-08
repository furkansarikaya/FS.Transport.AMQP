namespace FS.RabbitMQ.Core.Abstractions;

/// <summary>
/// Contains configuration for dead letter exchange handling of failed messages.
/// </summary>
/// <remarks>
/// Dead letter configuration provides comprehensive setup for handling messages
/// that cannot be processed successfully, ensuring they are preserved for analysis
/// and potential reprocessing while preventing them from blocking normal message flow.
/// </remarks>
public sealed record DeadLetterConfiguration
{
    /// <summary>
    /// Gets the name of the dead letter exchange.
    /// </summary>
    /// <value>The exchange name where failed messages will be routed.</value>
    public required string ExchangeName { get; init; }

    /// <summary>
    /// Gets the routing key to use when sending messages to the dead letter exchange.
    /// </summary>
    /// <value>The routing key for dead letter message routing, or null to use the original routing key.</value>
    public string? RoutingKey { get; init; }

    /// <summary>
    /// Gets additional headers to add to dead letter messages.
    /// </summary>
    /// <value>A dictionary of headers to include with dead letter messages, or null if no additional headers are needed.</value>
    public IReadOnlyDictionary<string, object>? AdditionalHeaders { get; init; }

    /// <summary>
    /// Gets a value indicating whether to include the original exception details in dead letter headers.
    /// </summary>
    /// <value><c>true</c> to include exception information in message headers; otherwise, <c>false</c>.</value>
    public bool IncludeExceptionDetails { get; init; } = true;

    /// <summary>
    /// Gets a value indicating whether to include processing attempt history in dead letter headers.
    /// </summary>
    /// <value><c>true</c> to include attempt history in message headers; otherwise, <c>false</c>.</value>
    public bool IncludeAttemptHistory { get; init; } = true;

    /// <summary>
    /// Creates a basic dead letter configuration with the specified exchange.
    /// </summary>
    /// <param name="exchangeName">The dead letter exchange name.</param>
    /// <param name="routingKey">Optional routing key for dead letter messages.</param>
    /// <returns>A dead letter configuration with default settings.</returns>
    public static DeadLetterConfiguration Create(string exchangeName, string? routingKey = null) =>
        new() { ExchangeName = exchangeName, RoutingKey = routingKey };
}