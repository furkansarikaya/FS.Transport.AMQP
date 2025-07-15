# Getting Started with FS.StreamFlow

This guide will help you get started with FS.StreamFlow, a comprehensive messaging framework for .NET 9 applications.

## What is FS.StreamFlow?

FS.StreamFlow is a production-ready messaging framework that provides:

- **Provider-agnostic abstractions** - Work with any messaging provider
- **Enterprise-grade features** - Automatic recovery, error handling, monitoring
- **Event-driven architecture** - Built-in support for domain and integration events
- **High performance** - Optimized for throughput and low latency
- **Easy integration** - Simple configuration and dependency injection

## Architecture Overview

FS.StreamFlow follows a layered architecture:

```
┌─────────────────────────────────────┐
│        Your Application             │
├─────────────────────────────────────┤
│      FS.StreamFlow.Core             │
│   (Abstractions & Interfaces)      │
├─────────────────────────────────────┤
│    FS.StreamFlow.RabbitMQ           │
│   (RabbitMQ Implementation)         │
├─────────────────────────────────────┤
│      RabbitMQ.Client                │
│    (Native RabbitMQ Client)         │
└─────────────────────────────────────┘
```

## Installation

### Package Installation

Install the RabbitMQ provider package (includes Core abstractions):

```bash
dotnet add package FS.StreamFlow.RabbitMQ
```

Or install packages separately:

```bash
# Core abstractions
dotnet add package FS.StreamFlow.Core

# RabbitMQ implementation
dotnet add package FS.StreamFlow.RabbitMQ
```

### Prerequisites

- .NET 9.0 SDK or later
- RabbitMQ server (local or remote)

## Basic Setup

### 1. Add Services to DI Container

```csharp
// Program.cs
using FS.StreamFlow.RabbitMQ.DependencyInjection;

var builder = WebApplication.CreateBuilder(args);

// Add StreamFlow with RabbitMQ
builder.Services.AddRabbitMQStreamFlow(options =>
{
    options.ConnectionString = "amqp://guest:guest@localhost:5672";
    options.EnableHealthChecks = true;
    options.EnableEventBus = true;
    options.EnableMonitoring = true;
});

var app = builder.Build();
```

### 2. Configuration Options

You can configure options in multiple ways:

#### Using Lambda Expression

```csharp
builder.Services.AddRabbitMQStreamFlow(options =>
{
    options.ConnectionString = "amqp://localhost";
    options.Connection.AutomaticRecovery = true;
    options.Connection.HeartbeatInterval = TimeSpan.FromSeconds(60);
    options.Producer.EnableConfirmations = true;
    options.Consumer.PrefetchCount = 50;
    options.ErrorHandling.EnableDeadLetterQueue = true;
});
```

#### Using Configuration File

```json
// appsettings.json
{
  "StreamFlow": {
    "RabbitMQ": {
      "ConnectionString": "amqp://localhost",
      "Connection": {
        "AutomaticRecovery": true,
        "HeartbeatInterval": "00:01:00",
        "MaxChannels": 100
      },
      "Producer": {
        "EnableConfirmations": true,
        "BatchSize": 100
      },
      "Consumer": {
        "PrefetchCount": 50,
        "ConcurrentConsumers": 5
      },
      "ErrorHandling": {
        "EnableDeadLetterQueue": true,
        "RetryPolicy": "ExponentialBackoff",
        "MaxRetryAttempts": 3
      }
    }
  }
}
```

```csharp
// Program.cs
builder.Services.Configure<RabbitMQStreamFlowOptions>(
    builder.Configuration.GetSection("StreamFlow:RabbitMQ"));
```

### 3. Inject and Use Client

```csharp
using FS.StreamFlow.Core.Features.Messaging.Interfaces;

public class OrderService
{
    private readonly IStreamFlowClient _streamFlow;
    
    public OrderService(IStreamFlowClient streamFlow)
    {
        _streamFlow = streamFlow;
    }
    
    public async Task ProcessOrderAsync(Order order)
    {
        // Publish a message
        await _streamFlow.Producer.PublishAsync(
            exchange: "orders",
            routingKey: "order.created",
            message: order);
    }
}
```

