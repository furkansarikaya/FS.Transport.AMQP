# Saga Orchestration Example

**Difficulty**: 🔴 Advanced  
**Focus**: Long-running workflow management, distributed transactions  
**Time**: 45 minutes

This example demonstrates how to implement saga orchestration using FS.StreamFlow. It covers saga state, event handling, compensation, error handling, and monitoring in a distributed order processing scenario.

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
using FS.StreamFlow.Core.Features.Events.Interfaces;
using FS.StreamFlow.Core.Features.Events.Models;
using FS.StreamFlow.Core.Features.Messaging.Interfaces;
using Microsoft.Extensions.Logging;

public class OrderProcessingSaga : ISaga
{
    private readonly ILogger<OrderProcessingSaga> _logger;
    private readonly IStreamFlowClient _streamFlow;
    
    public string SagaId { get; }
    public string SagaType => "OrderProcessing";
    public SagaState State { get; private set; }
    public long Version { get; private set; }
    public string? CorrelationId { get; set; }
    public SagaContext Context { get; }
    
    public event EventHandler<SagaStateChangedEventArgs>? StateChanged;
    public event EventHandler<SagaStepCompletedEventArgs>? StepCompleted;
    public event EventHandler<SagaStepFailedEventArgs>? StepFailed;
    public event EventHandler<SagaCompensationEventArgs>? CompensationTriggered;

    public OrderProcessingSaga(
        ILogger<OrderProcessingSaga> logger,
        IStreamFlowClient streamFlow,
        string sagaId)
    {
        _logger = logger;
        _streamFlow = streamFlow;
        SagaId = sagaId;
        Context = new SagaContext { SagaId = sagaId, SagaType = SagaType };
        State = SagaState.NotStarted;
    }

    public async Task HandleEventAsync(IEvent @event, CancellationToken cancellationToken = default)
    {
        switch (@event)
        {
            case OrderCreated orderCreated:
                await HandleOrderCreatedAsync(orderCreated, cancellationToken);
                break;
            case InventoryReserved inventoryReserved:
                await HandleInventoryReservedAsync(inventoryReserved, cancellationToken);
                break;
            case PaymentProcessed paymentProcessed:
                await HandlePaymentProcessedAsync(paymentProcessed, cancellationToken);
                break;
            case OrderShipped orderShipped:
                await HandleOrderShippedAsync(orderShipped, cancellationToken);
                break;
            default:
                _logger.LogWarning("Unknown event type: {EventType}", @event.GetType().Name);
                break;
        }
    }

