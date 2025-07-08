namespace FS.RabbitMQ.Core.Configuration;

/// <summary>
/// Defines access permissions for messaging operations.
/// </summary>
[Flags]
public enum AccessPermissions
{
    /// <summary>
    /// No permissions granted.
    /// </summary>
    None = 0,

    /// <summary>
    /// Read permission for consuming messages.
    /// </summary>
    Read = 1,

    /// <summary>
    /// Write permission for publishing messages.
    /// </summary>
    Write = 2,

    /// <summary>
    /// Configure permission for topology management.
    /// </summary>
    Configure = 4,

    /// <summary>
    /// Combined read and write permissions.
    /// </summary>
    ReadWrite = Read | Write,

    /// <summary>
    /// All permissions granted.
    /// </summary>
    All = Read | Write | Configure
}