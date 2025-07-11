using FS.RabbitMQ.ErrorHandling;
using FS.RabbitMQ.Events;
using FS.RabbitMQ.RetryPolicies;
using Microsoft.Extensions.Logging;

namespace FS.RabbitMQ.EventHandlers;

/// <summary>
/// Default implementation of event handler factory
/// </summary>
public class EventHandlerFactory : IEventHandlerFactory
{
    private readonly IEventHandlerRegistry _registry;
    private readonly IErrorHandler _errorHandler;
    private readonly IRetryPolicy _retryPolicy;
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<EventHandlerFactory> _logger;
    private readonly EventHandlerExecutionStatistics _statistics;
    private readonly SemaphoreSlim _concurrencyLimiter;

    public EventHandlerFactory(
        IEventHandlerRegistry registry,
        IErrorHandler errorHandler,
        IRetryPolicy retryPolicy,
        IServiceProvider serviceProvider,
        ILogger<EventHandlerFactory> logger)
    {
        _registry = registry ?? throw new ArgumentNullException(nameof(registry));
        _errorHandler = errorHandler ?? throw new ArgumentNullException(nameof(errorHandler));
        _retryPolicy = retryPolicy ?? throw new ArgumentNullException(nameof(retryPolicy));
        _serviceProvider = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _statistics = new EventHandlerExecutionStatistics();
        _concurrencyLimiter = new SemaphoreSlim(Environment.ProcessorCount * 2, Environment.ProcessorCount * 2);
    }

    /// <summary>
    /// Executes all handlers for a specific event
    /// </summary>
    public async Task<IEnumerable<EventHandlingResult>> ExecuteHandlersAsync(IEvent @event, EventContext context, CancellationToken cancellationToken = default)
    {
        if (@event == null)
            throw new ArgumentNullException(nameof(@event));
        if (context == null)
            throw new ArgumentNullException(nameof(context));

        var handlers = _registry.GetHandlersForEvent(@event.GetType()).ToList();
        if (!handlers.Any())
        {
            _logger.LogDebug("No handlers found for event type {EventType}", @event.EventType);
            return Enumerable.Empty<EventHandlingResult>();
        }

        _logger.LogDebug("Executing {HandlerCount} handlers for event {EventType} (HandlingId: {HandlingId})", 
            handlers.Count, @event.EventType, context.HandlingId);

        _statistics.TotalEvents++;
        var startTime = DateTime.UtcNow;
        var results = new List<EventHandlingResult>();

        try
        {
            // Group handlers by concurrency preference
            var concurrentHandlers = handlers.Where(h => h.AllowConcurrentExecution).ToList();
            var sequentialHandlers = handlers.Where(h => !h.AllowConcurrentExecution).ToList();

            // Execute concurrent handlers in parallel
            if (concurrentHandlers.Any())
            {
                var concurrentTasks = concurrentHandlers.Select(handler => 
                    ExecuteHandlerWithLimitingAsync(handler, @event, context, cancellationToken));
                
                var concurrentResults = await Task.WhenAll(concurrentTasks);
                results.AddRange(concurrentResults);
            }

            // Execute sequential handlers one by one
            foreach (var handler in sequentialHandlers)
            {
                var result = await ExecuteHandlerAsync(handler, @event, context, cancellationToken);
                results.Add(result);
            }

            var successCount = results.Count(r => r.IsSuccess);
            var duration = DateTime.UtcNow - startTime;
            
            _statistics.SuccessfulEvents += successCount > 0 ? 1 : 0;
            _statistics.FailedEvents += successCount == 0 ? 1 : 0;
            _statistics.AverageExecutionTime = CalculateAverageTime(duration);
            
            _logger.LogInformation("Completed execution of {HandlerCount} handlers for event {EventType} in {Duration}ms - {SuccessCount} successful, {FailureCount} failed (HandlingId: {HandlingId})", 
                handlers.Count, @event.EventType, duration.TotalMilliseconds, successCount, results.Count - successCount, context.HandlingId);

            return results;
        }
        catch (Exception ex)
        {
            _statistics.FailedEvents++;
            var duration = DateTime.UtcNow - startTime;
            _statistics.AverageExecutionTime = CalculateAverageTime(duration);
            
            _logger.LogError(ex, "Failed to execute handlers for event {EventType} (HandlingId: {HandlingId})", 
                @event.EventType, context.HandlingId);
            throw;
        }
    }

    /// <summary>
    /// Executes a specific handler with concurrency limiting
    /// </summary>
    private async Task<EventHandlingResult> ExecuteHandlerWithLimitingAsync(IEventHandler handler, IEvent @event, EventContext context, CancellationToken cancellationToken)
    {
        await _concurrencyLimiter.WaitAsync(cancellationToken);
        try
        {
            return await ExecuteHandlerAsync(handler, @event, context, cancellationToken);
        }
        finally
        {
            _concurrencyLimiter.Release();
        }
    }

