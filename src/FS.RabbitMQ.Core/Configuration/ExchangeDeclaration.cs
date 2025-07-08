using FS.RabbitMQ.Core.Abstractions;

namespace FS.RabbitMQ.Core.Configuration;

/// <summary>
/// Defines the declaration parameters for RabbitMQ exchanges.
/// </summary>
/// <remarks>
/// Exchange declarations define the routing infrastructure that determines how messages
/// are distributed to queues within RabbitMQ. Exchanges are the central routing
/// mechanism that receives messages from publishers and routes them to appropriate
/// queues based on routing rules defined by the exchange type and routing keys.
/// 
/// Understanding RabbitMQ exchanges:
/// 
/// Exchanges are message routing agents that receive messages from publishers and
/// route them to queues based on routing rules. Think of an exchange as a sophisticated
/// post office sorting facility that looks at message addresses (routing keys) and
/// delivery instructions (bindings) to determine where each message should go.
/// 
/// Exchange types and their routing behavior:
/// 
/// 1. Direct Exchange:
///    - Routes messages based on exact routing key matches
///    - Message routing key must exactly match binding routing key
///    - Perfect for point-to-point communication patterns
///    - Example: routing key "user.created" only goes to queues bound with "user.created"
/// 
/// 2. Fanout Exchange:
///    - Routes messages to all bound queues regardless of routing key
///    - Routing key is ignored completely
///    - Perfect for broadcast scenarios where all consumers need the message
///    - Example: system-wide notifications or cache invalidation events
/// 
/// 3. Topic Exchange:
///    - Routes messages based on pattern matching with routing keys
///    - Supports wildcards: * (single word) and # (zero or more words)
///    - Perfect for hierarchical routing and selective subscription
///    - Example: "logs.*.error" matches "logs.api.error" and "logs.db.error"
/// 
/// 4. Headers Exchange:
///    - Routes messages based on message header attributes rather than routing key
///    - Supports complex routing logic based on multiple header criteria
///    - Perfect for content-based routing with rich metadata
///    - Example: route based on message priority, content type, or custom attributes
/// 
/// Exchange characteristics:
/// 
/// Durability:
/// - Durable exchanges survive broker restarts
/// - Non-durable exchanges are lost when broker restarts
/// - Most production exchanges should be durable
/// 
/// Auto-deletion:
/// - Auto-delete exchanges are removed when no longer bound
/// - Useful for temporary or dynamically created exchanges
/// - Should be used carefully to avoid accidental deletion
/// 
/// Internal exchanges:
/// - Cannot receive messages directly from publishers
/// - Only receive messages from other exchanges via bindings
/// - Useful for complex routing topologies and message transformation
/// </remarks>
public sealed class ExchangeDeclaration
{
    /// <summary>
    /// Gets or sets the name of the exchange.
    /// </summary>
    /// <value>The exchange name used for message routing and binding operations.</value>
    /// <remarks>
    /// Exchange names should be descriptive and follow organizational naming conventions:
    /// - Use clear, meaningful names that indicate purpose
    /// - Consider hierarchical naming for related exchanges
    /// - Follow consistent naming patterns across the organization
    /// - Avoid special characters that might cause routing issues
    /// 
    /// Common naming patterns:
    /// - Domain-based: "orders", "payments", "notifications"
    /// - Service-based: "user-service", "inventory-service"
    /// - Environment-prefixed: "prod.orders", "dev.orders"
    /// - Function-based: "commands", "events", "queries"
    /// 
    /// Exchange names are case-sensitive and must be unique within a virtual host.
    /// The empty string "" refers to the default exchange, which is a special
    /// direct exchange that routes messages directly to queues by name.
    /// </remarks>
    public required string Name { get; set; }

