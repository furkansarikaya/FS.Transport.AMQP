namespace FS.RabbitMQ.Core.Abstractions;

/// <summary>
/// Contains configuration parameters for binding a queue to an exchange in RabbitMQ.
/// </summary>
/// <remarks>
/// Queue bindings establish the routing relationship between exchanges and queues,
/// determining which messages are delivered to which queues based on routing rules.
/// The binding configuration varies significantly based on the exchange type:
/// 
/// - Direct exchanges: Routing key must exactly match the binding key
/// - Fanout exchanges: Routing key is ignored, all bound queues receive messages
/// - Topic exchanges: Routing key is matched against binding patterns with wildcards
/// - Headers exchanges: Routing is based on message headers rather than routing key
/// 
/// Bindings are lightweight and can be created/deleted dynamically to implement
/// sophisticated routing topologies and runtime message flow control.
/// </remarks>
public sealed class QueueBinding
{
    /// <summary>
    /// Gets the name of the queue to bind to the exchange.
    /// </summary>
    /// <value>A non-empty string representing the queue name. Cannot be null or whitespace.</value>
    /// <remarks>
    /// The queue must exist before attempting to create a binding. If the queue
    /// was declared with a server-generated name, use the actual name returned
    /// from the queue declaration operation.
    /// 
    /// Queue names are case-sensitive and must match exactly with the existing queue.
    /// Binding operations will fail if the specified queue does not exist.
    /// </remarks>
    public required string QueueName { get; init; }

    /// <summary>
    /// Gets the name of the exchange to bind the queue to.
    /// </summary>
    /// <value>A non-empty string representing the exchange name. Cannot be null or whitespace.</value>
    /// <remarks>
    /// The exchange must exist before attempting to create a binding. Internal exchanges
    /// cannot be used for client message publishing but can have bindings for routing
    /// between exchanges.
    /// 
    /// Exchange names are case-sensitive and must match exactly with the existing exchange.
    /// The default exchange (empty string) can be used for direct queue access patterns.
    /// </remarks>
    public required string ExchangeName { get; init; }

    /// <summary>
    /// Gets the routing key pattern used for message routing decisions.
    /// </summary>
    /// <value>
    /// A string representing the routing key or pattern. Can be empty for fanout exchanges.
    /// Cannot be null, but empty strings are valid for certain exchange types.
    /// </value>
    /// <remarks>
    /// Routing key interpretation depends on the exchange type:
    /// 
    /// Direct exchanges:
    /// - Must exactly match the message routing key for delivery
    /// - Case-sensitive string comparison
    /// - Empty routing key matches only messages with empty routing key
    /// 
    /// Fanout exchanges:
    /// - Routing key is completely ignored
    /// - All bound queues receive copies of every message
    /// - Convention is to use empty string, but any value is acceptable
    /// 
    /// Topic exchanges:
    /// - Supports wildcard patterns: * (single word) and # (zero or more words)
    /// - Words are separated by dots (.)
    /// - Examples: "order.*", "*.important", "user.#", "audit.order.#"
    /// 
    /// Headers exchanges:
    /// - Routing key is ignored in favor of message headers
    /// - Use Arguments property to specify header matching criteria
    /// </remarks>
    public required string RoutingKey { get; init; }

    /// <summary>
    /// Gets additional arguments for the binding operation.
    /// </summary>
    /// <value>
    /// A dictionary containing optional binding arguments, or <c>null</c> if no additional arguments are needed.
    /// </value>
    /// <remarks>
    /// Binding arguments are primarily used with headers exchanges to specify
    /// message header matching criteria:
    /// 
    /// Headers exchange arguments:
    /// - "x-match": "any" or "all" - determines if any or all headers must match
    /// - Header key-value pairs: specify which headers and values to match against
    /// - Example: {"x-match": "all", "format": "pdf", "priority": "high"}
    /// 
    /// Other exchange types typically ignore binding arguments, but some
    /// RabbitMQ plugins may define custom argument behaviors.
    /// 
    /// Arguments are case-sensitive and must match the expected data types
    /// for proper routing behavior.
    /// </remarks>
    public IReadOnlyDictionary<string, object>? Arguments { get; init; }

