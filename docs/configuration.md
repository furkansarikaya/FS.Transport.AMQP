# Configuration Reference

This comprehensive guide covers all configuration options available in FS.StreamFlow, including connection settings, producer and consumer configurations, error handling, and advanced features.

## Overview

FS.StreamFlow provides flexible configuration options through:

- **Options Pattern**: Type-safe configuration using `IOptions<T>`
- **Configuration Files**: JSON-based configuration with `appsettings.json`
- **Environment Variables**: Environment-specific overrides
- **Fluent API**: Programmatic configuration with lambda expressions
- **Dependency Injection**: Full integration with Microsoft.Extensions.DependencyInjection

## Basic Configuration

### Quick Start

```csharp
using FS.StreamFlow.RabbitMQ.DependencyInjection;

// Program.cs
builder.Services.AddRabbitMQStreamFlow(options =>
{
    // Client configuration
    options.ClientConfiguration.ClientName = "My Application";
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
```

### Configuration from appsettings.json

```json
{
  "RabbitMQ": {
    "ClientConfiguration": {
      "ClientName": "My Application",
      "EnableAutoRecovery": true,
      "EnableHeartbeat": true,
      "HeartbeatInterval": "00:01:00"
    },
    "ConnectionSettings": {
      "Host": "localhost",
      "Port": 5672,
      "Username": "guest",
      "Password": "guest",
      "VirtualHost": "/",
      "ConnectionTimeout": "00:00:30",
      "UseSsl": false
    },
    "ProducerSettings": {
      "EnablePublisherConfirms": true,
      "ConfirmationTimeout": "00:00:10",
      "MaxConcurrentPublishes": 100,
      "PublishTimeout": "00:00:30"
    },
    "ConsumerSettings": {
      "PrefetchCount": 50,
      "AutoAcknowledge": false,
      "MaxConcurrentConsumers": 5,
      "ConsumerTimeout": "00:00:30"
    }
  }
}
```

```csharp
// Program.cs
builder.Services.Configure<RabbitMQStreamFlowOptions>(
    builder.Configuration.GetSection("StreamFlow:RabbitMQ"));

builder.Services.AddRabbitMQStreamFlow();
```

### Using Fluent APIs After Configuration

Once configured, you can use the fluent APIs to manage infrastructure and messaging:

```csharp
public class StartupService
{
    private readonly IStreamFlowClient _streamFlow;
    
    public StartupService(IStreamFlowClient streamFlow)
    {
        _streamFlow = streamFlow;
    }
    
    public async Task InitializeAsync()
    {
        // Setup infrastructure with fluent APIs
        await SetupExchangesAsync();
        await SetupQueuesAsync();
        await SetupEventStreamsAsync();
        
        // Start consuming messages
        await StartConsumersAsync();
    }
    
    private async Task SetupExchangesAsync()
    {
        // Create exchanges with fluent API
        await _streamFlow.ExchangeManager.Exchange("orders")
            .AsTopic()
            .WithDurable(true)
            .WithAlternateExchange("alt-orders")
            .DeclareAsync();
            
        await _streamFlow.ExchangeManager.Exchange("notifications")
            .AsFanout()
            .WithDurable(true)
            .DeclareAsync();
    }
    
    private async Task SetupQueuesAsync()
    {
        // Create queues with fluent API
        await _streamFlow.QueueManager.Queue("order-processing")
            .WithDurable(true)
            .WithDeadLetterExchange("dlx")
            .WithMessageTtl(TimeSpan.FromHours(24))
            .WithMaxLength(10000)
            .BindToExchange("orders", "order.created")
            .DeclareAsync();
    }
    
    private async Task SetupEventStreamsAsync()
    {
        // Create event streams with fluent API
        await _streamFlow.EventStore.Stream("order-events")
            .CreateAsync();
            
        await _streamFlow.EventStore.Stream("user-events")
            .CreateAsync();
    }
    
    private async Task StartConsumersAsync()
    {
        // Start consuming with fluent API
        _ = Task.Run(async () =>
        {
            await _streamFlow.Consumer.Queue<Order>("order-processing")
                .WithConcurrency(5)
                .WithPrefetch(100)
                .WithErrorHandling(ErrorHandlingStrategy.Retry)
                .HandleAsync(async (order, context) =>
                {
                    await ProcessOrderAsync(order);
                    return true;
                });
        });
    }
}
```

