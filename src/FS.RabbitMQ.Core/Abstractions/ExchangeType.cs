namespace FS.RabbitMQ.Core.Abstractions;

/// <summary>
/// Defines the types of exchanges available in RabbitMQ for message routing.
/// </summary>
/// <remarks>
/// Each exchange type implements a different routing algorithm that determines
/// how messages are distributed to bound queues. Understanding these patterns
/// is crucial for designing effective message routing topologies.
/// </remarks>
public enum ExchangeType
{
    /// <summary>
    /// Direct exchange routes messages to queues whose binding key exactly matches the message routing key.
    /// This is the default exchange type and provides simple point-to-point routing.
    /// </summary>
    /// <remarks>
    /// Direct exchanges are ideal for:
    /// - Simple request-response patterns
    /// - Routing messages to specific workers based on message type
    /// - Load balancing messages among multiple consumers of the same queue
    /// 
    /// Performance characteristics: Very fast routing decisions with O(1) lookup complexity.
    /// </remarks>
    Direct,

    /// <summary>
    /// Fanout exchange routes messages to all bound queues, completely ignoring the routing key.
    /// This provides broadcast-style message distribution.
    /// </summary>
    /// <remarks>
    /// Fanout exchanges are ideal for:
    /// - Broadcasting notifications to multiple services
    /// - Event sourcing patterns where multiple read models need updates
    /// - Implementing publish-subscribe patterns without filtering
    /// 
    /// Performance characteristics: Fastest exchange type for multiple queue delivery.
    /// </remarks>
    Fanout,

    /// <summary>
    /// Topic exchange routes messages to queues based on wildcard pattern matching between
    /// the message routing key and queue binding patterns.
    /// </summary>
    /// <remarks>
    /// Topic exchanges support two wildcards:
    /// - * (asterisk) matches exactly one word
    /// - # (hash) matches zero or more words
    /// Words are separated by dots (.)
    /// 
    /// Example routing key: "orders.electronics.smartphones"
    /// Matching patterns: "orders.*.*", "orders.#", "*.electronics.*", "#.smartphones"
    /// 
    /// Topic exchanges are ideal for:
    /// - Hierarchical message categorization
    /// - Selective subscription patterns
    /// - Complex routing scenarios with partial matching requirements
    /// 
    /// Performance characteristics: Slower than direct/fanout due to pattern matching overhead.
    /// </remarks>
    Topic,

    /// <summary>
    /// Headers exchange routes messages based on header attributes rather than routing key.
    /// Routing decisions are made by matching message headers against binding arguments.
    /// </summary>
    /// <remarks>
    /// Headers exchanges support two matching modes:
    /// - "any": Message is routed if any header matches binding arguments
    /// - "all": Message is routed only if all headers match binding arguments
    /// 
    /// Headers exchanges are ideal for:
    /// - Complex routing based on multiple message attributes
    /// - Scenarios where routing key limitations are insufficient
    /// - Integration with systems that use header-based routing
    /// 
    /// Performance characteristics: Slowest exchange type due to header inspection overhead.
    /// </remarks>
    Headers
}