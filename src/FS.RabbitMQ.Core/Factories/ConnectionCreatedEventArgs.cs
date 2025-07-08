using RabbitMQ.Client;

namespace FS.RabbitMQ.Core.Factories;

/// <summary>
/// Event arguments for connection creation events.
/// </summary>
public sealed class ConnectionCreatedEventArgs : EventArgs
{
    /// <summary>
    /// Initializes a new instance of the <see cref="ConnectionCreatedEventArgs"/> class.
    /// </summary>
    /// <param name="connection">The created connection.</param>
    /// <param name="connectionName">The name of the connection.</param>
    public ConnectionCreatedEventArgs(IConnection connection, string connectionName)
    {
        Connection = connection ?? throw new ArgumentNullException(nameof(connection));
        ConnectionName = connectionName ?? throw new ArgumentNullException(nameof(connectionName));
        CreatedAt = DateTimeOffset.UtcNow;
    }

    /// <summary>
    /// Gets the created connection.
    /// </summary>
    public IConnection Connection { get; }

    /// <summary>
    /// Gets the name of the connection.
    /// </summary>
    public string ConnectionName { get; }

    /// <summary>
    /// Gets the time when the connection was created.
    /// </summary>
    public DateTimeOffset CreatedAt { get; }
}