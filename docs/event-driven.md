# Event-Driven Architecture Guide

This guide explains how to implement event-driven architecture using FS.StreamFlow with RabbitMQ.

## Table of Contents
- [Overview](#overview)
- [Event Types](#event-types)
- [Event Bus Setup](#event-bus-setup)
- [Event Publishing](#event-publishing)
- [Event Handling](#event-handling)
- [Event Store](#event-store)
- [Best Practices](#best-practices)
- [Complete Examples](#complete-examples)

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
    decimal Amount) : IDomainEvent
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

public record OrderCancelled(
    Guid OrderId,
    string Reason) : IDomainEvent
{
    public Guid Id { get; } = Guid.NewGuid();
    public DateTime OccurredOn { get; } = DateTime.UtcNow;
    public int Version { get; } = 1;
    public string EventType => nameof(OrderCancelled);
    public string? CorrelationId { get; set; }
    public string? CausationId { get; set; }
    public IDictionary<string, object> Metadata { get; } = new Dictionary<string, object>();
    public string AggregateId => OrderId.ToString();
    public string AggregateType => "Order";
    public long AggregateVersion { get; set; }
    public string? InitiatedBy { get; set; }
}
```

### 2. Integration Events

Integration events are used for communication between different services:

```csharp
public record PaymentProcessed(
    Guid OrderId,
    string TransactionId,
    decimal Amount) : IIntegrationEvent
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

public record ShipmentScheduled(
    Guid OrderId,
    DateTime EstimatedDelivery) : IIntegrationEvent
{
    public Guid Id { get; } = Guid.NewGuid();
    public DateTime OccurredOn { get; } = DateTime.UtcNow;
    public int Version { get; } = 1;
    public string EventType => nameof(ShipmentScheduled);
    public string? CorrelationId { get; set; }
    public string? CausationId { get; set; }
    public IDictionary<string, object> Metadata { get; } = new Dictionary<string, object>();
    public string Source => "shipping-service";
    public string RoutingKey => "shipment.scheduled";
    public string? Target { get; set; }
    public string SchemaVersion => "1.0";
    public TimeSpan? TimeToLive { get; set; }
}
```

## Event Bus Setup

### 1. Service Registration

First, register the event bus services in your application:

```csharp
// Program.cs
using FS.StreamFlow.RabbitMQ.DependencyInjection;

var builder = WebApplication.CreateBuilder(args);

// Add RabbitMQ StreamFlow with event bus support
builder.Services.AddRabbitMQStreamFlow(options =>
{
    // Client configuration
    options.ClientConfiguration.ClientName = "Event-Driven Application";
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
    options.ProducerSettings.ConfirmationTimeout = TimeSpan.FromSeconds(5);
    options.ProducerSettings.MaxConcurrentPublishes = 100;
    
    // Consumer settings
    options.ConsumerSettings.PrefetchCount = 50;
    options.ConsumerSettings.AutoAcknowledge = false;
    options.ConsumerSettings.MaxConcurrentConsumers = 5;
});

var app = builder.Build();

// Initialize the StreamFlow client
var streamFlow = app.Services.GetRequiredService<IStreamFlowClient>();
await streamFlow.InitializeAsync();

// Start the event bus (required for event publishing and subscription)
await streamFlow.EventBus.StartAsync();

// Register application shutdown handlers
var lifetime = app.Services.GetRequiredService<IHostApplicationLifetime>();
lifetime.ApplicationStopping.Register(async () =>
{
    try
    {
        // Stop event bus gracefully
        await streamFlow.EventBus.StopAsync();
        
        // Shutdown StreamFlow client gracefully
        await streamFlow.ShutdownAsync();
        
        Console.WriteLine("Event bus and StreamFlow client stopped gracefully");
    }
    catch (Exception ex)
    {
        Console.WriteLine($"Error during shutdown: {ex.Message}");
    }
});
```

### 2. Required Using Statements

```csharp
using FS.StreamFlow.Core.Features.Events.Interfaces;
using FS.StreamFlow.Core.Features.Events.Models;
using FS.StreamFlow.Core.Features.Messaging.Interfaces;
using FS.StreamFlow.Core.Features.Messaging.Models;
using FS.StreamFlow.RabbitMQ.DependencyInjection;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System.Diagnostics;
```

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

        // Publish the domain event
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

        // Publish the integration event
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
    private readonly ILogger<AdvancedOrderService> _logger;

    public AdvancedOrderService(IStreamFlowClient streamFlow, ILogger<AdvancedOrderService> logger)
    {
        _streamFlow = streamFlow;
        _logger = logger;
    }

    public async Task CreateOrderWithFluentApiAsync(CreateOrderRequest request)
    {
        var orderId = Guid.NewGuid();
        var correlationId = Guid.NewGuid().ToString();

        var orderCreatedEvent = new OrderCreated(orderId, request.CustomerName, request.Amount);

        // Use fluent API for advanced configuration
        await _streamFlow.EventBus.Event<OrderCreated>()
            .WithCorrelationId(correlationId)
            .WithSource("order-service")
            .WithVersion("1.0")
            .WithAggregateId(orderId.ToString())
            .WithAggregateType("Order")
            .WithPriority(1)
            .WithTtl(TimeSpan.FromMinutes(30))
            .WithProperty("customer-tier", "premium")
            .WithProperty("order-type", "online")
            .PublishAsync(orderCreatedEvent);

        _logger.LogInformation("Order created with fluent API: {OrderId}", orderId);
    }

    public async Task PublishBatchEventsAsync(List<CreateOrderRequest> requests)
    {
        var events = new List<IEvent>();
        var correlationId = Guid.NewGuid().ToString();

        foreach (var request in requests)
        {
            var orderId = Guid.NewGuid();
            var orderCreatedEvent = new OrderCreated(orderId, request.CustomerName, request.Amount)
            {
                CorrelationId = correlationId,
                InitiatedBy = "order-service"
            };
            events.Add(orderCreatedEvent);
        }

        // Publish multiple events in a batch
        await _streamFlow.EventBus.PublishBatchAsync(events);
        
        _logger.LogInformation("Published batch of {Count} events", events.Count);
    }
}
```

## Event Handling

### 1. Event Handler Implementation

```csharp
public class OrderCreatedHandler : IAsyncEventHandler<OrderCreated>
{
    private readonly IEmailService _emailService;
    private readonly IInventoryService _inventoryService;
    private readonly IStreamFlowClient _streamFlow;
    private readonly ILogger<OrderCreatedHandler> _logger;

    public OrderCreatedHandler(
        IEmailService emailService,
        IInventoryService inventoryService,
        IStreamFlowClient streamFlow,
        ILogger<OrderCreatedHandler> logger)
    {
        _emailService = emailService;
        _inventoryService = inventoryService;
        _streamFlow = streamFlow;
        _logger = logger;
    }

    public async Task HandleAsync(OrderCreated @event, EventContext context, CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("Processing OrderCreated event for order {OrderId}", @event.OrderId);

            // Send confirmation email
            await _emailService.SendOrderConfirmationAsync(
                @event.OrderId,
                @event.CustomerName,
                cancellationToken);

            // Reserve inventory
            var reserved = await _inventoryService.ReserveInventoryAsync(@event.OrderId, cancellationToken);

            if (reserved)
            {
                // Publish follow-up integration event
                var inventoryReservedEvent = new InventoryReserved(@event.OrderId, DateTime.UtcNow)
                {
                    CorrelationId = context.CorrelationId,
                    CausationId = context.EventId
                };

                await _streamFlow.EventBus.PublishIntegrationEventAsync(inventoryReservedEvent, cancellationToken);
                
                _logger.LogInformation("Inventory reserved for order {OrderId}", @event.OrderId);
            }
            else
            {
                // Publish failure event
                var inventoryReservationFailedEvent = new InventoryReservationFailed(@event.OrderId, "Insufficient stock")
                {
                    CorrelationId = context.CorrelationId,
                    CausationId = context.EventId
                };

                await _streamFlow.EventBus.PublishIntegrationEventAsync(inventoryReservationFailedEvent, cancellationToken);
                
                _logger.LogWarning("Inventory reservation failed for order {OrderId}", @event.OrderId);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing OrderCreated event for order {OrderId}", @event.OrderId);
            throw;
        }
    }
}

public class PaymentProcessedHandler : IAsyncEventHandler<PaymentProcessed>
{
    private readonly IOrderService _orderService;
    private readonly IStreamFlowClient _streamFlow;
    private readonly ILogger<PaymentProcessedHandler> _logger;

    public PaymentProcessedHandler(
        IOrderService orderService,
        IStreamFlowClient streamFlow,
        ILogger<PaymentProcessedHandler> logger)
    {
        _orderService = orderService;
        _streamFlow = streamFlow;
        _logger = logger;
    }

    public async Task HandleAsync(PaymentProcessed @event, EventContext context, CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("Processing PaymentProcessed event for order {OrderId}", @event.OrderId);

            // Update order payment status
            await _orderService.UpdatePaymentStatusAsync(@event.OrderId, @event.TransactionId, cancellationToken);

            // Publish shipment scheduling event
            var shipmentScheduledEvent = new ShipmentScheduled(@event.OrderId, DateTime.UtcNow.AddDays(2))
            {
                CorrelationId = context.CorrelationId,
                CausationId = context.EventId
            };

            await _streamFlow.EventBus.PublishIntegrationEventAsync(shipmentScheduledEvent, cancellationToken);
            
            _logger.LogInformation("Payment processed successfully for order {OrderId}", @event.OrderId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing PaymentProcessed event for order {OrderId}", @event.OrderId);
            throw;
        }
    }
}
```

### 2. Event Handler Registration

```csharp
// Program.cs
// Register event handlers with dependency injection
builder.Services.AddScoped<IAsyncEventHandler<OrderCreated>, OrderCreatedHandler>();
builder.Services.AddScoped<IAsyncEventHandler<PaymentProcessed>, PaymentProcessedHandler>();
builder.Services.AddScoped<IAsyncEventHandler<InventoryReserved>, InventoryReservedHandler>();
builder.Services.AddScoped<IAsyncEventHandler<ShipmentScheduled>, ShipmentScheduledHandler>();

// Subscribe to events after application startup
var app = builder.Build();

// Get services
var streamFlow = app.Services.GetRequiredService<IStreamFlowClient>();
var orderCreatedHandler = app.Services.GetRequiredService<IAsyncEventHandler<OrderCreated>>();
var paymentProcessedHandler = app.Services.GetRequiredService<IAsyncEventHandler<PaymentProcessed>>();

// Subscribe to domain events
await streamFlow.EventBus.SubscribeToDomainEventAsync(orderCreatedHandler);

// Subscribe to integration events
await streamFlow.EventBus.SubscribeToIntegrationEventAsync(paymentProcessedHandler);
```

### 3. Event Monitoring and Observability

```csharp
public class EventMonitoringService
{
    private readonly IStreamFlowClient _streamFlow;
    private readonly ILogger<EventMonitoringService> _logger;

    public EventMonitoringService(IStreamFlowClient streamFlow, ILogger<EventMonitoringService> logger)
    {
        _streamFlow = streamFlow;
        _logger = logger;
        
        // Subscribe to event bus events for monitoring
        _streamFlow.EventBus.EventPublished += OnEventPublished;
        _streamFlow.EventBus.EventReceived += OnEventReceived;
        _streamFlow.EventBus.EventProcessingFailed += OnEventProcessingFailed;
    }

    private void OnEventPublished(object? sender, EventPublishedEventArgs e)
    {
        if (e.Success)
        {
            _logger.LogInformation("Event {EventType} published successfully to {Exchange} with routing key {RoutingKey}", 
                e.Event.EventType, e.ExchangeName, e.RoutingKey);
        }
        else
        {
            _logger.LogError("Failed to publish event {EventType} to {Exchange}: {ErrorMessage}", 
                e.Event.EventType, e.ExchangeName, e.ErrorMessage);
        }
    }

    private void OnEventReceived(object? sender, EventReceivedEventArgs e)
    {
        _logger.LogInformation("Event {EventType} received from {Exchange} with routing key {RoutingKey}", 
            e.Event.EventType, e.ExchangeName, e.RoutingKey);
    }

    private void OnEventProcessingFailed(object? sender, EventProcessingFailedEventArgs e)
    {
        _logger.LogError(e.Exception, "Event {EventType} processing failed. Attempt: {AttemptCount}, WillRetry: {WillRetry}", 
            e.Event.EventType, e.AttemptCount, e.WillRetry);
    }

    public void Dispose()
    {
        _streamFlow.EventBus.EventPublished -= OnEventPublished;
        _streamFlow.EventBus.EventReceived -= OnEventReceived;
        _streamFlow.EventBus.EventProcessingFailed -= OnEventProcessingFailed;
    }
}
```

### 4. Custom Event Handler with Function

```csharp
public class CustomEventHandler
{
    private readonly IStreamFlowClient _streamFlow;
    private readonly ILogger<CustomEventHandler> _logger;

    public CustomEventHandler(IStreamFlowClient streamFlow, ILogger<CustomEventHandler> logger)
    {
        _streamFlow = streamFlow;
        _logger = logger;
    }

    public async Task SubscribeToCustomEventsAsync()
    {
        // Subscribe with custom handler function
        await _streamFlow.EventBus.SubscribeAsync<OrderCreated>(async (orderCreated, context) =>
        {
            try
            {
                _logger.LogInformation("Custom handler processing order {OrderId}", orderCreated.OrderId);
                
                // Custom business logic
                await ProcessOrderCreatedCustomAsync(orderCreated, context);
                
                return true; // Success
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Custom handler failed for order {OrderId}", orderCreated.OrderId);
                return false; // Failure - will be retried
            }
        });
    }

    private async Task ProcessOrderCreatedCustomAsync(OrderCreated orderCreated, EventContext context)
    {
        // Custom processing logic
        await Task.Delay(100); // Simulate work
    }
}
```

## Event Store

### 1. Event Store Usage

The event store is automatically initialized with the StreamFlow client and accessed through it:

### 2. Event Store Usage

```csharp
public class OrderEventStore
{
    private readonly IStreamFlowClient _streamFlow;
    private readonly ILogger<OrderEventStore> _logger;

    public OrderEventStore(IStreamFlowClient streamFlow, ILogger<OrderEventStore> logger)
    {
        _streamFlow = streamFlow;
        _logger = logger;
    }

    public async Task StoreOrderEventAsync(Guid orderId, IEvent @event)
    {
        var streamName = $"order-{orderId}";
        
        // Get current version for optimistic concurrency
        var currentVersion = await _streamFlow.EventStore.GetStreamVersionAsync(streamName);
        
        await _streamFlow.EventStore.SaveEventsAsync(streamName, new[] { @event }, currentVersion);
            
        _logger.LogInformation("Event {EventType} stored in stream {StreamName}", @event.EventType, streamName);
    }

    public async Task<IEnumerable<object>> GetOrderEventsAsync(Guid orderId, long fromVersion = 0)
    {
        var streamName = $"order-{orderId}";
        
        var events = await _streamFlow.EventStore.GetEventsAsync(streamName, fromVersion);
            
        return events;
    }

    public async Task<long> GetOrderVersionAsync(Guid orderId)
    {
        var streamName = $"order-{orderId}";
        
        var version = await _streamFlow.EventStore.GetStreamVersionAsync(streamName);
            
        return version;
    }
}
```

### 2. Event Sourcing with Event Store

```csharp
public class OrderAggregate
{
    private readonly List<IEvent> _uncommittedEvents = new();
    public Guid Id { get; private set; }
    public string CustomerName { get; private set; } = string.Empty;
    public decimal Total { get; private set; }
    public OrderStatus Status { get; private set; }
    public long Version { get; private set; }

    public static OrderAggregate Create(Guid id, string customerName, decimal total)
    {
        var order = new OrderAggregate();
        order.Apply(new OrderCreated(id, customerName, total));
        return order;
    }

    public void ApplyPayment(string transactionId, decimal amount)
    {
        Apply(new PaymentProcessed(Id, transactionId, amount));
    }

    public void Cancel(string reason)
    {
        Apply(new OrderCancelled(Id, reason));
    }

    private void Apply(IEvent @event)
    {
        When(@event);
        _uncommittedEvents.Add(@event);
    }

    private void When(IEvent @event)
    {
        switch (@event)
        {
            case OrderCreated orderCreated:
                Id = orderCreated.OrderId;
                CustomerName = orderCreated.CustomerName;
                Total = orderCreated.Amount;
                Status = OrderStatus.Created;
                Version++;
                break;
            case PaymentProcessed paymentProcessed:
                Status = OrderStatus.Paid;
                Version++;
                break;
            case OrderCancelled orderCancelled:
                Status = OrderStatus.Cancelled;
                Version++;
                break;
        }
    }

    public IEnumerable<IEvent> GetUncommittedEvents() => _uncommittedEvents.AsReadOnly();

    public void MarkEventsAsCommitted() => _uncommittedEvents.Clear();

    public static OrderAggregate FromEvents(IEnumerable<IEvent> events)
    {
        var order = new OrderAggregate();
        foreach (var @event in events)
        {
            order.When(@event);
        }
        return order;
    }
}

public enum OrderStatus
{
    Created,
    Paid,
    Cancelled
}
```

## Best Practices

### 1. Event Design

- **Use past tense for event names**: `OrderCreated`, `PaymentProcessed`, `ShipmentScheduled`
- **Keep events immutable**: Use records or readonly properties
- **Include only necessary data**: Don't include sensitive information
- **Use strong typing**: Avoid dynamic objects
- **Consider versioning**: Plan for schema evolution

### 2. Event Handler Design

- **Keep handlers focused**: One handler per event type
- **Handle errors gracefully**: Implement proper error handling
- **Use dependency injection**: Inject required services
- **Log important operations**: Include correlation IDs
- **Avoid long-running operations**: Use async/await properly
- **Return boolean for success/failure**: Return `true` for success, `false` for retry
- **Use cancellation tokens**: Support graceful cancellation

### 3. Event Bus Configuration

- **Use StreamFlow client**: Access event bus through `IStreamFlowClient.EventBus`
- **Initialize StreamFlow client**: Call `streamFlow.InitializeAsync()` first
- **Start event bus explicitly**: Call `streamFlow.EventBus.StartAsync()` after initialization
- **Stop event bus gracefully**: Call `streamFlow.EventBus.StopAsync()` before shutdown
- **Handle connection failures**: Implement retry policies
- **Monitor event processing**: Use built-in metrics and events
- **Configure appropriate timeouts**: Set reasonable values
- **Use persistent delivery**: For critical events
- **Subscribe to events**: Use `EventPublished`, `EventReceived`, `EventProcessingFailed` events for monitoring

### 4. Event Store Usage

- **Access through StreamFlow client**: Use `streamFlow.EventStore` for all operations
- **Use meaningful stream names**: Include aggregate ID
- **Store events atomically**: Use transactions when possible
- **Handle concurrency**: Implement optimistic concurrency control with expected version
- **Backup event streams**: Regular backups for disaster recovery
- **Monitor storage usage**: Track event store growth
- **Automatic lifecycle**: Event store is managed by StreamFlow client

## Complete Examples

### 1. Complete Order Processing Flow

```csharp
// Program.cs
using FS.StreamFlow.RabbitMQ.DependencyInjection;

var builder = WebApplication.CreateBuilder(args);

// Add services
builder.Services.AddRabbitMQStreamFlow(options =>
{
    options.ClientConfiguration.ClientName = "Order Processing System";
    options.ConnectionSettings.Host = "localhost";
    options.ConnectionSettings.Port = 5672;
    options.ConnectionSettings.Username = "guest";
    options.ConnectionSettings.Password = "guest";
    options.ConnectionSettings.VirtualHost = "/";
});

// Register business services
builder.Services.AddScoped<IOrderService, OrderService>();
builder.Services.AddScoped<IEmailService, EmailService>();
builder.Services.AddScoped<IInventoryService, InventoryService>();
builder.Services.AddScoped<IPaymentService, PaymentService>();

// Register event handlers
builder.Services.AddScoped<IAsyncEventHandler<OrderCreated>, OrderCreatedHandler>();
builder.Services.AddScoped<IAsyncEventHandler<PaymentProcessed>, PaymentProcessedHandler>();
builder.Services.AddScoped<IAsyncEventHandler<InventoryReserved>, InventoryReservedHandler>();
builder.Services.AddScoped<IAsyncEventHandler<ShipmentScheduled>, ShipmentScheduledHandler>();

var app = builder.Build();

// Initialize StreamFlow
var streamFlow = app.Services.GetRequiredService<IStreamFlowClient>();
await streamFlow.InitializeAsync();

// Start the event bus (required for event publishing and subscription)
await streamFlow.EventBus.StartAsync();

// Subscribe to events
var orderCreatedHandler = app.Services.GetRequiredService<IAsyncEventHandler<OrderCreated>>();
var paymentProcessedHandler = app.Services.GetRequiredService<IAsyncEventHandler<PaymentProcessed>>();
var inventoryReservedHandler = app.Services.GetRequiredService<IAsyncEventHandler<InventoryReserved>>();
var shipmentScheduledHandler = app.Services.GetRequiredService<IAsyncEventHandler<ShipmentScheduled>>();

await streamFlow.EventBus.SubscribeToDomainEventAsync(orderCreatedHandler);
await streamFlow.EventBus.SubscribeToIntegrationEventAsync(paymentProcessedHandler);
await streamFlow.EventBus.SubscribeToIntegrationEventAsync(inventoryReservedHandler);
await streamFlow.EventBus.SubscribeToIntegrationEventAsync(shipmentScheduledHandler);

// Register application shutdown handlers
var lifetime = app.Services.GetRequiredService<IHostApplicationLifetime>();
lifetime.ApplicationStopping.Register(async () =>
{
    try
    {
        // Stop event bus gracefully
        await eventBus.StopAsync();
        
        // Shutdown StreamFlow client
        await streamFlow.ShutdownAsync();
        
        Console.WriteLine("Event bus and StreamFlow client stopped gracefully");
    }
    catch (Exception ex)
    {
        Console.WriteLine($"Error during shutdown: {ex.Message}");
    }
});

await app.RunAsync();
```

### 2. Controller Example

```csharp
[ApiController]
[Route("api/[controller]")]
public class OrdersController : ControllerBase
{
    private readonly IOrderService _orderService;
    private readonly ILogger<OrdersController> _logger;

    public OrdersController(IOrderService orderService, ILogger<OrdersController> logger)
    {
        _orderService = orderService;
        _logger = logger;
    }

    [HttpPost]
    public async Task<IActionResult> CreateOrder([FromBody] CreateOrderRequest request)
    {
        try
        {
            var orderId = await _orderService.CreateOrderAsync(request);
            
            _logger.LogInformation("Order created successfully: {OrderId}", orderId);
            
            return Ok(new { OrderId = orderId });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to create order");
            return StatusCode(500, "Failed to create order");
        }
    }

    [HttpPost("{orderId}/payments")]
    public async Task<IActionResult> ProcessPayment(Guid orderId, [FromBody] ProcessPaymentRequest request)
    {
        try
        {
            await _orderService.ProcessPaymentAsync(orderId, request.TransactionId, request.Amount);
            
            _logger.LogInformation("Payment processed for order: {OrderId}", orderId);
            
            return Ok();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to process payment for order {OrderId}", orderId);
            return StatusCode(500, "Failed to process payment");
        }
    }
}

public record CreateOrderRequest(string CustomerName, decimal Amount);
public record ProcessPaymentRequest(string TransactionId, decimal Amount);
```

### 3. Error Handling Example

```csharp
public class ResilientEventHandler<T> : IAsyncEventHandler<T> where T : class, IEvent
{
    private readonly IAsyncEventHandler<T> _innerHandler;
    private readonly ILogger<ResilientEventHandler<T>> _logger;
    private readonly IRetryPolicy _retryPolicy;

    public ResilientEventHandler(
        IAsyncEventHandler<T> innerHandler,
        ILogger<ResilientEventHandler<T>> logger,
        IRetryPolicy retryPolicy)
    {
        _innerHandler = innerHandler;
        _logger = logger;
        _retryPolicy = retryPolicy;
    }

    public async Task HandleAsync(T @event, EventContext context, CancellationToken cancellationToken = default)
    {
        var attempt = 0;
        var maxAttempts = 3;

        while (attempt < maxAttempts)
        {
            try
            {
                attempt++;
                _logger.LogInformation("Processing event {EventType} attempt {Attempt}", typeof(T).Name, attempt);

                await _innerHandler.HandleAsync(@event, context, cancellationToken);
                
                _logger.LogInformation("Successfully processed event {EventType}", typeof(T).Name);
                return;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing event {EventType} attempt {Attempt}", typeof(T).Name, attempt);

                if (attempt >= maxAttempts)
                {
                    _logger.LogError("Max retry attempts reached for event {EventType}", typeof(T).Name);
                    throw;
                }

                var delay = _retryPolicy.GetDelay(attempt);
                await Task.Delay(delay, cancellationToken);
            }
        }
    }
}
```

This comprehensive guide covers all aspects of implementing event-driven architecture with FS.StreamFlow, from basic setup to advanced patterns and best practices. 