using FS.Mediator.Features.RequestHandling.Core;

namespace FS.Transport.AMQP.Connection;

/// <summary>
/// Query to get connection status
/// </summary>
public class GetConnectionStatusQuery : IRequest<ConnectionStatusQueryResult>
{
    /// <summary>
    /// Whether to include detailed information
    /// </summary>
    public bool IncludeDetails { get; set; }
    
    /// <summary>
    /// Whether to include statistics
    /// </summary>
    public bool IncludeStatistics { get; set; }
    
    /// <summary>
    /// Correlation ID for tracing
    /// </summary>
    public string? CorrelationId { get; set; }
    
    /// <summary>
    /// Creates a get connection status query
    /// </summary>
    /// <param name="includeDetails">Include detailed information</param>
    /// <param name="includeStatistics">Include statistics</param>
    /// <param name="correlationId">Correlation ID</param>
    /// <returns>Get connection status query</returns>
    public static GetConnectionStatusQuery Create(bool includeDetails = false, bool includeStatistics = false, 
        string? correlationId = null)
    {
        return new GetConnectionStatusQuery
        {
            IncludeDetails = includeDetails,
            IncludeStatistics = includeStatistics,
            CorrelationId = correlationId
        };
    }
}

/// <summary>
/// Query to get connection statistics
/// </summary>
public class GetConnectionStatisticsQuery : IRequest<ConnectionStatisticsQueryResult>
{
    /// <summary>
    /// Statistics categories to include
    /// </summary>
    public ConnectionStatisticsCategory Categories { get; set; } = ConnectionStatisticsCategory.All;
    
    /// <summary>
    /// Time period for statistics
    /// </summary>
    public TimeSpan? TimePeriod { get; set; }
    
    /// <summary>
    /// Whether to reset statistics after read
    /// </summary>
    public bool ResetAfterRead { get; set; }
    
    /// <summary>
    /// Correlation ID for tracing
    /// </summary>
    public string? CorrelationId { get; set; }
    
    /// <summary>
    /// Creates a get connection statistics query
    /// </summary>
    /// <param name="categories">Statistics categories</param>
    /// <param name="timePeriod">Time period</param>
    /// <param name="resetAfterRead">Reset after read</param>
    /// <param name="correlationId">Correlation ID</param>
    /// <returns>Get connection statistics query</returns>
    public static GetConnectionStatisticsQuery Create(ConnectionStatisticsCategory categories = ConnectionStatisticsCategory.All, 
        TimeSpan? timePeriod = null, bool resetAfterRead = false, string? correlationId = null)
    {
        return new GetConnectionStatisticsQuery
        {
            Categories = categories,
            TimePeriod = timePeriod,
            ResetAfterRead = resetAfterRead,
            CorrelationId = correlationId
        };
    }
}

/// <summary>
/// Query to get connection health information
/// </summary>
public class GetConnectionHealthQuery : IRequest<ConnectionHealthQueryResult>
{
    /// <summary>
    /// Health check type to perform
    /// </summary>
    public ConnectionTestType CheckType { get; set; } = ConnectionTestType.Basic;
    
    /// <summary>
    /// Whether to include detailed health information
    /// </summary>
    public bool IncludeDetails { get; set; }
    
    /// <summary>
    /// Health check timeout in milliseconds
    /// </summary>
    public int? TimeoutMs { get; set; }
    
    /// <summary>
    /// Correlation ID for tracing
    /// </summary>
    public string? CorrelationId { get; set; }
    
    /// <summary>
    /// Creates a get connection health query
    /// </summary>
    /// <param name="checkType">Health check type</param>
    /// <param name="includeDetails">Include detailed information</param>
    /// <param name="timeoutMs">Check timeout</param>
    /// <param name="correlationId">Correlation ID</param>
    /// <returns>Get connection health query</returns>
    public static GetConnectionHealthQuery Create(ConnectionTestType checkType = ConnectionTestType.Basic, 
        bool includeDetails = false, int? timeoutMs = null, string? correlationId = null)
    {
        return new GetConnectionHealthQuery
        {
            CheckType = checkType,
            IncludeDetails = includeDetails,
            TimeoutMs = timeoutMs,
            CorrelationId = correlationId
        };
    }
}

