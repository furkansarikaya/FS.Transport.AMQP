using FS.Mediator.Features.RequestHandling.Core;
using FS.Transport.AMQP.Configuration;
using RabbitMQ.Client;

namespace FS.Transport.AMQP.Connection;

/// <summary>
/// Command to establish connection to RabbitMQ server
/// </summary>
public class ConnectCommand : IRequest<ConnectionResult>
{
    /// <summary>
    /// Connection settings to use (optional, will use configured settings if not provided)
    /// </summary>
    public ConnectionSettings? Settings { get; set; }
    
    /// <summary>
    /// Force reconnection even if already connected
    /// </summary>
    public bool ForceReconnect { get; set; }
    
    /// <summary>
    /// Connection timeout in milliseconds
    /// </summary>
    public int? TimeoutMs { get; set; }
    
    /// <summary>
    /// Correlation ID for tracing
    /// </summary>
    public string? CorrelationId { get; set; }
    
    /// <summary>
    /// Creates a connect command
    /// </summary>
    /// <param name="settings">Connection settings</param>
    /// <param name="forceReconnect">Force reconnection</param>
    /// <param name="timeoutMs">Connection timeout</param>
    /// <param name="correlationId">Correlation ID</param>
    /// <returns>Connect command</returns>
    public static ConnectCommand Create(ConnectionSettings? settings = null, bool forceReconnect = false, 
        int? timeoutMs = null, string? correlationId = null)
    {
        return new ConnectCommand
        {
            Settings = settings,
            ForceReconnect = forceReconnect,
            TimeoutMs = timeoutMs,
            CorrelationId = correlationId
        };
    }
}

/// <summary>
/// Command to disconnect from RabbitMQ server
/// </summary>
public class DisconnectCommand : IRequest<ConnectionResult>
{
    /// <summary>
    /// Whether to perform graceful disconnection
    /// </summary>
    public bool Graceful { get; set; } = true;
    
    /// <summary>
    /// Disconnect timeout in milliseconds
    /// </summary>
    public int? TimeoutMs { get; set; }
    
    /// <summary>
    /// Reason for disconnection
    /// </summary>
    public string? Reason { get; set; }
    
    /// <summary>
    /// Correlation ID for tracing
    /// </summary>
    public string? CorrelationId { get; set; }
    
    /// <summary>
    /// Creates a disconnect command
    /// </summary>
    /// <param name="graceful">Whether to disconnect gracefully</param>
    /// <param name="reason">Disconnection reason</param>
    /// <param name="timeoutMs">Disconnect timeout</param>
    /// <param name="correlationId">Correlation ID</param>
    /// <returns>Disconnect command</returns>
    public static DisconnectCommand Create(bool graceful = true, string? reason = null, 
        int? timeoutMs = null, string? correlationId = null)
    {
        return new DisconnectCommand
        {
            Graceful = graceful,
            Reason = reason,
            TimeoutMs = timeoutMs,
            CorrelationId = correlationId
        };
    }
}

/// <summary>
/// Command to recover/reconnect to RabbitMQ server
/// </summary>
public class RecoverConnectionCommand : IRequest<ConnectionResult>
{
    /// <summary>
    /// Recovery strategy to use
    /// </summary>
    public ConnectionRecoveryStrategy Strategy { get; set; } = ConnectionRecoveryStrategy.Automatic;
    
    /// <summary>
    /// Maximum recovery attempts
    /// </summary>
    public int? MaxAttempts { get; set; }
    
    /// <summary>
    /// Delay between recovery attempts in milliseconds
    /// </summary>
    public int? DelayMs { get; set; }
    
    /// <summary>
    /// Recovery timeout in milliseconds
    /// </summary>
    public int? TimeoutMs { get; set; }
    
    /// <summary>
    /// Correlation ID for tracing
    /// </summary>
    public string? CorrelationId { get; set; }
    
