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
├── Models/
│   └── WorkTask.cs
├── Services/
│   ├── TaskProducer.cs
│   └── TaskWorker.cs
└── WorkQueues.csproj
```

## 🏗️ Implementation

### 1. Work Task Model

```csharp
// Models/WorkTask.cs
namespace WorkQueues.Models;

public class WorkTask
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string TaskType { get; set; } = string.Empty;
    public string Data { get; set; } = string.Empty;
    public int Priority { get; set; } = 0;
    public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;
    public string WorkerId { get; set; } = string.Empty;
}
```

### 2. Task Producer

```csharp
// Services/TaskProducer.cs
using FS.StreamFlow.Core.Features.Messaging.Interfaces;
using WorkQueues.Models;
using Microsoft.Extensions.Logging;

public class TaskProducer
{
    private readonly IStreamFlowClient _streamFlow;
    private readonly ILogger<TaskProducer> _logger;

    public TaskProducer(IStreamFlowClient streamFlow, ILogger<TaskProducer> logger)
    {
        _streamFlow = streamFlow;
        _logger = logger;
    }

    public async Task EnqueueTaskAsync(WorkTask task)
    {
        try
        {
            // Initialize the client first
            await _streamFlow.InitializeAsync();
            
            // Publish task with fluent API
            await _streamFlow.Producer.Message(task)
                .WithExchange("work-queue-exchange")
                .WithRoutingKey("task.created")
                .WithDeliveryMode(DeliveryMode.Persistent)
                .WithPriority(task.Priority)
                .PublishAsync();
                
            _logger.LogInformation("Task {TaskId} of type {TaskType} enqueued with priority {Priority}", 
                task.Id, task.TaskType, task.Priority);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to enqueue task {TaskId}", task.Id);
            throw;
        }
    }

    public async Task EnqueueMultipleTasksAsync(int count)
    {
        _logger.LogInformation("Enqueuing {Count} tasks...", count);
        
        var tasks = new[]
        {
            "image-processing",
            "data-analysis", 
            "report-generation",
            "email-sending",
            "file-compression"
        };

        for (int i = 1; i <= count; i++)
        {
            var task = new WorkTask
            {
                TaskType = tasks[Random.Shared.Next(tasks.Length)],
                Data = $"Task data #{i}",
                Priority = Random.Shared.Next(1, 6)
            };
            
            await EnqueueTaskAsync(task);
            await Task.Delay(50); // Small delay between tasks
        }
        
        _logger.LogInformation("Finished enqueuing {Count} tasks", count);
    }
}
```

### 3. Task Worker

```csharp
// Services/TaskWorker.cs
using FS.StreamFlow.Core.Features.Messaging.Interfaces;
using WorkQueues.Models;
using Microsoft.Extensions.Logging;

public class TaskWorker
{
    private readonly IStreamFlowClient _streamFlow;
    private readonly ILogger<TaskWorker> _logger;
    private readonly string _workerId;
    private int _processedCount = 0;

    public TaskWorker(IStreamFlowClient streamFlow, ILogger<TaskWorker> logger)
    {
        _streamFlow = streamFlow;
        _logger = logger;
        _workerId = $"worker-{Guid.NewGuid():N}"[..8];
    }

    public async Task StartWorkingAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("Worker {WorkerId} starting to process tasks...", _workerId);
        
        // Initialize the client first
        await _streamFlow.InitializeAsync();

        // Start consuming tasks with fluent API
        await _streamFlow.Consumer.Queue<WorkTask>("work-queue")
            .WithConcurrency(2)
            .WithPrefetchCount(5)
            .WithAutoAck(false)
            .WithConsumerTag($"worker-{_workerId}")
            .WithErrorHandler(async (exception, context) =>
            {
                _logger.LogError(exception, "Worker {WorkerId} encountered error processing task", _workerId);
                return exception is ConnectFailureException || exception is BrokerUnreachableException;
            })
            .ConsumeAsync(async (task, context) =>
            {
                return await ProcessTaskAsync(task, context);
            });
    }

    private async Task<bool> ProcessTaskAsync(WorkTask task, MessageContext context)
    {
        try
        {
            _logger.LogInformation("Worker {WorkerId} processing task {TaskId} of type {TaskType} with priority {Priority}", 
                _workerId, task.Id, task.TaskType, task.Priority);

            // Simulate different processing times based on task type
            var processingTime = task.TaskType switch
            {
                "image-processing" => 2000,
                "data-analysis" => 1500,
                "report-generation" => 3000,
                "email-sending" => 500,
                "file-compression" => 2500,
                _ => 1000
            };

            await Task.Delay(processingTime);

            // Simulate occasional processing failure (3% chance)
            if (Random.Shared.Next(1, 34) == 1)
            {
                throw new InvalidOperationException($"Simulated failure processing task {task.Id}");
            }

            // Update task with worker information
            task.WorkerId = _workerId;
            
            // Increment processed count
            Interlocked.Increment(ref _processedCount);

            _logger.LogInformation("Worker {WorkerId} completed task {TaskId} in {ProcessingTime}ms. Total processed: {ProcessedCount}", 
                _workerId, task.Id, processingTime, _processedCount);

            return true; // Acknowledge the task
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Worker {WorkerId} failed to process task {TaskId}: {TaskType}", 
                _workerId, task.Id, task.TaskType);
            
            // Return false to reject the task (it will be requeued or sent to dead letter queue)
            return false;
        }
    }

    public int GetProcessedCount() => _processedCount;
    public string GetWorkerId() => _workerId;
}
```

### 4. Main Program

```csharp
// Program.cs
using FS.StreamFlow.RabbitMQ.DependencyInjection;
using WorkQueues.Services;
using WorkQueues.Models;