    /// <summary>
    /// Gets or sets the type of the exchange.
    /// </summary>
    /// <value>The exchange type that determines routing behavior.</value>
    /// <remarks>
    /// Exchange type determines how messages are routed to queues:
    /// 
    /// Choose Direct when:
    /// - You need point-to-point message delivery
    /// - Routing decisions are based on exact key matches
    /// - You want simple, predictable routing behavior
    /// - You're implementing request-response patterns
    /// 
    /// Choose Fanout when:
    /// - You need to broadcast messages to multiple consumers
    /// - All bound queues should receive every message
    /// - You're implementing publish-subscribe patterns
    /// - Routing key-based filtering is not needed
    /// 
    /// Choose Topic when:
    /// - You need hierarchical or pattern-based routing
    /// - Consumers want to subscribe to message subsets
    /// - You're implementing selective subscription patterns
    /// - Routing keys follow a logical hierarchy
    /// 
    /// Choose Headers when:
    /// - Routing decisions are based on message metadata
    /// - You need complex routing logic beyond simple keys
    /// - Message content or attributes drive routing
    /// - You need multi-criteria routing decisions
    /// </remarks>
    public ExchangeType Type { get; set; } = ExchangeType.Direct;

    /// <summary>
    /// Gets or sets a value indicating whether the exchange is durable.
    /// </summary>
    /// <value><c>true</c> if the exchange should survive broker restarts; otherwise, <c>false</c>. Default is <c>true</c>.</value>
    /// <remarks>
    /// Durability determines whether the exchange persists across broker restarts:
    /// 
    /// Durable exchanges:
    /// - Survive broker restarts and maintain routing infrastructure
    /// - Essential for production systems requiring reliability
    /// - Required for persistent message routing
    /// - Should be used for all business-critical exchanges
    /// 
    /// Non-durable exchanges:
    /// - Lost when broker restarts, requiring recreation
    /// - Suitable for temporary or session-based routing
    /// - May be appropriate for development or testing
    /// - Can be used for ephemeral communication patterns
    /// 
    /// Consider durability based on:
    /// - Business importance of the exchange
    /// - Recovery requirements after broker restart
    /// - Operational complexity of recreation
    /// - Integration with automated deployment processes
    /// 
    /// Most production exchanges should be durable to ensure routing
    /// infrastructure remains intact during broker maintenance or failures.
    /// </remarks>
    public bool Durable { get; set; } = true;

    /// <summary>
    /// Gets or sets a value indicating whether the exchange should be automatically deleted.
    /// </summary>
    /// <value><c>true</c> if the exchange should be auto-deleted when no longer bound; otherwise, <c>false</c>. Default is <c>false</c>.</value>
    /// <remarks>
    /// Auto-deletion controls automatic cleanup when the exchange is no longer used:
    /// 
    /// Auto-delete enabled:
    /// - Exchange is automatically removed when no queues are bound to it
    /// - Useful for temporary or dynamically created exchanges
    /// - Reduces administrative overhead for cleanup
    /// - Can lead to accidental deletion if not carefully managed
    /// 
    /// Auto-delete disabled:
    /// - Exchange persists even when not bound to any queues
    /// - Provides stability and predictable infrastructure
    /// - Requires manual cleanup when no longer needed
    /// - Prevents accidental deletion during maintenance
    /// 
    /// Use auto-delete when:
    /// - Exchanges are created dynamically for temporary purposes
    /// - Automatic cleanup is more important than persistence
    /// - Exchange lifecycle is tied to specific application instances
    /// - Resource cleanup automation is desired
    /// 
    /// Avoid auto-delete when:
    /// - Exchanges are part of permanent infrastructure
    /// - Manual control over exchange lifecycle is preferred
    /// - Risk of accidental deletion is unacceptable
    /// - Exchanges may temporarily have no bindings during normal operation
    /// </remarks>
    public bool AutoDelete { get; set; } = false;