    private async Task HandleOrderCreatedAsync(OrderCreated @event, CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("Starting saga for order {OrderId}", @event.OrderId);
            
            Context.Data["OrderId"] = @event.OrderId;
            Context.Data["Amount"] = @event.Amount;
            Context.Data["Status"] = "Processing";
            Context.Data["StartedAt"] = DateTime.UtcNow;
            
            State = SagaState.Running;
            Version++;
            StateChanged?.Invoke(this, new SagaStateChangedEventArgs(SagaId, State));
            
            // Initialize the client first
            await _streamFlow.InitializeAsync();
            
            await _streamFlow.Producer.Message(new ReserveInventory(@event.OrderId, @event.Items))
                .WithExchange("inventory")
                .WithRoutingKey("inventory.reserve")
                .WithCorrelationId(SagaId)
                .PublishAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to handle OrderCreated event for saga {SagaId}", SagaId);
            State = SagaState.Failed;
            Version++;
            StateChanged?.Invoke(this, new SagaStateChangedEventArgs(SagaId, State));
            throw;
        }
    }

    private async Task HandleInventoryReservedAsync(InventoryReserved @event, CancellationToken cancellationToken = default)
    {
        try
        {
            Context.Data["IsInventoryReserved"] = true;
            Context.Data["Status"] = "InventoryReserved";
            
            // Initialize the client first
            await _streamFlow.InitializeAsync();
            
            await _streamFlow.Producer.Message(new ProcessPayment(Context.Data["OrderId"].ToString(), (decimal)Context.Data["Amount"]))
                .WithExchange("payments")
                .WithRoutingKey("payment.process")
                .WithCorrelationId(SagaId)
                .PublishAsync();
                
            State = SagaState.Running;
            Version++;
            StateChanged?.Invoke(this, new SagaStateChangedEventArgs(SagaId, State));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to handle InventoryReserved event for saga {SagaId}", SagaId);
            State = SagaState.Failed;
            Version++;
            StateChanged?.Invoke(this, new SagaStateChangedEventArgs(SagaId, State));
            throw;
        }
    }

    private async Task HandlePaymentProcessedAsync(PaymentProcessed @event, CancellationToken cancellationToken = default)
    {
        try
        {
            Context.Data["IsPaymentProcessed"] = true;
            Context.Data["Status"] = "PaymentProcessed";
            
            // Initialize the client first
            await _streamFlow.InitializeAsync();
            
            await _streamFlow.Producer.Message(new ShipOrder(Context.Data["OrderId"].ToString()))
                .WithExchange("shipping")
                .WithRoutingKey("order.ship")
                .WithCorrelationId(SagaId)
                .PublishAsync();
                
            State = SagaState.Running;
            Version++;
            StateChanged?.Invoke(this, new SagaStateChangedEventArgs(SagaId, State));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to handle PaymentProcessed event for saga {SagaId}", SagaId);
            State = SagaState.Failed;
            Version++;
            StateChanged?.Invoke(this, new SagaStateChangedEventArgs(SagaId, State));
            throw;
        }
    }

    private async Task HandleOrderShippedAsync(OrderShipped @event, CancellationToken cancellationToken = default)
    {
        try
        {
            Context.Data["ShippingTrackingNumber"] = @event.TrackingNumber;
            Context.Data["Status"] = "Completed";
            Context.Data["CompletedAt"] = DateTime.UtcNow;
            
            State = SagaState.Completed;
            Version++;
            StateChanged?.Invoke(this, new SagaStateChangedEventArgs(SagaId, State));
            
            await CompleteAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to handle OrderShipped event for saga {SagaId}", SagaId);
            State = SagaState.Failed;
            Version++;
            StateChanged?.Invoke(this, new SagaStateChangedEventArgs(SagaId, State));
            throw;
        }
    }

    // Compensation for failures
    public async Task HandleAsync(InventoryReservationFailed @event, CancellationToken cancellationToken = default)
    {
        try
        {
            Context.Data["Status"] = "Failed";
            State = SagaState.Failed;
            Version++;
            StateChanged?.Invoke(this, new SagaStateChangedEventArgs(SagaId, State));
            
            await CompensateAsync("Inventory reservation failed");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to handle InventoryReservationFailed event for saga {SagaId}", SagaId);
            throw;
        }
    }

    public async Task HandleAsync(PaymentFailed @event, CancellationToken cancellationToken = default)
    {
        try
        {
            Context.Data["Status"] = "Failed";
            State = SagaState.Failed;
            Version++;
            StateChanged?.Invoke(this, new SagaStateChangedEventArgs(SagaId, State));
            
            await CompensateAsync("Payment processing failed");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to handle PaymentFailed event for saga {SagaId}", SagaId);
            throw;
        }
    }

    // ISaga interface implementation
    public async Task StartAsync(Dictionary<string, object> inputData, CancellationToken cancellationToken = default)
    {
        State = SagaState.Running;
        Context.Data = inputData;
        Version++;
        StateChanged?.Invoke(this, new SagaStateChangedEventArgs(SagaId, State));
    }

    public async Task ExecuteNextStepAsync(CancellationToken cancellationToken = default)
    {
        // Implementation depends on saga logic
        await Task.CompletedTask;
    }

    public async Task CompensateAsync(string reason, CancellationToken cancellationToken = default)
    {
        State = SagaState.Compensating;
        Version++;
        CompensationTriggered?.Invoke(this, new SagaCompensationEventArgs(SagaId, reason));
        
        // Initialize the client first
        await _streamFlow.InitializeAsync();
        
        // Send compensation commands
        if (Context.Data.ContainsKey("IsInventoryReserved") && (bool)Context.Data["IsInventoryReserved"])
        {
            await _streamFlow.Producer.Message(new CancelOrder(Context.Data["OrderId"].ToString()))
                .WithExchange("orders")
                .WithRoutingKey("order.cancel")
                .WithCorrelationId(SagaId)
                .PublishAsync();
        }
        
        State = SagaState.Aborted;
        Version++;
        StateChanged?.Invoke(this, new SagaStateChangedEventArgs(SagaId, State));
    }

    public async Task CompleteAsync(Dictionary<string, object>? result = null, CancellationToken cancellationToken = default)
    {
        State = SagaState.Completed;
        Context.Result = result;
        Version++;
        StateChanged?.Invoke(this, new SagaStateChangedEventArgs(SagaId, State));
    }

    public async Task AbortAsync(Exception error, CancellationToken cancellationToken = default)
    {
        State = SagaState.Aborted;
        Context.Error = new SagaErrorInfo
        {
            Message = error.Message,
            ErrorType = error.GetType().Name,
            StackTrace = error.StackTrace
        };
        Version++;
        StateChanged?.Invoke(this, new SagaStateChangedEventArgs(SagaId, State));
    }

    public async Task PersistAsync(CancellationToken cancellationToken = default)
    {
        // Initialize the client first
        await _streamFlow.InitializeAsync();
        
        await _streamFlow.EventStore.Stream($"saga-{SagaId}")
            .AppendEvent(new SagaStateUpdated(SagaId, State, Context.Data))
            .SaveAsync();
    }

    public async Task RestoreAsync(string sagaId, CancellationToken cancellationToken = default)
    {
        // Initialize the client first
        await _streamFlow.InitializeAsync();
        
        var events = await _streamFlow.EventStore.Stream($"saga-{sagaId}")
            .ReadAsync();
        
        foreach (var @event in events)
        {
            if (@event is SagaStateUpdated stateUpdate)
            {
                Context.Data = stateUpdate.Data;
                State = stateUpdate.State;
            }
        }
    }

    public Dictionary<string, object> GetSagaData() => Context.Data;

    public async Task UpdateDataAsync(Dictionary<string, object> data, CancellationToken cancellationToken = default)
    {
        Context.Data = data;
        Version++;
    }

    public bool CanHandle(IEvent @event) => @event switch
    {
        OrderCreated => true,
        InventoryReserved => true,
        PaymentProcessed => true,
        OrderShipped => true,
        InventoryReservationFailed => true,
        PaymentFailed => true,
        _ => false
    };

    public TimeSpan? GetTimeout() => TimeSpan.FromMinutes(30);

    public async Task HandleTimeoutAsync(CancellationToken cancellationToken = default)
    {
        _logger.LogWarning("Saga {SagaId} timed out", SagaId);
        State = SagaState.TimedOut;
        Version++;
        await CompensateAsync("Saga timed out", cancellationToken);
    }

    public async Task HandleEventAsync(IEvent @event, CancellationToken cancellationToken = default)
    {
        switch (@event)
        {
            case OrderCreated orderCreated:
                await HandleAsync(orderCreated, cancellationToken);
                break;
            case InventoryReserved inventoryReserved:
                await HandleAsync(inventoryReserved, cancellationToken);
                break;
            case PaymentProcessed paymentProcessed:
                await HandleAsync(paymentProcessed, cancellationToken);
                break;
            case OrderShipped orderShipped:
                await HandleAsync(orderShipped, cancellationToken);
                break;
            case InventoryReservationFailed inventoryReservationFailed:
                await HandleAsync(inventoryReservationFailed, cancellationToken);
                break;
            case PaymentFailed paymentFailed:
                await HandleAsync(paymentFailed, cancellationToken);
                break;
            default:
                _logger.LogWarning("Unknown event type {EventType} for saga {SagaId}", @event.GetType().Name, SagaId);
                break;
        }
    }

    public void Dispose()
    {
        // Cleanup resources
    }
}

