using FS.Transport.AMQP.Events;
using FS.Transport.AMQP.Producer;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace FS.Transport.AMQP.Consumer;

/// <summary>
/// Extension methods for message consumer configuration and dependency injection
/// </summary>
public static class ConsumerExtensions
{
    /// <summary>
    /// Adds message consumer services to the dependency injection container
    /// </summary>
    /// <param name="services">Service collection</param>
    /// <param name="configure">Configuration action for consumer settings</param>
    /// <returns>Service collection for method chaining</returns>
    public static IServiceCollection AddMessageConsumer(this IServiceCollection services, Action<ConsumerSettings>? configure = null)
    {
        services.TryAddSingleton<IMessageConsumer, MessageConsumer>();
        
        if (configure != null)
        {
            services.Configure<ConsumerSettings>(configure);
        }
        
        return services;
    }
    
    /// <summary>
    /// Adds message consumer services with specific settings
    /// </summary>
    /// <param name="services">Service collection</param>
    /// <param name="settings">Consumer settings</param>
    /// <returns>Service collection for method chaining</returns>
    public static IServiceCollection AddMessageConsumer(this IServiceCollection services, ConsumerSettings settings)
    {
        services.TryAddSingleton<IMessageConsumer, MessageConsumer>();
        services.Configure<ConsumerSettings>(options =>
        {
            options.Name = settings.Name;
            options.ConsumerTag = settings.ConsumerTag;
            options.AutoAcknowledge = settings.AutoAcknowledge;
            options.PrefetchCount = settings.PrefetchCount;
            options.GlobalPrefetch = settings.GlobalPrefetch;
            options.Priority = settings.Priority;
            options.Exclusive = settings.Exclusive;
            options.Arguments = settings.Arguments;
            options.MaxConcurrentMessages = settings.MaxConcurrentMessages;
            options.ProcessingTimeout = settings.ProcessingTimeout;
            options.EnableDeduplication = settings.EnableDeduplication;
            options.DeduplicationCacheSize = settings.DeduplicationCacheSize;
            options.DeduplicationCacheTtl = settings.DeduplicationCacheTtl;
            options.EnableCompression = settings.EnableCompression;
            options.CompressionAlgorithm = settings.CompressionAlgorithm;
            options.EnableEncryption = settings.EnableEncryption;
            options.EncryptionKey = settings.EncryptionKey;
            options.RetryPolicy = settings.RetryPolicy;
            options.ErrorHandling = settings.ErrorHandling;
            options.DeadLetterExchange = settings.DeadLetterExchange;
            options.DeadLetterRoutingKey = settings.DeadLetterRoutingKey;
            options.EnableHeartbeat = settings.EnableHeartbeat;
            options.HeartbeatInterval = settings.HeartbeatInterval;
            options.EnableStatistics = settings.EnableStatistics;
            options.StatisticsInterval = settings.StatisticsInterval;
            options.EnableMetrics = settings.EnableMetrics;
            options.MetricsInterval = settings.MetricsInterval;
            options.EnableBatchProcessing = settings.EnableBatchProcessing;
            options.BatchSize = settings.BatchSize;
            options.BatchTimeout = settings.BatchTimeout;
            options.EnableCircuitBreaker = settings.EnableCircuitBreaker;
            options.CircuitBreakerFailureThreshold = settings.CircuitBreakerFailureThreshold;
            options.CircuitBreakerRecoveryTimeout = settings.CircuitBreakerRecoveryTimeout;
            options.EnablePauseResume = settings.EnablePauseResume;
            options.Serializer = settings.Serializer;
            options.ConnectionRecovery = settings.ConnectionRecovery;
        });
        
        return services;
    }
    
    /// <summary>
    /// Creates a fluent consumer API for the specified message type and queue
    /// </summary>
    /// <typeparam name="T">Message type</typeparam>
    /// <param name="consumer">Message consumer instance</param>
    /// <param name="queueName">Queue name to consume from</param>
    /// <returns>Fluent consumer API</returns>
    public static IFluentConsumerApi<T> For<T>(this IMessageConsumer consumer, string queueName) where T : class
    {
        return consumer.Fluent<T>(queueName);
    }
    
