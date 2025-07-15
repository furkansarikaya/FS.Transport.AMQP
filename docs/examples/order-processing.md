# Order Processing Example

**Difficulty**: 🟡 Intermediate  
**Focus**: Real-world e-commerce scenario with events  
**Time**: 30 minutes  

This example demonstrates a complete order processing workflow using FS.StreamFlow, including event-driven architecture, error handling, and monitoring.

## 📋 What You'll Learn

- Event-driven architecture patterns
- Complex message routing
- Error handling and retry strategies
- Dead letter queue management
- Real-time order status updates
- Integration between multiple services

## 🏗️ System Architecture

```
┌─────────────────┐    ┌─────────────────┐    ┌─────────────────┐
│   Order API     │    │ Inventory Service│    │ Payment Service │
│                 │    │                 │    │                 │
│ Creates Orders  │    │ Reserves Items  │    │ Processes Payments│
└─────────────────┘    └─────────────────┘    └─────────────────┘
         │                        │                        │
         │                        │                        │
         └──────────────────────────────────────────────────┘
                                  │
                         ┌─────────────────┐
                         │   RabbitMQ      │
                         │                 │
                         │ • order.created │
                         │ • inventory.*   │
                         │ • payment.*     │
                         │ • notification.*│
                         └─────────────────┘
                                  │
         ┌──────────────────────────────────────────────────┐
         │                        │                        │
┌─────────────────┐    ┌─────────────────┐    ┌─────────────────┐
│ Notification    │    │ Audit Service   │    │ Shipping Service│
│ Service         │    │                 │    │                 │
│                 │    │ Logs Events     │    │ Arranges Delivery│
│ Sends Emails    │    │                 │    │                 │
└─────────────────┘    └─────────────────┘    └─────────────────┘
```

## 🛠️ Setup

### Project Structure
```
OrderProcessing/
├── Program.cs
├── Models/
│   ├── Order.cs
│   ├── OrderItem.cs
│   ├── Events/
│   │   ├── OrderCreated.cs
│   │   ├── InventoryReserved.cs
│   │   ├── PaymentProcessed.cs
│   │   └── OrderCompleted.cs
│   └── Requests/
│       ├── ReserveInventoryRequest.cs
│       └── ProcessPaymentRequest.cs
├── Services/
│   ├── OrderService.cs
│   ├── InventoryService.cs
│   ├── PaymentService.cs
│   ├── NotificationService.cs
│   └── AuditService.cs
└── OrderProcessing.csproj
```

## 🏗️ Implementation

### 1. Models

```csharp
// Models/Order.cs
namespace OrderProcessing.Models;

public class Order
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string CustomerName { get; set; } = string.Empty;
    public string CustomerEmail { get; set; } = string.Empty;
    public List<OrderItem> Items { get; set; } = new();
    public decimal Total { get; set; }
    public OrderStatus Status { get; set; } = OrderStatus.Created;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? CompletedAt { get; set; }
    public string? ShippingAddress { get; set; }
}

public class OrderItem
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string ProductName { get; set; } = string.Empty;
    public string ProductSku { get; set; } = string.Empty;
    public int Quantity { get; set; }
    public decimal Price { get; set; }
    public decimal Total => Quantity * Price;
}

public enum OrderStatus
{
    Created,
    InventoryReserved,
    PaymentProcessed,
    Shipped,
    Completed,
    Cancelled,
    Failed
}
```

### 2. Events

