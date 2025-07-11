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
        /// <summary>
        /// Message unique identifier header key
        /// </summary>
        public const string MessageId = "x-message-id";
        
        /// <summary>
        /// Correlation identifier header key for message correlation
        /// </summary>
        public const string CorrelationId = "x-correlation-id";
        
        /// <summary>
        /// Event type header key for event classification
        /// </summary>
        public const string EventType = "x-event-type";
        
        /// <summary>
        /// Event version header key for event versioning
        /// </summary>
        public const string EventVersion = "x-event-version";
        
        /// <summary>
        /// Aggregate identifier header key for domain events
        /// </summary>
        public const string AggregateId = "x-aggregate-id";
        
        /// <summary>
        /// Aggregate type header key for domain events
        /// </summary>
        public const string AggregateType = "x-aggregate-type";
        
        /// <summary>
        /// Source application header key for message origin tracking
        /// </summary>
        public const string Source = "x-source";
        
        /// <summary>
        /// Timestamp header key for message creation time
        /// </summary>
        public const string Timestamp = "x-timestamp";
        
        /// <summary>
        /// Retry count header key for retry tracking
        /// </summary>
        public const string RetryCount = "x-retry-count";
        
        /// <summary>
        /// Death reason header key for failed message diagnostics
        /// </summary>
        public const string DeathReason = "x-death-reason";
        
        /// <summary>
        /// Original exchange header key for dead letter queue tracking
        /// </summary>
        public const string OriginalExchange = "x-original-exchange";
        
        /// <summary>
        /// Original routing key header key for dead letter queue tracking
        /// </summary>
        public const string OriginalRoutingKey = "x-original-routing-key";
    }
    
    /// <summary>
    /// Content types for message serialization
    /// </summary>
    public static class ContentTypes
    {
        /// <summary>
        /// JSON content type for structured message data
        /// </summary>
        public const string Json = "application/json";
        
        /// <summary>
        /// Binary content type for raw binary data
        /// </summary>
        public const string Binary = "application/octet-stream";
        
        /// <summary>
        /// Plain text content type for simple text messages
        /// </summary>
        public const string Text = "text/plain";
        
        /// <summary>
        /// XML content type for structured XML data
        /// </summary>
        public const string Xml = "application/xml";
    }
    
    /// <summary>
    /// Default prefetch counts for different performance scenarios
    /// </summary>
    public static class PrefetchCounts
    {
        /// <summary>
        /// Default prefetch count for balanced performance
        /// </summary>
        public const ushort Default = 10;
        
        /// <summary>
        /// High prefetch count for high-throughput scenarios
        /// </summary>
        public const ushort High = 50;
        
        /// <summary>
        /// Low prefetch count for low-latency scenarios
        /// </summary>
        public const ushort Low = 1;
    }
}
