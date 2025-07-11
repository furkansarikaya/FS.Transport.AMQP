# Event-Driven Architecture Guide

This guide explains how to implement event-driven architecture using FS.RabbitMQ.

## Table of Contents
- [Overview](#overview)
- [Event Types](#event-types)
- [Event Bus](#event-bus)
- [Event Handlers](#event-handlers)
- [Best Practices](#best-practices)
- [Examples](#examples)

## Overview

Event-driven architecture (EDA) is a software design pattern where components communicate through events. FS.RabbitMQ provides comprehensive support for implementing EDA through its event bus, event handlers, and event store features.

## Event Types

FS.RabbitMQ supports three main types of events:

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
builder.Services.AddRabbitMQ()
    .WithEventBus(config =>
    {
        config.DomainEventExchange = "domain-events";
        config.IntegrationEventExchange = "integration-events";
        config.EventTypesAssembly = typeof(OrderCreated).Assembly;
    })
    .Build();
```

### Publishing Events

```csharp
// Publishing domain events
await _eventBus.PublishDomainEventAsync(
    new OrderCreated(orderId, customerName, amount));

// Publishing integration events
await _eventBus.PublishIntegrationEventAsync(
    new PaymentProcessed(orderId, transactionId, amount));

// Publishing with metadata
await _eventBus.PublishDomainEventAsync(
    new OrderCreated(orderId, customerName, amount),
    metadata => {
        metadata.CorrelationId = correlationId;
        metadata.Source = "order-service";
        metadata.Headers["priority"] = "high";
    });
```

### Subscribing to Events

```csharp
// Subscribe to domain events
await _eventBus.SubscribeDomainEventAsync<OrderCreated>(
    async (@event, context) =>
    {
        await ProcessOrderCreatedEventAsync(@event);
        return true;
    });

// Subscribe to integration events
await _eventBus.SubscribeIntegrationEventAsync<PaymentProcessed>(
    async (@event, context) =>
    {
        await UpdateOrderPaymentStatusAsync(@event);
        return true;
    });
```

## Event Handlers

Event handlers process specific event types:

```csharp
public class OrderCreatedHandler : IEventHandler<OrderCreated>
{
    private readonly IEmailService _emailService;
    private readonly IInventoryService _inventoryService;

    public OrderCreatedHandler(
        IEmailService emailService,
        IInventoryService inventoryService)
    {
        _emailService = emailService;
        _inventoryService = inventoryService;
    }

    public async Task HandleAsync(OrderCreated @event, EventContext context)
    {
        // Send confirmation email
        await _emailService.SendOrderConfirmationAsync(
            @event.OrderId,
            @event.CustomerName);

        // Reserve inventory
        await _inventoryService.ReserveItemsAsync(@event.OrderId);

        // Access metadata
        var correlationId = context.Metadata.CorrelationId;
        var source = context.Metadata.Source;
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