/// <summary>
/// Query to get active channels information
/// </summary>
public class GetActiveChannelsQuery : IRequest<ActiveChannelsQueryResult>
{
    /// <summary>
    /// Whether to include channel details
    /// </summary>
    public bool IncludeDetails { get; set; }
    
    /// <summary>
    /// Maximum number of channels to return
    /// </summary>
    public int? MaxResults { get; set; }
    
    /// <summary>
    /// Channel filter criteria
    /// </summary>
    public ChannelFilter? Filter { get; set; }
    
    /// <summary>
    /// Correlation ID for tracing
    /// </summary>
    public string? CorrelationId { get; set; }
    
    /// <summary>
    /// Creates a get active channels query
    /// </summary>
    /// <param name="includeDetails">Include channel details</param>
    /// <param name="maxResults">Maximum results</param>
    /// <param name="filter">Channel filter</param>
    /// <param name="correlationId">Correlation ID</param>
    /// <returns>Get active channels query</returns>
    public static GetActiveChannelsQuery Create(bool includeDetails = false, int? maxResults = null, 
        ChannelFilter? filter = null, string? correlationId = null)
    {
        return new GetActiveChannelsQuery
        {
            IncludeDetails = includeDetails,
            MaxResults = maxResults,
            Filter = filter,
            CorrelationId = correlationId
        };
    }
}

/// <summary>
/// Query to get connection metrics
/// </summary>
public class GetConnectionMetricsQuery : IRequest<ConnectionMetricsQueryResult>
{
    /// <summary>
    /// Metrics time period
    /// </summary>
    public TimeSpan TimePeriod { get; set; } = TimeSpan.FromMinutes(5);
    
    /// <summary>
    /// Metrics aggregation level
    /// </summary>
    public MetricsAggregation Aggregation { get; set; } = MetricsAggregation.Summary;
    
    /// <summary>
    /// Whether to include percentiles
    /// </summary>
    public bool IncludePercentiles { get; set; }
    
    /// <summary>
    /// Correlation ID for tracing
    /// </summary>
    public string? CorrelationId { get; set; }
    
    /// <summary>
    /// Creates a get connection metrics query
    /// </summary>
    /// <param name="timePeriod">Time period</param>
    /// <param name="aggregation">Aggregation level</param>
    /// <param name="includePercentiles">Include percentiles</param>
    /// <param name="correlationId">Correlation ID</param>
    /// <returns>Get connection metrics query</returns>
    public static GetConnectionMetricsQuery Create(TimeSpan? timePeriod = null, 
        MetricsAggregation aggregation = MetricsAggregation.Summary, bool includePercentiles = false, 
        string? correlationId = null)
    {
        return new GetConnectionMetricsQuery
        {
            TimePeriod = timePeriod ?? TimeSpan.FromMinutes(5),
            Aggregation = aggregation,
            IncludePercentiles = includePercentiles,
            CorrelationId = correlationId
        };
    }
}

/// <summary>
/// Query to get connection configuration
/// </summary>
public class GetConnectionConfigurationQuery : IRequest<ConnectionConfigurationQueryResult>
{
    /// <summary>
    /// Whether to include sensitive information
    /// </summary>
    public bool IncludeSensitive { get; set; }
    
    /// <summary>
    /// Configuration sections to include
    /// </summary>
    public ConnectionConfigurationSection Sections { get; set; } = ConnectionConfigurationSection.All;
    
    /// <summary>
    /// Correlation ID for tracing
    /// </summary>
    public string? CorrelationId { get; set; }
    
    /// <summary>
    /// Creates a get connection configuration query
    /// </summary>
    /// <param name="includeSensitive">Include sensitive information</param>
    /// <param name="sections">Configuration sections</param>
    /// <param name="correlationId">Correlation ID</param>
    /// <returns>Get connection configuration query</returns>
    public static GetConnectionConfigurationQuery Create(bool includeSensitive = false, 
        ConnectionConfigurationSection sections = ConnectionConfigurationSection.All, string? correlationId = null)
    {
        return new GetConnectionConfigurationQuery
        {
            IncludeSensitive = includeSensitive,
            Sections = sections,
            CorrelationId = correlationId
        };
    }
}

