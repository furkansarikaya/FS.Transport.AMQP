namespace FS.RabbitMQ.Core.Configuration;

/// <summary>
/// Defines the sources from which client certificates can be loaded.
/// </summary>
/// <remarks>
/// Different certificate sources address different operational requirements,
/// security policies, and deployment strategies. The choice of source affects
/// certificate management, security characteristics, and operational complexity.
/// </remarks>
public enum CertificateSource
{
    /// <summary>
    /// Load the certificate from the operating system's certificate store.
    /// Provides the highest security and best integration with enterprise certificate management.
    /// </summary>
    Store,

    /// <summary>
    /// Load the certificate from a file on the file system.
    /// Provides flexibility and compatibility with various deployment strategies.
    /// </summary>
    File
}