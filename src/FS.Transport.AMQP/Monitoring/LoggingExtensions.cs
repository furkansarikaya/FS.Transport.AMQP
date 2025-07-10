using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using Microsoft.Extensions.Logging;

namespace FS.Transport.AMQP.Monitoring;

/// <summary>
/// Provides enhanced logging extensions for RabbitMQ transport components.
/// Includes structured logging, performance tracking, event correlation, and monitoring-specific log methods.
/// </summary>
public static class LoggingExtensions
{
    /// <summary>
    /// Logs a connection event with structured data.
    /// </summary>
    /// <param name="logger">The logger instance.</param>
    /// <param name="level">The log level.</param>
    /// <param name="message">The log message.</param>
    /// <param name="connectionId">The connection identifier.</param>
    /// <param name="hostName">The RabbitMQ host name.</param>
    /// <param name="port">The RabbitMQ port.</param>
    /// <param name="virtualHost">The virtual host.</param>
    /// <param name="exception">Optional exception.</param>
    public static void LogConnectionEvent(
        this ILogger logger,
        LogLevel level,
        string message,
        string? connectionId = null,
        string? hostName = null,
        int? port = null,
        string? virtualHost = null,
        Exception? exception = null)
    {
        using var scope = logger.BeginScope(new Dictionary<string, object>
        {
            ["EventType"] = "Connection",
            ["ConnectionId"] = connectionId ?? "Unknown",
            ["HostName"] = hostName ?? "Unknown",
            ["Port"] = port ?? 0,
            ["VirtualHost"] = virtualHost ?? "Unknown",
            ["Timestamp"] = DateTimeOffset.UtcNow
        });

        logger.Log(level, exception, message);
    }

    /// <summary>
    /// Logs a message processing event with structured data.
    /// </summary>
    /// <param name="logger">The logger instance.</param>
    /// <param name="level">The log level.</param>
    /// <param name="message">The log message.</param>
    /// <param name="messageId">The message identifier.</param>
    /// <param name="correlationId">The correlation identifier.</param>
    /// <param name="exchange">The exchange name.</param>
    /// <param name="routingKey">The routing key.</param>
    /// <param name="queueName">The queue name.</param>
    /// <param name="processingTimeMs">The processing time in milliseconds.</param>
    /// <param name="exception">Optional exception.</param>
    public static void LogMessageEvent(
        this ILogger logger,
        LogLevel level,
        string message,
        string? messageId = null,
        string? correlationId = null,
        string? exchange = null,
        string? routingKey = null,
        string? queueName = null,
        double? processingTimeMs = null,
        Exception? exception = null)
    {
        using var scope = logger.BeginScope(new Dictionary<string, object>
        {
            ["EventType"] = "Message",
            ["MessageId"] = messageId ?? "Unknown",
            ["CorrelationId"] = correlationId ?? "Unknown",
            ["Exchange"] = exchange ?? "Unknown",
            ["RoutingKey"] = routingKey ?? "Unknown",
            ["QueueName"] = queueName ?? "Unknown",
            ["ProcessingTimeMs"] = processingTimeMs ?? 0,
            ["Timestamp"] = DateTimeOffset.UtcNow
        });

        logger.Log(level, exception, message);
    }

    /// <summary>
    /// Logs a health check event with structured data.
    /// </summary>
    /// <param name="logger">The logger instance.</param>
    /// <param name="level">The log level.</param>
    /// <param name="message">The log message.</param>
    /// <param name="componentName">The component name.</param>
    /// <param name="healthStatus">The health status.</param>
    /// <param name="checkDurationMs">The check duration in milliseconds.</param>
    /// <param name="exception">Optional exception.</param>
    public static void LogHealthCheckEvent(
        this ILogger logger,
        LogLevel level,
        string message,
        string componentName,
        HealthStatus healthStatus,
        double checkDurationMs,
        Exception? exception = null)
    {
        using var scope = logger.BeginScope(new Dictionary<string, object>
        {
            ["EventType"] = "HealthCheck",
            ["ComponentName"] = componentName,
            ["HealthStatus"] = healthStatus.ToString(),
            ["CheckDurationMs"] = checkDurationMs,
            ["Timestamp"] = DateTimeOffset.UtcNow
        });

        logger.Log(level, exception, message);
    }

