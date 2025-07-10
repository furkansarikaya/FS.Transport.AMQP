using FS.Mediator.Features.RequestHandling.Core;
using Microsoft.Extensions.Logging;

namespace FS.Transport.AMQP.Queue;

/// <summary>
/// Handler for GetQueueInfoQuery
/// </summary>
public class GetQueueInfoQueryHandler : IRequestHandler<GetQueueInfoQuery, QueueInfoResult>
{
    private readonly IQueueManager _queueManager;
    private readonly ILogger<GetQueueInfoQueryHandler> _logger;

    public GetQueueInfoQueryHandler(IQueueManager queueManager, ILogger<GetQueueInfoQueryHandler> logger)
    {
        _queueManager = queueManager ?? throw new ArgumentNullException(nameof(queueManager));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<QueueInfoResult> HandleAsync(GetQueueInfoQuery request, CancellationToken cancellationToken = default)
    {
        try
        {
            var queueInfo = await _queueManager.GetInfoAsync(request.Name, cancellationToken);
            
            if (queueInfo == null)
            {
                return QueueInfoResult.CreateFailure($"Queue '{request.Name}' not found");
            }

            var bindings = request.IncludeBindings ? new List<QueueBindingInfo>() : null;
            return QueueInfoResult.CreateSuccess(queueInfo, bindings);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get queue info for {QueueName}", request.Name);
            return QueueInfoResult.CreateFailure(ex.Message);
        }
    }
}

/// <summary>
/// Handler for GetQueueStatisticsQuery
/// </summary>
public class GetQueueStatisticsQueryHandler : IRequestHandler<GetQueueStatisticsQuery, QueueStatisticsResult>
{
    private readonly IQueueManager _queueManager;
    private readonly ILogger<GetQueueStatisticsQueryHandler> _logger;

    public GetQueueStatisticsQueryHandler(IQueueManager queueManager, ILogger<GetQueueStatisticsQueryHandler> logger)
    {
        _queueManager = queueManager ?? throw new ArgumentNullException(nameof(queueManager));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<QueueStatisticsResult> HandleAsync(GetQueueStatisticsQuery request, CancellationToken cancellationToken = default)
    {
        try
        {
            var statistics = _queueManager.GetStatistics();
            var history = request.IncludeHistory ? new List<QueueStatisticsSnapshot>() : null;
            return QueueStatisticsResult.CreateSuccess(statistics, history);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get queue statistics");
            return QueueStatisticsResult.CreateFailure(ex.Message);
        }
    }
}

/// <summary>
/// Handler for GetQueueMessageCountQuery
/// </summary>
public class GetQueueMessageCountQueryHandler : IRequestHandler<GetQueueMessageCountQuery, QueueMessageCountResult>
{
    private readonly IQueueManager _queueManager;
    private readonly ILogger<GetQueueMessageCountQueryHandler> _logger;

    public GetQueueMessageCountQueryHandler(IQueueManager queueManager, ILogger<GetQueueMessageCountQueryHandler> logger)
    {
        _queueManager = queueManager ?? throw new ArgumentNullException(nameof(queueManager));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<QueueMessageCountResult> HandleAsync(GetQueueMessageCountQuery request, CancellationToken cancellationToken = default)
    {
        try
        {
            var messageCount = await _queueManager.GetMessageCountAsync(request.Name, cancellationToken);
            // Note: QueueManager returns only total count, we'd need RabbitMQ Management API for detailed counts
            return QueueMessageCountResult.CreateSuccess(messageCount ?? 0, messageCount ?? 0, 0);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get message count for queue {QueueName}", request.Name);
            return QueueMessageCountResult.CreateFailure(ex.Message);
        }
    }
}

/// <summary>
/// Handler for GetQueueConsumerCountQuery
/// </summary>
public class GetQueueConsumerCountQueryHandler : IRequestHandler<GetQueueConsumerCountQuery, QueueConsumerCountResult>
{
    private readonly IQueueManager _queueManager;
    private readonly ILogger<GetQueueConsumerCountQueryHandler> _logger;

    public GetQueueConsumerCountQueryHandler(IQueueManager queueManager, ILogger<GetQueueConsumerCountQueryHandler> logger)
    {
        _queueManager = queueManager ?? throw new ArgumentNullException(nameof(queueManager));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<QueueConsumerCountResult> HandleAsync(GetQueueConsumerCountQuery request, CancellationToken cancellationToken = default)
    {
        try
        {
            var consumerCount = await _queueManager.GetConsumerCountAsync(request.Name, cancellationToken);
            var details = request.IncludeDetails ? new List<QueueConsumerInfo>() : null;
            return QueueConsumerCountResult.CreateSuccess(consumerCount ?? 0, details);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get consumer count for queue {QueueName}", request.Name);
            return QueueConsumerCountResult.CreateFailure(ex.Message);
        }
    }
}

/// <summary>
/// Handler for QueueExistsQuery
/// </summary>
public class QueueExistsQueryHandler : IRequestHandler<QueueExistsQuery, QueueExistsResult>
{
    private readonly IQueueManager _queueManager;
    private readonly ILogger<QueueExistsQueryHandler> _logger;

