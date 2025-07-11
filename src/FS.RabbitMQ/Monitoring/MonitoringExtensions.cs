using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace FS.RabbitMQ.Monitoring;

/// <summary>
/// Provides extension methods for configuring monitoring services in the dependency injection container.
/// Includes health checks, metrics collection, and monitoring integration with the RabbitMQ transport.
/// </summary>
public static class MonitoringExtensions
{
    /// <summary>
    /// Adds comprehensive monitoring services to the service collection.
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <returns>The service collection for fluent configuration.</returns>
    public static IServiceCollection AddRabbitMQMonitoring(this IServiceCollection services)
    {
        services.TryAddSingleton<IHealthChecker, HealthChecker>();
        services.TryAddSingleton<IMetricsCollector, MetricsCollector>();
        
        return services;
    }

    /// <summary>
    /// Adds health checking services with default configuration.
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <returns>The service collection for fluent configuration.</returns>
    public static IServiceCollection AddRabbitMQHealthChecks(this IServiceCollection services)
    {
        services.TryAddSingleton<IHealthChecker, HealthChecker>();
        services.AddHostedService<HealthCheckBackgroundService>();
        
        return services;
    }

    /// <summary>
    /// Adds health checking services with custom configuration.
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <param name="configureOptions">The health check configuration action.</param>
    /// <returns>The service collection for fluent configuration.</returns>
    public static IServiceCollection AddRabbitMQHealthChecks(
        this IServiceCollection services,
        Action<HealthCheckOptions> configureOptions)
    {
        services.Configure(configureOptions);
        services.TryAddSingleton<IHealthChecker, HealthChecker>();
        services.AddHostedService<HealthCheckBackgroundService>();
        
        return services;
    }

    /// <summary>
    /// Adds metrics collection services with default configuration.
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <returns>The service collection for fluent configuration.</returns>
    public static IServiceCollection AddRabbitMQMetrics(this IServiceCollection services)
    {
        services.TryAddSingleton<IMetricsCollector, MetricsCollector>();
        services.AddHostedService<MetricsBackgroundService>();
        
        return services;
    }

    /// <summary>
    /// Adds metrics collection services with custom configuration.
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <param name="configureOptions">The metrics configuration action.</param>
    /// <returns>The service collection for fluent configuration.</returns>
    public static IServiceCollection AddRabbitMQMetrics(
        this IServiceCollection services,
        Action<MetricsOptions> configureOptions)
    {
        services.Configure(configureOptions);
        services.TryAddSingleton<IMetricsCollector, MetricsCollector>();
        services.AddHostedService<MetricsBackgroundService>();
        
        return services;
    }

    /// <summary>
    /// Configures health check options.
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <param name="configureOptions">The health check configuration action.</param>
    /// <returns>The service collection for fluent configuration.</returns>
    public static IServiceCollection ConfigureHealthChecks(
        this IServiceCollection services,
        Action<HealthCheckOptions> configureOptions)
    {
        services.Configure(configureOptions);
        return services;
    }

    /// <summary>
    /// Configures metrics options.
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <param name="configureOptions">The metrics configuration action.</param>
    /// <returns>The service collection for fluent configuration.</returns>
    public static IServiceCollection ConfigureMetrics(
        this IServiceCollection services,
        Action<MetricsOptions> configureOptions)
    {
        services.Configure(configureOptions);
        return services;
    }

    /// <summary>
    /// Enables automatic health check startup.
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <param name="interval">The health check interval. If null, uses default interval.</param>
    /// <returns>The service collection for fluent configuration.</returns>
    public static IServiceCollection EnableAutoHealthChecks(
        this IServiceCollection services,
        TimeSpan? interval = null)
    {
        services.Configure<HealthCheckOptions>(options =>
        {
            options.AutoStart = true;
            if (interval.HasValue)
                options.Interval = interval.Value;
        });
        
        return services;
    }

    /// <summary>
    /// Enables automatic metrics collection startup.
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <param name="interval">The metrics collection interval. If null, uses default interval.</param>
    /// <returns>The service collection for fluent configuration.</returns>
    public static IServiceCollection EnableAutoMetrics(
        this IServiceCollection services,
        TimeSpan? interval = null)
    {
        services.Configure<MetricsOptions>(options =>
        {
            options.AutoStart = true;
            if (interval.HasValue)
                options.CollectionInterval = interval.Value;
        });
        
        return services;
    }

    /// <summary>
    /// Adds custom health check.
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <param name="name">The health check name.</param>
    /// <param name="healthCheckFactory">The health check factory.</param>
    /// <param name="timeout">The health check timeout. If null, uses default timeout.</param>
    /// <returns>The service collection for fluent configuration.</returns>
    public static IServiceCollection AddCustomHealthCheck(
        this IServiceCollection services,
        string name,
        Func<IServiceProvider, Func<System.Threading.CancellationToken, System.Threading.Tasks.Task<HealthCheckResult>>> healthCheckFactory,
        TimeSpan? timeout = null)
    {
        services.Configure<HealthCheckOptions>(options =>
        {
            options.CustomHealthChecks[name] = new CustomHealthCheckRegistration(healthCheckFactory, timeout);
        });
        
        return services;
    }
}

