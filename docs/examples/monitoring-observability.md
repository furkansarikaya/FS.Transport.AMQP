# Monitoring and Observability Example

**Difficulty**: 🟡 Intermediate  
**Focus**: Production monitoring setup  
**Time**: 20 minutes

This example demonstrates how to implement monitoring and observability for FS.StreamFlow-based systems. It covers health checks, metrics, and logging.

## 📋 What You'll Learn
- Health check integration
- Metrics collection and reporting
- Logging best practices
- Performance monitoring

## 🛠️ Setup

### Prerequisites
- .NET 9 SDK
- RabbitMQ server running on localhost
- Basic C# knowledge

### Project Structure
```
MonitoringObservability/
├── Program.cs
├── Services/
│   ├── MonitoringService.cs
│   └── MessageProcessor.cs
└── MonitoringObservability.csproj
```

## 🏗️ Implementation

### 1. Program.cs - Complete Setup

```csharp
// Program.cs
using FS.StreamFlow.RabbitMQ.DependencyInjection;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Diagnostics.HealthChecks;

var builder = Host.CreateApplicationBuilder(args);

// Add FS.StreamFlow with RabbitMQ
builder.Services.AddRabbitMQStreamFlow(options =>
{
    // Client configuration
    options.ClientConfiguration.ClientName = "Monitoring Example";
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

// Add health checks
builder.Services.AddHealthChecks()
    .AddCheck("streamflow", async () =>
    {
        var streamFlow = host.Services.GetRequiredService<IStreamFlowClient>();
        await streamFlow.InitializeAsync();
        
        var healthResult = await streamFlow.HealthChecker.CheckHealthAsync();
        
        if (healthResult.Status == HealthStatus.Healthy)
        {
            return Microsoft.Extensions.Diagnostics.HealthChecks.HealthCheckResult.Healthy(
                "StreamFlow is healthy",
                new Dictionary<string, object>
                {
                    ["description"] = healthResult.Description,
                    ["duration"] = healthResult.Duration.TotalMilliseconds,
                    ["timestamp"] = healthResult.Timestamp
                });
        }
        else
        {
            return Microsoft.Extensions.Diagnostics.HealthChecks.HealthCheckResult.Unhealthy(
                "StreamFlow is unhealthy",
                healthResult.Exception,
                new Dictionary<string, object>
                {
                    ["description"] = healthResult.Description,
                    ["status"] = healthResult.Status.ToString(),
                    ["duration"] = healthResult.Duration.TotalMilliseconds,
                    ["timestamp"] = healthResult.Timestamp
                });
        }
    }, tags: new[] { "messaging" });

// Add logging
builder.Services.AddLogging(config =>
{
    config.AddConsole();
    config.SetMinimumLevel(LogLevel.Information);
});

// Register services
builder.Services.AddScoped<MonitoringService>();
builder.Services.AddScoped<MessageProcessor>();

var host = builder.Build();

// Initialize StreamFlow client
var streamFlow = host.Services.GetRequiredService<IStreamFlowClient>();
await streamFlow.InitializeAsync();

// Setup infrastructure
await streamFlow.ExchangeManager.Exchange("monitoring")
    .AsTopic()
    .WithDurable(true)
    .DeclareAsync();
    
await streamFlow.QueueManager.Queue("message-processing")
    .WithDurable(true)
    .WithDeadLetterExchange("dlx")
    .BindToExchange("monitoring", "message.*")
    .DeclareAsync();

// Start services
var monitoringService = host.Services.GetRequiredService<MonitoringService>();
var messageProcessor = host.Services.GetRequiredService<MessageProcessor>();

await messageProcessor.StartAsync();

Console.WriteLine("Monitoring and observability example started. Press any key to exit.");
Console.ReadKey();
```

### 2. Health Check Implementation

FS.StreamFlow provides built-in health checking through the `IHealthChecker` interface. The health check is already configured in the Program.cs above using a lambda function that directly uses the framework's health checking capabilities.

For more advanced health check scenarios, you can also use the health checker directly in your services:

