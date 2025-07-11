namespace FS.RabbitMQ.Configuration;

/// <summary>
/// SSL/TLS connection settings
/// </summary>
public class SslSettings
{
    /// <summary>
    /// SSL server name (for SNI)
    /// </summary>
    public string? ServerName { get; set; }
    
    /// <summary>
    /// Path to client certificate file
    /// </summary>
    public string? CertificatePath { get; set; }
    
    /// <summary>
    /// Client certificate password
    /// </summary>
    public string? CertificatePassword { get; set; }
    
    /// <summary>
    /// Whether to check certificate revocation
    /// </summary>
    public bool CheckCertificateRevocation { get; set; } = true;
    
    /// <summary>
    /// Whether to accept invalid certificates (for development only)
    /// </summary>
    public bool AcceptInvalidCertificates { get; set; } = false;
    
    /// <summary>
    /// SSL protocol version
    /// </summary>
    public string SslProtocol { get; set; } = "Tls12";

    /// <summary>
    /// Gets or sets whether SSL is enabled (for backward compatibility)
    /// </summary>
    public bool Enabled { get; set; } = false;

    /// <summary>
    /// Gets or sets the SSL version (for backward compatibility)
    /// </summary>
    public System.Security.Authentication.SslProtocols Version 
    { 
        get => Enum.TryParse<System.Security.Authentication.SslProtocols>(SslProtocol, out var result) 
               ? result 
               : System.Security.Authentication.SslProtocols.Tls12;
        set => SslProtocol = value.ToString();
    }

    /// <summary>
    /// Gets or sets the acceptable policy errors (for backward compatibility)
    /// </summary>
    public System.Net.Security.SslPolicyErrors AcceptablePolicyErrors { get; set; } = 
        System.Net.Security.SslPolicyErrors.None;

    /// <summary>
    /// Validates SSL settings
    /// </summary>
    /// <exception cref="ArgumentException">Thrown when SSL settings are invalid</exception>
    public void Validate()
    {
        if (!string.IsNullOrEmpty(CertificatePath) && !File.Exists(CertificatePath))
            throw new ArgumentException($"Certificate file not found: {CertificatePath}");

        if (AcceptInvalidCertificates)
        {
            // Log a warning about using this in production
            System.Diagnostics.Debug.WriteLine("WARNING: AcceptInvalidCertificates is enabled. This should not be used in production!");
        }

        var validProtocols = new[] { "None", "Ssl2", "Ssl3", "Tls", "Tls11", "Tls12", "Tls13" };
        if (!validProtocols.Contains(SslProtocol))
            throw new ArgumentException($"SslProtocol must be one of: {string.Join(", ", validProtocols)}");
    }

    /// <summary>
    /// Gets a string representation of SSL settings (without sensitive data)
    /// </summary>
    /// <returns>SSL settings description</returns>
    public override string ToString()
    {
        var hasClientCert = !string.IsNullOrEmpty(CertificatePath);
        return $"SSL: {SslProtocol}, ServerName: {ServerName ?? "default"}, " +
               $"ClientCert: {(hasClientCert ? "provided" : "none")}, " +
               $"AcceptInvalid: {AcceptInvalidCertificates}";
    }
}