/// <summary>
/// Represents health check configuration options.
/// </summary>
public class HealthCheckOptions
{
    /// <summary>
    /// Gets or sets a value indicating whether to automatically start health checks.
    /// </summary>
    public bool AutoStart { get; set; } = true;

    /// <summary>
    /// Gets or sets the health check interval.
    /// </summary>
    public TimeSpan Interval { get; set; } = TimeSpan.FromSeconds(30);

    /// <summary>
    /// Gets or sets the health check timeout.
    /// </summary>
    public TimeSpan Timeout { get; set; } = TimeSpan.FromSeconds(10);

    /// <summary>
    /// Gets the collection of custom health checks.
    /// </summary>
    public Dictionary<string, CustomHealthCheckRegistration> CustomHealthChecks { get; } = new();

    /// <summary>
    /// Gets or sets a value indicating whether to log health check results.
    /// </summary>
    public bool EnableLogging { get; set; } = true;

    /// <summary>
    /// Gets or sets the log level for health check events.
    /// </summary>
    public LogLevel LogLevel { get; set; } = LogLevel.Information;
}

/// <summary>
/// Represents metrics collection configuration options.
/// </summary>
public class MetricsOptions
{
    /// <summary>
    /// Gets or sets a value indicating whether to automatically start metrics collection.
    /// </summary>
    public bool AutoStart { get; set; } = true;

    /// <summary>
    /// Gets or sets the metrics collection interval.
    /// </summary>
    public TimeSpan CollectionInterval { get; set; } = TimeSpan.FromSeconds(10);

    /// <summary>
    /// Gets or sets a value indicating whether to enable performance metrics.
    /// </summary>
    public bool EnablePerformanceMetrics { get; set; } = true;

    /// <summary>
    /// Gets or sets a value indicating whether to enable connection metrics.
    /// </summary>
    public bool EnableConnectionMetrics { get; set; } = true;

    /// <summary>
    /// Gets or sets a value indicating whether to enable message metrics.
    /// </summary>
    public bool EnableMessageMetrics { get; set; } = true;

    /// <summary>
    /// Gets or sets a value indicating whether to log metric updates.
    /// </summary>
    public bool EnableLogging { get; set; } = false;

    /// <summary>
    /// Gets or sets the log level for metric events.
    /// </summary>
    public LogLevel LogLevel { get; set; } = LogLevel.Debug;

    /// <summary>
    /// Gets or sets the export format for metrics.
    /// </summary>
    public MetricExportFormat ExportFormat { get; set; } = MetricExportFormat.Json;
}

/// <summary>
/// Represents a custom health check registration.
/// </summary>
public class CustomHealthCheckRegistration
{
    /// <summary>
    /// Initializes a new instance of the <see cref="CustomHealthCheckRegistration"/> class.
    /// </summary>
    /// <param name="healthCheckFactory">The health check factory.</param>
    /// <param name="timeout">The health check timeout.</param>
    public CustomHealthCheckRegistration(
        Func<IServiceProvider, Func<System.Threading.CancellationToken, System.Threading.Tasks.Task<HealthCheckResult>>> healthCheckFactory,
        TimeSpan? timeout)
    {
        HealthCheckFactory = healthCheckFactory ?? throw new ArgumentNullException(nameof(healthCheckFactory));
        Timeout = timeout ?? TimeSpan.FromSeconds(10);
    }

    /// <summary>
    /// Gets the health check factory.
    /// </summary>
    public Func<IServiceProvider, Func<System.Threading.CancellationToken, System.Threading.Tasks.Task<HealthCheckResult>>> HealthCheckFactory { get; }

    /// <summary>
    /// Gets the health check timeout.
    /// </summary>
    public TimeSpan Timeout { get; }
}