## Your First Message

### 1. Define Your Message Model

```csharp
public record Order(
    Guid Id,
    string CustomerName,
    decimal Amount,
    DateTime CreatedAt);
```

### 2. Publisher Service

```csharp
using FS.StreamFlow.Core.Features.Messaging.Interfaces;

public class OrderPublisher
{
    private readonly IStreamFlowClient _streamFlow;
    
    public OrderPublisher(IStreamFlowClient streamFlow)
    {
        _streamFlow = streamFlow;
    }
    
    public async Task PublishOrderAsync(Order order)
    {
        var result = await _streamFlow.Producer.PublishAsync(
            exchange: "orders",
            routingKey: "order.created",
            message: order);
        
        if (result.IsSuccess)
        {
            Console.WriteLine($"Order {order.Id} published successfully!");
        }
        else
        {
            Console.WriteLine($"Failed to publish order {order.Id}");
        }
    }
}
```

### 3. Consumer Service

```csharp
using FS.StreamFlow.Core.Features.Messaging.Interfaces;

public class OrderConsumer
{
    private readonly IStreamFlowClient _streamFlow;
    private readonly ILogger<OrderConsumer> _logger;
    
    public OrderConsumer(IStreamFlowClient streamFlow, ILogger<OrderConsumer> logger)
    {
        _streamFlow = streamFlow;
        _logger = logger;
    }
    
    public async Task StartConsumingAsync(CancellationToken cancellationToken = default)
    {
        await _streamFlow.Consumer.ConsumeAsync<Order>(
            queueName: "order-processing",
            messageHandler: async (order, context) =>
            {
                try
                {
                    _logger.LogInformation("Processing order {OrderId}", order.Id);
                    
                    // Process the order
                    await ProcessOrderAsync(order);
                    
                    _logger.LogInformation("Order {OrderId} processed successfully", order.Id);
                    
                    return true; // Acknowledge message
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Failed to process order {OrderId}", order.Id);
                    
                    // Return false to reject message (will be requeued or sent to DLQ)
                    return false;
                }
            },
            cancellationToken);
    }
    
    private async Task ProcessOrderAsync(Order order)
    {
        // Your business logic here
        await Task.Delay(1000); // Simulate processing
    }
}
```

### 4. Background Service for Consuming

```csharp
public class OrderConsumerBackgroundService : BackgroundService
{
    private readonly OrderConsumer _orderConsumer;
    private readonly ILogger<OrderConsumerBackgroundService> _logger;
    
    public OrderConsumerBackgroundService(
        OrderConsumer orderConsumer,
        ILogger<OrderConsumerBackgroundService> logger)
    {
        _orderConsumer = orderConsumer;
        _logger = logger;
    }
    
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("Order consumer background service started");
        
        try
        {
            await _orderConsumer.StartConsumingAsync(stoppingToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in order consumer background service");
        }
    }
}
```

### 5. Register Services

```csharp
// Program.cs
builder.Services.AddRabbitMQStreamFlow(options =>
{
    options.ConnectionString = "amqp://localhost";
    options.EnableHealthChecks = true;
});

// Register your services
builder.Services.AddScoped<OrderPublisher>();
builder.Services.AddScoped<OrderConsumer>();
builder.Services.AddHostedService<OrderConsumerBackgroundService>();

var app = builder.Build();
```

## Basic Patterns

### Request-Response Pattern

```csharp
public class OrderProcessor
{
    private readonly IStreamFlowClient _streamFlow;
    
    public OrderProcessor(IStreamFlowClient streamFlow)
    {
        _streamFlow = streamFlow;
    }
    
    public async Task<OrderResponse> ProcessOrderAsync(OrderRequest request)
    {
        // Publish request
        var result = await _streamFlow.Producer.PublishAsync(
            exchange: "orders",
            routingKey: "order.process",
            message: request);
        
        // In a real scenario, you would implement a correlation mechanism
        // to wait for the response
        
        return new OrderResponse { Success = result.IsSuccess };
    }
}
```

