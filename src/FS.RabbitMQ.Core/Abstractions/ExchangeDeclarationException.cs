namespace FS.RabbitMQ.Core.Abstractions;

/// <summary>
/// Exception thrown when exchange declaration operations fail.
/// </summary>
/// <remarks>
/// Exchange declaration exceptions occur when exchanges cannot be created or
/// validated due to name conflicts, parameter mismatches, or broker policy
/// restrictions. These exceptions help identify topology-related issues.
/// </remarks>
[Serializable]
public sealed class ExchangeDeclarationException : MessagingException
{
    /// <summary>
    /// Gets the name of the exchange that failed to declare.
    /// </summary>
    /// <value>The exchange name, or null if not available.</value>
    public string? ExchangeName { get; }

    /// <summary>
    /// Gets the exchange type that was being declared.
    /// </summary>
    /// <value>The exchange type, or null if not available.</value>
    public ExchangeType? ExchangeType { get; }

    /// <summary>
    /// Initializes a new instance of the ExchangeDeclarationException class.
    /// </summary>
    /// <param name="message">The error message describing the declaration failure.</param>
    /// <param name="exchangeName">The name of the exchange that failed to declare.</param>
    /// <param name="exchangeType">The type of exchange that was being declared.</param>
    /// <param name="errorCode">Optional error code for programmatic error handling.</param>
    /// <param name="context">Optional additional context information.</param>
    public ExchangeDeclarationException(string message, string? exchangeName = null, ExchangeType? exchangeType = null, string? errorCode = null, IReadOnlyDictionary<string, object>? context = null)
        : base(message, errorCode, context)
    {
        ExchangeName = exchangeName;
        ExchangeType = exchangeType;
    }

    /// <summary>
    /// Initializes a new instance of the ExchangeDeclarationException class.
    /// </summary>
    /// <param name="message">The error message describing the declaration failure.</param>
    /// <param name="innerException">The exception that caused the declaration failure.</param>
    /// <param name="exchangeName">The name of the exchange that failed to declare.</param>
    /// <param name="exchangeType">The type of exchange that was being declared.</param>
    /// <param name="errorCode">Optional error code for programmatic error handling.</param>
    /// <param name="context">Optional additional context information.</param>
    public ExchangeDeclarationException(string message, Exception innerException, string? exchangeName = null, ExchangeType? exchangeType = null, string? errorCode = null, IReadOnlyDictionary<string, object>? context = null)
        : base(message, innerException, errorCode, context)
    {
        ExchangeName = exchangeName;
        ExchangeType = exchangeType;
    }

    /// <summary>
    /// Initializes a new instance of the ExchangeDeclarationException class with serialized data.
    /// </summary>
    /// <param name="info">The SerializationInfo that holds the serialized object data.</param>
    /// <param name="context">The StreamingContext that contains contextual information.</param>
    private ExchangeDeclarationException(System.Runtime.Serialization.SerializationInfo info, System.Runtime.Serialization.StreamingContext context)
        : base(info, context)
    {
        ExchangeName = info.GetString(nameof(ExchangeName));
        // Note: Enum serialization would need special handling
    }
}