/// <summary>
/// Background service that manages health checks automatically.
/// </summary>
public class HealthCheckBackgroundService : BackgroundService
{
    private readonly IHealthChecker _healthChecker;
    private readonly HealthCheckOptions _options;
    private readonly ILogger<HealthCheckBackgroundService> _logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="HealthCheckBackgroundService"/> class.
    /// </summary>
    /// <param name="healthChecker">The health checker.</param>
    /// <param name="options">The health check options.</param>
    /// <param name="logger">The logger.</param>
    public HealthCheckBackgroundService(
        IHealthChecker healthChecker,
        IOptions<HealthCheckOptions> options,
        ILogger<HealthCheckBackgroundService> logger)
    {
        _healthChecker = healthChecker ?? throw new ArgumentNullException(nameof(healthChecker));
        _options = options?.Value ?? throw new ArgumentNullException(nameof(options));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>
    /// Executes the health check background service.
    /// </summary>
    /// <param name="stoppingToken">The cancellation token that indicates when the service should stop.</param>
    /// <returns>A task that represents the asynchronous operation.</returns>
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        if (!_options.AutoStart)
        {
            _logger.LogInformation("Health check auto-start is disabled");
            return;
        }

        try
        {
            // Register custom health checks
            foreach (var customCheck in _options.CustomHealthChecks)
            {
                // Note: This would need service provider access to properly implement
                // For now, we'll just log that custom checks are configured
                _logger.LogInformation("Custom health check registered: {Name}", customCheck.Key);
            }

            // Start health checker
            await _healthChecker.StartAsync(_options.Interval, stoppingToken);

            if (_options.EnableLogging)
            {
                _healthChecker.HealthStatusChanged += OnHealthStatusChanged;
                _healthChecker.HealthCheckResultChanged += OnHealthCheckResultChanged;
            }

            _logger.LogInformation("Health check background service started with interval: {Interval}", _options.Interval);

            // Keep running until cancellation is requested
            await Task.Delay(Timeout.Infinite, stoppingToken);
        }
        catch (OperationCanceledException)
        {
            // This is expected when the service is being stopped
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in health check background service");
            throw;
        }
        finally
        {
            if (_options.EnableLogging)
            {
                _healthChecker.HealthStatusChanged -= OnHealthStatusChanged;
                _healthChecker.HealthCheckResultChanged -= OnHealthCheckResultChanged;
            }

            await _healthChecker.StopAsync(CancellationToken.None);
            _logger.LogInformation("Health check background service stopped");
        }
    }

    /// <summary>
    /// Handles health status changed events.
    /// </summary>
    private void OnHealthStatusChanged(object? sender, HealthStatusChangedEventArgs e)
    {
        _logger.LogHealthCheckEvent(
            _options.LogLevel,
            "Overall health status changed from {PreviousStatus} to {CurrentStatus}",
            "Overall",
            e.CurrentStatus,
            0);
    }

    /// <summary>
    /// Handles health check result changed events.
    /// </summary>
    private void OnHealthCheckResultChanged(object? sender, HealthCheckResultChangedEventArgs e)
    {
        _logger.LogHealthCheckEvent(
            _options.LogLevel,
            "Health check result changed for component {ComponentName}: {Status} - {Description}",
            e.ComponentName,
            e.CurrentResult.Status,
            e.CurrentResult.Duration.TotalMilliseconds);
    }
}

/// <summary>
/// Background service that manages metrics collection automatically.
/// </summary>
public class MetricsBackgroundService : BackgroundService
{
    private readonly IMetricsCollector _metricsCollector;
    private readonly MetricsOptions _options;
    private readonly ILogger<MetricsBackgroundService> _logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="MetricsBackgroundService"/> class.
    /// </summary>
    /// <param name="metricsCollector">The metrics collector.</param>
    /// <param name="options">The metrics options.</param>
    /// <param name="logger">The logger.</param>
    public MetricsBackgroundService(
        IMetricsCollector metricsCollector,
        IOptions<MetricsOptions> options,
        ILogger<MetricsBackgroundService> logger)
    {
        _metricsCollector = metricsCollector ?? throw new ArgumentNullException(nameof(metricsCollector));
        _options = options?.Value ?? throw new ArgumentNullException(nameof(options));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>
    /// Executes the metrics collection background service.
    /// </summary>
    /// <param name="stoppingToken">The cancellation token that indicates when the service should stop.</param>
    /// <returns>A task that represents the asynchronous operation.</returns>
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        if (!_options.AutoStart)
        {
            _logger.LogInformation("Metrics auto-start is disabled");
            return;
        }

        try
        {
            if (_options.EnableLogging)
            {
                _metricsCollector.MetricUpdated += OnMetricUpdated;
            }

            _logger.LogInformation("Metrics collection background service started with interval: {Interval}", _options.CollectionInterval);

            // Keep running until cancellation is requested
            await Task.Delay(Timeout.Infinite, stoppingToken);
        }
        catch (OperationCanceledException)
        {
            // This is expected when the service is being stopped
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in metrics collection background service");
            throw;
        }
        finally
        {
            if (_options.EnableLogging)
            {
                _metricsCollector.MetricUpdated -= OnMetricUpdated;
            }

            _logger.LogInformation("Metrics collection background service stopped");
        }
    }

    /// <summary>
    /// Handles metric updated events.
    /// </summary>
    private void OnMetricUpdated(object? sender, MetricUpdatedEventArgs e)
    {
        if (_options.EnableLogging)
        {
            _logger.LogMetricEvent(
                _options.LogLevel,
                "Metric updated: {MetricName} = {MetricValue}",
                e.Metric.Name,
                e.Metric.Value,
                e.Metric.Type,
                e.Metric.Tags);
        }
    }
}