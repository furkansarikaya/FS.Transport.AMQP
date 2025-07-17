# Error Handling Patterns Example

**Difficulty**: 🟡 Intermediate  
**Focus**: Comprehensive error handling  
**Time**: 25 minutes

This example demonstrates how to implement robust error handling patterns using FS.StreamFlow. It covers retry, dead letter, and circuit breaker patterns.

## 📋 What You'll Learn
- Retry policies for transient errors
- Dead letter queue usage
- Circuit breaker for persistent failures

## 🛠️ Setup

### Prerequisites
- .NET 9 SDK
- RabbitMQ server running on localhost
- Basic C# knowledge

### Project Structure
```
ErrorHandlingPatterns/
├── Program.cs
├── Services/
│   ├── ErrorProneService.cs
│   └── ErrorHandler.cs
└── ErrorHandlingPatterns.csproj
```

## 🏗️ Implementation

### 1. Error-Prone Service

```csharp
// Services/ErrorProneService.cs
using FS.StreamFlow.Core.Features.Messaging.Interfaces;
using Microsoft.Extensions.Logging;

public class ErrorProneService
{
    private readonly IStreamFlowClient _streamFlow;
    private readonly ILogger<ErrorProneService> _logger;

    public ErrorProneService(IStreamFlowClient streamFlow, ILogger<ErrorProneService> logger)
    {
        _streamFlow = streamFlow;
        _logger = logger;
    }

    public async Task StartProcessingAsync()
    {
        // Initialize the client first
        await _streamFlow.InitializeAsync();
        
        // Setup error-prone consumer with comprehensive error handling
        await _streamFlow.Consumer.Queue<ProcessingMessage>("error-prone-queue")
            .WithConcurrency(3)
            .WithPrefetchCount(10)
            .WithErrorHandler(async (exception, context) =>
            {
                _logger.LogError(exception, "Error processing message {MessageId}", context.MessageId);
                
                // Custom error handling logic
                if (exception is InvalidOperationException)
                {
                    return true; // Retry
                }
                
                if (exception is ArgumentException)
                {
                    return false; // Send to dead letter queue
                }
                
                return true; // Default retry
            })
            .WithRetryPolicy(new RetryPolicySettings
            {
                RetryPolicy = RetryPolicyType.ExponentialBackoff,
                MaxRetryAttempts = 3,
                InitialRetryDelay = TimeSpan.FromSeconds(1),
                MaxRetryDelay = TimeSpan.FromSeconds(30),
                RetryDelayMultiplier = 2.0,
                UseJitter = true
            })
            .WithDeadLetterQueue(new DeadLetterSettings
            {
                ExchangeName = "dlx",
                RoutingKey = "error-prone.failed",
                Enabled = true,
                MaxRetries = 3,
                MessageTtl = TimeSpan.FromHours(24)
            })
            .ConsumeAsync(async (message, context) =>
            {
                await ProcessAsync(message);
                return true;
            });
    }

    public async Task ProcessAsync(ProcessingMessage message)
    {
        _logger.LogInformation("Processing message {MessageId}", message.Id);
        
        // Simulate different types of errors
        var random = new Random();
        var errorType = random.Next(1, 6);
        
        switch (errorType)
        {
            case 1:
                throw new InvalidOperationException("Simulated transient failure");
            case 2:
                throw new ArgumentException("Simulated validation error");
            case 3:
                throw new TimeoutException("Simulated timeout");
            case 4:
                throw new UnauthorizedAccessException("Simulated authentication error");
            case 5:
                // Success case
                _logger.LogInformation("Message {MessageId} processed successfully", message.Id);
                break;
        }
    }
}

public class ProcessingMessage
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public string Content { get; set; } = string.Empty;
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;
}
```

### 2. Error Handler

