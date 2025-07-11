using FS.RabbitMQ.Configuration;
using FS.RabbitMQ.ErrorHandling;
using RabbitMQ.Client;

namespace FS.RabbitMQ.Consumer;

/// <summary>
/// Context information for message consumption including queue, exchange, processing options, and metadata
/// </summary>
public class ConsumerContext
{
    /// <summary>
    /// Queue name to consume from
    /// </summary>
    public required string QueueName { get; set; }
    
    /// <summary>
    /// Exchange name (optional, for topic-based consumption)
    /// </summary>
    public string? ExchangeName { get; set; }
    
    /// <summary>
    /// Routing key pattern for message filtering
    /// </summary>
    public string? RoutingKey { get; set; }
    
    /// <summary>
    /// Consumer settings and options
    /// </summary>
    public ConsumerSettings Settings { get; set; } = new();
    
    /// <summary>
    /// Custom consumer arguments
    /// </summary>
    public IDictionary<string, object?> Arguments { get; set; } = new Dictionary<string, object?>();
    
    /// <summary>
    /// Message processing context
    /// </summary>
    public MessageProcessingContext? ProcessingContext { get; set; }
    
    /// <summary>
    /// Consumer tag for identification
    /// </summary>
    public string? ConsumerTag { get; set; }
    
    /// <summary>
    /// Whether this consumer is exclusive to the queue
    /// </summary>
    public bool Exclusive { get; set; } = false;
    
    /// <summary>
    /// Whether to automatically acknowledge messages
    /// </summary>
    public bool AutoAcknowledge { get; set; } = false;
    
    /// <summary>
    /// Custom headers for message filtering
    /// </summary>
    public IDictionary<string, object>? Headers { get; set; }
    
    /// <summary>
    /// Consumer priority for message distribution
    /// </summary>
    public int Priority { get; set; } = 0;
    
    /// <summary>
    /// Maximum number of concurrent message processors
    /// </summary>
    public int MaxConcurrentMessages { get; set; } = Environment.ProcessorCount;
    
    /// <summary>
    /// Timeout for message processing
    /// </summary>
    public TimeSpan ProcessingTimeout { get; set; } = TimeSpan.FromMinutes(5);
    
    /// <summary>
    /// Whether to enable message deduplication
    /// </summary>
    public bool EnableDeduplication { get; set; } = false;
    
    /// <summary>
    /// Deduplication cache size
    /// </summary>
    public int DeduplicationCacheSize { get; set; } = 10000;
    
    /// <summary>
    /// Whether to enable batch processing
    /// </summary>
    public bool EnableBatchProcessing { get; set; } = false;
    
    /// <summary>
    /// Batch size for batch processing
    /// </summary>
    public int BatchSize { get; set; } = 100;
    
    /// <summary>
    /// Batch timeout for batch processing
    /// </summary>
    public TimeSpan BatchTimeout { get; set; } = TimeSpan.FromSeconds(5);
    
    /// <summary>
    /// Error handling strategy
    /// </summary>
    public ErrorHandlingStrategy ErrorHandling { get; set; } = ErrorHandlingStrategy.RetryThenDeadLetter;
    
    /// <summary>
    /// Dead letter exchange for failed messages
    /// </summary>
    public string? DeadLetterExchange { get; set; }
    
    /// <summary>
    /// Dead letter routing key
    /// </summary>
    public string? DeadLetterRoutingKey { get; set; }
    
    /// <summary>
    /// Retry policy settings
    /// </summary>
    public RetryPolicySettings RetryPolicy { get; set; } = new();
    
    /// <summary>
    /// Whether to enable circuit breaker pattern
    /// </summary>
    public bool EnableCircuitBreaker { get; set; } = true;
    
    /// <summary>
    /// Circuit breaker failure threshold
    /// </summary>
    public int CircuitBreakerFailureThreshold { get; set; } = 5;
    
    /// <summary>
    /// Circuit breaker recovery timeout
    /// </summary>
    public TimeSpan CircuitBreakerRecoveryTimeout { get; set; } = TimeSpan.FromMinutes(1);
    
    /// <summary>
    /// Custom serializer settings
    /// </summary>
    public SerializerSettings Serializer { get; set; } = new();
    
    /// <summary>
    /// Consumer start time
    /// </summary>
    public DateTimeOffset StartTime { get; set; } = DateTimeOffset.UtcNow;
    
    /// <summary>
    /// Consumer metadata
    /// </summary>
    public IDictionary<string, object> Metadata { get; set; } = new Dictionary<string, object>();
    
