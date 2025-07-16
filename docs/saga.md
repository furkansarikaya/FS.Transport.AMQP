# Saga Orchestration Guide

This guide explains how to implement saga pattern for distributed transactions using FS.StreamFlow.

## Table of Contents
- [Overview](#overview)
- [Saga Configuration](#saga-configuration)
- [Implementing Sagas](#implementing-sagas)
- [Saga State Management](#saga-state-management)
- [Error Handling](#error-handling)
- [Best Practices](#best-practices)
- [Examples](#examples)

## Overview

A saga is a sequence of local transactions that need to be coordinated across multiple services. FS.StreamFlow provides built-in support for implementing sagas through its saga orchestration component.

## Saga Configuration

```csharp
builder.Services.AddRabbitMQStreamFlow(options =>
{
    // Connection settings
    options.ConnectionSettings.Host = "localhost";
    options.ConnectionSettings.Port = 5672;
    options.ConnectionSettings.Username = "guest";
    options.ConnectionSettings.Password = "guest";
    options.ConnectionSettings.VirtualHost = "/";
    
    // Saga settings
    options.SagaSettings.EnableSagaOrchestration = true;
    options.SagaSettings.SagaStateExchange = "saga-state";
    options.SagaSettings.SagaTimeoutExchange = "saga-timeout";
    options.SagaSettings.SagaTimeout = TimeSpan.FromMinutes(30);
});
```

## Implementing Sagas

### Saga State

```csharp
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

### Saga Implementation

```csharp
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

    public async Task HandleAsync(OrderCreated @event, CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation(
                "Starting order processing saga for order {OrderId}",
                @event.OrderId);

            Context.Data["OrderId"] = @event.OrderId;
            Context.Data["Amount"] = @event.Amount;
            Context.Data["Status"] = "Processing";
            Context.Data["StartedAt"] = DateTime.UtcNow;
            
            State = SagaState.Running;
            Version++;
            StateChanged?.Invoke(this, new SagaStateChangedEventArgs(SagaId, State));

            await _streamFlow.Producer.Message(new ReserveInventory(@event.OrderId, @event.Items))
                .WithExchange("inventory")
                .WithRoutingKey("inventory.reserve")
                .PublishAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex,
                "Error starting order processing saga for order {OrderId}",
                @event.OrderId);
            throw;
        }
    }

    public async Task HandleAsync(InventoryReserved @event, CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation(
                "Inventory reserved for order {OrderId}",
                Context.Data["OrderId"]);

            Context.Data["IsInventoryReserved"] = true;
            Context.Data["Status"] = "InventoryReserved";
            Version++;
            
            StepCompleted?.Invoke(this, new SagaStepCompletedEventArgs(SagaId, "InventoryReserved"));

            await _streamFlow.Producer.Message(new ProcessPayment(Context.Data["OrderId"].ToString(), (decimal)Context.Data["Amount"]))
                .WithExchange("payments")
                .WithRoutingKey("payment.process")
                .PublishAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex,
                "Error processing inventory reserved event for order {OrderId}",
                Context.Data["OrderId"]);
            throw;
        }
    }

    public async Task HandleAsync(PaymentProcessed @event, CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation(
                "Payment processed for order {OrderId}",
                Context.Data["OrderId"]);

            Context.Data["IsPaymentProcessed"] = true;
            Context.Data["Status"] = "PaymentProcessed";
            Version++;
            
            StepCompleted?.Invoke(this, new SagaStepCompletedEventArgs(SagaId, "PaymentProcessed"));

            await _streamFlow.Producer.Message(new ShipOrder(Context.Data["OrderId"].ToString()))
                .WithExchange("shipping")
                .WithRoutingKey("shipping.ship")
                .PublishAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex,
                "Error processing payment event for order {OrderId}",
                Context.Data["OrderId"]);
            throw;
        }
    }

    public async Task HandleAsync(OrderShipped @event, CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation(
                "Order {OrderId} shipped with tracking number {TrackingNumber}",
                Context.Data["OrderId"],
                @event.TrackingNumber);

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
            _logger.LogError(ex,
                "Error processing order shipped event for order {OrderId}",
                Context.Data["OrderId"]);
            throw;
        }
    }

    public async Task HandleTimeoutAsync(CancellationToken cancellationToken = default)
    {
        _logger.LogWarning(
            "Order processing saga timed out for order {OrderId}",
            Context.Data["OrderId"]);

        State = SagaState.TimedOut;
        Version++;
        await CompensateAsync("Saga timed out");
    }

    // ISaga interface implementation (simplified for brevity)
    public async Task StartAsync(Dictionary<string, object> inputData, CancellationToken cancellationToken = default)
    {
        State = SagaState.Running;
        Context.Data = inputData;
        Version++;
        StateChanged?.Invoke(this, new SagaStateChangedEventArgs(SagaId, State));
    }

    public async Task ExecuteNextStepAsync(CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException("ExecuteNextStepAsync not implemented in this example");
    }

    public async Task CompensateAsync(string reason, CancellationToken cancellationToken = default)
    {
        State = SagaState.Compensating;
        Version++;
        
        // Send compensation commands
        if (Context.Data.ContainsKey("IsInventoryReserved") && (bool)Context.Data["IsInventoryReserved"])
        {
            _logger.LogInformation(
                "Compensating inventory reservation for order {OrderId}",
                Context.Data["OrderId"]);

            await _streamFlow.Producer.Message(new ReleaseInventory(Context.Data["OrderId"].ToString()))
                .WithExchange("inventory")
                .WithRoutingKey("inventory.release")
                .PublishAsync();
        }
        
        if (Context.Data.ContainsKey("IsPaymentProcessed") && (bool)Context.Data["IsPaymentProcessed"])
        {
            _logger.LogInformation(
                "Compensating payment for order {OrderId}",
                Context.Data["OrderId"]);

            await _streamFlow.Producer.Message(new RefundPayment(Context.Data["OrderId"].ToString(), (decimal)Context.Data["Amount"]))
                .WithExchange("payments")
                .WithRoutingKey("payment.refund")
                .PublishAsync();
        }
        
        State = SagaState.Aborted;
        Version++;
        CompensationTriggered?.Invoke(this, new SagaCompensationEventArgs(SagaId, reason));
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
        await _streamFlow.EventStore.Stream($"saga-{SagaId}")
            .AppendEvent(new SagaStateUpdated(SagaId, State, Context.Data))
            .SaveAsync();
    }

    public async Task RestoreAsync(string sagaId, CancellationToken cancellationToken = default)
    {
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

    public void Dispose()
    {
        // Cleanup resources
    }
}
```

## Saga State Management

### Starting a Saga

```csharp
public class OrderService
{
    private readonly IStreamFlowClient _streamFlow;

