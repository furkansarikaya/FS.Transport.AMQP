using FS.RabbitMQ.Core.Abstractions;

namespace FS.RabbitMQ.Core.Configuration;

/// <summary>
/// Defines the declaration parameters for RabbitMQ bindings between exchanges and queues.
/// </summary>
/// <remarks>
/// Binding declarations define the routing relationships that connect exchanges to queues
/// (or exchanges to other exchanges) within RabbitMQ. Bindings are the "wiring" that
/// determines how messages flow from publishers through exchanges to consumers via queues.
/// 
/// Understanding RabbitMQ bindings:
/// 
/// Bindings create the routing rules that determine which messages from an exchange
/// are delivered to which queues. Think of bindings as routing instructions that tell
/// the exchange "send messages that match this pattern to that queue."
/// 
/// The routing decision depends on:
/// - The exchange type (direct, fanout, topic, headers)
/// - The routing key in the message
/// - The binding key specified in the binding
/// - Any additional binding arguments for complex routing
/// 
/// Binding relationships and patterns:
/// 
/// 1. Exchange-to-Queue Bindings (most common):
///    - Connect exchanges directly to queues for message delivery
///    - Define the primary routing paths for message consumption
///    - Enable load balancing when multiple queues bind to the same exchange
///    - Support all exchange types and routing patterns
/// 
/// 2. Exchange-to-Exchange Bindings:
///    - Connect exchanges to other exchanges for complex routing topologies
///    - Enable multi-stage routing and message transformation workflows
///    - Support advanced messaging patterns like content-based routing
///    - Allow for separation of routing concerns across multiple exchanges
/// 
/// Routing behavior by exchange type:
/// 
/// Direct Exchange Bindings:
/// - Routing key must exactly match binding key
/// - Simple one-to-one or one-to-many routing
/// - Example: routing key "user.created" matches binding key "user.created"
/// 
/// Fanout Exchange Bindings:
/// - Routing key is ignored; all bound queues receive messages
/// - Simple broadcast to all bound destinations
/// - Binding key can be empty or any value
/// 
/// Topic Exchange Bindings:
/// - Pattern matching with wildcards (* and #)
/// - Hierarchical routing based on dot-separated routing keys
/// - Examples:
///   - Binding key "user.*" matches "user.created" and "user.updated"
///   - Binding key "logs.#" matches "logs.api.error" and "logs.db.connection.timeout"
/// 
/// Headers Exchange Bindings:
/// - Routing based on message header attributes
/// - Binding arguments specify header matching criteria
/// - Can match any or all specified headers
/// - Most flexible but also most complex routing type
/// 
/// Binding design patterns:
/// 
/// Point-to-Point Routing:
/// - Single exchange bound to single queue with specific routing key
/// - Used for direct message delivery to specific consumers
/// - Common in command and request-response patterns
/// 
/// Publish-Subscribe:
/// - Single exchange bound to multiple queues (usually fanout)
/// - All subscribers receive all messages
/// - Common in event notification and broadcast scenarios
/// 
/// Content-Based Routing:
/// - Topic or headers exchange with multiple bindings based on content criteria
/// - Messages routed to different queues based on their characteristics
/// - Common in workflow systems and multi-tenant applications
/// 
/// Load Balancing:
/// - Multiple queues bound to the same exchange with the same binding key
/// - Messages are distributed across queues (and their consumers)
/// - Common in work queue and competing consumer patterns
/// 
/// Hierarchical Routing:
/// - Topic exchange with hierarchical binding key patterns
/// - Enables selective subscription to message subsets
/// - Common in logging, monitoring, and domain-driven architectures
/// </remarks>
public sealed class BindingDeclaration
{
    /// <summary>
    /// Gets or sets the name of the source exchange.
    /// </summary>
    /// <value>The exchange name that will route messages through this binding.</value>
    /// <remarks>
    /// The source exchange is where messages originate before being routed through the binding.
    /// This must be an existing exchange that has been declared before the binding is created.
    /// 
    /// Source exchange considerations:
    /// - Must exist before binding creation (or be declared in the same topology operation)
    /// - Exchange type determines how routing key matching works
    /// - Source exchange name cannot be empty (unlike destination queue names)
    /// - Default exchange ("") can be used but has special routing behavior
    /// 
    /// The default exchange is a special direct exchange that routes messages directly
    /// to queues based on the queue name as the routing key. When using the default
    /// exchange, the routing key should match the destination queue name exactly.
    /// </remarks>
    public required string SourceExchange { get; set; }