    /// <summary>
    /// Creates a recover connection command
    /// </summary>
    /// <param name="strategy">Recovery strategy</param>
    /// <param name="maxAttempts">Maximum attempts</param>
    /// <param name="delayMs">Delay between attempts</param>
    /// <param name="timeoutMs">Recovery timeout</param>
    /// <param name="correlationId">Correlation ID</param>
    /// <returns>Recover connection command</returns>
    public static RecoverConnectionCommand Create(ConnectionRecoveryStrategy strategy = ConnectionRecoveryStrategy.Automatic, 
        int? maxAttempts = null, int? delayMs = null, int? timeoutMs = null, string? correlationId = null)
    {
        return new RecoverConnectionCommand
        {
            Strategy = strategy,
            MaxAttempts = maxAttempts,
            DelayMs = delayMs,
            TimeoutMs = timeoutMs,
            CorrelationId = correlationId
        };
    }
}

/// <summary>
/// Command to test connection health
/// </summary>
public class TestConnectionCommand : IRequest<ConnectionTestResult>
{
    /// <summary>
    /// Type of test to perform
    /// </summary>
    public ConnectionTestType TestType { get; set; } = ConnectionTestType.Basic;
    
    /// <summary>
    /// Test timeout in milliseconds
    /// </summary>
    public int? TimeoutMs { get; set; }
    
    /// <summary>
    /// Include detailed test results
    /// </summary>
    public bool IncludeDetails { get; set; }
    
    /// <summary>
    /// Correlation ID for tracing
    /// </summary>
    public string? CorrelationId { get; set; }
    
    /// <summary>
    /// Creates a test connection command
    /// </summary>
    /// <param name="testType">Type of test</param>
    /// <param name="timeoutMs">Test timeout</param>
    /// <param name="includeDetails">Include detailed results</param>
    /// <param name="correlationId">Correlation ID</param>
    /// <returns>Test connection command</returns>
    public static TestConnectionCommand Create(ConnectionTestType testType = ConnectionTestType.Basic, 
        int? timeoutMs = null, bool includeDetails = false, string? correlationId = null)
    {
        return new TestConnectionCommand
        {
            TestType = testType,
            TimeoutMs = timeoutMs,
            IncludeDetails = includeDetails,
            CorrelationId = correlationId
        };
    }
}

/// <summary>
/// Command to create a new channel
/// </summary>
public class CreateChannelCommand : IRequest<ChannelResult>
{
    /// <summary>
    /// Whether to create a pooled channel
    /// </summary>
    public bool UsePooling { get; set; } = true;
    
    /// <summary>
    /// Channel configuration options
    /// </summary>
    public ChannelOptions? Options { get; set; }
    
    /// <summary>
    /// Channel purpose/description
    /// </summary>
    public string? Purpose { get; set; }
    
    /// <summary>
    /// Correlation ID for tracing
    /// </summary>
    public string? CorrelationId { get; set; }
    
    /// <summary>
    /// Creates a create channel command
    /// </summary>
    /// <param name="usePooling">Whether to use pooling</param>
    /// <param name="options">Channel options</param>
    /// <param name="purpose">Channel purpose</param>
    /// <param name="correlationId">Correlation ID</param>
    /// <returns>Create channel command</returns>
    public static CreateChannelCommand Create(bool usePooling = true, ChannelOptions? options = null, 
        string? purpose = null, string? correlationId = null)
    {
        return new CreateChannelCommand
        {
            UsePooling = usePooling,
            Options = options,
            Purpose = purpose,
            CorrelationId = correlationId
        };
    }
}

/// <summary>
/// Command to return a channel to the pool
/// </summary>
public class ReturnChannelCommand : IRequest<ChannelOperationResult>
{
    /// <summary>
    /// Channel number to return
    /// </summary>
    public int ChannelNumber { get; set; }
    
    /// <summary>
    /// Whether to force closure
    /// </summary>
    public bool ForceClose { get; set; }
    
    /// <summary>
    /// Return reason
    /// </summary>
    public string? Reason { get; set; }
    
    /// <summary>
    /// Correlation ID for tracing
    /// </summary>
    public string? CorrelationId { get; set; }
    
