namespace FS.RabbitMQ.Core.Configuration;

/// <summary>
/// Defines credential validation policies for different security requirements.
/// </summary>
public enum CredentialValidationPolicy
{
    /// <summary>
    /// Relaxed validation suitable for development environments.
    /// </summary>
    Relaxed,

    /// <summary>
    /// Standard validation with basic security checks.
    /// </summary>
    Standard,

    /// <summary>
    /// Strict validation with comprehensive security verification.
    /// </summary>
    Strict,

    /// <summary>
    /// Custom validation using application-defined logic.
    /// </summary>
    Custom
}