    /// <summary>
    /// Starts consuming messages from a queue with high-throughput settings
    /// </summary>
    /// <typeparam name="T">Message type</typeparam>
    /// <param name="consumer">Message consumer instance</param>
    /// <param name="queueName">Queue name to consume from</param>
    /// <param name="messageHandler">Message handler function</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Task representing the consumption operation</returns>
    public static Task ConsumeWithHighThroughputAsync<T>(this IMessageConsumer consumer, string queueName, Func<T, MessageContext, Task<bool>> messageHandler, CancellationToken cancellationToken = default) where T : class
    {
        return consumer.Fluent<T>(queueName)
            .ForHighThroughput()
            .ConsumeAsync(messageHandler, cancellationToken);
    }
    
    /// <summary>
    /// Starts consuming messages from a queue with low-latency settings
    /// </summary>
    /// <typeparam name="T">Message type</typeparam>
    /// <param name="consumer">Message consumer instance</param>
    /// <param name="queueName">Queue name to consume from</param>
    /// <param name="messageHandler">Message handler function</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Task representing the consumption operation</returns>
    public static Task ConsumeWithLowLatencyAsync<T>(this IMessageConsumer consumer, string queueName, Func<T, MessageContext, Task<bool>> messageHandler, CancellationToken cancellationToken = default) where T : class
    {
        return consumer.Fluent<T>(queueName)
            .ForLowLatency()
            .ConsumeAsync(messageHandler, cancellationToken);
    }
    
    /// <summary>
    /// Starts consuming messages from a queue with reliable processing settings
    /// </summary>
    /// <typeparam name="T">Message type</typeparam>
    /// <param name="consumer">Message consumer instance</param>
    /// <param name="queueName">Queue name to consume from</param>
    /// <param name="messageHandler">Message handler function</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Task representing the consumption operation</returns>
    public static Task ConsumeWithReliabilityAsync<T>(this IMessageConsumer consumer, string queueName, Func<T, MessageContext, Task<bool>> messageHandler, CancellationToken cancellationToken = default) where T : class
    {
        return consumer.Fluent<T>(queueName)
            .ForReliability()
            .ConsumeAsync(messageHandler, cancellationToken);
    }
    
    /// <summary>
    /// Starts consuming domain events for a specific aggregate type
    /// </summary>
    /// <typeparam name="T">Domain event type</typeparam>
    /// <param name="consumer">Message consumer instance</param>
    /// <param name="aggregateType">Aggregate type to consume events for</param>
    /// <param name="eventHandler">Domain event handler</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Task representing the consumption operation</returns>
    public static Task ConsumeDomainEventsAsync<T>(this IMessageConsumer consumer, string aggregateType, Func<T, Task<bool>> eventHandler, CancellationToken cancellationToken = default) where T : class, IDomainEvent
    {
        var wrappedHandler = new Func<T, FS.Transport.AMQP.EventHandlers.EventContext, Task<bool>>((evt, ctx) => eventHandler(evt));
        return consumer.ConsumeDomainEventAsync(aggregateType, wrappedHandler, cancellationToken);
    }
    
    /// <summary>
    /// Starts consuming integration events for a specific service
    /// </summary>
    /// <typeparam name="T">Integration event type</typeparam>
    /// <param name="consumer">Message consumer instance</param>
    /// <param name="serviceName">Service name to consume events from</param>
    /// <param name="eventHandler">Integration event handler</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Task representing the consumption operation</returns>
    public static Task ConsumeIntegrationEventsAsync<T>(this IMessageConsumer consumer, string serviceName, Func<T, Task<bool>> eventHandler, CancellationToken cancellationToken = default) where T : class, IIntegrationEvent
    {
        var wrappedHandler = new Func<T, FS.Transport.AMQP.EventHandlers.EventContext, Task<bool>>((evt, ctx) => eventHandler(evt));
        return consumer.ConsumeIntegrationEventAsync(serviceName, wrappedHandler, cancellationToken);
    }
    
