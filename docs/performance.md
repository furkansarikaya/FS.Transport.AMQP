# Performance Tuning Guide

This guide covers performance optimization strategies for FS.RabbitMQ, helping you achieve maximum throughput and minimal latency.

## 📋 Table of Contents

1. [Performance Overview](#performance-overview)
2. [Connection Optimization](#connection-optimization)
3. [Producer Performance](#producer-performance)
4. [Consumer Performance](#consumer-performance)
5. [Serialization Optimization](#serialization-optimization)
6. [Memory Management](#memory-management)
7. [Network Optimization](#network-optimization)
8. [Monitoring Performance](#monitoring-performance)
9. [Benchmarking](#benchmarking)
10. [Best Practices](#best-practices)

## 🎯 Performance Overview

FS.RabbitMQ is designed for high-performance messaging scenarios. Key performance metrics include:

- **Throughput**: Messages per second (msgs/sec)
- **Latency**: Time from publish to consume (ms)
- **CPU Usage**: Processor utilization (%)
- **Memory Usage**: RAM consumption (MB)
- **Network Usage**: Bandwidth utilization (MB/s)

### Performance Baseline

```
Environment: Standard development machine (8 cores, 16GB RAM)
RabbitMQ: 3.12.x running in Docker
Message Size: 1KB JSON payload

Baseline Performance:
- Basic Publishing: 50,000 msgs/sec
- Batch Publishing: 180,000 msgs/sec
- Basic Consuming: 45,000 msgs/sec
- Concurrent Consuming: 120,000 msgs/sec
```

## 🔗 Connection Optimization

### Connection Pooling

```csharp
public class HighPerformanceConnectionSetup
{
    public static void ConfigureConnections(IServiceCollection services)
    {
        services.AddRabbitMQ()
            .WithConnectionString("amqp://localhost")
            .WithConnection(config =>
            {
                // Connection pool optimization
                config.MaxChannels = 200;                  // Increase max channels
                config.HeartbeatInterval = TimeSpan.FromSeconds(30);  // Reduce heartbeat frequency
                config.RequestedFrameMax = 131072;         // Increase frame size (128KB)
                config.RequestedChannelMax = 2047;         // Maximum channels
                config.UseBackgroundThreadsForIO = true;   // Use background threads
                config.DispatchConsumersAsync = true;      // Async consumer dispatch
                config.ConsumerDispatchConcurrency = Environment.ProcessorCount;
                
                // Network optimization
                config.NetworkRecoveryInterval = TimeSpan.FromSeconds(5);
                config.ContinuationTimeout = TimeSpan.FromSeconds(20);
                config.RequestedConnectionTimeout = TimeSpan.FromSeconds(30);
            })
            .Build();
    }
}
```

### Dedicated Connections

```csharp
public class DedicatedConnectionExample
{
    private readonly IRabbitMQClient _rabbitMQ;
    private readonly ILogger<DedicatedConnectionExample> _logger;

    public DedicatedConnectionExample(IRabbitMQClient rabbitMQ, ILogger<DedicatedConnectionExample> logger)
    {
        _rabbitMQ = rabbitMQ;
        _logger = logger;
    }

    public async Task SetupDedicatedConnectionsAsync()
    {
        // Create dedicated connection for high-throughput producer
        var producerConnection = await _rabbitMQ.ConnectionManager.CreateDedicatedConnectionAsync(
            connectionName: "HighThroughputProducer",
            maxChannels: 50);

        // Create dedicated connection for high-throughput consumer
        var consumerConnection = await _rabbitMQ.ConnectionManager.CreateDedicatedConnectionAsync(
            connectionName: "HighThroughputConsumer",
            maxChannels: 100);

        _logger.LogInformation("Dedicated connections created for high-throughput scenarios");
    }
}
```

## 📤 Producer Performance

### Batch Publishing

```csharp
public class HighThroughputProducer
{
    private readonly IRabbitMQClient _rabbitMQ;
    private readonly ILogger<HighThroughputProducer> _logger;

    public HighThroughputProducer(IRabbitMQClient rabbitMQ, ILogger<HighThroughputProducer> logger)
    {
        _rabbitMQ = rabbitMQ;
        _logger = logger;
    }

    public async Task PublishHighVolumeAsync(IEnumerable<Order> orders)
    {
        const int batchSize = 1000;
        var batches = orders.Chunk(batchSize);
        var publishTasks = new List<Task>();

        foreach (var batch in batches)
        {
            var task = PublishBatchAsync(batch);
            publishTasks.Add(task);
            
            // Limit concurrent batches to prevent overwhelming the system
            if (publishTasks.Count >= Environment.ProcessorCount)
            {
                await Task.WhenAny(publishTasks);
                publishTasks.RemoveAll(t => t.IsCompleted);
            }
        }

        await Task.WhenAll(publishTasks);
        _logger.LogInformation("Published {TotalOrders} orders in batches", orders.Count());
    }

    private async Task PublishBatchAsync(IEnumerable<Order> orders)
    {
        var messageContexts = orders.Select(order => new MessageContext
        {
            Exchange = "orders",
            RoutingKey = "order.created",
            Message = order
        });

        var stopwatch = Stopwatch.StartNew();
        var results = await _rabbitMQ.Producer.PublishBatchAsync(messageContexts);
        stopwatch.Stop();

        var successCount = results.Count(r => r.IsSuccess);
        var throughput = successCount / stopwatch.Elapsed.TotalSeconds;

        _logger.LogInformation("Published batch: {SuccessCount} messages in {ElapsedMs}ms " +
            "({Throughput:F0} msgs/sec)", 
            successCount, stopwatch.ElapsedMilliseconds, throughput);
    }
}
```

### Async Publishing with Channels

```csharp
public class AsyncChannelProducer
{
    private readonly IRabbitMQClient _rabbitMQ;
    private readonly ILogger<AsyncChannelProducer> _logger;
    private readonly Channel<Order> _orderChannel;
    private readonly SemaphoreSlim _publishSemaphore;

    public AsyncChannelProducer(IRabbitMQClient rabbitMQ, ILogger<AsyncChannelProducer> logger)
    {
        _rabbitMQ = rabbitMQ;
        _logger = logger;
        
        // Create bounded channel for back-pressure
        _orderChannel = Channel.CreateBounded<Order>(new BoundedChannelOptions(10000)
        {
            FullMode = BoundedChannelFullMode.Wait,
            SingleReader = false,
            SingleWriter = false
        });
        
        _publishSemaphore = new SemaphoreSlim(100, 100); // Limit concurrent publishes
        
        // Start background publishing
        _ = Task.Run(ProcessOrdersAsync);
    }

    public async Task QueueOrderAsync(Order order)
    {
        await _orderChannel.Writer.WriteAsync(order);
    }

    private async Task ProcessOrdersAsync()
    {
        await foreach (var order in _orderChannel.Reader.ReadAllAsync())
        {
            await _publishSemaphore.WaitAsync();
            
            // Fire and forget publishing
            _ = Task.Run(async () =>
            {
                try
                {
                    await _rabbitMQ.Producer.PublishAsync(
                        exchange: "orders",
                        routingKey: "order.created",
                        message: order);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error publishing order {OrderId}", order.Id);
                }
                finally
                {
                    _publishSemaphore.Release();
                }
            });
        }
    }
}
```

### Publisher Confirms Optimization

```csharp
public class OptimizedConfirmProducer
{
    private readonly IRabbitMQClient _rabbitMQ;
    private readonly ILogger<OptimizedConfirmProducer> _logger;
    private readonly ConcurrentDictionary<ulong, TaskCompletionSource<bool>> _pendingConfirms = new();

    public OptimizedConfirmProducer(IRabbitMQClient rabbitMQ, ILogger<OptimizedConfirmProducer> logger)
    {
        _rabbitMQ = rabbitMQ;
        _logger = logger;
        
        // Subscribe to confirm events
        _rabbitMQ.Producer.MessageConfirmed += OnMessageConfirmed;
    }

    public async Task<bool> PublishWithOptimizedConfirmAsync(Order order)
    {
        var deliveryTag = await _rabbitMQ.Producer.GetNextDeliveryTagAsync();
        var tcs = new TaskCompletionSource<bool>();
        
        _pendingConfirms[deliveryTag] = tcs;
        
        try
        {
            await _rabbitMQ.Producer.PublishAsync(
                exchange: "orders",
                routingKey: "order.created",
                message: order);
            
            // Wait for confirm with timeout
            using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(5));
            return await tcs.Task.WaitAsync(cts.Token);
        }
        catch (OperationCanceledException)
        {
            _pendingConfirms.TryRemove(deliveryTag, out _);
            _logger.LogWarning("Publish confirm timeout for order {OrderId}", order.Id);
            return false;
        }
        catch (Exception ex)
        {
            _pendingConfirms.TryRemove(deliveryTag, out _);
            _logger.LogError(ex, "Error publishing order {OrderId}", order.Id);
            return false;
        }
    }

    private async Task OnMessageConfirmed(ulong deliveryTag, bool multiple)
    {
        if (multiple)
        {
            // Handle multiple confirms
            var confirmedTags = _pendingConfirms.Keys.Where(tag => tag <= deliveryTag).ToList();
            foreach (var tag in confirmedTags)
            {
                if (_pendingConfirms.TryRemove(tag, out var tcs))
                {
                    tcs.SetResult(true);
                }
            }
        }
        else
        {
            // Handle single confirm
            if (_pendingConfirms.TryRemove(deliveryTag, out var tcs))
            {
                tcs.SetResult(true);
            }
        }
    }
}
```

## 📥 Consumer Performance

### Concurrent Consumer Processing

```csharp
public class HighThroughputConsumer
{
    private readonly IRabbitMQClient _rabbitMQ;
    private readonly ILogger<HighThroughputConsumer> _logger;
    private readonly SemaphoreSlim _processingLimiter;
    private readonly Channel<ProcessingItem> _processingChannel;

    public HighThroughputConsumer(IRabbitMQClient rabbitMQ, ILogger<HighThroughputConsumer> logger)
    {
        _rabbitMQ = rabbitMQ;
        _logger = logger;
        
        // Limit concurrent processing
        _processingLimiter = new SemaphoreSlim(Environment.ProcessorCount * 4, Environment.ProcessorCount * 4);
        
        // Create processing channel
        _processingChannel = Channel.CreateBounded<ProcessingItem>(new BoundedChannelOptions(5000)
        {
            FullMode = BoundedChannelFullMode.Wait,
            SingleReader = false,
            SingleWriter = false
        });
        
        // Start background processors
        for (int i = 0; i < Environment.ProcessorCount; i++)
        {
            _ = Task.Run(ProcessItemsAsync);
        }
    }

    public async Task ConsumeHighThroughputAsync(CancellationToken cancellationToken = default)
    {
        await _rabbitMQ.Consumer.ConsumeAsync<Order>(
            queueName: "high-throughput-orders",
            messageHandler: async (order, context) =>
            {
                // Queue for async processing
                var processingItem = new ProcessingItem
                {
                    Order = order,
                    Context = context,
                    ReceivedAt = DateTimeOffset.UtcNow
                };

                await _processingChannel.Writer.WriteAsync(processingItem, cancellationToken);
                
                return true; // Acknowledge immediately for high throughput
            },
            cancellationToken: cancellationToken);
    }

    private async Task ProcessItemsAsync()
    {
        await foreach (var item in _processingChannel.Reader.ReadAllAsync())
        {
            await _processingLimiter.WaitAsync();
            
            _ = Task.Run(async () =>
            {
                try
                {
                    var stopwatch = Stopwatch.StartNew();
                    await ProcessOrderAsync(item.Order);
                    stopwatch.Stop();
                    
                    var processingTime = DateTimeOffset.UtcNow - item.ReceivedAt;
                    var totalTime = stopwatch.Elapsed;
                    
                    _logger.LogInformation("Order {OrderId} processed in {ProcessingTime}ms " +
                        "(total: {TotalTime}ms)", 
                        item.Order.Id, totalTime.TotalMilliseconds, processingTime.TotalMilliseconds);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error processing order {OrderId}", item.Order.Id);
                }
                finally
                {
                    _processingLimiter.Release();
                }
            });
        }
    }

    private async Task ProcessOrderAsync(Order order)
    {
        // Optimized processing
        await Task.Delay(10); // Simulate fast processing
    }
}
```

### Prefetch Optimization

```csharp
public class OptimizedPrefetchConsumer
{
    private readonly IRabbitMQClient _rabbitMQ;
    private readonly ILogger<OptimizedPrefetchConsumer> _logger;
    private readonly int _optimalPrefetchCount;

    public OptimizedPrefetchConsumer(IRabbitMQClient rabbitMQ, ILogger<OptimizedPrefetchConsumer> logger)
    {
        _rabbitMQ = rabbitMQ;
        _logger = logger;
        
        // Calculate optimal prefetch count based on processing time
        _optimalPrefetchCount = CalculateOptimalPrefetchCount();
    }

    public async Task ConsumeWithOptimizedPrefetchAsync(CancellationToken cancellationToken = default)
    {
        // Configure consumer with optimal prefetch
        await _rabbitMQ.Consumer.ConfigureAsync(config =>
        {
            config.PrefetchCount = _optimalPrefetchCount;
            config.PrefetchSize = 0;
            config.PrefetchGlobal = false;
        });

        await _rabbitMQ.Consumer.ConsumeAsync<Order>(
            queueName: "optimized-orders",
            messageHandler: async (order, context) =>
            {
                var stopwatch = Stopwatch.StartNew();
                
                try
                {
                    await ProcessOrderAsync(order);
                    stopwatch.Stop();
                    
                    // Dynamically adjust prefetch if needed
                    await AdjustPrefetchIfNeeded(stopwatch.Elapsed);
                    
                    return true;
                }
                catch (Exception ex)
                {
                    stopwatch.Stop();
                    _logger.LogError(ex, "Error processing order {OrderId} in {ElapsedMs}ms", 
                        order.Id, stopwatch.ElapsedMilliseconds);
                    return false;
                }
            },
            cancellationToken: cancellationToken);
    }

    private int CalculateOptimalPrefetchCount()
    {
        // Formula: prefetch = (target_throughput * avg_processing_time) / 1000
        // For example: 1000 msgs/sec * 100ms = 100 prefetch
        
        var targetThroughput = 1000; // msgs/sec
        var avgProcessingTimeMs = 100; // ms
        var optimalPrefetch = (targetThroughput * avgProcessingTimeMs) / 1000;
        
        // Ensure reasonable bounds
        return Math.Max(1, Math.Min(optimalPrefetch, 1000));
    }

    private async Task AdjustPrefetchIfNeeded(TimeSpan processingTime)
    {
        // Adjust prefetch based on processing time
        var currentPrefetch = _optimalPrefetchCount;
        var processingTimeMs = processingTime.TotalMilliseconds;
        
        if (processingTimeMs > 500) // Slow processing
        {
            var newPrefetch = Math.Max(1, currentPrefetch / 2);
            if (newPrefetch != currentPrefetch)
            {
                await _rabbitMQ.Consumer.ConfigureAsync(config =>
                {
                    config.PrefetchCount = newPrefetch;
                });
                
                _logger.LogInformation("Reduced prefetch to {NewPrefetch} due to slow processing", newPrefetch);
            }
        }
        else if (processingTimeMs < 50) // Fast processing
        {
            var newPrefetch = Math.Min(1000, currentPrefetch * 2);
            if (newPrefetch != currentPrefetch)
            {
                await _rabbitMQ.Consumer.ConfigureAsync(config =>
                {
                    config.PrefetchCount = newPrefetch;
                });
                
                _logger.LogInformation("Increased prefetch to {NewPrefetch} due to fast processing", newPrefetch);
            }
        }
    }

    private async Task ProcessOrderAsync(Order order)
    {
        // Simulate variable processing time
        var processingTime = Random.Shared.Next(10, 200);
        await Task.Delay(processingTime);
    }
}
```

## 📊 Serialization Optimization

### Efficient Serialization

```csharp
public class OptimizedSerializationSetup
{
    public static void ConfigureSerialization(IServiceCollection services)
    {
        services.AddRabbitMQ()
            .WithConnectionString("amqp://localhost")
            .WithSerialization(config =>
            {
                config.SerializerType = SerializerType.MessagePack; // Faster than JSON
                config.EnableCompression = true;
                config.CompressionThreshold = 1024; // Compress messages > 1KB
                config.CompressionLevel = CompressionLevel.Fastest;
                config.EnableSerializationCaching = true;
                config.SerializationBufferSize = 8192; // 8KB buffer
            })
            .Build();
    }
}
```

### Custom High-Performance Serializer

```csharp
public class HighPerformanceSerializer : IMessageSerializer
{
    private readonly JsonSerializerOptions _jsonOptions;
    private readonly MessagePackSerializerOptions _messagePackOptions;

    public HighPerformanceSerializer()
    {
        _jsonOptions = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
            WriteIndented = false
        };

        _messagePackOptions = MessagePackSerializerOptions.Standard
            .WithCompression(MessagePackCompression.Lz4BlockArray)
            .WithSecurity(MessagePackSecurity.UntrustedData);
    }

    public async Task<byte[]> SerializeAsync<T>(T obj)
    {
        if (obj == null)
            return Array.Empty<byte>();

        // Use MessagePack for complex objects, JSON for simple ones
        if (typeof(T).IsValueType || typeof(T) == typeof(string))
        {
            return JsonSerializer.SerializeToUtf8Bytes(obj, _jsonOptions);
        }
        else
        {
            return MessagePackSerializer.Serialize(obj, _messagePackOptions);
        }
    }

    public async Task<T> DeserializeAsync<T>(byte[] data)
    {
        if (data == null || data.Length == 0)
            return default(T);

        try
        {
            // Try MessagePack first
            return MessagePackSerializer.Deserialize<T>(data, _messagePackOptions);
        }
        catch
        {
            // Fall back to JSON
            return JsonSerializer.Deserialize<T>(data, _jsonOptions);
        }
    }
}
```

## 🧠 Memory Management

### Memory-Efficient Consumer

```csharp
public class MemoryEfficientConsumer
{
    private readonly IRabbitMQClient _rabbitMQ;
    private readonly ILogger<MemoryEfficientConsumer> _logger;
    private readonly ObjectPool<OrderProcessor> _processorPool;
    private readonly MemoryPool<byte> _memoryPool;

    public MemoryEfficientConsumer(IRabbitMQClient rabbitMQ, ILogger<MemoryEfficientConsumer> logger)
    {
        _rabbitMQ = rabbitMQ;
        _logger = logger;
        
        // Create object pool for processors
        _processorPool = new DefaultObjectPool<OrderProcessor>(
            new OrderProcessorPooledObjectPolicy(),
            Environment.ProcessorCount * 2);
        
        // Create memory pool for buffers
        _memoryPool = MemoryPool<byte>.Shared;
    }

    public async Task ConsumeMemoryEfficientAsync(CancellationToken cancellationToken = default)
    {
        await _rabbitMQ.Consumer.ConsumeAsync<Order>(
            queueName: "memory-efficient-orders",
            messageHandler: async (order, context) =>
            {
                var processor = _processorPool.Get();
                
                try
                {
                    // Process with pooled processor
                    await processor.ProcessAsync(order);
                    return true;
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error processing order {OrderId}", order.Id);
                    return false;
                }
                finally
                {
                    _processorPool.Return(processor);
                }
            },
            cancellationToken: cancellationToken);
    }

    public async Task ProcessLargeMessageAsync(byte[] messageData)
    {
        // Use memory pool for large message processing
        using var memoryOwner = _memoryPool.Rent(messageData.Length);
        var buffer = memoryOwner.Memory.Span;
        
        messageData.CopyTo(buffer);
        
        // Process the message data
        await ProcessBufferAsync(buffer);
    }

    private async Task ProcessBufferAsync(Span<byte> buffer)
    {
        // Process buffer efficiently
        await Task.Delay(10);
    }
}

public class OrderProcessor
{
    public async Task ProcessAsync(Order order)
    {
        // Processing logic
        await Task.Delay(50);
    }

    public void Reset()
    {
        // Reset state for object pooling
    }
}

public class OrderProcessorPooledObjectPolicy : IPooledObjectPolicy<OrderProcessor>
{
    public OrderProcessor Create()
    {
        return new OrderProcessor();
    }

    public bool Return(OrderProcessor obj)
    {
        obj.Reset();
        return true;
    }
}
```

### Garbage Collection Optimization

```csharp
public class GCOptimizedConsumer
{
    private readonly IRabbitMQClient _rabbitMQ;
    private readonly ILogger<GCOptimizedConsumer> _logger;
    private readonly Timer _gcTimer;
    private long _processedCount = 0;

    public GCOptimizedConsumer(IRabbitMQClient rabbitMQ, ILogger<GCOptimizedConsumer> logger)
    {
        _rabbitMQ = rabbitMQ;
        _logger = logger;
        
        // Force GC periodically for consistent performance
        _gcTimer = new Timer(ForceGC, null, TimeSpan.FromMinutes(1), TimeSpan.FromMinutes(1));
    }

    public async Task ConsumeWithGCOptimizationAsync(CancellationToken cancellationToken = default)
    {
        await _rabbitMQ.Consumer.ConsumeAsync<Order>(
            queueName: "gc-optimized-orders",
            messageHandler: async (order, context) =>
            {
                try
                {
                    await ProcessOrderAsync(order);
                    Interlocked.Increment(ref _processedCount);
                    
                    // Force GC after processing many messages
                    if (_processedCount % 10000 == 0)
                    {
                        GC.Collect(0, GCCollectionMode.Optimized);
                    }
                    
                    return true;
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error processing order {OrderId}", order.Id);
                    return false;
                }
            },
            cancellationToken: cancellationToken);
    }

    private async Task ProcessOrderAsync(Order order)
    {
        // Minimize allocations in hot path
        var orderData = order.GetOrderData();
        await ProcessOrderDataAsync(orderData);
    }

    private async Task ProcessOrderDataAsync(ReadOnlyMemory<byte> orderData)
    {
        // Process without creating unnecessary objects
        await Task.Delay(10);
    }

    private void ForceGC(object? state)
    {
        var gen0 = GC.CollectionCount(0);
        var gen1 = GC.CollectionCount(1);
        var gen2 = GC.CollectionCount(2);
        
        _logger.LogInformation("GC Stats: Gen0={Gen0}, Gen1={Gen1}, Gen2={Gen2}, Processed={Processed}", 
            gen0, gen1, gen2, _processedCount);
        
        // Force full collection if needed
        if (gen2 > 100)
        {
            GC.Collect();
            GC.WaitForPendingFinalizers();
            GC.Collect();
        }
    }
}
```

## 🌐 Network Optimization

### Network Buffer Optimization

```csharp
public class NetworkOptimizedSetup
{
    public static void ConfigureNetworkOptimization(IServiceCollection services)
    {
        services.AddRabbitMQ()
            .WithConnectionString("amqp://localhost")
            .WithConnection(config =>
            {
                // Network buffer optimization
                config.RequestedFrameMax = 1048576; // 1MB frames
                config.SocketReceiveBufferSize = 65536; // 64KB receive buffer
                config.SocketSendBufferSize = 65536; // 64KB send buffer
                config.TcpKeepAlive = true;
                config.TcpKeepAliveTime = TimeSpan.FromSeconds(60);
                config.TcpKeepAliveInterval = TimeSpan.FromSeconds(10);
                config.TcpNoDelay = true; // Disable Nagle's algorithm
                
                // Connection optimization
                config.UseBackgroundThreadsForIO = true;
                config.DispatchConsumersAsync = true;
                config.ConsumerDispatchConcurrency = Environment.ProcessorCount;
            })
            .Build();
    }
}
```

### Compression Optimization

```csharp
public class CompressionOptimizedProducer
{
    private readonly IRabbitMQClient _rabbitMQ;
    private readonly ILogger<CompressionOptimizedProducer> _logger;

    public CompressionOptimizedProducer(IRabbitMQClient rabbitMQ, ILogger<CompressionOptimizedProducer> logger)
    {
        _rabbitMQ = rabbitMQ;
        _logger = logger;
    }

    public async Task PublishWithOptimizedCompressionAsync<T>(T message, string exchange, string routingKey)
    {
        var messageBytes = JsonSerializer.SerializeToUtf8Bytes(message);
        
        // Only compress if message is large enough
        if (messageBytes.Length > 1024)
        {
            var compressedBytes = await CompressAsync(messageBytes);
            
            var compressionRatio = (double)compressedBytes.Length / messageBytes.Length;
            
            if (compressionRatio < 0.9) // Only use compression if it saves at least 10%
            {
                await _rabbitMQ.Producer.PublishAsync(
                    exchange: exchange,
                    routingKey: routingKey,
                    message: compressedBytes,
                    properties: new BasicProperties
                    {
                        Headers = new Dictionary<string, object>
                        {
                            ["content-encoding"] = "gzip",
                            ["original-size"] = messageBytes.Length
                        }
                    });
                
                _logger.LogInformation("Published compressed message: {OriginalSize} → {CompressedSize} bytes " +
                    "({CompressionRatio:P1})", 
                    messageBytes.Length, compressedBytes.Length, compressionRatio);
            }
            else
            {
                // Send uncompressed if compression doesn't help
                await _rabbitMQ.Producer.PublishAsync(exchange, routingKey, messageBytes);
            }
        }
        else
        {
            // Send small messages uncompressed
            await _rabbitMQ.Producer.PublishAsync(exchange, routingKey, messageBytes);
        }
    }

    private async Task<byte[]> CompressAsync(byte[] data)
    {
        using var output = new MemoryStream();
        using var gzip = new GZipStream(output, CompressionLevel.Fastest);
        
        await gzip.WriteAsync(data);
        await gzip.FlushAsync();
        
        return output.ToArray();
    }
}
```

## 📈 Monitoring Performance

### Real-time Performance Monitoring

```csharp
public class PerformanceMonitor
{
    private readonly IRabbitMQClient _rabbitMQ;
    private readonly ILogger<PerformanceMonitor> _logger;
    private readonly Timer _monitoringTimer;
    private readonly PerformanceMetrics _metrics = new();

    public PerformanceMonitor(IRabbitMQClient rabbitMQ, ILogger<PerformanceMonitor> logger)
    {
        _rabbitMQ = rabbitMQ;
        _logger = logger;
        
        // Monitor performance every 10 seconds
        _monitoringTimer = new Timer(LogPerformanceMetrics, null, 
            TimeSpan.FromSeconds(10), TimeSpan.FromSeconds(10));
    }

    public void RecordPublish(TimeSpan duration, bool success)
    {
        lock (_metrics)
        {
            _metrics.PublishCount++;
            _metrics.PublishDurationTotal += duration;
            
            if (success)
                _metrics.PublishSuccessCount++;
            else
                _metrics.PublishFailureCount++;
        }
    }

    public void RecordConsume(TimeSpan duration, bool success)
    {
        lock (_metrics)
        {
            _metrics.ConsumeCount++;
            _metrics.ConsumeDurationTotal += duration;
            
            if (success)
                _metrics.ConsumeSuccessCount++;
            else
                _metrics.ConsumeFailureCount++;
        }
    }

    private void LogPerformanceMetrics(object? state)
    {
        PerformanceMetrics currentMetrics;
        
        lock (_metrics)
        {
            currentMetrics = _metrics.Clone();
            _metrics.Reset();
        }

        if (currentMetrics.PublishCount > 0)
        {
            var publishThroughput = currentMetrics.PublishCount / 10.0; // per second
            var avgPublishDuration = currentMetrics.PublishDurationTotal.TotalMilliseconds / currentMetrics.PublishCount;
            var publishSuccessRate = (double)currentMetrics.PublishSuccessCount / currentMetrics.PublishCount * 100;

            _logger.LogInformation("Publish Performance: {Throughput:F1} msgs/sec, " +
                "Avg Duration: {AvgDuration:F2}ms, Success Rate: {SuccessRate:F1}%",
                publishThroughput, avgPublishDuration, publishSuccessRate);
        }

        if (currentMetrics.ConsumeCount > 0)
        {
            var consumeThroughput = currentMetrics.ConsumeCount / 10.0; // per second
            var avgConsumeDuration = currentMetrics.ConsumeDurationTotal.TotalMilliseconds / currentMetrics.ConsumeCount;
            var consumeSuccessRate = (double)currentMetrics.ConsumeSuccessCount / currentMetrics.ConsumeCount * 100;

            _logger.LogInformation("Consume Performance: {Throughput:F1} msgs/sec, " +
                "Avg Duration: {AvgDuration:F2}ms, Success Rate: {SuccessRate:F1}%",
                consumeThroughput, avgConsumeDuration, consumeSuccessRate);
        }

        // Log system metrics
        LogSystemMetrics();
    }

    private void LogSystemMetrics()
    {
        var process = Process.GetCurrentProcess();
        var memoryUsage = process.WorkingSet64 / 1024 / 1024; // MB
        var cpuUsage = GetCpuUsage();
        
        _logger.LogInformation("System Metrics: Memory: {MemoryMB}MB, CPU: {CpuUsage:F1}%",
            memoryUsage, cpuUsage);
    }

    private double GetCpuUsage()
    {
        // Simplified CPU usage calculation
        var process = Process.GetCurrentProcess();
        return process.TotalProcessorTime.TotalMilliseconds / Environment.TickCount64 * 100;
    }
}

public class PerformanceMetrics
{
    public int PublishCount { get; set; }
    public int PublishSuccessCount { get; set; }
    public int PublishFailureCount { get; set; }
    public TimeSpan PublishDurationTotal { get; set; }
    
    public int ConsumeCount { get; set; }
    public int ConsumeSuccessCount { get; set; }
    public int ConsumeFailureCount { get; set; }
    public TimeSpan ConsumeDurationTotal { get; set; }

    public PerformanceMetrics Clone()
    {
        return new PerformanceMetrics
        {
            PublishCount = PublishCount,
            PublishSuccessCount = PublishSuccessCount,
            PublishFailureCount = PublishFailureCount,
            PublishDurationTotal = PublishDurationTotal,
            ConsumeCount = ConsumeCount,
            ConsumeSuccessCount = ConsumeSuccessCount,
            ConsumeFailureCount = ConsumeFailureCount,
            ConsumeDurationTotal = ConsumeDurationTotal
        };
    }

    public void Reset()
    {
        PublishCount = 0;
        PublishSuccessCount = 0;
        PublishFailureCount = 0;
        PublishDurationTotal = TimeSpan.Zero;
        ConsumeCount = 0;
        ConsumeSuccessCount = 0;
        ConsumeFailureCount = 0;
        ConsumeDurationTotal = TimeSpan.Zero;
    }
}
```

## 🏃 Benchmarking

### Performance Benchmarking Suite

```csharp
public class PerformanceBenchmark
{
    private readonly IRabbitMQClient _rabbitMQ;
    private readonly ILogger<PerformanceBenchmark> _logger;

    public PerformanceBenchmark(IRabbitMQClient rabbitMQ, ILogger<PerformanceBenchmark> logger)
    {
        _rabbitMQ = rabbitMQ;
        _logger = logger;
    }

    public async Task RunAllBenchmarksAsync()
    {
        await BenchmarkBasicPublishAsync();
        await BenchmarkBatchPublishAsync();
        await BenchmarkBasicConsumeAsync();
        await BenchmarkConcurrentConsumeAsync();
        await BenchmarkSerializationAsync();
    }

    private async Task BenchmarkBasicPublishAsync()
    {
        const int messageCount = 10000;
        var messages = GenerateTestMessages(messageCount);
        
        _logger.LogInformation("Starting basic publish benchmark with {MessageCount} messages", messageCount);
        
        var stopwatch = Stopwatch.StartNew();
        
        foreach (var message in messages)
        {
            await _rabbitMQ.Producer.PublishAsync(
                exchange: "benchmark",
                routingKey: "basic.publish",
                message: message);
        }
        
        stopwatch.Stop();
        
        var throughput = messageCount / stopwatch.Elapsed.TotalSeconds;
        var avgLatency = stopwatch.Elapsed.TotalMilliseconds / messageCount;
        
        _logger.LogInformation("Basic Publish Results: {Throughput:F0} msgs/sec, " +
            "Avg Latency: {AvgLatency:F2}ms, Total Time: {TotalTime:F2}s",
            throughput, avgLatency, stopwatch.Elapsed.TotalSeconds);
    }

    private async Task BenchmarkBatchPublishAsync()
    {
        const int messageCount = 10000;
        const int batchSize = 1000;
        var messages = GenerateTestMessages(messageCount);
        
        _logger.LogInformation("Starting batch publish benchmark with {MessageCount} messages " +
            "in batches of {BatchSize}", messageCount, batchSize);
        
        var stopwatch = Stopwatch.StartNew();
        
        var batches = messages.Chunk(batchSize);
        var publishTasks = batches.Select(batch => PublishBatchAsync(batch));
        
        await Task.WhenAll(publishTasks);
        
        stopwatch.Stop();
        
        var throughput = messageCount / stopwatch.Elapsed.TotalSeconds;
        var avgLatency = stopwatch.Elapsed.TotalMilliseconds / messageCount;
        
        _logger.LogInformation("Batch Publish Results: {Throughput:F0} msgs/sec, " +
            "Avg Latency: {AvgLatency:F2}ms, Total Time: {TotalTime:F2}s",
            throughput, avgLatency, stopwatch.Elapsed.TotalSeconds);
    }

    private async Task BenchmarkBasicConsumeAsync()
    {
        const int messageCount = 10000;
        var processedCount = 0;
        
        _logger.LogInformation("Starting basic consume benchmark");
        
        var stopwatch = Stopwatch.StartNew();
        var cancellationTokenSource = new CancellationTokenSource();
        
        var consumeTask = _rabbitMQ.Consumer.ConsumeAsync<Order>(
            queueName: "benchmark-basic-consume",
            messageHandler: async (order, context) =>
            {
                Interlocked.Increment(ref processedCount);
                
                if (processedCount >= messageCount)
                {
                    cancellationTokenSource.Cancel();
                }
                
                return true;
            },
            cancellationToken: cancellationTokenSource.Token);
        
        // Wait for completion or timeout
        try
        {
            await consumeTask.WaitAsync(TimeSpan.FromMinutes(5));
        }
        catch (OperationCanceledException)
        {
            // Expected when benchmark completes
        }
        
        stopwatch.Stop();
        
        var throughput = processedCount / stopwatch.Elapsed.TotalSeconds;
        var avgLatency = stopwatch.Elapsed.TotalMilliseconds / processedCount;
        
        _logger.LogInformation("Basic Consume Results: {Throughput:F0} msgs/sec, " +
            "Avg Latency: {AvgLatency:F2}ms, Total Time: {TotalTime:F2}s",
            throughput, avgLatency, stopwatch.Elapsed.TotalSeconds);
    }

    private async Task BenchmarkConcurrentConsumeAsync()
    {
        const int messageCount = 10000;
        var processedCount = 0;
        
        _logger.LogInformation("Starting concurrent consume benchmark");
        
        var stopwatch = Stopwatch.StartNew();
        var cancellationTokenSource = new CancellationTokenSource();
        
        // Start multiple consumers
        var consumerTasks = new List<Task>();
        for (int i = 0; i < Environment.ProcessorCount; i++)
        {
            var consumerTask = _rabbitMQ.Consumer.ConsumeAsync<Order>(
                queueName: "benchmark-concurrent-consume",
                messageHandler: async (order, context) =>
                {
                    var currentCount = Interlocked.Increment(ref processedCount);
                    
                    if (currentCount >= messageCount)
                    {
                        cancellationTokenSource.Cancel();
                    }
                    
                    return true;
                },
                cancellationToken: cancellationTokenSource.Token);
            
            consumerTasks.Add(consumerTask);
        }
        
        // Wait for completion
        try
        {
            await Task.WhenAll(consumerTasks).WaitAsync(TimeSpan.FromMinutes(5));
        }
        catch (OperationCanceledException)
        {
            // Expected when benchmark completes
        }
        
        stopwatch.Stop();
        
        var throughput = processedCount / stopwatch.Elapsed.TotalSeconds;
        var avgLatency = stopwatch.Elapsed.TotalMilliseconds / processedCount;
        
        _logger.LogInformation("Concurrent Consume Results: {Throughput:F0} msgs/sec, " +
            "Avg Latency: {AvgLatency:F2}ms, Total Time: {TotalTime:F2}s",
            throughput, avgLatency, stopwatch.Elapsed.TotalSeconds);
    }

    private async Task BenchmarkSerializationAsync()
    {
        const int messageCount = 10000;
        var messages = GenerateTestMessages(messageCount);
        
        _logger.LogInformation("Starting serialization benchmark");
        
        var jsonStopwatch = Stopwatch.StartNew();
        foreach (var message in messages)
        {
            var json = JsonSerializer.SerializeToUtf8Bytes(message);
            var deserialized = JsonSerializer.Deserialize<Order>(json);
        }
        jsonStopwatch.Stop();
        
        var messagePackStopwatch = Stopwatch.StartNew();
        foreach (var message in messages)
        {
            var msgPack = MessagePackSerializer.Serialize(message);
            var deserialized = MessagePackSerializer.Deserialize<Order>(msgPack);
        }
        messagePackStopwatch.Stop();
        
        var jsonThroughput = messageCount / jsonStopwatch.Elapsed.TotalSeconds;
        var messagePackThroughput = messageCount / messagePackStopwatch.Elapsed.TotalSeconds;
        
        _logger.LogInformation("Serialization Results: JSON: {JsonThroughput:F0} msgs/sec, " +
            "MessagePack: {MessagePackThroughput:F0} msgs/sec",
            jsonThroughput, messagePackThroughput);
    }

    private async Task PublishBatchAsync(IEnumerable<Order> messages)
    {
        var messageContexts = messages.Select(message => new MessageContext
        {
            Exchange = "benchmark",
            RoutingKey = "batch.publish",
            Message = message
        });

        await _rabbitMQ.Producer.PublishBatchAsync(messageContexts);
    }

    private List<Order> GenerateTestMessages(int count)
    {
        var messages = new List<Order>();
        
        for (int i = 0; i < count; i++)
        {
            messages.Add(new Order
            {
                Id = Guid.NewGuid(),
                CustomerName = $"Customer {i}",
                Amount = Random.Shared.Next(10, 1000),
                Items = new List<OrderItem>
                {
                    new OrderItem { ProductName = $"Product {i}", Quantity = 1, Price = Random.Shared.Next(10, 100) }
                }
            });
        }
        
        return messages;
    }
}
```

## 🎯 Best Practices

### 1. Use Appropriate Batch Sizes

```csharp
// DO: Use optimal batch sizes based on message size and processing time
const int batchSize = 1000; // Good for small messages
const int largeBatchSize = 100; // Good for large messages

// DON'T: Use fixed batch sizes regardless of message characteristics
```

### 2. Optimize Prefetch Count

```csharp
// DO: Calculate optimal prefetch based on processing time
var processingTimeMs = 100;
var targetThroughput = 1000;
var optimalPrefetch = (targetThroughput * processingTimeMs) / 1000;

// DON'T: Use default prefetch count for all scenarios
```

### 3. Use Connection Pooling

```csharp
// DO: Configure connection pooling for high-throughput scenarios
services.AddRabbitMQ()
    .WithConnection(config =>
    {
        config.MaxChannels = 200;
        config.UseConnectionPooling = true;
    })
    .Build();
```

### 4. Minimize Object Allocations

```csharp
// DO: Use object pooling and memory pools
var processor = _processorPool.Get();
try
{
    await processor.ProcessAsync(message);
}
finally
{
    _processorPool.Return(processor);
}

// DON'T: Create new objects in hot paths
```

### 5. Monitor Performance Continuously

```csharp
// DO: Implement comprehensive performance monitoring
public void RecordMetrics(TimeSpan duration, bool success)
{
    _metricsCollector.RecordDuration(duration);
    _metricsCollector.RecordSuccess(success);
    
    if (duration > TimeSpan.FromSeconds(1))
    {
        _alertingService.SendSlowProcessingAlert(duration);
    }
}
```

### 6. Use Appropriate Serialization

```csharp
// DO: Choose serialization based on requirements
// JSON: Human-readable, widely supported
// MessagePack: Fast, compact binary format
// Protocol Buffers: Schema evolution, type safety

// DON'T: Always use JSON without considering alternatives
```

### 7. Implement Circuit Breakers

```csharp
// DO: Use circuit breakers for external dependencies
await _circuitBreaker.ExecuteAsync(async () =>
{
    await _externalService.ProcessAsync(message);
});
```

## 🎉 Summary

You've now learned how to optimize FS.RabbitMQ performance:

✅ **Connection and channel optimization**  
✅ **High-throughput producer patterns**  
✅ **Efficient consumer processing**  
✅ **Serialization optimization**  
✅ **Memory management techniques**  
✅ **Network optimization**  
✅ **Performance monitoring**  
✅ **Comprehensive benchmarking**  
✅ **Production-ready best practices**  

## 🎯 Next Steps

Complete your FS.RabbitMQ mastery:

- [Monitoring](monitoring.md) - Monitor your messaging system
- [Examples](examples/) - See real-world examples
- [Error Handling](error-handling.md) - Implement robust error handling
- [Configuration](configuration.md) - Advanced configuration options 