# Consumer Guide

This guide covers everything you need to know about consuming messages with FS.StreamFlow, from basic consumption to advanced enterprise patterns.

## 📋 Table of Contents

1. [Getting Started](#getting-started)
2. [Basic Message Consumption](#basic-message-consumption)
3. [Advanced Consumption Patterns](#advanced-consumption-patterns)
4. [Message Acknowledgment](#message-acknowledgment)
5. [Error Handling and Retries](#error-handling-and-retries)
6. [Concurrent Processing](#concurrent-processing)
7. [Message Filtering](#message-filtering)
8. [Dead Letter Queues](#dead-letter-queues)
9. [Performance Optimization](#performance-optimization)
10. [Monitoring and Metrics](#monitoring-and-metrics)
11. [Best Practices](#best-practices)

## 🚀 Getting Started

### Basic Consumer Setup

```csharp
// Program.cs
using FS.StreamFlow.RabbitMQ.DependencyInjection;

var builder = WebApplication.CreateBuilder(args);

// Add FS.StreamFlow with consumer configuration
builder.Services.AddRabbitMQStreamFlow(options =>
{
    // Connection settings
    options.ConnectionSettings.Host = "localhost";
    options.ConnectionSettings.Port = 5672;
    options.ConnectionSettings.Username = "guest";
    options.ConnectionSettings.Password = "guest";
    options.ConnectionSettings.VirtualHost = "/";
    
    // Consumer settings
    options.ConsumerSettings.PrefetchCount = 50;
    options.ConsumerSettings.AutoAcknowledge = false;
    options.ConsumerSettings.MaxConcurrentConsumers = 5;
});

var app = builder.Build();
```

### Injecting the Consumer

```csharp
public class OrderProcessor
{
    private readonly IStreamFlowClient _streamFlow;
    private readonly ILogger<OrderProcessor> _logger;

    public OrderProcessor(IStreamFlowClient streamFlow, ILogger<OrderProcessor> logger)
    {
        _streamFlow = streamFlow;
        _logger = logger;
    }

    public async Task StartProcessingAsync(CancellationToken cancellationToken = default)
    {
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
            
        // Consume with fluent API
        await _streamFlow.Consumer.Queue<Order>("order-processing")
            .WithConcurrency(5)
            .WithPrefetchCount(100)
            .WithAutoAck(false)
            .WithErrorHandler(async (exception, context) =>
            {
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
                DeadLetterExchange = "dlx",
                DeadLetterQueue = "dlq"
            })
            .ConsumeAsync(ProcessOrderAsync, cancellationToken);
    }

    private async Task<bool> ProcessOrderAsync(Order order, MessageContext context)
    {
        _logger.LogInformation("Processing order {OrderId}", order.Id);
        
        // Process the order
        await Task.Delay(1000); // Simulate processing
        
        return true; // Acknowledge the message
    }
}
```

## 📥 Basic Message Consumption

### Simple Message Consumer with Fluent API

```csharp
public class SimpleConsumer
{
    private readonly IStreamFlowClient _streamFlow;
    private readonly ILogger<SimpleConsumer> _logger;

    public SimpleConsumer(IStreamFlowClient streamFlow, ILogger<SimpleConsumer> logger)
    {
        _streamFlow = streamFlow;
        _logger = logger;
    }

    // Consume typed messages with fluent API
    public async Task ConsumeOrdersAsync(CancellationToken cancellationToken = default)
    {
        await _streamFlow.Consumer.Queue<Order>("order-processing")
            .WithConcurrency(3)
            .WithPrefetchCount(50)
            .WithAutoAck(false)
            .WithErrorHandler(async (exception, context) =>
            {
                // Custom error handling
                return exception is ConnectFailureException || exception is BrokerUnreachableException;
            })
            .WithRetryPolicy(new RetryPolicySettings
            {
                RetryPolicy = RetryPolicyType.Linear,
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
                _logger.LogInformation("Received order {OrderId} from {CustomerName}", 
                    order.Id, order.CustomerName);
                
                // Process the order
                await ProcessOrderAsync(order);
                
                return true; // Acknowledge
            }, cancellationToken);
    }

    // Raw message consumption with fluent API
    public async Task ConsumeRawMessagesAsync(CancellationToken cancellationToken = default)
    {
        await _streamFlow.Consumer.Queue<byte[]>("raw-data")
            .WithConcurrency(5)
            .WithPrefetchCount(100)
            .WithAutoAck(false)
            .WithErrorHandler(async (exception, context) =>
            {
                // Log error and don't requeue raw data
                _logger.LogError(exception, "Error processing raw data");
                return false;
            })
            .ConsumeAsync(async (data, context) =>
            {
                _logger.LogInformation("Received {Size} bytes from {Exchange}", 
                    data.Length, context.Exchange);
                
                // Process raw data
                await ProcessRawDataAsync(data);
                
                return true; // Acknowledge
            }, cancellationToken);
    }
    
    // Event consumption with fluent API
    public async Task ConsumeEventsAsync(CancellationToken cancellationToken = default)
    {
        await _streamFlow.Consumer.Queue<OrderCreated>()
            .WithConcurrency(3)
            .WithPrefetch(50)
            .WithErrorHandling(ErrorHandlingStrategy.Retry)
            .WithMetadataFilter(metadata => metadata.Source == "order-service")
            .WithDeadLetterQueue("event-dlq")
            .HandleAsync(async (orderCreated, context) =>
            {
                _logger.LogInformation("Received OrderCreated event for order {OrderId}", 
                    orderCreated.OrderId);
                
                // Handle the event
                await HandleOrderCreatedAsync(orderCreated, context);
                
                return true; // Acknowledge
            }, cancellationToken);
    }

    private async Task ProcessOrderAsync(Order order)
    {
        // Business logic
        await Task.Delay(1000);
        
        // Store in event store
        await _streamFlow.EventStore.Stream($"order-{order.Id}")
            .AppendEvent(new OrderProcessed(order.Id, DateTime.UtcNow))
            .SaveAsync();
    }
    
    private async Task ProcessRawDataAsync(byte[] data)
    {
        // Process raw data
        await Task.Delay(500);
    }
    
    private async Task HandleOrderCreatedAsync(OrderCreated orderCreated, EventContext context)
    {
        // Handle event
        await Task.Delay(200);
    }
}
```

### Advanced Consumer Configuration

```csharp
public class AdvancedConsumer
{
    private readonly IStreamFlowClient _streamFlow;
    
    public async Task SetupAdvancedConsumerAsync()
    {
        // High-throughput consumer with advanced configuration
        await _streamFlow.Consumer.Queue<Order>("order-processing")
            .WithConcurrency(10)
            .WithPrefetch(500)
            .WithAutoAck(false)
            .WithErrorHandling(ErrorHandlingStrategy.Retry)
            .WithRetryPolicy(RetryPolicyType.ExponentialBackoff)
            .WithMaxRetries(5)
            .WithRetryDelay(TimeSpan.FromSeconds(1))
            .WithMaxRetryDelay(TimeSpan.FromMinutes(5))
            .WithDeadLetterQueue("dlq")
            .WithMessageFilter(order => order.Status == OrderStatus.Pending)
            .WithTimeout(TimeSpan.FromMinutes(10))
            .WithHealthCheck(enabled: true)
            .WithMetrics(enabled: true)
            .HandleAsync(async (order, context) =>
            {
                // Complex processing with error handling
                try
                {
                    await ComplexOrderProcessingAsync(order);
                    return true;
                }
                catch (BusinessException ex)
                {
                    // Log business exception and reject message
                    _logger.LogError(ex, "Business error processing order {OrderId}", order.Id);
                    return false; // This will trigger retry or dead letter
                }
                catch (Exception ex)
                {
                    // Log unexpected exception and reject message
                    _logger.LogError(ex, "Unexpected error processing order {OrderId}", order.Id);
                    return false;
                }
            });
    }
    
    private async Task ComplexOrderProcessingAsync(Order order)
    {
        // Complex business logic
        await Task.Delay(2000);
    }
}
```

### Legacy Consumer API

```csharp
// Legacy API - still supported but fluent API is recommended
public class LegacyConsumer
{
    private readonly IStreamFlowClient _streamFlow;
    
    public async Task ConsumeLegacyStyleAsync(CancellationToken cancellationToken = default)
    {
        await _streamFlow.Consumer.ConsumeAsync<Order>(
            queueName: "order-processing",
            messageHandler: async (order, context) =>
            {
                _logger.LogInformation("Received order {OrderId} from {CustomerName}", 
                    order.Id, order.CustomerName);
                
                // Process the order
                await ProcessOrderAsync(order);
                
                return true; // Acknowledge
            },
            cancellationToken: cancellationToken);
    }
}
```

### Infrastructure Setup for Consumers

```csharp
public class ConsumerInfrastructureSetup
{
    private readonly IStreamFlowClient _streamFlow;
    
    public async Task SetupConsumerInfrastructureAsync()
    {
        // Create exchanges
        await _streamFlow.ExchangeManager.Exchange("orders")
            .AsTopic()
            .WithDurable(true)
            .WithAlternateExchange("alt-orders")
            .DeclareAsync();
            
        await _streamFlow.ExchangeManager.Exchange("dlx")
            .AsDirect()
            .WithDurable(true)
            .DeclareAsync();
            
        // Create consumer queues
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
            
        await _streamFlow.QueueManager.Queue("high-priority-orders")
            .WithDurable(true)
            .WithDeadLetterExchange("dlx")
            .WithDeadLetterRoutingKey("high-priority.failed")
            .WithMessageTtl(TimeSpan.FromHours(12))
            .WithMaxLength(1000)
            .WithPriority(10)
            .BindToExchange("orders", "order.priority.high")
            .DeclareAsync();
            
        // Create dead letter queue
        await _streamFlow.QueueManager.Queue("dlq")
            .WithDurable(true)
            .BindToExchange("dlx", "order.failed")
            .BindToExchange("dlx", "high-priority.failed")
            .DeclareAsync();
    }
}
```

## 🔄 Advanced Consumption Patterns

### Multi-Type Consumer

```csharp
public class MultiTypeConsumer
{
    private readonly IRabbitMQClient _rabbitMQ;
    private readonly ILogger<MultiTypeConsumer> _logger;

    public MultiTypeConsumer(IRabbitMQClient rabbitMQ, ILogger<MultiTypeConsumer> logger)
    {
        _rabbitMQ = rabbitMQ;
        _logger = logger;
    }

    public async Task StartMultiTypeConsumingAsync(CancellationToken cancellationToken = default)
    {
        // Start multiple consumers concurrently
        var tasks = new[]
        {
            ConsumeOrdersAsync(cancellationToken),
            ConsumeUsersAsync(cancellationToken),
            ConsumeProductsAsync(cancellationToken)
        };

        await Task.WhenAll(tasks);
    }

    private async Task ConsumeOrdersAsync(CancellationToken cancellationToken)
    {
        await _rabbitMQ.Consumer.ConsumeAsync<Order>(
            queueName: "order-processing",
            messageHandler: async (order, context) =>
            {
                await ProcessOrderAsync(order);
                return true;
            },
            cancellationToken: cancellationToken);
    }

    private async Task ConsumeUsersAsync(CancellationToken cancellationToken)
    {
        await _rabbitMQ.Consumer.ConsumeAsync<User>(
            queueName: "user-processing",
            messageHandler: async (user, context) =>
            {
                await ProcessUserAsync(user);
                return true;
            },
            cancellationToken: cancellationToken);
    }

    private async Task ConsumeProductsAsync(CancellationToken cancellationToken)
    {
        await _rabbitMQ.Consumer.ConsumeAsync<Product>(
            queueName: "product-processing",
            messageHandler: async (product, context) =>
            {
                await ProcessProductAsync(product);
                return true;
            },
            cancellationToken: cancellationToken);
    }

    private async Task ProcessOrderAsync(Order order)
    {
        _logger.LogInformation("Processing order {OrderId}", order.Id);
        await Task.Delay(1000);
    }

    private async Task ProcessUserAsync(User user)
    {
        _logger.LogInformation("Processing user {UserId}", user.Id);
        await Task.Delay(500);
    }

    private async Task ProcessProductAsync(Product product)
    {
        _logger.LogInformation("Processing product {ProductId}", product.Id);
        await Task.Delay(800);
    }
}
```

### Pattern-Based Consumer

```csharp
public class PatternBasedConsumer
{
    private readonly IRabbitMQClient _rabbitMQ;
    private readonly ILogger<PatternBasedConsumer> _logger;

    public PatternBasedConsumer(IRabbitMQClient rabbitMQ, ILogger<PatternBasedConsumer> logger)
    {
        _rabbitMQ = rabbitMQ;
        _logger = logger;
    }

    public async Task ConsumeByPatternAsync(CancellationToken cancellationToken = default)
    {
        await _rabbitMQ.Consumer.ConsumeAsync<object>(
            queueName: "pattern-queue",
            messageHandler: async (message, context) =>
            {
                // Route based on routing key pattern
                var handled = context.RoutingKey switch
                {
                    var key when key.StartsWith("order.") => await HandleOrderMessage(message, context),
                    var key when key.StartsWith("user.") => await HandleUserMessage(message, context),
                    var key when key.StartsWith("product.") => await HandleProductMessage(message, context),
                    _ => await HandleUnknownMessage(message, context)
                };

                return handled;
            },
            cancellationToken: cancellationToken);
    }

    private async Task<bool> HandleOrderMessage(object message, MessageContext context)
    {
        _logger.LogInformation("Handling order message with routing key {RoutingKey}", context.RoutingKey);
        
        // Deserialize to appropriate type based on routing key
        var order = context.RoutingKey switch
        {
            "order.created" => JsonSerializer.Deserialize<Order>(message.ToString()),
            "order.updated" => JsonSerializer.Deserialize<Order>(message.ToString()),
            "order.cancelled" => JsonSerializer.Deserialize<Order>(message.ToString()),
            _ => null
        };

        if (order != null)
        {
            await ProcessOrderAsync(order);
            return true;
        }

        return false;
    }

    private async Task<bool> HandleUserMessage(object message, MessageContext context)
    {
        _logger.LogInformation("Handling user message with routing key {RoutingKey}", context.RoutingKey);
        
        // Handle user-related messages
        var user = JsonSerializer.Deserialize<User>(message.ToString());
        if (user != null)
        {
            await ProcessUserAsync(user);
            return true;
        }

        return false;
    }

    private async Task<bool> HandleProductMessage(object message, MessageContext context)
    {
        _logger.LogInformation("Handling product message with routing key {RoutingKey}", context.RoutingKey);
        
        // Handle product-related messages
        var product = JsonSerializer.Deserialize<Product>(message.ToString());
        if (product != null)
        {
            await ProcessProductAsync(product);
            return true;
        }

        return false;
    }

    private async Task<bool> HandleUnknownMessage(object message, MessageContext context)
    {
        _logger.LogWarning("Unknown message type with routing key {RoutingKey}", context.RoutingKey);
        return false; // Reject unknown messages
    }
}
```

## ✅ Message Acknowledgment

### Manual Acknowledgment

```csharp
public class ManualAckConsumer
{
    private readonly IRabbitMQClient _rabbitMQ;
    private readonly ILogger<ManualAckConsumer> _logger;

    public ManualAckConsumer(IRabbitMQClient rabbitMQ, ILogger<ManualAckConsumer> logger)
    {
        _rabbitMQ = rabbitMQ;
        _logger = logger;
    }

    public async Task ConsumeWithManualAckAsync(CancellationToken cancellationToken = default)
    {
        await _rabbitMQ.Consumer.ConsumeAsync<Order>(
            queueName: "order-processing",
            messageHandler: async (order, context) =>
            {
                try
                {
                    // Process the order
                    await ProcessOrderAsync(order);
                    
                    // Explicitly acknowledge the message
                    await context.AckAsync();
                    
                    _logger.LogInformation("Order {OrderId} processed and acknowledged", order.Id);
                    
                    return true;
                }
                catch (ConnectFailureException ex)
                {
                    _logger.LogWarning(ex, "Connection failure processing order {OrderId}. Rejecting for retry", order.Id);
                    
                    // Reject the message and requeue for retry
                    await context.NackAsync(requeue: true);
                    
                    return false;
                }
                catch (PermanentException ex)
                {
                    _logger.LogError(ex, "Permanent error processing order {OrderId}. Rejecting without requeue", order.Id);
                    
                    // Reject the message without requeuing
                    await context.NackAsync(requeue: false);
                    
                    return false;
                }
            },
            cancellationToken: cancellationToken);
    }

    private async Task ProcessOrderAsync(Order order)
    {
        // Simulate processing that might fail
        if (order.Amount < 0)
        {
            throw new PermanentException("Invalid order amount");
        }

        if (DateTime.UtcNow.Second % 10 == 0) // Simulate occasional connection failures
        {
            throw new ConnectFailureException("Temporary service unavailable");
        }

        await Task.Delay(1000);
    }
}
```

### Conditional Acknowledgment

```csharp
public class ConditionalAckConsumer
{
    private readonly IRabbitMQClient _rabbitMQ;
    private readonly ILogger<ConditionalAckConsumer> _logger;

    public ConditionalAckConsumer(IRabbitMQClient rabbitMQ, ILogger<ConditionalAckConsumer> logger)
    {
        _rabbitMQ = rabbitMQ;
        _logger = logger;
    }

    public async Task ConsumeWithConditionalAckAsync(CancellationToken cancellationToken = default)
    {
        await _rabbitMQ.Consumer.ConsumeAsync<Order>(
            queueName: "order-processing",
            messageHandler: async (order, context) =>
            {
                var processingResult = await ProcessOrderAsync(order);
                
                switch (processingResult.Status)
                {
                    case ProcessingStatus.Success:
                        _logger.LogInformation("Order {OrderId} processed successfully", order.Id);
                        return true; // Acknowledge

                    case ProcessingStatus.RetryableFailure:
                        _logger.LogWarning("Order {OrderId} failed with retryable error: {Error}", 
                            order.Id, processingResult.Error);
                        
                        // Check retry count
                        var retryCount = GetRetryCount(context);
                        if (retryCount < 3)
                        {
                            await Task.Delay(TimeSpan.FromSeconds(Math.Pow(2, retryCount))); // Exponential backoff
                            return false; // Requeue for retry
                        }
                        else
                        {
                            _logger.LogError("Order {OrderId} exceeded retry limit", order.Id);
                            await SendToDeadLetterQueue(order, processingResult.Error);
                            return true; // Acknowledge to prevent infinite loop
                        }

                    case ProcessingStatus.PermanentFailure:
                        _logger.LogError("Order {OrderId} failed permanently: {Error}", 
                            order.Id, processingResult.Error);
                        await SendToDeadLetterQueue(order, processingResult.Error);
                        return true; // Acknowledge

                    default:
                        return false; // Requeue
                }
            },
            cancellationToken: cancellationToken);
    }

    private async Task<ProcessingResult> ProcessOrderAsync(Order order)
    {
        try
        {
            // Simulate various processing scenarios
            await Task.Delay(500);
            
            return new ProcessingResult
            {
                Status = ProcessingStatus.Success
            };
        }
        catch (Exception ex)
        {
            return new ProcessingResult
            {
                Status = ex is ConnectFailureException ? ProcessingStatus.RetryableFailure : ProcessingStatus.PermanentFailure,
                Error = ex.Message
            };
        }
    }

    private int GetRetryCount(MessageContext context)
    {
        if (context.Headers.TryGetValue("x-retry-count", out var retryCountObj))
        {
            return Convert.ToInt32(retryCountObj);
        }
        return 0;
    }

    private async Task SendToDeadLetterQueue(Order order, string error)
    {
        // Send to dead letter queue
        await _rabbitMQ.Producer.PublishAsync(
            exchange: "dlx",
            routingKey: "order.failed",
            message: new
            {
                OriginalMessage = order,
                Error = error,
                Timestamp = DateTimeOffset.UtcNow
            });
    }
}

public class ProcessingResult
{
    public ProcessingStatus Status { get; set; }
    public string Error { get; set; } = string.Empty;
}

public enum ProcessingStatus
{
    Success,
    RetryableFailure,
    PermanentFailure
}
```

## 🚨 Error Handling and Retries

### Automatic Retry Consumer

```csharp
public class AutoRetryConsumer
{
    private readonly IRabbitMQClient _rabbitMQ;
    private readonly ILogger<AutoRetryConsumer> _logger;

    public AutoRetryConsumer(IRabbitMQClient rabbitMQ, ILogger<AutoRetryConsumer> logger)
    {
        _rabbitMQ = rabbitMQ;
        _logger = logger;
    }

    public async Task ConsumeWithRetryAsync(CancellationToken cancellationToken = default)
    {
        await _rabbitMQ.Consumer.ConsumeAsync<Order>(
            queueName: "order-processing",
            messageHandler: ProcessOrderWithRetryAsync,
            cancellationToken: cancellationToken);
    }

    private async Task<bool> ProcessOrderWithRetryAsync(Order order, MessageContext context)
    {
        const int maxRetries = 3;
        var retryCount = 0;

        while (retryCount <= maxRetries)
        {
            try
            {
                await ProcessOrderAsync(order);
                
                _logger.LogInformation("Order {OrderId} processed successfully on attempt {Attempt}", 
                    order.Id, retryCount + 1);
                
                return true; // Success
            }
            catch (ConnectFailureException ex)
            {
                retryCount++;
                
                if (retryCount <= maxRetries)
                {
                    var delay = TimeSpan.FromSeconds(Math.Pow(2, retryCount - 1)); // Exponential backoff
                    
                    _logger.LogWarning(ex, "Connection failure processing order {OrderId} on attempt {Attempt}. " +
                        "Retrying in {Delay}s", order.Id, retryCount, delay.TotalSeconds);
                    
                    await Task.Delay(delay);
                }
                else
                {
                    _logger.LogError(ex, "Order {OrderId} failed after {MaxRetries} attempts", 
                        order.Id, maxRetries);
                    
                    await SendToDeadLetterQueue(order, ex.Message);
                    return true; // Acknowledge to prevent infinite loop
                }
            }
            catch (PermanentException ex)
            {
                _logger.LogError(ex, "Permanent error processing order {OrderId}", order.Id);
                
                await SendToDeadLetterQueue(order, ex.Message);
                return true; // Acknowledge
            }
        }

        return false; // Should not reach here
    }

    private async Task ProcessOrderAsync(Order order)
    {
        // Simulate processing with occasional failures
        var random = new Random();
        
        if (random.Next(1, 5) == 1) // 25% chance of connection failure
        {
            throw new ConnectFailureException("Temporary service unavailable");
        }
        
        if (random.Next(1, 20) == 1) // 5% chance of permanent failure
        {
            throw new PermanentException("Invalid order data");
        }
        
        await Task.Delay(1000);
    }

    private async Task SendToDeadLetterQueue(Order order, string error)
    {
        await _rabbitMQ.Producer.PublishAsync(
            exchange: "dlx",
            routingKey: "order.failed",
            message: new
            {
                OriginalMessage = order,
                Error = error,
                Timestamp = DateTimeOffset.UtcNow
            });
    }
}
```

### Circuit Breaker Consumer

```csharp
public class CircuitBreakerConsumer
{
    private readonly IRabbitMQClient _rabbitMQ;
    private readonly ILogger<CircuitBreakerConsumer> _logger;
    private readonly CircuitBreaker _circuitBreaker;

    public CircuitBreakerConsumer(IRabbitMQClient rabbitMQ, ILogger<CircuitBreakerConsumer> logger)
    {
        _rabbitMQ = rabbitMQ;
        _logger = logger;
        _circuitBreaker = new CircuitBreaker(
            failureThreshold: 5,
            recoveryTimeout: TimeSpan.FromMinutes(1));
    }

    public async Task ConsumeWithCircuitBreakerAsync(CancellationToken cancellationToken = default)
    {
        await _rabbitMQ.Consumer.ConsumeAsync<Order>(
            queueName: "order-processing",
            messageHandler: async (order, context) =>
            {
                try
                {
                    await _circuitBreaker.ExecuteAsync(async () =>
                    {
                        await ProcessOrderAsync(order);
                    });

                    _logger.LogInformation("Order {OrderId} processed successfully", order.Id);
                    return true;
                }
                catch (CircuitBreakerOpenException)
                {
                    _logger.LogWarning("Circuit breaker is open. Requeuing order {OrderId}", order.Id);
                    
                    // Requeue the message for later processing
                    await Task.Delay(TimeSpan.FromSeconds(30)); // Wait before requeuing
                    return false;
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error processing order {OrderId}", order.Id);
                    
                    // Send to dead letter queue
                    await SendToDeadLetterQueue(order, ex.Message);
                    return true;
                }
            },
            cancellationToken: cancellationToken);
    }

    private async Task ProcessOrderAsync(Order order)
    {
        // Simulate external service call that might fail
        await Task.Delay(500);
        
        // Simulate occasional failures
        if (DateTime.UtcNow.Millisecond % 3 == 0)
        {
            throw new HttpRequestException("External service unavailable");
        }
    }

    private async Task SendToDeadLetterQueue(Order order, string error)
    {
        await _rabbitMQ.Producer.PublishAsync(
            exchange: "dlx",
            routingKey: "order.failed",
            message: new
            {
                OriginalMessage = order,
                Error = error,
                Timestamp = DateTimeOffset.UtcNow
            });
    }
}
```

## 🔄 Concurrent Processing

### Parallel Consumer

```csharp
public class ParallelConsumer
{
    private readonly IRabbitMQClient _rabbitMQ;
    private readonly ILogger<ParallelConsumer> _logger;
    private readonly SemaphoreSlim _semaphore;

    public ParallelConsumer(IRabbitMQClient rabbitMQ, ILogger<ParallelConsumer> logger)
    {
        _rabbitMQ = rabbitMQ;
        _logger = logger;
        _semaphore = new SemaphoreSlim(10, 10); // Limit concurrent processing
    }

    public async Task ConsumeInParallelAsync(CancellationToken cancellationToken = default)
    {
        await _rabbitMQ.Consumer.ConsumeAsync<Order>(
            queueName: "order-processing",
            messageHandler: ProcessOrderInParallelAsync,
            cancellationToken: cancellationToken);
    }

    private async Task<bool> ProcessOrderInParallelAsync(Order order, MessageContext context)
    {
        await _semaphore.WaitAsync();
        
        try
        {
            // Process order concurrently
            var processingTask = ProcessOrderAsync(order);
            
            // Don't await here to allow parallel processing
            _ = Task.Run(async () =>
            {
                try
                {
                    await processingTask;
                    _logger.LogInformation("Order {OrderId} processed successfully", order.Id);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error processing order {OrderId}", order.Id);
                }
                finally
                {
                    _semaphore.Release();
                }
            });
            
            return true; // Acknowledge immediately
        }
        catch (Exception ex)
        {
            _semaphore.Release();
            _logger.LogError(ex, "Error starting parallel processing for order {OrderId}", order.Id);
            return false;
        }
    }

    private async Task ProcessOrderAsync(Order order)
    {
        // Simulate processing
        await Task.Delay(2000);
        
        // Simulate occasional failures
        if (DateTime.UtcNow.Millisecond % 10 == 0)
        {
            throw new InvalidOperationException("Processing failed");
        }
    }
}
```

### Batch Processing Consumer

```csharp
public class BatchProcessingConsumer
{
    private readonly IRabbitMQClient _rabbitMQ;
    private readonly ILogger<BatchProcessingConsumer> _logger;
    private readonly List<Order> _batch = new();
    private readonly Timer _batchTimer;
    private readonly object _batchLock = new();

    public BatchProcessingConsumer(IRabbitMQClient rabbitMQ, ILogger<BatchProcessingConsumer> logger)
    {
        _rabbitMQ = rabbitMQ;
        _logger = logger;
        
        // Process batch every 5 seconds
        _batchTimer = new Timer(ProcessBatch, null, TimeSpan.FromSeconds(5), TimeSpan.FromSeconds(5));
    }

    public async Task ConsumeForBatchProcessingAsync(CancellationToken cancellationToken = default)
    {
        await _rabbitMQ.Consumer.ConsumeAsync<Order>(
            queueName: "order-processing",
            messageHandler: async (order, context) =>
            {
                lock (_batchLock)
                {
                    _batch.Add(order);
                    
                    // Process batch when it reaches target size
                    if (_batch.Count >= 10)
                    {
                        _ = Task.Run(() => ProcessBatch(null));
                    }
                }
                
                return true; // Acknowledge immediately
            },
            cancellationToken: cancellationToken);
    }

    private void ProcessBatch(object? state)
    {
        List<Order> currentBatch;
        
        lock (_batchLock)
        {
            if (_batch.Count == 0)
                return;
            
            currentBatch = new List<Order>(_batch);
            _batch.Clear();
        }
        
        _ = Task.Run(async () =>
        {
            try
            {
                await ProcessOrderBatchAsync(currentBatch);
                _logger.LogInformation("Processed batch of {Count} orders", currentBatch.Count);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing batch of {Count} orders", currentBatch.Count);
                
                // Handle batch processing error
                await HandleBatchProcessingError(currentBatch, ex);
            }
        });
    }

    private async Task ProcessOrderBatchAsync(List<Order> orders)
    {
        // Simulate batch processing
        await Task.Delay(1000);
        
        foreach (var order in orders)
        {
            // Process each order in the batch
            await ProcessSingleOrderAsync(order);
        }
    }

    private async Task ProcessSingleOrderAsync(Order order)
    {
        // Individual order processing
        await Task.Delay(100);
    }

    private async Task HandleBatchProcessingError(List<Order> orders, Exception ex)
    {
        // Handle batch processing error
        // Could retry individual orders, send to dead letter queue, etc.
        
        foreach (var order in orders)
        {
            await _rabbitMQ.Producer.PublishAsync(
                exchange: "dlx",
                routingKey: "order.failed",
                message: new
                {
                    OriginalMessage = order,
                    Error = ex.Message,
                    Timestamp = DateTimeOffset.UtcNow
                });
        }
    }
}
```

## 🔍 Message Filtering

### Content-Based Filtering

```csharp
public class FilteringConsumer
{
    private readonly IRabbitMQClient _rabbitMQ;
    private readonly ILogger<FilteringConsumer> _logger;

    public FilteringConsumer(IRabbitMQClient rabbitMQ, ILogger<FilteringConsumer> logger)
    {
        _rabbitMQ = rabbitMQ;
        _logger = logger;
    }

    public async Task ConsumeWithFilteringAsync(CancellationToken cancellationToken = default)
    {
        await _rabbitMQ.Consumer.ConsumeAsync<Order>(
            queueName: "order-processing",
            messageHandler: async (order, context) =>
            {
                // Content-based filtering
                if (!ShouldProcessOrder(order))
                {
                    _logger.LogInformation("Skipping order {OrderId} due to filtering rules", order.Id);
                    return true; // Acknowledge but don't process
                }

                // Header-based filtering
                if (!ShouldProcessBasedOnHeaders(context))
                {
                    _logger.LogInformation("Skipping order {OrderId} due to header filtering", order.Id);
                    return true; // Acknowledge but don't process
                }

                // Routing key-based filtering
                if (!ShouldProcessBasedOnRoutingKey(context))
                {
                    _logger.LogInformation("Skipping order {OrderId} due to routing key filtering", order.Id);
                    return true; // Acknowledge but don't process
                }

                // Process the order
                await ProcessOrderAsync(order);
                return true;
            },
            cancellationToken: cancellationToken);
    }

    private bool ShouldProcessOrder(Order order)
    {
        // Business rules for order processing
        return order.Amount > 0 && 
               order.CustomerId > 0 && 
               order.Items.Count > 0 &&
               order.Status == OrderStatus.Pending;
    }

    private bool ShouldProcessBasedOnHeaders(MessageContext context)
    {
        // Check source header
        if (context.Headers.TryGetValue("source", out var source))
        {
            var sourceStr = source.ToString();
            if (sourceStr == "legacy-system")
            {
                return false; // Skip legacy system messages
            }
        }

        // Check priority header
        if (context.Headers.TryGetValue("priority", out var priority))
        {
            var priorityInt = Convert.ToInt32(priority);
            if (priorityInt < 3)
            {
                return false; // Skip low priority messages
            }
        }

        return true;
    }

    private bool ShouldProcessBasedOnRoutingKey(MessageContext context)
    {
        // Process only certain routing key patterns
        var routingKey = context.RoutingKey;
        
        return routingKey.StartsWith("order.created") ||
               routingKey.StartsWith("order.updated") ||
               routingKey.StartsWith("order.priority");
    }

    private async Task ProcessOrderAsync(Order order)
    {
        _logger.LogInformation("Processing filtered order {OrderId}", order.Id);
        await Task.Delay(1000);
    }
}
```

### Dynamic Filtering

```csharp
public class DynamicFilteringConsumer
{
    private readonly IRabbitMQClient _rabbitMQ;
    private readonly ILogger<DynamicFilteringConsumer> _logger;
    private readonly IConfiguration _configuration;

    public DynamicFilteringConsumer(IRabbitMQClient rabbitMQ, ILogger<DynamicFilteringConsumer> logger, IConfiguration configuration)
    {
        _rabbitMQ = rabbitMQ;
        _logger = logger;
        _configuration = configuration;
    }

    public async Task ConsumeWithDynamicFilteringAsync(CancellationToken cancellationToken = default)
    {
        await _rabbitMQ.Consumer.ConsumeAsync<Order>(
            queueName: "order-processing",
            messageHandler: async (order, context) =>
            {
                // Dynamic filtering based on configuration
                var filters = LoadFilters();
                
                if (!ApplyFilters(order, context, filters))
                {
                    _logger.LogInformation("Order {OrderId} filtered out by dynamic rules", order.Id);
                    return true; // Acknowledge but don't process
                }

                await ProcessOrderAsync(order);
                return true;
            },
            cancellationToken: cancellationToken);
    }

    private List<MessageFilter> LoadFilters()
    {
        // Load filters from configuration
        var filterSection = _configuration.GetSection("MessageFilters");
        return filterSection.Get<List<MessageFilter>>() ?? new List<MessageFilter>();
    }

    private bool ApplyFilters(Order order, MessageContext context, List<MessageFilter> filters)
    {
        foreach (var filter in filters)
        {
            if (!ApplyFilter(order, context, filter))
            {
                return false;
            }
        }
        return true;
    }

    private bool ApplyFilter(Order order, MessageContext context, MessageFilter filter)
    {
        return filter.Type switch
        {
            FilterType.AmountRange => order.Amount >= filter.MinAmount && order.Amount <= filter.MaxAmount,
            FilterType.CustomerType => filter.AllowedCustomerTypes.Contains(order.CustomerType),
            FilterType.RoutingKeyPattern => Regex.IsMatch(context.RoutingKey, filter.Pattern),
            FilterType.HeaderValue => context.Headers.TryGetValue(filter.HeaderName, out var value) && 
                                    value.ToString() == filter.HeaderValue,
            _ => true
        };
    }

    private async Task ProcessOrderAsync(Order order)
    {
        _logger.LogInformation("Processing dynamically filtered order {OrderId}", order.Id);
        await Task.Delay(1000);
    }
}

public class MessageFilter
{
    public FilterType Type { get; set; }
    public decimal MinAmount { get; set; }
    public decimal MaxAmount { get; set; }
    public List<string> AllowedCustomerTypes { get; set; } = new();
    public string Pattern { get; set; } = string.Empty;
    public string HeaderName { get; set; } = string.Empty;
    public string HeaderValue { get; set; } = string.Empty;
}

public enum FilterType
{
    AmountRange,
    CustomerType,
    RoutingKeyPattern,
    HeaderValue
}
```

## 💀 Dead Letter Queues

### Dead Letter Queue Processing

```csharp
public class DeadLetterConsumer
{
    private readonly IRabbitMQClient _rabbitMQ;
    private readonly ILogger<DeadLetterConsumer> _logger;

    public DeadLetterConsumer(IRabbitMQClient rabbitMQ, ILogger<DeadLetterConsumer> logger)
    {
        _rabbitMQ = rabbitMQ;
        _logger = logger;
    }

    public async Task ProcessDeadLetterQueueAsync(CancellationToken cancellationToken = default)
    {
        await _rabbitMQ.Consumer.ConsumeAsync<object>(
            queueName: "dlq",
            messageHandler: async (message, context) =>
            {
                _logger.LogWarning("Processing dead letter message: {MessageId}", context.MessageId);
                
                // Extract death information from headers
                var deathInfo = ExtractDeathInformation(context);
                
                // Log death information
                _logger.LogError("Message died: " +
                    "OriginalExchange={OriginalExchange}, " +
                    "OriginalRoutingKey={OriginalRoutingKey}, " +
                    "DeathReason={DeathReason}, " +
                    "DeathCount={DeathCount}",
                    deathInfo.OriginalExchange,
                    deathInfo.OriginalRoutingKey,
                    deathInfo.DeathReason,
                    deathInfo.DeathCount);

                // Handle based on death reason
                var handled = deathInfo.DeathReason switch
                {
                    "rejected" => await HandleRejectedMessage(message, context, deathInfo),
                    "expired" => await HandleExpiredMessage(message, context, deathInfo),
                    "maxlen" => await HandleMaxLengthMessage(message, context, deathInfo),
                    _ => await HandleUnknownDeathReason(message, context, deathInfo)
                };

                return handled;
            },
            cancellationToken: cancellationToken);
    }

    private DeathInformation ExtractDeathInformation(MessageContext context)
    {
        var deathInfo = new DeathInformation();
        
        if (context.Headers.TryGetValue("x-death", out var xDeathObj) && xDeathObj is List<object> xDeath)
        {
            if (xDeath.Count > 0 && xDeath[0] is Dictionary<string, object> firstDeath)
            {
                if (firstDeath.TryGetValue("exchange", out var exchange))
                    deathInfo.OriginalExchange = exchange.ToString();
                
                if (firstDeath.TryGetValue("routing-keys", out var routingKeys) && routingKeys is List<object> keys)
                    deathInfo.OriginalRoutingKey = keys.FirstOrDefault()?.ToString() ?? "";
                
                if (firstDeath.TryGetValue("reason", out var reason))
                    deathInfo.DeathReason = reason.ToString();
                
                if (firstDeath.TryGetValue("count", out var count))
                    deathInfo.DeathCount = Convert.ToInt32(count);
            }
        }
        
        return deathInfo;
    }

    private async Task<bool> HandleRejectedMessage(object message, MessageContext context, DeathInformation deathInfo)
    {
        _logger.LogInformation("Handling rejected message");
        
        // Could attempt to reprocess, send to another queue, or archive
        await ArchiveMessage(message, context, deathInfo);
        return true;
    }

    private async Task<bool> HandleExpiredMessage(object message, MessageContext context, DeathInformation deathInfo)
    {
        _logger.LogInformation("Handling expired message");
        
        // Could attempt to reprocess with new TTL, or archive
        await ArchiveMessage(message, context, deathInfo);
        return true;
    }

    private async Task<bool> HandleMaxLengthMessage(object message, MessageContext context, DeathInformation deathInfo)
    {
        _logger.LogInformation("Handling max length exceeded message");
        
        // Could send to alternative queue or archive
        await ArchiveMessage(message, context, deathInfo);
        return true;
    }

    private async Task<bool> HandleUnknownDeathReason(object message, MessageContext context, DeathInformation deathInfo)
    {
        _logger.LogWarning("Unknown death reason: {DeathReason}", deathInfo.DeathReason);
        
        await ArchiveMessage(message, context, deathInfo);
        return true;
    }

    private async Task ArchiveMessage(object message, MessageContext context, DeathInformation deathInfo)
    {
        // Archive message for manual review
        var archiveMessage = new
        {
            OriginalMessage = message,
            DeathInformation = deathInfo,
            Context = new
            {
                context.MessageId,
                context.CorrelationId,
                context.Exchange,
                context.RoutingKey,
                context.Headers
            },
            ArchivedAt = DateTimeOffset.UtcNow
        };

        await _rabbitMQ.Producer.PublishAsync(
            exchange: "archive",
            routingKey: "dead-letter.archived",
            message: archiveMessage);
    }
}

public class DeathInformation
{
    public string OriginalExchange { get; set; } = string.Empty;
    public string OriginalRoutingKey { get; set; } = string.Empty;
    public string DeathReason { get; set; } = string.Empty;
    public int DeathCount { get; set; }
}
```

## ⚡ Performance Optimization

### High-Throughput Consumer

```csharp
public class HighThroughputConsumer
{
    private readonly IRabbitMQClient _rabbitMQ;
    private readonly ILogger<HighThroughputConsumer> _logger;
    private readonly Channel<ProcessingItem> _processingChannel;
    private readonly SemaphoreSlim _processingLimiter;

    public HighThroughputConsumer(IRabbitMQClient rabbitMQ, ILogger<HighThroughputConsumer> logger)
    {
        _rabbitMQ = rabbitMQ;
        _logger = logger;
        _processingChannel = Channel.CreateBounded<ProcessingItem>(new BoundedChannelOptions(1000)
        {
            FullMode = BoundedChannelFullMode.Wait,
            SingleReader = false,
            SingleWriter = false
        });
        _processingLimiter = new SemaphoreSlim(Environment.ProcessorCount * 2, Environment.ProcessorCount * 2);
        
        // Start background processors
        for (int i = 0; i < Environment.ProcessorCount; i++)
        {
            _ = Task.Run(ProcessItemsAsync);
        }
    }

    public async Task ConsumeHighThroughputAsync(CancellationToken cancellationToken = default)
    {
        await _rabbitMQ.Consumer.ConsumeAsync<Order>(
            queueName: "high-throughput-orders",
            messageHandler: async (order, context) =>
            {
                // Queue for async processing
                var processingItem = new ProcessingItem
                {
                    Order = order,
                    Context = context,
                    ReceivedAt = DateTimeOffset.UtcNow
                };

                await _processingChannel.Writer.WriteAsync(processingItem);
                
                return true; // Acknowledge immediately
            },
            cancellationToken: cancellationToken);
    }

    private async Task ProcessItemsAsync()
    {
        await foreach (var item in _processingChannel.Reader.ReadAllAsync())
        {
            await _processingLimiter.WaitAsync();
            
            _ = Task.Run(async () =>
            {
                try
                {
                    await ProcessOrderAsync(item.Order);
                    
                    var processingTime = DateTimeOffset.UtcNow - item.ReceivedAt;
                    _logger.LogInformation("Order {OrderId} processed in {ProcessingTime}ms", 
                        item.Order.Id, processingTime.TotalMilliseconds);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error processing order {OrderId}", item.Order.Id);
                }
                finally
                {
                    _processingLimiter.Release();
                }
            });
        }
    }

    private async Task ProcessOrderAsync(Order order)
    {
        // High-performance processing
        await Task.Delay(50); // Simulate fast processing
    }
}

public class ProcessingItem
{
    public Order Order { get; set; } = null!;
    public MessageContext Context { get; set; } = null!;
    public DateTimeOffset ReceivedAt { get; set; }
}
```

## 📊 Monitoring and Metrics

### Consumer Metrics

```csharp
public class MonitoredConsumer
{
    private readonly IRabbitMQClient _rabbitMQ;
    private readonly ILogger<MonitoredConsumer> _logger;
    private long _processedCount = 0;
    private long _failedCount = 0;
    private readonly Timer _metricsTimer;

    public MonitoredConsumer(IRabbitMQClient rabbitMQ, ILogger<MonitoredConsumer> logger)
    {
        _rabbitMQ = rabbitMQ;
        _logger = logger;
        
        // Log metrics every minute
        _metricsTimer = new Timer(LogMetrics, null, TimeSpan.FromMinutes(1), TimeSpan.FromMinutes(1));
    }

    public async Task ConsumeWithMonitoringAsync(CancellationToken cancellationToken = default)
    {
        await _rabbitMQ.Consumer.ConsumeAsync<Order>(
            queueName: "order-processing",
            messageHandler: async (order, context) =>
            {
                var stopwatch = Stopwatch.StartNew();
                
                try
                {
                    await ProcessOrderAsync(order);
                    
                    stopwatch.Stop();
                    Interlocked.Increment(ref _processedCount);
                    
                    _logger.LogInformation("Order {OrderId} processed in {ElapsedMs}ms", 
                        order.Id, stopwatch.ElapsedMilliseconds);
                    
                    // Record metrics
                    RecordProcessingMetrics(true, stopwatch.ElapsedMilliseconds);
                    
                    return true;
                }
                catch (Exception ex)
                {
                    stopwatch.Stop();
                    Interlocked.Increment(ref _failedCount);
                    
                    _logger.LogError(ex, "Error processing order {OrderId} after {ElapsedMs}ms", 
                        order.Id, stopwatch.ElapsedMilliseconds);
                    
                    // Record metrics
                    RecordProcessingMetrics(false, stopwatch.ElapsedMilliseconds);
                    
                    return false;
                }
            },
            cancellationToken: cancellationToken);
    }

    private async Task ProcessOrderAsync(Order order)
    {
        // Simulate processing
        await Task.Delay(Random.Shared.Next(100, 2000));
        
        // Simulate occasional failures
        if (Random.Shared.Next(1, 10) == 1)
        {
            throw new InvalidOperationException("Processing failed");
        }
    }

    private void RecordProcessingMetrics(bool success, long elapsedMs)
    {
        // Record metrics in your monitoring system
        // Examples: Application Insights, Prometheus, CloudWatch, etc.
        
        if (success)
        {
            // Record success metrics
            // _metricsClient.Increment("order.processed.success");
            // _metricsClient.Histogram("order.processing.duration", elapsedMs);
        }
        else
        {
            // Record failure metrics
            // _metricsClient.Increment("order.processed.failure");
            // _metricsClient.Histogram("order.processing.duration.failed", elapsedMs);
        }
    }

    private void LogMetrics(object? state)
    {
        var processed = Interlocked.Read(ref _processedCount);
        var failed = Interlocked.Read(ref _failedCount);
        var total = processed + failed;
        var successRate = total > 0 ? (double)processed / total * 100 : 0;

        _logger.LogInformation("Consumer Metrics: " +
            "Processed={Processed}, " +
            "Failed={Failed}, " +
            "Total={Total}, " +
            "SuccessRate={SuccessRate:F2}%",
            processed, failed, total, successRate);

        // Get consumer statistics
        var stats = _rabbitMQ.Consumer.Statistics;
        _logger.LogInformation("Consumer Statistics: " +
            "Status={Status}, " +
            "MessagesProcessed={MessagesProcessed}, " +
            "MessagesAcknowledged={MessagesAcknowledged}, " +
            "MessagesRejected={MessagesRejected}",
            stats.Status,
            stats.MessagesProcessed,
            stats.MessagesAcknowledged,
            stats.MessagesRejected);
    }
}
```

## 🎯 Best Practices

### 1. Always Use Manual Acknowledgment for Critical Messages

```csharp
// DO: Use manual acknowledgment for critical processing
await _rabbitMQ.Consumer.ConsumeAsync<Order>(
    queueName: "critical-orders",
    messageHandler: async (order, context) =>
    {
        try
        {
            await ProcessCriticalOrderAsync(order);
            return true; // Acknowledge on success
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Critical order processing failed");
            return false; // Reject on failure
        }
    });

// DON'T: Use auto-acknowledgment for critical messages
// (This acknowledges before processing, risking message loss)
```

### 2. Implement Idempotent Message Processing

```csharp
// DO: Implement idempotent processing
public async Task<bool> ProcessOrderIdempotentAsync(Order order, MessageContext context)
{
    // Check if already processed
    if (await _orderRepository.IsProcessedAsync(order.Id))
    {
        _logger.LogInformation("Order {OrderId} already processed, skipping", order.Id);
        return true; // Already processed, acknowledge
    }

    // Process the order
    await ProcessOrderAsync(order);
    
    // Mark as processed
    await _orderRepository.MarkAsProcessedAsync(order.Id);
    
    return true;
}
```

### 3. Use Proper Error Classification

```csharp
// DO: Classify errors properly
public async Task<bool> ProcessOrderWithErrorClassificationAsync(Order order, MessageContext context)
{
    try
    {
        await ProcessOrderAsync(order);
        return true;
    }
    catch (ValidationException ex)
    {
        // Permanent error - don't retry
        _logger.LogError(ex, "Order validation failed");
        await SendToDeadLetterQueue(order, ex.Message);
        return true; // Acknowledge to avoid retry
    }
    catch (HttpRequestException ex)
    {
        // Transient error - retry
        _logger.LogWarning(ex, "HTTP request failed, will retry");
        return false; // Reject for retry
    }
    catch (Exception ex)
    {
        // Unknown error - log and reject
        _logger.LogError(ex, "Unknown error processing order");
        return false; // Reject for retry
    }
}
```

### 4. Set Appropriate Prefetch Count

```csharp
// DO: Set appropriate prefetch count based on processing speed
builder.Services.AddRabbitMQ()
    .WithConsumer(config =>
    {
        // For fast processing (< 100ms per message)
        config.PrefetchCount = 100;
        
        // For slow processing (> 1s per message)
        // config.PrefetchCount = 1;
        
        // For medium processing (100ms - 1s per message)
        // config.PrefetchCount = 10;
    })
    .Build();
```

### 5. Monitor Consumer Health

```csharp
// DO: Implement consumer health checks
public class ConsumerHealthCheck : IHealthCheck
{
    private readonly IRabbitMQClient _rabbitMQ;

    public ConsumerHealthCheck(IRabbitMQClient rabbitMQ) => _rabbitMQ = rabbitMQ;

    public Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
    {
        var isHealthy = _rabbitMQ.Consumer.Status == ConsumerStatus.Running;
        var stats = _rabbitMQ.Consumer.Statistics;
        
        var data = new Dictionary<string, object>
        {
            ["status"] = _rabbitMQ.Consumer.Status.ToString(),
            ["messages_processed"] = stats.MessagesProcessed,
            ["messages_acknowledged"] = stats.MessagesAcknowledged,
            ["messages_rejected"] = stats.MessagesRejected
        };

        return Task.FromResult(isHealthy 
            ? HealthCheckResult.Healthy("Consumer is healthy", data)
            : HealthCheckResult.Unhealthy("Consumer is not running", data: data));
    }
}
```

### 6. Use Message Filtering to Reduce Processing Load

```csharp
// DO: Filter messages early to reduce processing load
public async Task<bool> ProcessOrderWithFilteringAsync(Order order, MessageContext context)
{
    // Early filtering
    if (order.Amount <= 0)
    {
        _logger.LogInformation("Skipping order with invalid amount: {Amount}", order.Amount);
        return true; // Acknowledge but don't process
    }

    if (context.Headers.TryGetValue("test-mode", out var testMode) && testMode.ToString() == "true")
    {
        _logger.LogInformation("Skipping test message");
        return true; // Acknowledge but don't process
    }

    // Process the order
    await ProcessOrderAsync(order);
    return true;
}
```

### 7. Implement Circuit Breaker for External Dependencies

```csharp
// DO: Use circuit breaker for external service calls
public class ExternalServiceConsumer
{
    private readonly IRabbitMQClient _rabbitMQ;
    private readonly ILogger<ExternalServiceConsumer> _logger;
    private readonly CircuitBreaker _circuitBreaker;

    public ExternalServiceConsumer(IRabbitMQClient rabbitMQ, ILogger<ExternalServiceConsumer> logger)
    {
        _rabbitMQ = rabbitMQ;
        _logger = logger;
        _circuitBreaker = new CircuitBreaker(
            failureThreshold: 5,
            recoveryTimeout: TimeSpan.FromMinutes(1));
    }

    public async Task<bool> ProcessOrderWithCircuitBreakerAsync(Order order, MessageContext context)
    {
        try
        {
            await _circuitBreaker.ExecuteAsync(async () =>
            {
                await CallExternalServiceAsync(order);
            });
            
            return true;
        }
        catch (CircuitBreakerOpenException)
        {
            _logger.LogWarning("Circuit breaker open, requeuing order {OrderId}", order.Id);
            return false; // Requeue for later processing
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing order {OrderId}", order.Id);
            return false; // Requeue for retry
        }
    }

    private async Task CallExternalServiceAsync(Order order)
    {
        // External service call
        await Task.Delay(100);
    }
}
```

## 🎉 Summary

You've now mastered FS.StreamFlow Consumer functionality:

✅ **Basic and advanced consumption patterns**  
✅ **Message acknowledgment strategies**  
✅ **Comprehensive error handling and retries**  
✅ **Concurrent and parallel processing**  
✅ **Message filtering techniques**  
✅ **Dead letter queue handling**  
✅ **Performance optimization**  
✅ **Monitoring and metrics**  
✅ **Production-ready best practices**  

## 🎯 Next Steps

Continue your FS.StreamFlow journey:

- [Event-Driven Architecture](event-driven.md) - Build event-driven systems
- [Error Handling](error-handling.md) - Master error handling patterns
- [Performance Tuning](performance.md) - Optimize for high throughput
- [Monitoring](monitoring.md) - Monitor your messaging system
- [Examples](examples/) - See real-world examples 