    public OrderService(IStreamFlowClient streamFlow)
    {
        _streamFlow = streamFlow;
    }

    public async Task CreateOrderAsync(CreateOrderRequest request)
    {
        // Initialize the client first
        await _streamFlow.InitializeAsync();
        
        var orderId = Guid.NewGuid();

        // Start the saga
        await _streamFlow.SagaOrchestrator.StartSagaAsync<OrderProcessingSaga>(
            sagaId: orderId,
            initialEvent: new OrderCreated(orderId, request.Amount));
    }
}
```

### Managing Saga State

```csharp
public class SagaStateManager
{
    private readonly IStreamFlowClient _streamFlow;

    public SagaStateManager(IStreamFlowClient streamFlow)
    {
        _streamFlow = streamFlow;
    }

    public async Task<SagaState> GetSagaStateAsync(Guid sagaId)
    {
        // Initialize the client first
        await _streamFlow.InitializeAsync();
        
        return await _streamFlow.SagaOrchestrator.GetSagaStateAsync(sagaId);
    }

    public async Task ResumeSagaAsync(Guid sagaId)
    {
        // Initialize the client first
        await _streamFlow.InitializeAsync();
        
        await _streamFlow.SagaOrchestrator.ResumeSagaAsync(sagaId);
    }

    public async Task CompensateSagaAsync(Guid sagaId)
    {
        // Initialize the client first
        await _streamFlow.InitializeAsync();
        
        await _streamFlow.SagaOrchestrator.CompensateSagaAsync(sagaId);
    }
}
```

## Error Handling

### Compensation Actions

```csharp
public class OrderProcessingSaga : ISaga
{
    // Note: Compensation is handled in the CompensateAsync method
    // See the main saga implementation above for complete compensation logic
    
