namespace FS.RabbitMQ.Core.Abstractions;

/// <summary>
/// Provides a fluent API for building QueueBinding instances with method chaining.
/// </summary>
/// <remarks>
/// The QueueBindingBuilder is particularly valuable for complex binding scenarios,
/// especially with headers exchanges that require multiple arguments, or when
/// creating multiple similar bindings with programmatic variations.
/// 
/// While simple bindings can be created directly using QueueBinding factory methods,
/// the builder pattern provides enhanced flexibility for:
/// - Headers exchange configurations with multiple header criteria
/// - Conditional binding argument application based on runtime conditions
/// - Template-based binding creation with parameter substitution
/// - Complex routing scenarios requiring argument customization
/// 
/// Example usage:
/// <code>
/// var binding = QueueBinding
///     .Create("notifications", "user.events", "user.*.priority.*")
///     .ForHeadersMatching()
///     .WithMatchAll()
///     .WithHeader("format", "json")
///     .WithHeader("priority", "high")
///     .Build();
/// </code>
/// </remarks>
public sealed class QueueBindingBuilder
{
    private readonly string _queueName;
    private readonly string _exchangeName;
    private readonly string _routingKey;
    private Dictionary<string, object>? _arguments;

    /// <summary>
    /// Initializes a new instance of the QueueBindingBuilder class.
    /// </summary>
    /// <param name="queueName">The name of the queue to bind.</param>
    /// <param name="exchangeName">The name of the exchange to bind to.</param>
    /// <param name="routingKey">The routing key or pattern for the binding.</param>
    /// <remarks>
    /// This constructor is internal to ensure that instances are created only through
    /// the QueueBinding.Create() factory method, which provides input validation
    /// and consistent builder initialization.
    /// </remarks>
    internal QueueBindingBuilder(string queueName, string exchangeName, string routingKey)
    {
        _queueName = queueName;
        _exchangeName = exchangeName;
        _routingKey = routingKey;
    }

    /// <summary>
    /// Configures the binding for headers exchange matching with 'any' criteria.
    /// </summary>
    /// <returns>The current builder instance for method chaining.</returns>
    /// <remarks>
    /// The 'any' match criteria means that a message will be routed to the queue
    /// if ANY of the specified header conditions are met. This provides flexible
    /// message filtering where multiple different message types can be routed
    /// to the same queue.
    /// 
    /// Use 'any' matching when:
    /// - Multiple message types should be processed by the same consumer
    /// - You want to implement OR-logic for header-based routing
    /// - Flexibility is more important than precise message filtering
    /// 
    /// Example: A queue processing both "urgent" and "error" messages would use
    /// 'any' matching with headers for both priority levels.
    /// </remarks>
    public QueueBindingBuilder WithMatchAny()
    {
        return WithArgument("x-match", "any");
    }

    /// <summary>
    /// Configures the binding for headers exchange matching with 'all' criteria.
    /// </summary>
    /// <returns>The current builder instance for method chaining.</returns>
    /// <remarks>
    /// The 'all' match criteria means that a message will be routed to the queue
    /// only if ALL of the specified header conditions are met. This provides
    /// precise message filtering where multiple conditions must be satisfied
    /// simultaneously.
    /// 
    /// Use 'all' matching when:
    /// - Precise message filtering with multiple criteria is required
    /// - You want to implement AND-logic for header-based routing
    /// - Message processing requires strict adherence to multiple conditions
    /// 
    /// Example: A queue processing only "urgent AND financial" messages would use
    /// 'all' matching with headers for both priority and category.
    /// </remarks>
    public QueueBindingBuilder WithMatchAll()
    {
        return WithArgument("x-match", "all");
    }

    /// <summary>
    /// Adds a header matching criterion for headers exchange routing.
    /// </summary>
    /// <param name="headerName">The name of the header to match. Cannot be null or whitespace.</param>
    /// <param name="expectedValue">The expected value for the header. Cannot be null.</param>
    /// <returns>The current builder instance for method chaining.</returns>
    /// <exception cref="ArgumentException">
    /// Thrown when <paramref name="headerName"/> is null, empty, or whitespace.
    /// </exception>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="expectedValue"/> is null.</exception>
    /// <remarks>
    /// Header matching enables sophisticated message routing based on message metadata:
    /// - Headers are case-sensitive for both names and values
    /// - Header values can be strings, numbers, or other serializable types
    /// - Multiple headers can be specified to create complex matching rules
    /// - Works in conjunction with match criteria (any/all) to determine routing logic
    /// 
    /// Common header-based routing scenarios:
    /// - Message priority or urgency levels
    /// - Content type or format specifications
    /// - Source system or tenant identification
    /// - Processing requirements or special handling flags
    /// 
    /// Headers provide more flexibility than routing keys for complex message
    /// classification and routing scenarios.
    /// </remarks>
    public QueueBindingBuilder WithHeader(string headerName, object expectedValue)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(headerName);
        ArgumentNullException.ThrowIfNull(expectedValue);

