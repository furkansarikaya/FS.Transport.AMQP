namespace FS.RabbitMQ.ErrorHandling;

/// <summary>
/// Configuration settings for dead letter handling
/// </summary>
public class DeadLetterSettings
{
    /// <summary>
    /// Gets or sets the dead letter exchange name
    /// </summary>
    public string DeadLetterExchange { get; set; } = "dlx";

    /// <summary>
    /// Gets or sets the dead letter queue name
    /// </summary>
    public string DeadLetterQueue { get; set; } = "dlq";

    /// <summary>
    /// Gets or sets the dead letter routing key
    /// </summary>
    public string DeadLetterRoutingKey { get; set; } = "dead";

    /// <summary>
    /// Gets or sets whether dead letter handling is enabled
    /// </summary>
    public bool Enabled { get; set; } = true;

    /// <summary>
    /// Gets or sets the maximum number of retry attempts before sending to dead letter
    /// </summary>
    public int MaxRetryAttempts { get; set; } = 3;

    /// <summary>
    /// Gets or sets the time to live for messages in the dead letter queue
    /// </summary>
    public TimeSpan? MessageTtl { get; set; }

    /// <summary>
    /// Gets or sets whether to automatically requeue messages from dead letter
    /// </summary>
    public bool AutoRequeue { get; set; } = false;

    /// <summary>
    /// Gets or sets the interval for automatic requeue attempts
    /// </summary>
    public TimeSpan AutoRequeueInterval { get; set; } = TimeSpan.FromHours(1);

    /// <summary>
    /// Gets or sets the maximum number of requeue attempts
    /// </summary>
    public int MaxRequeueAttempts { get; set; } = 5;

    /// <summary>
    /// Gets or sets whether to include original message headers in dead letter
    /// </summary>
    public bool IncludeOriginalHeaders { get; set; } = true;

    /// <summary>
    /// Gets or sets whether to include stack trace in dead letter
    /// </summary>
    public bool IncludeStackTrace { get; set; } = true;

    /// <summary>
    /// Gets or sets custom headers to add to dead letter messages
    /// </summary>
    public Dictionary<string, object> CustomHeaders { get; set; } = new();
} 