using FS.StreamFlow.Core.Features.Events.Interfaces;

namespace FS.StreamFlow.Examples.RabbitMQ.Models;

public record PaymentProcessed(
    Guid OrderId,
    string TransactionId,
    decimal Amount) : IIntegrationEvent
{
    public Guid Id { get; } = Guid.NewGuid();
    public DateTime OccurredOn { get; } = DateTime.UtcNow;
    public int Version { get; } = 1;
    public string EventType => nameof(PaymentProcessed);
    public string? CorrelationId { get; set; }
    public string? CausationId { get; set; }
    public IDictionary<string, object> Metadata { get; } = new Dictionary<string, object>();
    public string Source => "payment-service";
    public string ExchangeName => "payment-events";
    public string? Target { get; set; }
    public string SchemaVersion => "1.0";
    public TimeSpan? TimeToLive { get; set; }
}