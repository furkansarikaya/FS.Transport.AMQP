# High-Throughput Processing Example

**Difficulty**: 🔴 Advanced  
**Focus**: Optimized for performance  
**Time**: 30 minutes

This example demonstrates how to implement high-throughput message processing using FS.StreamFlow. It covers batch publishing, consumer concurrency, error handling, and monitoring.

## 📋 What You'll Learn
- Batch publishing for high throughput
- Optimizing consumer concurrency
- Error handling for high-load scenarios
- Monitoring throughput and performance

## 🛠️ Setup

### Prerequisites
- .NET 9 SDK
- RabbitMQ server running on localhost
- Basic C# knowledge

### Project Structure
```
HighThroughputProcessing/
├── Program.cs
├── Services/
│   ├── BatchProducer.cs
│   └── HighThroughputConsumer.cs
└── HighThroughputProcessing.csproj
```

## 🏗️ Implementation

### 1. Batch Producer

```csharp
// Services/BatchProducer.cs
using FS.StreamFlow.Core.Features.Messaging.Interfaces;
using Microsoft.Extensions.Logging;

public class BatchProducer
{
    private readonly IStreamFlowClient _streamFlow;
    private readonly ILogger<BatchProducer> _logger;

    public BatchProducer(IStreamFlowClient streamFlow, ILogger<BatchProducer> logger)
    {
        _streamFlow = streamFlow;
        _logger = logger;
    }

    public async Task PublishBatchAsync(IEnumerable<ProcessingMessage> messages)
    {
        // Initialize the client first
        await _streamFlow.InitializeAsync();
        
        var batchSize = 100;
        var batches = messages.Chunk(batchSize);
        
        foreach (var batch in batches)
        {
            var tasks = batch.Select(message => 
                _streamFlow.Producer.Message(message)
                    .WithExchange("high-throughput-exchange")
                    .WithRoutingKey("batch")
                    .WithDeliveryMode(DeliveryMode.Persistent)
                    .WithPriority(5)
                    .PublishAsync());
            
            await Task.WhenAll(tasks);
            
            _logger.LogInformation("Batch of {Count} messages published", batch.Length);
        }
    }

    public async Task PublishHighVolumeAsync(int messageCount)
    {
        // Initialize the client first
        await _streamFlow.InitializeAsync();
        
        var messages = Enumerable.Range(0, messageCount)
            .Select(i => new ProcessingMessage
            {
                Id = Guid.NewGuid().ToString(),
                Content = $"Message {i}",
                Timestamp = DateTime.UtcNow,
                Priority = i % 10
            });

        await PublishBatchAsync(messages);
    }
}

public class ProcessingMessage
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public string Content { get; set; } = string.Empty;
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;
    public int Priority { get; set; }
    public Dictionary<string, object> Metadata { get; set; } = new();
}
```

### 2. High-Throughput Consumer

```csharp
// Services/HighThroughputConsumer.cs
using FS.StreamFlow.Core.Features.Messaging.Interfaces;
using Microsoft.Extensions.Logging;

public class HighThroughputConsumer
{
    private readonly IStreamFlowClient _streamFlow;
    private readonly ILogger<HighThroughputConsumer> _logger;
    private readonly SemaphoreSlim _semaphore;

    public HighThroughputConsumer(IStreamFlowClient streamFlow, ILogger<HighThroughputConsumer> logger)
    {
        _streamFlow = streamFlow;
        _logger = logger;
        _semaphore = new SemaphoreSlim(Environment.ProcessorCount * 2); // Optimize for CPU cores
    }

    public async Task StartConsumingAsync(CancellationToken cancellationToken)
    {
        // Initialize the client first
        await _streamFlow.InitializeAsync();
        
        await _streamFlow.Consumer.Queue<ProcessingMessage>("high-throughput-queue")
            .WithConcurrency(Environment.ProcessorCount * 2) // High concurrency
            .WithPrefetchCount(1000) // High prefetch for throughput
            .WithAutoAck(false) // Manual acknowledgment for control
            .WithErrorHandler(async (exception, context) =>
            {
                _logger.LogError(exception, "Error processing message {MessageId}", context.MessageId);
                
                // For high-throughput scenarios, we might want to reject quickly
                if (exception is InvalidOperationException)
                    return false; // Send to DLQ immediately
                
                return true; // Retry for other errors
            })
            .WithRetryPolicy(new RetryPolicySettings
            {
                UseExponentialBackoff = true,
                MaxRetryAttempts = 2, // Fewer retries for high throughput
                InitialRetryDelay = TimeSpan.FromMilliseconds(100)
            })
            .ConsumeAsync(async (message, context) =>
            {
                await _semaphore.WaitAsync(cancellationToken);
                
                try
                {
                    // Process message with high throughput
                    await ProcessMessageAsync(message, context);
                    return true; // Acknowledge
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Failed to process message {MessageId}", message.Id);
                    return false; // Reject
                }
                finally
                {
                    _semaphore.Release();
                }
            });
    }

    private async Task ProcessMessageAsync(ProcessingMessage message, MessageContext context)
    {
        // Simulate fast processing
        await Task.Delay(5, CancellationToken.None);
        
        // Process based on priority
        if (message.Priority > 5)
        {
            await ProcessHighPriorityMessageAsync(message);
        }
        else
        {
            await ProcessStandardMessageAsync(message);
        }
        
        _logger.LogDebug("Processed message {MessageId} with priority {Priority}", 
            message.Id, message.Priority);
    }

    private async Task ProcessHighPriorityMessageAsync(ProcessingMessage message)
    {
        // High priority processing logic
        await Task.Delay(2, CancellationToken.None);
    }

    private async Task ProcessStandardMessageAsync(ProcessingMessage message)
    {
        // Standard processing logic
        await Task.Delay(5, CancellationToken.None);
    }

    public void Dispose()
    {
        _semaphore?.Dispose();
    }
}
```

