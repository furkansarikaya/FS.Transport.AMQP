namespace FS.Transport.AMQP.Producer;

/// <summary>
/// Fluent API for batch publishing
/// </summary>
/// <typeparam name="T">Message type</typeparam>
public class FluentBatchApi<T> where T : class
{
    private readonly IMessageProducer _producer;
    private readonly List<T> _messages;
    private string? _exchange;
    private Func<T, string>? _routingKeySelector;
    private readonly Dictionary<string, object> _commonHeaders = new();
    private byte? _priority;
    private bool _waitForConfirmations = true;

    internal FluentBatchApi(IMessageProducer producer, IEnumerable<T> messages)
    {
        _producer = producer ?? throw new ArgumentNullException(nameof(producer));
        _messages = messages?.ToList() ?? throw new ArgumentNullException(nameof(messages));
    }

    /// <summary>
    /// Sets the target exchange for all messages
    /// </summary>
    /// <param name="exchange">Exchange name</param>
    /// <returns>Fluent API instance</returns>
    public FluentBatchApi<T> ToExchange(string exchange)
    {
        _exchange = exchange ?? throw new ArgumentNullException(nameof(exchange));
        return this;
    }

    /// <summary>
    /// Sets routing key selector function
    /// </summary>
    /// <param name="selector">Routing key selector</param>
    /// <returns>Fluent API instance</returns>
    public FluentBatchApi<T> WithRoutingKey(Func<T, string> selector)
    {
        _routingKeySelector = selector ?? throw new ArgumentNullException(nameof(selector));
        return this;
    }

    /// <summary>
    /// Sets the same routing key for all messages
    /// </summary>
    /// <param name="routingKey">Routing key</param>
    /// <returns>Fluent API instance</returns>
    public FluentBatchApi<T> WithRoutingKey(string routingKey)
    {
        _routingKeySelector = _ => routingKey;
        return this;
    }

    /// <summary>
    /// Sets common priority for all messages
    /// </summary>
    /// <param name="priority">Priority (0-255)</param>
    /// <returns>Fluent API instance</returns>
    public FluentBatchApi<T> WithPriority(byte priority)
    {
        _priority = priority;
        return this;
    }

    /// <summary>
    /// Adds a common header to all messages
    /// </summary>
    /// <param name="key">Header key</param>
    /// <param name="value">Header value</param>
    /// <returns>Fluent API instance</returns>
    public FluentBatchApi<T> WithCommonHeader(string key, object value)
    {
        _commonHeaders[key] = value;
        return this;
    }

    /// <summary>
    /// Disables confirmation waiting for better performance
    /// </summary>
    /// <returns>Fluent API instance</returns>
    public FluentBatchApi<T> WithoutConfirmations()
    {
        _waitForConfirmations = false;
        return this;
    }

    /// <summary>
    /// Publishes all messages as batch
    /// </summary>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Batch publish result</returns>
    public async Task<BatchPublishResult> PublishAsync(CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrEmpty(_exchange) || _routingKeySelector == null)
        {
            throw new InvalidOperationException("Exchange and routing key selector must be specified");
        }

        var messageContexts = _messages.Select(message =>
        {
            var context = MessageContext.Create(_exchange, _routingKeySelector(message));
            context.WaitForConfirmation = _waitForConfirmations;
            
            if (_priority.HasValue)
                context.Priority = _priority.Value;
            
            if (_commonHeaders.Any())
                context.Headers = new Dictionary<string, object>(_commonHeaders);

            return MessageWithContext<T>.Create(message, context);
        });

        return await _producer.PublishBatchAsync(messageContexts, cancellationToken);
    }
}

/// <summary>
/// Fluent API for scheduled publishing
/// </summary>
/// <typeparam name="T">Message type</typeparam>
public class FluentScheduleApi<T> where T : class
{
    private readonly IMessageProducer _producer;
    private readonly T _message;
    private string? _exchange;
    private string? _routingKey;
    private DateTimeOffset? _scheduleTime;

    internal FluentScheduleApi(IMessageProducer producer, T message)
    {
        _producer = producer ?? throw new ArgumentNullException(nameof(producer));
        _message = message ?? throw new ArgumentNullException(nameof(message));
    }

    /// <summary>
    /// Sets the target exchange
    /// </summary>
    /// <param name="exchange">Exchange name</param>
    /// <returns>Fluent API instance</returns>
    public FluentScheduleApi<T> ToExchange(string exchange)
    {
        _exchange = exchange ?? throw new ArgumentNullException(nameof(exchange));
        return this;
    }

    /// <summary>
    /// Sets the routing key
    /// </summary>
    /// <param name="routingKey">Routing key</param>
    /// <returns>Fluent API instance</returns>
    public FluentScheduleApi<T> WithRoutingKey(string routingKey)
    {
        _routingKey = routingKey ?? throw new ArgumentNullException(nameof(routingKey));
        return this;
    }

    /// <summary>
    /// Schedules message to be published at specific time
    /// </summary>
    /// <param name="scheduleTime">Schedule time</param>
    /// <returns>Fluent API instance</returns>
    public FluentScheduleApi<T> At(DateTimeOffset scheduleTime)
    {
        _scheduleTime = scheduleTime;
        return this;
    }

