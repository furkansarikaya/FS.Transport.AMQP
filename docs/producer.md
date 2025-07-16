# Producer Guide

This guide covers everything you need to know about producing messages with FS.StreamFlow, from basic publishing to advanced enterprise patterns.

## 📋 Table of Contents

1. [Getting Started](#getting-started)
2. [Basic Publishing](#basic-publishing)
3. [Advanced Publishing](#advanced-publishing)
4. [Publisher Confirmations](#publisher-confirmations)
5. [Batch Operations](#batch-operations)
6. [Transactional Publishing](#transactional-publishing)
7. [Fluent Producer API](#fluent-producer-api)
8. [Error Handling](#error-handling)
9. [Performance Optimization](#performance-optimization)
10. [Monitoring and Metrics](#monitoring-and-metrics)
11. [Best Practices](#best-practices)

## 🚀 Getting Started

### Basic Producer Setup

```csharp
// Program.cs
using FS.StreamFlow.RabbitMQ.DependencyInjection;

var builder = WebApplication.CreateBuilder(args);

// Add FS.StreamFlow with producer configuration
builder.Services.AddRabbitMQStreamFlow(options =>
{
    // Connection settings
    options.ConnectionSettings.Host = "localhost";
    options.ConnectionSettings.Port = 5672;
    options.ConnectionSettings.Username = "guest";
    options.ConnectionSettings.Password = "guest";
    options.ConnectionSettings.VirtualHost = "/";
    
    // Producer settings
    options.ProducerSettings.EnablePublisherConfirms = true;
    options.ProducerSettings.ConfirmationTimeout = TimeSpan.FromSeconds(10);
    options.ProducerSettings.MaxConcurrentPublishes = 100;
});

var app = builder.Build();
```

### Injecting the Producer

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

    public async Task CreateOrderAsync(Order order)
    {
        // Initialize the client first
        await _streamFlow.InitializeAsync();
        
        // Setup infrastructure with fluent API
        await _streamFlow.ExchangeManager.Exchange("orders")
            .AsTopic()
            .WithDurable(true)
            .DeclareAsync();
            
        await _streamFlow.QueueManager.Queue("order-processing")
            .WithDurable(true)
            .WithDeadLetterExchange("dlx")
            .BindToExchange("orders", "order.created")
            .DeclareAsync();
            
        // Publish with fluent API
        // Option 1: With pre-configured message (can use PublishAsync() without parameters)
        await _streamFlow.Producer.Message(order)
            .WithExchange("orders")
            .WithRoutingKey("order.created")
            .WithDeliveryMode(DeliveryMode.Persistent)
            .PublishAsync();

        // Option 2: With generic type (MUST pass message to PublishAsync)
        await _streamFlow.Producer.Message<Order>()
            .WithExchange("orders")
            .WithRoutingKey("order.created")
            .WithDeliveryMode(DeliveryMode.Persistent)
            .PublishAsync(order);
    }
}
```

## 📤 Basic Publishing

### Simple Message Publishing

```csharp
public class MessageService
{
    private readonly IStreamFlowClient _streamFlow;

    public MessageService(IStreamFlowClient streamFlow) => _streamFlow = streamFlow;

    // Publish a simple message with fluent API
    public async Task PublishSimpleMessageAsync()
    {
        // Initialize the client first
        await _streamFlow.InitializeAsync();
        
        // Option 1: With pre-configured message (can use PublishAsync() without parameters)
        await _streamFlow.Producer.Message("Hello, World!")
            .WithExchange("notifications")
            .WithRoutingKey("email.send")
            .PublishAsync();

        // Option 2: With generic type (MUST pass message to PublishAsync)
        await _streamFlow.Producer.Message<string>()
            .WithExchange("notifications")
            .WithRoutingKey("email.send")
            .PublishAsync("Hello, World!");
    }

    // Publish a complex object with fluent API
    public async Task PublishOrderAsync(Order order)
    {
        // Initialize the client first
        await _streamFlow.InitializeAsync();
        
        // Option 1: With pre-configured message (can use PublishAsync() without parameters)
        await _streamFlow.Producer.Message(order)
            .WithExchange("orders")
            .WithRoutingKey("order.created")
            .WithDeliveryMode(DeliveryMode.Persistent)
            .WithExpiration(TimeSpan.FromHours(24))
            .WithPriority(5)
            .PublishAsync();

        // Option 2: With generic type (MUST pass message to PublishAsync)
        await _streamFlow.Producer.Message<Order>()
            .WithExchange("orders")
            .WithRoutingKey("order.created")
            .WithDeliveryMode(DeliveryMode.Persistent)
            .WithExpiration(TimeSpan.FromHours(24))
            .WithPriority(5)
            .PublishAsync(order);
    }

    // Publish with byte array
    public async Task PublishBytesAsync(byte[] data)
    {
        // Initialize the client first
        await _streamFlow.InitializeAsync();
        
        // Option 1: With pre-configured message (can use PublishAsync() without parameters)
        await _streamFlow.Producer.Message(data)
            .WithExchange("data")
            .WithRoutingKey("binary.data")
            .WithDeliveryMode(DeliveryMode.Persistent)
            .PublishAsync();

        // Option 2: With generic type (MUST pass message to PublishAsync)
        await _streamFlow.Producer.Message<byte[]>()
            .WithExchange("data")
            .WithRoutingKey("binary.data")
            .WithDeliveryMode(DeliveryMode.Persistent)
            .PublishAsync(data);
    }
}
```

### Advanced Message Publishing with Fluent API

```csharp
public async Task PublishAdvancedMessageAsync(Order order)
{
    // Initialize the client first
    await _streamFlow.InitializeAsync();
    
    // Option 1: With pre-configured message (can use PublishAsync() without parameters)
    var result = await _streamFlow.Producer.Message(order)
        .WithExchange("orders")
        .WithRoutingKey("order.created")
        .WithDeliveryMode(DeliveryMode.Persistent)
        .WithExpiration(TimeSpan.FromMinutes(30))
        .WithPriority(5)
        .WithHeaders(new Dictionary<string, object>
        {
            ["source"] = "order-service",
            ["version"] = "1.0",
            ["timestamp"] = DateTimeOffset.UtcNow.ToUnixTimeSeconds(),
            ["correlation-id"] = Guid.NewGuid().ToString(),
            ["retry-policy"] = "exponential-backoff",
            ["confirmation-timeout"] = TimeSpan.FromSeconds(5).TotalMilliseconds
        })
        .PublishAsync();

    // Option 2: With generic type (MUST pass message to PublishAsync)
    // var result = await _streamFlow.Producer.Message<Order>()
    //     .WithExchange("orders")
    //     .WithRoutingKey("order.created")
    //     .WithDeliveryMode(DeliveryMode.Persistent)
    //     .WithExpiration(TimeSpan.FromMinutes(30))
    //     .WithPriority(5)
    //     .WithHeaders(new Dictionary<string, object>
    //     {
    //         ["source"] = "order-service",
    //         ["version"] = "1.0",
    //         ["timestamp"] = DateTimeOffset.UtcNow.ToUnixTimeSeconds(),
    //         ["correlation-id"] = Guid.NewGuid().ToString()
    //     })
    //     .PublishAsync(order);
        
    if (result.IsSuccess)
    {
        Console.WriteLine($"Order {order.Id} published successfully!");
    }
    else
    {
        Console.WriteLine($"Failed to publish order {order.Id}: {result.Error}");
    }
}
```

### Message Properties (Legacy API)

```csharp
// Legacy API - still supported but fluent API is recommended
public async Task PublishWithPropertiesAsync(Order order)
{
    // Initialize the client first
    await _streamFlow.InitializeAsync();
    
    var properties = new BasicProperties
    {
        MessageId = Guid.NewGuid().ToString(),
        CorrelationId = "order-123",
        ReplyTo = "order-responses",
        Expiration = TimeSpan.FromMinutes(30).TotalMilliseconds.ToString(),
        Priority = 5,
        DeliveryMode = DeliveryMode.Persistent,
        ContentType = "application/json",
        ContentEncoding = "utf-8",
        Headers = new Dictionary<string, object>
        {
            ["source"] = "order-service",
            ["version"] = "1.0",
            ["timestamp"] = DateTimeOffset.UtcNow.ToUnixTimeSeconds()
        }
    };

    await _streamFlow.Producer.PublishAsync(
        exchange: "orders",
        routingKey: "order.created",
        message: order,
        properties: properties);
}
```

### Default Exchange Publishing

```csharp
// Publish directly to a queue using default exchange
public async Task PublishToQueueAsync(string queueName, object message)
{
    // Initialize the client first
    await _streamFlow.InitializeAsync();
    
    // Option 1: With pre-configured message (can use PublishAsync() without parameters)
    await _streamFlow.Producer.Message(message)
        .WithExchange("") // Default exchange
        .WithRoutingKey(queueName)
        .WithDeliveryMode(DeliveryMode.Persistent)
        .PublishAsync();

    // Option 2: With generic type (MUST pass message to PublishAsync)
    await _streamFlow.Producer.Message<object>()
        .WithExchange("") // Default exchange
        .WithRoutingKey(queueName)
        .WithDeliveryMode(DeliveryMode.Persistent)
        .PublishAsync(message);
}
```

### Infrastructure Setup with Fluent API

```csharp
// Complete infrastructure setup before publishing
public class InfrastructureSetup
{
    private readonly IStreamFlowClient _streamFlow;
    
    public async Task SetupOrderInfrastructureAsync()
    {
        // Initialize the client first
        await _streamFlow.InitializeAsync();
        
        // Create main exchange
        await _streamFlow.ExchangeManager.Exchange("orders")
            .AsTopic()
            .WithDurable(true)
            .WithAlternateExchange("alt-orders")
            .DeclareAsync();
            
        // Create dead letter exchange
        await _streamFlow.ExchangeManager.Exchange("dlx")
            .AsDirect()
            .WithDurable(true)
            .DeclareAsync();
            
        // Create processing queue
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
            
        // Create dead letter queue
        await _streamFlow.QueueManager.Queue("dlq")
            .WithDurable(true)
            .BindToExchange("dlx", "order.failed")
            .DeclareAsync();
    }
}
```

## 🔧 Advanced Publishing

### Publishing with Routing Patterns

```csharp
public class AdvancedPublisher
{
    private readonly IStreamFlowClient _streamFlow;

    public AdvancedPublisher(IStreamFlowClient streamFlow) => _streamFlow = streamFlow;

    // Topic exchange routing
    public async Task PublishTopicMessageAsync(string region, string category, object message)
    {
        // Initialize the client first
        await _streamFlow.InitializeAsync();
        
        var routingKey = $"{region}.{category}";
        
        await _streamFlow.Producer.PublishAsync(
            exchange: "topic-exchange",
            routingKey: routingKey,
            message: message);
    }

    // Fanout exchange publishing
    public async Task PublishBroadcastMessageAsync(object message)
    {
        // Initialize the client first
        await _streamFlow.InitializeAsync();
        
        await _streamFlow.Producer.PublishAsync(
            exchange: "fanout-exchange",
            routingKey: "", // Ignored for fanout
            message: message);
    }

    // Headers exchange publishing
    public async Task PublishHeadersMessageAsync(object message, Dictionary<string, object> headers)
    {
        // Initialize the client first
        await _streamFlow.InitializeAsync();
        
        var properties = new BasicProperties
        {
            Headers = headers
        };

        await _streamFlow.Producer.PublishAsync(
            exchange: "headers-exchange",
            routingKey: "", // Ignored for headers
            message: message,
            properties: properties);
    }
}
```

### Message Scheduling

```csharp
public class ScheduledPublisher
{
    private readonly IStreamFlowClient _streamFlow;

    public ScheduledPublisher(IStreamFlowClient streamFlow) => _streamFlow = streamFlow;

    // Schedule message for future delivery
    public async Task ScheduleMessageAsync(object message, TimeSpan delay)
    {
        // Initialize the client first
        await _streamFlow.InitializeAsync();
        
        await _streamFlow.Producer.ScheduleAsync(
            message: message,
            delay: delay);
    }

    // Schedule message for specific time
    public async Task ScheduleMessageAsync(object message, DateTimeOffset scheduleTime)
    {
        // Initialize the client first
        await _streamFlow.InitializeAsync();
        
        var delay = scheduleTime - DateTimeOffset.UtcNow;
        if (delay > TimeSpan.Zero)
        {
            await _streamFlow.Producer.ScheduleAsync(
                message: message,
                delay: delay);
        }
    }

    // Schedule recurring message
    public async Task ScheduleRecurringMessageAsync(object message, TimeSpan interval)
    {
        // Initialize the client first
        await _streamFlow.InitializeAsync();
        
        // Note: This is a simplified example
        // In production, use a proper scheduling library or service
        using var timer = new Timer(async _ =>
        {
            await _streamFlow.Producer.PublishAsync(
                exchange: "scheduled",
                routingKey: "recurring.message",
                message: message);
        }, null, TimeSpan.Zero, interval);

        // Keep timer alive
        await Task.Delay(Timeout.Infinite);
    }
}
```

## ✅ Publisher Confirmations

### Basic Confirmations

```csharp
public class ConfirmationPublisher
{
    private readonly IStreamFlowClient _streamFlow;
    private readonly ILogger<ConfirmationPublisher> _logger;

    public ConfirmationPublisher(IStreamFlowClient streamFlow, ILogger<ConfirmationPublisher> logger)
    {
        _streamFlow = streamFlow;
        _logger = logger;
    }

    public async Task PublishWithConfirmationAsync(Order order)
    {
        // Initialize the client first
        await _streamFlow.InitializeAsync();
        
        try
        {
            // Enable confirmations for this operation
            var success = await _streamFlow.Producer.PublishAsync(
                exchange: "orders",
                routingKey: "order.created",
                message: order,
                waitForConfirmation: true);

            if (success)
            {
                _logger.LogInformation("Order {OrderId} published successfully", order.Id);
            }
            else
            {
                _logger.LogError("Failed to publish order {OrderId}", order.Id);
                throw new InvalidOperationException("Message was not confirmed by broker");
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error publishing order {OrderId}", order.Id);
            throw;
        }
    }
}
```

### Advanced Confirmations with Callbacks

```csharp
public class CallbackPublisher
{
    private readonly IStreamFlowClient _streamFlow;
    private readonly ILogger<CallbackPublisher> _logger;

    public CallbackPublisher(IStreamFlowClient streamFlow, ILogger<CallbackPublisher> logger)
    {
        _streamFlow = streamFlow;
        _logger = logger;
        
        // Subscribe to confirmation events
        _streamFlow.Producer.MessageConfirmed += OnMessageConfirmed;
        _streamFlow.Producer.ErrorOccurred += OnErrorOccurred;
    }

    private async Task OnMessageConfirmed(ulong deliveryTag, bool multiple)
    {
        _logger.LogInformation("Message {DeliveryTag} confirmed (multiple: {Multiple})", 
            deliveryTag, multiple);
    }

    private async Task OnErrorOccurred(Exception exception)
    {
        _logger.LogError(exception, "Producer error occurred");
    }

    public async Task PublishWithCallbackAsync(Order order)
    {
        // Initialize the client first
        await _streamFlow.InitializeAsync();
        
        await _streamFlow.Producer.PublishAsync(
            exchange: "orders",
            routingKey: "order.created",
            message: order);
    }
}
```

### Bulk Confirmations

```csharp
public class BulkConfirmationPublisher
{
    private readonly IStreamFlowClient _streamFlow;
    private readonly ILogger<BulkConfirmationPublisher> _logger;

    public BulkConfirmationPublisher(IStreamFlowClient rabbitMQ, ILogger<BulkConfirmationPublisher> logger)
    {
        _streamFlow = rabbitMQ;
        _logger = logger;
    }

    public async Task PublishBulkWithConfirmationsAsync(IEnumerable<Order> orders)
    {
        // Initialize the client first
        await _streamFlow.InitializeAsync();
        
        var publishTasks = new List<Task<bool>>();

        foreach (var order in orders)
        {
            var task = _streamFlow.Producer.PublishAsync(
                exchange: "orders",
                routingKey: "order.created",
                message: order,
                waitForConfirmation: true);
            
            publishTasks.Add(task);
        }

        var results = await Task.WhenAll(publishTasks);
        
        var successCount = results.Count(r => r);
        var failureCount = results.Length - successCount;

        _logger.LogInformation(
            "Bulk publish completed. Success: {SuccessCount}, Failures: {FailureCount}", 
            successCount, failureCount);

        if (failureCount > 0)
        {
            throw new InvalidOperationException($"{failureCount} messages failed to publish");
        }
    }
}
```

## 📦 Batch Operations

### Basic Batch Publishing

```csharp
public class BatchPublisher
{
    private readonly IStreamFlowClient _streamFlow;
    private readonly ILogger<BatchPublisher> _logger;

    public BatchPublisher(IStreamFlowClient rabbitMQ, ILogger<BatchPublisher> logger)
    {
        _streamFlow = rabbitMQ;
        _logger = logger;
    }

    public async Task PublishBatchAsync(IEnumerable<Order> orders)
    {
        // Initialize the client first
        await _streamFlow.InitializeAsync();
        
        var messageContexts = orders.Select(order => new MessageContext
        {
            Exchange = "orders",
            RoutingKey = "order.created",
            Message = order
        });

        var results = await _streamFlow.Producer.PublishBatchAsync(messageContexts);
        
        _logger.LogInformation("Published {Count} orders in batch", results.Count);
    }
}
```

### Advanced Batch Operations

```csharp
public class AdvancedBatchPublisher
{
    private readonly IStreamFlowClient _streamFlow;
    private readonly ILogger<AdvancedBatchPublisher> _logger;

    public AdvancedBatchPublisher(IStreamFlowClient rabbitMQ, ILogger<AdvancedBatchPublisher> logger)
    {
        _streamFlow = rabbitMQ;
        _logger = logger;
    }

    public async Task PublishMixedBatchAsync(IEnumerable<object> messages)
    {
        // Initialize the client first
        await _streamFlow.InitializeAsync();
        
        var messageContexts = messages.Select(message => new MessageContext
        {
            Exchange = GetExchangeForMessage(message),
            RoutingKey = GetRoutingKeyForMessage(message),
            Message = message,
            Properties = GetPropertiesForMessage(message)
        });

        var results = await _streamFlow.Producer.PublishBatchAsync(messageContexts);
        
        foreach (var result in results)
        {
            if (result.IsSuccess)
            {
                _logger.LogInformation("Message {MessageId} published successfully", result.MessageId);
            }
            else
            {
                _logger.LogError("Failed to publish message {MessageId}: {Error}", 
                    result.MessageId, result.Error);
            }
        }
    }

    private string GetExchangeForMessage(object message)
    {
        return message switch
        {
            Order => "orders",
            User => "users",
            Product => "products",
            _ => "default"
        };
    }

    private string GetRoutingKeyForMessage(object message)
    {
        return message switch
        {
            Order => "order.created",
            User => "user.registered",
            Product => "product.added",
            _ => "message.generic"
        };
    }

    private BasicProperties GetPropertiesForMessage(object message)
    {
        return new BasicProperties
        {
            MessageId = Guid.NewGuid().ToString(),
            Timestamp = DateTimeOffset.UtcNow,
            ContentType = "application/json",
            DeliveryMode = DeliveryMode.Persistent,
            Headers = new Dictionary<string, object>
            {
                ["message-type"] = message.GetType().Name,
                ["timestamp"] = DateTimeOffset.UtcNow.ToUnixTimeSeconds()
            }
        };
    }
}
```

### Batch with Partitioning

```csharp
public class PartitionedBatchPublisher
{
    private readonly IStreamFlowClient _streamFlow;
    private readonly ILogger<PartitionedBatchPublisher> _logger;

    public PartitionedBatchPublisher(IStreamFlowClient rabbitMQ, ILogger<PartitionedBatchPublisher> logger)
    {
        _streamFlow = rabbitMQ;
        _logger = logger;
    }

    public async Task PublishPartitionedBatchAsync(IEnumerable<Order> orders, int batchSize = 100)
    {
        var batches = orders
            .Select((order, index) => new { order, index })
            .GroupBy(x => x.index / batchSize)
            .Select(g => g.Select(x => x.order));

        var publishTasks = new List<Task>();

        foreach (var batch in batches)
        {
            var task = PublishBatchAsync(batch);
            publishTasks.Add(task);
        }

        await Task.WhenAll(publishTasks);
        
        _logger.LogInformation("Published {TotalOrders} orders in {BatchCount} batches", 
            orders.Count(), publishTasks.Count);
    }

    private async Task PublishBatchAsync(IEnumerable<Order> orders)
    {
        var messageContexts = orders.Select(order => new MessageContext
        {
            Exchange = "orders",
            RoutingKey = $"order.created.{order.CustomerId}",
            Message = order
        });

        await _streamFlow.Producer.PublishBatchAsync(messageContexts);
    }
}
```

## 🔒 Transactional Publishing

### Basic Transactions

```csharp
public class TransactionalPublisher
{
    private readonly IStreamFlowClient _streamFlow;
    private readonly ILogger<TransactionalPublisher> _logger;

    public TransactionalPublisher(IStreamFlowClient rabbitMQ, ILogger<TransactionalPublisher> logger)
    {
        _streamFlow = rabbitMQ;
        _logger = logger;
    }

    public async Task PublishTransactionalAsync(Order order)
    {
        await _streamFlow.Producer.PublishTransactionalAsync(async () =>
        {
            // All operations within this block are transactional
            await _streamFlow.Producer.PublishAsync(
                exchange: "orders",
                routingKey: "order.created",
                message: order);

            await _streamFlow.Producer.PublishAsync(
                exchange: "audit",
                routingKey: "audit.order.created",
                message: new AuditEvent
                {
                    EntityType = "Order",
                    EntityId = order.Id.ToString(),
                    Action = "Created",
                    Timestamp = DateTimeOffset.UtcNow
                });
        });
    }
}
```

### Advanced Transactional Operations

```csharp
public class AdvancedTransactionalPublisher
{
    private readonly IStreamFlowClient _streamFlow;
    private readonly ILogger<AdvancedTransactionalPublisher> _logger;

    public AdvancedTransactionalPublisher(IStreamFlowClient rabbitMQ, ILogger<AdvancedTransactionalPublisher> logger)
    {
        _streamFlow = rabbitMQ;
        _logger = logger;
    }

    public async Task ProcessOrderTransactionallyAsync(Order order)
    {
        try
        {
            await _streamFlow.Producer.PublishTransactionalAsync(async () =>
            {
                // Publish order created event
                await _streamFlow.Producer.PublishAsync(
                    exchange: "orders",
                    routingKey: "order.created",
                    message: order);

                // Publish inventory reservation request
                await _streamFlow.Producer.PublishAsync(
                    exchange: "inventory",
                    routingKey: "inventory.reserve",
                    message: new InventoryReservationRequest
                    {
                        OrderId = order.Id,
                        Items = order.Items
                    });

                // Publish payment request
                await _streamFlow.Producer.PublishAsync(
                    exchange: "payments",
                    routingKey: "payment.process",
                    message: new PaymentRequest
                    {
                        OrderId = order.Id,
                        Amount = order.Total,
                        CustomerId = order.CustomerId
                    });

                // Publish audit event
                await _streamFlow.Producer.PublishAsync(
                    exchange: "audit",
                    routingKey: "audit.order.workflow.started",
                    message: new AuditEvent
                    {
                        EntityType = "Order",
                        EntityId = order.Id.ToString(),
                        Action = "WorkflowStarted",
                        Timestamp = DateTimeOffset.UtcNow
                    });
            });

            _logger.LogInformation("Order {OrderId} processed transactionally", order.Id);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to process order {OrderId} transactionally", order.Id);
            throw;
        }
    }
}
```

## 🌊 Fluent Producer API

### Basic Fluent Operations

```csharp
public class FluentPublisher
{
    private readonly IStreamFlowClient _streamFlow;

    public FluentPublisher(IStreamFlowClient rabbitMQ) => _streamFlow = rabbitMQ;

    public async Task PublishWithFluentApiAsync(Order order)
    {
        await _streamFlow.Producer
            .Message(order)
            .ToExchange("orders")
            .WithRoutingKey("order.created")
            .WithDeliveryMode(DeliveryMode.Persistent)
            .WithPriority(5)
            .WithExpiration(TimeSpan.FromMinutes(30))
            .WithHeaders(new Dictionary<string, object>
            {
                ["source"] = "order-service",
                ["version"] = "1.0"
            })
            .PublishAsync();
    }
}
```

### Advanced Fluent Operations

```csharp
public class AdvancedFluentPublisher
{
    private readonly IStreamFlowClient _streamFlow;

    public AdvancedFluentPublisher(IStreamFlowClient rabbitMQ) => _streamFlow = rabbitMQ;

    public async Task PublishComplexFluentAsync(Order order)
    {
        var result = await _streamFlow.Producer
            .Message(order)
            .ToExchange("orders")
            .WithRoutingKey($"order.created.{order.CustomerId}")
            .WithMessageId(order.Id.ToString())
            .WithCorrelationId(Guid.NewGuid().ToString())
            .WithReplyTo("order-responses")
            .WithDeliveryMode(DeliveryMode.Persistent)
            .WithPriority(order.Priority)
            .WithExpiration(TimeSpan.FromHours(24))
            .WithContentType("application/json")
            .WithContentEncoding("utf-8")
            .WithHeaders(new Dictionary<string, object>
            {
                ["source"] = "order-service",
                ["version"] = "1.0",
                ["customer-id"] = order.CustomerId,
                ["order-type"] = order.Type,
                ["total-amount"] = order.Total
            })
            .WithConfirmation(true)
            .WithTimeout(TimeSpan.FromSeconds(30))
            .PublishAsync();

        if (result.IsSuccess)
        {
            Console.WriteLine($"Order {order.Id} published successfully with message ID {result.MessageId}");
        }
        else
        {
            Console.WriteLine($"Failed to publish order {order.Id}: {result.Error}");
        }
    }
}
```

### Fluent Batch Operations

```csharp
public class FluentBatchPublisher
{
    private readonly IStreamFlowClient _streamFlow;

    public FluentBatchPublisher(IStreamFlowClient rabbitMQ) => _streamFlow = rabbitMQ;

    public async Task PublishFluentBatchAsync(IEnumerable<Order> orders)
    {
        var batchBuilder = _streamFlow.Producer.CreateBatch();

        foreach (var order in orders)
        {
            batchBuilder
                .Message(order)
                .ToExchange("orders")
                .WithRoutingKey($"order.created.{order.CustomerId}")
                .WithDeliveryMode(DeliveryMode.Persistent)
                .WithHeaders(new Dictionary<string, object>
                {
                    ["customer-id"] = order.CustomerId,
                    ["order-type"] = order.Type
                })
                .AddToBatch();
        }

        var results = await batchBuilder.PublishAsync();
        
        foreach (var result in results)
        {
            if (result.IsSuccess)
            {
                Console.WriteLine($"Message {result.MessageId} published successfully");
            }
            else
            {
                Console.WriteLine($"Failed to publish message {result.MessageId}: {result.Error}");
            }
        }
    }
}
```

## 🚨 Error Handling

### Retry Mechanisms

```csharp
public class RetryPublisher
{
    private readonly IStreamFlowClient _streamFlow;
    private readonly ILogger<RetryPublisher> _logger;

    public RetryPublisher(IStreamFlowClient rabbitMQ, ILogger<RetryPublisher> logger)
    {
        _streamFlow = rabbitMQ;
        _logger = logger;
    }

    public async Task PublishWithRetryAsync(Order order)
    {
        var maxRetries = 3;
        var retryDelay = TimeSpan.FromSeconds(1);

        for (int attempt = 1; attempt <= maxRetries; attempt++)
        {
            try
            {
                await _streamFlow.Producer.PublishAsync(
                    exchange: "orders",
                    routingKey: "order.created",
                    message: order);

                _logger.LogInformation("Order {OrderId} published successfully on attempt {Attempt}", 
                    order.Id, attempt);
                return;
            }
            catch (Exception ex) when (attempt < maxRetries)
            {
                _logger.LogWarning(ex, "Failed to publish order {OrderId} on attempt {Attempt}. Retrying...", 
                    order.Id, attempt);
                
                await Task.Delay(retryDelay);
                retryDelay = TimeSpan.FromMilliseconds(retryDelay.TotalMilliseconds * 2); // Exponential backoff
            }
        }

        _logger.LogError("Failed to publish order {OrderId} after {MaxRetries} attempts", 
            order.Id, maxRetries);
        throw new InvalidOperationException($"Failed to publish order {order.Id} after {maxRetries} attempts");
    }
}
```

### Circuit Breaker Pattern

```csharp
public class CircuitBreakerPublisher
{
    private readonly IStreamFlowClient _streamFlow;
    private readonly ILogger<CircuitBreakerPublisher> _logger;
    private readonly CircuitBreaker _circuitBreaker;

    public CircuitBreakerPublisher(IStreamFlowClient rabbitMQ, ILogger<CircuitBreakerPublisher> logger)
    {
        _streamFlow = rabbitMQ;
        _logger = logger;
        _circuitBreaker = new CircuitBreaker(
            failureThreshold: 5,
            recoveryTimeout: TimeSpan.FromMinutes(1));
    }

    public async Task PublishWithCircuitBreakerAsync(Order order)
    {
        try
        {
            await _circuitBreaker.ExecuteAsync(async () =>
            {
                await _streamFlow.Producer.PublishAsync(
                    exchange: "orders",
                    routingKey: "order.created",
                    message: order);
            });

            _logger.LogInformation("Order {OrderId} published successfully", order.Id);
        }
        catch (CircuitBreakerOpenException)
        {
            _logger.LogWarning("Circuit breaker is open. Cannot publish order {OrderId}", order.Id);
            
            // Handle circuit breaker open state
            await HandleCircuitBreakerOpen(order);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to publish order {OrderId}", order.Id);
            throw;
        }
    }

    private async Task HandleCircuitBreakerOpen(Order order)
    {
        // Store order for later processing
        await StoreOrderForLaterProcessing(order);
        
        // Or use alternative publishing method
        await PublishToAlternativeQueue(order);
    }

    private async Task StoreOrderForLaterProcessing(Order order)
    {
        // Implementation for storing order in database or file system
        _logger.LogInformation("Order {OrderId} stored for later processing", order.Id);
    }

    private async Task PublishToAlternativeQueue(Order order)
    {
        // Implementation for alternative publishing method
        _logger.LogInformation("Order {OrderId} published to alternative queue", order.Id);
    }
}
```

### Dead Letter Handling

```csharp
public class DeadLetterPublisher
{
    private readonly IStreamFlowClient _streamFlow;
    private readonly ILogger<DeadLetterPublisher> _logger;

    public DeadLetterPublisher(IStreamFlowClient rabbitMQ, ILogger<DeadLetterPublisher> logger)
    {
        _streamFlow = rabbitMQ;
        _logger = logger;
    }

    public async Task PublishWithDeadLetterHandlingAsync(Order order)
    {
        var properties = new BasicProperties
        {
            MessageId = order.Id.ToString(),
            DeliveryMode = DeliveryMode.Persistent,
            Headers = new Dictionary<string, object>
            {
                ["x-death-count"] = 0,
                ["x-first-death-exchange"] = "orders",
                ["x-first-death-queue"] = "order-processing",
                ["x-first-death-reason"] = "original"
            }
        };

        try
        {
            await _streamFlow.Producer.PublishAsync(
                exchange: "orders",
                routingKey: "order.created",
                message: order,
                properties: properties);

            _logger.LogInformation("Order {OrderId} published successfully", order.Id);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to publish order {OrderId}. Sending to dead letter queue", order.Id);
            
            // Send to dead letter queue
            await SendToDeadLetterQueue(order, ex);
        }
    }

    private async Task SendToDeadLetterQueue(Order order, Exception exception)
    {
        var deadLetterProperties = new BasicProperties
        {
            MessageId = order.Id.ToString(),
            DeliveryMode = DeliveryMode.Persistent,
            Headers = new Dictionary<string, object>
            {
                ["x-death-reason"] = "failed",
                ["x-death-timestamp"] = DateTimeOffset.UtcNow.ToUnixTimeSeconds(),
                ["x-death-exception"] = exception.Message,
                ["x-death-stack-trace"] = exception.StackTrace
            }
        };

        await _streamFlow.Producer.PublishAsync(
            exchange: "dlx",
            routingKey: "order.failed",
            message: order,
            properties: deadLetterProperties);
    }
}
```

## ⚡ Performance Optimization

### Connection Pooling

```csharp
public class HighPerformancePublisher
{
    private readonly IStreamFlowClient _streamFlow;
    private readonly ILogger<HighPerformancePublisher> _logger;
    private readonly SemaphoreSlim _semaphore;

    public HighPerformancePublisher(IStreamFlowClient rabbitMQ, ILogger<HighPerformancePublisher> logger)
    {
        _streamFlow = rabbitMQ;
        _logger = logger;
        _semaphore = new SemaphoreSlim(100, 100); // Limit concurrent operations
    }

    public async Task PublishHighVolumeAsync(IEnumerable<Order> orders)
    {
        var publishTasks = orders.Select(order => PublishWithSemaphoreAsync(order));
        
        await Task.WhenAll(publishTasks);
        
        _logger.LogInformation("Published {Count} orders", orders.Count());
    }

    private async Task PublishWithSemaphoreAsync(Order order)
    {
        await _semaphore.WaitAsync();
        try
        {
            await _streamFlow.Producer.PublishAsync(
                exchange: "orders",
                routingKey: "order.created",
                message: order);
        }
        finally
        {
            _semaphore.Release();
        }
    }
}
```

### Asynchronous Publishing

```csharp
public class AsyncPublisher
{
    private readonly IStreamFlowClient _streamFlow;
    private readonly ILogger<AsyncPublisher> _logger;
    private readonly Channel<Order> _channel;

    public AsyncPublisher(IStreamFlowClient rabbitMQ, ILogger<AsyncPublisher> logger)
    {
        _streamFlow = rabbitMQ;
        _logger = logger;
        _channel = Channel.CreateUnbounded<Order>();
        
        // Start background processing
        _ = Task.Run(ProcessOrdersAsync);
    }

    public async Task QueueOrderAsync(Order order)
    {
        await _channel.Writer.WriteAsync(order);
    }

    private async Task ProcessOrdersAsync()
    {
        await foreach (var order in _channel.Reader.ReadAllAsync())
        {
            try
            {
                await _streamFlow.Producer.PublishAsync(
                    exchange: "orders",
                    routingKey: "order.created",
                    message: order);

                _logger.LogInformation("Order {OrderId} published successfully", order.Id);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to publish order {OrderId}", order.Id);
                
                // Handle error (retry, dead letter, etc.)
            }
        }
    }
}
```

## 📊 Monitoring and Metrics

### Producer Statistics

```csharp
public class MonitoredPublisher
{
    private readonly IStreamFlowClient _streamFlow;
    private readonly ILogger<MonitoredPublisher> _logger;

    public MonitoredPublisher(IStreamFlowClient rabbitMQ, ILogger<MonitoredPublisher> logger)
    {
        _streamFlow = rabbitMQ;
        _logger = logger;
    }

    public async Task PublishWithMonitoringAsync(Order order)
    {
        var stopwatch = Stopwatch.StartNew();
        
        try
        {
            await _streamFlow.Producer.PublishAsync(
                exchange: "orders",
                routingKey: "order.created",
                message: order);

            stopwatch.Stop();
            
            // Log performance metrics
            _logger.LogInformation("Order {OrderId} published in {ElapsedMs}ms", 
                order.Id, stopwatch.ElapsedMilliseconds);
            
            // Update metrics
            UpdateMetrics(success: true, stopwatch.ElapsedMilliseconds);
        }
        catch (Exception ex)
        {
            stopwatch.Stop();
            
            _logger.LogError(ex, "Failed to publish order {OrderId} after {ElapsedMs}ms", 
                order.Id, stopwatch.ElapsedMilliseconds);
            
            // Update metrics
            UpdateMetrics(success: false, stopwatch.ElapsedMilliseconds);
            
            throw;
        }
    }

    private void UpdateMetrics(bool success, long elapsedMs)
    {
        // Update your metrics system (Application Insights, Prometheus, etc.)
        if (success)
        {
            // Increment success counter
            // Record response time
        }
        else
        {
            // Increment failure counter
            // Record error rate
        }
    }

    public void LogStatistics()
    {
        var stats = _streamFlow.Producer.Statistics;
        
        _logger.LogInformation("Producer Statistics: " +
            "Status={Status}, " +
            "TotalPublished={TotalPublished}, " +
            "TotalConfirmed={TotalConfirmed}, " +
            "TotalFailed={TotalFailed}, " +
            "StartTime={StartTime}, " +
            "LastUpdateTime={LastUpdateTime}",
            stats.Status,
            stats.TotalPublished,
            stats.TotalConfirmed,
            stats.TotalFailed,
            stats.StartTime,
            stats.LastUpdateTime);
    }
}
```

## 🎯 Best Practices

### 1. Use Publisher Confirmations for Critical Messages

```csharp
// DO: Use confirmations for critical messages
await _streamFlow.Producer.PublishAsync(
    exchange: "orders",
    routingKey: "order.created",
    message: order,
    waitForConfirmation: true);

// DON'T: Ignore confirmations for critical messages
await _streamFlow.Producer.PublishAsync(
    exchange: "orders",
    routingKey: "order.created",
    message: order);
```

### 2. Use Persistent Messages for Durability

```csharp
// DO: Use persistent delivery mode for durable messages
var properties = new BasicProperties
{
    DeliveryMode = DeliveryMode.Persistent
};

await _streamFlow.Producer.PublishAsync(
    exchange: "orders",
    routingKey: "order.created",
    message: order,
    properties: properties);
```

### 3. Implement Proper Error Handling

```csharp
// DO: Implement comprehensive error handling
public async Task PublishOrderAsync(Order order)
{
    try
    {
        await _streamFlow.Producer.PublishAsync(
            exchange: "orders",
            routingKey: "order.created",
            message: order);
    }
    catch (ConnectionException ex)
    {
        // Handle connection errors
        _logger.LogError(ex, "Connection error while publishing order {OrderId}", order.Id);
        throw;
    }
    catch (Exception ex)
    {
        // Handle other errors
        _logger.LogError(ex, "Unexpected error while publishing order {OrderId}", order.Id);
        throw;
    }
}
```

### 4. Use Batch Operations for High Volume

```csharp
// DO: Use batch operations for high volume
var messageContexts = orders.Select(order => new MessageContext
{
    Exchange = "orders",
    RoutingKey = "order.created",
    Message = order
});

await _streamFlow.Producer.PublishBatchAsync(messageContexts);

// DON'T: Publish individual messages in a loop
foreach (var order in orders)
{
    await _streamFlow.Producer.PublishAsync(
        exchange: "orders",
        routingKey: "order.created",
        message: order);
}
```

### 5. Set Message Expiration for Time-Sensitive Messages

```csharp
// DO: Set expiration for time-sensitive messages
var properties = new BasicProperties
{
    Expiration = TimeSpan.FromMinutes(30).TotalMilliseconds.ToString()
};

await _streamFlow.Producer.PublishAsync(
    exchange: "notifications",
    routingKey: "email.send",
    message: emailMessage,
    properties: properties);
```

### 6. Use Message IDs for Deduplication

```csharp
// DO: Use message IDs for deduplication
var properties = new BasicProperties
{
    MessageId = order.Id.ToString()
};

await _streamFlow.Producer.PublishAsync(
    exchange: "orders",
    routingKey: "order.created",
    message: order,
    properties: properties);
```

### 7. Monitor Producer Performance

```csharp
// DO: Monitor producer performance
public class ProducerHealthCheck : IHealthCheck
{
    private readonly IStreamFlowClient _streamFlow;

    public ProducerHealthCheck(IStreamFlowClient rabbitMQ) => _streamFlow = rabbitMQ;

    public Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
    {
        var isHealthy = _streamFlow.Producer.IsReady;
        var stats = _streamFlow.Producer.Statistics;
        
        var data = new Dictionary<string, object>
        {
            ["is_ready"] = isHealthy,
            ["total_published"] = stats.TotalPublished,
            ["total_confirmed"] = stats.TotalConfirmed,
            ["total_failed"] = stats.TotalFailed
        };

        return Task.FromResult(isHealthy 
            ? HealthCheckResult.Healthy("Producer is healthy", data)
            : HealthCheckResult.Unhealthy("Producer is not ready", data: data));
    }
}
```

## 🎉 Summary

You've now learned how to use the FS.StreamFlow Producer effectively:

✅ **Basic and advanced publishing patterns**  
✅ **Publisher confirmations for reliability**  
✅ **Batch operations for performance**  
✅ **Transactional publishing for consistency**  
✅ **Fluent API for clean code**  
✅ **Comprehensive error handling**  
✅ **Performance optimization techniques**  
✅ **Monitoring and metrics**  
✅ **Production-ready best practices**  

## 🔧 Troubleshooting

### Common Producer Issues

**Problem**: `PublishAsync()` throws "Exchange must be configured before publishing"  
**Solution**: Always declare exchanges before publishing:
```csharp
await _streamFlow.ExchangeManager.Exchange("orders")
    .AsTopic()
    .WithDurable(true)
    .DeclareAsync();
```

**Problem**: `Message<T>()` with `.PublishAsync()` throws compilation error  
**Solution**: Use the correct overload:
```csharp
// ❌ Wrong - Message<T>() requires message parameter in PublishAsync
await _streamFlow.Producer.Message<Order>()
    .WithExchange("orders")
    .WithRoutingKey("order.created")
    .PublishAsync(); // COMPILATION ERROR!

// ✅ Correct - Message<T>() MUST pass message to PublishAsync
await _streamFlow.Producer.Message<Order>()
    .WithExchange("orders")
    .WithRoutingKey("order.created")
    .PublishAsync(order); // Correct!

// ✅ Alternative - Message(order) can use PublishAsync() without parameters
await _streamFlow.Producer.Message(order)
    .WithExchange("orders")
    .WithRoutingKey("order.created")
    .PublishAsync(); // Correct!
```

**Problem**: Messages not being confirmed  
**Solution**: Enable publisher confirmations:
```csharp
builder.Services.AddRabbitMQStreamFlow(options =>
{
    options.ProducerSettings.EnablePublisherConfirms = true;
    options.ProducerSettings.ConfirmationTimeout = TimeSpan.FromSeconds(10);
});
```

**Problem**: High latency in publishing  
**Solution**: Use batch operations for multiple messages:
```csharp
var messages = orders.Select(order => new MessageContext
{
    Exchange = "orders",
    RoutingKey = "order.created",
    Message = order
});

await _streamFlow.Producer.PublishBatchAsync(messages);
```

## 🎯 Next Steps

Continue learning about FS.StreamFlow:

- [Consumer Guide](consumer.md) - Master message consumption
- [Event-Driven Architecture](event-driven.md) - Build event-driven systems
- [Error Handling](error-handling.md) - Implement robust error handling
- [Performance Tuning](performance.md) - Optimize for high throughput
- [Examples](examples/) - See real-world examples 