    /// <summary>
    /// Starts consuming messages with automatic retry and dead letter handling
    /// </summary>
    /// <typeparam name="T">Message type</typeparam>
    /// <param name="consumer">Message consumer instance</param>
    /// <param name="queueName">Queue name to consume from</param>
    /// <param name="messageHandler">Message handler function</param>
    /// <param name="maxRetries">Maximum number of retries</param>
    /// <param name="retryDelay">Initial retry delay</param>
    /// <param name="deadLetterExchange">Dead letter exchange for failed messages</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Task representing the consumption operation</returns>
    public static Task ConsumeWithRetryAsync<T>(this IMessageConsumer consumer, string queueName, Func<T, MessageContext, Task<bool>> messageHandler, int maxRetries = 3, TimeSpan? retryDelay = null, string? deadLetterExchange = null, CancellationToken cancellationToken = default) where T : class
    {
        return consumer.Fluent<T>(queueName)
            .WithRetryPolicy(maxRetries, retryDelay ?? TimeSpan.FromSeconds(1))
            .When(!string.IsNullOrEmpty(deadLetterExchange), api => api.WithDeadLetter(deadLetterExchange!))
            .ConsumeAsync(messageHandler, cancellationToken);
    }
    
    /// <summary>
    /// Starts consuming messages with custom prefetch and concurrency settings
    /// </summary>
    /// <typeparam name="T">Message type</typeparam>
    /// <param name="consumer">Message consumer instance</param>
    /// <param name="queueName">Queue name to consume from</param>
    /// <param name="messageHandler">Message handler function</param>
    /// <param name="prefetchCount">Number of messages to prefetch</param>
    /// <param name="maxConcurrent">Maximum concurrent message processors</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Task representing the consumption operation</returns>
    public static Task ConsumeWithCustomConcurrencyAsync<T>(this IMessageConsumer consumer, string queueName, Func<T, MessageContext, Task<bool>> messageHandler, ushort prefetchCount, int maxConcurrent, CancellationToken cancellationToken = default) where T : class
    {
        return consumer.Fluent<T>(queueName)
            .WithPrefetch(prefetchCount)
            .WithConcurrency(maxConcurrent)
            .ConsumeAsync(messageHandler, cancellationToken);
    }
    
    /// <summary>
    /// Starts consuming messages with batch processing
    /// </summary>
    /// <typeparam name="T">Message type</typeparam>
    /// <param name="consumer">Message consumer instance</param>
    /// <param name="queueName">Queue name to consume from</param>
    /// <param name="messageHandler">Message handler function</param>
    /// <param name="batchSize">Batch size for processing</param>
    /// <param name="batchTimeout">Batch timeout</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Task representing the consumption operation</returns>
    public static Task ConsumeWithBatchingAsync<T>(this IMessageConsumer consumer, string queueName, Func<T, MessageContext, Task<bool>> messageHandler, int batchSize = 100, TimeSpan? batchTimeout = null, CancellationToken cancellationToken = default) where T : class
    {
        return consumer.Fluent<T>(queueName)
            .WithBatchProcessing(batchSize, batchTimeout)
            .ConsumeAsync(messageHandler, cancellationToken);
    }
    
    /// <summary>
    /// Starts consuming messages with deduplication enabled
    /// </summary>
    /// <typeparam name="T">Message type</typeparam>
    /// <param name="consumer">Message consumer instance</param>
    /// <param name="queueName">Queue name to consume from</param>
    /// <param name="messageHandler">Message handler function</param>
    /// <param name="cacheSize">Deduplication cache size</param>
    /// <param name="cacheTtl">Cache TTL</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Task representing the consumption operation</returns>
    public static Task ConsumeWithDeduplicationAsync<T>(this IMessageConsumer consumer, string queueName, Func<T, MessageContext, Task<bool>> messageHandler, int cacheSize = 10000, TimeSpan? cacheTtl = null, CancellationToken cancellationToken = default) where T : class
    {
        return consumer.Fluent<T>(queueName)
            .WithDeduplication(cacheSize, cacheTtl)
            .ConsumeAsync(messageHandler, cancellationToken);
    }
    
