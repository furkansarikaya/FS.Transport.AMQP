# Inventory Management Example

**Difficulty**: 🟡 Intermediate  
**Focus**: Real-time inventory updates and reservations  
**Time**: 30 minutes

This example demonstrates how to implement real-time inventory management using FS.StreamFlow. It covers inventory reservation, update flows, error handling, and monitoring.

## 📋 What You'll Learn
- Inventory reservation and update patterns
- Event-driven inventory workflows
- Error handling for inventory operations
- Monitoring inventory events

## 🛠️ Setup

### Prerequisites
- .NET 9 SDK
- RabbitMQ server running on localhost
- Basic C# knowledge

### Project Structure
```
InventoryManagement/
├── Program.cs
├── Models/
│   ├── InventoryReserved.cs
│   └── InventoryUpdated.cs
├── Services/
│   ├── InventoryService.cs
│   └── InventoryReservedHandler.cs
└── InventoryManagement.csproj
```

## 🏗️ Implementation

### 1. Inventory Event Models

```csharp
// Models/InventoryReserved.cs
public record InventoryReserved(Guid OrderId, List<string> Items);

// Models/InventoryUpdated.cs
public record InventoryUpdated(Guid ProductId, int NewStock);
```

### 2. Inventory Service (Publisher)

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

    public async Task ReserveInventoryAsync(Guid orderId, List<string> items)
    {
        // Reserve inventory logic (omitted)
        await _rabbitMQ.EventBus.PublishIntegrationEventAsync(
            new InventoryReserved(orderId, items));
        _logger.LogInformation("InventoryReserved event published for Order {OrderId}", orderId);
    }

    public async Task UpdateInventoryAsync(Guid productId, int newStock)
    {
        await _rabbitMQ.EventBus.PublishIntegrationEventAsync(
            new InventoryUpdated(productId, newStock));
        _logger.LogInformation("InventoryUpdated event published for Product {ProductId}", productId);
    }
}
```

### 3. Inventory Reserved Handler (Consumer)

```csharp
// Services/InventoryReservedHandler.cs
using FS.StreamFlow.Core.Features.Messaging.Interfaces;
using Microsoft.Extensions.Logging;

public class InventoryReservedHandler : IEventHandler<InventoryReserved>
{
    private readonly ILogger<InventoryReservedHandler> _logger;

    public InventoryReservedHandler(ILogger<InventoryReservedHandler> logger)
    {
        _logger = logger;
    }

    public async Task HandleAsync(InventoryReserved @event, EventContext context)
    {
        _logger.LogInformation("Handling InventoryReserved event for Order {OrderId}", @event.OrderId);
        // Business logic (e.g., update stock, notify user)
        await Task.Delay(100); // Simulate work
    }
}
```

## 🛡️ Error Handling
- All failures are routed to a dead letter queue after max retries.
- Handler logs errors and processing failures.

## 📊 Monitoring
- Use RabbitMQ Management UI at http://localhost:15672 to monitor inventory events.
- Logs show event publishing and handling in real time.

## 🎯 Key Takeaways
- Event-driven inventory management enables real-time updates.
- Error handling and monitoring are essential for inventory flows.
- FS.StreamFlow simplifies inventory event workflows. 