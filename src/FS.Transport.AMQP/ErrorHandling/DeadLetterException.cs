namespace FS.Transport.AMQP.ErrorHandling;

/// <summary>
/// Exception information for dead letter messages
/// </summary>
public class DeadLetterException
{
    public string? Type { get; set; }
    public string? Message { get; set; }
    public string? StackTrace { get; set; }
    public string? Source { get; set; }
}