    /// <summary>
    /// Executes a specific handler for an event
    /// </summary>
    public async Task<EventHandlingResult> ExecuteHandlerAsync(IEventHandler handler, IEvent @event, EventContext context, CancellationToken cancellationToken = default)
    {
        if (handler == null)
            throw new ArgumentNullException(nameof(handler));
        if (@event == null)
            throw new ArgumentNullException(nameof(@event));
        if (context == null)
            throw new ArgumentNullException(nameof(context));

        var handlerContext = new EventContext(@event, cancellationToken)
        {
            HandlerName = handler.HandlerName,
            AttemptCount = context.AttemptCount,
            Source = context.Source,
            RoutingKey = context.RoutingKey,
            Exchange = context.Exchange,
            Queue = context.Queue
        };

        foreach (var property in context.Properties)
        {
            handlerContext.Properties[property.Key] = property.Value;
        }

        _statistics.TotalHandlerExecutions++;
        var startTime = DateTime.UtcNow;

        try
        {
            _logger.LogDebug("Executing handler {HandlerName} for event {EventType} (HandlingId: {HandlingId})", 
                handler.HandlerName, @event.EventType, context.HandlingId);

            // Resolve handler if it's a DI wrapper
            var actualHandler = handler is DependencyInjectionEventHandler diHandler 
                ? diHandler.ResolveHandler() 
                : handler;

            // Execute with retry policy
            await _retryPolicy.ExecuteAsync(async () =>
            {
                await ExecuteHandlerInternalAsync(actualHandler, @event, handlerContext, cancellationToken);
            }, cancellationToken);

            var duration = DateTime.UtcNow - startTime;
            _statistics.SuccessfulHandlerExecutions++;
            _statistics.AverageHandlerExecutionTime = CalculateAverageHandlerTime(duration);

            _logger.LogDebug("Successfully executed handler {HandlerName} for event {EventType} in {Duration}ms (HandlingId: {HandlingId})", 
                handler.HandlerName, @event.EventType, duration.TotalMilliseconds, context.HandlingId);

            return EventHandlingResult.Success(handler.HandlerName, duration);
        }
        catch (Exception ex)
        {
            var duration = DateTime.UtcNow - startTime;
            _statistics.FailedHandlerExecutions++;
            _statistics.AverageHandlerExecutionTime = CalculateAverageHandlerTime(duration);

            _logger.LogError(ex, "Failed to execute handler {HandlerName} for event {EventType} after {Duration}ms (HandlingId: {HandlingId})", 
                handler.HandlerName, @event.EventType, duration.TotalMilliseconds, context.HandlingId);

            // Handle the error through error handler
            var errorContext = ErrorHandling.ErrorContext.FromEvent(ex, @event, $"EventHandler:{handler.HandlerName}");
            await _errorHandler.HandleErrorAsync(errorContext, cancellationToken);

            return EventHandlingResult.Failure(handler.HandlerName, ex, duration);
        }
    }

    /// <summary>
    /// Internal handler execution with type checking and delegation
    /// </summary>
    private static async Task ExecuteHandlerInternalAsync(IEventHandler handler, IEvent @event, EventContext context, CancellationToken cancellationToken)
    {
        var eventType = @event.GetType();
        var handlerType = handler.GetType();

        // Check for async handler first
        var asyncHandlerInterface = handlerType.GetInterfaces()
            .FirstOrDefault(i => i.IsGenericType && 
                            i.GetGenericTypeDefinition() == typeof(IAsyncEventHandler<>) &&
                            i.GetGenericArguments()[0].IsAssignableFrom(eventType));

        if (asyncHandlerInterface != null)
        {
            var method = asyncHandlerInterface.GetMethod("HandleAsync");
            if (method != null)
            {
                var task = (Task)method.Invoke(handler, new object[] { @event, context, cancellationToken })!;
                await task;
                return;
            }
        }

        // Check for sync handler
        var syncHandlerInterface = handlerType.GetInterfaces()
            .FirstOrDefault(i => i.IsGenericType && 
                            i.GetGenericTypeDefinition() == typeof(IEventHandler<>) &&
                            i.GetGenericArguments()[0].IsAssignableFrom(eventType));

        if (syncHandlerInterface != null)
        {
            var method = syncHandlerInterface.GetMethod("Handle");
            method?.Invoke(handler, new object[] { @event, context });
            return;
        }

        throw new InvalidOperationException($"Handler {handler.HandlerName} does not implement a compatible interface for event type {eventType.Name}");
    }

    /// <summary>
    /// Gets execution statistics for event handlers
    /// </summary>
    public EventHandlerExecutionStatistics GetStatistics()
    {
        return _statistics.Clone();
    }

    private TimeSpan CalculateAverageTime(TimeSpan currentDuration)
    {
        var totalEvents = _statistics.SuccessfulEvents + _statistics.FailedEvents;
        if (totalEvents <= 1)
            return currentDuration;

        var currentAverage = _statistics.AverageExecutionTime;
        var newAverage = ((currentAverage.TotalMilliseconds * (totalEvents - 1)) + currentDuration.TotalMilliseconds) / totalEvents;
        
        return TimeSpan.FromMilliseconds(newAverage);
    }

    private TimeSpan CalculateAverageHandlerTime(TimeSpan currentDuration)
    {
        var totalExecutions = _statistics.SuccessfulHandlerExecutions + _statistics.FailedHandlerExecutions;
        if (totalExecutions <= 1)
            return currentDuration;

        var currentAverage = _statistics.AverageHandlerExecutionTime;
        var newAverage = ((currentAverage.TotalMilliseconds * (totalExecutions - 1)) + currentDuration.TotalMilliseconds) / totalExecutions;
        
        return TimeSpan.FromMilliseconds(newAverage);
    }
}
