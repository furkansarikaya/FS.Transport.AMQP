# Monitoring and Observability Example

**Difficulty**: 🟡 Intermediate  
**Focus**: Production monitoring setup  
**Time**: 20 minutes

This example demonstrates how to implement monitoring and observability for FS.RabbitMQ-based systems. It covers health checks, metrics, and logging.

## 📋 What You'll Learn
- Health check integration
- Metrics collection and reporting
- Logging best practices

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
│   └── MonitoringService.cs
└── MonitoringObservability.csproj
```

## 🏗️ Implementation

### 1. Health Checks

```csharp
// Program.cs
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Diagnostics.HealthChecks;

var builder = Host.CreateApplicationBuilder(args);
builder.Services.AddHealthChecks()
    .AddCheck<RabbitMQHealthCheck>("rabbitmq");

var app = builder.Build();
app.MapHealthChecks("/health");
```

### 2. Metrics Collection

```csharp
// Services/MonitoringService.cs
using Microsoft.Extensions.Logging;

public class MonitoringService
{
    private readonly ILogger<MonitoringService> _logger;

    public MonitoringService(ILogger<MonitoringService> logger)
    {
        _logger = logger;
    }

    public void RecordMetric(string name, double value)
    {
        _logger.LogInformation("Metric {Name}: {Value}", name, value);
    }
}
```

### 3. Logging

```csharp
// Program.cs
builder.Services.AddLogging(config =>
{
    config.AddConsole();
    config.SetMinimumLevel(LogLevel.Information);
});
```

## 📊 Monitoring
- Use RabbitMQ Management UI at http://localhost:15672 to monitor queues and exchanges.
- Health check endpoint at `/health`.
- Logs show metrics and health status in real time.

## 🎯 Key Takeaways
- Monitoring and observability are essential for production systems.
- FS.RabbitMQ integrates with .NET health checks and logging. 