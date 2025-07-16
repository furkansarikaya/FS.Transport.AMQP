# Payment Processing Example

**Difficulty**: 🟡 Intermediate  
**Focus**: Payment workflows with retry and error handling  
**Time**: 30 minutes

This example demonstrates how to implement payment processing workflows using FS.StreamFlow. It covers payment request, confirmation, error handling, and monitoring.

## 📋 What You'll Learn
- Payment request and confirmation patterns
- Event-driven payment workflows
- Error handling for payment operations
- Monitoring payment events

## 🛠️ Setup

### Prerequisites
- .NET 9 SDK
- RabbitMQ server running on localhost
- Basic C# knowledge

### Project Structure
```
PaymentProcessing/
├── Program.cs
├── Models/
│   ├── PaymentRequested.cs
│   └── PaymentProcessed.cs
├── Services/
│   ├── PaymentService.cs
│   └── PaymentProcessedHandler.cs
└── PaymentProcessing.csproj
```

## 🏗️ Implementation

### 1. Payment Event Models

```csharp
// Models/PaymentRequested.cs
public record PaymentRequested(Guid OrderId, decimal Amount);

// Models/PaymentProcessed.cs
public record PaymentProcessed(Guid OrderId, string TransactionId);
```

### 2. Payment Service (Publisher)

```csharp
// Services/PaymentService.cs
using FS.StreamFlow.Core.Features.Messaging.Interfaces;
using Microsoft.Extensions.Logging;

public class PaymentService
{
    private readonly IStreamFlowClient _streamFlow;
    private readonly ILogger<PaymentService> _logger;

    public PaymentService(IStreamFlowClient rabbitMQ, ILogger<PaymentService> logger)
    {
        _streamFlow = rabbitMQ;
        _logger = logger;
    }

    public async Task RequestPaymentAsync(Guid orderId, decimal amount)
    {
        // Payment request logic (omitted)
        await _streamFlow.EventBus.PublishIntegrationEventAsync(
            new PaymentRequested(orderId, amount));
        _logger.LogInformation("PaymentRequested event published for Order {OrderId}", orderId);
    }

    public async Task ConfirmPaymentAsync(Guid orderId, string transactionId)
    {
        await _streamFlow.EventBus.PublishIntegrationEventAsync(
            new PaymentProcessed(orderId, transactionId));
        _logger.LogInformation("PaymentProcessed event published for Order {OrderId}", orderId);
    }
}
```

### 3. Payment Processed Handler (Consumer)

```csharp
// Services/PaymentProcessedHandler.cs
using FS.StreamFlow.Core.Features.Messaging.Interfaces;
using Microsoft.Extensions.Logging;

public class PaymentProcessedHandler : IEventHandler<PaymentProcessed>
{
    private readonly ILogger<PaymentProcessedHandler> _logger;

    public PaymentProcessedHandler(ILogger<PaymentProcessedHandler> logger)
    {
        _logger = logger;
    }

    public async Task HandleAsync(PaymentProcessed @event, EventContext context)
    {
        _logger.LogInformation("Handling PaymentProcessed event for Order {OrderId}", @event.OrderId);
        // Business logic (e.g., update order status, notify user)
        await Task.Delay(100); // Simulate work
    }
}
```

## 🛡️ Error Handling
- All failures are routed to a dead letter queue after max retries.
- Handler logs errors and processing failures.

## 📊 Monitoring
- Use RabbitMQ Management UI at http://localhost:15672 to monitor payment events.
- Logs show event publishing and handling in real time.

## 🎯 Key Takeaways
- Event-driven payment processing enables reliable workflows.
- Error handling and monitoring are essential for payment flows.
- FS.StreamFlow simplifies payment event workflows. 