    // Handle timeout
    public async Task HandleTimeoutAsync(CancellationToken cancellationToken = default)
    {
        State = SagaState.TimedOut;
        Version++;
        await CompensateAsync("Saga timed out");
    }
}
```

### Retry Policies

```csharp
public class OrderProcessingSaga : ISaga
{
    // Note: Retry policies are configured at the consumer level
    // See consumer.md for retry policy configuration examples
    
    // Example retry configuration in consumer setup:
    // await _streamFlow.Consumer.Queue<OrderCreated>("saga-events")
    //     .WithRetryPolicy(new RetryPolicySettings
    //     {
    //         UseExponentialBackoff = true,
    //         MaxRetryAttempts = 3,
    //         InitialRetryDelay = TimeSpan.FromSeconds(5)
    //     })
    //     .ConsumeAsync(async (orderCreated, context) =>
    //     {
    //         await HandleAsync(orderCreated);
    //         return true;
    //     });
}
```

## Best Practices

1. **Saga Design**
   - Keep sagas focused and cohesive
   - Define clear compensation actions
   - Handle timeouts appropriately
   - Use meaningful state names

2. **State Management**
   - Keep state minimal but sufficient
   - Include timestamps for tracking
   - Consider state persistence
   - Handle state transitions carefully

3. **Error Handling**
   - Implement proper compensation
   - Use appropriate retry policies
   - Handle timeouts gracefully
   - Log state transitions

4. **Monitoring**
   - Track saga progress
   - Monitor completion rates
   - Alert on failures
   - Measure performance

5. **Testing**
   - Test happy path flows
   - Test compensation scenarios
   - Test timeout handling
   - Test concurrent sagas

## Examples

### E-commerce Order Processing

```csharp
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

    public async Task HandleAsync(OrderCreated @event, CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation(
                "Starting order processing saga for order {OrderId}",
                @event.OrderId);

            Context.Data["OrderId"] = @event.OrderId;
            Context.Data["Amount"] = @event.Amount;
            Context.Data["Status"] = "Processing";
            Context.Data["StartedAt"] = DateTime.UtcNow;
            
            State = SagaState.Running;
            Version++;
            StateChanged?.Invoke(this, new SagaStateChangedEventArgs(SagaId, State));

            await _streamFlow.Producer.Message(new ReserveInventory(@event.OrderId, @event.Items))
                .WithExchange("inventory")
                .WithRoutingKey("inventory.reserve")
                .PublishAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex,
                "Error starting order processing saga for order {OrderId}",
                @event.OrderId);
            throw;
        }
    }

    public async Task HandleAsync(InventoryReserved @event, CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation(
                "Inventory reserved for order {OrderId}",
                Context.Data["OrderId"]);

            Context.Data["IsInventoryReserved"] = true;
            Context.Data["Status"] = "InventoryReserved";
            Version++;
            
            StepCompleted?.Invoke(this, new SagaStepCompletedEventArgs(SagaId, "InventoryReserved"));

            await _streamFlow.Producer.Message(new ProcessPayment(Context.Data["OrderId"].ToString(), (decimal)Context.Data["Amount"]))
                .WithExchange("payments")
                .WithRoutingKey("payment.process")
                .PublishAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex,
                "Error processing inventory reserved event for order {OrderId}",
                Context.Data["OrderId"]);
            throw;
        }
    }

    public async Task HandleAsync(PaymentProcessed @event, CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation(
                "Payment processed for order {OrderId}",
                Context.Data["OrderId"]);

            Context.Data["IsPaymentProcessed"] = true;
            Context.Data["Status"] = "PaymentProcessed";
            Version++;
            
            StepCompleted?.Invoke(this, new SagaStepCompletedEventArgs(SagaId, "PaymentProcessed"));

            await _streamFlow.Producer.Message(new ShipOrder(Context.Data["OrderId"].ToString()))
                .WithExchange("shipping")
                .WithRoutingKey("shipping.ship")
                .PublishAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex,
                "Error processing payment event for order {OrderId}",
                Context.Data["OrderId"]);
            throw;
        }
    }

    public async Task HandleAsync(OrderShipped @event, CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation(
                "Order {OrderId} shipped with tracking number {TrackingNumber}",
                Context.Data["OrderId"],
                @event.TrackingNumber);

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
            _logger.LogError(ex,
                "Error processing order shipped event for order {OrderId}",
                Context.Data["OrderId"]);
            throw;
        }
    }

    public async Task HandleTimeoutAsync(CancellationToken cancellationToken = default)
    {
        _logger.LogWarning(
            "Order processing saga timed out for order {OrderId}",
            Context.Data["OrderId"]);

        State = SagaState.TimedOut;
        Version++;
        await CompensateAsync("Saga timed out");
    }

    // ISaga interface implementation (simplified for brevity)
    public async Task StartAsync(Dictionary<string, object> inputData, CancellationToken cancellationToken = default)
    {
        State = SagaState.Running;
        Context.Data = inputData;
        Version++;
        StateChanged?.Invoke(this, new SagaStateChangedEventArgs(SagaId, State));
    }

    public async Task ExecuteNextStepAsync(CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException("ExecuteNextStepAsync not implemented in this example");
    }

    public async Task CompensateAsync(string reason, CancellationToken cancellationToken = default)
    {
        State = SagaState.Compensating;
        Version++;
        
        // Send compensation commands
        if (Context.Data.ContainsKey("IsInventoryReserved") && (bool)Context.Data["IsInventoryReserved"])
        {
            _logger.LogInformation(
                "Compensating inventory reservation for order {OrderId}",
                Context.Data["OrderId"]);

            await _streamFlow.Producer.Message(new ReleaseInventory(Context.Data["OrderId"].ToString()))
                .WithExchange("inventory")
                .WithRoutingKey("inventory.release")
                .PublishAsync();
        }
        
        if (Context.Data.ContainsKey("IsPaymentProcessed") && (bool)Context.Data["IsPaymentProcessed"])
        {
            _logger.LogInformation(
                "Compensating payment for order {OrderId}",
                Context.Data["OrderId"]);

            await _streamFlow.Producer.Message(new RefundPayment(Context.Data["OrderId"].ToString(), (decimal)Context.Data["Amount"]))
                .WithExchange("payments")
                .WithRoutingKey("payment.refund")
                .PublishAsync();
        }
        
        State = SagaState.Aborted;
        Version++;
        CompensationTriggered?.Invoke(this, new SagaCompensationEventArgs(SagaId, reason));
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
        await _streamFlow.EventStore.Stream($"saga-{SagaId}")
            .AppendEvent(new SagaStateUpdated(SagaId, State, Context.Data))
            .SaveAsync();
    }

    public async Task RestoreAsync(string sagaId, CancellationToken cancellationToken = default)
    {
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

    public void Dispose()
    {
        // Cleanup resources
    }
}
```

### Monitoring and Metrics

```csharp
public class SagaMonitor
{
    private readonly IStreamFlowClient _streamFlow;
    private readonly ILogger<SagaMonitor> _logger;