### Event Publishing

```csharp
using FS.StreamFlow.Core.Features.Events.Interfaces;

public record OrderCreatedEvent(Guid OrderId, string CustomerName, decimal Amount) : IDomainEvent;

public class OrderService
{
    private readonly IStreamFlowClient _streamFlow;
    
    public OrderService(IStreamFlowClient streamFlow)
    {
        _streamFlow = streamFlow;
    }
    
    public async Task CreateOrderAsync(Order order)
    {
        // Save order to database
        await SaveOrderAsync(order);
        
        // Publish domain event
        await _streamFlow.EventBus.PublishAsync(
            new OrderCreatedEvent(order.Id, order.CustomerName, order.Amount));
    }
}
```

### Error Handling

```csharp
public class OrderConsumerWithErrorHandling
{
    private readonly IStreamFlowClient _streamFlow;
    private readonly ILogger<OrderConsumerWithErrorHandling> _logger;
    
    public OrderConsumerWithErrorHandling(
        IStreamFlowClient streamFlow,
        ILogger<OrderConsumerWithErrorHandling> logger)
    {
        _streamFlow = streamFlow;
        _logger = logger;
    }
    
    public async Task StartConsumingAsync(CancellationToken cancellationToken = default)
    {
        await _streamFlow.Consumer.ConsumeAsync<Order>(
            queueName: "order-processing",
            messageHandler: async (order, context) =>
            {
                try
                {
                    await ProcessOrderAsync(order);
                    return true;
                }
                catch (TemporaryException ex)
                {
                    _logger.LogWarning(ex, "Temporary error processing order {OrderId}, will retry", order.Id);
                    return false; // Will be retried
                }
                catch (PermanentException ex)
                {
                    _logger.LogError(ex, "Permanent error processing order {OrderId}, sending to DLQ", order.Id);
                    return false; // Will be sent to dead letter queue after max retries
                }
            },
            cancellationToken);
    }
}
```

## Configuration Examples

### Production Configuration

```csharp
builder.Services.AddRabbitMQStreamFlow(options =>
{
    options.ConnectionString = builder.Configuration.GetConnectionString("RabbitMQ");
    
    // Connection settings
    options.Connection.AutomaticRecovery = true;
    options.Connection.HeartbeatInterval = TimeSpan.FromSeconds(60);
    options.Connection.MaxChannels = 100;
    options.Connection.ConnectionTimeout = TimeSpan.FromSeconds(30);
    
    // Producer settings
    options.Producer.EnableConfirmations = true;
    options.Producer.ConfirmationTimeout = TimeSpan.FromSeconds(5);
    options.Producer.BatchSize = 100;
    
    // Consumer settings
    options.Consumer.PrefetchCount = 50;
    options.Consumer.ConcurrentConsumers = 5;
    options.Consumer.AutoAck = false;
    
    // Error handling
    options.ErrorHandling.EnableDeadLetterQueue = true;
    options.ErrorHandling.DeadLetterExchange = "dlx";
    options.ErrorHandling.DeadLetterQueue = "dlq";
    options.ErrorHandling.RetryPolicy = RetryPolicyType.ExponentialBackoff;
    options.ErrorHandling.MaxRetryAttempts = 3;
    
    // SSL/TLS for production
    options.Ssl.Enabled = true;
    options.Ssl.ServerName = "rabbitmq.production.com";
    options.Ssl.CertificatePath = "/path/to/certificate.pfx";
    
    // Monitoring
    options.EnableHealthChecks = true;
    options.EnableMonitoring = true;
});
```

### Development Configuration

