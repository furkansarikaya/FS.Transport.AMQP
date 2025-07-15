using FS.StreamFlow.Core.Features.Messaging.Interfaces;
using FS.StreamFlow.Core.Features.Messaging.Models;
using Microsoft.Extensions.Logging;
using System.Collections.Concurrent;

namespace FS.StreamFlow.RabbitMQ.Features.RetryPolicies;

/// <summary>
/// RabbitMQ implementation of retry policy factory providing exponential backoff, linear, circuit breaker, and no-retry policies
/// </summary>
public class RabbitMQRetryPolicyFactory : IRetryPolicyFactory
{
    private readonly ILogger<RabbitMQRetryPolicyFactory> _logger;
    private readonly ConcurrentDictionary<string, IRetryPolicy> _registeredPolicies;
    private readonly ConcurrentDictionary<SerializationFormat, Func<SerializationSettings, IRetryPolicy>> _policyFactories;

    /// <summary>
    /// Initializes a new instance of the RabbitMQRetryPolicyFactory class
    /// </summary>
    /// <param name="logger">Logger instance</param>
    public RabbitMQRetryPolicyFactory(ILogger<RabbitMQRetryPolicyFactory> logger)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _registeredPolicies = new ConcurrentDictionary<string, IRetryPolicy>();
        _policyFactories = new ConcurrentDictionary<SerializationFormat, Func<SerializationSettings, IRetryPolicy>>();
        
