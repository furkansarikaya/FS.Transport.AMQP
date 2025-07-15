# Request-Reply Pattern Example

**Difficulty**: 🟡 Intermediate  
**Focus**: Synchronous communication  
**Time**: 20 minutes

This example demonstrates how to implement the request-reply messaging pattern using FS.StreamFlow. It covers synchronous communication, correlation IDs, error handling, and monitoring.

## 📋 What You'll Learn
- Request-reply pattern for synchronous messaging
- Using correlation IDs for message tracking
- Error handling for request-reply flows
- Monitoring request and response queues

## 🛠️ Setup

### Prerequisites
- .NET 9 SDK
- RabbitMQ server running on localhost
- Basic C# knowledge

### Project Structure
```
RequestReplyPattern/
├── Program.cs
├── Services/
│   ├── RequesterService.cs
│   └── ResponderService.cs
└── RequestReplyPattern.csproj
```

## 🏗️ Implementation

### 1. Requester Service

```csharp
// Services/RequesterService.cs
using FS.StreamFlow.Core.Features.Messaging.Interfaces;
using Microsoft.Extensions.Logging;

public class RequesterService
{
    private readonly IRabbitMQClient _rabbitMQ;
    private readonly ILogger<RequesterService> _logger;

    public RequesterService(IRabbitMQClient rabbitMQ, ILogger<RequesterService> logger)
    {
        _rabbitMQ = rabbitMQ;
        _logger = logger;
    }

    public async Task SendRequestAsync(object request, string replyTo)
    {
        var correlationId = Guid.NewGuid().ToString();
        await _rabbitMQ.Producer.PublishAsync(
            exchange: "requests",
            routingKey: "calculate.sum",
            message: request,
            properties: new BasicProperties { CorrelationId = correlationId, ReplyTo = replyTo });
        _logger.LogInformation("Request sent with CorrelationId {CorrelationId}", correlationId);
    }
}
```

### 2. Responder Service

```csharp
// Services/ResponderService.cs
using FS.StreamFlow.Core.Features.Messaging.Interfaces;
using Microsoft.Extensions.Logging;

public class ResponderService
{
    private readonly IRabbitMQClient _rabbitMQ;
    private readonly ILogger<ResponderService> _logger;

    public ResponderService(IRabbitMQClient rabbitMQ, ILogger<ResponderService> logger)
    {
        _rabbitMQ = rabbitMQ;
        _logger = logger;
    }

    public async Task StartRespondingAsync(CancellationToken cancellationToken)
    {
        await _rabbitMQ.Consumer.ConsumeAsync<object>(
            queueName: "calculate-requests",
            messageHandler: async (request, context) =>
            {
                // Process request and send response
                var result = new { Result = 42 }; // Simulated result
                await _rabbitMQ.Producer.PublishAsync(
                    exchange: "",
                    routingKey: context.ReplyTo,
                    message: result,
                    properties: new BasicProperties { CorrelationId = context.CorrelationId });
                return true;
            },
            cancellationToken: cancellationToken);
    }
}
```

## 🛡️ Error Handling
- All failures are routed to a dead letter queue after max retries.
- Services log errors and processing failures.

## 📊 Monitoring
- Use RabbitMQ Management UI at http://localhost:15672 to monitor request and response queues.
- Logs show request and response activity in real time.

## 🎯 Key Takeaways
- Request-reply enables synchronous messaging over RabbitMQ.
- Correlation IDs are essential for tracking responses.
- FS.StreamFlow simplifies request-reply flows. 