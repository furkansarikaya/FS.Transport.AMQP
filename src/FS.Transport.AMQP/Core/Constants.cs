namespace FS.Transport.AMQP.Core;

/// <summary>
/// Constants used throughout the RabbitMQ client library
/// </summary>
public static class Constants
{
    /// <summary>
    /// Default connection timeout in milliseconds
    /// </summary>
    public const int DefaultConnectionTimeout = 30000; // 30 seconds
    
    /// <summary>
    /// Default heartbeat interval in seconds
    /// </summary>
    public const ushort DefaultHeartbeat = 60;
    
    /// <summary>
    /// Default requested channel max
    /// </summary>
    public const ushort DefaultRequestedChannelMax = 2047;
    
    /// <summary>
    /// Default requested frame max
    /// </summary>
    public const uint DefaultRequestedFrameMax = 0; // No limit
    
    /// <summary>
    /// Default retry count for operations
    /// </summary>
    public const int DefaultRetryCount = 3;
    
    /// <summary>
    /// Default retry delay in milliseconds
    /// </summary>
    public const int DefaultRetryDelayMs = 1000;
    
    /// <summary>
    /// Maximum retry delay in milliseconds
    /// </summary>
    public const int MaxRetryDelayMs = 30000; // 30 seconds
    
    /// <summary>
    /// Default exchange names
    /// </summary>
    public static class Exchanges
    {
        public const string Default = "";
        public const string DeadLetter = "dlx";
        public const string DomainEvents = "domain-events";
        public const string IntegrationEvents = "integration-events";
        public const string EventStore = "event-store";
    }
    
    /// <summary>
    /// Default queue names
    /// </summary>
    public static class Queues
    {
        public const string DeadLetter = "dlq";
        public const string EventStore = "event-store";
    }
    
    /// <summary>
    /// Default routing keys
    /// </summary>
    public static class RoutingKeys
    {
        public const string Default = "";
        public const string All = "#";
        public const string DomainEvents = "domain.*";
        public const string IntegrationEvents = "integration.*";
    }
    
    /// <summary>
    /// Header keys used in messages
    /// </summary>
    public static class Headers
    {
        public const string MessageId = "x-message-id";
        public const string CorrelationId = "x-correlation-id";
        public const string EventType = "x-event-type";
        public const string EventVersion = "x-event-version";
        public const string AggregateId = "x-aggregate-id";
        public const string AggregateType = "x-aggregate-type";
        public const string Source = "x-source";
        public const string Timestamp = "x-timestamp";
        public const string RetryCount = "x-retry-count";
        public const string DeathReason = "x-death-reason";
        public const string OriginalExchange = "x-original-exchange";
        public const string OriginalRoutingKey = "x-original-routing-key";
    }
    
    /// <summary>
    /// Content types for message serialization
    /// </summary>
    public static class ContentTypes
    {
        public const string Json = "application/json";
        public const string Binary = "application/octet-stream";
        public const string Text = "text/plain";
        public const string Xml = "application/xml";
    }
    
    /// <summary>
    /// Default prefetch counts
    /// </summary>
    public static class PrefetchCounts
    {
        public const ushort Default = 10;
        public const ushort High = 50;
        public const ushort Low = 1;
    }
}
