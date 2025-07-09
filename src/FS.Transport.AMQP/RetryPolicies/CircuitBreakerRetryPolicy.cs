using FS.Transport.AMQP.Configuration;
using Microsoft.Extensions.Logging;

namespace FS.Transport.AMQP.RetryPolicies;

/// <summary>
/// Circuit breaker pattern implementation that stops retrying when failure threshold is exceeded
/// </summary>
public class CircuitBreakerRetryPolicy : IRetryPolicy
{
    private readonly RetryPolicySettings _settings;
    private readonly ILogger<CircuitBreakerRetryPolicy> _logger;
    private volatile CircuitBreakerState _state = CircuitBreakerState.Closed;
    private volatile int _failureCount;
    private DateTime _lastFailureTime = DateTime.MinValue;
    private DateTime _lastSuccessTime = DateTime.UtcNow;
    private readonly object _lock = new();

    public string Name => "CircuitBreaker";
    public int MaxRetries => _settings.MaxRetries;
    
    /// <summary>
    /// Current state of the circuit breaker
    /// </summary>
    public CircuitBreakerState State => _state;
    
    /// <summary>
    /// Current failure count
    /// </summary>
    public int FailureCount => _failureCount;

    public event EventHandler<RetryEventArgs>? Retrying;
    public event EventHandler<RetryEventArgs>? MaxRetriesExceeded;
    public event EventHandler<CircuitBreakerStateChangedEventArgs>? StateChanged;

    public CircuitBreakerRetryPolicy(RetryPolicySettings settings, ILogger<CircuitBreakerRetryPolicy> logger)
    {
        _settings = settings ?? throw new ArgumentNullException(nameof(settings));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        
        _logger.LogDebug("CircuitBreakerRetryPolicy initialized with FailureThreshold: {FailureThreshold}, " +
                        "RecoveryTimeout: {RecoveryTimeout}ms", 
                        _settings.CircuitBreakerFailureThreshold, _settings.CircuitBreakerRecoveryTimeoutMs);
    }

    /// <summary>
    /// Determines if operation should be retried based on circuit breaker state
    /// </summary>
    public bool ShouldRetry(Exception exception, int attemptCount)
    {
        lock (_lock)
        {
            var currentTime = DateTime.UtcNow;
            
            switch (_state)
            {
                case CircuitBreakerState.Open:
                    // Circuit is open - check if recovery timeout has elapsed
                    if (currentTime - _lastFailureTime >= TimeSpan.FromMilliseconds(_settings.CircuitBreakerRecoveryTimeoutMs))
                    {
                        TransitionTo(CircuitBreakerState.HalfOpen);
                        _logger.LogInformation("Circuit breaker transitioning to Half-Open state for recovery attempt");
                        return attemptCount < _settings.MaxRetries;
                    }
                    
                    _logger.LogWarning("Circuit breaker is OPEN - rejecting operation");
                    return false;
                
                case CircuitBreakerState.HalfOpen:
                    // In half-open state, allow one retry attempt
                    return attemptCount <= 1;
                
                case CircuitBreakerState.Closed:
                default:
                    return attemptCount < _settings.MaxRetries;
            }
        }
    }

    /// <summary>
    /// Calculates delay with circuit breaker considerations
    /// </summary>
    public TimeSpan CalculateDelay(int attemptCount)
    {
        if (_state == CircuitBreakerState.Open)
        {
            // When circuit is open, return recovery timeout
            var timeSinceLastFailure = DateTime.UtcNow - _lastFailureTime;
            var remainingRecoveryTime = TimeSpan.FromMilliseconds(_settings.CircuitBreakerRecoveryTimeoutMs) - timeSinceLastFailure;
            return remainingRecoveryTime > TimeSpan.Zero ? remainingRecoveryTime : TimeSpan.Zero;
        }
        
        // Use exponential backoff for normal retry delays
        var baseDelay = _settings.InitialDelayMs * Math.Pow(_settings.BackoffMultiplier, attemptCount - 1);
        return TimeSpan.FromMilliseconds(Math.Min(baseDelay, _settings.MaxDelayMs));
    }

