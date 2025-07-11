using FS.Mediator.Features.RequestHandling.Core;
using Microsoft.Extensions.Logging;

namespace FS.RabbitMQ.Exchange;

/// <summary>
/// Handler for GetExchangeInfoQuery
/// </summary>
public class GetExchangeInfoQueryHandler : IRequestHandler<GetExchangeInfoQuery, ExchangeInfoResult>
{
    private readonly IExchangeManager _exchangeManager;
    private readonly ILogger<GetExchangeInfoQueryHandler> _logger;

    public GetExchangeInfoQueryHandler(IExchangeManager exchangeManager, ILogger<GetExchangeInfoQueryHandler> logger)
    {
        _exchangeManager = exchangeManager ?? throw new ArgumentNullException(nameof(exchangeManager));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<ExchangeInfoResult> HandleAsync(GetExchangeInfoQuery request, CancellationToken cancellationToken = default)
    {
        try
        {
            var exchangeInfo = await _exchangeManager.GetInfoAsync(request.Name, cancellationToken);
            
            if (exchangeInfo == null)
            {
                return ExchangeInfoResult.CreateFailure($"Exchange '{request.Name}' not found");
            }

            var bindings = request.IncludeBindings ? new List<ExchangeBindingInfo>() : null;
            return ExchangeInfoResult.CreateSuccess(exchangeInfo, bindings);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get exchange info for {ExchangeName}", request.Name);
            return ExchangeInfoResult.CreateFailure(ex.Message);
        }
    }
}

/// <summary>
/// Handler for GetExchangeStatisticsQuery
/// </summary>
public class GetExchangeStatisticsQueryHandler : IRequestHandler<GetExchangeStatisticsQuery, ExchangeStatisticsResult>
{
    private readonly IExchangeManager _exchangeManager;
    private readonly ILogger<GetExchangeStatisticsQueryHandler> _logger;

    public GetExchangeStatisticsQueryHandler(IExchangeManager exchangeManager, ILogger<GetExchangeStatisticsQueryHandler> logger)
    {
        _exchangeManager = exchangeManager ?? throw new ArgumentNullException(nameof(exchangeManager));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <inheritdoc />
    public Task<ExchangeStatisticsResult> HandleAsync(GetExchangeStatisticsQuery request, CancellationToken cancellationToken = default)
    {
        try
        {
            var statistics = _exchangeManager.GetStatistics();
            var history = request.IncludeHistory ? new List<ExchangeStatisticsSnapshot>() : null;
            return Task.FromResult(ExchangeStatisticsResult.CreateSuccess(statistics, history));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get exchange statistics");
            return Task.FromResult(ExchangeStatisticsResult.CreateFailure(ex.Message));
        }
    }
}

/// <summary>
/// Handler for ExchangeExistsQuery
/// </summary>
public class ExchangeExistsQueryHandler : IRequestHandler<ExchangeExistsQuery, ExchangeExistsResult>
{
    private readonly IExchangeManager _exchangeManager;
    private readonly ILogger<ExchangeExistsQueryHandler> _logger;

    public ExchangeExistsQueryHandler(IExchangeManager exchangeManager, ILogger<ExchangeExistsQueryHandler> logger)
    {
        _exchangeManager = exchangeManager ?? throw new ArgumentNullException(nameof(exchangeManager));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<ExchangeExistsResult> HandleAsync(ExchangeExistsQuery request, CancellationToken cancellationToken = default)
    {
        try
        {
            var exists = await _exchangeManager.ExistsAsync(request.Name, cancellationToken);
            return ExchangeExistsResult.CreateSuccess(exists);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to check if exchange {ExchangeName} exists", request.Name);
            return ExchangeExistsResult.CreateFailure(ex.Message);
        }
    }
}

/// <summary>
/// Handler for GetExchangeBindingsQuery
/// </summary>
public class GetExchangeBindingsQueryHandler : IRequestHandler<GetExchangeBindingsQuery, ExchangeBindingsResult>
{
    private readonly IExchangeManager _exchangeManager;
    private readonly ILogger<GetExchangeBindingsQueryHandler> _logger;

    public GetExchangeBindingsQueryHandler(IExchangeManager exchangeManager, ILogger<GetExchangeBindingsQueryHandler> logger)
    {
        _exchangeManager = exchangeManager ?? throw new ArgumentNullException(nameof(exchangeManager));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<ExchangeBindingsResult> HandleAsync(GetExchangeBindingsQuery request, CancellationToken cancellationToken = default)
    {
        try
        {
            var bindings = await _exchangeManager.GetBindingsAsync(request.Name, cancellationToken);
            return ExchangeBindingsResult.CreateSuccess(bindings.ToList());
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get bindings for exchange {ExchangeName}", request.Name);
            return ExchangeBindingsResult.CreateFailure(ex.Message);
        }
    }
}

/// <summary>
/// Handler for GetExchangeHealthQuery
/// </summary>
public class GetExchangeHealthQueryHandler : IRequestHandler<GetExchangeHealthQuery, ExchangeHealthResult>
{
    private readonly IExchangeManager _exchangeManager;
    private readonly ILogger<GetExchangeHealthQueryHandler> _logger;

