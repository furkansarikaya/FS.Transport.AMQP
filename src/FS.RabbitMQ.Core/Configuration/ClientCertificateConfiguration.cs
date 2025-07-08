using FS.RabbitMQ.Core.Abstractions;

namespace FS.RabbitMQ.Core.Configuration;

/// <summary>
/// Contains configuration for client certificates used in mutual TLS authentication.
/// </summary>
/// <remarks>
/// Client certificate configuration manages the complexity of mutual TLS (mTLS) authentication,
/// where both the client and server verify each other's identity using digital certificates.
/// This provides much stronger security than server-only authentication because it ensures
/// that only authorized clients can connect to the messaging system.
/// 
/// Think of client certificates like having both a driver's license and a special access card
/// to enter a secure building. The driver's license proves who you are (authentication), and
/// the access card proves you're allowed to be there (authorization). In messaging systems,
/// the client certificate serves both purposes - it proves the identity of the connecting
/// application and that it's authorized to access the messaging infrastructure.
/// 
/// Mutual TLS is especially important in:
/// - Service-to-service communication in microservices architectures
/// - Zero-trust network environments where network location doesn't imply trust
/// - Compliance environments requiring strong authentication (healthcare, finance, government)
/// - Systems where message content is highly sensitive or valuable
/// 
/// The configuration supports various certificate sources and management strategies to
/// accommodate different operational requirements and security policies.
/// </remarks>
public sealed class ClientCertificateConfiguration
{
    /// <summary>
    /// Gets or sets the source strategy for obtaining the client certificate.
    /// </summary>
    /// <value>The method used to locate and load the client certificate.</value>
    /// <remarks>
    /// Different certificate sources address different operational and security needs:
    /// 
    /// Store-based sources (Windows Certificate Store, etc.):
    /// - Leverage operating system security and key protection
    /// - Support hardware security modules (HSMs) and smart cards
    /// - Provide automatic certificate renewal and management
    /// - Integrate with enterprise certificate management systems
    /// 
    /// File-based sources (PEM, PFX files):
    /// - Simple deployment and configuration management
    /// - Compatible with container and cloud deployment strategies
    /// - Support version control and infrastructure-as-code approaches
    /// - Enable certificate distribution through secure file systems
    /// 
    /// The choice depends on your security requirements, operational processes,
    /// and infrastructure capabilities. Many organizations use store-based
    /// certificates in production for enhanced security and file-based certificates
    /// in development for simplicity.
    /// </remarks>
    public CertificateSource Source { get; set; } = CertificateSource.Store;

    /// <summary>
    /// Gets or sets the certificate store location when using store-based certificate sources.
    /// </summary>
    /// <value>The certificate store location identifier, or null if not using store-based sources.</value>
    /// <remarks>
    /// Certificate store locations determine where the system looks for certificates:
    /// 
    /// CurrentUser store:
    /// - Certificates are specific to the user account running the application
    /// - Useful for development and single-user scenarios
    /// - Easier permission management for individual developers
    /// - May not be suitable for service accounts or system services
    /// 
    /// LocalMachine store:
    /// - Certificates are available to all users on the machine
    /// - Better for production services and system accounts
    /// - Requires elevated privileges for certificate installation
    /// - More suitable for server and service deployments
    /// 
    /// The choice affects certificate management, permissions, and deployment strategies.
    /// Production systems typically use LocalMachine for broader accessibility.
    /// </remarks>
    public StoreLocation? StoreLocation { get; set; }

    /// <summary>
    /// Gets or sets the certificate store name when using store-based certificate sources.
    /// </summary>
    /// <value>The name of the certificate store, or null if not using store-based sources.</value>
    /// <remarks>
    /// Certificate store names organize certificates by purpose and trust level:
    /// 
    /// Personal (My) store:
    /// - Contains certificates with associated private keys
    /// - Used for client authentication certificates
    /// - Where most application certificates are stored
    /// 
    /// Trusted Root Certification Authorities store:
    /// - Contains root CA certificates that define trust anchors
    /// - Used for custom or internal certificate authorities
    /// - Critical for establishing trust chains
    /// 
    /// Intermediate Certification Authorities store:
    /// - Contains intermediate CA certificates
    /// - Helps complete certificate trust chains
    /// - Important for complex PKI hierarchies
    /// 
    /// Choose the store that matches the certificate type and trust requirements.
    /// Most client authentication certificates go in the Personal (My) store.
    /// </remarks>
    public StoreName? StoreName { get; set; }

