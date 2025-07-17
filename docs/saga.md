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
    options.ConnectionSettings.ConnectionTimeout = TimeSpan.FromSeconds(30);
    
    // Client configuration
    options.ClientConfiguration.ClientName = "Saga Orchestration Example";
    options.ClientConfiguration.EnableAutoRecovery = true;
    options.ClientConfiguration.EnableHeartbeat = true;
    options.ClientConfiguration.HeartbeatInterval = TimeSpan.FromSeconds(60);
    
    // Producer settings
    options.ProducerSettings.EnablePublisherConfirms = true;
    options.ProducerSettings.ConfirmationTimeout = TimeSpan.FromSeconds(5);
    options.ProducerSettings.MaxConcurrentPublishes = 100;
    
    // Consumer settings
    options.ConsumerSettings.PrefetchCount = 50;
    options.ConsumerSettings.AutoAcknowledge = false;
    options.ConsumerSettings.MaxConcurrentConsumers = 5;
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

    private async Task HandleInventoryReservedAsync(InventoryReserved @event, CancellationToken cancellationToken = default)
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

    private async Task HandlePaymentProcessedAsync(PaymentProcessed @event, CancellationToken cancellationToken = default)
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

    private async Task HandleOrderShippedAsync(OrderShipped @event, CancellationToken cancellationToken = default)
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
    private readonly ISagaOrchestrator _sagaOrchestrator;
    
    public OrderService(IStreamFlowClient streamFlow, ISagaOrchestrator sagaOrchestrator)
    {
        _streamFlow = streamFlow;
        _sagaOrchestrator = sagaOrchestrator;
    }
    
    public async Task CreateOrderAsync(CreateOrderRequest request)
    {
        // Initialize the client first
        await _streamFlow.InitializeAsync();
        
        var order = new Order(request.CustomerId, request.Items);
        
        // Save to database
        await _orderRepository.SaveAsync(order);
        
        // Start saga
        var sagaId = await _sagaOrchestrator.StartSagaAsync(
            sagaType: "OrderProcessing",
            inputData: new Dictionary<string, object>
            {
                ["OrderId"] = order.Id,
                ["CustomerId"] = order.CustomerId,
                ["Amount"] = order.Total,
                ["Items"] = order.Items
            },
            correlationId: order.Id.ToString());
        
        // Publish initial event
        await _streamFlow.EventBus.Event<OrderCreated>()
            .WithCorrelationId(order.Id.ToString())
            .WithSource("order-service")
            .WithAggregateId(order.Id.ToString())
            .WithAggregateType("Order")
            .PublishAsync(new OrderCreated(order.Id, order.CustomerId, order.Total));
    }
}
```

### Saga Event Handling

```csharp
public class SagaEventHandler
{
    private readonly ISagaOrchestrator _sagaOrchestrator;
    private readonly ILogger<SagaEventHandler> _logger;
    
    public SagaEventHandler(ISagaOrchestrator sagaOrchestrator, ILogger<SagaEventHandler> logger)
    {
        _sagaOrchestrator = sagaOrchestrator;
        _logger = logger;
    }
    
    public async Task HandleOrderCreatedAsync(OrderCreated @event)
    {
        try
        {
            await _sagaOrchestrator.HandleEventAsync(@event);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error handling OrderCreated event for order {OrderId}", @event.OrderId);
            throw;
        }
    }
    
    public async Task HandleInventoryReservedAsync(InventoryReserved @event)
    {
        try
        {
            await _sagaOrchestrator.HandleEventAsync(@event);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error handling InventoryReserved event for order {OrderId}", @event.OrderId);
            throw;
        }
    }
    
    public async Task HandlePaymentProcessedAsync(PaymentProcessed @event)
    {
        try
        {
            await _sagaOrchestrator.HandleEventAsync(@event);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error handling PaymentProcessed event for order {OrderId}", @event.OrderId);
            throw;
        }
    }
    
    public async Task HandleOrderShippedAsync(OrderShipped @event)
    {
        try
        {
            await _sagaOrchestrator.HandleEventAsync(@event);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error handling OrderShipped event for order {OrderId}", @event.OrderId);
            throw;
        }
    }
}
```

## Error Handling

### Saga Compensation

```csharp
public class SagaCompensationHandler
{
    private readonly IStreamFlowClient _streamFlow;
    private readonly ILogger<SagaCompensationHandler> _logger;
    
