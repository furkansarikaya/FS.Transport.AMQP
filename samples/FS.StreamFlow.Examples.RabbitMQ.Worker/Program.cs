using FS.StreamFlow.Core.Features.Events.Interfaces;
using FS.StreamFlow.Core.Features.Messaging.Interfaces;
using FS.StreamFlow.RabbitMQ.DependencyInjection;
using FS.StreamFlow.Examples.RabbitMQ.Worker;
using FS.StreamFlow.Examples.RabbitMQ.Worker.Handlers;
using FS.StreamFlow.Examples.RabbitMQ.Worker.Models;

var builder = Host.CreateApplicationBuilder(args);

builder.Services.AddRabbitMQStreamFlow(options =>
{
    // Client configuration
    options.ClientConfiguration.ClientName = "RabbitMQ Sample Worker";
    options.ClientConfiguration.EnableAutoRecovery = true;
    options.ClientConfiguration.EnableHeartbeat = true;
    options.ClientConfiguration.HeartbeatInterval = TimeSpan.FromSeconds(60);
    options.ClientConfiguration.Serialization.IncludeTypeInformation = false;
    
    // Connection settings
    options.ConnectionSettings.Host = "localhost";
    options.ConnectionSettings.Port = 5672;
    options.ConnectionSettings.Username = "admin";
    options.ConnectionSettings.Password = "R@bb1tMQ_S3cure_P@55w0rd!!";
    options.ConnectionSettings.VirtualHost = builder.Environment.EnvironmentName.ToLower();
    options.ConnectionSettings.ConnectionTimeout = TimeSpan.FromSeconds(30);
    
    // Producer settings
    options.ProducerSettings.EnablePublisherConfirms = false;
    options.ProducerSettings.ConfirmationTimeout = TimeSpan.FromSeconds(10);
    options.ProducerSettings.MaxConcurrentPublishes = 100;
    
    // Consumer settings
    options.ConsumerSettings.PrefetchCount = 50;
    options.ConsumerSettings.AutoAcknowledge = false;
    options.ConsumerSettings.MaxConcurrentConsumers = 5;
});

builder.Services.AddScoped<IAsyncEventHandler<PaymentProcessed>, PaymentProcessedHandler>();

builder.Services.AddHostedService<Worker>();

var host = builder.Build();
using (var scope = host.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    var streamFlow = services.GetRequiredService<IStreamFlowClient>();
    var lifetime = services.GetRequiredService<IHostApplicationLifetime>();

    await streamFlow.InitializeAsync();

    // Start the event bus
    await streamFlow.EventBus.StartAsync();

    // Get event handlers from dependency injection
    var paymentProcessedHandler = services.GetRequiredService<IAsyncEventHandler<PaymentProcessed>>();

    // Subscribe to integration events - uses ExchangeName from event properties
    await streamFlow.EventBus.SubscribeToIntegrationEventAsync("payment-events", paymentProcessedHandler);

    Console.WriteLine("Event handlers registered and subscribed successfully");
    lifetime.ApplicationStopping.Register(async void () =>
    {
        try
        {
            await streamFlow.EventBus.StopAsync();
            await streamFlow.ShutdownAsync();
            Console.WriteLine("Event handlers unregistered and unsubscribed successfully");
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
        }
    });
}

await host.RunAsync();