    /// <summary>
    /// Creates a basic consumer context for queue consumption
    /// </summary>
    /// <param name="queueName">Queue name to consume from</param>
    /// <returns>New consumer context</returns>
    public static ConsumerContext CreateForQueue(string queueName)
    {
        return new ConsumerContext
        {
            QueueName = queueName,
            Settings = new ConsumerSettings(),
            StartTime = DateTimeOffset.UtcNow
        };
    }
    
    /// <summary>
    /// Creates a consumer context for topic-based consumption
    /// </summary>
    /// <param name="exchangeName">Exchange name</param>
    /// <param name="routingKey">Routing key pattern</param>
    /// <param name="queueName">Queue name (optional, will be generated if not provided)</param>
    /// <returns>New consumer context</returns>
    public static ConsumerContext CreateForTopic(string exchangeName, string routingKey, string? queueName = null)
    {
        return new ConsumerContext
        {
            QueueName = queueName ?? $"consumer.{exchangeName}.{routingKey.Replace("*", "star").Replace("#", "hash")}",
            ExchangeName = exchangeName,
            RoutingKey = routingKey,
            Settings = new ConsumerSettings(),
            StartTime = DateTimeOffset.UtcNow
        };
    }
    
    /// <summary>
    /// Creates a consumer context for domain events
    /// </summary>
    /// <param name="aggregateType">Aggregate type to consume events for</param>
    /// <param name="eventTypes">Specific event types to consume (optional)</param>
    /// <returns>New consumer context</returns>
    public static ConsumerContext CreateForDomainEvents(string aggregateType, params string[] eventTypes)
    {
        var routingKey = eventTypes.Length > 0 
            ? $"{aggregateType}.{string.Join("|", eventTypes)}"
            : $"{aggregateType}.*";
            
        return new ConsumerContext
        {
            QueueName = $"domain.events.{aggregateType}",
            ExchangeName = "domain.events",
            RoutingKey = routingKey,
            Settings = new ConsumerSettings(),
            StartTime = DateTimeOffset.UtcNow,
            Metadata = new Dictionary<string, object>
            {
                { "ConsumerType", "DomainEvent" },
                { "AggregateType", aggregateType },
                { "EventTypes", eventTypes }
            }
        };
    }
    
    /// <summary>
    /// Creates a consumer context for integration events
    /// </summary>
    /// <param name="serviceName">Service name to consume events from</param>
    /// <param name="eventTypes">Specific event types to consume (optional)</param>
    /// <returns>New consumer context</returns>
    public static ConsumerContext CreateForIntegrationEvents(string serviceName, params string[] eventTypes)
    {
        var routingKey = eventTypes.Length > 0 
            ? $"{serviceName}.{string.Join("|", eventTypes)}"
            : $"{serviceName}.*";
            
        return new ConsumerContext
        {
            QueueName = $"integration.events.{serviceName}",
            ExchangeName = "integration.events",
            RoutingKey = routingKey,
            Settings = new ConsumerSettings(),
            StartTime = DateTimeOffset.UtcNow,
            Metadata = new Dictionary<string, object>
            {
                { "ConsumerType", "IntegrationEvent" },
                { "ServiceName", serviceName },
                { "EventTypes", eventTypes }
            }
        };
    }
    
    /// <summary>
    /// Creates a consumer context for high-throughput scenarios
    /// </summary>
    /// <param name="queueName">Queue name to consume from</param>
    /// <returns>High-throughput consumer context</returns>
    public static ConsumerContext CreateHighThroughput(string queueName)
    {
        return new ConsumerContext
        {
            QueueName = queueName,
            Settings = ConsumerSettings.CreateHighThroughput(),
            MaxConcurrentMessages = Environment.ProcessorCount * 4,
            EnableBatchProcessing = true,
            BatchSize = 100,
            BatchTimeout = TimeSpan.FromSeconds(1),
            StartTime = DateTimeOffset.UtcNow
        };
    }
    
    /// <summary>
    /// Creates a consumer context for low-latency scenarios
    /// </summary>
    /// <param name="queueName">Queue name to consume from</param>
    /// <returns>Low-latency consumer context</returns>
    public static ConsumerContext CreateLowLatency(string queueName)
    {
        return new ConsumerContext
        {
            QueueName = queueName,
            Settings = ConsumerSettings.CreateLowLatency(),
            MaxConcurrentMessages = 1,
            EnableBatchProcessing = false,
            ProcessingTimeout = TimeSpan.FromSeconds(5),
            StartTime = DateTimeOffset.UtcNow
        };
    }
    
