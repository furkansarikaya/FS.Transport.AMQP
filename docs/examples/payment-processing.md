# Payment Processing Example with Encryption

**Difficulty**: 🟡 Intermediate  
**Focus**: Payment workflows with encryption, retry and error handling  
**Time**: 30 minutes

This example demonstrates how to implement secure payment processing workflows using FS.StreamFlow with encryption for sensitive data. It covers payment request, confirmation, encryption, error handling, and monitoring.

## 📋 What You'll Learn
- Payment request and confirmation patterns with encryption
- Event-driven payment workflows with secure data handling
- Encryption and decryption of sensitive payment information
- Error handling for payment operations
- Monitoring payment events

## 🛠️ Setup

### Prerequisites
- .NET 9 SDK
- RabbitMQ server running on localhost
- Basic C# knowledge

### Project Structure
```
PaymentProcessing/
├── Program.cs
├── Models/
│   ├── PaymentRequested.cs
│   ├── PaymentProcessed.cs
│   └── EncryptedPaymentData.cs
├── Services/
│   ├── PaymentService.cs
│   ├── PaymentProcessedHandler.cs
│   └── EncryptionService.cs
└── PaymentProcessing.csproj
```

## 🏗️ Implementation

### 1. Encryption Service

```csharp
// Services/EncryptionService.cs
using System.Security.Cryptography;
using System.Text;
using Microsoft.Extensions.Configuration;

public interface IEncryptionService
{
    string Encrypt(string plainText);
    string Decrypt(string cipherText);
    string EncryptObject<T>(T obj);
    T DecryptObject<T>(string encryptedData);
}

public class EncryptionService : IEncryptionService
{
    private readonly byte[] _key;
    private readonly byte[] _iv;

    public EncryptionService(IConfiguration configuration)
    {
        // In production, use proper key management (Azure Key Vault, AWS KMS, etc.)
        var encryptionKey = configuration["Encryption:Key"] ?? "YourSecretKey12345678901234567890123456789012";
        var encryptionIv = configuration["Encryption:IV"] ?? "YourSecretIV12345678901234567890123456789012";
        
        _key = Encoding.UTF8.GetBytes(encryptionKey.PadRight(32).Substring(0, 32));
        _iv = Encoding.UTF8.GetBytes(encryptionIv.PadRight(16).Substring(0, 16));
    }

    public string Encrypt(string plainText)
    {
        if (string.IsNullOrEmpty(plainText)) return plainText;

        using var aes = Aes.Create();
        aes.Key = _key;
        aes.IV = _iv;

        using var encryptor = aes.CreateEncryptor();
        using var msEncrypt = new MemoryStream();
        using var csEncrypt = new CryptoStream(msEncrypt, encryptor, CryptoStreamMode.Write);
        using var swEncrypt = new StreamWriter(csEncrypt);

        swEncrypt.Write(plainText);
        swEncrypt.Flush();
        csEncrypt.FlushFinalBlock();

        return Convert.ToBase64String(msEncrypt.ToArray());
    }

    public string Decrypt(string cipherText)
    {
        if (string.IsNullOrEmpty(cipherText)) return cipherText;

        using var aes = Aes.Create();
        aes.Key = _key;
        aes.IV = _iv;

        using var decryptor = aes.CreateDecryptor();
        using var msDecrypt = new MemoryStream(Convert.FromBase64String(cipherText));
        using var csDecrypt = new CryptoStream(msDecrypt, decryptor, CryptoStreamMode.Read);
        using var srDecrypt = new StreamReader(csDecrypt);

        return srDecrypt.ReadToEnd();
    }

    public string EncryptObject<T>(T obj)
    {
        var json = System.Text.Json.JsonSerializer.Serialize(obj);
        return Encrypt(json);
    }

    public T DecryptObject<T>(string encryptedData)
    {
        var json = Decrypt(encryptedData);
        return System.Text.Json.JsonSerializer.Deserialize<T>(json)!;
    }
}
```

### 2. Encrypted Payment Data Model

```csharp
// Models/EncryptedPaymentData.cs
public class EncryptedPaymentData
{
    public string EncryptedCardNumber { get; set; } = string.Empty;
    public string EncryptedCvv { get; set; } = string.Empty;
    public string EncryptedExpiryDate { get; set; } = string.Empty;
    public string EncryptedCardholderName { get; set; } = string.Empty;
    public string EncryptedBillingAddress { get; set; } = string.Empty;
}

public class PaymentCardData
{
    public string CardNumber { get; set; } = string.Empty;
    public string Cvv { get; set; } = string.Empty;
    public string ExpiryDate { get; set; } = string.Empty;
    public string CardholderName { get; set; } = string.Empty;
    public string BillingAddress { get; set; } = string.Empty;
}
```

### 3. Payment Event Models

