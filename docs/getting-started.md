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
│   (Abstractions & Interfaces)       │
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

// Add FS.StreamFlow with RabbitMQ
builder.Services.AddRabbitMQStreamFlow(options =>
{
    // Connection settings
    options.ConnectionSettings.Host = "localhost";
    options.ConnectionSettings.Port = 5672;
    options.ConnectionSettings.Username = "guest";
    options.ConnectionSettings.Password = "guest";
    options.ConnectionSettings.VirtualHost = "/";
});

var app = builder.Build();
```

### 2. Configuration Options

You can configure options in multiple ways:

#### Using Lambda Expression

```csharp
builder.Services.AddRabbitMQStreamFlow(options =>
{
    // Client configuration
    options.ClientConfiguration.ClientName = "My Application";
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
    options.ConsumerSettings.PrefetchCount = 50;
    options.ConsumerSettings.AutoAcknowledge = false;
    options.ConsumerSettings.MaxConcurrentConsumers = 5;
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
        // Setup infrastructure with fluent APIs
        await _streamFlow.ExchangeManager.Exchange("orders")
            .AsTopic()
            .WithDurable(true)
            .DeclareAsync();
            
        await _streamFlow.QueueManager.Queue("order-processing")
            .WithDurable(true)
            .WithDeadLetterExchange("dlx")
            .BindToExchange("orders", "order.created")
            .DeclareAsync();
        
        // Publish a message with fluent API
        // Option 1: With pre-configured message (can use PublishAsync() without parameters)
        await _streamFlow.Producer.Message(order)
            .WithExchange("orders")
            .WithRoutingKey("order.created")
            .WithDeliveryMode(DeliveryMode.Persistent)
            .PublishAsync();

        // Option 2: With generic type (MUST pass message to PublishAsync)
        await _streamFlow.Producer.Message<Order>()
            .WithExchange("orders")
            .WithRoutingKey("order.created")
            .WithDeliveryMode(DeliveryMode.Persistent)
            .PublishAsync(order);
            
        // Store event in event store with fluent API
        await _streamFlow.EventStore.Stream($"order-{order.Id}")
            .AppendEvent(new OrderCreated(order.Id, order.CustomerName, order.Amount))
            .SaveAsync();
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
    
public record OrderCreated(
    Guid OrderId,
    string CustomerName,
    decimal Amount) : IDomainEvent;
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
        // Setup infrastructure first
        await SetupInfrastructureAsync();
        
        // Publish with fluent API
        // Option 1: With pre-configured message (can use PublishAsync() without parameters)
        var result = await _streamFlow.Producer.Message(order)
            .WithExchange("orders")
            .WithRoutingKey("order.created")
            .WithDeliveryMode(DeliveryMode.Persistent)
            .WithExpiration(TimeSpan.FromHours(24))
            .WithPriority(5)
            .PublishAsync();

        // Option 2: With generic type (MUST pass message to PublishAsync)
         var result = await _streamFlow.Producer.Message<Order>()
        //     .WithExchange("orders")
        //     .WithRoutingKey("order.created")
        //     .WithDeliveryMode(DeliveryMode.Persistent)
        //     .WithExpiration(TimeSpan.FromHours(24))
        //     .WithPriority(5)
        //     .PublishAsync(order);
        
        if (result.IsSuccess)
        {
            Console.WriteLine($"Order {order.Id} published successfully!");
        }
        else
        {
            Console.WriteLine($"Failed to publish order {order.Id}");
        }
    }
    
    private async Task SetupInfrastructureAsync()
    {
        // Create exchange with fluent API
        await _streamFlow.ExchangeManager.Exchange("orders")
            .AsTopic()
            .WithDurable(true)
            .DeclareAsync();
            
        // Create queue with fluent API
        await _streamFlow.QueueManager.Queue("order-processing")
            .WithDurable(true)
            .WithDeadLetterExchange("dlx")
            .BindToExchange("orders", "order.created")
            .DeclareAsync();
    }
}
```

### 3. Consumer Service

```csharp
using FS.StreamFlow.Core.Features.Messaging.Interfaces;

public class OrderConsumer
{
    private readonly IStreamFlowClient _streamFlow;
    
    public OrderConsumer(IStreamFlowClient streamFlow)
    {
        _streamFlow = streamFlow;
    }
    
