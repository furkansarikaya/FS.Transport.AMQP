using FS.RabbitMQ.Configuration;
using FS.RabbitMQ.RetryPolicies;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace FS.RabbitMQ.ErrorHandling;

/// <summary>
/// Comprehensive error handler with multiple strategies and retry logic integration
/// </summary>
public class ErrorHandler : IErrorHandler
{
    private readonly ErrorHandlingSettings _settings;
    private readonly IRetryPolicy _retryPolicy;
    private readonly IDeadLetterHandler _deadLetterHandler;
    private readonly ILogger<ErrorHandler> _logger;
    private readonly ErrorHandlingStatistics _statistics;

    public ErrorHandlingStrategy Strategy { get; }
    public IRetryPolicy RetryPolicy => _retryPolicy;

    public event EventHandler<ErrorHandlingEventArgs>? ErrorHandling;
    public event EventHandler<ErrorHandlingEventArgs>? ErrorHandled;
    public event EventHandler<ErrorHandlingEventArgs>? ErrorSentToDeadLetter;

    public ErrorHandler(
        IOptions<RabbitMQConfiguration> configuration,
        IRetryPolicy retryPolicy,
        IDeadLetterHandler deadLetterHandler,
        ILogger<ErrorHandler> logger)
    {
        _settings = configuration?.Value?.ErrorHandling ?? throw new ArgumentNullException(nameof(configuration));
        _retryPolicy = retryPolicy ?? throw new ArgumentNullException(nameof(retryPolicy));
        _deadLetterHandler = deadLetterHandler ?? throw new ArgumentNullException(nameof(deadLetterHandler));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _statistics = new ErrorHandlingStatistics();
        
        Strategy = Enum.TryParse<ErrorHandlingStrategy>(_settings.Strategy, true, out var strategy) 
            ? strategy 
            : ErrorHandlingStrategy.DeadLetter;
            
        _logger.LogDebug("ErrorHandler initialized with strategy: {Strategy}", Strategy);
    }

    /// <summary>
    /// Handles an error asynchronously with comprehensive strategies
    /// </summary>
    public async Task<ErrorHandlingResult> HandleErrorAsync(ErrorContext context, CancellationToken cancellationToken = default)
    {
        if (context == null)
            throw new ArgumentNullException(nameof(context));

        _statistics.TotalErrors++;
        var startTime = DateTime.UtcNow;

        try
        {
            _logger.LogWarning(context.Exception, "Handling error: {ErrorContext}", context.ToString());
            
            // Raise error handling event
            var handlingArgs = new ErrorHandlingEventArgs(context, Strategy);
            ErrorHandling?.Invoke(this, handlingArgs);

            var result = Strategy switch
            {
                ErrorHandlingStrategy.Ignore => await HandleIgnoreAsync(context, cancellationToken),
                ErrorHandlingStrategy.Retry => await HandleRetryAsync(context, cancellationToken),
                ErrorHandlingStrategy.DeadLetter => await HandleDeadLetterAsync(context, cancellationToken),
                ErrorHandlingStrategy.Requeue => await HandleRequeueAsync(context, cancellationToken),
                ErrorHandlingStrategy.Custom => await HandleCustomAsync(context, cancellationToken),
                _ => await HandleDeadLetterAsync(context, cancellationToken) // Default fallback
            };

            // Update statistics
            if (result.IsSuccess)
            {
                _statistics.SuccessfullyHandled++;
            }
            else
            {
                _statistics.FailedToHandle++;
            }

            var duration = DateTime.UtcNow - startTime;
            _statistics.AverageHandlingTime = CalculateAverageHandlingTime(duration);

            // Raise error handled event
            var handledArgs = new ErrorHandlingEventArgs(context, Strategy, result);
            ErrorHandled?.Invoke(this, handledArgs);

            _logger.LogInformation("Error handling completed: {Action} in {Duration}ms", 
                result.Action, duration.TotalMilliseconds);

            return result;
        }
        catch (Exception ex)
        {
            _statistics.FailedToHandle++;
            _logger.LogError(ex, "Error handling failed for context: {ErrorContext}", context.ToString());
            
            return ErrorHandlingResult.Failure($"Error handling failed: {ex.Message}");
        }
    }

    /// <summary>
    /// Handles an error synchronously
    /// </summary>
    public ErrorHandlingResult HandleError(ErrorContext context)
    {
        return HandleErrorAsync(context).GetAwaiter().GetResult();
    }

