using RabbitMQ.Client;

namespace FS.Transport.AMQP.Connection;

/// <summary>
/// Represents a pooled channel with metadata
/// </summary>
internal class PooledChannel(IModel channel, DateTime created)
{
    private static readonly TimeSpan DefaultTtl = TimeSpan.FromMinutes(30);
    
    public IModel Channel { get; } = channel ?? throw new ArgumentNullException(nameof(channel));
    public DateTime Created { get; } = created;
    public DateTime LastUsed { get; set; } = created;

    public bool IsExpired => DateTime.UtcNow - LastUsed > DefaultTtl;
}