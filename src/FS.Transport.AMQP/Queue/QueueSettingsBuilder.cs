using FS.Transport.AMQP.Configuration;

namespace FS.Transport.AMQP.Queue;

/// <summary>
/// Fluent builder for queue settings
/// </summary>
public class QueueSettingsBuilder
{
    private readonly QueueSettings _settings;

    internal QueueSettingsBuilder(string name)
    {
        _settings = new QueueSettings { Name = name };
    }

    /// <summary>
    /// Sets whether the queue is durable
    /// </summary>
    /// <param name="durable">Durable flag</param>
    /// <returns>Builder for fluent configuration</returns>
    public QueueSettingsBuilder WithDurable(bool durable = true)
    {
        _settings.Durable = durable;
        return this;
    }

    /// <summary>
    /// Sets whether the queue is exclusive
    /// </summary>
    /// <param name="exclusive">Exclusive flag</param>
    /// <returns>Builder for fluent configuration</returns>
    public QueueSettingsBuilder WithExclusive(bool exclusive = true)
    {
        _settings.Exclusive = exclusive;
        return this;
    }

    /// <summary>
    /// Sets whether the queue auto-deletes
    /// </summary>
    /// <param name="autoDelete">Auto-delete flag</param>
    /// <returns>Builder for fluent configuration</returns>
    public QueueSettingsBuilder WithAutoDelete(bool autoDelete = true)
    {
        _settings.AutoDelete = autoDelete;
        return this;
    }

    /// <summary>
    /// Adds an argument
    /// </summary>
    /// <param name="key">Argument key</param>
    /// <param name="value">Argument value</param>
    /// <returns>Builder for fluent configuration</returns>
    public QueueSettingsBuilder WithArgument(string key, object value)
    {
        _settings.Arguments[key] = value;
        return this;
    }

    /// <summary>
    /// Adds multiple arguments
    /// </summary>
    /// <param name="arguments">Arguments dictionary</param>
    /// <returns>Builder for fluent configuration</returns>
    public QueueSettingsBuilder WithArguments(IDictionary<string, object> arguments)
    {
        foreach (var kvp in arguments)
        {
            _settings.Arguments[kvp.Key] = kvp.Value;
        }
        return this;
    }

    /// <summary>
    /// Adds a binding to an exchange
    /// </summary>
    /// <param name="exchangeName">Exchange name</param>
    /// <param name="routingKey">Routing key for binding</param>
    /// <returns>Builder for fluent configuration</returns>
    public QueueSettingsBuilder WithBinding(string exchangeName, string routingKey)
    {
        _settings.Bindings.Add(new QueueBinding
        {
            Exchange = exchangeName,
            RoutingKey = routingKey
        });
        return this;
    }

    /// <summary>
    /// Sets message TTL
    /// </summary>
    /// <param name="ttl">Time to live</param>
    /// <returns>Builder for fluent configuration</returns>
    public QueueSettingsBuilder WithTtl(TimeSpan ttl)
    {
        return WithArgument("x-message-ttl", (int)ttl.TotalMilliseconds);
    }

    /// <summary>
    /// Sets maximum queue length
    /// </summary>
    /// <param name="maxLength">Maximum number of messages</param>
    /// <returns>Builder for fluent configuration</returns>
    public QueueSettingsBuilder WithMaxLength(long maxLength)
    {
        return WithArgument("x-max-length", maxLength);
    }

    /// <summary>
    /// Sets maximum priority
    /// </summary>
    /// <param name="maxPriority">Maximum priority level</param>
    /// <returns>Builder for fluent configuration</returns>
    public QueueSettingsBuilder WithPriority(int maxPriority)
    {
        return WithArgument("x-max-priority", maxPriority);
    }

    /// <summary>
    /// Configures as quorum queue
    /// </summary>
    /// <returns>Builder for fluent configuration</returns>
    public QueueSettingsBuilder AsQuorumQueue()
    {
        return WithArgument("x-queue-type", "quorum");
    }

    /// <summary>
    /// Configures dead letter settings
    /// </summary>
    /// <param name="deadLetterExchange">Dead letter exchange</param>
    /// <param name="deadLetterRoutingKey">Dead letter routing key</param>
    /// <returns>Builder for fluent configuration</returns>
    public QueueSettingsBuilder WithDeadLetter(string deadLetterExchange, string? deadLetterRoutingKey = null)
    {
        WithArgument("x-dead-letter-exchange", deadLetterExchange);
        if (!string.IsNullOrEmpty(deadLetterRoutingKey))
        {
            WithArgument("x-dead-letter-routing-key", deadLetterRoutingKey);
        }
        return this;
    }

    /// <summary>
    /// Builds the queue settings
    /// </summary>
    /// <returns>Configured queue settings</returns>
    public QueueSettings Build()
    {
        _settings.Validate();
        return _settings;
    }
}