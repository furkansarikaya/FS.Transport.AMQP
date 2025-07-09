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
    /// Validates SSL settings
    /// </summary>
    public void Validate()
    {
        if (!string.IsNullOrEmpty(CertificatePath) && !File.Exists(CertificatePath))
            throw new ArgumentException($"Certificate file not found: {CertificatePath}");
    }
}