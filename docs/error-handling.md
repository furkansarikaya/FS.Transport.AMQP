# Error Handling Guide

This guide covers comprehensive error handling strategies in FS.StreamFlow, from basic retry mechanisms to advanced resilience patterns.

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

FS.StreamFlow provides multiple layers of error handling to ensure message processing reliability:

### Error Handling Layers

1. **Producer Error Handling**: Publication failures, confirmations, and retries
2. **Consumer Error Handling**: Processing failures, acknowledgments, and requeues
3. **Connection Error Handling**: Connection failures and automatic recovery
4. **Application Error Handling**: Business logic errors and custom handling

### Error Handling Configuration

```csharp
builder.Services.AddRabbitMQStreamFlow(options =>
{
    // Error handling settings
    options.ConsumerSettings.ErrorHandling = new ErrorHandlingSettings
    {
        Strategy = ErrorHandlingStrategy.Requeue,
        MaxRetries = 3,
        RetryDelay = TimeSpan.FromSeconds(1),
        UseExponentialBackoff = true,
        LogErrors = true,
        ContinueOnError = true
    };
    
    // Dead letter queue settings
    options.ConsumerSettings.DeadLetterSettings = new DeadLetterSettings
    {
        ExchangeName = "dlx",
        RoutingKey = "failed",
        Enabled = true,
        MaxRetries = 3,
        MessageTtl = TimeSpan.FromHours(24)
    };
    
    // Retry policy settings
    options.ConsumerSettings.RetryPolicy = new RetryPolicySettings
    {
        MaxRetryAttempts = 3,
        InitialRetryDelay = TimeSpan.FromSeconds(1),
        MaxRetryDelay = TimeSpan.FromMinutes(5),
        RetryDelayMultiplier = 2.0,
        UseExponentialBackoff = true,
        UseJitter = true
    };
});
```

## 🔄 Retry Policies

### Built-in Retry Policies

FS.StreamFlow provides several built-in retry policies:

#### 1. Linear Retry Policy

```csharp
public class LinearRetryExample
{
    private readonly IStreamFlowClient _streamFlow;
    private readonly IRetryPolicyFactory _retryPolicyFactory;
    private readonly ILogger<LinearRetryExample> _logger;

    public LinearRetryExample(IStreamFlowClient streamFlow, IRetryPolicyFactory retryPolicyFactory, ILogger<LinearRetryExample> logger)
    {
        _streamFlow = streamFlow;
        _retryPolicyFactory = retryPolicyFactory;
        _logger = logger;
    }

    public async Task ProcessWithLinearRetryAsync(Order order)
    {
        await _streamFlow.InitializeAsync();
        var retryPolicy = _retryPolicyFactory.CreateLinearPolicy(
            maxRetryAttempts: 3,
            delay: TimeSpan.FromSeconds(2));

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
    private readonly IStreamFlowClient _streamFlow;
    private readonly IRetryPolicyFactory _retryPolicyFactory;
    private readonly ILogger<ExponentialBackoffRetryExample> _logger;

    public ExponentialBackoffRetryExample(IStreamFlowClient streamFlow, IRetryPolicyFactory retryPolicyFactory, ILogger<ExponentialBackoffRetryExample> logger)
    {
        _streamFlow = streamFlow;
        _retryPolicyFactory = retryPolicyFactory;
        _logger = logger;
    }

    public async Task ProcessWithExponentialBackoffAsync(Order order)
    {
        await _streamFlow.InitializeAsync();
        var retryPolicy = _retryPolicyFactory.CreateExponentialBackoffPolicy(
            maxRetryAttempts: 5,
            initialDelay: TimeSpan.FromSeconds(1),
            maxDelay: TimeSpan.FromMinutes(5),
            multiplier: 2.0,
            useJitter: true);

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
public class CustomRetryPolicy : RetryPolicyBase
{
    private readonly Random _random = new();

    public CustomRetryPolicy(RetryPolicySettings settings, ILogger<CustomRetryPolicy> logger) 
        : base(settings, logger)
    {
    }

    public override string Name => "Custom";

    public override bool ShouldRetry(Exception exception, int attemptNumber)
    {
        // Don't retry if max attempts reached
        if (attemptNumber >= MaxRetryAttempts)
        {
            _logger.LogError("Max retry attempts ({MaxAttempts}) reached for exception: {Exception}", 
                MaxRetryAttempts, exception.Message);
            return false;
        }

        // Don't retry permanent errors
        if (IsPermanentError(exception))
        {
            _logger.LogError("Permanent error detected, not retrying: {Exception}", exception.Message);
            return false;
        }

        return true;
    }

    public override TimeSpan CalculateDelay(int attemptNumber)
    {
        var delay = TimeSpan.FromMilliseconds(Settings.InitialRetryDelay.TotalMilliseconds * Math.Pow(Settings.RetryDelayMultiplier, attemptNumber));
        
        // Add jitter to prevent thundering herd
        var jitter = TimeSpan.FromMilliseconds(_random.Next(0, (int)(delay.TotalMilliseconds * 0.1)));
        delay = delay.Add(jitter);
        
        // Cap at maximum delay
        return delay > Settings.MaxRetryDelay ? Settings.MaxRetryDelay : delay;
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
}
```

