using FS.StreamFlow.Core.Features.Messaging.Interfaces;
using FS.StreamFlow.Core.Features.Messaging.Models;
using Microsoft.AspNetCore.Mvc;

namespace FS.StreamFlow.Examples.RabbitMQ.Controllers;

[ApiController]
[Route("api/[controller]")]
public class WeatherForecastController(IStreamFlowClient streamFlow) : ControllerBase
{
    private static readonly string[] Summaries =
    [
        "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
    ];

    public async Task<IActionResult> Get(CancellationToken cancellationToken)
    {
        var results = Enumerable.Range(1, 5).Select(index => new WeatherForecast
            {
                Date = DateOnly.FromDateTime(DateTime.Now.AddDays(index)),
                TemperatureC = Random.Shared.Next(-20, 55),
                Summary = Summaries[Random.Shared.Next(Summaries.Length)]
            })
            .ToArray();

        foreach (var result in results)
        {
            await streamFlow.Producer.Message(result)
                .WithExchange("weather-exchange")
                .WithDeliveryMode(DeliveryMode.NonPersistent)
                .PublishAsync(cancellationToken);
        }

        return Ok(results);
    }
}