```csharp
// Services/ErrorHandler.cs
using FS.StreamFlow.Core.Features.Messaging.Interfaces;
using FS.StreamFlow.Core.Features.Messaging.Models;
using Microsoft.Extensions.Logging;

public class CustomErrorHandler : IErrorHandler
{
    private readonly ILogger<CustomErrorHandler> _logger;
    private readonly IStreamFlowClient _streamFlow;

    public CustomErrorHandler(ILogger<CustomErrorHandler> logger, IStreamFlowClient streamFlow)
    {
        _logger = logger;
        _streamFlow = streamFlow;
    }

    public string Name => "Custom Error Handler";

    public ErrorHandlingSettings Settings { get; } = new ErrorHandlingSettings
    {
        MaxRetries = 3,
        RetryDelay = TimeSpan.FromSeconds(1),
        UseExponentialBackoff = true,
        EnableDeadLetterQueue = true,
        Strategy = ErrorHandlingStrategy.Requeue
    };

    public event EventHandler<ErrorHandledEventArgs>? ErrorHandled;
    public event EventHandler<DeadLetterEventArgs>? MessageSentToDeadLetterQueue;

    public async Task<ErrorHandlingResult> HandleErrorAsync(Exception exception, MessageContext context, CancellationToken cancellationToken = default)
    {
        return await HandleErrorAsync(exception, context, 1, cancellationToken);
    }

    public async Task<ErrorHandlingResult> HandleErrorAsync(Exception exception, MessageContext context, int attemptNumber, CancellationToken cancellationToken = default)
    {
        _logger.LogError(exception, "Handling error for message {MessageId} on attempt {AttemptNumber}", 
            context.MessageId, attemptNumber);

        try
        {
            // Initialize the client first
            await _streamFlow.InitializeAsync();

            // Classify error and determine action
            var errorType = ClassifyError(exception);
            
            switch (errorType)
            {
                case ErrorType.Transient:
                    return await HandleTransientError(exception, context, attemptNumber);
                
                case ErrorType.Permanent:
                    return await HandlePermanentError(exception, context, attemptNumber);
                
                case ErrorType.Validation:
                    return await HandleValidationError(exception, context, attemptNumber);
                
                case ErrorType.Authentication:
                    return await HandleAuthenticationError(exception, context, attemptNumber);
                
                default:
                    return await HandleUnknownError(exception, context, attemptNumber);
            }
        }
        catch (Exception handlerException)
        {
            _logger.LogError(handlerException, "Error handler failed for message {MessageId}", context.MessageId);
            return ErrorHandlingResult.Failed($"Error handler failed: {handlerException.Message}");
        }
    }

    public bool CanHandle(Exception exception)
    {
        return exception != null;
    }

    public IRetryPolicy? GetRetryPolicy(Exception exception)
    {
        var errorType = ClassifyError(exception);
        
        return errorType switch
        {
            ErrorType.Transient => new ExponentialBackoffRetryPolicy(3, TimeSpan.FromSeconds(1)),
            ErrorType.Validation => null, // No retry for validation errors
            ErrorType.Authentication => new LinearRetryPolicy(2, TimeSpan.FromSeconds(5)),
            _ => new ExponentialBackoffRetryPolicy(1, TimeSpan.FromSeconds(2))
        };
    }

    public bool ShouldSendToDeadLetterQueue(Exception exception, MessageContext context, int attemptNumber)
    {
        var errorType = ClassifyError(exception);
        
        return errorType switch
        {
            ErrorType.Permanent => true,
            ErrorType.Validation => true,
            ErrorType.Authentication => attemptNumber >= 2,
            _ => attemptNumber >= Settings.MaxRetries
        };
    }

    private ErrorType ClassifyError(Exception exception)
    {
        return exception switch
        {
            TimeoutException => ErrorType.Transient,
            InvalidOperationException => ErrorType.Transient,
            ArgumentException => ErrorType.Validation,
            UnauthorizedAccessException => ErrorType.Authentication,
            NotSupportedException => ErrorType.Permanent,
            _ => ErrorType.Unknown
        };
    }

    private async Task<ErrorHandlingResult> HandleTransientError(Exception exception, MessageContext context, int attemptNumber)
    {
        _logger.LogWarning("Transient error for message {MessageId}, attempt {AttemptNumber}", 
            context.MessageId, attemptNumber);
        
        var retryDelay = TimeSpan.FromSeconds(Math.Pow(2, attemptNumber - 1));
        return ErrorHandlingResult.Retry(retryDelay);
    }

    private async Task<ErrorHandlingResult> HandlePermanentError(Exception exception, MessageContext context, int attemptNumber)
    {
        _logger.LogError("Permanent error for message {MessageId}, sending to dead letter queue", 
            context.MessageId);
        
        await SendToDeadLetterQueue(exception, context, attemptNumber);
        return ErrorHandlingResult.DeadLetter($"Permanent error: {exception.Message}");
    }

    private async Task<ErrorHandlingResult> HandleValidationError(Exception exception, MessageContext context, int attemptNumber)
    {
        _logger.LogWarning("Validation error for message {MessageId}, sending to dead letter queue", 
            context.MessageId);
        
        await SendToDeadLetterQueue(exception, context, attemptNumber);
        return ErrorHandlingResult.DeadLetter($"Validation error: {exception.Message}");
    }

    private async Task<ErrorHandlingResult> HandleAuthenticationError(Exception exception, MessageContext context, int attemptNumber)
    {
        _logger.LogWarning("Authentication error for message {MessageId}, attempt {AttemptNumber}", 
            context.MessageId, attemptNumber);
        
        if (attemptNumber >= 2)
        {
            await SendToDeadLetterQueue(exception, context, attemptNumber);
            return ErrorHandlingResult.DeadLetter($"Authentication error: {exception.Message}");
        }
        
        return ErrorHandlingResult.Retry(TimeSpan.FromSeconds(5));
    }

    private async Task<ErrorHandlingResult> HandleUnknownError(Exception exception, MessageContext context, int attemptNumber)
    {
        _logger.LogError("Unknown error for message {MessageId}, attempt {AttemptNumber}", 
            context.MessageId, attemptNumber);
        
        if (attemptNumber >= Settings.MaxRetries)
        {
            await SendToDeadLetterQueue(exception, context, attemptNumber);
            return ErrorHandlingResult.DeadLetter($"Unknown error: {exception.Message}");
        }
        
        return ErrorHandlingResult.Retry(TimeSpan.FromSeconds(2));
    }

    private async Task SendToDeadLetterQueue(Exception exception, MessageContext context, int attemptNumber)
    {
        try
        {
            await _streamFlow.Producer.Message(new DeadLetterMessage
            {
                MessageId = context.MessageId,
                Data = context.MessageBody ?? Array.Empty<byte>(),
                Context = context,
                Exception = exception,
                AttemptNumber = attemptNumber,
                Reason = exception.Message
            })
            .WithExchange("dlx")
            .WithRoutingKey("error-handling.failed")
            .PublishAsync();
            
            MessageSentToDeadLetterQueue?.Invoke(this, new DeadLetterEventArgs(
                context.MessageBody ?? Array.Empty<byte>(), 
                context, 
                exception, 
                attemptNumber));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to send message {MessageId} to dead letter queue", context.MessageId);
        }
    }
}

public enum ErrorType
{
    Transient,
    Permanent,
    Validation,
    Authentication,
    Unknown
}
```

