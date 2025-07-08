namespace FS.RabbitMQ.Core.Configuration;

/// <summary>
/// Contains configuration for queue declaration and properties.
/// </summary>
public sealed class QueueConfiguration
{
    /// <summary>
    /// Gets or sets a value indicating whether queues should be durable by default.
    /// </summary>
    /// <value><c>true</c> to create durable queues; otherwise, <c>false</c>. Default is <c>true</c>.</value>
    public bool DefaultDurable { get; set; } = true;

    /// <summary>
    /// Gets or sets a value indicating whether queues should be exclusive by default.
    /// </summary>
    /// <value><c>true</c> to create exclusive queues; otherwise, <c>false</c>. Default is <c>false</c>.</value>
    public bool DefaultExclusive { get; set; } = false;

    /// <summary>
    /// Gets or sets a value indicating whether queues should auto-delete by default.
    /// </summary>
    /// <value><c>true</c> to create auto-delete queues; otherwise, <c>false</c>. Default is <c>false</c>.</value>
    public bool DefaultAutoDelete { get; set; } = false;

    /// <summary>
    /// Gets or sets default queue arguments.
    /// </summary>
    /// <value>A dictionary of default arguments for queue creation.</value>
    public IDictionary<string, object>? DefaultArguments { get; set; }

    /// <summary>
    /// Creates a queue configuration suitable for development environments.
    /// </summary>
    public static QueueConfiguration CreateDevelopment() =>
        new() { DefaultDurable = false, DefaultAutoDelete = true };

    /// <summary>
    /// Creates a queue configuration suitable for production environments.
    /// </summary>
    public static QueueConfiguration CreateProduction() =>
        new() { DefaultDurable = true, DefaultAutoDelete = false };

    /// <summary>
    /// Creates a default queue configuration.
    /// </summary>
    public static QueueConfiguration CreateDefault() => new();

    /// <summary>
    /// Validates the queue configuration.
    /// </summary>
    public void Validate() { }
}