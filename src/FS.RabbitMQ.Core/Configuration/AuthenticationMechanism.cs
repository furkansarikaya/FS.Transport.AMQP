namespace FS.RabbitMQ.Core.Configuration;

/// <summary>
/// Defines the authentication mechanisms supported by the broker.
/// </summary>
public enum AuthenticationMechanism
{
    /// <summary>
    /// Plain text username and password authentication.
    /// </summary>
    Plain,

    /// <summary>
    /// Certificate-based authentication using client certificates.
    /// </summary>
    Certificate,

    /// <summary>
    /// External authentication through third-party providers.
    /// </summary>
    External,

    /// <summary>
    /// Custom authentication mechanism.
    /// </summary>
    Custom
}