    /// <summary>
    /// Creates a consumer context for reliable processing
    /// </summary>
    /// <param name="queueName">Queue name to consume from</param>
    /// <returns>Reliable consumer context</returns>
    public static ConsumerContext CreateReliable(string queueName)
    {
        return new ConsumerContext
        {
            QueueName = queueName,
            Settings = ConsumerSettings.CreateReliable(),
            AutoAcknowledge = false,
            EnableDeduplication = true,
            DeduplicationCacheSize = 10000,
            RetryPolicy = new RetryPolicySettings
            {
                MaxRetries = 3,
                InitialDelay = TimeSpan.FromSeconds(1),
                MaxDelay = TimeSpan.FromMinutes(5),
                BackoffMultiplier = 2.0
            },
            ErrorHandling = ErrorHandlingStrategy.RetryThenDeadLetter,
            EnableCircuitBreaker = true,
            CircuitBreakerFailureThreshold = 5,
            StartTime = DateTimeOffset.UtcNow
        };
    }
    
    /// <summary>
    /// Creates a copy of the consumer context
    /// </summary>
    /// <returns>New instance with copied values</returns>
    public ConsumerContext Clone()
    {
        return new ConsumerContext
        {
            QueueName = QueueName,
            ExchangeName = ExchangeName,
            RoutingKey = RoutingKey,
            Settings = Settings.Clone(),
            Arguments = new Dictionary<string, object?>(Arguments),
            ProcessingContext = ProcessingContext?.Clone(),
            ConsumerTag = ConsumerTag,
            Exclusive = Exclusive,
            AutoAcknowledge = AutoAcknowledge,
            Headers = Headers != null ? new Dictionary<string, object>(Headers) : null,
            Priority = Priority,
            MaxConcurrentMessages = MaxConcurrentMessages,
            ProcessingTimeout = ProcessingTimeout,
            EnableDeduplication = EnableDeduplication,
            DeduplicationCacheSize = DeduplicationCacheSize,
            EnableBatchProcessing = EnableBatchProcessing,
            BatchSize = BatchSize,
            BatchTimeout = BatchTimeout,
            ErrorHandling = ErrorHandling,
            DeadLetterExchange = DeadLetterExchange,
            DeadLetterRoutingKey = DeadLetterRoutingKey,
            RetryPolicy = RetryPolicy.Clone(),
            EnableCircuitBreaker = EnableCircuitBreaker,
            CircuitBreakerFailureThreshold = CircuitBreakerFailureThreshold,
            CircuitBreakerRecoveryTimeout = CircuitBreakerRecoveryTimeout,
            Serializer = Serializer.Clone(),
            StartTime = StartTime,
            Metadata = new Dictionary<string, object>(Metadata)
        };
    }
}

/// <summary>
/// Message processing context information
/// </summary>
public class MessageProcessingContext
{
    /// <summary>
    /// Message delivery tag
    /// </summary>
    public ulong DeliveryTag { get; set; }
    
    /// <summary>
    /// Whether the message was redelivered
    /// </summary>
    public bool Redelivered { get; set; }
    
    /// <summary>
    /// Exchange name where the message was published
    /// </summary>
    public string? Exchange { get; set; }
    
    /// <summary>
    /// Routing key used for message routing
    /// </summary>
    public string? RoutingKey { get; set; }
    
    /// <summary>
    /// Message properties
    /// </summary>
    public IBasicProperties? Properties { get; set; }
    
    /// <summary>
    /// Consumer tag
    /// </summary>
    public string? ConsumerTag { get; set; }
    
    /// <summary>
    /// Message receipt timestamp
    /// </summary>
    public DateTimeOffset ReceivedAt { get; set; } = DateTimeOffset.UtcNow;
    
    /// <summary>
    /// Processing start timestamp
    /// </summary>
    public DateTimeOffset? ProcessingStarted { get; set; }
    
    /// <summary>
    /// Processing end timestamp
    /// </summary>
    public DateTimeOffset? ProcessingCompleted { get; set; }
    
    /// <summary>
    /// Processing duration
    /// </summary>
    public TimeSpan? ProcessingDuration => ProcessingCompleted - ProcessingStarted;
    
    /// <summary>
    /// Custom metadata for the processing context
    /// </summary>
    public IDictionary<string, object> Metadata { get; set; } = new Dictionary<string, object>();
    
    /// <summary>
    /// Creates a copy of the message processing context
    /// </summary>
    /// <returns>New instance with copied values</returns>
    public MessageProcessingContext Clone()
    {
        return new MessageProcessingContext
        {
            DeliveryTag = DeliveryTag,
            Redelivered = Redelivered,
            Exchange = Exchange,
            RoutingKey = RoutingKey,
            Properties = Properties,
            ConsumerTag = ConsumerTag,
            ReceivedAt = ReceivedAt,
            ProcessingStarted = ProcessingStarted,
            ProcessingCompleted = ProcessingCompleted,
            Metadata = new Dictionary<string, object>(Metadata)
        };
    }
}