    /// <summary>
    /// Determines if this handler can handle the specified error
    /// </summary>
    public bool CanHandle(Exception exception, ErrorContext context)
    {
        // Basic validation
        if (exception == null || context == null)
            return false;

        // Check if exception is retryable for retry strategy
        if (Strategy == ErrorHandlingStrategy.Retry)
        {
            return _retryPolicy.ShouldRetry(exception, context.AttemptCount);
        }

        // All other strategies can handle any exception
        return true;
    }

    /// <summary>
    /// Gets error handling statistics
    /// </summary>
    public ErrorHandlingStatistics GetStatistics()
    {
        return _statistics.Clone();
    }

    private Task<ErrorHandlingResult> HandleIgnoreAsync(ErrorContext context, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Ignoring error and acknowledging message: {ErrorContext}", context.ToString());
        
        // Log the error but acknowledge the message
        if (_settings.LogErrors)
        {
            LogError(context);
        }

        return Task.FromResult(ErrorHandlingResult.Success(ErrorHandlingAction.Acknowledged, "Error ignored and message acknowledged"));
    }

    private async Task<ErrorHandlingResult> HandleRetryAsync(ErrorContext context, CancellationToken cancellationToken)
    {
        if (!_retryPolicy.ShouldRetry(context.Exception, context.AttemptCount))
        {
            _logger.LogWarning("Retry policy indicates no more retries for error: {ErrorContext}", context.ToString());
            
            // Fall back to dead letter if retries exhausted
            return await HandleDeadLetterAsync(context, cancellationToken);
        }

        var delay = _retryPolicy.CalculateDelay(context.AttemptCount + 1);
        
        _logger.LogInformation("Retrying operation after {Delay}ms for error: {ErrorContext}", 
            delay.TotalMilliseconds, context.ToString());

        return ErrorHandlingResult.Retry(delay, $"Retrying after {delay.TotalMilliseconds}ms");
    }

    private async Task<ErrorHandlingResult> HandleDeadLetterAsync(ErrorContext context, CancellationToken cancellationToken)
    {
        try
        {
            _logger.LogWarning("Sending message to dead letter queue: {ErrorContext}", context.ToString());
            
            var success = await _deadLetterHandler.HandleDeadLetterAsync(context, cancellationToken);
            
            if (success)
            {
                // Raise dead letter event
                var deadLetterArgs = new ErrorHandlingEventArgs(context, Strategy);
                ErrorSentToDeadLetter?.Invoke(this, deadLetterArgs);
                
                return ErrorHandlingResult.Success(ErrorHandlingAction.SentToDeadLetter, "Message sent to dead letter queue");
            }
            else
            {
                return ErrorHandlingResult.Failure("Failed to send message to dead letter queue");
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to send message to dead letter queue: {ErrorContext}", context.ToString());
            return ErrorHandlingResult.Failure($"Dead letter handling failed: {ex.Message}");
        }
    }

    private Task<ErrorHandlingResult> HandleRequeueAsync(ErrorContext context, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Requeuing message for later processing: {ErrorContext}", context.ToString());
        
        // For requeue, we don't acknowledge the message and let it be redelivered
        return Task.FromResult(ErrorHandlingResult.Requeue("Message requeued for later processing"));
    }

    private async Task<ErrorHandlingResult> HandleCustomAsync(ErrorContext context, CancellationToken cancellationToken)
    {
        // Apply custom error handling logic based on context and configuration
        // Delegate to appropriate error handling strategy based on the error type and context
        _logger.LogInformation("Using custom error handling strategy: {ErrorContext}", context);
        
        return await HandleDeadLetterAsync(context, cancellationToken);
    }

    private void LogError(ErrorContext context)
    {
        var logLevel = Enum.TryParse<LogLevel>(_settings.ErrorLogLevel, true, out var level) 
            ? level 
            : LogLevel.Error;

        _logger.Log(logLevel, context.Exception, "Message processing error: {ErrorContext}", context.ToString());
    }

    private TimeSpan CalculateAverageHandlingTime(TimeSpan currentDuration)
    {
        var totalHandled = _statistics.SuccessfullyHandled + _statistics.FailedToHandle;
        if (totalHandled <= 1)
            return currentDuration;

        var currentAverage = _statistics.AverageHandlingTime;
        var newAverage = ((currentAverage.TotalMilliseconds * (totalHandled - 1)) + currentDuration.TotalMilliseconds) / totalHandled;
        
        return TimeSpan.FromMilliseconds(newAverage);
    }
}
