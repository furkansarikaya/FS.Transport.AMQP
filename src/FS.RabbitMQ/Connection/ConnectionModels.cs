namespace FS.RabbitMQ.Connection;

/// <summary>
/// Metrics aggregation levels
/// </summary>
public enum MetricsAggregation
{
    /// <summary>
    /// Raw data points
    /// </summary>
    Raw,
    
    /// <summary>
    /// Summary statistics
    /// </summary>
    Summary,
    
    /// <summary>
    /// Detailed breakdown
    /// </summary>
    Detailed,
    
    /// <summary>
    /// Aggregated over time periods
    /// </summary>
    Aggregated
}

/// <summary>
/// Connection performance metrics
/// </summary>
public class ConnectionPerformanceMetrics
{
    /// <summary>
    /// Connection establishment time in milliseconds
    /// </summary>
    public double ConnectionTime { get; set; }
    
    /// <summary>
    /// Average round-trip time in milliseconds
    /// </summary>
    public double RoundTripTime { get; set; }
    
    /// <summary>
    /// Connection throughput in operations per second
    /// </summary>
    public double Throughput { get; set; }
    
    /// <summary>
    /// Channel creation rate per second
    /// </summary>
    public double ChannelCreationRate { get; set; }
    
    /// <summary>
    /// Channel pooling hit rate percentage
    /// </summary>
    public double PoolingHitRate { get; set; }
    
    /// <summary>
    /// Network utilization percentage
    /// </summary>
    public double NetworkUtilization { get; set; }
    
    /// <summary>
    /// Memory usage in bytes
    /// </summary>
    public long MemoryUsage { get; set; }
    
    /// <summary>
    /// CPU usage percentage
    /// </summary>
    public double CpuUsage { get; set; }
    
    /// <summary>
    /// Error rate percentage
    /// </summary>
    public double ErrorRate { get; set; }
    
    /// <summary>
    /// Recovery success rate percentage
    /// </summary>
    public double RecoverySuccessRate { get; set; }
    
    /// <summary>
    /// Metrics collection timestamp
    /// </summary>
    public DateTimeOffset Timestamp { get; set; } = DateTimeOffset.UtcNow;
}

/// <summary>
/// Connection reliability metrics
/// </summary>
public class ConnectionReliabilityMetrics
{
    /// <summary>
    /// Connection uptime percentage
    /// </summary>
    public double UptimePercentage { get; set; }
    
    /// <summary>
    /// Mean time between failures (MTBF) in hours
    /// </summary>
    public double MeanTimeBetweenFailures { get; set; }
    
    /// <summary>
    /// Mean time to recovery (MTTR) in minutes
    /// </summary>
    public double MeanTimeToRecovery { get; set; }
    
    /// <summary>
    /// Total number of connection failures
    /// </summary>
    public long TotalFailures { get; set; }
    
    /// <summary>
    /// Total number of recovery attempts
    /// </summary>
    public long TotalRecoveryAttempts { get; set; }
    
    /// <summary>
    /// Successful recovery count
    /// </summary>
    public long SuccessfulRecoveries { get; set; }
    
    /// <summary>
    /// Maximum consecutive failures
    /// </summary>
    public int MaxConsecutiveFailures { get; set; }
    
    /// <summary>
    /// Current consecutive failures
    /// </summary>
    public int CurrentConsecutiveFailures { get; set; }
    
    /// <summary>
    /// Last failure timestamp
    /// </summary>
    public DateTimeOffset? LastFailure { get; set; }
    
    /// <summary>
    /// Last recovery timestamp
    /// </summary>
    public DateTimeOffset? LastRecovery { get; set; }
}

/// <summary>
/// Channel pool metrics
/// </summary>
public class ChannelPoolMetrics
{
    /// <summary>
    /// Total channels in pool
    /// </summary>
    public int TotalChannels { get; set; }
    
    /// <summary>
    /// Active channels count
    /// </summary>
    public int ActiveChannels { get; set; }
    
    /// <summary>
    /// Idle channels count
    /// </summary>
    public int IdleChannels { get; set; }
    
    /// <summary>
    /// Peak channels count
    /// </summary>
    public int PeakChannels { get; set; }
    
    /// <summary>
    /// Average channel usage
    /// </summary>
    public double AverageChannelUsage { get; set; }
    
    /// <summary>
    /// Pool utilization percentage
    /// </summary>
    public double PoolUtilization { get; set; }
    
    /// <summary>
    /// Channel creation rate per second
    /// </summary>
    public double ChannelCreationRate { get; set; }
    
    /// <summary>
    /// Channel disposal rate per second
    /// </summary>
    public double ChannelDisposalRate { get; set; }
    
