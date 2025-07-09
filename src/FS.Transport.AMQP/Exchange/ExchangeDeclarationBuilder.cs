using RabbitMQ.Client;

namespace FS.Transport.AMQP.Exchange;

/// <summary>
/// Fluent builder for exchange declaration
/// </summary>
public class ExchangeDeclarationBuilder
{
    private readonly IExchangeManager _exchangeManager;
    private readonly string _name;
    private string _type = ExchangeType.Topic;
    private bool _durable = true;
    private bool _autoDelete = false;
    private readonly Dictionary<string, object> _arguments = new();

    internal ExchangeDeclarationBuilder(IExchangeManager exchangeManager, string name)
    {
        _exchangeManager = exchangeManager ?? throw new ArgumentNullException(nameof(exchangeManager));
        _name = name ?? throw new ArgumentNullException(nameof(name));
    }

    /// <summary>
    /// Sets the exchange type
    /// </summary>
    /// <param name="type">Exchange type</param>
    /// <returns>Builder for fluent configuration</returns>
    public ExchangeDeclarationBuilder WithType(string type)
    {
        _type = type;
        return this;
    }

    /// <summary>
    /// Sets durability
    /// </summary>
    /// <param name="durable">Durable flag</param>
    /// <returns>Builder for fluent configuration</returns>
    public ExchangeDeclarationBuilder WithDurable(bool durable = true)
    {
        _durable = durable;
        return this;
    }

    /// <summary>
    /// Sets auto-delete behavior
    /// </summary>
    /// <param name="autoDelete">Auto-delete flag</param>
    /// <returns>Builder for fluent configuration</returns>
    public ExchangeDeclarationBuilder WithAutoDelete(bool autoDelete = true)
    {
        _autoDelete = autoDelete;
        return this;
    }

    /// <summary>
    /// Adds an argument
    /// </summary>
    /// <param name="key">Argument key</param>
    /// <param name="value">Argument value</param>
    /// <returns>Builder for fluent configuration</returns>
    public ExchangeDeclarationBuilder WithArgument(string key, object value)
    {
        _arguments[key] = value;
        return this;
    }

    /// <summary>
    /// Declares the exchange asynchronously
    /// </summary>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>True if declaration was successful</returns>
    public async Task<bool> DeclareAsync(CancellationToken cancellationToken = default)
    {
        return await _exchangeManager.DeclareAsync(_name, _type, _durable, _autoDelete, _arguments, cancellationToken);
    }

    /// <summary>
    /// Declares the exchange synchronously
    /// </summary>
    /// <returns>True if declaration was successful</returns>
    public bool Declare()
    {
        return DeclareAsync().GetAwaiter().GetResult();
    }
}