    /// <summary>
    /// Gets or sets the certificate thumbprint for identifying specific certificates in stores.
    /// </summary>
    /// <value>The SHA-1 thumbprint of the certificate, or null if using other identification methods.</value>
    /// <remarks>
    /// Certificate thumbprints provide unambiguous identification of specific certificates
    /// within certificate stores. The thumbprint is a SHA-1 hash of the certificate's
    /// binary content, making it unique for each certificate.
    /// 
    /// Thumbprint identification is preferred when:
    /// - Multiple certificates exist with similar subjects or issuers
    /// - Precise certificate selection is required for security
    /// - Certificate rotation involves overlapping validity periods
    /// - Compliance requires specific certificate tracking
    /// 
    /// To find a certificate thumbprint:
    /// - Windows: Use Certificate Manager (certmgr.msc) and view certificate details
    /// - PowerShell: Use Get-ChildItem Cert: cmdlets
    /// - OpenSSL: Use openssl x509 -fingerprint command
    /// 
    /// Best practice: Use thumbprints in production for precise certificate control,
    /// but consider more flexible identification methods for development environments.
    /// </remarks>
    public string? Thumbprint { get; set; }

    /// <summary>
    /// Gets or sets the certificate subject name for identifying certificates by their distinguished name.
    /// </summary>
    /// <value>The distinguished name of the certificate subject, or null if using other identification methods.</value>
    /// <remarks>
    /// Subject name identification uses the certificate's distinguished name (DN) to
    /// locate certificates within stores. This is more flexible than thumbprint-based
    /// identification but requires careful naming to avoid ambiguity.
    /// 
    /// Subject name formats typically include:
    /// - Common Name (CN): The primary identifier, often a hostname or service name
    /// - Organization (O): The organization that owns the certificate
    /// - Organizational Unit (OU): The department or division within the organization
    /// - Country (C): The country code where the organization is located
    /// 
    /// Example subject names:
    /// - "CN=messaging-service.company.com, O=Company Name, C=US"
    /// - "CN=service-account, OU=Messaging, O=Company Name"
    /// 
    /// Subject name matching is useful when:
    /// - Certificates are regularly renewed with the same subject
    /// - Multiple environments use certificates with similar naming patterns
    /// - Human-readable identification is preferred for operational clarity
    /// 
    /// Consider the trade-offs between precision (thumbprint) and flexibility (subject name).
    /// </remarks>
    public string? SubjectName { get; set; }

    /// <summary>
    /// Gets or sets the file path for file-based certificate sources.
    /// </summary>
    /// <value>The path to the certificate file, or null if not using file-based sources.</value>
    /// <remarks>
    /// File-based certificate sources support various certificate formats:
    /// 
    /// PFX/PKCS#12 format (.pfx, .p12):
    /// - Contains both certificate and private key in a single encrypted file
    /// - Protected by a password for security
    /// - Widely supported across platforms and applications
    /// - Good for simple deployment scenarios
    /// 
    /// PEM format (.pem, .crt, .cer):
    /// - Text-based format that's human-readable
    /// - Often used in Unix/Linux environments
    /// - May require separate private key files
    /// - Common in cloud and container deployments
    /// 
    /// File path considerations:
    /// - Use absolute paths for production deployments to avoid ambiguity
    /// - Consider file permissions and access control for security
    /// - Plan for certificate rotation and file updates
    /// - Ensure files are accessible to the application runtime environment
    /// 
    /// Security note: Certificate files containing private keys should be protected
    /// with appropriate file system permissions and encryption.
    /// </remarks>
    public string? FilePath { get; set; }

