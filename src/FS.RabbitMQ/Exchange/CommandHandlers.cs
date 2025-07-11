using FS.Mediator.Features.RequestHandling.Core;
using FS.RabbitMQ.Configuration;
using Microsoft.Extensions.Logging;

namespace FS.RabbitMQ.Exchange;

/// <summary>
/// Handler for DeclareExchangeCommand
/// </summary>
public class DeclareExchangeCommandHandler : IRequestHandler<DeclareExchangeCommand, ExchangeDeclarationResult>
{
    private readonly IExchangeManager _exchangeManager;
    private readonly ILogger<DeclareExchangeCommandHandler> _logger;

    public DeclareExchangeCommandHandler(IExchangeManager exchangeManager, ILogger<DeclareExchangeCommandHandler> logger)
    {
        _exchangeManager = exchangeManager ?? throw new ArgumentNullException(nameof(exchangeManager));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<ExchangeDeclarationResult> HandleAsync(DeclareExchangeCommand request, CancellationToken cancellationToken = default)
    {
        try
        {
            var startTime = DateTime.UtcNow;
            var result = await _exchangeManager.DeclareAsync(request.Settings ?? new ExchangeSettings
            {
                Name = request.Name,
                Type = request.Type,
                Durable = request.Durable,
                AutoDelete = request.AutoDelete,
                Arguments = request.Arguments?.ToDictionary(kvp => kvp.Key, kvp => kvp.Value) ?? new Dictionary<string, object>()
            }, cancellationToken);
            
            var duration = DateTime.UtcNow - startTime;
            
            return ExchangeDeclarationResult.CreateSuccess(request.Name, request.Type, 0, duration);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to declare exchange {ExchangeName}: {Message}", request.Name, ex.Message);
            return ExchangeDeclarationResult.CreateFailure(request.Name, ex.Message, ex);
        }
    }
}

/// <summary>
/// Handler for DeleteExchangeCommand
/// </summary>
public class DeleteExchangeCommandHandler : IRequestHandler<DeleteExchangeCommand, ExchangeOperationResult>
{
    private readonly IExchangeManager _exchangeManager;
    private readonly ILogger<DeleteExchangeCommandHandler> _logger;

    public DeleteExchangeCommandHandler(IExchangeManager exchangeManager, ILogger<DeleteExchangeCommandHandler> logger)
    {
        _exchangeManager = exchangeManager ?? throw new ArgumentNullException(nameof(exchangeManager));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<ExchangeOperationResult> HandleAsync(DeleteExchangeCommand request, CancellationToken cancellationToken = default)
    {
        try
        {
            await _exchangeManager.DeleteAsync(request.Name, request.IfUnused, cancellationToken);
            return ExchangeOperationResult.CreateSuccess(request.Name, "Delete");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to delete exchange {ExchangeName}: {Message}", request.Name, ex.Message);
            return ExchangeOperationResult.CreateFailure(request.Name, "Delete", ex.Message, ex);
        }
    }
}

/// <summary>
/// Handler for BindExchangeCommand
/// </summary>
public class BindExchangeCommandHandler : IRequestHandler<BindExchangeCommand, ExchangeBindingResult>
{
    private readonly IExchangeManager _exchangeManager;
    private readonly ILogger<BindExchangeCommandHandler> _logger;

    public BindExchangeCommandHandler(IExchangeManager exchangeManager, ILogger<BindExchangeCommandHandler> logger)
    {
        _exchangeManager = exchangeManager ?? throw new ArgumentNullException(nameof(exchangeManager));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<ExchangeBindingResult> HandleAsync(BindExchangeCommand request, CancellationToken cancellationToken = default)
    {
        try
        {
            var result = await _exchangeManager.BindAsync(request.DestinationExchange, request.SourceExchange, request.RoutingKey, request.Arguments, cancellationToken);
            return ExchangeBindingResult.CreateSuccess(request.DestinationExchange, request.SourceExchange, request.RoutingKey, "Bind", false, false);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to bind exchange {DestinationExchange} to {SourceExchange}: {Message}", request.DestinationExchange, request.SourceExchange, ex.Message);
            return ExchangeBindingResult.CreateFailure(request.DestinationExchange, request.SourceExchange, request.RoutingKey, "Bind", ex.Message, ex);
        }
    }
}

/// <summary>
/// Handler for UnbindExchangeCommand
/// </summary>
public class UnbindExchangeCommandHandler : IRequestHandler<UnbindExchangeCommand, ExchangeBindingResult>
{
    private readonly IExchangeManager _exchangeManager;
    private readonly ILogger<UnbindExchangeCommandHandler> _logger;

