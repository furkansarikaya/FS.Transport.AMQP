namespace FS.Transport.AMQP.Configuration;

/// <summary>
/// Settings for connection recovery behavior
/// </summary>
public class ConnectionRecoverySettings
{
    /// <summary>
    /// Whether to enable automatic connection recovery
    /// </summary>
    public bool EnableAutoRecovery { get; set; } = true;
    
    /// <summary>
    /// Initial delay before first recovery attempt
    /// </summary>
    public TimeSpan InitialDelay { get; set; } = TimeSpan.FromSeconds(5);
    
    /// <summary>
    /// Maximum delay between recovery attempts
    /// </summary>
    public TimeSpan MaxDelay { get; set; } = TimeSpan.FromMinutes(5);
    
    /// <summary>
    /// Multiplier for exponential backoff
    /// </summary>
    public double BackoffMultiplier { get; set; } = 2.0;
    
    /// <summary>
    /// Maximum number of recovery attempts (0 for unlimited)
    /// </summary>
    public int MaxAttempts { get; set; } = 0;
    
    /// <summary>
    /// Timeout for each recovery attempt
    /// </summary>
    public TimeSpan RecoveryTimeout { get; set; } = TimeSpan.FromSeconds(30);
    
    /// <summary>
    /// Whether to recover topology (exchanges, queues, bindings)
    /// </summary>
    public bool RecoverTopology { get; set; } = true;
    
    /// <summary>
    /// Whether to recover consumers
    /// </summary>
    public bool RecoverConsumers { get; set; } = true;
    
    /// <summary>
    /// Whether to recover publishers
    /// </summary>
    public bool RecoverPublishers { get; set; } = true;
    
    /// <summary>
    /// Jitter factor for randomizing recovery delays (0.0 to 1.0)
    /// </summary>
    public double JitterFactor { get; set; } = 0.1;
    
    /// <summary>
    /// Whether to log recovery attempts
    /// </summary>
    public bool LogRecoveryAttempts { get; set; } = true;
    
    /// <summary>
    /// Whether to throw exceptions on recovery failure
    /// </summary>
    public bool ThrowOnRecoveryFailure { get; set; } = false;

    /// <summary>
    /// Validates the recovery settings
    /// </summary>
    /// <exception cref="ArgumentException">Thrown when settings are invalid</exception>
    public void Validate()
    {
        if (InitialDelay < TimeSpan.Zero)
            throw new ArgumentException("InitialDelay cannot be negative", nameof(InitialDelay));
            
        if (MaxDelay < TimeSpan.Zero)
            throw new ArgumentException("MaxDelay cannot be negative", nameof(MaxDelay));
            
        if (BackoffMultiplier <= 0)
            throw new ArgumentException("BackoffMultiplier must be greater than 0", nameof(BackoffMultiplier));
            
        if (MaxAttempts < 0)
            throw new ArgumentException("MaxAttempts cannot be negative", nameof(MaxAttempts));
            
        if (RecoveryTimeout <= TimeSpan.Zero)
            throw new ArgumentException("RecoveryTimeout must be greater than zero", nameof(RecoveryTimeout));
            
        if (JitterFactor < 0.0 || JitterFactor > 1.0)
            throw new ArgumentException("JitterFactor must be between 0.0 and 1.0", nameof(JitterFactor));
    }
    
    /// <summary>
    /// Creates a copy of the recovery settings
    /// </summary>
    /// <returns>New instance with copied values</returns>
    public ConnectionRecoverySettings Clone()
    {
        return new ConnectionRecoverySettings
        {
            EnableAutoRecovery = EnableAutoRecovery,
            InitialDelay = InitialDelay,
            MaxDelay = MaxDelay,
            BackoffMultiplier = BackoffMultiplier,
            MaxAttempts = MaxAttempts,
            RecoveryTimeout = RecoveryTimeout,
            RecoverTopology = RecoverTopology,
            RecoverConsumers = RecoverConsumers,
            RecoverPublishers = RecoverPublishers,
            JitterFactor = JitterFactor,
            LogRecoveryAttempts = LogRecoveryAttempts,
            ThrowOnRecoveryFailure = ThrowOnRecoveryFailure
        };
    }
    
    /// <summary>
    /// Creates default settings optimized for high availability
    /// </summary>
    /// <returns>High availability recovery settings</returns>
    public static ConnectionRecoverySettings CreateHighAvailability()
    {
        return new ConnectionRecoverySettings
        {
            EnableAutoRecovery = true,
            InitialDelay = TimeSpan.FromSeconds(1),
            MaxDelay = TimeSpan.FromMinutes(1),
            BackoffMultiplier = 1.5,
            MaxAttempts = 0, // Unlimited
            RecoveryTimeout = TimeSpan.FromSeconds(15),
            RecoverTopology = true,
            RecoverConsumers = true,
            RecoverPublishers = true,
            JitterFactor = 0.2,
            LogRecoveryAttempts = true,
            ThrowOnRecoveryFailure = false
        };
    }
    
    /// <summary>
    /// Creates default settings optimized for development
    /// </summary>
    /// <returns>Development recovery settings</returns>
    public static ConnectionRecoverySettings CreateDevelopment()
    {
        return new ConnectionRecoverySettings
        {
            EnableAutoRecovery = true,
            InitialDelay = TimeSpan.FromSeconds(2),
            MaxDelay = TimeSpan.FromSeconds(30),
            BackoffMultiplier = 2.0,
            MaxAttempts = 10,
            RecoveryTimeout = TimeSpan.FromSeconds(10),
            RecoverTopology = true,
            RecoverConsumers = true,
            RecoverPublishers = true,
            JitterFactor = 0.1,
            LogRecoveryAttempts = true,
            ThrowOnRecoveryFailure = true
        };
    }
} 