```csharp
// Direct health check usage in services
public class HealthMonitoringService
{
    private readonly IStreamFlowClient _streamFlow;
    private readonly ILogger<HealthMonitoringService> _logger;

    public HealthMonitoringService(IStreamFlowClient streamFlow, ILogger<HealthMonitoringService> logger)
    {
        _streamFlow = streamFlow;
        _logger = logger;
    }

    public async Task<HealthCheckResult> CheckHealthAsync()
    {
        // Initialize the client first
        await _streamFlow.InitializeAsync();
        
        // Use built-in health checker
        return await _streamFlow.HealthChecker.CheckHealthAsync();
    }

    public async Task<Dictionary<string, HealthCheckResult>> GetAllComponentHealthAsync()
    {
        // Initialize the client first
        await _streamFlow.InitializeAsync();
        
        // Get health status of all components
        return await _streamFlow.HealthChecker.GetAllHealthStatusesAsync();
    }
}
```

### 3. Monitoring Service

```csharp
// Services/MonitoringService.cs
using FS.StreamFlow.Core.Features.Messaging.Interfaces;
using Microsoft.Extensions.Logging;

public class MonitoringService
{
    private readonly IStreamFlowClient _streamFlow;
    private readonly ILogger<MonitoringService> _logger;

    public MonitoringService(IStreamFlowClient streamFlow, ILogger<MonitoringService> logger)
    {
        _streamFlow = streamFlow;
        _logger = logger;
    }

    public async Task RecordMetricsAsync()
    {
        try
        {
            // Get connection statistics
            var connectionStats = await _streamFlow.ConnectionManager.GetStatisticsAsync();
            _logger.LogInformation("Connection Statistics - Active Connections: {ActiveConnections}, Total Messages: {TotalMessages}", 
                connectionStats.ActiveConnections, connectionStats.TotalMessagesSent);

            // Get queue statistics
            var queueStats = await _streamFlow.QueueManager.GetStatisticsAsync("message-processing");
            _logger.LogInformation("Queue Statistics - Queue: {QueueName}, Message Count: {MessageCount}, Consumer Count: {ConsumerCount}", 
                queueStats.QueueName, queueStats.MessageCount, queueStats.ConsumerCount);

            // Get exchange statistics
            var exchangeStats = await _streamFlow.ExchangeManager.GetStatisticsAsync("monitoring");
            _logger.LogInformation("Exchange Statistics - Exchange: {ExchangeName}, Message Count: {MessageCount}", 
                exchangeStats.ExchangeName, exchangeStats.MessageCount);

            // Get producer statistics
            var producerStats = await _streamFlow.Producer.GetStatisticsAsync();
            _logger.LogInformation("Producer Statistics - Messages Sent: {MessagesSent}, Failed Messages: {FailedMessages}", 
                producerStats.MessagesSent, producerStats.FailedMessages);

            // Get consumer statistics
            var consumerStats = await _streamFlow.Consumer.GetStatisticsAsync();
            _logger.LogInformation("Consumer Statistics - Messages Processed: {MessagesProcessed}, Failed Messages: {FailedMessages}", 
                consumerStats.MessagesProcessed, consumerStats.FailedMessages);

            // Get event bus statistics
            var eventBusStats = await _streamFlow.EventBus.GetStatisticsAsync();
            _logger.LogInformation("Event Bus Statistics - Events Published: {EventsPublished}, Events Consumed: {EventsConsumed}", 
                eventBusStats.EventsPublished, eventBusStats.EventsConsumed);

        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error recording metrics");
        }
    }

    public async Task StartMetricsCollectionAsync()
    {
        // Start periodic metrics collection
        var timer = new Timer(async _ => await RecordMetricsAsync(), null, TimeSpan.Zero, TimeSpan.FromMinutes(1));
        
        _logger.LogInformation("Metrics collection started");
    }
}
```

### 4. Message Processor with Monitoring