/// <summary>
/// Query to get connection history
/// </summary>
public class GetConnectionHistoryQuery : IRequest<ConnectionHistoryQueryResult>
{
    /// <summary>
    /// Start time for history period
    /// </summary>
    public DateTimeOffset? StartTime { get; set; }
    
    /// <summary>
    /// End time for history period
    /// </summary>
    public DateTimeOffset? EndTime { get; set; }
    
    /// <summary>
    /// Maximum number of history entries
    /// </summary>
    public int? MaxResults { get; set; }
    
    /// <summary>
    /// Event types to include
    /// </summary>
    public ConnectionEventType EventTypes { get; set; } = ConnectionEventType.All;
    
    /// <summary>
    /// Whether to include event details
    /// </summary>
    public bool IncludeDetails { get; set; }
    
    /// <summary>
    /// Correlation ID for tracing
    /// </summary>
    public string? CorrelationId { get; set; }
    
    /// <summary>
    /// Creates a get connection history query
    /// </summary>
    /// <param name="startTime">Start time</param>
    /// <param name="endTime">End time</param>
    /// <param name="maxResults">Maximum results</param>
    /// <param name="eventTypes">Event types</param>
    /// <param name="includeDetails">Include details</param>
    /// <param name="correlationId">Correlation ID</param>
    /// <returns>Get connection history query</returns>
    public static GetConnectionHistoryQuery Create(DateTimeOffset? startTime = null, DateTimeOffset? endTime = null, 
        int? maxResults = null, ConnectionEventType eventTypes = ConnectionEventType.All, 
        bool includeDetails = false, string? correlationId = null)
    {
        return new GetConnectionHistoryQuery
        {
            StartTime = startTime,
            EndTime = endTime,
            MaxResults = maxResults,
            EventTypes = eventTypes,
            IncludeDetails = includeDetails,
            CorrelationId = correlationId
        };
    }
}

// Supporting classes and enums

/// <summary>
/// Channel filter criteria
/// </summary>
public class ChannelFilter
{
    /// <summary>
    /// Filter by pooled channels only
    /// </summary>
    public bool? PooledOnly { get; set; }
    
    /// <summary>
    /// Filter by minimum channel number
    /// </summary>
    public int? MinChannelNumber { get; set; }
    
    /// <summary>
    /// Filter by maximum channel number
    /// </summary>
    public int? MaxChannelNumber { get; set; }
    
    /// <summary>
    /// Filter by channel state
    /// </summary>
    public ChannelState? State { get; set; }
    
    /// <summary>
    /// Filter by minimum age
    /// </summary>
    public TimeSpan? MinAge { get; set; }
    
    /// <summary>
    /// Filter by maximum age
    /// </summary>
    public TimeSpan? MaxAge { get; set; }
}

/// <summary>
/// Channel state enumeration
/// </summary>
public enum ChannelState
{
    /// <summary>
    /// Channel is open and available
    /// </summary>
    Open,
    
    /// <summary>
    /// Channel is closed
    /// </summary>
    Closed,
    
    /// <summary>
    /// Channel is in use
    /// </summary>
    InUse,
    
    /// <summary>
    /// Channel is being closed
    /// </summary>
    Closing
}

/// <summary>
/// Connection configuration sections
/// </summary>
[Flags]
public enum ConnectionConfigurationSection
{
    /// <summary>
    /// Connection details
    /// </summary>
    Connection = 1,
    
    /// <summary>
    /// SSL configuration
    /// </summary>
    Ssl = 2,
    
    /// <summary>
    /// Recovery settings
    /// </summary>
    Recovery = 4,
    
    /// <summary>
    /// Pool configuration
    /// </summary>
    Pool = 8,
    
    /// <summary>
    /// Authentication settings
    /// </summary>
    Authentication = 16,
    
    /// <summary>
    /// All configuration sections
    /// </summary>
    All = Connection | Ssl | Recovery | Pool | Authentication
}

/// <summary>
/// Connection event types
/// </summary>
[Flags]
public enum ConnectionEventType
{
    /// <summary>
    /// Connection established events
    /// </summary>
    Connected = 1,
    