    /// <summary>
    /// Schedules message to be published after a delay
    /// </summary>
    /// <param name="delay">Delay before publishing</param>
    /// <returns>Fluent API instance</returns>
    public FluentScheduleApi<T> After(TimeSpan delay)
    {
        _scheduleTime = DateTimeOffset.UtcNow.Add(delay);
        return this;
    }

    /// <summary>
    /// Schedules message to be published in specified minutes
    /// </summary>
    /// <param name="minutes">Minutes to wait</param>
    /// <returns>Fluent API instance</returns>
    public FluentScheduleApi<T> InMinutes(int minutes)
    {
        _scheduleTime = DateTimeOffset.UtcNow.AddMinutes(minutes);
        return this;
    }

    /// <summary>
    /// Schedules message to be published in specified hours
    /// </summary>
    /// <param name="hours">Hours to wait</param>
    /// <returns>Fluent API instance</returns>
    public FluentScheduleApi<T> InHours(int hours)
    {
        _scheduleTime = DateTimeOffset.UtcNow.AddHours(hours);
        return this;
    }

    /// <summary>
    /// Schedules message to be published tomorrow at specific time
    /// </summary>
    /// <param name="time">Time of day</param>
    /// <returns>Fluent API instance</returns>
    public FluentScheduleApi<T> TomorrowAt(TimeOnly time)
    {
        var tomorrow = DateOnly.FromDateTime(DateTime.Today.AddDays(1));
        var scheduleDateTime = tomorrow.ToDateTime(time);
        _scheduleTime = new DateTimeOffset(scheduleDateTime, TimeZoneInfo.Local.GetUtcOffset(scheduleDateTime));
        return this;
    }

    /// <summary>
    /// Executes the scheduling
    /// </summary>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Schedule result</returns>
    public async Task<ScheduleResult> ScheduleAsync(CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrEmpty(_exchange) || string.IsNullOrEmpty(_routingKey) || !_scheduleTime.HasValue)
        {
            throw new InvalidOperationException("Exchange, routing key, and schedule time must be specified");
        }

        return await _producer.ScheduleAsync(_message, _exchange, _routingKey, _scheduleTime.Value, cancellationToken);
    }
}

/// <summary>
/// Fluent API for transactional publishing
/// </summary>
public class FluentTransactionApi
{
    private readonly IMessageProducer _producer;
    private readonly string _transactionId;
    private readonly List<Func<string, Task>> _operations = new();

    internal FluentTransactionApi(IMessageProducer producer, string transactionId)
    {
        _producer = producer ?? throw new ArgumentNullException(nameof(producer));
        _transactionId = transactionId ?? throw new ArgumentNullException(nameof(transactionId));
    }

    /// <summary>
    /// Adds a message to the transaction
    /// </summary>
    /// <typeparam name="T">Message type</typeparam>
    /// <param name="message">Message to publish</param>
    /// <param name="exchange">Exchange name</param>
    /// <param name="routingKey">Routing key</param>
    /// <returns>Fluent API instance</returns>
    public FluentTransactionApi AddMessage<T>(T message, string exchange, string routingKey) where T : class
    {
        _operations.Add(async txId => 
            await _producer.PublishTransactionalAsync(message, exchange, routingKey, txId));
        return this;
    }

    /// <summary>
    /// Adds multiple messages to the transaction
    /// </summary>
    /// <typeparam name="T">Message type</typeparam>
    /// <param name="messages">Messages to publish</param>
    /// <param name="exchange">Exchange name</param>
    /// <param name="routingKeySelector">Routing key selector</param>
    /// <returns>Fluent API instance</returns>
    public FluentTransactionApi AddMessages<T>(IEnumerable<T> messages, string exchange, 
        Func<T, string> routingKeySelector) where T : class
    {
        foreach (var message in messages)
        {
            var routingKey = routingKeySelector(message);
            _operations.Add(async txId => 
                await _producer.PublishTransactionalAsync(message, exchange, routingKey, txId));
        }
        return this;
    }

    /// <summary>
    /// Adds a custom operation to the transaction
    /// </summary>
    /// <param name="operation">Operation to execute</param>
    /// <returns>Fluent API instance</returns>
    public FluentTransactionApi AddOperation(Func<string, Task> operation)
    {
        _operations.Add(operation ?? throw new ArgumentNullException(nameof(operation)));
        return this;
    }

    /// <summary>
    /// Executes all operations and commits the transaction
    /// </summary>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>True if transaction was committed successfully</returns>
    public async Task<bool> CommitAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            // Execute all operations
            foreach (var operation in _operations)
            {
                await operation(_transactionId);
            }

            // Commit transaction
            return await _producer.CommitTransactionAsync(_transactionId, cancellationToken);
        }
        catch
        {
            // Rollback on any error
            await _producer.RollbackTransactionAsync(_transactionId, cancellationToken);
            throw;
        }
    }

    /// <summary>
    /// Executes all operations and explicitly rolls back the transaction
    /// </summary>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>True if transaction was rolled back successfully</returns>
    public async Task<bool> RollbackAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            // Execute all operations
            foreach (var operation in _operations)
            {
                await operation(_transactionId);
            }
        }
        catch
        {
            // Continue with rollback even if operations failed
        }

        return await _producer.RollbackTransactionAsync(_transactionId, cancellationToken);
    }
} 