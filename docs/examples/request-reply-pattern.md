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
using FS.StreamFlow.Core.Features.Messaging.Models;
using Microsoft.Extensions.Logging;

public class RequesterService
{
    private readonly IStreamFlowClient _streamFlow;
    private readonly ILogger<RequesterService> _logger;

    public RequesterService(IStreamFlowClient streamFlow, ILogger<RequesterService> logger)
    {
        _streamFlow = streamFlow;
        _logger = logger;
    }

    public async Task SendRequestAsync(object request, string replyTo)
    {
        // Initialize the client first
        await _streamFlow.InitializeAsync();
        
        var correlationId = Guid.NewGuid().ToString();
        
        // Use fluent API for publishing
        await _streamFlow.Producer.Message(request)
            .WithExchange("requests")
            .WithRoutingKey("calculate.sum")
            .WithCorrelationId(correlationId)
            .WithReplyTo(replyTo)
            .PublishAsync();
            
        _logger.LogInformation("Request sent with CorrelationId {CorrelationId}", correlationId);
    }
}
```

### 2. Responder Service

```csharp
// Services/ResponderService.cs
using FS.StreamFlow.Core.Features.Messaging.Interfaces;
using FS.StreamFlow.Core.Features.Messaging.Models;
using Microsoft.Extensions.Logging;

public class ResponderService
{
    private readonly IStreamFlowClient _streamFlow;
    private readonly ILogger<ResponderService> _logger;

    public ResponderService(IStreamFlowClient streamFlow, ILogger<ResponderService> logger)
    {
        _streamFlow = streamFlow;
        _logger = logger;
    }

    public async Task StartRespondingAsync()
    {
        // Initialize the client first
        await _streamFlow.InitializeAsync();
        
        await _streamFlow.Consumer.Queue<object>("calculate-requests")
            .WithConcurrency(3)
            .WithPrefetchCount(50)
            .WithErrorHandler(async (exception, context) =>
            {
                return exception is ConnectFailureException;
            })
            .ConsumeAsync(async (request, context) =>
            {
                // Process request and send response
                var result = new { Result = 42 }; // Simulated result
                
                await _streamFlow.Producer.Message(result)
                    .WithExchange("") // Default exchange
                    .WithRoutingKey(context.Properties.ReplyTo)
                    .WithCorrelationId(context.Properties.CorrelationId)
                    .PublishAsync();
                    
                return true; // Acknowledge message
            });
    }
}
```

### 3. Program.cs

```csharp
// Program.cs
using FS.StreamFlow.RabbitMQ.DependencyInjection;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

var builder = Host.CreateApplicationBuilder(args);

// Add FS.StreamFlow with RabbitMQ
builder.Services.AddRabbitMQStreamFlow(options =>
{
    // Client configuration
    options.ClientConfiguration.ClientName = "Request-Reply Pattern Example";
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
    
    // Producer settings
    options.ProducerSettings.EnablePublisherConfirms = true;
    options.ProducerSettings.ConfirmationTimeout = TimeSpan.FromSeconds(10);
    options.ProducerSettings.MaxConcurrentPublishes = 100;
    
    // Consumer settings
    options.ConsumerSettings.PrefetchCount = 50;
    options.ConsumerSettings.AutoAcknowledge = false;
    options.ConsumerSettings.MaxConcurrentConsumers = 5;
});

// Add services
builder.Services.AddScoped<RequesterService>();
builder.Services.AddScoped<ResponderService>();

var host = builder.Build();

// Initialize StreamFlow client
var streamFlow = host.Services.GetRequiredService<IStreamFlowClient>();
await streamFlow.InitializeAsync();

// Setup infrastructure
await streamFlow.ExchangeManager.Exchange("requests")
    .AsTopic()
    .WithDurable(true)
    .DeclareAsync();
    
await streamFlow.QueueManager.Queue("calculate-requests")
    .WithDurable(true)
    .WithDeadLetterExchange("dlx")
    .BindToExchange("requests", "calculate.sum")
    .DeclareAsync();

// Start responder service
var responderService = host.Services.GetRequiredService<ResponderService>();
await responderService.StartRespondingAsync();

// Simulate request-reply pattern
var requesterService = host.Services.GetRequiredService<RequesterService>();

// Send a request
await requesterService.SendRequestAsync(
    request: new { Numbers = new[] { 1, 2, 3, 4, 5 } },
    replyTo: "response-queue");

Console.WriteLine("Request-reply pattern example started. Press any key to exit.");
Console.ReadKey();
```

## 🛡️ Error Handling
- All failures are routed to a dead letter queue after max retries
- Services log errors and processing failures
- Connection failures are automatically retried with exponential backoff

## 📊 Monitoring
- Use RabbitMQ Management UI at http://localhost:15672 to monitor request and response queues
- Logs show request and response activity in real time
- Health checks are available for request-reply services

## 🎯 Key Takeaways
- Request-reply enables synchronous messaging over RabbitMQ
- Correlation IDs are essential for tracking responses
- FS.StreamFlow simplifies request-reply flows with fluent APIs
- Always call InitializeAsync() before using the client
- Use proper message properties for correlation and reply-to routing 