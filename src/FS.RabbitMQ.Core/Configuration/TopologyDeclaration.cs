using FS.RabbitMQ.Core.Abstractions;

namespace FS.RabbitMQ.Core.Configuration;

/// <summary>
/// Represents a complete topology declaration including exchanges, queues, and bindings.
/// </summary>
public sealed class TopologyDeclaration
{
    /// <summary>
    /// Gets or sets the exchanges to be declared.
    /// </summary>
    /// <value>A collection of exchange declarations.</value>
    public IList<ExchangeDeclaration>? Exchanges { get; set; }

    /// <summary>
    /// Gets or sets the queues to be declared.
    /// </summary>
    /// <value>A collection of queue declarations.</value>
    public IList<QueueDeclaration>? Queues { get; set; }

    /// <summary>
    /// Gets or sets the bindings to be created.
    /// </summary>
    /// <value>A collection of binding declarations.</value>
    public IList<QueueBinding>? Bindings { get; set; }

    /// <summary>
    /// Validates the topology declaration.
    /// </summary>
    /// <exception cref="BrokerConfigurationException">Thrown when the declaration is invalid.</exception>
    public void Validate()
    {
        if (Exchanges?.Any() == true)
        {
            foreach (var exchange in Exchanges)
            {
                exchange.Validate();
            }
        }

        if (Queues?.Any() == true)
        {
            foreach (var queue in Queues)
            {
                queue.Validate();
            }
        }

        if (Bindings?.Any() != true) return;
        foreach (var binding in Bindings)
        {
            binding.Validate();
        }
    }
}