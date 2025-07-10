namespace FS.Transport.AMQP.Queue;

/// <summary>
/// Fluent builder for queue declaration
/// </summary>
public class QueueDeclarationBuilder
{
    private readonly IQueueManager _queueManager;
    private readonly string _name;
    private bool _durable = true;
    private bool _exclusive = false;
    private bool _autoDelete = false;
    private readonly Dictionary<string, object> _arguments = new();

    internal QueueDeclarationBuilder(IQueueManager queueManager, string name)
    {
        _queueManager = queueManager ?? throw new ArgumentNullException(nameof(queueManager));
        _name = name ?? throw new ArgumentNullException(nameof(name));
    }

    /// <summary>
    /// Sets durability
    /// </summary>
    /// <param name="durable">Durable flag</param>
    /// <returns>Builder for fluent configuration</returns>
    public QueueDeclarationBuilder WithDurable(bool durable = true)
    {
        _durable = durable;
        return this;
    }

    /// <summary>
    /// Sets exclusivity
    /// </summary>
    /// <param name="exclusive">Exclusive flag</param>
    /// <returns>Builder for fluent configuration</returns>
    public QueueDeclarationBuilder WithExclusive(bool exclusive = true)
    {
        _exclusive = exclusive;
        return this;
    }

    /// <summary>
    /// Sets auto-delete behavior
    /// </summary>
    /// <param name="autoDelete">Auto-delete flag</param>
    /// <returns>Builder for fluent configuration</returns>
    public QueueDeclarationBuilder WithAutoDelete(bool autoDelete = true)
    {
        _autoDelete = autoDelete;
        return this;
    }

    /// <summary>
    /// Adds an argument
    /// </summary>
    /// <param name="key">Argument key</param>
    /// <param name="value">Argument value</param>
    /// <returns>Builder for fluent configuration</returns>
    public QueueDeclarationBuilder WithArgument(string key, object value)
    {
        _arguments[key] = value;
        return this;
    }

    /// <summary>
    /// Declares the queue asynchronously
    /// </summary>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Queue declaration result</returns>
    public async Task<QueueDeclareResult?> DeclareAsync(CancellationToken cancellationToken = default)
    {
        return await _queueManager.DeclareAsync(_name, _durable, _exclusive, _autoDelete, _arguments, cancellationToken);
    }

    /// <summary>
    /// Declares the queue synchronously
    /// </summary>
    /// <returns>Queue declaration result</returns>
    public QueueDeclareResult? Declare()
    {
        return DeclareAsync().GetAwaiter().GetResult();
    }
}