    public GetExchangeHealthQueryHandler(IExchangeManager exchangeManager, ILogger<GetExchangeHealthQueryHandler> logger)
    {
        _exchangeManager = exchangeManager ?? throw new ArgumentNullException(nameof(exchangeManager));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<ExchangeHealthResult> HandleAsync(GetExchangeHealthQuery request, CancellationToken cancellationToken = default)
    {
        try
        {
            var healthStatus = ExchangeHealthStatus.Healthy;
            var healthInfo = request.IncludeDetails ? new List<ExchangeHealthInfo>() : null;
            
            if (request.Name != null)
            {
                var exists = await _exchangeManager.ExistsAsync(request.Name, cancellationToken);
                if (!exists)
                {
                    healthStatus = ExchangeHealthStatus.Unhealthy;
                }
            }

            return ExchangeHealthResult.CreateSuccess(healthStatus, healthInfo);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get health for exchange {ExchangeName}", request.Name);
            return ExchangeHealthResult.CreateFailure(ex.Message);
        }
    }
}

/// <summary>
/// Handler for GetExchangePerformanceQuery
/// </summary>
public class GetExchangePerformanceQueryHandler : IRequestHandler<GetExchangePerformanceQuery, ExchangePerformanceResult>
{
    private readonly IExchangeManager _exchangeManager;
    private readonly ILogger<GetExchangePerformanceQueryHandler> _logger;

    public GetExchangePerformanceQueryHandler(IExchangeManager exchangeManager, ILogger<GetExchangePerformanceQueryHandler> logger)
    {
        _exchangeManager = exchangeManager ?? throw new ArgumentNullException(nameof(exchangeManager));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <inheritdoc />
    public Task<ExchangePerformanceResult> HandleAsync(GetExchangePerformanceQuery request, CancellationToken cancellationToken = default)
    {
        try
        {
            var statistics = _exchangeManager.GetStatistics();
            var performance = new ExchangePerformanceData
            {
                ExchangeName = request.Name,
                TimeWindow = request.TimeWindow,
                AverageRoutingLatency = statistics.AverageOperationTime,
                Timestamp = DateTimeOffset.UtcNow
            };

            return Task.FromResult(ExchangePerformanceResult.CreateSuccess(performance));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get performance data for exchange {ExchangeName}", request.Name);
            return Task.FromResult(ExchangePerformanceResult.CreateFailure(ex.Message));
        }
    }
}

/// <summary>
/// Handler for ListExchangesQuery
/// </summary>
public class ListExchangesQueryHandler : IRequestHandler<ListExchangesQuery, ExchangeListResult>
{
    private readonly IExchangeManager _exchangeManager;
    private readonly ILogger<ListExchangesQueryHandler> _logger;

    public ListExchangesQueryHandler(IExchangeManager exchangeManager, ILogger<ListExchangesQueryHandler> logger)
    {
        _exchangeManager = exchangeManager ?? throw new ArgumentNullException(nameof(exchangeManager));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<ExchangeListResult> HandleAsync(ListExchangesQuery request, CancellationToken cancellationToken = default)
    {
        try
        {
            var exchanges = await _exchangeManager.ListExchangesAsync(request.NamePattern, request.IncludeDetails, cancellationToken);
            var exchangeList = exchanges.ToList();
            return ExchangeListResult.CreateSuccess(exchangeList, exchangeList.Count);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to list exchanges");
            return ExchangeListResult.CreateFailure(ex.Message);
        }
    }
}

/// <summary>
/// Handler for GetExchangeTopologyQuery
/// </summary>
public class GetExchangeTopologyQueryHandler : IRequestHandler<GetExchangeTopologyQuery, ExchangeTopologyResult>
{
    private readonly IExchangeManager _exchangeManager;
    private readonly ILogger<GetExchangeTopologyQueryHandler> _logger;

    public GetExchangeTopologyQueryHandler(IExchangeManager exchangeManager, ILogger<GetExchangeTopologyQueryHandler> logger)
    {
        _exchangeManager = exchangeManager ?? throw new ArgumentNullException(nameof(exchangeManager));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<ExchangeTopologyResult> HandleAsync(GetExchangeTopologyQuery request, CancellationToken cancellationToken = default)
    {
        try
        {
            var topology = await _exchangeManager.GetTopologyAsync(request.Name, cancellationToken);
            return ExchangeTopologyResult.CreateSuccess(topology);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get exchange topology for {ExchangeName}", request.Name);
            return ExchangeTopologyResult.CreateFailure(ex.Message);
        }
    }
} 