    /// <summary>
    /// Pool hit rate percentage
    /// </summary>
    public double PoolHitRate { get; set; }
    
    /// <summary>
    /// Pool miss rate percentage
    /// </summary>
    public double PoolMissRate { get; set; }
    
    /// <summary>
    /// Average channel lifetime in minutes
    /// </summary>
    public double AverageChannelLifetime { get; set; }
    
    /// <summary>
    /// Expired channels count
    /// </summary>
    public int ExpiredChannels { get; set; }
}

/// <summary>
/// Connection health score
/// </summary>
public class ConnectionHealthScore
{
    /// <summary>
    /// Overall health score (0-100)
    /// </summary>
    public double OverallScore { get; set; }
    
    /// <summary>
    /// Connection stability score (0-100)
    /// </summary>
    public double StabilityScore { get; set; }
    
    /// <summary>
    /// Performance score (0-100)
    /// </summary>
    public double PerformanceScore { get; set; }
    
    /// <summary>
    /// Reliability score (0-100)
    /// </summary>
    public double ReliabilityScore { get; set; }
    
    /// <summary>
    /// Resource utilization score (0-100)
    /// </summary>
    public double ResourceUtilizationScore { get; set; }
    
    /// <summary>
    /// Error rate score (0-100)
    /// </summary>
    public double ErrorRateScore { get; set; }
    
    /// <summary>
    /// Recovery capability score (0-100)
    /// </summary>
    public double RecoveryCapabilityScore { get; set; }
    
    /// <summary>
    /// Health score factors
    /// </summary>
    public Dictionary<string, double> Factors { get; set; } = new();
    
    /// <summary>
    /// Health recommendations
    /// </summary>
    public List<string> Recommendations { get; set; } = new();
    
    /// <summary>
    /// Score calculation timestamp
    /// </summary>
    public DateTimeOffset Timestamp { get; set; } = DateTimeOffset.UtcNow;
}

/// <summary>
/// Connection security metrics
/// </summary>
public class ConnectionSecurityMetrics
{
    /// <summary>
    /// SSL/TLS encryption enabled
    /// </summary>
    public bool SslEnabled { get; set; }
    
    /// <summary>
    /// SSL/TLS protocol version
    /// </summary>
    public string? SslProtocol { get; set; }
    
    /// <summary>
    /// Certificate validation status
    /// </summary>
    public bool CertificateValid { get; set; }
    
    /// <summary>
    /// Certificate expiration date
    /// </summary>
    public DateTimeOffset? CertificateExpiration { get; set; }
    
    /// <summary>
    /// Authentication method
    /// </summary>
    public string AuthenticationMethod { get; set; } = string.Empty;
    
    /// <summary>
    /// Authentication failures count
    /// </summary>
    public long AuthenticationFailures { get; set; }
    
    /// <summary>
    /// Last authentication failure timestamp
    /// </summary>
    public DateTimeOffset? LastAuthenticationFailure { get; set; }
    
    /// <summary>
    /// Connection security score (0-100)
    /// </summary>
    public double SecurityScore { get; set; }
    
    /// <summary>
    /// Security warnings
    /// </summary>
    public List<string> SecurityWarnings { get; set; } = new();
    
    /// <summary>
    /// Security recommendations
    /// </summary>
    public List<string> SecurityRecommendations { get; set; } = new();
}

/// <summary>
/// Connection diagnostic information
/// </summary>
public class ConnectionDiagnosticInfo
{
    /// <summary>
    /// Connection ID
    /// </summary>
    public string ConnectionId { get; set; } = string.Empty;
    
    /// <summary>
    /// Connection name
    /// </summary>
    public string? ConnectionName { get; set; }
    
    /// <summary>
    /// Server endpoint
    /// </summary>
    public string ServerEndpoint { get; set; } = string.Empty;
    
    /// <summary>
    /// Client endpoint
    /// </summary>
    public string ClientEndpoint { get; set; } = string.Empty;
    
    /// <summary>
    /// Protocol version
    /// </summary>
    public string ProtocolVersion { get; set; } = string.Empty;
    
    /// <summary>
    /// Server properties
    /// </summary>
    public Dictionary<string, object> ServerProperties { get; set; } = new();
    
    /// <summary>
    /// Client properties
    /// </summary>
    public Dictionary<string, object> ClientProperties { get; set; } = new();
    
    /// <summary>
    /// Connection capabilities
    /// </summary>
    public List<string> Capabilities { get; set; } = new();
    
    /// <summary>
    /// Heartbeat interval in seconds
    /// </summary>
    public int HeartbeatInterval { get; set; }
    
