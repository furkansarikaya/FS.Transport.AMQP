namespace FS.Transport.AMQP.Core;

/// <summary>
/// Event arguments for client status changes
/// </summary>
public class ClientStatusChangedEventArgs : EventArgs
{
    /// <summary>
    /// Previous client status
    /// </summary>
    public ClientStatus PreviousStatus { get; }
    
    /// <summary>
    /// Current client status
    /// </summary>
    public ClientStatus CurrentStatus { get; }
    
    /// <summary>
    /// Timestamp of the status change
    /// </summary>
    public DateTime Timestamp { get; }

    public ClientStatusChangedEventArgs(ClientStatus previousStatus, ClientStatus currentStatus)
    {
        PreviousStatus = previousStatus;
        CurrentStatus = currentStatus;
        Timestamp = DateTime.UtcNow;
    }
}