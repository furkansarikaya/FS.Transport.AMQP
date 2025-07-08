namespace FS.RabbitMQ.Core.Abstractions;

/// <summary>
/// Exception thrown when broker configuration is invalid or incomplete.
/// </summary>
/// <remarks>
/// Configuration exceptions occur when the messaging infrastructure is configured
/// with invalid parameters, missing required settings, or incompatible options.
/// These exceptions help identify configuration issues that prevent proper
/// broker initialization and operation.
/// </remarks>
[Serializable]
public sealed class BrokerConfigurationException : MessagingException
{
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
    /// Gets the configuration section where the error occurred.
    /// </summary>
    /// <value>The section name (e.g., "Connection", "Exchange", "Queue"), or null if not applicable.</value>
    public string? ConfigurationSection { get; }

    /// <summary>
    /// Initializes a new instance of the BrokerConfigurationException class.
    /// </summary>
    /// <param name="message">The error message describing the configuration issue.</param>
    /// <param name="parameterName">The configuration parameter that caused the error.</param>
    /// <param name="parameterValue">The invalid parameter value.</param>
    /// <param name="configurationSection">The configuration section where the error occurred.</param>
    /// <param name="errorCode">Optional error code for programmatic error handling.</param>
    /// <param name="context">Optional additional context information.</param>
    public BrokerConfigurationException(string message, string? parameterName = null, object? parameterValue = null, string? configurationSection = null, string? errorCode = null, IReadOnlyDictionary<string, object>? context = null)
        : base(message, errorCode, context)
    {
        ParameterName = parameterName;
        ParameterValue = parameterValue;
        ConfigurationSection = configurationSection;
    }

    /// <summary>
    /// Initializes a new instance of the BrokerConfigurationException class.
    /// </summary>
    /// <param name="message">The error message describing the configuration issue.</param>
    /// <param name="innerException">The exception that revealed the configuration issue.</param>
    /// <param name="parameterName">The configuration parameter that caused the error.</param>
    /// <param name="parameterValue">The invalid parameter value.</param>
    /// <param name="configurationSection">The configuration section where the error occurred.</param>
    /// <param name="errorCode">Optional error code for programmatic error handling.</param>
    /// <param name="context">Optional additional context information.</param>
    public BrokerConfigurationException(string message, Exception innerException, string? parameterName = null, object? parameterValue = null, string? configurationSection = null, string? errorCode = null, IReadOnlyDictionary<string, object>? context = null)
        : base(message, innerException, errorCode, context)
    {
        ParameterName = parameterName;
        ParameterValue = parameterValue;
        ConfigurationSection = configurationSection;
    }

    /// <summary>
    /// Initializes a new instance of the BrokerConfigurationException class with serialized data.
    /// </summary>
    /// <param name="info">The SerializationInfo that holds the serialized object data.</param>
    /// <param name="context">The StreamingContext that contains contextual information.</param>
    private BrokerConfigurationException(System.Runtime.Serialization.SerializationInfo info, System.Runtime.Serialization.StreamingContext context)
        : base(info, context)
    {
        ParameterName = info.GetString(nameof(ParameterName));
        ParameterValue = info.GetValue(nameof(ParameterValue), typeof(object));
        ConfigurationSection = info.GetString(nameof(ConfigurationSection));
    }
}
