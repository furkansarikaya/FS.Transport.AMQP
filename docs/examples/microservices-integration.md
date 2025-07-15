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
public record OrderCreated(Guid OrderId, string CustomerId, List<string> Items);

// Models/InventoryReserved.cs
public record InventoryReserved(Guid OrderId, List<string> Items);

// Models/PaymentProcessed.cs
public record PaymentProcessed(Guid OrderId, string TransactionId);
```

### 2. Order Service (Publisher)

```csharp
// Services/OrderService.cs
using FS.StreamFlow.Core.Features.Messaging.Interfaces;
using Microsoft.Extensions.Logging;

public class OrderService
{
    private readonly IRabbitMQClient _rabbitMQ;
    private readonly ILogger<OrderService> _logger;

    public OrderService(IRabbitMQClient rabbitMQ, ILogger<OrderService> logger)
    {
        _rabbitMQ = rabbitMQ;
        _logger = logger;
    }

    public async Task CreateOrderAsync(Guid orderId, string customerId, List<string> items)
    {
        // Save order to database (omitted)
        await _rabbitMQ.EventBus.PublishIntegrationEventAsync(
            new OrderCreated(orderId, customerId, items));
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
    private readonly IRabbitMQClient _rabbitMQ;
    private readonly ILogger<InventoryService> _logger;

    public InventoryService(IRabbitMQClient rabbitMQ, ILogger<InventoryService> logger)
    {
        _rabbitMQ = rabbitMQ;
        _logger = logger;
    }

    public async Task HandleOrderCreatedAsync(OrderCreated @event)
    {
        // Reserve inventory logic (omitted)
        await _rabbitMQ.EventBus.PublishIntegrationEventAsync(
            new InventoryReserved(@event.OrderId, @event.Items));
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
    private readonly IRabbitMQClient _rabbitMQ;
    private readonly ILogger<PaymentService> _logger;

    public PaymentService(IRabbitMQClient rabbitMQ, ILogger<PaymentService> logger)
    {
        _rabbitMQ = rabbitMQ;
        _logger = logger;
    }

    public async Task HandleInventoryReservedAsync(InventoryReserved @event)
    {
        // Process payment logic (omitted)
        await _rabbitMQ.EventBus.PublishIntegrationEventAsync(
            new PaymentProcessed(@event.OrderId, "TXN123456"));
        _logger.LogInformation("PaymentProcessed event published for Order {OrderId}", @event.OrderId);
    }
}
```

## 🛡️ Error Handling
- All failures are routed to a dead letter queue after max retries.
- Each service logs errors and processing failures.

## 📊 Monitoring
- Use RabbitMQ Management UI at http://localhost:15672 to monitor integration events.
- Logs show event publishing and handling in real time.

## 🎯 Key Takeaways
- Event-driven microservices enable scalable, decoupled systems.
- Error handling and monitoring are essential for distributed flows.
- FS.StreamFlow simplifies microservices integration. 