    /// <summary>
    /// Logs a performance metric event with structured data.
    /// </summary>
    /// <param name="logger">The logger instance.</param>
    /// <param name="level">The log level.</param>
    /// <param name="message">The log message.</param>
    /// <param name="metricName">The metric name.</param>
    /// <param name="metricValue">The metric value.</param>
    /// <param name="metricType">The metric type.</param>
    /// <param name="tags">Optional metric tags.</param>
    public static void LogMetricEvent(
        this ILogger logger,
        LogLevel level,
        string message,
        string metricName,
        object metricValue,
        MetricType metricType,
        Dictionary<string, string>? tags = null)
    {
        var scopeData = new Dictionary<string, object>
        {
            ["EventType"] = "Metric",
            ["MetricName"] = metricName,
            ["MetricValue"] = metricValue,
            ["MetricType"] = metricType.ToString(),
            ["Timestamp"] = DateTimeOffset.UtcNow
        };

        if (tags != null)
        {
            foreach (var tag in tags)
            {
                scopeData[$"Tag_{tag.Key}"] = tag.Value;
            }
        }

        using var scope = logger.BeginScope(scopeData);
        logger.Log(level, message);
    }

    /// <summary>
    /// Logs a queue operation event with structured data.
    /// </summary>
    /// <param name="logger">The logger instance.</param>
    /// <param name="level">The log level.</param>
    /// <param name="message">The log message.</param>
    /// <param name="operation">The queue operation.</param>
    /// <param name="queueName">The queue name.</param>
    /// <param name="messageCount">The message count.</param>
    /// <param name="consumerCount">The consumer count.</param>
    /// <param name="exception">Optional exception.</param>
    public static void LogQueueEvent(
        this ILogger logger,
        LogLevel level,
        string message,
        string operation,
        string queueName,
        uint? messageCount = null,
        uint? consumerCount = null,
        Exception? exception = null)
    {
        using var scope = logger.BeginScope(new Dictionary<string, object>
        {
            ["EventType"] = "Queue",
            ["Operation"] = operation,
            ["QueueName"] = queueName,
            ["MessageCount"] = messageCount ?? 0,
            ["ConsumerCount"] = consumerCount ?? 0,
            ["Timestamp"] = DateTimeOffset.UtcNow
        });

        logger.Log(level, exception, message);
    }

    /// <summary>
    /// Logs an exchange operation event with structured data.
    /// </summary>
    /// <param name="logger">The logger instance.</param>
    /// <param name="level">The log level.</param>
    /// <param name="message">The log message.</param>
    /// <param name="operation">The exchange operation.</param>
    /// <param name="exchangeName">The exchange name.</param>
    /// <param name="exchangeType">The exchange type.</param>
    /// <param name="isDurable">Whether the exchange is durable.</param>
    /// <param name="exception">Optional exception.</param>
    public static void LogExchangeEvent(
        this ILogger logger,
        LogLevel level,
        string message,
        string operation,
        string exchangeName,
        string? exchangeType = null,
        bool? isDurable = null,
        Exception? exception = null)
    {
        using var scope = logger.BeginScope(new Dictionary<string, object>
        {
            ["EventType"] = "Exchange",
            ["Operation"] = operation,
            ["ExchangeName"] = exchangeName,
            ["ExchangeType"] = exchangeType ?? "Unknown",
            ["IsDurable"] = isDurable ?? false,
            ["Timestamp"] = DateTimeOffset.UtcNow
        });

        logger.Log(level, exception, message);
    }

