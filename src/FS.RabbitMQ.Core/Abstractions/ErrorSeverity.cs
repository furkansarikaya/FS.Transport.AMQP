namespace FS.RabbitMQ.Core.Abstractions;

/// <summary>
/// Defines the severity levels for broker errors.
/// </summary>
/// <remarks>
/// Error severity classification enables appropriate response strategies and helps
/// prioritize operational attention. The severity levels are designed to align with
/// common incident management and monitoring practices in enterprise environments.
/// </remarks>
public enum ErrorSeverity
{
    /// <summary>
    /// Informational events that don't indicate errors but provide useful operational insight.
    /// These events typically don't require any action but should be logged for audit purposes.
    /// </summary>
    /// <remarks>
    /// Examples of informational events:
    /// - Successful connection establishment after retry
    /// - Automatic recovery completion
    /// - Configuration changes taking effect
    /// - Performance metrics reaching informational thresholds
    /// </remarks>
    Informational = 0,

    /// <summary>
    /// Low-severity issues that don't significantly impact functionality.
    /// These can typically be addressed during routine maintenance windows.
    /// </summary>
    /// <remarks>
    /// Examples of low-severity errors:
    /// - Temporary performance degradation
    /// - Non-critical configuration warnings
    /// - Recoverable resource constraints
    /// - Minor protocol irregularities that are automatically handled
    /// </remarks>
    Low = 1,

    /// <summary>
    /// Medium-severity issues that may impact some functionality but don't prevent core operations.
    /// These should be addressed during business hours but don't require immediate emergency response.
    /// </summary>
    /// <remarks>
    /// Examples of medium-severity errors:
    /// - Intermittent connectivity issues
    /// - Non-critical service degradation
    /// - Resource usage approaching limits
    /// - Configuration issues affecting secondary features
    /// </remarks>
    Medium = 2,

    /// <summary>
    /// High-severity issues that significantly impact functionality and require urgent attention.
    /// These may affect core system operations and should trigger immediate response procedures.
    /// </summary>
    /// <remarks>
    /// Examples of high-severity errors:
    /// - Persistent connection failures
    /// - Major performance degradation
    /// - Critical resource exhaustion
    /// - Security-related incidents
    /// </remarks>
    High = 3,

    /// <summary>
    /// Critical issues that render the system unusable or pose significant risk.
    /// These require immediate emergency response and may trigger escalation procedures.
    /// </summary>
    /// <remarks>
    /// Examples of critical errors:
    /// - Complete system unavailability
    /// - Data corruption or loss risks
    /// - Security breaches or vulnerabilities
    /// - Catastrophic infrastructure failures
    /// </remarks>
    Critical = 4
}