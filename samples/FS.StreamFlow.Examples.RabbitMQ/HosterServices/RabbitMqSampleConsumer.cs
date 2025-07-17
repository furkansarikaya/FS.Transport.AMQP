using System.Text.Json;
using FS.StreamFlow.Core.Features.Messaging.Interfaces;
using FS.StreamFlow.Core.Features.Messaging.Models;
using FS.StreamFlow.RabbitMQ.Features.ErrorHandling;

namespace FS.StreamFlow.Examples.RabbitMQ.HosterServices;

public class RabbitMqSampleConsumer(ILogger<RabbitMqSampleConsumer> logger,IHostApplicationLifetime lifetime,IStreamFlowClient streamFlow) : IHostedService
{
    public Task StartAsync(CancellationToken cancellationToken)
    {
        lifetime.ApplicationStarted.Register(async void () =>
        {
            try
            {
                logger.LogInformation($"{nameof(RabbitMqSampleConsumer)} is starting");
                await StartProcessingAsync(cancellationToken);
            }
            catch (Exception e)
            {
                logger.LogError(e, $"{nameof(RabbitMqSampleConsumer)} failed to start");
            }
        });
        return Task.CompletedTask;
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        logger.LogInformation($"{nameof(RabbitMqSampleConsumer)} is stopping");
        return Task.FromResult(Task.CompletedTask);
    }

    private async Task StartProcessingAsync(CancellationToken cancellationToken)
    {
        await streamFlow.ExchangeManager.Exchange("weather-exchange")
            .AsDirect()
            .WithDurable()
            .DeclareAsync(cancellationToken);

        await streamFlow.QueueManager.Queue("weather-queue")
            .WithDurable()
            .WithExclusive(false)
            .WithAutoDelete()
            .WithDeadLetterExchange("dlx")
            .WithDeadLetterRoutingKey("weather.failed")
            .WithMessageTtl(TimeSpan.FromHours(24))
            .BindToExchange("weather-exchange")
            .DeclareAsync(cancellationToken);

        await streamFlow.Consumer.Queue<WeatherForecast>("weather-queue")
            .WithAutoAck(false)
            .WithErrorHandler((exception, context) => Task.FromResult(exception is ConnectFailureException or BrokerUnreachableException))
            .WithRetryPolicy(new RetryPolicySettings
            {
                UseExponentialBackoff = true,
                MaxRetryAttempts = 3,
                InitialRetryDelay = TimeSpan.FromSeconds(1)
            })
            .WithDeadLetterQueue(new DeadLetterSettings
            {
                ExchangeName = "dlx",
                RoutingKey = "dlq"
            })
            .ConsumeAsync(ProcessRabbitMqMessagesAsync, cancellationToken);
    }

    private Task<bool> ProcessRabbitMqMessagesAsync(WeatherForecast forecast, MessageContext context)
    {
        logger.LogInformation("{RabbitMqSampleConsumerName} is processing a message: {Serialize}", nameof(RabbitMqSampleConsumer), JsonSerializer.Serialize(forecast));
        return Task.FromResult(true);
    }
}