### 3. Program Setup

```csharp
// Program.cs
using FS.StreamFlow.RabbitMQ.DependencyInjection;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

var builder = WebApplication.CreateBuilder(args);

// Add FS.StreamFlow with RabbitMQ optimized for high throughput
builder.Services.AddRabbitMQStreamFlow(options =>
{
    // Client configuration
    options.ClientConfiguration.ClientName = "High-Throughput Processing";
    options.ClientConfiguration.EnableAutoRecovery = true;
    options.ClientConfiguration.EnableHeartbeat = true;
    options.ClientConfiguration.HeartbeatInterval = TimeSpan.FromSeconds(30);
    
    // Connection settings
    options.ConnectionSettings.Host = "localhost";
    options.ConnectionSettings.Port = 5672;
    options.ConnectionSettings.Username = "guest";
    options.ConnectionSettings.Password = "guest";
    options.ConnectionSettings.VirtualHost = "/";
    options.ConnectionSettings.ConnectionTimeout = TimeSpan.FromSeconds(30);
    
    // Producer settings optimized for high throughput
    options.ProducerSettings.EnablePublisherConfirms = true;
    options.ProducerSettings.ConfirmationTimeout = TimeSpan.FromSeconds(5);
    options.ProducerSettings.MaxConcurrentPublishes = 1000; // High concurrency
    
    // Consumer settings optimized for high throughput
    options.ConsumerSettings.PrefetchCount = 1000; // High prefetch
    options.ConsumerSettings.AutoAcknowledge = false; // Manual ack for control
    options.ConsumerSettings.MaxConcurrentConsumers = Environment.ProcessorCount * 2;
    
    // Error handling settings
    options.ErrorHandlingSettings.MaxRetries = 2; // Fewer retries for throughput
    options.ErrorHandlingSettings.RetryDelay = TimeSpan.FromMilliseconds(100);
    options.ErrorHandlingSettings.UseExponentialBackoff = true;
    options.ErrorHandlingSettings.EnableDeadLetterQueue = true;
    
    // Dead letter settings
    options.DeadLetterSettings.ExchangeName = "dlx";
    options.DeadLetterSettings.RoutingKey = "high-throughput.failed";
});

// Register services
builder.Services.AddScoped<BatchProducer>();
builder.Services.AddScoped<HighThroughputConsumer>();

var app = builder.Build();

// Initialize the client
var streamFlow = app.Services.GetRequiredService<IStreamFlowClient>();
await streamFlow.InitializeAsync();

// Setup infrastructure optimized for high throughput
await streamFlow.ExchangeManager.Exchange("high-throughput-exchange")
    .AsTopic()
    .WithDurable(true)
    .DeclareAsync();

await streamFlow.ExchangeManager.Exchange("dlx")
    .AsTopic()
    .WithDurable(true)
    .DeclareAsync();

await streamFlow.QueueManager.Queue("high-throughput-queue")
    .WithDurable(true)
    .WithDeadLetterExchange("dlx")
    .WithDeadLetterRoutingKey("high-throughput.failed")
    .WithMessageTtl(TimeSpan.FromHours(1)) // TTL for cleanup
    .WithMaxLength(100000) // High capacity
    .WithMaxLengthBytes(1024 * 1024 * 100) // 100MB
    .BindToExchange("high-throughput-exchange", "batch")
    .DeclareAsync();

await streamFlow.QueueManager.Queue("dead-letter-queue")
    .WithDurable(true)
    .BindToExchange("dlx", "high-throughput.failed")
    .DeclareAsync();

// Start high-throughput consumer
var consumer = app.Services.GetRequiredService<HighThroughputConsumer>();
await consumer.StartConsumingAsync(CancellationToken.None);

// Start dead letter processor
await streamFlow.Consumer.Queue<DeadLetterMessage>("dead-letter-queue")
    .WithConcurrency(1)
    .WithPrefetchCount(10)
    .ConsumeAsync(async (deadLetterMessage, context) =>
    {
        var logger = app.Services.GetRequiredService<ILogger<Program>>();
        logger.LogWarning("Processing dead letter message {MessageId}: {Reason}", 
            deadLetterMessage.MessageId, deadLetterMessage.Reason);
        return true;
    });

// Start the application
app.Run();
```

