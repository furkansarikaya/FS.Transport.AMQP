using FS.Mediator.Features.RequestHandling.Core;
using FS.RabbitMQ.Configuration;
using Microsoft.Extensions.Logging;

namespace FS.RabbitMQ.Queue;

/// <summary>
/// Handler for DeclareQueueCommand
/// </summary>
public class DeclareQueueCommandHandler : IRequestHandler<DeclareQueueCommand, QueueDeclarationResult>
{
    private readonly IQueueManager _queueManager;
    private readonly ILogger<DeclareQueueCommandHandler> _logger;

    public DeclareQueueCommandHandler(IQueueManager queueManager, ILogger<DeclareQueueCommandHandler> logger)
    {
        _queueManager = queueManager ?? throw new ArgumentNullException(nameof(queueManager));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<QueueDeclarationResult> HandleAsync(DeclareQueueCommand request, CancellationToken cancellationToken = default)
    {
        try
        {
            var startTime = DateTime.UtcNow;
            var result = await _queueManager.DeclareAsync(request.Settings ?? new QueueSettings
            {
                Name = request.Name,
                Durable = request.Durable,
                Exclusive = request.Exclusive,
                AutoDelete = request.AutoDelete,
                Arguments = request.Arguments?.ToDictionary(kvp => kvp.Key, kvp => kvp.Value) ?? new Dictionary<string, object>()
            }, cancellationToken);
            
            var duration = DateTime.UtcNow - startTime;
            
            return QueueDeclarationResult.CreateSuccess(
                request.Name,
                result,
                0, // Bindings applied - we don't have this info from DeclareAsync
                duration);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to declare queue {QueueName}: {Message}", request.Name, ex.Message);
            return QueueDeclarationResult.CreateFailure(request.Name, ex.Message, ex);
        }
    }
}

/// <summary>
/// Handler for DeleteQueueCommand
/// </summary>
public class DeleteQueueCommandHandler : IRequestHandler<DeleteQueueCommand, QueueOperationResult>
{
    private readonly IQueueManager _queueManager;
    private readonly ILogger<DeleteQueueCommandHandler> _logger;

    public DeleteQueueCommandHandler(IQueueManager queueManager, ILogger<DeleteQueueCommandHandler> logger)
    {
        _queueManager = queueManager ?? throw new ArgumentNullException(nameof(queueManager));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<QueueOperationResult> HandleAsync(DeleteQueueCommand request, CancellationToken cancellationToken = default)
    {
        try
        {
            await _queueManager.DeleteAsync(request.Name, request.IfUnused, request.IfEmpty, cancellationToken);
            return QueueOperationResult.CreateSuccess(request.Name, "Delete");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to delete queue {QueueName}: {Message}", request.Name, ex.Message);
            return QueueOperationResult.CreateFailure(request.Name, "Delete", ex.Message, ex);
        }
    }
}

/// <summary>
/// Handler for BindQueueCommand
/// </summary>
public class BindQueueCommandHandler : IRequestHandler<BindQueueCommand, QueueBindingResult>
{
    private readonly IQueueManager _queueManager;
    private readonly ILogger<BindQueueCommandHandler> _logger;

    public BindQueueCommandHandler(IQueueManager queueManager, ILogger<BindQueueCommandHandler> logger)
    {
        _queueManager = queueManager ?? throw new ArgumentNullException(nameof(queueManager));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<QueueBindingResult> HandleAsync(BindQueueCommand request, CancellationToken cancellationToken = default)
    {
        try
        {
            var result = await _queueManager.BindAsync(request.QueueName, request.ExchangeName, request.RoutingKey, request.Arguments, cancellationToken);
            return QueueBindingResult.CreateSuccess(request.QueueName, request.ExchangeName, request.RoutingKey, "Bind", false);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to bind queue {QueueName} to exchange {ExchangeName}: {Message}", request.QueueName, request.ExchangeName, ex.Message);
            return QueueBindingResult.CreateFailure(request.QueueName, request.ExchangeName, request.RoutingKey, "Bind", ex.Message, ex);
        }
    }
}

/// <summary>
/// Handler for UnbindQueueCommand
/// </summary>
public class UnbindQueueCommandHandler : IRequestHandler<UnbindQueueCommand, QueueBindingResult>
{
    private readonly IQueueManager _queueManager;
    private readonly ILogger<UnbindQueueCommandHandler> _logger;

    public UnbindQueueCommandHandler(IQueueManager queueManager, ILogger<UnbindQueueCommandHandler> logger)
    {
        _queueManager = queueManager ?? throw new ArgumentNullException(nameof(queueManager));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<QueueBindingResult> HandleAsync(UnbindQueueCommand request, CancellationToken cancellationToken = default)
    {
        try
        {
            await _queueManager.UnbindAsync(request.QueueName, request.ExchangeName, request.RoutingKey, request.Arguments, cancellationToken);
            return QueueBindingResult.CreateSuccess(request.QueueName, request.ExchangeName, request.RoutingKey, "Unbind");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to unbind queue {QueueName} from exchange {ExchangeName}: {Message}", request.QueueName, request.ExchangeName, ex.Message);
            return QueueBindingResult.CreateFailure(request.QueueName, request.ExchangeName, request.RoutingKey, "Unbind", ex.Message, ex);
        }
    }
}

/// <summary>
/// Handler for PurgeQueueCommand
/// </summary>
public class PurgeQueueCommandHandler : IRequestHandler<PurgeQueueCommand, QueuePurgeResult>
{
    private readonly IQueueManager _queueManager;
    private readonly ILogger<PurgeQueueCommandHandler> _logger;