### Using Retry Policies in Consumers

```csharp
public class RetryConsumer
{
    private readonly IStreamFlowClient _streamFlow;
    private readonly IRetryPolicyFactory _retryPolicyFactory;
    private readonly ILogger<RetryConsumer> _logger;

    public RetryConsumer(IStreamFlowClient streamFlow, IRetryPolicyFactory retryPolicyFactory, ILogger<RetryConsumer> logger)
    {
        _streamFlow = streamFlow;
        _retryPolicyFactory = retryPolicyFactory;
        _logger = logger;
    }

    public async Task ConsumeWithRetryAsync(CancellationToken cancellationToken = default)
    {
        await _streamFlow.InitializeAsync();
        
        // Create retry policy
        var retryPolicy = _retryPolicyFactory.CreateExponentialBackoffPolicy(
            maxRetryAttempts: 3,
            initialDelay: TimeSpan.FromSeconds(1),
            maxDelay: TimeSpan.FromSeconds(30),
            multiplier: 2.0,
            useJitter: true);

        await _streamFlow.Consumer.Queue<Order>("order-processing")
            .WithConcurrency(3)
            .WithPrefetchCount(10)
            .WithErrorHandler(async (exception, context) =>
            {
                try
                {
                    await retryPolicy.ExecuteAsync(async () =>
                    {
                        await ProcessOrderAsync(context.Message as Order);
                    });
                    
                    return true; // Success
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Failed to process order after all retry attempts");
                    
                    // Send to dead letter queue
                    await SendToDeadLetterQueue(context.Message as Order, ex);
                    
                    return true; // Acknowledge to prevent infinite retry
                }
            })
            .ConsumeAsync(async (order, context) =>
            {
                await ProcessOrderAsync(order);
                return true; // Acknowledge message
            }, cancellationToken);
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
        await _streamFlow.InitializeAsync();
        await _streamFlow.Producer.Message(new
        {
            OriginalMessage = order,
            Error = exception.Message,
            StackTrace = exception.StackTrace,
            Timestamp = DateTimeOffset.UtcNow
        })
        .WithExchange("dlx")
        .WithRoutingKey("order.failed")
        .PublishAsync();
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
    private readonly IStreamFlowClient _streamFlow;
    private readonly IRetryPolicyFactory _retryPolicyFactory;
    private readonly ILogger<CircuitBreakerExample> _logger;

    public CircuitBreakerExample(IStreamFlowClient streamFlow, IRetryPolicyFactory retryPolicyFactory, ILogger<CircuitBreakerExample> logger)
    {
        _streamFlow = streamFlow;
        _retryPolicyFactory = retryPolicyFactory;
        _logger = logger;
    }

    public async Task ProcessWithCircuitBreakerAsync(Order order)
    {
        await _streamFlow.InitializeAsync();
        
        // Create circuit breaker retry policy
        var circuitBreakerPolicy = _retryPolicyFactory.CreateCircuitBreakerPolicy(
            maxRetryAttempts: 3,
            circuitBreakerThreshold: 5,
            circuitBreakerTimeout: TimeSpan.FromMinutes(1));
        
        try
        {
            await circuitBreakerPolicy.ExecuteAsync(async () =>
            {
                await CallExternalServiceAsync(order);
            });
            
            _logger.LogInformation("Order {OrderId} processed successfully", order.Id);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing order {OrderId}", order.Id);
            
            // Handle circuit breaker open state
            await HandleCircuitBreakerOpen(order);
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
        
        await _streamFlow.Producer.Message(order)
            .WithExchange("delayed-processing")
            .WithRoutingKey("order.delayed")
            .PublishAsync();
    }
}
```

