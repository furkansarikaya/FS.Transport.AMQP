namespace FS.RabbitMQ.ErrorHandling;

/// <summary>
/// Event arguments for error handling events
/// </summary>
public class ErrorHandlingEventArgs : EventArgs
{
    /// <summary>
    /// Error context
    /// </summary>
    public ErrorContext Context { get; }
    
    /// <summary>
    /// Error handling strategy used
    /// </summary>
    public ErrorHandlingStrategy Strategy { get; }
    
    /// <summary>
    /// Result of error handling (if completed)
    /// </summary>
    public ErrorHandlingResult? Result { get; }
    
    /// <summary>
    /// Timestamp of the event
    /// </summary>
    public DateTime Timestamp { get; }

    public ErrorHandlingEventArgs(ErrorContext context, ErrorHandlingStrategy strategy, ErrorHandlingResult? result = null)
    {
        Context = context ?? throw new ArgumentNullException(nameof(context));
        Strategy = strategy;
        Result = result;
        Timestamp = DateTime.UtcNow;
    }

    /// <summary>
    /// Gets a string representation of the event
    /// </summary>
    /// <returns>Event description</returns>
    public override string ToString()
    {
        var result = Result != null ? $", Result: {Result.Action}" : "";
        return $"[{Timestamp:yyyy-MM-dd HH:mm:ss}] Error handling: {Strategy} for {Context}{result}";
    }
}