    public async Task StartConsumingAsync()
    {
        // Consume with fluent API
        await _streamFlow.Consumer.Queue<Order>("order-processing")
            .WithConcurrency(5)
            .WithPrefetch(100)
            .WithAutoAck(false)
            .WithErrorHandling(ErrorHandlingStrategy.Retry)
            .WithRetryPolicy(RetryPolicyType.ExponentialBackoff)
            .WithMaxRetries(3)
            .WithDeadLetterQueue("dlq")
            .HandleAsync(async (order, context) =>
            {
                Console.WriteLine($"Processing order {order.Id}");
                
                // Process the order
                await ProcessOrderAsync(order);
                
                Console.WriteLine($"Order {order.Id} processed successfully!");
                return true; // Acknowledge message
            });
    }
    
    private async Task ProcessOrderAsync(Order order)
    {
        // Simulate processing
        await Task.Delay(1000);
        
        // Store event in event store with fluent API
        await _streamFlow.EventStore.Stream($"order-{order.Id}")
            .AppendEvent(new OrderCreated(order.Id, order.CustomerName, order.Amount))
            .SaveAsync();
    }
}
```

### 4. Event-Driven Processing

```csharp
// Event publisher
public class OrderEventPublisher
{
    private readonly IStreamFlowClient _streamFlow;
    
    public async Task PublishOrderCreatedAsync(OrderCreated orderCreated)
    {
        await _streamFlow.EventBus.Event<OrderCreated>()
            .WithMetadata(metadata =>
            {
                metadata.CorrelationId = Guid.NewGuid().ToString();
                metadata.Source = "order-service";
                metadata.Version = "1.0";
            })
            .WithRetryPolicy(RetryPolicyType.Linear)
            .WithDeadLetterHandling(enabled: true)
            .PublishAsync(orderCreated);
    }
}

// Event handler
public class OrderCreatedHandler : IAsyncEventHandler<OrderCreated>
{
    private readonly IStreamFlowClient _streamFlow;
    
    public async Task HandleAsync(OrderCreated orderCreated, EventContext context)
    {
        Console.WriteLine($"Handling OrderCreated event for order {orderCreated.OrderId}");
        
        // Business logic
        await ProcessOrderCreatedAsync(orderCreated);
        
        // Publish follow-up events
        await _streamFlow.EventBus.Event<InventoryRequested>()
            .WithMetadata(metadata =>
            {
                metadata.CorrelationId = context.CorrelationId;
                metadata.CausationId = context.EventId;
                metadata.Source = "order-service";
            })
            .PublishAsync(new InventoryRequested(orderCreated.OrderId, items));
    }
}
```

### 5. Infrastructure Management

```csharp
// Complete infrastructure setup
public class InfrastructureSetup
{
    private readonly IStreamFlowClient _streamFlow;
    
    public async Task SetupAsync()
    {
        // Create exchanges with fluent API
        await _streamFlow.ExchangeManager.Exchange("orders")
            .AsTopic()
            .WithDurable(true)
            .WithAlternateExchange("alt-orders")
            .DeclareAsync();
            
        await _streamFlow.ExchangeManager.Exchange("notifications")
            .AsFanout()
            .WithDurable(true)
            .DeclareAsync();
            
        await _streamFlow.ExchangeManager.Exchange("dlx")
            .AsDirect()
            .WithDurable(true)
            .DeclareAsync();
        
        // Create queues with fluent API
        await _streamFlow.QueueManager.Queue("order-processing")
            .WithDurable(true)
            .WithDeadLetterExchange("dlx")
            .WithDeadLetterRoutingKey("order.failed")
            .WithMessageTtl(TimeSpan.FromHours(24))
            .WithMaxLength(10000)
            .WithPriority(5)
            .BindToExchange("orders", "order.created")
            .BindToExchange("orders", "order.updated")
            .DeclareAsync();
            
        await _streamFlow.QueueManager.Queue("email-notifications")
            .WithDurable(true)
            .BindToExchange("notifications", "")
            .DeclareAsync();
            
        await _streamFlow.QueueManager.Queue("dlq")
            .WithDurable(true)
            .BindToExchange("dlx", "order.failed")
            .DeclareAsync();
        
        // Create event streams with fluent API
        await _streamFlow.EventStore.Stream("order-events")
            .CreateAsync();
            
        await _streamFlow.EventStore.Stream("user-events")
            .CreateAsync();
    }
}
```

### 6. Event Sourcing

```csharp
// Event store operations
public class OrderEventStore
{
    private readonly IStreamFlowClient _streamFlow;
    