    /// <summary>
    /// Creates a return channel command
    /// </summary>
    /// <param name="channelNumber">Channel number</param>
    /// <param name="forceClose">Force closure</param>
    /// <param name="reason">Return reason</param>
    /// <param name="correlationId">Correlation ID</param>
    /// <returns>Return channel command</returns>
    public static ReturnChannelCommand Create(int channelNumber, bool forceClose = false, 
        string? reason = null, string? correlationId = null)
    {
        return new ReturnChannelCommand
        {
            ChannelNumber = channelNumber,
            ForceClose = forceClose,
            Reason = reason,
            CorrelationId = correlationId
        };
    }
}

/// <summary>
/// Command to close a specific channel
/// </summary>
public class CloseChannelCommand : IRequest<ChannelOperationResult>
{
    /// <summary>
    /// Channel number to close
    /// </summary>
    public int ChannelNumber { get; set; }
    
    /// <summary>
    /// AMQP reply code
    /// </summary>
    public ushort? ReplyCode { get; set; }
    
    /// <summary>
    /// AMQP reply text
    /// </summary>
    public string? ReplyText { get; set; }
    
    /// <summary>
    /// Class ID for AMQP protocol
    /// </summary>
    public ushort? ClassId { get; set; }
    
    /// <summary>
    /// Method ID for AMQP protocol
    /// </summary>
    public ushort? MethodId { get; set; }
    
    /// <summary>
    /// Whether to wait for close confirmation
    /// </summary>
    public bool WaitForConfirmation { get; set; } = true;
    
    /// <summary>
    /// Correlation ID for tracing
    /// </summary>
    public string? CorrelationId { get; set; }
    
    /// <summary>
    /// Creates a close channel command
    /// </summary>
    /// <param name="channelNumber">Channel number</param>
    /// <param name="replyCode">AMQP reply code</param>
    /// <param name="replyText">AMQP reply text</param>
    /// <param name="waitForConfirmation">Wait for confirmation</param>
    /// <param name="correlationId">Correlation ID</param>
    /// <returns>Close channel command</returns>
    public static CloseChannelCommand Create(int channelNumber, ushort? replyCode = null, 
        string? replyText = null, bool waitForConfirmation = true, string? correlationId = null)
    {
        return new CloseChannelCommand
        {
            ChannelNumber = channelNumber,
            ReplyCode = replyCode,
            ReplyText = replyText,
            WaitForConfirmation = waitForConfirmation,
            CorrelationId = correlationId
        };
    }
}

/// <summary>
/// Command to update connection settings
/// </summary>
public class UpdateConnectionSettingsCommand : IRequest<ConnectionUpdateResult>
{
    /// <summary>
    /// New connection settings
    /// </summary>
    public ConnectionSettings Settings { get; set; } = new();
    
    /// <summary>
    /// Whether to apply changes immediately (requires reconnection)
    /// </summary>
    public bool ApplyImmediately { get; set; }
    
    /// <summary>
    /// Settings to update (if null, all settings are updated)
    /// </summary>
    public ConnectionSettingsUpdate? UpdateMask { get; set; }
    
    /// <summary>
    /// Correlation ID for tracing
    /// </summary>
    public string? CorrelationId { get; set; }
    
    /// <summary>
    /// Creates an update connection settings command
    /// </summary>
    /// <param name="settings">New settings</param>
    /// <param name="applyImmediately">Apply immediately</param>
    /// <param name="updateMask">Settings update mask</param>
    /// <param name="correlationId">Correlation ID</param>
    /// <returns>Update connection settings command</returns>
    public static UpdateConnectionSettingsCommand Create(ConnectionSettings settings, bool applyImmediately = false, 
        ConnectionSettingsUpdate? updateMask = null, string? correlationId = null)
    {
        return new UpdateConnectionSettingsCommand
        {
            Settings = settings,
            ApplyImmediately = applyImmediately,
            UpdateMask = updateMask,
            CorrelationId = correlationId
        };
    }
}

/// <summary>
/// Command to flush all pending operations
/// </summary>
public class FlushConnectionCommand : IRequest<ConnectionOperationResult>
{
    /// <summary>
    /// Flush timeout in milliseconds
    /// </summary>
    public int? TimeoutMs { get; set; }
    
