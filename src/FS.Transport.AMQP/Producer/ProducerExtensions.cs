using Microsoft.Extensions.DependencyInjection;

namespace FS.Transport.AMQP.Producer;

/// <summary>
/// Extension methods for producer functionality
/// </summary>
public static class ProducerExtensions
{
    /// <summary>
    /// Adds message producer services to the dependency injection container
    /// </summary>
    /// <param name="services">Service collection</param>
    /// <param name="configure">Configuration action</param>
    /// <returns>Service collection</returns>
    public static IServiceCollection AddMessageProducer(this IServiceCollection services, Action<ProducerSettings>? configure = null)
    {
        // Configure producer settings
        if (configure != null)
        {
            services.Configure<ProducerSettings>(configure);
        }
        
        // Register producer as singleton
        services.AddSingleton<IMessageProducer, MessageProducer>();
        
        // Register fluent API
        services.AddTransient(typeof(FluentProducerApi<>));
        
        return services;
    }
    
    /// <summary>
    /// Creates a fluent API for message publishing
    /// </summary>
    /// <typeparam name="T">Message type</typeparam>
    /// <param name="producer">Message producer</param>
    /// <param name="message">Message to publish</param>
    /// <returns>Fluent API instance</returns>
    public static FluentProducerApi<T> Fluent<T>(this IMessageProducer producer, T message) where T : class
    {
        return new FluentProducerApi<T>(producer, message);
    }
    
    /// <summary>
    /// Creates publish options for high priority messages
    /// </summary>
    /// <returns>High priority publish options</returns>
    public static PublishOptions CreateHighPriorityOptions()
    {
        return new PublishOptions
        {
            Priority = 255,
            Persistent = true,
            WaitForConfirmation = true,
            Mandatory = true
        };
    }
    
    /// <summary>
    /// Creates publish options for reliable messaging
    /// </summary>
    /// <returns>Reliable publish options</returns>
    public static PublishOptions CreateReliableOptions()
    {
        return new PublishOptions
        {
            WaitForConfirmation = true,
            Persistent = true,
            Mandatory = true,
            MaxRetries = 3
        };
    }
    
    /// <summary>
    /// Gets producer health information
    /// </summary>
    /// <param name="producer">Message producer</param>
    /// <returns>Health information</returns>
    public static ProducerHealth GetHealth(this IMessageProducer producer)
    {
        var stats = producer.Statistics;
        
        return new ProducerHealth
        {
            Status = stats.Status,
            IsHealthy = stats.Status == ProducerStatus.Running,
            Uptime = DateTimeOffset.UtcNow - stats.StartTime,
            TotalMessages = stats.TotalMessages,
            SuccessfulMessages = stats.SuccessfulMessages,
            FailedMessages = stats.FailedMessages,
            SuccessRate = stats.SuccessRate,
            AverageLatency = stats.AverageLatency,
            MessagesPerSecond = stats.MessagesPerSecond,
            LastError = stats.LastError?.Message,
            LastErrorTime = stats.LastErrorTime
        };
    }
    
    /// <summary>
    /// Gets producer metrics for monitoring
    /// </summary>
    /// <param name="producer">Message producer</param>
    /// <returns>Producer metrics</returns>
    public static ProducerMetrics GetMetrics(this IMessageProducer producer)
    {
        var stats = producer.Statistics;
        
        return new ProducerMetrics
        {
            ProducerName = stats.Name,
            Status = stats.Status.ToString(),
            TotalMessages = stats.TotalMessages,
            SuccessfulMessages = stats.SuccessfulMessages,
            FailedMessages = stats.FailedMessages,
            PendingConfirmations = stats.PendingConfirmations,
            ScheduledMessages = stats.ScheduledMessages,
            ActiveTransactions = stats.ActiveTransactions,
            AverageLatency = stats.AverageLatency,
            MinLatency = stats.MinLatency,
            MaxLatency = stats.MaxLatency,
            MessagesPerSecond = stats.MessagesPerSecond,
            SuccessRate = stats.SuccessRate,
            FailureRate = stats.FailureRate,
            LastErrorTime = stats.LastErrorTime,
            LastErrorMessage = stats.LastError?.Message
        };
    }
    
    /// <summary>
    /// Resets producer statistics
    /// </summary>
    /// <param name="producer">Message producer</param>
    public static void ResetStatistics(this IMessageProducer producer)
    {
        var stats = producer.Statistics;
        stats.TotalMessages = 0;
        stats.SuccessfulMessages = 0;
        stats.FailedMessages = 0;
        stats.TotalBatches = 0;
        stats.PendingConfirmations = 0;
        stats.ScheduledMessages = 0;
        stats.ActiveTransactions = 0;
        stats.AverageLatency = 0;
        stats.MinLatency = 0;
        stats.MaxLatency = 0;
        stats.MessagesPerSecond = 0;
        stats.StartTime = DateTimeOffset.UtcNow;
        stats.LastUpdateTime = DateTimeOffset.UtcNow;
        stats.LastError = null;
        stats.LastErrorTime = null;
    }
    
    /// <summary>
    /// Validates producer configuration
    /// </summary>
    /// <param name="settings">Producer settings</param>
    /// <returns>Validation result</returns>
    public static ValidationResult ValidateSettings(this ProducerSettings settings)
    {
        var errors = new List<string>();
        
        if (string.IsNullOrEmpty(settings.Name))
            errors.Add("Producer name is required");
            
        if (settings.ConfirmationTimeout <= TimeSpan.Zero)
            errors.Add("Confirmation timeout must be positive");
            
        if (settings.MaxRetries < 0)
            errors.Add("Max retries cannot be negative");
            
        return new ValidationResult
        {
            IsValid = errors.Count == 0,
            Errors = errors
        };
    }
}

/// <summary>
/// Producer health information
/// </summary>
public class ProducerHealth
{
    public ProducerStatus Status { get; set; }
    public bool IsHealthy { get; set; }
    public TimeSpan Uptime { get; set; }
    public long TotalMessages { get; set; }
    public long SuccessfulMessages { get; set; }
    public long FailedMessages { get; set; }
    public double SuccessRate { get; set; }
    public double AverageLatency { get; set; }
    public double MessagesPerSecond { get; set; }
    public string? LastError { get; set; }
    public DateTimeOffset? LastErrorTime { get; set; }
}

/// <summary>
/// Producer metrics for monitoring
/// </summary>
public class ProducerMetrics
{
    public string ProducerName { get; set; } = "";
    public string Status { get; set; } = "";
    public long TotalMessages { get; set; }
    public long SuccessfulMessages { get; set; }
    public long FailedMessages { get; set; }
    public long PendingConfirmations { get; set; }
    public long ScheduledMessages { get; set; }
    public long ActiveTransactions { get; set; }
    public double AverageLatency { get; set; }
    public double MinLatency { get; set; }
    public double MaxLatency { get; set; }
    public double MessagesPerSecond { get; set; }
    public double SuccessRate { get; set; }
    public double FailureRate { get; set; }
    public DateTimeOffset? LastErrorTime { get; set; }
    public string? LastErrorMessage { get; set; }
}

/// <summary>
/// Validation result
/// </summary>
public class ValidationResult
{
    public bool IsValid { get; set; }
    public List<string> Errors { get; set; } = new();
}