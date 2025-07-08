namespace FS.RabbitMQ.Core.Configuration;

/// <summary>
/// Specifies the name of the X.509 certificate store to open.
/// </summary>
/// <remarks>
/// Store names organize certificates by their intended purpose and trust level.
/// This maps directly to the .NET X509Store names for consistent behavior.
/// </remarks>
public enum StoreName
{
    /// <summary>
    /// The certificate store for other users.
    /// </summary>
    AddressBook = System.Security.Cryptography.X509Certificates.StoreName.AddressBook,

    /// <summary>
    /// The certificate store for third-party certificate authorities (CAs).
    /// </summary>
    AuthRoot = System.Security.Cryptography.X509Certificates.StoreName.AuthRoot,

    /// <summary>
    /// The certificate store for intermediate certificate authorities (CAs).
    /// </summary>
    CertificateAuthority = System.Security.Cryptography.X509Certificates.StoreName.CertificateAuthority,

    /// <summary>
    /// The certificate store for revoked certificates.
    /// </summary>
    Disallowed = System.Security.Cryptography.X509Certificates.StoreName.Disallowed,

    /// <summary>
    /// The certificate store for personal certificates.
    /// </summary>
    My = System.Security.Cryptography.X509Certificates.StoreName.My,

    /// <summary>
    /// The certificate store for trusted root certificate authorities (CAs).
    /// </summary>
    Root = System.Security.Cryptography.X509Certificates.StoreName.Root,

    /// <summary>
    /// The certificate store for trusted people and resources.
    /// </summary>
    TrustedPeople = System.Security.Cryptography.X509Certificates.StoreName.TrustedPeople,

    /// <summary>
    /// The certificate store for trusted publishers.
    /// </summary>
    TrustedPublisher = System.Security.Cryptography.X509Certificates.StoreName.TrustedPublisher
}