var builder = Host.CreateApplicationBuilder(args);

// Add logging
builder.Services.AddLogging(config =>
{
    config.AddConsole();
    config.SetMinimumLevel(LogLevel.Information);
});

// Add FS.StreamFlow with RabbitMQ
builder.Services.AddRabbitMQStreamFlow(options =>
{
    // Client configuration
    options.ClientConfiguration.ClientName = "Work Queues Example";
    options.ClientConfiguration.EnableAutoRecovery = true;
    options.ClientConfiguration.EnableHeartbeat = true;
    options.ClientConfiguration.HeartbeatInterval = TimeSpan.FromSeconds(60);
    
    // Connection settings
    options.ConnectionSettings.Host = "localhost";
    options.ConnectionSettings.Port = 5672;
    options.ConnectionSettings.Username = "guest";
    options.ConnectionSettings.Password = "guest";
    options.ConnectionSettings.VirtualHost = "/";
    options.ConnectionSettings.ConnectionTimeout = TimeSpan.FromSeconds(30);
    
    // Producer settings
    options.ProducerSettings.EnablePublisherConfirms = true;
    options.ProducerSettings.ConfirmationTimeout = TimeSpan.FromSeconds(10);
    options.ProducerSettings.MaxConcurrentPublishes = 100;
    
    // Consumer settings
    options.ConsumerSettings.PrefetchCount = 5;
    options.ConsumerSettings.AutoAcknowledge = false;
    options.ConsumerSettings.MaxConcurrentConsumers = 2;
    
    // Error handling settings
    options.ErrorHandlingSettings.EnableDeadLetterQueue = true;
    options.ErrorHandlingSettings.DeadLetterExchange = "dlx";
    options.ErrorHandlingSettings.DeadLetterQueue = "dlq";
    options.ErrorHandlingSettings.MaxRetryAttempts = 3;
    options.ErrorHandlingSettings.RetryDelay = TimeSpan.FromSeconds(2);
});

// Add our services
builder.Services.AddSingleton<TaskProducer>();
builder.Services.AddSingleton<TaskWorker>();

var host = builder.Build();

// Setup cancellation handling
var cancellationTokenSource = new CancellationTokenSource();
Console.CancelKeyPress += (_, e) =>
{
    e.Cancel = true;
    cancellationTokenSource.Cancel();
};

try
{
    // Get StreamFlow client and initialize
    var streamFlow = host.Services.GetRequiredService<IStreamFlowClient>();
    await streamFlow.InitializeAsync();
    
    // Setup infrastructure
    await SetupInfrastructureAsync(streamFlow);

    // Get services
    var producer = host.Services.GetRequiredService<TaskProducer>();
    var worker = host.Services.GetRequiredService<TaskWorker>();

    // Start worker
    var workerTask = worker.StartWorkingAsync(cancellationTokenSource.Token);

    // Wait a moment for worker to start
    await Task.Delay(1000);

    // Enqueue some tasks
    await producer.EnqueueTaskAsync(new WorkTask 
    { 
        TaskType = "image-processing", 
        Data = "Process user uploaded image", 
        Priority = 3 
    });
    
    await producer.EnqueueTaskAsync(new WorkTask 
    { 
        TaskType = "data-analysis", 
        Data = "Analyze sales data", 
        Priority = 2 
    });
    
    await producer.EnqueueMultipleTasksAsync(15);

    Console.WriteLine("Tasks enqueued! Press Ctrl+C to stop processing...");
    Console.WriteLine("Check the logs to see tasks being processed by workers.");

    // Wait for cancellation
    await Task.Delay(Timeout.Infinite, cancellationTokenSource.Token);
}
catch (OperationCanceledException)
{
    Console.WriteLine("Shutting down gracefully...");
}
catch (Exception ex)
{
    Console.WriteLine($"An error occurred: {ex.Message}");
}
finally
{
    var worker = host.Services.GetRequiredService<TaskWorker>();
    Console.WriteLine($"Worker {worker.GetWorkerId()} processed {worker.GetProcessedCount()} tasks");
    
    await host.StopAsync();
}