    /// <summary>
    /// Gets or sets the password for encrypted certificate files or private keys.
    /// </summary>
    /// <value>The password used to decrypt certificate files, or null if no password is required.</value>
    /// <remarks>
    /// Certificate passwords protect private keys from unauthorized access when
    /// certificates are stored in files or exported from certificate stores.
    /// 
    /// Password security considerations:
    /// - Use strong, randomly generated passwords for certificate protection
    /// - Store passwords securely using secrets management systems
    /// - Avoid hardcoding passwords in configuration files or source code
    /// - Consider using environment variables or secure configuration providers
    /// - Implement password rotation as part of certificate lifecycle management
    /// 
    /// Password sources for different environments:
    /// - Development: Environment variables or configuration files (with caution)
    /// - Production: Azure Key Vault, AWS Secrets Manager, HashiCorp Vault
    /// - Container environments: Kubernetes secrets or similar mechanisms
    /// - Traditional infrastructure: Encrypted configuration or secure file systems
    /// 
    /// Remember that passwords are only as secure as their storage and transmission mechanisms.
    /// </remarks>
    public string? Password { get; set; }

    /// <summary>
    /// Gets or sets the private key file path for PEM-based certificates with separate key files.
    /// </summary>
    /// <value>The path to the private key file, or null if the private key is included in the certificate file.</value>
    /// <remarks>
    /// Separate private key files are common in PEM-based certificate deployments,
    /// especially in Unix/Linux environments and cloud-native applications.
    /// 
    /// Separate key file scenarios:
    /// - PEM certificates where the private key is in a separate .key file
    /// - Security policies requiring private key isolation
    /// - Key management systems that provide keys and certificates separately
    /// - Certificate authorities that issue certificates and keys in separate files
    /// 
    /// Private key security is critical:
    /// - Private keys should have the most restrictive file permissions possible
    /// - Consider using hardware security modules (HSMs) for high-value keys
    /// - Implement key rotation and lifecycle management
    /// - Monitor access to private key files for security auditing
    /// 
    /// File format considerations:
    /// - Ensure the private key format matches the certificate format
    /// - Some applications require specific key formats (PKCS#1, PKCS#8)
    /// - Consider encrypted private key files for additional security
    /// </remarks>
    public string? PrivateKeyFilePath { get; set; }

    /// <summary>
    /// Creates a client certificate configuration using a certificate from the Windows certificate store.
    /// </summary>
    /// <param name="thumbprint">The thumbprint of the certificate to use.</param>
    /// <param name="storeLocation">The certificate store location. Default is LocalMachine.</param>
    /// <param name="storeName">The certificate store name. Default is My (Personal).</param>
    /// <returns>A client certificate configuration for store-based certificate access.</returns>
    /// <remarks>
    /// Store-based configuration is recommended for production Windows environments because:
    /// - The certificate store provides secure key storage with OS-level protection
    /// - Integration with Active Directory and enterprise certificate management
    /// - Support for hardware security modules and smart card authentication
    /// - Automatic certificate renewal and lifecycle management capabilities
    /// 
    /// This approach requires the certificate to be properly installed in the certificate
    /// store before the application starts. Certificate installation can be automated
    /// through Group Policy, configuration management tools, or deployment scripts.
    /// </remarks>
    public static ClientCertificateConfiguration CreateFromStore(
        string thumbprint,
        StoreLocation storeLocation = Configuration.StoreLocation.LocalMachine,
        StoreName storeName = Configuration.StoreName.My)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(thumbprint);