    /// <summary>
    /// Gets or sets the name of the destination queue.
    /// </summary>
    /// <value>The queue name that will receive messages through this binding, or null for exchange-to-exchange bindings.</value>
    /// <remarks>
    /// The destination queue is where messages will be delivered for consumption.
    /// For exchange-to-queue bindings, this specifies the target queue name.
    /// For exchange-to-exchange bindings, this should be null and DestinationExchange should be used instead.
    /// 
    /// Destination queue considerations:
    /// - Must exist before binding creation (or be declared in the same topology operation)  
    /// - Queue name must match exactly (case-sensitive)
    /// - Cannot be used together with DestinationExchange (use one or the other)
    /// - Queue properties (durable, exclusive, etc.) don't affect binding behavior
    /// 
    /// When multiple queues are bound to the same exchange with the same binding key,
    /// messages are distributed across the queues in a round-robin fashion, providing
    /// natural load balancing for competing consumer patterns.
    /// </remarks>
    public string? DestinationQueue { get; set; }

    /// <summary>
    /// Gets or sets the name of the destination exchange for exchange-to-exchange bindings.
    /// </summary>
    /// <value>The exchange name that will receive messages through this binding, or null for exchange-to-queue bindings.</value>
    /// <remarks>
    /// The destination exchange enables complex routing topologies where messages flow
    /// through multiple exchanges before reaching queues. This is useful for:
    /// - Multi-stage routing and message transformation
    /// - Content-based routing with different routing logic at each stage
    /// - Integration patterns that require message mediation
    /// - Separation of routing concerns across different exchanges
    /// 
    /// Exchange-to-exchange binding considerations:
    /// - Cannot be used together with DestinationQueue (use one or the other)
    /// - Destination exchange must exist before binding creation
    /// - Can create complex routing graphs but also increases complexity
    /// - Messages may pass through multiple exchanges before reaching queues
    /// - Useful for advanced messaging patterns but should be used judiciously
    /// 
    /// Common exchange-to-exchange patterns:
    /// - Primary exchange for initial routing, secondary exchange for filtering
    /// - Routing based on message source, then routing based on message content
    /// - Error handling where failed messages are routed through error exchanges
    /// - Transformation exchanges that modify messages before final routing
    /// </remarks>
    public string? DestinationExchange { get; set; }

    /// <summary>
    /// Gets or sets the routing key for the binding.
    /// </summary>
    /// <value>The routing key used to determine if messages should flow through this binding.</value>
    /// <remarks>
    /// The routing key (also called binding key) defines the routing criteria for this binding.
    /// How the routing key is interpreted depends on the source exchange type:
    /// 
    /// Direct Exchange Routing Keys:
    /// - Must exactly match the message routing key
    /// - Case-sensitive string comparison
    /// - Examples: "user.created", "order.payment.completed", "system.alert"
    /// 
    /// Fanout Exchange Routing Keys:
    /// - Completely ignored; can be empty or any value
    /// - All messages are routed to all bound queues regardless of routing key
    /// - Convention is to use empty string "" for fanout bindings
    /// 
    /// Topic Exchange Routing Keys:
    /// - Support pattern matching with wildcards
    /// - * (asterisk) matches exactly one word
    /// - # (hash) matches zero or more words
    /// - Words are separated by dots (.)
    /// - Examples:
    ///   - "user.*" matches "user.created" and "user.updated" but not "user.profile.updated"
    ///   - "logs.#" matches "logs.api", "logs.api.error", and "logs.database.connection.failed"
    ///   - "*.error" matches "api.error" and "database.error" but not "api.warning"
    /// 
    /// Headers Exchange Routing Keys:
    /// - Usually ignored as routing is based on message headers
    /// - Can be used for additional routing logic in combination with headers
    /// - Header matching criteria are specified in binding arguments
    /// 
    /// Routing key design patterns:
    /// 
    /// Hierarchical Keys:
    /// - Use dot-separated words to create logical hierarchies
    /// - Examples: "region.country.city", "service.operation.result"
    /// - Enable selective subscription with topic exchange wildcards
    /// 
    /// Functional Keys:
    /// - Group related operations or message types
    /// - Examples: "commands.user", "events.order", "queries.inventory"
    /// - Support domain-driven design and service boundaries
    /// 
    /// Priority or Category Keys:
    /// - Include priority or category information in routing keys
    /// - Examples: "high.priority.alerts", "low.priority.notifications"
    /// - Enable priority-based routing and handling
    /// 
    /// Routing key best practices:
    /// - Use consistent naming conventions across the organization
    /// - Keep keys meaningful and self-documenting
    /// - Consider future routing requirements when designing key hierarchies
    /// - Avoid overly long or complex routing keys
    /// - Document routing key patterns for operational teams
    /// </remarks>
    public string RoutingKey { get; set; } = "";