    public SagaMonitor(IStreamFlowClient streamFlow, ILogger<SagaMonitor> logger)
    {
        _streamFlow = streamFlow;
        _logger = logger;
    }

    public async Task MonitorSagaAsync(string sagaId)
    {
        try
        {
            // Initialize the client first
            await _streamFlow.InitializeAsync();
            
            var saga = await _streamFlow.SagaOrchestrator.GetSagaAsync(sagaId);
            
            if (saga == null)
            {
                _logger.LogWarning("Saga {SagaId} not found", sagaId);
                return;
            }

            if (saga.State == SagaState.Failed)
            {
                _logger.LogError(
                    "Saga {SagaType} failed for ID {SagaId}",
                    saga.SagaType,
                    sagaId);

                await AlertOperatorsAsync(saga);
            }

            if (saga.State == SagaState.Completed)
            {
                var duration = DateTime.UtcNow - saga.Context.CreatedAt;
                _logger.LogInformation(
                    "Saga {SagaType} completed for ID {SagaId} in {Duration}",
                    saga.SagaType,
                    sagaId,
                    duration);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex,
                "Error monitoring saga {SagaId}",
                sagaId);
            throw;
        }
    }

    private async Task AlertOperatorsAsync(ISaga saga)
    {
        // Send alert to operators
        await _streamFlow.Producer.Message(new SagaAlert(saga.SagaId, saga.SagaType, saga.State))
            .WithExchange("alerts")
            .WithRoutingKey("saga.failed")
            .PublishAsync();
    }
}
``` 