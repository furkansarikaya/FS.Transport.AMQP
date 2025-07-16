# Inventory Management Example

**Difficulty**: 🟡 Intermediate  
**Focus**: Real-time inventory updates and reservations  
**Time**: 30 minutes

This example demonstrates how to implement real-time inventory management using FS.StreamFlow. It covers inventory reservation, update flows, error handling, and monitoring.

## 📋 What You'll Learn
- Inventory reservation and update patterns
- Event-driven inventory workflows
- Error handling for inventory operations
- Monitoring inventory events

## 🛠️ Setup

### Prerequisites
- .NET 9 SDK
- RabbitMQ server running on localhost
- Basic C# knowledge

### Project Structure
```
InventoryManagement/
├── Program.cs
├── Models/
│   ├── InventoryReserved.cs
│   └── InventoryUpdated.cs
├── Services/
│   ├── InventoryService.cs
│   └── InventoryReservedHandler.cs
└── InventoryManagement.csproj
```

## 🏗️ Implementation

### 1. Inventory Event Models

```csharp
// Models/InventoryReserved.cs
using FS.StreamFlow.Core.Features.Events.Interfaces;

public record InventoryReserved(Guid OrderId, List<string> Items) : IIntegrationEvent
{
    public Guid Id { get; } = Guid.NewGuid();
    public DateTime OccurredOn { get; } = DateTime.UtcNow;
    public int Version { get; } = 1;
    public string EventType => nameof(InventoryReserved);
    public string? CorrelationId { get; set; }
    public string? CausationId { get; set; }
    public IDictionary<string, object> Metadata { get; } = new Dictionary<string, object>();
    public string Source => "inventory-service";
    public string RoutingKey => "inventory.reserved";
    public string? Target { get; set; }
    public string SchemaVersion => "1.0";
    public TimeSpan? TimeToLive { get; set; }
}

// Models/InventoryUpdated.cs
using FS.StreamFlow.Core.Features.Events.Interfaces;

public record InventoryUpdated(Guid ProductId, int NewStock) : IIntegrationEvent
{
    public Guid Id { get; } = Guid.NewGuid();
    public DateTime OccurredOn { get; } = DateTime.UtcNow;
    public int Version { get; } = 1;
    public string EventType => nameof(InventoryUpdated);
    public string? CorrelationId { get; set; }
    public string? CausationId { get; set; }
    public IDictionary<string, object> Metadata { get; } = new Dictionary<string, object>();
    public string Source => "inventory-service";
    public string RoutingKey => "inventory.updated";
    public string? Target { get; set; }
    public string SchemaVersion => "1.0";
    public TimeSpan? TimeToLive { get; set; }
}
```

### 2. Inventory Service (Publisher)

