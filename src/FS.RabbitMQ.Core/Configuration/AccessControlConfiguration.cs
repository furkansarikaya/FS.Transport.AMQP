namespace FS.RabbitMQ.Core.Configuration;

/// <summary>
/// Contains configuration for access control and authorization policies.
/// </summary>
public sealed class AccessControlConfiguration
{
    /// <summary>
    /// Gets or sets a value indicating whether role-based access control is enabled.
    /// </summary>
    /// <value><c>true</c> to enable RBAC; otherwise, <c>false</c>. Default is <c>true</c>.</value>
    public bool EnableRoleBasedAccess { get; set; } = true;

    /// <summary>
    /// Gets or sets the default permissions for authenticated users.
    /// </summary>
    /// <value>The default permission set for new connections.</value>
    public AccessPermissions DefaultPermissions { get; set; } = AccessPermissions.ReadWrite;

    /// <summary>
    /// Creates a default access control configuration.
    /// </summary>
    /// <returns>An access control configuration with standard settings.</returns>
    public static AccessControlConfiguration CreateDefault()
    {
        return new AccessControlConfiguration
        {
            EnableRoleBasedAccess = true,
            DefaultPermissions = AccessPermissions.ReadWrite
        };
    }

    /// <summary>
    /// Validates the access control configuration.
    /// </summary>
    /// <exception cref="BrokerConfigurationException">Thrown when the configuration is invalid.</exception>
    public void Validate()
    {
        // Access control validation logic would go here
    }
}