    /// <summary>
    /// Whether to flush all channels
    /// </summary>
    public bool FlushAllChannels { get; set; } = true;
    
    /// <summary>
    /// Correlation ID for tracing
    /// </summary>
    public string? CorrelationId { get; set; }
    
    /// <summary>
    /// Creates a flush connection command
    /// </summary>
    /// <param name="timeoutMs">Flush timeout</param>
    /// <param name="flushAllChannels">Flush all channels</param>
    /// <param name="correlationId">Correlation ID</param>
    /// <returns>Flush connection command</returns>
    public static FlushConnectionCommand Create(int? timeoutMs = null, bool flushAllChannels = true, 
        string? correlationId = null)
    {
        return new FlushConnectionCommand
        {
            TimeoutMs = timeoutMs,
            FlushAllChannels = flushAllChannels,
            CorrelationId = correlationId
        };
    }
}

/// <summary>
/// Command to reset connection statistics
/// </summary>
public class ResetConnectionStatisticsCommand : IRequest<ConnectionOperationResult>
{
    /// <summary>
    /// Statistics categories to reset
    /// </summary>
    public ConnectionStatisticsCategory Categories { get; set; } = ConnectionStatisticsCategory.All;
    
    /// <summary>
    /// Confirmation required
    /// </summary>
    public bool RequireConfirmation { get; set; } = true;
    
    /// <summary>
    /// Correlation ID for tracing
    /// </summary>
    public string? CorrelationId { get; set; }
    
    /// <summary>
    /// Creates a reset connection statistics command
    /// </summary>
    /// <param name="categories">Categories to reset</param>
    /// <param name="requireConfirmation">Require confirmation</param>
    /// <param name="correlationId">Correlation ID</param>
    /// <returns>Reset connection statistics command</returns>
    public static ResetConnectionStatisticsCommand Create(ConnectionStatisticsCategory categories = ConnectionStatisticsCategory.All, 
        bool requireConfirmation = true, string? correlationId = null)
    {
        return new ResetConnectionStatisticsCommand
        {
            Categories = categories,
            RequireConfirmation = requireConfirmation,
            CorrelationId = correlationId
        };
    }
}

// Supporting enums and classes

/// <summary>
/// Connection recovery strategies
/// </summary>
public enum ConnectionRecoveryStrategy
{
    /// <summary>
    /// Automatic recovery using default settings
    /// </summary>
    Automatic,
    
    /// <summary>
    /// Manual recovery with specific parameters
    /// </summary>
    Manual,
    
    /// <summary>
    /// Exponential backoff recovery
    /// </summary>
    ExponentialBackoff,
    
    /// <summary>
    /// Linear retry recovery
    /// </summary>
    LinearRetry,
    
    /// <summary>
    /// Circuit breaker recovery
    /// </summary>
    CircuitBreaker
}

/// <summary>
/// Connection test types
/// </summary>
public enum ConnectionTestType
{
    /// <summary>
    /// Basic connectivity test
    /// </summary>
    Basic,
    
    /// <summary>
    /// Comprehensive health check
    /// </summary>
    Comprehensive,
    
    /// <summary>
    /// Performance test
    /// </summary>
    Performance,
    
    /// <summary>
    /// Load test
    /// </summary>
    Load,
    
    /// <summary>
    /// Security test
    /// </summary>
    Security
}

/// <summary>
/// Channel options for channel creation
/// </summary>
public class ChannelOptions
{
    /// <summary>
    /// Prefetch count for the channel
    /// </summary>
    public ushort? PrefetchCount { get; set; }
    
    /// <summary>
    /// Whether to enable publisher confirms
    /// </summary>
    public bool EnablePublisherConfirms { get; set; }
    
    /// <summary>
    /// Whether to enable mandatory publishing
    /// </summary>
    public bool EnableMandatory { get; set; }
    
    /// <summary>
    /// Channel timeout in milliseconds
    /// </summary>
    public int? TimeoutMs { get; set; }
    
    /// <summary>
    /// Channel priority
    /// </summary>
    public byte? Priority { get; set; }
    