    /// <summary>
    /// Logs a retry event with structured data.
    /// </summary>
    /// <param name="logger">The logger instance.</param>
    /// <param name="level">The log level.</param>
    /// <param name="message">The log message.</param>
    /// <param name="operation">The operation being retried.</param>
    /// <param name="attemptNumber">The current attempt number.</param>
    /// <param name="maxAttempts">The maximum number of attempts.</param>
    /// <param name="delayMs">The delay before retry in milliseconds.</param>
    /// <param name="exception">Optional exception.</param>
    public static void LogRetryEvent(
        this ILogger logger,
        LogLevel level,
        string message,
        string operation,
        int attemptNumber,
        int maxAttempts,
        double delayMs,
        Exception? exception = null)
    {
        using var scope = logger.BeginScope(new Dictionary<string, object>
        {
            ["EventType"] = "Retry",
            ["Operation"] = operation,
            ["AttemptNumber"] = attemptNumber,
            ["MaxAttempts"] = maxAttempts,
            ["DelayMs"] = delayMs,
            ["Timestamp"] = DateTimeOffset.UtcNow
        });

        logger.Log(level, exception, message);
    }

    /// <summary>
    /// Logs a saga event with structured data.
    /// </summary>
    /// <param name="logger">The logger instance.</param>
    /// <param name="level">The log level.</param>
    /// <param name="message">The log message.</param>
    /// <param name="sagaId">The saga identifier.</param>
    /// <param name="sagaType">The saga type.</param>
    /// <param name="sagaState">The saga state.</param>
    /// <param name="stepName">The current step name.</param>
    /// <param name="correlationId">The correlation identifier.</param>
    /// <param name="exception">Optional exception.</param>
    public static void LogSagaEvent(
        this ILogger logger,
        LogLevel level,
        string message,
        string sagaId,
        string sagaType,
        string sagaState,
        string? stepName = null,
        string? correlationId = null,
        Exception? exception = null)
    {
        using var scope = logger.BeginScope(new Dictionary<string, object>
        {
            ["EventType"] = "Saga",
            ["SagaId"] = sagaId,
            ["SagaType"] = sagaType,
            ["SagaState"] = sagaState,
            ["StepName"] = stepName ?? "Unknown",
            ["CorrelationId"] = correlationId ?? "Unknown",
            ["Timestamp"] = DateTimeOffset.UtcNow
        });

        logger.Log(level, exception, message);
    }

    /// <summary>
    /// Creates a performance timing scope that automatically logs elapsed time.
    /// </summary>
    /// <param name="logger">The logger instance.</param>
    /// <param name="operation">The operation being timed.</param>
    /// <param name="level">The log level for the timing result.</param>
    /// <param name="memberName">The calling member name (auto-populated).</param>
    /// <param name="filePath">The calling file path (auto-populated).</param>
    /// <param name="lineNumber">The calling line number (auto-populated).</param>
    /// <returns>A disposable timing scope.</returns>
    public static IDisposable BeginTimedScope(
        this ILogger logger,
        string operation,
        LogLevel level = LogLevel.Debug,
        [CallerMemberName] string memberName = "",
        [CallerFilePath] string filePath = "",
        [CallerLineNumber] int lineNumber = 0)
    {
        return new TimedLoggingScope(logger, operation, level, memberName, filePath, lineNumber);
    }

    /// <summary>
    /// Logs an error with enhanced context information.
    /// </summary>
    /// <param name="logger">The logger instance.</param>
    /// <param name="exception">The exception to log.</param>
    /// <param name="message">The log message.</param>
    /// <param name="context">Additional context information.</param>
    /// <param name="memberName">The calling member name (auto-populated).</param>
    /// <param name="filePath">The calling file path (auto-populated).</param>
    /// <param name="lineNumber">The calling line number (auto-populated).</param>
    public static void LogErrorWithContext(
        this ILogger logger,
        Exception exception,
        string message,
        Dictionary<string, object>? context = null,
        [CallerMemberName] string memberName = "",
        [CallerFilePath] string filePath = "",
        [CallerLineNumber] int lineNumber = 0)
    {
        var scopeData = new Dictionary<string, object>
        {
            ["EventType"] = "Error",
            ["CallerMember"] = memberName,
            ["CallerFile"] = System.IO.Path.GetFileName(filePath),
            ["CallerLine"] = lineNumber,
            ["ExceptionType"] = exception.GetType().Name,
            ["Timestamp"] = DateTimeOffset.UtcNow
        };

        if (context != null)
        {
            foreach (var item in context)
            {
                scopeData[$"Context_{item.Key}"] = item.Value;
            }
        }

        using var scope = logger.BeginScope(scopeData);
        logger.LogError(exception, message);
    }

