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
using FS.StreamFlow.Core.Features.Events.Interfaces;

public record UserRegistered(Guid UserId, string Email, DateTime RegisteredAt) : IDomainEvent
{
    public Guid Id { get; } = Guid.NewGuid();
    public DateTime OccurredOn { get; } = DateTime.UtcNow;
    public int Version { get; } = 1;
    public string EventType => nameof(UserRegistered);
    public string? CorrelationId { get; set; }
    public string? CausationId { get; set; }
    public IDictionary<string, object> Metadata { get; } = new Dictionary<string, object>();
    public string AggregateId => UserId.ToString();
    public string AggregateType => "User";
    public long AggregateVersion { get; set; }
    public string? InitiatedBy { get; set; }
}

// Models/EmailSent.cs
using FS.StreamFlow.Core.Features.Events.Interfaces;

public record EmailSent(Guid UserId, string Email, DateTime SentAt) : IIntegrationEvent
{
    public Guid Id { get; } = Guid.NewGuid();
    public DateTime OccurredOn { get; } = DateTime.UtcNow;
    public int Version { get; } = 1;
    public string EventType => nameof(EmailSent);
    public string? CorrelationId { get; set; }
    public string? CausationId { get; set; }
    public IDictionary<string, object> Metadata { get; } = new Dictionary<string, object>();
    public string Source => "email-service";
    public string ExchangeName => "email-events";
    public string? Target { get; set; }
    public string SchemaVersion => "1.0";
    public TimeSpan? TimeToLive { get; set; }
}
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

    public UserService(IStreamFlowClient streamFlow, ILogger<UserService> logger)
    {
        _streamFlow = streamFlow;
        _logger = logger;
    }

    public async Task RegisterUserAsync(string email)
    {
        // Initialize the client first
        await _streamFlow.InitializeAsync();
        
        var userId = Guid.NewGuid();
        var registeredAt = DateTime.UtcNow;
        
        // Save user to database (omitted)
        // await _userRepository.SaveAsync(user);
        
        // Publish domain event with fluent API
        await _streamFlow.EventBus.Event<UserRegistered>()
            .WithMetadata(metadata =>
            {
                metadata.CorrelationId = Guid.NewGuid().ToString();
                metadata.Source = "user-service";
                metadata.Version = "1.0";
            })
            .WithAggregateId(userId.ToString())
            .WithAggregateType("User")
            .WithPriority(1)
            .WithTtl(TimeSpan.FromMinutes(30))
            .PublishAsync(new UserRegistered(userId, email, registeredAt));
            
        _logger.LogInformation("UserRegistered event published for {Email}", email);
    }
}
```

### 3. Email Service (Subscriber)

```csharp
// Services/EmailService.cs
using FS.StreamFlow.Core.Features.Events.Interfaces;
using FS.StreamFlow.Core.Features.Messaging.Interfaces;
using Microsoft.Extensions.Logging;

public class EmailService : IAsyncEventHandler<UserRegistered>
{
    private readonly IStreamFlowClient _streamFlow;
    private readonly ILogger<EmailService> _logger;

    public EmailService(IStreamFlowClient streamFlow, ILogger<EmailService> logger)
    {
        _streamFlow = streamFlow;
        _logger = logger;
    }