    /// <summary>
    /// Connection lost events
    /// </summary>
    Disconnected = 2,
    
    /// <summary>
    /// Recovery events
    /// </summary>
    Recovery = 4,
    
    /// <summary>
    /// Error events
    /// </summary>
    Errors = 8,
    
    /// <summary>
    /// Channel events
    /// </summary>
    Channels = 16,
    
    /// <summary>
    /// All event types
    /// </summary>
    All = Connected | Disconnected | Recovery | Errors | Channels
}

// Result models

/// <summary>
/// Result of connection status query
/// </summary>
public class ConnectionStatusQueryResult
{
    /// <summary>
    /// Whether the query was successful
    /// </summary>
    public bool Success { get; set; }
    
    /// <summary>
    /// Connection status information
    /// </summary>
    public ConnectionStatusInfo? StatusInfo { get; set; }
    
    /// <summary>
    /// Error message if not successful
    /// </summary>
    public string? ErrorMessage { get; set; }
    
    /// <summary>
    /// Exception if one occurred
    /// </summary>
    public Exception? Exception { get; set; }
    
    /// <summary>
    /// Query timestamp
    /// </summary>
    public DateTimeOffset Timestamp { get; set; } = DateTimeOffset.UtcNow;
    
    /// <summary>
    /// Creates a success result
    /// </summary>
    /// <param name="statusInfo">Status information</param>
    /// <returns>Success result</returns>
    public static ConnectionStatusQueryResult CreateSuccess(ConnectionStatusInfo statusInfo)
    {
        return new ConnectionStatusQueryResult
        {
            Success = true,
            StatusInfo = statusInfo
        };
    }
    
    /// <summary>
    /// Creates a failure result
    /// </summary>
    /// <param name="errorMessage">Error message</param>
    /// <param name="exception">Exception</param>
    /// <returns>Failure result</returns>
    public static ConnectionStatusQueryResult CreateFailure(string errorMessage, Exception? exception = null)
    {
        return new ConnectionStatusQueryResult
        {
            Success = false,
            ErrorMessage = errorMessage,
            Exception = exception
        };
    }
}

/// <summary>
/// Connection status information
/// </summary>
public class ConnectionStatusInfo
{
    /// <summary>
    /// Current connection state
    /// </summary>
    public ConnectionState State { get; set; }
    
    /// <summary>
    /// Whether connection is healthy
    /// </summary>
    public bool IsHealthy { get; set; }
    
    /// <summary>
    /// Connection uptime
    /// </summary>
    public TimeSpan? Uptime { get; set; }
    
    /// <summary>
    /// Server host and port
    /// </summary>
    public string ServerEndpoint { get; set; } = string.Empty;
    
    /// <summary>
    /// Virtual host
    /// </summary>
    public string VirtualHost { get; set; } = string.Empty;
    
    /// <summary>
    /// Connected user
    /// </summary>
    public string UserName { get; set; } = string.Empty;
    
    /// <summary>
    /// Connection name
    /// </summary>
    public string? ConnectionName { get; set; }
    
    /// <summary>
    /// Active channels count
    /// </summary>
    public int ActiveChannels { get; set; }
    
    /// <summary>
    /// Pooled channels count
    /// </summary>
    public int PooledChannels { get; set; }
    
    /// <summary>
    /// Last connection time
    /// </summary>
    public DateTimeOffset? LastConnected { get; set; }
    
    /// <summary>
    /// Last disconnection time
    /// </summary>
    public DateTimeOffset? LastDisconnected { get; set; }
    
    /// <summary>
    /// Connection statistics
    /// </summary>
    public ConnectionStatistics? Statistics { get; set; }
}

/// <summary>
/// Result of connection statistics query
/// </summary>
public class ConnectionStatisticsQueryResult
{
    /// <summary>
    /// Whether the query was successful
    /// </summary>
    public bool Success { get; set; }
    
    /// <summary>
    /// Connection statistics
    /// </summary>
    public ConnectionStatistics? Statistics { get; set; }
    
