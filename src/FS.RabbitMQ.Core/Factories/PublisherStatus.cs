namespace FS.RabbitMQ.Core.Factories;

/// <summary>
/// Defines the status of a publisher.
/// </summary>
public enum PublisherStatus
{
    /// <summary>
    /// Publisher status is unknown.
    /// </summary>
    Unknown,

    /// <summary>
    /// Publisher is initializing.
    /// </summary>
    Initializing,

    /// <summary>
    /// Publisher is ready and can publish messages.
    /// </summary>
    Ready,

    /// <summary>
    /// Publisher is actively publishing messages.
    /// </summary>
    Publishing,

    /// <summary>
    /// Publisher is temporarily paused.
    /// </summary>
    Paused,

    /// <summary>
    /// Publisher is stopping.
    /// </summary>
    Stopping,

    /// <summary>
    /// Publisher has stopped.
    /// </summary>
    Stopped,

    /// <summary>
    /// Publisher has encountered an error.
    /// </summary>
    Error
}