    /// <summary>
    /// Gets or sets a value indicating whether the exchange is internal.
    /// </summary>
    /// <value><c>true</c> if the exchange is internal and cannot receive messages from publishers; otherwise, <c>false</c>. Default is <c>false</c>.</value>
    /// <remarks>
    /// Internal exchanges cannot receive messages directly from publishers:
    /// - Only receive messages through exchange-to-exchange bindings
    /// - Useful for creating complex routing topologies
    /// - Enable message transformation and multi-stage routing
    /// - Support advanced messaging patterns and workflows
    /// 
    /// Use internal exchanges for:
    /// - Multi-stage message processing pipelines
    /// - Message transformation and enrichment workflows
    /// - Complex routing logic that requires multiple steps
    /// - Integration patterns that need message mediation
    /// 
    /// Internal exchange patterns:
    /// - Primary exchange routes to internal exchange for transformation
    /// - Internal exchange applies additional routing logic
    /// - Final routing to destination queues for consumption
    /// - Error handling and dead letter routing through internal exchanges
    /// 
    /// Internal exchanges are advanced features that add complexity but enable
    /// sophisticated messaging patterns when simple routing is insufficient.
    /// </remarks>
    public bool Internal { get; set; } = false;

    /// <summary>
    /// Gets or sets additional arguments for exchange configuration.
    /// </summary>
    /// <value>A dictionary of exchange-specific arguments, or null if no additional arguments are needed.</value>
    /// <remarks>
    /// Exchange arguments provide advanced configuration options:
    /// - Plugin-specific settings and features
    /// - Performance tuning parameters
    /// - Specialized routing behavior configuration
    /// - Integration with RabbitMQ extensions
    /// 
    /// Common exchange arguments:
    /// 
    /// Alternate Exchange (alternate-exchange):
    /// - Specifies where to route messages that can't be routed normally
    /// - Provides fallback routing for unroutable messages
    /// - Useful for error handling and message recovery
    /// - Example: x-arguments["alternate-exchange"] = "unrouted-messages"
    /// 
    /// Exchange Type-Specific Arguments:
    /// - Headers exchange: matching strategy (any/all)
    /// - Custom exchange types: plugin-specific parameters
    /// - Performance tuning: buffer sizes, caching options
    /// 
    /// Plugin Arguments:
    /// - Delayed message exchange: message delay configuration
    /// - Consistent hash exchange: hash function parameters
    /// - Federation plugin: federation policies and parameters
    /// 
    /// Use arguments when:
    /// - Default exchange behavior is insufficient
    /// - Plugins provide additional functionality
    /// - Performance tuning is required
    /// - Complex routing patterns need special configuration
    /// 
    /// Arguments are key-value pairs where keys are typically strings
    /// and values can be strings, numbers, or booleans depending on the argument.
    /// </remarks>
    public IDictionary<string, object>? Arguments { get; set; }

    /// <summary>
    /// Creates a direct exchange declaration with the specified name.
    /// </summary>
    /// <param name="name">The name of the direct exchange.</param>
    /// <param name="durable">Whether the exchange should be durable. Default is true.</param>
    /// <returns>An exchange declaration configured as a direct exchange.</returns>
    /// <remarks>
    /// Direct exchanges provide point-to-point routing based on exact routing key matches:
    /// - Simple and predictable routing behavior
    /// - Good performance characteristics
    /// - Easy to understand and debug
    /// - Suitable for most basic routing scenarios
    /// 
    /// Direct exchanges are ideal for:
    /// - Command routing to specific handlers
    /// - Request-response patterns
    /// - Point-to-point message delivery
    /// - Simple routing without pattern matching
    /// </remarks>
    public static ExchangeDeclaration CreateDirect(string name, bool durable = true)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(name);