    /// <summary>
    /// Executes async operation with circuit breaker logic
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
                _logger.LogTrace("Executing operation, attempt {Attempt} of {MaxAttempts} (Circuit State: {State})", 
                    attempt, _settings.MaxRetries + 1, _state);
                
                var result = await operation();
                
                // Operation succeeded - handle circuit breaker state
                OnOperationSuccess();
                
                return result;
            }
            catch (Exception ex) when (!(ex is OperationCanceledException && cancellationToken.IsCancellationRequested))
            {
                lastException = ex;
                
                // Operation failed - handle circuit breaker state
                OnOperationFailure(ex);
                
                _logger.LogWarning(ex, "Operation failed on attempt {Attempt}: {ErrorMessage} (Circuit State: {State})", 
                    attempt, ex.Message, _state);
                
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
                    
                    _logger.LogInformation("Retrying operation in {Delay}ms (attempt {Attempt} of {MaxAttempts}, Circuit State: {State})", 
                        delay.TotalMilliseconds, attempt + 1, _settings.MaxRetries + 1, _state);
                    
                    if (delay > TimeSpan.Zero)
                    {
                        await Task.Delay(delay, cancellationToken);
                    }
                }
            }
        }
        
        _logger.LogError(lastException, "Operation failed after {MaxRetries} retry attempts (Final Circuit State: {State})", 
            _settings.MaxRetries, _state);
        throw lastException ?? new InvalidOperationException("Operation failed with no exception details");
    }

    /// <summary>
    /// Executes async operation with circuit breaker logic
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
    /// Executes synchronous operation with circuit breaker logic
    /// </summary>
    public T Execute<T>(Func<T> operation)
    {
        return ExecuteAsync(() => Task.FromResult(operation())).GetAwaiter().GetResult();
    }

    /// <summary>
    /// Executes synchronous operation with circuit breaker logic
    /// </summary>
    public void Execute(Action operation)
    {
        Execute(() =>
        {
            operation();
            return true;
        });
    }

    private void OnOperationSuccess()
    {
        lock (_lock)
        {
            _lastSuccessTime = DateTime.UtcNow;
            
            if (_state == CircuitBreakerState.HalfOpen)
            {
                // Success in half-open state - transition back to closed
                _failureCount = 0;
                TransitionTo(CircuitBreakerState.Closed);
                _logger.LogInformation("Circuit breaker transitioning to CLOSED state after successful recovery");
            }
            else if (_state == CircuitBreakerState.Closed && _failureCount > 0)
            {
                // Reset failure count on success in closed state
                _failureCount = 0;
                _logger.LogDebug("Circuit breaker failure count reset after successful operation");
            }
        }
    }

    private void OnOperationFailure(Exception exception)
    {
        lock (_lock)
        {
            _failureCount++;
            _lastFailureTime = DateTime.UtcNow;
            
            if (_state == CircuitBreakerState.HalfOpen)
            {
                // Failure in half-open state - transition back to open
                TransitionTo(CircuitBreakerState.Open);
                _logger.LogWarning("Circuit breaker transitioning to OPEN state after failed recovery attempt");
            }
            else if (_state == CircuitBreakerState.Closed && _failureCount >= _settings.CircuitBreakerFailureThreshold)
            {
                // Too many failures in closed state - transition to open
                TransitionTo(CircuitBreakerState.Open);
                _logger.LogWarning("Circuit breaker transitioning to OPEN state after {FailureCount} consecutive failures", 
                    _failureCount);
            }
        }
    }

    private void TransitionTo(CircuitBreakerState newState)
    {
        var oldState = _state;
        _state = newState;
        
        StateChanged?.Invoke(this, new CircuitBreakerStateChangedEventArgs(oldState, newState, _failureCount));
    }
}