## Connection Configuration

### Basic Connection Settings

```csharp
builder.Services.AddRabbitMQStreamFlow(options =>
{
    // Client configuration
    options.ClientConfiguration.ClientName = "MyApplication";
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
    options.ConnectionSettings.RequestTimeout = TimeSpan.FromSeconds(30);
    options.ConnectionSettings.UseSsl = false;
});
```

### Connection String Formats

```csharp
// Use individual connection settings instead of connection string
options.ConnectionSettings.Host = "localhost";
options.ConnectionSettings.Port = 5672;
options.ConnectionSettings.Username = "username";
options.ConnectionSettings.Password = "password";
options.ConnectionSettings.VirtualHost = "/myvhost";
```

## SSL/TLS Configuration

### Basic SSL Configuration

```csharp
builder.Services.AddRabbitMQStreamFlow(options =>
{
    // Client configuration
    options.ClientConfiguration.ClientName = "SSL Application";
    options.ClientConfiguration.EnableAutoRecovery = true;
    options.ClientConfiguration.EnableHeartbeat = true;
    options.ClientConfiguration.HeartbeatInterval = TimeSpan.FromSeconds(60);
    
    // Connection settings with SSL
    options.ConnectionSettings.Host = "rabbitmq.example.com";
    options.ConnectionSettings.Port = 5671;
    options.ConnectionSettings.Username = "guest";
    options.ConnectionSettings.Password = "guest";
    options.ConnectionSettings.VirtualHost = "/";
    options.ConnectionSettings.ConnectionTimeout = TimeSpan.FromSeconds(30);
    options.ConnectionSettings.UseSsl = true;
    
    // SSL settings
    options.ConnectionSettings.Ssl = new SslSettings
    {
        Enabled = true,
        CertificatePath = "/path/to/certificate.pfx",
        CertificatePassword = "password",
        VerifyCertificate = true,
        ProtocolVersion = "Tls12"
    };
});
```

### Certificate-Based Authentication

```csharp
builder.Services.AddRabbitMQStreamFlow(options =>
{
    options.Ssl.Enabled = true;
    options.Ssl.CertificatePath = "/path/to/client-certificate.pfx";
    options.Ssl.CertificatePassword = "certificate-password";
    options.Ssl.CertificateSelectionCallback = (sender, targetHost, localCertificates, remoteCertificate, acceptableIssuers) =>
    {
        // Custom certificate selection logic
        return localCertificates[0];
    };
});
```

### Custom SSL Configuration

```json
{
  "StreamFlow": {
    "RabbitMQ": {
      "ConnectionString": "amqps://localhost:5671",
      "Ssl": {
        "Enabled": true,
        "ServerName": "rabbitmq.example.com",
        "CertificatePath": "/path/to/certificate.pfx",
        "CertificatePassword": "password",
        "AcceptablePolicyErrors": "None",
        "Version": "Tls12,Tls13",
        "CheckCertificateRevocation": true
      }
    }
  }
}
```

## Producer Configuration

### Basic Producer Settings

```csharp
builder.Services.AddRabbitMQStreamFlow(options =>
{
    options.Producer.EnableConfirmations = true;
    options.Producer.ConfirmationTimeout = TimeSpan.FromSeconds(5);
    options.Producer.BatchSize = 100;
    options.Producer.MaxBatchWaitTime = TimeSpan.FromSeconds(1);
    options.Producer.EnableTransactions = false;
    options.Producer.TransactionTimeout = TimeSpan.FromSeconds(30);
    options.Producer.MaxRetryAttempts = 3;
    options.Producer.RetryDelay = TimeSpan.FromSeconds(1);
    options.Producer.EnableCompression = false;
    options.Producer.CompressionLevel = CompressionLevel.Fastest;
});
```

### Performance Optimization

```csharp
builder.Services.AddRabbitMQStreamFlow(options =>
{
    // High-throughput settings
    options.Producer.EnableConfirmations = true;
    options.Producer.BatchSize = 1000;
    options.Producer.MaxBatchWaitTime = TimeSpan.FromMilliseconds(100);
    options.Producer.MaxConcurrentPublishes = 100;
    options.Producer.PublishTimeout = TimeSpan.FromSeconds(10);
    options.Producer.UseBackgroundThreads = true;
    options.Producer.ChannelPoolSize = 10;
});
```

