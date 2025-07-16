# Work Queues Example

**Difficulty**: 🟢 Beginner  
**Focus**: Task distribution patterns  
**Time**: 15 minutes

This example demonstrates how to implement work queues using FS.StreamFlow. It covers task distribution, worker scaling, error handling, and monitoring.

## 📋 What You'll Learn
- Work queue pattern for task distribution
- Scaling workers for parallel processing
- Error handling for work queues
- Monitoring queue depth and worker activity

## 🛠️ Setup

### Prerequisites
- .NET 9 SDK
- RabbitMQ server running on localhost
- Basic C# knowledge

### Project Structure
```
WorkQueues/
├── Program.cs
├── Services/
│   ├── TaskProducer.cs
│   └── TaskWorker.cs
└── WorkQueues.csproj
```

## 🏗️ Implementation

### 1. Task Producer

```csharp
// Services/TaskProducer.cs
using FS.StreamFlow.Core.Features.Messaging.Interfaces;
using Microsoft.Extensions.Logging;

public class TaskProducer
{
    private readonly IStreamFlowClient _streamFlow;
    private readonly ILogger<TaskProducer> _logger;

    public TaskProducer(IStreamFlowClient rabbitMQ, ILogger<TaskProducer> logger)
    {
        _streamFlow = rabbitMQ;
        _logger = logger;
    }

    public async Task EnqueueTaskAsync(object task)
    {
        await _streamFlow.Producer.PublishAsync(
            exchange: "",
            routingKey: "work-queue",
            message: task);
        _logger.LogInformation("Task enqueued");
    }
}
```

### 2. Task Worker

```csharp
// Services/TaskWorker.cs
using FS.StreamFlow.Core.Features.Messaging.Interfaces;
using Microsoft.Extensions.Logging;

public class TaskWorker
{
    private readonly IStreamFlowClient _streamFlow;
    private readonly ILogger<TaskWorker> _logger;

    public TaskWorker(IStreamFlowClient rabbitMQ, ILogger<TaskWorker> logger)
    {
        _streamFlow = rabbitMQ;
        _logger = logger;
    }

    public async Task StartWorkingAsync(CancellationToken cancellationToken)
    {
        await _streamFlow.Consumer.ConsumeAsync<object>(
            queueName: "work-queue",
            messageHandler: async (task, ctx) =>
            {
                // Process task
                await Task.Delay(100); // Simulate work
                return true;
            },
            cancellationToken: cancellationToken);
    }
}
```

## 🛡️ Error Handling
- All failures are routed to a dead letter queue after max retries.
- Workers log errors and processing failures.

## 📊 Monitoring
- Use RabbitMQ Management UI at http://localhost:15672 to monitor work queues.
- Logs show task enqueuing and worker activity in real time.

## 🎯 Key Takeaways
- Work queues enable scalable task distribution.
- Error handling and monitoring are essential for reliable processing.
- FS.StreamFlow simplifies work queue implementation. 