    public QueueExistsQueryHandler(IQueueManager queueManager, ILogger<QueueExistsQueryHandler> logger)
    {
        _queueManager = queueManager ?? throw new ArgumentNullException(nameof(queueManager));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<QueueExistsResult> HandleAsync(QueueExistsQuery request, CancellationToken cancellationToken = default)
    {
        try
        {
            var exists = await _queueManager.ExistsAsync(request.Name, cancellationToken);
            return QueueExistsResult.CreateSuccess(exists);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to check if queue {QueueName} exists", request.Name);
            return QueueExistsResult.CreateFailure(ex.Message);
        }
    }
}

/// <summary>
/// Handler for GetQueueBindingsQuery
/// </summary>
public class GetQueueBindingsQueryHandler : IRequestHandler<GetQueueBindingsQuery, QueueBindingsResult>
{
    private readonly IQueueManager _queueManager;
    private readonly ILogger<GetQueueBindingsQueryHandler> _logger;

    public GetQueueBindingsQueryHandler(IQueueManager queueManager, ILogger<GetQueueBindingsQueryHandler> logger)
    {
        _queueManager = queueManager ?? throw new ArgumentNullException(nameof(queueManager));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<QueueBindingsResult> HandleAsync(GetQueueBindingsQuery request, CancellationToken cancellationToken = default)
    {
        try
        {
            // Note: QueueManager doesn't have a GetBindings method, so we'll return an empty result
            // In a real implementation, you'd add this method to IQueueManager
            var bindings = new List<QueueBindingInfo>();
            return QueueBindingsResult.CreateSuccess(bindings);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get bindings for queue {QueueName}", request.Name);
            return QueueBindingsResult.CreateFailure(ex.Message);
        }
    }
}

/// <summary>
/// Handler for GetQueueHealthQuery
/// </summary>
public class GetQueueHealthQueryHandler : IRequestHandler<GetQueueHealthQuery, QueueHealthResult>
{
    private readonly IQueueManager _queueManager;
    private readonly ILogger<GetQueueHealthQueryHandler> _logger;

    public GetQueueHealthQueryHandler(IQueueManager queueManager, ILogger<GetQueueHealthQueryHandler> logger)
    {
        _queueManager = queueManager ?? throw new ArgumentNullException(nameof(queueManager));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<QueueHealthResult> HandleAsync(GetQueueHealthQuery request, CancellationToken cancellationToken = default)
    {
        try
        {
            var healthStatus = QueueHealthStatus.Healthy;
            var healthInfo = request.IncludeDetails ? new List<QueueHealthInfo>() : null;
            
            if (request.Name != null)
            {
                var exists = await _queueManager.ExistsAsync(request.Name, cancellationToken);
                if (!exists)
                {
                    healthStatus = QueueHealthStatus.Unhealthy;
                }
            }

            return QueueHealthResult.CreateSuccess(healthStatus, healthInfo);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get health for queue {QueueName}", request.Name);
            return QueueHealthResult.CreateFailure(ex.Message);
        }
    }
}

/// <summary>
/// Handler for GetQueuePerformanceQuery
/// </summary>
public class GetQueuePerformanceQueryHandler : IRequestHandler<GetQueuePerformanceQuery, QueuePerformanceResult>
{
    private readonly IQueueManager _queueManager;
    private readonly ILogger<GetQueuePerformanceQueryHandler> _logger;

    public GetQueuePerformanceQueryHandler(IQueueManager queueManager, ILogger<GetQueuePerformanceQueryHandler> logger)
    {
        _queueManager = queueManager ?? throw new ArgumentNullException(nameof(queueManager));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<QueuePerformanceResult> HandleAsync(GetQueuePerformanceQuery request, CancellationToken cancellationToken = default)
    {
        try
        {
            var statistics = _queueManager.GetStatistics();
            var performance = new QueuePerformanceData
            {
                QueueName = request.Name,
                TimeWindow = request.TimeWindow,
                AverageLatency = statistics.AverageOperationTime,
                Timestamp = DateTimeOffset.UtcNow
            };

            return QueuePerformanceResult.CreateSuccess(performance);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get performance data for queue {QueueName}", request.Name);
            return QueuePerformanceResult.CreateFailure(ex.Message);
        }
    }
}

/// <summary>
/// Handler for ListQueuesQuery
/// </summary>
public class ListQueuesQueryHandler : IRequestHandler<ListQueuesQuery, QueueListResult>
{
    private readonly IQueueManager _queueManager;
    private readonly ILogger<ListQueuesQueryHandler> _logger;

    public ListQueuesQueryHandler(IQueueManager queueManager, ILogger<ListQueuesQueryHandler> logger)
    {
        _queueManager = queueManager ?? throw new ArgumentNullException(nameof(queueManager));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<QueueListResult> HandleAsync(ListQueuesQuery request, CancellationToken cancellationToken = default)
    {
        try
        {
            // Note: QueueManager doesn't have a ListQueues method, so we'll return an empty result
            // In a real implementation, you'd add this method to IQueueManager or use RabbitMQ Management API
            var queues = new List<QueueInfo>();
            return QueueListResult.CreateSuccess(queues, 0);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to list queues");
            return QueueListResult.CreateFailure(ex.Message);
        }
    }
} 