```csharp
// Services/InventoryService.cs
using FS.StreamFlow.Core.Features.Messaging.Interfaces;
using Microsoft.Extensions.Logging;

public class InventoryService
{
    private readonly IStreamFlowClient _streamFlow;
    private readonly ILogger<InventoryService> _logger;

    public InventoryService(IStreamFlowClient streamFlow, ILogger<InventoryService> logger)
    {
        _streamFlow = streamFlow;
        _logger = logger;
    }

    public async Task ReserveInventoryAsync(Guid orderId, List<string> items)
    {
        // Initialize the client first
        await _streamFlow.InitializeAsync();
        
        // Reserve inventory logic (omitted)
        // await _inventoryRepository.ReserveItemsAsync(orderId, items);
        
        // Publish integration event with fluent API
        await _streamFlow.EventBus.Event<InventoryReserved>()
            .WithMetadata(metadata =>
            {
                metadata.CorrelationId = Guid.NewGuid().ToString();
                metadata.Source = "inventory-service";
                metadata.Version = "1.0";
            })
            .WithProperty("order-id", orderId.ToString())
            .WithProperty("item-count", items.Count)
            .PublishAsync(new InventoryReserved(orderId, items));
            
        _logger.LogInformation("InventoryReserved event published for Order {OrderId}", orderId);
    }

    public async Task UpdateInventoryAsync(Guid productId, int newStock)
    {
        // Initialize the client first
        await _streamFlow.InitializeAsync();
        
        // Update inventory logic (omitted)
        // await _inventoryRepository.UpdateStockAsync(productId, newStock);
        
        // Publish integration event with fluent API
        await _streamFlow.EventBus.Event<InventoryUpdated>()
            .WithMetadata(metadata =>
            {
                metadata.CorrelationId = Guid.NewGuid().ToString();
                metadata.Source = "inventory-service";
                metadata.Version = "1.0";
            })
            .WithProperty("product-id", productId.ToString())
            .WithProperty("new-stock", newStock)
            .PublishAsync(new InventoryUpdated(productId, newStock));
            
        _logger.LogInformation("InventoryUpdated event published for Product {ProductId}", productId);
    }

    public async Task ProcessInventoryReservationAsync(Guid orderId, List<InventoryItem> items)
    {
        // Initialize the client first
        await _streamFlow.InitializeAsync();
        
        try
        {
            // Check availability
            var availableItems = await CheckAvailabilityAsync(items);
            
            if (availableItems.Count != items.Count)
            {
                var unavailableItems = items.Except(availableItems).ToList();
                throw new InventoryException($"Items not available: {string.Join(", ", unavailableItems.Select(i => i.ProductId))}");
            }
            
            // Reserve items
            await ReserveInventoryAsync(orderId, items.Select(i => i.ProductId.ToString()).ToList());
            
            // Update stock levels
            foreach (var item in items)
            {
                var newStock = item.CurrentStock - item.Quantity;
                await UpdateInventoryAsync(item.ProductId, newStock);
            }
            
            _logger.LogInformation("Inventory reservation completed for Order {OrderId}", orderId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to process inventory reservation for Order {OrderId}", orderId);
            throw;
        }
    }

    private async Task<List<InventoryItem>> CheckAvailabilityAsync(List<InventoryItem> items)
    {
        // Simulate availability check
        await Task.Delay(50);
        return items.Where(item => item.CurrentStock >= item.Quantity).ToList();
    }
}

public class InventoryItem
{
    public Guid ProductId { get; set; }
    public int Quantity { get; set; }
    public int CurrentStock { get; set; }
}

public class InventoryException : Exception
{
    public InventoryException(string message) : base(message) { }
    public InventoryException(string message, Exception innerException) : base(message, innerException) { }
}
```

### 3. Inventory Reserved Handler (Consumer)

```csharp
// Services/InventoryReservedHandler.cs
using FS.StreamFlow.Core.Features.Events.Interfaces;
using FS.StreamFlow.Core.Features.Messaging.Interfaces;
using Microsoft.Extensions.Logging;

public class InventoryReservedHandler : IAsyncEventHandler<InventoryReserved>
{
    private readonly IStreamFlowClient _streamFlow;
    private readonly ILogger<InventoryReservedHandler> _logger;

    public InventoryReservedHandler(IStreamFlowClient streamFlow, ILogger<InventoryReservedHandler> logger)
    {
        _streamFlow = streamFlow;
        _logger = logger;
    }

    public async Task HandleAsync(InventoryReserved @event, EventContext context)
    {
        try
        {
            // Initialize the client first
            await _streamFlow.InitializeAsync();
            
            _logger.LogInformation("Handling InventoryReserved event for Order {OrderId}", @event.OrderId);
            
            // Business logic (e.g., update stock, notify user)
            await ProcessInventoryReservationAsync(@event, context);
            
            // Publish follow-up event
            await _streamFlow.EventBus.Event<InventoryReservationProcessed>()
                .WithCorrelationId(context.CorrelationId)
                .WithCausationId(context.EventId)
                .WithSource("inventory-service")
                .WithProperty("order-id", @event.OrderId.ToString())
                .WithProperty("item-count", @event.Items.Count)
                .PublishAsync(new InventoryReservationProcessed(@event.OrderId, @event.Items, DateTime.UtcNow));
                
            _logger.LogInformation("Inventory reservation processed for Order {OrderId}", @event.OrderId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing inventory reservation for Order {OrderId}", @event.OrderId);
            throw;
        }
    }

    private async Task ProcessInventoryReservationAsync(InventoryReserved @event, EventContext context)
    {
        // Simulate inventory processing
        await Task.Delay(100);
        
        // Update inventory records
        // await _inventoryRepository.UpdateReservationAsync(@event.OrderId, @event.Items);
        
        // Send notifications
        // await _notificationService.SendInventoryReservedNotificationAsync(@event.OrderId);
    }
}

public record InventoryReservationProcessed(Guid OrderId, List<string> Items, DateTime ProcessedAt) : IIntegrationEvent
{
    public Guid Id { get; } = Guid.NewGuid();
    public DateTime OccurredOn { get; } = DateTime.UtcNow;
    public int Version { get; } = 1;
    public string EventType => nameof(InventoryReservationProcessed);
    public string? CorrelationId { get; set; }
    public string? CausationId { get; set; }
    public IDictionary<string, object> Metadata { get; } = new Dictionary<string, object>();
    public string Source => "inventory-service";
    public string RoutingKey => "inventory.reservation.processed";
    public string? Target { get; set; }
    public string SchemaVersion => "1.0";
    public TimeSpan? TimeToLive { get; set; }
}
```