    /// <summary>
    /// Statistics time period
    /// </summary>
    public TimeSpan? TimePeriod { get; set; }
    
    /// <summary>
    /// Categories included
    /// </summary>
    public ConnectionStatisticsCategory Categories { get; set; }
    
    /// <summary>
    /// Error message if not successful
    /// </summary>
    public string? ErrorMessage { get; set; }
    
    /// <summary>
    /// Exception if one occurred
    /// </summary>
    public Exception? Exception { get; set; }
    
    /// <summary>
    /// Query timestamp
    /// </summary>
    public DateTimeOffset Timestamp { get; set; } = DateTimeOffset.UtcNow;
    
    /// <summary>
    /// Creates a success result
    /// </summary>
    /// <param name="statistics">Connection statistics</param>
    /// <param name="timePeriod">Time period</param>
    /// <param name="categories">Categories included</param>
    /// <returns>Success result</returns>
    public static ConnectionStatisticsQueryResult CreateSuccess(ConnectionStatistics statistics, 
        TimeSpan? timePeriod, ConnectionStatisticsCategory categories)
    {
        return new ConnectionStatisticsQueryResult
        {
            Success = true,
            Statistics = statistics,
            TimePeriod = timePeriod,
            Categories = categories
        };
    }
    
    /// <summary>
    /// Creates a failure result
    /// </summary>
    /// <param name="errorMessage">Error message</param>
    /// <param name="exception">Exception</param>
    /// <returns>Failure result</returns>
    public static ConnectionStatisticsQueryResult CreateFailure(string errorMessage, Exception? exception = null)
    {
        return new ConnectionStatisticsQueryResult
        {
            Success = false,
            ErrorMessage = errorMessage,
            Exception = exception
        };
    }
}

/// <summary>
/// Result of connection health query
/// </summary>
public class ConnectionHealthQueryResult
{
    /// <summary>
    /// Whether the query was successful
    /// </summary>
    public bool Success { get; set; }
    
    /// <summary>
    /// Health check result
    /// </summary>
    public ConnectionTestResult? HealthResult { get; set; }
    
    /// <summary>
    /// Health status summary
    /// </summary>
    public HealthStatus HealthStatus { get; set; }
    
    /// <summary>
    /// Health details
    /// </summary>
    public Dictionary<string, object> HealthDetails { get; set; } = new();
    
    /// <summary>
    /// Error message if not successful
    /// </summary>
    public string? ErrorMessage { get; set; }
    
    /// <summary>
    /// Exception if one occurred
    /// </summary>
    public Exception? Exception { get; set; }
    
    /// <summary>
    /// Query timestamp
    /// </summary>
    public DateTimeOffset Timestamp { get; set; } = DateTimeOffset.UtcNow;
    
    /// <summary>
    /// Creates a success result
    /// </summary>
    /// <param name="healthResult">Health check result</param>
    /// <param name="healthStatus">Health status</param>
    /// <param name="healthDetails">Health details</param>
    /// <returns>Success result</returns>
    public static ConnectionHealthQueryResult CreateSuccess(ConnectionTestResult healthResult, 
        HealthStatus healthStatus, Dictionary<string, object>? healthDetails = null)
    {
        return new ConnectionHealthQueryResult
        {
            Success = true,
            HealthResult = healthResult,
            HealthStatus = healthStatus,
            HealthDetails = healthDetails ?? new Dictionary<string, object>()
        };
    }
    
    /// <summary>
    /// Creates a failure result
    /// </summary>
    /// <param name="errorMessage">Error message</param>
    /// <param name="exception">Exception</param>
    /// <returns>Failure result</returns>
    public static ConnectionHealthQueryResult CreateFailure(string errorMessage, Exception? exception = null)
    {
        return new ConnectionHealthQueryResult
        {
            Success = false,
            ErrorMessage = errorMessage,
            Exception = exception
        };
    }
}

/// <summary>
/// Health status enumeration
/// </summary>
public enum HealthStatus
{
    /// <summary>
    /// Unknown health status
    /// </summary>
    Unknown,
    
    /// <summary>
    /// Healthy status
    /// </summary>
    Healthy,
    
    /// <summary>
    /// Degraded performance
    /// </summary>
    Degraded,
    
