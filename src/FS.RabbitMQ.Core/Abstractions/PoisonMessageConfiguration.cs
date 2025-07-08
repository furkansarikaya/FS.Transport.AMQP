namespace FS.RabbitMQ.Core.Abstractions;

/// <summary>
/// Contains configuration for poison message detection and handling.
/// </summary>
/// <remarks>
/// Poison message configuration provides automatic detection of messages
/// that consistently fail processing, enabling isolation and special handling
/// to prevent system disruption and resource exhaustion.
/// </remarks>
public sealed record PoisonMessageConfiguration
{
    /// <summary>
    /// Gets the delivery count threshold for poison message detection.
    /// </summary>
    /// <value>The number of delivery attempts after which a message is considered poisonous.</value>
    public int DeliveryCountThreshold { get; init; } = 10;

    /// <summary>
    /// Gets the time window for evaluating poison message patterns.
    /// </summary>
    /// <value>The duration within which delivery attempts are counted for poison detection.</value>
    public TimeSpan EvaluationWindow { get; init; } = TimeSpan.FromHours(1);

    /// <summary>
    /// Gets the action to take when a poison message is detected.
    /// </summary>
    /// <value>The handling strategy for identified poison messages.</value>
    public PoisonMessageAction Action { get; init; } = PoisonMessageAction.DeadLetter;

    /// <summary>
    /// Gets the custom handler for poison messages when using custom action.
    /// </summary>
    /// <value>A custom handler function for poison message processing, or null if not using custom action.</value>
    public Func<MessageContext<object>, Task>? CustomHandler { get; init; }

    /// <summary>
    /// Creates a poison message configuration with the specified threshold.
    /// </summary>
    /// <param name="deliveryCountThreshold">The delivery count threshold for poison detection.</param>
    /// <param name="action">The action to take for poison messages.</param>
    /// <returns>A poison message configuration with the specified settings.</returns>
    public static PoisonMessageConfiguration Create(int deliveryCountThreshold = 10, PoisonMessageAction action = PoisonMessageAction.DeadLetter) =>
        new() { DeliveryCountThreshold = deliveryCountThreshold, Action = action };
}