    /// <summary>
    /// Creates default channel options
    /// </summary>
    /// <returns>Default channel options</returns>
    public static ChannelOptions CreateDefault()
    {
        return new ChannelOptions
        {
            PrefetchCount = 250,
            EnablePublisherConfirms = true,
            EnableMandatory = false,
            TimeoutMs = 30000,
            Priority = 0
        };
    }
    
    /// <summary>
    /// Creates high-performance channel options
    /// </summary>
    /// <returns>High-performance channel options</returns>
    public static ChannelOptions CreateHighPerformance()
    {
        return new ChannelOptions
        {
            PrefetchCount = 1000,
            EnablePublisherConfirms = false,
            EnableMandatory = false,
            TimeoutMs = 10000,
            Priority = 10
        };
    }
    
    /// <summary>
    /// Creates reliable channel options
    /// </summary>
    /// <returns>Reliable channel options</returns>
    public static ChannelOptions CreateReliable()
    {
        return new ChannelOptions
        {
            PrefetchCount = 50,
            EnablePublisherConfirms = true,
            EnableMandatory = true,
            TimeoutMs = 60000,
            Priority = 5
        };
    }
}

/// <summary>
/// Connection settings update mask
/// </summary>
public class ConnectionSettingsUpdate
{
    /// <summary>
    /// Update host name
    /// </summary>
    public bool UpdateHostName { get; set; }
    
    /// <summary>
    /// Update port
    /// </summary>
    public bool UpdatePort { get; set; }
    
    /// <summary>
    /// Update credentials
    /// </summary>
    public bool UpdateCredentials { get; set; }
    
    /// <summary>
    /// Update virtual host
    /// </summary>
    public bool UpdateVirtualHost { get; set; }
    
    /// <summary>
    /// Update heartbeat settings
    /// </summary>
    public bool UpdateHeartbeat { get; set; }
    
    /// <summary>
    /// Update SSL settings
    /// </summary>
    public bool UpdateSsl { get; set; }
    
    /// <summary>
    /// Update recovery settings
    /// </summary>
    public bool UpdateRecovery { get; set; }
    
    /// <summary>
    /// Update pool settings
    /// </summary>
    public bool UpdatePool { get; set; }
    
    /// <summary>
    /// Creates an update mask for all settings
    /// </summary>
    /// <returns>Update mask for all settings</returns>
    public static ConnectionSettingsUpdate All()
    {
        return new ConnectionSettingsUpdate
        {
            UpdateHostName = true,
            UpdatePort = true,
            UpdateCredentials = true,
            UpdateVirtualHost = true,
            UpdateHeartbeat = true,
            UpdateSsl = true,
            UpdateRecovery = true,
            UpdatePool = true
        };
    }
    
    /// <summary>
    /// Creates an update mask for connection details only
    /// </summary>
    /// <returns>Update mask for connection details</returns>
    public static ConnectionSettingsUpdate ConnectionOnly()
    {
        return new ConnectionSettingsUpdate
        {
            UpdateHostName = true,
            UpdatePort = true,
            UpdateCredentials = true,
            UpdateVirtualHost = true
        };
    }
}

/// <summary>
/// Connection statistics categories
/// </summary>
[Flags]
public enum ConnectionStatisticsCategory
{
    /// <summary>
    /// Connection counts
    /// </summary>
    Connections = 1,
    
    /// <summary>
    /// Recovery statistics
    /// </summary>
    Recovery = 2,
    
    /// <summary>
    /// Channel statistics
    /// </summary>
    Channels = 4,
    
    /// <summary>
    /// Performance metrics
    /// </summary>
    Performance = 8,
    
    /// <summary>
    /// Error statistics
    /// </summary>
    Errors = 16,
    
    /// <summary>
    /// All statistics categories
    /// </summary>
    All = Connections | Recovery | Channels | Performance | Errors
}

// Result models

/// <summary>
/// Result of connection operation
/// </summary>
public class ConnectionResult
{
    /// <summary>
    /// Whether the operation was successful
    /// </summary>
    public bool Success { get; set; }
    
    /// <summary>
    /// Current connection state
    /// </summary>
    public ConnectionState State { get; set; }
    