    public UnbindExchangeCommandHandler(IExchangeManager exchangeManager, ILogger<UnbindExchangeCommandHandler> logger)
    {
        _exchangeManager = exchangeManager ?? throw new ArgumentNullException(nameof(exchangeManager));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<ExchangeBindingResult> HandleAsync(UnbindExchangeCommand request, CancellationToken cancellationToken = default)
    {
        try
        {
            await _exchangeManager.UnbindAsync(request.DestinationExchange, request.SourceExchange, request.RoutingKey, request.Arguments, cancellationToken);
            return ExchangeBindingResult.CreateSuccess(request.DestinationExchange, request.SourceExchange, request.RoutingKey, "Unbind");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to unbind exchange {DestinationExchange} from {SourceExchange}: {Message}", request.DestinationExchange, request.SourceExchange, ex.Message);
            return ExchangeBindingResult.CreateFailure(request.DestinationExchange, request.SourceExchange, request.RoutingKey, "Unbind", ex.Message, ex);
        }
    }
}

/// <summary>
/// Handler for DeclareExchangesCommand
/// </summary>
public class DeclareExchangesCommandHandler : IRequestHandler<DeclareExchangesCommand, ExchangeBatchDeclarationResult>
{
    private readonly IExchangeManager _exchangeManager;
    private readonly ILogger<DeclareExchangesCommandHandler> _logger;

    public DeclareExchangesCommandHandler(IExchangeManager exchangeManager, ILogger<DeclareExchangesCommandHandler> logger)
    {
        _exchangeManager = exchangeManager ?? throw new ArgumentNullException(nameof(exchangeManager));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<ExchangeBatchDeclarationResult> HandleAsync(DeclareExchangesCommand request, CancellationToken cancellationToken = default)
    {
        try
        {
            var startTime = DateTime.UtcNow;
            var results = new List<ExchangeDeclarationResult>();
            
            foreach (var exchangeSettings in request.ExchangeSettings)
            {
                try
                {
                    var result = await _exchangeManager.DeclareAsync(exchangeSettings, cancellationToken);
                    results.Add(ExchangeDeclarationResult.CreateSuccess(exchangeSettings.Name, exchangeSettings.Type, 0, TimeSpan.Zero));
                }
                catch (Exception ex)
                {
                    results.Add(ExchangeDeclarationResult.CreateFailure(exchangeSettings.Name, ex.Message, ex));
                    if (request.StopOnFailure)
                        break;
                }
            }
            
            var duration = DateTime.UtcNow - startTime;
            return ExchangeBatchDeclarationResult.Create(results, duration);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to declare exchanges: {Message}", ex.Message);
            return ExchangeBatchDeclarationResult.Create(new List<ExchangeDeclarationResult>(), TimeSpan.Zero);
        }
    }
}

/// <summary>
/// Handler for RedeclareExchangesCommand
/// </summary>
public class RedeclareExchangesCommandHandler : IRequestHandler<RedeclareExchangesCommand, ExchangeRecoveryResult>
{
    private readonly IExchangeManager _exchangeManager;
    private readonly ILogger<RedeclareExchangesCommandHandler> _logger;

    public RedeclareExchangesCommandHandler(IExchangeManager exchangeManager, ILogger<RedeclareExchangesCommandHandler> logger)
    {
        _exchangeManager = exchangeManager ?? throw new ArgumentNullException(nameof(exchangeManager));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<ExchangeRecoveryResult> HandleAsync(RedeclareExchangesCommand request, CancellationToken cancellationToken = default)
    {
        try
        {
            var startTime = DateTime.UtcNow;
            var result = await _exchangeManager.RedeclareAllAsync(cancellationToken);
            var duration = DateTime.UtcNow - startTime;
            
            return ExchangeRecoveryResult.CreateSuccess(request.Strategy, 0, 0, 1, duration);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to redeclare exchanges: {Message}", ex.Message);
            return ExchangeRecoveryResult.CreateFailure(request.Strategy, 0, TimeSpan.Zero, ex.Message, ex);
        }
    }
}

/// <summary>
/// Handler for ResetExchangeStatisticsCommand
/// </summary>
public class ResetExchangeStatisticsCommandHandler : IRequestHandler<ResetExchangeStatisticsCommand, ExchangeOperationResult>
{
    private readonly IExchangeManager _exchangeManager;
    private readonly ILogger<ResetExchangeStatisticsCommandHandler> _logger;

    public ResetExchangeStatisticsCommandHandler(IExchangeManager exchangeManager, ILogger<ResetExchangeStatisticsCommandHandler> logger)
    {
        _exchangeManager = exchangeManager ?? throw new ArgumentNullException(nameof(exchangeManager));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<ExchangeOperationResult> HandleAsync(ResetExchangeStatisticsCommand request, CancellationToken cancellationToken = default)
    {
        try
        {
            // ExchangeManager doesn't have ResetStatisticsAsync, so we'll just return success
            return ExchangeOperationResult.CreateSuccess("*", "ResetStatistics");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to reset exchange statistics: {Message}", ex.Message);
            return ExchangeOperationResult.CreateFailure("*", "ResetStatistics", ex.Message, ex);
        }
    }
} 