    /// <summary>
    /// Frame max size
    /// </summary>
    public uint FrameMax { get; set; }
    
    /// <summary>
    /// Channel max count
    /// </summary>
    public ushort ChannelMax { get; set; }
    
    /// <summary>
    /// Connection establishment time
    /// </summary>
    public DateTimeOffset EstablishedAt { get; set; }
    
    /// <summary>
    /// Last heartbeat timestamp
    /// </summary>
    public DateTimeOffset? LastHeartbeat { get; set; }
    
    /// <summary>
    /// Diagnostic flags
    /// </summary>
    public Dictionary<string, bool> DiagnosticFlags { get; set; } = new();
    
    /// <summary>
    /// Performance metrics
    /// </summary>
    public ConnectionPerformanceMetrics? PerformanceMetrics { get; set; }
    
    /// <summary>
    /// Reliability metrics
    /// </summary>
    public ConnectionReliabilityMetrics? ReliabilityMetrics { get; set; }
    
    /// <summary>
    /// Security metrics
    /// </summary>
    public ConnectionSecurityMetrics? SecurityMetrics { get; set; }
    
    /// <summary>
    /// Pool metrics
    /// </summary>
    public ChannelPoolMetrics? PoolMetrics { get; set; }
    
    /// <summary>
    /// Health score
    /// </summary>
    public ConnectionHealthScore? HealthScore { get; set; }
}

/// <summary>
/// Connection recovery context
/// </summary>
public class ConnectionRecoveryContext
{
    /// <summary>
    /// Recovery ID
    /// </summary>
    public string RecoveryId { get; set; } = string.Empty;
    
    /// <summary>
    /// Recovery strategy used
    /// </summary>
    public ConnectionRecoveryStrategy Strategy { get; set; }
    
    /// <summary>
    /// Recovery start time
    /// </summary>
    public DateTimeOffset StartTime { get; set; }
    
    /// <summary>
    /// Recovery end time
    /// </summary>
    public DateTimeOffset? EndTime { get; set; }
    
    /// <summary>
    /// Recovery duration
    /// </summary>
    public TimeSpan? Duration => EndTime - StartTime;
    
    /// <summary>
    /// Recovery attempt number
    /// </summary>
    public int AttemptNumber { get; set; }
    
    /// <summary>
    /// Maximum attempts allowed
    /// </summary>
    public int MaxAttempts { get; set; }
    
    /// <summary>
    /// Recovery success status
    /// </summary>
    public bool? Success { get; set; }
    
    /// <summary>
    /// Recovery failure reason
    /// </summary>
    public string? FailureReason { get; set; }
    
    /// <summary>
    /// Recovery exception
    /// </summary>
    public Exception? Exception { get; set; }
    
    /// <summary>
    /// Recovery steps performed
    /// </summary>
    public List<RecoveryStep> Steps { get; set; } = new();
    
    /// <summary>
    /// Recovery metadata
    /// </summary>
    public Dictionary<string, object> Metadata { get; set; } = new();
}

/// <summary>
/// Recovery step information
/// </summary>
public class RecoveryStep
{
    /// <summary>
    /// Step ID
    /// </summary>
    public string StepId { get; set; } = string.Empty;
    
    /// <summary>
    /// Step name
    /// </summary>
    public string StepName { get; set; } = string.Empty;
    
    /// <summary>
    /// Step description
    /// </summary>
    public string Description { get; set; } = string.Empty;
    
    /// <summary>
    /// Step start time
    /// </summary>
    public DateTimeOffset StartTime { get; set; }
    
    /// <summary>
    /// Step end time
    /// </summary>
    public DateTimeOffset? EndTime { get; set; }
    
    /// <summary>
    /// Step duration
    /// </summary>
    public TimeSpan? Duration => EndTime - StartTime;
    
    /// <summary>
    /// Step success status
    /// </summary>
    public bool? Success { get; set; }
    
    /// <summary>
    /// Step error message
    /// </summary>
    public string? ErrorMessage { get; set; }
    
    /// <summary>
    /// Step exception
    /// </summary>
    public Exception? Exception { get; set; }
    
    /// <summary>
    /// Step retry count
    /// </summary>
    public int RetryCount { get; set; }
    
    /// <summary>
    /// Step metadata
    /// </summary>
    public Dictionary<string, object> Metadata { get; set; } = new();
}

/// <summary>
/// Connection monitoring thresholds
/// </summary>
public class ConnectionMonitoringThresholds
{
    /// <summary>
    /// Maximum connection time in milliseconds
    /// </summary>
    public double MaxConnectionTime { get; set; } = 5000;
    