// Event models (simplified)
public record OrderCreated(Guid OrderId, decimal Amount, List<string> Items) : IIntegrationEvent
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

public record InventoryReserved(Guid OrderId) : IIntegrationEvent
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

public record PaymentProcessed(Guid OrderId) : IIntegrationEvent
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

public record OrderShipped(Guid OrderId, string TrackingNumber) : IIntegrationEvent
{
    public Guid Id { get; } = Guid.NewGuid();
    public DateTime OccurredOn { get; } = DateTime.UtcNow;
    public int Version { get; } = 1;
    public string EventType => nameof(OrderShipped);
    public string? CorrelationId { get; set; }
    public string? CausationId { get; set; }
    public IDictionary<string, object> Metadata { get; } = new Dictionary<string, object>();
    public string Source => "shipping-service";
    public string RoutingKey => "order.shipped";
    public string? Target { get; set; }
    public string SchemaVersion => "1.0";
    public TimeSpan? TimeToLive { get; set; }
}

public record InventoryReservationFailed(Guid OrderId) : IIntegrationEvent
{
    public Guid Id { get; } = Guid.NewGuid();
    public DateTime OccurredOn { get; } = DateTime.UtcNow;
    public int Version { get; } = 1;
    public string EventType => nameof(InventoryReservationFailed);
    public string? CorrelationId { get; set; }
    public string? CausationId { get; set; }
    public IDictionary<string, object> Metadata { get; } = new Dictionary<string, object>();
    public string Source => "inventory-service";
    public string RoutingKey => "inventory.reservation.failed";
    public string? Target { get; set; }
    public string SchemaVersion => "1.0";
    public TimeSpan? TimeToLive { get; set; }
}

