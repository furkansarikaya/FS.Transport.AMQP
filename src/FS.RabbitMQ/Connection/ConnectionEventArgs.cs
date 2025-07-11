namespace FS.RabbitMQ.Connection;

/// <summary>
/// Event arguments for connection events
/// </summary>
public class ConnectionEventArgs : EventArgs
{
    /// <summary>
    /// Gets the message describing the event
    /// </summary>
    public string Message { get; }

    /// <summary>
    /// Gets the exception associated with the event (if any)
    /// </summary>
    public Exception? Exception { get; }

    /// <summary>
    /// Gets when the event occurred
    /// </summary>
    public DateTimeOffset Timestamp { get; }

    /// <summary>
    /// Gets additional event data
    /// </summary>
    public Dictionary<string, object?> Data { get; }

    /// <summary>
    /// Initializes a new instance of the ConnectionEventArgs class
    /// </summary>
    /// <param name="message">Message describing the event</param>
    /// <param name="exception">Exception associated with the event</param>
    public ConnectionEventArgs(string message, Exception? exception = null)
    {
        Message = message ?? throw new ArgumentNullException(nameof(message));
        Exception = exception;
        Timestamp = DateTimeOffset.UtcNow;
        Data = new Dictionary<string, object?>();
    }

    /// <summary>
    /// Initializes a new instance of the ConnectionEventArgs class with additional data
    /// </summary>
    /// <param name="message">Message describing the event</param>
    /// <param name="exception">Exception associated with the event</param>
    /// <param name="data">Additional event data</param>
    public ConnectionEventArgs(string message, Exception? exception, Dictionary<string, object?> data)
    {
        Message = message ?? throw new ArgumentNullException(nameof(message));
        Exception = exception;
        Timestamp = DateTimeOffset.UtcNow;
        Data = data ?? new Dictionary<string, object?>();
    }

    /// <summary>
    /// Adds additional data to the event
    /// </summary>
    /// <param name="key">Data key</param>
    /// <param name="value">Data value</param>
    public void AddData(string key, object? value)
    {
        Data[key] = value;
    }

    /// <summary>
    /// Gets data from the event
    /// </summary>
    /// <typeparam name="T">Type of the data</typeparam>
    /// <param name="key">Data key</param>
    /// <returns>The data value or default if not found</returns>
    public T? GetData<T>(string key)
    {
        if (Data.TryGetValue(key, out var value))
        {
            try
            {
                return (T?)value;
            }
            catch
            {
                return default;
            }
        }
        return default;
    }

    /// <summary>
    /// Returns a string representation of the event
    /// </summary>
    public override string ToString()
    {
        var result = $"[{Timestamp:yyyy-MM-dd HH:mm:ss.fff}] {Message}";
        if (Exception != null)
        {
            result += $" - Exception: {Exception.Message}";
        }
        return result;
    }
}