    /// <summary>
    /// Maximum round-trip time in milliseconds
    /// </summary>
    public double MaxRoundTripTime { get; set; } = 1000;
    
    /// <summary>
    /// Minimum throughput in operations per second
    /// </summary>
    public double MinThroughput { get; set; } = 100;
    
    /// <summary>
    /// Maximum error rate percentage
    /// </summary>
    public double MaxErrorRate { get; set; } = 5.0;
    
    /// <summary>
    /// Minimum uptime percentage
    /// </summary>
    public double MinUptimePercentage { get; set; } = 99.0;
    
    /// <summary>
    /// Maximum recovery time in minutes
    /// </summary>
    public double MaxRecoveryTime { get; set; } = 5.0;
    
    /// <summary>
    /// Minimum pool efficiency percentage
    /// </summary>
    public double MinPoolEfficiency { get; set; } = 80.0;
    
    /// <summary>
    /// Maximum memory usage in bytes
    /// </summary>
    public long MaxMemoryUsage { get; set; } = 500_000_000; // 500MB
    
    /// <summary>
    /// Maximum CPU usage percentage
    /// </summary>
    public double MaxCpuUsage { get; set; } = 80.0;
    
    /// <summary>
    /// Maximum consecutive failures
    /// </summary>
    public int MaxConsecutiveFailures { get; set; } = 3;
    
    /// <summary>
    /// Creates default thresholds
    /// </summary>
    /// <returns>Default monitoring thresholds</returns>
    public static ConnectionMonitoringThresholds CreateDefault()
    {
        return new ConnectionMonitoringThresholds();
    }
    
    /// <summary>
    /// Creates strict thresholds for production
    /// </summary>
    /// <returns>Strict monitoring thresholds</returns>
    public static ConnectionMonitoringThresholds CreateStrict()
    {
        return new ConnectionMonitoringThresholds
        {
            MaxConnectionTime = 3000,
            MaxRoundTripTime = 500,
            MinThroughput = 500,
            MaxErrorRate = 1.0,
            MinUptimePercentage = 99.9,
            MaxRecoveryTime = 2.0,
            MinPoolEfficiency = 90.0,
            MaxMemoryUsage = 200_000_000, // 200MB
            MaxCpuUsage = 50.0,
            MaxConsecutiveFailures = 2
        };
    }
    
    /// <summary>
    /// Creates lenient thresholds for development
    /// </summary>
    /// <returns>Lenient monitoring thresholds</returns>
    public static ConnectionMonitoringThresholds CreateLenient()
    {
        return new ConnectionMonitoringThresholds
        {
            MaxConnectionTime = 10000,
            MaxRoundTripTime = 2000,
            MinThroughput = 50,
            MaxErrorRate = 10.0,
            MinUptimePercentage = 95.0,
            MaxRecoveryTime = 10.0,
            MinPoolEfficiency = 70.0,
            MaxMemoryUsage = 1_000_000_000, // 1GB
            MaxCpuUsage = 90.0,
            MaxConsecutiveFailures = 5
        };
    }
}

/// <summary>
/// Connection optimization recommendations
/// </summary>
public class ConnectionOptimizationRecommendations
{
    /// <summary>
    /// Performance recommendations
    /// </summary>
    public List<OptimizationRecommendation> Performance { get; set; } = new();
    
    /// <summary>
    /// Reliability recommendations
    /// </summary>
    public List<OptimizationRecommendation> Reliability { get; set; } = new();
    
    /// <summary>
    /// Security recommendations
    /// </summary>
    public List<OptimizationRecommendation> Security { get; set; } = new();
    
    /// <summary>
    /// Resource utilization recommendations
    /// </summary>
    public List<OptimizationRecommendation> ResourceUtilization { get; set; } = new();
    
    /// <summary>
    /// Configuration recommendations
    /// </summary>
    public List<OptimizationRecommendation> Configuration { get; set; } = new();
    
    /// <summary>
    /// Monitoring recommendations
    /// </summary>
    public List<OptimizationRecommendation> Monitoring { get; set; } = new();
    
    /// <summary>
    /// All recommendations
    /// </summary>
    public IEnumerable<OptimizationRecommendation> All => 
        Performance.Concat(Reliability).Concat(Security)
            .Concat(ResourceUtilization).Concat(Configuration).Concat(Monitoring);
    
    /// <summary>
    /// Critical recommendations
    /// </summary>
    public IEnumerable<OptimizationRecommendation> Critical => 
        All.Where(r => r.Priority == RecommendationPriority.Critical);
    
