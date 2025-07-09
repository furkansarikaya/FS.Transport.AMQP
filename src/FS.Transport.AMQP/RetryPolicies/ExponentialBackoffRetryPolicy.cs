using System.Net.Sockets;
using FS.Transport.AMQP.Configuration;
using FS.Transport.AMQP.Core.Exceptions;
using Microsoft.Extensions.Logging;

namespace FS.Transport.AMQP.RetryPolicies;

/// <summary>
/// Retry policy that implements exponential backoff with jitter and circuit breaker behavior
/// </summary>
public class ExponentialBackoffRetryPolicy : IRetryPolicy
{
    private readonly RetryPolicySettings _settings;
    private readonly ILogger<ExponentialBackoffRetryPolicy> _logger;
    private readonly Random _random;
    private volatile int _consecutiveFailures;
    private DateTime _lastFailureTime = DateTime.MinValue;

    public string Name => "ExponentialBackoff";
    public int MaxRetries => _settings.MaxRetries;

    public event EventHandler<RetryEventArgs>? Retrying;
    public event EventHandler<RetryEventArgs>? MaxRetriesExceeded;

    public ExponentialBackoffRetryPolicy(RetryPolicySettings settings, ILogger<ExponentialBackoffRetryPolicy> logger)
    {
        _settings = settings ?? throw new ArgumentNullException(nameof(settings));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _random = new Random();
        
        _logger.LogDebug("ExponentialBackoffRetryPolicy initialized with MaxRetries: {MaxRetries}, " +
                        "InitialDelay: {InitialDelay}ms, MaxDelay: {MaxDelay}ms", 
                        _settings.MaxRetries, _settings.InitialDelayMs, _settings.MaxDelayMs);
    }

    /// <summary>
    /// Determines if operation should be retried based on exception type and attempt count
    /// </summary>
    public bool ShouldRetry(Exception exception, int attemptCount)
    {
        if (attemptCount >= _settings.MaxRetries)
        {
            _logger.LogWarning("Maximum retry attempts ({MaxRetries}) reached for exception: {Exception}", 
                _settings.MaxRetries, exception.Message);
            return false;
        }

        // Circuit breaker logic - if too many consecutive failures, stop retrying temporarily
        if (_consecutiveFailures >= _settings.CircuitBreakerFailureThreshold)
        {
            var timeSinceLastFailure = DateTime.UtcNow - _lastFailureTime;
            if (timeSinceLastFailure < TimeSpan.FromMilliseconds(_settings.CircuitBreakerRecoveryTimeoutMs))
            {
                _logger.LogWarning("Circuit breaker is open - too many consecutive failures ({FailureCount}). " +
                                  "Waiting {RemainingTime}ms before retry", 
                                  _consecutiveFailures, 
                                  (_settings.CircuitBreakerRecoveryTimeoutMs - timeSinceLastFailure.TotalMilliseconds));
                return false;
            }
            else
            {
                _logger.LogInformation("Circuit breaker recovery timeout elapsed, resetting failure count");
                _consecutiveFailures = 0;
            }
        }

        // Check if exception is retryable
        return IsRetryableException(exception);
    }

    /// <summary>
    /// Calculates exponential backoff delay with jitter
    /// </summary>
    public TimeSpan CalculateDelay(int attemptCount)
    {
        if (attemptCount <= 0)
            return TimeSpan.Zero;

        // Exponential backoff: delay = initialDelay * (backoffMultiplier ^ (attemptCount - 1))
        var baseDelay = _settings.InitialDelayMs * Math.Pow(_settings.BackoffMultiplier, attemptCount - 1);
        
        // Cap at maximum delay
        baseDelay = Math.Min(baseDelay, _settings.MaxDelayMs);
        
        // Add jitter to prevent thundering herd
        var jitter = baseDelay * _settings.JitterFactor * (_random.NextDouble() - 0.5) * 2;
        var finalDelay = Math.Max(0, baseDelay + jitter);
        
        _logger.LogTrace("Calculated retry delay: {Delay}ms for attempt {AttemptCount}", finalDelay, attemptCount);
        
        return TimeSpan.FromMilliseconds(finalDelay);
    }

