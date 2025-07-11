# Saga Orchestration Example

**Difficulty**: 🔴 Advanced  
**Focus**: Long-running workflow management, distributed transactions  
**Time**: 45 minutes

This example demonstrates how to implement saga orchestration using FS.RabbitMQ. It covers saga state, event handling, compensation, error handling, and monitoring in a distributed order processing scenario.

## 📋 What You'll Learn
- Saga orchestration patterns
- Managing distributed transactions
- Saga state management
- Compensation and error handling
- Monitoring saga workflows

## 🛠️ Setup

### Prerequisites
- .NET 9 SDK
- RabbitMQ server running on localhost
- Basic C# knowledge

### Project Structure
```
SagaOrchestration/
├── Program.cs
├── Models/
│   ├── OrderCreated.cs
│   ├── InventoryReserved.cs
│   ├── PaymentProcessed.cs
│   ├── OrderShipped.cs
│   └── SagaState.cs
├── Services/
│   ├── OrderProcessingSaga.cs
│   └── SagaHandlers.cs
└── SagaOrchestration.csproj
```

## 🏗️ Implementation

### 1. Saga State Model

```csharp
// Models/SagaState.cs
public class OrderProcessingSagaState
{
    public Guid OrderId { get; set; }
    public string Status { get; set; }
    public decimal Amount { get; set; }
    public bool IsInventoryReserved { get; set; }
    public bool IsPaymentProcessed { get; set; }
    public string ShippingTrackingNumber { get; set; }
    public DateTime StartedAt { get; set; }
    public DateTime? CompletedAt { get; set; }
}
```

### 2. Saga Implementation

```csharp
// Services/OrderProcessingSaga.cs
using FS.RabbitMQ.Saga;
using Microsoft.Extensions.Logging;

public class OrderProcessingSaga : SagaBase<OrderProcessingSagaState>
{
    private readonly ILogger<OrderProcessingSaga> _logger;

    public OrderProcessingSaga(ILogger<OrderProcessingSaga> logger)
    {
        _logger = logger;
    }

    // Start saga
    public async Task Handle(OrderCreated @event, SagaContext context)
    {
        _logger.LogInformation("Starting saga for order {OrderId}", @event.OrderId);
        State.OrderId = @event.OrderId;
        State.Amount = @event.Amount;
        State.Status = "Processing";
        State.StartedAt = DateTime.UtcNow;
        await context.SendAsync(new ReserveInventory(@event.OrderId, @event.Items));
    }

    // Inventory reserved
    public async Task Handle(InventoryReserved @event, SagaContext context)
    {
        State.IsInventoryReserved = true;
        State.Status = "InventoryReserved";
        await context.SendAsync(new ProcessPayment(State.OrderId, State.Amount));
    }

    // Payment processed
    public async Task Handle(PaymentProcessed @event, SagaContext context)
    {
        State.IsPaymentProcessed = true;
        State.Status = "PaymentProcessed";
        await context.SendAsync(new ShipOrder(State.OrderId));
    }

    // Order shipped
    public async Task Handle(OrderShipped @event, SagaContext context)
    {
        State.ShippingTrackingNumber = @event.TrackingNumber;
        State.Status = "Completed";
        State.CompletedAt = DateTime.UtcNow;
        await context.CompleteAsync();
    }

    // Compensation for failures
    public async Task Handle(InventoryReservationFailed @event, SagaContext context)
    {
        State.Status = "Failed";
        await context.CompensateAsync(new CancelOrder(State.OrderId));
    }

    public async Task Handle(PaymentFailed @event, SagaContext context)
    {
        State.Status = "Failed";
        await context.CompensateAsync(new RefundPayment(State.OrderId));
    }
}
```

### 3. Saga Handler Registration

```csharp
// Program.cs
using FS.RabbitMQ.DependencyInjection;
using Microsoft.Extensions.DependencyInjection;

var builder = Host.CreateApplicationBuilder(args);
builder.Services.AddLogging(config =>
{
    config.AddConsole();
    config.SetMinimumLevel(LogLevel.Information);
});
builder.Services.AddRabbitMQ()
    .WithConnectionString("amqp://localhost")
    .WithSaga(config =>
    {
        config.SagaStateExchange = "saga-state";
        config.SagaTimeoutExchange = "saga-timeout";
        config.SagaTypesAssembly = typeof(OrderProcessingSaga).Assembly;
    })
    .WithErrorHandling(config =>
    {
        config.EnableDeadLetterQueue = true;
        config.DeadLetterExchange = "dlx";
        config.DeadLetterQueue = "dlq";
        config.MaxRetryAttempts = 3;
        config.RetryDelay = TimeSpan.FromSeconds(2);
    })
    .Build();
builder.Services.AddSingleton<OrderProcessingSaga>();
var host = builder.Build();
```

### 4. Running the Example

1. **Start RabbitMQ**:
   ```bash
   docker run -d --name rabbitmq -p 5672:5672 -p 15672:15672 rabbitmq:3-management
   ```
2. **Run the application**:
   ```bash
   dotnet run
   ```
3. **Expected flow**:
   ```
   OrderCreated → InventoryReserved → PaymentProcessed → OrderShipped
   ```

## 🛡️ Error Handling
- All failures are routed to a dead letter queue after max retries.
- Saga compensations are triggered on failures.
- Handlers log errors and processing failures.

## 📊 Monitoring
- Use RabbitMQ Management UI at http://localhost:15672 to monitor saga state and events.
- Logs show saga progress and compensation in real time.

## 🎯 Key Takeaways
- Saga orchestration enables reliable distributed transactions.
- Compensation and error handling are essential for robust workflows.
- FS.RabbitMQ simplifies saga management in .NET. 