public record PaymentFailed(Guid OrderId) : IIntegrationEvent
{
    public Guid Id { get; } = Guid.NewGuid();
    public DateTime OccurredOn { get; } = DateTime.UtcNow;
    public int Version { get; } = 1;
    public string EventType => nameof(PaymentFailed);
    public string? CorrelationId { get; set; }
    public string? CausationId { get; set; }
    public IDictionary<string, object> Metadata { get; } = new Dictionary<string, object>();
    public string Source => "payment-service";
    public string RoutingKey => "payment.failed";
    public string? Target { get; set; }
    public string SchemaVersion => "1.0";
    public TimeSpan? TimeToLive { get; set; }
}

// Command models
public record ReserveInventory(Guid OrderId, List<string> Items);
public record ProcessPayment(string OrderId, decimal Amount);
public record ShipOrder(string OrderId);
public record CancelOrder(string OrderId);

// Event for saga state updates
public record SagaStateUpdated(string SagaId, SagaState State, Dictionary<string, object> Data);
```

### 3. Program.cs

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
    options.ClientConfiguration.ClientName = "Saga Orchestration Example";
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
    
    // Saga settings
    options.SagaSettings.EnableSagaOrchestration = true;
    options.SagaSettings.SagaStateExchange = "saga-state";
    options.SagaSettings.SagaTimeoutExchange = "saga-timeout";
    options.SagaSettings.DefaultTimeout = TimeSpan.FromMinutes(30);
    options.SagaSettings.MaxRetryAttempts = 3;
    options.SagaSettings.EnablePersistence = true;
    options.SagaSettings.EnableRecovery = true;
});

// Register saga factory
builder.Services.AddScoped<OrderProcessingSaga>(provider =>
{
    var logger = provider.GetRequiredService<ILogger<OrderProcessingSaga>>();
    var streamFlow = provider.GetRequiredService<IStreamFlowClient>();
    var sagaId = Guid.NewGuid().ToString();
    return new OrderProcessingSaga(logger, streamFlow, sagaId);
});

var host = builder.Build();

// Initialize StreamFlow client
var streamFlow = host.Services.GetRequiredService<IStreamFlowClient>();
await streamFlow.InitializeAsync();

// Setup infrastructure
await streamFlow.ExchangeManager.Exchange("saga-state")
    .AsTopic()
    .WithDurable(true)
    .DeclareAsync();
    
await streamFlow.ExchangeManager.Exchange("saga-timeout")
    .AsTopic()
    .WithDurable(true)
    .DeclareAsync();

// Start saga orchestrator
await streamFlow.SagaOrchestrator.StartAsync();

// Register saga type
await streamFlow.SagaOrchestrator.RegisterSagaTypeAsync("OrderProcessing", sagaId =>
{
    var logger = host.Services.GetRequiredService<ILogger<OrderProcessingSaga>>();
    return new OrderProcessingSaga(logger, streamFlow, sagaId);
});

// Simulate saga execution
var sagaId = await streamFlow.SagaOrchestrator.StartSagaAsync("OrderProcessing", new Dictionary<string, object>
{
    ["OrderId"] = Guid.NewGuid(),
    ["Amount"] = 99.99m,
    ["Items"] = new List<string> { "item1", "item2" }
}, correlationId: Guid.NewGuid().ToString());

Console.WriteLine($"Saga {sagaId} started. Press any key to exit.");
Console.ReadKey();
```

## 🛡️ Error Handling
- All failures are routed to a dead letter queue after max retries
- Saga compensations are triggered on failures
- Handlers log errors and processing failures
- Connection failures are automatically retried with exponential backoff

## 📊 Monitoring
- Use RabbitMQ Management UI at http://localhost:15672 to monitor saga state and events
- Logs show saga progress and compensation in real time
- Health checks are available for saga orchestrator

## 🎯 Key Takeaways
- Saga orchestration enables reliable distributed transactions
- Compensation and error handling are essential for robust workflows
- FS.StreamFlow simplifies saga management with fluent APIs
- Always call InitializeAsync() before using the client
- Use proper event contracts that implement IIntegrationEvent interface
- Implement ISaga interface for custom saga logic 