    /// <summary>
    /// High priority recommendations
    /// </summary>
    public IEnumerable<OptimizationRecommendation> High => 
        All.Where(r => r.Priority == RecommendationPriority.High);
    
    /// <summary>
    /// Medium priority recommendations
    /// </summary>
    public IEnumerable<OptimizationRecommendation> Medium => 
        All.Where(r => r.Priority == RecommendationPriority.Medium);
    
    /// <summary>
    /// Low priority recommendations
    /// </summary>
    public IEnumerable<OptimizationRecommendation> Low => 
        All.Where(r => r.Priority == RecommendationPriority.Low);
    
    /// <summary>
    /// Recommendation count
    /// </summary>
    public int TotalCount => All.Count();
    
    /// <summary>
    /// Recommendations generation timestamp
    /// </summary>
    public DateTimeOffset Timestamp { get; set; } = DateTimeOffset.UtcNow;
}

/// <summary>
/// Optimization recommendation
/// </summary>
public class OptimizationRecommendation
{
    /// <summary>
    /// Recommendation ID
    /// </summary>
    public string Id { get; set; } = string.Empty;
    
    /// <summary>
    /// Recommendation title
    /// </summary>
    public string Title { get; set; } = string.Empty;
    
    /// <summary>
    /// Recommendation description
    /// </summary>
    public string Description { get; set; } = string.Empty;
    
    /// <summary>
    /// Recommendation category
    /// </summary>
    public RecommendationCategory Category { get; set; }
    
    /// <summary>
    /// Recommendation priority
    /// </summary>
    public RecommendationPriority Priority { get; set; }
    
    /// <summary>
    /// Recommendation impact
    /// </summary>
    public RecommendationImpact Impact { get; set; }
    
    /// <summary>
    /// Implementation effort
    /// </summary>
    public ImplementationEffort Effort { get; set; }
    
    /// <summary>
    /// Expected benefit
    /// </summary>
    public string ExpectedBenefit { get; set; } = string.Empty;
    
    /// <summary>
    /// Implementation steps
    /// </summary>
    public List<string> ImplementationSteps { get; set; } = new();
    
    /// <summary>
    /// Related metrics
    /// </summary>
    public List<string> RelatedMetrics { get; set; } = new();
    
    /// <summary>
    /// Recommendation metadata
    /// </summary>
    public Dictionary<string, object> Metadata { get; set; } = new();
    
    /// <summary>
    /// Recommendation timestamp
    /// </summary>
    public DateTimeOffset Timestamp { get; set; } = DateTimeOffset.UtcNow;
}

/// <summary>
/// Recommendation categories
/// </summary>
public enum RecommendationCategory
{
    /// <summary>
    /// Performance optimization
    /// </summary>
    Performance,
    
    /// <summary>
    /// Reliability improvement
    /// </summary>
    Reliability,
    
    /// <summary>
    /// Security enhancement
    /// </summary>
    Security,
    
    /// <summary>
    /// Resource optimization
    /// </summary>
    ResourceOptimization,
    
    /// <summary>
    /// Configuration tuning
    /// </summary>
    Configuration,
    
    /// <summary>
    /// Monitoring improvement
    /// </summary>
    Monitoring,
    
    /// <summary>
    /// Maintenance task
    /// </summary>
    Maintenance
}

/// <summary>
/// Recommendation priorities
/// </summary>
public enum RecommendationPriority
{
    /// <summary>
    /// Low priority
    /// </summary>
    Low,
    
    /// <summary>
    /// Medium priority
    /// </summary>
    Medium,
    
    /// <summary>
    /// High priority
    /// </summary>
    High,
    
    /// <summary>
    /// Critical priority
    /// </summary>
    Critical
}

/// <summary>
/// Recommendation impacts
/// </summary>
public enum RecommendationImpact
{
    /// <summary>
    /// Low impact
    /// </summary>
    Low,
    
    /// <summary>
    /// Medium impact
    /// </summary>
    Medium,
    
    /// <summary>
    /// High impact
    /// </summary>
    High,
    
    /// <summary>
    /// Critical impact
    /// </summary>
    Critical
}

/// <summary>
/// Implementation effort levels
/// </summary>
public enum ImplementationEffort
{
    /// <summary>
    /// Minimal effort required
    /// </summary>
    Minimal,
    
    /// <summary>
    /// Low effort required
    /// </summary>
    Low,
    
    /// <summary>
    /// Medium effort required
    /// </summary>
    Medium,
    
    /// <summary>
    /// High effort required
    /// </summary>
    High,
    
    /// <summary>
    /// Extensive effort required
    /// </summary>
    Extensive
} 