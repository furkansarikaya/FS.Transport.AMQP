using FS.RabbitMQ.Core.Abstractions;

namespace FS.RabbitMQ.Core.Configuration;

/// <summary>
/// Contains authentication credentials for RabbitMQ broker access.
/// </summary>
/// <remarks>
/// Authentication credentials support various authentication mechanisms including
/// username/password, certificate-based authentication, and external authentication providers.
/// 
/// The credential system follows security best practices:
/// - Secure credential storage and management
/// - Support for credential rotation and lifecycle management
/// - Integration with enterprise identity management systems
/// - Compliance with security standards and organizational policies
/// </remarks>
public sealed class AuthenticationCredentials
{
    /// <summary>
    /// Gets or sets the username for basic authentication.
    /// </summary>
    /// <value>The username for broker authentication, or null if not using username/password authentication.</value>
    /// <remarks>
    /// Username authentication is the most common authentication method for RabbitMQ.
    /// The username should align with the broker's user management system and
    /// organizational identity management policies.
    /// 
    /// Security considerations:
    /// - Use service accounts rather than personal accounts for applications
    /// - Follow organizational naming conventions for consistency
    /// - Implement proper access controls and least privilege principles
    /// - Consider username as non-sensitive information (unlike passwords)
    /// </remarks>
    public string? Username { get; set; }

    /// <summary>
    /// Gets or sets the password for basic authentication.
    /// </summary>
    /// <value>The password for broker authentication, or null if not using username/password authentication.</value>
    /// <remarks>
    /// Password security is critical for system security:
    /// - Use strong, randomly generated passwords
    /// - Store passwords securely using secrets management systems
    /// - Implement regular password rotation policies
    /// - Never log or expose passwords in configuration files
    /// - Consider using certificate-based authentication for enhanced security
    /// </remarks>
    public string? Password { get; set; }

    /// <summary>
    /// Gets or sets the client certificate for certificate-based authentication.
    /// </summary>
    /// <value>The client certificate configuration for mutual TLS authentication.</value>
    /// <remarks>
    /// Certificate-based authentication provides stronger security than username/password:
    /// - Cryptographically secure authentication based on PKI
    /// - Support for hardware security modules (HSMs)
    /// - Integration with enterprise certificate management
    /// - Automatic certificate lifecycle management capabilities
    /// 
    /// Certificate authentication is recommended for:
    /// - Production environments with high security requirements
    /// - Service-to-service communication in zero-trust architectures
    /// - Compliance environments requiring strong authentication
    /// </remarks>
    public ClientCertificateConfiguration? ClientCertificate { get; set; }

    /// <summary>
    /// Gets or sets the authentication mechanism to use.
    /// </summary>
    /// <value>The authentication mechanism. Default is Plain (username/password).</value>
    /// <remarks>
    /// Different authentication mechanisms provide different security characteristics:
    /// - Plain: Traditional username/password authentication
    /// - External: Integration with external authentication providers
    /// - Certificate: PKI-based authentication using client certificates
    /// - Custom: Application-specific authentication mechanisms
    /// </remarks>
    public AuthenticationMechanism Mechanism { get; set; } = AuthenticationMechanism.Plain;

    /// <summary>
    /// Gets or sets additional authentication properties for custom mechanisms.
    /// </summary>
    /// <value>A dictionary of authentication-specific properties.</value>
    /// <remarks>
    /// Authentication properties enable integration with specialized authentication systems:
    /// - OAuth tokens for modern identity providers
    /// - Kerberos tickets for enterprise authentication
    /// - Custom tokens for proprietary authentication systems
    /// - Additional metadata required by authentication plugins
    /// </remarks>
    public IDictionary<string, object>? Properties { get; set; }

    /// <summary>
    /// Creates basic username/password credentials.
    /// </summary>
    /// <param name="username">The username for authentication.</param>
    /// <param name="password">The password for authentication.</param>
    /// <returns>Authentication credentials configured for username/password authentication.</returns>
    public static AuthenticationCredentials CreateBasic(string username, string password)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(username);
        ArgumentException.ThrowIfNullOrWhiteSpace(password);

        return new AuthenticationCredentials
        {
            Username = username,
            Password = password,
            Mechanism = AuthenticationMechanism.Plain
        };
    }

    /// <summary>
    /// Creates certificate-based authentication credentials.
    /// </summary>
    /// <param name="clientCertificate">The client certificate configuration.</param>
    /// <returns>Authentication credentials configured for certificate-based authentication.</returns>
    public static AuthenticationCredentials CreateCertificate(ClientCertificateConfiguration clientCertificate)
    {
        ArgumentNullException.ThrowIfNull(clientCertificate);

        return new AuthenticationCredentials
        {
            ClientCertificate = clientCertificate,
            Mechanism = AuthenticationMechanism.Certificate
        };
    }

    /// <summary>
    /// Creates external authentication credentials with custom properties.
    /// </summary>
    /// <param name="properties">The authentication properties for external systems.</param>
    /// <returns>Authentication credentials configured for external authentication.</returns>
    public static AuthenticationCredentials CreateExternal(IDictionary<string, object> properties)
    {
        ArgumentNullException.ThrowIfNull(properties);

        return new AuthenticationCredentials
        {
            Properties = properties,
            Mechanism = AuthenticationMechanism.External
        };
    }

    /// <summary>
    /// Validates the authentication credentials for completeness and consistency.
    /// </summary>
    /// <exception cref="BrokerConfigurationException">Thrown when credentials are invalid or incomplete.</exception>
    public void Validate()
    {
        switch (Mechanism)
        {
            case AuthenticationMechanism.Plain:
                if (string.IsNullOrWhiteSpace(Username))
                {
                    throw new BrokerConfigurationException(
                        "Username is required for Plain authentication mechanism.",
                        configurationSection: nameof(AuthenticationCredentials),
                        parameterName: nameof(Username));
                }
                if (string.IsNullOrWhiteSpace(Password))
                {
                    throw new BrokerConfigurationException(
                        "Password is required for Plain authentication mechanism.",
                        configurationSection: nameof(AuthenticationCredentials),
                        parameterName: nameof(Password));
                }
                break;

            case AuthenticationMechanism.Certificate:
                if (ClientCertificate == null)
                {
                    throw new BrokerConfigurationException(
                        "Client certificate is required for Certificate authentication mechanism.",
                        configurationSection: nameof(AuthenticationCredentials),
                        parameterName: nameof(ClientCertificate));
                }
                ClientCertificate.Validate();
                break;

            case AuthenticationMechanism.External:
                if (Properties?.Any() != true)
                {
                    throw new BrokerConfigurationException(
                        "Authentication properties are required for External authentication mechanism.",
                        configurationSection: nameof(AuthenticationCredentials),
                        parameterName: nameof(Properties));
                }
                break;
        }
    }

    /// <summary>
    /// Returns a string representation of the authentication credentials.
    /// </summary>
    /// <returns>A string describing the authentication mechanism and user information.</returns>
    public override string ToString()
    {
        return Mechanism switch
        {
            AuthenticationMechanism.Plain => $"Basic[{Username}]",
            AuthenticationMechanism.Certificate => $"Certificate[{ClientCertificate}]",
            AuthenticationMechanism.External => "External",
            _ => $"Unknown[{Mechanism}]"
        };
    }
}