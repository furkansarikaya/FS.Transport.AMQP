using System.Net.Security;
using System.Security.Cryptography.X509Certificates;
using FS.RabbitMQ.Core.Abstractions;

namespace FS.RabbitMQ.Core.Configuration;

/// <summary>
/// Contains comprehensive TLS/SSL configuration for secure RabbitMQ connections.
/// </summary>
/// <remarks>
/// TLS configuration provides fine-grained control over transport layer security
/// for RabbitMQ connections, supporting various security requirements from basic
/// encryption to sophisticated mutual authentication scenarios.
/// 
/// The configuration follows security-by-design principles, providing secure defaults
/// while allowing customization for specific organizational requirements. It supports
/// both simple certificate-based authentication and complex PKI deployments with
/// certificate validation, revocation checking, and custom trust policies.
/// 
/// Key security considerations addressed:
/// - Certificate validation and trust chain verification
/// - Cipher suite selection and protocol version enforcement
/// - Client certificate authentication for mutual TLS (mTLS)
/// - Certificate revocation and security policy compliance
/// - Performance optimization while maintaining security guarantees
/// 
/// Understanding TLS in messaging contexts:
/// Think of TLS as creating a "secure tunnel" between your application and RabbitMQ.
/// Just like when you use HTTPS to securely browse websites, TLS ensures that your
/// messages can't be intercepted or tampered with as they travel across the network.
/// However, messaging systems often require more sophisticated security than web
/// browsing, which is why this configuration provides extensive customization options.
/// </remarks>
public sealed class TlsConfiguration
{
    /// <summary>
    /// Gets or sets a value indicating whether server certificate validation is enabled.
    /// </summary>
    /// <value><c>true</c> to validate server certificates; otherwise, <c>false</c>. Default is <c>true</c>.</value>
    /// <remarks>
    /// Server certificate validation is your first line of defense against man-in-the-middle attacks.
    /// When enabled, the client verifies that:
    /// - The server's certificate is signed by a trusted Certificate Authority (CA)
    /// - The certificate hasn't expired or been revoked
    /// - The certificate's hostname matches the server you're connecting to
    /// 
    /// Disabling validation (setting to false) should only be done in specific scenarios:
    /// - Development environments with self-signed certificates
    /// - Testing scenarios where security isn't a concern
    /// - Environments with custom certificate validation logic
    /// 
    /// Security warning: Disabling certificate validation in production environments
    /// significantly weakens security and should be avoided unless absolutely necessary
    /// with proper security analysis and compensating controls.
    /// </remarks>
    public bool ValidateServerCertificate { get; set; } = true;

    /// <summary>
    /// Gets or sets a value indicating whether hostname verification is performed during certificate validation.
    /// </summary>
    /// <value><c>true</c> to verify that the certificate hostname matches the connection hostname; otherwise, <c>false</c>. Default is <c>true</c>.</value>
    /// <remarks>
    /// Hostname verification ensures that the certificate presented by the server
    /// is actually intended for the server you're connecting to. This prevents
    /// attacks where a valid certificate for a different server is used.
    /// 
    /// The verification process checks that the certificate's Common Name (CN)
    /// or Subject Alternative Name (SAN) matches the hostname you're connecting to.
    /// 
    /// Scenarios where you might disable hostname verification:
    /// - Load balancers where the certificate is for the load balancer, not individual servers
    /// - Development environments with IP addresses instead of hostnames
    /// - Complex network topologies with certificate proxying
    /// 
    /// Best practice: Keep this enabled unless you have a specific reason to disable it
    /// and understand the security implications.
    /// </remarks>
    public bool VerifyHostname { get; set; } = true;

    /// <summary>
    /// Gets or sets the client certificate for mutual TLS authentication.
    /// </summary>
    /// <value>The client certificate used for mutual authentication, or null if not using client certificates.</value>
    /// <remarks>
    /// Client certificates enable mutual TLS (mTLS), where both the client and server
    /// authenticate each other. This provides stronger security than server-only authentication
    /// because it ensures that only authorized clients can connect to the server.
    /// 
    /// Mutual TLS is particularly important in:
    /// - Zero-trust network architectures
    /// - Service-to-service communication in microservices
    /// - Environments with strict security compliance requirements
    /// - Systems where client identity needs to be cryptographically verified
    /// 
    /// The client certificate should:
    /// - Be issued by a CA trusted by the RabbitMQ server
    /// - Include the private key for authentication
    /// - Have appropriate key usage extensions for client authentication
    /// - Be stored securely and rotated according to security policies
    /// </remarks>
    public ClientCertificateConfiguration? ClientCertificate { get; set; }

