# Monitoring Guide

This guide covers comprehensive monitoring strategies for FS.StreamFlow applications, from basic health checks to advanced observability patterns.

## 📋 Table of Contents

1. [Monitoring Overview](#monitoring-overview)
2. [Health Checks](#health-checks)
3. [Metrics Collection](#metrics-collection)
4. [Logging](#logging)
5. [Alerting](#alerting)
6. [Dashboards](#dashboards)
7. [Distributed Tracing](#distributed-tracing)
8. [Performance Monitoring](#performance-monitoring)
9. [Best Practices](#best-practices)

## 🎯 Monitoring Overview

Comprehensive monitoring is essential for production RabbitMQ applications. FS.StreamFlow provides built-in monitoring capabilities that integrate with popular observability platforms.

### Key Monitoring Areas

- **Connection Health**: Connection status, recovery events
- **Producer Performance**: Throughput, latency, error rates
- **Consumer Performance**: Processing speed, acknowledgment rates
- **Queue Metrics**: Queue depth, message rates
- **Error Tracking**: Error counts, error types, dead letter statistics
- **System Resources**: CPU, memory, network usage

## 🏥 Health Checks

### Basic Health Check Implementation

```csharp
public class RabbitMQHealthCheck : IHealthCheck
{
    private readonly IStreamFlowClient _streamFlow;
    private readonly ILogger<RabbitMQHealthCheck> _logger;

    public RabbitMQHealthCheck(IStreamFlowClient streamFlow, ILogger<RabbitMQHealthCheck> logger)
    {
        _streamFlow = streamFlow;
        _logger = logger;
    }

    public async Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
    {
        try
        {
            // Initialize the client first
            await _streamFlow.InitializeAsync(cancellationToken);
            
            var data = new Dictionary<string, object>();
            
            // Check connection health
            var connectionHealthy = await CheckConnectionHealthAsync();
            data["connection_healthy"] = connectionHealthy;
            
            // Check producer health
            var producerHealthy = await CheckProducerHealthAsync();
            data["producer_healthy"] = producerHealthy;
            
            // Check consumer health
            var consumerHealthy = await CheckConsumerHealthAsync();
            data["consumer_healthy"] = consumerHealthy;
            
            // Get statistics
            var stats = await GetHealthStatisticsAsync();
            data.Add("statistics", stats);
            
            var isHealthy = connectionHealthy && producerHealthy && consumerHealthy;
            
            return isHealthy 
                ? HealthCheckResult.Healthy("RabbitMQ is healthy", data)
                : HealthCheckResult.Unhealthy("RabbitMQ has issues", data: data);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Health check failed");
            return HealthCheckResult.Unhealthy("Health check failed", ex, new Dictionary<string, object>
            {
                ["error"] = ex.Message
            });
        }
    }

    private async Task<bool> CheckConnectionHealthAsync()
    {
        return _streamFlow.ConnectionManager.IsConnected;
    }

    private async Task<bool> CheckProducerHealthAsync()
    {
        return _streamFlow.Producer.IsReady;
    }

    private async Task<bool> CheckConsumerHealthAsync()
    {
        return _streamFlow.Consumer.Status == ConsumerStatus.Running;
    }

    private async Task<object> GetHealthStatisticsAsync()
    {
        return new
        {
            ConnectionState = _streamFlow.ConnectionManager.State.ToString(),
            ProducerStatistics = _streamFlow.Producer.Statistics,
            ConsumerStatistics = _streamFlow.Consumer.Statistics,
            LastHealthCheck = DateTimeOffset.UtcNow
        };
    }
}
```

### Advanced Health Check with Probes

```csharp
public class DetailedRabbitMQHealthCheck : IHealthCheck
{
    private readonly IStreamFlowClient _streamFlow;
    private readonly ILogger<DetailedRabbitMQHealthCheck> _logger;

    public DetailedRabbitMQHealthCheck(IStreamFlowClient streamFlow, ILogger<DetailedRabbitMQHealthCheck> logger)
    {
        _streamFlow = streamFlow;
        _logger = logger;
    }

    public async Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
    {
        // Initialize the client first
        await _streamFlow.InitializeAsync(cancellationToken);
        
        var healthData = new Dictionary<string, object>();
        var issues = new List<string>();

        try
        {
            // Connection health probe
            var connectionResult = await ProbeConnectionHealthAsync();
            healthData["connection"] = connectionResult;
            if (!connectionResult.IsHealthy)
                issues.Add("Connection issues detected");

            // Producer health probe
            var producerResult = await ProbeProducerHealthAsync();
            healthData["producer"] = producerResult;
            if (!producerResult.IsHealthy)
                issues.Add("Producer issues detected");

            // Consumer health probe
            var consumerResult = await ProbeConsumerHealthAsync();
            healthData["consumer"] = consumerResult;
            if (!consumerResult.IsHealthy)
                issues.Add("Consumer issues detected");

            // Queue health probe
            var queueResult = await ProbeQueueHealthAsync();
            healthData["queues"] = queueResult;
            if (!queueResult.IsHealthy)
                issues.Add("Queue issues detected");

            // Error rate probe
            var errorResult = await ProbeErrorRatesAsync();
            healthData["error_rates"] = errorResult;
            if (!errorResult.IsHealthy)
                issues.Add("High error rates detected");

            // Performance probe
            var performanceResult = await ProbePerformanceAsync();
            healthData["performance"] = performanceResult;
            if (!performanceResult.IsHealthy)
                issues.Add("Performance issues detected");

            var isHealthy = issues.Count == 0;
            var status = isHealthy ? "Healthy" : $"Issues: {string.Join(", ", issues)}";

            return isHealthy
                ? HealthCheckResult.Healthy(status, healthData)
                : HealthCheckResult.Unhealthy(status, data: healthData);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Detailed health check failed");
            return HealthCheckResult.Unhealthy("Health check failed", ex, healthData);
        }
    }

    private async Task<ProbeResult> ProbeConnectionHealthAsync()
    {
        var connectionManager = _streamFlow.ConnectionManager;
        var isConnected = connectionManager.IsConnected;
        var statistics = connectionManager.Statistics;

        return new ProbeResult
        {
            IsHealthy = isConnected,
            Data = new
            {
                IsConnected = isConnected,
                State = connectionManager.State.ToString(),
                ConnectionCount = statistics.ConnectionCount,
                ChannelCount = statistics.ChannelCount,
                LastRecovery = statistics.LastRecoveryTime,
                RecoveryCount = statistics.RecoveryCount
            }
        };
    }

    private async Task<ProbeResult> ProbeProducerHealthAsync()
    {
        var producer = _streamFlow.Producer;
        var isReady = producer.IsReady;
        var statistics = producer.Statistics;

        var errorRate = statistics.TotalPublished > 0 
            ? (double)statistics.TotalFailed / statistics.TotalPublished * 100 
            : 0;

        return new ProbeResult
        {
            IsHealthy = isReady && errorRate < 5, // Less than 5% error rate
            Data = new
            {
                IsReady = isReady,
                Status = statistics.Status.ToString(),
                TotalPublished = statistics.TotalPublished,
                TotalConfirmed = statistics.TotalConfirmed,
                TotalFailed = statistics.TotalFailed,
                ErrorRate = errorRate,
                LastPublish = statistics.LastUpdateTime
            }
        };
    }

    private async Task<ProbeResult> ProbeConsumerHealthAsync()
    {
        var consumer = _streamFlow.Consumer;
        var status = consumer.Status;
        var statistics = consumer.Statistics;

        var errorRate = statistics.MessagesProcessed > 0 
            ? (double)statistics.MessagesRejected / statistics.MessagesProcessed * 100 
            : 0;

        return new ProbeResult
        {
            IsHealthy = status == ConsumerStatus.Running && errorRate < 5,
            Data = new
            {
                Status = status.ToString(),
                MessagesProcessed = statistics.MessagesProcessed,
                MessagesAcknowledged = statistics.MessagesAcknowledged,
                MessagesRejected = statistics.MessagesRejected,
                ErrorRate = errorRate,
                LastMessage = statistics.LastMessageTime
            }
        };
    }

    private async Task<ProbeResult> ProbeQueueHealthAsync()
    {
        try
        {
            var queueInfo = await _streamFlow.QueueManager.GetQueueInfoAsync("order-processing");
            var messageCount = queueInfo.MessageCount;
            var consumerCount = queueInfo.ConsumerCount;

            // Check for queue depth issues
            var isHealthy = messageCount < 10000 && consumerCount > 0;

            return new ProbeResult
            {
                IsHealthy = isHealthy,
                Data = new
                {
                    MessageCount = messageCount,
                    ConsumerCount = consumerCount,
                    QueueName = queueInfo.Name,
                    IsDurable = queueInfo.IsDurable,
                    IsExclusive = queueInfo.IsExclusive,
                    IsAutoDelete = queueInfo.IsAutoDelete
                }
            };
        }
        catch (Exception ex)
        {
            return new ProbeResult
            {
                IsHealthy = false,
                Data = new { Error = ex.Message }
            };
        }
    }

    private async Task<ProbeResult> ProbeErrorRatesAsync()
    {
        var statistics = _streamFlow.ErrorHandler.Statistics;
        var errorRate = statistics.TotalProcessed > 0 
            ? (double)statistics.TotalErrors / statistics.TotalProcessed * 100 
            : 0;

        return new ProbeResult
        {
            IsHealthy = errorRate < 5, // Less than 5% error rate
            Data = new
            {
                ErrorRate = errorRate,
                TotalErrors = statistics.TotalErrors,
                TotalProcessed = statistics.TotalProcessed,
                DeadLetterCount = statistics.DeadLetterCount,
                LastError = statistics.LastError?.Message
            }
        };
    }

    private async Task<ProbeResult> ProbePerformanceAsync()
    {
        var producerStats = _streamFlow.Producer.Statistics;
        var consumerStats = _streamFlow.Consumer.Statistics;

        // Calculate throughput over the last minute
        var timeWindow = TimeSpan.FromMinutes(1);
        var producerThroughput = CalculateThroughput(producerStats, timeWindow);
        var consumerThroughput = CalculateThroughput(consumerStats, timeWindow);

        var isHealthy = producerThroughput > 0 && consumerThroughput > 0;

        return new ProbeResult
        {
            IsHealthy = isHealthy,
            Data = new
            {
                ProducerThroughput = producerThroughput,
                ConsumerThroughput = consumerThroughput,
                TimeWindow = timeWindow.TotalMinutes
            }
        };
    }

    private double CalculateThroughput(object statistics, TimeSpan timeWindow)
    {
        // Simplified throughput calculation
        // In a real implementation, you'd track metrics over time
        return Random.Shared.NextDouble() * 1000; // Placeholder
    }
}

public class ProbeResult
{
    public bool IsHealthy { get; set; }
    public object Data { get; set; } = new();
}
```

## 📊 Metrics Collection

### Built-in Metrics

```csharp
public class RabbitMQMetricsCollector
{
    private readonly IStreamFlowClient _streamFlow;
    private readonly ILogger<RabbitMQMetricsCollector> _logger;
    private readonly Timer _metricsTimer;

    public RabbitMQMetricsCollector(IStreamFlowClient streamFlow, ILogger<RabbitMQMetricsCollector> logger)
    {
        _streamFlow = streamFlow;
        _logger = logger;
        
        // Collect metrics every 30 seconds
        _metricsTimer = new Timer(CollectMetrics, null, TimeSpan.Zero, TimeSpan.FromSeconds(30));
    }

    private async void CollectMetrics(object? state)
    {
        try
        {
            // Initialize the client first
            await _streamFlow.InitializeAsync();
            
            var metrics = await CollectAllMetricsAsync();
            await PublishMetricsAsync(metrics);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to collect metrics");
        }
    }

    private async Task<object> CollectAllMetricsAsync()
    {
        var connectionStats = _streamFlow.ConnectionManager.Statistics;
        var producerStats = _streamFlow.Producer.Statistics;
        var consumerStats = _streamFlow.Consumer.Statistics;

        return new
        {
            Timestamp = DateTimeOffset.UtcNow,
            Connection = new
            {
                IsConnected = _streamFlow.ConnectionManager.IsConnected,
                State = _streamFlow.ConnectionManager.State.ToString(),
                ConnectionCount = connectionStats.ConnectionCount,
                ChannelCount = connectionStats.ChannelCount,
                RecoveryCount = connectionStats.RecoveryCount,
                LastRecoveryTime = connectionStats.LastRecoveryTime
            },
            Producer = new
            {
                IsReady = _streamFlow.Producer.IsReady,
                Status = producerStats.Status.ToString(),
                TotalPublished = producerStats.TotalPublished,
                TotalConfirmed = producerStats.TotalConfirmed,
                TotalFailed = producerStats.TotalFailed,
                PublishRate = CalculateRate(producerStats.TotalPublished, producerStats.StartTime),
                ConfirmRate = CalculateRate(producerStats.TotalConfirmed, producerStats.StartTime),
                ErrorRate = producerStats.TotalPublished > 0 ? 
                    (double)producerStats.TotalFailed / producerStats.TotalPublished * 100 : 0
            },
            Consumer = new
            {
                Status = consumerStats.Status.ToString(),
                MessagesProcessed = consumerStats.MessagesProcessed,
                MessagesAcknowledged = consumerStats.MessagesAcknowledged,
                MessagesRejected = consumerStats.MessagesRejected,
                ProcessingRate = CalculateRate(consumerStats.MessagesProcessed, consumerStats.StartTime),
                AckRate = CalculateRate(consumerStats.MessagesAcknowledged, consumerStats.StartTime),
                RejectRate = CalculateRate(consumerStats.MessagesRejected, consumerStats.StartTime)
            },
            System = await CollectSystemMetricsAsync()
        };
    }

    private double CalculateRate(long count, DateTimeOffset startTime)
    {
        var elapsed = DateTimeOffset.UtcNow - startTime;
        return elapsed.TotalSeconds > 0 ? count / elapsed.TotalSeconds : 0;
    }

    private async Task<object> CollectSystemMetricsAsync()
    {
        var process = Process.GetCurrentProcess();
        
        return new
        {
            CpuUsage = await GetCpuUsageAsync(),
            MemoryUsage = process.WorkingSet64,
            ThreadCount = process.Threads.Count,
            HandleCount = process.HandleCount,
            GCCollectionCount = new Dictionary<string, long>
            {
                ["Gen0"] = GC.CollectionCount(0),
                ["Gen1"] = GC.CollectionCount(1),
                ["Gen2"] = GC.CollectionCount(2)
            }
        };
    }

    private async Task<double> GetCpuUsageAsync()
    {
        var process = Process.GetCurrentProcess();
        var startTime = DateTime.UtcNow;
        var startCpuUsage = process.TotalProcessorTime;
        
        await Task.Delay(1000);
        
        var endTime = DateTime.UtcNow;
        var endCpuUsage = process.TotalProcessorTime;
        
        var cpuUsedMs = (endCpuUsage - startCpuUsage).TotalMilliseconds;
        var totalMsPassed = (endTime - startTime).TotalMilliseconds;
        
        return cpuUsedMs / (Environment.ProcessorCount * totalMsPassed) * 100;
    }

    private async Task PublishMetricsAsync(object metrics)
    {
        // Publish metrics to monitoring system
        await _streamFlow.Producer.Message(metrics)
            .WithExchange("monitoring")
            .WithRoutingKey("metrics.rabbitmq")
            .PublishAsync();
        
        // Log key metrics
        _logger.LogInformation("RabbitMQ Metrics: " +
            "Producer Rate: {ProducerRate:F1} msgs/sec, " +
            "Consumer Rate: {ConsumerRate:F1} msgs/sec, " +
            "Error Rate: {ErrorRate:F2}%, " +
            "Memory: {MemoryMB:F1} MB",
            ((dynamic)metrics).Producer.PublishRate,
            ((dynamic)metrics).Consumer.ProcessingRate,
            ((dynamic)metrics).Producer.ErrorRate,
            ((dynamic)metrics).System.MemoryUsage / 1024.0 / 1024.0);
    }
}
```

### Custom Metrics

```csharp
public class CustomMetricsCollector
{
    private readonly IStreamFlowClient _streamFlow;
    private readonly ILogger<CustomMetricsCollector> _logger;
    private readonly ConcurrentDictionary<string, CustomMetric> _customMetrics = new();

    public CustomMetricsCollector(IStreamFlowClient streamFlow, ILogger<CustomMetricsCollector> logger)
    {
        _streamFlow = streamFlow;
        _logger = logger;
    }

    public void RecordCounter(string name, long value, Dictionary<string, string>? tags = null)
    {
        var metric = new CustomMetric
        {
            Name = name,
            Type = MetricType.Counter,
            Value = value,
            Tags = tags ?? new Dictionary<string, string>(),
            Timestamp = DateTimeOffset.UtcNow
        };

        _customMetrics[name] = metric;
    }

    public void RecordGauge(string name, double value, Dictionary<string, string>? tags = null)
    {
        var metric = new CustomMetric
        {
            Name = name,
            Type = MetricType.Gauge,
            Value = value,
            Tags = tags ?? new Dictionary<string, string>(),
            Timestamp = DateTimeOffset.UtcNow
        };

        _customMetrics[name] = metric;
    }

    public void RecordHistogram(string name, double value, Dictionary<string, string>? tags = null)
    {
        var key = $"{name}_{string.Join("_", tags?.Select(kv => $"{kv.Key}:{kv.Value}") ?? Enumerable.Empty<string>())}";
        
        _customMetrics.AddOrUpdate(key, 
            new CustomMetric
            {
                Name = name,
                Type = MetricType.Histogram,
                Value = value,
                Tags = tags ?? new Dictionary<string, string>(),
                Timestamp = DateTimeOffset.UtcNow,
                Count = 1,
                Sum = value,
                Min = value,
                Max = value
            },
            (k, existing) =>
            {
                existing.Count++;
                existing.Sum += value;
                existing.Min = Math.Min(existing.Min, value);
                existing.Max = Math.Max(existing.Max, value);
                existing.Timestamp = DateTimeOffset.UtcNow;
                return existing;
            });
    }

    public async Task FlushMetricsAsync()
    {
        // Initialize the client first
        await _streamFlow.InitializeAsync();
        
        var metrics = _customMetrics.Values.ToList();
        _customMetrics.Clear();

        foreach (var metric in metrics)
        {
            await _streamFlow.Producer.Message(metric)
                .WithExchange("metrics")
                .WithRoutingKey($"custom.{metric.Type.ToString().ToLower()}")
                .PublishAsync();
        }

        _logger.LogInformation("Flushed {MetricsCount} custom metrics", metrics.Count);
    }
}

public class CustomMetric
{
    public string Name { get; set; } = string.Empty;
    public MetricType Type { get; set; }
    public double Value { get; set; }
    public Dictionary<string, string> Tags { get; set; } = new();
    public DateTimeOffset Timestamp { get; set; }
    public long Count { get; set; }
    public double Sum { get; set; }
    public double Min { get; set; }
    public double Max { get; set; }
}

public enum MetricType
{
    Counter,
    Gauge,
    Histogram
}
```

## 📝 Logging

### Structured Logging

```csharp
public class RabbitMQLogger
{
    private readonly ILogger<RabbitMQLogger> _logger;
    private readonly IStreamFlowClient _streamFlow;

    public RabbitMQLogger(ILogger<RabbitMQLogger> logger, IStreamFlowClient streamFlow)
    {
        _logger = logger;
        _streamFlow = streamFlow;
    }

    public void LogMessagePublished(string messageId, string exchange, string routingKey, int size)
    {
        using var scope = _logger.BeginScope(new Dictionary<string, object>
        {
            ["MessageId"] = messageId,
            ["Exchange"] = exchange,
            ["RoutingKey"] = routingKey,
            ["MessageSize"] = size
        });

        _logger.LogInformation("Message published: {MessageId} to {Exchange}/{RoutingKey} ({Size} bytes)", 
            messageId, exchange, routingKey, size);
    }

    public void LogMessageConsumed(string messageId, string queue, TimeSpan processingTime, bool success)
    {
        using var scope = _logger.BeginScope(new Dictionary<string, object>
        {
            ["MessageId"] = messageId,
            ["Queue"] = queue,
            ["ProcessingTime"] = processingTime.TotalMilliseconds,
            ["Success"] = success
        });

        if (success)
        {
            _logger.LogInformation("Message consumed: {MessageId} from {Queue} in {ProcessingTime}ms", 
                messageId, queue, processingTime.TotalMilliseconds);
        }
        else
        {
            _logger.LogError("Message consumption failed: {MessageId} from {Queue} after {ProcessingTime}ms", 
                messageId, queue, processingTime.TotalMilliseconds);
        }
    }

    public void LogConnectionEvent(string eventType, string connectionName, string details)
    {
        using var scope = _logger.BeginScope(new Dictionary<string, object>
        {
            ["EventType"] = eventType,
            ["ConnectionName"] = connectionName,
            ["Details"] = details
        });

        _logger.LogWarning("Connection event: {EventType} for {ConnectionName} - {Details}", 
            eventType, connectionName, details);
    }

    public void LogErrorEvent(Exception exception, string context, Dictionary<string, object>? additionalData = null)
    {
        var scopeData = new Dictionary<string, object>
        {
            ["Context"] = context,
            ["ExceptionType"] = exception.GetType().Name
        };

        if (additionalData != null)
        {
            foreach (var kv in additionalData)
            {
                scopeData[kv.Key] = kv.Value;
            }
        }

        using var scope = _logger.BeginScope(scopeData);

        _logger.LogError(exception, "Error in {Context}: {ErrorMessage}", context, exception.Message);
    }
}
```

### Log Aggregation

```csharp
public class LogAggregator
{
    private readonly IStreamFlowClient _streamFlow;
    private readonly ILogger<LogAggregator> _logger;
    private readonly Channel<LogEntry> _logChannel;

    public LogAggregator(IStreamFlowClient streamFlow, ILogger<LogAggregator> logger)
    {
        _streamFlow = streamFlow;
        _logger = logger;
        
        _logChannel = Channel.CreateBounded<LogEntry>(new BoundedChannelOptions(10000)
        {
            FullMode = BoundedChannelFullMode.Wait,
            SingleReader = true,
            SingleWriter = false
        });
        
        // Start log processing
        _ = Task.Run(ProcessLogsAsync);
    }

    public async Task LogAsync(LogLevel level, string message, Dictionary<string, object>? properties = null)
    {
        var logEntry = new LogEntry
        {
            Level = level,
            Message = message,
            Properties = properties ?? new Dictionary<string, object>(),
            Timestamp = DateTimeOffset.UtcNow,
            Source = "RabbitMQ"
        };

        await _logChannel.Writer.WriteAsync(logEntry);
    }

    private async Task ProcessLogsAsync()
    {
        var batch = new List<LogEntry>();
        
        await foreach (var logEntry in _logChannel.Reader.ReadAllAsync())
        {
            batch.Add(logEntry);
            
            // Process batch when full or after timeout
            if (batch.Count >= 100 || 
                (batch.Count > 0 && DateTimeOffset.UtcNow - batch[0].Timestamp > TimeSpan.FromSeconds(5)))
            {
                await ProcessLogBatchAsync(batch);
                batch.Clear();
            }
        }
    }

    private async Task ProcessLogBatchAsync(List<LogEntry> logs)
    {
        try
        {
            // Initialize the client first
            await _streamFlow.InitializeAsync();
            
            // Send logs to centralized logging system
            await _streamFlow.Producer.Message(new LogBatch
            {
                Logs = logs,
                BatchId = Guid.NewGuid().ToString(),
                Timestamp = DateTimeOffset.UtcNow
            })
            .WithExchange("logs")
            .WithRoutingKey("application.logs")
            .PublishAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to process log batch with {LogCount} entries", logs.Count);
        }
    }
}

public class LogEntry
{
    public LogLevel Level { get; set; }
    public string Message { get; set; } = string.Empty;
    public Dictionary<string, object> Properties { get; set; } = new();
    public DateTimeOffset Timestamp { get; set; }
    public string Source { get; set; } = string.Empty;
}

public class LogBatch
{
    public List<LogEntry> Logs { get; set; } = new();
    public string BatchId { get; set; } = string.Empty;
    public DateTimeOffset Timestamp { get; set; }
}
```

## 🚨 Alerting

### Alert Manager

```csharp
public class AlertManager
{
    private readonly IStreamFlowClient _streamFlow;
    private readonly ILogger<AlertManager> _logger;
    private readonly Dictionary<string, Alert> _activeAlerts = new();

    public AlertManager(IStreamFlowClient streamFlow, ILogger<AlertManager> logger)
    {
        _streamFlow = streamFlow;
        _logger = logger;
    }

    public async Task SendAlertAsync(string alertType, string message, AlertSeverity severity, Dictionary<string, object>? metadata = null)
    {
        // Initialize the client first
        await _streamFlow.InitializeAsync();
        
        var alert = new Alert
        {
            Id = Guid.NewGuid().ToString(),
            Type = alertType,
            Message = message,
            Severity = severity,
            Timestamp = DateTimeOffset.UtcNow,
            Metadata = metadata ?? new Dictionary<string, object>(),
            Source = "RabbitMQ"
        };

        // Check if this is a duplicate alert
        var alertKey = $"{alertType}:{message}";
        if (_activeAlerts.ContainsKey(alertKey))
        {
            _logger.LogDebug("Suppressing duplicate alert: {AlertType}", alertType);
            return;
        }

        _activeAlerts[alertKey] = alert;

        // Send alert
        await _streamFlow.Producer.Message(alert)
            .WithExchange("alerts")
            .WithRoutingKey($"alert.{severity.ToString().ToLower()}")
            .PublishAsync();

        _logger.LogWarning("Alert sent: {AlertType} - {Message} (Severity: {Severity})", 
            alertType, message, severity);

        // Schedule alert cleanup
        _ = Task.Delay(TimeSpan.FromMinutes(5)).ContinueWith(_ => 
        {
            _activeAlerts.TryRemove(alertKey, out _);
        });
    }

    public async Task SendConnectionAlert(string connectionName, string reason)
    {
        await SendAlertAsync(
            "CONNECTION_FAILURE",
            $"Connection '{connectionName}' failed: {reason}",
            AlertSeverity.High,
            new Dictionary<string, object>
            {
                ["connection_name"] = connectionName,
                ["failure_reason"] = reason
            });
    }

    public async Task SendHighErrorRateAlert(string component, double errorRate)
    {
        await SendAlertAsync(
            "HIGH_ERROR_RATE",
            $"High error rate detected in {component}: {errorRate:F2}%",
            AlertSeverity.Medium,
            new Dictionary<string, object>
            {
                ["component"] = component,
                ["error_rate"] = errorRate
            });
    }

    public async Task SendQueueDepthAlert(string queueName, long messageCount)
    {
        await SendAlertAsync(
            "QUEUE_DEPTH_HIGH",
            $"Queue '{queueName}' has {messageCount} messages",
            AlertSeverity.Medium,
            new Dictionary<string, object>
            {
                ["queue_name"] = queueName,
                ["message_count"] = messageCount
            });
    }

    public async Task SendPerformanceAlert(string metric, double value, double threshold)
    {
        var severity = value > threshold * 2 ? AlertSeverity.High : AlertSeverity.Medium;
        
        await SendAlertAsync(
            "PERFORMANCE_DEGRADATION",
            $"Performance metric '{metric}' is {value:F2} (threshold: {threshold:F2})",
            severity,
            new Dictionary<string, object>
            {
                ["metric"] = metric,
                ["value"] = value,
                ["threshold"] = threshold
            });
    }
}

public class Alert
{
    public string Id { get; set; } = string.Empty;
    public string Type { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
    public AlertSeverity Severity { get; set; }
    public DateTimeOffset Timestamp { get; set; }
    public Dictionary<string, object> Metadata { get; set; } = new();
    public string Source { get; set; } = string.Empty;
}

public enum AlertSeverity
{
    Low,
    Medium,
    High,
    Critical
}
```

## 📊 Dashboards

### Dashboard Data Provider

```csharp
public class DashboardDataProvider
{
    private readonly IStreamFlowClient _streamFlow;
    private readonly ILogger<DashboardDataProvider> _logger;

    public DashboardDataProvider(IStreamFlowClient streamFlow, ILogger<DashboardDataProvider> logger)
    {
        _streamFlow = streamFlow;
        _logger = logger;
    }

    public async Task<DashboardData> GetDashboardDataAsync()
    {
        // Initialize the client first
        await _streamFlow.InitializeAsync();
        
        var data = new DashboardData
        {
            Timestamp = DateTimeOffset.UtcNow,
            Overview = await GetOverviewDataAsync(),
            Producer = await GetProducerDataAsync(),
            Consumer = await GetConsumerDataAsync(),
            Queues = await GetQueueDataAsync(),
            Errors = await GetErrorDataAsync(),
            Performance = await GetPerformanceDataAsync()
        };

        return data;
    }

    private async Task<OverviewData> GetOverviewDataAsync()
    {
        var connectionManager = _streamFlow.ConnectionManager;
        var producer = _streamFlow.Producer;
        var consumer = _streamFlow.Consumer;

        return new OverviewData
        {
            IsHealthy = connectionManager.IsConnected && producer.IsReady && consumer.Status == ConsumerStatus.Running,
            ConnectionState = connectionManager.State.ToString(),
            ProducerStatus = producer.IsReady ? "Ready" : "Not Ready",
            ConsumerStatus = consumer.Status.ToString(),
            Uptime = DateTimeOffset.UtcNow - Process.GetCurrentProcess().StartTime
        };
    }

    private async Task<ProducerData> GetProducerDataAsync()
    {
        var stats = _streamFlow.Producer.Statistics;
        var runtime = DateTimeOffset.UtcNow - stats.StartTime;

        return new ProducerData
        {
            IsReady = _streamFlow.Producer.IsReady,
            TotalPublished = stats.TotalPublished,
            TotalConfirmed = stats.TotalConfirmed,
            TotalFailed = stats.TotalFailed,
            PublishRate = runtime.TotalSeconds > 0 ? stats.TotalPublished / runtime.TotalSeconds : 0,
            ErrorRate = stats.TotalPublished > 0 ? (double)stats.TotalFailed / stats.TotalPublished * 100 : 0,
            LastActivity = stats.LastUpdateTime
        };
    }

    private async Task<ConsumerData> GetConsumerDataAsync()
    {
        var stats = _streamFlow.Consumer.Statistics;
        var runtime = DateTimeOffset.UtcNow - stats.StartTime;

        return new ConsumerData
        {
            Status = stats.Status.ToString(),
            MessagesProcessed = stats.MessagesProcessed,
            MessagesAcknowledged = stats.MessagesAcknowledged,
            MessagesRejected = stats.MessagesRejected,
            ProcessingRate = runtime.TotalSeconds > 0 ? stats.MessagesProcessed / runtime.TotalSeconds : 0,
            AckRate = stats.MessagesProcessed > 0 ? (double)stats.MessagesAcknowledged / stats.MessagesProcessed * 100 : 0,
            RejectRate = stats.MessagesProcessed > 0 ? (double)stats.MessagesRejected / stats.MessagesProcessed * 100 : 0,
            LastActivity = stats.LastMessageTime
        };
    }

    private async Task<List<QueueData>> GetQueueDataAsync()
    {
        var queues = new List<QueueData>();
        
        try
        {
            var queueNames = new[] { "order-processing", "user-processing", "product-processing" };
            
            foreach (var queueName in queueNames)
            {
                var queueInfo = await _streamFlow.QueueManager.GetQueueInfoAsync(queueName);
                
                queues.Add(new QueueData
                {
                    Name = queueInfo.Name,
                    MessageCount = queueInfo.MessageCount,
                    ConsumerCount = queueInfo.ConsumerCount,
                    MessageRate = CalculateMessageRate(queueName), // Simplified
                    IsDurable = queueInfo.IsDurable
                });
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get queue data");
        }

        return queues;
    }

    private async Task<ErrorData> GetErrorDataAsync()
    {
        var errorStats = _streamFlow.ErrorHandler.Statistics;
        var runtime = DateTimeOffset.UtcNow - errorStats.StartTime;

        return new ErrorData
        {
            TotalErrors = errorStats.TotalErrors,
            ErrorRate = runtime.TotalSeconds > 0 ? errorStats.TotalErrors / runtime.TotalSeconds : 0,
            DeadLetterCount = errorStats.DeadLetterCount,
            LastError = errorStats.LastError?.Message ?? "None",
            LastErrorTime = errorStats.LastErrorTime
        };
    }

    private async Task<PerformanceData> GetPerformanceDataAsync()
    {
        var process = Process.GetCurrentProcess();
        
        return new PerformanceData
        {
            CpuUsage = await GetCpuUsageAsync(),
            MemoryUsage = process.WorkingSet64,
            ThreadCount = process.Threads.Count,
            GCCollections = new Dictionary<string, long>
            {
                ["Gen0"] = GC.CollectionCount(0),
                ["Gen1"] = GC.CollectionCount(1),
                ["Gen2"] = GC.CollectionCount(2)
            }
        };
    }

    private double CalculateMessageRate(string queueName)
    {
        // Simplified rate calculation
        return Random.Shared.NextDouble() * 1000;
    }

    private async Task<double> GetCpuUsageAsync()
    {
        // Simplified CPU usage calculation
        return Random.Shared.NextDouble() * 100;
    }
}

// Dashboard data models
public class DashboardData
{
    public DateTimeOffset Timestamp { get; set; }
    public OverviewData Overview { get; set; } = new();
    public ProducerData Producer { get; set; } = new();
    public ConsumerData Consumer { get; set; } = new();
    public List<QueueData> Queues { get; set; } = new();
    public ErrorData Errors { get; set; } = new();
    public PerformanceData Performance { get; set; } = new();
}

public class OverviewData
{
    public bool IsHealthy { get; set; }
    public string ConnectionState { get; set; } = string.Empty;
    public string ProducerStatus { get; set; } = string.Empty;
    public string ConsumerStatus { get; set; } = string.Empty;
    public TimeSpan Uptime { get; set; }
}

public class ProducerData
{
    public bool IsReady { get; set; }
    public long TotalPublished { get; set; }
    public long TotalConfirmed { get; set; }
    public long TotalFailed { get; set; }
    public double PublishRate { get; set; }
    public double ErrorRate { get; set; }
    public DateTimeOffset LastActivity { get; set; }
}

public class ConsumerData
{
    public string Status { get; set; } = string.Empty;
    public long MessagesProcessed { get; set; }
    public long MessagesAcknowledged { get; set; }
    public long MessagesRejected { get; set; }
    public double ProcessingRate { get; set; }
    public double AckRate { get; set; }
    public double RejectRate { get; set; }
    public DateTimeOffset LastActivity { get; set; }
}

public class QueueData
{
    public string Name { get; set; } = string.Empty;
    public long MessageCount { get; set; }
    public int ConsumerCount { get; set; }
    public double MessageRate { get; set; }
    public bool IsDurable { get; set; }
}

public class ErrorData
{
    public long TotalErrors { get; set; }
    public double ErrorRate { get; set; }
    public long DeadLetterCount { get; set; }
    public string LastError { get; set; } = string.Empty;
    public DateTimeOffset LastErrorTime { get; set; }
}

public class PerformanceData
{
    public double CpuUsage { get; set; }
    public long MemoryUsage { get; set; }
    public int ThreadCount { get; set; }
    public Dictionary<string, long> GCCollections { get; set; } = new();
}
```

## 🔍 Distributed Tracing

### Tracing Integration

```csharp
public class RabbitMQTracer
{
    private readonly IStreamFlowClient _streamFlow;
    private readonly ILogger<RabbitMQTracer> _logger;
    private readonly ActivitySource _activitySource;

    public RabbitMQTracer(IStreamFlowClient streamFlow, ILogger<RabbitMQTracer> logger)
    {
        _streamFlow = streamFlow;
        _logger = logger;
        _activitySource = new ActivitySource("FS.StreamFlow");
    }

    public async Task<T> TraceOperationAsync<T>(string operationName, Func<Activity?, Task<T>> operation)
    {
        using var activity = _activitySource.StartActivity(operationName);
        
        try
        {
            activity?.SetTag("messaging.system", "rabbitmq");
            activity?.SetTag("messaging.operation", operationName);
            
            var result = await operation(activity);
            
            activity?.SetStatus(ActivityStatusCode.Ok);
            return result;
        }
        catch (Exception ex)
        {
            activity?.SetStatus(ActivityStatusCode.Error, ex.Message);
            activity?.SetTag("error.type", ex.GetType().Name);
            activity?.SetTag("error.message", ex.Message);
            throw;
        }
    }

    public async Task TracePublishAsync(string exchange, string routingKey, object message)
    {
        await TraceOperationAsync("publish", async (activity) =>
        {
            activity?.SetTag("messaging.destination", exchange);
            activity?.SetTag("messaging.routing_key", routingKey);
            activity?.SetTag("messaging.message_id", Guid.NewGuid().ToString());
            
            // Add trace context to message headers
            var headers = new Dictionary<string, object>();
            if (activity != null)
            {
                headers["trace-id"] = activity.TraceId.ToString();
                headers["span-id"] = activity.SpanId.ToString();
            }
            
            // Initialize the client first
            await _streamFlow.InitializeAsync();
            
            await _streamFlow.Producer.Message(message)
                .WithExchange(exchange)
                .WithRoutingKey(routingKey)
                .WithHeaders(headers)
                .PublishAsync();
                
            return true;
        });
    }

    public async Task TraceConsumeAsync<T>(string queueName, Func<T, MessageContext, Task<bool>> handler)
    {
        // Initialize the client first
        await _streamFlow.InitializeAsync();
        
        await _streamFlow.Consumer.Queue<T>(queueName)
            .ConsumeAsync(async (message, context) =>
            {
                return await TraceOperationAsync("consume", async (activity) =>
                {
                    activity?.SetTag("messaging.source", queueName);
                    activity?.SetTag("messaging.message_id", context.MessageId);
                    
                    // Extract trace context from message headers
                    if (context.Headers.TryGetValue("trace-id", out var traceId))
                    {
                        activity?.SetTag("messaging.trace_id", traceId.ToString());
                    }
                    
                    if (context.Headers.TryGetValue("span-id", out var spanId))
                    {
                        activity?.SetTag("messaging.parent_span_id", spanId.ToString());
                    }
                    
                    var result = await handler(message, context);
                    
                    activity?.SetTag("messaging.acknowledged", result);
                    return result;
                });
            });
    }
}
```

## 🎯 Best Practices

### 1. Implement Comprehensive Health Checks

```csharp
// DO: Check all components
public async Task<HealthCheckResult> CheckHealthAsync()
{
    // Initialize the client first
    await _streamFlow.InitializeAsync();
    
    var checks = new[]
    {
        CheckConnectionHealth(),
        CheckProducerHealth(),
        CheckConsumerHealth(),
        CheckQueueHealth(),
        CheckErrorRates()
    };
    
    var results = await Task.WhenAll(checks);
    return results.All(r => r.IsHealthy) ? HealthCheckResult.Healthy() : HealthCheckResult.Unhealthy();
}
```

### 2. Use Structured Logging

```csharp
// DO: Use structured logging with context
_logger.LogInformation("Message processed: {MessageId} in {ProcessingTime}ms", 
    messageId, processingTime);

// DON'T: Use string interpolation
_logger.LogInformation($"Message processed: {messageId} in {processingTime}ms");
```

### 3. Implement Proper Alerting

```csharp
// DO: Alert on important metrics
if (errorRate > 5.0)
{
    await _alertManager.SendHighErrorRateAlert("Consumer", errorRate);
}

// DON'T: Alert on every minor issue
```

### 4. Monitor Key Metrics

```csharp
// DO: Monitor these key metrics
var keyMetrics = new[]
{
    "connection.state",
    "producer.publish_rate",
    "consumer.processing_rate",
    "queue.message_count",
    "error.rate",
    "system.memory_usage"
};
```

### 5. Use Distributed Tracing

```csharp
// DO: Add tracing to async operations
using var activity = _activitySource.StartActivity("ProcessOrder");
activity?.SetTag("order.id", order.Id);

await ProcessOrderAsync(order);
```

### 6. Implement Circuit Breaker for Monitoring

```csharp
// DO: Use circuit breaker for monitoring systems
await _monitoringCircuitBreaker.ExecuteAsync(async () =>
{
    await _metricsCollector.SendMetricsAsync();
});
```

## 🎉 Summary

You've now learned comprehensive monitoring for FS.StreamFlow:

✅ **Health checks and probes**  
✅ **Metrics collection and custom metrics**  
✅ **Structured logging and log aggregation**  
✅ **Alerting and notification systems**  
✅ **Dashboard data providers**  
✅ **Distributed tracing integration**  
✅ **Performance monitoring**  
✅ **Production monitoring best practices**  

## 🎯 Next Steps

Complete your FS.StreamFlow journey:

- [Examples](examples/) - See real-world examples
- [Performance Tuning](performance.md) - Optimize for high throughput
- [Error Handling](error-handling.md) - Implement robust error handling
- [Configuration](configuration.md) - Advanced configuration options 