// Infrastructure setup method
static async Task SetupInfrastructureAsync(IStreamFlowClient streamFlow)
{
    try
    {
        Console.WriteLine("Setting up work queue infrastructure...");

        // Declare work queue exchange with fluent API
        await streamFlow.ExchangeManager.Exchange("work-queue-exchange")
            .AsTopic()
            .WithDurable(true)
            .DeclareAsync();

        // Declare dead letter exchange with fluent API
        await streamFlow.ExchangeManager.Exchange("dlx")
            .AsTopic()
            .WithDurable(true)
            .DeclareAsync();

        // Declare work queue with fluent API
        await streamFlow.QueueManager.Queue("work-queue")
            .WithDurable(true)
            .WithDeadLetterExchange("dlx")
            .WithDeadLetterRoutingKey("task.failed")
            .WithMessageTtl(TimeSpan.FromHours(24))
            .WithMaxLength(10000)
            .BindToExchange("work-queue-exchange", "task.created")
            .DeclareAsync();

        // Declare dead letter queue with fluent API
        await streamFlow.QueueManager.Queue("dlq")
            .WithDurable(true)
            .BindToExchange("dlx", "task.failed")
            .DeclareAsync();

        Console.WriteLine("Work queue infrastructure setup completed successfully");
    }
    catch (Exception ex)
    {
        Console.WriteLine($"Failed to setup work queue infrastructure: {ex.Message}");
        throw;
    }
}
```

## 🚀 Running the Example

1. **Start RabbitMQ** (if not already running):
   ```bash
   docker run -d --name rabbitmq -p 5672:5672 -p 15672:15672 rabbitmq:3-management
   ```

2. **Run the application**:
   ```bash
   dotnet run
   ```

3. **Expected output**:
   ```
   Setting up work queue infrastructure...
   Work queue infrastructure setup completed successfully
   info: WorkQueues.Services.TaskWorker[0]
         Worker worker-abc12345 starting to process tasks...
   info: WorkQueues.Services.TaskProducer[0]
         Task 12345678-... of type image-processing enqueued with priority 3
   info: WorkQueues.Services.TaskWorker[0]
         Worker worker-abc12345 processing task 12345678-... of type image-processing with priority 3
   info: WorkQueues.Services.TaskWorker[0]
         Worker worker-abc12345 completed task 12345678-... in 2000ms. Total processed: 1
   ```

## 🔧 Configuration Options

### Worker Configuration
```csharp
options.ConsumerSettings.PrefetchCount = 5;                      // Number of tasks to prefetch
options.ConsumerSettings.AutoAcknowledge = false;                // Manual acknowledgment
options.ConsumerSettings.MaxConcurrentConsumers = 2;             // Number of concurrent workers
```

### Producer Configuration
```csharp
options.ProducerSettings.EnablePublisherConfirms = true;         // Wait for broker confirmation
options.ProducerSettings.ConfirmationTimeout = TimeSpan.FromSeconds(10);  // Confirmation timeout
options.ProducerSettings.MaxConcurrentPublishes = 100;           // Max concurrent publishes
```

## 🐛 Troubleshooting

### Common Issues

1. **Tasks not being processed**
   ```
   Tasks stay in queue but workers don't process them
   ```
   **Solution**: Check if the queue binding is correct and workers are started

2. **High memory usage**
   ```
   Application uses increasing amounts of memory
   ```
   **Solution**: Reduce prefetch count or implement backpressure handling

3. **Tasks being lost**
   ```
   Tasks disappear without being processed
   ```
   **Solution**: Check dead letter queue for failed tasks

### Debug Tips

1. **Check RabbitMQ Management UI**:
   - Go to http://localhost:15672
   - Login with guest/guest
   - Monitor work-queue and dlq queues

2. **Monitor queue depth**:
   ```csharp
   var queueInfo = await streamFlow.QueueManager.Queue("work-queue").GetInfoAsync();
   Console.WriteLine($"Work queue has {queueInfo.MessageCount} pending tasks");
   ```

## 📈 Next Steps

Once you've mastered this example, try:

1. **[High Throughput Processing](high-throughput-processing.md)** - Optimize for maximum performance
2. **[Order Processing](order-processing.md)** - Real-world e-commerce scenario
3. **[Saga Orchestration](saga-orchestration.md)** - Complex workflow management

## 🎯 Key Takeaways

- ✅ Work queue pattern for scalable task distribution
- ✅ Infrastructure setup (exchanges, queues, bindings) with fluent API
- ✅ Worker scaling and parallel processing
- ✅ Error handling and retry mechanisms
- ✅ Task prioritization and processing
- ✅ Monitoring and logging
- ✅ Always call InitializeAsync() before using the client

This example demonstrates how to build scalable task processing systems with FS.StreamFlow! 🚀 