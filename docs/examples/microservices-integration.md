# Microservices Integration Example

**Difficulty**: 🔴 Advanced  
**Focus**: Service-to-service communication  
**Time**: 45 minutes

This example demonstrates how to implement microservices integration using FS.StreamFlow. It covers service-to-service event contracts, communication flows, error handling, and monitoring.

## 📋 What You'll Learn
- Service-to-service event contracts
- Event-driven microservices communication
- Error handling for distributed systems
- Monitoring integration events

## 🛠️ Setup

### Prerequisites
- .NET 9 SDK
- RabbitMQ server running on localhost
- Basic C# knowledge

### Project Structure
```
MicroservicesIntegration/
├── Program.cs
├── Models/
│   ├── OrderCreated.cs
│   ├── InventoryReserved.cs
│   └── PaymentProcessed.cs
├── Services/
│   ├── OrderService.cs
│   ├── InventoryService.cs
│   └── PaymentService.cs
└── MicroservicesIntegration.csproj
```

## 🏗️ Implementation

### 1. Event Contracts

```csharp
// Models/OrderCreated.cs
using FS.StreamFlow.Core.Features.Events.Interfaces;

public record OrderCreated(Guid OrderId, string CustomerId, List<string> Items) : IIntegrationEvent
{
    public Guid Id { get; } = Guid.NewGuid();
    public DateTime OccurredOn { get; } = DateTime.UtcNow;
    public int Version { get; } = 1;
    public string EventType => nameof(OrderCreated);
    public string? CorrelationId { get; set; }
    public string? CausationId { get; set; }
    public IDictionary<string, object> Metadata { get; } = new Dictionary<string, object>();
    public string Source => "order-service";
    public string RoutingKey => "order.created";
    public string? Target { get; set; }
    public string SchemaVersion => "1.0";
    public TimeSpan? TimeToLive { get; set; }
}

// Models/InventoryReserved.cs
using FS.StreamFlow.Core.Features.Events.Interfaces;

public record InventoryReserved(Guid OrderId, List<string> Items) : IIntegrationEvent
{
    public Guid Id { get; } = Guid.NewGuid();
    public DateTime OccurredOn { get; } = DateTime.UtcNow;
    public int Version { get; } = 1;
    public string EventType => nameof(InventoryReserved);
    public string? CorrelationId { get; set; }
    public string? CausationId { get; set; }
    public IDictionary<string, object> Metadata { get; } = new Dictionary<string, object>();
    public string Source => "inventory-service";
    public string RoutingKey => "inventory.reserved";
    public string? Target { get; set; }
    public string SchemaVersion => "1.0";
    public TimeSpan? TimeToLive { get; set; }
}

// Models/PaymentProcessed.cs
using FS.StreamFlow.Core.Features.Events.Interfaces;

public record PaymentProcessed(Guid OrderId, string TransactionId) : IIntegrationEvent
{
    public Guid Id { get; } = Guid.NewGuid();
    public DateTime OccurredOn { get; } = DateTime.UtcNow;
    public int Version { get; } = 1;
    public string EventType => nameof(PaymentProcessed);
    public string? CorrelationId { get; set; }
    public string? CausationId { get; set; }
    public IDictionary<string, object> Metadata { get; } = new Dictionary<string, object>();
    public string Source => "payment-service";
    public string RoutingKey => "payment.processed";
    public string? Target { get; set; }
    public string SchemaVersion => "1.0";
    public TimeSpan? TimeToLive { get; set; }
}
```

### 2. Order Service (Publisher)

```csharp
// Services/OrderService.cs
using FS.StreamFlow.Core.Features.Messaging.Interfaces;
using Microsoft.Extensions.Logging;

public class OrderService
{
    private readonly IStreamFlowClient _streamFlow;
    private readonly ILogger<OrderService> _logger;

    public OrderService(IStreamFlowClient streamFlow, ILogger<OrderService> logger)
    {
        _streamFlow = streamFlow;
        _logger = logger;
    }

    public async Task CreateOrderAsync(Guid orderId, string customerId, List<string> items)
    {
        // Initialize the client first
        await _streamFlow.InitializeAsync();
        
        // Save order to database (omitted)
        
        // Publish integration event with fluent API
        await _streamFlow.EventBus.Event<OrderCreated>()
            .WithCorrelationId(Guid.NewGuid().ToString())
            .WithSource("order-service")
            .WithVersion("1.0")
            .WithAggregateId(orderId.ToString())
            .WithAggregateType("Order")
            .PublishAsync(new OrderCreated(orderId, customerId, items));
            
        _logger.LogInformation("OrderCreated event published for Order {OrderId}", orderId);
    }
}
```

### 3. Inventory Service (Consumer/Publisher)

