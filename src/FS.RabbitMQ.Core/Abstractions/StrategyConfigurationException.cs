namespace FS.RabbitMQ.Core.Abstractions;

/// <summary>
/// Exception thrown when processing strategy configuration is invalid or incomplete.
/// </summary>
/// <remarks>
/// Strategy configuration exceptions occur when custom processing strategies
/// cannot be properly configured due to missing parameters, invalid values,
/// or incompatible settings. These exceptions help identify configuration
/// issues that prevent strategy initialization and operation.
/// </remarks>
[Serializable]
public sealed class StrategyConfigurationException : MessagingException
{
    /// <summary>
    /// Gets the processing strategy identifier with the configuration issue.
    /// </summary>
    /// <value>The strategy ID, or null if not available.</value>
    public string? StrategyId { get; }

    /// <summary>
    /// Gets the configuration parameter that caused the error.
    /// </summary>
    /// <value>The parameter name, or null if not specific to a single parameter.</value>
    public string? ParameterName { get; }

    /// <summary>
    /// Gets the invalid configuration value that caused the error.
    /// </summary>
    /// <value>The parameter value, or null if not applicable.</value>
    public object? ParameterValue { get; }

    /// <summary>
    /// Initializes a new instance of the StrategyConfigurationException class.
    /// </summary>
    /// <param name="message">The error message describing the configuration issue.</param>
    /// <param name="strategyId">The processing strategy identifier.</param>
    /// <param name="parameterName">The configuration parameter that caused the error.</param>
    /// <param name="parameterValue">The invalid parameter value.</param>
    /// <param name="errorCode">Optional error code for programmatic error handling.</param>
    /// <param name="context">Optional additional context information.</param>
    public StrategyConfigurationException(string message, string? strategyId = null, string? parameterName = null, object? parameterValue = null, string? errorCode = null, IReadOnlyDictionary<string, object>? context = null)
        : base(message, errorCode, context)
    {
        StrategyId = strategyId;
        ParameterName = parameterName;
        ParameterValue = parameterValue;
    }

    /// <summary>
    /// Initializes a new instance of the StrategyConfigurationException class.
    /// </summary>
    /// <param name="message">The error message describing the configuration issue.</param>
    /// <param name="innerException">The exception that revealed the configuration issue.</param>
    /// <param name="strategyId">The processing strategy identifier.</param>
    /// <param name="parameterName">The configuration parameter that caused the error.</param>
    /// <param name="parameterValue">The invalid parameter value.</param>
    /// <param name="errorCode">Optional error code for programmatic error handling.</param>
    /// <param name="context">Optional additional context information.</param>
    public StrategyConfigurationException(string message, Exception innerException, string? strategyId = null, string? parameterName = null, object? parameterValue = null, string? errorCode = null, IReadOnlyDictionary<string, object>? context = null)
        : base(message, innerException, errorCode, context)
    {
        StrategyId = strategyId;
        ParameterName = parameterName;
        ParameterValue = parameterValue;
    }

    /// <summary>
    /// Initializes a new instance of the StrategyConfigurationException class with serialized data.
    /// </summary>
    /// <param name="info">The SerializationInfo that holds the serialized object data.</param>
    /// <param name="context">The StreamingContext that contains contextual information.</param>
    private StrategyConfigurationException(System.Runtime.Serialization.SerializationInfo info, System.Runtime.Serialization.StreamingContext context)
        : base(info, context)
    {
        StrategyId = info.GetString(nameof(StrategyId));
        ParameterName = info.GetString(nameof(ParameterName));
        ParameterValue = info.GetValue(nameof(ParameterValue), typeof(object));
    }
}