```csharp
// Models/Events/OrderCreated.cs
using FS.StreamFlow.Core.Features.Events.Interfaces;

namespace OrderProcessing.Models.Events;

public class OrderCreated : IntegrationEventBase
{
    public Guid OrderId { get; set; }
    public string CustomerName { get; set; } = string.Empty;
    public string CustomerEmail { get; set; } = string.Empty;
    public List<OrderItem> Items { get; set; } = new();
    public decimal Total { get; set; }
    public DateTime CreatedAt { get; set; }
}

// Models/Events/InventoryReserved.cs
public class InventoryReserved : IntegrationEventBase
{
    public Guid OrderId { get; set; }
    public List<ReservedItem> ReservedItems { get; set; } = new();
    public DateTime ReservedAt { get; set; }
}

public class ReservedItem
{
    public string ProductSku { get; set; } = string.Empty;
    public int Quantity { get; set; }
    public Guid ReservationId { get; set; }
}

// Models/Events/PaymentProcessed.cs
public class PaymentProcessed : IntegrationEventBase
{
    public Guid OrderId { get; set; }
    public decimal Amount { get; set; }
    public string PaymentMethod { get; set; } = string.Empty;
    public string TransactionId { get; set; } = string.Empty;
    public DateTime ProcessedAt { get; set; }
}

// Models/Events/OrderCompleted.cs
public class OrderCompleted : IntegrationEventBase
{
    public Guid OrderId { get; set; }
    public string CustomerName { get; set; } = string.Empty;
    public string CustomerEmail { get; set; } = string.Empty;
    public decimal Total { get; set; }
    public DateTime CompletedAt { get; set; }
    public string TrackingNumber { get; set; } = string.Empty;
}
```

### 3. Services

```csharp
// Services/OrderService.cs
using FS.StreamFlow.Core.Features.Messaging.Interfaces;
using OrderProcessing.Models;
using OrderProcessing.Models.Events;

namespace OrderProcessing.Services;

public class OrderService
{
    private readonly IRabbitMQClient _rabbitMQ;
    private readonly ILogger<OrderService> _logger;
    private readonly Dictionary<Guid, Order> _orders = new(); // In-memory storage for demo

    public OrderService(IRabbitMQClient rabbitMQ, ILogger<OrderService> logger)
    {
        _rabbitMQ = rabbitMQ;
        _logger = logger;
    }

    public async Task<Order> CreateOrderAsync(CreateOrderRequest request)
    {
        var order = new Order
        {
            CustomerName = request.CustomerName,
            CustomerEmail = request.CustomerEmail,
            Items = request.Items,
            Total = request.Items.Sum(i => i.Total),
            ShippingAddress = request.ShippingAddress
        };

        // Store order
        _orders[order.Id] = order;

        _logger.LogInformation("Order created: {OrderId} for {CustomerName} - Total: {Total:C}", 
            order.Id, order.CustomerName, order.Total);

        // Publish order created event
        await _rabbitMQ.EventBus.PublishIntegrationEventAsync(new OrderCreated
        {
            OrderId = order.Id,
            CustomerName = order.CustomerName,
            CustomerEmail = order.CustomerEmail,
            Items = order.Items,
            Total = order.Total,
            CreatedAt = order.CreatedAt
        });

        return order;
    }

    public async Task UpdateOrderStatusAsync(Guid orderId, OrderStatus status)
    {
        if (_orders.TryGetValue(orderId, out var order))
        {
            order.Status = status;
            
            if (status == OrderStatus.Completed)
            {
                order.CompletedAt = DateTime.UtcNow;
            }

            _logger.LogInformation("Order {OrderId} status updated to {Status}", orderId, status);
        }
    }

    public Order? GetOrder(Guid orderId)
    {
        return _orders.TryGetValue(orderId, out var order) ? order : null;
    }

    public List<Order> GetAllOrders()
    {
        return _orders.Values.ToList();
    }
}

public class CreateOrderRequest
{
    public string CustomerName { get; set; } = string.Empty;
    public string CustomerEmail { get; set; } = string.Empty;
    public List<OrderItem> Items { get; set; } = new();
    public string ShippingAddress { get; set; } = string.Empty;
}
```