```csharp
// Services/InventoryService.cs
using FS.StreamFlow.Core.Features.Messaging.Interfaces;
using Microsoft.Extensions.Logging;

public class InventoryService
{
    private readonly IStreamFlowClient _streamFlow;
    private readonly ILogger<InventoryService> _logger;

    public InventoryService(IStreamFlowClient streamFlow, ILogger<InventoryService> logger)
    {
        _streamFlow = streamFlow;
        _logger = logger;
    }

    public async Task StartAsync()
    {
        // Initialize the client first
        await _streamFlow.InitializeAsync();
        
        // Setup infrastructure
        await _streamFlow.ExchangeManager.Exchange("orders")
            .AsTopic()
            .WithDurable(true)
            .DeclareAsync();
            
        await _streamFlow.QueueManager.Queue("inventory-service")
            .WithDurable(true)
            .WithDeadLetterExchange("dlx")
            .BindToExchange("orders", "order.created")
            .DeclareAsync();
        
        // Subscribe to order events
        await _streamFlow.Consumer.Queue<OrderCreated>("inventory-service")
            .WithConcurrency(3)
            .WithPrefetchCount(50)
            .WithErrorHandler(async (exception, context) =>
            {
                _logger.LogError(exception, "Error processing order event");
                return exception is ConnectFailureException;
            })
            .ConsumeAsync(async (orderCreated, context) =>
            {
                await HandleOrderCreatedAsync(orderCreated);
                return true; // Acknowledge message
            });
    }

    private async Task HandleOrderCreatedAsync(OrderCreated @event)
    {
        // Reserve inventory logic (omitted)
        
        // Publish inventory reserved event with fluent API
        await _streamFlow.EventBus.Event<InventoryReserved>()
            .WithCorrelationId(@event.CorrelationId)
            .WithCausationId(@event.Id.ToString())
            .WithSource("inventory-service")
            .WithAggregateId(@event.OrderId.ToString())
            .WithAggregateType("Order")
            .PublishAsync(new InventoryReserved(@event.OrderId, @event.Items));
            
        _logger.LogInformation("InventoryReserved event published for Order {OrderId}", @event.OrderId);
    }
}
```

### 4. Payment Service (Consumer)

```csharp
// Services/PaymentService.cs
using FS.StreamFlow.Core.Features.Messaging.Interfaces;
using Microsoft.Extensions.Logging;

public class PaymentService
{
    private readonly IStreamFlowClient _streamFlow;
    private readonly ILogger<PaymentService> _logger;

    public PaymentService(IStreamFlowClient streamFlow, ILogger<PaymentService> logger)
    {
        _streamFlow = streamFlow;
        _logger = logger;
    }

    public async Task StartAsync()
    {
        // Initialize the client first
        await _streamFlow.InitializeAsync();
        
        // Setup infrastructure
        await _streamFlow.ExchangeManager.Exchange("inventory")
            .AsTopic()
            .WithDurable(true)
            .DeclareAsync();
            
        await _streamFlow.QueueManager.Queue("payment-service")
            .WithDurable(true)
            .WithDeadLetterExchange("dlx")
            .BindToExchange("inventory", "inventory.reserved")
            .DeclareAsync();
        
        // Subscribe to inventory events
        await _streamFlow.Consumer.Queue<InventoryReserved>("payment-service")
            .WithConcurrency(2)
            .WithPrefetchCount(20)
            .WithErrorHandler(async (exception, context) =>
            {
                _logger.LogError(exception, "Error processing inventory event");
                return exception is ConnectFailureException;
            })
            .ConsumeAsync(async (inventoryReserved, context) =>
            {
                await HandleInventoryReservedAsync(inventoryReserved);
                return true; // Acknowledge message
            });
    }

    private async Task HandleInventoryReservedAsync(InventoryReserved @event)
    {
        // Process payment logic (omitted)
        
        // Publish payment processed event with fluent API
        await _streamFlow.EventBus.Event<PaymentProcessed>()
            .WithCorrelationId(@event.CorrelationId)
            .WithCausationId(@event.Id.ToString())
            .WithSource("payment-service")
            .WithAggregateId(@event.OrderId.ToString())
            .WithAggregateType("Order")
            .PublishAsync(new PaymentProcessed(@event.OrderId, "TXN123456"));
            
        _logger.LogInformation("PaymentProcessed event published for Order {OrderId}", @event.OrderId);
    }
}
```

### 5. Program.cs

```csharp
// Program.cs
using FS.StreamFlow.RabbitMQ.DependencyInjection;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

var builder = Host.CreateApplicationBuilder(args);

// Add FS.StreamFlow with RabbitMQ
builder.Services.AddRabbitMQStreamFlow(options =>
{
    // Client configuration
    options.ClientConfiguration.ClientName = "Microservices Integration";
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

// Register services
builder.Services.AddScoped<OrderService>();
builder.Services.AddScoped<InventoryService>();
builder.Services.AddScoped<PaymentService>();

var host = builder.Build();

// Initialize services
var orderService = host.Services.GetRequiredService<OrderService>();
var inventoryService = host.Services.GetRequiredService<InventoryService>();
var paymentService = host.Services.GetRequiredService<PaymentService>();

// Start consumers
await inventoryService.StartAsync();
await paymentService.StartAsync();

// Simulate order creation
await orderService.CreateOrderAsync(
    orderId: Guid.NewGuid(),
    customerId: "CUST001",
    items: new List<string> { "Item1", "Item2" });

Console.WriteLine("Microservices integration example started. Press any key to exit.");
Console.ReadKey();
```

## 🛡️ Error Handling
- All failures are routed to a dead letter queue after max retries.
- Each service logs errors and processing failures.
- Connection failures are automatically retried with exponential backoff.

## 📊 Monitoring
- Use RabbitMQ Management UI at http://localhost:15672 to monitor integration events.
- Logs show event publishing and handling in real time.
- Health checks are available for each service.

## 🎯 Key Takeaways
- Event-driven microservices enable scalable, decoupled systems.
- Error handling and monitoring are essential for distributed flows.
- FS.StreamFlow simplifies microservices integration with fluent APIs.
- Always call InitializeAsync() before using the client.
- Use proper event contracts that implement IIntegrationEvent interface. 