### Producer Retry Configuration

```csharp
builder.Services.AddRabbitMQStreamFlow(options =>
{
    options.Producer.RetryPolicy = new ExponentialBackoffRetryPolicy
    {
        MaxRetryAttempts = 5,
        InitialDelay = TimeSpan.FromMilliseconds(500),
        MaxDelay = TimeSpan.FromSeconds(30),
        BackoffMultiplier = 2.0,
        Jitter = true
    };
});
```

## Consumer Configuration

### Basic Consumer Settings

```csharp
builder.Services.AddRabbitMQStreamFlow(options =>
{
    options.Consumer.PrefetchCount = 50;
    options.Consumer.ConcurrentConsumers = 5;
    options.Consumer.AutoAck = false;
    options.Consumer.RequeueOnFailure = true;
    options.Consumer.MaxConsumerConcurrency = 10;
    options.Consumer.ConsumerTag = "my-consumer";
    options.Consumer.Priority = 0;
    options.Consumer.Exclusive = false;
    options.Consumer.Arguments = new Dictionary<string, object>();
});
```

### Consumer Performance Tuning

```csharp
builder.Services.AddRabbitMQStreamFlow(options =>
{
    // High-throughput consumer settings
    options.Consumer.PrefetchCount = 100;
    options.Consumer.ConcurrentConsumers = 10;
    options.Consumer.MaxConsumerConcurrency = 20;
    options.Consumer.ProcessingTimeout = TimeSpan.FromSeconds(30);
    options.Consumer.UseBackgroundThreads = true;
    options.Consumer.ChannelPoolSize = 5;
});
```

### Consumer Error Handling

```csharp
builder.Services.AddRabbitMQStreamFlow(options =>
{
    options.Consumer.ErrorHandling.Strategy = ErrorHandlingStrategy.Retry;
    options.Consumer.ErrorHandling.MaxRetryAttempts = 3;
    options.Consumer.ErrorHandling.RetryDelay = TimeSpan.FromSeconds(2);
    options.Consumer.ErrorHandling.RequeueOnFailure = true;
    options.Consumer.ErrorHandling.SendToDeadLetterQueueOnFailure = true;
});
```

## Error Handling Configuration

### Dead Letter Queue Configuration

```csharp
builder.Services.AddRabbitMQStreamFlow(options =>
{
    options.ErrorHandling.EnableDeadLetterQueue = true;
    options.ErrorHandling.DeadLetterExchange = "dlx";
    options.ErrorHandling.DeadLetterQueue = "dlq";
    options.ErrorHandling.DeadLetterRoutingKey = "failed";
    options.ErrorHandling.MessageTtl = TimeSpan.FromHours(24);
    options.ErrorHandling.MaxMessageLength = 1000000;
    options.ErrorHandling.MaxQueueLength = 10000;
});
```

### Retry Policy Configuration

```csharp
builder.Services.AddRabbitMQStreamFlow(options =>
{
    // Exponential backoff retry policy
    options.ErrorHandling.RetryPolicy = RetryPolicyType.ExponentialBackoff;
    options.ErrorHandling.MaxRetryAttempts = 5;
    options.ErrorHandling.InitialRetryDelay = TimeSpan.FromSeconds(1);
    options.ErrorHandling.MaxRetryDelay = TimeSpan.FromMinutes(5);
    options.ErrorHandling.BackoffMultiplier = 2.0;
    options.ErrorHandling.RetryJitter = true;
});
```

### Circuit Breaker Configuration

```csharp
builder.Services.AddRabbitMQStreamFlow(options =>
{
    options.ErrorHandling.CircuitBreaker.Enabled = true;
    options.ErrorHandling.CircuitBreaker.FailureThreshold = 5;
    options.ErrorHandling.CircuitBreaker.RecoveryTimeout = TimeSpan.FromMinutes(1);
    options.ErrorHandling.CircuitBreaker.HalfOpenRetryCount = 3;
    options.ErrorHandling.CircuitBreaker.TimeWindow = TimeSpan.FromMinutes(5);
});
```

## Serialization Configuration

### JSON Serialization

