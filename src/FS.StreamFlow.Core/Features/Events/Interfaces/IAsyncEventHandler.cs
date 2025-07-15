using FS.StreamFlow.Core.Features.Events.Models;

namespace FS.StreamFlow.Core.Features.Events.Interfaces;

/// <summary>
/// Generic asynchronous event handler interface
/// </summary>
/// <typeparam name="T">Event type</typeparam>
public interface IAsyncEventHandler<in T> : IEventHandler where T : class, IEvent
{
    /// <summary>
    /// Handles the event asynchronously
    /// </summary>
    /// <param name="event">Event instance</param>
    /// <param name="context">Event handling context</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Task representing the handling operation</returns>
    Task HandleAsync(T @event, EventContext context, CancellationToken cancellationToken = default);
}

/// <summary>
/// Interface for handlers that support both sync and async operations
/// </summary>
/// <typeparam name="T">Event type</typeparam>
public interface IHybridEventHandler<in T> : IEventHandler<T>, IAsyncEventHandler<T> where T : class, IEvent
{
    /// <summary>
    /// Preferred execution mode for this handler
    /// </summary>
    ExecutionMode PreferredExecutionMode { get; }
}