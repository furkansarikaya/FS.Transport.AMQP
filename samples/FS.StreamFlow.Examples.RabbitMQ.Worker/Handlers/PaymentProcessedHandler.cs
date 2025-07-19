using FS.StreamFlow.Core.Features.Events.Interfaces;
using FS.StreamFlow.Core.Features.Events.Models;
using FS.StreamFlow.Examples.RabbitMQ.Worker.Models;

namespace FS.StreamFlow.Examples.RabbitMQ.Worker.Handlers;

public class PaymentProcessedHandler(ILogger<PaymentProcessedHandler> logger) : IAsyncEventHandler<PaymentProcessed>
{
    public int Priority => 1;
    public Type[] EventTypes { get; } = [typeof(PaymentProcessed)];
    public string HandlerName => nameof(PaymentProcessedHandler);
    public bool CanHandle(Type eventType) => eventType == typeof(PaymentProcessed);

    public bool AllowConcurrentExecution => true;

    public async Task HandleAsync(PaymentProcessed @event, EventContext context, CancellationToken cancellationToken = default)
    {
        logger.LogInformation("Received PaymentProcessed event with correlationId: {CorrelationId}", @event.CorrelationId);
       
        // Business logic (e.g., update order status, notify user)
        await Task.Delay(100, cancellationToken); // Simulate work
        
        logger.LogInformation("PaymentProcessed event with correlationId: {CorrelationId} has been processed", @event.CorrelationId);
    }
    
}