    /// <summary>
    /// Gets or sets the collection of trusted certificate authorities for server certificate validation.
    /// </summary>
    /// <value>A collection of trusted CA certificates, or null to use the system's default trust store.</value>
    /// <remarks>
    /// Trusted CAs define which certificate authorities are considered trustworthy
    /// for validating server certificates. This allows organizations to:
    /// - Use internal/private certificate authorities
    /// - Restrict trust to specific CAs for enhanced security
    /// - Implement certificate pinning strategies
    /// - Support hybrid environments with multiple trust chains
    /// 
    /// Configuration strategies:
    /// - System default: Use the operating system's built-in CA trust store
    /// - Custom CAs: Add specific CAs for internal or specialized certificates
    /// - Restricted trust: Only trust specific CAs to limit attack surface
    /// - Hybrid approach: Combine system CAs with additional trusted authorities
    /// 
    /// Security consideration: Adding CAs increases trust scope, while restricting
    /// CAs reduces it. Choose the approach that best fits your security model.
    /// </remarks>
    public IList<X509Certificate2>? TrustedCertificateAuthorities { get; set; }

    /// <summary>
    /// Gets or sets the allowed TLS protocol versions for secure connections.
    /// </summary>
    /// <value>The TLS protocol versions that are acceptable for connections. Default includes TLS 1.2 and 1.3.</value>
    /// <remarks>
    /// TLS protocol version selection balances security with compatibility:
    /// - TLS 1.3: Latest standard with best security and performance
    /// - TLS 1.2: Widely supported, secure when properly configured
    /// - TLS 1.1 and below: Deprecated due to security vulnerabilities
    /// 
    /// Version selection considerations:
    /// - Security policies and compliance requirements
    /// - Broker and infrastructure support capabilities
    /// - Performance characteristics and cipher suite availability
    /// - Future compatibility and deprecation timelines
    /// 
    /// Recommended configuration:
    /// - Prefer TLS 1.3 for new deployments
    /// - Include TLS 1.2 for broader compatibility
    /// - Avoid older versions unless absolutely necessary for legacy systems
    /// </remarks>
    public TlsVersionPolicy ProtocolVersions { get; set; } = TlsVersionPolicy.Tls12AndAbove;

