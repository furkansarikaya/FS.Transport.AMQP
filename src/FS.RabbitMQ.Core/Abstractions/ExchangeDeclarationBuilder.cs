namespace FS.RabbitMQ.Core.Abstractions;

/// <summary>
/// Provides a fluent API for building ExchangeDeclaration instances with method chaining.
/// </summary>
/// <remarks>
/// The builder pattern is particularly useful for exchange declarations because:
/// 1. It provides clear, readable configuration code
/// 2. It prevents invalid intermediate states during construction
/// 3. It allows for conditional configuration based on runtime conditions
/// 4. It centralizes validation logic in a single place
/// 
/// Example usage:
/// <code>
/// var exchange = ExchangeDeclaration
///     .Create("order.events", ExchangeType.Topic)
///     .AsDurable()
///     .WithArgument("x-alternate-exchange", "failed.orders")
///     .Build();
/// </code>
/// </remarks>
public sealed class ExchangeDeclarationBuilder
{
    private readonly string _name;
    private readonly ExchangeType _type;
    private bool _durable = true;
    private bool _autoDelete = false;
    private bool _internal = false;
    private Dictionary<string, object>? _arguments;

    /// <summary>
    /// Initializes a new instance of the ExchangeDeclarationBuilder class.
    /// </summary>
    /// <param name="name">The name of the exchange to declare.</param>
    /// <param name="type">The type of exchange that determines routing behavior.</param>
    /// <remarks>
    /// This constructor is internal to ensure that instances are created only through
    /// the ExchangeDeclaration.Create() factory method, which provides input validation.
    /// </remarks>
    internal ExchangeDeclarationBuilder(string name, ExchangeType type)
    {
        _name = name;
        _type = type;
    }

    /// <summary>
    /// Configures the exchange to be durable, surviving broker restarts.
    /// </summary>
    /// <returns>The current builder instance for method chaining.</returns>
    /// <remarks>
    /// Durable exchanges are persisted to disk and automatically recreated when the broker restarts.
    /// This is the recommended setting for production environments where message delivery
    /// guarantees are important. Durable exchanges have slightly higher declaration overhead
    /// but provide significantly better reliability guarantees.
    /// </remarks>
    public ExchangeDeclarationBuilder AsDurable()
    {
        _durable = true;
        return this;
    }

    /// <summary>
    /// Configures the exchange to be transient, not surviving broker restarts.
    /// </summary>
    /// <returns>The current builder instance for method chaining.</returns>
    /// <remarks>
    /// Transient exchanges exist only in memory and are faster to declare and use.
    /// They are suitable for:
    /// - Development and testing environments
    /// - Temporary exchanges for short-lived operations
    /// - High-performance scenarios where persistence is not required
    /// 
    /// Use with caution in production environments as broker restarts will require
    /// manual recreation of the exchange and its bindings.
    /// </remarks>
    public ExchangeDeclarationBuilder AsTransient()
    {
        _durable = false;
        return this;
    }

    /// <summary>
    /// Configures the exchange to be automatically deleted when no longer in use.
    /// </summary>
    /// <returns>The current builder instance for method chaining.</returns>
    /// <remarks>
    /// Auto-delete exchanges are automatically removed when:
    /// - No queues are bound to the exchange
    /// - No other exchanges are bound to this exchange (for exchange-to-exchange bindings)
    /// 
    /// This is useful for:
    /// - Temporary exchanges in dynamic scenarios
    /// - Resource cleanup in applications with varying topology requirements
    /// 
    /// Warning: In distributed systems, temporary disconnections might trigger
    /// premature deletion. Consider the implications carefully in production environments.
    /// </remarks>
    public ExchangeDeclarationBuilder WithAutoDelete()
    {
        _autoDelete = true;
        return this;
    }

    /// <summary>
    /// Configures the exchange to persist even when not in use.
    /// </summary>
    /// <returns>The current builder instance for method chaining.</returns>
    /// <remarks>
    /// Persistent exchanges remain available even when no queues are bound to them.
    /// This is the default and recommended behavior for most production scenarios
    /// as it prevents accidental topology changes due to temporary disconnections
    /// or application restarts.
    /// </remarks>
    public ExchangeDeclarationBuilder WithoutAutoDelete()
    {
        _autoDelete = false;
        return this;
    }

    /// <summary>
    /// Configures the exchange as internal, preventing direct message publishing by clients.
    /// </summary>
    /// <returns>The current builder instance for method chaining.</returns>
    /// <remarks>
    /// Internal exchanges can only receive messages from other exchanges through
    /// exchange-to-exchange bindings. They are typically used for:
    /// - Advanced routing topologies
    /// - Message transformation pipelines
    /// - Broker plugin integrations
    /// - Creating alternate exchange hierarchies
    /// 
    /// Client applications cannot publish directly to internal exchanges,
    /// making them useful for controlled message flow scenarios.
    /// </remarks>
    public ExchangeDeclarationBuilder AsInternal()
    {
        _internal = true;
        return this;
    }