    public async Task<long> SaveOrderEventsAsync(Guid orderId, IEnumerable<object> events)
    {
        return await _streamFlow.EventStore.Stream($"order-{orderId}")
            .AppendEvents(events)
            .SaveAsync();
    }
    
    public async Task<IEnumerable<object>> GetOrderEventsAsync(Guid orderId)
    {
        return await _streamFlow.EventStore.Stream($"order-{orderId}")
            .FromVersion(0)
            .ReadAsync();
    }
    
    public async Task<object?> GetOrderSnapshotAsync(Guid orderId)
    {
        return await _streamFlow.EventStore.Stream($"order-{orderId}")
            .GetSnapshotAsync();
    }
    
    public async Task SaveOrderSnapshotAsync(Guid orderId, object snapshot, long version)
    {
        await _streamFlow.EventStore.Stream($"order-{orderId}")
            .WithSnapshot(snapshot, version)
            .SaveSnapshotAsync();
    }
}
```

## Running Your Application

### 1. Complete Example

```csharp
using FS.StreamFlow.RabbitMQ.DependencyInjection;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

var builder = Host.CreateApplicationBuilder(args);

// Add logging
builder.Services.AddLogging(config =>
{
    config.AddConsole();
    config.SetMinimumLevel(LogLevel.Information);
});

// Add FS.StreamFlow
builder.Services.AddRabbitMQStreamFlow(options =>
{
    // Connection settings
    options.ConnectionSettings.Host = "localhost";
    options.ConnectionSettings.Port = 5672;
    options.ConnectionSettings.Username = "guest";
    options.ConnectionSettings.Password = "guest";
    options.ConnectionSettings.VirtualHost = "/";
    
    // Client configuration
    options.ClientConfiguration.EnableAutoRecovery = true;
    options.ClientConfiguration.HeartbeatInterval = TimeSpan.FromSeconds(60);
    
    // Producer settings
    options.ProducerSettings.EnablePublisherConfirms = true;
    
    // Consumer settings
    options.ConsumerSettings.PrefetchCount = 50;
    options.ConsumerSettings.AutoAcknowledge = false;
    options.ConsumerSettings.MaxConcurrentConsumers = 5;

});

// Add your services
builder.Services.AddSingleton<OrderPublisher>();
builder.Services.AddSingleton<OrderConsumer>();
builder.Services.AddSingleton<InfrastructureSetup>();

var host = builder.Build();

// Setup infrastructure
var infrastructureSetup = host.Services.GetRequiredService<InfrastructureSetup>();
await infrastructureSetup.SetupAsync();

// Start consumer
var consumer = host.Services.GetRequiredService<OrderConsumer>();
_ = Task.Run(async () => await consumer.StartConsumingAsync());

// Publish some test messages
var publisher = host.Services.GetRequiredService<OrderPublisher>();
for (int i = 0; i < 10; i++)
{
    var order = new Order(
        Guid.NewGuid(),
        $"Customer {i}",
        Random.Shared.Next(100, 1000),
        DateTime.UtcNow);
        
    await publisher.PublishOrderAsync(order);
    await Task.Delay(1000);
}