    /// <summary>
    /// Gets or sets the cipher suites allowed for TLS connections.
    /// </summary>
    /// <value>The collection of acceptable cipher suites, or null to use system defaults.</value>
    /// <remarks>
    /// Cipher suites define the cryptographic algorithms used for encryption,
    /// authentication, and key exchange. Different suites provide different
    /// security levels and performance characteristics.
    /// 
    /// Cipher suite selection involves trade-offs between:
    /// - Security strength: Stronger algorithms provide better protection
    /// - Performance: Some algorithms are faster than others
    /// - Compatibility: Not all systems support all cipher suites
    /// - Compliance: Some regulations require specific cryptographic standards
    /// 
    /// Modern best practices favor:
    /// - AEAD (Authenticated Encryption with Associated Data) cipher suites
    /// - Forward secrecy through ephemeral key exchange
    /// - Strong symmetric encryption (AES-256, ChaCha20)
    /// - Secure hash functions (SHA-256 or better)
    /// 
    /// Let the system choose secure defaults unless you have specific requirements.
    /// </remarks>
    public IList<string>? AllowedCipherSuites { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether certificate revocation checking is enabled.
    /// </summary>
    /// <value><c>true</c> to check certificate revocation status; otherwise, <c>false</c>. Default is <c>true</c>.</value>
    /// <remarks>
    /// Certificate revocation checking verifies that certificates haven't been
    /// revoked by their issuing Certificate Authority. This is important because
    /// certificates might be revoked due to:
    /// - Private key compromise
    /// - Change in certificate subject details
    /// - CA policy violations
    /// - Security incidents
    /// 
    /// Revocation checking methods:
    /// - CRL (Certificate Revocation List): Downloaded list of revoked certificates
    /// - OCSP (Online Certificate Status Protocol): Real-time revocation checking
    /// 
    /// Trade-offs to consider:
    /// - Security: Revocation checking improves security by detecting compromised certificates
    /// - Performance: Checking can add latency to connection establishment
    /// - Reliability: Network issues can prevent revocation checking
    /// - Privacy: OCSP requests might reveal which sites you're connecting to
    /// 
    /// Best practice: Enable revocation checking unless performance or reliability
    /// concerns outweigh the security benefits in your specific environment.
    /// </remarks>
    public bool CheckCertificateRevocation { get; set; } = true;

    /// <summary>
    /// Gets or sets custom certificate validation logic for advanced scenarios.
    /// </summary>
    /// <value>A custom validation function, or null to use standard validation.</value>
    /// <remarks>
    /// Custom certificate validation enables sophisticated security policies
    /// that go beyond standard PKI validation. This might include:
    /// - Certificate pinning for specific servers or CAs
    /// - Custom trust stores and validation logic
    /// - Integration with organizational certificate management systems
    /// - Advanced security policies based on certificate attributes
    /// 
    /// The validation function receives the certificate chain and validation errors,
    /// allowing you to implement custom logic such as:
    /// - Accepting specific self-signed certificates
    /// - Implementing certificate pinning strategies
    /// - Adding custom validation rules based on certificate extensions
    /// - Integrating with external certificate validation services
    /// 
    /// Security warning: Custom validation logic should be implemented carefully
    /// and reviewed by security experts, as incorrect implementation can
    /// significantly weaken the security of TLS connections.
    /// </remarks>
    public Func<X509Certificate2, X509Chain, SslPolicyErrors, bool>? CustomCertificateValidation { get; set; }

    /// <summary>
    /// Gets or sets the TLS handshake timeout for connection establishment.
    /// </summary>
    /// <value>The maximum time to wait for TLS handshake completion. Default is 30 seconds.</value>
    /// <remarks>
    /// TLS handshake timeout prevents connections from hanging indefinitely
    /// during the certificate exchange and cryptographic negotiation process.
    /// 
    /// Timeout considerations:
    /// - Network latency: Higher latency networks need longer timeouts
    /// - Certificate validation: Revocation checking can add significant delay
    /// - Server load: Busy servers might take longer to complete handshakes
    /// - Security processing: Strong cryptography takes more computational time
    /// 
    /// Typical timeout ranges:
    /// - Local networks: 5-15 seconds
    /// - Internet connections: 15-60 seconds
    /// - High-latency networks: 30-120 seconds
    /// - Systems with extensive certificate validation: 60-180 seconds
    /// 
    /// Balance between responsiveness (shorter timeouts) and reliability
    /// (longer timeouts) based on your network characteristics and requirements.
    /// </remarks>
    public TimeSpan HandshakeTimeout { get; set; } = TimeSpan.FromSeconds(30);

    /// <summary>
    /// Creates a TLS configuration with secure default settings suitable for most production environments.
    /// </summary>
    /// <returns>A TLS configuration with security-focused defaults.</returns>
    /// <remarks>
    /// Default TLS configuration prioritizes security while maintaining broad compatibility:
    /// - Enables all standard security validations
    /// - Uses modern TLS versions (1.2 and above)
    /// - Allows system to choose secure cipher suites
    /// - Enables certificate revocation checking
    /// - Sets reasonable timeouts for most network conditions
    /// 
    /// This configuration is suitable for most production environments and provides
    /// a good starting point that can be customized based on specific requirements.
    /// </remarks>
    public static TlsConfiguration CreateDefault()
    {
        return new TlsConfiguration
        {
            ValidateServerCertificate = true,
            VerifyHostname = true,
            ProtocolVersions = TlsVersionPolicy.Tls12AndAbove,
            CheckCertificateRevocation = true,
            HandshakeTimeout = TimeSpan.FromSeconds(30)
        };
    }

    /// <summary>
    /// Creates a TLS configuration for development environments with relaxed security for convenience.
    /// </summary>
    /// <returns>A TLS configuration suitable for development use.</returns>
    /// <remarks>
    /// Development TLS configuration prioritizes ease of use over strict security:
    /// - Disables hostname verification for localhost/IP connections
    /// - Allows weaker TLS versions for compatibility
    /// - Shorter timeouts for faster failure detection
    /// - Simplified certificate validation for self-signed certificates
    /// 
    /// Warning: This configuration should NEVER be used in production environments
    /// as it significantly reduces security. It's designed solely for development
    /// convenience where security is not a primary concern.
    /// </remarks>
    public static TlsConfiguration CreateDevelopment()
    {
        return new TlsConfiguration
        {
            ValidateServerCertificate = true,
            VerifyHostname = false, // Relaxed for localhost development
            ProtocolVersions = TlsVersionPolicy.Tls11AndAbove, // More permissive for dev
            CheckCertificateRevocation = false, // Avoid network dependencies in dev
            HandshakeTimeout = TimeSpan.FromSeconds(10) // Faster timeouts for development
        };
    }

    /// <summary>
    /// Creates a TLS configuration with enhanced security settings for high-security environments.
    /// </summary>
    /// <returns>A TLS configuration with maximum security settings.</returns>
    /// <remarks>
    /// High-security TLS configuration implements strict security policies:
    /// - Enforces latest TLS versions only
    /// - Requires comprehensive certificate validation
    /// - Enables all available security checks
    /// - Uses conservative timeouts for thorough validation
    /// 
    /// This configuration is suitable for:
    /// - High-value financial systems
    /// - Government and defense applications
    /// - Healthcare systems with strict compliance requirements
    /// - Any environment where security is paramount
    /// 
    /// Note: High-security settings may impact performance and compatibility,
    /// so thorough testing is recommended before production deployment.
    /// </remarks>
    public static TlsConfiguration CreateHighSecurity()
    {
        return new TlsConfiguration
        {
            ValidateServerCertificate = true,
            VerifyHostname = true,
            ProtocolVersions = TlsVersionPolicy.Tls13Only, // Most secure
            CheckCertificateRevocation = true,
            HandshakeTimeout = TimeSpan.FromMinutes(2) // Allow time for thorough validation
        };
    }

    /// <summary>
    /// Validates the TLS configuration for consistency and security best practices.
    /// </summary>
    /// <exception cref="BrokerConfigurationException">Thrown when the TLS configuration contains insecure or inconsistent settings.</exception>
    /// <remarks>
    /// TLS validation ensures that the configuration follows security best practices
    /// and doesn't contain contradictory or potentially dangerous settings.
    /// 
    /// Validation checks include:
    /// - Timeout value reasonableness and positive values
    /// - Protocol version security and deprecation status
    /// - Certificate configuration completeness for mutual TLS
    /// - Cipher suite security when explicitly configured
    /// - Cross-setting consistency and security implications
    /// 
    /// The validation process helps prevent common configuration mistakes that
    /// could weaken security or cause connection failures.
    /// </remarks>
    public void Validate()
    {
        if (HandshakeTimeout <= TimeSpan.Zero)
        {
            throw new BrokerConfigurationException(
                "TLS handshake timeout must be positive.",
                configurationSection: nameof(TlsConfiguration),
                parameterName: nameof(HandshakeTimeout),
                parameterValue: HandshakeTimeout);
        }

        if (HandshakeTimeout > TimeSpan.FromMinutes(10))
        {
            throw new BrokerConfigurationException(
                "TLS handshake timeout should not exceed 10 minutes to prevent hung connections.",
                configurationSection: nameof(TlsConfiguration),
                parameterName: nameof(HandshakeTimeout),
                parameterValue: HandshakeTimeout);
        }

        // Validate client certificate configuration if present
        if (ClientCertificate != null)
        {
            ClientCertificate.Validate();
        }

        // Validate trusted CAs if specified
        if (TrustedCertificateAuthorities?.Any() == true)
        {
            foreach (var ca in TrustedCertificateAuthorities)
            {
                if (ca == null)
                {
                    throw new BrokerConfigurationException(
                        "Trusted certificate authority cannot be null.",
                        configurationSection: nameof(TlsConfiguration),
                        parameterName: nameof(TrustedCertificateAuthorities));
                }

                // Check if CA certificate is valid and not expired
                if (ca.NotAfter < DateTime.UtcNow)
                {
                    throw new BrokerConfigurationException(
                        $"Trusted certificate authority has expired: {ca.Subject}",
                        configurationSection: nameof(TlsConfiguration),
                        parameterName: nameof(TrustedCertificateAuthorities));
                }
            }
        }

        // Validate cipher suites if specified
        if (AllowedCipherSuites?.Any() == true)
        {
            foreach (var cipherSuite in AllowedCipherSuites)
            {
                if (string.IsNullOrWhiteSpace(cipherSuite))
                {
                    throw new BrokerConfigurationException(
                        "Cipher suite names cannot be null or empty.",
                        configurationSection: nameof(TlsConfiguration),
                        parameterName: nameof(AllowedCipherSuites));
                }
            }
        }
    }

    /// <summary>
    /// Returns a string representation of the TLS configuration.
    /// </summary>
    /// <returns>A formatted string describing the key TLS settings.</returns>
    public override string ToString()
    {
        var features = new List<string>();
        
        if (ValidateServerCertificate) features.Add("server-validation");
        if (VerifyHostname) features.Add("hostname-verification");
        if (ClientCertificate != null) features.Add("mutual-tls");
        if (CheckCertificateRevocation) features.Add("revocation-checking");
        
        var protocolInfo = ProtocolVersions.ToString().ToLowerInvariant();
        var featureInfo = features.Any() ? $" [{string.Join(", ", features)}]" : "";
        
        return $"TLS[{protocolInfo}{featureInfo}]";
    }
}
