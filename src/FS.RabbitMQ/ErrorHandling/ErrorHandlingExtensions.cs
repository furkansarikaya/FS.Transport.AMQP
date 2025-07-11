using System.Net.Sockets;
using System.Text.Json;
using FS.RabbitMQ.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using RabbitMQ.Client.Exceptions;

namespace FS.RabbitMQ.ErrorHandling;

/// <summary>
/// Extension methods for error handling configuration and utilities
/// </summary>
public static class ErrorHandlingExtensions
{
    /// <summary>
    /// Adds error handling services to the service collection
    /// </summary>
    /// <param name="services">Service collection</param>
    /// <returns>Service collection for fluent configuration</returns>
    public static IServiceCollection AddErrorHandling(this IServiceCollection services)
    {
        services.TryAddSingleton<IErrorHandler, ErrorHandler>();
        services.TryAddSingleton<IDeadLetterHandler, DeadLetterHandler>();
        
        return services;
    }

    /// <summary>
    /// Configures error handling settings
    /// </summary>
    /// <param name="services">Service collection</param>
    /// <param name="configureErrorHandling">Error handling configuration action</param>
    /// <returns>Service collection for fluent configuration</returns>
    public static IServiceCollection ConfigureErrorHandling(this IServiceCollection services, 
        Action<ErrorHandlingSettings> configureErrorHandling)
    {
        services.Configure<RabbitMQConfiguration>(config => configureErrorHandling(config.ErrorHandling));
        return services;
    }

    /// <summary>
    /// Sets the error handling strategy
    /// </summary>
    /// <param name="services">Service collection</param>
    /// <param name="strategy">Error handling strategy</param>
    /// <returns>Service collection for fluent configuration</returns>
    public static IServiceCollection UseErrorHandlingStrategy(this IServiceCollection services, 
        ErrorHandlingStrategy strategy)
    {
        services.Configure<RabbitMQConfiguration>(config => config.ErrorHandling.Strategy = strategy.ToString());
        return services;
    }

    /// <summary>
    /// Configures dead letter settings
    /// </summary>
    /// <param name="services">Service collection</param>
    /// <param name="exchangeName">Dead letter exchange name</param>
    /// <param name="queueName">Dead letter queue name</param>
    /// <param name="routingKey">Dead letter routing key</param>
    /// <returns>Service collection for fluent configuration</returns>
    public static IServiceCollection ConfigureDeadLetter(this IServiceCollection services,
        string exchangeName = "dlx",
        string queueName = "dlq", 
        string routingKey = "dead-letter")
    {
        services.Configure<RabbitMQConfiguration>(config =>
        {
            config.ErrorHandling.DeadLetterExchange = exchangeName;
            config.ErrorHandling.DeadLetterQueue = queueName;
            config.ErrorHandling.DeadLetterRoutingKey = routingKey;
        });
        return services;
    }

    /// <summary>
    /// Enables error logging with specified log level
    /// </summary>
    /// <param name="services">Service collection</param>
    /// <param name="logLevel">Log level for errors</param>
    /// <returns>Service collection for fluent configuration</returns>
    public static IServiceCollection EnableErrorLogging(this IServiceCollection services, string logLevel = "Error")
    {
        services.Configure<RabbitMQConfiguration>(config =>
        {
            config.ErrorHandling.LogErrors = true;
            config.ErrorHandling.ErrorLogLevel = logLevel;
        });
        return services;
    }

    /// <summary>
    /// Configures error details inclusion in dead letter messages
    /// </summary>
    /// <param name="services">Service collection</param>
    /// <param name="includeErrorDetails">Whether to include error details</param>
    /// <param name="includeOriginalHeaders">Whether to include original headers</param>
    /// <param name="maxErrorMessageLength">Maximum error message length</param>
    /// <returns>Service collection for fluent configuration</returns>
    public static IServiceCollection ConfigureErrorDetails(this IServiceCollection services,
        bool includeErrorDetails = true,
        bool includeOriginalHeaders = true,
        int maxErrorMessageLength = 1000)
    {
        services.Configure<RabbitMQConfiguration>(config =>
        {
            config.ErrorHandling.IncludeErrorDetails = includeErrorDetails;
            config.ErrorHandling.IncludeOriginalHeaders = includeOriginalHeaders;
            config.ErrorHandling.MaxErrorMessageLength = maxErrorMessageLength;
        });
        return services;
    }

