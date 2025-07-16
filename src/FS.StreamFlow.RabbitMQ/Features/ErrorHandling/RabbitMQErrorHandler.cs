using FS.StreamFlow.Core.Features.Messaging.Interfaces;
using FS.StreamFlow.Core.Features.Messaging.Models;
using FS.StreamFlow.RabbitMQ.Features.Connection;
using Microsoft.Extensions.Logging;
using RabbitMQ.Client;
using System.Collections.Concurrent;
using System.Text.Json;

namespace FS.StreamFlow.RabbitMQ.Features.ErrorHandling;

/// <summary>
/// RabbitMQ implementation of error handler providing comprehensive error management, dead letter queue support, and retry policies
/// </summary>
public class RabbitMQErrorHandler : IErrorHandler
{
    private readonly IConnectionManager _connectionManager;
    private readonly IRetryPolicyFactory _retryPolicyFactory;
    private readonly ILogger<RabbitMQErrorHandler> _logger;
    private readonly ConcurrentDictionary<Type, IRetryPolicy> _retryPolicies;
    private readonly object _lock = new();
    private volatile bool _disposed;

    /// <summary>
    /// Gets the error handler name
    /// </summary>
    public string Name => "RabbitMQ Error Handler";

    /// <summary>
    /// Gets the error handling settings
    /// </summary>
    public ErrorHandlingSettings Settings { get; }

    /// <summary>
    /// Event raised when an error is handled
    /// </summary>
    public event EventHandler<ErrorHandledEventArgs>? ErrorHandled;

    /// <summary>
    /// Event raised when a message is sent to dead letter queue
    /// </summary>
    public event EventHandler<DeadLetterEventArgs>? MessageSentToDeadLetterQueue;

    /// <summary>
    /// Initializes a new instance of the RabbitMQErrorHandler class
    /// </summary>
    /// <param name="connectionManager">Connection manager</param>
    /// <param name="retryPolicyFactory">Retry policy factory</param>
    /// <param name="logger">Logger instance</param>
    /// <param name="settings">Error handling settings</param>
    public RabbitMQErrorHandler(
        IConnectionManager connectionManager,
        IRetryPolicyFactory retryPolicyFactory,
        ILogger<RabbitMQErrorHandler> logger,
        ErrorHandlingSettings? settings = null)
    {
        _connectionManager = connectionManager ?? throw new ArgumentNullException(nameof(connectionManager));
        _retryPolicyFactory = retryPolicyFactory ?? throw new ArgumentNullException(nameof(retryPolicyFactory));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        Settings = settings ?? new ErrorHandlingSettings();
        _retryPolicies = new ConcurrentDictionary<Type, IRetryPolicy>();
    }

    /// <summary>
    /// Handles an error that occurred during message processing
    /// </summary>
    /// <param name="exception">Exception that occurred</param>
    /// <param name="context">Message context</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Task with error handling result</returns>
    public async Task<ErrorHandlingResult> HandleErrorAsync(Exception exception, MessageContext context, CancellationToken cancellationToken = default)
    {
        return await HandleErrorAsync(exception, context, 1, cancellationToken);
    }

    /// <summary>
    /// Handles an error that occurred during message processing with retry information
    /// </summary>
    /// <param name="exception">Exception that occurred</param>
    /// <param name="context">Message context</param>
    /// <param name="attemptNumber">Current attempt number</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Task with error handling result</returns>
    public async Task<ErrorHandlingResult> HandleErrorAsync(Exception exception, MessageContext context, int attemptNumber, CancellationToken cancellationToken = default)
    {
        if (_disposed)
            throw new ObjectDisposedException(nameof(RabbitMQErrorHandler));

        try
        {
            _logger.LogError(exception, "Handling error for message {MessageId} on attempt {AttemptNumber}", 
                context.MessageId, attemptNumber);

            var result = await DetermineErrorHandlingAction(exception, context, attemptNumber, cancellationToken);

            // Raise error handled event
            var eventArgs = new ErrorHandledEventArgs(exception, context, result, Name);
            ErrorHandled?.Invoke(this, eventArgs);

            return result;
        }
        catch (Exception handlerException)
        {
            _logger.LogError(handlerException, "Error occurred while handling error for message {MessageId}", context.MessageId);
            return ErrorHandlingResult.Failed($"Error handler failed: {handlerException.Message}");
        }
    }