    /// <summary>
    /// Gets or sets additional arguments for binding configuration.
    /// </summary>
    /// <value>A dictionary of binding-specific arguments, or null if no additional arguments are needed.</value>
    /// <remarks>
    /// Binding arguments provide advanced configuration options for specialized routing behavior:
    /// - Headers exchange matching criteria and logic
    /// - Plugin-specific routing parameters
    /// - Performance tuning and optimization settings
    /// - Custom routing algorithms and extensions
    /// 
    /// Common binding arguments:
    /// 
    /// Headers Exchange Arguments:
    /// Headers exchanges use binding arguments to specify which message headers to match
    /// and how the matching should be performed:
    /// 
    /// Header Matching (x-match):
    /// - "any": Message matches if any specified header matches
    /// - "all": Message matches only if all specified headers match
    /// - Example: Arguments["x-match"] = "any"
    /// 
    /// Header Criteria:
    /// - Specify header names and expected values
    /// - Only messages with matching headers are routed through the binding
    /// - Examples:
    ///   - Arguments["content-type"] = "application/json"
    ///   - Arguments["priority"] = "high"
    ///   - Arguments["region"] = "us-east"
    /// 
    /// Combined Example for Headers Exchange:
    /// Arguments = {
    ///     ["x-match"] = "all",
    ///     ["content-type"] = "application/json",
    ///     ["priority"] = "high"
    /// }
    /// This binding only routes messages that have both content-type=application/json
    /// AND priority=high headers.
    /// 
    /// Plugin-Specific Arguments:
    /// Various RabbitMQ plugins may define additional binding arguments:
    /// 
    /// Consistent Hash Exchange:
    /// - Arguments may specify hash functions or distribution weights
    /// - Enable custom load balancing algorithms
    /// 
    /// Federation Plugin:
    /// - Arguments may control federation policies and behavior
    /// - Enable cross-cluster message routing
    /// 
    /// Custom Exchange Types:
    /// - Third-party exchanges may define custom argument sets
    /// - Enable specialized routing algorithms and patterns
    /// 
    /// Use binding arguments when:
    /// - Using headers exchanges for content-based routing
    /// - Implementing complex routing logic beyond simple key matching
    /// - Integrating with RabbitMQ plugins that require additional configuration
    /// - Customizing routing behavior for specialized requirements
    /// 
    /// Arguments are key-value pairs where:
    /// - Keys are typically strings defining the argument name
    /// - Values can be strings, numbers, or booleans depending on the argument
    /// - Argument interpretation depends on the exchange type and plugins
    /// - Incorrect arguments may cause binding creation to fail
    /// </remarks>
    public IDictionary<string, object>? Arguments { get; set; }

