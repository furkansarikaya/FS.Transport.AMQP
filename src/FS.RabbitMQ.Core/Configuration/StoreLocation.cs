namespace FS.RabbitMQ.Core.Configuration;

/// <summary>
/// Specifies the location of the certificate store.
/// </summary>
/// <remarks>
/// Certificate store locations determine where certificates are stored and who can access them.
/// This maps directly to the .NET X509Store locations for consistent behavior.
/// </remarks>
public enum StoreLocation
{
    /// <summary>
    /// The certificate store used by the current user.
    /// </summary>
    CurrentUser = System.Security.Cryptography.X509Certificates.StoreLocation.CurrentUser,

    /// <summary>
    /// The certificate store used by the local machine.
    /// </summary>
    LocalMachine = System.Security.Cryptography.X509Certificates.StoreLocation.LocalMachine
}