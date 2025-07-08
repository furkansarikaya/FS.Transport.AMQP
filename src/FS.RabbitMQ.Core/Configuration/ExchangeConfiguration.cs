using FS.RabbitMQ.Core.Abstractions;

namespace FS.RabbitMQ.Core.Configuration;

/// <summary>
/// Contains configuration for exchange declaration and properties.
/// </summary>
public sealed class ExchangeConfiguration
{
    /// <summary>
    /// Gets or sets the default exchange type.
    /// </summary>
    /// <value>The default type for exchange creation. Default is Direct.</value>
    public ExchangeType DefaultType { get; set; } = ExchangeType.Direct;

    /// <summary>
    /// Gets or sets a value indicating whether exchanges should be durable by default.
    /// </summary>
    /// <value><c>true</c> to create durable exchanges; otherwise, <c>false</c>. Default is <c>true</c>.</value>
    public bool DefaultDurable { get; set; } = true;

    /// <summary>
    /// Gets or sets a value indicating whether exchanges should auto-delete by default.
    /// </summary>
    /// <value><c>true</c> to create auto-delete exchanges; otherwise, <c>false</c>. Default is <c>false</c>.</value>
    public bool DefaultAutoDelete { get; set; } = false;

    /// <summary>
    /// Gets or sets default exchange arguments.
    /// </summary>
    /// <value>A dictionary of default arguments for exchange creation.</value>
    public IDictionary<string, object>? DefaultArguments { get; set; }

    /// <summary>
    /// Creates an exchange configuration suitable for development environments.
    /// </summary>
    public static ExchangeConfiguration CreateDevelopment() =>
        new() { DefaultDurable = false, DefaultAutoDelete = true };

    /// <summary>
    /// Creates an exchange configuration suitable for production environments.
    /// </summary>
    public static ExchangeConfiguration CreateProduction() =>
        new() { DefaultDurable = true, DefaultAutoDelete = false };

    /// <summary>
    /// Creates a default exchange configuration.
    /// </summary>
    public static ExchangeConfiguration CreateDefault() => new();

    /// <summary>
    /// Validates the exchange configuration.
    /// </summary>
    public void Validate() { }
}
