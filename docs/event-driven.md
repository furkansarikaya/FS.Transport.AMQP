# Event-Driven Architecture Guide

This guide explains how to implement event-driven architecture using FS.StreamFlow.

## Table of Contents
- [Overview](#overview)
- [Event Types](#event-types)
- [Event Bus](#event-bus)
- [Event Handlers](#event-handlers)
- [Best Practices](#best-practices)
- [Examples](#examples)

## Overview

Event-driven architecture (EDA) is a software design pattern where components communicate through events. FS.StreamFlow provides comprehensive support for implementing EDA through its event bus, event handlers, and event store features.

## Event Types

FS.StreamFlow supports three main types of events:

### 1. Domain Events

Domain events represent something significant that happened within your domain:

```csharp
public record OrderCreated(
    Guid OrderId,
    string CustomerName,
    decimal Amount) : IDomainEvent;

public record OrderCancelled(
    Guid OrderId,
    string Reason) : IDomainEvent;
```

### 2. Integration Events

Integration events are used for communication between different services:

```csharp
public record PaymentProcessed(
    Guid OrderId,
    string TransactionId,
    decimal Amount) : IIntegrationEvent;

public record ShipmentScheduled(
    Guid OrderId,
    DateTime EstimatedDelivery) : IIntegrationEvent;
```

### 3. Event Metadata

All events include metadata:

```csharp
public class EventMetadata
{
    public string EventId { get; set; }
    public string EventType { get; set; }
    public DateTime Timestamp { get; set; }
    public string CorrelationId { get; set; }
    public string CausationId { get; set; }
    public string Source { get; set; }
    public IDictionary<string, object> Headers { get; set; }
}
```

## Event Bus

The event bus is the central component for publishing and subscribing to events:

### Configuration

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
```

### Publishing Events with Fluent API

```csharp
// Publishing domain events with fluent API
await _streamFlow.EventBus.Event<OrderCreated>()
    .WithMetadata(metadata =>
    {
        metadata.CorrelationId = correlationId;
        metadata.Source = "order-service";
        metadata.Version = "1.0";
    })
    .WithRetryPolicy(RetryPolicyType.ExponentialBackoff)
    .WithDeadLetterHandling(enabled: true)
    .PublishAsync(new OrderCreated(orderId, customerName, amount));

// Publishing integration events with fluent API
await _streamFlow.EventBus.Event<PaymentProcessed>()
    .WithMetadata(metadata =>
    {
        metadata.CorrelationId = correlationId;
        metadata.CausationId = causationId;
        metadata.Source = "payment-service";
        metadata.Headers["priority"] = "high";
    })
    .WithRetryPolicy(RetryPolicyType.Linear)
    .WithConfirmation(timeout: TimeSpan.FromSeconds(5))
    .PublishAsync(new PaymentProcessed(orderId, transactionId, amount));

// Batch event publishing
var events = new IEvent[]
{
    new OrderCreated(orderId1, customerName1, amount1),
    new OrderCreated(orderId2, customerName2, amount2),
    new OrderCreated(orderId3, customerName3, amount3)
};

await _streamFlow.EventBus.PublishBatchAsync(events);
```

### Subscribing to Events with Fluent API

```csharp
// Subscribe to domain events using dedicated queues
await _streamFlow.Consumer.Queue<OrderCreated>("order-created-events")
    .WithConcurrency(3)
    .WithPrefetchCount(50)
    .WithErrorHandler(async (exception, context) =>
    {
        // Custom error handling
        return exception is ConnectFailureException || exception is BrokerUnreachableException;
    })
    .WithRetryPolicy(new RetryPolicySettings
    {
        RetryPolicy = RetryPolicyType.ExponentialBackoff,
        MaxRetryAttempts = 3,
        RetryDelay = TimeSpan.FromSeconds(1)
    })
    .WithDeadLetterQueue(new DeadLetterSettings
    {
        DeadLetterExchange = "event-dlx",
        DeadLetterQueue = "event-dlq"
    })
    .ConsumeAsync(async (orderCreated, context) =>
    {
        await ProcessOrderCreatedEventAsync(orderCreated, context);
        return true;
    });

// Subscribe to integration events using dedicated queues
await _streamFlow.Consumer.Queue<PaymentProcessed>("payment-processed-events")
    .WithConcurrency(5)
    .WithPrefetchCount(100)
    .WithErrorHandler(async (exception, context) =>
    {
        // Custom error handling for payment events
        return exception is ConnectFailureException || exception is BrokerUnreachableException;
    })
    .ConsumeAsync(async (paymentProcessed, context) =>
    {
        await UpdateOrderPaymentStatusAsync(paymentProcessed, context);
        return true;
    });
```

### Legacy Event Publishing API

```csharp
// Legacy API - still supported but fluent API is recommended
public class LegacyEventPublisher
{
    private readonly IStreamFlowClient _streamFlow;
    
    public async Task PublishLegacyStyleAsync()
    {
        // Publishing domain events (legacy)
        await _streamFlow.EventBus.PublishDomainEventAsync(
            new OrderCreated(orderId, customerName, amount));

        // Publishing integration events (legacy)
        await _streamFlow.EventBus.PublishIntegrationEventAsync(
            new PaymentProcessed(orderId, transactionId, amount));

        // Publishing with metadata (legacy)
        await _streamFlow.EventBus.PublishDomainEventAsync(
            new OrderCreated(orderId, customerName, amount),
            metadata => {
                metadata.CorrelationId = correlationId;
                metadata.Source = "order-service";
                metadata.Headers["priority"] = "high";
            });
    }
}
```

## Event Handlers

Event handlers process specific event types:

```csharp
public class OrderCreatedHandler : IAsyncEventHandler<OrderCreated>
{
    private readonly IEmailService _emailService;
    private readonly IInventoryService _inventoryService;
    private readonly IStreamFlowClient _streamFlow;

    public OrderCreatedHandler(
        IEmailService emailService,
        IInventoryService inventoryService,
        IStreamFlowClient streamFlow)
    {
        _emailService = emailService;
        _inventoryService = inventoryService;
        _streamFlow = streamFlow;
    }

    public async Task HandleAsync(OrderCreated @event, EventContext context)
    {
        // Send confirmation email
        await _emailService.SendOrderConfirmationAsync(
            @event.OrderId,
            @event.CustomerName);

        // Reserve inventory
        await _inventoryService.ReserveInventoryAsync(@event.OrderId);
        
        // Store event in event store
        await _streamFlow.EventStore.Stream($"order-{@event.OrderId}")
            .AppendEvent(new OrderProcessingStarted(@event.OrderId, DateTime.UtcNow))
            .SaveAsync();
        
        // Publish follow-up events
        await _streamFlow.EventBus.Event<InventoryReservationRequested>()
            .WithMetadata(metadata =>
            {
                metadata.CorrelationId = context.CorrelationId;
                metadata.CausationId = context.EventId;
                metadata.Source = "order-service";
            })
            .PublishAsync(new InventoryReservationRequested(@event.OrderId, @event.Items));
    }
}
```

### Advanced Event Handler with Infrastructure Setup

```csharp
public class AdvancedEventHandler : IAsyncEventHandler<OrderCreated>
{
    private readonly IStreamFlowClient _streamFlow;
    private readonly ILogger<AdvancedEventHandler> _logger;
    
    public async Task HandleAsync(OrderCreated @event, EventContext context)
    {
        // Setup infrastructure if needed
        await SetupInfrastructureAsync();
        
        // Complex event processing
        await ProcessEventAsync(@event, context);
    }
    
    private async Task SetupInfrastructureAsync()
    {
        // Create exchanges for follow-up events
        await _streamFlow.ExchangeManager.Exchange("inventory-events")
            .AsTopic()
            .WithDurable(true)
            .DeclareAsync();
            
        await _streamFlow.ExchangeManager.Exchange("notification-events")
            .AsFanout()
            .WithDurable(true)
            .DeclareAsync();
            
        // Create queues for processing
        await _streamFlow.QueueManager.Queue("inventory-processing")
            .WithDurable(true)
            .WithDeadLetterExchange("dlx")
            .BindToExchange("inventory-events", "inventory.reserved")
            .DeclareAsync();
    }
    
    private async Task ProcessEventAsync(OrderCreated @event, EventContext context)
    {
        try
        {
            // Business logic
            await ProcessOrderCreatedAsync(@event);
            
            // Store in event store
            await _streamFlow.EventStore.Stream($"order-{@event.OrderId}")
                .AppendEvent(new OrderProcessingStarted(@event.OrderId, DateTime.UtcNow))
                .SaveAsync();
            
            // Publish follow-up events
            await _streamFlow.EventBus.Event<InventoryReservationRequested>()
                .WithMetadata(metadata =>
                {
                    metadata.CorrelationId = context.CorrelationId;
                    metadata.CausationId = context.EventId;
                    metadata.Source = "order-service";
                })
                .PublishAsync(new InventoryReservationRequested(@event.OrderId, @event.Items));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing OrderCreated event for order {OrderId}", @event.OrderId);
            
            // Publish error event
            await _streamFlow.EventBus.Event<OrderProcessingFailed>()
                .WithMetadata(metadata =>
                {
                    metadata.CorrelationId = context.CorrelationId;
                    metadata.CausationId = context.EventId;
                    metadata.Source = "order-service";
                })
                .PublishAsync(new OrderProcessingFailed(@event.OrderId, ex.Message));
            
            throw;
        }
    }
}
```

### Registering Event Handlers

```csharp
// Register handlers with dependency injection
builder.Services.AddEventHandler<OrderCreatedHandler, OrderCreated>();
builder.Services.AddEventHandler<PaymentProcessedHandler, PaymentProcessed>();

// Or register manually
await _eventBus.RegisterHandlerAsync<OrderCreatedHandler, OrderCreated>();
```

## Best Practices

1. **Event Naming**
   - Use past tense for event names (OrderCreated, PaymentProcessed)
   - Be specific and descriptive
   - Include relevant business context

2. **Event Design**
   - Keep events immutable
   - Include only necessary data
   - Consider versioning strategy
   - Use strong typing

3. **Error Handling**
   - Implement proper error handling in handlers
   - Use dead letter queues for failed events
   - Consider retry policies

4. **Monitoring**
   - Log event processing
   - Track event processing metrics
   - Monitor handler performance

5. **Testing**
   - Unit test event handlers
   - Integration test event flow
   - Test error scenarios

## Examples

### Order Processing Flow

```csharp
public class OrderService
{
    private readonly IEventBus _eventBus;
    private readonly IOrderRepository _orderRepository;

    public async Task CreateOrderAsync(CreateOrderRequest request)
    {
        // Create order
        var order = new Order(request);
        await _orderRepository.SaveAsync(order);

        // Publish domain event
        await _eventBus.PublishDomainEventAsync(
            new OrderCreated(
                order.Id,
                order.CustomerName,
                order.Total));
    }
}

public class OrderCreatedHandler : IEventHandler<OrderCreated>
{
    private readonly IInventoryService _inventoryService;
    private readonly IEventBus _eventBus;

    public async Task HandleAsync(OrderCreated @event, EventContext context)
    {
        // Reserve inventory
        var reserved = await _inventoryService.ReserveItemsAsync(@event.OrderId);

        // Publish integration event
        await _eventBus.PublishIntegrationEventAsync(
            new InventoryReserved(@event.OrderId, reserved));
    }
}

public class InventoryReservedHandler : IEventHandler<InventoryReserved>
{
    private readonly IPaymentService _paymentService;
    private readonly IEventBus _eventBus;

    public async Task HandleAsync(InventoryReserved @event, EventContext context)
    {
        // Process payment
        var payment = await _paymentService.ProcessPaymentAsync(@event.OrderId);

        // Publish integration event
        await _eventBus.PublishIntegrationEventAsync(
            new PaymentProcessed(
                @event.OrderId,
                payment.TransactionId,
                payment.Amount));
    }
}
```

### Error Handling Example

```csharp
public class OrderCreatedHandler : IEventHandler<OrderCreated>
{
    public async Task HandleAsync(OrderCreated @event, EventContext context)
    {
        try
        {
            await ProcessOrderAsync(@event);
        }
        catch (TemporaryException ex)
        {
            // Retry later
            throw new RetryableEventException(
                "Temporary error processing order",
                ex);
        }
        catch (Exception ex)
        {
            // Move to dead letter queue
            throw new NonRetryableEventException(
                "Permanent error processing order",
                ex);
        }
    }
}
```

### Monitoring Example

```csharp
public class MonitoredEventHandler<T> : IEventHandler<T> where T : IEvent
{
    private readonly IEventHandler<T> _innerHandler;
    private readonly IMetricsCollector _metrics;
    private readonly ILogger<MonitoredEventHandler<T>> _logger;

    public async Task HandleAsync(T @event, EventContext context)
    {
        var stopwatch = Stopwatch.StartNew();

        try
        {
            await _innerHandler.HandleAsync(@event, context);

            _metrics.RecordEventProcessed(
                typeof(T).Name,
                stopwatch.Elapsed);
        }
        catch (Exception ex)
        {
            _metrics.RecordEventFailed(
                typeof(T).Name,
                ex.GetType().Name);

            _logger.LogError(ex,
                "Error processing event {EventType}",
                typeof(T).Name);

            throw;
        }
    }
}
``` 