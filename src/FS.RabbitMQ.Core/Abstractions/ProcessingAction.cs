namespace FS.RabbitMQ.Core.Abstractions;

/// <summary>
/// Defines the actions that can be taken following message processing.
/// </summary>
public enum ProcessingAction
{
    /// <summary>
    /// Acknowledge the message as successfully processed.
    /// </summary>
    Acknowledge,

    /// <summary>
    /// Retry processing the message according to the configured retry policy.
    /// </summary>
    Retry,

    /// <summary>
    /// Route the message to the dead letter exchange for analysis or reprocessing.
    /// </summary>
    DeadLetter,

    /// <summary>
    /// Discard the message without further processing.
    /// </summary>
    Discard
}