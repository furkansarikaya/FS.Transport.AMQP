# Event-Driven Architecture Example

**Difficulty**: 🟡 Intermediate  
**Focus**: Domain and integration events, decoupled services  
**Time**: 30 minutes

This example demonstrates how to implement an event-driven architecture using FS.StreamFlow. It covers event publishing, subscribing, error handling, and monitoring in a decoupled microservices scenario.

## 📋 What You'll Learn
- Event-driven design patterns
- Domain and integration event flows
- Event handler implementation
- Error handling and retries
- Monitoring event flows

## 🛠️ Setup

### Prerequisites
- .NET 9 SDK
- RabbitMQ server running on localhost
- Basic C# knowledge

### Project Structure
```
EventDrivenArchitecture/
├── Program.cs
├── Models/
│   ├── UserRegistered.cs
│   └── EmailSent.cs
├── Services/
│   ├── UserService.cs
│   └── EmailService.cs
└── EventDrivenArchitecture.csproj
```

## 🏗️ Implementation

### 1. Event Models

```csharp
// Models/UserRegistered.cs
public record UserRegistered(Guid UserId, string Email, DateTime RegisteredAt);

// Models/EmailSent.cs
public record EmailSent(Guid UserId, string Email, DateTime SentAt);
```

### 2. User Service (Publisher)

```csharp
// Services/UserService.cs
using FS.StreamFlow.Core.Features.Messaging.Interfaces;
using Microsoft.Extensions.Logging;

public class UserService
{
    private readonly IStreamFlowClient _streamFlow;
    private readonly ILogger<UserService> _logger;

    public UserService(IStreamFlowClient rabbitMQ, ILogger<UserService> logger)
    {
        _streamFlow = rabbitMQ;
        _logger = logger;
    }

    public async Task RegisterUserAsync(string email)
    {
        var userId = Guid.NewGuid();
        var registeredAt = DateTime.UtcNow;
        // Save user to database (omitted)
        await _streamFlow.EventBus.PublishDomainEventAsync(
            new UserRegistered(userId, email, registeredAt));
        _logger.LogInformation("UserRegistered event published for {Email}", email);
    }
}
```

### 3. Email Service (Subscriber)

```csharp
// Services/EmailService.cs
using FS.StreamFlow.Core.Features.Messaging.Interfaces;
using Microsoft.Extensions.Logging;

public class EmailService : IEventHandler<UserRegistered>
{
    private readonly IStreamFlowClient _streamFlow;
    private readonly ILogger<EmailService> _logger;

    public EmailService(IStreamFlowClient rabbitMQ, ILogger<EmailService> logger)
    {
        _streamFlow = rabbitMQ;
        _logger = logger;
    }

    public async Task HandleAsync(UserRegistered @event, EventContext context)
    {
        // Send welcome email (omitted)
        await _streamFlow.EventBus.PublishIntegrationEventAsync(
            new EmailSent(@event.UserId, @event.Email, DateTime.UtcNow));
        _logger.LogInformation("EmailSent event published for {Email}", @event.Email);
    }
}
```

### 4. Event Handler Registration

```csharp
// Program.cs
using FS.StreamFlow.RabbitMQ.DependencyInjection;
using Microsoft.Extensions.DependencyInjection;

var builder = Host.CreateApplicationBuilder(args);
builder.Services.AddLogging(config =>
{
    config.AddConsole();
    config.SetMinimumLevel(LogLevel.Information);
});
builder.Services.AddRabbitMQ()
    .WithConnectionString("amqp://localhost")
    .WithEventBus(config =>
    {
        config.DomainEventExchange = "domain-events";
        config.IntegrationEventExchange = "integration-events";
        config.EventTypesAssembly = typeof(UserRegistered).Assembly;
    })
    .WithErrorHandling(config =>
    {
        config.EnableDeadLetterQueue = true;
        config.DeadLetterExchange = "dlx";
        config.DeadLetterQueue = "dlq";
        config.MaxRetryAttempts = 3;
        config.RetryDelay = TimeSpan.FromSeconds(2);
    })
    .Build();
builder.Services.AddSingleton<UserService>();
builder.Services.AddSingleton<EmailService>();
var host = builder.Build();
```

### 5. Running the Example

1. **Start RabbitMQ**:
   ```bash
   docker run -d --name rabbitmq -p 5672:5672 -p 15672:15672 rabbitmq:3-management
   ```
2. **Run the application**:
   ```bash
   dotnet run
   ```
3. **Expected flow**:
   ```
   UserRegistered → EmailSent
   ```

## 🛡️ Error Handling
- All failures are routed to a dead letter queue after max retries.
- Handlers log errors and processing failures.

## 📊 Monitoring
- Use RabbitMQ Management UI at http://localhost:15672 to monitor event flows.
- Logs show event publishing and handling in real time.

## 🎯 Key Takeaways
- Event-driven architecture enables decoupled, scalable systems.
- Error handling and monitoring are essential for reliable event flows.
- FS.StreamFlow simplifies event-driven design in .NET. 