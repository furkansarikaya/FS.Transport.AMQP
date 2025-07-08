using FS.RabbitMQ.Core.Abstractions;

namespace FS.RabbitMQ.Core.Configuration;

/// <summary>
/// Contains configuration for automatic connection recovery and resilience mechanisms.
/// </summary>
/// <remarks>
/// Recovery configuration defines how the system responds to connection failures and
/// network disruptions. It encompasses automatic reconnection, topology restoration,
/// and consumer recovery to ensure system resilience and minimal downtime.
/// 
/// Recovery mechanisms are critical for production systems that must maintain
/// high availability despite network instability, broker restarts, and infrastructure
/// maintenance activities.
/// </remarks>
public sealed class RecoveryConfiguration
{
    /// <summary>
    /// Gets or sets a value indicating whether automatic recovery is enabled.
    /// </summary>
    /// <value><c>true</c> to enable automatic recovery; otherwise, <c>false</c>. Default is <c>true</c>.</value>
    /// <remarks>
    /// Automatic recovery enables the system to automatically reconnect and restore
    /// topology when connections are lost due to network issues or broker restarts.
    /// 
    /// When enabled, the system will:
    /// - Automatically reconnect to brokers when connections are lost
    /// - Restore exchanges, queues, and bindings
    /// - Resume consumers and their subscriptions
    /// - Maintain application state across disconnections
    /// </remarks>
    public bool EnableAutoRecovery { get; set; } = true;

    /// <summary>
    /// Gets or sets the initial delay before attempting recovery.
    /// </summary>
    /// <value>The delay before the first recovery attempt. Default is 5 seconds.</value>
    /// <remarks>
    /// Initial recovery delay prevents immediate retry storms and allows transient
    /// issues to resolve naturally before attempting reconnection.
    /// </remarks>
    public TimeSpan InitialRecoveryDelay { get; set; } = TimeSpan.FromSeconds(5);

    /// <summary>
    /// Gets or sets the maximum delay between recovery attempts.
    /// </summary>
    /// <value>The maximum delay between recovery attempts. Default is 5 minutes.</value>
    /// <remarks>
    /// Maximum recovery delay prevents excessive waiting periods while still providing
    /// backpressure during extended outages.
    /// </remarks>
    public TimeSpan MaxRecoveryDelay { get; set; } = TimeSpan.FromMinutes(5);

    /// <summary>
    /// Gets or sets the backoff multiplier for recovery delay calculation.
    /// </summary>
    /// <value>The multiplier applied to recovery delays for exponential backoff. Default is 2.0.</value>
    /// <remarks>
    /// Backoff multiplier controls how quickly recovery delays increase with each
    /// failed attempt, providing exponential backoff behavior to reduce load on
    /// failing systems.
    /// </remarks>
    public double BackoffMultiplier { get; set; } = 2.0;

    /// <summary>
    /// Gets or sets the maximum number of recovery attempts before giving up.
    /// </summary>
    /// <value>The maximum recovery attempts, or -1 for unlimited attempts. Default is -1 (unlimited).</value>
    /// <remarks>
    /// Maximum recovery attempts provide a circuit breaker mechanism to prevent
    /// infinite retry loops. Set to -1 for unlimited attempts in scenarios where
    /// the system should never stop trying to recover.
    /// </remarks>
    public int MaxRecoveryAttempts { get; set; } = -1; // Unlimited

    /// <summary>
    /// Gets or sets a value indicating whether topology recovery is enabled.
    /// </summary>
    /// <value><c>true</c> to recover exchanges, queues, and bindings; otherwise, <c>false</c>. Default is <c>true</c>.</value>
    /// <remarks>
    /// Topology recovery automatically recreates exchanges, queues, and bindings
    /// that were declared before the connection was lost. This ensures that the
    /// messaging infrastructure is restored to its previous state.
    /// </remarks>
    public bool RecoverTopology { get; set; } = true;

    /// <summary>
    /// Gets or sets a value indicating whether consumer recovery is enabled.
    /// </summary>
    /// <value><c>true</c> to recover consumers and their subscriptions; otherwise, <c>false</c>. Default is <c>true</c>.</value>
    /// <remarks>
    /// Consumer recovery automatically restores consumer subscriptions and their
    /// configuration after connection recovery. This ensures that message consumption
    /// resumes without manual intervention.
    /// </remarks>
    public bool RecoverConsumers { get; set; } = true;

    /// <summary>
    /// Gets or sets the recovery retry policy for handling recovery failures.
    /// </summary>
    /// <value>The retry policy for recovery operations, or null to use exponential backoff.</value>
    /// <remarks>
    /// Recovery retry policy defines how the system retries individual recovery
    /// operations (like topology declaration or consumer restoration) when they fail.
    /// </remarks>
    public RetryPolicy? RecoveryRetryPolicy { get; set; }