    /// <summary>
    /// Creates a simple exchange-to-queue binding with the specified parameters.
    /// </summary>
    /// <param name="sourceExchange">The name of the source exchange.</param>
    /// <param name="destinationQueue">The name of the destination queue.</param>
    /// <param name="routingKey">The routing key for the binding. Default is empty string.</param>
    /// <returns>A binding declaration for exchange-to-queue routing.</returns>
    /// <remarks>
    /// Simple bindings provide straightforward routing from exchanges to queues:
    /// - Most common type of binding in messaging systems
    /// - Direct routing path from publisher to consumer
    /// - Easy to understand and debug
    /// - Suitable for most basic messaging scenarios
    /// 
    /// Simple bindings are ideal for:
    /// - Point-to-point messaging patterns
    /// - Basic publish-subscribe scenarios
    /// - Command routing to specific handlers
    /// - Simple event distribution
    /// </remarks>
    public static BindingDeclaration CreateSimple(string sourceExchange, string destinationQueue, string routingKey = "")
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(sourceExchange);
        ArgumentException.ThrowIfNullOrWhiteSpace(destinationQueue);

        return new BindingDeclaration
        {
            SourceExchange = sourceExchange,
            DestinationQueue = destinationQueue,
            DestinationExchange = null,
            RoutingKey = routingKey
        };
    }

    /// <summary>
    /// Creates an exchange-to-exchange binding for complex routing topologies.
    /// </summary>
    /// <param name="sourceExchange">The name of the source exchange.</param>
    /// <param name="destinationExchange">The name of the destination exchange.</param>
    /// <param name="routingKey">The routing key for the binding. Default is empty string.</param>
    /// <returns>A binding declaration for exchange-to-exchange routing.</returns>
    /// <remarks>
    /// Exchange-to-exchange bindings enable complex routing topologies:
    /// - Multi-stage routing and message transformation
    /// - Separation of routing concerns across exchanges
    /// - Advanced messaging patterns and workflows
    /// - Integration with message mediation systems
    /// 
    /// Exchange-to-exchange bindings are useful for:
    /// - Content-based routing with multiple routing stages
    /// - Message transformation and enrichment workflows
    /// - Error handling and dead letter routing
    /// - Integration patterns requiring message mediation
    /// 
    /// Use with caution as they increase system complexity and can make
    /// message flow harder to understand and debug.
    /// </remarks>
    public static BindingDeclaration CreateExchangeToExchange(string sourceExchange, string destinationExchange, string routingKey = "")
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(sourceExchange);
        ArgumentException.ThrowIfNullOrWhiteSpace(destinationExchange);

        return new BindingDeclaration
        {
            SourceExchange = sourceExchange,
            DestinationQueue = null,
            DestinationExchange = destinationExchange,
            RoutingKey = routingKey
        };
    }

    /// <summary>
    /// Creates a topic binding with wildcard pattern matching.
    /// </summary>
    /// <param name="sourceExchange">The name of the source topic exchange.</param>
    /// <param name="destinationQueue">The name of the destination queue.</param>
    /// <param name="pattern">The topic pattern with wildcards (* and #).</param>
    /// <returns>A binding declaration for topic pattern matching.</returns>
    /// <remarks>
    /// Topic bindings enable hierarchical and pattern-based routing:
    /// - Support selective subscription based on message characteristics
    /// - Enable flexible routing without changing exchange configuration
    /// - Support complex routing patterns with wildcards
    /// - Allow consumers to specify exactly what messages they want
    /// 
    /// Topic pattern examples:
    /// - "user.*" matches "user.created", "user.updated", "user.deleted"
    /// - "logs.#" matches "logs.api", "logs.api.error", "logs.database.connection.failed"
    /// - "*.error" matches "api.error", "database.error", "payment.error"
    /// - "order.*.completed" matches "order.payment.completed", "order.shipping.completed"
    /// 
    /// Topic bindings are ideal for:
    /// - Event-driven architectures with selective subscription
    /// - Logging and monitoring systems with hierarchical filtering
    /// - Domain-driven design with bounded context messaging
    /// - Multi-tenant systems with tenant-specific routing
    /// </remarks>
    public static BindingDeclaration CreateTopic(string sourceExchange, string destinationQueue, string pattern)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(sourceExchange);
        ArgumentException.ThrowIfNullOrWhiteSpace(destinationQueue);
        ArgumentException.ThrowIfNullOrWhiteSpace(pattern);

        return new BindingDeclaration
        {
            SourceExchange = sourceExchange,
            DestinationQueue = destinationQueue,
            DestinationExchange = null,
            RoutingKey = pattern
        };
    }

    /// <summary>
    /// Creates a headers binding with header-based routing criteria.
    /// </summary>
    /// <param name="sourceExchange">The name of the source headers exchange.</param>
    /// <param name="destinationQueue">The name of the destination queue.</param>
    /// <param name="matchType">The header matching type ("any" or "all").</param>
    /// <param name="headers">The header criteria for routing.</param>
    /// <returns>A binding declaration for headers-based routing.</returns>
    /// <remarks>
    /// Headers bindings provide content-based routing using message metadata:
    /// - Route based on message headers rather than routing keys
    /// - Support complex routing logic with multiple criteria
    /// - Enable rich metadata-driven routing decisions
    /// - Most flexible routing type but also most complex
    /// 
    /// Header matching types:
    /// - "any": Route if any specified header matches
    /// - "all": Route only if all specified headers match
    /// 
    /// Headers bindings are ideal for:
    /// - Content-based routing with rich metadata
    /// - Multi-criteria routing decisions
    /// - Integration scenarios with complex message attributes
    /// - Business rule-driven routing logic
    /// 
    /// Example usage:
    /// - Route high-priority messages from specific regions
    /// - Route messages based on content type and processing requirements
    /// - Route messages based on customer type and SLA requirements
    /// </remarks>
    public static BindingDeclaration CreateHeaders(string sourceExchange, string destinationQueue, string matchType, IDictionary<string, object> headers)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(sourceExchange);
        ArgumentException.ThrowIfNullOrWhiteSpace(destinationQueue);
        ArgumentNullException.ThrowIfNull(headers);

        if (matchType != "any" && matchType != "all")
        {
            throw new ArgumentException("Match type must be 'any' or 'all'.", nameof(matchType));
        }

        var arguments = new Dictionary<string, object>(headers)
        {
            ["x-match"] = matchType
        };

        return new BindingDeclaration
        {
            SourceExchange = sourceExchange,
            DestinationQueue = destinationQueue,
            DestinationExchange = null,
            RoutingKey = "", // Routing key is typically ignored for headers exchanges
            Arguments = arguments
        };
    }

    /// <summary>
    /// Creates a fanout binding that ignores routing keys.
    /// </summary>
    /// <param name="sourceExchange">The name of the source fanout exchange.</param>
    /// <param name="destinationQueue">The name of the destination queue.</param>
    /// <returns>A binding declaration for fanout (broadcast) routing.</returns>
    /// <remarks>
    /// Fanout bindings provide simple broadcast routing:
    /// - All messages are routed to all bound queues
    /// - Routing key is ignored completely
    /// - Simple and predictable broadcast behavior
    /// - Highest performance for broadcast scenarios
    /// 
    /// Fanout bindings are ideal for:
    /// - System-wide notifications and alerts
    /// - Cache invalidation across multiple services
    /// - Event broadcasting to multiple subscribers
    /// - Real-time data distribution to all consumers
    /// 
    /// Fanout routing is the simplest and most predictable routing pattern,
    /// making it ideal for scenarios where all subscribers need all messages.
    /// </remarks>
    public static BindingDeclaration CreateFanout(string sourceExchange, string destinationQueue)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(sourceExchange);
        ArgumentException.ThrowIfNullOrWhiteSpace(destinationQueue);

        return new BindingDeclaration
        {
            SourceExchange = sourceExchange,
            DestinationQueue = destinationQueue,
            DestinationExchange = null,
            RoutingKey = "" // Routing key is ignored for fanout exchanges
        };
    }

    /// <summary>
    /// Validates the binding declaration for consistency and correctness.
    /// </summary>
    /// <exception cref="BrokerConfigurationException">Thrown when the binding declaration is invalid.</exception>
    /// <remarks>
    /// Binding validation ensures that the declaration parameters are consistent
    /// and will result in a functional binding configuration.
    /// 
    /// Validation checks include:
    /// - Required field presence and format
    /// - Mutual exclusivity of destination queue and exchange
    /// - Routing key format for different exchange types
    /// - Argument format and consistency
    /// - Cross-parameter validation and consistency
    /// 
    /// Early validation helps prevent runtime errors during binding creation
    /// and provides clear feedback about configuration issues.
    /// </remarks>
    public void Validate()
    {
        if (string.IsNullOrWhiteSpace(SourceExchange))
        {
            throw new BrokerConfigurationException(
                "Source exchange name cannot be null or empty.",
                configurationSection: nameof(BindingDeclaration),
                parameterName: nameof(SourceExchange),
                parameterValue: SourceExchange);
        }

        // Must have either destination queue or destination exchange, but not both
        var hasDestQueue = !string.IsNullOrWhiteSpace(DestinationQueue);
        var hasDestExchange = !string.IsNullOrWhiteSpace(DestinationExchange);

        if (!hasDestQueue && !hasDestExchange)
        {
            throw new BrokerConfigurationException(
                "Binding must specify either destination queue or destination exchange.",
                configurationSection: nameof(BindingDeclaration));
        }

        if (hasDestQueue && hasDestExchange)
        {
            throw new BrokerConfigurationException(
                "Binding cannot specify both destination queue and destination exchange.",
                configurationSection: nameof(BindingDeclaration));
        }

        // Validate routing key (can be empty for fanout exchanges)
        if (RoutingKey == null)
        {
            throw new BrokerConfigurationException(
                "Routing key cannot be null (use empty string for fanout exchanges).",
                configurationSection: nameof(BindingDeclaration),
                parameterName: nameof(RoutingKey));
        }

        // Validate arguments if present
        if (Arguments?.Any() == true)
        {
            ValidateArguments();
        }
    }

    /// <summary>
    /// Validates binding arguments for correctness and consistency.
    /// </summary>
    private void ValidateArguments()
    {
        if (Arguments == null) return;

        foreach (var argument in Arguments)
        {
            if (string.IsNullOrWhiteSpace(argument.Key))
            {
                throw new BrokerConfigurationException(
                    "Binding argument keys cannot be null or empty.",
                    configurationSection: nameof(BindingDeclaration),
                    parameterName: nameof(Arguments));
            }
        }

        // Validate headers exchange specific arguments
        if (Arguments.ContainsKey("x-match"))
        {
            var matchValue = Arguments["x-match"];
            if (matchValue is not string matchStr || (matchStr != "any" && matchStr != "all"))
            {
                throw new BrokerConfigurationException(
                    "Headers exchange 'x-match' argument must be 'any' or 'all'.",
                    configurationSection: nameof(BindingDeclaration),
                    parameterName: nameof(Arguments));
            }
        }
    }

    /// <summary>
    /// Returns a string representation of the binding declaration.
    /// </summary>
    /// <returns>A formatted string describing the binding configuration.</returns>
    public override string ToString()
    {
        var destination = !string.IsNullOrEmpty(DestinationQueue) 
            ? $"Queue '{DestinationQueue}'" 
            : $"Exchange '{DestinationExchange}'";
        
        var routingInfo = !string.IsNullOrEmpty(RoutingKey) 
            ? $" (key: '{RoutingKey}')" 
            : "";
        
        var argsInfo = Arguments?.Any() == true 
            ? $" [args: {Arguments.Count}]" 
            : "";

        return $"Binding: '{SourceExchange}' -> {destination}{routingInfo}{argsInfo}";
    }

    /// <summary>
    /// Determines whether two binding declarations are equivalent.
    /// </summary>
    /// <param name="obj">The object to compare with this declaration.</param>
    /// <returns><c>true</c> if the declarations are equivalent; otherwise, <c>false</c>.</returns>
    public override bool Equals(object? obj)
    {
        return obj is BindingDeclaration other &&
               SourceExchange == other.SourceExchange &&
               DestinationQueue == other.DestinationQueue &&
               DestinationExchange == other.DestinationExchange &&
               RoutingKey == other.RoutingKey;
    }

    /// <summary>
    /// Returns a hash code for the binding declaration.
    /// </summary>
    /// <returns>A hash code suitable for use in hash tables and collections.</returns>
    public override int GetHashCode()
    {
        return HashCode.Combine(SourceExchange, DestinationQueue, DestinationExchange, RoutingKey);
    }
}