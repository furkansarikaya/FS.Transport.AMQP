# Getting Started with FS.RabbitMQ

Welcome to FS.RabbitMQ! This guide will take you from zero to hero, teaching you everything you need to know to build production-ready messaging applications.

## 📚 What You'll Learn

By the end of this guide, you'll be able to:
- Set up RabbitMQ and FS.RabbitMQ in your project
- Publish and consume messages
- Configure error handling and retries
- Build event-driven applications
- Monitor your messaging system
- Deploy to production

## 🎯 Prerequisites

- Basic knowledge of C# and .NET
- Visual Studio or Visual Studio Code
- .NET 9 SDK installed
- Docker (optional, for local RabbitMQ)

## 🚀 Step 1: Setting Up RabbitMQ

### Option 1: Using Docker (Recommended for Development)

```bash
# Run RabbitMQ with management UI
docker run -d --name rabbitmq \
  -p 5672:5672 \
  -p 15672:15672 \
  rabbitmq:3-management
```

Access the management UI at `http://localhost:15672` (guest/guest)

### Option 2: Installing RabbitMQ Locally

1. Download RabbitMQ from [https://www.rabbitmq.com/download.html](https://www.rabbitmq.com/download.html)
2. Install following the platform-specific instructions
3. Start the RabbitMQ service

### Option 3: Using Cloud Services

- **CloudAMQP**: Managed RabbitMQ service
- **Amazon MQ**: AWS managed message broker
- **Azure Service Bus**: Microsoft's messaging service

## 🛠️ Step 2: Create Your First Project

### 2.1 Create a New Project

```bash
# Create a new console application
dotnet new console -n MyFirstRabbitMQApp
cd MyFirstRabbitMQApp

# Add FS.RabbitMQ package
dotnet add package FS.RabbitMQ

# Add additional packages for dependency injection
dotnet add package Microsoft.Extensions.Hosting
dotnet add package Microsoft.Extensions.DependencyInjection
dotnet add package Microsoft.Extensions.Logging
```

### 2.2 Basic Project Structure

```
MyFirstRabbitMQApp/
├── Program.cs
├── Models/
│   └── Order.cs
├── Services/
│   ├── OrderService.cs
│   └── OrderProcessor.cs
└── appsettings.json
```

### 2.3 Create Your First Message Model

```csharp
// Models/Order.cs
namespace MyFirstRabbitMQApp.Models;

public class Order
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string CustomerName { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public List<OrderItem> Items { get; set; } = new();
}

public class OrderItem
{
    public string ProductName { get; set; } = string.Empty;
    public int Quantity { get; set; }
    public decimal Price { get; set; }
}
```

## 📨 Step 3: Your First Message Publisher

### 3.1 Create Order Service

```csharp
// Services/OrderService.cs
using FS.RabbitMQ.Core;
using MyFirstRabbitMQApp.Models;

namespace MyFirstRabbitMQApp.Services;

public class OrderService
{
    private readonly IRabbitMQClient _rabbitMQ;
    private readonly ILogger<OrderService> _logger;

    public OrderService(IRabbitMQClient rabbitMQ, ILogger<OrderService> logger)
    {
        _rabbitMQ = rabbitMQ;
        _logger = logger;
    }

    public async Task CreateOrderAsync(Order order)
    {
        try
        {
            _logger.LogInformation("Creating order {OrderId} for customer {CustomerName}", 
                order.Id, order.CustomerName);

            // Publish the order to RabbitMQ
            await _rabbitMQ.Producer.PublishAsync(
                exchange: "orders",
                routingKey: "order.created",
                message: order);

            _logger.LogInformation("Order {OrderId} published successfully", order.Id);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to create order {OrderId}", order.Id);
            throw;
        }
    }
}
```

### 3.2 Setup Dependency Injection

```csharp
// Program.cs
using FS.RabbitMQ.DependencyInjection;
using MyFirstRabbitMQApp.Services;

var builder = Host.CreateApplicationBuilder(args);

// Add logging
builder.Services.AddLogging(config =>
{
    config.AddConsole();
    config.SetMinimumLevel(LogLevel.Information);
});

// Add FS.RabbitMQ
builder.Services.AddRabbitMQ()
    .WithConnectionString("amqp://localhost") // Default RabbitMQ connection
    .WithHealthChecks()
    .WithMonitoring()
    .Build();

// Add your services
builder.Services.AddScoped<OrderService>();
builder.Services.AddScoped<OrderProcessor>();

var host = builder.Build();

// Example usage
var orderService = host.Services.GetRequiredService<OrderService>();

// Create a sample order
var order = new Order
{
    CustomerName = "John Doe",
    Amount = 99.99m,
    Items = new List<OrderItem>
    {
        new() { ProductName = "Laptop", Quantity = 1, Price = 99.99m }
    }
};

await orderService.CreateOrderAsync(order);

Console.WriteLine("Order created and published!");
Console.WriteLine("Press any key to exit...");
Console.ReadKey();
```

## 📥 Step 4: Your First Message Consumer

### 4.1 Create Order Processor

```csharp
// Services/OrderProcessor.cs
using FS.RabbitMQ.Core;
using FS.RabbitMQ.Producer;
using MyFirstRabbitMQApp.Models;

namespace MyFirstRabbitMQApp.Services;

public class OrderProcessor
{
    private readonly IRabbitMQClient _rabbitMQ;
    private readonly ILogger<OrderProcessor> _logger;

    public OrderProcessor(IRabbitMQClient rabbitMQ, ILogger<OrderProcessor> logger)
    {
        _rabbitMQ = rabbitMQ;
        _logger = logger;
    }

    public async Task StartProcessingAsync(CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Starting order processing...");

        // Start consuming messages
        await _rabbitMQ.Consumer.ConsumeAsync<Order>(
            queueName: "order-processing",
            messageHandler: ProcessOrderAsync,
            cancellationToken: cancellationToken);
    }

    private async Task<bool> ProcessOrderAsync(Order order, MessageContext context)
    {
        try
        {
            _logger.LogInformation("Processing order {OrderId} for customer {CustomerName}", 
                order.Id, order.CustomerName);

            // Simulate order processing
            await Task.Delay(1000); // Simulate work

            // Business logic here
            await ValidateOrderAsync(order);
            await ProcessPaymentAsync(order);
            await UpdateInventoryAsync(order);

            _logger.LogInformation("Order {OrderId} processed successfully", order.Id);
            
            return true; // Acknowledge the message
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to process order {OrderId}", order.Id);
            return false; // Reject the message (will be retried or sent to dead letter queue)
        }
    }

    private async Task ValidateOrderAsync(Order order)
    {
        // Validation logic
        if (order.Amount <= 0)
            throw new InvalidOperationException("Order amount must be greater than zero");
        
        await Task.Delay(100); // Simulate validation
    }

    private async Task ProcessPaymentAsync(Order order)
    {
        // Payment processing logic
        _logger.LogInformation("Processing payment for order {OrderId}", order.Id);
        await Task.Delay(500); // Simulate payment processing
    }

    private async Task UpdateInventoryAsync(Order order)
    {
        // Inventory update logic
        _logger.LogInformation("Updating inventory for order {OrderId}", order.Id);
        await Task.Delay(300); // Simulate inventory update
    }
}
```

### 4.2 Update Program.cs for Consumer

```csharp
// Program.cs - Updated version
using FS.RabbitMQ.DependencyInjection;
using MyFirstRabbitMQApp.Services;

var builder = Host.CreateApplicationBuilder(args);

// Add logging
builder.Services.AddLogging(config =>
{
    config.AddConsole();
    config.SetMinimumLevel(LogLevel.Information);
});

// Add FS.RabbitMQ with error handling
builder.Services.AddRabbitMQ()
    .WithConnectionString("amqp://localhost")
    .WithErrorHandling(config =>
    {
        config.EnableDeadLetterQueue = true;
        config.DeadLetterExchange = "dlx";
        config.DeadLetterQueue = "dlq";
        config.MaxRetryAttempts = 3;
        config.RetryDelay = TimeSpan.FromSeconds(2);
    })
    .WithHealthChecks()
    .WithMonitoring()
    .Build();

// Add your services
builder.Services.AddScoped<OrderService>();
builder.Services.AddScoped<OrderProcessor>();

var host = builder.Build();

// Setup cancellation token for graceful shutdown
var cancellationTokenSource = new CancellationTokenSource();
Console.CancelKeyPress += (_, e) =>
{
    e.Cancel = true;
    cancellationTokenSource.Cancel();
};

try
{
    // Start the consumer
    var orderProcessor = host.Services.GetRequiredService<OrderProcessor>();
    var processingTask = orderProcessor.StartProcessingAsync(cancellationTokenSource.Token);

    // Create and send a sample order
    var orderService = host.Services.GetRequiredService<OrderService>();
    var order = new Order
    {
        CustomerName = "John Doe",
        Amount = 99.99m,
        Items = new List<OrderItem>
        {
            new() { ProductName = "Laptop", Quantity = 1, Price = 99.99m }
        }
    };

    await orderService.CreateOrderAsync(order);

    Console.WriteLine("Order created and consumer started!");
    Console.WriteLine("Press Ctrl+C to stop...");

    // Wait for cancellation
    await Task.Delay(Timeout.Infinite, cancellationTokenSource.Token);
}
catch (OperationCanceledException)
{
    Console.WriteLine("Shutting down...");
}
finally
{
    await host.StopAsync();
}
```

## ⚙️ Step 5: Configuration

### 5.1 Add Configuration File

```json
// appsettings.json
{
  "RabbitMQ": {
    "Connection": {
      "ConnectionString": "amqp://localhost",
      "AutomaticRecovery": true,
      "HeartbeatInterval": "00:01:00",
      "MaxChannels": 100,
      "ConnectionTimeout": "00:00:30"
    },
    "Producer": {
      "EnableConfirmations": true,
      "ConfirmationTimeout": "00:00:05",
      "BatchSize": 100,
      "MaxBatchWaitTime": "00:00:01"
    },
    "Consumer": {
      "PrefetchCount": 10,
      "ConcurrentConsumers": 2,
      "AutoAck": false,
      "RequeueOnFailure": true
    },
    "ErrorHandling": {
      "EnableDeadLetterQueue": true,
      "DeadLetterExchange": "dlx",
      "DeadLetterQueue": "dlq",
      "MaxRetryAttempts": 3,
      "RetryDelay": "00:00:02"
    },
    "HealthCheck": {
      "Enabled": true,
      "Interval": "00:00:30",
      "Timeout": "00:00:05"
    }
  },
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "FS.RabbitMQ": "Information",
      "Microsoft": "Warning"
    }
  }
}
```

### 5.2 Use Configuration in Code

```csharp
// Program.cs - Configuration version
using FS.RabbitMQ.DependencyInjection;
using MyFirstRabbitMQApp.Services;

var builder = Host.CreateApplicationBuilder(args);

// Add configuration
builder.Configuration
    .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
    .AddEnvironmentVariables();

// Add logging
builder.Services.AddLogging(config =>
{
    config.AddConsole();
    config.AddConfiguration(builder.Configuration.GetSection("Logging"));
});

// Add FS.RabbitMQ with configuration
builder.Services.ConfigureRabbitMQ(builder.Configuration.GetSection("RabbitMQ"));

// Add your services
builder.Services.AddScoped<OrderService>();
builder.Services.AddScoped<OrderProcessor>();

var host = builder.Build();

// Rest of your code...
```

## 🔧 Step 6: Exchange and Queue Setup

### 6.1 Declare Exchanges and Queues

```csharp
// Services/InfrastructureService.cs
using FS.RabbitMQ.Core;
using FS.RabbitMQ.Configuration;

namespace MyFirstRabbitMQApp.Services;

public class InfrastructureService
{
    private readonly IRabbitMQClient _rabbitMQ;
    private readonly ILogger<InfrastructureService> _logger;

    public InfrastructureService(IRabbitMQClient rabbitMQ, ILogger<InfrastructureService> logger)
    {
        _rabbitMQ = rabbitMQ;
        _logger = logger;
    }

    public async Task SetupInfrastructureAsync()
    {
        _logger.LogInformation("Setting up RabbitMQ infrastructure...");

        try
        {
            // Declare exchanges
            await _rabbitMQ.ExchangeManager.DeclareExchangeAsync(
                exchange: "orders",
                type: "topic",
                durable: true,
                autoDelete: false);

            await _rabbitMQ.ExchangeManager.DeclareExchangeAsync(
                exchange: "dlx",
                type: "topic",
                durable: true,
                autoDelete: false);

            // Declare queues
            await _rabbitMQ.QueueManager.DeclareQueueAsync(
                queue: "order-processing",
                durable: true,
                exclusive: false,
                autoDelete: false,
                arguments: new Dictionary<string, object>
                {
                    {"x-dead-letter-exchange", "dlx"},
                    {"x-dead-letter-routing-key", "order.failed"}
                });

            await _rabbitMQ.QueueManager.DeclareQueueAsync(
                queue: "dlq",
                durable: true,
                exclusive: false,
                autoDelete: false);

            // Bind queues to exchanges
            await _rabbitMQ.QueueManager.BindQueueAsync(
                queue: "order-processing",
                exchange: "orders",
                routingKey: "order.created");

            await _rabbitMQ.QueueManager.BindQueueAsync(
                queue: "dlq",
                exchange: "dlx",
                routingKey: "order.failed");

            _logger.LogInformation("RabbitMQ infrastructure setup completed successfully");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to setup RabbitMQ infrastructure");
            throw;
        }
    }
}
```

### 6.2 Initialize Infrastructure on Startup

```csharp
// Program.cs - Add infrastructure setup
var host = builder.Build();

// Setup infrastructure
var infrastructureService = host.Services.GetRequiredService<InfrastructureService>();
await infrastructureService.SetupInfrastructureAsync();

// Rest of your code...
```

## 🎯 Step 7: Running Your Application

### 7.1 Build and Run

```bash
# Build the application
dotnet build

# Run the application
dotnet run
```

### 7.2 What You Should See

```
info: MyFirstRabbitMQApp.Services.InfrastructureService[0]
      Setting up RabbitMQ infrastructure...
info: MyFirstRabbitMQApp.Services.InfrastructureService[0]
      RabbitMQ infrastructure setup completed successfully
info: MyFirstRabbitMQApp.Services.OrderService[0]
      Creating order a1b2c3d4-e5f6-7890-abcd-ef1234567890 for customer John Doe
info: MyFirstRabbitMQApp.Services.OrderService[0]
      Order a1b2c3d4-e5f6-7890-abcd-ef1234567890 published successfully
info: MyFirstRabbitMQApp.Services.OrderProcessor[0]
      Starting order processing...
info: MyFirstRabbitMQApp.Services.OrderProcessor[0]
      Processing order a1b2c3d4-e5f6-7890-abcd-ef1234567890 for customer John Doe
info: MyFirstRabbitMQApp.Services.OrderProcessor[0]
      Processing payment for order a1b2c3d4-e5f6-7890-abcd-ef1234567890
info: MyFirstRabbitMQApp.Services.OrderProcessor[0]
      Updating inventory for order a1b2c3d4-e5f6-7890-abcd-ef1234567890
info: MyFirstRabbitMQApp.Services.OrderProcessor[0]
      Order a1b2c3d4-e5f6-7890-abcd-ef1234567890 processed successfully
Order created and consumer started!
Press Ctrl+C to stop...
```

## 🔍 Step 8: Monitoring and Debugging

### 8.1 View Messages in RabbitMQ Management UI

1. Open `http://localhost:15672` in your browser
2. Login with `guest/guest`
3. Go to the "Queues" tab
4. Click on your queue to see messages

### 8.2 Add Health Checks

```csharp
// Program.cs - Add health checks
builder.Services.AddHealthChecks()
    .AddCheck<RabbitMQHealthCheck>("rabbitmq");

var app = builder.Build();

// Add health check endpoint
app.MapHealthChecks("/health");
```

## 🚀 Step 9: Next Steps

Congratulations! You've successfully created your first RabbitMQ application with FS.RabbitMQ. Here's what you can explore next:

1. **[Producer Guide](producer.md)** - Learn advanced publishing techniques
2. **[Consumer Guide](consumer.md)** - Master message consumption patterns
3. **[Event-Driven Architecture](event-driven.md)** - Build event-driven applications
4. **[Error Handling](error-handling.md)** - Implement robust error handling
5. **[Performance Tuning](performance.md)** - Optimize for high throughput
6. **[Examples](examples/)** - See real-world examples

## 📝 Common Patterns

### Pattern 1: Request-Reply

```csharp
// Publisher
var correlationId = Guid.NewGuid().ToString();
await _rabbitMQ.Producer.PublishAsync(
    exchange: "requests",
    routingKey: "calculate.sum",
    message: new { Numbers = new[] { 1, 2, 3, 4, 5 } },
    properties: new BasicProperties { CorrelationId = correlationId, ReplyTo = "responses" });

// Consumer
await _rabbitMQ.Consumer.ConsumeAsync<CalculateRequest>(
    queueName: "calculate-requests",
    messageHandler: async (request, context) =>
    {
        var result = request.Numbers.Sum();
        
        await _rabbitMQ.Producer.PublishAsync(
            exchange: "",
            routingKey: context.ReplyTo,
            message: new { Result = result },
            properties: new BasicProperties { CorrelationId = context.CorrelationId });
        
        return true;
    });
```

### Pattern 2: Publish-Subscribe

```csharp
// Publisher
await _rabbitMQ.Producer.PublishAsync(
    exchange: "notifications",
    routingKey: "user.registered",
    message: new UserRegistered { UserId = userId, Email = email });

// Multiple subscribers
await _rabbitMQ.Consumer.ConsumeAsync<UserRegistered>(
    queueName: "email-service",
    messageHandler: async (user, context) =>
    {
        await _emailService.SendWelcomeEmailAsync(user.Email);
        return true;
    });

await _rabbitMQ.Consumer.ConsumeAsync<UserRegistered>(
    queueName: "analytics-service",
    messageHandler: async (user, context) =>
    {
        await _analyticsService.TrackUserRegistrationAsync(user.UserId);
        return true;
    });
```

### Pattern 3: Work Queues

```csharp
// Producer
for (int i = 0; i < 1000; i++)
{
    await _rabbitMQ.Producer.PublishAsync(
        exchange: "",
        routingKey: "work-queue",
        message: new WorkItem { Id = i, Data = $"Work item {i}" });
}

// Multiple workers
await _rabbitMQ.Consumer.ConsumeAsync<WorkItem>(
    queueName: "work-queue",
    messageHandler: async (item, context) =>
    {
        await ProcessWorkItemAsync(item);
        return true;
    });
```

## 🎉 Congratulations!

You've successfully learned the basics of FS.RabbitMQ! You can now:

✅ Set up RabbitMQ infrastructure  
✅ Publish messages to exchanges  
✅ Consume messages from queues  
✅ Handle errors and retries  
✅ Monitor your messaging system  
✅ Build scalable messaging applications  

Ready to dive deeper? Check out our [advanced guides](../README.md#-documentation) to master enterprise-grade messaging patterns! 