    /// <summary>
    /// Connection duration for the operation
    /// </summary>
    public TimeSpan Duration { get; set; }
    
    /// <summary>
    /// Error message if not successful
    /// </summary>
    public string? ErrorMessage { get; set; }
    
    /// <summary>
    /// Exception if one occurred
    /// </summary>
    public Exception? Exception { get; set; }
    
    /// <summary>
    /// Connection statistics snapshot
    /// </summary>
    public ConnectionStatistics? Statistics { get; set; }
    
    /// <summary>
    /// Operation timestamp
    /// </summary>
    public DateTimeOffset Timestamp { get; set; } = DateTimeOffset.UtcNow;
    
    /// <summary>
    /// Additional metadata
    /// </summary>
    public Dictionary<string, object> Metadata { get; set; } = new();
    
    /// <summary>
    /// Creates a success result
    /// </summary>
    /// <param name="state">Connection state</param>
    /// <param name="duration">Operation duration</param>
    /// <param name="statistics">Connection statistics</param>
    /// <returns>Success result</returns>
    public static ConnectionResult CreateSuccess(ConnectionState state, TimeSpan duration, ConnectionStatistics? statistics = null)
    {
        return new ConnectionResult
        {
            Success = true,
            State = state,
            Duration = duration,
            Statistics = statistics
        };
    }
    
    /// <summary>
    /// Creates a failure result
    /// </summary>
    /// <param name="state">Connection state</param>
    /// <param name="errorMessage">Error message</param>
    /// <param name="exception">Exception</param>
    /// <returns>Failure result</returns>
    public static ConnectionResult CreateFailure(ConnectionState state, string errorMessage, Exception? exception = null)
    {
        return new ConnectionResult
        {
            Success = false,
            State = state,
            ErrorMessage = errorMessage,
            Exception = exception
        };
    }
}

/// <summary>
/// Result of connection test operation
/// </summary>
public class ConnectionTestResult
{
    /// <summary>
    /// Whether the test passed
    /// </summary>
    public bool Passed { get; set; }
    
    /// <summary>
    /// Test type that was performed
    /// </summary>
    public ConnectionTestType TestType { get; set; }
    
    /// <summary>
    /// Test duration
    /// </summary>
    public TimeSpan Duration { get; set; }
    
    /// <summary>
    /// Test score (0-100)
    /// </summary>
    public double Score { get; set; }
    
    /// <summary>
    /// Test details and metrics
    /// </summary>
    public Dictionary<string, object> Details { get; set; } = new();
    
    /// <summary>
    /// Test warnings
    /// </summary>
    public List<string> Warnings { get; set; } = new();
    
    /// <summary>
    /// Test errors
    /// </summary>
    public List<string> Errors { get; set; } = new();
    
    /// <summary>
    /// Test timestamp
    /// </summary>
    public DateTimeOffset Timestamp { get; set; } = DateTimeOffset.UtcNow;
    
    /// <summary>
    /// Creates a successful test result
    /// </summary>
    /// <param name="testType">Test type</param>
    /// <param name="duration">Test duration</param>
    /// <param name="score">Test score</param>
    /// <returns>Successful test result</returns>
    public static ConnectionTestResult CreateSuccess(ConnectionTestType testType, TimeSpan duration, double score = 100.0)
    {
        return new ConnectionTestResult
        {
            Passed = true,
            TestType = testType,
            Duration = duration,
            Score = score
        };
    }
    
    /// <summary>
    /// Creates a failed test result
    /// </summary>
    /// <param name="testType">Test type</param>
    /// <param name="duration">Test duration</param>
    /// <param name="errors">Test errors</param>
    /// <param name="score">Test score</param>
    /// <returns>Failed test result</returns>
    public static ConnectionTestResult CreateFailure(ConnectionTestType testType, TimeSpan duration, 
        List<string> errors, double score = 0.0)
    {
        return new ConnectionTestResult
        {
            Passed = false,
            TestType = testType,
            Duration = duration,
            Score = score,
            Errors = errors
        };
    }
}

