namespace FS.RabbitMQ.Core.Configuration;

/// <summary>
/// Defines the TLS protocol versions that are acceptable for secure connections.
/// </summary>
/// <remarks>
/// TLS version policies provide a way to enforce security standards while
/// maintaining compatibility with different deployment environments.
/// Different policies represent different balances between security and compatibility.
/// </remarks>
public enum TlsVersionPolicy
{
    /// <summary>
    /// Allow TLS 1.1 and above. Provides broad compatibility but includes deprecated protocols.
    /// </summary>
    /// <remarks>
    /// Use only when compatibility with very old systems is required.
    /// TLS 1.1 has known security weaknesses and should be avoided in production.
    /// </remarks>
    Tls11AndAbove,

    /// <summary>
    /// Allow TLS 1.2 and above. Recommended for most production environments.
    /// </summary>
    /// <remarks>
    /// Balances security and compatibility. TLS 1.2 is widely supported and secure
    /// when properly configured. This is the recommended setting for most deployments.
    /// </remarks>
    Tls12AndAbove,

    /// <summary>
    /// Allow only TLS 1.3. Provides the highest security but may have compatibility limitations.
    /// </summary>
    /// <remarks>
    /// Use when maximum security is required and all components support TLS 1.3.
    /// TLS 1.3 provides better security and performance than earlier versions.
    /// </remarks>
    Tls13Only
}