### Advanced Circuit Breaker with State Events

```csharp
public class AdvancedCircuitBreakerExample
{
    private readonly IStreamFlowClient _streamFlow;
    private readonly ICircuitBreaker _circuitBreaker;
    private readonly ILogger<AdvancedCircuitBreakerExample> _logger;

    public AdvancedCircuitBreakerExample(IStreamFlowClient rabbitMQ, ILogger<AdvancedCircuitBreakerExample> logger)
    {
        _streamFlow = rabbitMQ;
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
        await _streamFlow.Producer.PublishAsync(
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
        await _streamFlow.Producer.PublishAsync(
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
        await _streamFlow.Producer.PublishAsync(
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
    private readonly IStreamFlowClient _streamFlow;
    private readonly ILogger<DeadLetterQueueSetup> _logger;

    public DeadLetterQueueSetup(IStreamFlowClient streamFlow, ILogger<DeadLetterQueueSetup> logger)
    {
        _streamFlow = streamFlow;
        _logger = logger;
    }

    public async Task SetupDeadLetterInfrastructureAsync()
    {
        await _streamFlow.InitializeAsync();
        
        // Declare dead letter exchange
        await _streamFlow.ExchangeManager.Exchange("dlx")
            .AsTopic()
            .WithDurable(true)
            .DeclareAsync();

        // Declare dead letter queue
        await _streamFlow.QueueManager.Queue("dlq")
            .WithDurable(true)
            .WithExclusive(false)
            .WithAutoDelete(false)
            .BindToExchange("dlx", "#") // Catch all dead letters
            .DeclareAsync();

        // Declare main queue with dead letter configuration
        await _streamFlow.QueueManager.Queue("order-processing")
            .WithDurable(true)
            .WithExclusive(false)
            .WithAutoDelete(false)
            .WithDeadLetterExchange("dlx")
            .WithDeadLetterRoutingKey("order.failed")
            .WithMessageTtl(TimeSpan.FromMinutes(5))
            .WithArgument("x-max-retries", 3)
            .DeclareAsync();

        _logger.LogInformation("Dead letter infrastructure setup completed");
    }
}
```

### Dead Letter Queue Consumer