### 3. Program Setup

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
    options.ClientConfiguration.ClientName = "Error Handling Example";
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
    
    // Error handling settings
    options.ErrorHandlingSettings.MaxRetries = 3;
    options.ErrorHandlingSettings.RetryDelay = TimeSpan.FromSeconds(1);
    options.ErrorHandlingSettings.UseExponentialBackoff = true;
    options.ErrorHandlingSettings.EnableDeadLetterQueue = true;
    options.ErrorHandlingSettings.Strategy = ErrorHandlingStrategy.Custom;
    
    // Dead letter settings
    options.DeadLetterSettings.ExchangeName = "dlx";
    options.DeadLetterSettings.RoutingKey = "error-handling.failed";
});

// Register services
builder.Services.AddScoped<ErrorProneService>();
builder.Services.AddScoped<CustomErrorHandler>();

var app = builder.Build();

// Initialize the client
var streamFlow = app.Services.GetRequiredService<IStreamFlowClient>();
await streamFlow.InitializeAsync();

// Setup infrastructure
await streamFlow.ExchangeManager.Exchange("dlx")
    .AsTopic()
    .WithDurable(true)
    .DeclareAsync();

await streamFlow.QueueManager.Queue("error-prone-queue")
    .WithDurable(true)
    .WithDeadLetterExchange("dlx")
    .WithDeadLetterRoutingKey("error-prone.failed")
    .DeclareAsync();

await streamFlow.QueueManager.Queue("dead-letter-queue")
    .WithDurable(true)
    .BindToExchange("dlx", "error-handling.failed")
    .DeclareAsync();

// Start error-prone service
var errorProneService = app.Services.GetRequiredService<ErrorProneService>();
await errorProneService.StartProcessingAsync();

// Start dead letter processor
await streamFlow.Consumer.Queue<DeadLetterMessage>("dead-letter-queue")
    .WithConcurrency(1)
    .WithPrefetchCount(5)
    .ConsumeAsync(async (deadLetterMessage, context) =>
    {
        var logger = app.Services.GetRequiredService<ILogger<Program>>();
        logger.LogWarning("Processing dead letter message {MessageId}: {Reason}", 
            deadLetterMessage.MessageId, deadLetterMessage.Reason);
        
        // Here you could implement retry logic, alerting, or manual intervention
        return true;
    });

app.Run();
```

## 🛡️ Error Handling Patterns

### Retry Policy
- **Exponential Backoff**: Retry with increasing delays
- **Linear Retry**: Retry with fixed delays
- **Circuit Breaker**: Stop retrying after threshold

### Dead Letter Queue
- **Permanent Failures**: Send to DLQ immediately
- **Validation Errors**: No retry, direct to DLQ
- **Max Retries Exceeded**: Send to DLQ after all attempts

### Error Classification
- **Transient**: Network issues, timeouts
- **Permanent**: Unsupported operations
- **Validation**: Invalid data, format errors
- **Authentication**: Access denied, expired tokens

## 📊 Monitoring

### Dead Letter Queue Monitoring
```csharp
// Monitor dead letter queue statistics
var statistics = await streamFlow.DeadLetterHandler.GetStatisticsAsync();
Console.WriteLine($"Dead letter messages: {statistics.TotalMessages}");
Console.WriteLine($"Reprocessed: {statistics.ReprocessedMessages}");
Console.WriteLine($"Failed reprocessing: {statistics.FailedReprocessing}");
```

### Error Metrics
- Use RabbitMQ Management UI at http://localhost:15672
- Monitor error queues and retry patterns
- Track dead letter queue growth
- Alert on high error rates

## 🎯 Key Takeaways

- **Robust error handling** is critical for reliable messaging
- **FS.StreamFlow provides built-in support** for common error handling patterns
- **Custom error handlers** allow fine-grained control over error responses
- **Dead letter queues** ensure no messages are lost
- **Retry policies** handle transient failures automatically
- **Error classification** helps determine appropriate handling strategies