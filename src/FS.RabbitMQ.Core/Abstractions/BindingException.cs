namespace FS.RabbitMQ.Core.Abstractions;

/// <summary>
/// Exception thrown when queue binding operations fail.
/// </summary>
/// <remarks>
/// Binding exceptions occur when queue-to-exchange bindings cannot be created
/// due to missing exchanges or queues, parameter conflicts, or permission issues.
/// These exceptions help identify binding-specific topology problems.
/// </remarks>
[Serializable]
public sealed class BindingException : MessagingException
{
    /// <summary>
    /// Gets the name of the queue involved in the failed binding.
    /// </summary>
    /// <value>The queue name, or null if not available.</value>
    public string? QueueName { get; }

    /// <summary>
    /// Gets the name of the exchange involved in the failed binding.
    /// </summary>
    /// <value>The exchange name, or null if not available.</value>
    public string? ExchangeName { get; }

    /// <summary>
    /// Gets the routing key used in the failed binding.
    /// </summary>
    /// <value>The routing key, or null if not available.</value>
    public string? RoutingKey { get; }

    /// <summary>
    /// Initializes a new instance of the BindingException class.
    /// </summary>
    /// <param name="message">The error message describing the binding failure.</param>
    /// <param name="queueName">The name of the queue involved in the binding.</param>
    /// <param name="exchangeName">The name of the exchange involved in the binding.</param>
    /// <param name="routingKey">The routing key used in the binding.</param>
    /// <param name="errorCode">Optional error code for programmatic error handling.</param>
    /// <param name="context">Optional additional context information.</param>
    public BindingException(string message, string? queueName = null, string? exchangeName = null, string? routingKey = null, string? errorCode = null, IReadOnlyDictionary<string, object>? context = null)
        : base(message, errorCode, context)
    {
        QueueName = queueName;
        ExchangeName = exchangeName;
        RoutingKey = routingKey;
    }

    /// <summary>
    /// Initializes a new instance of the BindingException class.
    /// </summary>
    /// <param name="message">The error message describing the binding failure.</param>
    /// <param name="innerException">The exception that caused the binding failure.</param>
    /// <param name="queueName">The name of the queue involved in the binding.</param>
    /// <param name="exchangeName">The name of the exchange involved in the binding.</param>
    /// <param name="routingKey">The routing key used in the binding.</param>
    /// <param name="errorCode">Optional error code for programmatic error handling.</param>
    /// <param name="context">Optional additional context information.</param>
    public BindingException(string message, Exception innerException, string? queueName = null, string? exchangeName = null, string? routingKey = null, string? errorCode = null, IReadOnlyDictionary<string, object>? context = null)
        : base(message, innerException, errorCode, context)
    {
        QueueName = queueName;
        ExchangeName = exchangeName;
        RoutingKey = routingKey;
    }

    /// <summary>
    /// Initializes a new instance of the BindingException class with serialized data.
    /// </summary>
    /// <param name="info">The SerializationInfo that holds the serialized object data.</param>
    /// <param name="context">The StreamingContext that contains contextual information.</param>
    private BindingException(System.Runtime.Serialization.SerializationInfo info, System.Runtime.Serialization.StreamingContext context)
        : base(info, context)
    {
        QueueName = info.GetString(nameof(QueueName));
        ExchangeName = info.GetString(nameof(ExchangeName));
        RoutingKey = info.GetString(nameof(RoutingKey));
    }
}