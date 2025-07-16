# FS.StreamFlow

[![Core NuGet Version](https://img.shields.io/nuget/v/FS.StreamFlow.Core.svg)](https://www.nuget.org/packages/FS.StreamFlow.Core/)
[![RabbitMQ NuGet Version](https://img.shields.io/nuget/v/FS.StreamFlow.RabbitMQ.svg)](https://www.nuget.org/packages/FS.StreamFlow.RabbitMQ/)
[![NuGet Downloads](https://img.shields.io/nuget/dt/FS.StreamFlow.RabbitMQ.svg)](https://www.nuget.org/packages/FS.StreamFlow.RabbitMQ/)
[![GitHub License](https://img.shields.io/github/license/furkansarikaya/FS.StreamFlow)](https://github.com/furkansarikaya/FS.StreamFlow/blob/main/LICENSE)
[![GitHub Stars](https://img.shields.io/github/stars/furkansarikaya/FS.StreamFlow.svg)](https://github.com/furkansarikaya/FS.StreamFlow/stargazers)

**A comprehensive, production-ready messaging framework for .NET 9 with enterprise-grade features, automatic recovery, and event-driven architecture support.**

FS.StreamFlow isn't just another messaging client wrapper. It's a complete messaging solution for building scalable, resilient applications with sophisticated event-driven capabilities. Whether you're building microservices, implementing CQRS patterns, or creating real-time applications, FS.StreamFlow provides everything you need in one cohesive framework.

## 🏗️ Architecture

FS.StreamFlow follows a layered architecture with clear separation of concerns:

- **FS.StreamFlow.Core**: Provider-agnostic abstractions, interfaces, and models
- **FS.StreamFlow.RabbitMQ**: Production-ready RabbitMQ implementation
- **Future Providers**: Extensible to other messaging providers (Azure Service Bus, Apache Kafka, etc.)

## ✨ Why FS.StreamFlow?

Imagine you're building a modern distributed application that needs to handle thousands of messages per second, process complex event flows, and remain resilient under pressure. Traditional messaging clients give you basic publish/subscribe functionality, but when you need enterprise-grade features like automatic recovery, event sourcing, saga orchestration, and comprehensive error handling, you're left building everything from scratch.

FS.StreamFlow bridges this gap by providing **everything you need in one production-ready framework**:

- 🔄 **Automatic Recovery**: Intelligent connection recovery with exponential backoff
- 📨 **Enterprise Messaging**: Producer/Consumer patterns with confirmations and transactions
- 🎯 **Event-Driven Architecture**: Built-in support for domain and integration events
- 📚 **Event Sourcing**: Complete event store implementation with snapshots
- 🔀 **Saga Orchestration**: Long-running workflow management
- 🛡️ **Comprehensive Error Handling**: Dead letter queues, retry policies, and circuit breakers
- 📊 **Production Monitoring**: Health checks, metrics, and performance tracking
- ⚡ **High Performance**: Async-first API with connection pooling
- 🔌 **Provider Agnostic**: Extensible architecture for multiple messaging providers

## 🚀 Quick Start

### Installation

Choose your messaging provider:

```bash
# For RabbitMQ support
dotnet add package FS.StreamFlow.RabbitMQ

# Core abstractions (automatically included with providers)
dotnet add package FS.StreamFlow.Core
```

### Basic Setup

```csharp
// Program.cs
using FS.StreamFlow.RabbitMQ.DependencyInjection;

var builder = WebApplication.CreateBuilder(args);

// Add FS.StreamFlow with RabbitMQ
builder.Services.AddRabbitMQStreamFlow(options =>
{
    // Client configuration
    options.ClientConfiguration.ClientName = "My Application";
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
});

var app = builder.Build();
```

### Your First Message

```csharp
// Inject the StreamFlow client
public class OrderService
{
    private readonly IStreamFlowClient _streamFlow;
    
    public OrderService(IStreamFlowClient streamFlow) => _streamFlow = streamFlow;
    
    public async Task CreateOrderAsync(Order order)
    {
        // Create exchange and queue with fluent API
        await _streamFlow.ExchangeManager.Exchange("orders")
            .AsTopic()
            .WithDurable(true)
            .DeclareAsync();
            
        await _streamFlow.QueueManager.Queue("order-processing")
            .WithDurable(true)
            .WithDeadLetterExchange("dlx")
            .BindToExchange("orders", "order.created")
            .DeclareAsync();
        
        // Publish a message with fluent API
        await _streamFlow.Producer.Message(order)
            .WithExchange("orders")
            .WithRoutingKey("order.created")
            .WithDeliveryMode(DeliveryMode.Persistent)
            .PublishAsync();
    }
}

// Consume messages with fluent API
public class OrderProcessor
{
    private readonly IStreamFlowClient _streamFlow;
    
    public OrderProcessor(IStreamFlowClient streamFlow) => _streamFlow = streamFlow;
    
    public async Task StartProcessingAsync()
    {
        await _streamFlow.Consumer.Queue<Order>("order-processing")
            .WithConcurrency(5)
            .WithPrefetchCount(100)
            .WithErrorHandler(async (exception, context) =>
            {
                // Handle error and return true to requeue, false to reject
                return exception is TransientException;
            })
            .ConsumeAsync(async (order, context) =>
            {
                await ProcessOrderAsync(order);
                return true; // Acknowledge message
            });
    }
}
```

That's it! You now have a production-ready messaging system with automatic recovery, health monitoring, and comprehensive error handling.

## 🌟 Key Features

### 1. Production-Ready Connection Management

Automatic connection recovery with intelligent backoff strategies:

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
    options.ClientConfiguration.EnableAutoRecovery = true;
    options.ClientConfiguration.EnableHeartbeat = true;
    options.ClientConfiguration.HeartbeatInterval = TimeSpan.FromSeconds(60);
});
```

### 2. Event-Driven Architecture

Built-in support for domain and integration events:

```csharp
// Domain Event
public record OrderCreated(Guid OrderId, string CustomerName, decimal Amount) : IDomainEvent;

// Integration Event  
public record OrderShipped(Guid OrderId, string TrackingNumber) : IIntegrationEvent;

// Event Handlers
public class OrderCreatedHandler : IAsyncEventHandler<OrderCreated>
{
    public async Task HandleAsync(OrderCreated eventData, EventContext context)
    {
        // Handle domain event
        await _emailService.SendOrderConfirmationAsync(eventData);
    }
}

// Publishing events with fluent API
await _streamFlow.EventBus.Event<OrderCreated>()
    .WithMetadata(metadata =>
    {
        metadata.CorrelationId = correlationId;
        metadata.Source = "order-service";
        metadata.Version = "1.0";
    })
    .WithPriority(1)
    .WithTtl(TimeSpan.FromMinutes(30))
    .PublishAsync(new OrderCreated(orderId, customerName, amount));

await _streamFlow.EventBus.Event<OrderShipped>()
    .WithCorrelationId(correlationId)
    .WithCausationId(causationId)
    .WithAggregateId(orderId.ToString())
    .WithAggregateType("Order")
    .WithProperty("priority", "high")
    .PublishAsync(new OrderShipped(orderId, trackingNumber));
```

### 3. Event Sourcing Support

Complete event store implementation with fluent API and snapshots:

```csharp
// Store events with fluent API
await _streamFlow.EventStore.Stream($"order-{orderId}")
    .AppendEvent(new OrderCreated(orderId, customerName, amount))
    .AppendEvent(new OrderItemAdded(orderId, itemId, quantity))
    .SaveAsync();

// Read events with fluent API
var events = await _streamFlow.EventStore.Stream($"order-{orderId}")
    .FromVersion(0)
    .WithMaxCount(100)
    .ReadAsync();

// Create snapshots with fluent API
await _streamFlow.EventStore.Stream($"order-{orderId}")
    .WithSnapshot(orderSnapshot, version: 50)
    .SaveAsync();

// Advanced event store operations
await _streamFlow.EventStore.Stream($"order-{orderId}")
    .AppendEventsWithExpectedVersion(events, expectedVersion: 10)
    .WithSnapshot(snapshot, version: 20)
    .SaveAsync();

// Read events backwards
var recentEvents = await _streamFlow.EventStore.Stream($"order-{orderId}")
    .FromVersion(-1)
    .WithMaxCount(10)
    .ReadBackwardAsync();
```

### 4. Saga Orchestration

Long-running workflow management:

```csharp
public class OrderProcessingSaga : ISaga<OrderProcessingSagaState>
{
    public async Task HandleAsync(OrderCreated @event, SagaContext context)
    {
        State.OrderId = @event.OrderId;
        State.Status = "Processing";
        
        // Send command to reserve inventory
        await context.SendAsync(new ReserveInventory(@event.OrderId, @event.Items));
    }
    
    public async Task HandleAsync(InventoryReserved @event, SagaContext context)
    {
        State.Status = "InventoryReserved";
        
        // Send command to process payment
        await context.SendAsync(new ProcessPayment(@event.OrderId, State.Amount));
    }
    
    public async Task HandleAsync(PaymentProcessed @event, SagaContext context)
    {
        State.Status = "Completed";
        
        // Complete the saga
        await context.CompleteAsync();
    }
}
```

### 5. Advanced Producer Features

Publisher confirms, transactions, and batch operations:

```csharp
// Advanced producer with fluent API
await _streamFlow.Producer.Message(order)
    .WithExchange("orders")
    .WithRoutingKey("order.created")
    .WithDeliveryMode(DeliveryMode.Persistent)
    .WithPriority(5)
    .WithExpiration(TimeSpan.FromHours(24))
    .WithHeaders(new Dictionary<string, object>
    {
        ["source"] = "order-service",
        ["version"] = "1.0"
    })
    .WithHeader("priority", "high")
    .WithContentType("application/json")
    .WithContentEncoding("utf-8")
    .WithCorrelationId(correlationId)
    .WithMessageId(Guid.NewGuid().ToString())
    .WithMandatory(true)
    .WithConfirmationTimeout(TimeSpan.FromSeconds(5))
    .PublishAsync();

// Alternative syntax
await _streamFlow.Producer.Message(order)
    .ToExchange("orders")
    .WithRoutingKey("order.created")
    .WithDeliveryMode(DeliveryMode.Persistent)
    .PublishAsync();

// Batch operations
var messages = new[]
{
    new MessageContext("orders", "order.created", order1),
    new MessageContext("orders", "order.updated", order2),
    new MessageContext("orders", "order.shipped", order3)
};

var batchResult = await _streamFlow.Producer.PublishBatchAsync(messages);
```

### 6. Advanced Consumer Features

Consumer with comprehensive configuration:

```csharp
// Advanced consumer with fluent API
await _streamFlow.Consumer.Queue<Order>("order-processing")
    .FromExchange("orders")
    .WithRoutingKey("order.*")
    .WithSettings(settings =>
    {
        settings.PrefetchCount = 100;
        settings.MaxConcurrentConsumers = 5;
        settings.AutoAcknowledge = false;
    })
    .WithConsumerTag("order-processor-1")
    .WithAutoAck(false)
    .WithPrefetchCount(100)
    .WithConcurrency(5)
    .WithErrorHandler(async (exception, context) =>
    {
        if (exception is TransientException)
            return true; // Requeue
        
        await _deadLetterService.SendAsync(context.Message);
        return false; // Don't requeue
    })
    .WithRetryPolicy(new RetryPolicySettings
    {
        RetryPolicy = RetryPolicyType.ExponentialBackoff,
        MaxRetryAttempts = 3,
        RetryDelay = TimeSpan.FromSeconds(1)
    })
    .WithDeadLetterQueue(new DeadLetterSettings
    {
        DeadLetterExchange = "dlx",
        DeadLetterQueue = "dlq"
    })
    .ConsumeAsync(async (order, context) =>
    {
        await ProcessOrderAsync(order);
        return true;
    });
```

### 7. Infrastructure Management

Fluent APIs for queue, exchange, and event stream management:

```csharp
// Advanced Queue management with fluent API
await _streamFlow.QueueManager.Queue("order-processing")
    .WithDurable(true)
    .WithExclusive(false)
    .WithAutoDelete(false)
    .WithArguments(new Dictionary<string, object>
    {
        ["x-message-ttl"] = 3600000,
        ["x-max-length"] = 10000
    })
    .WithArgument("x-queue-mode", "lazy")
    .WithDeadLetterExchange("dlx")
    .WithDeadLetterRoutingKey("order.failed")
    .WithMessageTtl(TimeSpan.FromHours(24))
    .WithMaxLength(10000)
    .WithMaxLengthBytes(1024 * 1024 * 100) // 100MB
    .WithPriority(5)
    .BindToExchange("orders", "order.created")
    .BindToExchange("orders", "order.updated", new Dictionary<string, object>
    {
        ["x-match"] = "all"
    })
    .DeclareAsync();

// Advanced Exchange management with fluent API
await _streamFlow.ExchangeManager.Exchange("orders")
    .WithType("topic")
    .AsTopic() // Alternative syntax
    .WithDurable(true)
    .WithAutoDelete(false)
    .WithArguments(new Dictionary<string, object>
    {
        ["alternate-exchange"] = "alt-orders"
    })
    .WithArgument("x-delayed-message", true)
    .WithAlternateExchange("alt-orders")
    .WithInternal(false)
    .BindToExchange("master-orders", "order.*")
    .BindToExchange("notifications", "order.created", new Dictionary<string, object>
    {
        ["x-match"] = "any"
    })
    .DeclareAsync();

// Support for all exchange types
await _streamFlow.ExchangeManager.Exchange("direct-orders").AsDirect().DeclareAsync();
await _streamFlow.ExchangeManager.Exchange("fanout-orders").AsFanout().DeclareAsync();
await _streamFlow.ExchangeManager.Exchange("header-orders").AsHeaders().DeclareAsync();

// Event stream management with fluent API
await _streamFlow.EventStore.Stream("order-events")
    .AppendEvents(new[] { event1, event2, event3 })
    .AppendEventsWithExpectedVersion(moreEvents, expectedVersion: 10)
    .WithSnapshot(snapshot, version: 100)
    .SaveAsync();

// Stream operations
await _streamFlow.EventStore.Stream("order-events").CreateAsync();
await _streamFlow.EventStore.Stream("order-events").DeleteAsync();
await _streamFlow.EventStore.Stream("order-events").TruncateAsync(version: 50);

// Snapshot management
var snapshot = await _streamFlow.EventStore.Stream("order-events").GetSnapshotAsync();
await _streamFlow.EventStore.Stream("order-events")
    .WithSnapshot(orderSnapshot, version: 100)
    .SaveAsync();
```

### 8. Comprehensive Error Handling

Built-in retry policies, circuit breakers, and dead letter queues:

```csharp
builder.Services.AddRabbitMQStreamFlow(options =>
{
    // Connection settings
    options.ConnectionSettings.Host = "localhost";
    options.ConnectionSettings.Port = 5672;
    options.ConnectionSettings.Username = "guest";
    options.ConnectionSettings.Password = "guest";
    options.ConnectionSettings.VirtualHost = "/";
    
    // Error handling configuration
    options.ErrorHandling.EnableDeadLetterQueue = true;
    options.ErrorHandling.DeadLetterExchange = "dlx";
    options.ErrorHandling.DeadLetterQueue = "dlq";
    options.ErrorHandling.RetryPolicy = RetryPolicyType.ExponentialBackoff;
    options.ErrorHandling.MaxRetryAttempts = 3;
    options.ErrorHandling.RetryDelay = TimeSpan.FromSeconds(1);
});
```

## 📋 Comprehensive Examples

### Example 1: E-commerce Order Processing

```csharp
// Complete order processing workflow with fluent APIs
public class OrderProcessingService
{
    private readonly IStreamFlowClient _streamFlow;
    
    public OrderProcessingService(IStreamFlowClient streamFlow) => _streamFlow = streamFlow;
    
    public async Task ProcessOrderAsync(Order order)
    {
        // 1. Setup infrastructure with fluent APIs
        await _streamFlow.ExchangeManager.Exchange("orders")
            .AsTopic()
            .WithDurable(true)
            .WithAlternateExchange("alt-orders")
            .DeclareAsync();
            
        await _streamFlow.QueueManager.Queue("order-processing")
            .WithDurable(true)
            .WithDeadLetterExchange("dlx")
            .WithMessageTtl(TimeSpan.FromHours(24))
            .WithMaxLength(10000)
            .WithPriority(5)
            .BindToExchange("orders", "order.*")
            .DeclareAsync();
        
        // 2. Publish domain event with fluent API
        await _streamFlow.EventBus.Event<OrderCreated>()
            .WithMetadata(metadata =>
            {
                metadata.CorrelationId = Guid.NewGuid().ToString();
                metadata.Source = "order-service";
                metadata.Version = "1.0";
            })
            .WithAggregateId(order.Id.ToString())
            .WithAggregateType("Order")
            .WithPriority(1)
            .WithTtl(TimeSpan.FromMinutes(30))
            .PublishAsync(new OrderCreated(order.Id, order.CustomerName, order.Total));
        
        // 3. Store event in event store with fluent API
        await _streamFlow.EventStore.Stream($"order-{order.Id}")
            .AppendEvent(new OrderCreated(order.Id, order.CustomerName, order.Total))
            .AppendEvent(new OrderItemsAdded(order.Id, order.Items))
            .SaveAsync();
        
        // 4. Start saga for order processing
        await _streamFlow.SagaOrchestrator.StartSagaAsync<OrderProcessingSaga>(
            sagaId: order.Id,
            initialEvent: new OrderCreated(order.Id, order.CustomerName, order.Total));
    }
}

// Event handlers with automatic retry and error handling
public class OrderCreatedHandler : IAsyncEventHandler<OrderCreated>
{
    private readonly IStreamFlowClient _streamFlow;
    
    public async Task HandleAsync(OrderCreated @event, EventContext context)
    {
        try
        {
            // Business logic with automatic retry on failure
            await _inventoryService.ReserveItemsAsync(@event.OrderId);
            
            // Publish follow-up events
            await _streamFlow.EventBus.Event<InventoryRequested>()
                .WithMetadata(metadata =>
                {
                    metadata.CorrelationId = context.CorrelationId;
                    metadata.CausationId = context.EventId;
                    metadata.Source = "inventory-service";
                })
                .WithAggregateId(@event.OrderId.ToString())
                .WithAggregateType("Order")
                .PublishAsync(new InventoryRequested(@event.OrderId, @event.Items));
        }
        catch (Exception ex)
        {
            // Automatic error handling with dead letter queue
            throw new BusinessException("Failed to process order", ex);
        }
    }
}
```

### Example 2: Microservices Integration

```csharp
// Service A - Order Service
public class OrderService
{
    private readonly IStreamFlowClient _streamFlow;
    
    public async Task CreateOrderAsync(CreateOrderRequest request)
    {
        var order = new Order(request.CustomerId, request.Items);
        
        // Save to database
        await _orderRepository.SaveAsync(order);
        
        // Publish integration event for other services
        await _streamFlow.EventBus.Event<OrderCreated>()
            .WithCorrelationId(Guid.NewGuid().ToString())
            .WithSource("order-service")
            .WithVersion("1.0")
            .WithAggregateId(order.Id.ToString())
            .WithAggregateType("Order")
            .PublishAsync(new OrderCreated(order.Id, order.CustomerId, order.Items));
    }
}

// Service B - Inventory Service
public class InventoryService
{
    private readonly IStreamFlowClient _streamFlow;
    
    public async Task StartAsync()
    {
        // Subscribe to order events
        await _streamFlow.Consumer.Queue<OrderCreated>("inventory-service")
            .WithConcurrency(3)
            .WithPrefetchCount(50)
            .WithErrorHandler(async (exception, context) =>
            {
                // Custom error handling
                return exception is TransientException;
            })
            .ConsumeAsync(async (orderCreated, context) =>
            {
                // Reserve inventory
                var reserved = await ReserveInventoryAsync(orderCreated.Items);
                
                // Publish inventory reserved event
                await _streamFlow.EventBus.Event<InventoryReserved>()
                    .WithCorrelationId(context.CorrelationId)
                    .WithCausationId(context.MessageId)
                    .WithSource("inventory-service")
                    .WithAggregateId(orderCreated.OrderId.ToString())
                    .WithAggregateType("Order")
                    .PublishAsync(new InventoryReserved(orderCreated.OrderId, reserved));
                
                return true; // Acknowledge message
            });
    }
}

// Service C - Payment Service
public class PaymentService
{
    private readonly IStreamFlowClient _streamFlow;
    
    public async Task StartAsync()
    {
        await _streamFlow.Consumer.Queue<InventoryReserved>("payment-service")
            .WithConcurrency(2)
            .WithPrefetchCount(20)
            .WithRetryPolicy(new RetryPolicySettings
            {
                RetryPolicy = RetryPolicyType.ExponentialBackoff,
                MaxRetryAttempts = 3,
                RetryDelay = TimeSpan.FromSeconds(1)
            })
            .ConsumeAsync(async (inventoryReserved, context) =>
            {
                // Process payment
                var payment = await ProcessPaymentAsync(inventoryReserved.OrderId);
                
                // Publish payment processed event
                await _streamFlow.EventBus.Event<PaymentProcessed>()
                    .WithCorrelationId(context.CorrelationId)
                    .WithCausationId(context.MessageId)
                    .WithSource("payment-service")
                    .WithAggregateId(inventoryReserved.OrderId.ToString())
                    .WithAggregateType("Order")
                    .WithProperty("transaction-id", payment.TransactionId)
                    .PublishAsync(new PaymentProcessed(inventoryReserved.OrderId, payment.TransactionId));
                
                return true;
            });
    }
}
```

### Example 3: Real-time Analytics

```csharp
// High-throughput analytics processing
public class AnalyticsProcessor
{
    private readonly IStreamFlowClient _streamFlow;
    
    public async Task StartProcessingAsync()
    {
        // Process user events in real-time
        await _streamFlow.Consumer.Queue<UserEvent>("analytics-processing")
            .WithConcurrency(10)
            .WithPrefetchCount(1000)
            .WithAutoAck(true) // High-throughput, less durability
            .WithErrorHandler(async (exception, context) =>
            {
                // Log and continue processing
                _logger.LogError(exception, "Error processing user event");
                return false; // Don't requeue analytics events
            })
            .ConsumeAsync(async (userEvent, context) =>
            {
                // Process analytics
                await ProcessUserEventAsync(userEvent);
                
                // Batch analytics data
                await _analyticsBuffer.AddAsync(userEvent);
                
                return true;
            });
    }
}

// Configuration for high-throughput scenarios
builder.Services.AddRabbitMQStreamFlow(options =>
{
    // Client configuration
    options.ClientConfiguration.ClientName = "High-Throughput Application";
    options.ClientConfiguration.EnableAutoRecovery = true;
    options.ClientConfiguration.EnableHeartbeat = true;
    options.ClientConfiguration.HeartbeatInterval = TimeSpan.FromSeconds(30);
    
    // Connection settings
    options.ConnectionSettings.Host = "localhost";
    options.ConnectionSettings.Port = 5672;
    options.ConnectionSettings.Username = "guest";
    options.ConnectionSettings.Password = "guest";
    options.ConnectionSettings.VirtualHost = "/";
    options.ConnectionSettings.ConnectionTimeout = TimeSpan.FromSeconds(30);
    
    // Producer settings
    options.ProducerSettings.EnablePublisherConfirms = true;
    options.ProducerSettings.MaxConcurrentPublishes = 1000;
    
    // Consumer settings
    options.ConsumerSettings.PrefetchCount = 100;
    options.ConsumerSettings.MaxConcurrentConsumers = 10;
    options.ConsumerSettings.AutoAcknowledge = false;
});
```

## ⚙️ Configuration Reference

### Fluent Configuration API

Complete configuration with the options pattern:

```csharp
builder.Services.AddRabbitMQStreamFlow(options =>
{
    // Client configuration
    options.ClientConfiguration.ClientName = "My Application";
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
    
    // Error handling
    options.ErrorHandling.EnableDeadLetterQueue = true;
    options.ErrorHandling.DeadLetterExchange = "dlx";
    options.ErrorHandling.DeadLetterQueue = "dlq";
    options.ErrorHandling.RetryPolicy = RetryPolicyType.ExponentialBackoff;
    options.ErrorHandling.MaxRetryAttempts = 3;
    options.ErrorHandling.RetryDelay = TimeSpan.FromSeconds(1);
    
    // Serialization
    options.Serialization.Format = SerializationFormat.Json;
    options.Serialization.UseCompression = true;
    
    // Health checks
    options.HealthCheck.Enabled = true;
    options.HealthCheck.Interval = TimeSpan.FromSeconds(30);
    options.HealthCheck.Timeout = TimeSpan.FromSeconds(5);
    
    // Event bus
    options.EventBus.DomainEventExchange = "domain-events";
    options.EventBus.IntegrationEventExchange = "integration-events";
    
    // Event store
    options.EventStore.StreamPrefix = "eventstore";
    options.EventStore.SnapshotInterval = 100;
    options.EventStore.EnableSnapshots = true;
    
    // Monitoring
    options.Monitoring.EnableMetrics = true;
    options.Monitoring.MetricsInterval = TimeSpan.FromSeconds(60);
});
```

### Configuration via appsettings.json

```json
{
  "RabbitMQ": {
    "ClientConfiguration": {
      "ClientName": "My Application",
      "EnableAutoRecovery": true,
      "EnableHeartbeat": true,
      "HeartbeatInterval": "00:01:00"
    },
    "ConnectionSettings": {
      "Host": "localhost",
      "Port": 5672,
      "Username": "guest",
      "Password": "guest",
      "VirtualHost": "/",
      "ConnectionTimeout": "00:00:30",
      "RequestTimeout": "00:00:30",
      "UseSsl": false
    },
    "ProducerSettings": {
      "EnablePublisherConfirms": true,
      "ConfirmationTimeout": "00:00:10",
      "MaxConcurrentPublishes": 100,
      "PublishTimeout": "00:00:30",
      "DefaultExchange": "",
      "DefaultRoutingKey": ""
    },
    "ConsumerSettings": {
      "PrefetchCount": 50,
      "AutoAcknowledge": false,
      "MaxConcurrentConsumers": 5,
      "ConsumerTimeout": "00:00:30",
      "EnableDeadLetterQueue": true
    }
  }
}
```

```csharp
// Use configuration from appsettings.json
builder.Services.Configure<RabbitMQStreamFlowOptions>(
    builder.Configuration.GetSection("StreamFlow:RabbitMQ"));
```

## 🔧 Advanced Features

### Fluent Infrastructure Management

Set up your messaging infrastructure with intuitive fluent APIs:

```csharp
// Complete infrastructure setup
public class InfrastructureSetup
{
    private readonly IStreamFlowClient _streamFlow;
    
    public async Task SetupAsync()
    {
        // Create exchanges with fluent API
        await _streamFlow.ExchangeManager.Exchange("orders")
            .AsTopic()
            .WithDurable(true)
            .WithAlternateExchange("alt-orders")
            .DeclareAsync();
            
        await _streamFlow.ExchangeManager.Exchange("notifications")
            .AsFanout()
            .WithDurable(true)
            .DeclareAsync();
        
        // Create queues with fluent API
        await _streamFlow.QueueManager.Queue("order-processing")
            .WithDurable(true)
            .WithDeadLetterExchange("dlx")
            .WithDeadLetterRoutingKey("order.failed")
            .WithMessageTtl(TimeSpan.FromHours(24))
            .WithMaxLength(10000)
            .WithPriority(5)
            .BindToExchange("orders", "order.created")
            .BindToExchange("orders", "order.updated")
            .DeclareAsync();
            
        await _streamFlow.QueueManager.Queue("email-notifications")
            .WithDurable(true)
            .BindToExchange("notifications", "")
            .DeclareAsync();
            
        // Create event streams with fluent API
        await _streamFlow.EventStore.Stream("order-events")
            .CreateAsync();
            
        await _streamFlow.EventStore.Stream("user-events")
            .CreateAsync();
    }
}
```

### Fluent Producer API

Chain operations with the comprehensive fluent API:

```csharp
// Advanced message publishing with fluent API
await _streamFlow.Producer.Message(order)
    .WithExchange("orders")
    .WithRoutingKey("order.created")
    .WithDeliveryMode(DeliveryMode.Persistent)
    .WithExpiration(TimeSpan.FromHours(24))
    .WithPriority(5)
    .WithHeaders(new Dictionary<string, object>
    {
        ["source"] = "order-service",
        ["version"] = "1.0",
        ["correlation-id"] = Guid.NewGuid().ToString()
    })
    .WithHeader("priority", "high")
    .WithMandatory(true)
    .WithConfirmationTimeout(TimeSpan.FromSeconds(5))
    .PublishAsync();

// Alternative syntax
await _streamFlow.Producer.Message(order)
    .ToExchange("orders")
    .WithRoutingKey("order.created")
    .WithDeliveryMode(DeliveryMode.Persistent)
    .PublishAsync();
```

### Fluent Consumer API

Configure consumers with comprehensive fluent syntax:

```csharp
// Advanced consumer configuration with fluent API
await _streamFlow.Consumer.Queue<Order>("order-processing")
    .FromExchange("orders")
    .WithRoutingKey("order.*")
    .WithSettings(settings =>
    {
        settings.PrefetchCount = 100;
        settings.MaxConcurrentConsumers = 5;
        settings.AutoAcknowledge = false;
    })
    .WithConsumerTag("order-processor-1")
    .WithAutoAck(false)
    .WithPrefetchCount(100)
    .WithConcurrency(5)
    .WithErrorHandler(async (exception, context) =>
    {
        if (exception is TransientException)
            return true; // Requeue
        
        await _deadLetterService.SendAsync(context.Message);
        return false; // Don't requeue
    })
    .WithRetryPolicy(new RetryPolicySettings
    {
        RetryPolicy = RetryPolicyType.ExponentialBackoff,
        MaxRetryAttempts = 3,
        RetryDelay = TimeSpan.FromSeconds(1)
    })
    .WithDeadLetterQueue(new DeadLetterSettings
    {
        DeadLetterExchange = "dlx",
        DeadLetterQueue = "dlq"
    })
    .ConsumeAsync(async (order, context) =>
    {
        await ProcessOrderAsync(order);
        return true;
    });
```

### Fluent Event Bus API

Manage events with comprehensive fluent operations:

```
```
