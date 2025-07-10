using FS.Mediator.Features.RequestHandling.Core;
using FS.Transport.AMQP.Queue;
using FS.Transport.AMQP.Exchange;
using Microsoft.Extensions.DependencyInjection;

namespace FS.Transport.AMQP.DependencyInjection;

public static class RabbitMQServiceCollectionExtensions
{
    /// <summary>
    /// Adds RabbitMQ CQRS handlers to the service collection
    /// </summary>
    /// <param name="services">Service collection</param>
    /// <returns>Service collection for chaining</returns>
    public static IServiceCollection AddRabbitMQCQRS(this IServiceCollection services)
    {
        // Register Queue Command Handlers
        services.AddScoped<IRequestHandler<DeclareQueueCommand, QueueDeclarationResult>, DeclareQueueCommandHandler>();
        services.AddScoped<IRequestHandler<DeleteQueueCommand, QueueOperationResult>, DeleteQueueCommandHandler>();
        services.AddScoped<IRequestHandler<BindQueueCommand, QueueBindingResult>, BindQueueCommandHandler>();
        services.AddScoped<IRequestHandler<UnbindQueueCommand, QueueBindingResult>, UnbindQueueCommandHandler>();
        services.AddScoped<IRequestHandler<PurgeQueueCommand, QueuePurgeResult>, PurgeQueueCommandHandler>();
        services.AddScoped<IRequestHandler<DeclareQueuesCommand, QueueBatchDeclarationResult>, DeclareQueuesCommandHandler>();
        services.AddScoped<IRequestHandler<RedeclareQueuesCommand, QueueRecoveryResult>, RedeclareQueuesCommandHandler>();
        services.AddScoped<IRequestHandler<ResetQueueStatisticsCommand, QueueOperationResult>, ResetQueueStatisticsCommandHandler>();
        
        // Register Queue Query Handlers
        services.AddScoped<IRequestHandler<GetQueueInfoQuery, QueueInfoResult>, GetQueueInfoQueryHandler>();
        services.AddScoped<IRequestHandler<GetQueueStatisticsQuery, QueueStatisticsResult>, GetQueueStatisticsQueryHandler>();
        services.AddScoped<IRequestHandler<GetQueueMessageCountQuery, QueueMessageCountResult>, GetQueueMessageCountQueryHandler>();
        services.AddScoped<IRequestHandler<GetQueueConsumerCountQuery, QueueConsumerCountResult>, GetQueueConsumerCountQueryHandler>();
        services.AddScoped<IRequestHandler<QueueExistsQuery, QueueExistsResult>, QueueExistsQueryHandler>();
        services.AddScoped<IRequestHandler<GetQueueBindingsQuery, QueueBindingsResult>, GetQueueBindingsQueryHandler>();
        services.AddScoped<IRequestHandler<GetQueueHealthQuery, QueueHealthResult>, GetQueueHealthQueryHandler>();
        services.AddScoped<IRequestHandler<GetQueuePerformanceQuery, QueuePerformanceResult>, GetQueuePerformanceQueryHandler>();
        services.AddScoped<IRequestHandler<ListQueuesQuery, QueueListResult>, ListQueuesQueryHandler>();
        
        // Register Exchange Command Handlers
        services.AddScoped<IRequestHandler<DeclareExchangeCommand, ExchangeDeclarationResult>, DeclareExchangeCommandHandler>();
        services.AddScoped<IRequestHandler<DeleteExchangeCommand, ExchangeOperationResult>, DeleteExchangeCommandHandler>();
        services.AddScoped<IRequestHandler<BindExchangeCommand, ExchangeBindingResult>, BindExchangeCommandHandler>();
        services.AddScoped<IRequestHandler<UnbindExchangeCommand, ExchangeBindingResult>, UnbindExchangeCommandHandler>();
        services.AddScoped<IRequestHandler<DeclareExchangesCommand, ExchangeBatchDeclarationResult>, DeclareExchangesCommandHandler>();
        services.AddScoped<IRequestHandler<RedeclareExchangesCommand, ExchangeRecoveryResult>, RedeclareExchangesCommandHandler>();
        services.AddScoped<IRequestHandler<ResetExchangeStatisticsCommand, ExchangeOperationResult>, ResetExchangeStatisticsCommandHandler>();
        
        // Register Exchange Query Handlers
        services.AddScoped<IRequestHandler<GetExchangeInfoQuery, ExchangeInfoResult>, GetExchangeInfoQueryHandler>();
        services.AddScoped<IRequestHandler<GetExchangeStatisticsQuery, ExchangeStatisticsResult>, GetExchangeStatisticsQueryHandler>();
        services.AddScoped<IRequestHandler<ExchangeExistsQuery, ExchangeExistsResult>, ExchangeExistsQueryHandler>();
        services.AddScoped<IRequestHandler<GetExchangeBindingsQuery, ExchangeBindingsResult>, GetExchangeBindingsQueryHandler>();
        services.AddScoped<IRequestHandler<GetExchangeHealthQuery, ExchangeHealthResult>, GetExchangeHealthQueryHandler>();
        services.AddScoped<IRequestHandler<GetExchangePerformanceQuery, ExchangePerformanceResult>, GetExchangePerformanceQueryHandler>();
        services.AddScoped<IRequestHandler<ListExchangesQuery, ExchangeListResult>, ListExchangesQueryHandler>();
        services.AddScoped<IRequestHandler<GetExchangeTopologyQuery, ExchangeTopologyResult>, GetExchangeTopologyQueryHandler>();
        
        return services;
    }
    
    /// <summary>
    /// Adds all RabbitMQ services including CQRS handlers
    /// </summary>
    /// <param name="services">Service collection</param>
    /// <returns>Service collection for chaining</returns>
    public static IServiceCollection AddRabbitMQ(this IServiceCollection services)
    {
        // Register core services (assuming these are registered elsewhere)
        // services.AddScoped<IQueueManager, QueueManager>();
        // services.AddScoped<IExchangeManager, ExchangeManager>();
        // services.AddScoped<IConnectionManager, ConnectionManager>();
        
        // Register CQRS handlers
        services.AddRabbitMQCQRS();
        
        return services;
    }
}