```csharp
// Services/MessageProcessor.cs
using FS.StreamFlow.Core.Features.Messaging.Interfaces;
using Microsoft.Extensions.Logging;

public class MessageProcessor
{
    private readonly IStreamFlowClient _streamFlow;
    private readonly ILogger<MessageProcessor> _logger;

    public MessageProcessor(IStreamFlowClient streamFlow, ILogger<MessageProcessor> logger)
    {
        _streamFlow = streamFlow;
        _logger = logger;
    }

    public async Task StartAsync()
    {
        // Initialize the client first
        await _streamFlow.InitializeAsync();

        // Start consumer with monitoring
        await _streamFlow.Consumer.Queue<TestMessage>("message-processing")
            .WithConcurrency(3)
            .WithPrefetchCount(50)
            .WithErrorHandler(async (exception, context) =>
            {
                _logger.LogError(exception, "Error processing message {MessageId}", context.MessageId);
                return exception is ConnectFailureException;
            })
            .ConsumeAsync(async (message, context) =>
            {
                _logger.LogInformation("Processing message {MessageId} with content: {Content}", 
                    context.MessageId, message.Content);
                
                // Simulate processing time
                await Task.Delay(100);
                
                _logger.LogInformation("Message {MessageId} processed successfully", context.MessageId);
                return true; // Acknowledge message
            });

        // Start periodic message publishing for testing
        _ = Task.Run(async () =>
        {
            while (true)
            {
                try
                {
                    var testMessage = new TestMessage { Content = $"Test message at {DateTime.UtcNow}" };
                    
                    await _streamFlow.Producer.Message(testMessage)
                        .WithExchange("monitoring")
                        .WithRoutingKey("message.test")
                        .WithDeliveryMode(DeliveryMode.Persistent)
                        .PublishAsync();
                    
                    _logger.LogInformation("Test message published");
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error publishing test message");
                }
                
                await Task.Delay(TimeSpan.FromSeconds(30));
            }
        });
    }
}

// Test message model
public class TestMessage
{
    public string Content { get; set; } = string.Empty;
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;
}
```

### 5. Health Check Endpoint (Web Application)

```csharp
// For web applications, add health check endpoint
// Program.cs (Web Application)
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;

var builder = WebApplication.CreateBuilder(args);

// Add services (same as above)
builder.Services.AddRabbitMQStreamFlow(options => { /* configuration */ });
builder.Services.AddHealthChecks()
    .AddCheck("streamflow", async () =>
    {
        var streamFlow = app.Services.GetRequiredService<IStreamFlowClient>();
        await streamFlow.InitializeAsync();
        
        var healthResult = await streamFlow.HealthChecker.CheckHealthAsync();
        
        if (healthResult.Status == HealthStatus.Healthy)
        {
            return Microsoft.Extensions.Diagnostics.HealthChecks.HealthCheckResult.Healthy(
                "StreamFlow is healthy",
                new Dictionary<string, object>
                {
                    ["description"] = healthResult.Description,
                    ["duration"] = healthResult.Duration.TotalMilliseconds,
                    ["timestamp"] = healthResult.Timestamp
                });
        }
        else
        {
            return Microsoft.Extensions.Diagnostics.HealthChecks.HealthCheckResult.Unhealthy(
                "StreamFlow is unhealthy",
                healthResult.Exception,
                new Dictionary<string, object>
                {
                    ["description"] = healthResult.Description,
                    ["status"] = healthResult.Status.ToString(),
                    ["duration"] = healthResult.Duration.TotalMilliseconds,
                    ["timestamp"] = healthResult.Timestamp
                });
        }
    }, tags: new[] { "messaging" });

var app = builder.Build();

// Map health check endpoint
app.MapHealthChecks("/health", new HealthCheckOptions
{
    ResponseWriter = async (context, report) =>
    {
        context.Response.ContentType = "application/json";
        var result = new
        {
            status = report.Status.ToString(),
            checks = report.Entries.Select(e => new
            {
                name = e.Key,
                status = e.Value.Status.ToString(),
                description = e.Value.Description
            })
        };
        await context.Response.WriteAsJsonAsync(result);
    }
});

app.MapHealthChecks("/health/ready", new HealthCheckOptions
{
    Predicate = check => check.Tags.Contains("messaging")
});

app.MapHealthChecks("/health/live", new HealthCheckOptions
{
    Predicate = _ => false
});

app.Run();
```

## 📊 Monitoring

### Health Check Endpoints
- **Overall Health**: `GET /health` - Complete system health status
- **Readiness**: `GET /health/ready` - Service readiness for traffic
- **Liveness**: `GET /health/live` - Service is running

### Metrics Available
- Connection statistics (active connections, total messages)
- Queue statistics (message count, consumer count)
- Exchange statistics (message count)
- Producer statistics (messages sent, failed messages)
- Consumer statistics (messages processed, failed messages)
- Event bus statistics (events published, events consumed)

### Logging
- Structured logging with correlation IDs
- Performance metrics in real-time
- Error tracking and monitoring
- Connection health status

### External Monitoring
- Use RabbitMQ Management UI at http://localhost:15672
- Monitor queues, exchanges, and connections
- Track message rates and consumer performance

## 🎯 Key Takeaways
- Monitoring and observability are essential for production systems
- FS.StreamFlow provides comprehensive health checks and metrics
- Always call InitializeAsync() before using the client
- Use structured logging for better observability
- Health checks help with container orchestration and load balancers
- Metrics collection enables performance optimization and alerting 