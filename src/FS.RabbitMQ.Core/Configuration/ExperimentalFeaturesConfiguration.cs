namespace FS.RabbitMQ.Core.Configuration;

/// <summary>
/// Contains configuration for experimental and preview features.
/// </summary>
public sealed class ExperimentalFeaturesConfiguration
{
    /// <summary>
    /// Gets or sets a value indicating whether experimental features are enabled.
    /// </summary>
    public bool EnableExperimentalFeatures { get; set; } = false;

    /// <summary>
    /// Creates experimental features configuration for development.
    /// </summary>
    public static ExperimentalFeaturesConfiguration CreateDevelopment() => 
        new() { EnableExperimentalFeatures = true };

    /// <summary>
    /// Validates the experimental features configuration.
    /// </summary>
    public void Validate() { }
}