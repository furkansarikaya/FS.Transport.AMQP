using FS.Mediator.Features.RequestHandling.Core;

namespace FS.Transport.AMQP.EventStore;

/// <summary>
/// Query to get events from the event store
/// </summary>
public class GetEventsQuery : IRequest<GetEventsResult>
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
    /// Starting version (inclusive)
    /// </summary>
    public long? FromVersion { get; set; }
    
    /// <summary>
    /// Ending version (inclusive)
    /// </summary>
    public long? ToVersion { get; set; }
    
    /// <summary>
    /// Maximum number of events to return
    /// </summary>
    public int? MaxEvents { get; set; }
    
    /// <summary>
    /// Skip number of events
    /// </summary>
    public int? Skip { get; set; }
    
    /// <summary>
    /// Include processed events
    /// </summary>
    public bool IncludeProcessed { get; set; } = true;
    
    /// <summary>
    /// Creates a GetEventsQuery for all events
    /// </summary>
    /// <param name="aggregateId">Aggregate identifier</param>
    /// <param name="aggregateType">Aggregate type</param>
    /// <returns>GetEventsQuery instance</returns>
    public static GetEventsQuery AllEvents(string aggregateId, string aggregateType)
    {
        return new GetEventsQuery
        {
            AggregateId = aggregateId,
            AggregateType = aggregateType
        };
    }
}

/// <summary>
/// Result of getting events
/// </summary>
public class GetEventsResult
{
    /// <summary>
    /// Retrieved events
    /// </summary>
    public IEnumerable<EventSnapshot> Events { get; set; } = Enumerable.Empty<EventSnapshot>();
    
    /// <summary>
    /// Total number of events available
    /// </summary>
    public long TotalEvents { get; set; }
    
    /// <summary>
    /// Current version of the aggregate
    /// </summary>
    public long CurrentVersion { get; set; }
    
    /// <summary>
    /// Whether the query was successful
    /// </summary>
    public bool Success { get; set; }
    
    /// <summary>
    /// Error message if query failed
    /// </summary>
    public string? ErrorMessage { get; set; }
    
    /// <summary>
    /// Creates a successful get events result
    /// </summary>
    /// <param name="events">Retrieved events</param>
    /// <param name="totalEvents">Total events available</param>
    /// <param name="currentVersion">Current version</param>
    /// <returns>Successful get events result</returns>
    public static GetEventsResult CreateSuccess(IEnumerable<EventSnapshot> events, long totalEvents, long currentVersion)
    {
        return new GetEventsResult
        {
            Success = true,
            Events = events,
            TotalEvents = totalEvents,
            CurrentVersion = currentVersion
        };
    }
    
    /// <summary>
    /// Creates a failed get events result
    /// </summary>
    /// <param name="errorMessage">Error message</param>
    /// <returns>Failed get events result</returns>
    public static GetEventsResult CreateFailure(string errorMessage)
    {
        return new GetEventsResult
        {
            Success = false,
            ErrorMessage = errorMessage
        };
    }
}

/// <summary>
/// Query to get an event stream
/// </summary>
public class GetEventStreamQuery : IRequest<GetEventStreamResult>
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
    /// Include stream metadata
    /// </summary>
    public bool IncludeMetadata { get; set; } = true;
    
    /// <summary>
    /// Creates a GetEventStreamQuery
    /// </summary>
    /// <param name="aggregateId">Aggregate identifier</param>
    /// <param name="aggregateType">Aggregate type</param>
    /// <returns>GetEventStreamQuery instance</returns>
    public static GetEventStreamQuery Create(string aggregateId, string aggregateType)
    {
        return new GetEventStreamQuery
        {
            AggregateId = aggregateId,
            AggregateType = aggregateType
        };
    }
}

/// <summary>
/// Result of getting an event stream
/// </summary>
public class GetEventStreamResult
{
    /// <summary>
    /// Event stream
    /// </summary>
    public IEventStream? EventStream { get; set; }
    
    /// <summary>
    /// Whether the stream exists
    /// </summary>
    public bool StreamExists { get; set; }
    
    /// <summary>
    /// Whether the query was successful
    /// </summary>
    public bool Success { get; set; }
    
    /// <summary>
    /// Error message if query failed
    /// </summary>
    public string? ErrorMessage { get; set; }
    
    /// <summary>
    /// Creates a successful get event stream result
    /// </summary>
    /// <param name="eventStream">Event stream</param>
    /// <returns>Successful get event stream result</returns>
    public static GetEventStreamResult CreateSuccess(IEventStream eventStream)
    {
        return new GetEventStreamResult
        {
            Success = true,
            EventStream = eventStream,
            StreamExists = true
        };
    }
    
    /// <summary>
    /// Creates a result indicating stream does not exist
    /// </summary>
    /// <returns>Stream not found result</returns>
    public static GetEventStreamResult StreamNotFound()
    {
        return new GetEventStreamResult
        {
            Success = true,
            StreamExists = false
        };
    }
    
    /// <summary>
    /// Creates a failed get event stream result
    /// </summary>
    /// <param name="errorMessage">Error message</param>
    /// <returns>Failed get event stream result</returns>
    public static GetEventStreamResult CreateFailure(string errorMessage)
    {
        return new GetEventStreamResult
        {
            Success = false,
            ErrorMessage = errorMessage
        };
    }
}

/// <summary>
/// Query to check if an event stream exists
/// </summary>
public class StreamExistsQuery : IRequest<StreamExistsResult>
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
    /// Creates a StreamExistsQuery
    /// </summary>
    /// <param name="aggregateId">Aggregate identifier</param>
    /// <param name="aggregateType">Aggregate type</param>
    /// <returns>StreamExistsQuery instance</returns>
    public static StreamExistsQuery Create(string aggregateId, string aggregateType)
    {
        return new StreamExistsQuery
        {
            AggregateId = aggregateId,
            AggregateType = aggregateType
        };
    }
}

/// <summary>
/// Result of checking if a stream exists
/// </summary>
public class StreamExistsResult
{
    /// <summary>
    /// Whether the stream exists
    /// </summary>
    public bool Exists { get; set; }
    
    /// <summary>
    /// Current version of the stream (if exists)
    /// </summary>
    public long? CurrentVersion { get; set; }
    
    /// <summary>
    /// Whether the query was successful
    /// </summary>
    public bool Success { get; set; }
    
    /// <summary>
    /// Error message if query failed
    /// </summary>
    public string? ErrorMessage { get; set; }
    
    /// <summary>
    /// Creates a successful stream exists result
    /// </summary>
    /// <param name="exists">Whether stream exists</param>
    /// <param name="currentVersion">Current version if exists</param>
    /// <returns>Successful stream exists result</returns>
    public static StreamExistsResult CreateSuccess(bool exists, long? currentVersion = null)
    {
        return new StreamExistsResult
        {
            Success = true,
            Exists = exists,
            CurrentVersion = currentVersion
        };
    }
    
    /// <summary>
    /// Creates a failed stream exists result
    /// </summary>
    /// <param name="errorMessage">Error message</param>
    /// <returns>Failed stream exists result</returns>
    public static StreamExistsResult CreateFailure(string errorMessage)
    {
        return new StreamExistsResult
        {
            Success = false,
            ErrorMessage = errorMessage
        };
    }
} 