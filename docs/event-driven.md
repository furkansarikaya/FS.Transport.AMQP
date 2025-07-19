# Event-Driven Architecture Guide

This guide explains how to implement event-driven architecture using FS.StreamFlow with RabbitMQ.

## Table of Contents
- [Overview](#overview)
- [Event Types](#event-types)
- [Event Bus Setup](#event-bus-setup)
- [Event Publishing](#event-publishing)
- [Event Handling](#event-handling)
- [Best Practices](#best-practices)

## Overview

Event-driven architecture (EDA) is a software design pattern where components communicate through events. FS.StreamFlow provides comprehensive support for implementing EDA through its event bus, event handlers, and event store features.

**🚀 Key Features:**
- **Automatic Infrastructure**: No manual exchange/queue creation needed
- **Fanout Exchanges**: Simplified event distribution without routing keys
- **Type-Safe Events**: Strongly typed event models with interfaces
- **Fluent API**: Chainable configuration and publishing

## Event Types

### 1. Domain Events

Domain events represent something significant that happened within your domain:

```csharp
public record OrderCreated(
    Guid OrderId,
    string CustomerName,
    decimal Amount
) : IDomainEvent
{
    public Guid Id { get; init; } = Guid.NewGuid();
    public DateTime OccurredOn { get; init; } = DateTime.UtcNow;
    public int Version { get; init; } = 1;
    public string EventType { get; init; } = nameof(OrderCreated);
    public string? CorrelationId { get; init; }
    public string? CausationId { get; init; }
    public IDictionary<string, object> Metadata { get; init; } = new Dictionary<string, object>();
    
    // Domain event specific properties
    public string AggregateId { get; init; } = OrderId.ToString();
    public string AggregateType { get; init; } = "Order";
    public long AggregateVersion { get; init; } = 1;
    public string? InitiatedBy { get; init; }
}
```

### 2. Integration Events

Integration events are used for communication between different services:

```csharp
public record PaymentProcessed(
    Guid OrderId,
    string TransactionId,
    decimal Amount
) : IIntegrationEvent
{
    public Guid Id { get; init; } = Guid.NewGuid();
    public DateTime OccurredOn { get; init; } = DateTime.UtcNow;
    public int Version { get; init; } = 1;
    public string EventType { get; init; } = nameof(PaymentProcessed);
    public string? CorrelationId { get; init; }
    public string? CausationId { get; init; }
    public IDictionary<string, object> Metadata { get; init; } = new Dictionary<string, object>();
    
    // Integration event specific properties
    public string Source { get; init; } = "payment-service";
    public string ExchangeName { get; init; } = "payment-events"; // 🔥 Used for exchange naming
    public string? Target { get; init; }
    public string SchemaVersion { get; init; } = "1.0";
    public TimeSpan? TimeToLive { get; init; }
}

public record ShipmentScheduled(
    Guid OrderId,
    DateTime EstimatedDelivery
) : IIntegrationEvent
{
    public Guid Id { get; init; } = Guid.NewGuid();
    public DateTime OccurredOn { get; init; } = DateTime.UtcNow;
    public int Version { get; init; } = 1;
    public string EventType { get; init; } = nameof(ShipmentScheduled);
    public string? CorrelationId { get; init; }
    public string? CausationId { get; init; }
    public IDictionary<string, object> Metadata { get; init; } = new Dictionary<string, object>();
    
    public string Source { get; init; } = "shipping-service";
    public string ExchangeName { get; init; } = "shipping-events"; // 🔥 Service-based exchange naming
    public string? Target { get; init; }
    public string SchemaVersion { get; init; } = "1.0";
    public TimeSpan? TimeToLive { get; init; }
}
```

## Event Bus Setup

### 1. Service Registration

```csharp
var builder = WebApplication.CreateBuilder(args);

// Add FS.StreamFlow services with RabbitMQ
builder.Services.AddStreamFlow(options =>
{
    options.UseRabbitMQ(rabbitMqOptions =>
    {
        // Connection settings
        rabbitMqOptions.ConnectionString = "amqp://localhost:5672";
        rabbitMqOptions.VirtualHost = "/";
        rabbitMqOptions.Username = "guest";
        rabbitMqOptions.Password = "guest";
        
        // Client configuration
        rabbitMqOptions.ClientConfiguration.ClientName = "Event-Driven Application";
        rabbitMqOptions.ClientConfiguration.EnableAutoRecovery = true;
        rabbitMqOptions.ClientConfiguration.EnableHeartbeat = true;
        
        // Producer settings
        rabbitMqOptions.ProducerSettings.EnableConfirmation = true;
        rabbitMqOptions.ProducerSettings.EnablePersistence = true;
        
        // Consumer settings
        rabbitMqOptions.ConsumerSettings.AutoAcknowledge = false;
        rabbitMqOptions.ConsumerSettings.MaxConcurrentConsumers = 5;
        
        // Error handling settings
        rabbitMqOptions.ErrorHandlingSettings.MaxRetries = 3;
        rabbitMqOptions.ErrorHandlingSettings.RetryDelay = TimeSpan.FromSeconds(2);
        rabbitMqOptions.ErrorHandlingSettings.UseExponentialBackoff = true;
        rabbitMqOptions.ErrorHandlingSettings.EnableDeadLetterQueue = true;
    });
});

// Register event handlers
builder.Services.AddScoped<IAsyncEventHandler<OrderCreated>, OrderCreatedHandler>();
builder.Services.AddScoped<IAsyncEventHandler<PaymentProcessed>, PaymentProcessedHandler>();

var app = builder.Build();

// Initialize the StreamFlow client
var streamFlow = app.Services.GetRequiredService<IStreamFlowClient>();
await streamFlow.InitializeAsync();

// ✅ No manual exchange creation needed!
// EventBus automatically creates exchanges and queues when publishing/subscribing

// Start the event bus (CRITICAL: Event bus must be started explicitly)
await streamFlow.EventBus.StartAsync();

app.Run();
```

### 2. Automatic Infrastructure Management

🚀 **EventBus automatically handles all RabbitMQ infrastructure creation:**

#### Domain Events:
- **Exchange**: Automatically created as `"domain.{aggregateType}"` (e.g., `"domain.order"`)
- **Type**: Fanout exchange (no routing keys needed)
- **Queue**: Auto-created with pattern `"{exchangeName}.{eventType}"`

#### Integration Events:
- **Exchange**: Uses the `ExchangeName` property from your event
- **Type**: Fanout exchange (simplified event distribution)
- **Queue**: Auto-created with pattern `"{exchangeName}.{eventType}"`

#### Infrastructure Properties:
- All exchanges and queues are **durable** by default
- **Production-ready** configuration out of the box
- **No manual setup** required - just publish and subscribe!

## Event Publishing

### 1. Basic Event Publishing

```csharp
public class OrderService
{
    private readonly IStreamFlowClient _streamFlow;
    private readonly ILogger<OrderService> _logger;

    public OrderService(IStreamFlowClient streamFlow, ILogger<OrderService> logger)
    {
        _streamFlow = streamFlow;
        _logger = logger;
    }

    public async Task CreateOrderAsync(CreateOrderRequest request)
    {
        var orderId = Guid.NewGuid();
        var correlationId = Guid.NewGuid().ToString();

        // Create the domain event
        var orderCreatedEvent = new OrderCreated(orderId, request.CustomerName, request.Amount)
        {
            CorrelationId = correlationId,
            InitiatedBy = "order-service"
        };

        // Publish the domain event - automatically creates "domain.order" exchange
        await _streamFlow.EventBus.PublishDomainEventAsync(orderCreatedEvent);
        
        _logger.LogInformation("Order created event published: {OrderId}", orderId);
    }

    public async Task ProcessPaymentAsync(Guid orderId, string transactionId, decimal amount)
    {
        var correlationId = Guid.NewGuid().ToString();

        // Create the integration event
        var paymentProcessedEvent = new PaymentProcessed(orderId, transactionId, amount)
        {
            CorrelationId = correlationId,
            CausationId = orderId.ToString()
        };

        // Publish the integration event - uses ExchangeName property for exchange
        await _streamFlow.EventBus.PublishIntegrationEventAsync(paymentProcessedEvent);
        
        _logger.LogInformation("Payment processed event published: {OrderId}", orderId);
    }
}
```

### 2. Fluent API for Advanced Publishing

```csharp
public class AdvancedOrderService
{
    private readonly IStreamFlowClient _streamFlow;

    public AdvancedOrderService(IStreamFlowClient streamFlow)
    {
        _streamFlow = streamFlow;
    }

    public async Task CreateOrderWithAdvancedPublishingAsync(CreateOrderRequest request)
    {
        var orderId = Guid.NewGuid();

        // Advanced domain event publishing with fluent API
        await _streamFlow.EventBus.Event<OrderCreated>()
            .WithMetadata(metadata =>
            {
                metadata.CorrelationId = Guid.NewGuid().ToString();
                metadata.Source = "order-service";
                metadata.Version = 2;
            })
            .WithAggregateId(orderId.ToString())
            .WithAggregateType("Order")
            .WithPriority(1)
            .WithTtl(TimeSpan.FromMinutes(30))
            .WithProperty("order-type", "premium")
            .PublishAsync(new OrderCreated(orderId, request.CustomerName, request.Amount));

        // Advanced integration event publishing
        await _streamFlow.EventBus.Event<PaymentProcessed>()
            .WithCorrelationId(Guid.NewGuid().ToString())
            .WithCausationId(orderId.ToString())
            .WithSource("payment-service")
            .WithVersion(1)
            .WithTtl(TimeSpan.FromHours(1))
            .WithProperty("payment-method", "credit-card")
            .PublishAsync(new PaymentProcessed(orderId, "txn-123", request.Amount));
    }
}
```

## Event Handling

### 1. Event Handler Implementation

```csharp
public class OrderCreatedHandler : IAsyncEventHandler<OrderCreated>
{
    private readonly ILogger<OrderCreatedHandler> _logger;
    private readonly IInventoryService _inventoryService;

    public OrderCreatedHandler(ILogger<OrderCreatedHandler> logger, IInventoryService inventoryService)
    {
        _logger = logger;
        _inventoryService = inventoryService;
    }

    public int Priority => 1;
    public string HandlerName => nameof(OrderCreatedHandler);

    public async Task<bool> CanHandleAsync(IEvent @event)
    {
        return @event is OrderCreated;
    }

    public async Task HandleAsync(OrderCreated @event, EventContext context)
    {
        _logger.LogInformation("Processing OrderCreated event for order {OrderId}", @event.OrderId);

        try
        {
            // Reserve inventory
            await _inventoryService.ReserveInventoryAsync(@event.OrderId, @event.Amount);
            
            _logger.LogInformation("Inventory reserved for order {OrderId}", @event.OrderId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to reserve inventory for order {OrderId}", @event.OrderId);
            throw;
        }
    }
}
```

### 2. Event Handler Registration and Subscription

```csharp
// Program.cs
// Register event handlers with dependency injection
builder.Services.AddScoped<IAsyncEventHandler<OrderCreated>, OrderCreatedHandler>();
builder.Services.AddScoped<IAsyncEventHandler<PaymentProcessed>, PaymentProcessedHandler>();

var app = builder.Build();

// Initialize StreamFlow
var streamFlow = app.Services.GetRequiredService<IStreamFlowClient>();
await streamFlow.InitializeAsync();

// Start the event bus
await streamFlow.EventBus.StartAsync();

// Get event handlers from dependency injection
var orderCreatedHandler = app.Services.GetRequiredService<IAsyncEventHandler<OrderCreated>>();
var paymentProcessedHandler = app.Services.GetRequiredService<IAsyncEventHandler<PaymentProcessed>>();

// Subscribe to domain events - automatically creates "domain.order" exchange and queues
await streamFlow.EventBus.SubscribeToDomainEventAsync("order", orderCreatedHandler);

// Subscribe to integration events - uses ExchangeName from event properties
await streamFlow.EventBus.SubscribeToIntegrationEventAsync("payment-events", paymentProcessedHandler);

Console.WriteLine("Event handlers registered and subscribed successfully");

app.Run();
```

## Best Practices

### 1. Event Design

- **Use meaningful names**: Events should clearly describe what happened
- **Include essential data**: Events should contain all necessary information
- **Keep events immutable**: Once published, events should never change
- **Use proper versioning**: Include version information for schema evolution
- **Add correlation IDs**: Enable event tracing across services

### 2. Event Handler Design

- **Keep handlers idempotent**: Handlers should produce the same result when called multiple times
- **Handle failures gracefully**: Implement proper error handling and retry logic
- **Use async patterns**: Prefer async handlers for I/O operations
- **Implement proper logging**: Log important events and errors for debugging

### 3. Event Bus Configuration

- **Use appropriate exchange names**: Service-based naming for integration events
- **Configure retry policies**: Set up proper retry and dead letter handling
- **Monitor event flow**: Implement monitoring and alerting for event processing
- **Use correlation IDs**: Enable end-to-end tracing of event flows

### 4. Infrastructure Management

- **Let EventBus handle infrastructure**: No manual exchange/queue creation needed
- **Use fanout exchanges**: Simplified event distribution without routing complexity
- **Monitor queue health**: Keep an eye on queue lengths and processing rates
- **Plan for scaling**: Design events and handlers with horizontal scaling in mind

## Complete Example

```csharp
// Program.cs - Complete setup
using FS.StreamFlow.RabbitMQ.DependencyInjection;

var builder = WebApplication.CreateBuilder(args);

// Add FS.StreamFlow with RabbitMQ
builder.Services.AddStreamFlow(options =>
{
    options.UseRabbitMQ(rabbitMqOptions =>
    {
        rabbitMqOptions.ConnectionString = "amqp://localhost:5672";
        rabbitMqOptions.ClientConfiguration.ClientName = "Order Processing Service";
        rabbitMqOptions.ProducerSettings.EnableConfirmation = true;
        rabbitMqOptions.ConsumerSettings.AutoAcknowledge = false;
        rabbitMqOptions.ErrorHandlingSettings.MaxRetries = 3;
        rabbitMqOptions.ErrorHandlingSettings.EnableDeadLetterQueue = true;
    });
});

// Register services and handlers
builder.Services.AddScoped<OrderService>();
builder.Services.AddScoped<IAsyncEventHandler<OrderCreated>, OrderCreatedHandler>();
builder.Services.AddScoped<IAsyncEventHandler<PaymentProcessed>, PaymentProcessedHandler>();

var app = builder.Build();

// Initialize and start EventBus
var streamFlow = app.Services.GetRequiredService<IStreamFlowClient>();
await streamFlow.InitializeAsync();
await streamFlow.EventBus.StartAsync();

// Subscribe to events - EventBus automatically creates exchanges and queues
var orderCreatedHandler = app.Services.GetRequiredService<IAsyncEventHandler<OrderCreated>>();
var paymentProcessedHandler = app.Services.GetRequiredService<IAsyncEventHandler<PaymentProcessed>>();

await streamFlow.EventBus.SubscribeToDomainEventAsync("order", orderCreatedHandler);
await streamFlow.EventBus.SubscribeToIntegrationEventAsync("payment-events", paymentProcessedHandler);

Console.WriteLine("Event-driven architecture setup complete!");

app.Run();
```

## Migration from Manual Infrastructure

If you have existing manual exchange creation code, you can safely remove it:

```csharp
// ❌ OLD - Remove this code
await streamFlow.ExchangeManager.Exchange("domain-events")
    .AsTopic()
    .WithDurable(true)
    .DeclareAsync();

await streamFlow.QueueManager.Queue("domain-events-OrderCreated")
    .WithDurable(true)
    .BindToExchange("domain-events", "domain.order.ordercreated")
    .DeclareAsync();

// ✅ NEW - Just publish and subscribe, infrastructure is automatic
await streamFlow.EventBus.PublishDomainEventAsync(orderCreatedEvent);
await streamFlow.EventBus.SubscribeToDomainEventAsync("order", orderCreatedHandler);
```

The EventBus will handle everything automatically when you publish or subscribe to events!