    /// <summary>
    /// Executes async operation with exponential backoff retry logic
    /// </summary>
    public async Task<T> ExecuteAsync<T>(Func<Task<T>> operation, CancellationToken cancellationToken = default)
    {
        if (operation == null)
            throw new ArgumentNullException(nameof(operation));

        Exception? lastException = null;
        
        for (int attempt = 1; attempt <= _settings.MaxRetries + 1; attempt++)
        {
            try
            {
                _logger.LogTrace("Executing operation, attempt {Attempt} of {MaxAttempts}", 
                    attempt, _settings.MaxRetries + 1);
                
                var result = await operation();
                
                // Reset failure count on success
                if (_consecutiveFailures > 0)
                {
                    _logger.LogDebug("Operation succeeded after {FailureCount} consecutive failures, resetting count", 
                        _consecutiveFailures);
                    _consecutiveFailures = 0;
                }
                
                return result;
            }
            catch (Exception ex) when (!(ex is OperationCanceledException && cancellationToken.IsCancellationRequested))
            {
                lastException = ex;
                _consecutiveFailures++;
                _lastFailureTime = DateTime.UtcNow;
                
                _logger.LogWarning(ex, "Operation failed on attempt {Attempt}: {ErrorMessage}", attempt, ex.Message);
                
                if (!ShouldRetry(ex, attempt))
                {
                    var eventArgs = new RetryEventArgs(attempt, ex, TimeSpan.Zero, true);
                    MaxRetriesExceeded?.Invoke(this, eventArgs);
                    break;
                }
                
                if (attempt <= _settings.MaxRetries)
                {
                    var delay = CalculateDelay(attempt);
                    var retryEventArgs = new RetryEventArgs(attempt, ex, delay, attempt == _settings.MaxRetries);
                    Retrying?.Invoke(this, retryEventArgs);
                    
                    _logger.LogInformation("Retrying operation in {Delay}ms (attempt {Attempt} of {MaxAttempts})", 
                        delay.TotalMilliseconds, attempt + 1, _settings.MaxRetries + 1);
                    
                    await Task.Delay(delay, cancellationToken);
                }
            }
        }
        
        // All retries exhausted
        _logger.LogError(lastException, "Operation failed after {MaxRetries} retry attempts", _settings.MaxRetries);
        throw lastException ?? new InvalidOperationException("Operation failed with no exception details");
    }

    /// <summary>
    /// Executes async operation with retry logic
    /// </summary>
    public async Task ExecuteAsync(Func<Task> operation, CancellationToken cancellationToken = default)
    {
        await ExecuteAsync(async () =>
        {
            await operation();
            return true;
        }, cancellationToken);
    }

    /// <summary>
    /// Executes synchronous operation with retry logic
    /// </summary>
    public T Execute<T>(Func<T> operation)
    {
        return ExecuteAsync(() => Task.FromResult(operation())).GetAwaiter().GetResult();
    }

    /// <summary>
    /// Executes synchronous operation with retry logic
    /// </summary>
    public void Execute(Action operation)
    {
        Execute(() =>
        {
            operation();
            return true;
        });
    }

    private static bool IsRetryableException(Exception exception)
    {
        return exception switch
        {
            // Network and connection related exceptions are generally retryable
            ConnectionException => true,
            TimeoutException => true,
            SocketException => true,
            HttpRequestException => true,
            TaskCanceledException => false, // Usually indicates cancellation, not worth retrying
            OperationCanceledException => false,
            
            // Specific RabbitMQ exceptions
            RabbitMQ.Client.Exceptions.BrokerUnreachableException => true,
            RabbitMQ.Client.Exceptions.ConnectFailureException => true,
            RabbitMQ.Client.Exceptions.AlreadyClosedException => true,
            
            // Serialization exceptions are typically not retryable
            SerializationException => false,
            
            // Argument exceptions indicate programming errors, not retryable
            ArgumentException => false,
            InvalidOperationException => false,
            
            // Default: retry for other exceptions
            _ => true
        };
    }
}