### 4. Program Setup

```csharp
// Program.cs
using FS.StreamFlow.RabbitMQ.DependencyInjection;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

var builder = WebApplication.CreateBuilder(args);

// Add FS.StreamFlow with RabbitMQ
builder.Services.AddRabbitMQStreamFlow(options =>
{
    // Client configuration
    options.ClientConfiguration.ClientName = "Inventory Management";
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
    
    // Event bus settings
    options.EventBusSettings.EnableEventBus = true;
    options.EventBusSettings.IntegrationEventExchange = "inventory-events";
    options.EventBusSettings.EventHandlerAssembly = typeof(InventoryReserved).Assembly;
    
    // Error handling settings
    options.ErrorHandlingSettings.MaxRetries = 3;
    options.ErrorHandlingSettings.RetryDelay = TimeSpan.FromSeconds(1);
    options.ErrorHandlingSettings.UseExponentialBackoff = true;
    options.ErrorHandlingSettings.EnableDeadLetterQueue = true;
    
    // Dead letter settings
    options.DeadLetterSettings.ExchangeName = "dlx";
    options.DeadLetterSettings.RoutingKey = "inventory.failed";
});

// Register services
builder.Services.AddScoped<InventoryService>();
builder.Services.AddScoped<InventoryReservedHandler>();

var app = builder.Build();

// Initialize the client
var streamFlow = app.Services.GetRequiredService<IStreamFlowClient>();
await streamFlow.InitializeAsync();

// Setup infrastructure
await streamFlow.ExchangeManager.Exchange("inventory-events")
    .AsTopic()
    .WithDurable(true)
    .DeclareAsync();

await streamFlow.ExchangeManager.Exchange("dlx")
    .AsTopic()
    .WithDurable(true)
    .DeclareAsync();

// Setup queues
await streamFlow.QueueManager.Queue("inventory-reserved-queue")
    .WithDurable(true)
    .WithDeadLetterExchange("dlx")
    .WithDeadLetterRoutingKey("inventory-reserved.failed")
    .BindToExchange("inventory-events", "inventory.reserved")
    .DeclareAsync();

await streamFlow.QueueManager.Queue("inventory-updated-queue")
    .WithDurable(true)
    .WithDeadLetterExchange("dlx")
    .WithDeadLetterRoutingKey("inventory-updated.failed")
    .BindToExchange("inventory-events", "inventory.updated")
    .DeclareAsync();

await streamFlow.QueueManager.Queue("inventory-processed-queue")
    .WithDurable(true)
    .WithDeadLetterExchange("dlx")
    .WithDeadLetterRoutingKey("inventory-processed.failed")
    .BindToExchange("inventory-events", "inventory.reservation.processed")
    .DeclareAsync();

await streamFlow.QueueManager.Queue("dead-letter-queue")
    .WithDurable(true)
    .BindToExchange("dlx", "inventory.failed")
    .DeclareAsync();

// Start event consumers
await streamFlow.Consumer.Queue<InventoryReserved>("inventory-reserved-queue")
    .WithConcurrency(3)
    .WithPrefetchCount(50)
    .WithErrorHandler(async (exception, context) =>
    {
        var logger = app.Services.GetRequiredService<ILogger<Program>>();
        logger.LogError(exception, "Error processing inventory reserved event {MessageId}", context.MessageId);
        return exception is InventoryException ? false : true; // Don't retry inventory exceptions
    })
    .ConsumeAsync(async (inventoryReserved, context) =>
    {
        var handler = app.Services.GetRequiredService<InventoryReservedHandler>();
        await handler.HandleAsync(inventoryReserved, new EventContext
        {
            EventId = context.MessageId,
            CorrelationId = context.CorrelationId,
            Timestamp = DateTimeOffset.UtcNow
        });
        return true;
    });

// Start dead letter processor
await streamFlow.Consumer.Queue<DeadLetterMessage>("dead-letter-queue")
    .WithConcurrency(1)
    .WithPrefetchCount(10)
    .ConsumeAsync(async (deadLetterMessage, context) =>
    {
        var logger = app.Services.GetRequiredService<ILogger<Program>>();
        logger.LogWarning("Processing dead letter inventory event {MessageId}: {Reason}", 
            deadLetterMessage.MessageId, deadLetterMessage.Reason);
        return true;
    });

// Start the application
app.Run();
```