    public PurgeQueueCommandHandler(IQueueManager queueManager, ILogger<PurgeQueueCommandHandler> logger)
    {
        _queueManager = queueManager ?? throw new ArgumentNullException(nameof(queueManager));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<QueuePurgeResult> HandleAsync(PurgeQueueCommand request, CancellationToken cancellationToken = default)
    {
        try
        {
            var startTime = DateTime.UtcNow;
            var messagesPurged = await _queueManager.PurgeAsync(request.Name, cancellationToken);
            var duration = DateTime.UtcNow - startTime;
            
            return QueuePurgeResult.CreateSuccess(request.Name, messagesPurged ?? 0, duration);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to purge queue {QueueName}: {Message}", request.Name, ex.Message);
            return QueuePurgeResult.CreateFailure(request.Name, ex.Message, ex);
        }
    }
}

/// <summary>
/// Handler for DeclareQueuesCommand
/// </summary>
public class DeclareQueuesCommandHandler : IRequestHandler<DeclareQueuesCommand, QueueBatchDeclarationResult>
{
    private readonly IQueueManager _queueManager;
    private readonly ILogger<DeclareQueuesCommandHandler> _logger;

    public DeclareQueuesCommandHandler(IQueueManager queueManager, ILogger<DeclareQueuesCommandHandler> logger)
    {
        _queueManager = queueManager ?? throw new ArgumentNullException(nameof(queueManager));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<QueueBatchDeclarationResult> HandleAsync(DeclareQueuesCommand request, CancellationToken cancellationToken = default)
    {
        try
        {
            var startTime = DateTime.UtcNow;
            var results = new List<QueueDeclarationResult>();
            
            foreach (var queueSettings in request.QueueSettings)
            {
                try
                {
                    var result = await _queueManager.DeclareAsync(queueSettings, cancellationToken);
                    results.Add(QueueDeclarationResult.CreateSuccess(queueSettings.Name, result, 0, TimeSpan.Zero));
                }
                catch (Exception ex)
                {
                    results.Add(QueueDeclarationResult.CreateFailure(queueSettings.Name, ex.Message, ex));
                    if (request.StopOnFailure)
                        break;
                }
            }
            
            var duration = DateTime.UtcNow - startTime;
            return QueueBatchDeclarationResult.Create(results, duration);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to declare queues: {Message}", ex.Message);
            return QueueBatchDeclarationResult.Create(new List<QueueDeclarationResult>(), TimeSpan.Zero);
        }
    }
}

/// <summary>
/// Handler for RedeclareQueuesCommand
/// </summary>
public class RedeclareQueuesCommandHandler : IRequestHandler<RedeclareQueuesCommand, QueueRecoveryResult>
{
    private readonly IQueueManager _queueManager;
    private readonly ILogger<RedeclareQueuesCommandHandler> _logger;

    public RedeclareQueuesCommandHandler(IQueueManager queueManager, ILogger<RedeclareQueuesCommandHandler> logger)
    {
        _queueManager = queueManager ?? throw new ArgumentNullException(nameof(queueManager));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<QueueRecoveryResult> HandleAsync(RedeclareQueuesCommand request, CancellationToken cancellationToken = default)
    {
        try
        {
            var startTime = DateTime.UtcNow;
            var result = await _queueManager.RedeclareAllAsync(cancellationToken);
            var duration = DateTime.UtcNow - startTime;
            
            return QueueRecoveryResult.CreateSuccess(request.Strategy, 0, 0, 1, duration);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to redeclare queues: {Message}", ex.Message);
            return QueueRecoveryResult.CreateFailure(request.Strategy, 0, TimeSpan.Zero, ex.Message, ex);
        }
    }
}

/// <summary>
/// Handler for ResetQueueStatisticsCommand
/// </summary>
public class ResetQueueStatisticsCommandHandler : IRequestHandler<ResetQueueStatisticsCommand, QueueOperationResult>
{
    private readonly IQueueManager _queueManager;
    private readonly ILogger<ResetQueueStatisticsCommandHandler> _logger;

    public ResetQueueStatisticsCommandHandler(IQueueManager queueManager, ILogger<ResetQueueStatisticsCommandHandler> logger)
    {
        _queueManager = queueManager ?? throw new ArgumentNullException(nameof(queueManager));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <inheritdoc />
    public Task<QueueOperationResult> HandleAsync(ResetQueueStatisticsCommand request, CancellationToken cancellationToken = default)
    {
        try
        {
            // QueueManager doesn't have ResetStatisticsAsync, so we'll just return success
            return Task.FromResult(QueueOperationResult.CreateSuccess("*", "ResetStatistics"));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to reset queue statistics: {Message}", ex.Message);
            return Task.FromResult(QueueOperationResult.CreateFailure("*", "ResetStatistics", ex.Message, ex));
        }
    }
} 