    /// <summary>
    /// Executes an operation with error handling
    /// </summary>
    /// <typeparam name="T">Return type</typeparam>
    /// <param name="errorHandler">Error handler</param>
    /// <param name="operation">Operation to execute</param>
    /// <param name="operationName">Name of the operation for context</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Operation result or throws if error handling fails</returns>
    public static async Task<T> ExecuteWithErrorHandlingAsync<T>(this IErrorHandler errorHandler,
        Func<Task<T>> operation,
        string operationName,
        CancellationToken cancellationToken = default)
    {
        try
        {
            return await operation();
        }
        catch (Exception ex)
        {
            var context = ErrorContext.FromOperation(ex, operationName);
            var result = await errorHandler.HandleErrorAsync(context, cancellationToken);
            
            if (!result.IsSuccess)
            {
                throw new InvalidOperationException($"Error handling failed: {result.Message}", ex);
            }
            
            // If we reach here, error was handled but operation should be retried or failed
            throw; // Re-throw original exception for now
        }
    }

    /// <summary>
    /// Executes an operation with error handling
    /// </summary>
    /// <param name="errorHandler">Error handler</param>
    /// <param name="operation">Operation to execute</param>
    /// <param name="operationName">Name of the operation for context</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Task representing the operation</returns>
    public static async Task ExecuteWithErrorHandlingAsync(this IErrorHandler errorHandler,
        Func<Task> operation,
        string operationName,
        CancellationToken cancellationToken = default)
    {
        await errorHandler.ExecuteWithErrorHandlingAsync(async () =>
        {
            await operation();
            return true;
        }, operationName, cancellationToken);
    }

    /// <summary>
    /// Creates an error context from an exception and operation
    /// </summary>
    /// <param name="exception">Exception that occurred</param>
    /// <param name="operation">Operation name</param>
    /// <param name="correlationId">Optional correlation ID</param>
    /// <returns>Error context</returns>
    public static ErrorContext ToErrorContext(this Exception exception, string operation, string? correlationId = null)
    {
        var context = ErrorContext.FromOperation(exception, operation);
        if (!string.IsNullOrEmpty(correlationId))
        {
            context.CorrelationId = correlationId;
        }
        return context;
    }

    /// <summary>
    /// Determines if an exception indicates a transient error that might be worth retrying
    /// </summary>
    /// <param name="exception">Exception to check</param>
    /// <returns>True if the exception appears to be transient</returns>
    public static bool IsTransientError(this Exception exception)
    {
        return exception switch
        {
            TimeoutException => true,
            SocketException => true,
            HttpRequestException => true,
            BrokerUnreachableException => true,
            ConnectFailureException => true,
            AlreadyClosedException => true,
            TaskCanceledException => false, // Usually indicates cancellation
            OperationCanceledException => false,
            ArgumentException => false, // Programming errors
            InvalidOperationException => false,
            _ => false // Conservative approach for unknown exceptions
        };
    }

    /// <summary>
    /// Determines if an exception indicates a permanent error that should not be retried
    /// </summary>
    /// <param name="exception">Exception to check</param>
    /// <returns>True if the exception appears to be permanent</returns>
    public static bool IsPermanentError(this Exception exception)
    {
        return !exception.IsTransientError();
    }

    /// <summary>
    /// Gets a user-friendly error category for an exception
    /// </summary>
    /// <param name="exception">Exception to categorize</param>
    /// <returns>Error category</returns>
    public static string GetErrorCategory(this Exception exception)
    {
        return exception switch
        {
            TimeoutException => "Timeout",
            SocketException => "Network",
            HttpRequestException => "HTTP",
            BrokerUnreachableException => "Connection",
            ConnectFailureException => "Connection",
            AlreadyClosedException => "Connection",
            TaskCanceledException => "Cancellation",
            OperationCanceledException => "Cancellation",
            ArgumentException => "Validation",
            InvalidOperationException => "Logic",
            UnauthorizedAccessException => "Security",
            FormatException => "Serialization",
            JsonException => "Serialization",
            _ => "Unknown"
        };
    }
}