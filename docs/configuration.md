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
    // Connection settings with SSL
    options.ConnectionSettings.Host = "localhost";
    options.ConnectionSettings.Port = 5671;
    options.ConnectionSettings.Username = "guest";
    options.ConnectionSettings.Password = "guest";
    options.ConnectionSettings.VirtualHost = "/";
    options.ConnectionSettings.UseSsl = true;
    
    // SSL settings
    options.ConnectionSettings.Ssl = new SslSettings
    {
        Enabled = true,
        CertificatePath = "/path/to/client-certificate.pfx",
        CertificatePassword = "certificate-password",
        VerifyCertificate = true,
        ProtocolVersion = "Tls12"
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
    // Producer configuration
    options.ProducerSettings.EnablePublisherConfirms = true;
    options.ProducerSettings.ConfirmationTimeout = TimeSpan.FromSeconds(5);
    options.ProducerSettings.MaxConcurrentPublishes = 100;
    options.ProducerSettings.PublishTimeout = TimeSpan.FromSeconds(30);
});
```

### Performance Optimization

```csharp
builder.Services.AddRabbitMQStreamFlow(options =>
{
    // High-throughput settings
    options.ProducerSettings.EnablePublisherConfirms = true;
    options.ProducerSettings.MaxConcurrentPublishes = 100;
    options.ProducerSettings.PublishTimeout = TimeSpan.FromSeconds(10);
});
```

### Producer Retry Configuration

```csharp
builder.Services.AddRabbitMQStreamFlow(options =>
{
    options.ProducerSettings.RetryPolicy = new RetryPolicySettings
    {
        MaxRetryAttempts = 5,
        InitialRetryDelay = TimeSpan.FromMilliseconds(500),
        MaxRetryDelay = TimeSpan.FromSeconds(30),
        RetryDelayMultiplier = 2.0,
        UseExponentialBackoff = true,
        UseJitter = true
    };
});
```

## Consumer Configuration

### Basic Consumer Settings

```csharp
// Consumer configuration
options.ConsumerSettings.PrefetchCount = 50;
options.ConsumerSettings.MaxConcurrentConsumers = 5;
options.ConsumerSettings.AutoAcknowledge = false;
options.ConsumerSettings.ConsumerTag = "my-consumer";
options.ConsumerSettings.Exclusive = false;
```

### Consumer Performance Tuning

```csharp
builder.Services.AddRabbitMQStreamFlow(options =>
{
    // High-throughput consumer settings
    options.ConsumerSettings.PrefetchCount = 100;
    options.ConsumerSettings.MaxConcurrentConsumers = 10;
    options.ConsumerSettings.MessageProcessingTimeout = TimeSpan.FromSeconds(30);
});
```

### Consumer Error Handling

```csharp
builder.Services.AddRabbitMQStreamFlow(options =>
{
    // Consumer error handling settings
    options.ConsumerSettings.ErrorHandling.Strategy = ErrorHandlingStrategy.Requeue;
    options.ConsumerSettings.ErrorHandling.MaxRetries = 3;
    options.ConsumerSettings.ErrorHandling.RetryDelay = TimeSpan.FromSeconds(2);
    options.ConsumerSettings.ErrorHandling.LogErrors = true;
    options.ConsumerSettings.ErrorHandling.ContinueOnError = true;
    
    // Dead letter queue settings
    options.ConsumerSettings.EnableDeadLetterQueue = true;
    options.ConsumerSettings.DeadLetterSettings.ExchangeName = "dlx";
    options.ConsumerSettings.DeadLetterSettings.RoutingKey = "failed";
    options.ConsumerSettings.DeadLetterSettings.MaxRetries = 3;
    options.ConsumerSettings.DeadLetterSettings.MessageTtl = TimeSpan.FromHours(24);
});
```

## Error Handling Configuration

### Dead Letter Queue Configuration

```csharp
builder.Services.AddRabbitMQStreamFlow(options =>
{
    // Dead letter queue settings
    options.ConsumerSettings.EnableDeadLetterQueue = true;
    options.ConsumerSettings.DeadLetterSettings.ExchangeName = "dlx";
    options.ConsumerSettings.DeadLetterSettings.RoutingKey = "failed";
    options.ConsumerSettings.DeadLetterSettings.MaxRetries = 3;
    options.ConsumerSettings.DeadLetterSettings.MessageTtl = TimeSpan.FromHours(24);
    
    // Error handling settings
    options.ConsumerSettings.ErrorHandling.Strategy = ErrorHandlingStrategy.Reject;
    options.ConsumerSettings.ErrorHandling.MaxRetries = 3;
    options.ConsumerSettings.ErrorHandling.LogErrors = true;
});
```

### Retry Policy Configuration

```csharp
builder.Services.AddRabbitMQStreamFlow(options =>
{
    // Consumer retry policy settings
    options.ConsumerSettings.RetryPolicy.MaxRetryAttempts = 5;
    options.ConsumerSettings.RetryPolicy.InitialRetryDelay = TimeSpan.FromSeconds(1);
    options.ConsumerSettings.RetryPolicy.MaxRetryDelay = TimeSpan.FromMinutes(5);
    options.ConsumerSettings.RetryPolicy.RetryDelayMultiplier = 2.0;
    options.ConsumerSettings.RetryPolicy.UseExponentialBackoff = true;
    options.ConsumerSettings.RetryPolicy.UseJitter = true;
    
    // Producer retry policy settings
    options.ProducerSettings.RetryPolicy.MaxRetryAttempts = 3;
    options.ProducerSettings.RetryPolicy.InitialRetryDelay = TimeSpan.FromMilliseconds(500);
    options.ProducerSettings.RetryPolicy.MaxRetryDelay = TimeSpan.FromSeconds(30);
    options.ProducerSettings.RetryPolicy.UseExponentialBackoff = true;
});
```



## Serialization Configuration

### JSON Serialization

```csharp
builder.Services.AddRabbitMQStreamFlow(options =>
{
    // Producer serialization settings
    options.ProducerSettings.Serialization.Format = SerializationFormat.Json;
    options.ProducerSettings.Serialization.EnableCompression = true;
    options.ProducerSettings.Serialization.CompressionAlgorithm = CompressionAlgorithm.Gzip;
    options.ProducerSettings.Serialization.CompressionThreshold = 1024;
    options.ProducerSettings.Serialization.IncludeTypeInformation = true;
    
    // Consumer serialization settings
    options.ConsumerSettings.Serialization.Format = SerializationFormat.Json;
    options.ConsumerSettings.Serialization.EnableCompression = true;
    options.ConsumerSettings.Serialization.CompressionAlgorithm = CompressionAlgorithm.Gzip;
    options.ConsumerSettings.Serialization.IncludeTypeInformation = true;
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
      "ConnectionSettings": {
        "Host": "localhost",
        "Port": 5672,
        "Username": "guest",
        "Password": "guest",
        "VirtualHost": "/",
        "ConnectionTimeout": "00:00:30"
      },
      "ProducerSettings": {
        "EnablePublisherConfirms": false,
        "MaxConcurrentPublishes": 10
      },
      "ConsumerSettings": {
        "PrefetchCount": 10,
        "MaxConcurrentConsumers": 2,
        "AutoAcknowledge": false
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
      "ConnectionSettings": {
        "Host": "rabbitmq.production.com",
        "Port": 5671,
        "Username": "username",
        "Password": "password",
        "VirtualHost": "/",
        "ConnectionTimeout": "00:00:30",
        "UseSsl": true
      },
      "Ssl": {
        "Enabled": true,
        "ServerName": "rabbitmq.production.com",
        "CertificatePath": "/path/to/certificate.pfx",
        "CheckCertificateRevocation": true
      },
      "ProducerSettings": {
        "EnablePublisherConfirms": true,
        "MaxConcurrentPublishes": 1000,
        "PublishTimeout": "00:00:30"
      },
      "ConsumerSettings": {
        "PrefetchCount": 100,
        "MaxConcurrentConsumers": 10,
        "AutoAcknowledge": false
      },


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
    

});
```

### Custom Validation

```csharp
builder.Services.AddRabbitMQStreamFlow(options =>
{
    // Connection settings
    options.ConnectionSettings.Host = "localhost";
    options.ConnectionSettings.Port = 5672;
    options.ConnectionSettings.Username = "guest";
    options.ConnectionSettings.Password = "guest";
    options.ConnectionSettings.VirtualHost = "/";
    

});


```

## Advanced Configuration Scenarios

### Multi-Environment Configuration

```csharp
builder.Services.AddRabbitMQStreamFlow(options =>
{
    if (builder.Environment.IsDevelopment())
    {
        // Development settings
        options.ConnectionSettings.Host = "localhost";
        options.ConnectionSettings.Port = 5672;
        options.ConnectionSettings.Username = "guest";
        options.ConnectionSettings.Password = "guest";
        options.ConnectionSettings.VirtualHost = "/";
        
        // Producer settings
        options.ProducerSettings.EnablePublisherConfirms = false;
    }
    else
    {
        // Production settings
        options.ConnectionSettings.Host = builder.Configuration["RabbitMQ:Host"] ?? "localhost";
        options.ConnectionSettings.Port = int.Parse(builder.Configuration["RabbitMQ:Port"] ?? "5672");
        options.ConnectionSettings.Username = builder.Configuration["RabbitMQ:Username"] ?? "guest";
        options.ConnectionSettings.Password = builder.Configuration["RabbitMQ:Password"] ?? "guest";
        options.ConnectionSettings.VirtualHost = builder.Configuration["RabbitMQ:VirtualHost"] ?? "/";
        options.ConnectionSettings.UseSsl = true;
        
        // SSL settings
        options.ConnectionSettings.Ssl = new SslSettings
        {
            Enabled = true,
            CertificatePath = builder.Configuration["RabbitMQ:Ssl:CertificatePath"],
            CertificatePassword = builder.Configuration["RabbitMQ:Ssl:CertificatePassword"],
            VerifyCertificate = true,
            ProtocolVersion = "Tls12"
        };
        
        // Producer settings
        options.ProducerSettings.EnablePublisherConfirms = true;
    }
});
```

### Configuration from Environment Variables

```csharp
// Environment variables
// STREAMFLOW_RABBITMQ_CONNECTIONSETTINGS__HOST=localhost
// STREAMFLOW_RABBITMQ_PRODUCERSETTINGS__MAXCONCURRENTPUBLISHES=100
// STREAMFLOW_RABBITMQ_CONSUMERSETTINGS__PREFETCHCOUNT=50

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
    // Connection settings from Key Vault
    options.ConnectionSettings.Host = builder.Configuration["rabbitmq-host"] ?? "localhost";
    options.ConnectionSettings.Port = int.Parse(builder.Configuration["rabbitmq-port"] ?? "5672");
    options.ConnectionSettings.Username = builder.Configuration["rabbitmq-username"] ?? "guest";
    options.ConnectionSettings.Password = builder.Configuration["rabbitmq-password"] ?? "guest";
    options.ConnectionSettings.VirtualHost = builder.Configuration["rabbitmq-virtualhost"] ?? "/";
    
    // SSL settings from Key Vault
    options.ConnectionSettings.Ssl = new SslSettings
    {
        Enabled = true,
        CertificatePath = builder.Configuration["rabbitmq-certificate-path"],
        CertificatePassword = builder.Configuration["rabbitmq-certificate-password"],
        VerifyCertificate = true,
        ProtocolVersion = "Tls12"
    };
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
        // Development settings
        options.ConnectionSettings.Host = "localhost";
        options.ConnectionSettings.Port = 5672;
        options.ConnectionSettings.Username = "guest";
        options.ConnectionSettings.Password = "guest";
        options.ConnectionSettings.VirtualHost = "/";
        
        // Producer settings
        options.ProducerSettings.EnablePublisherConfirms = false;
    }
    else
    {
        // Production settings
        options.ConnectionSettings.Host = builder.Configuration["RabbitMQ:Host"] ?? "localhost";
        options.ConnectionSettings.Port = int.Parse(builder.Configuration["RabbitMQ:Port"] ?? "5672");
        options.ConnectionSettings.Username = builder.Configuration["RabbitMQ:Username"] ?? "guest";
        options.ConnectionSettings.Password = builder.Configuration["RabbitMQ:Password"] ?? "guest";
        options.ConnectionSettings.VirtualHost = builder.Configuration["RabbitMQ:VirtualHost"] ?? "/";
        options.ConnectionSettings.UseSsl = true;
        
        // SSL settings
        options.ConnectionSettings.Ssl = new SslSettings
        {
            Enabled = true,
            CertificatePath = builder.Configuration["RabbitMQ:Ssl:CertificatePath"],
            CertificatePassword = builder.Configuration["RabbitMQ:Ssl:CertificatePassword"],
            VerifyCertificate = true,
            ProtocolVersion = "Tls12"
        };
        
        // Producer settings
        options.ProducerSettings.EnablePublisherConfirms = true;
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
    // Connection settings from configuration
    options.ConnectionSettings.Host = builder.Configuration["RabbitMQ:Host"] ?? "localhost";
    options.ConnectionSettings.Port = int.Parse(builder.Configuration["RabbitMQ:Port"] ?? "5672");
    options.ConnectionSettings.Username = builder.Configuration["RabbitMQ:Username"] ?? "guest";
    options.ConnectionSettings.Password = builder.Configuration["RabbitMQ:Password"] ?? "guest";
    options.ConnectionSettings.VirtualHost = builder.Configuration["RabbitMQ:VirtualHost"] ?? "/";
    
    // SSL settings from configuration
    options.ConnectionSettings.Ssl = new SslSettings
    {
        Enabled = true,
        CertificatePath = builder.Configuration["RabbitMQ:Ssl:CertificatePath"],
        CertificatePassword = builder.Configuration["RabbitMQ:Ssl:CertificatePassword"],
        VerifyCertificate = true,
        ProtocolVersion = "Tls12"
    };
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
   options.ConnectionSettings.Host = "localhost";
   options.ConnectionSettings.Port = 5672;
   
   // Correct
   options.ConnectionSettings.Host = "localhost";
   options.ConnectionSettings.Port = 5672;
   options.ConnectionSettings.Username = "guest";
   options.ConnectionSettings.Password = "guest";
   options.ConnectionSettings.VirtualHost = "/";
   ```

2. **SSL Configuration**
   ```csharp
   // Ensure SSL is properly configured
   options.ConnectionSettings.Host = "hostname";
   options.ConnectionSettings.Port = 5671;
   options.ConnectionSettings.UseSsl = true;
   options.ConnectionSettings.Ssl = new SslSettings
   {
       Enabled = true,
       CertificatePath = "/path/to/certificate.pfx",
       CertificatePassword = "password",
       VerifyCertificate = true,
       ProtocolVersion = "Tls12"
   };
   ```

3. **Performance Settings**
   ```csharp
   // Balance prefetch count with consumer concurrency
   options.ConsumerSettings.PrefetchCount = 50;
   options.ConsumerSettings.MaxConcurrentConsumers = 5;
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