using FS.RabbitMQ.Core.Abstractions;

namespace FS.RabbitMQ.Core.Configuration;

/// <summary>
/// Contains comprehensive security configuration for RabbitMQ connections and operations.
/// </summary>
/// <remarks>
/// Security configuration encompasses authentication, authorization, encryption,
/// and compliance requirements to ensure secure messaging operations across
/// various deployment environments and threat models.
/// </remarks>
public sealed class SecurityConfiguration
{
    /// <summary>
    /// Gets or sets a value indicating whether TLS encryption is required for all connections.
    /// </summary>
    /// <value><c>true</c> to require TLS for all connections; otherwise, <c>false</c>. Default is <c>false</c>.</value>
    /// <remarks>
    /// TLS requirement ensures that all communication with brokers is encrypted,
    /// protecting message content and authentication credentials from interception.
    /// </remarks>
    public bool RequireTls { get; set; } = false;

    /// <summary>
    /// Gets or sets the minimum TLS version required for connections.
    /// </summary>
    /// <value>The minimum acceptable TLS version. Default is TLS 1.2.</value>
    /// <remarks>
    /// Minimum TLS version enforcement ensures that only secure protocol versions
    /// are used, protecting against attacks on older, vulnerable protocols.
    /// </remarks>
    public TlsVersionPolicy MinimumTlsVersion { get; set; } = TlsVersionPolicy.Tls12AndAbove;

    /// <summary>
    /// Gets or sets a value indicating whether certificate validation is enforced.
    /// </summary>
    /// <value><c>true</c> to enforce certificate validation; otherwise, <c>false</c>. Default is <c>true</c>.</value>
    /// <remarks>
    /// Certificate validation enforcement prevents connections to brokers with
    /// invalid, expired, or untrusted certificates, protecting against
    /// man-in-the-middle attacks.
    /// </remarks>
    public bool EnforceCertificateValidation { get; set; } = true;

    /// <summary>
    /// Gets or sets the allowed cipher suites for TLS connections.
    /// </summary>
    /// <value>A collection of acceptable cipher suite names, or null to use system defaults.</value>
    /// <remarks>
    /// Cipher suite restrictions enable enforcement of cryptographic standards
    /// and compliance requirements by limiting connections to approved algorithms.
    /// </remarks>
    public IList<string>? AllowedCipherSuites { get; set; }

    /// <summary>
    /// Gets or sets the credential validation policy.
    /// </summary>
    /// <value>The policy for validating authentication credentials. Default is Standard.</value>
    /// <remarks>
    /// Credential validation policy defines how authentication credentials
    /// are verified and what additional security checks are performed.
    /// </remarks>
    public CredentialValidationPolicy CredentialValidation { get; set; } = CredentialValidationPolicy.Standard;

    /// <summary>
    /// Gets or sets the access control configuration.
    /// </summary>
    /// <value>The access control settings for authorization and permissions.</value>
    /// <remarks>
    /// Access control configuration defines how permissions are managed
    /// and enforced for different operations and resources.
    /// </remarks>
    public AccessControlConfiguration? AccessControl { get; set; }

    /// <summary>
    /// Gets or sets the audit configuration for security event logging.
    /// </summary>
    /// <value>The audit settings for tracking security-relevant events.</value>
    /// <remarks>
    /// Audit configuration enables comprehensive logging of security events
    /// for compliance, incident response, and security monitoring purposes.
    /// </remarks>
    public AuditConfiguration? Audit { get; set; }

    /// <summary>
    /// Gets or sets security-related custom properties.
    /// </summary>
    /// <value>A dictionary of custom security properties and settings.</value>
    /// <remarks>
    /// Custom security properties enable integration with specialized security
    /// systems, compliance frameworks, and organizational security policies.
    /// </remarks>
    public IDictionary<string, object>? SecurityProperties { get; set; }

    /// <summary>
    /// Creates a security configuration suitable for development environments.
    /// </summary>
    /// <returns>A security configuration with relaxed settings for development convenience.</returns>
    public static SecurityConfiguration CreateDevelopment()
    {
        return new SecurityConfiguration
        {
            RequireTls = false,
            MinimumTlsVersion = TlsVersionPolicy.Tls11AndAbove,
            EnforceCertificateValidation = false,
            CredentialValidation = CredentialValidationPolicy.Relaxed
        };
    }

    /// <summary>
    /// Creates a security configuration suitable for production environments.
    /// </summary>
    /// <returns>A security configuration with robust security settings.</returns>
    public static SecurityConfiguration CreateProduction()
    {
        return new SecurityConfiguration
        {
            RequireTls = true,
            MinimumTlsVersion = TlsVersionPolicy.Tls12AndAbove,
            EnforceCertificateValidation = true,
            CredentialValidation = CredentialValidationPolicy.Strict,
            AccessControl = AccessControlConfiguration.CreateDefault(),
            Audit = AuditConfiguration.CreateDefault()
        };
    }

    /// <summary>
    /// Creates a security configuration with default balanced settings.
    /// </summary>
    /// <returns>A security configuration with standard security practices.</returns>
    public static SecurityConfiguration CreateDefault()
    {
        return new SecurityConfiguration
        {
            RequireTls = false,
            MinimumTlsVersion = TlsVersionPolicy.Tls12AndAbove,
            EnforceCertificateValidation = true,
            CredentialValidation = CredentialValidationPolicy.Standard
        };
    }

    /// <summary>
    /// Validates the security configuration for consistency and security best practices.
    /// </summary>
    /// <exception cref="BrokerConfigurationException">Thrown when the security configuration is invalid or insecure.</exception>
    public void Validate()
    {
        if (AllowedCipherSuites?.Any() == true)
        {
            foreach (var cipherSuite in AllowedCipherSuites)
            {
                if (string.IsNullOrWhiteSpace(cipherSuite))
                {
                    throw new BrokerConfigurationException(
                        "Cipher suite names cannot be null or empty.",
                        configurationSection: nameof(SecurityConfiguration),
                        parameterName: nameof(AllowedCipherSuites));
                }
            }
        }

        AccessControl?.Validate();
        Audit?.Validate();

        if (SecurityProperties?.Any() == true)
        {
            foreach (var property in SecurityProperties)
            {
                if (string.IsNullOrWhiteSpace(property.Key))
                {
                    throw new BrokerConfigurationException(
                        "Security property keys cannot be null or empty.",
                        configurationSection: nameof(SecurityConfiguration),
                        parameterName: "SecurityProperties.Key");
                }
            }
        }
    }

    /// <summary>
    /// Returns a string representation of the security configuration.
    /// </summary>
    /// <returns>A formatted string describing the security settings.</returns>
    public override string ToString()
    {
        var features = new List<string>();
        if (RequireTls) features.Add("TLS-required");
        if (EnforceCertificateValidation) features.Add("cert-validation");
        if (AccessControl != null) features.Add("access-control");
        if (Audit != null) features.Add("audit");

        return $"Security[{string.Join(", ", features)}, min-TLS: {MinimumTlsVersion}]";
    }
}