## 🚀 Performance Optimization

### Batch Publishing Strategy
```csharp
// Optimize batch size based on message size and network conditions
public async Task PublishOptimizedBatchAsync(IEnumerable<ProcessingMessage> messages)
{
    await _streamFlow.InitializeAsync();
    
    // Dynamic batch sizing based on message size
    var batchSize = messages.FirstOrDefault()?.Content.Length > 1000 ? 50 : 100;
    var batches = messages.Chunk(batchSize);
    
    var semaphore = new SemaphoreSlim(10); // Limit concurrent batches
    
    var tasks = batches.Select(async batch =>
    {
        await semaphore.WaitAsync();
        try
        {
            var publishTasks = batch.Select(message => 
                _streamFlow.Producer.Message(message)
                    .WithExchange("high-throughput-exchange")
                    .WithRoutingKey("batch")
                    .WithDeliveryMode(DeliveryMode.Persistent)
                    .WithPriority(message.Priority)
                    .PublishAsync());
            
            await Task.WhenAll(publishTasks);
        }
        finally
        {
            semaphore.Release();
        }
    });
    
    await Task.WhenAll(tasks);
}
```

### Consumer Optimization
```csharp
// Optimize consumer for maximum throughput
public async Task StartOptimizedConsumingAsync(CancellationToken cancellationToken)
{
    await _streamFlow.InitializeAsync();
    
    // Use multiple consumers for different priority levels
    var highPriorityConsumer = _streamFlow.Consumer.Queue<ProcessingMessage>("high-throughput-queue")
        .WithConcurrency(Environment.ProcessorCount)
        .WithPrefetchCount(500)
        .WithConsumerTag("high-priority-consumer")
        .ConsumeAsync(async (message, context) =>
        {
            if (message.Priority > 5)
            {
                await ProcessHighPriorityMessageAsync(message, context);
                return true;
            }
            return false; // Let other consumer handle
        });
    
    var standardConsumer = _streamFlow.Consumer.Queue<ProcessingMessage>("high-throughput-queue")
        .WithConcurrency(Environment.ProcessorCount * 2)
        .WithPrefetchCount(1000)
        .WithConsumerTag("standard-consumer")
        .ConsumeAsync(async (message, context) =>
        {
            if (message.Priority <= 5)
            {
                await ProcessStandardMessageAsync(message, context);
                return true;
            }
            return false; // Let other consumer handle
        });
    
    await Task.WhenAll(highPriorityConsumer, standardConsumer);
}
```

## 🛡️ Error Handling

### High-Throughput Error Strategies
- **Quick Rejection**: Invalid messages sent to DLQ immediately
- **Limited Retries**: Fewer retry attempts to maintain throughput
- **Circuit Breaker**: Stop processing when error rate is high
- **Dead Letter Queue**: Failed messages stored for later analysis

### Error Handling Configuration
```csharp
.WithErrorHandler(async (exception, context) =>
{
    // Quick rejection for known bad messages
    if (exception is ValidationException || exception is FormatException)
        return false;
    
    // Limited retries for transient errors
    if (exception is TimeoutException && context.AttemptNumber < 2)
        return true;
    
    // Send to DLQ for other errors
    return false;
})
```

## 📊 Monitoring

### Throughput Monitoring
```csharp
// Monitor throughput metrics
var producerStats = await streamFlow.Producer.GetStatisticsAsync();
var consumerStats = await streamFlow.Consumer.GetStatisticsAsync();

Console.WriteLine($"Messages published: {producerStats.MessagesPublished}");
Console.WriteLine($"Messages consumed: {consumerStats.MessagesConsumed}");
Console.WriteLine($"Average publish rate: {producerStats.AveragePublishRate} msg/sec");
Console.WriteLine($"Average consume rate: {consumerStats.AverageConsumeRate} msg/sec");
```

### Performance Metrics
- **Message Rate**: Messages per second
- **Latency**: End-to-end processing time
- **Throughput**: Total messages processed
- **Error Rate**: Failed messages percentage
- **Queue Depth**: Messages waiting in queue

### RabbitMQ Management
- **Management UI**: http://localhost:15672
- **Queue Monitoring**: Track queue depth and processing rates
- **Connection Monitoring**: Monitor client connections
- **Performance Alerts**: Set up alerts for high error rates

## 🎯 Key Takeaways

- **Batch publishing** significantly improves throughput
- **High concurrency** and prefetch optimize consumer performance
- **Error handling** must be optimized for high-load scenarios
- **Monitoring** is critical for maintaining performance
- **Queue configuration** affects throughput and reliability
- **FS.StreamFlow provides** built-in optimizations for high-performance messaging
- **Resource management** (semaphores, connection pooling) is essential
- **Priority-based processing** helps manage different message types 