```csharp
builder.Services.AddRabbitMQStreamFlow(options =>
{
    options.Serialization.Format = SerializationFormat.Json;
    options.Serialization.UseCompression = true;
    options.Serialization.CompressionLevel = CompressionLevel.Fastest;
    options.Serialization.JsonOptions = new JsonSerializerOptions
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        WriteIndented = false,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
    };
});
```

### Binary Serialization

```csharp
builder.Services.AddRabbitMQStreamFlow(options =>
{
    options.Serialization.Format = SerializationFormat.Binary;
    options.Serialization.UseCompression = true;
    options.Serialization.EnableTypeInformation = true;
});
```

### MessagePack Serialization

```csharp
builder.Services.AddRabbitMQStreamFlow(options =>
{
    options.Serialization.Format = SerializationFormat.MessagePack;
    options.Serialization.MessagePackOptions = MessagePackSerializerOptions.Standard
        .WithCompression(MessagePackCompression.Lz4Block);
});
```

## Event Bus Configuration

### Basic Event Bus Settings

```csharp
builder.Services.AddRabbitMQStreamFlow(options =>
{
    options.EnableEventBus = true;
    options.EventBus.DomainEventExchange = "domain-events";
    options.EventBus.IntegrationEventExchange = "integration-events";
    options.EventBus.EventTypeHeaderName = "event-type";
    options.EventBus.CorrelationIdHeaderName = "correlation-id";
    options.EventBus.CausationIdHeaderName = "causation-id";
    options.EventBus.EnableEventTypeDiscovery = true;
    options.EventBus.EventTypesAssembly = typeof(Program).Assembly;
});
```

### Event Store Configuration

```csharp
builder.Services.AddRabbitMQStreamFlow(options =>
{
    options.EnableEventStore = true;
    options.EventStore.StreamPrefix = "eventstore";
    options.EventStore.SnapshotInterval = 100;
    options.EventStore.EnableSnapshots = true;
    options.EventStore.SnapshotPrefix = "snapshot";
    options.EventStore.MaxEventsPerRead = 1000;
    options.EventStore.EnableEventTypeDiscovery = true;
    options.EventStore.EventTypesAssembly = typeof(Program).Assembly;
});
```

## Health Checks Configuration

### Basic Health Check Settings

```csharp
builder.Services.AddRabbitMQStreamFlow(options =>
{
    options.EnableHealthChecks = true;
    options.HealthCheck.Enabled = true;
    options.HealthCheck.Interval = TimeSpan.FromSeconds(30);
    options.HealthCheck.Timeout = TimeSpan.FromSeconds(5);
    options.HealthCheck.FailureThreshold = 3;
    options.HealthCheck.SuccessThreshold = 2;
    options.HealthCheck.CheckConnection = true;
    options.HealthCheck.CheckQueues = true;
    options.HealthCheck.CheckExchanges = true;
    options.HealthCheck.CheckConsumers = true;
});
```

### Custom Health Check Configuration

```csharp
builder.Services.AddRabbitMQStreamFlow(options =>
{
    options.HealthCheck.CustomChecks.Add("queue-depth", new QueueDepthHealthCheck
    {
        QueueName = "critical-queue",
        MaxDepth = 1000,
        WarningThreshold = 500
    });
    
    options.HealthCheck.CustomChecks.Add("consumer-lag", new ConsumerLagHealthCheck
    {
        QueueName = "processing-queue",
        MaxLag = TimeSpan.FromMinutes(5)
    });
});
```

## Monitoring Configuration

### Metrics Collection

```csharp
builder.Services.AddRabbitMQStreamFlow(options =>
{
    options.EnableMonitoring = true;
    options.Monitoring.EnableMetrics = true;
    options.Monitoring.MetricsInterval = TimeSpan.FromSeconds(60);
    options.Monitoring.EnableStatistics = true;
    options.Monitoring.StatisticsInterval = TimeSpan.FromSeconds(30);
    options.Monitoring.EnablePerformanceCounters = true;
    options.Monitoring.MetricsPrefix = "streamflow";
    options.Monitoring.IncludeHostname = true;
    options.Monitoring.IncludeApplicationName = true;
});
```

### Logging Configuration

```csharp
builder.Services.AddRabbitMQStreamFlow(options =>
{
    options.Logging.EnableVerboseLogging = false;
    options.Logging.LogLevel = LogLevel.Information;
    options.Logging.LogMessagePayload = false;
    options.Logging.LogMessageHeaders = true;
    options.Logging.LogConnectionEvents = true;
    options.Logging.LogChannelEvents = true;
    options.Logging.MaxPayloadLength = 1000;
});
```