    /// <summary>
    /// Logs a warning with enhanced context information.
    /// </summary>
    /// <param name="logger">The logger instance.</param>
    /// <param name="message">The log message.</param>
    /// <param name="context">Additional context information.</param>
    /// <param name="memberName">The calling member name (auto-populated).</param>
    /// <param name="filePath">The calling file path (auto-populated).</param>
    /// <param name="lineNumber">The calling line number (auto-populated).</param>
    public static void LogWarningWithContext(
        this ILogger logger,
        string message,
        Dictionary<string, object>? context = null,
        [CallerMemberName] string memberName = "",
        [CallerFilePath] string filePath = "",
        [CallerLineNumber] int lineNumber = 0)
    {
        var scopeData = new Dictionary<string, object>
        {
            ["EventType"] = "Warning",
            ["CallerMember"] = memberName,
            ["CallerFile"] = System.IO.Path.GetFileName(filePath),
            ["CallerLine"] = lineNumber,
            ["Timestamp"] = DateTimeOffset.UtcNow
        };

        if (context != null)
        {
            foreach (var item in context)
            {
                scopeData[$"Context_{item.Key}"] = item.Value;
            }
        }

        using var scope = logger.BeginScope(scopeData);
        logger.LogWarning(message);
    }

    /// <summary>
    /// Logs information with enhanced context information.
    /// </summary>
    /// <param name="logger">The logger instance.</param>
    /// <param name="message">The log message.</param>
    /// <param name="context">Additional context information.</param>
    /// <param name="memberName">The calling member name (auto-populated).</param>
    /// <param name="filePath">The calling file path (auto-populated).</param>
    /// <param name="lineNumber">The calling line number (auto-populated).</param>
    public static void LogInformationWithContext(
        this ILogger logger,
        string message,
        Dictionary<string, object>? context = null,
        [CallerMemberName] string memberName = "",
        [CallerFilePath] string filePath = "",
        [CallerLineNumber] int lineNumber = 0)
    {
        var scopeData = new Dictionary<string, object>
        {
            ["EventType"] = "Information",
            ["CallerMember"] = memberName,
            ["CallerFile"] = System.IO.Path.GetFileName(filePath),
            ["CallerLine"] = lineNumber,
            ["Timestamp"] = DateTimeOffset.UtcNow
        };

        if (context != null)
        {
            foreach (var item in context)
            {
                scopeData[$"Context_{item.Key}"] = item.Value;
            }
        }

        using var scope = logger.BeginScope(scopeData);
        logger.LogInformation(message);
    }

    /// <summary>
    /// Logs a debug message with enhanced context information.
    /// </summary>
    /// <param name="logger">The logger instance.</param>
    /// <param name="message">The log message.</param>
    /// <param name="context">Additional context information.</param>
    /// <param name="memberName">The calling member name (auto-populated).</param>
    /// <param name="filePath">The calling file path (auto-populated).</param>
    /// <param name="lineNumber">The calling line number (auto-populated).</param>
    public static void LogDebugWithContext(
        this ILogger logger,
        string message,
        Dictionary<string, object>? context = null,
        [CallerMemberName] string memberName = "",
        [CallerFilePath] string filePath = "",
        [CallerLineNumber] int lineNumber = 0)
    {
        if (!logger.IsEnabled(LogLevel.Debug))
            return;

        var scopeData = new Dictionary<string, object>
        {
            ["EventType"] = "Debug",
            ["CallerMember"] = memberName,
            ["CallerFile"] = System.IO.Path.GetFileName(filePath),
            ["CallerLine"] = lineNumber,
            ["Timestamp"] = DateTimeOffset.UtcNow
        };

        if (context != null)
        {
            foreach (var item in context)
            {
                scopeData[$"Context_{item.Key}"] = item.Value;
            }
        }

        using var scope = logger.BeginScope(scopeData);
        logger.LogDebug(message);
    }

