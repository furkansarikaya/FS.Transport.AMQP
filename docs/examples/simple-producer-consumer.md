# Simple Producer-Consumer Example

**Difficulty**: 🟢 Beginner  
**Focus**: Basic publish/subscribe pattern  
**Time**: 15 minutes  

This example demonstrates the fundamental producer-consumer pattern using FS.StreamFlow. Perfect for getting started with message-based communication.

## 📋 What You'll Learn

- Basic message publishing and consuming
- Connection setup and configuration
- Message serialization and deserialization
- Error handling fundamentals
- Clean shutdown patterns

## 🛠️ Setup

### Prerequisites
- .NET 9 SDK
- RabbitMQ server running on localhost
- Basic C# knowledge

### Project Structure
```
SimpleProducerConsumer/
├── Program.cs
├── Models/
│   └── Message.cs
├── Services/
│   ├── MessageProducer.cs
│   └── MessageConsumer.cs
└── SimpleProducerConsumer.csproj
```

## 🏗️ Implementation

### 1. Project Setup

```xml
<!-- SimpleProducerConsumer.csproj -->
<Project Sdk="Microsoft.NET.Sdk">

  <PropertyGroup>
    <OutputType>Exe</OutputType>
    <TargetFramework>net9.0</TargetFramework>
    <Nullable>enable</Nullable>
    <ImplicitUsings>enable</ImplicitUsings>
  </PropertyGroup>

  <ItemGroup>
    <PackageReference Include="FS.StreamFlow.RabbitMQ" Version="9.0.0" />
    <PackageReference Include="Microsoft.Extensions.Hosting" Version="9.0.0" />
    <PackageReference Include="Microsoft.Extensions.Logging" Version="9.0.0" />
  </ItemGroup>

</Project>
```

### 2. Message Model

```csharp
// Models/Message.cs
namespace SimpleProducerConsumer.Models;

public class Message
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string Content { get; set; } = string.Empty;
    public DateTimeOffset Timestamp { get; set; } = DateTimeOffset.UtcNow;
    public string Sender { get; set; } = string.Empty;
    public int Priority { get; set; } = 0;
}
```

### 3. Message Producer

```csharp
// Services/MessageProducer.cs
using FS.StreamFlow.Core.Features.Messaging.Interfaces;
using SimpleProducerConsumer.Models;

namespace SimpleProducerConsumer.Services;

public class MessageProducer
{
    private readonly IStreamFlowClient _streamFlow;
    private readonly ILogger<MessageProducer> _logger;

    public MessageProducer(IStreamFlowClient streamFlow, ILogger<MessageProducer> logger)
    {
        _streamFlow = streamFlow;
        _logger = logger;
    }

    public async Task PublishMessageAsync(string content, string sender = "System")
    {
        var message = new Message
        {
            Content = content,
            Sender = sender,
            Priority = Random.Shared.Next(1, 5)
        };

        try
        {
            // Initialize the client first
            await _streamFlow.InitializeAsync();
            
            // Publish message with fluent API
            await _streamFlow.Producer.Message(message)
                .WithExchange("simple-messages")
                .WithRoutingKey("message.created")
                .WithDeliveryMode(DeliveryMode.Persistent)
                .PublishAsync();

            _logger.LogInformation("Message published: {MessageId} - {Content}", 
                message.Id, message.Content);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to publish message: {Content}", content);
            throw;
        }
    }

    public async Task PublishMultipleMessagesAsync(int count)
    {
        _logger.LogInformation("Publishing {Count} messages...", count);

        for (int i = 1; i <= count; i++)
        {
            await PublishMessageAsync($"Message #{i}", "Bulk Producer");
            
            // Add small delay to see messages being processed
            await Task.Delay(100);
        }

        _logger.LogInformation("Finished publishing {Count} messages", count);
    }
}
```

### 4. Message Consumer