```csharp
// Services/InventoryService.cs
using FS.StreamFlow.Core.Features.Messaging.Interfaces;
using FS.StreamFlow.Core.Features.Events.Interfaces;
using OrderProcessing.Models;
using OrderProcessing.Models.Events;

namespace OrderProcessing.Services;

public class InventoryService : IEventHandler<OrderCreated>
{
    private readonly IRabbitMQClient _rabbitMQ;
    private readonly ILogger<InventoryService> _logger;
    private readonly Dictionary<string, int> _inventory = new(); // In-memory inventory

    public InventoryService(IRabbitMQClient rabbitMQ, ILogger<InventoryService> logger)
    {
        _rabbitMQ = rabbitMQ;
        _logger = logger;
        
        // Initialize some inventory
        InitializeInventory();
    }

    private void InitializeInventory()
    {
        _inventory["LAPTOP001"] = 10;
        _inventory["MOUSE001"] = 50;
        _inventory["KEYBOARD001"] = 25;
        _inventory["MONITOR001"] = 15;
        _inventory["PHONE001"] = 30;
    }

    public async Task HandleAsync(OrderCreated eventData, EventContext context)
    {
        _logger.LogInformation("Processing inventory reservation for order {OrderId}", eventData.OrderId);

        try
        {
            var reservedItems = new List<ReservedItem>();
            var reservationSuccessful = true;

            // Check and reserve inventory
            foreach (var item in eventData.Items)
            {
                if (_inventory.TryGetValue(item.ProductSku, out var availableQuantity))
                {
                    if (availableQuantity >= item.Quantity)
                    {
                        _inventory[item.ProductSku] -= item.Quantity;
                        
                        reservedItems.Add(new ReservedItem
                        {
                            ProductSku = item.ProductSku,
                            Quantity = item.Quantity,
                            ReservationId = Guid.NewGuid()
                        });

                        _logger.LogInformation("Reserved {Quantity} units of {ProductSku} for order {OrderId}", 
                            item.Quantity, item.ProductSku, eventData.OrderId);
                    }
                    else
                    {
                        _logger.LogWarning("Insufficient inventory for {ProductSku}. Available: {Available}, Requested: {Requested}", 
                            item.ProductSku, availableQuantity, item.Quantity);
                        reservationSuccessful = false;
                        break;
                    }
                }
                else
                {
                    _logger.LogWarning("Product {ProductSku} not found in inventory", item.ProductSku);
                    reservationSuccessful = false;
                    break;
                }
            }

            if (reservationSuccessful)
            {
                // Publish inventory reserved event
                await _rabbitMQ.EventBus.PublishIntegrationEventAsync(new InventoryReserved
                {
                    OrderId = eventData.OrderId,
                    ReservedItems = reservedItems,
                    ReservedAt = DateTime.UtcNow
                });

                _logger.LogInformation("Inventory successfully reserved for order {OrderId}", eventData.OrderId);
            }
            else
            {
                // Rollback reservations
                foreach (var item in reservedItems)
                {
                    _inventory[item.ProductSku] += item.Quantity;
                }

                // Publish inventory reservation failed event
                await _rabbitMQ.EventBus.PublishIntegrationEventAsync(new InventoryReservationFailed
                {
                    OrderId = eventData.OrderId,
                    Reason = "Insufficient inventory",
                    FailedAt = DateTime.UtcNow
                });

                _logger.LogError("Inventory reservation failed for order {OrderId}", eventData.OrderId);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing inventory reservation for order {OrderId}", eventData.OrderId);
            throw;
        }
    }
}

public class InventoryReservationFailed : IntegrationEventBase
{
    public Guid OrderId { get; set; }
    public string Reason { get; set; } = string.Empty;
    public DateTime FailedAt { get; set; }
}
```