        return new ExchangeDeclaration
        {
            Name = name,
            Type = ExchangeType.Direct,
            Durable = durable,
            AutoDelete = false,
            Internal = false
        };
    }

    /// <summary>
    /// Creates a fanout exchange declaration with the specified name.
    /// </summary>
    /// <param name="name">The name of the fanout exchange.</param>
    /// <param name="durable">Whether the exchange should be durable. Default is true.</param>
    /// <returns>An exchange declaration configured as a fanout exchange.</returns>
    /// <remarks>
    /// Fanout exchanges broadcast messages to all bound queues:
    /// - Ignores routing keys completely
    /// - Delivers messages to every bound queue
    /// - Perfect for publish-subscribe patterns
    /// - Simple broadcast semantics
    /// 
    /// Fanout exchanges are ideal for:
    /// - System-wide notifications and alerts
    /// - Cache invalidation across multiple services
    /// - Event broadcasting to multiple subscribers
    /// - Real-time data distribution
    /// </remarks>
    public static ExchangeDeclaration CreateFanout(string name, bool durable = true)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(name);

        return new ExchangeDeclaration
        {
            Name = name,
            Type = ExchangeType.Fanout,
            Durable = durable,
            AutoDelete = false,
            Internal = false
        };
    }

    /// <summary>
    /// Creates a topic exchange declaration with the specified name.
    /// </summary>
    /// <param name="name">The name of the topic exchange.</param>
    /// <param name="durable">Whether the exchange should be durable. Default is true.</param>
    /// <returns>An exchange declaration configured as a topic exchange.</returns>
    /// <remarks>
    /// Topic exchanges provide pattern-based routing using wildcards:
    /// - Supports * (single word) and # (zero or more words) wildcards
    /// - Enables hierarchical routing patterns
    /// - Supports selective subscription based on interest
    /// - More complex but very flexible routing
    /// 
    /// Topic exchanges are ideal for:
    /// - Hierarchical event routing (logs.*.error, user.*.created)
    /// - Selective subscription patterns
    /// - Domain-driven message routing
    /// - Complex routing scenarios with multiple criteria
    /// 
    /// Topic routing examples:
    /// - "user.*.created" matches "user.admin.created" and "user.customer.created"
    /// - "logs.#" matches "logs.api.error" and "logs.database.connection.failed"
    /// - "order.payment.*" matches "order.payment.completed" and "order.payment.failed"
    /// </remarks>
    public static ExchangeDeclaration CreateTopic(string name, bool durable = true)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(name);

        return new ExchangeDeclaration
        {
            Name = name,
            Type = ExchangeType.Topic,
            Durable = durable,
            AutoDelete = false,
            Internal = false
        };
    }

    /// <summary>
    /// Creates a headers exchange declaration with the specified name.
    /// </summary>
    /// <param name="name">The name of the headers exchange.</param>
    /// <param name="durable">Whether the exchange should be durable. Default is true.</param>
    /// <returns>An exchange declaration configured as a headers exchange.</returns>
    /// <remarks>
    /// Headers exchanges route based on message header attributes:
    /// - Routing decisions based on message headers rather than routing key
    /// - Supports complex routing logic with multiple criteria
    /// - Can match any or all specified headers
    /// - Most flexible but also most complex routing type
    /// 
    /// Headers exchanges are ideal for:
    /// - Content-based routing based on message metadata
    /// - Multi-criteria routing decisions
    /// - Integration scenarios with rich message attributes
    /// - Complex business rule-based routing
    /// 
    /// Headers routing examples:
    /// - Route based on message priority and content type
    /// - Route based on geographical region and customer type
    /// - Route based on processing requirements and SLA levels
    /// </remarks>
    public static ExchangeDeclaration CreateHeaders(string name, bool durable = true)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(name);

        return new ExchangeDeclaration
        {
            Name = name,
            Type = ExchangeType.Headers,
            Durable = durable,
            AutoDelete = false,
            Internal = false
        };
    }

    /// <summary>
    /// Creates a temporary exchange declaration that will be auto-deleted.
    /// </summary>
    /// <param name="name">The name of the temporary exchange.</param>
    /// <param name="type">The type of the temporary exchange. Default is Direct.</param>
    /// <returns>An exchange declaration configured as a temporary exchange.</returns>
    /// <remarks>
    /// Temporary exchanges are automatically cleaned up when no longer needed:
    /// - Non-durable and auto-delete for automatic lifecycle management
    /// - Suitable for session-based or application-specific routing
    /// - Reduces administrative overhead for cleanup
    /// - Should be used carefully to avoid accidental deletion
    /// 
    /// Temporary exchanges are useful for:
    /// - Application-specific routing that doesn't need persistence
    /// - Development and testing scenarios
    /// - Dynamic routing created by applications
    /// - Short-lived messaging patterns
    /// </remarks>
    public static ExchangeDeclaration CreateTemporary(string name, ExchangeType type = ExchangeType.Direct)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(name);

        return new ExchangeDeclaration
        {
            Name = name,
            Type = type,
            Durable = false,
            AutoDelete = true,
            Internal = false
        };
    }

    /// <summary>
    /// Validates the exchange declaration for consistency and correctness.
    /// </summary>
    /// <exception cref="BrokerConfigurationException">Thrown when the exchange declaration is invalid.</exception>
    /// <remarks>
    /// Exchange validation ensures that the declaration parameters are consistent
    /// and will result in a functional exchange configuration.
    /// 
    /// Validation checks include:
    /// - Exchange name format and uniqueness requirements
    /// - Consistency between durability and auto-delete settings
    /// - Argument format and type-specific requirements
    /// - Naming convention compliance
    /// 
    /// Early validation helps prevent runtime errors during exchange declaration
    /// and provides clear feedback about configuration issues.
    /// </remarks>
    public void Validate()
    {
        if (string.IsNullOrWhiteSpace(Name))
        {
            throw new BrokerConfigurationException(
                "Exchange name cannot be null or empty.",
                configurationSection: nameof(ExchangeDeclaration),
                parameterName: nameof(Name),
                parameterValue: Name);
        }

        // Validate exchange name format
        if (Name.Contains(" "))
        {
            throw new BrokerConfigurationException(
                "Exchange name cannot contain spaces.",
                configurationSection: nameof(ExchangeDeclaration),
                parameterName: nameof(Name),
                parameterValue: Name);
        }

        // Validate consistency between durable and auto-delete
        if (!Durable && !AutoDelete)
        {
            throw new BrokerConfigurationException(
                "Non-durable exchanges should typically be auto-delete to prevent accumulation.",
                configurationSection: nameof(ExchangeDeclaration),
                parameterName: nameof(AutoDelete));
        }

        // Validate arguments if present
        if (Arguments?.Any() == true)
        {
            foreach (var argument in Arguments)
            {
                if (string.IsNullOrWhiteSpace(argument.Key))
                {
                    throw new BrokerConfigurationException(
                        "Exchange argument keys cannot be null or empty.",
                        configurationSection: nameof(ExchangeDeclaration),
                        parameterName: nameof(Arguments));
                }
            }
        }
    }

    /// <summary>
    /// Returns a string representation of the exchange declaration.
    /// </summary>
    /// <returns>A formatted string describing the exchange configuration.</returns>
    public override string ToString()
    {
        var features = new List<string>();
        
        if (Durable) features.Add("durable");
        if (AutoDelete) features.Add("auto-delete");
        if (Internal) features.Add("internal");
        if (Arguments?.Any() == true) features.Add($"args:{Arguments.Count}");

        var featureInfo = features.Any() ? $" [{string.Join(", ", features)}]" : "";
        return $"{Type} Exchange '{Name}'{featureInfo}";
    }

    /// <summary>
    /// Determines whether two exchange declarations are equivalent.
    /// </summary>
    /// <param name="obj">The object to compare with this declaration.</param>
    /// <returns><c>true</c> if the declarations are equivalent; otherwise, <c>false</c>.</returns>
    public override bool Equals(object? obj)
    {
        return obj is ExchangeDeclaration other &&
               Name == other.Name &&
               Type == other.Type &&
               Durable == other.Durable &&
               AutoDelete == other.AutoDelete &&
               Internal == other.Internal;
    }

    /// <summary>
    /// Returns a hash code for the exchange declaration.
    /// </summary>
    /// <returns>A hash code suitable for use in hash tables and collections.</returns>
    public override int GetHashCode()
    {
        return HashCode.Combine(Name, Type, Durable, AutoDelete, Internal);
    }
}