```csharp
// Services/MessageConsumer.cs
using FS.StreamFlow.Core.Features.Messaging.Interfaces;
using SimpleProducerConsumer.Models;

namespace SimpleProducerConsumer.Services;

public class MessageConsumer
{
    private readonly IStreamFlowClient _streamFlow;
    private readonly ILogger<MessageConsumer> _logger;
    private int _processedCount = 0;

    public MessageConsumer(IStreamFlowClient streamFlow, ILogger<MessageConsumer> logger)
    {
        _streamFlow = streamFlow;
        _logger = logger;
    }

    public async Task StartConsumingAsync()
    {
        _logger.LogInformation("Starting message consumer...");

        // Initialize the client first
        await _streamFlow.InitializeAsync();

        // Start consuming messages with fluent API
        await _streamFlow.Consumer.Queue<Message>("simple-message-queue")
            .WithConcurrency(3)
            .WithPrefetchCount(10)
            .WithAutoAck(false)
            .WithErrorHandler(async (exception, context) =>
            {
                return exception is ConnectFailureException || exception is BrokerUnreachableException;
            })
            .ConsumeAsync(async (message, context) =>
            {
                return await ProcessMessageAsync(message, context);
            });
    }

    private async Task<bool> ProcessMessageAsync(Message message, MessageContext context)
    {
        try
        {
            _logger.LogInformation("Processing message {MessageId}: {Content} from {Sender}", 
                message.Id, message.Content, message.Sender);

            // Simulate some processing work
            await Task.Delay(200);

            // Increment processed count
            Interlocked.Increment(ref _processedCount);

            // Log progress every 10 messages
            if (_processedCount % 10 == 0)
            {
                _logger.LogInformation("Processed {ProcessedCount} messages so far", _processedCount);
            }

            // Simulate occasional processing failure (5% chance)
            if (Random.Shared.Next(1, 21) == 1)
            {
                throw new InvalidOperationException("Simulated processing failure");
            }

            _logger.LogInformation("Message {MessageId} processed successfully", message.Id);
            return true; // Acknowledge the message
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to process message {MessageId}: {Content}", 
                message.Id, message.Content);
            
            // Return false to reject the message (it will be requeued or sent to dead letter queue)
            return false;
        }
    }

    public int GetProcessedCount() => _processedCount;
}
```

### 5. Main Program

```csharp
// Program.cs
using FS.StreamFlow.RabbitMQ.DependencyInjection;
using SimpleProducerConsumer.Services;

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
    options.ClientConfiguration.ClientName = "Simple Producer-Consumer Example";
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
    options.ProducerSettings.ConfirmationTimeout = TimeSpan.FromSeconds(5);
    options.ProducerSettings.MaxConcurrentPublishes = 100;
    
    // Consumer settings
    options.ConsumerSettings.PrefetchCount = 10;
    options.ConsumerSettings.AutoAcknowledge = false;
    options.ConsumerSettings.MaxConcurrentConsumers = 3;
    
    // Error handling settings
    options.ErrorHandlingSettings.EnableDeadLetterQueue = true;
    options.ErrorHandlingSettings.DeadLetterExchange = "dlx";
    options.ErrorHandlingSettings.DeadLetterQueue = "dlq";
    options.ErrorHandlingSettings.MaxRetryAttempts = 3;
    options.ErrorHandlingSettings.RetryDelay = TimeSpan.FromSeconds(2);
});

// Add our services
builder.Services.AddSingleton<MessageProducer>();
builder.Services.AddSingleton<MessageConsumer>();

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
    var producer = host.Services.GetRequiredService<MessageProducer>();
    var consumer = host.Services.GetRequiredService<MessageConsumer>();

    // Start consumer
    var consumerTask = consumer.StartConsumingAsync(cancellationTokenSource.Token);

    // Wait a moment for consumer to start
    await Task.Delay(1000);

    // Publish some messages
    await producer.PublishMessageAsync("Hello, RabbitMQ!", "Producer");
    await producer.PublishMessageAsync("This is a test message", "Producer");
    await producer.PublishMultipleMessagesAsync(20);

    Console.WriteLine("Messages published! Press Ctrl+C to stop consuming...");
    Console.WriteLine("Check the logs to see messages being processed.");

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
    var consumer = host.Services.GetRequiredService<MessageConsumer>();
    Console.WriteLine($"Total messages processed: {consumer.GetProcessedCount()}");
    
    await host.StopAsync();
}

// Infrastructure setup method
static async Task SetupInfrastructureAsync(IStreamFlowClient streamFlow)
{
    try
    {
        Console.WriteLine("Setting up RabbitMQ infrastructure...");

        // Declare exchange with fluent API
        await streamFlow.ExchangeManager.Exchange("simple-messages")
            .AsTopic()
            .WithDurable(true)
            .DeclareAsync();

        // Declare dead letter exchange with fluent API
        await streamFlow.ExchangeManager.Exchange("dlx")
            .AsTopic()
            .WithDurable(true)
            .DeclareAsync();

        // Declare main queue with fluent API
        await streamFlow.QueueManager.Queue("simple-message-queue")
            .WithDurable(true)
            .WithDeadLetterExchange("dlx")
            .WithDeadLetterRoutingKey("message.failed")
            .BindToExchange("simple-messages", "message.created")
            .DeclareAsync();

        // Declare dead letter queue with fluent API
        await streamFlow.QueueManager.Queue("dlq")
            .WithDurable(true)
            .BindToExchange("dlx", "message.failed")
            .DeclareAsync();

        Console.WriteLine("RabbitMQ infrastructure setup completed successfully");
    }
    catch (Exception ex)
    {
        Console.WriteLine($"Failed to setup RabbitMQ infrastructure: {ex.Message}");
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
   Setting up RabbitMQ infrastructure...
   RabbitMQ infrastructure setup completed successfully
   info: SimpleProducerConsumer.Services.MessageConsumer[0]
         Starting message consumer...
   info: SimpleProducerConsumer.Services.MessageProducer[0]
         Message published: a1b2c3d4-... - Hello, RabbitMQ!
   info: SimpleProducerConsumer.Services.MessageConsumer[0]
         Processing message a1b2c3d4-...: Hello, RabbitMQ! from Producer
   info: SimpleProducerConsumer.Services.MessageConsumer[0]
         Message a1b2c3d4-... processed successfully
   ```

