using FS.Transport.AMQP.Events;
using Microsoft.Extensions.Logging;

namespace FS.Transport.AMQP.EventHandlers;

/// <summary>
/// Base class for event handlers providing common functionality and logging
/// </summary>
/// <typeparam name="T">Event type</typeparam>
public abstract class EventHandlerBase<T> : IAsyncEventHandler<T> where T : class, IEvent
{
    protected readonly ILogger Logger;

    /// <summary>
    /// Event types this handler can process
    /// </summary>
    public virtual Type[] EventTypes => new[] { typeof(T) };
    
    /// <summary>
    /// Handler name for identification and logging
    /// </summary>
    public virtual string HandlerName => GetType().Name;
    
    /// <summary>
    /// Handler priority for ordering (higher values execute first)
    /// </summary>
    public virtual int Priority => 0;
    
    /// <summary>
    /// Whether this handler should run concurrently with other handlers
    /// </summary>
    public virtual bool AllowConcurrentExecution => true;

    protected EventHandlerBase(ILogger logger)
    {
        Logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>
    /// Determines if this handler can handle the specified event type
    /// </summary>
    /// <param name="eventType">Event type to check</param>
    /// <returns>True if handler can process this event type</returns>
    public virtual bool CanHandle(Type eventType)
    {
        return EventTypes.Any(t => t.IsAssignableFrom(eventType));
    }

    /// <summary>
    /// Handles the event asynchronously with logging and error handling
    /// </summary>
    /// <param name="event">Event instance</param>
    /// <param name="context">Event handling context</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Task representing the handling operation</returns>
    public async Task HandleAsync(T @event, EventContext context, CancellationToken cancellationToken = default)
    {
        if (@event == null)
            throw new ArgumentNullException(nameof(@event));
        if (context == null)
            throw new ArgumentNullException(nameof(context));

        context.WithHandler(HandlerName);
        
        Logger.LogDebug("Starting to handle event {EventType} with handler {HandlerName} (HandlingId: {HandlingId})", 
            @event.EventType, HandlerName, context.HandlingId);

        var startTime = DateTime.UtcNow;
        try
        {
            await OnHandleAsync(@event, context, cancellationToken);
            
            var duration = DateTime.UtcNow - startTime;
            Logger.LogInformation("Successfully handled event {EventType} with handler {HandlerName} in {Duration}ms (HandlingId: {HandlingId})", 
                @event.EventType, HandlerName, duration.TotalMilliseconds, context.HandlingId);
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            Logger.LogWarning("Event handling was cancelled for {EventType} with handler {HandlerName} (HandlingId: {HandlingId})", 
                @event.EventType, HandlerName, context.HandlingId);
            throw;
        }
        catch (Exception ex)
        {
            var duration = DateTime.UtcNow - startTime;
            Logger.LogError(ex, "Failed to handle event {EventType} with handler {HandlerName} after {Duration}ms (HandlingId: {HandlingId})", 
                @event.EventType, HandlerName, duration.TotalMilliseconds, context.HandlingId);
            throw;
        }
    }

    /// <summary>
    /// Abstract method that derived classes must implement for actual event handling
    /// </summary>
    /// <param name="event">Event instance</param>
    /// <param name="context">Event handling context</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Task representing the handling operation</returns>
    protected abstract Task OnHandleAsync(T @event, EventContext context, CancellationToken cancellationToken);

    /// <summary>
    /// Validates the event before processing (override for custom validation)
    /// </summary>
    /// <param name="event">Event to validate</param>
    /// <param name="context">Event context</param>
    /// <returns>True if event is valid</returns>
    protected virtual bool ValidateEvent(T @event, EventContext context)
    {
        return @event != null && !string.IsNullOrEmpty(@event.EventType);
    }

    /// <summary>
    /// Called before event handling (override for custom pre-processing)
    /// </summary>
    /// <param name="event">Event instance</param>
    /// <param name="context">Event context</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Task representing the pre-processing operation</returns>
    protected virtual Task OnBeforeHandleAsync(T @event, EventContext context, CancellationToken cancellationToken)
    {
        return Task.CompletedTask;
    }

    /// <summary>
    /// Called after successful event handling (override for custom post-processing)
    /// </summary>
    /// <param name="event">Event instance</param>
    /// <param name="context">Event context</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Task representing the post-processing operation</returns>
    protected virtual Task OnAfterHandleAsync(T @event, EventContext context, CancellationToken cancellationToken)
    {
        return Task.CompletedTask;
    }

    /// <summary>
    /// Called when event handling fails (override for custom error handling)
    /// </summary>
    /// <param name="event">Event instance</param>
    /// <param name="context">Event context</param>
    /// <param name="exception">Exception that occurred</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Task representing the error handling operation</returns>
    protected virtual Task OnHandleErrorAsync(T @event, EventContext context, Exception exception, CancellationToken cancellationToken)
    {
        return Task.CompletedTask;
    }
}