## Environment-Specific Configuration

### Development Configuration

```json
{
  "StreamFlow": {
    "RabbitMQ": {
      "ConnectionString": "amqp://guest:guest@localhost:5672",
      "Connection": {
        "AutomaticRecovery": true,
        "HeartbeatInterval": "00:00:30"
      },
      "Producer": {
        "EnableConfirmations": false,
        "BatchSize": 10
      },
      "Consumer": {
        "PrefetchCount": 10,
        "ConcurrentConsumers": 2
      },
      "ErrorHandling": {
        "EnableDeadLetterQueue": false,
        "MaxRetryAttempts": 1
      },
      "Logging": {
        "EnableVerboseLogging": true,
        "LogLevel": "Debug"
      }
    }
  }
}
```

### Production Configuration

```json
{
  "StreamFlow": {
    "RabbitMQ": {
      "ConnectionString": "amqps://username:password@rabbitmq.production.com:5671",
      "Connection": {
        "AutomaticRecovery": true,
        "HeartbeatInterval": "00:01:00",
        "MaxChannels": 200,
        "ConnectionTimeout": "00:00:30"
      },
      "Ssl": {
        "Enabled": true,
        "ServerName": "rabbitmq.production.com",
        "CertificatePath": "/path/to/certificate.pfx",
        "CheckCertificateRevocation": true
      },
      "Producer": {
        "EnableConfirmations": true,
        "BatchSize": 1000,
        "MaxBatchWaitTime": "00:00:00.100"
      },
      "Consumer": {
        "PrefetchCount": 100,
        "ConcurrentConsumers": 10,
        "MaxConsumerConcurrency": 20
      },
      "ErrorHandling": {
        "EnableDeadLetterQueue": true,
        "RetryPolicy": "ExponentialBackoff",
        "MaxRetryAttempts": 5,
        "CircuitBreaker": {
          "Enabled": true,
          "FailureThreshold": 10,
          "RecoveryTimeout": "00:05:00"
        }
      },
      "EnableHealthChecks": true,
      "EnableMonitoring": true,
      "Logging": {
        "LogLevel": "Information",
        "LogMessagePayload": false
      }
    }
  }
}
```

## Configuration Validation

### Built-in Validation

```csharp
builder.Services.AddRabbitMQStreamFlow(options =>
{
    // Connection settings
    options.ConnectionSettings.Host = "localhost";
    options.ConnectionSettings.Port = 5672;
    options.ConnectionSettings.Username = "guest";
    options.ConnectionSettings.Password = "guest";
    options.ConnectionSettings.VirtualHost = "/";
    
    // Additional configuration options
    options.EnableConfigurationValidation = true;
    options.ValidateOnStart = true;
    options.ThrowOnValidationErrors = true;
});
```

### Custom Validation

```csharp
builder.Services.AddRabbitMQStreamFlow(options =>
{
    options.ConnectionString = "amqp://localhost";
    options.CustomValidators.Add(new CustomConfigurationValidator());
});

public class CustomConfigurationValidator : IConfigurationValidator
{
    public ValidationResult Validate(RabbitMQStreamFlowOptions options)
    {
        var errors = new List<string>();
        
        if (options.Consumer.PrefetchCount > 1000)
        {
            errors.Add("PrefetchCount should not exceed 1000 for optimal performance");
        }
        
        if (options.Producer.BatchSize > 10000)
        {
            errors.Add("BatchSize should not exceed 10000");
        }
        
        return new ValidationResult(errors);
    }
}
```

## Advanced Configuration Scenarios

### Multi-Environment Configuration

```csharp
builder.Services.AddRabbitMQStreamFlow(options =>
{
    var environment = builder.Environment.EnvironmentName;
    
    switch (environment)
    {
        case "Development":
            options.ConnectionString = "amqp://localhost";
            options.Producer.EnableConfirmations = false;
            options.Consumer.PrefetchCount = 10;
            break;
            
        case "Staging":
            options.ConnectionString = builder.Configuration.GetConnectionString("RabbitMQ-Staging");
            options.Producer.EnableConfirmations = true;
            options.Consumer.PrefetchCount = 50;
            break;
            
        case "Production":
            options.ConnectionString = builder.Configuration.GetConnectionString("RabbitMQ-Production");
            options.Producer.EnableConfirmations = true;
            options.Consumer.PrefetchCount = 100;
            options.Ssl.Enabled = true;
            break;
    }
});
```