```csharp
builder.Services.AddRabbitMQStreamFlow(options =>
{
    options.ConnectionString = "amqp://guest:guest@localhost:5672";
    
    // Simplified settings for development
    options.Connection.AutomaticRecovery = true;
    options.Producer.EnableConfirmations = false; // Faster for development
    options.Consumer.PrefetchCount = 10;
    options.ErrorHandling.EnableDeadLetterQueue = false;
    
    // Enable health checks for development monitoring
    options.EnableHealthChecks = true;
});
```

## Health Checks

FS.StreamFlow includes built-in health checks:

```csharp
builder.Services.AddRabbitMQStreamFlow(options =>
{
    options.ConnectionString = "amqp://localhost";
    options.EnableHealthChecks = true;
    options.HealthCheck.Interval = TimeSpan.FromSeconds(30);
    options.HealthCheck.Timeout = TimeSpan.FromSeconds(5);
});

// Add health check middleware
app.UseHealthChecks("/health");
```

## Testing

### Unit Testing

```csharp
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using FS.StreamFlow.Core.Features.Messaging.Interfaces;

public class OrderServiceTests
{
    [Test]
    public async Task ProcessOrderAsync_ShouldPublishMessage()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddRabbitMQStreamFlow(options =>
        {
            options.ConnectionString = "amqp://localhost";
        });
        
        var provider = services.BuildServiceProvider();
        var streamFlow = provider.GetRequiredService<IStreamFlowClient>();
        
        var service = new OrderService(streamFlow);
        var order = new Order(Guid.NewGuid(), "John Doe", 100.00m, DateTime.UtcNow);
        
        // Act
        await service.ProcessOrderAsync(order);
        
        // Assert
        // Verify message was published (implementation depends on your testing approach)
    }
}
```

### Integration Testing

```csharp
public class OrderIntegrationTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly WebApplicationFactory<Program> _factory;
    
    public OrderIntegrationTests(WebApplicationFactory<Program> factory)
    {
        _factory = factory;
    }
    
    [Test]
    public async Task CreateOrder_ShouldPublishEvent()
    {
        // Arrange
        var client = _factory.CreateClient();
        var streamFlow = _factory.Services.GetRequiredService<IStreamFlowClient>();
        
        // Act
        var response = await client.PostAsync("/orders", 
            new StringContent(JsonSerializer.Serialize(new { CustomerName = "John", Amount = 100 })));
        
        // Assert
        response.EnsureSuccessStatusCode();
        // Verify event was published
    }
}
```

## Next Steps

Now that you have the basics working, explore these advanced features:

1. **[Event-Driven Architecture](event-driven.md)** - Build event-driven systems
2. **[Event Sourcing](event-sourcing.md)** - Implement event sourcing patterns
3. **[Saga Orchestration](saga.md)** - Handle distributed transactions
4. **[Error Handling](error-handling.md)** - Implement comprehensive error handling
5. **[Performance Tuning](performance.md)** - Optimize for high throughput
6. **[Monitoring](monitoring.md)** - Set up monitoring and observability

## Common Issues

### Connection Issues

If you're having connection problems:

```csharp
builder.Services.AddRabbitMQStreamFlow(options =>
{
    options.ConnectionString = "amqp://guest:guest@localhost:5672";
    options.Connection.ConnectionTimeout = TimeSpan.FromSeconds(30);
    options.Connection.AutomaticRecovery = true;
});
```

### Queue Declaration

To automatically declare queues:

```csharp
// This will be handled automatically by the framework
await _streamFlow.Consumer.ConsumeAsync<Order>(
    queueName: "order-processing",
    messageHandler: async (order, context) => { /* ... */ });
```

### Serialization Issues

By default, JSON serialization is used. For custom serialization:

```csharp
builder.Services.AddRabbitMQStreamFlow(options =>
{
    options.Serialization.Format = SerializationFormat.Json;
    options.Serialization.UseCompression = true;
});
```

## Support

For additional help:

- Check the [Configuration Reference](configuration.md)
- Review [Examples](examples/)
- Visit the [GitHub repository](https://github.com/furkansarikaya/FS.StreamFlow)
- Report issues on [GitHub Issues](https://github.com/furkansarikaya/FS.StreamFlow/issues) 