    /// <summary>
    /// Determines if the error can be handled by this handler
    /// </summary>
    /// <param name="exception">Exception to check</param>
    /// <returns>True if the error can be handled, otherwise false</returns>
    public bool CanHandle(Exception exception)
    {
        if (exception == null)
            return false;

        // Handle all RabbitMQ-related exceptions
        if (exception is RabbitMQClientException ||
            exception is AlreadyClosedException ||
            exception is BrokerUnreachableException ||
            exception is ConnectFailureException ||
            exception is OperationInterruptedException ||
            exception is TimeoutException)
        {
            return true;
        }

        // Handle general exceptions based on settings
        return Settings.Strategy != ErrorHandlingStrategy.Custom;
    }

    /// <summary>
    /// Gets the retry policy for a specific error
    /// </summary>
    /// <param name="exception">Exception</param>
    /// <returns>Retry policy or null if no retry should be performed</returns>
    public IRetryPolicy? GetRetryPolicy(Exception exception)
    {
        if (exception == null)
            return null;

        var exceptionType = exception.GetType();
        
        if (_retryPolicies.TryGetValue(exceptionType, out var policy))
        {
            return policy;
        }

        // Create appropriate retry policy based on exception type
        policy = exception switch
        {
            BrokerUnreachableException => _retryPolicyFactory.CreateExponentialBackoffPolicy(maxRetryAttempts: 5),
            ConnectFailureException => _retryPolicyFactory.CreateExponentialBackoffPolicy(maxRetryAttempts: 3),
            TimeoutException => _retryPolicyFactory.CreateLinearPolicy(maxRetryAttempts: 3),
            _ => _retryPolicyFactory.CreateRetryPolicy(new RetryPolicySettings
            {
                MaxRetryAttempts = Settings.MaxRetries,
                InitialRetryDelay = Settings.RetryDelay,
                UseExponentialBackoff = Settings.UseExponentialBackoff
            })
        };

        _retryPolicies.TryAdd(exceptionType, policy);
        return policy;
    }

    /// <summary>
    /// Determines if the message should be sent to dead letter queue
    /// </summary>
    /// <param name="exception">Exception that occurred</param>
    /// <param name="context">Message context</param>
    /// <param name="attemptNumber">Current attempt number</param>
    /// <returns>True if message should be sent to dead letter queue, otherwise false</returns>
    public bool ShouldSendToDeadLetterQueue(Exception exception, MessageContext context, int attemptNumber)
    {
        if (exception == null || context == null)
            return false;

        // Check if maximum retries exceeded
        if (attemptNumber >= Settings.MaxRetries)
        {
            _logger.LogInformation("Maximum retries exceeded for message {MessageId}, sending to dead letter queue", context.MessageId);
            return true;
        }

        // Check for specific exceptions that should go straight to dead letter queue
        if (exception is ArgumentException ||
            exception is InvalidOperationException ||
            exception is NotSupportedException ||
            exception is SerializationException)
        {
            _logger.LogInformation("Non-retryable exception for message {MessageId}, sending to dead letter queue", context.MessageId);
            return true;
        }

        return false;
    }

    /// <summary>
    /// Determines the appropriate error handling action
    /// </summary>
    /// <param name="exception">Exception that occurred</param>
    /// <param name="context">Message context</param>
    /// <param name="attemptNumber">Current attempt number</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Error handling result</returns>
    private async Task<ErrorHandlingResult> DetermineErrorHandlingAction(Exception exception, MessageContext context, int attemptNumber, CancellationToken cancellationToken)
    {
        switch (Settings.Strategy)
        {
            case ErrorHandlingStrategy.Requeue:
                return await HandleRequeueStrategy(exception, context, attemptNumber, cancellationToken);

            case ErrorHandlingStrategy.Reject:
                return await HandleRejectStrategy(exception, context, attemptNumber, cancellationToken);

            case ErrorHandlingStrategy.Acknowledge:
                return await HandleAcknowledgeStrategy(exception, context, attemptNumber, cancellationToken);

            case ErrorHandlingStrategy.Custom:
                return await HandleCustomStrategy(exception, context, attemptNumber, cancellationToken);

            default:
                return await HandleRequeueStrategy(exception, context, attemptNumber, cancellationToken);
        }
    }

