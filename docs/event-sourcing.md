# Event Sourcing Guide

This guide explains how to implement event sourcing using FS.StreamFlow.

## Table of Contents
- [Overview](#overview)
- [Event Store](#event-store)
- [Aggregates](#aggregates)
- [Snapshots](#snapshots)
- [Best Practices](#best-practices)
- [Examples](#examples)

## Overview

Event sourcing is a pattern where the state of your application is determined by a sequence of events rather than just the current state. FS.StreamFlow provides a complete event sourcing implementation through its event store component.

## Event Store

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

### Basic Operations

```csharp
// Append events to a stream with fluent API
await _streamFlow.InitializeAsync();
await _streamFlow.EventStore.Stream($"order-{orderId}")
    .AppendEvent(new OrderCreated(orderId, customerName, amount))
    .AppendEvent(new OrderItemAdded(orderId, itemId, quantity))
    .SaveAsync();

// Read events from a stream with fluent API
await _streamFlow.InitializeAsync();
var events = await _streamFlow.EventStore.Stream($"order-{orderId}")
    .FromVersion(0)
    .WithMaxCount(100)
    .ReadAsync();

// Get stream metadata with fluent API
await _streamFlow.InitializeAsync();
var metadata = await _streamFlow.EventStore.Stream($"order-{orderId}")
    .GetMetadataAsync();
    
// Check if stream exists with fluent API
await _streamFlow.InitializeAsync();
var exists = await _streamFlow.EventStore.Stream($"order-{orderId}")
    .ExistsAsync();
    
// Get stream version with fluent API
await _streamFlow.InitializeAsync();
var version = await _streamFlow.EventStore.Stream($"order-{orderId}")
    .GetVersionAsync();
```

### Advanced Event Store Operations

```csharp
// Complex event store operations with fluent API
public class OrderEventStore
{
    private readonly IStreamFlowClient _streamFlow;
    
    public async Task<long> SaveOrderEventsAsync(Guid orderId, IEnumerable<object> events)
    {
        await _streamFlow.InitializeAsync();
        return await _streamFlow.EventStore.Stream($"order-{orderId}")
            .AppendEvents(events)
            .SaveAsync();
    }
    
    public async Task<long> SaveOrderEventsWithExpectedVersionAsync(Guid orderId, IEnumerable<object> events, long expectedVersion)
    {
        await _streamFlow.InitializeAsync();
        return await _streamFlow.EventStore.Stream($"order-{orderId}")
            .AppendEventsWithExpectedVersion(events, expectedVersion)
            .SaveAsync();
    }
    
    public async Task<IEnumerable<object>> GetOrderEventsAsync(Guid orderId, long fromVersion = 0)
    {
        await _streamFlow.InitializeAsync();
        return await _streamFlow.EventStore.Stream($"order-{orderId}")
            .FromVersion(fromVersion)
            .WithMaxCount(100)
            .ReadAsync();
    }
    
    public async Task<IEnumerable<object>> GetRecentOrderEventsAsync(Guid orderId)
    {
        await _streamFlow.InitializeAsync();
        return await _streamFlow.EventStore.Stream($"order-{orderId}")
            .FromVersion(-1)
            .WithMaxCount(10)
            .ReadBackwardAsync();
    }
    
    public async Task<bool> TruncateOrderStreamAsync(Guid orderId, long version)
    {
        await _streamFlow.InitializeAsync();
        return await _streamFlow.EventStore.Stream($"order-{orderId}")
            .TruncateAsync(version);
    }
    
    public async Task<bool> DeleteOrderStreamAsync(Guid orderId)
    {
        await _streamFlow.InitializeAsync();
        return await _streamFlow.EventStore.Stream($"order-{orderId}")
            .DeleteAsync();
    }
}
```

### Legacy Event Store API

```csharp
// Legacy API - still supported but fluent API is recommended
public class LegacyEventStore
{
    private readonly IStreamFlowClient _streamFlow;
    
    public async Task LegacyOperationsAsync()
    {
        await _streamFlow.InitializeAsync();
        // NOTE: The following methods are not part of FS.StreamFlow public API, this is for illustration only.
        // Append events to a stream (legacy)
        await _streamFlow.EventStore.AppendToStreamAsync(
            streamName: $"order-{orderId}",
            expectedVersion: 0,
            events: new[] { orderCreatedEvent, orderShippedEvent });

        // Read events from a stream (legacy)
        var events = await _streamFlow.EventStore.ReadStreamAsync(
            streamName: $"order-{orderId}",
            fromVersion: 0);

        // Get stream metadata (legacy)
        var metadata = await _streamFlow.EventStore.GetStreamMetadataAsync(
            streamName: $"order-{orderId}");
    }
}
```

## Aggregates

Aggregates are the building blocks of your domain model:

```csharp
public class OrderAggregate
{
    public Guid Id { get; private set; }
    public string Status { get; private set; }
    public decimal Total { get; private set; }
    public List<OrderItem> Items { get; private set; }

    // Event application methods
    public void Apply(OrderCreated @event)
    {
        Id = @event.OrderId;
        Status = "Created";
        Total = @event.Amount;
        Items = new List<OrderItem>();
    }

    public void Apply(OrderItemAdded @event)
    {
        var item = new OrderItem(@event.ItemId, @event.Quantity);
        Items.Add(item);
        Total += item.Price * item.Quantity;
    }

    public void Apply(OrderShipped @event)
    {
        Status = "Shipped";
    }
}
```

### Loading Aggregates

```csharp
// Load aggregate from event stream with fluent API
public async Task<OrderAggregate> LoadOrderAggregateAsync(Guid orderId)
{
    await _streamFlow.InitializeAsync();
    var events = await _streamFlow.EventStore.Stream($"order-{orderId}")
        .FromVersion(0)
        .ReadAsync();
    
    var aggregate = new OrderAggregate();
    foreach (var @event in events)
    {
        aggregate.Apply((dynamic)@event);
    }
    
    return aggregate;
}

// Load aggregate with snapshot using fluent API
public async Task<OrderAggregate> LoadOrderAggregateWithSnapshotAsync(Guid orderId)
{
    await _streamFlow.InitializeAsync();
    var snapshot = await _streamFlow.EventStore.Stream($"order-{orderId}")
        .GetSnapshotAsync();
    
    var aggregate = new OrderAggregate();
    long fromVersion = 0;
    
    if (snapshot != null)
    {
        aggregate = (OrderAggregate)snapshot;
        fromVersion = aggregate.Version + 1;
    }
    
    var events = await _streamFlow.EventStore.Stream($"order-{orderId}")
        .FromVersion(fromVersion)
        .ReadAsync();
    
    foreach (var @event in events)
    {
        aggregate.Apply((dynamic)@event);
    }
    
    return aggregate;
}
```

## Snapshots

Snapshots improve performance by reducing the number of events that need to be replayed:

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

### Managing Snapshots with Fluent API

```csharp
// Save snapshot with fluent API
await _streamFlow.InitializeAsync();
await _streamFlow.EventStore.Stream($"order-{orderId}")
    .WithSnapshot(orderSnapshot, version: 100)
    .SaveSnapshotAsync();

// Get snapshot with fluent API
await _streamFlow.InitializeAsync();
var snapshot = await _streamFlow.EventStore.Stream($"order-{orderId}")
    .GetSnapshotAsync();

// Save events with snapshot in one operation
await _streamFlow.InitializeAsync();
await _streamFlow.EventStore.Stream($"order-{orderId}")
    .AppendEvents(newEvents)
    .WithSnapshot(orderSnapshot, version: 100)
    .SaveAsync();
```

### Advanced Snapshot Management

```csharp
public class OrderSnapshotManager
{
    private readonly IStreamFlowClient _streamFlow;
    
    public async Task<bool> ShouldCreateSnapshotAsync(Guid orderId)
    {
        await _streamFlow.InitializeAsync();
        var version = await _streamFlow.EventStore.Stream($"order-{orderId}")
            .GetVersionAsync();
            
        return version > 0 && version % 100 == 0; // Create snapshot every 100 events
    }
    
    public async Task CreateSnapshotAsync(Guid orderId)
    {
        await _streamFlow.InitializeAsync();
        // Load current aggregate
        var aggregate = await LoadOrderAggregateAsync(orderId);
        
        // Save snapshot
        await _streamFlow.EventStore.Stream($"order-{orderId}")
            .WithSnapshot(aggregate, aggregate.Version)
            .SaveSnapshotAsync();
    }
    
    public async Task<OrderAggregate> LoadAggregateWithSnapshotAsync(Guid orderId)
    {
        await _streamFlow.InitializeAsync();
        var snapshot = await _streamFlow.EventStore.Stream($"order-{orderId}")
            .GetSnapshotAsync();
        
        var aggregate = new OrderAggregate();
        long fromVersion = 0;
        
        if (snapshot != null)
        {
            aggregate = (OrderAggregate)snapshot;
            fromVersion = aggregate.Version + 1;
        }
        
        var events = await _streamFlow.EventStore.Stream($"order-{orderId}")
            .FromVersion(fromVersion)
            .ReadAsync();
        
        foreach (var @event in events)
        {
            aggregate.Apply((dynamic)@event);
        }
        
        return aggregate;
    }
}
```

## Best Practices

1. **Event Design**
   - Make events immutable
   - Include only necessary data
   - Use meaningful event names
   - Version your events

2. **Stream Management**
   - Use consistent naming conventions
   - Keep streams focused and cohesive
   - Consider stream size and performance

3. **Snapshots**
   - Choose appropriate snapshot intervals
   - Consider storage implications
   - Handle snapshot failures gracefully

4. **Performance**
   - Use snapshots for large streams
   - Implement caching where appropriate
   - Monitor stream sizes and growth

5. **Error Handling**
   - Handle concurrency conflicts
   - Implement proper error recovery
   - Validate event data

## Examples

### Order Processing Example

```csharp
public class OrderService
{
    private readonly IStreamFlowClient _streamFlow;
    private readonly IEventBus _eventBus;

    public async Task CreateOrderAsync(CreateOrderRequest request)
    {
        await _streamFlow.InitializeAsync();
        var orderId = Guid.NewGuid();
        var streamName = $"order-{orderId}";

        // NOTE: IEvent interface and LoadAggregateAsync method are not part of FS.StreamFlow public API, this is for illustration only.
        // Create and append events
        var events = new object[]
        {
            new OrderCreated(orderId, request.CustomerName, request.Total),
            new OrderItemAdded(orderId, request.ItemId, request.Quantity)
        };

        await _streamFlow.EventStore.Stream(streamName)
            .AppendEvents(events)
            .SaveAsync();

        // Publish domain events
        foreach (var @event in events)
        {
            await _streamFlow.EventBus.Event<object>()
                .PublishAsync(@event);
        }
    }

    public async Task UpdateOrderAsync(UpdateOrderRequest request)
    {
        await _streamFlow.InitializeAsync();
        var streamName = $"order-{request.OrderId}";

        // NOTE: LoadAggregateAsync method is not part of FS.StreamFlow public API, this is for illustration only.
        // Load current state
        var order = await LoadOrderAggregateAsync(request.OrderId);

        // Generate and append new events
        var events = new object[]
        {
            new OrderItemAdded(request.OrderId, request.ItemId, request.Quantity)
        };

        await _streamFlow.EventStore.Stream(streamName)
            .AppendEventsWithExpectedVersion(events, order.Version)
            .SaveAsync();

        // Publish domain events
        foreach (var @event in events)
        {
            await _streamFlow.EventBus.Event<object>()
                .PublishAsync(@event);
        }
    }

    public async Task GetOrderHistoryAsync(Guid orderId)
    {
        await _streamFlow.InitializeAsync();
        var streamName = $"order-{orderId}";

        // Read all events
        var events = await _streamFlow.EventStore.Stream(streamName)
            .FromVersion(0)
            .ReadAsync();

        // Process events
        foreach (var @event in events)
        {
            switch (@event)
            {
                case OrderCreated created:
                    Console.WriteLine($"Order created: {created.OrderId}");
                    break;
                case OrderItemAdded itemAdded:
                    Console.WriteLine($"Item added: {itemAdded.ItemId}");
                    break;
                case OrderShipped shipped:
                    Console.WriteLine($"Order shipped");
                    break;
            }
        }
    }
}
```

### Concurrency Handling Example

```csharp
public async Task UpdateOrderAsync(UpdateOrderRequest request)
{
    await _streamFlow.InitializeAsync();
    var streamName = $"order-{request.OrderId}";
    var maxRetries = 3;
    var currentRetry = 0;

    while (currentRetry < maxRetries)
    {
        try
        {
            // NOTE: LoadAggregateAsync and ConcurrencyException are not part of FS.StreamFlow public API, this is for illustration only.
            // Load aggregate
            var order = await LoadOrderAggregateAsync(request.OrderId);

            // Generate new events
            var events = new object[]
            {
                new OrderItemAdded(request.OrderId, request.ItemId, request.Quantity)
            };

            // Append events with expected version
            await _streamFlow.EventStore.Stream(streamName)
                .AppendEventsWithExpectedVersion(events, order.Version)
                .SaveAsync();

            // Success - exit loop
            break;
        }
        catch (Exception ex) // NOTE: ConcurrencyException is not part of FS.StreamFlow public API
        {
            currentRetry++;
            if (currentRetry >= maxRetries)
            {
                throw new Exception("Failed to update order after max retries");
            }
            await Task.Delay(100 * currentRetry); // Exponential backoff
        }
    }
}
```

### Snapshot Management Example

```csharp
public class SnapshotManager
{
    private readonly IStreamFlowClient _streamFlow;
    private readonly ILogger<SnapshotManager> _logger;

    public async Task ManageSnapshotsAsync(string streamName)
    {
        await _streamFlow.InitializeAsync();
        try
        {
            // NOTE: GetStreamMetadataAsync and CreateSnapshotAsync are not part of FS.StreamFlow public API, this is for illustration only.
            // Get stream info
            var version = await _streamFlow.EventStore.Stream(streamName)
                .GetVersionAsync();

            // Check if snapshot needed
            if (version >= 100) // Snapshot interval
            {
                // Load aggregate
                var aggregate = await LoadOrderAggregateAsync(Guid.Parse(streamName.Replace("order-", "")));

                // Create snapshot
                await _streamFlow.EventStore.Stream(streamName)
                    .WithSnapshot(aggregate, aggregate.Version)
                    .SaveSnapshotAsync();

                _logger.LogInformation(
                    "Created snapshot for stream {StreamName} at version {Version}",
                    streamName,
                    aggregate.Version);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex,
                "Error managing snapshots for stream {StreamName}",
                streamName);
            throw;
        }
    }
}
``` 