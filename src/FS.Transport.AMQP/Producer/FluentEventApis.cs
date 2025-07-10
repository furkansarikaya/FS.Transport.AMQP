using FS.Transport.AMQP.Events;

namespace FS.Transport.AMQP.Producer;

/// <summary>
/// Fluent API for publishing domain events
/// </summary>
/// <typeparam name="T">Domain event type</typeparam>
public class FluentDomainEventApi<T> where T : class, IDomainEvent
{
    private readonly IMessageProducer _producer;
    private readonly T _domainEvent;
    private readonly EventPublishContext _context;

    internal FluentDomainEventApi(IMessageProducer producer, T domainEvent)
    {
        _producer = producer ?? throw new ArgumentNullException(nameof(producer));
        _domainEvent = domainEvent ?? throw new ArgumentNullException(nameof(domainEvent));
        _context = EventPublishContext.CreateDomainEvent(
            domainEvent.AggregateId, 
            domainEvent.AggregateType, 
            domainEvent.EventType);
    }

    /// <summary>
    /// Sets the event sequence number
    /// </summary>
    /// <param name="sequence">Event sequence</param>
    /// <returns>Fluent API instance</returns>
    public FluentDomainEventApi<T> WithSequence(long sequence)
    {
        _context.EventSequence = sequence;
        return this;
    }

    /// <summary>
    /// Sets the causation ID
    /// </summary>
    /// <param name="causationId">Causation ID</param>
    /// <returns>Fluent API instance</returns>
    public FluentDomainEventApi<T> CausedBy(string causationId)
    {
        _context.CausationId = causationId;
        return this;
    }

    /// <summary>
    /// Sets the event version
    /// </summary>
    /// <param name="version">Event version</param>
    /// <returns>Fluent API instance</returns>
    public FluentDomainEventApi<T> WithVersion(string version)
    {
        _context.EventVersion = version;
        return this;
    }

    /// <summary>
    /// Sets retry policy
    /// </summary>
    /// <param name="policyName">Retry policy name</param>
    /// <returns>Fluent API instance</returns>
    public FluentDomainEventApi<T> WithRetryPolicy(string policyName)
    {
        _context.RetryPolicyName = policyName;
        return this;
    }

    /// <summary>
    /// Sets event as persistent
    /// </summary>
    /// <returns>Fluent API instance</returns>
    public FluentDomainEventApi<T> AsPersistent()
    {
        _context.DeliveryMode = 2;
        return this;
    }

    /// <summary>
    /// Enables confirmation waiting
    /// </summary>
    /// <returns>Fluent API instance</returns>
    public FluentDomainEventApi<T> WithConfirmation()
    {
        _context.WaitForConfirmation = true;
        return this;
    }

    /// <summary>
    /// Conditionally applies configuration
    /// </summary>
    /// <param name="condition">Condition to check</param>
    /// <param name="configure">Configuration action</param>
    /// <returns>Fluent API instance</returns>
    public FluentDomainEventApi<T> When(bool condition, Func<FluentDomainEventApi<T>, FluentDomainEventApi<T>> configure)
    {
        return condition ? configure(this) : this;
    }

    /// <summary>
    /// Publishes the domain event
    /// </summary>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Publish result</returns>
    public async Task<PublishResult> PublishAsync(CancellationToken cancellationToken = default)
    {
        return await _producer.PublishEventAsync(_domainEvent, _context, cancellationToken);
    }
}

/// <summary>
/// Fluent API for publishing integration events
/// </summary>
/// <typeparam name="T">Integration event type</typeparam>
public class FluentIntegrationEventApi<T> where T : class, IIntegrationEvent
{
    private readonly IMessageProducer _producer;
    private readonly T _integrationEvent;
    private readonly EventPublishContext _context;

    internal FluentIntegrationEventApi(IMessageProducer producer, T integrationEvent)
    {
        _producer = producer ?? throw new ArgumentNullException(nameof(producer));
        _integrationEvent = integrationEvent ?? throw new ArgumentNullException(nameof(integrationEvent));
        _context = EventPublishContext.CreateIntegrationEvent(
            integrationEvent.EventType, 
            integrationEvent.RoutingKey);
    }

    /// <summary>
    /// Sets custom routing key (overrides default)
    /// </summary>
    /// <param name="routingKey">Routing key</param>
    /// <returns>Fluent API instance</returns>
    public FluentIntegrationEventApi<T> WithRoutingKey(string routingKey)
    {
        _context.RoutingKey = routingKey;
        return this;
    }

    /// <summary>
    /// Sets event version
    /// </summary>
    /// <param name="version">Event version</param>
    /// <returns>Fluent API instance</returns>
    public FluentIntegrationEventApi<T> WithVersion(string version)
    {
        _context.EventVersion = version;
        return this;
    }

    /// <summary>
    /// Sets message priority
    /// </summary>
    /// <param name="priority">Priority (0-255)</param>
    /// <returns>Fluent API instance</returns>
    public FluentIntegrationEventApi<T> WithPriority(byte priority)
    {
        _context.Priority = priority;
        return this;
    }

    /// <summary>
    /// Sets TTL from event or custom value
    /// </summary>
    /// <param name="ttl">Time to live (optional, uses event TTL if not specified)</param>
    /// <returns>Fluent API instance</returns>
    public FluentIntegrationEventApi<T> WithTtl(TimeSpan? ttl = null)
    {
        _context.TimeToLive = ttl ?? _integrationEvent.TimeToLive ?? TimeSpan.FromMinutes(30);
        return this;
    }

    /// <summary>
    /// Sets event as persistent
    /// </summary>
    /// <returns>Fluent API instance</returns>
    public FluentIntegrationEventApi<T> AsPersistent()
    {
        _context.DeliveryMode = 2;
        return this;
    }

    /// <summary>
    /// Disables retry for this event
    /// </summary>
    /// <returns>Fluent API instance</returns>
    public FluentIntegrationEventApi<T> WithoutRetry()
    {
        _context.MaxRetries = 0;
        return this;
    }

    /// <summary>
    /// Disables confirmation for this event
    /// </summary>
    /// <returns>Fluent API instance</returns>
    public FluentIntegrationEventApi<T> WithoutConfirmation()
    {
        _context.WaitForConfirmation = false;
        return this;
    }

    /// <summary>
    /// Enables confirmation waiting
    /// </summary>
    /// <returns>Fluent API instance</returns>
    public FluentIntegrationEventApi<T> WithConfirmation()
    {
        _context.WaitForConfirmation = true;
        return this;
    }

    /// <summary>
    /// Sets retry policy
    /// </summary>
    /// <param name="policyName">Retry policy name</param>
    /// <returns>Fluent API instance</returns>
    public FluentIntegrationEventApi<T> WithRetryPolicy(string policyName)
    {
        _context.RetryPolicyName = policyName;
        return this;
    }

    /// <summary>
    /// Conditionally applies configuration
    /// </summary>
    /// <param name="condition">Condition to check</param>
    /// <param name="configure">Configuration action</param>
    /// <returns>Fluent API instance</returns>
    public FluentIntegrationEventApi<T> When(bool condition, Func<FluentIntegrationEventApi<T>, FluentIntegrationEventApi<T>> configure)
    {
        return condition ? configure(this) : this;
    }

    /// <summary>
    /// Publishes the integration event
    /// </summary>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Publish result</returns>
    public async Task<PublishResult> PublishAsync(CancellationToken cancellationToken = default)
    {
        return await _producer.PublishEventAsync(_integrationEvent, _context, cancellationToken);
    }
} 