    /// <summary>
    /// Handles requeue strategy
    /// </summary>
    /// <param name="exception">Exception that occurred</param>
    /// <param name="context">Message context</param>
    /// <param name="attemptNumber">Current attempt number</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Error handling result</returns>
    private async Task<ErrorHandlingResult> HandleRequeueStrategy(Exception exception, MessageContext context, int attemptNumber, CancellationToken cancellationToken)
    {
        if (ShouldSendToDeadLetterQueue(exception, context, attemptNumber))
        {
            await SendToDeadLetterQueue(exception, context, attemptNumber, cancellationToken);
            return ErrorHandlingResult.DeadLetter($"Message sent to dead letter queue after {attemptNumber} attempts");
        }

        var retryPolicy = GetRetryPolicy(exception);
        if (retryPolicy != null && retryPolicy.ShouldRetry(exception, attemptNumber))
        {
            var delay = retryPolicy.CalculateDelay(attemptNumber);
            return ErrorHandlingResult.Retry(delay, new Dictionary<string, object>
            {
                ["AttemptNumber"] = attemptNumber,
                ["ExceptionType"] = exception.GetType().Name,
                ["RetryPolicy"] = retryPolicy.Name
            });
        }

        return ErrorHandlingResult.DeadLetter($"Retry policy exhausted for message {context.MessageId}");
    }

    /// <summary>
    /// Handles reject strategy
    /// </summary>
    /// <param name="exception">Exception that occurred</param>
    /// <param name="context">Message context</param>
    /// <param name="attemptNumber">Current attempt number</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Error handling result</returns>
    private async Task<ErrorHandlingResult> HandleRejectStrategy(Exception exception, MessageContext context, int attemptNumber, CancellationToken cancellationToken)
    {
        await SendToDeadLetterQueue(exception, context, attemptNumber, cancellationToken);
        return ErrorHandlingResult.Success(ErrorHandlingAction.Reject, new Dictionary<string, object>
        {
            ["AttemptNumber"] = attemptNumber,
            ["ExceptionType"] = exception.GetType().Name,
            ["Action"] = "Rejected and sent to dead letter queue"
        });
    }

    /// <summary>
    /// Handles acknowledge strategy
    /// </summary>
    /// <param name="exception">Exception that occurred</param>
    /// <param name="context">Message context</param>
    /// <param name="attemptNumber">Current attempt number</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Error handling result</returns>
    private Task<ErrorHandlingResult> HandleAcknowledgeStrategy(Exception exception, MessageContext context, int attemptNumber, CancellationToken cancellationToken)
    {
        _logger.LogWarning("Acknowledging message {MessageId} with error: {Exception}", context.MessageId, exception.Message);
        
        // Additional error processing completed
        _logger.LogDebug("Error handling completed for message {MessageId}", context.MessageId);
        
        return Task.FromResult(ErrorHandlingResult.Success(ErrorHandlingAction.Acknowledge, new Dictionary<string, object>
        {
            ["AttemptNumber"] = attemptNumber,
            ["ExceptionType"] = exception.GetType().Name,
            ["Action"] = "Acknowledged despite error"
        }));
    }

    /// <summary>
    /// Handles custom strategy
    /// </summary>
    /// <param name="exception">Exception that occurred</param>
    /// <param name="context">Message context</param>
    /// <param name="attemptNumber">Current attempt number</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Error handling result</returns>
    private async Task<ErrorHandlingResult> HandleCustomStrategy(Exception exception, MessageContext context, int attemptNumber, CancellationToken cancellationToken)
    {
        if (Settings.CustomErrorHandler != null)
        {
            try
            {
                var customResult = await Settings.CustomErrorHandler(exception, context);
                if (customResult)
                {
                    return ErrorHandlingResult.Success(ErrorHandlingAction.Acknowledge, new Dictionary<string, object>
                    {
                        ["AttemptNumber"] = attemptNumber,
                        ["ExceptionType"] = exception.GetType().Name,
                        ["Action"] = "Handled by custom handler"
                    });
                }
            }
            catch (Exception customException)
            {
                _logger.LogError(customException, "Custom error handler failed for message {MessageId}", context.MessageId);
            }
        }

        // Fallback to default strategy
        return await HandleRequeueStrategy(exception, context, attemptNumber, cancellationToken);
    }

