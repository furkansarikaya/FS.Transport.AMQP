namespace FS.RabbitMQ.Core.Abstractions;

/// <summary>
/// Defines the types of errors that can occur during message publishing operations.
/// </summary>
/// <remarks>
/// Error type classification enables appropriate error handling strategies and helps
/// applications implement targeted responses to different failure conditions.
/// Each error type has different characteristics regarding recoverability,
/// appropriate retry strategies, and required interventions.
/// </remarks>
public enum PublishErrorType
{
    /// <summary>
    /// Network connectivity or communication error occurred during publishing.
    /// These errors are typically recoverable with retry after a delay.
    /// </summary>
    /// <remarks>
    /// Examples include:
    /// - Network timeouts or connection drops
    /// - DNS resolution failures
    /// - Socket errors and communication interruptions
    /// - Temporary broker unavailability
    /// 
    /// Recovery strategy: Retry with exponential backoff
    /// </remarks>
    NetworkError,

    /// <summary>
    /// Broker rejected the message due to policy or capacity limitations.
    /// Recovery depends on the specific rejection reason and may require intervention.
    /// </summary>
    /// <remarks>
    /// Examples include:
    /// - Queue length limits exceeded
    /// - Message size limits exceeded
    /// - Broker resource exhaustion
    /// - Flow control restrictions
    /// 
    /// Recovery strategy: Delayed retry or alternative routing
    /// </remarks>
    BrokerRejection,

    /// <summary>
    /// Message could not be routed to any queue due to routing configuration.
    /// Usually not recoverable without topology or configuration changes.
    /// </summary>
    /// <remarks>
    /// Examples include:
    /// - No matching bindings for the routing key
    /// - Exchange does not exist
    /// - Invalid routing key format
    /// - Misconfigured exchange types
    /// 
    /// Recovery strategy: Verify topology and routing configuration
    /// </remarks>
    RoutingFailure,

    /// <summary>
    /// Authentication or authorization failure prevented publishing.
    /// Not recoverable without credential or permission updates.
    /// </summary>
    /// <remarks>
    /// Examples include:
    /// - Invalid credentials or expired tokens
    /// - Insufficient permissions for exchange access
    /// - User account disabled or suspended
    /// - Resource access restrictions
    /// 
    /// Recovery strategy: Update credentials or permissions
    /// </remarks>
    AuthenticationFailure,

    /// <summary>
    /// Message serialization failed due to data format or compatibility issues.
    /// Usually not recoverable without message format changes.
    /// </summary>
    /// <remarks>
    /// Examples include:
    /// - Serialization library errors
    /// - Unsupported data types
    /// - Circular reference issues
    /// - Encoding or format compatibility problems
    /// 
    /// Recovery strategy: Fix message format or serialization configuration
    /// </remarks>
    SerializationError,

    /// <summary>
    /// Invalid configuration or parameters provided for the publishing operation.
    /// Not recoverable without correcting the configuration.
    /// </summary>
    /// <remarks>
    /// Examples include:
    /// - Invalid exchange or queue names
    /// - Malformed routing keys
    /// - Incompatible publishing options
    /// - Missing required parameters
    /// 
    /// Recovery strategy: Correct configuration parameters
    /// </remarks>
    ConfigurationError,

    /// <summary>
    /// Timeout occurred while waiting for broker confirmation or response.
    /// May be recoverable with retry or timeout adjustment.
    /// </summary>
    /// <remarks>
    /// Examples include:
    /// - Publisher confirmation timeout
    /// - Request-response timeout
    /// - Channel operation timeout
    /// - Connection establishment timeout
    /// 
    /// Recovery strategy: Retry with longer timeout or check broker health
    /// </remarks>
    Timeout,

    /// <summary>
    /// An unexpected or unclassified error occurred during publishing.
    /// Recoverability depends on the specific underlying cause.
    /// </summary>
    /// <remarks>
    /// Examples include:
    /// - Unexpected exceptions in publishing pipeline
    /// - System resource exhaustion
    /// - Hardware or infrastructure failures
    /// - Software bugs or version compatibility issues
    /// 
    /// Recovery strategy: Investigate underlying cause and apply appropriate fix
    /// </remarks>
    UnknownError
}