await host.RunAsync();
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
    // Client configuration
    options.ClientConfiguration.ClientName = "Production Application";
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
    
    // Consumer settings
    options.ConsumerSettings.PrefetchCount = 50;
    options.ConsumerSettings.AutoAcknowledge = false;
    options.ConsumerSettings.EnableDeadLetterQueue = true;
    
    // SSL settings
    options.ConnectionSettings.UseSsl = true;
    options.ConnectionSettings.Ssl = new SslSettings
    {
        Enabled = true,
        CertificatePath = "/path/to/certificate.pfx",
        CertificatePassword = "certificate-password",
        VerifyCertificate = true,
        ProtocolVersion = "Tls12"
    };
    
});
```

### Development Configuration

```csharp
builder.Services.AddRabbitMQStreamFlow(options =>
{
    // Client configuration
    options.ClientConfiguration.ClientName = "Development Application";
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
    options.ProducerSettings.EnablePublisherConfirms = false; // Faster for development
    options.ProducerSettings.MaxConcurrentPublishes = 50;
    
    // Consumer settings
    options.ConsumerSettings.PrefetchCount = 10;
    options.ConsumerSettings.AutoAcknowledge = false;
    options.ConsumerSettings.MaxConcurrentConsumers = 2;
});
```

## Health Checks

FS.StreamFlow includes built-in health checks:

```csharp
builder.Services.AddRabbitMQStreamFlow(options =>
{
    // Connection settings
    options.ConnectionSettings.Host = "localhost";
    options.ConnectionSettings.Port = 5672;
    options.ConnectionSettings.Username = "guest";
    options.ConnectionSettings.Password = "guest";
    options.ConnectionSettings.VirtualHost = "/";
    

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
            // Connection settings
            options.ConnectionSettings.Host = "localhost";
            options.ConnectionSettings.Port = 5672;
            options.ConnectionSettings.Username = "guest";
            options.ConnectionSettings.Password = "guest";
            options.ConnectionSettings.VirtualHost = "/";
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

## 🔧 Troubleshooting

### Common Producer Issues

**Problem**: `PublishAsync()` throws "Exchange must be configured before publishing"  
**Solution**: Always declare exchanges before publishing:
```csharp
await _streamFlow.ExchangeManager.Exchange("orders")
    .AsTopic()
    .WithDurable(true)
    .DeclareAsync();
```

**Problem**: `Message<T>()` with `.PublishAsync()` throws compilation error  
**Solution**: Use the correct overload:
```csharp
// ❌ Wrong - Message<T>() requires message parameter in PublishAsync
await _streamFlow.Producer.Message<Order>()
    .WithExchange("orders")
    .WithRoutingKey("order.created")
    .PublishAsync(); // COMPILATION ERROR!

// ✅ Correct - Message<T>() MUST pass message to PublishAsync
await _streamFlow.Producer.Message<Order>()
    .WithExchange("orders")
    .WithRoutingKey("order.created")
    .PublishAsync(order); // Correct!

// ✅ Alternative - Message(order) can use PublishAsync() without parameters
await _streamFlow.Producer.Message(order)
    .WithExchange("orders")
    .WithRoutingKey("order.created")
    .PublishAsync(); // Correct!
```

### Common Consumer Issues

**Problem**: Consumer not receiving messages  
**Solution**: Check queue bindings and routing keys:
```csharp
await _streamFlow.QueueManager.Queue("order-processing")
    .BindToExchange("orders", "order.created")  // Must match producer routing key
    .DeclareAsync();
```

### Connection Issues

If you're having connection problems:

```csharp
builder.Services.AddRabbitMQStreamFlow(options =>
{
    // Connection settings
    options.ConnectionSettings.Host = "localhost";
    options.ConnectionSettings.Port = 5672;
    options.ConnectionSettings.Username = "guest";
    options.ConnectionSettings.Password = "guest";
    options.ConnectionSettings.VirtualHost = "/";
    options.ConnectionSettings.ConnectionTimeout = TimeSpan.FromSeconds(30);
    
    // Client configuration
    options.ClientConfiguration.EnableAutoRecovery = true;
});
```

### Serialization Issues

By default, JSON serialization is used. For custom serialization:

```csharp
builder.Services.AddRabbitMQStreamFlow(options =>
{
    // Serialization settings
    options.ProducerSettings.Serialization.Format = SerializationFormat.Json;
    options.ProducerSettings.Serialization.EnableCompression = true;
    options.ProducerSettings.Serialization.CompressionAlgorithm = CompressionAlgorithm.Gzip;
    options.ConsumerSettings.Serialization.Format = SerializationFormat.Json;
    options.ConsumerSettings.Serialization.EnableCompression = true;
});
```

## Support

For additional help:

- Check the [Configuration Reference](configuration.md)
- Review [Examples](examples/)
- Visit the [GitHub repository](https://github.com/furkansarikaya/FS.StreamFlow)
- Report issues on [GitHub Issues](https://github.com/furkansarikaya/FS.StreamFlow/issues) 