    /// <summary>
    /// Unhealthy status
    /// </summary>
    Unhealthy,
    
    /// <summary>
    /// Critical failure
    /// </summary>
    Critical
}

/// <summary>
/// Result of active channels query
/// </summary>
public class ActiveChannelsQueryResult
{
    /// <summary>
    /// Whether the query was successful
    /// </summary>
    public bool Success { get; set; }
    
    /// <summary>
    /// Active channels information
    /// </summary>
    public IEnumerable<ChannelInfo> Channels { get; set; } = Enumerable.Empty<ChannelInfo>();
    
    /// <summary>
    /// Total count of active channels
    /// </summary>
    public int TotalCount { get; set; }
    
    /// <summary>
    /// Error message if not successful
    /// </summary>
    public string? ErrorMessage { get; set; }
    
    /// <summary>
    /// Exception if one occurred
    /// </summary>
    public Exception? Exception { get; set; }
    
    /// <summary>
    /// Query timestamp
    /// </summary>
    public DateTimeOffset Timestamp { get; set; } = DateTimeOffset.UtcNow;
    
    /// <summary>
    /// Creates a success result
    /// </summary>
    /// <param name="channels">Active channels</param>
    /// <param name="totalCount">Total count</param>
    /// <returns>Success result</returns>
    public static ActiveChannelsQueryResult CreateSuccess(IEnumerable<ChannelInfo> channels, int totalCount)
    {
        return new ActiveChannelsQueryResult
        {
            Success = true,
            Channels = channels,
            TotalCount = totalCount
        };
    }
    
    /// <summary>
    /// Creates a failure result
    /// </summary>
    /// <param name="errorMessage">Error message</param>
    /// <param name="exception">Exception</param>
    /// <returns>Failure result</returns>
    public static ActiveChannelsQueryResult CreateFailure(string errorMessage, Exception? exception = null)
    {
        return new ActiveChannelsQueryResult
        {
            Success = false,
            ErrorMessage = errorMessage,
            Exception = exception
        };
    }
}

/// <summary>
/// Channel information
/// </summary>
public class ChannelInfo
{
    /// <summary>
    /// Channel number
    /// </summary>
    public int Number { get; set; }
    
    /// <summary>
    /// Whether channel is pooled
    /// </summary>
    public bool IsPooled { get; set; }
    
    /// <summary>
    /// Channel state
    /// </summary>
    public ChannelState State { get; set; }
    
    /// <summary>
    /// Channel creation time
    /// </summary>
    public DateTimeOffset Created { get; set; }
    
    /// <summary>
    /// Last used time
    /// </summary>
    public DateTimeOffset LastUsed { get; set; }
    
    /// <summary>
    /// Channel purpose
    /// </summary>
    public string? Purpose { get; set; }
    
    /// <summary>
    /// Prefetch count
    /// </summary>
    public ushort? PrefetchCount { get; set; }
    
    /// <summary>
    /// Publisher confirms enabled
    /// </summary>
    public bool PublisherConfirms { get; set; }
    
    /// <summary>
    /// Channel metadata
    /// </summary>
    public Dictionary<string, object> Metadata { get; set; } = new();
}

/// <summary>
/// Result of connection metrics query
/// </summary>
public class ConnectionMetricsQueryResult
{
    /// <summary>
    /// Whether the query was successful
    /// </summary>
    public bool Success { get; set; }
    
    /// <summary>
    /// Connection metrics
    /// </summary>
    public ConnectionMetrics? Metrics { get; set; }
    
    /// <summary>
    /// Time period for metrics
    /// </summary>
    public TimeSpan TimePeriod { get; set; }
    
    /// <summary>
    /// Aggregation level used
    /// </summary>
    public MetricsAggregation Aggregation { get; set; }
    
    /// <summary>
    /// Error message if not successful
    /// </summary>
    public string? ErrorMessage { get; set; }
    
    /// <summary>
    /// Exception if one occurred
    /// </summary>
    public Exception? Exception { get; set; }
    
    /// <summary>
    /// Query timestamp
    /// </summary>
    public DateTimeOffset Timestamp { get; set; } = DateTimeOffset.UtcNow;
    