```csharp
// Models/PaymentRequested.cs
using FS.StreamFlow.Core.Features.Events.Interfaces;

public class PaymentRequested : IIntegrationEvent
{
    public Guid Id { get; } = Guid.NewGuid();
    public DateTime OccurredOn { get; } = DateTime.UtcNow;
    public int Version { get; } = 1;
    public string EventType => nameof(PaymentRequested);
    public string? CorrelationId { get; set; }
    public string? CausationId { get; set; }
    public IDictionary<string, object> Metadata { get; } = new Dictionary<string, object>();
    public string Source => "payment-service";
    public string RoutingKey => "payment.requested";
    public string? Target { get; set; }
    public string SchemaVersion => "1.0";
    public TimeSpan? TimeToLive { get; set; }
    // Custom properties
    public Guid OrderId { get; set; }
    public decimal Amount { get; set; }
    public string PaymentMethod { get; set; } = "Credit Card";
    public string Currency { get; set; } = "USD";
    public EncryptedPaymentData EncryptedCardData { get; set; } = new();
    public string? MaskedCardNumber { get; set; } // For display purposes only
}

// Models/PaymentProcessed.cs
public class PaymentProcessed : IIntegrationEvent
{
    public Guid Id { get; } = Guid.NewGuid();
    public DateTime OccurredOn { get; } = DateTime.UtcNow;
    public int Version { get; } = 1;
    public string EventType => nameof(PaymentProcessed);
    public string? CorrelationId { get; set; }
    public string? CausationId { get; set; }
    public IDictionary<string, object> Metadata { get; } = new Dictionary<string, object>();
    public string Source => "payment-service";
    public string RoutingKey => "payment.processed";
    public string? Target { get; set; }
    public string SchemaVersion => "1.0";
    public TimeSpan? TimeToLive { get; set; }
    // Custom properties
    public Guid OrderId { get; set; }
    public string TransactionId { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public string PaymentMethod { get; set; } = "Credit Card";
    public string Status { get; set; } = "Completed";
    public DateTime ProcessedAt { get; set; } = DateTime.UtcNow;
    public string? MaskedCardNumber { get; set; } // For audit purposes
}
```

### 4. Payment Service (Publisher) with Encryption

```csharp
// Services/PaymentService.cs
using FS.StreamFlow.Core.Features.Messaging.Interfaces;
using Microsoft.Extensions.Logging;

public class PaymentService
{
    private readonly IStreamFlowClient _streamFlow;
    private readonly IEncryptionService _encryptionService;
    private readonly ILogger<PaymentService> _logger;

    public PaymentService(
        IStreamFlowClient streamFlow, 
        IEncryptionService encryptionService,
        ILogger<PaymentService> logger)
    {
        _streamFlow = streamFlow;
        _encryptionService = encryptionService;
        _logger = logger;
    }

    public async Task RequestPaymentAsync(
        Guid orderId, 
        decimal amount, 
        PaymentCardData cardData,
        string paymentMethod = "Credit Card")
    {
        // Initialize the client first
        await _streamFlow.InitializeAsync();
        
        // Encrypt sensitive payment data
        var encryptedData = new EncryptedPaymentData
        {
            EncryptedCardNumber = _encryptionService.Encrypt(cardData.CardNumber),
            EncryptedCvv = _encryptionService.Encrypt(cardData.Cvv),
            EncryptedExpiryDate = _encryptionService.Encrypt(cardData.ExpiryDate),
            EncryptedCardholderName = _encryptionService.Encrypt(cardData.CardholderName),
            EncryptedBillingAddress = _encryptionService.Encrypt(cardData.BillingAddress)
        };

        // Create masked card number for display/audit
        var maskedCardNumber = MaskCardNumber(cardData.CardNumber);
        
        await _streamFlow.EventBus.Event<PaymentRequested>()
            .WithCorrelationId(Guid.NewGuid().ToString())
            .WithSource("payment-service")
            .WithVersion("1.0")
            .WithAggregateId(orderId.ToString())
            .WithAggregateType("Order")
            .WithProperty("encrypted", "true")
            .PublishAsync(new PaymentRequested
            {
                OrderId = orderId,
                Amount = amount,
                PaymentMethod = paymentMethod,
                EncryptedCardData = encryptedData,
                MaskedCardNumber = maskedCardNumber
            });
            
        _logger.LogInformation("PaymentRequested event published for Order {OrderId} with masked card {MaskedCard}", 
            orderId, maskedCardNumber);
    }

    public async Task ConfirmPaymentAsync(Guid orderId, string transactionId, decimal amount, string? maskedCardNumber = null)
    {
        // Initialize the client first
        await _streamFlow.InitializeAsync();
        
        await _streamFlow.EventBus.Event<PaymentProcessed>()
            .WithCorrelationId(Guid.NewGuid().ToString())
            .WithSource("payment-service")
            .WithVersion("1.0")
            .WithAggregateId(orderId.ToString())
            .WithAggregateType("Order")
            .PublishAsync(new PaymentProcessed
            {
                OrderId = orderId,
                TransactionId = transactionId,
                Amount = amount,
                Status = "Completed",
                MaskedCardNumber = maskedCardNumber
            });
            
        _logger.LogInformation("PaymentProcessed event published for Order {OrderId} with Transaction {TransactionId}", 
            orderId, transactionId);
    }

    private static string MaskCardNumber(string cardNumber)
    {
        if (string.IsNullOrEmpty(cardNumber) || cardNumber.Length < 4)
            return "****";
            
        return $"****-****-****-{cardNumber[^4..]}";
    }
}
```

