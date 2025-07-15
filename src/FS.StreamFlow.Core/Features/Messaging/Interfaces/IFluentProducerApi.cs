using FS.StreamFlow.Core.Features.Messaging.Models;

namespace FS.StreamFlow.Core.Features.Messaging.Interfaces;

/// <summary>
/// Fluent API interface for advanced producer configuration and message publishing
/// </summary>
/// <typeparam name="T">Message type</typeparam>
public interface IFluentProducerApi<T> where T : class
{
    /// <summary>
    /// Configures the exchange to publish to
    /// </summary>
    /// <param name="exchangeName">Exchange name</param>
    /// <returns>Fluent producer API for method chaining</returns>
    IFluentProducerApi<T> WithExchange(string exchangeName);
    
    /// <summary>
    /// Configures the routing key for message routing
    /// </summary>
    /// <param name="routingKey">Routing key</param>
    /// <returns>Fluent producer API for method chaining</returns>
    IFluentProducerApi<T> WithRoutingKey(string routingKey);
    
    /// <summary>
    /// Configures message delivery mode
    /// </summary>
    /// <param name="deliveryMode">Delivery mode (persistent or non-persistent)</param>
    /// <returns>Fluent producer API for method chaining</returns>
    IFluentProducerApi<T> WithDeliveryMode(DeliveryMode deliveryMode);
    
    /// <summary>
    /// Configures message priority
    /// </summary>
    /// <param name="priority">Message priority (0-255)</param>
    /// <returns>Fluent producer API for method chaining</returns>
    IFluentProducerApi<T> WithPriority(byte priority);
    
    /// <summary>
    /// Configures message expiration
    /// </summary>
    /// <param name="expiration">Message expiration time</param>
    /// <returns>Fluent producer API for method chaining</returns>
    IFluentProducerApi<T> WithExpiration(TimeSpan expiration);
    
    /// <summary>
    /// Configures message headers
    /// </summary>
    /// <param name="headers">Message headers dictionary</param>
    /// <returns>Fluent producer API for method chaining</returns>
    IFluentProducerApi<T> WithHeaders(Dictionary<string, object> headers);
    
    /// <summary>
    /// Adds a single header to the message
    /// </summary>
    /// <param name="key">Header key</param>
    /// <param name="value">Header value</param>
    /// <returns>Fluent producer API for method chaining</returns>
    IFluentProducerApi<T> WithHeader(string key, object value);
    
    /// <summary>
    /// Configures content type
    /// </summary>
    /// <param name="contentType">Content type (e.g., "application/json")</param>
    /// <returns>Fluent producer API for method chaining</returns>
    IFluentProducerApi<T> WithContentType(string contentType);
    
    /// <summary>
    /// Configures content encoding
    /// </summary>
    /// <param name="contentEncoding">Content encoding (e.g., "utf-8")</param>
    /// <returns>Fluent producer API for method chaining</returns>
    IFluentProducerApi<T> WithContentEncoding(string contentEncoding);
    
    /// <summary>
    /// Configures correlation ID for message tracking
    /// </summary>
    /// <param name="correlationId">Correlation ID</param>
    /// <returns>Fluent producer API for method chaining</returns>
    IFluentProducerApi<T> WithCorrelationId(string correlationId);
    
    /// <summary>
    /// Configures message ID
    /// </summary>
    /// <param name="messageId">Message ID</param>
    /// <returns>Fluent producer API for method chaining</returns>
    IFluentProducerApi<T> WithMessageId(string messageId);
    
    /// <summary>
    /// Configures whether the message is mandatory
    /// </summary>
    /// <param name="mandatory">Mandatory flag</param>
    /// <returns>Fluent producer API for method chaining</returns>
    IFluentProducerApi<T> WithMandatory(bool mandatory = true);
    
    /// <summary>
    /// Configures publisher confirmation timeout
    /// </summary>
    /// <param name="timeout">Confirmation timeout</param>
    /// <returns>Fluent producer API for method chaining</returns>
    IFluentProducerApi<T> WithConfirmationTimeout(TimeSpan timeout);
    
    /// <summary>
    /// Publishes the message with the configured settings
    /// </summary>
    /// <param name="message">Message to publish</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Task representing the publish operation with results</returns>
    Task<PublishResult> PublishAsync(T message, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Publishes the message synchronously with the configured settings
    /// </summary>
    /// <param name="message">Message to publish</param>
    /// <returns>Publish result</returns>
    PublishResult Publish(T message);
} 