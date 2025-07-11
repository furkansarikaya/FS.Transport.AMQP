# Error Handling Guide

This guide covers comprehensive error handling strategies in FS.RabbitMQ, from basic retry mechanisms to advanced resilience patterns.

## 📋 Table of Contents

1. [Error Handling Overview](#error-handling-overview)
2. [Retry Policies](#retry-policies)
3. [Circuit Breaker Pattern](#circuit-breaker-pattern)
4. [Dead Letter Queues](#dead-letter-queues)
5. [Error Classification](#error-classification)
6. [Custom Error Handlers](#custom-error-handlers)
7. [Monitoring and Alerting](#monitoring-and-alerting)
8. [Best Practices](#best-practices)

## 🎯 Error Handling Overview

FS.RabbitMQ provides multiple layers of error handling to ensure message processing reliability:

### Error Handling Layers

1. **Producer Error Handling**: Publication failures, confirmations, and retries
2. **Consumer Error Handling**: Processing failures, acknowledgments, and requeues
3. **Connection Error Handling**: Connection failures and automatic recovery
4. **Application Error Handling**: Business logic errors and custom handling

### Error Handling Configuration

```csharp
builder.Services.AddRabbitMQ()
    .WithErrorHandling(config =>
    {
        config.EnableDeadLetterQueue = true;
        config.DeadLetterExchange = "dlx";
        config.DeadLetterQueue = "dlq";
        config.RetryPolicy = RetryPolicyType.ExponentialBackoff;
        config.MaxRetryAttempts = 3;
        config.RetryDelay = TimeSpan.FromSeconds(1);
        config.MaxRetryDelay = TimeSpan.FromMinutes(5);
        config.EnableCircuitBreaker = true;
        config.CircuitBreakerFailureThreshold = 5;
        config.CircuitBreakerRecoveryTimeout = TimeSpan.FromMinutes(1);
    })
    .Build();
```

## 🔄 Retry Policies

### Built-in Retry Policies

FS.RabbitMQ provides several built-in retry policies:

#### 1. Linear Retry Policy

```csharp
public class LinearRetryExample
{
    private readonly IRabbitMQClient _rabbitMQ;
    private readonly ILogger<LinearRetryExample> _logger;

    public LinearRetryExample(IRabbitMQClient rabbitMQ, ILogger<LinearRetryExample> logger)
    {
        _rabbitMQ = rabbitMQ;
        _logger = logger;
    }

    public async Task ProcessWithLinearRetryAsync(Order order)
    {
        var retryPolicy = new LinearRetryPolicy(
            maxAttempts: 3,
            retryDelay: TimeSpan.FromSeconds(2));

        await retryPolicy.ExecuteAsync(async () =>
        {
            await ProcessOrderAsync(order);
        });
    }

    private async Task ProcessOrderAsync(Order order)
    {
        // Simulate processing that might fail
        await Task.Delay(100);
        
        if (Random.Shared.Next(1, 3) == 1)
        {
            throw new InvalidOperationException("Processing failed");
        }
    }
}
```

#### 2. Exponential Backoff Retry Policy

```csharp
public class ExponentialBackoffRetryExample
{
    private readonly IRabbitMQClient _rabbitMQ;
    private readonly ILogger<ExponentialBackoffRetryExample> _logger;

    public ExponentialBackoffRetryExample(IRabbitMQClient rabbitMQ, ILogger<ExponentialBackoffRetryExample> logger)
    {
        _rabbitMQ = rabbitMQ;
        _logger = logger;
    }

    public async Task ProcessWithExponentialBackoffAsync(Order order)
    {
        var retryPolicy = new ExponentialBackoffRetryPolicy(
            maxAttempts: 5,
            initialDelay: TimeSpan.FromSeconds(1),
            maxDelay: TimeSpan.FromMinutes(5),
            backoffMultiplier: 2.0);

        await retryPolicy.ExecuteAsync(async () =>
        {
            await ProcessOrderAsync(order);
        });
    }

    private async Task ProcessOrderAsync(Order order)
    {
        // Simulate processing
        await Task.Delay(100);
        
        if (Random.Shared.Next(1, 4) == 1)
        {
            throw new HttpRequestException("External service unavailable");
        }
    }
}
```

### Custom Retry Policy

```csharp
public class CustomRetryPolicy : IRetryPolicy
{
    private readonly int _maxAttempts;
    private readonly TimeSpan _initialDelay;
    private readonly double _backoffMultiplier;
    private readonly TimeSpan _maxDelay;
    private readonly ILogger<CustomRetryPolicy> _logger;

    public CustomRetryPolicy(
        int maxAttempts,
        TimeSpan initialDelay,
        double backoffMultiplier,
        TimeSpan maxDelay,
        ILogger<CustomRetryPolicy> logger)
    {
        _maxAttempts = maxAttempts;
        _initialDelay = initialDelay;
        _backoffMultiplier = backoffMultiplier;
        _maxDelay = maxDelay;
        _logger = logger;
    }

    public async Task<bool> ShouldRetryAsync(Exception exception, int attemptNumber, TimeSpan elapsed)
    {
        // Don't retry if max attempts reached
        if (attemptNumber >= _maxAttempts)
        {
            _logger.LogError("Max retry attempts ({MaxAttempts}) reached for exception: {Exception}", 
                _maxAttempts, exception.Message);
            return false;
        }

        // Don't retry permanent errors
        if (IsPermanentError(exception))
        {
            _logger.LogError("Permanent error detected, not retrying: {Exception}", exception.Message);
            return false;
        }

        // Calculate delay with jitter
        var delay = CalculateDelay(attemptNumber);
        
        _logger.LogWarning("Retrying in {Delay}ms (attempt {Attempt}/{MaxAttempts}): {Exception}", 
            delay.TotalMilliseconds, attemptNumber + 1, _maxAttempts, exception.Message);

        await Task.Delay(delay);
        return true;
    }

    private bool IsPermanentError(Exception exception)
    {
        return exception switch
        {
            ArgumentException => true,
            ArgumentNullException => true,
            InvalidOperationException => true,
            NotSupportedException => true,
            ValidationException => true,
            _ => false
        };
    }

    private TimeSpan CalculateDelay(int attemptNumber)
    {
        var delay = TimeSpan.FromMilliseconds(_initialDelay.TotalMilliseconds * Math.Pow(_backoffMultiplier, attemptNumber));
        
        // Add jitter to prevent thundering herd
        var jitter = TimeSpan.FromMilliseconds(Random.Shared.Next(0, (int)(delay.TotalMilliseconds * 0.1)));
        delay = delay.Add(jitter);
        
        // Cap at maximum delay
        return delay > _maxDelay ? _maxDelay : delay;
    }
}
```

### Using Retry Policies in Consumers

```csharp
public class RetryConsumer
{
    private readonly IRabbitMQClient _rabbitMQ;
    private readonly IRetryPolicy _retryPolicy;
    private readonly ILogger<RetryConsumer> _logger;

    public RetryConsumer(IRabbitMQClient rabbitMQ, IRetryPolicy retryPolicy, ILogger<RetryConsumer> logger)
    {
        _rabbitMQ = rabbitMQ;
        _retryPolicy = retryPolicy;
        _logger = logger;
    }

    public async Task ConsumeWithRetryAsync(CancellationToken cancellationToken = default)
    {
        await _rabbitMQ.Consumer.ConsumeAsync<Order>(
            queueName: "order-processing",
            messageHandler: async (order, context) =>
            {
                try
                {
                    await _retryPolicy.ExecuteAsync(async () =>
                    {
                        await ProcessOrderAsync(order);
                    });
                    
                    return true; // Success
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Failed to process order {OrderId} after all retry attempts", order.Id);
                    
                    // Send to dead letter queue
                    await SendToDeadLetterQueue(order, ex);
                    
                    return true; // Acknowledge to prevent infinite retry
                }
            },
            cancellationToken: cancellationToken);
    }

    private async Task ProcessOrderAsync(Order order)
    {
        // Simulate processing
        await Task.Delay(100);
        
        if (Random.Shared.Next(1, 3) == 1)
        {
            throw new InvalidOperationException("Processing failed");
        }
    }

    private async Task SendToDeadLetterQueue(Order order, Exception exception)
    {
        await _rabbitMQ.Producer.PublishAsync(
            exchange: "dlx",
            routingKey: "order.failed",
            message: new
            {
                OriginalMessage = order,
                Error = exception.Message,
                StackTrace = exception.StackTrace,
                Timestamp = DateTimeOffset.UtcNow
            });
    }
}
```

## 🔌 Circuit Breaker Pattern

The circuit breaker pattern prevents cascading failures by stopping calls to failing services.

### Circuit Breaker States

1. **Closed**: Normal operation, calls pass through
2. **Open**: Failure threshold reached, calls fail immediately  
3. **Half-Open**: Testing if service has recovered

### Basic Circuit Breaker Implementation

```csharp
public class CircuitBreakerExample
{
    private readonly IRabbitMQClient _rabbitMQ;
    private readonly ICircuitBreaker _circuitBreaker;
    private readonly ILogger<CircuitBreakerExample> _logger;

    public CircuitBreakerExample(IRabbitMQClient rabbitMQ, ILogger<CircuitBreakerExample> logger)
    {
        _rabbitMQ = rabbitMQ;
        _logger = logger;
        _circuitBreaker = new CircuitBreaker(
            failureThreshold: 5,
            recoveryTimeout: TimeSpan.FromMinutes(1),
            samplingDuration: TimeSpan.FromSeconds(60));
    }

    public async Task ProcessWithCircuitBreakerAsync(Order order)
    {
        try
        {
            await _circuitBreaker.ExecuteAsync(async () =>
            {
                await CallExternalServiceAsync(order);
            });
            
            _logger.LogInformation("Order {OrderId} processed successfully", order.Id);
        }
        catch (CircuitBreakerOpenException)
        {
            _logger.LogWarning("Circuit breaker is open, cannot process order {OrderId}", order.Id);
            
            // Handle circuit breaker open state
            await HandleCircuitBreakerOpen(order);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing order {OrderId}", order.Id);
            throw;
        }
    }

    private async Task CallExternalServiceAsync(Order order)
    {
        // Simulate external service call
        await Task.Delay(100);
        
        if (Random.Shared.Next(1, 3) == 1)
        {
            throw new HttpRequestException("External service unavailable");
        }
    }

    private async Task HandleCircuitBreakerOpen(Order order)
    {
        // Options when circuit breaker is open:
        // 1. Return cached/default response
        // 2. Queue for later processing
        // 3. Use alternative service
        // 4. Reject the request
        
        _logger.LogInformation("Queueing order {OrderId} for later processing", order.Id);
        
        await _rabbitMQ.Producer.PublishAsync(
            exchange: "delayed-processing",
            routingKey: "order.delayed",
            message: order);
    }
}
```

### Advanced Circuit Breaker with State Events

```csharp
public class AdvancedCircuitBreakerExample
{
    private readonly IRabbitMQClient _rabbitMQ;
    private readonly ICircuitBreaker _circuitBreaker;
    private readonly ILogger<AdvancedCircuitBreakerExample> _logger;

    public AdvancedCircuitBreakerExample(IRabbitMQClient rabbitMQ, ILogger<AdvancedCircuitBreakerExample> logger)
    {
        _rabbitMQ = rabbitMQ;
        _logger = logger;
        
        _circuitBreaker = new CircuitBreaker(
            failureThreshold: 3,
            recoveryTimeout: TimeSpan.FromSeconds(30),
            samplingDuration: TimeSpan.FromSeconds(60));
        
        // Subscribe to circuit breaker state changes
        _circuitBreaker.StateChanged += OnCircuitBreakerStateChanged;
    }

    private async Task OnCircuitBreakerStateChanged(CircuitBreakerStateChangedEventArgs args)
    {
        _logger.LogWarning("Circuit breaker state changed from {OldState} to {NewState}. Reason: {Reason}", 
            args.OldState, args.NewState, args.Reason);
        
        // Notify monitoring system
        await NotifyMonitoringSystem(args);
        
        // Take action based on state
        switch (args.NewState)
        {
            case CircuitBreakerState.Open:
                await HandleCircuitBreakerOpened(args);
                break;
            case CircuitBreakerState.HalfOpen:
                await HandleCircuitBreakerHalfOpen(args);
                break;
            case CircuitBreakerState.Closed:
                await HandleCircuitBreakerClosed(args);
                break;
        }
    }

    private async Task HandleCircuitBreakerOpened(CircuitBreakerStateChangedEventArgs args)
    {
        _logger.LogError("Circuit breaker opened due to failures. Switching to degraded mode.");
        
        // Publish alert
        await _rabbitMQ.Producer.PublishAsync(
            exchange: "alerts",
            routingKey: "circuit-breaker.opened",
            message: new
            {
                ServiceName = "OrderProcessingService",
                Timestamp = DateTimeOffset.UtcNow,
                Reason = args.Reason,
                FailureCount = args.FailureCount
            });
    }

    private async Task HandleCircuitBreakerHalfOpen(CircuitBreakerStateChangedEventArgs args)
    {
        _logger.LogInformation("Circuit breaker is half-open. Testing service recovery.");
        
        // Optionally notify that we're testing recovery
        await _rabbitMQ.Producer.PublishAsync(
            exchange: "alerts",
            routingKey: "circuit-breaker.testing",
            message: new
            {
                ServiceName = "OrderProcessingService",
                Timestamp = DateTimeOffset.UtcNow,
                Message = "Testing service recovery"
            });
    }

    private async Task HandleCircuitBreakerClosed(CircuitBreakerStateChangedEventArgs args)
    {
        _logger.LogInformation("Circuit breaker closed. Service has recovered.");
        
        // Publish recovery notification
        await _rabbitMQ.Producer.PublishAsync(
            exchange: "alerts",
            routingKey: "circuit-breaker.recovered",
            message: new
            {
                ServiceName = "OrderProcessingService",
                Timestamp = DateTimeOffset.UtcNow,
                Message = "Service has recovered",
                SuccessCount = args.SuccessCount
            });
    }

    private async Task NotifyMonitoringSystem(CircuitBreakerStateChangedEventArgs args)
    {
        // Send metrics to monitoring system
        // Examples: Application Insights, Prometheus, CloudWatch
        
        // _metricsClient.Gauge("circuit_breaker.state", (int)args.NewState);
        // _metricsClient.Increment($"circuit_breaker.state_changed.{args.NewState}");
    }
}
```

## 💀 Dead Letter Queues

Dead letter queues handle messages that cannot be processed successfully.

### Dead Letter Queue Configuration

```csharp
public class DeadLetterQueueSetup
{
    private readonly IRabbitMQClient _rabbitMQ;
    private readonly ILogger<DeadLetterQueueSetup> _logger;

    public DeadLetterQueueSetup(IRabbitMQClient rabbitMQ, ILogger<DeadLetterQueueSetup> logger)
    {
        _rabbitMQ = rabbitMQ;
        _logger = logger;
    }

    public async Task SetupDeadLetterInfrastructureAsync()
    {
        // Declare dead letter exchange
        await _rabbitMQ.ExchangeManager.DeclareExchangeAsync(
            exchange: "dlx",
            type: "topic",
            durable: true,
            autoDelete: false);

        // Declare dead letter queue
        await _rabbitMQ.QueueManager.DeclareQueueAsync(
            queue: "dlq",
            durable: true,
            exclusive: false,
            autoDelete: false);

        // Bind dead letter queue to exchange
        await _rabbitMQ.QueueManager.BindQueueAsync(
            queue: "dlq",
            exchange: "dlx",
            routingKey: "#"); // Catch all dead letters

        // Declare main queue with dead letter configuration
        await _rabbitMQ.QueueManager.DeclareQueueAsync(
            queue: "order-processing",
            durable: true,
            exclusive: false,
            autoDelete: false,
            arguments: new Dictionary<string, object>
            {
                ["x-dead-letter-exchange"] = "dlx",
                ["x-dead-letter-routing-key"] = "order.failed",
                ["x-message-ttl"] = 300000, // 5 minutes TTL
                ["x-max-retries"] = 3
            });

        _logger.LogInformation("Dead letter infrastructure setup completed");
    }
}
```

### Dead Letter Queue Consumer

```csharp
public class DeadLetterQueueConsumer
{
    private readonly IRabbitMQClient _rabbitMQ;
    private readonly ILogger<DeadLetterQueueConsumer> _logger;

    public DeadLetterQueueConsumer(IRabbitMQClient rabbitMQ, ILogger<DeadLetterQueueConsumer> logger)
    {
        _rabbitMQ = rabbitMQ;
        _logger = logger;
    }

    public async Task ProcessDeadLetterMessagesAsync(CancellationToken cancellationToken = default)
    {
        await _rabbitMQ.Consumer.ConsumeAsync<DeadLetterMessage>(
            queueName: "dlq",
            messageHandler: async (deadLetterMessage, context) =>
            {
                _logger.LogError("Processing dead letter message: {MessageId}", context.MessageId);
                
                // Extract death information
                var deathInfo = ExtractDeathInformation(context);
                
                // Handle based on death reason
                var handled = await HandleDeadLetterMessage(deadLetterMessage, deathInfo);
                
                if (handled)
                {
                    _logger.LogInformation("Dead letter message {MessageId} handled successfully", context.MessageId);
                }
                else
                {
                    _logger.LogError("Failed to handle dead letter message {MessageId}", context.MessageId);
                }
                
                return handled;
            },
            cancellationToken: cancellationToken);
    }

    private DeathInformation ExtractDeathInformation(MessageContext context)
    {
        var deathInfo = new DeathInformation
        {
            MessageId = context.MessageId,
            OriginalExchange = context.Exchange,
            OriginalRoutingKey = context.RoutingKey,
            DeathTimestamp = DateTimeOffset.UtcNow
        };

        // Extract x-death headers
        if (context.Headers.TryGetValue("x-death", out var xDeathObj) && xDeathObj is List<object> xDeath)
        {
            if (xDeath.Count > 0 && xDeath[0] is Dictionary<string, object> firstDeath)
            {
                if (firstDeath.TryGetValue("reason", out var reason))
                    deathInfo.DeathReason = reason.ToString();
                
                if (firstDeath.TryGetValue("count", out var count))
                    deathInfo.DeathCount = Convert.ToInt32(count);
                
                if (firstDeath.TryGetValue("exchange", out var exchange))
                    deathInfo.OriginalExchange = exchange.ToString();
                
                if (firstDeath.TryGetValue("routing-keys", out var routingKeys) && routingKeys is List<object> keys)
                    deathInfo.OriginalRoutingKey = keys.FirstOrDefault()?.ToString() ?? "";
            }
        }

        return deathInfo;
    }

    private async Task<bool> HandleDeadLetterMessage(DeadLetterMessage deadLetterMessage, DeathInformation deathInfo)
    {
        try
        {
            // Handle different death reasons
            switch (deathInfo.DeathReason)
            {
                case "rejected":
                    return await HandleRejectedMessage(deadLetterMessage, deathInfo);
                case "expired":
                    return await HandleExpiredMessage(deadLetterMessage, deathInfo);
                case "maxlen":
                    return await HandleMaxLengthMessage(deadLetterMessage, deathInfo);
                default:
                    return await HandleUnknownDeathReason(deadLetterMessage, deathInfo);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error handling dead letter message");
            return false;
        }
    }

    private async Task<bool> HandleRejectedMessage(DeadLetterMessage deadLetterMessage, DeathInformation deathInfo)
    {
        _logger.LogInformation("Handling rejected message: {MessageId}", deathInfo.MessageId);
        
        // Check if message can be reprocessed
        if (deathInfo.DeathCount <= 5) // Allow up to 5 retries
        {
            // Attempt to reprocess after delay
            await Task.Delay(TimeSpan.FromMinutes(5));
            
            await _rabbitMQ.Producer.PublishAsync(
                exchange: deathInfo.OriginalExchange,
                routingKey: deathInfo.OriginalRoutingKey,
                message: deadLetterMessage.OriginalMessage);
            
            return true;
        }
        else
        {
            // Archive message for manual review
            await ArchiveMessage(deadLetterMessage, deathInfo);
            return true;
        }
    }

    private async Task<bool> HandleExpiredMessage(DeadLetterMessage deadLetterMessage, DeathInformation deathInfo)
    {
        _logger.LogInformation("Handling expired message: {MessageId}", deathInfo.MessageId);
        
        // Archive expired messages
        await ArchiveMessage(deadLetterMessage, deathInfo);
        return true;
    }

    private async Task<bool> HandleMaxLengthMessage(DeadLetterMessage deadLetterMessage, DeathInformation deathInfo)
    {
        _logger.LogInformation("Handling max length exceeded message: {MessageId}", deathInfo.MessageId);
        
        // Send to alternative queue with higher capacity
        await _rabbitMQ.Producer.PublishAsync(
            exchange: "high-capacity",
            routingKey: deathInfo.OriginalRoutingKey,
            message: deadLetterMessage.OriginalMessage);
        
        return true;
    }

    private async Task<bool> HandleUnknownDeathReason(DeadLetterMessage deadLetterMessage, DeathInformation deathInfo)
    {
        _logger.LogWarning("Unknown death reason: {DeathReason}", deathInfo.DeathReason);
        
        // Archive for manual review
        await ArchiveMessage(deadLetterMessage, deathInfo);
        return true;
    }

    private async Task ArchiveMessage(DeadLetterMessage deadLetterMessage, DeathInformation deathInfo)
    {
        var archiveData = new
        {
            DeadLetterMessage = deadLetterMessage,
            DeathInformation = deathInfo,
            ArchivedAt = DateTimeOffset.UtcNow
        };

        await _rabbitMQ.Producer.PublishAsync(
            exchange: "archive",
            routingKey: "dead-letter.archived",
            message: archiveData);
    }
}
```

## 🏷️ Error Classification

Proper error classification helps determine the appropriate handling strategy.

### Error Types

```csharp
public enum ErrorType
{
    Transient,      // Temporary errors that might succeed on retry
    Permanent,      // Errors that won't succeed on retry
    Timeout,        // Timeout errors
    Authentication, // Authentication/authorization errors
    Validation,     // Input validation errors
    Business,       // Business logic errors
    System,         // System errors
    Unknown         // Unknown errors
}

public class ErrorClassifier
{
    private readonly ILogger<ErrorClassifier> _logger;

    public ErrorClassifier(ILogger<ErrorClassifier> logger)
    {
        _logger = logger;
    }

    public ErrorType ClassifyError(Exception exception)
    {
        return exception switch
        {
            // Transient errors
            TimeoutException => ErrorType.Timeout,
            HttpRequestException => ErrorType.Transient,
            SocketException => ErrorType.Transient,
            TaskCanceledException => ErrorType.Timeout,
            
            // Permanent errors
            ArgumentException => ErrorType.Validation,
            ArgumentNullException => ErrorType.Validation,
            InvalidOperationException => ErrorType.Permanent,
            NotSupportedException => ErrorType.Permanent,
            
            // Authentication errors
            UnauthorizedAccessException => ErrorType.Authentication,
            SecurityException => ErrorType.Authentication,
            
            // Business errors
            BusinessException => ErrorType.Business,
            ValidationException => ErrorType.Validation,
            
            // System errors
            OutOfMemoryException => ErrorType.System,
            StackOverflowException => ErrorType.System,
            
            // Unknown errors
            _ => ErrorType.Unknown
        };
    }

    public bool ShouldRetry(Exception exception, int attemptNumber, int maxAttempts)
    {
        if (attemptNumber >= maxAttempts)
            return false;

        var errorType = ClassifyError(exception);
        
        return errorType switch
        {
            ErrorType.Transient => true,
            ErrorType.Timeout => true,
            ErrorType.Unknown => attemptNumber < 2, // Only retry once for unknown errors
            ErrorType.Permanent => false,
            ErrorType.Validation => false,
            ErrorType.Authentication => false,
            ErrorType.Business => false,
            ErrorType.System => false,
            _ => false
        };
    }

    public TimeSpan GetRetryDelay(Exception exception, int attemptNumber)
    {
        var errorType = ClassifyError(exception);
        
        return errorType switch
        {
            ErrorType.Transient => TimeSpan.FromSeconds(Math.Pow(2, attemptNumber)), // Exponential backoff
            ErrorType.Timeout => TimeSpan.FromSeconds(1 + attemptNumber), // Linear backoff
            ErrorType.Unknown => TimeSpan.FromSeconds(5), // Fixed delay
            _ => TimeSpan.Zero
        };
    }
}
```

### Error Classification in Consumer

```csharp
public class ClassifyingErrorConsumer
{
    private readonly IRabbitMQClient _rabbitMQ;
    private readonly ErrorClassifier _errorClassifier;
    private readonly ILogger<ClassifyingErrorConsumer> _logger;

    public ClassifyingErrorConsumer(IRabbitMQClient rabbitMQ, ErrorClassifier errorClassifier, ILogger<ClassifyingErrorConsumer> logger)
    {
        _rabbitMQ = rabbitMQ;
        _errorClassifier = errorClassifier;
        _logger = logger;
    }

    public async Task ConsumeWithErrorClassificationAsync(CancellationToken cancellationToken = default)
    {
        await _rabbitMQ.Consumer.ConsumeAsync<Order>(
            queueName: "order-processing",
            messageHandler: async (order, context) =>
            {
                var attemptNumber = GetAttemptNumber(context);
                const int maxAttempts = 3;

                try
                {
                    await ProcessOrderAsync(order);
                    return true;
                }
                catch (Exception ex)
                {
                    var errorType = _errorClassifier.ClassifyError(ex);
                    
                    _logger.LogError(ex, "Error processing order {OrderId}. Error type: {ErrorType}, Attempt: {Attempt}", 
                        order.Id, errorType, attemptNumber);

                    if (_errorClassifier.ShouldRetry(ex, attemptNumber, maxAttempts))
                    {
                        var delay = _errorClassifier.GetRetryDelay(ex, attemptNumber);
                        
                        _logger.LogWarning("Retrying order {OrderId} in {Delay}ms", 
                            order.Id, delay.TotalMilliseconds);
                        
                        // Add retry headers
                        await PublishForRetry(order, context, attemptNumber + 1, delay);
                        return true; // Acknowledge original message
                    }
                    else
                    {
                        _logger.LogError("Not retrying order {OrderId}. Error type: {ErrorType}", 
                            order.Id, errorType);
                        
                        // Send to appropriate dead letter queue based on error type
                        await SendToDeadLetterQueue(order, ex, errorType);
                        return true; // Acknowledge to prevent infinite loop
                    }
                }
            },
            cancellationToken: cancellationToken);
    }

    private async Task ProcessOrderAsync(Order order)
    {
        // Simulate processing with various error types
        await Task.Delay(100);
        
        var errorType = Random.Shared.Next(1, 6);
        switch (errorType)
        {
            case 1:
                throw new HttpRequestException("External service unavailable");
            case 2:
                throw new ArgumentException("Invalid order data");
            case 3:
                throw new BusinessException("Order violates business rules");
            case 4:
                throw new TimeoutException("Processing timeout");
            case 5:
                throw new UnauthorizedAccessException("Insufficient permissions");
            default:
                // Success case
                break;
        }
    }

    private int GetAttemptNumber(MessageContext context)
    {
        if (context.Headers.TryGetValue("x-retry-count", out var retryCount))
        {
            return Convert.ToInt32(retryCount);
        }
        return 0;
    }

    private async Task PublishForRetry(Order order, MessageContext context, int attemptNumber, TimeSpan delay)
    {
        // Publish to retry queue with delay
        var retryProperties = new BasicProperties
        {
            Headers = new Dictionary<string, object>(context.Headers)
            {
                ["x-retry-count"] = attemptNumber,
                ["x-retry-timestamp"] = DateTimeOffset.UtcNow.ToUnixTimeSeconds()
            },
            Expiration = delay.TotalMilliseconds.ToString()
        };

        await _rabbitMQ.Producer.PublishAsync(
            exchange: "retry-exchange",
            routingKey: context.RoutingKey,
            message: order,
            properties: retryProperties);
    }

    private async Task SendToDeadLetterQueue(Order order, Exception exception, ErrorType errorType)
    {
        var deadLetterRoutingKey = errorType switch
        {
            ErrorType.Permanent => "permanent.error",
            ErrorType.Validation => "validation.error",
            ErrorType.Authentication => "auth.error",
            ErrorType.Business => "business.error",
            ErrorType.System => "system.error",
            _ => "unknown.error"
        };

        await _rabbitMQ.Producer.PublishAsync(
            exchange: "dlx",
            routingKey: deadLetterRoutingKey,
            message: new
            {
                OriginalMessage = order,
                ErrorType = errorType.ToString(),
                ErrorMessage = exception.Message,
                StackTrace = exception.StackTrace,
                Timestamp = DateTimeOffset.UtcNow
            });
    }
}

// Custom exceptions
public class BusinessException : Exception
{
    public BusinessException(string message) : base(message) { }
    public BusinessException(string message, Exception innerException) : base(message, innerException) { }
}

public class ValidationException : Exception
{
    public ValidationException(string message) : base(message) { }
    public ValidationException(string message, Exception innerException) : base(message, innerException) { }
}
```

## 🔧 Custom Error Handlers

Create custom error handlers for specific scenarios.

### Custom Error Handler Implementation

```csharp
public interface ICustomErrorHandler
{
    Task<ErrorHandlingResult> HandleErrorAsync(ErrorContext context, CancellationToken cancellationToken = default);
}

public class CustomErrorHandler : ICustomErrorHandler
{
    private readonly IRabbitMQClient _rabbitMQ;
    private readonly ILogger<CustomErrorHandler> _logger;
    private readonly ErrorClassifier _errorClassifier;

    public CustomErrorHandler(IRabbitMQClient rabbitMQ, ILogger<CustomErrorHandler> logger, ErrorClassifier errorClassifier)
    {
        _rabbitMQ = rabbitMQ;
        _logger = logger;
        _errorClassifier = errorClassifier;
    }

    public async Task<ErrorHandlingResult> HandleErrorAsync(ErrorContext context, CancellationToken cancellationToken = default)
    {
        var errorType = _errorClassifier.ClassifyError(context.Exception);
        
        _logger.LogError(context.Exception, "Handling error for message {MessageId}. Error type: {ErrorType}", 
            context.MessageId, errorType);

        try
        {
            // Handle based on error type
            switch (errorType)
            {
                case ErrorType.Transient:
                    return await HandleTransientError(context, cancellationToken);
                
                case ErrorType.Permanent:
                    return await HandlePermanentError(context, cancellationToken);
                
                case ErrorType.Validation:
                    return await HandleValidationError(context, cancellationToken);
                
                case ErrorType.Authentication:
                    return await HandleAuthenticationError(context, cancellationToken);
                
                case ErrorType.Business:
                    return await HandleBusinessError(context, cancellationToken);
                
                case ErrorType.System:
                    return await HandleSystemError(context, cancellationToken);
                
                default:
                    return await HandleUnknownError(context, cancellationToken);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in custom error handler");
            return ErrorHandlingResult.Reject;
        }
    }

    private async Task<ErrorHandlingResult> HandleTransientError(ErrorContext context, CancellationToken cancellationToken)
    {
        if (context.AttemptNumber < 3)
        {
            var delay = TimeSpan.FromSeconds(Math.Pow(2, context.AttemptNumber));
            
            _logger.LogWarning("Transient error on attempt {Attempt}. Retrying in {Delay}s", 
                context.AttemptNumber, delay.TotalSeconds);
            
            await Task.Delay(delay, cancellationToken);
            return ErrorHandlingResult.Retry;
        }
        
        _logger.LogError("Transient error exceeded retry limit");
        await SendToDeadLetterQueue(context, "transient.max-retries");
        return ErrorHandlingResult.Acknowledge;
    }

    private async Task<ErrorHandlingResult> HandlePermanentError(ErrorContext context, CancellationToken cancellationToken)
    {
        _logger.LogError("Permanent error detected. Sending to dead letter queue");
        await SendToDeadLetterQueue(context, "permanent.error");
        return ErrorHandlingResult.Acknowledge;
    }

    private async Task<ErrorHandlingResult> HandleValidationError(ErrorContext context, CancellationToken cancellationToken)
    {
        _logger.LogError("Validation error detected. Sending to validation error queue");
        await SendToValidationErrorQueue(context);
        return ErrorHandlingResult.Acknowledge;
    }

    private async Task<ErrorHandlingResult> HandleAuthenticationError(ErrorContext context, CancellationToken cancellationToken)
    {
        _logger.LogError("Authentication error detected. Sending to auth error queue");
        await SendToAuthErrorQueue(context);
        return ErrorHandlingResult.Acknowledge;
    }

    private async Task<ErrorHandlingResult> HandleBusinessError(ErrorContext context, CancellationToken cancellationToken)
    {
        _logger.LogError("Business error detected. Sending to business error queue");
        await SendToBusinessErrorQueue(context);
        return ErrorHandlingResult.Acknowledge;
    }

    private async Task<ErrorHandlingResult> HandleSystemError(ErrorContext context, CancellationToken cancellationToken)
    {
        _logger.LogError("System error detected. Sending to system error queue");
        await SendToSystemErrorQueue(context);
        return ErrorHandlingResult.Acknowledge;
    }

    private async Task<ErrorHandlingResult> HandleUnknownError(ErrorContext context, CancellationToken cancellationToken)
    {
        if (context.AttemptNumber < 1) // Only retry once for unknown errors
        {
            _logger.LogWarning("Unknown error on attempt {Attempt}. Retrying once", context.AttemptNumber);
            await Task.Delay(TimeSpan.FromSeconds(5), cancellationToken);
            return ErrorHandlingResult.Retry;
        }
        
        _logger.LogError("Unknown error exceeded retry limit");
        await SendToDeadLetterQueue(context, "unknown.error");
        return ErrorHandlingResult.Acknowledge;
    }

    private async Task SendToDeadLetterQueue(ErrorContext context, string errorType)
    {
        await _rabbitMQ.Producer.PublishAsync(
            exchange: "dlx",
            routingKey: errorType,
            message: new
            {
                OriginalMessage = context.Message,
                ErrorType = errorType,
                ErrorMessage = context.Exception.Message,
                StackTrace = context.Exception.StackTrace,
                AttemptNumber = context.AttemptNumber,
                Timestamp = DateTimeOffset.UtcNow
            });
    }

    private async Task SendToValidationErrorQueue(ErrorContext context)
    {
        await _rabbitMQ.Producer.PublishAsync(
            exchange: "validation-errors",
            routingKey: "validation.failed",
            message: new
            {
                OriginalMessage = context.Message,
                ValidationErrors = ExtractValidationErrors(context.Exception),
                Timestamp = DateTimeOffset.UtcNow
            });
    }

    private async Task SendToAuthErrorQueue(ErrorContext context)
    {
        await _rabbitMQ.Producer.PublishAsync(
            exchange: "auth-errors",
            routingKey: "auth.failed",
            message: new
            {
                OriginalMessage = context.Message,
                AuthError = context.Exception.Message,
                Timestamp = DateTimeOffset.UtcNow
            });
    }

    private async Task SendToBusinessErrorQueue(ErrorContext context)
    {
        await _rabbitMQ.Producer.PublishAsync(
            exchange: "business-errors",
            routingKey: "business.rule.violated",
            message: new
            {
                OriginalMessage = context.Message,
                BusinessRule = context.Exception.Message,
                Timestamp = DateTimeOffset.UtcNow
            });
    }

    private async Task SendToSystemErrorQueue(ErrorContext context)
    {
        await _rabbitMQ.Producer.PublishAsync(
            exchange: "system-errors",
            routingKey: "system.error",
            message: new
            {
                OriginalMessage = context.Message,
                SystemError = context.Exception.Message,
                StackTrace = context.Exception.StackTrace,
                Timestamp = DateTimeOffset.UtcNow
            });
    }

    private List<string> ExtractValidationErrors(Exception exception)
    {
        // Extract validation errors from exception
        var errors = new List<string>();
        
        if (exception is ValidationException validationEx)
        {
            errors.Add(validationEx.Message);
        }
        else if (exception is ArgumentException argumentEx)
        {
            errors.Add(argumentEx.Message);
        }
        else
        {
            errors.Add(exception.Message);
        }
        
        return errors;
    }
}

public enum ErrorHandlingResult
{
    Acknowledge,
    Retry,
    Reject
}
```

## 📊 Monitoring and Alerting

Monitor error patterns and set up alerting for critical issues.

### Error Monitoring Service

```csharp
public class ErrorMonitoringService
{
    private readonly IRabbitMQClient _rabbitMQ;
    private readonly ILogger<ErrorMonitoringService> _logger;
    private readonly Dictionary<string, ErrorMetrics> _errorMetrics = new();
    private readonly Timer _monitoringTimer;

    public ErrorMonitoringService(IRabbitMQClient rabbitMQ, ILogger<ErrorMonitoringService> logger)
    {
        _rabbitMQ = rabbitMQ;
        _logger = logger;
        
        // Monitor every minute
        _monitoringTimer = new Timer(MonitorErrors, null, TimeSpan.FromMinutes(1), TimeSpan.FromMinutes(1));
    }

    public void RecordError(string errorType, string serviceName, Exception exception)
    {
        var key = $"{serviceName}:{errorType}";
        
        lock (_errorMetrics)
        {
            if (!_errorMetrics.ContainsKey(key))
            {
                _errorMetrics[key] = new ErrorMetrics
                {
                    ServiceName = serviceName,
                    ErrorType = errorType
                };
            }
            
            _errorMetrics[key].ErrorCount++;
            _errorMetrics[key].LastError = exception.Message;
            _errorMetrics[key].LastErrorTime = DateTimeOffset.UtcNow;
        }
    }

    private async void MonitorErrors(object? state)
    {
        try
        {
            var currentMetrics = new Dictionary<string, ErrorMetrics>();
            
            lock (_errorMetrics)
            {
                foreach (var metric in _errorMetrics)
                {
                    currentMetrics[metric.Key] = metric.Value.Clone();
                }
                _errorMetrics.Clear();
            }

            foreach (var metric in currentMetrics.Values)
            {
                await ProcessErrorMetrics(metric);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in error monitoring service");
        }
    }

    private async Task ProcessErrorMetrics(ErrorMetrics metrics)
    {
        // Check for error rate thresholds
        if (metrics.ErrorCount >= 10) // High error rate
        {
            await SendHighErrorRateAlert(metrics);
        }
        
        // Check for new error types
        if (metrics.ErrorType == "Unknown")
        {
            await SendNewErrorTypeAlert(metrics);
        }
        
        // Check for system errors
        if (metrics.ErrorType == "System")
        {
            await SendSystemErrorAlert(metrics);
        }
        
        // Log metrics
        _logger.LogInformation("Error metrics: Service={ServiceName}, Type={ErrorType}, Count={ErrorCount}", 
            metrics.ServiceName, metrics.ErrorType, metrics.ErrorCount);
    }

    private async Task SendHighErrorRateAlert(ErrorMetrics metrics)
    {
        await _rabbitMQ.Producer.PublishAsync(
            exchange: "alerts",
            routingKey: "error.high-rate",
            message: new
            {
                AlertType = "HighErrorRate",
                ServiceName = metrics.ServiceName,
                ErrorType = metrics.ErrorType,
                ErrorCount = metrics.ErrorCount,
                LastError = metrics.LastError,
                LastErrorTime = metrics.LastErrorTime,
                Timestamp = DateTimeOffset.UtcNow
            });
    }

    private async Task SendNewErrorTypeAlert(ErrorMetrics metrics)
    {
        await _rabbitMQ.Producer.PublishAsync(
            exchange: "alerts",
            routingKey: "error.new-type",
            message: new
            {
                AlertType = "NewErrorType",
                ServiceName = metrics.ServiceName,
                ErrorType = metrics.ErrorType,
                ErrorCount = metrics.ErrorCount,
                LastError = metrics.LastError,
                LastErrorTime = metrics.LastErrorTime,
                Timestamp = DateTimeOffset.UtcNow
            });
    }

    private async Task SendSystemErrorAlert(ErrorMetrics metrics)
    {
        await _rabbitMQ.Producer.PublishAsync(
            exchange: "alerts",
            routingKey: "error.system",
            message: new
            {
                AlertType = "SystemError",
                ServiceName = metrics.ServiceName,
                ErrorType = metrics.ErrorType,
                ErrorCount = metrics.ErrorCount,
                LastError = metrics.LastError,
                LastErrorTime = metrics.LastErrorTime,
                Timestamp = DateTimeOffset.UtcNow
            });
    }
}

public class ErrorMetrics
{
    public string ServiceName { get; set; } = string.Empty;
    public string ErrorType { get; set; } = string.Empty;
    public int ErrorCount { get; set; }
    public string LastError { get; set; } = string.Empty;
    public DateTimeOffset LastErrorTime { get; set; }

    public ErrorMetrics Clone()
    {
        return new ErrorMetrics
        {
            ServiceName = ServiceName,
            ErrorType = ErrorType,
            ErrorCount = ErrorCount,
            LastError = LastError,
            LastErrorTime = LastErrorTime
        };
    }
}
```

## 🎯 Best Practices

### 1. Always Classify Errors

```csharp
// DO: Classify errors to determine appropriate handling
public async Task<bool> ProcessMessageAsync(Order order, MessageContext context)
{
    try
    {
        await ProcessOrderAsync(order);
        return true;
    }
    catch (Exception ex)
    {
        var errorType = _errorClassifier.ClassifyError(ex);
        
        switch (errorType)
        {
            case ErrorType.Transient:
                return false; // Retry
            case ErrorType.Permanent:
                await SendToDeadLetterQueue(order, ex);
                return true; // Acknowledge
            default:
                return false; // Retry once
        }
    }
}
```

### 2. Implement Exponential Backoff

```csharp
// DO: Use exponential backoff for retries
var delay = TimeSpan.FromSeconds(Math.Pow(2, attemptNumber));
await Task.Delay(delay);
```

### 3. Set Maximum Retry Limits

```csharp
// DO: Always set maximum retry limits
const int maxRetries = 3;
if (attemptNumber >= maxRetries)
{
    await SendToDeadLetterQueue(message, exception);
    return true; // Acknowledge to prevent infinite loop
}
```

### 4. Monitor Error Patterns

```csharp
// DO: Monitor and alert on error patterns
public void RecordError(Exception exception, string context)
{
    var errorType = _errorClassifier.ClassifyError(exception);
    _metricsCollector.RecordError(errorType, context);
    
    if (errorType == ErrorType.System)
    {
        _alertingService.SendAlert($"System error in {context}: {exception.Message}");
    }
}
```

### 5. Use Circuit Breakers for External Services

```csharp
// DO: Use circuit breakers for external service calls
await _circuitBreaker.ExecuteAsync(async () =>
{
    await _externalService.ProcessOrderAsync(order);
});
```

### 6. Log Errors with Context

```csharp
// DO: Log errors with sufficient context
_logger.LogError(ex, "Failed to process order {OrderId} for customer {CustomerId}. " +
    "Attempt {Attempt}/{MaxAttempts}. Error type: {ErrorType}", 
    order.Id, order.CustomerId, attemptNumber, maxAttempts, errorType);
```

### 7. Implement Health Checks

```csharp
// DO: Implement health checks for error handling components
public class ErrorHandlingHealthCheck : IHealthCheck
{
    private readonly ICircuitBreaker _circuitBreaker;
    private readonly IErrorHandler _errorHandler;

    public async Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
    {
        var data = new Dictionary<string, object>
        {
            ["circuit_breaker_state"] = _circuitBreaker.State.ToString(),
            ["error_handler_status"] = _errorHandler.Status.ToString()
        };

        var isHealthy = _circuitBreaker.State != CircuitBreakerState.Open;
        
        return isHealthy 
            ? HealthCheckResult.Healthy("Error handling is healthy", data)
            : HealthCheckResult.Unhealthy("Circuit breaker is open", data: data);
    }
}
```

## 🎉 Summary

You've now mastered comprehensive error handling in FS.RabbitMQ:

✅ **Retry policies and strategies**  
✅ **Circuit breaker pattern implementation**  
✅ **Dead letter queue handling**  
✅ **Error classification and routing**  
✅ **Custom error handlers**  
✅ **Monitoring and alerting**  
✅ **Production-ready best practices**  

## 🎯 Next Steps

Continue your FS.RabbitMQ journey:

- [Performance Tuning](performance.md) - Optimize for high throughput
- [Monitoring](monitoring.md) - Monitor your messaging system
- [Examples](examples/) - See real-world examples
- [Configuration](configuration.md) - Advanced configuration options 