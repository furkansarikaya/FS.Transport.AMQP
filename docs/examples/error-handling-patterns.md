# Error Handling Patterns Example

**Difficulty**: 🟡 Intermediate  
**Focus**: Comprehensive error handling  
**Time**: 25 minutes

This example demonstrates how to implement robust error handling patterns using FS.RabbitMQ. It covers retry, dead letter, and circuit breaker patterns.

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
using FS.RabbitMQ.Core;
using Microsoft.Extensions.Logging;

public class ErrorProneService
{
    private readonly IRabbitMQClient _rabbitMQ;
    private readonly ILogger<ErrorProneService> _logger;

    public ErrorProneService(IRabbitMQClient rabbitMQ, ILogger<ErrorProneService> logger)
    {
        _rabbitMQ = rabbitMQ;
        _logger = logger;
    }

    public async Task ProcessAsync(object message)
    {
        // Simulate error
        throw new InvalidOperationException("Simulated failure");
    }
}
```

### 2. Error Handler

```csharp
// Services/ErrorHandler.cs
using FS.RabbitMQ.ErrorHandling;
using Microsoft.Extensions.Logging;

public class ErrorHandler : IErrorHandler
{
    private readonly ILogger<ErrorHandler> _logger;

    public ErrorHandler(ILogger<ErrorHandler> logger)
    {
        _logger = logger;
    }

    public async Task<ErrorHandlingResult> HandleErrorAsync(ErrorContext context, CancellationToken cancellationToken)
    {
        _logger.LogError(context.Exception, "Error processing message");
        // Retry, dead letter, or circuit breaker logic here
        return ErrorHandlingResult.Retry;
    }
}
```

## 🛡️ Error Handling
- Retry policy for transient errors
- Dead letter queue for persistent failures
- Circuit breaker for repeated errors

## 📊 Monitoring
- Use RabbitMQ Management UI at http://localhost:15672 to monitor error queues.
- Logs show error handling actions in real time.

## 🎯 Key Takeaways
- Robust error handling is critical for reliable messaging.
- FS.RabbitMQ provides built-in support for common error handling patterns. 