using Microsoft.Extensions.Logging;

namespace FS.RabbitMQ.Core.Factories;

/// <summary>
/// Provides abstract base functionality for all factories in the RabbitMQ system.
/// </summary>
/// <remarks>
/// The base factory provides common infrastructure including logging, configuration validation,
/// resource management, and error handling patterns that are shared across all factory implementations.
/// This ensures consistent behavior and reduces code duplication across the factory hierarchy.
/// </remarks>
/// <typeparam name="T">The type of objects this factory creates.</typeparam>
public abstract class FactoryBase<T> : IDisposable where T : class
{
    private readonly ILogger _logger;
    private readonly object _lock = new();
    private bool _disposed;

    /// <summary>
    /// Initializes a new instance of the <see cref="FactoryBase{T}"/> class.
    /// </summary>
    /// <param name="logger">The logger for factory operations.</param>
    /// <exception cref="ArgumentNullException">Thrown when logger is null.</exception>
    protected FactoryBase(ILogger logger)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>
    /// Gets the logger instance for this factory.
    /// </summary>
    protected ILogger Logger => _logger;

    /// <summary>
    /// Gets the synchronization lock for thread-safe operations.
    /// </summary>
    protected object SyncLock => _lock;

    /// <summary>
    /// Gets a value indicating whether this factory has been disposed.
    /// </summary>
    protected bool IsDisposed => _disposed;

    /// <summary>
    /// Creates a new instance of type T with the specified parameters.
    /// </summary>
    /// <param name="parameters">The parameters for object creation.</param>
    /// <returns>A new instance of type T.</returns>
    /// <exception cref="ObjectDisposedException">Thrown when the factory has been disposed.</exception>
    /// <exception cref="FactoryException">Thrown when object creation fails.</exception>
    public T Create(params object[] parameters)
    {
        ThrowIfDisposed();

        lock (_lock)
        {
            try
            {
                _logger.LogDebug("Creating new instance of {Type} with {ParameterCount} parameters", 
                    typeof(T).Name, parameters?.Length ?? 0);

                var instance = CreateCore(parameters);
                
                _logger.LogDebug("Successfully created instance of {Type}", typeof(T).Name);
                return instance;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to create instance of {Type}", typeof(T).Name);
                throw new FactoryException($"Failed to create instance of {typeof(T).Name}", ex);
            }
        }
    }

    /// <summary>
    /// Attempts to create a new instance of type T with the specified parameters.
    /// </summary>
    /// <param name="instance">When this method returns, contains the created instance if successful; otherwise, null.</param>
    /// <param name="parameters">The parameters for object creation.</param>
    /// <returns><c>true</c> if the instance was created successfully; otherwise, <c>false</c>.</returns>
    public bool TryCreate(out T? instance, params object[] parameters)
    {
        instance = null;

        try
        {
            instance = Create(parameters);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to create instance of {Type}", typeof(T).Name);
            return false;
        }
    }

    /// <summary>
    /// When overridden in a derived class, creates the core instance of type T.
    /// </summary>
    /// <param name="parameters">The parameters for object creation.</param>
    /// <returns>A new instance of type T.</returns>
    protected abstract T CreateCore(params object[] parameters);

    /// <summary>
    /// Validates the provided parameters for object creation.
    /// </summary>
    /// <param name="parameters">The parameters to validate.</param>
    /// <exception cref="ArgumentException">Thrown when parameters are invalid.</exception>
    protected virtual void ValidateParameters(params object[] parameters)
    {
        // Default implementation - can be overridden by derived classes
    }

    /// <summary>
    /// Throws an <see cref="ObjectDisposedException"/> if this factory has been disposed.
    /// </summary>
    /// <exception cref="ObjectDisposedException">Thrown when the factory has been disposed.</exception>
    protected void ThrowIfDisposed()
    {
        if (_disposed)
        {
            throw new ObjectDisposedException(GetType().Name);
        }
    }

    /// <summary>
    /// Releases all resources used by the factory.
    /// </summary>
    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    /// <summary>
    /// Releases the unmanaged resources used by the factory and optionally releases the managed resources.
    /// </summary>
    /// <param name="disposing"><c>true</c> to release both managed and unmanaged resources; otherwise, <c>false</c>.</param>
    protected virtual void Dispose(bool disposing)
    {
        if (_disposed || !disposing) return;
        lock (_lock)
        {
            if (_disposed) return;
            _logger.LogDebug("Disposing factory of type {Type}", GetType().Name);
            _disposed = true;
        }
    }
}