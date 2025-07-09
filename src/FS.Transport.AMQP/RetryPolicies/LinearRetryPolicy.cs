using FS.Transport.AMQP.Configuration;
using FS.Transport.AMQP.Core.Exceptions;
using Microsoft.Extensions.Logging;

namespace FS.Transport.AMQP.RetryPolicies;

/// <summary>
/// Retry policy that implements linear (fixed) delay between retry attempts
/// </summary>
public class LinearRetryPolicy : IRetryPolicy
{
    private readonly RetryPolicySettings _settings;
    private readonly ILogger<LinearRetryPolicy> _logger;

    public string Name => "Linear";
    public int MaxRetries => _settings.MaxRetries;

    public event EventHandler<RetryEventArgs>? Retrying;
    public event EventHandler<RetryEventArgs>? MaxRetriesExceeded;

    public LinearRetryPolicy(RetryPolicySettings settings, ILogger<LinearRetryPolicy> logger)
    {
        _settings = settings ?? throw new ArgumentNullException(nameof(settings));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        
        _logger.LogDebug("LinearRetryPolicy initialized with MaxRetries: {MaxRetries}, " +
                        "FixedDelay: {FixedDelay}ms", 
                        _settings.MaxRetries, _settings.InitialDelayMs);
    }

    /// <summary>
    /// Determines if operation should be retried
    /// </summary>
    public bool ShouldRetry(Exception exception, int attemptCount)
    {
        if (attemptCount >= _settings.MaxRetries)
        {
            _logger.LogWarning("Maximum retry attempts ({MaxRetries}) reached for exception: {Exception}", 
                _settings.MaxRetries, exception.Message);
            return false;
        }

        return IsRetryableException(exception);
    }

    /// <summary>
    /// Calculates fixed delay between retries
    /// </summary>
    public TimeSpan CalculateDelay(int attemptCount)
    {
        // Linear policy uses fixed delay regardless of attempt count
        return TimeSpan.FromMilliseconds(_settings.InitialDelayMs);
    }

    /// <summary>
    /// Executes async operation with linear retry logic
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
                
                return await operation();
            }
            catch (Exception ex) when (!(ex is OperationCanceledException && cancellationToken.IsCancellationRequested))
            {
                lastException = ex;
                
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
        // Use same logic as ExponentialBackoffRetryPolicy
        return exception switch
        {
            ConnectionException => true,
            TimeoutException => true,
            TaskCanceledException => false,
            OperationCanceledException => false,
            RabbitMQ.Client.Exceptions.BrokerUnreachableException => true,
            RabbitMQ.Client.Exceptions.ConnectFailureException => true,
            ArgumentException => false,
            _ => true
        };
    }
}