### 5. Payment Processed Handler (Consumer) with Decryption

```csharp
// Services/PaymentProcessedHandler.cs
using FS.StreamFlow.Core.Features.Messaging.Interfaces;
using Microsoft.Extensions.Logging;

public class PaymentProcessedHandler : IEventHandler<PaymentProcessed>
{
    private readonly ILogger<PaymentProcessedHandler> _logger;

    public PaymentProcessedHandler(ILogger<PaymentProcessedHandler> logger)
    {
        _logger = logger;
    }

    public async Task HandleAsync(PaymentProcessed @event, EventContext context)
    {
        _logger.LogInformation("Handling PaymentProcessed event for Order {OrderId} with Transaction {TransactionId}", 
            @event.OrderId, @event.TransactionId);
        
        // Business logic (e.g., update order status, notify user)
        await Task.Delay(100); // Simulate work
        
        _logger.LogInformation("Payment processed successfully for Order {OrderId}", @event.OrderId);
    }
}

public class PaymentRequestedHandler : IEventHandler<PaymentRequested>
{
    private readonly IEncryptionService _encryptionService;
    private readonly ILogger<PaymentRequestedHandler> _logger;

    public PaymentRequestedHandler(
        IEncryptionService encryptionService,
        ILogger<PaymentRequestedHandler> logger)
    {
        _encryptionService = encryptionService;
        _logger = logger;
    }

    public async Task HandleAsync(PaymentRequested @event, EventContext context)
    {
        _logger.LogInformation("Processing payment request for Order {OrderId} with masked card {MaskedCard}", 
            @event.OrderId, @event.MaskedCardNumber);

        try
        {
            // Decrypt payment data for processing
            var cardData = new PaymentCardData
            {
                CardNumber = _encryptionService.Decrypt(@event.EncryptedCardData.EncryptedCardNumber),
                Cvv = _encryptionService.Decrypt(@event.EncryptedCardData.EncryptedCvv),
                ExpiryDate = _encryptionService.Decrypt(@event.EncryptedCardData.EncryptedExpiryDate),
                CardholderName = _encryptionService.Decrypt(@event.EncryptedCardData.EncryptedCardholderName),
                BillingAddress = _encryptionService.Decrypt(@event.EncryptedCardData.EncryptedBillingAddress)
            };

            // Process payment with decrypted data
            await ProcessPaymentAsync(@event.OrderId, @event.Amount, cardData);
            
            _logger.LogInformation("Payment processed successfully for Order {OrderId}", @event.OrderId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to process payment for Order {OrderId}", @event.OrderId);
            throw;
        }
    }

    private async Task ProcessPaymentAsync(Guid orderId, decimal amount, PaymentCardData cardData)
    {
        // Simulate payment processing
        await Task.Delay(500);
        
        // In real implementation, you would:
        // 1. Validate card data
        // 2. Send to payment gateway
        // 3. Handle response
        // 4. Store transaction details
        
        _logger.LogInformation("Payment processed for Order {OrderId} with amount {Amount}", orderId, amount);
    }
}
```

### 6. Program.cs with Encryption

