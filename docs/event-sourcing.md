# Event Sourcing Guide

This guide explains how to implement event sourcing using FS.RabbitMQ.

## Table of Contents
- [Overview](#overview)
- [Event Store](#event-store)
- [Aggregates](#aggregates)
- [Snapshots](#snapshots)
- [Best Practices](#best-practices)
- [Examples](#examples)

## Overview

Event sourcing is a pattern where the state of your application is determined by a sequence of events rather than just the current state. FS.RabbitMQ provides a complete event sourcing implementation through its event store component.

## Event Store

### Configuration

```csharp
builder.Services.AddRabbitMQ()
    .WithEventStore(config =>
    {
        config.StreamPrefix = "eventstore";
        config.SnapshotInterval = 100;
        config.EnableSnapshots = true;
    })
    .Build();
```

### Basic Operations

```csharp
// Append events to a stream
await _eventStore.AppendToStreamAsync(
    streamName: $"order-{orderId}",
    expectedVersion: 0,
    events: new[] {
        new OrderCreated(orderId, customerName, amount),
        new OrderItemAdded(orderId, itemId, quantity)
    });

// Read events from a stream
var events = await _eventStore.ReadStreamAsync(
    streamName: $"order-{orderId}",
    fromVersion: 0);

// Get stream metadata
var metadata = await _eventStore.GetStreamMetadataAsync(
    streamName: $"order-{orderId}");
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
// Load aggregate from event stream
var order = await _eventStore.LoadAggregateAsync<OrderAggregate>(
    streamName: $"order-{orderId}");

// Load aggregate with snapshot
var order = await _eventStore.LoadAggregateAsync<OrderAggregate>(
    streamName: $"order-{orderId}",
    useSnapshot: true);
```

## Snapshots

Snapshots improve performance by reducing the number of events that need to be replayed:

### Configuration

```csharp
builder.Services.AddRabbitMQ()
    .WithEventStore(config =>
    {
        config.EnableSnapshots = true;
        config.SnapshotInterval = 100; // Take snapshot every 100 events
    })
    .Build();
```

### Managing Snapshots

```csharp
// Create snapshot
await _eventStore.CreateSnapshotAsync(
    streamName: $"order-{orderId}",
    snapshot: order);

// Get latest snapshot
var snapshot = await _eventStore.GetLatestSnapshotAsync<OrderAggregate>(
    streamName: $"order-{orderId}");

// Delete snapshot
await _eventStore.DeleteSnapshotAsync(
    streamName: $"order-{orderId}",
    version: snapshotVersion);
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
    private readonly IEventStore _eventStore;
    private readonly IEventBus _eventBus;

    public async Task CreateOrderAsync(CreateOrderRequest request)
    {
        var orderId = Guid.NewGuid();
        var streamName = $"order-{orderId}";

        // Create and append events
        var events = new IEvent[]
        {
            new OrderCreated(orderId, request.CustomerName, request.Total),
            new OrderItemAdded(orderId, request.ItemId, request.Quantity)
        };

        await _eventStore.AppendToStreamAsync(streamName, 0, events);

        // Publish domain events
        foreach (var @event in events)
        {
            await _eventBus.PublishDomainEventAsync(@event);
        }
    }

    public async Task UpdateOrderAsync(UpdateOrderRequest request)
    {
        var streamName = $"order-{request.OrderId}";

        // Load current state
        var order = await _eventStore.LoadAggregateAsync<OrderAggregate>(
            streamName,
            useSnapshot: true);

        // Generate and append new events
        var events = new IEvent[]
        {
            new OrderItemAdded(request.OrderId, request.ItemId, request.Quantity)
        };

        await _eventStore.AppendToStreamAsync(
            streamName,
            order.Version,
            events);

        // Publish domain events
        foreach (var @event in events)
        {
            await _eventBus.PublishDomainEventAsync(@event);
        }
    }

    public async Task GetOrderHistoryAsync(Guid orderId)
    {
        var streamName = $"order-{orderId}";

        // Read all events
        var events = await _eventStore.ReadStreamAsync(streamName);

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
    var streamName = $"order-{request.OrderId}";
    var maxRetries = 3;
    var currentRetry = 0;

    while (currentRetry < maxRetries)
    {
        try
        {
            // Load aggregate
            var order = await _eventStore.LoadAggregateAsync<OrderAggregate>(
                streamName);

            // Generate new events
            var events = new IEvent[]
            {
                new OrderItemAdded(request.OrderId, request.ItemId, request.Quantity)
            };

            // Append events with expected version
            await _eventStore.AppendToStreamAsync(
                streamName,
                order.Version,
                events);

            // Success - exit loop
            break;
        }
        catch (ConcurrencyException)
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
    private readonly IEventStore _eventStore;
    private readonly ILogger<SnapshotManager> _logger;

    public async Task ManageSnapshotsAsync(string streamName)
    {
        try
        {
            // Get stream info
            var metadata = await _eventStore.GetStreamMetadataAsync(streamName);

            // Check if snapshot needed
            if (metadata.EventCount >= 100) // Snapshot interval
            {
                // Load aggregate
                var aggregate = await _eventStore.LoadAggregateAsync<OrderAggregate>(
                    streamName);

                // Create snapshot
                await _eventStore.CreateSnapshotAsync(
                    streamName,
                    aggregate);

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