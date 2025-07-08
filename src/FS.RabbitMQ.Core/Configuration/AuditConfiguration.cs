namespace FS.RabbitMQ.Core.Configuration;

/// <summary>
/// Contains configuration for security audit logging and monitoring.
/// </summary>
public sealed class AuditConfiguration
{
    /// <summary>
    /// Gets or sets a value indicating whether audit logging is enabled.
    /// </summary>
    /// <value><c>true</c> to enable audit logging; otherwise, <c>false</c>. Default is <c>true</c>.</value>
    public bool EnableAuditLogging { get; set; } = true;

    /// <summary>
    /// Gets or sets the audit events to be logged.
    /// </summary>
    /// <value>The types of events that should be audited. Default is SecurityEvents.</value>
    public AuditEventTypes AuditEvents { get; set; } = AuditEventTypes.SecurityEvents;

    /// <summary>
    /// Creates a default audit configuration.
    /// </summary>
    /// <returns>An audit configuration with standard settings.</returns>
    public static AuditConfiguration CreateDefault()
    {
        return new AuditConfiguration
        {
            EnableAuditLogging = true,
            AuditEvents = AuditEventTypes.SecurityEvents
        };
    }

    /// <summary>
    /// Validates the audit configuration.
    /// </summary>
    /// <exception cref="BrokerConfigurationException">Thrown when the configuration is invalid.</exception>
    public void Validate()
    {
        // Audit configuration validation logic would go here
    }
}