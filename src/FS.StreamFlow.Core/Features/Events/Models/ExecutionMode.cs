namespace FS.StreamFlow.Core.Features.Events.Models;

/// <summary>
/// Execution mode for hybrid handlers
/// </summary>
public enum ExecutionMode
{
    /// <summary>
    /// Prefer synchronous execution
    /// </summary>
    Synchronous,
    
    /// <summary>
    /// Prefer asynchronous execution
    /// </summary>
    Asynchronous
}