    public async Task HandleAsync(UserRegistered @event, EventContext context)
    {
        try
        {
            // Initialize the client first
            await _streamFlow.InitializeAsync();
            
            _logger.LogInformation("Processing UserRegistered event for user {UserId}", @event.UserId);
            
            // Send welcome email (omitted)
            // await _emailProvider.SendWelcomeEmailAsync(@event.Email);
            
            // Publish integration event with fluent API
            await _streamFlow.EventBus.Event<EmailSent>()
                .WithCorrelationId(context.CorrelationId)
                .WithCausationId(context.EventId)
                .WithSource("email-service")
                .WithVersion("1.0")
                .WithAggregateId(@event.UserId.ToString())
                .WithAggregateType("User")
                .WithProperty("email-type", "welcome")
                .PublishAsync(new EmailSent(@event.UserId, @event.Email, DateTime.UtcNow));
                
            _logger.LogInformation("EmailSent event published for {Email}", @event.Email);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing UserRegistered event for user {UserId}", @event.UserId);
            throw;
        }
    }
}
```

### 4. Program Setup

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
    options.ClientConfiguration.ClientName = "Event-Driven Architecture Example";
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
    
    // Event bus settings
    options.EventBusSettings.EnableEventBus = true;
    options.EventBusSettings.DomainEventExchange = "domain-events";
    options.EventBusSettings.IntegrationEventExchange = "integration-events";
    options.EventBusSettings.EventHandlerAssembly = typeof(UserRegistered).Assembly;
    
    // Error handling settings
    options.ErrorHandlingSettings.MaxRetries = 3;
    options.ErrorHandlingSettings.RetryDelay = TimeSpan.FromSeconds(2);
    options.ErrorHandlingSettings.UseExponentialBackoff = true;
    options.ErrorHandlingSettings.EnableDeadLetterQueue = true;
    
    // Dead letter settings
    options.DeadLetterSettings.ExchangeName = "dlx";
    options.DeadLetterSettings.RoutingKey = "event-handling.failed";
});

// Register services
builder.Services.AddScoped<UserService>();
builder.Services.AddScoped<EmailService>();

var app = builder.Build();

// Initialize the client
var streamFlow = app.Services.GetRequiredService<IStreamFlowClient>();
await streamFlow.InitializeAsync();

// ✅ No manual infrastructure setup needed!
// EventBus automatically creates exchanges and queues when publishing/subscribing
//
// 🚀 Automatic Infrastructure:
// - Domain events: Creates fanout exchanges like "domain.user", "domain.order", etc.
// - Integration events: Creates fanout exchanges using ExchangeName property
// - Queues: Auto-created with pattern "{exchangeName}.{eventType}"
// - Dead letter queues: Can still be configured manually if needed for advanced scenarios

// Start EventBus and subscribe to events
await streamFlow.EventBus.StartAsync();

// Subscribe to domain events using EventBus API
// Domain events are automatically routed to "domain.user" exchange
var emailService = app.Services.GetRequiredService<EmailService>();
await streamFlow.EventBus.SubscribeToDomainEventAsync<UserRegistered>("user", emailService);

// Subscribe to integration events using EventBus API  
// Integration events use the ExchangeName property for routing
var userService = app.Services.GetRequiredService<UserService>();
await streamFlow.EventBus.SubscribeToIntegrationEventAsync<EmailSent>("email-events", userService);

// Start dead letter processor
await streamFlow.Consumer.Queue<DeadLetterMessage>("dead-letter-queue")
    .WithConcurrency(1)
    .WithPrefetchCount(5)
    .ConsumeAsync(async (deadLetterMessage, context) =>
    {
        var logger = app.Services.GetRequiredService<ILogger<Program>>();
        logger.LogWarning("Processing dead letter event {MessageId}: {Reason}", 
            deadLetterMessage.MessageId, deadLetterMessage.Reason);
        
        // Here you could implement retry logic, alerting, or manual intervention
        return true;
    });

// Start the application
app.Run();
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

3. **Test the event flow**:
   ```csharp
   // In a separate console or test
   var userService = app.Services.GetRequiredService<UserService>();
   await userService.RegisterUserAsync("user@example.com");
   ```

4. **Expected flow**:
   ```
   UserRegistered → EmailSent
   ```

5. **Monitor the flow**:
   - Check RabbitMQ Management UI at http://localhost:15672
   - Monitor queues: user-events, email-events, dead-letter-queue
   - View logs for event processing

## 🛡️ Error Handling

### Event Processing Errors
- **Retry Policy**: Transient errors are retried with exponential backoff
- **Dead Letter Queue**: Permanent failures are sent to DLQ
- **Error Classification**: Different error types have different handling strategies

### Error Handling Configuration
```csharp
// Custom error handling for specific events
.WithErrorHandler(async (exception, context) =>
{
    if (exception is ValidationException)
        return false; // Send to DLQ immediately
    
    if (exception is TimeoutException)
        return true; // Retry
    
    return true; // Default retry
})
```

## 📊 Monitoring

### Event Flow Monitoring
```csharp
// Monitor event processing statistics
var eventBusStats = await streamFlow.EventBus.GetStatisticsAsync();
Console.WriteLine($"Events published: {eventBusStats.EventsPublished}");
Console.WriteLine($"Events processed: {eventBusStats.EventsProcessed}");
Console.WriteLine($"Failed events: {eventBusStats.FailedEvents}");
```

### Dead Letter Queue Monitoring
```csharp
// Monitor dead letter queue
var dlqStats = await streamFlow.DeadLetterHandler.GetStatisticsAsync();
Console.WriteLine($"Dead letter messages: {dlqStats.TotalMessages}");
Console.WriteLine($"Reprocessed: {dlqStats.ReprocessedMessages}");
```

### RabbitMQ Management
- **Management UI**: http://localhost:15672 (guest/guest)
- **Queue Monitoring**: Track message rates and processing times
- **Exchange Monitoring**: Monitor event routing and delivery
- **Connection Monitoring**: Track client connections and health

## 🎯 Key Takeaways

- **Event-driven architecture** enables decoupled, scalable systems
- **Domain and integration events** provide clear separation of concerns
- **Fluent API** simplifies event publishing and subscription
- **Error handling and monitoring** are essential for reliable event flows
- **Dead letter queues** ensure no events are lost
- **FS.StreamFlow simplifies** event-driven design in .NET applications
- **Correlation and causation IDs** enable event tracing and debugging
- **Event versioning** supports schema evolution and backward compatibility 