    /// <summary>
    /// Creates a success result
    /// </summary>
    /// <param name="metrics">Connection metrics</param>
    /// <param name="timePeriod">Time period</param>
    /// <param name="aggregation">Aggregation level</param>
    /// <returns>Success result</returns>
    public static ConnectionMetricsQueryResult CreateSuccess(ConnectionMetrics metrics, 
        TimeSpan timePeriod, MetricsAggregation aggregation)
    {
        return new ConnectionMetricsQueryResult
        {
            Success = true,
            Metrics = metrics,
            TimePeriod = timePeriod,
            Aggregation = aggregation
        };
    }
    
    /// <summary>
    /// Creates a failure result
    /// </summary>
    /// <param name="errorMessage">Error message</param>
    /// <param name="exception">Exception</param>
    /// <returns>Failure result</returns>
    public static ConnectionMetricsQueryResult CreateFailure(string errorMessage, Exception? exception = null)
    {
        return new ConnectionMetricsQueryResult
        {
            Success = false,
            ErrorMessage = errorMessage,
            Exception = exception
        };
    }
}

/// <summary>
/// Connection metrics data
/// </summary>
public class ConnectionMetrics
{
    /// <summary>
    /// Connection availability percentage
    /// </summary>
    public double AvailabilityPercentage { get; set; }
    
    /// <summary>
    /// Average connection latency in milliseconds
    /// </summary>
    public double AverageLatency { get; set; }
    
    /// <summary>
    /// Connection recovery rate
    /// </summary>
    public double RecoverySuccessRate { get; set; }
    
    /// <summary>
    /// Channel creation rate (per second)
    /// </summary>
    public double ChannelCreationRate { get; set; }
    
    /// <summary>
    /// Channel pooling efficiency percentage
    /// </summary>
    public double PoolingEfficiency { get; set; }
    
    /// <summary>
    /// Total bytes sent
    /// </summary>
    public long TotalBytesSent { get; set; }
    
    /// <summary>
    /// Total bytes received
    /// </summary>
    public long TotalBytesReceived { get; set; }
    
    /// <summary>
    /// Network throughput (bytes per second)
    /// </summary>
    public double NetworkThroughput { get; set; }
    
    /// <summary>
    /// Error rate percentage
    /// </summary>
    public double ErrorRate { get; set; }
    
    /// <summary>
    /// Metrics timestamp
    /// </summary>
    public DateTimeOffset Timestamp { get; set; } = DateTimeOffset.UtcNow;
}

/// <summary>
/// Result of connection configuration query
/// </summary>
public class ConnectionConfigurationQueryResult
{
    /// <summary>
    /// Whether the query was successful
    /// </summary>
    public bool Success { get; set; }
    
    /// <summary>
    /// Connection configuration
    /// </summary>
    public Dictionary<string, object> Configuration { get; set; } = new();
    
    /// <summary>
    /// Configuration sections included
    /// </summary>
    public ConnectionConfigurationSection Sections { get; set; }
    
    /// <summary>
    /// Whether sensitive data is included
    /// </summary>
    public bool IncludesSensitiveData { get; set; }
    
    /// <summary>
    /// Error message if not successful
    /// </summary>
    public string? ErrorMessage { get; set; }
    
    /// <summary>
    /// Exception if one occurred
    /// </summary>
    public Exception? Exception { get; set; }
    
    /// <summary>
    /// Query timestamp
    /// </summary>
    public DateTimeOffset Timestamp { get; set; } = DateTimeOffset.UtcNow;
    
    /// <summary>
    /// Creates a success result
    /// </summary>
    /// <param name="configuration">Configuration data</param>
    /// <param name="sections">Sections included</param>
    /// <param name="includesSensitiveData">Includes sensitive data</param>
    /// <returns>Success result</returns>
    public static ConnectionConfigurationQueryResult CreateSuccess(Dictionary<string, object> configuration, 
        ConnectionConfigurationSection sections, bool includesSensitiveData)
    {
        return new ConnectionConfigurationQueryResult
        {
            Success = true,
            Configuration = configuration,
            Sections = sections,
            IncludesSensitiveData = includesSensitiveData
        };
    }
    