        RegisterDefaultPolicies();
    }

    /// <summary>
    /// Creates a retry policy with the specified settings
    /// </summary>
    /// <param name="settings">Retry policy settings</param>
    /// <returns>Retry policy instance</returns>
    public IRetryPolicy CreateRetryPolicy(RetryPolicySettings settings)
    {
        if (settings == null)
            throw new ArgumentNullException(nameof(settings));

        return settings.UseExponentialBackoff
            ? CreateExponentialBackoffPolicy(settings.MaxRetryAttempts, settings.InitialRetryDelay, settings.MaxRetryDelay, settings.RetryDelayMultiplier, settings.UseJitter)
            : CreateLinearPolicy(settings.MaxRetryAttempts, settings.InitialRetryDelay);
    }

    /// <summary>
    /// Creates a retry policy with the specified name and settings
    /// </summary>
    /// <param name="name">Policy name</param>
    /// <param name="settings">Retry policy settings</param>
    /// <returns>Retry policy instance</returns>
    public IRetryPolicy CreateRetryPolicy(string name, RetryPolicySettings settings)
    {
        if (string.IsNullOrEmpty(name))
            throw new ArgumentException("Policy name cannot be null or empty", nameof(name));

        var policy = CreateRetryPolicy(settings);
        RegisterRetryPolicy(name, policy);
        return policy;
    }

    /// <summary>
    /// Creates an exponential backoff retry policy
    /// </summary>
    /// <param name="maxRetryAttempts">Maximum retry attempts</param>
    /// <param name="initialDelay">Initial delay</param>
    /// <param name="maxDelay">Maximum delay</param>
    /// <param name="multiplier">Delay multiplier</param>
    /// <param name="useJitter">Whether to use jitter</param>
    /// <returns>Retry policy instance</returns>
    public IRetryPolicy CreateExponentialBackoffPolicy(int maxRetryAttempts = 3, TimeSpan? initialDelay = null, TimeSpan? maxDelay = null, double multiplier = 2.0, bool useJitter = true)
    {
        var settings = new RetryPolicySettings
        {
            MaxRetryAttempts = maxRetryAttempts,
            InitialRetryDelay = initialDelay ?? TimeSpan.FromSeconds(1),
            MaxRetryDelay = maxDelay ?? TimeSpan.FromSeconds(30),
            RetryDelayMultiplier = multiplier,
            UseExponentialBackoff = true,
            UseJitter = useJitter
        };

        return new ExponentialBackoffRetryPolicy(settings, _logger);
    }

    /// <summary>
    /// Creates a linear retry policy
    /// </summary>
    /// <param name="maxRetryAttempts">Maximum retry attempts</param>
    /// <param name="delay">Fixed delay between retries</param>
    /// <returns>Retry policy instance</returns>
    public IRetryPolicy CreateLinearPolicy(int maxRetryAttempts = 3, TimeSpan? delay = null)
    {
        var settings = new RetryPolicySettings
        {
            MaxRetryAttempts = maxRetryAttempts,
            InitialRetryDelay = delay ?? TimeSpan.FromSeconds(1),
            UseExponentialBackoff = false
        };

        return new LinearRetryPolicy(settings, _logger);
    }

    /// <summary>
    /// Creates a circuit breaker retry policy
    /// </summary>
    /// <param name="maxRetryAttempts">Maximum retry attempts</param>
    /// <param name="circuitBreakerThreshold">Circuit breaker threshold</param>
    /// <param name="circuitBreakerTimeout">Circuit breaker timeout</param>
    /// <returns>Retry policy instance</returns>
    public IRetryPolicy CreateCircuitBreakerPolicy(int maxRetryAttempts = 3, int circuitBreakerThreshold = 5, TimeSpan? circuitBreakerTimeout = null)
    {
        var settings = new RetryPolicySettings
        {
            MaxRetryAttempts = maxRetryAttempts,
            InitialRetryDelay = TimeSpan.FromSeconds(1),
            UseExponentialBackoff = false
        };

        return new CircuitBreakerRetryPolicy(settings, circuitBreakerThreshold, circuitBreakerTimeout ?? TimeSpan.FromMinutes(1), _logger);
    }

    /// <summary>
    /// Creates a no-retry policy
    /// </summary>
    /// <returns>Retry policy instance</returns>
    public IRetryPolicy CreateNoRetryPolicy()
    {
        var settings = new RetryPolicySettings { MaxRetryAttempts = 0 };
        return new NoRetryPolicy(settings, _logger);
    }

    /// <summary>
    /// Gets a registered retry policy by name
    /// </summary>
    /// <param name="name">Policy name</param>
    /// <returns>Retry policy instance or null if not found</returns>
    public IRetryPolicy? GetRetryPolicy(string name)
    {
        if (string.IsNullOrEmpty(name))
            return null;

        return _registeredPolicies.TryGetValue(name, out var policy) ? policy : null;
    }

    /// <summary>
    /// Registers a retry policy
    /// </summary>
    /// <param name="name">Policy name</param>
    /// <param name="policy">Policy instance</param>
    public void RegisterRetryPolicy(string name, IRetryPolicy policy)
    {
        if (string.IsNullOrEmpty(name))
            throw new ArgumentException("Policy name cannot be null or empty", nameof(name));

        if (policy == null)
            throw new ArgumentNullException(nameof(policy));

        _registeredPolicies.AddOrUpdate(name, policy, (_, _) => policy);
        _logger.LogInformation("Registered retry policy: {PolicyName}", name);
    }

    /// <summary>
    /// Unregisters a retry policy
    /// </summary>
    /// <param name="name">Policy name</param>
    /// <returns>True if policy was removed, otherwise false</returns>
    public bool UnregisterRetryPolicy(string name)
    {
        if (string.IsNullOrEmpty(name))
            return false;

        var removed = _registeredPolicies.TryRemove(name, out _);
        if (removed)
        {
            _logger.LogInformation("Unregistered retry policy: {PolicyName}", name);
        }
        return removed;
    }

    /// <summary>
    /// Registers default retry policies
    /// </summary>
    private void RegisterDefaultPolicies()
    {
        RegisterRetryPolicy("default", CreateExponentialBackoffPolicy());
        RegisterRetryPolicy("exponential", CreateExponentialBackoffPolicy());
        RegisterRetryPolicy("linear", CreateLinearPolicy());
        RegisterRetryPolicy("circuit-breaker", CreateCircuitBreakerPolicy());
        RegisterRetryPolicy("no-retry", CreateNoRetryPolicy());
    }
}

/// <summary>
/// Base class for retry policies
/// </summary>
public abstract class RetryPolicyBase : IRetryPolicy
{
    protected readonly ILogger _logger;
    protected int _currentAttempt;