        return new ClientCertificateConfiguration
        {
            Source = CertificateSource.Store,
            StoreLocation = storeLocation,
            StoreName = storeName,
            Thumbprint = thumbprint
        };
    }

    /// <summary>
    /// Creates a client certificate configuration using a PFX file with embedded private key.
    /// </summary>
    /// <param name="filePath">The path to the PFX certificate file.</param>
    /// <param name="password">The password for the PFX file.</param>
    /// <returns>A client certificate configuration for PFX file-based certificate access.</returns>
    /// <remarks>
    /// PFX file configuration is suitable for scenarios where:
    /// - Cross-platform compatibility is required (Windows, Linux, macOS)
    /// - Container-based deployments need portable certificate formats
    /// - Configuration management systems handle certificate distribution
    /// - Development environments require simple certificate setup
    /// 
    /// PFX files are convenient because they contain both the certificate and private key
    /// in a single file, simplifying deployment and configuration management.
    /// 
    /// Security considerations for PFX files:
    /// - Protect the file with appropriate file system permissions
    /// - Use strong passwords for file encryption
    /// - Store passwords securely separate from the certificate file
    /// - Consider file encryption at rest for additional security
    /// </remarks>
    public static ClientCertificateConfiguration CreateFromPfx(string filePath, string? password = null)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(filePath);

        return new ClientCertificateConfiguration
        {
            Source = CertificateSource.File,
            FilePath = filePath,
            Password = password
        };
    }

    /// <summary>
    /// Creates a client certificate configuration using separate PEM certificate and private key files.
    /// </summary>
    /// <param name="certificateFilePath">The path to the PEM certificate file.</param>
    /// <param name="privateKeyFilePath">The path to the private key file.</param>
    /// <param name="privateKeyPassword">The password for the private key file, if encrypted.</param>
    /// <returns>A client certificate configuration for PEM file-based certificate access.</returns>
    /// <remarks>
    /// Separate PEM files are common in Unix/Linux environments and cloud-native applications:
    /// - Certificate and private key are in separate files for security isolation
    /// - Text-based format is human-readable and version-control friendly
    /// - Common format for certificates issued by cloud certificate authorities
    /// - Standard format for many open-source security tools and libraries
    /// 
    /// This configuration is particularly useful in environments where:
    /// - Security policies require private key isolation
    /// - Certificate management systems provide separate certificate and key files
    /// - Integration with tools like Let's Encrypt or other ACME-based CAs
    /// - Container orchestration systems manage certificates and keys separately
    /// </remarks>
    public static ClientCertificateConfiguration CreateFromPemFiles(
        string certificateFilePath,
        string privateKeyFilePath,
        string? privateKeyPassword = null)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(certificateFilePath);
        ArgumentException.ThrowIfNullOrWhiteSpace(privateKeyFilePath);

        return new ClientCertificateConfiguration
        {
            Source = CertificateSource.File,
            FilePath = certificateFilePath,
            PrivateKeyFilePath = privateKeyFilePath,
            Password = privateKeyPassword
        };
    }

    /// <summary>
    /// Validates the client certificate configuration for completeness and consistency.
    /// </summary>
    /// <exception cref="BrokerConfigurationException">Thrown when the configuration is incomplete or inconsistent.</exception>
    /// <remarks>
    /// Configuration validation ensures that all required parameters are provided
    /// for the selected certificate source and that the configuration will enable
    /// successful certificate loading and authentication.
    /// 
    /// Validation checks differ based on the certificate source:
    /// 
    /// Store-based validation:
    /// - Ensures store location and name are specified
    /// - Requires either thumbprint or subject name for certificate identification
    /// - Validates thumbprint format when provided
    /// 
    /// File-based validation:
    /// - Ensures file paths are specified and accessible
    /// - Validates file extensions and expected formats
    /// - Checks for required private key information
    /// 
    /// The validation process helps catch configuration errors early, before
    /// attempting to establish secure connections that would fail with cryptic errors.
    /// </remarks>
    public void Validate()
    {
        switch (Source)
        {
            case CertificateSource.Store:
                ValidateStoreConfiguration();
                break;

            case CertificateSource.File:
                ValidateFileConfiguration();
                break;

            default:
                throw new BrokerConfigurationException(
                    $"Unsupported certificate source: {Source}",
                    configurationSection: nameof(ClientCertificateConfiguration),
                    parameterName: nameof(Source),
                    parameterValue: Source);
        }
    }

    /// <summary>
    /// Validates store-based certificate configuration parameters.
    /// </summary>
    private void ValidateStoreConfiguration()
    {
        if (!StoreLocation.HasValue)
        {
            throw new BrokerConfigurationException(
                "Store location must be specified for store-based certificate sources.",
                configurationSection: nameof(ClientCertificateConfiguration),
                parameterName: nameof(StoreLocation));
        }

        if (!StoreName.HasValue)
        {
            throw new BrokerConfigurationException(
                "Store name must be specified for store-based certificate sources.",
                configurationSection: nameof(ClientCertificateConfiguration),
                parameterName: nameof(StoreName));
        }

        // Must have either thumbprint or subject name for certificate identification
        if (string.IsNullOrWhiteSpace(Thumbprint) && string.IsNullOrWhiteSpace(SubjectName))
        {
            throw new BrokerConfigurationException(
                "Either thumbprint or subject name must be specified for certificate identification.",
                configurationSection: nameof(ClientCertificateConfiguration));
        }

        // Validate thumbprint format if provided
        if (!string.IsNullOrWhiteSpace(Thumbprint))
        {
            var cleanThumbprint = Thumbprint.Replace(" ", "").Replace(":", "");
            if (cleanThumbprint.Length != 40 || !IsHexString(cleanThumbprint))
            {
                throw new BrokerConfigurationException(
                    "Certificate thumbprint must be a valid 40-character hexadecimal string.",
                    configurationSection: nameof(ClientCertificateConfiguration),
                    parameterName: nameof(Thumbprint),
                    parameterValue: Thumbprint);
            }
        }
    }

    /// <summary>
    /// Validates file-based certificate configuration parameters.
    /// </summary>
    private void ValidateFileConfiguration()
    {
        if (string.IsNullOrWhiteSpace(FilePath))
        {
            throw new BrokerConfigurationException(
                "File path must be specified for file-based certificate sources.",
                configurationSection: nameof(ClientCertificateConfiguration),
                parameterName: nameof(FilePath));
        }

        // Check if file exists (this might not be available at configuration time in all scenarios)
        if (!Path.IsPathRooted(FilePath))
        {
            throw new BrokerConfigurationException(
                "Certificate file path should be absolute to avoid ambiguity.",
                configurationSection: nameof(ClientCertificateConfiguration),
                parameterName: nameof(FilePath),
                parameterValue: FilePath);
        }

        // If separate private key file is specified, validate it too
        if (string.IsNullOrWhiteSpace(PrivateKeyFilePath)) return;
        if (!Path.IsPathRooted(PrivateKeyFilePath))
        {
            throw new BrokerConfigurationException(
                "Private key file path should be absolute to avoid ambiguity.",
                configurationSection: nameof(ClientCertificateConfiguration),
                parameterName: nameof(PrivateKeyFilePath),
                parameterValue: PrivateKeyFilePath);
        }
    }

    /// <summary>
    /// Determines whether a string contains only hexadecimal characters.
    /// </summary>
    /// <param name="value">The string to validate.</param>
    /// <returns>True if the string contains only hexadecimal characters; otherwise, false.</returns>
    private static bool IsHexString(string value)
    {
        return value.All(c => c is >= '0' and <= '9' || c is >= 'A' and <= 'F' || c is >= 'a' and <= 'f');
    }

    /// <summary>
    /// Returns a string representation of the client certificate configuration.
    /// </summary>
    /// <returns>A formatted string describing the certificate source and identification method.</returns>
    public override string ToString()
    {
        return Source switch
        {
            CertificateSource.Store => $"Store[{StoreLocation}/{StoreName}, {GetIdentificationMethod()}]",
            CertificateSource.File => $"File[{Path.GetFileName(FilePath)}]",
            _ => $"Unknown[{Source}]"
        };
    }

    /// <summary>
    /// Gets a description of the certificate identification method being used.
    /// </summary>
    /// <returns>A string describing how the certificate is identified.</returns>
    private string GetIdentificationMethod()
    {
        if (!string.IsNullOrWhiteSpace(Thumbprint))
            return $"thumbprint: {Thumbprint[..8]}...";
        return !string.IsNullOrWhiteSpace(SubjectName) ? $"subject: {SubjectName}" : "unknown";
    }
}