    public SagaCompensationHandler(IStreamFlowClient streamFlow, ILogger<SagaCompensationHandler> logger)
    {
        _streamFlow = streamFlow;
        _logger = logger;
    }
    
    public async Task HandleCompensationAsync(SagaCompensationEventArgs args)
    {
        _logger.LogWarning("Saga compensation triggered for saga {SagaId}: {Reason}", 
            args.SagaId, args.Reason);
        
        // Send compensation events
        await _streamFlow.EventBus.Event<SagaCompensationTriggered>()
            .WithCorrelationId(args.SagaId)
            .WithSource("saga-orchestrator")
            .PublishAsync(new SagaCompensationTriggered(args.SagaId, args.Reason));
    }
}
```

### Saga Timeout Handling

```csharp
public class SagaTimeoutHandler
{
    private readonly ISagaOrchestrator _sagaOrchestrator;
    private readonly ILogger<SagaTimeoutHandler> _logger;
    
    public SagaTimeoutHandler(ISagaOrchestrator sagaOrchestrator, ILogger<SagaTimeoutHandler> logger)
    {
        _sagaOrchestrator = sagaOrchestrator;
        _logger = logger;
    }
    
    public async Task HandleTimeoutAsync(string sagaId)
    {
        try
        {
            var saga = await _sagaOrchestrator.GetSagaAsync(sagaId);
            if (saga != null)
            {
                await saga.HandleTimeoutAsync();
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error handling timeout for saga {SagaId}", sagaId);
        }
    }
}
```

## Best Practices

### 1. Saga Design Principles

- **Keep sagas short**: Long-running sagas are harder to manage and debug
- **Use compensating actions**: Always provide compensation for each step
- **Handle timeouts**: Set appropriate timeouts for saga steps
- **Monitor saga state**: Track saga progress and failures

### 2. Event Design

```csharp
// Good: Clear event structure
public record OrderCreated(Guid OrderId, string CustomerId, decimal Amount) : IDomainEvent
{
    public Guid Id { get; } = Guid.NewGuid();
    public DateTime OccurredOn { get; } = DateTime.UtcNow;
    public int Version { get; } = 1;
    public string EventType => nameof(OrderCreated);
    public string? CorrelationId { get; set; }
    public string? CausationId { get; set; }
    public IDictionary<string, object> Metadata { get; } = new Dictionary<string, object>();
    public string AggregateId => OrderId.ToString();
    public string AggregateType => "Order";
    public long AggregateVersion { get; set; }
    public string? InitiatedBy { get; set; }
}

// Good: Compensation event
public record InventoryReservationFailed(Guid OrderId, string Reason) : IDomainEvent
{
    public Guid Id { get; } = Guid.NewGuid();
    public DateTime OccurredOn { get; } = DateTime.UtcNow;
    public int Version { get; } = 1;
    public string EventType => nameof(InventoryReservationFailed);
    public string? CorrelationId { get; set; }
    public string? CausationId { get; set; }
    public IDictionary<string, object> Metadata { get; } = new Dictionary<string, object>();
    public string AggregateId => OrderId.ToString();
    public string AggregateType => "Order";
    public long AggregateVersion { get; set; }
    public string? InitiatedBy { get; set; }
}
```

### 3. Saga Registration

```csharp
public class SagaRegistrationService
{
    private readonly ISagaOrchestrator _sagaOrchestrator;
    private readonly IServiceProvider _serviceProvider;
    
    public SagaRegistrationService(ISagaOrchestrator sagaOrchestrator, IServiceProvider serviceProvider)
    {
        _sagaOrchestrator = sagaOrchestrator;
        _serviceProvider = serviceProvider;
    }
    