    /// <summary>
    /// Gets the retry policy name
    /// </summary>
    public abstract string Name { get; }

    /// <summary>
    /// Gets the maximum number of retry attempts
    /// </summary>
    public int MaxRetryAttempts { get; }

    /// <summary>
    /// Gets the retry policy settings
    /// </summary>
    public RetryPolicySettings Settings { get; }

    /// <summary>
    /// Gets the current retry attempt count
    /// </summary>
    public int CurrentAttempt => _currentAttempt;

    /// <summary>
    /// Event raised when a retry attempt is made
    /// </summary>
    public event EventHandler<RetryAttemptEventArgs>? RetryAttempt;

    /// <summary>
    /// Event raised when all retry attempts are exhausted
    /// </summary>
    public event EventHandler<RetryExhaustedEventArgs>? RetryExhausted;

    /// <summary>
    /// Initializes a new instance of the RetryPolicyBase class
    /// </summary>
    /// <param name="settings">Retry policy settings</param>
    /// <param name="logger">Logger instance</param>
    protected RetryPolicyBase(RetryPolicySettings settings, ILogger logger)
    {
        Settings = settings ?? throw new ArgumentNullException(nameof(settings));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        MaxRetryAttempts = settings.MaxRetryAttempts;
        _currentAttempt = 0;
    }

    /// <summary>
    /// Determines if a retry should be performed based on the exception and attempt count
    /// </summary>
    /// <param name="exception">Exception that occurred</param>
    /// <param name="attemptNumber">Current attempt number</param>
    /// <returns>True if retry should be performed, otherwise false</returns>
    public virtual bool ShouldRetry(Exception exception, int attemptNumber)
    {
        return attemptNumber < MaxRetryAttempts;
    }

    /// <summary>
    /// Calculates the delay before the next retry attempt
    /// </summary>
    /// <param name="attemptNumber">Current attempt number</param>
    /// <returns>Delay before next retry</returns>
    public abstract TimeSpan CalculateDelay(int attemptNumber);

    /// <summary>
    /// Executes an action with retry logic
    /// </summary>
    /// <param name="action">Action to execute</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Task representing the operation</returns>
    public async Task ExecuteAsync(Func<Task> action, CancellationToken cancellationToken = default)
    {
        var startTime = DateTimeOffset.UtcNow;
        Exception? lastException = null;

        for (int attempt = 1; attempt <= MaxRetryAttempts + 1; attempt++)
        {
            try
            {
                _currentAttempt = attempt;
                await action();
                return;
            }
            catch (Exception ex)
            {
                lastException = ex;
                
                if (attempt <= MaxRetryAttempts && ShouldRetry(ex, attempt))
                {
                    var delay = CalculateDelay(attempt);
                    var eventArgs = new RetryAttemptEventArgs(attempt, ex, delay, Name);
                    RetryAttempt?.Invoke(this, eventArgs);
                    
                    _logger.LogWarning("Retry attempt {Attempt} for {PolicyName} after {Delay}ms. Exception: {Exception}", 
                        attempt, Name, delay.TotalMilliseconds, ex.Message);
                    
                    await Task.Delay(delay, cancellationToken);
                }
                else
                {
                    var totalTime = DateTimeOffset.UtcNow - startTime;
                    var exhaustedArgs = new RetryExhaustedEventArgs(attempt, ex, Name, totalTime);
                    RetryExhausted?.Invoke(this, exhaustedArgs);
                    
                    _logger.LogError("Retry policy {PolicyName} exhausted after {Attempts} attempts in {TotalTime}ms", 
                        Name, attempt, totalTime.TotalMilliseconds);
                    
                    throw;
                }
            }
        }

        throw lastException!;
    }