```csharp
// Program.cs
using FS.StreamFlow.RabbitMQ.DependencyInjection;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

var builder = Host.CreateApplicationBuilder(args);

// Add encryption configuration
builder.Configuration.AddInMemoryCollection(new Dictionary<string, string>
{
    ["Encryption:Key"] = "YourSecretKey12345678901234567890123456789012",
    ["Encryption:IV"] = "YourSecretIV12345678901234567890123456789012"
});

// Add FS.StreamFlow with RabbitMQ
builder.Services.AddRabbitMQStreamFlow(options =>
{
    // Client configuration
    options.ClientConfiguration.ClientName = "Secure Payment Processing System";
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
builder.Services.AddScoped<PaymentService>();
builder.Services.AddScoped<PaymentProcessedHandler>();
builder.Services.AddScoped<PaymentRequestedHandler>();
builder.Services.AddScoped<IEncryptionService, EncryptionService>();

var host = builder.Build();

// Initialize StreamFlow client
var streamFlow = host.Services.GetRequiredService<IStreamFlowClient>();
await streamFlow.InitializeAsync();

// Setup infrastructure
await streamFlow.ExchangeManager.Exchange("payment-events")
    .AsTopic()
    .WithDurable(true)
    .DeclareAsync();
    
await streamFlow.QueueManager.Queue("payment-requested")
    .WithDurable(true)
    .WithDeadLetterExchange("dlx")
    .BindToExchange("payment-events", "payment.requested")
    .DeclareAsync();
    
await streamFlow.QueueManager.Queue("payment-processed")
    .WithDurable(true)
    .WithDeadLetterExchange("dlx")
    .BindToExchange("payment-events", "payment.processed")
    .DeclareAsync();

// Start consumers
var paymentRequestedHandler = host.Services.GetRequiredService<PaymentRequestedHandler>();
var paymentProcessedHandler = host.Services.GetRequiredService<PaymentProcessedHandler>();

// Consumer for payment requests
await streamFlow.Consumer.Queue<PaymentRequested>("payment-requested")
    .WithConcurrency(3)
    .WithPrefetchCount(50)
    .WithErrorHandler(async (exception, context) =>
    {
        return exception is ConnectFailureException;
    })
    .ConsumeAsync(async (eventData, context) =>
    {
        await paymentRequestedHandler.HandleAsync(eventData, new EventContext());
        return true; // Acknowledge message
    });

// Consumer for payment processed
await streamFlow.Consumer.Queue<PaymentProcessed>("payment-processed")
    .WithConcurrency(3)
    .WithPrefetchCount(50)
    .WithErrorHandler(async (exception, context) =>
    {
        return exception is ConnectFailureException;
    })
    .ConsumeAsync(async (eventData, context) =>
    {
        await paymentProcessedHandler.HandleAsync(eventData, new EventContext());
        return true; // Acknowledge message
    });

// Simulate secure payment processing
var paymentService = host.Services.GetRequiredService<PaymentService>();

// Create test payment card data
var cardData = new PaymentCardData
{
    CardNumber = "4111111111111111",
    Cvv = "123",
    ExpiryDate = "12/25",
    CardholderName = "John Doe",
    BillingAddress = "123 Main St, City, State 12345"
};

// Request payment with encrypted data
await paymentService.RequestPaymentAsync(
    orderId: Guid.NewGuid(),
    amount: 99.99m,
    cardData: cardData,
    paymentMethod: "Credit Card");

// Simulate payment confirmation
await Task.Delay(2000);

await paymentService.ConfirmPaymentAsync(
    orderId: Guid.NewGuid(),
    transactionId: $"TXN-{DateTime.UtcNow:yyyyMMdd}-{Random.Shared.Next(100000, 999999)}",
    amount: 99.99m,
    maskedCardNumber: "****-****-****-1111");

Console.WriteLine("Secure payment processing example started. Press any key to exit.");
Console.ReadKey();
```

### 7. appsettings.json Configuration

```json
{
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft.AspNetCore": "Warning"
    }
  },
  "Encryption": {
    "Key": "YourSecretKey12345678901234567890123456789012",
    "IV": "YourSecretIV12345678901234567890123456789012"
  }
}
```

## 🔐 Security Features

### Encryption Benefits
- **Data Protection**: Sensitive payment data is encrypted before transmission
- **Compliance**: Helps meet PCI DSS requirements for card data handling
- **Audit Trail**: Masked card numbers for logging and monitoring
- **Secure Processing**: Decryption only happens when needed for processing

### Security Best Practices
1. **Key Management**: Use Azure Key Vault or AWS KMS in production
2. **Key Rotation**: Implement regular key rotation policies
3. **Access Control**: Limit access to encryption keys
4. **Audit Logging**: Log all encryption/decryption operations
5. **Data Minimization**: Only encrypt necessary fields

## 🛡️ Error Handling
- All failures are routed to a dead letter queue after max retries
- Encryption/decryption errors are logged and handled gracefully
- Handler logs errors and processing failures
- Connection failures are automatically retried with exponential backoff

## 📊 Monitoring
- Use RabbitMQ Management UI at http://localhost:15672 to monitor payment events
- Logs show event publishing and handling in real time
- Masked card numbers are logged for audit purposes
- Health checks are available for payment service

## 🎯 Key Takeaways
- Event-driven payment processing enables reliable workflows with encryption
- Sensitive data should always be encrypted before transmission
- Use masked data for logging and audit purposes
- Error handling and monitoring are essential for payment flows
- FS.StreamFlow simplifies secure payment event workflows with fluent APIs
- Always call InitializeAsync() before using the client
- Use proper event contracts that implement IIntegrationEvent interface
- Implement proper key management in production environments 