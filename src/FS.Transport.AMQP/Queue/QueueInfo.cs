namespace FS.Transport.AMQP.Queue;

/// <summary>
/// Information about an existing queue
/// </summary>
public class QueueInfo
{
    /// <summary>
    /// Queue name
    /// </summary>
    public string Name { get; set; } = string.Empty;
    
    /// <summary>
    /// Number of messages in the queue
    /// </summary>
    public uint MessageCount { get; set; }
    
    /// <summary>
    /// Number of consumers connected to the queue
    /// </summary>
    public uint ConsumerCount { get; set; }
    
    /// <summary>
    /// Whether the queue is durable
    /// </summary>
    public bool Durable { get; set; } = true;
    
    /// <summary>
    /// Whether the queue is exclusive
    /// </summary>
    public bool Exclusive { get; set; } = false;
    
    /// <summary>
    /// Whether the queue auto-deletes
    /// </summary>
    public bool AutoDelete { get; set; } = false;
    
    /// <summary>
    /// Additional arguments for the queue
    /// </summary>
    public IDictionary<string, object> Arguments { get; set; } = new Dictionary<string, object>();
    
    /// <summary>
    /// Queue state (running, idle, etc.) if available from management API
    /// </summary>
    public string? State { get; set; }
    
    /// <summary>
    /// Node where the queue is located if in a cluster
    /// </summary>
    public string? Node { get; set; }
    
    /// <summary>
    /// Queue type (classic, quorum, stream) if available
    /// </summary>
    public string? Type { get; set; }

    /// <summary>
    /// Gets the queue's TTL (Time To Live) setting if configured
    /// </summary>
    public TimeSpan? MessageTtl
    {
        get
        {
            if (Arguments.TryGetValue("x-message-ttl", out var ttl) && ttl is int ttlMs)
                return TimeSpan.FromMilliseconds(ttlMs);
            return null;
        }
    }

    /// <summary>
    /// Gets the queue's maximum length if configured
    /// </summary>
    public long? MaxLength
    {
        get
        {
            if (Arguments.TryGetValue("x-max-length", out var length) && length is long maxLen)
                return maxLen;
            return null;
        }
    }

    /// <summary>
    /// Gets the queue's maximum length in bytes if configured
    /// </summary>
    public long? MaxLengthBytes
    {
        get
        {
            if (Arguments.TryGetValue("x-max-length-bytes", out var bytes) && bytes is long maxBytes)
                return maxBytes;
            return null;
        }
    }

    /// <summary>
    /// Gets the queue's dead letter exchange if configured
    /// </summary>
    public string? DeadLetterExchange
    {
        get
        {
            if (Arguments.TryGetValue("x-dead-letter-exchange", out var dlx) && dlx is string dlxName)
                return dlxName;
            return null;
        }
    }

    /// <summary>
    /// Gets the queue's dead letter routing key if configured
    /// </summary>
    public string? DeadLetterRoutingKey
    {
        get
        {
            if (Arguments.TryGetValue("x-dead-letter-routing-key", out var dlrk) && dlrk is string dlrkValue)
                return dlrkValue;
            return null;
        }
    }

    /// <summary>
    /// Determines if the queue is a priority queue
    /// </summary>
    public bool IsPriorityQueue => Arguments.ContainsKey("x-max-priority");

    /// <summary>
    /// Gets the maximum priority level if this is a priority queue
    /// </summary>
    public int? MaxPriority
    {
        get
        {
            if (Arguments.TryGetValue("x-max-priority", out var priority) && priority is int maxPri)
                return maxPri;
            return null;
        }
    }

    /// <summary>
    /// Determines if the queue is a quorum queue
    /// </summary>
    public bool IsQuorumQueue => Type?.Equals("quorum", StringComparison.OrdinalIgnoreCase) == true ||
                                Arguments.TryGetValue("x-queue-type", out var queueType) && 
                                queueType?.ToString()?.Equals("quorum", StringComparison.OrdinalIgnoreCase) == true;

    /// <summary>
    /// Determines if the queue is a classic queue
    /// </summary>
    public bool IsClassicQueue => Type?.Equals("classic", StringComparison.OrdinalIgnoreCase) == true ||
                                 (!IsQuorumQueue && !Arguments.ContainsKey("x-queue-type"));

    /// <summary>
    /// Gets a string representation of the queue info
    /// </summary>
    /// <returns>Queue description</returns>
    public override string ToString()
    {
        var type = IsQuorumQueue ? "Quorum" : IsClassicQueue ? "Classic" : "Unknown";
        return $"Queue '{Name}' ({type}, Durable: {Durable}, Messages: {MessageCount}, Consumers: {ConsumerCount})";
    }
}