/// <summary>
/// Result of channel operation
/// </summary>
public class ChannelResult
{
    /// <summary>
    /// Whether the operation was successful
    /// </summary>
    public bool Success { get; set; }
    
    /// <summary>
    /// Channel number (if created)
    /// </summary>
    public int? ChannelNumber { get; set; }
    
    /// <summary>
    /// Whether the channel is pooled
    /// </summary>
    public bool IsPooled { get; set; }
    
    /// <summary>
    /// Channel creation timestamp
    /// </summary>
    public DateTimeOffset CreatedAt { get; set; }
    
    /// <summary>
    /// Error message if not successful
    /// </summary>
    public string? ErrorMessage { get; set; }
    
    /// <summary>
    /// Exception if one occurred
    /// </summary>
    public Exception? Exception { get; set; }
    
    /// <summary>
    /// Channel metadata
    /// </summary>
    public Dictionary<string, object> Metadata { get; set; } = new();
    
    /// <summary>
    /// Creates a success result
    /// </summary>
    /// <param name="channelNumber">Channel number</param>
    /// <param name="isPooled">Whether pooled</param>
    /// <returns>Success result</returns>
    public static ChannelResult CreateSuccess(int channelNumber, bool isPooled = true)
    {
        return new ChannelResult
        {
            Success = true,
            ChannelNumber = channelNumber,
            IsPooled = isPooled,
            CreatedAt = DateTimeOffset.UtcNow
        };
    }
    
    /// <summary>
    /// Creates a failure result
    /// </summary>
    /// <param name="errorMessage">Error message</param>
    /// <param name="exception">Exception</param>
    /// <returns>Failure result</returns>
    public static ChannelResult CreateFailure(string errorMessage, Exception? exception = null)
    {
        return new ChannelResult
        {
            Success = false,
            ErrorMessage = errorMessage,
            Exception = exception
        };
    }
}

/// <summary>
/// Result of channel operation
/// </summary>
public class ChannelOperationResult
{
    /// <summary>
    /// Whether the operation was successful
    /// </summary>
    public bool Success { get; set; }
    
    /// <summary>
    /// Channel number
    /// </summary>
    public int ChannelNumber { get; set; }
    
    /// <summary>
    /// Operation performed
    /// </summary>
    public string Operation { get; set; } = string.Empty;
    
    /// <summary>
    /// Error message if not successful
    /// </summary>
    public string? ErrorMessage { get; set; }
    
    /// <summary>
    /// Exception if one occurred
    /// </summary>
    public Exception? Exception { get; set; }
    
    /// <summary>
    /// Operation timestamp
    /// </summary>
    public DateTimeOffset Timestamp { get; set; } = DateTimeOffset.UtcNow;
    
    /// <summary>
    /// Creates a success result
    /// </summary>
    /// <param name="channelNumber">Channel number</param>
    /// <param name="operation">Operation performed</param>
    /// <returns>Success result</returns>
    public static ChannelOperationResult CreateSuccess(int channelNumber, string operation)
    {
        return new ChannelOperationResult
        {
            Success = true,
            ChannelNumber = channelNumber,
            Operation = operation
        };
    }
    
    /// <summary>
    /// Creates a failure result
    /// </summary>
    /// <param name="channelNumber">Channel number</param>
    /// <param name="operation">Operation performed</param>
    /// <param name="errorMessage">Error message</param>
    /// <param name="exception">Exception</param>
    /// <returns>Failure result</returns>
    public static ChannelOperationResult CreateFailure(int channelNumber, string operation, 
        string errorMessage, Exception? exception = null)
    {
        return new ChannelOperationResult
        {
            Success = false,
            ChannelNumber = channelNumber,
            Operation = operation,
            ErrorMessage = errorMessage,
            Exception = exception
        };
    }
}

/// <summary>
/// Result of connection update operation
/// </summary>
public class ConnectionUpdateResult
{
    /// <summary>
    /// Whether the update was successful
    /// </summary>
    public bool Success { get; set; }
    
    /// <summary>
    /// Settings that were updated
    /// </summary>
    public ConnectionSettingsUpdate? UpdatedSettings { get; set; }
    
