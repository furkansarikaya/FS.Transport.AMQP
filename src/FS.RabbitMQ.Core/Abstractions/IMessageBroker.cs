namespace FS.RabbitMQ.Core.Abstractions;

/// <summary>
/// Represents the main entry point for all messaging operations in the RabbitMQ broker.
/// Provides high-level abstractions for publishing, consuming, and managing message lifecycle.
/// </summary>
/// <remarks>
/// This interface serves as the primary facade for interacting with RabbitMQ messaging infrastructure.
/// It encapsulates connection management, channel lifecycle, and provides a simplified API for common messaging patterns.
/// Implementation should handle connection resilience, automatic reconnection, and graceful degradation scenarios.
/// </remarks>
public interface IMessageBroker : IAsyncDisposable, IDisposable
{
    /// <summary>
    /// Gets the current connection state of the message broker.
    /// </summary>
    /// <value>The current connection state indicating whether the broker is connected, disconnected, or in a transitional state.</value>
    ConnectionState ConnectionState { get; }

    /// <summary>
    /// Gets the publisher instance for sending messages to exchanges and queues.
    /// </summary>
    /// <value>An implementation of IMessagePublisher for message publishing operations.</value>
    IMessagePublisher Publisher { get; }

    /// <summary>
    /// Gets the consumer factory for creating message consumers with different consumption patterns.
    /// </summary>
    /// <value>A factory implementation for creating specialized message consumers.</value>
    IMessageConsumerFactory ConsumerFactory { get; }

    /// <summary>
    /// Establishes connection to the RabbitMQ broker with the configured connection parameters.
    /// </summary>
    /// <param name="cancellationToken">Token to monitor for cancellation requests during connection establishment.</param>
    /// <returns>A task that represents the asynchronous connection operation.</returns>
    /// <exception cref="BrokerConnectionException">Thrown when connection cannot be established after exhausting retry attempts.</exception>
    /// <exception cref="BrokerConfigurationException">Thrown when broker configuration is invalid or incomplete.</exception>
    Task ConnectAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Gracefully disconnects from the RabbitMQ broker, ensuring all pending messages are processed.
    /// </summary>
    /// <param name="cancellationToken">Token to monitor for cancellation requests during disconnection.</param>
    /// <returns>A task that represents the asynchronous disconnection operation.</returns>
    Task DisconnectAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Ensures that the specified exchange exists with the given configuration.
    /// Creates the exchange if it doesn't exist or validates existing exchange configuration.
    /// </summary>
    /// <param name="exchangeDeclaration">Configuration parameters for the exchange declaration.</param>
    /// <param name="cancellationToken">Token to monitor for cancellation requests.</param>
    /// <returns>A task that represents the asynchronous exchange declaration operation.</returns>
    /// <exception cref="ExchangeDeclarationException">Thrown when exchange declaration fails due to configuration conflicts.</exception>
    Task DeclareExchangeAsync(ExchangeDeclaration exchangeDeclaration, CancellationToken cancellationToken = default);

    /// <summary>
    /// Ensures that the specified queue exists with the given configuration.
    /// Creates the queue if it doesn't exist or validates existing queue configuration.
    /// </summary>
    /// <param name="queueDeclaration">Configuration parameters for the queue declaration.</param>
    /// <param name="cancellationToken">Token to monitor for cancellation requests.</param>
    /// <returns>A task that represents the asynchronous queue declaration operation containing queue information.</returns>
    /// <exception cref="QueueDeclarationException">Thrown when queue declaration fails due to configuration conflicts.</exception>
    Task<QueueDeclareResult> DeclareQueueAsync(QueueDeclaration queueDeclaration, CancellationToken cancellationToken = default);

    /// <summary>
    /// Binds a queue to an exchange with the specified routing configuration.
    /// </summary>
    /// <param name="binding">Configuration parameters for the queue binding operation.</param>
    /// <param name="cancellationToken">Token to monitor for cancellation requests.</param>
    /// <returns>A task that represents the asynchronous binding operation.</returns>
    /// <exception cref="BindingException">Thrown when binding operation fails due to configuration or connectivity issues.</exception>
    Task BindQueueAsync(QueueBinding binding, CancellationToken cancellationToken = default);

    /// <summary>
    /// Event raised when the broker connection state changes.
    /// </summary>
    /// <remarks>
    /// This event is useful for implementing connection monitoring, logging, and automatic failover scenarios.
    /// Subscribers should be prepared to handle rapid state changes during connection instability.
    /// </remarks>
    event EventHandler<ConnectionStateChangedEventArgs> ConnectionStateChanged;

    /// <summary>
    /// Event raised when an unhandled error occurs in the broker operations.
    /// </summary>
    /// <remarks>
    /// This event captures errors that cannot be attributed to specific operations and require global error handling.
    /// Common scenarios include background reconnection failures, channel recovery errors, and protocol-level exceptions.
    /// </remarks>
    event EventHandler<BrokerErrorEventArgs> ErrorOccurred;
}