```csharp
public class DeadLetterQueueConsumer
{
    private readonly IStreamFlowClient _streamFlow;
    private readonly ILogger<DeadLetterQueueConsumer> _logger;

    public DeadLetterQueueConsumer(IStreamFlowClient streamFlow, ILogger<DeadLetterQueueConsumer> logger)
    {
        _streamFlow = streamFlow;
        _logger = logger;
    }

    public async Task ProcessDeadLetterMessagesAsync(CancellationToken cancellationToken = default)
    {
        await _streamFlow.InitializeAsync();
        
        await _streamFlow.Consumer.Queue<DeadLetterMessage>("dlq")
            .WithConcurrency(2)
            .WithPrefetchCount(10)
            .ConsumeAsync(async (deadLetterMessage, context) =>
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
            }, cancellationToken);
    }
```

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
            
            await _streamFlow.Producer.Message(deadLetterMessage.OriginalMessage)
                .WithExchange(deathInfo.OriginalExchange)
                .WithRoutingKey(deathInfo.OriginalRoutingKey)
                .PublishAsync();
            
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
        await _streamFlow.Producer.Message(deadLetterMessage.OriginalMessage)
            .WithExchange("high-capacity")
            .WithRoutingKey(deathInfo.OriginalRoutingKey)
            .PublishAsync();
        
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
        await _streamFlow.InitializeAsync();
        var archiveData = new
        {
            DeadLetterMessage = deadLetterMessage,
            DeathInformation = deathInfo,
            ArchivedAt = DateTimeOffset.UtcNow
        };

        await _streamFlow.Producer.Message(archiveData)
            .WithExchange("archive")
            .WithRoutingKey("dead-letter.archived")
            .PublishAsync();
    }
}
```
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
    private readonly IStreamFlowClient _streamFlow;
    private readonly ErrorClassifier _errorClassifier;
    private readonly ILogger<ClassifyingErrorConsumer> _logger;

    public ClassifyingErrorConsumer(IStreamFlowClient streamFlow, ErrorClassifier errorClassifier, ILogger<ClassifyingErrorConsumer> logger)
    {
        _streamFlow = streamFlow;
        _errorClassifier = errorClassifier;
        _logger = logger;
    }

    public async Task ConsumeWithErrorClassificationAsync(CancellationToken cancellationToken = default)
    {
        await _streamFlow.InitializeAsync();
        
        await _streamFlow.Consumer.Queue<Order>("order-processing")
            .WithConcurrency(3)
            .WithPrefetchCount(10)
            .ConsumeAsync(async (order, context) =>
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
            }, cancellationToken);
    }
```

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
        await _streamFlow.InitializeAsync();
        
        await _streamFlow.Producer.Message(order)
            .WithExchange("retry-exchange")
            .WithRoutingKey(context.RoutingKey)
            .WithExpiration(delay)
            .WithHeader("x-retry-count", attemptNumber)
            .WithHeader("x-retry-timestamp", DateTimeOffset.UtcNow.ToUnixTimeSeconds())
            .PublishAsync();
    }

    private async Task SendToDeadLetterQueue(Order order, Exception exception, ErrorType errorType)
    {
        await _streamFlow.InitializeAsync();
        var deadLetterRoutingKey = errorType switch
        {
            ErrorType.Permanent => "permanent.error",
            ErrorType.Validation => "validation.error",
            ErrorType.Authentication => "auth.error",
            ErrorType.Business => "business.error",
            ErrorType.System => "system.error",
            _ => "unknown.error"
        };

        await _streamFlow.Producer.Message(new
        {
            OriginalMessage = order,
            ErrorType = errorType.ToString(),
            ErrorMessage = exception.Message,
            StackTrace = exception.StackTrace,
            Timestamp = DateTimeOffset.UtcNow
        })
        .WithExchange("dlx")
        .WithRoutingKey(deadLetterRoutingKey)
        .PublishAsync();
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
public class CustomErrorHandler : IErrorHandler
{
    private readonly IStreamFlowClient _streamFlow;
    private readonly ILogger<CustomErrorHandler> _logger;
    private readonly ErrorClassifier _errorClassifier;

    public CustomErrorHandler(IStreamFlowClient streamFlow, ILogger<CustomErrorHandler> logger, ErrorClassifier errorClassifier)
    {
        _streamFlow = streamFlow;
        _logger = logger;
        _errorClassifier = errorClassifier;
    }

    public string Name => "Custom Error Handler";

    public ErrorHandlingSettings Settings { get; } = new();

    public event EventHandler<ErrorHandledEventArgs>? ErrorHandled;
    public event EventHandler<DeadLetterEventArgs>? MessageSentToDeadLetterQueue;

    public async Task<ErrorHandlingResult> HandleErrorAsync(Exception exception, MessageContext context, CancellationToken cancellationToken = default)
    {
        return await HandleErrorAsync(exception, context, 1, cancellationToken);
    }

    public async Task<ErrorHandlingResult> HandleErrorAsync(Exception exception, MessageContext context, int attemptNumber, CancellationToken cancellationToken = default)
    {
        await _streamFlow.InitializeAsync();
        var errorType = _errorClassifier.ClassifyError(exception);
        
        _logger.LogError(exception, "Handling error for message {MessageId}. Error type: {ErrorType}", 
            context.MessageId, errorType);

        try
        {
            // Handle based on error type
            switch (errorType)
            {
                case ErrorType.Transient:
                    return await HandleTransientError(exception, context, attemptNumber, cancellationToken);
                
                case ErrorType.Permanent:
                    return await HandlePermanentError(exception, context, attemptNumber, cancellationToken);
                
                case ErrorType.Validation:
                    return await HandleValidationError(exception, context, attemptNumber, cancellationToken);
                
                case ErrorType.Authentication:
                    return await HandleAuthenticationError(exception, context, attemptNumber, cancellationToken);
                
                case ErrorType.Business:
                    return await HandleBusinessError(exception, context, attemptNumber, cancellationToken);
                
                case ErrorType.System:
                    return await HandleSystemError(exception, context, attemptNumber, cancellationToken);
                
                default:
                    return await HandleUnknownError(exception, context, attemptNumber, cancellationToken);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in custom error handler");
            return ErrorHandlingResult.Failed("Error in custom error handler");
        }
    }
```

    private async Task<ErrorHandlingResult> HandleTransientError(Exception exception, MessageContext context, int attemptNumber, CancellationToken cancellationToken)
    {
        if (attemptNumber < 3)
        {
            var delay = TimeSpan.FromSeconds(Math.Pow(2, attemptNumber));
            
            _logger.LogWarning("Transient error on attempt {Attempt}. Retrying in {Delay}s", 
                attemptNumber, delay.TotalSeconds);
            
            await Task.Delay(delay, cancellationToken);
            return ErrorHandlingResult.Retry(delay);
        }
        
        _logger.LogError("Transient error exceeded retry limit");
        await SendToDeadLetterQueue(context, exception, "transient.max-retries");
        return ErrorHandlingResult.Success(ErrorHandlingAction.Acknowledge);
    }

    private async Task<ErrorHandlingResult> HandlePermanentError(Exception exception, MessageContext context, int attemptNumber, CancellationToken cancellationToken)
    {
        _logger.LogError("Permanent error detected. Sending to dead letter queue");
        await SendToDeadLetterQueue(context, exception, "permanent.error");
        return ErrorHandlingResult.Success(ErrorHandlingAction.Acknowledge);
    }

    private async Task<ErrorHandlingResult> HandleValidationError(Exception exception, MessageContext context, int attemptNumber, CancellationToken cancellationToken)
    {
        _logger.LogError("Validation error detected. Sending to validation error queue");
        await SendToValidationErrorQueue(context, exception);
        return ErrorHandlingResult.Success(ErrorHandlingAction.Acknowledge);
    }

    private async Task<ErrorHandlingResult> HandleAuthenticationError(Exception exception, MessageContext context, int attemptNumber, CancellationToken cancellationToken)
    {
        _logger.LogError("Authentication error detected. Sending to auth error queue");
        await SendToAuthErrorQueue(context, exception);
        return ErrorHandlingResult.Success(ErrorHandlingAction.Acknowledge);
    }

    private async Task<ErrorHandlingResult> HandleBusinessError(Exception exception, MessageContext context, int attemptNumber, CancellationToken cancellationToken)
    {
        _logger.LogError("Business error detected. Sending to business error queue");
        await SendToBusinessErrorQueue(context, exception);
        return ErrorHandlingResult.Success(ErrorHandlingAction.Acknowledge);
    }

    private async Task<ErrorHandlingResult> HandleSystemError(Exception exception, MessageContext context, int attemptNumber, CancellationToken cancellationToken)
    {
        _logger.LogError("System error detected. Sending to system error queue");
        await SendToSystemErrorQueue(context, exception);
        return ErrorHandlingResult.Success(ErrorHandlingAction.Acknowledge);
    }

    private async Task<ErrorHandlingResult> HandleUnknownError(Exception exception, MessageContext context, int attemptNumber, CancellationToken cancellationToken)
    {
        if (attemptNumber < 1) // Only retry once for unknown errors
        {
            _logger.LogWarning("Unknown error on attempt {Attempt}. Retrying once", attemptNumber);
            await Task.Delay(TimeSpan.FromSeconds(5), cancellationToken);
            return ErrorHandlingResult.Retry(TimeSpan.FromSeconds(5));
        }
        
        _logger.LogError("Unknown error exceeded retry limit");
        await SendToDeadLetterQueue(context, exception, "unknown.error");
        return ErrorHandlingResult.Success(ErrorHandlingAction.Acknowledge);
    }

    private async Task SendToDeadLetterQueue(MessageContext context, Exception exception, string errorType)
    {
        await _streamFlow.InitializeAsync();
        await _streamFlow.Producer.Message(new
        {
            OriginalMessage = context.Message,
            ErrorType = errorType,
            ErrorMessage = exception.Message,
            StackTrace = exception.StackTrace,
            AttemptNumber = context.AttemptCount,
            Timestamp = DateTimeOffset.UtcNow
        })
        .WithExchange("dlx")
        .WithRoutingKey(errorType)
        .PublishAsync();
    }

    private async Task SendToValidationErrorQueue(MessageContext context, Exception exception)
    {
        await _streamFlow.InitializeAsync();
        await _streamFlow.Producer.Message(new
        {
            OriginalMessage = context.Message,
            ValidationErrors = ExtractValidationErrors(exception),
            Timestamp = DateTimeOffset.UtcNow
        })
        .WithExchange("validation-errors")
        .WithRoutingKey("validation.failed")
        .PublishAsync();
    }

    private async Task SendToAuthErrorQueue(MessageContext context, Exception exception)
    {
        await _streamFlow.InitializeAsync();
        await _streamFlow.Producer.Message(new
        {
            OriginalMessage = context.Message,
            AuthError = exception.Message,
            Timestamp = DateTimeOffset.UtcNow
        })
        .WithExchange("auth-errors")
        .WithRoutingKey("auth.failed")
        .PublishAsync();
    }

    private async Task SendToBusinessErrorQueue(MessageContext context, Exception exception)
    {
        await _streamFlow.InitializeAsync();
        await _streamFlow.Producer.Message(new
        {
            OriginalMessage = context.Message,
            BusinessRule = exception.Message,
            Timestamp = DateTimeOffset.UtcNow
        })
        .WithExchange("business-errors")
        .WithRoutingKey("business.rule.violated")
        .PublishAsync();
    }

    private async Task SendToSystemErrorQueue(MessageContext context, Exception exception)
    {
        await _streamFlow.InitializeAsync();
        await _streamFlow.Producer.Message(new
        {
            OriginalMessage = context.Message,
            SystemError = exception.Message,
            StackTrace = exception.StackTrace,
            Timestamp = DateTimeOffset.UtcNow
        })
        .WithExchange("system-errors")
        .WithRoutingKey("system.error")
        .PublishAsync();
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

    public bool CanHandle(Exception exception)
    {
        return true; // Handle all exceptions
    }

    public IRetryPolicy? GetRetryPolicy(Exception exception)
    {
        return null; // Use default retry policy
    }

    public bool ShouldSendToDeadLetterQueue(Exception exception, MessageContext context, int attemptNumber)
    {
        return attemptNumber >= Settings.MaxRetries;
    }
}
```


```

## 📊 Monitoring and Alerting

Monitor error patterns and set up alerting for critical issues.

### Error Monitoring Service

```csharp
public class ErrorMonitoringService
{
    private readonly IStreamFlowClient _streamFlow;
    private readonly ILogger<ErrorMonitoringService> _logger;
    private readonly Dictionary<string, ErrorMetrics> _errorMetrics = new();
    private readonly Timer _monitoringTimer;

    public ErrorMonitoringService(IStreamFlowClient streamFlow, ILogger<ErrorMonitoringService> logger)
    {
        _streamFlow = streamFlow;
        _logger = logger;
        
        // Monitor every minute
        _monitoringTimer = new Timer(MonitorErrors, null, TimeSpan.FromMinutes(1), TimeSpan.FromMinutes(1));
    }
```

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
        await _streamFlow.InitializeAsync();
        await _streamFlow.Producer.Message(new
        {
            AlertType = "HighErrorRate",
            ServiceName = metrics.ServiceName,
            ErrorType = metrics.ErrorType,
            ErrorCount = metrics.ErrorCount,
            LastError = metrics.LastError,
            LastErrorTime = metrics.LastErrorTime,
            Timestamp = DateTimeOffset.UtcNow
        })
        .WithExchange("alerts")
        .WithRoutingKey("error.high-rate")
        .PublishAsync();
    }

    private async Task SendNewErrorTypeAlert(ErrorMetrics metrics)
    {
        await _streamFlow.InitializeAsync();
        await _streamFlow.Producer.Message(new
        {
            AlertType = "NewErrorType",
            ServiceName = metrics.ServiceName,
            ErrorType = metrics.ErrorType,
            ErrorCount = metrics.ErrorCount,
            LastError = metrics.LastError,
            LastErrorTime = metrics.LastErrorTime,
            Timestamp = DateTimeOffset.UtcNow
        })
        .WithExchange("alerts")
        .WithRoutingKey("error.new-type")
        .PublishAsync();
    }

    private async Task SendSystemErrorAlert(ErrorMetrics metrics)
    {
        await _streamFlow.InitializeAsync();
        await _streamFlow.Producer.Message(new
        {
            AlertType = "SystemError",
            ServiceName = metrics.ServiceName,
            ErrorType = metrics.ErrorType,
            ErrorCount = metrics.ErrorCount,
            LastError = metrics.LastError,
            LastErrorTime = metrics.LastErrorTime,
            Timestamp = DateTimeOffset.UtcNow
        })
        .WithExchange("alerts")
        .WithRoutingKey("error.system")
        .PublishAsync();
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
var circuitBreakerPolicy = _retryPolicyFactory.CreateCircuitBreakerPolicy(
    maxRetryAttempts: 3,
    circuitBreakerThreshold: 5,
    circuitBreakerTimeout: TimeSpan.FromMinutes(1));

await circuitBreakerPolicy.ExecuteAsync(async () =>
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
    private readonly IStreamFlowClient _streamFlow;
    private readonly IErrorHandler _errorHandler;

    public ErrorHandlingHealthCheck(IStreamFlowClient streamFlow, IErrorHandler errorHandler)
    {
        _streamFlow = streamFlow;
        _errorHandler = errorHandler;
    }

    public async Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
    {
        var data = new Dictionary<string, object>
        {
            ["error_handler_name"] = _errorHandler.Name,
            ["client_status"] = _streamFlow.Status.ToString()
        };

        var isHealthy = _streamFlow.Status == ClientStatus.Connected;
        
        return isHealthy 
            ? HealthCheckResult.Healthy("Error handling is healthy", data)
            : HealthCheckResult.Unhealthy("StreamFlow client is not connected", data: data);
    }
}
```

## 🎉 Summary

You've now mastered comprehensive error handling in FS.StreamFlow:

✅ **Retry policies and strategies**  
✅ **Circuit breaker pattern implementation**  
✅ **Dead letter queue handling**  
✅ **Error classification and routing**  
✅ **Custom error handlers**  
✅ **Monitoring and alerting**  
✅ **Production-ready best practices**  

## 🎯 Next Steps

Continue your FS.StreamFlow journey:

- [Performance Tuning](performance.md) - Optimize for high throughput
- [Monitoring](monitoring.md) - Monitor your messaging system
- [Examples](examples/) - See real-world examples
- [Configuration](configuration.md) - Advanced configuration options

## 📚 Required Using Statements

```csharp
using FS.StreamFlow.Core.Features.Messaging.Interfaces;
using FS.StreamFlow.Core.Features.Messaging.Models;
using FS.StreamFlow.RabbitMQ.Features.RetryPolicies;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using System.Collections.Concurrent;
``` 