## 🔧 Configuration Options

### Producer Configuration
```csharp
options.ProducerSettings.EnablePublisherConfirms = true;          // Wait for broker confirmation
options.ProducerSettings.ConfirmationTimeout = TimeSpan.FromSeconds(5);  // Confirmation timeout
options.ProducerSettings.MaxConcurrentPublishes = 100;            // Max concurrent publishes
```

### Consumer Configuration
```csharp
options.ConsumerSettings.PrefetchCount = 10;                      // Number of messages to prefetch
options.ConsumerSettings.AutoAcknowledge = false;                 // Manual acknowledgment
options.ConsumerSettings.MaxConcurrentConsumers = 3;              // Number of concurrent consumers
```

## 🐛 Troubleshooting

### Common Issues

1. **Connection refused**
   ```
   RabbitMQ.Client.Exceptions.BrokerUnreachableException: None of the specified endpoints were reachable
   ```
   **Solution**: Ensure RabbitMQ is running on localhost:5672

2. **Messages not being consumed**
   ```
   Messages stay in queue but consumer doesn't process them
   ```
   **Solution**: Check if the queue binding is correct and consumer is started

3. **High memory usage**
   ```
   Application uses increasing amounts of memory
   ```
   **Solution**: Reduce prefetch count or implement backpressure handling

### Debug Tips

1. **Check RabbitMQ Management UI**:
   - Go to http://localhost:15672
   - Login with guest/guest
   - Monitor queues, exchanges, and connections

2. **Enable debug logging**:
   ```csharp
   builder.Services.AddLogging(config =>
   {
       config.AddConsole();
       config.SetMinimumLevel(LogLevel.Debug);
   });
   ```

3. **Monitor queue depth**:
   ```csharp
   var queueInfo = await streamFlow.QueueManager.Queue("simple-message-queue").GetInfoAsync();
   Console.WriteLine($"Queue has {queueInfo.MessageCount} messages");
   ```

## 📈 Next Steps

Once you've mastered this example, try:

1. **[Request-Reply Pattern](request-reply-pattern.md)** - Synchronous communication
2. **[Work Queues](work-queues.md)** - Distribute work among multiple workers
3. **[Order Processing](order-processing.md)** - Real-world e-commerce scenario

## 🎯 Key Takeaways

- ✅ Basic producer-consumer pattern with FS.StreamFlow
- ✅ Infrastructure setup (exchanges, queues, bindings) with fluent API
- ✅ Message serialization and deserialization
- ✅ Error handling and retry mechanisms
- ✅ Graceful shutdown patterns
- ✅ Monitoring and logging
- ✅ Always call InitializeAsync() before using the client

This example provides a solid foundation for building more complex messaging applications with FS.StreamFlow! 🚀 