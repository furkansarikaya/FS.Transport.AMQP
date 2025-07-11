# Configuration Guide

This guide covers all configuration options available in FS.RabbitMQ, from basic setup to advanced enterprise scenarios.

## 📋 Table of Contents

1. [Configuration Methods](#configuration-methods)
2. [Connection Configuration](#connection-configuration)
3. [Producer Configuration](#producer-configuration)
4. [Consumer Configuration](#consumer-configuration)
5. [Error Handling Configuration](#error-handling-configuration)
6. [Event Bus Configuration](#event-bus-configuration)
7. [Event Store Configuration](#event-store-configuration)
8. [Saga Configuration](#saga-configuration)
9. [Health Check Configuration](#health-check-configuration)
10. [Serialization Configuration](#serialization-configuration)
11. [Environment-Specific Configuration](#environment-specific-configuration)
12. [Security Configuration](#security-configuration)

## 🔧 Configuration Methods

FS.RabbitMQ supports multiple configuration approaches:

### 1. Fluent API (Recommended)

```csharp
builder.Services.AddRabbitMQ()
    .WithConnectionString("amqp://localhost")
    .WithProducer(config => config.EnableConfirmations = true)
    .WithConsumer(config => config.PrefetchCount = 50)
    .WithErrorHandling(config => config.EnableDeadLetterQueue = true)
    .Build();
```

### 2. Configuration File (appsettings.json)

```json
{
  "RabbitMQ": {
    "Connection": {
      "ConnectionString": "amqp://localhost"
    },
    "Producer": {
      "EnableConfirmations": true
    },
    "Consumer": {
      "PrefetchCount": 50
    }
  }
}
```

```csharp
builder.Services.ConfigureRabbitMQ(builder.Configuration.GetSection("RabbitMQ"));
```

### 3. Options Pattern

```csharp
builder.Services.Configure<RabbitMQConfiguration>(options =>
{
    options.Connection.ConnectionString = "amqp://localhost";
    options.Producer.EnableConfirmations = true;
    options.Consumer.PrefetchCount = 50;
});

builder.Services.AddRabbitMQ().Build();
```

### 4. Environment Variables

```bash
export RABBITMQ__CONNECTION__CONNECTIONSTRING="amqp://localhost"
export RABBITMQ__PRODUCER__ENABLECONFIRMATIONS=true
export RABBITMQ__CONSUMER__PREFETCHCOUNT=50
```

## 🔗 Connection Configuration

### Basic Connection Settings

```csharp
builder.Services.AddRabbitMQ()
    .WithConnectionString("amqp://username:password@hostname:5672/virtualhost")
    .WithConnection(config =>
    {
        // Basic connection settings
        config.AutomaticRecovery = true;              // Enable automatic recovery
        config.HeartbeatInterval = TimeSpan.FromSeconds(60);  // Heartbeat interval
        config.ConnectionTimeout = TimeSpan.FromSeconds(30);  // Connection timeout
        config.RequestedConnectionTimeout = TimeSpan.FromSeconds(30);  // Requested connection timeout
        config.MaxChannels = 100;                     // Maximum channels per connection
        config.ClientProvidedName = "MyApplication";  // Client name for monitoring
        
        // Advanced connection settings
        config.ContinuationTimeout = TimeSpan.FromSeconds(10);  // Continuation timeout
        config.HandshakeContinuationTimeout = TimeSpan.FromSeconds(10);  // Handshake timeout
        config.NetworkRecoveryInterval = TimeSpan.FromSeconds(5);  // Recovery interval
        config.TopologyRecoveryEnabled = true;        // Enable topology recovery
        config.RequestedFrameMax = 0;                  // Frame size (0 = no limit)
        config.RequestedHeartbeat = TimeSpan.FromSeconds(60);  // Requested heartbeat
        config.UseBackgroundThreadsForIO = true;      // Use background threads for I/O
        config.DispatchConsumersAsync = true;         // Dispatch consumers asynchronously
        config.ConsumerDispatchConcurrency = 1;       // Consumer dispatch concurrency
    })
    .Build();
```

### Connection String Formats

```csharp
// Standard format
"amqp://username:password@hostname:5672/virtualhost"

// With SSL
"amqps://username:password@hostname:5671/virtualhost"

// With query parameters
"amqp://localhost?heartbeat=60&connection_timeout=30"

// Multiple hosts (cluster)
"amqp://host1:5672,host2:5672,host3:5672"
```

### Connection Pool Configuration

```csharp
builder.Services.AddRabbitMQ()
    .WithConnectionString("amqp://localhost")
    .WithConnectionPool(config =>
    {
        config.MaxConnections = 10;                   // Maximum connections in pool
        config.MaxChannelsPerConnection = 100;        // Maximum channels per connection
        config.ConnectionIdleTimeout = TimeSpan.FromMinutes(5);  // Idle timeout
        config.ConnectionLeaseTimeout = TimeSpan.FromMinutes(30); // Lease timeout
        config.EnableConnectionPooling = true;        // Enable connection pooling
        config.PoolGrowthFactor = 2;                 // Pool growth factor
        config.PoolShrinkFactor = 0.5;               // Pool shrink factor
    })
    .Build();
```

## 📤 Producer Configuration

### Basic Producer Settings

```csharp
builder.Services.AddRabbitMQ()
    .WithProducer(config =>
    {
        // Publisher confirmations
        config.EnableConfirmations = true;            // Enable publisher confirmations
        config.ConfirmationTimeout = TimeSpan.FromSeconds(5);  // Confirmation timeout
        config.WaitForConfirmations = true;           // Wait for confirmations
        
        // Batch operations
        config.BatchSize = 100;                       // Batch size for batch operations
        config.MaxBatchWaitTime = TimeSpan.FromSeconds(1);  // Maximum batch wait time
        config.EnableBatchPublishing = true;          // Enable batch publishing
        
        // Transactions
        config.EnableTransactions = false;            // Enable transactions
        config.TransactionTimeout = TimeSpan.FromSeconds(30);  // Transaction timeout
        
        // Performance settings
        config.MaxConcurrentPublishes = 10;           // Maximum concurrent publishes
        config.PublishTimeout = TimeSpan.FromSeconds(30);  // Publish timeout
        config.RetryOnFailure = true;                 // Retry on failure
        config.MaxRetryAttempts = 3;                  // Maximum retry attempts
        config.RetryDelay = TimeSpan.FromSeconds(1);  // Retry delay
        
        // Message settings
        config.DefaultExchange = "default";           // Default exchange
        config.DefaultRoutingKey = "default";         // Default routing key
        config.DefaultDeliveryMode = DeliveryMode.Persistent;  // Default delivery mode
        config.DefaultPriority = 0;                   // Default priority
        config.DefaultExpiration = null;              // Default expiration
        config.DefaultMessageId = null;               // Default message ID
        config.DefaultTimestamp = true;               // Include timestamp
        config.DefaultHeaders = new Dictionary<string, object>();  // Default headers
    })
    .Build();
```

### Advanced Producer Settings

```csharp
builder.Services.AddRabbitMQ()
    .WithProducer(config =>
    {
        // Advanced routing
        config.EnableMandatory = true;                // Enable mandatory flag
        config.EnableImmediate = false;               // Enable immediate flag (deprecated)
        config.AlternateExchange = "alt-exchange";    // Alternate exchange
        
        // Message tracking
        config.EnableMessageTracking = true;          // Enable message tracking
        config.MessageTrackingTimeout = TimeSpan.FromMinutes(5);  // Tracking timeout
        config.MaxTrackedMessages = 10000;            // Maximum tracked messages
        
        // Compression
        config.EnableCompression = false;             // Enable message compression
        config.CompressionThreshold = 1024;           // Compression threshold (bytes)
        config.CompressionLevel = CompressionLevel.Optimal;  // Compression level
        
        // Serialization
        config.SerializerType = SerializerType.Json;  // Serializer type
        config.SerializationBufferSize = 4096;        // Serialization buffer size
        config.EnableSerializationCaching = true;     // Enable serialization caching
        
        // Connection settings
        config.DedicatedConnection = false;           // Use dedicated connection
        config.ConnectionName = "ProducerConnection"; // Connection name
        config.ChannelPoolSize = 10;                  // Channel pool size
        config.MaxChannelsPerConnection = 100;        // Maximum channels per connection
    })
    .Build();
```

## 📥 Consumer Configuration

### Basic Consumer Settings

```csharp
builder.Services.AddRabbitMQ()
    .WithConsumer(config =>
    {
        // Basic settings
        config.PrefetchCount = 50;                    // Prefetch count
        config.PrefetchSize = 0;                      // Prefetch size (0 = no limit)
        config.PrefetchGlobal = false;                // Global prefetch
        config.AutoAck = false;                       // Auto acknowledgment
        config.ConcurrentConsumers = 1;               // Concurrent consumers
        config.MaxConsumerConcurrency = 10;           // Maximum consumer concurrency
        
        // Queue settings
        config.QueueDurable = true;                   // Queue durability
        config.QueueExclusive = false;                // Queue exclusivity
        config.QueueAutoDelete = false;               // Queue auto-delete
        config.QueueArguments = new Dictionary<string, object>();  // Queue arguments
        
        // Consumer behavior
        config.RequeueOnFailure = true;               // Requeue on failure
        config.RequeueOnException = false;            // Requeue on exception
        config.ConsumeLatestOnly = false;             // Consume latest only
        config.ConsumerTag = "consumer-tag";          // Consumer tag
        config.ConsumerPriority = 0;                  // Consumer priority
        config.Exclusive = false;                     // Exclusive consumer
        
        // Message handling
        config.MessageHandlingTimeout = TimeSpan.FromMinutes(5);  // Message handling timeout
        config.MaxMessageSize = 1024 * 1024;         // Maximum message size (1MB)
        config.EnableMessageDeduplication = false;    // Enable message deduplication
        config.DeduplicationTimeWindow = TimeSpan.FromMinutes(10);  // Deduplication window
        
        // Error handling
        config.MaxRetryAttempts = 3;                  // Maximum retry attempts
        config.RetryDelay = TimeSpan.FromSeconds(1);  // Retry delay
        config.RetryDelayBackoffMultiplier = 2.0;     // Backoff multiplier
        config.MaxRetryDelay = TimeSpan.FromMinutes(5);  // Maximum retry delay
        config.DeadLetterQueue = "dlq";               // Dead letter queue
        config.DeadLetterExchange = "dlx";            // Dead letter exchange
        config.DeadLetterRoutingKey = "failed";       // Dead letter routing key
    })
    .Build();
```

### Advanced Consumer Settings

```csharp
builder.Services.AddRabbitMQ()
    .WithConsumer(config =>
    {
        // Advanced routing
        config.RoutingKey = "*.orders.*";             // Routing key pattern
        config.ExchangeType = "topic";                // Exchange type
        config.ExchangeDurable = true;                // Exchange durability
        config.ExchangeAutoDelete = false;            // Exchange auto-delete
        
        // Performance settings
        config.EnableParallelProcessing = true;       // Enable parallel processing
        config.MaxDegreeOfParallelism = Environment.ProcessorCount;  // Max parallelism
        config.ProcessingTimeout = TimeSpan.FromMinutes(10);  // Processing timeout
        config.HeartbeatInterval = TimeSpan.FromSeconds(60);  // Heartbeat interval
        
        // Message filtering
        config.MessageFilter = message => message.RoutingKey.StartsWith("orders");  // Message filter
        config.ContentTypeFilter = new[] { "application/json" };  // Content type filter
        config.HeaderFilters = new Dictionary<string, object>  // Header filters
        {
            ["source"] = "order-service",
            ["version"] = "1.0"
        };
        
        // Connection settings
        config.DedicatedConnection = false;           // Use dedicated connection
        config.ConnectionName = "ConsumerConnection"; // Connection name
        config.ChannelPoolSize = 5;                   // Channel pool size
        config.MaxChannelsPerConnection = 50;         // Maximum channels per connection
        
        // Monitoring
        config.EnableMetrics = true;                  // Enable metrics
        config.MetricsInterval = TimeSpan.FromMinutes(1);  // Metrics interval
        config.EnablePerformanceCounters = true;      // Enable performance counters
        config.LogMessageDetails = false;             // Log message details
        config.LogPerformanceMetrics = true;          // Log performance metrics
    })
    .Build();
```

## 🚨 Error Handling Configuration

### Basic Error Handling

```csharp
builder.Services.AddRabbitMQ()
    .WithErrorHandling(config =>
    {
        // Dead letter queue
        config.EnableDeadLetterQueue = true;          // Enable dead letter queue
        config.DeadLetterExchange = "dlx";            // Dead letter exchange
        config.DeadLetterQueue = "dlq";               // Dead letter queue
        config.DeadLetterRoutingKey = "failed";       // Dead letter routing key
        
        // Retry policy
        config.RetryPolicy = RetryPolicyType.ExponentialBackoff;  // Retry policy
        config.MaxRetryAttempts = 3;                  // Maximum retry attempts
        config.RetryDelay = TimeSpan.FromSeconds(1);  // Initial retry delay
        config.MaxRetryDelay = TimeSpan.FromMinutes(5);  // Maximum retry delay
        config.RetryDelayBackoffMultiplier = 2.0;     // Backoff multiplier
        
        // Error handling strategy
        config.ErrorHandlingStrategy = ErrorHandlingStrategy.DeadLetter;  // Error handling strategy
        config.RetryOnTransientErrors = true;         // Retry on transient errors
        config.RetryOnAllErrors = false;              // Retry on all errors
        config.RequeueOnFailure = false;              // Requeue on failure
        config.RejectOnFailure = true;                // Reject on failure
        
        // Circuit breaker
        config.EnableCircuitBreaker = true;           // Enable circuit breaker
        config.CircuitBreakerFailureThreshold = 5;    // Failure threshold
        config.CircuitBreakerRecoveryTimeout = TimeSpan.FromMinutes(1);  // Recovery timeout
        config.CircuitBreakerSamplingDuration = TimeSpan.FromSeconds(30);  // Sampling duration
        config.CircuitBreakerMinimumThroughput = 10;  // Minimum throughput
    })
    .Build();
```

### Advanced Error Handling

```csharp
builder.Services.AddRabbitMQ()
    .WithErrorHandling(config =>
    {
        // Custom error handling
        config.ErrorHandler = typeof(CustomErrorHandler);  // Custom error handler
        config.ErrorHandlerFactory = typeof(CustomErrorHandlerFactory);  // Error handler factory
        
        // Error classification
        config.TransientErrorTypes = new[]  // Transient error types
        {
            typeof(TimeoutException),
            typeof(SocketException),
            typeof(HttpRequestException)
        };
        
        config.PermanentErrorTypes = new[]  // Permanent error types
        {
            typeof(ArgumentException),
            typeof(InvalidOperationException),
            typeof(NotSupportedException)
        };
        
        // Error logging
        config.LogErrors = true;                      // Log errors
        config.LogRetries = true;                     // Log retries
        config.LogDeadLetterMessages = true;          // Log dead letter messages
        config.LogSuccessfulRetries = false;          // Log successful retries
        config.LogCircuitBreakerEvents = true;        // Log circuit breaker events
        
        // Error metrics
        config.EnableErrorMetrics = true;             // Enable error metrics
        config.ErrorMetricsInterval = TimeSpan.FromMinutes(1);  // Error metrics interval
        config.TrackErrorsByType = true;              // Track errors by type
        config.TrackErrorsByMessage = false;          // Track errors by message
        
        // Error context
        config.IncludeErrorContext = true;            // Include error context
        config.IncludeStackTrace = false;             // Include stack trace
        config.IncludeInnerExceptions = true;         // Include inner exceptions
        config.MaxErrorContextSize = 4096;            // Maximum error context size
    })
    .Build();
```

## 🎯 Event Bus Configuration

### Basic Event Bus Settings

```csharp
builder.Services.AddRabbitMQ()
    .WithEventBus(config =>
    {
        // Exchange settings
        config.DomainEventExchange = "domain-events";    // Domain event exchange
        config.IntegrationEventExchange = "integration-events";  // Integration event exchange
        config.EventExchangeType = "topic";              // Event exchange type
        config.EventExchangeDurable = true;              // Event exchange durability
        config.EventExchangeAutoDelete = false;          // Event exchange auto-delete
        
        // Routing settings
        config.DomainEventRoutingKey = "domain.{eventType}";  // Domain event routing key
        config.IntegrationEventRoutingKey = "integration.{eventType}";  // Integration event routing key
        config.UseEventTypeAsRoutingKey = true;          // Use event type as routing key
        config.RoutingKeyTransformation = RoutingKeyTransformation.LowerCase;  // Routing key transformation
        
        // Event handling
        config.EnableEventHandlerDiscovery = true;       // Enable event handler discovery
        config.EventHandlerAssemblies = new[] { typeof(Program).Assembly };  // Event handler assemblies
        config.EventHandlerConcurrency = 1;              // Event handler concurrency
        config.EventHandlerTimeout = TimeSpan.FromMinutes(5);  // Event handler timeout
        
        // Event metadata
        config.IncludeEventMetadata = true;              // Include event metadata
        config.EventMetadataHeaders = new[]              // Event metadata headers
        {
            "EventId",
            "EventType",
            "EventVersion",
            "CorrelationId",
            "CausationId",
            "Timestamp"
        };
        
        // Event versioning
        config.EnableEventVersioning = true;             // Enable event versioning
        config.DefaultEventVersion = "1.0";              // Default event version
        config.EventVersionHeader = "EventVersion";      // Event version header
        config.EventVersionHandling = EventVersionHandling.Strict;  // Event version handling
    })
    .Build();
```

### Advanced Event Bus Settings

```csharp
builder.Services.AddRabbitMQ()
    .WithEventBus(config =>
    {
        // Event publishing
        config.PublishStrategy = EventPublishStrategy.Immediate;  // Publish strategy
        config.EnablePublishConfirmations = true;        // Enable publish confirmations
        config.PublishTimeout = TimeSpan.FromSeconds(30);  // Publish timeout
        config.MaxPublishRetries = 3;                    // Maximum publish retries
        config.PublishRetryDelay = TimeSpan.FromSeconds(1);  // Publish retry delay
        
        // Event subscription
        config.SubscriptionStrategy = EventSubscriptionStrategy.Automatic;  // Subscription strategy
        config.EnableSubscriptionRecovery = true;        // Enable subscription recovery
        config.SubscriptionRecoveryDelay = TimeSpan.FromSeconds(5);  // Subscription recovery delay
        config.MaxSubscriptionRetries = 5;               // Maximum subscription retries
        
        // Event filtering
        config.EnableEventFiltering = true;              // Enable event filtering
        config.EventFilter = eventType => !eventType.Name.StartsWith("Internal");  // Event filter
        config.EventTypeFilter = new[] { typeof(OrderCreated), typeof(OrderShipped) };  // Event type filter
        
        // Event serialization
        config.EventSerializerType = SerializerType.Json;  // Event serializer type
        config.EventSerializationSettings = new SerializationSettings  // Event serialization settings
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            IgnoreNullValues = true,
            WriteIndented = false
        };
        
        // Event store integration
        config.EnableEventStoreIntegration = true;       // Enable event store integration
        config.EventStoreExchange = "event-store";       // Event store exchange
        config.EventStoreQueue = "event-store-queue";    // Event store queue
        config.EventStoreRoutingKey = "eventstore.{eventType}";  // Event store routing key
        
        // Event replay
        config.EnableEventReplay = true;                 // Enable event replay
        config.EventReplayExchange = "event-replay";     // Event replay exchange
        config.EventReplayQueue = "event-replay-queue";  // Event replay queue
        config.EventReplayBatchSize = 100;               // Event replay batch size
        config.EventReplayInterval = TimeSpan.FromSeconds(1);  // Event replay interval
    })
    .Build();
```

## 📚 Event Store Configuration

### Basic Event Store Settings

```csharp
builder.Services.AddRabbitMQ()
    .WithEventStore(config =>
    {
        // Stream settings
        config.StreamPrefix = "eventstore";             // Stream prefix
        config.StreamExchange = "event-store";          // Stream exchange
        config.StreamQueue = "event-store-queue";       // Stream queue
        config.StreamRoutingKey = "eventstore.{streamName}";  // Stream routing key
        
        // Event storage
        config.EventStorageType = EventStorageType.RabbitMQ;  // Event storage type
        config.EventCompressionType = CompressionType.None;   // Event compression type
        config.EventEncryptionType = EncryptionType.None;     // Event encryption type
        config.MaxEventSize = 1024 * 1024;             // Maximum event size (1MB)
        config.EventBatchSize = 100;                    // Event batch size
        config.EventBatchTimeout = TimeSpan.FromSeconds(1);  // Event batch timeout
        
        // Snapshot settings
        config.EnableSnapshots = true;                  // Enable snapshots
        config.SnapshotInterval = 100;                  // Snapshot interval
        config.SnapshotExchange = "snapshots";          // Snapshot exchange
        config.SnapshotQueue = "snapshots-queue";       // Snapshot queue
        config.SnapshotRoutingKey = "snapshot.{streamName}";  // Snapshot routing key
        config.SnapshotCompressionType = CompressionType.GZip;  // Snapshot compression type
        
        // Aggregate settings
        config.AggregateAssemblies = new[] { typeof(Program).Assembly };  // Aggregate assemblies
        config.AggregateIdHeader = "AggregateId";        // Aggregate ID header
        config.AggregateTypeHeader = "AggregateType";    // Aggregate type header
        config.AggregateVersionHeader = "AggregateVersion";  // Aggregate version header
        
        // Event replay
        config.EnableEventReplay = true;                // Enable event replay
        config.ReplayBatchSize = 1000;                  // Replay batch size
        config.ReplayInterval = TimeSpan.FromSeconds(1);  // Replay interval
        config.ReplayTimeout = TimeSpan.FromMinutes(10);  // Replay timeout
        config.ReplayFromSnapshot = true;               // Replay from snapshot
    })
    .Build();
```

## 🔄 Saga Configuration

### Basic Saga Settings

```csharp
builder.Services.AddRabbitMQ()
    .WithSaga(config =>
    {
        // Saga state
        config.SagaStateExchange = "saga-state";        // Saga state exchange
        config.SagaStateQueue = "saga-state-queue";     // Saga state queue
        config.SagaStateRoutingKey = "saga.{sagaType}.{sagaId}";  // Saga state routing key
        
        // Saga timeout
        config.SagaTimeoutExchange = "saga-timeout";    // Saga timeout exchange
        config.SagaTimeoutQueue = "saga-timeout-queue"; // Saga timeout queue
        config.SagaTimeoutRoutingKey = "timeout.{sagaType}.{sagaId}";  // Saga timeout routing key
        
        // Saga discovery
        config.EnableSagaDiscovery = true;              // Enable saga discovery
        config.SagaAssemblies = new[] { typeof(Program).Assembly };  // Saga assemblies
        config.SagaHandlerDiscovery = SagaHandlerDiscovery.Automatic;  // Saga handler discovery
        
        // Saga persistence
        config.SagaPersistenceType = SagaPersistenceType.RabbitMQ;  // Saga persistence type
        config.SagaStateCompression = CompressionType.None;  // Saga state compression
        config.SagaStateEncryption = EncryptionType.None;    // Saga state encryption
        config.SagaStateTtl = TimeSpan.FromDays(30);    // Saga state TTL
        
        // Saga execution
        config.SagaExecutionTimeout = TimeSpan.FromMinutes(30);  // Saga execution timeout
        config.SagaLockTimeout = TimeSpan.FromMinutes(5);  // Saga lock timeout
        config.SagaConcurrencyControl = SagaConcurrencyControl.Pessimistic;  // Saga concurrency control
        config.MaxSagaExecutionAttempts = 3;            // Maximum saga execution attempts
        
        // Saga monitoring
        config.EnableSagaMonitoring = true;             // Enable saga monitoring
        config.SagaMonitoringInterval = TimeSpan.FromMinutes(1);  // Saga monitoring interval
        config.SagaMetricsEnabled = true;               // Enable saga metrics
        config.SagaLoggingEnabled = true;               // Enable saga logging
    })
    .Build();
```

## 🏥 Health Check Configuration

### Basic Health Check Settings

```csharp
builder.Services.AddRabbitMQ()
    .WithHealthChecks(config =>
    {
        // Health check settings
        config.Enabled = true;                          // Enable health checks
        config.Interval = TimeSpan.FromSeconds(30);     // Health check interval
        config.Timeout = TimeSpan.FromSeconds(5);       // Health check timeout
        config.FailureThreshold = 3;                    // Failure threshold
        config.SuccessThreshold = 1;                    // Success threshold
        config.InitialDelay = TimeSpan.FromSeconds(5);  // Initial delay
        
        // Connection health
        config.CheckConnectionHealth = true;            // Check connection health
        config.CheckChannelHealth = true;               // Check channel health
        config.CheckExchangeHealth = true;              // Check exchange health
        config.CheckQueueHealth = true;                 // Check queue health
        
        // Service health
        config.CheckProducerHealth = true;              // Check producer health
        config.CheckConsumerHealth = true;              // Check consumer health
        config.CheckEventBusHealth = true;              // Check event bus health
        config.CheckEventStoreHealth = true;            // Check event store health
        config.CheckSagaHealth = true;                  // Check saga health
        
        // Health check endpoints
        config.HealthCheckExchange = "health-checks";   // Health check exchange
        config.HealthCheckQueue = "health-check-queue"; // Health check queue
        config.HealthCheckRoutingKey = "health.{service}";  // Health check routing key
        
        // Health metrics
        config.EnableHealthMetrics = true;              // Enable health metrics
        config.HealthMetricsInterval = TimeSpan.FromMinutes(1);  // Health metrics interval
        config.HealthMetricsRetention = TimeSpan.FromDays(7);  // Health metrics retention
    })
    .Build();
```

## 🔐 Security Configuration

### SSL/TLS Configuration

```csharp
builder.Services.AddRabbitMQ()
    .WithSsl(config =>
    {
        // SSL settings
        config.Enabled = true;                          // Enable SSL
        config.ServerName = "rabbitmq.example.com";    // Server name
        config.Version = SslProtocols.Tls12 | SslProtocols.Tls13;  // SSL version
        config.CertificatePath = "/path/to/certificate.pfx";  // Certificate path
        config.CertificatePassword = "password";         // Certificate password
        config.CertificateSubject = "CN=rabbitmq.example.com";  // Certificate subject
        config.CertificateThumbprint = "1234567890ABCDEF";  // Certificate thumbprint
        
        // SSL validation
        config.AcceptablePolicyErrors = SslPolicyErrors.None;  // Acceptable policy errors
        config.CheckCertificateRevocation = true;       // Check certificate revocation
        config.ValidateServerCertificate = true;        // Validate server certificate
        config.ClientCertificateRequired = false;       // Client certificate required
        
        // SSL options
        config.UseSslStreamForAll = true;               // Use SSL stream for all
        config.SslStreamOptions = new SslClientAuthenticationOptions  // SSL stream options
        {
            TargetHost = "rabbitmq.example.com",
            ClientCertificates = new X509CertificateCollection(),
            EnabledSslProtocols = SslProtocols.Tls12 | SslProtocols.Tls13,
            CertificateRevocationCheckMode = X509RevocationMode.Online
        };
    })
    .Build();
```

### Authentication Configuration

```csharp
builder.Services.AddRabbitMQ()
    .WithAuthentication(config =>
    {
        // Basic authentication
        config.AuthenticationMethod = AuthenticationMethod.Plain;  // Authentication method
        config.Username = "username";                   // Username
        config.Password = "password";                   // Password
        
        // OAuth2 authentication
        config.OAuth2TokenUrl = "https://auth.example.com/oauth2/token";  // OAuth2 token URL
        config.OAuth2ClientId = "client-id";           // OAuth2 client ID
        config.OAuth2ClientSecret = "client-secret";   // OAuth2 client secret
        config.OAuth2Scope = "rabbitmq.read rabbitmq.write";  // OAuth2 scope
        
        // JWT authentication
        config.JwtToken = "jwt-token";                  // JWT token
        config.JwtTokenProvider = typeof(JwtTokenProvider);  // JWT token provider
        config.JwtTokenRefreshInterval = TimeSpan.FromMinutes(30);  // JWT token refresh interval
        
        // External authentication
        config.ExternalAuthenticationProvider = typeof(ExternalAuthProvider);  // External auth provider
        config.ExternalAuthenticationSettings = new Dictionary<string, object>  // External auth settings
        {
            ["setting1"] = "value1",
            ["setting2"] = "value2"
        };
    })
    .Build();
```

## 🌍 Environment-Specific Configuration

### Development Environment

```json
{
  "RabbitMQ": {
    "Connection": {
      "ConnectionString": "amqp://localhost",
      "AutomaticRecovery": true,
      "HeartbeatInterval": "00:01:00"
    },
    "Producer": {
      "EnableConfirmations": false,
      "BatchSize": 10
    },
    "Consumer": {
      "PrefetchCount": 5,
      "ConcurrentConsumers": 1
    },
    "ErrorHandling": {
      "EnableDeadLetterQueue": true,
      "MaxRetryAttempts": 2
    },
    "HealthCheck": {
      "Enabled": true,
      "Interval": "00:00:30"
    }
  }
}
```

### Production Environment

```json
{
  "RabbitMQ": {
    "Connection": {
      "ConnectionString": "amqps://username:password@production-rabbitmq.example.com:5671",
      "AutomaticRecovery": true,
      "HeartbeatInterval": "00:01:00",
      "MaxChannels": 200,
      "ConnectionTimeout": "00:00:30"
    },
    "Producer": {
      "EnableConfirmations": true,
      "ConfirmationTimeout": "00:00:05",
      "BatchSize": 100,
      "MaxBatchWaitTime": "00:00:01"
    },
    "Consumer": {
      "PrefetchCount": 50,
      "ConcurrentConsumers": 10,
      "MaxConsumerConcurrency": 50
    },
    "ErrorHandling": {
      "EnableDeadLetterQueue": true,
      "MaxRetryAttempts": 5,
      "RetryDelay": "00:00:02",
      "EnableCircuitBreaker": true
    },
    "HealthCheck": {
      "Enabled": true,
      "Interval": "00:00:15",
      "Timeout": "00:00:03"
    },
    "Ssl": {
      "Enabled": true,
      "ServerName": "production-rabbitmq.example.com",
      "CertificatePath": "/etc/ssl/certs/rabbitmq-client.pfx",
      "CertificatePassword": "${RABBITMQ_CERT_PASSWORD}"
    }
  }
}
```

## 📊 Configuration Validation

### Enable Configuration Validation

```csharp
builder.Services.AddRabbitMQ()
    .WithValidation(config =>
    {
        config.EnableValidation = true;                 // Enable validation
        config.ValidateOnStartup = true;               // Validate on startup
        config.ValidateOnConfigurationChange = true;    // Validate on configuration change
        config.ThrowOnValidationError = true;          // Throw on validation error
        config.LogValidationErrors = true;             // Log validation errors
        config.ValidationTimeout = TimeSpan.FromSeconds(30);  // Validation timeout
    })
    .Build();
```

### Custom Validation Rules

```csharp
public class CustomRabbitMQConfigurationValidator : IConfigurationValidator<RabbitMQConfiguration>
{
    public ValidationResult Validate(RabbitMQConfiguration configuration)
    {
        var errors = new List<string>();
        
        // Validate connection string
        if (string.IsNullOrEmpty(configuration.Connection.ConnectionString))
        {
            errors.Add("Connection string is required");
        }
        
        // Validate producer settings
        if (configuration.Producer.BatchSize <= 0)
        {
            errors.Add("Producer batch size must be greater than zero");
        }
        
        // Validate consumer settings
        if (configuration.Consumer.PrefetchCount <= 0)
        {
            errors.Add("Consumer prefetch count must be greater than zero");
        }
        
        return new ValidationResult(errors.Count == 0, errors);
    }
}

// Register custom validator
builder.Services.AddSingleton<IConfigurationValidator<RabbitMQConfiguration>, CustomRabbitMQConfigurationValidator>();
```

## 🔧 Configuration Best Practices

### 1. Use Environment Variables for Secrets

```csharp
// Never hardcode secrets in configuration files
builder.Services.AddRabbitMQ()
    .WithConnectionString(Environment.GetEnvironmentVariable("RABBITMQ_CONNECTION_STRING"))
    .Build();
```

### 2. Use Configuration Sections

```csharp
// Organize configuration into logical sections
builder.Services.Configure<RabbitMQConfiguration>(
    builder.Configuration.GetSection("RabbitMQ"));
```

### 3. Enable Configuration Validation

```csharp
// Always validate configuration in production
builder.Services.AddRabbitMQ()
    .WithValidation(config =>
    {
        config.EnableValidation = true;
        config.ValidateOnStartup = true;
        config.ThrowOnValidationError = true;
    })
    .Build();
```

### 4. Use Named Options for Multiple Configurations

```csharp
// Configure multiple RabbitMQ instances
builder.Services.Configure<RabbitMQConfiguration>("Primary", config =>
{
    config.Connection.ConnectionString = "amqp://primary-rabbitmq";
});

builder.Services.Configure<RabbitMQConfiguration>("Secondary", config =>
{
    config.Connection.ConnectionString = "amqp://secondary-rabbitmq";
});
```

### 5. Monitor Configuration Changes

```csharp
// React to configuration changes
builder.Services.AddRabbitMQ()
    .WithConfigurationMonitoring(config =>
    {
        config.EnableConfigurationMonitoring = true;
        config.ConfigurationChangeDelay = TimeSpan.FromSeconds(5);
        config.RestartOnConfigurationChange = true;
    })
    .Build();
```

## 🎯 Next Steps

Now that you understand how to configure FS.RabbitMQ, explore these topics:

- [Producer Guide](producer.md) - Learn advanced publishing techniques
- [Consumer Guide](consumer.md) - Master message consumption patterns
- [Error Handling](error-handling.md) - Implement robust error handling
- [Performance Tuning](performance.md) - Optimize for high throughput
- [Monitoring](monitoring.md) - Monitor your messaging system 