```csharp
// Services/PaymentService.cs
using FS.StreamFlow.Core.Features.Messaging.Interfaces;
using FS.StreamFlow.Core.Features.Events.Interfaces;
using OrderProcessing.Models.Events;

namespace OrderProcessing.Services;

public class PaymentService : IEventHandler<InventoryReserved>
{
    private readonly IRabbitMQClient _rabbitMQ;
    private readonly ILogger<PaymentService> _logger;

    public PaymentService(IRabbitMQClient rabbitMQ, ILogger<PaymentService> logger)
    {
        _rabbitMQ = rabbitMQ;
        _logger = logger;
    }

    public async Task HandleAsync(InventoryReserved eventData, EventContext context)
    {
        _logger.LogInformation("Processing payment for order {OrderId}", eventData.OrderId);

        try
        {
            // Simulate payment processing
            await Task.Delay(2000); // Simulate external payment API call

            // Simulate random payment failures (10% chance)
            if (Random.Shared.Next(1, 11) == 1)
            {
                throw new PaymentException("Payment declined by bank");
            }

            var transactionId = $"TXN-{DateTime.UtcNow:yyyyMMdd}-{Random.Shared.Next(100000, 999999)}";

            // Publish payment processed event
            await _rabbitMQ.EventBus.PublishIntegrationEventAsync(new PaymentProcessed
            {
                OrderId = eventData.OrderId,
                Amount = CalculateOrderTotal(eventData), // Simplified calculation
                PaymentMethod = "Credit Card",
                TransactionId = transactionId,
                ProcessedAt = DateTime.UtcNow
            });

            _logger.LogInformation("Payment processed successfully for order {OrderId} - Transaction: {TransactionId}", 
                eventData.OrderId, transactionId);
        }
        catch (PaymentException ex)
        {
            _logger.LogError(ex, "Payment failed for order {OrderId}", eventData.OrderId);
            
            // Publish payment failed event
            await _rabbitMQ.EventBus.PublishIntegrationEventAsync(new PaymentFailed
            {
                OrderId = eventData.OrderId,
                Reason = ex.Message,
                FailedAt = DateTime.UtcNow
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing payment for order {OrderId}", eventData.OrderId);
            throw;
        }
    }

    private decimal CalculateOrderTotal(InventoryReserved eventData)
    {
        // Simplified calculation - in real world, you'd get this from order data
        return eventData.ReservedItems.Count * 99.99m;
    }
}

public class PaymentException : Exception
{
    public PaymentException(string message) : base(message) { }
}

public class PaymentFailed : IntegrationEventBase
{
    public Guid OrderId { get; set; }
    public string Reason { get; set; } = string.Empty;
    public DateTime FailedAt { get; set; }
}
```

```csharp
// Services/NotificationService.cs
using FS.StreamFlow.Core.Features.Events.Interfaces;
using OrderProcessing.Models.Events;

namespace OrderProcessing.Services;

public class NotificationService : 
    IEventHandler<OrderCreated>,
    IEventHandler<PaymentProcessed>,
    IEventHandler<OrderCompleted>
{
    private readonly ILogger<NotificationService> _logger;

    public NotificationService(ILogger<NotificationService> logger)
    {
        _logger = logger;
    }

    public async Task HandleAsync(OrderCreated eventData, EventContext context)
    {
        _logger.LogInformation("Sending order confirmation email to {CustomerEmail} for order {OrderId}", 
            eventData.CustomerEmail, eventData.OrderId);

        // Simulate sending email
        await Task.Delay(500);

        _logger.LogInformation("Order confirmation email sent to {CustomerEmail}", eventData.CustomerEmail);
    }

    public async Task HandleAsync(PaymentProcessed eventData, EventContext context)
    {
        _logger.LogInformation("Sending payment confirmation for order {OrderId}", eventData.OrderId);

        // Simulate sending payment confirmation
        await Task.Delay(300);

        _logger.LogInformation("Payment confirmation sent for order {OrderId}", eventData.OrderId);
    }

    public async Task HandleAsync(OrderCompleted eventData, EventContext context)
    {
        _logger.LogInformation("Sending order completion notification to {CustomerEmail} for order {OrderId}", 
            eventData.CustomerEmail, eventData.OrderId);

        // Simulate sending completion notification with tracking
        await Task.Delay(400);

        _logger.LogInformation("Order completion notification sent to {CustomerEmail} - Tracking: {TrackingNumber}", 
            eventData.CustomerEmail, eventData.TrackingNumber);
    }
}
```

### 4. Main Program

