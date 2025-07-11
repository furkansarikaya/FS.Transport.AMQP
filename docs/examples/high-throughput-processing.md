# High-Throughput Processing Example

**Difficulty**: 🔴 Advanced  
**Focus**: Optimized for performance  
**Time**: 30 minutes

This example demonstrates how to implement high-throughput message processing using FS.RabbitMQ. It covers batch publishing, consumer concurrency, error handling, and monitoring.

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
using FS.RabbitMQ.Core;
using Microsoft.Extensions.Logging;

public class BatchProducer
{
    private readonly IRabbitMQClient _rabbitMQ;
    private readonly ILogger<BatchProducer> _logger;

    public BatchProducer(IRabbitMQClient rabbitMQ, ILogger<BatchProducer> logger)
    {
        _rabbitMQ = rabbitMQ;
        _logger = logger;
    }

    public async Task PublishBatchAsync(IEnumerable<object> messages)
    {
        // Batch publish logic
        await _rabbitMQ.Producer.PublishBatchAsync(messages.Select(m =>
            new MessageContext("high-throughput-exchange", "batch", m)));
        _logger.LogInformation("Batch of {Count} messages published", messages.Count());
    }
}
```

### 2. High-Throughput Consumer

```csharp
// Services/HighThroughputConsumer.cs
using FS.RabbitMQ.Core;
using Microsoft.Extensions.Logging;

public class HighThroughputConsumer
{
    private readonly IRabbitMQClient _rabbitMQ;
    private readonly ILogger<HighThroughputConsumer> _logger;

    public HighThroughputConsumer(IRabbitMQClient rabbitMQ, ILogger<HighThroughputConsumer> logger)
    {
        _rabbitMQ = rabbitMQ;
        _logger = logger;
    }

    public async Task StartConsumingAsync(CancellationToken cancellationToken)
    {
        await _rabbitMQ.Consumer.ConsumeAsync<object>(
            queueName: "high-throughput-queue",
            messageHandler: async (msg, ctx) =>
            {
                // Process message
                await Task.Delay(10); // Simulate fast processing
                return true;
            },
            cancellationToken: cancellationToken);
    }
}
```

## 🛡️ Error Handling
- All failures are routed to a dead letter queue after max retries.
- Consumer logs errors and processing failures.

## 📊 Monitoring
- Use RabbitMQ Management UI at http://localhost:15672 to monitor throughput.
- Logs show batch publishing and consumer activity in real time.

## 🎯 Key Takeaways
- Batch publishing and concurrency boost throughput.
- Error handling and monitoring are essential for high-load scenarios.
- FS.RabbitMQ enables high-performance messaging. 