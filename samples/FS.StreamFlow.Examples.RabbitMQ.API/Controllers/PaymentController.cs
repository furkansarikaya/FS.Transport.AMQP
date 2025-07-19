using FS.StreamFlow.Core.Features.Messaging.Interfaces;
using FS.StreamFlow.Examples.RabbitMQ.Models;
using Microsoft.AspNetCore.Mvc;

namespace FS.StreamFlow.Examples.RabbitMQ.Controllers;

[ApiController]
[Route("api/[controller]")]
public class PaymentController(IStreamFlowClient streamFlow) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult> TestEventDriven()
    {
        var correlationId = Guid.CreateVersion7().ToString();
        var orderId = Guid.CreateVersion7();
        var paymentProcessedEvent = new PaymentProcessed(orderId, "TXN123456", 100.00m)
        {
            CorrelationId = correlationId,
            CausationId = orderId.ToString()
        };
        
        //Direct API
        await streamFlow.EventBus.PublishIntegrationEventAsync(paymentProcessedEvent);
        
        //Fluent API
        // await streamFlow.EventBus.Event<PaymentProcessed>()
        //     .WithCorrelationId(correlationId)
        //     .WithCausationId(orderId.ToString())
        //     .WithSource("payment-service")
        //     .WithVersion("1")
        //     .WithTtl(TimeSpan.FromMinutes(30))
        //     .WithProperty("payment-method", "credit-card")
        //     .WithAggregateId(orderId.ToString())
        //     .WithAggregateType("Order")
        //     .PublishAsync(paymentProcessedEvent);

        return Ok("Event published");
    }
}