    /// <summary>
    /// Executes an action with retry logic and returns a result
    /// </summary>
    /// <typeparam name="T">Result type</typeparam>
    /// <param name="action">Action to execute</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Task with result</returns>
    public async Task<T> ExecuteAsync<T>(Func<Task<T>> action, CancellationToken cancellationToken = default)
    {
        var startTime = DateTimeOffset.UtcNow;
        Exception? lastException = null;

        for (int attempt = 1; attempt <= MaxRetryAttempts + 1; attempt++)
        {
            try
            {
                _currentAttempt = attempt;
                return await action();
            }
            catch (Exception ex)
            {
                lastException = ex;
                
                if (attempt <= MaxRetryAttempts && ShouldRetry(ex, attempt))
                {
                    var delay = CalculateDelay(attempt);
                    var eventArgs = new RetryAttemptEventArgs(attempt, ex, delay, Name);
                    RetryAttempt?.Invoke(this, eventArgs);
                    
                    _logger.LogWarning("Retry attempt {Attempt} for {PolicyName} after {Delay}ms. Exception: {Exception}", 
                        attempt, Name, delay.TotalMilliseconds, ex.Message);
                    
                    await Task.Delay(delay, cancellationToken);
                }
                else
                {
                    var totalTime = DateTimeOffset.UtcNow - startTime;
                    var exhaustedArgs = new RetryExhaustedEventArgs(attempt, ex, Name, totalTime);
                    RetryExhausted?.Invoke(this, exhaustedArgs);
                    
                    _logger.LogError("Retry policy {PolicyName} exhausted after {Attempts} attempts in {TotalTime}ms", 
                        Name, attempt, totalTime.TotalMilliseconds);
                    
                    throw;
                }
            }
        }

        throw lastException!;
    }

    /// <summary>
    /// Resets the retry policy state
    /// </summary>
    public virtual void Reset()
    {
        _currentAttempt = 0;
    }
}

/// <summary>
/// Exponential backoff retry policy
/// </summary>
public class ExponentialBackoffRetryPolicy : RetryPolicyBase
{
    private readonly Random _random = new();

    /// <summary>
    /// Gets the retry policy name
    /// </summary>
    public override string Name => "ExponentialBackoff";

    /// <summary>
    /// Initializes a new instance of the ExponentialBackoffRetryPolicy class
    /// </summary>
    /// <param name="settings">Retry policy settings</param>
    /// <param name="logger">Logger instance</param>
    public ExponentialBackoffRetryPolicy(RetryPolicySettings settings, ILogger logger) : base(settings, logger)
    {
    }

    /// <summary>
    /// Calculates the delay before the next retry attempt using exponential backoff
    /// </summary>
    /// <param name="attemptNumber">Current attempt number</param>
    /// <returns>Delay before next retry</returns>
    public override TimeSpan CalculateDelay(int attemptNumber)
    {
        var delay = Settings.InitialRetryDelay;
        
        for (int i = 1; i < attemptNumber; i++)
        {
            delay = TimeSpan.FromMilliseconds(delay.TotalMilliseconds * Settings.RetryDelayMultiplier);
        }

        if (delay > Settings.MaxRetryDelay)
        {
            delay = Settings.MaxRetryDelay;
        }

        if (Settings.UseJitter)
        {
            var jitter = _random.NextDouble() * 0.1; // 10% jitter
            delay = TimeSpan.FromMilliseconds(delay.TotalMilliseconds * (1 + jitter));
        }

        return delay;
    }
}

/// <summary>
/// Linear retry policy
/// </summary>
public class LinearRetryPolicy : RetryPolicyBase
{
    /// <summary>
    /// Gets the retry policy name
    /// </summary>
    public override string Name => "Linear";

    /// <summary>
    /// Initializes a new instance of the LinearRetryPolicy class
    /// </summary>
    /// <param name="settings">Retry policy settings</param>
    /// <param name="logger">Logger instance</param>
    public LinearRetryPolicy(RetryPolicySettings settings, ILogger logger) : base(settings, logger)
    {
    }

    /// <summary>
    /// Calculates the delay before the next retry attempt using linear delay
    /// </summary>
    /// <param name="attemptNumber">Current attempt number</param>
    /// <returns>Delay before next retry</returns>
    public override TimeSpan CalculateDelay(int attemptNumber)
    {
        return Settings.InitialRetryDelay;
    }
}

