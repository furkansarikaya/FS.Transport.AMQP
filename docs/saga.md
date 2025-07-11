# Saga Orchestration Guide

This guide explains how to implement saga pattern for distributed transactions using FS.RabbitMQ.

## Table of Contents
- [Overview](#overview)
- [Saga Configuration](#saga-configuration)
- [Implementing Sagas](#implementing-sagas)
- [Saga State Management](#saga-state-management)
- [Error Handling](#error-handling)
- [Best Practices](#best-practices)
- [Examples](#examples)

## Overview

A saga is a sequence of local transactions that need to be coordinated across multiple services. FS.RabbitMQ provides built-in support for implementing sagas through its saga orchestration component.

## Saga Configuration

```csharp
builder.Services.AddRabbitMQ()
    .WithSaga(config =>
    {
        config.SagaStateExchange = "saga-state";
        config.SagaTimeoutExchange = "saga-timeout";
        config.SagaTypesAssembly = typeof(OrderProcessingSaga).Assembly;
    })
    .Build();
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
public class OrderProcessingSaga : SagaBase<OrderProcessingSagaState>
{
    private readonly ILogger<OrderProcessingSaga> _logger;

    public OrderProcessingSaga(ILogger<OrderProcessingSaga> logger)
    {
        _logger = logger;
    }

    // Starting the saga
    public async Task Handle(OrderCreated @event, SagaContext context)
    {
        _logger.LogInformation("Starting saga for order {OrderId}", @event.OrderId);

        // Initialize state
        State.OrderId = @event.OrderId;
        State.Amount = @event.Amount;
        State.Status = "Processing";
        State.StartedAt = DateTime.UtcNow;

        // Send command to reserve inventory
        await context.SendAsync(
            new ReserveInventory(@event.OrderId, @event.Items));
    }

    // Handle inventory reserved event
    public async Task Handle(InventoryReserved @event, SagaContext context)
    {
        State.IsInventoryReserved = true;
        State.Status = "InventoryReserved";

        // Send command to process payment
        await context.SendAsync(
            new ProcessPayment(State.OrderId, State.Amount));
    }

    // Handle payment processed event
    public async Task Handle(PaymentProcessed @event, SagaContext context)
    {
        State.IsPaymentProcessed = true;
        State.Status = "PaymentProcessed";

        // Send command to ship order
        await context.SendAsync(
            new ShipOrder(State.OrderId));
    }

    // Handle order shipped event
    public async Task Handle(OrderShipped @event, SagaContext context)
    {
        State.ShippingTrackingNumber = @event.TrackingNumber;
        State.Status = "Completed";
        State.CompletedAt = DateTime.UtcNow;

        // Complete the saga
        await context.CompleteAsync();
    }

    // Handle failures
    public async Task Handle(InventoryReservationFailed @event, SagaContext context)
    {
        State.Status = "Failed";
        await context.CompensateAsync(new CancelOrder(State.OrderId));
    }

    public async Task Handle(PaymentFailed @event, SagaContext context)
    {
        State.Status = "Failed";
        await context.CompensateAsync(new ReleaseInventory(State.OrderId));
        await context.CompensateAsync(new CancelOrder(State.OrderId));
    }
}
```

## Saga State Management

### Starting a Saga

```csharp
public class OrderService
{
    private readonly ISagaOrchestrator _sagaOrchestrator;

    public async Task CreateOrderAsync(CreateOrderRequest request)
    {
        var orderId = Guid.NewGuid();

        // Start the saga
        await _sagaOrchestrator.StartSagaAsync<OrderProcessingSaga>(
            sagaId: orderId,
            initialEvent: new OrderCreated(orderId, request.Amount));
    }
}
```

### Managing Saga State

```csharp
public class SagaStateManager
{
    private readonly ISagaOrchestrator _sagaOrchestrator;

    public async Task<SagaState> GetSagaStateAsync(Guid sagaId)
    {
        return await _sagaOrchestrator.GetSagaStateAsync(sagaId);
    }

    public async Task ResumeSagaAsync(Guid sagaId)
    {
        await _sagaOrchestrator.ResumeSagaAsync(sagaId);
    }

    public async Task CompensateSagaAsync(Guid sagaId)
    {
        await _sagaOrchestrator.CompensateSagaAsync(sagaId);
    }
}
```

## Error Handling

### Compensation Actions

```csharp
public class OrderProcessingSaga : SagaBase<OrderProcessingSagaState>
{
    // Define compensation actions
    protected override async Task DefineCompensationAsync(
        ICompensationBuilder builder)
    {
        builder
            .ForState("InventoryReserved", async context =>
            {
                await context.SendAsync(
                    new ReleaseInventory(State.OrderId));
            })
            .ForState("PaymentProcessed", async context =>
            {
                await context.SendAsync(
                    new RefundPayment(State.OrderId, State.Amount));
            });
    }

    // Handle timeout
    protected override async Task HandleTimeoutAsync(
        SagaContext context)
    {
        State.Status = "TimedOut";
        await context.CompensateAsync();
    }
}
```

### Retry Policies

```csharp
public class OrderProcessingSaga : SagaBase<OrderProcessingSagaState>
{
    protected override void ConfigureRetryPolicy(
        RetryPolicyBuilder builder)
    {
        builder
            .ForEvent<InventoryReserved>(policy =>
            {
                policy.MaxRetries = 3;
                policy.RetryDelay = TimeSpan.FromSeconds(5);
                policy.BackoffMultiplier = 2;
            })
            .ForEvent<PaymentProcessed>(policy =>
            {
                policy.MaxRetries = 5;
                policy.RetryDelay = TimeSpan.FromSeconds(10);
            });
    }
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
public class OrderProcessingSaga : SagaBase<OrderProcessingSagaState>
{
    private readonly ILogger<OrderProcessingSaga> _logger;
    private readonly IMetricsCollector _metrics;

    public OrderProcessingSaga(
        ILogger<OrderProcessingSaga> logger,
        IMetricsCollector metrics)
    {
        _logger = logger;
        _metrics = metrics;
    }

    public async Task Handle(OrderCreated @event, SagaContext context)
    {
        try
        {
            _logger.LogInformation(
                "Starting order processing saga for order {OrderId}",
                @event.OrderId);

            State.OrderId = @event.OrderId;
            State.Amount = @event.Amount;
            State.Status = "Processing";
            State.StartedAt = DateTime.UtcNow;

            _metrics.RecordSagaStarted("OrderProcessing");

            await context.SendAsync(
                new ReserveInventory(@event.OrderId, @event.Items));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex,
                "Error starting order processing saga for order {OrderId}",
                @event.OrderId);
            throw;
        }
    }

    public async Task Handle(InventoryReserved @event, SagaContext context)
    {
        try
        {
            _logger.LogInformation(
                "Inventory reserved for order {OrderId}",
                State.OrderId);

            State.IsInventoryReserved = true;
            State.Status = "InventoryReserved";

            _metrics.RecordSagaStepCompleted(
                "OrderProcessing",
                "InventoryReserved");

            await context.SendAsync(
                new ProcessPayment(State.OrderId, State.Amount));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex,
                "Error processing inventory reserved event for order {OrderId}",
                State.OrderId);
            throw;
        }
    }

    public async Task Handle(PaymentProcessed @event, SagaContext context)
    {
        try
        {
            _logger.LogInformation(
                "Payment processed for order {OrderId}",
                State.OrderId);

            State.IsPaymentProcessed = true;
            State.Status = "PaymentProcessed";

            _metrics.RecordSagaStepCompleted(
                "OrderProcessing",
                "PaymentProcessed");

            await context.SendAsync(
                new ShipOrder(State.OrderId));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex,
                "Error processing payment event for order {OrderId}",
                State.OrderId);
            throw;
        }
    }

    public async Task Handle(OrderShipped @event, SagaContext context)
    {
        try
        {
            _logger.LogInformation(
                "Order {OrderId} shipped with tracking number {TrackingNumber}",
                State.OrderId,
                @event.TrackingNumber);

            State.ShippingTrackingNumber = @event.TrackingNumber;
            State.Status = "Completed";
            State.CompletedAt = DateTime.UtcNow;

            _metrics.RecordSagaCompleted(
                "OrderProcessing",
                State.StartedAt,
                State.CompletedAt.Value);

            await context.CompleteAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex,
                "Error processing order shipped event for order {OrderId}",
                State.OrderId);
            throw;
        }
    }

    protected override async Task HandleTimeoutAsync(SagaContext context)
    {
        _logger.LogWarning(
            "Order processing saga timed out for order {OrderId}",
            State.OrderId);

        State.Status = "TimedOut";
        _metrics.RecordSagaTimedOut("OrderProcessing");

        await context.CompensateAsync();
    }

    protected override async Task DefineCompensationAsync(
        ICompensationBuilder builder)
    {
        builder
            .ForState("InventoryReserved", async context =>
            {
                _logger.LogInformation(
                    "Compensating inventory reservation for order {OrderId}",
                    State.OrderId);

                await context.SendAsync(
                    new ReleaseInventory(State.OrderId));
            })
            .ForState("PaymentProcessed", async context =>
            {
                _logger.LogInformation(
                    "Compensating payment for order {OrderId}",
                    State.OrderId);

                await context.SendAsync(
                    new RefundPayment(State.OrderId, State.Amount));
            });
    }
}
```

### Monitoring and Metrics

```csharp
public class SagaMonitor
{
    private readonly ISagaOrchestrator _sagaOrchestrator;
    private readonly ILogger<SagaMonitor> _logger;
    private readonly IMetricsCollector _metrics;

    public async Task MonitorSagaAsync(Guid sagaId)
    {
        try
        {
            var state = await _sagaOrchestrator.GetSagaStateAsync(sagaId);

            _metrics.RecordSagaState(
                state.SagaType,
                state.Status);

            if (state.Status == "Failed")
            {
                _logger.LogError(
                    "Saga {SagaType} failed for ID {SagaId}",
                    state.SagaType,
                    sagaId);

                await AlertOperatorsAsync(state);
            }

            if (state.Status == "Completed")
            {
                var duration = state.CompletedAt - state.StartedAt;
                _metrics.RecordSagaDuration(
                    state.SagaType,
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
}
``` 