using FS.RabbitMQ.Configuration;

namespace FS.RabbitMQ.Exchange;

/// <summary>
/// Fluent builder for exchange settings
/// </summary>
public class ExchangeSettingsBuilder
{
    private readonly ExchangeSettings _settings;

    internal ExchangeSettingsBuilder(string name)
    {
        _settings = new ExchangeSettings { Name = name };
    }

    /// <summary>
    /// Sets the exchange type
    /// </summary>
    /// <param name="type">Exchange type</param>
    /// <returns>Builder for fluent configuration</returns>
    public ExchangeSettingsBuilder WithType(string type)
    {
        _settings.Type = type;
        return this;
    }

    /// <summary>
    /// Sets whether the exchange is durable
    /// </summary>
    /// <param name="durable">Durable flag</param>
    /// <returns>Builder for fluent configuration</returns>
    public ExchangeSettingsBuilder WithDurable(bool durable = true)
    {
        _settings.Durable = durable;
        return this;
    }

    /// <summary>
    /// Sets whether the exchange auto-deletes
    /// </summary>
    /// <param name="autoDelete">Auto-delete flag</param>
    /// <returns>Builder for fluent configuration</returns>
    public ExchangeSettingsBuilder WithAutoDelete(bool autoDelete = true)
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
    public ExchangeSettingsBuilder WithArgument(string key, object value)
    {
        _settings.Arguments[key] = value;
        return this;
    }

    /// <summary>
    /// Adds a binding to another exchange
    /// </summary>
    /// <param name="destinationExchange">Destination exchange name</param>
    /// <param name="routingKey">Routing key for binding</param>
    /// <returns>Builder for fluent configuration</returns>
    public ExchangeSettingsBuilder WithBinding(string destinationExchange, string routingKey)
    {
        _settings.Bindings.Add(new ExchangeBinding
        {
            DestinationExchange = destinationExchange,
            RoutingKey = routingKey
        });
        return this;
    }

    /// <summary>
    /// Builds the exchange settings
    /// </summary>
    /// <returns>Configured exchange settings</returns>
    public ExchangeSettings Build()
    {
        _settings.Validate();
        return _settings;
    }
}