    /// <summary>
    /// Sends a message to the dead letter queue
    /// </summary>
    /// <param name="exception">Exception that caused the dead letter</param>
    /// <param name="context">Message context</param>
    /// <param name="attemptNumber">Current attempt number</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Task representing the operation</returns>
    private async Task SendToDeadLetterQueue(Exception exception, MessageContext context, int attemptNumber, CancellationToken cancellationToken)
    {
        try
        {
            if (!_connectionManager.IsConnected)
            {
                await _connectionManager.ConnectAsync(cancellationToken);
            }

            var channel = await _connectionManager.GetChannelAsync(cancellationToken);
            var rabbitChannel = ((RabbitMQChannel)channel).GetNativeChannel();

            var deadLetterExchange = context.DeadLetterExchange ?? "dead-letter-exchange";
            var deadLetterRoutingKey = context.DeadLetterRoutingKey ?? "dead-letter";

            // Declare dead letter exchange if it doesn't exist
            rabbitChannel.ExchangeDeclare(deadLetterExchange, "direct", durable: true);

            // Create dead letter message with metadata
            var deadLetterMessage = new DeadLetterMessage
            {
                MessageId = context.MessageId ?? Guid.CreateVersion7().ToString(),
                Data = context.Body.IsEmpty ? [] : context.Body.ToArray(),
                Context = context,
                Exception = exception,
                AttemptNumber = attemptNumber,
                DeadLetterTimestamp = DateTimeOffset.UtcNow,
                Reason = $"Error: {exception.Message}",
                Metadata = new Dictionary<string, object>
                {
                    ["OriginalExchange"] = context.Exchange ?? string.Empty,
                    ["OriginalRoutingKey"] = context.RoutingKey ?? string.Empty,
                    ["ExceptionType"] = exception.GetType().Name,
                    ["StackTrace"] = exception.StackTrace ?? string.Empty,
                    ["Timestamp"] = DateTimeOffset.UtcNow.ToString("O")
                }
            };

            var messageBody = JsonSerializer.SerializeToUtf8Bytes(deadLetterMessage);

            var properties = rabbitChannel.CreateBasicProperties();
            properties.Persistent = true;
            properties.MessageId = deadLetterMessage.MessageId;
            properties.Timestamp = new AmqpTimestamp(DateTimeOffset.UtcNow.ToUnixTimeSeconds());
            properties.Headers = new Dictionary<string, object>
            {
                ["x-original-exchange"] = context.Exchange ?? string.Empty,
                ["x-original-routing-key"] = context.RoutingKey ?? string.Empty,
                ["x-exception-type"] = exception.GetType().Name,
                ["x-attempt-number"] = attemptNumber,
                ["x-dead-letter-timestamp"] = DateTimeOffset.UtcNow.ToString("O")
            };

            rabbitChannel.BasicPublish(
                exchange: deadLetterExchange,
                routingKey: deadLetterRoutingKey,
                body: messageBody);

            _logger.LogInformation("Message {MessageId} sent to dead letter queue after {AttemptNumber} attempts", 
                context.MessageId, attemptNumber);

            // Raise dead letter event
            var eventArgs = new DeadLetterEventArgs(messageBody, context, exception, attemptNumber);
            MessageSentToDeadLetterQueue?.Invoke(this, eventArgs);
        }
        catch (Exception deadLetterException)
        {
            _logger.LogError(deadLetterException, "Failed to send message {MessageId} to dead letter queue", context.MessageId);
            throw;
        }
    }

    /// <summary>
    /// Disposes the error handler
    /// </summary>
    public void Dispose()
    {
        if (_disposed)
            return;

        lock (_lock)
        {
            if (_disposed)
                return;

            _disposed = true;
            
            // Dispose retry policies
            foreach (var policy in _retryPolicies.Values)
            {
                if (policy is IDisposable disposable)
                {
                    disposable.Dispose();
                }
            }
            _retryPolicies.Clear();
        }
    }
}

/// <summary>
/// RabbitMQ implementation of dead letter handler
/// </summary>
public class RabbitMQDeadLetterHandler : IDeadLetterHandler
{
    private readonly IConnectionManager _connectionManager;
    private readonly ILogger<RabbitMQDeadLetterHandler> _logger;
    private readonly object _lock = new();
    private volatile bool _disposed;