```csharp
// Program.cs
using FS.StreamFlow.RabbitMQ.DependencyInjection;
using OrderProcessing.Services;
using OrderProcessing.Models;
using OrderProcessing.Models.Events;

var builder = Host.CreateApplicationBuilder(args);

// Add logging
builder.Services.AddLogging(config =>
{
    config.AddConsole();
    config.SetMinimumLevel(LogLevel.Information);
});

// Add FS.StreamFlow with event bus
builder.Services.AddRabbitMQ()
    .WithConnectionString("amqp://localhost")
    .WithEventBus(config =>
    {
        config.DomainEventExchange = "domain-events";
        config.IntegrationEventExchange = "integration-events";
        config.EnableEventHandlerDiscovery = true;
        config.EventHandlerAssemblies = new[] { typeof(Program).Assembly };
    })
    .WithErrorHandling(config =>
    {
        config.EnableDeadLetterQueue = true;
        config.DeadLetterExchange = "dlx";
        config.DeadLetterQueue = "dlq";
        config.MaxRetryAttempts = 3;
        config.RetryDelay = TimeSpan.FromSeconds(5);
    })
    .WithProducer(config =>
    {
        config.EnableConfirmations = true;
        config.ConfirmationTimeout = TimeSpan.FromSeconds(10);
    })
    .WithConsumer(config =>
    {
        config.PrefetchCount = 10;
        config.ConcurrentConsumers = 2;
        config.AutoAck = false;
    })
    .Build();

// Add services
builder.Services.AddSingleton<OrderService>();
builder.Services.AddSingleton<InventoryService>();
builder.Services.AddSingleton<PaymentService>();
builder.Services.AddSingleton<NotificationService>();

var host = builder.Build();

// Setup cancellation
var cancellationTokenSource = new CancellationTokenSource();
Console.CancelKeyPress += (_, e) =>
{
    e.Cancel = true;
    cancellationTokenSource.Cancel();
};

try
{
    // Setup infrastructure
    await SetupInfrastructureAsync(host.Services);

    // Start event handlers
    await StartEventHandlersAsync(host.Services, cancellationTokenSource.Token);

    // Create some sample orders
    await CreateSampleOrdersAsync(host.Services);

    Console.WriteLine("Order processing system started!");
    Console.WriteLine("Creating sample orders... Check logs for processing details.");
    Console.WriteLine("Press Ctrl+C to stop...");

    // Wait for cancellation
    await Task.Delay(Timeout.Infinite, cancellationTokenSource.Token);
}
catch (OperationCanceledException)
{
    Console.WriteLine("Shutting down gracefully...");
}
catch (Exception ex)
{
    Console.WriteLine($"An error occurred: {ex.Message}");
}
finally
{
    await host.StopAsync();
}

// Infrastructure setup
static async Task SetupInfrastructureAsync(IServiceProvider services)
{
    var rabbitMQ = services.GetRequiredService<IRabbitMQClient>();
    var logger = services.GetRequiredService<ILogger<Program>>();

    logger.LogInformation("Setting up order processing infrastructure...");

    // Setup exchanges
    await rabbitMQ.ExchangeManager.DeclareExchangeAsync("integration-events", "topic", true, false);
    await rabbitMQ.ExchangeManager.DeclareExchangeAsync("dlx", "topic", true, false);

    // Setup queues
    var queueArgs = new Dictionary<string, object>
    {
        ["x-dead-letter-exchange"] = "dlx",
        ["x-dead-letter-routing-key"] = "failed"
    };

    await rabbitMQ.QueueManager.DeclareQueueAsync("order-created", true, false, false, queueArgs);
    await rabbitMQ.QueueManager.DeclareQueueAsync("inventory-reserved", true, false, false, queueArgs);
    await rabbitMQ.QueueManager.DeclareQueueAsync("payment-processed", true, false, false, queueArgs);
    await rabbitMQ.QueueManager.DeclareQueueAsync("order-completed", true, false, false, queueArgs);
    await rabbitMQ.QueueManager.DeclareQueueAsync("dlq", true, false, false);

    // Setup bindings
    await rabbitMQ.QueueManager.BindQueueAsync("order-created", "integration-events", "OrderCreated");
    await rabbitMQ.QueueManager.BindQueueAsync("inventory-reserved", "integration-events", "InventoryReserved");
    await rabbitMQ.QueueManager.BindQueueAsync("payment-processed", "integration-events", "PaymentProcessed");
    await rabbitMQ.QueueManager.BindQueueAsync("order-completed", "integration-events", "OrderCompleted");
    await rabbitMQ.QueueManager.BindQueueAsync("dlq", "dlx", "#");

    logger.LogInformation("Infrastructure setup completed");
}

// Start event handlers
static async Task StartEventHandlersAsync(IServiceProvider services, CancellationToken cancellationToken)
{
    var rabbitMQ = services.GetRequiredService<IRabbitMQClient>();
    var inventoryService = services.GetRequiredService<InventoryService>();
    var paymentService = services.GetRequiredService<PaymentService>();
    var notificationService = services.GetRequiredService<NotificationService>();

    // Start inventory service handler
    _ = Task.Run(async () =>
    {
        await rabbitMQ.Consumer.ConsumeAsync<OrderCreated>(
            "order-created",
            async (evt, ctx) =>
            {
                await inventoryService.HandleAsync(evt, new EventContext());
                return true;
            },
            cancellationToken);
    });

    // Start payment service handler
    _ = Task.Run(async () =>
    {
        await rabbitMQ.Consumer.ConsumeAsync<InventoryReserved>(
            "inventory-reserved",
            async (evt, ctx) =>
            {
                await paymentService.HandleAsync(evt, new EventContext());
                return true;
            },
            cancellationToken);
    });

    // Start notification service handlers
    _ = Task.Run(async () =>
    {
        await rabbitMQ.Consumer.ConsumeAsync<PaymentProcessed>(
            "payment-processed",
            async (evt, ctx) =>
            {
                await notificationService.HandleAsync(evt, new EventContext());
                return true;
            },
            cancellationToken);
    });

    await Task.Delay(1000); // Allow handlers to start
}

// Create sample orders
static async Task CreateSampleOrdersAsync(IServiceProvider services)
{
    var orderService = services.GetRequiredService<OrderService>();

    var sampleOrders = new[]
    {
        new CreateOrderRequest
        {
            CustomerName = "John Doe",
            CustomerEmail = "john@example.com",
            ShippingAddress = "123 Main St, City, State 12345",
            Items = new List<OrderItem>
            {
                new() { ProductName = "Laptop", ProductSku = "LAPTOP001", Quantity = 1, Price = 999.99m },
                new() { ProductName = "Mouse", ProductSku = "MOUSE001", Quantity = 1, Price = 29.99m }
            }
        },
        new CreateOrderRequest
        {
            CustomerName = "Jane Smith",
            CustomerEmail = "jane@example.com",
            ShippingAddress = "456 Oak Ave, City, State 67890",
            Items = new List<OrderItem>
            {
                new() { ProductName = "Keyboard", ProductSku = "KEYBOARD001", Quantity = 2, Price = 79.99m },
                new() { ProductName = "Monitor", ProductSku = "MONITOR001", Quantity = 1, Price = 299.99m }
            }
        }
    };

    foreach (var orderRequest in sampleOrders)
    {
        await orderService.CreateOrderAsync(orderRequest);
        await Task.Delay(2000); // Delay between orders
    }
}
```