    /// <summary>
    /// Creates a new builder instance for constructing QueueBinding objects.
    /// </summary>
    /// <param name="queueName">The name of the queue to bind. Cannot be null or whitespace.</param>
    /// <param name="exchangeName">The name of the exchange to bind to. Cannot be null or whitespace.</param>
    /// <param name="routingKey">The routing key or pattern for the binding. Cannot be null but can be empty.</param>
    /// <returns>A new QueueBindingBuilder instance for fluent configuration.</returns>
    /// <exception cref="ArgumentException">
    /// Thrown when <paramref name="queueName"/> or <paramref name="exchangeName"/> is null, empty, or whitespace.
    /// </exception>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="routingKey"/> is null.</exception>
    /// <remarks>
    /// The builder pattern is recommended for complex bindings, especially when working
    /// with headers exchanges that require multiple arguments, or when creating multiple
    /// similar bindings with slight variations.
    /// </remarks>
    public static QueueBindingBuilder Create(string queueName, string exchangeName, string routingKey)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(queueName);
        ArgumentException.ThrowIfNullOrWhiteSpace(exchangeName);
        ArgumentNullException.ThrowIfNull(routingKey);

        return new QueueBindingBuilder(queueName, exchangeName, routingKey);
    }

    /// <summary>
    /// Creates a simple queue binding for direct exchange routing.
    /// </summary>
    /// <param name="queueName">The name of the queue to bind. Cannot be null or whitespace.</param>
    /// <param name="exchangeName">The name of the direct exchange. Cannot be null or whitespace.</param>
    /// <param name="routingKey">The exact routing key for message matching. Cannot be null.</param>
    /// <returns>A configured QueueBinding instance for direct exchange routing.</returns>
    /// <exception cref="ArgumentException">
    /// Thrown when <paramref name="queueName"/> or <paramref name="exchangeName"/> is null, empty, or whitespace.
    /// </exception>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="routingKey"/> is null.</exception>
    /// <remarks>
    /// This factory method provides a convenient way to create simple bindings
    /// for direct exchanges without needing the builder pattern. Direct exchange
    /// bindings are the most common type and typically don't require additional arguments.
    /// </remarks>
    public static QueueBinding CreateDirect(string queueName, string exchangeName, string routingKey)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(queueName);
        ArgumentException.ThrowIfNullOrWhiteSpace(exchangeName);
        ArgumentNullException.ThrowIfNull(routingKey);

        return new QueueBinding
        {
            QueueName = queueName,
            ExchangeName = exchangeName,
            RoutingKey = routingKey,
            Arguments = null
        };
    }

    /// <summary>
    /// Creates a queue binding for fanout exchange routing.
    /// </summary>
    /// <param name="queueName">The name of the queue to bind. Cannot be null or whitespace.</param>
    /// <param name="exchangeName">The name of the fanout exchange. Cannot be null or whitespace.</param>
    /// <returns>A configured QueueBinding instance for fanout exchange routing.</returns>
    /// <exception cref="ArgumentException">
    /// Thrown when <paramref name="queueName"/> or <paramref name="exchangeName"/> is null, empty, or whitespace.
    /// </exception>
    /// <remarks>
    /// Fanout exchange bindings ignore the routing key, so this method uses an empty
    /// routing key by convention. All queues bound to a fanout exchange receive
    /// copies of every message published to that exchange.
    /// 
    /// This pattern is ideal for:
    /// - Broadcasting notifications to multiple services
    /// - Fan-out processing where multiple workers need the same data
    /// - Event sourcing scenarios with multiple read model updates
    /// </remarks>
    public static QueueBinding CreateFanout(string queueName, string exchangeName)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(queueName);
        ArgumentException.ThrowIfNullOrWhiteSpace(exchangeName);

        return new QueueBinding
        {
            QueueName = queueName,
            ExchangeName = exchangeName,
            RoutingKey = string.Empty,
            Arguments = null
        };
    }

    /// <summary>
    /// Creates a queue binding for topic exchange routing with pattern matching.
    /// </summary>
    /// <param name="queueName">The name of the queue to bind. Cannot be null or whitespace.</param>
    /// <param name="exchangeName">The name of the topic exchange. Cannot be null or whitespace.</param>
    /// <param name="pattern">The routing pattern with wildcards. Cannot be null.</param>
    /// <returns>A configured QueueBinding instance for topic exchange routing.</returns>
    /// <exception cref="ArgumentException">
    /// Thrown when <paramref name="queueName"/> or <paramref name="exchangeName"/> is null, empty, or whitespace.
    /// </exception>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="pattern"/> is null.</exception>
    /// <remarks>
    /// Topic exchange patterns support two wildcards:
    /// - * (asterisk): matches exactly one word
    /// - # (hash): matches zero or more words
    /// - Words are separated by dots (.)
    /// 
    /// Example patterns:
    /// - "order.*": matches "order.created", "order.updated", but not "order.item.added"
    /// - "order.#": matches "order.created", "order.item.added", "order.payment.processed"
    /// - "*.critical": matches "system.critical", "app.critical", but not "user.high.critical"
    /// 
    /// Topic exchanges are powerful for implementing hierarchical message routing
    /// and selective subscription patterns.
    /// </remarks>
    public static QueueBinding CreateTopic(string queueName, string exchangeName, string pattern)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(queueName);
        ArgumentException.ThrowIfNullOrWhiteSpace(exchangeName);
        ArgumentNullException.ThrowIfNull(pattern);

        return new QueueBinding
        {
            QueueName = queueName,
            ExchangeName = exchangeName,
            RoutingKey = pattern,
            Arguments = null
        };
    }

    /// <summary>
    /// Validates the queue binding configuration for consistency and completeness.
    /// </summary>
    /// <exception cref="InvalidOperationException">
    /// Thrown when the configuration contains invalid or conflicting settings.
    /// </exception>
    /// <remarks>
    /// Validation includes:
    /// - Queue and exchange name format validation
    /// - Routing key pattern syntax for topic exchanges (if determinable)
    /// - Argument format and consistency checks
    /// - Headers exchange argument validation
    /// 
    /// This validation is performed automatically during binding operations
    /// to prevent runtime errors due to configuration mistakes.
    /// </remarks>
    internal void Validate()
    {
        if (string.IsNullOrWhiteSpace(QueueName))
        {
            throw new InvalidOperationException("Queue name cannot be null or empty.");
        }

        if (string.IsNullOrWhiteSpace(ExchangeName))
        {
            throw new InvalidOperationException("Exchange name cannot be null or empty.");
        }

        // Routing key can be empty (valid for fanout exchanges) but not null
        if (RoutingKey is null)
        {
            throw new InvalidOperationException("Routing key cannot be null. Use empty string for fanout exchanges.");
        }

        // Validate arguments if present
        if (Arguments?.Any() == true)
        {
            foreach (var kvp in Arguments)
            {
                if (string.IsNullOrWhiteSpace(kvp.Key))
                {
                    throw new InvalidOperationException("Binding argument keys cannot be null or empty.");
                }

                // Validate headers exchange arguments
                if (kvp.Key == "x-match")
                {
                    if (kvp.Value is not string matchValue || (matchValue != "any" && matchValue != "all"))
                    {
                        throw new InvalidOperationException("Headers exchange 'x-match' argument must be 'any' or 'all'.");
                    }
                }
            }

            // If x-match is present, ensure there are other header arguments
            if (Arguments.ContainsKey("x-match") && Arguments.Count == 1)
            {
                throw new InvalidOperationException("Headers exchange bindings with 'x-match' must include header criteria arguments.");
            }
        }
    }

    /// <summary>
    /// Returns a string representation of the queue binding configuration.
    /// </summary>
    /// <returns>A formatted string containing the binding details.</returns>
    /// <remarks>
    /// The string representation is designed for logging and debugging purposes,
    /// providing a concise overview of the binding configuration.
    /// </remarks>
    public override string ToString()
    {
        var routingInfo = string.IsNullOrEmpty(RoutingKey) ? "[fanout]" : $"'{RoutingKey}'";
        var argInfo = Arguments?.Any() == true ? $" with {Arguments.Count} arguments" : "";
        return $"Bind queue '{QueueName}' to exchange '{ExchangeName}' with routing {routingInfo}{argInfo}";
    }
}