    /// <summary>
    /// Gets consumer statistics in a formatted string
    /// </summary>
    /// <param name="consumer">Message consumer instance</param>
    /// <returns>Formatted statistics string</returns>
    public static string GetFormattedStatistics(this IMessageConsumer consumer)
    {
        var stats = consumer.Statistics;
        return $"Consumer: {stats.Name} | Status: {stats.Status} | " +
               $"Messages: {stats.TotalMessages:N0} | Success Rate: {stats.SuccessRate:F1}% | " +
               $"Avg Processing: {stats.AverageProcessingTime:F1}ms | " +
               $"Processing: {stats.CurrentlyProcessing} | Uptime: {stats.Uptime:hh\\:mm\\:ss}";
    }
    
    /// <summary>
    /// Checks if the consumer is in a healthy state
    /// </summary>
    /// <param name="consumer">Message consumer instance</param>
    /// <returns>True if consumer is healthy</returns>
    public static bool IsHealthy(this IMessageConsumer consumer)
    {
        var status = consumer.Status;
        return status == ConsumerStatus.Running || status == ConsumerStatus.Paused;
    }
    
    /// <summary>
    /// Waits for the consumer to reach a specific status
    /// </summary>
    /// <param name="consumer">Message consumer instance</param>
    /// <param name="targetStatus">Target status to wait for</param>
    /// <param name="timeout">Timeout for waiting</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>True if target status was reached within timeout</returns>
    public static async Task<bool> WaitForStatusAsync(this IMessageConsumer consumer, ConsumerStatus targetStatus, TimeSpan timeout, CancellationToken cancellationToken = default)
    {
        var startTime = DateTimeOffset.UtcNow;
        
        while (consumer.Status != targetStatus && DateTimeOffset.UtcNow - startTime < timeout)
        {
            if (cancellationToken.IsCancellationRequested)
                return false;
                
            await Task.Delay(100, cancellationToken);
        }
        
        return consumer.Status == targetStatus;
    }
    
    /// <summary>
    /// Safely starts the consumer with retry on failure
    /// </summary>
    /// <param name="consumer">Message consumer instance</param>
    /// <param name="maxRetries">Maximum number of start attempts</param>
    /// <param name="retryDelay">Delay between retry attempts</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>True if consumer started successfully</returns>
    public static async Task<bool> TryStartAsync(this IMessageConsumer consumer, int maxRetries = 3, TimeSpan? retryDelay = null, CancellationToken cancellationToken = default)
    {
        var delay = retryDelay ?? TimeSpan.FromSeconds(1);
        
        for (int attempt = 1; attempt <= maxRetries; attempt++)
        {
            try
            {
                await consumer.StartAsync(cancellationToken);
                return true;
            }
            catch
            {
                if (attempt == maxRetries)
                    return false;
                    
                await Task.Delay(delay, cancellationToken);
            }
        }
        
        return false;
    }
    
    /// <summary>
    /// Safely stops the consumer with graceful shutdown
    /// </summary>
    /// <param name="consumer">Message consumer instance</param>
    /// <param name="gracefulTimeout">Timeout for graceful shutdown</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>True if consumer stopped gracefully</returns>
    public static async Task<bool> TryStopAsync(this IMessageConsumer consumer, TimeSpan? gracefulTimeout = null, CancellationToken cancellationToken = default)
    {
        var timeout = gracefulTimeout ?? TimeSpan.FromSeconds(30);
        
        try
        {
            using var cts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
            cts.CancelAfter(timeout);
            
            await consumer.StopAsync(cts.Token);
            return true;
        }
        catch
        {
            return false;
        }
    }
}