## 🚀 Running the Example

1. **Start RabbitMQ**:
   ```bash
   docker run -d --name rabbitmq -p 5672:5672 -p 15672:15672 rabbitmq:3-management
   ```

2. **Run the application**:
   ```bash
   dotnet run
   ```

3. **Expected flow**:
   ```
   Order Created → Inventory Reserved → Payment Processed → Notifications Sent
   ```

## 📊 Monitoring

View the RabbitMQ Management UI at http://localhost:15672 to see:
- Message flow between exchanges and queues
- Consumer connections and activity
- Dead letter queue statistics
- Exchange and queue bindings

## 🔧 Key Features Demonstrated

- ✅ **Event-Driven Architecture**: Loose coupling between services
- ✅ **Integration Events**: Cross-service communication
- ✅ **Error Handling**: Retry logic and dead letter queues
- ✅ **Async Processing**: Non-blocking message handling
- ✅ **Real-world Workflow**: Complete order processing pipeline

## 📈 Next Steps

1. Add **saga orchestration** for complex workflows
2. Implement **event sourcing** for audit trails
3. Add **API endpoints** for order management
4. Implement **real databases** instead of in-memory storage
5. Add **unit tests** and **integration tests**

## 🎯 Key Takeaways

This example demonstrates how to build a robust, event-driven order processing system using FS.StreamFlow. The pattern can be extended to handle any complex business workflow! 🚀 