    /// <summary>
    /// Creates a recovery configuration with default settings suitable for most scenarios.
    /// </summary>
    /// <returns>A recovery configuration with balanced defaults.</returns>
    public static RecoveryConfiguration CreateDefault()
    {
        return new RecoveryConfiguration
        {
            EnableAutoRecovery = true,
            InitialRecoveryDelay = TimeSpan.FromSeconds(5),
            MaxRecoveryDelay = TimeSpan.FromMinutes(5),
            BackoffMultiplier = 2.0,
            MaxRecoveryAttempts = -1,
            RecoverTopology = true,
            RecoverConsumers = true,
            RecoveryRetryPolicy = RetryPolicy.CreateExponentialBackoff(maxAttempts: 5)
        };
    }

    /// <summary>
    /// Creates a recovery configuration optimized for high-availability scenarios.
    /// </summary>
    /// <returns>A recovery configuration with aggressive recovery settings.</returns>
    public static RecoveryConfiguration CreateRobust()
    {
        return new RecoveryConfiguration
        {
            EnableAutoRecovery = true,
            InitialRecoveryDelay = TimeSpan.FromSeconds(1),
            MaxRecoveryDelay = TimeSpan.FromMinutes(2),
            BackoffMultiplier = 1.5,
            MaxRecoveryAttempts = -1,
            RecoverTopology = true,
            RecoverConsumers = true,
            RecoveryRetryPolicy = RetryPolicy.CreateExponentialBackoff(
                baseDelay: TimeSpan.FromMilliseconds(500),
                maxDelay: TimeSpan.FromSeconds(30),
                maxAttempts: 10)
        };
    }

    /// <summary>
    /// Creates a recovery configuration optimized for high-throughput scenarios.
    /// </summary>
    /// <returns>A recovery configuration with performance-focused settings.</returns>
    public static RecoveryConfiguration CreateHighThroughput()
    {
        return new RecoveryConfiguration
        {
            EnableAutoRecovery = true,
            InitialRecoveryDelay = TimeSpan.FromSeconds(2),
            MaxRecoveryDelay = TimeSpan.FromMinutes(1),
            BackoffMultiplier = 1.5,
            MaxRecoveryAttempts = 20, // Limited for performance
            RecoverTopology = true,
            RecoverConsumers = true,
            RecoveryRetryPolicy = RetryPolicy.CreateLinear(
                delay: TimeSpan.FromSeconds(1),
                maxAttempts: 5)
        };
    }

    /// <summary>
    /// Validates the recovery configuration for consistency and reasonableness.
    /// </summary>
    /// <exception cref="BrokerConfigurationException">Thrown when the configuration is invalid.</exception>
    public void Validate()
    {
        if (InitialRecoveryDelay < TimeSpan.Zero)
        {
            throw new BrokerConfigurationException(
                "Initial recovery delay cannot be negative.",
                configurationSection: nameof(RecoveryConfiguration),
                parameterName: nameof(InitialRecoveryDelay),
                parameterValue: InitialRecoveryDelay);
        }

        if (MaxRecoveryDelay < TimeSpan.Zero)
        {
            throw new BrokerConfigurationException(
                "Maximum recovery delay cannot be negative.",
                configurationSection: nameof(RecoveryConfiguration),
                parameterName: nameof(MaxRecoveryDelay),
                parameterValue: MaxRecoveryDelay);
        }

        if (MaxRecoveryDelay < InitialRecoveryDelay)
        {
            throw new BrokerConfigurationException(
                "Maximum recovery delay cannot be less than initial recovery delay.",
                configurationSection: nameof(RecoveryConfiguration));
        }

        if (BackoffMultiplier <= 0)
        {
            throw new BrokerConfigurationException(
                "Backoff multiplier must be positive.",
                configurationSection: nameof(RecoveryConfiguration),
                parameterName: nameof(BackoffMultiplier),
                parameterValue: BackoffMultiplier);
        }

        if (MaxRecoveryAttempts is < -1 or 0)
        {
            throw new BrokerConfigurationException(
                "Maximum recovery attempts must be positive or -1 for unlimited.",
                configurationSection: nameof(RecoveryConfiguration),
                parameterName: nameof(MaxRecoveryAttempts),
                parameterValue: MaxRecoveryAttempts);
        }
    }

    /// <summary>
    /// Returns a string representation of the recovery configuration.
    /// </summary>
    /// <returns>A formatted string describing the recovery settings.</returns>
    public override string ToString()
    {
        var enabled = EnableAutoRecovery ? "enabled" : "disabled";
        var attempts = MaxRecoveryAttempts == -1 ? "unlimited" : MaxRecoveryAttempts.ToString();
        return $"Recovery[{enabled}, max attempts: {attempts}, backoff: {BackoffMultiplier}x]";
    }
}