    public async Task RegisterSagasAsync()
    {
        // Register saga factory
        await _sagaOrchestrator.RegisterSagaTypeAsync("OrderProcessing", sagaId =>
        {
            var logger = _serviceProvider.GetRequiredService<ILogger<OrderProcessingSaga>>();
            var streamFlow = _serviceProvider.GetRequiredService<IStreamFlowClient>();
            return new OrderProcessingSaga(logger, streamFlow, sagaId);
        });
    }
}
```

## Examples

### Complete Saga Example

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
    options.ConnectionSettings.ConnectionTimeout = TimeSpan.FromSeconds(30);
    
    // Client configuration
    options.ClientConfiguration.ClientName = "Saga Example";
    options.ClientConfiguration.EnableAutoRecovery = true;
    options.ClientConfiguration.EnableHeartbeat = true;
    options.ClientConfiguration.HeartbeatInterval = TimeSpan.FromSeconds(60);
    
    // Producer settings
    options.ProducerSettings.EnablePublisherConfirms = true;
    options.ProducerSettings.ConfirmationTimeout = TimeSpan.FromSeconds(5);
    options.ProducerSettings.MaxConcurrentPublishes = 100;
    
    // Consumer settings
    options.ConsumerSettings.PrefetchCount = 50;
    options.ConsumerSettings.AutoAcknowledge = false;
    options.ConsumerSettings.MaxConcurrentConsumers = 5;
});

// Register services
builder.Services.AddScoped<OrderService>();
builder.Services.AddScoped<SagaEventHandler>();
builder.Services.AddScoped<SagaCompensationHandler>();
builder.Services.AddScoped<SagaTimeoutHandler>();
builder.Services.AddScoped<SagaRegistrationService>();

var app = builder.Build();

// Initialize saga registration
var sagaRegistrationService = app.Services.GetRequiredService<SagaRegistrationService>();
await sagaRegistrationService.RegisterSagasAsync();

// Initialize the client
var streamFlow = app.Services.GetRequiredService<IStreamFlowClient>();
await streamFlow.InitializeAsync();

app.Run();
```

### Monitoring and Metrics

```csharp
public class SagaMonitor
{
    private readonly ISagaOrchestrator _sagaOrchestrator;
    private readonly ILogger<SagaMonitor> _logger;
    
    public SagaMonitor(ISagaOrchestrator sagaOrchestrator, ILogger<SagaMonitor> logger)
    {
        _sagaOrchestrator = sagaOrchestrator;
        _logger = logger;
        
        // Subscribe to saga events
        _sagaOrchestrator.SagaStateChanged += OnSagaStateChanged;
        _sagaOrchestrator.SagaStepCompleted += OnSagaStepCompleted;
        _sagaOrchestrator.SagaStepFailed += OnSagaStepFailed;
        _sagaOrchestrator.SagaCompensationTriggered += OnSagaCompensationTriggered;
    }
    
    private void OnSagaStateChanged(object? sender, SagaStateChangedEventArgs args)
    {
        _logger.LogInformation("Saga {SagaId} state changed to {State}", args.SagaId, args.State);
    }
    
    private void OnSagaStepCompleted(object? sender, SagaStepCompletedEventArgs args)
    {
        _logger.LogInformation("Saga {SagaId} step {Step} completed", args.SagaId, args.StepName);
    }
    
    private void OnSagaStepFailed(object? sender, SagaStepFailedEventArgs args)
    {
        _logger.LogError("Saga {SagaId} step {Step} failed: {Error}", 
            args.SagaId, args.StepName, args.Error);
    }
    
    private void OnSagaCompensationTriggered(object? sender, SagaCompensationEventArgs args)
    {
        _logger.LogWarning("Saga {SagaId} compensation triggered: {Reason}", 
            args.SagaId, args.Reason);
    }
    
    public async Task<Dictionary<string, object>> GetSagaMetricsAsync()
    {
        var activeSagas = await _sagaOrchestrator.GetActiveSagasAsync();
        
        return new Dictionary<string, object>
        {
            ["ActiveSagas"] = activeSagas.Count(),
            ["RunningSagas"] = activeSagas.Count(s => s.State == SagaState.Running),
            ["CompletedSagas"] = activeSagas.Count(s => s.State == SagaState.Completed),
            ["FailedSagas"] = activeSagas.Count(s => s.State == SagaState.Failed),
            ["CompensatingSagas"] = activeSagas.Count(s => s.State == SagaState.Compensating)
        };
    }
}
```

This guide provides a comprehensive overview of saga orchestration with FS.StreamFlow, including implementation details, error handling, and best practices for building reliable distributed workflows. 