    /// <summary>
    /// Gets the dead letter handler name
    /// </summary>
    public string Name => "RabbitMQ Dead Letter Handler";

    /// <summary>
    /// Gets the dead letter settings
    /// </summary>
    public DeadLetterSettings Settings { get; }

    /// <summary>
    /// Event raised when a message is sent to dead letter queue
    /// </summary>
    public event EventHandler<DeadLetterEventArgs>? MessageSentToDeadLetter;

    /// <summary>
    /// Event raised when a message is reprocessed from dead letter queue
    /// </summary>
    public event EventHandler<DeadLetterReprocessedEventArgs>? MessageReprocessed;

    /// <summary>
    /// Initializes a new instance of the RabbitMQDeadLetterHandler class
    /// </summary>
    /// <param name="connectionManager">Connection manager</param>
    /// <param name="logger">Logger instance</param>
    /// <param name="settings">Dead letter settings</param>
    public RabbitMQDeadLetterHandler(
        IConnectionManager connectionManager,
        ILogger<RabbitMQDeadLetterHandler> logger,
        DeadLetterSettings? settings = null)
    {
        _connectionManager = connectionManager ?? throw new ArgumentNullException(nameof(connectionManager));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        Settings = settings ?? new DeadLetterSettings();
    }

    /// <summary>
    /// Sends a message to the dead letter queue
    /// </summary>
    /// <param name="message">Message to send</param>
    /// <param name="context">Message context</param>
    /// <param name="exception">Exception that caused the dead letter</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Task with operation result</returns>
    public async Task<bool> SendToDeadLetterAsync(byte[] message, MessageContext context, Exception exception, CancellationToken cancellationToken = default)
    {
        return await SendToDeadLetterAsync(message, context, exception, 1, cancellationToken);
    }

    /// <summary>
    /// Sends a message to the dead letter queue with retry information
    /// </summary>
    /// <param name="message">Message to send</param>
    /// <param name="context">Message context</param>
    /// <param name="exception">Exception that caused the dead letter</param>
    /// <param name="attemptNumber">Number of attempts made</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Task with operation result</returns>
    public async Task<bool> SendToDeadLetterAsync(byte[] message, MessageContext context, Exception exception, int attemptNumber, CancellationToken cancellationToken = default)
    {
        if (_disposed)
            throw new ObjectDisposedException(nameof(RabbitMQDeadLetterHandler));

        try
        {
            if (!_connectionManager.IsConnected)
            {
                await _connectionManager.ConnectAsync(cancellationToken);
            }

            var channel = await _connectionManager.GetChannelAsync(cancellationToken);
            var rabbitChannel = ((RabbitMQChannel)channel).GetNativeChannel();

            var deadLetterExchange = Settings.ExchangeName;
            var deadLetterRoutingKey = Settings.RoutingKey;

            // Declare dead letter exchange and queue
            rabbitChannel.ExchangeDeclare(deadLetterExchange, "direct", durable: true);
            
            var queueArgs = new Dictionary<string, object>();
            if (Settings.MessageTtl.HasValue)
            {
                queueArgs["x-message-ttl"] = (int)Settings.MessageTtl.Value.TotalMilliseconds;
            }

            var queueName = $"{deadLetterExchange}.queue";
            rabbitChannel.QueueDeclare(queueName, durable: true, exclusive: false, autoDelete: false, arguments: queueArgs);
            rabbitChannel.QueueBind(queueName, deadLetterExchange, deadLetterRoutingKey);

            // Create dead letter message with metadata
            var deadLetterMessage = new DeadLetterMessage
            {
                MessageId = context.MessageId ?? Guid.CreateVersion7().ToString(),
                Data = message,
                Context = context,
                Exception = exception,
                AttemptNumber = attemptNumber,
                DeadLetterTimestamp = DateTimeOffset.UtcNow,
                Reason = $"Error: {exception.Message}",
                Metadata = new Dictionary<string, object>
                {
                    ["OriginalExchange"] = context.Exchange ?? string.Empty,
                    ["OriginalRoutingKey"] = context.RoutingKey ?? string.Empty,
                    ["ExceptionType"] = exception.GetType().Name,
                    ["StackTrace"] = exception.StackTrace ?? string.Empty,
                    ["Timestamp"] = DateTimeOffset.UtcNow.ToString("O")
                }
            };

            var messageBody = JsonSerializer.SerializeToUtf8Bytes(deadLetterMessage);

            var properties = rabbitChannel.CreateBasicProperties();
            properties.Persistent = true;
            properties.MessageId = deadLetterMessage.MessageId;
            properties.Timestamp = new AmqpTimestamp(DateTimeOffset.UtcNow.ToUnixTimeSeconds());
            properties.Headers = new Dictionary<string, object>
            {
                ["x-original-exchange"] = context.Exchange ?? string.Empty,
                ["x-original-routing-key"] = context.RoutingKey ?? string.Empty,
                ["x-exception-type"] = exception.GetType().Name,
                ["x-attempt-number"] = attemptNumber,
                ["x-dead-letter-timestamp"] = DateTimeOffset.UtcNow.ToString("O")
            };

            rabbitChannel.BasicPublish(
                exchange: deadLetterExchange,
                routingKey: deadLetterRoutingKey,
                body: messageBody);

            _logger.LogInformation("Message {MessageId} sent to dead letter queue", deadLetterMessage.MessageId);

            // Raise event
            var eventArgs = new DeadLetterEventArgs(message, context, exception, attemptNumber);
            MessageSentToDeadLetter?.Invoke(this, eventArgs);

            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to send message to dead letter queue");
            return false;
        }
    }