### Configuration from Environment Variables

```csharp
// Environment variables
// STREAMFLOW_RABBITMQ_CONNECTIONSTRING=amqp://localhost
// STREAMFLOW_RABBITMQ_PRODUCER_BATCHSIZE=100
// STREAMFLOW_RABBITMQ_CONSUMER_PREFETCHCOUNT=50

builder.Configuration.AddEnvironmentVariables("STREAMFLOW_");

builder.Services.Configure<RabbitMQStreamFlowOptions>(
    builder.Configuration.GetSection("RabbitMQ"));
```

### Configuration with Azure Key Vault

```csharp
builder.Configuration.AddAzureKeyVault(
    keyVaultUrl: "https://myvault.vault.azure.net/",
    credential: new DefaultAzureCredential());

builder.Services.AddRabbitMQStreamFlow(options =>
{
    options.ConnectionString = builder.Configuration["rabbitmq-connection-string"];
    options.Ssl.CertificatePassword = builder.Configuration["rabbitmq-certificate-password"];
});
```

## Configuration Best Practices

### 1. Use Configuration Validation

```csharp
builder.Services.AddRabbitMQStreamFlow(options =>
{
    options.EnableConfigurationValidation = true;
    options.ValidateOnStart = true;
});
```

### 2. Environment-Specific Settings

```csharp
builder.Services.AddRabbitMQStreamFlow(options =>
{
    if (builder.Environment.IsDevelopment())
    {
        options.Logging.EnableVerboseLogging = true;
        options.Producer.EnableConfirmations = false;
    }
    else
    {
        options.Ssl.Enabled = true;
        options.Producer.EnableConfirmations = true;
    }
});
```

### 3. Secure Sensitive Data

```csharp
// Use secure configuration sources
builder.Configuration.AddUserSecrets<Program>();
builder.Configuration.AddAzureKeyVault(vaultUrl, credential);

// Don't hardcode sensitive values
builder.Services.AddRabbitMQStreamFlow(options =>
{
    options.ConnectionString = builder.Configuration.GetConnectionString("RabbitMQ");
    options.Ssl.CertificatePassword = builder.Configuration["RabbitMQ:CertificatePassword"];
});
```

### 4. Monitor Configuration Changes

```csharp
builder.Services.Configure<RabbitMQStreamFlowOptions>(
    builder.Configuration.GetSection("StreamFlow:RabbitMQ"));

builder.Services.AddSingleton<IOptionsMonitor<RabbitMQStreamFlowOptions>>();
```

## Troubleshooting Configuration Issues

### Common Configuration Problems

1. **Connection String Format**
   ```csharp
   // Incorrect
   options.ConnectionString = "localhost";
   
   // Correct
   options.ConnectionString = "amqp://localhost";
   ```

2. **SSL Configuration**
   ```csharp
   // Ensure SSL is properly configured
   options.ConnectionString = "amqps://hostname:5671";
   options.Ssl.Enabled = true;
   options.Ssl.ServerName = "hostname";
   ```

3. **Performance Settings**
   ```csharp
   // Balance prefetch count with consumer concurrency
   options.Consumer.PrefetchCount = 50;
   options.Consumer.ConcurrentConsumers = 5;
   ```

### Configuration Debugging

```csharp
builder.Services.AddRabbitMQStreamFlow(options =>
{
    options.EnableConfigurationValidation = true;
    options.Logging.EnableVerboseLogging = true;
    
    // Log configuration on startup
    options.LogConfigurationOnStartup = true;
});
```

## Configuration Schema Reference

For a complete reference of all configuration options, see the [RabbitMQStreamFlowOptions class](https://github.com/furkansarikaya/FS.StreamFlow/blob/main/src/FS.StreamFlow.RabbitMQ/Configuration/RabbitMQStreamFlowOptions.cs).

## Next Steps

- [Producer Configuration](producer.md#configuration)
- [Consumer Configuration](consumer.md#configuration)
- [Error Handling Configuration](error-handling.md#configuration)
- [Performance Tuning](performance.md#configuration)
- [Monitoring Configuration](monitoring.md#configuration) 