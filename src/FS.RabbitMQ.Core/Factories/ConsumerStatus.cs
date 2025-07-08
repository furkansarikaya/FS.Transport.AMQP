namespace FS.RabbitMQ.Core.Factories;

/// <summary>
/// Defines the status of a consumer.
/// </summary>
public enum ConsumerStatus
{
    /// <summary>
    /// Consumer status is unknown.
    /// </summary>
    Unknown,

    /// <summary>
    /// Consumer is starting up.
    /// </summary>
    Starting,

    /// <summary>
    /// Consumer is running and processing messages.
    /// </summary>
    Running,

    /// <summary>
    /// Consumer is temporarily paused.
    /// </summary>
    Paused,

    /// <summary>
    /// Consumer is stopping.
    /// </summary>
    Stopping,

    /// <summary>
    /// Consumer has stopped.
    /// </summary>
    Stopped,

    /// <summary>
    /// Consumer has encountered an error.
    /// </summary>
    Error
}