## 🛡️ Error Handling

### Inventory-Specific Error Handling
```csharp
// Custom error handling for inventory operations
.WithErrorHandler(async (exception, context) =>
{
    if (exception is InventoryException)
    {
        // Log inventory-specific errors
        _logger.LogWarning("Inventory error for message {MessageId}: {Message}", 
            context.MessageId, exception.Message);
        return false; // Send to DLQ immediately
    }
    
    if (exception is TimeoutException)
    {
        return true; // Retry timeout errors
    }
    
    return true; // Default retry for other errors
})
```

### Inventory Exception Types
- **InventoryException**: Business logic errors (insufficient stock, invalid items)
- **ValidationException**: Data validation errors
- **TimeoutException**: Network or processing timeouts
- **ConcurrencyException**: Race conditions in inventory updates

## 📊 Monitoring

### Inventory Event Monitoring
```csharp
// Monitor inventory event processing
var eventBusStats = await streamFlow.EventBus.GetStatisticsAsync();
Console.WriteLine($"Inventory events published: {eventBusStats.EventsPublished}");
Console.WriteLine($"Inventory events processed: {eventBusStats.EventsProcessed}");
Console.WriteLine($"Failed inventory events: {eventBusStats.FailedEvents}");
```

### Inventory Metrics
- **Reservation Rate**: Inventory reservations per minute
- **Update Rate**: Stock updates per minute
- **Error Rate**: Failed inventory operations percentage
- **Processing Time**: Average time to process inventory events
- **Stock Levels**: Current inventory levels by product

### RabbitMQ Management
- **Management UI**: http://localhost:15672
- **Queue Monitoring**: Track inventory event queues
- **Exchange Monitoring**: Monitor inventory event routing
- **Performance Alerts**: Set up alerts for high error rates

## 🎯 Key Takeaways

- **Event-driven inventory management** enables real-time updates and consistency
- **Integration events** provide clear separation between inventory operations
- **Error handling** must account for inventory-specific business rules
- **Monitoring** is critical for maintaining inventory accuracy
- **Correlation and causation IDs** enable tracking of inventory workflows
- **FS.StreamFlow simplifies** inventory event management in .NET applications
- **Dead letter queues** ensure no inventory events are lost
- **Real-time processing** improves inventory accuracy and customer experience 