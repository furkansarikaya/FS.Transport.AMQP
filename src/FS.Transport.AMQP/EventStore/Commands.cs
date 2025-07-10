using FS.Mediator.Features.RequestHandling.Core;
using FS.Transport.AMQP.Events;

namespace FS.Transport.AMQP.EventStore;

/// <summary>
/// Command to save events to the event store
/// </summary>
public class SaveEventsCommand : IRequest<SaveEventsResult>
{
    /// <summary>
    /// Events to save
    /// </summary>
    public IEnumerable<IEvent> Events { get; set; } = Enumerable.Empty<IEvent>();
    
    /// <summary>
    /// Aggregate identifier
    /// </summary>
    public string AggregateId { get; set; } = string.Empty;
    
    /// <summary>
    /// Type of the aggregate
    /// </summary>
    public string AggregateType { get; set; } = string.Empty;
    
    /// <summary>
    /// Expected version for concurrency control
    /// </summary>
    public long ExpectedVersion { get; set; } = -1;
    
    /// <summary>
    /// Creates a SaveEventsCommand
    /// </summary>
    /// <param name="aggregateId">Aggregate identifier</param>
    /// <param name="aggregateType">Aggregate type</param>
    /// <param name="events">Events to save</param>
    /// <param name="expectedVersion">Expected version</param>
    /// <returns>SaveEventsCommand instance</returns>
    public static SaveEventsCommand Create(string aggregateId, string aggregateType, IEnumerable<IEvent> events, long expectedVersion = -1)
    {
        return new SaveEventsCommand
        {
            AggregateId = aggregateId,
            AggregateType = aggregateType,
            Events = events,
            ExpectedVersion = expectedVersion
        };
    }
}

/// <summary>
/// Result of saving events
/// </summary>
public class SaveEventsResult
{
    /// <summary>
    /// New version after saving events
    /// </summary>
    public long NewVersion { get; set; }
    
    /// <summary>
    /// Number of events saved
    /// </summary>
    public int EventsSaved { get; set; }
    
    /// <summary>
    /// Whether the save operation was successful
    /// </summary>
    public bool Success { get; set; }
    
    /// <summary>
    /// Error message if save failed
    /// </summary>
    public string? ErrorMessage { get; set; }
    
    /// <summary>
    /// Creates a successful save result
    /// </summary>
    /// <param name="newVersion">New version</param>
    /// <param name="eventsSaved">Number of events saved</param>
    /// <returns>Successful save result</returns>
    public static SaveEventsResult CreateSuccess(long newVersion, int eventsSaved)
    {
        return new SaveEventsResult
        {
            Success = true,
            NewVersion = newVersion,
            EventsSaved = eventsSaved
        };
    }
    
    /// <summary>
    /// Creates a failed save result
    /// </summary>
    /// <param name="errorMessage">Error message</param>
    /// <returns>Failed save result</returns>
    public static SaveEventsResult CreateFailure(string errorMessage)
    {
        return new SaveEventsResult
        {
            Success = false,
            ErrorMessage = errorMessage
        };
    }
}

/// <summary>
/// Command to delete events from the event store
/// </summary>
public class DeleteEventsCommand : IRequest<DeleteEventsResult>
{
    /// <summary>
    /// Aggregate identifier
    /// </summary>
    public string AggregateId { get; set; } = string.Empty;
    
    /// <summary>
    /// Type of the aggregate
    /// </summary>
    public string AggregateType { get; set; } = string.Empty;
    
    /// <summary>
    /// Whether to delete the entire stream
    /// </summary>
    public bool DeleteEntireStream { get; set; }
    
    /// <summary>
    /// Creates a DeleteEventsCommand for entire stream
    /// </summary>
    /// <param name="aggregateId">Aggregate identifier</param>
    /// <param name="aggregateType">Aggregate type</param>
    /// <returns>DeleteEventsCommand instance</returns>
    public static DeleteEventsCommand DeleteStream(string aggregateId, string aggregateType)
    {
        return new DeleteEventsCommand
        {
            AggregateId = aggregateId,
            AggregateType = aggregateType,
            DeleteEntireStream = true
        };
    }
}

/// <summary>
/// Result of deleting events
/// </summary>
public class DeleteEventsResult
{
    /// <summary>
    /// Number of events deleted
    /// </summary>
    public int EventsDeleted { get; set; }
    
    /// <summary>
    /// Whether the delete operation was successful
    /// </summary>
    public bool Success { get; set; }
    
    /// <summary>
    /// Error message if delete failed
    /// </summary>
    public string? ErrorMessage { get; set; }
    
    /// <summary>
    /// Creates a successful delete result
    /// </summary>
    /// <param name="eventsDeleted">Number of events deleted</param>
    /// <returns>Successful delete result</returns>
    public static DeleteEventsResult CreateSuccess(int eventsDeleted)
    {
        return new DeleteEventsResult
        {
            Success = true,
            EventsDeleted = eventsDeleted
        };
    }
    
    /// <summary>
    /// Creates a failed delete result
    /// </summary>
    /// <param name="errorMessage">Error message</param>
    /// <returns>Failed delete result</returns>
    public static DeleteEventsResult CreateFailure(string errorMessage)
    {
        return new DeleteEventsResult
        {
            Success = false,
            ErrorMessage = errorMessage
        };
    }
} 