        return WithArgument(headerName, expectedValue);
    }

    /// <summary>
    /// Adds multiple header matching criteria for headers exchange routing.
    /// </summary>
    /// <param name="headers">A dictionary containing header name-value pairs to match. Cannot be null.</param>
    /// <returns>The current builder instance for method chaining.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="headers"/> is null.</exception>
    /// <exception cref="ArgumentException">
    /// Thrown when any header name is null, empty, or whitespace, or when any value is null.
    /// </exception>
    /// <remarks>
    /// Bulk header addition is useful for:
    /// - Applying header templates from configuration
    /// - Creating bindings with many header criteria
    /// - Implementing standardized routing rules across multiple bindings
    /// 
    /// All specified headers will be used for matching according to the configured
    /// match criteria (any/all). This method validates all headers before applying
    /// any changes, ensuring atomic operation.
    /// </remarks>
    public QueueBindingBuilder WithHeaders(IReadOnlyDictionary<string, object> headers)
    {
        ArgumentNullException.ThrowIfNull(headers);

        // Validate all headers before applying any changes
        foreach (var kvp in headers)
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(kvp.Key);
            ArgumentNullException.ThrowIfNull(kvp.Value);
        }

        _arguments ??= new Dictionary<string, object>();

        foreach (var kvp in headers)
        {
            _arguments[kvp.Key] = kvp.Value;
        }

        return this;
    }

    /// <summary>
    /// Adds a custom argument to the binding configuration.
    /// </summary>
    /// <param name="key">The argument key. Cannot be null or whitespace.</param>
    /// <param name="value">The argument value. Cannot be null.</param>
    /// <returns>The current builder instance for method chaining.</returns>
    /// <exception cref="ArgumentException">
    /// Thrown when <paramref name="key"/> is null, empty, or whitespace.
    /// </exception>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="value"/> is null.</exception>
    /// <remarks>
    /// Custom arguments enable advanced binding features and broker-specific functionality:
    /// 
    /// Standard arguments:
    /// - "x-match": Controls header matching behavior ("any" or "all")
    /// - Header names and values: Define matching criteria for headers exchanges
    /// 
    /// Plugin-specific arguments may include:
    /// - Custom routing behaviors
    /// - Message transformation parameters
    /// - Advanced filtering criteria
    /// 
    /// Always verify argument support in your target RabbitMQ environment,
    /// as custom arguments may require specific plugins or broker versions.
    /// </remarks>
    public QueueBindingBuilder WithArgument(string key, object value)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(key);
        ArgumentNullException.ThrowIfNull(value);

        _arguments ??= new Dictionary<string, object>();
        _arguments[key] = value;
        return this;
    }

    /// <summary>
    /// Adds multiple custom arguments to the binding configuration.
    /// </summary>
    /// <param name="arguments">A dictionary containing argument key-value pairs. Cannot be null.</param>
    /// <returns>The current builder instance for method chaining.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="arguments"/> is null.</exception>
    /// <exception cref="ArgumentException">
    /// Thrown when any argument key is null, empty, or whitespace, or when any value is null.
    /// </exception>
    /// <remarks>
    /// Bulk argument addition is convenient for:
    /// - Loading binding configuration from external sources
    /// - Applying argument templates to multiple bindings
    /// - Implementing complex binding configurations with many parameters
    /// 
    /// This method validates all arguments before applying any changes,
    /// ensuring atomic operation and consistent error handling.
    /// </remarks>
    public QueueBindingBuilder WithArguments(IReadOnlyDictionary<string, object> arguments)
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
    /// Configures the binding for headers exchange routing with the specified match criteria and headers.
    /// </summary>
    /// <param name="matchAll">
    /// <c>true</c> to require all headers to match (AND logic);
    /// <c>false</c> to require any header to match (OR logic).
    /// </param>
    /// <param name="headers">The headers to match against incoming messages. Cannot be null.</param>
    /// <returns>The current builder instance for method chaining.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="headers"/> is null.</exception>
    /// <exception cref="ArgumentException">
    /// Thrown when <paramref name="headers"/> is empty, or when any header name is null, empty, or whitespace,
    /// or when any header value is null.
    /// </exception>
    /// <remarks>
    /// This convenience method configures headers exchange routing in a single call,
    /// combining match criteria setup with header specification. It's particularly
    /// useful when the complete header matching configuration is known upfront.
    /// 
    /// The method automatically sets the appropriate "x-match" value and adds all
    /// specified headers as matching criteria. This ensures consistent configuration
    /// and reduces the chance of configuration errors.
    /// </remarks>
    public QueueBindingBuilder ForHeadersMatching(bool matchAll, IReadOnlyDictionary<string, object> headers)
    {
        ArgumentNullException.ThrowIfNull(headers);

        if (!headers.Any())
        {
            throw new ArgumentException("At least one header must be specified for headers exchange matching.", nameof(headers));
        }

        var matchCriteria = matchAll ? "all" : "any";
        WithArgument("x-match", matchCriteria);
        
        return WithHeaders(headers);
    }

    /// <summary>
    /// Builds and returns the configured QueueBinding instance.
    /// </summary>
    /// <returns>A new QueueBinding instance with the specified configuration.</returns>
    /// <exception cref="InvalidOperationException">
    /// Thrown when the current configuration is invalid or incomplete.
    /// </exception>
    /// <remarks>
    /// The build process performs comprehensive validation of the binding configuration,
    /// including argument consistency, headers exchange requirements, and format validation.
    /// 
    /// Specific validation includes:
    /// - Headers exchange configurations must have both "x-match" and header criteria
    /// - Argument names and values must be properly formatted
    /// - Queue and exchange names must be valid
    /// 
    /// The resulting QueueBinding is immutable and thread-safe, making it suitable
    /// for use across multiple threads and operations without additional synchronization.
    /// </remarks>
    public QueueBinding Build()
    {
        var binding = new QueueBinding
        {
            QueueName = _queueName,
            ExchangeName = _exchangeName,
            RoutingKey = _routingKey,
            Arguments = _arguments?.AsReadOnly()
        };

        // Perform validation before returning the instance
        binding.Validate();

        return binding;
    }
}