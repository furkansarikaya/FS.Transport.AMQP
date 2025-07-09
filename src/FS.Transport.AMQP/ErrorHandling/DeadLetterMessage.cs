namespace FS.Transport.AMQP.ErrorHandling;

/// <summary>
/// Dead letter message structure for JSON serialization
/// </summary>
public class DeadLetterMessage
{
    public string? MessageId { get; set; }
    public string? CorrelationId { get; set; }
    public DateTime OriginalTimestamp { get; set; }
    public DateTime ErrorTimestamp { get; set; }
    public int AttemptCount { get; set; }
    public string? OriginalExchange { get; set; }
    public string? OriginalRoutingKey { get; set; }
    public string? OriginalQueue { get; set; }
    public string? ConsumerTag { get; set; }
    public string? Operation { get; set; }
    public DeadLetterException? Exception { get; set; }
    public IDictionary<string, object>? Headers { get; set; }
    public IDictionary<string, object>? Properties { get; set; }
    public byte[]? OriginalMessage { get; set; }
}