    /// <summary>
    /// Whether reconnection was required
    /// </summary>
    public bool ReconnectionRequired { get; set; }
    
    /// <summary>
    /// Whether reconnection was performed
    /// </summary>
    public bool ReconnectionPerformed { get; set; }
    
    /// <summary>
    /// Previous connection state
    /// </summary>
    public ConnectionState? PreviousState { get; set; }
    
    /// <summary>
    /// Current connection state
    /// </summary>
    public ConnectionState? CurrentState { get; set; }
    
    /// <summary>
    /// Error message if not successful
    /// </summary>
    public string? ErrorMessage { get; set; }
    
    /// <summary>
    /// Exception if one occurred
    /// </summary>
    public Exception? Exception { get; set; }
    
    /// <summary>
    /// Update timestamp
    /// </summary>
    public DateTimeOffset Timestamp { get; set; } = DateTimeOffset.UtcNow;
    
    /// <summary>
    /// Creates a success result
    /// </summary>
    /// <param name="updatedSettings">Updated settings</param>
    /// <param name="reconnectionRequired">Reconnection required</param>
    /// <param name="reconnectionPerformed">Reconnection performed</param>
    /// <param name="previousState">Previous state</param>
    /// <param name="currentState">Current state</param>
    /// <returns>Success result</returns>
    public static ConnectionUpdateResult CreateSuccess(ConnectionSettingsUpdate updatedSettings, 
        bool reconnectionRequired, bool reconnectionPerformed, 
        ConnectionState? previousState, ConnectionState? currentState)
    {
        return new ConnectionUpdateResult
        {
            Success = true,
            UpdatedSettings = updatedSettings,
            ReconnectionRequired = reconnectionRequired,
            ReconnectionPerformed = reconnectionPerformed,
            PreviousState = previousState,
            CurrentState = currentState
        };
    }
    
    /// <summary>
    /// Creates a failure result
    /// </summary>
    /// <param name="errorMessage">Error message</param>
    /// <param name="exception">Exception</param>
    /// <returns>Failure result</returns>
    public static ConnectionUpdateResult CreateFailure(string errorMessage, Exception? exception = null)
    {
        return new ConnectionUpdateResult
        {
            Success = false,
            ErrorMessage = errorMessage,
            Exception = exception
        };
    }
}

/// <summary>
/// Result of general connection operation
/// </summary>
public class ConnectionOperationResult
{
    /// <summary>
    /// Whether the operation was successful
    /// </summary>
    public bool Success { get; set; }
    
    /// <summary>
    /// Operation that was performed
    /// </summary>
    public string Operation { get; set; } = string.Empty;
    
    /// <summary>
    /// Operation result data
    /// </summary>
    public Dictionary<string, object> ResultData { get; set; } = new();
    
    /// <summary>
    /// Error message if not successful
    /// </summary>
    public string? ErrorMessage { get; set; }
    
    /// <summary>
    /// Exception if one occurred
    /// </summary>
    public Exception? Exception { get; set; }
    
    /// <summary>
    /// Operation timestamp
    /// </summary>
    public DateTimeOffset Timestamp { get; set; } = DateTimeOffset.UtcNow;
    
    /// <summary>
    /// Creates a success result
    /// </summary>
    /// <param name="operation">Operation performed</param>
    /// <param name="resultData">Result data</param>
    /// <returns>Success result</returns>
    public static ConnectionOperationResult CreateSuccess(string operation, Dictionary<string, object>? resultData = null)
    {
        return new ConnectionOperationResult
        {
            Success = true,
            Operation = operation,
            ResultData = resultData ?? new Dictionary<string, object>()
        };
    }
    
    /// <summary>
    /// Creates a failure result
    /// </summary>
    /// <param name="operation">Operation performed</param>
    /// <param name="errorMessage">Error message</param>
    /// <param name="exception">Exception</param>
    /// <returns>Failure result</returns>
    public static ConnectionOperationResult CreateFailure(string operation, string errorMessage, Exception? exception = null)
    {
        return new ConnectionOperationResult
        {
            Success = false,
            Operation = operation,
            ErrorMessage = errorMessage,
            Exception = exception
        };
    }
} 