    /// <summary>
    /// Logs a trace message with enhanced context information.
    /// </summary>
    /// <param name="logger">The logger instance.</param>
    /// <param name="message">The log message.</param>
    /// <param name="context">Additional context information.</param>
    /// <param name="memberName">The calling member name (auto-populated).</param>
    /// <param name="filePath">The calling file path (auto-populated).</param>
    /// <param name="lineNumber">The calling line number (auto-populated).</param>
    public static void LogTraceWithContext(
        this ILogger logger,
        string message,
        Dictionary<string, object>? context = null,
        [CallerMemberName] string memberName = "",
        [CallerFilePath] string filePath = "",
        [CallerLineNumber] int lineNumber = 0)
    {
        if (!logger.IsEnabled(LogLevel.Trace))
            return;

        var scopeData = new Dictionary<string, object>
        {
            ["EventType"] = "Trace",
            ["CallerMember"] = memberName,
            ["CallerFile"] = System.IO.Path.GetFileName(filePath),
            ["CallerLine"] = lineNumber,
            ["Timestamp"] = DateTimeOffset.UtcNow
        };

        if (context != null)
        {
            foreach (var item in context)
            {
                scopeData[$"Context_{item.Key}"] = item.Value;
            }
        }

        using var scope = logger.BeginScope(scopeData);
        logger.LogTrace(message);
    }

    /// <summary>
    /// Creates a correlation scope for request tracing.
    /// </summary>
    /// <param name="logger">The logger instance.</param>
    /// <param name="correlationId">The correlation identifier.</param>
    /// <param name="operation">The operation being performed.</param>
    /// <param name="additionalContext">Additional context information.</param>
    /// <returns>A disposable correlation scope.</returns>
    public static IDisposable BeginCorrelationScope(
        this ILogger logger,
        string correlationId,
        string operation,
        Dictionary<string, object>? additionalContext = null)
    {
        var scopeData = new Dictionary<string, object>
        {
            ["CorrelationId"] = correlationId,
            ["Operation"] = operation,
            ["StartTime"] = DateTimeOffset.UtcNow
        };

        if (additionalContext != null)
        {
            foreach (var item in additionalContext)
            {
                scopeData[item.Key] = item.Value;
            }
        }

        return logger.BeginScope(scopeData);
    }
}

/// <summary>
/// Implements a timed logging scope that automatically logs elapsed time when disposed.
/// </summary>
internal class TimedLoggingScope : IDisposable
{
    private readonly ILogger _logger;
    private readonly string _operation;
    private readonly LogLevel _level;
    private readonly string _memberName;
    private readonly string _filePath;
    private readonly int _lineNumber;
    private readonly Stopwatch _stopwatch;
    private readonly IDisposable? _scope;

    /// <summary>
    /// Initializes a new instance of the <see cref="TimedLoggingScope"/> class.
    /// </summary>
    /// <param name="logger">The logger instance.</param>
    /// <param name="operation">The operation being timed.</param>
    /// <param name="level">The log level for the timing result.</param>
    /// <param name="memberName">The calling member name.</param>
    /// <param name="filePath">The calling file path.</param>
    /// <param name="lineNumber">The calling line number.</param>
    public TimedLoggingScope(
        ILogger logger,
        string operation,
        LogLevel level,
        string memberName,
        string filePath,
        int lineNumber)
    {
        _logger = logger;
        _operation = operation;
        _level = level;
        _memberName = memberName;
        _filePath = filePath;
        _lineNumber = lineNumber;
        _stopwatch = Stopwatch.StartNew();

        _scope = _logger.BeginScope(new Dictionary<string, object>
        {
            ["EventType"] = "TimedOperation",
            ["Operation"] = operation,
            ["CallerMember"] = memberName,
            ["CallerFile"] = System.IO.Path.GetFileName(filePath),
            ["CallerLine"] = lineNumber,
            ["StartTime"] = DateTimeOffset.UtcNow
        });

        if (_logger.IsEnabled(_level))
        {
            _logger.Log(_level, "Started operation: {Operation}", operation);
        }
    }

    /// <summary>
    /// Disposes the timed scope and logs the elapsed time.
    /// </summary>
    public void Dispose()
    {
        _stopwatch.Stop();
        
        if (_logger.IsEnabled(_level))
        {
            _logger.Log(_level, "Completed operation: {Operation} in {ElapsedMs}ms", 
                _operation, _stopwatch.ElapsedMilliseconds);
        }

        _scope?.Dispose();
    }
}