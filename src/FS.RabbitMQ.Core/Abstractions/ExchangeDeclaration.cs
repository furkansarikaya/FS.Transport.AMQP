namespace FS.RabbitMQ.Core.Abstractions;

/// <summary>
/// Contains configuration parameters for declaring an exchange in RabbitMQ.
/// </summary>
/// <remarks>
/// Exchange declaration is idempotent in RabbitMQ - if an exchange already exists with the same configuration,
/// the operation succeeds without modification. However, if configuration parameters differ,
/// a channel exception will be raised.
/// 
/// The Builder pattern is recommended for creating instances of this class to ensure
/// required parameters are provided and optional parameters have sensible defaults.
/// </remarks>
public sealed class ExchangeDeclaration
{
    /// <summary>
    /// Gets the name of the exchange to declare.
    /// </summary>
    /// <value>A non-empty string representing the exchange name. Cannot be null or whitespace.</value>
    public required string Name { get; init; }

    /// <summary>
    /// Gets the type of the exchange which determines message routing behavior.
    /// </summary>
    /// <value>The exchange type that defines how messages are routed to bound queues.</value>
    /// <remarks>
    /// Common exchange types:
    /// - Direct: Routes messages to queues whose binding key exactly matches the message routing key
    /// - Fanout: Routes messages to all bound queues, ignoring routing keys
    /// - Topic: Routes messages to queues based on wildcard matching between routing key and binding pattern
    /// - Headers: Routes messages based on header attributes instead of routing key
    /// </remarks>
    public required ExchangeType Type { get; init; }

    /// <summary>
    /// Gets a value indicating whether the exchange should survive broker restarts.
    /// </summary>
    /// <value>
    /// <c>true</c> if the exchange should be persisted to disk and survive broker restarts;
    /// <c>false</c> if the exchange should be deleted when the broker restarts.
    /// </value>
    /// <remarks>
    /// Durable exchanges are stored on disk and recreated during broker startup.
    /// Non-durable exchanges exist only in memory and are lost during broker restarts.
    /// Default value is <c>true</c> for production stability.
    /// </remarks>
    public bool Durable { get; init; } = true;

    /// <summary>
    /// Gets a value indicating whether the exchange should be deleted when no longer in use.
    /// </summary>
    /// <value>
    /// <c>true</c> if the exchange should be automatically deleted when no queues are bound to it;
    /// <c>false</c> if the exchange should persist even when unused.
    /// </value>
    /// <remarks>
    /// Auto-delete behavior is useful for temporary exchanges that should clean up automatically.
    /// Be cautious with this setting in production environments where exchanges may temporarily have no bindings.
    /// Default value is <c>false</c> to prevent accidental deletion.
    /// </remarks>
    public bool AutoDelete { get; init; } = false;

    /// <summary>
    /// Gets a value indicating whether the exchange is for internal use by the broker.
    /// </summary>
    /// <value>
    /// <c>true</c> if the exchange is internal and cannot be used by client applications for publishing;
    /// <c>false</c> if the exchange can be used for normal message publishing.
    /// </value>
    /// <remarks>
    /// Internal exchanges are typically used for broker plugins and advanced routing scenarios.
    /// Client applications cannot publish directly to internal exchanges.
    /// Default value is <c>false</c> for normal application usage.
    /// </remarks>
    public bool Internal { get; init; } = false;

    /// <summary>
    /// Gets additional arguments for exchange declaration.
    /// </summary>
    /// <value>A dictionary containing optional exchange arguments, or <c>null</c> if no additional arguments are needed.</value>
    /// <remarks>
    /// Exchange arguments are used for advanced features like:
    /// - Alternate exchange configuration (x-alternate-exchange)
    /// - Message TTL settings (x-message-ttl)
    /// - Plugin-specific configuration parameters
    /// 
    /// Arguments are broker-specific and may not be portable across different RabbitMQ deployments.
    /// </remarks>
    public IReadOnlyDictionary<string, object>? Arguments { get; init; }

    /// <summary>
    /// Creates a new builder instance for constructing ExchangeDeclaration objects.
    /// </summary>
    /// <param name="name">The name of the exchange to declare.</param>
    /// <param name="type">The type of exchange that determines routing behavior.</param>
    /// <returns>A new ExchangeDeclarationBuilder instance for fluent configuration.</returns>
    /// <exception cref="ArgumentException">Thrown when <paramref name="name"/> is null, empty, or contains only whitespace.</exception>
    public static ExchangeDeclarationBuilder Create(string name, ExchangeType type)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(name);
        return new ExchangeDeclarationBuilder(name, type);
    }

    /// <summary>
    /// Validates the exchange declaration configuration for consistency and completeness.
    /// </summary>
    /// <exception cref="InvalidOperationException">Thrown when the configuration contains invalid or conflicting settings.</exception>
    internal void Validate()
    {
        if (string.IsNullOrWhiteSpace(Name))
        {
            throw new InvalidOperationException("Exchange name cannot be null or empty.");
        }

        // Exchange names starting with "amq." are reserved for broker use
        if (Name.StartsWith("amq.", StringComparison.OrdinalIgnoreCase) && !Internal)
        {
            throw new InvalidOperationException($"Exchange name '{Name}' is reserved. Use Internal=true for amq.* exchanges or choose a different name.");
        }

        // Validate arguments if present
        if (Arguments?.Any() != true) return;
        if (Arguments.Any(kvp => string.IsNullOrWhiteSpace(kvp.Key)))
        {
            throw new InvalidOperationException("Exchange argument keys cannot be null or empty.");
        }
    }
}
