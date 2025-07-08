namespace FS.RabbitMQ.Core.Abstractions;

/// <summary>
/// Defines the types of actions available for error handling strategies.
/// </summary>
public enum ErrorHandlingAction
{
    /// <summary>
    /// Retry the failed operation according to the configured retry policy.
    /// </summary>
    Retry,

    /// <summary>
    /// Route the failed message to the dead letter exchange for analysis and potential reprocessing.
    /// </summary>
    DeadLetter,

    /// <summary>
    /// Discard the failed message without further processing or storage.
    /// </summary>
    Discard,

    /// <summary>
    /// Execute custom error handling logic defined by the strategy.
    /// </summary>
    Custom
}