    /// <summary>
    /// Processes messages from the dead letter queue
    /// </summary>
    /// <param name="messageHandler">Handler for processing dead letter messages</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Task representing the processing operation</returns>
    public async Task ProcessDeadLetterMessagesAsync(Func<DeadLetterMessage, Task<bool>> messageHandler, CancellationToken cancellationToken = default)
    {
        if (_disposed)
            throw new ObjectDisposedException(nameof(RabbitMQDeadLetterHandler));

        if (messageHandler == null)
            throw new ArgumentNullException(nameof(messageHandler));

        try
        {
            if (!_connectionManager.IsConnected)
            {
                await _connectionManager.ConnectAsync(cancellationToken);
            }

            var channel = await _connectionManager.GetChannelAsync(cancellationToken);
            var rabbitChannel = ((RabbitMQChannel)channel).GetNativeChannel();

            var queueName = $"{Settings.ExchangeName}.queue";
            
            while (!cancellationToken.IsCancellationRequested)
            {
                var result = rabbitChannel.BasicGet(queueName, autoAck: false);
                if (result == null)
                {
                    await Task.Delay(1000, cancellationToken); // Wait before checking again
                    continue;
                }

                try
                {
                    var deadLetterMessage = JsonSerializer.Deserialize<DeadLetterMessage>(result.Body.ToArray());
                    if (deadLetterMessage != null)
                    {
                        var success = await messageHandler(deadLetterMessage);
                        if (success)
                        {
                            rabbitChannel.BasicAck(result.DeliveryTag, false);
                            _logger.LogInformation("Successfully processed dead letter message {MessageId}", deadLetterMessage.MessageId);
                        }
                        else
                        {
                            rabbitChannel.BasicNack(result.DeliveryTag, false, true);
                            _logger.LogWarning("Failed to process dead letter message {MessageId}, requeuing", deadLetterMessage.MessageId);
                        }
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error processing dead letter message");
                    rabbitChannel.BasicNack(result.DeliveryTag, false, true);
                }
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in dead letter message processing");
            throw;
        }
    }

    /// <summary>
    /// Reprocesses a message from the dead letter queue
    /// </summary>
    /// <param name="deadLetterMessage">Dead letter message</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Task with operation result</returns>
    public async Task<bool> ReprocessMessageAsync(DeadLetterMessage deadLetterMessage, CancellationToken cancellationToken = default)
    {
        if (_disposed)
            throw new ObjectDisposedException(nameof(RabbitMQDeadLetterHandler));

        if (deadLetterMessage == null)
            throw new ArgumentNullException(nameof(deadLetterMessage));

        try
        {
            if (!_connectionManager.IsConnected)
            {
                await _connectionManager.ConnectAsync(cancellationToken);
            }

            var channel = await _connectionManager.GetChannelAsync(cancellationToken);
            var rabbitChannel = ((RabbitMQChannel)channel).GetNativeChannel();

            var originalExchange = deadLetterMessage.Context.Exchange ?? string.Empty;
            var originalRoutingKey = deadLetterMessage.Context.RoutingKey ?? string.Empty;

            var properties = rabbitChannel.CreateBasicProperties();
            properties.Persistent = true;
            properties.MessageId = deadLetterMessage.MessageId;
            properties.Timestamp = new AmqpTimestamp(DateTimeOffset.UtcNow.ToUnixTimeSeconds());
            properties.Headers = new Dictionary<string, object>
            {
                ["x-reprocessed"] = true,
                ["x-reprocessed-timestamp"] = DateTimeOffset.UtcNow.ToString("O"),
                ["x-original-dead-letter-timestamp"] = deadLetterMessage.DeadLetterTimestamp.ToString("O")
            };

            rabbitChannel.BasicPublish(
                exchange: originalExchange,
                routingKey: originalRoutingKey,
                body: deadLetterMessage.Data);

            _logger.LogInformation("Reprocessed dead letter message {MessageId}", deadLetterMessage.MessageId);

            // Raise event
            var eventArgs = new DeadLetterReprocessedEventArgs(deadLetterMessage, true);
            MessageReprocessed?.Invoke(this, eventArgs);

            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to reprocess dead letter message {MessageId}", deadLetterMessage.MessageId);
            
            // Raise event
            var eventArgs = new DeadLetterReprocessedEventArgs(deadLetterMessage, false, ex.Message);
            MessageReprocessed?.Invoke(this, eventArgs);
            
            return false;
        }
    }

    /// <summary>
    /// Gets statistics for the dead letter queue
    /// </summary>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Task with dead letter statistics</returns>
    public async Task<DeadLetterStatistics> GetStatisticsAsync(CancellationToken cancellationToken = default)
    {
        if (_disposed)
            throw new ObjectDisposedException(nameof(RabbitMQDeadLetterHandler));

        try
        {
            if (!_connectionManager.IsConnected)
            {
                await _connectionManager.ConnectAsync(cancellationToken);
            }

            var channel = await _connectionManager.GetChannelAsync(cancellationToken);
            var rabbitChannel = ((RabbitMQChannel)channel).GetNativeChannel();

            var queueName = $"{Settings.ExchangeName}.queue";
            var queueInfo = rabbitChannel.QueueDeclarePassive(queueName);

            return new DeadLetterStatistics
            {
                TotalMessages = queueInfo.MessageCount,
                ProcessedMessages = 0, // Would need to track this separately
                ReprocessedMessages = 0, // Would need to track this separately
                FailedReprocessing = 0, // Would need to track this separately
                AverageMessageAge = TimeSpan.Zero, // Would need to calculate from message timestamps
                OldestMessageTimestamp = null, // Would need to peek at oldest message
                NewestMessageTimestamp = null, // Would need to peek at newest message
                QueueSizeBytes = 0, // RabbitMQ doesn't provide this directly
                Timestamp = DateTimeOffset.UtcNow
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get dead letter statistics");
            throw;
        }
    }

    /// <summary>
    /// Purges the dead letter queue
    /// </summary>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Task with number of messages purged</returns>
    public async Task<int> PurgeDeadLetterQueueAsync(CancellationToken cancellationToken = default)
    {
        if (_disposed)
            throw new ObjectDisposedException(nameof(RabbitMQDeadLetterHandler));

        try
        {
            if (!_connectionManager.IsConnected)
            {
                await _connectionManager.ConnectAsync(cancellationToken);
            }

            var channel = await _connectionManager.GetChannelAsync(cancellationToken);
            var rabbitChannel = ((RabbitMQChannel)channel).GetNativeChannel();

            var queueName = $"{Settings.ExchangeName}.queue";
            var purgedCount = rabbitChannel.QueuePurge(queueName);

            _logger.LogInformation("Purged {Count} messages from dead letter queue", purgedCount);

            return (int)purgedCount;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to purge dead letter queue");
            throw;
        }
    }

    /// <summary>
    /// Disposes the dead letter handler
    /// </summary>
    public void Dispose()
    {
        if (_disposed)
            return;

        lock (_lock)
        {
            if (_disposed)
                return;

            _disposed = true;
        }
    }
} 