    /// <summary>
    /// Configures the exchange as external, allowing direct message publishing by clients.
    /// </summary>
    /// <returns>The current builder instance for method chaining.</returns>
    /// <remarks>
    /// External exchanges accept messages directly from client applications.
    /// This is the default behavior and is suitable for most standard messaging scenarios.
    /// </remarks>
    public ExchangeDeclarationBuilder AsExternal()
    {
        _internal = false;
        return this;
    }

    /// <summary>
    /// Adds a custom argument to the exchange declaration.
    /// </summary>
    /// <param name="key">The argument key. Cannot be null or whitespace.</param>
    /// <param name="value">The argument value. Cannot be null.</param>
    /// <returns>The current builder instance for method chaining.</returns>
    /// <exception cref="ArgumentException">Thrown when <paramref name="key"/> is null, empty, or whitespace.</exception>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="value"/> is null.</exception>
    /// <remarks>
    /// Exchange arguments enable advanced features and broker-specific functionality:
    /// 
    /// Common arguments include:
    /// - "x-alternate-exchange": Specifies an alternate exchange for unroutable messages
    /// - "x-message-ttl": Sets default message time-to-live in milliseconds
    /// - "x-max-length": Limits the maximum number of messages in bound queues
    /// - "x-delayed-type": Enables delayed message functionality (with rabbitmq-delayed-message-exchange plugin)
    /// 
    /// Arguments are broker-specific and may not be portable across different RabbitMQ configurations.
    /// Always verify argument support in your target environment.
    /// </remarks>
    public ExchangeDeclarationBuilder WithArgument(string key, object value)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(key);
        ArgumentNullException.ThrowIfNull(value);

        _arguments ??= new Dictionary<string, object>();
        _arguments[key] = value;
        return this;
    }

    /// <summary>
    /// Adds multiple custom arguments to the exchange declaration.
    /// </summary>
    /// <param name="arguments">A dictionary containing argument key-value pairs. Cannot be null.</param>
    /// <returns>The current builder instance for method chaining.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="arguments"/> is null.</exception>
    /// <exception cref="ArgumentException">Thrown when any argument key is null, empty, or whitespace, or when any value is null.</exception>
    /// <remarks>
    /// This method allows bulk addition of arguments, which is convenient when:
    /// - Configuring multiple related arguments
    /// - Loading configuration from external sources
    /// - Applying argument templates to multiple exchanges
    /// 
    /// Duplicate keys will overwrite existing values. The method validates all
    /// key-value pairs before applying any changes, ensuring atomic operation.
    /// </remarks>
    public ExchangeDeclarationBuilder WithArguments(IReadOnlyDictionary<string, object> arguments)
    {
        ArgumentNullException.ThrowIfNull(arguments);

        // Validate all arguments before applying any changes
        foreach (var kvp in arguments)
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(kvp.Key);
            ArgumentNullException.ThrowIfNull(kvp.Value);
        }

        _arguments ??= new Dictionary<string, object>();

        foreach (var kvp in arguments)
        {
            _arguments[kvp.Key] = kvp.Value;
        }

        return this;
    }

    /// <summary>
    /// Configures an alternate exchange for handling unroutable messages.
    /// </summary>
    /// <param name="alternateExchangeName">The name of the alternate exchange. Cannot be null or whitespace.</param>
    /// <returns>The current builder instance for method chaining.</returns>
    /// <exception cref="ArgumentException">Thrown when <paramref name="alternateExchangeName"/> is null, empty, or whitespace.</exception>
    /// <remarks>
    /// Alternate exchanges provide a mechanism for handling messages that cannot be routed
    /// to any queue. When a message published to this exchange cannot be routed:
    /// 1. If an alternate exchange is configured, the message is republished to that exchange
    /// 2. If no alternate exchange is configured, the message is dropped or returned to the publisher
    /// 
    /// This feature is particularly useful for:
    /// - Dead letter handling for unroutable messages
    /// - Logging and monitoring of routing failures
    /// - Implementing fallback routing strategies
    /// 
    /// The alternate exchange must exist before declaring this exchange, or the declaration will fail.
    /// </remarks>
    public ExchangeDeclarationBuilder WithAlternateExchange(string alternateExchangeName)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(alternateExchangeName);
        return WithArgument("x-alternate-exchange", alternateExchangeName);
    }

    /// <summary>
    /// Builds and returns the configured ExchangeDeclaration instance.
    /// </summary>
    /// <returns>A new ExchangeDeclaration instance with the specified configuration.</returns>
    /// <exception cref="InvalidOperationException">Thrown when the current configuration is invalid or incomplete.</exception>
    /// <remarks>
    /// This method performs final validation of the exchange configuration and creates
    /// an immutable ExchangeDeclaration instance. The validation includes:
    /// - Exchange name format and reservation rules
    /// - Argument consistency and format
    /// - Configuration compatibility checks
    /// 
    /// Once built, the ExchangeDeclaration cannot be modified, ensuring thread safety
    /// and preventing accidental configuration changes during use.
    /// </remarks>
    public ExchangeDeclaration Build()
    {
        var declaration = new ExchangeDeclaration
        {
            Name = _name,
            Type = _type,
            Durable = _durable,
            AutoDelete = _autoDelete,
            Internal = _internal,
            Arguments = _arguments?.AsReadOnly()
        };

        // Perform validation before returning the instance
        declaration.Validate();

        return declaration;
    }
}