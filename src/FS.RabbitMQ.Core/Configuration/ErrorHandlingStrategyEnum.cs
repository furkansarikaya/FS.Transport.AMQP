namespace FS.RabbitMQ.Core.Configuration;

/// <summary>
/// Defines error handling strategies for consumer message processing failures.
/// </summary>
public enum ErrorHandlingStrategyEnum
{
    /// <summary>
    /// Ignore errors and acknowledge failed messages.
    /// </summary>
    Ignore,

    /// <summary>
    /// Retry failed messages according to the retry policy.
    /// </summary>
    Retry,

    /// <summary>
    /// Route failed messages to a dead letter exchange.
    /// </summary>
    DeadLetter,

    /// <summary>
    /// Retry failed messages with limits, then route to dead letter exchange.
    /// </summary>
    RetryThenDeadLetter,

    /// <summary>
    /// Use custom error handling logic.
    /// </summary>
    Custom
}