    /// <summary>
    /// Creates a failure result
    /// </summary>
    /// <param name="errorMessage">Error message</param>
    /// <param name="exception">Exception</param>
    /// <returns>Failure result</returns>
    public static ConnectionConfigurationQueryResult CreateFailure(string errorMessage, Exception? exception = null)
    {
        return new ConnectionConfigurationQueryResult
        {
            Success = false,
            ErrorMessage = errorMessage,
            Exception = exception
        };
    }
}

/// <summary>
/// Result of connection history query
/// </summary>
public class ConnectionHistoryQueryResult
{
    /// <summary>
    /// Whether the query was successful
    /// </summary>
    public bool Success { get; set; }
    
    /// <summary>
    /// Connection history entries
    /// </summary>
    public IEnumerable<ConnectionHistoryEntry> HistoryEntries { get; set; } = Enumerable.Empty<ConnectionHistoryEntry>();
    
    /// <summary>
    /// Total count of history entries
    /// </summary>
    public int TotalCount { get; set; }
    
    /// <summary>
    /// Event types included
    /// </summary>
    public ConnectionEventType EventTypes { get; set; }
    
    /// <summary>
    /// Error message if not successful
    /// </summary>
    public string? ErrorMessage { get; set; }
    
    /// <summary>
    /// Exception if one occurred
    /// </summary>
    public Exception? Exception { get; set; }
    
    /// <summary>
    /// Query timestamp
    /// </summary>
    public DateTimeOffset Timestamp { get; set; } = DateTimeOffset.UtcNow;
    
    /// <summary>
    /// Creates a success result
    /// </summary>
    /// <param name="historyEntries">History entries</param>
    /// <param name="totalCount">Total count</param>
    /// <param name="eventTypes">Event types included</param>
    /// <returns>Success result</returns>
    public static ConnectionHistoryQueryResult CreateSuccess(IEnumerable<ConnectionHistoryEntry> historyEntries, 
        int totalCount, ConnectionEventType eventTypes)
    {
        return new ConnectionHistoryQueryResult
        {
            Success = true,
            HistoryEntries = historyEntries,
            TotalCount = totalCount,
            EventTypes = eventTypes
        };
    }
    
    /// <summary>
    /// Creates a failure result
    /// </summary>
    /// <param name="errorMessage">Error message</param>
    /// <param name="exception">Exception</param>
    /// <returns>Failure result</returns>
    public static ConnectionHistoryQueryResult CreateFailure(string errorMessage, Exception? exception = null)
    {
        return new ConnectionHistoryQueryResult
        {
            Success = false,
            ErrorMessage = errorMessage,
            Exception = exception
        };
    }
}

/// <summary>
/// Connection history entry
/// </summary>
public class ConnectionHistoryEntry
{
    /// <summary>
    /// Entry ID
    /// </summary>
    public string EntryId { get; set; } = string.Empty;
    
    /// <summary>
    /// Event type
    /// </summary>
    public ConnectionEventType EventType { get; set; }
    
    /// <summary>
    /// Event timestamp
    /// </summary>
    public DateTimeOffset Timestamp { get; set; }
    
    /// <summary>
    /// Event message
    /// </summary>
    public string Message { get; set; } = string.Empty;
    
    /// <summary>
    /// Connection state at the time of event
    /// </summary>
    public ConnectionState? ConnectionState { get; set; }
    
    /// <summary>
    /// Event severity
    /// </summary>
    public EventSeverity Severity { get; set; }
    
    /// <summary>
    /// Event details
    /// </summary>
    public Dictionary<string, object> Details { get; set; } = new();
    
    /// <summary>
    /// Related exception information
    /// </summary>
    public string? ExceptionInfo { get; set; }
}

/// <summary>
/// Event severity levels
/// </summary>
public enum EventSeverity
{
    /// <summary>
    /// Informational event
    /// </summary>
    Information,
    
    /// <summary>
    /// Warning event
    /// </summary>
    Warning,
    
    /// <summary>
    /// Error event
    /// </summary>
    Error,
    
    /// <summary>
    /// Critical event
    /// </summary>
    Critical
} 