/// <summary>
/// Circuit breaker retry policy
/// </summary>
public class CircuitBreakerRetryPolicy : RetryPolicyBase
{
    private readonly int _circuitBreakerThreshold;
    private readonly TimeSpan _circuitBreakerTimeout;
    private int _consecutiveFailures;
    private DateTimeOffset _lastFailureTime;
    private bool _circuitOpen;

    /// <summary>
    /// Gets the retry policy name
    /// </summary>
    public override string Name => "CircuitBreaker";

    /// <summary>
    /// Initializes a new instance of the CircuitBreakerRetryPolicy class
    /// </summary>
    /// <param name="settings">Retry policy settings</param>
    /// <param name="circuitBreakerThreshold">Circuit breaker threshold</param>
    /// <param name="circuitBreakerTimeout">Circuit breaker timeout</param>
    /// <param name="logger">Logger instance</param>
    public CircuitBreakerRetryPolicy(RetryPolicySettings settings, int circuitBreakerThreshold, TimeSpan circuitBreakerTimeout, ILogger logger) 
        : base(settings, logger)
    {
        _circuitBreakerThreshold = circuitBreakerThreshold;
        _circuitBreakerTimeout = circuitBreakerTimeout;
        _consecutiveFailures = 0;
        _circuitOpen = false;
    }

    /// <summary>
    /// Determines if a retry should be performed based on circuit breaker state
    /// </summary>
    /// <param name="exception">Exception that occurred</param>
    /// <param name="attemptNumber">Current attempt number</param>
    /// <returns>True if retry should be performed, otherwise false</returns>
    public override bool ShouldRetry(Exception exception, int attemptNumber)
    {
        if (_circuitOpen)
        {
            if (DateTimeOffset.UtcNow - _lastFailureTime > _circuitBreakerTimeout)
            {
                _circuitOpen = false;
                _consecutiveFailures = 0;
                _logger.LogInformation("Circuit breaker reset for {PolicyName}", Name);
            }
            else
            {
                _logger.LogWarning("Circuit breaker is open for {PolicyName}, not retrying", Name);
                return false;
            }
        }

        _consecutiveFailures++;
        _lastFailureTime = DateTimeOffset.UtcNow;

        if (_consecutiveFailures >= _circuitBreakerThreshold)
        {
            _circuitOpen = true;
            _logger.LogWarning("Circuit breaker opened for {PolicyName} after {Failures} consecutive failures", Name, _consecutiveFailures);
            return false;
        }

        return base.ShouldRetry(exception, attemptNumber);
    }

    /// <summary>
    /// Calculates the delay before the next retry attempt
    /// </summary>
    /// <param name="attemptNumber">Current attempt number</param>
    /// <returns>Delay before next retry</returns>
    public override TimeSpan CalculateDelay(int attemptNumber)
    {
        return Settings.InitialRetryDelay;
    }

    /// <summary>
    /// Resets the circuit breaker state
    /// </summary>
    public override void Reset()
    {
        base.Reset();
        _consecutiveFailures = 0;
        _circuitOpen = false;
    }
}

/// <summary>
/// No retry policy
/// </summary>
public class NoRetryPolicy : RetryPolicyBase
{
    /// <summary>
    /// Gets the retry policy name
    /// </summary>
    public override string Name => "NoRetry";

    /// <summary>
    /// Initializes a new instance of the NoRetryPolicy class
    /// </summary>
    /// <param name="settings">Retry policy settings</param>
    /// <param name="logger">Logger instance</param>
    public NoRetryPolicy(RetryPolicySettings settings, ILogger logger) : base(settings, logger)
    {
    }

    /// <summary>
    /// Always returns false since no retries are performed
    /// </summary>
    /// <param name="exception">Exception that occurred</param>
    /// <param name="attemptNumber">Current attempt number</param>
    /// <returns>Always false</returns>
    public override bool ShouldRetry(Exception exception, int attemptNumber)
    {
        return false;
    }

    /// <summary>
    /// Returns zero delay since no retries are performed
    /// </summary>
    /// <param name="attemptNumber">Current attempt number</param>
    /// <returns>Zero delay</returns>
    public override TimeSpan CalculateDelay(int attemptNumber)
    {
        return TimeSpan.Zero;
    }
} 