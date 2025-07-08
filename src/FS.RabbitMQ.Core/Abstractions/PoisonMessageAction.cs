namespace FS.RabbitMQ.Core.Abstractions;

/// <summary>
/// Defines the actions available for handling poison messages.
/// </summary>
public enum PoisonMessageAction
{
    /// <summary>
    /// Route poison messages to the dead letter exchange.
    /// </summary>
    DeadLetter,

    /// <summary>
    /// Discard poison messages without further processing.
    /// </summary>
    Discard,

    /// <summary>
    /// Execute custom poison message handling logic.
    /// </summary>
    Custom
}