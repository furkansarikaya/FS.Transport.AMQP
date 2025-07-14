using FS.StreamFlow.Core.Features.Events.Models;

namespace FS.StreamFlow.Core.Features.Events.Interfaces;

/// <summary>
/// Factory for creating and executing event handlers with error handling and retry logic
/// </summary>
public interface IEventHandlerFactory
{
    /// <summary>
    /// Executes all handlers for a specific event
    /// </summary>
    /// <param name="event">Event to handle</param>
    /// <param name="context">Event handling context</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Collection of handling results</returns>
    Task<IEnumerable<EventHandlingResult>> ExecuteHandlersAsync(IEvent @event, EventContext context, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Executes a specific handler for an event
    /// </summary>
    /// <param name="handler">Handler to execute</param>
    /// <param name="event">Event to handle</param>
    /// <param name="context">Event handling context</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Handling result</returns>
    Task<EventHandlingResult> ExecuteHandlerAsync(IEventHandler handler, IEvent @event, EventContext context, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Gets execution statistics for event handlers
    /// </summary>
    /// <returns>Execution statistics</returns>
    EventHandlerExecutionStatistics GetStatistics();
}