using FS.StreamFlow.Core.Features.Messaging.Interfaces;
using FS.StreamFlow.Core.Features.Messaging.Models;
using FS.StreamFlow.RabbitMQ.DependencyInjection;
using FS.StreamFlow.Examples.RabbitMQ.HosterServices;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddHttpContextAccessor();
builder.Services.AddControllers();
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

builder.Services.AddRabbitMQStreamFlow(options =>
{
    // Client configuration
    options.ClientConfiguration.ClientName = "RabbitMQ Sample Application";
    options.ClientConfiguration.EnableAutoRecovery = true;
    options.ClientConfiguration.EnableHeartbeat = true;
    options.ClientConfiguration.HeartbeatInterval = TimeSpan.FromSeconds(60);
    
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

builder.Services.AddHostedService<RabbitMqSampleConsumer>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

var streamFlow = app.Services.GetRequiredService<IStreamFlowClient>();
var lifetime = app.Services.GetRequiredService<IHostApplicationLifetime>();

await streamFlow.InitializeAsync();
lifetime.ApplicationStopping.Register(async void () =>
{
    try
    {
        await streamFlow.ShutdownAsync();
    }
    catch (Exception e)
    {
        Console.WriteLine(e);
    }
});

await app.RunAsync();