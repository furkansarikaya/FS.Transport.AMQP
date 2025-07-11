using FS.RabbitMQ.Configuration;
using FS.RabbitMQ.ErrorHandling;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Hosting;

namespace FS.RabbitMQ.DependencyInjection;

/// <summary>
/// Configuration extensions for RabbitMQ services using the options pattern
/// </summary>
/// <remarks>
/// This class provides extension methods for configuring RabbitMQ services using the
/// .NET options pattern. It supports configuration validation, change monitoring,
/// environment-specific settings, and named options for multi-tenant scenarios.
/// </remarks>
public static class RabbitMQConfigurationExtensions
{
    #region Basic Configuration Methods

    /// <summary>
    /// Configures RabbitMQ services with the options pattern
    /// </summary>
    /// <param name="services">The service collection</param>
    /// <param name="configure">Action to configure RabbitMQ options</param>
    /// <returns>The service collection for fluent configuration</returns>
    /// <exception cref="ArgumentNullException">Thrown when configure action is null</exception>
    /// <remarks>
    /// This method sets up RabbitMQ configuration using the options pattern with validation
    /// and change monitoring support. It's the preferred way to configure RabbitMQ services
    /// in production applications.
    /// </remarks>
    /// <example>
    /// <code>
    /// services.ConfigureRabbitMQ(options =>
    /// {
    ///     options.Connection.ConnectionString = "amqp://localhost";
    ///     options.Producer.EnableConfirmations = true;
    ///     options.Consumer.PrefetchCount = 10;
    /// });
    /// </code>
    /// </example>
    public static IServiceCollection ConfigureRabbitMQ(this IServiceCollection services, Action<RabbitMQConfiguration> configure)
    {
        ArgumentNullException.ThrowIfNull(configure);

        services.Configure<RabbitMQConfiguration>(configure);
        services.AddSingleton<IValidateOptions<RabbitMQConfiguration>, RabbitMQConfigurationValidator>();
        
        return services;
    }

    /// <summary>
    /// Configures RabbitMQ services from IConfiguration
    /// </summary>
    /// <param name="services">The service collection</param>
    /// <param name="configuration">The configuration instance</param>
    /// <param name="sectionKey">The configuration section key (default: "RabbitMQ")</param>
    /// <returns>The service collection for fluent configuration</returns>
    /// <exception cref="ArgumentNullException">Thrown when configuration is null</exception>
    /// <remarks>
    /// This method configures RabbitMQ services from appsettings.json or other configuration
    /// sources. It supports automatic configuration binding and validation.
    /// </remarks>
    /// <example>
    /// <code>
    /// services.ConfigureRabbitMQ(configuration, "MessageBroker:RabbitMQ");
    /// </code>
    /// </example>
    public static IServiceCollection ConfigureRabbitMQ(this IServiceCollection services, IConfiguration configuration, string sectionKey = "RabbitMQ")
    {
        ArgumentNullException.ThrowIfNull(configuration);

        services.Configure<RabbitMQConfiguration>(options =>
        {
            var section = configuration.GetSection(sectionKey);
            section.Bind(options);
        });
        services.AddSingleton<IValidateOptions<RabbitMQConfiguration>, RabbitMQConfigurationValidator>();
        
        return services;
    }

    /// <summary>
    /// Configures RabbitMQ services with validation
    /// </summary>
    /// <param name="services">The service collection</param>
    /// <param name="configure">Action to configure RabbitMQ options</param>
    /// <param name="validator">Custom validator for configuration</param>
    /// <returns>The service collection for fluent configuration</returns>
    /// <exception cref="ArgumentNullException">Thrown when configure action or validator is null</exception>
    /// <remarks>
    /// This method configures RabbitMQ services with custom validation logic. It allows
    /// for complex validation scenarios that go beyond attribute-based validation.
    /// </remarks>
    /// <example>
    /// <code>
    /// services.ConfigureRabbitMQWithValidation(
    ///     options => options.Connection.ConnectionString = "amqp://localhost",
    ///     config => config.Connection.Port > 0 ? ValidateOptionsResult.Success : ValidateOptionsResult.Fail("Invalid port"));
    /// </code>
    /// </example>
    public static IServiceCollection ConfigureRabbitMQWithValidation(
        this IServiceCollection services, 
        Action<RabbitMQConfiguration> configure,
        Func<RabbitMQConfiguration, ValidateOptionsResult> validator)
    {
        ArgumentNullException.ThrowIfNull(configure);
        ArgumentNullException.ThrowIfNull(validator);

        services.Configure<RabbitMQConfiguration>(configure);
        services.AddSingleton<IValidateOptions<RabbitMQConfiguration>>(provider =>
            new CustomRabbitMQConfigurationValidator(validator));

        return services;
    }

    #endregion

    #region Named Options Support

    /// <summary>
    /// Configures named RabbitMQ options for multi-tenant scenarios
    /// </summary>
    /// <param name="services">The service collection</param>
    /// <param name="name">The options name</param>
    /// <param name="configure">Action to configure RabbitMQ options</param>
    /// <returns>The service collection for fluent configuration</returns>
    /// <exception cref="ArgumentNullException">Thrown when name or configure action is null</exception>
    /// <remarks>
    /// This method enables multi-tenant scenarios where different RabbitMQ configurations
    /// are needed for different clients or environments within the same application.
    /// </remarks>
    /// <example>
    /// <code>
    /// services.ConfigureRabbitMQ("tenant1", options =>
    /// {
    ///     options.Connection.ConnectionString = "amqp://tenant1-server";
    /// });
    /// services.ConfigureRabbitMQ("tenant2", options =>
    /// {
    ///     options.Connection.ConnectionString = "amqp://tenant2-server";
    /// });
    /// </code>
    /// </example>
    public static IServiceCollection ConfigureRabbitMQ(this IServiceCollection services, string name, Action<RabbitMQConfiguration> configure)
    {
        ArgumentNullException.ThrowIfNull(name);
        ArgumentNullException.ThrowIfNull(configure);

        services.Configure<RabbitMQConfiguration>(name, configure);
        services.AddSingleton<IValidateOptions<RabbitMQConfiguration>>(provider =>
            new RabbitMQConfigurationValidator(name));

        return services;
    }

    /// <summary>
    /// Configures named RabbitMQ options from IConfiguration
    /// </summary>
    /// <param name="services">The service collection</param>
    /// <param name="name">The options name</param>
    /// <param name="configuration">The configuration instance</param>
    /// <param name="sectionKey">The configuration section key</param>
    /// <returns>The service collection for fluent configuration</returns>
    /// <exception cref="ArgumentNullException">Thrown when name or configuration is null</exception>
    /// <remarks>
    /// This method configures named RabbitMQ options from configuration sources,
    /// enabling multi-tenant scenarios with configuration-based setup.
    /// </remarks>
    /// <example>
    /// <code>
    /// services.ConfigureRabbitMQ("tenant1", configuration, "Tenants:Tenant1:RabbitMQ");
    /// services.ConfigureRabbitMQ("tenant2", configuration, "Tenants:Tenant2:RabbitMQ");
    /// </code>
    /// </example>
    public static IServiceCollection ConfigureRabbitMQ(this IServiceCollection services, string name, IConfiguration configuration, string sectionKey)
    {
        ArgumentNullException.ThrowIfNull(name);
        ArgumentNullException.ThrowIfNull(configuration);

        services.Configure<RabbitMQConfiguration>(name, options =>
        {
            var section = configuration.GetSection(sectionKey);
            section.Bind(options);
        });
        services.AddSingleton<IValidateOptions<RabbitMQConfiguration>>(provider =>
            new RabbitMQConfigurationValidator(name));

        return services;
    }

    #endregion

    #region Environment-Specific Configuration

    /// <summary>
    /// Configures RabbitMQ with environment-specific settings
    /// </summary>
    /// <param name="services">The service collection</param>
    /// <param name="environment">The hosting environment</param>
    /// <param name="configure">Action to configure RabbitMQ options</param>
    /// <returns>The service collection for fluent configuration</returns>
    /// <exception cref="ArgumentNullException">Thrown when environment or configure action is null</exception>
    /// <remarks>
    /// This method applies environment-specific configuration overrides based on the
    /// hosting environment (Development, Staging, Production).
    /// </remarks>
    /// <example>
    /// <code>
    /// services.ConfigureRabbitMQForEnvironment(environment, options =>
    /// {
    ///     options.Connection.ConnectionString = "amqp://localhost";
    ///     // Environment-specific settings will be applied automatically
    /// });
    /// </code>
    /// </example>
    public static IServiceCollection ConfigureRabbitMQForEnvironment(
        this IServiceCollection services, 
        IHostEnvironment environment, 
        Action<RabbitMQConfiguration> configure)
    {
        ArgumentNullException.ThrowIfNull(environment);
        ArgumentNullException.ThrowIfNull(configure);

        services.Configure<RabbitMQConfiguration>(options =>
        {
            configure(options);
            ApplyEnvironmentSpecificSettings(options, environment);
        });

        services.AddSingleton<IValidateOptions<RabbitMQConfiguration>, RabbitMQConfigurationValidator>();

        return services;
    }

    /// <summary>
    /// Configures RabbitMQ for development environment
    /// </summary>
    /// <param name="services">The service collection</param>
    /// <param name="configure">Action to configure RabbitMQ options</param>
    /// <returns>The service collection for fluent configuration</returns>
    /// <exception cref="ArgumentNullException">Thrown when configure action is null</exception>
    /// <remarks>
    /// This method applies development-specific configuration settings including
    /// extended timeouts, verbose logging, and development-friendly defaults.
    /// </remarks>
    /// <example>
    /// <code>
    /// services.ConfigureRabbitMQForDevelopment(options =>
    /// {
    ///     options.Connection.ConnectionString = "amqp://localhost";
    /// });
    /// </code>
    /// </example>
    public static IServiceCollection ConfigureRabbitMQForDevelopment(this IServiceCollection services, Action<RabbitMQConfiguration> configure)
    {
        ArgumentNullException.ThrowIfNull(configure);

        return services.Configure<RabbitMQConfiguration>(options =>
        {
            configure(options);
            ApplyDevelopmentSettings(options);
        });
    }

    /// <summary>
    /// Configures RabbitMQ for production environment
    /// </summary>
    /// <param name="services">The service collection</param>
    /// <param name="configure">Action to configure RabbitMQ options</param>
    /// <returns>The service collection for fluent configuration</returns>
    /// <exception cref="ArgumentNullException">Thrown when configure action is null</exception>
    /// <remarks>
    /// This method applies production-specific configuration settings including
    /// performance optimizations, security settings, and production-safe defaults.
    /// </remarks>
    /// <example>
    /// <code>
    /// services.ConfigureRabbitMQForProduction(options =>
    /// {
    ///     options.Connection.ConnectionString = Environment.GetEnvironmentVariable("RABBITMQ_CONNECTION_STRING");
    /// });
    /// </code>
    /// </example>
    public static IServiceCollection ConfigureRabbitMQForProduction(this IServiceCollection services, Action<RabbitMQConfiguration> configure)
    {
        ArgumentNullException.ThrowIfNull(configure);

        return services.Configure<RabbitMQConfiguration>(options =>
        {
            configure(options);
            ApplyProductionSettings(options);
        });
    }

    #endregion

    #region Configuration Validation

    /// <summary>
    /// Validates RabbitMQ configuration with custom validation logic
    /// </summary>
    /// <param name="services">The service collection</param>
    /// <param name="validator">Custom validator function</param>
    /// <returns>The service collection for fluent configuration</returns>
    /// <exception cref="ArgumentNullException">Thrown when validator is null</exception>
    /// <remarks>
    /// This method adds custom validation logic to RabbitMQ configuration. The validator
    /// function receives the configuration and should return a list of validation errors.
    /// </remarks>
    /// <example>
    /// <code>
    /// services.ValidateRabbitMQConfiguration(config =>
    /// {
    ///     var errors = new List&lt;string&gt;();
    ///     if (config.Connection.HostName == "localhost" &amp;&amp; Environment.IsProduction)
    ///         errors.Add("Cannot use localhost in production");
    ///     return errors;
    /// });
    /// </code>
    /// </example>
    public static IServiceCollection ValidateRabbitMQConfiguration(
        this IServiceCollection services,
        Func<RabbitMQConfiguration, IEnumerable<string>> validator)
    {
        ArgumentNullException.ThrowIfNull(services);
        ArgumentNullException.ThrowIfNull(validator);

        services.AddSingleton<IValidateOptions<RabbitMQConfiguration>>(provider =>
            new CustomRabbitMQConfigurationValidator(config =>
            {
                var errors = validator(config).ToList();
                return errors.Any() 
                    ? ValidateOptionsResult.Fail(errors)
                    : ValidateOptionsResult.Success;
            }));

        return services;
    }

    /// <summary>
    /// Validates RabbitMQ configuration on startup
    /// </summary>
    /// <param name="services">The service collection</param>
    /// <param name="throwOnValidationFailure">Whether to throw exception on validation failure</param>
    /// <returns>The service collection for fluent configuration</returns>
    /// <remarks>
    /// This method validates RabbitMQ configuration during application startup and
    /// optionally throws an exception if validation fails.
    /// </remarks>
    /// <example>
    /// <code>
    /// services.ValidateRabbitMQConfigurationOnStartup(throwOnValidationFailure: true);
    /// </code>
    /// </example>
    public static IServiceCollection ValidateRabbitMQConfigurationOnStartup(
        this IServiceCollection services,
        bool throwOnValidationFailure = true)
    {
        services.AddSingleton<IValidateOptions<RabbitMQConfiguration>, RabbitMQConfigurationValidator>();
        
        if (throwOnValidationFailure)
        {
            services.AddSingleton<IOptionsFactory<RabbitMQConfiguration>>(provider =>
                new ValidatingOptionsFactory<RabbitMQConfiguration>(
                    provider.GetRequiredService<IEnumerable<IConfigureOptions<RabbitMQConfiguration>>>(),
                    provider.GetRequiredService<IEnumerable<IPostConfigureOptions<RabbitMQConfiguration>>>(),
                    provider.GetRequiredService<IEnumerable<IValidateOptions<RabbitMQConfiguration>>>()));
        }

        return services;
    }

    #endregion

    #region Configuration Change Monitoring

    /// <summary>
    /// Enables configuration change monitoring for RabbitMQ settings
    /// </summary>
    /// <param name="services">The service collection</param>
    /// <param name="onChange">Action to execute when configuration changes</param>
    /// <returns>The service collection for fluent configuration</returns>
    /// <exception cref="ArgumentNullException">Thrown when onChange action is null</exception>
    /// <remarks>
    /// This method enables monitoring of RabbitMQ configuration changes and executes
    /// a callback when changes are detected. This is useful for hot-reloading configuration.
    /// </remarks>
    /// <example>
    /// <code>
    /// services.MonitorRabbitMQConfigurationChanges(config =>
    /// {
    ///     Console.WriteLine("RabbitMQ configuration changed");
    ///     // Restart connections, update settings, etc.
    /// });
    /// </code>
    /// </example>
    public static IServiceCollection MonitorRabbitMQConfigurationChanges(
        this IServiceCollection services,
        Action<RabbitMQConfiguration> onChange)
    {
        ArgumentNullException.ThrowIfNull(onChange);

        services.AddSingleton<IOptionsMonitor<RabbitMQConfiguration>>(provider =>
        {
            var monitor = provider.GetRequiredService<IOptionsMonitor<RabbitMQConfiguration>>();
            monitor.OnChange(onChange);
            return monitor;
        });

        return services;
    }

    /// <summary>
    /// Enables configuration change monitoring with named options
    /// </summary>
    /// <param name="services">The service collection</param>
    /// <param name="name">The options name</param>
    /// <param name="onChange">Action to execute when configuration changes</param>
    /// <returns>The service collection for fluent configuration</returns>
    /// <exception cref="ArgumentNullException">Thrown when name or onChange action is null</exception>
    /// <remarks>
    /// This method enables monitoring of named RabbitMQ configuration changes,
    /// useful for multi-tenant scenarios where different configurations need monitoring.
    /// </remarks>
    /// <example>
    /// <code>
    /// services.MonitorRabbitMQConfigurationChanges("tenant1", config =>
    /// {
    ///     Console.WriteLine("Tenant1 RabbitMQ configuration changed");
    /// });
    /// </code>
    /// </example>
    public static IServiceCollection MonitorRabbitMQConfigurationChanges(
        this IServiceCollection services,
        string name,
        Action<RabbitMQConfiguration> onChange)
    {
        ArgumentNullException.ThrowIfNull(name);
        ArgumentNullException.ThrowIfNull(onChange);

        services.AddSingleton<IOptionsMonitor<RabbitMQConfiguration>>(provider =>
        {
            var monitor = provider.GetRequiredService<IOptionsMonitor<RabbitMQConfiguration>>();
            monitor.OnChange((config, configName) =>
            {
                if (configName == name)
                {
                    onChange(config);
                }
            });
            return monitor;
        });

        return services;
    }

    #endregion

    #region Private Helper Methods

    /// <summary>
    /// Applies environment-specific settings to RabbitMQ configuration
    /// </summary>
    /// <param name="options">The RabbitMQ configuration</param>
    /// <param name="environment">The hosting environment</param>
    private static void ApplyEnvironmentSpecificSettings(RabbitMQConfiguration options, IHostEnvironment environment)
    {
        if (environment.IsDevelopment())
        {
            ApplyDevelopmentSettings(options);
        }
        else if (environment.IsProduction())
        {
            ApplyProductionSettings(options);
        }
        else
        {
            ApplyTestingSettings(options);
        }
    }

    /// <summary>
    /// Applies development-specific settings
    /// </summary>
    /// <param name="options">The RabbitMQ configuration</param>
    private static void ApplyDevelopmentSettings(RabbitMQConfiguration options)
    {
        options.Connection.RequestedHeartbeat = TimeSpan.FromMinutes(10);
        options.Connection.ConnectionTimeout = TimeSpan.FromMinutes(1);
        options.Producer.ConfirmationTimeout = TimeSpan.FromMinutes(1);
        options.Consumer.PrefetchCount = 1;
        options.RetryPolicy.MaxRetries = 3;
        options.HealthCheck.Enabled = true;
        options.Monitoring.Enabled = true;
    }

    /// <summary>
    /// Applies production-specific settings
    /// </summary>
    /// <param name="options">The RabbitMQ configuration</param>
    private static void ApplyProductionSettings(RabbitMQConfiguration options)
    {
        options.Connection.RequestedHeartbeat = TimeSpan.FromMinutes(1);
        options.Connection.ConnectionTimeout = TimeSpan.FromSeconds(30);
        options.Producer.ConfirmationTimeout = TimeSpan.FromSeconds(30);
        options.Consumer.PrefetchCount = 10;
        options.RetryPolicy.MaxRetries = 5;
        options.HealthCheck.Enabled = true;
        options.Monitoring.Enabled = true;
        options.ErrorHandling.Strategy = ErrorHandlingStrategy.DeadLetter.ToString();
    }

    /// <summary>
    /// Applies testing-specific settings
    /// </summary>
    /// <param name="options">The RabbitMQ configuration</param>
    private static void ApplyTestingSettings(RabbitMQConfiguration options)
    {
        options.Connection.RequestedHeartbeat = TimeSpan.FromSeconds(30);
        options.Connection.ConnectionTimeout = TimeSpan.FromSeconds(10);
        options.Producer.ConfirmationTimeout = TimeSpan.FromSeconds(5);
        options.Consumer.PrefetchCount = 1;
        options.RetryPolicy.MaxRetries = 1;
        options.HealthCheck.Enabled = false;
        options.Monitoring.Enabled = false;
    }

    #endregion
}

/// <summary>
/// Validator for RabbitMQ configuration
/// </summary>
/// <remarks>
/// This class provides validation for RabbitMQ configuration using data annotations
/// and custom validation logic.
/// </remarks>
internal class RabbitMQConfigurationValidator : IValidateOptions<RabbitMQConfiguration>
{
    private readonly string? _name;

    /// <summary>
    /// Initializes a new instance of the RabbitMQConfigurationValidator
    /// </summary>
    /// <param name="name">Optional name for named options</param>
    public RabbitMQConfigurationValidator(string? name = null)
    {
        _name = name;
    }

    /// <summary>
    /// Validates the RabbitMQ configuration
    /// </summary>
    /// <param name="name">The options name</param>
    /// <param name="options">The configuration options to validate</param>
    /// <returns>Validation result</returns>
    public ValidateOptionsResult Validate(string? name, RabbitMQConfiguration options)
    {
        // Skip validation if this is for a different named option
        if (_name != null && _name != name)
        {
            return ValidateOptionsResult.Skip;
        }

        var errors = new List<string>();

        // Validate connection settings
        if (string.IsNullOrWhiteSpace(options.Connection.ConnectionString) && string.IsNullOrWhiteSpace(options.Connection.HostName))
        {
            errors.Add("Either ConnectionString or HostName must be provided");
        }

        if (options.Connection.Port < 1 || options.Connection.Port > 65535)
        {
            errors.Add("Port must be between 1 and 65535");
        }

        if (options.Connection.RequestedHeartbeat < TimeSpan.Zero)
        {
            errors.Add("RequestedHeartbeat cannot be negative");
        }

        if (options.Connection.ConnectionTimeout < TimeSpan.Zero)
        {
            errors.Add("ConnectionTimeout cannot be negative");
        }

        // Validate producer settings
        if (options.Producer.ConfirmationTimeout < TimeSpan.Zero)
        {
            errors.Add("Producer ConfirmationTimeout cannot be negative");
        }

        // Validate consumer settings
        if (options.Consumer.PrefetchCount < 0)
        {
            errors.Add("Consumer PrefetchCount cannot be negative");
        }

        if (options.Consumer.ConcurrentConsumers < 1)
        {
            errors.Add("ConcurrentConsumers must be at least 1");
        }

        // Validate retry policy settings
        if (options.RetryPolicy.MaxRetries < 0)
        {
            errors.Add("MaxRetries cannot be negative");
        }

        if (options.RetryPolicy.InitialDelay < TimeSpan.Zero)
        {
            errors.Add("InitialDelay cannot be negative");
        }

        if (options.RetryPolicy.MaxDelay < TimeSpan.Zero)
        {
            errors.Add("MaxDelay cannot be negative");
        }

        if (options.RetryPolicy.Multiplier <= 0)
        {
            errors.Add("Multiplier must be greater than 0");
        }

        return errors.Any() ? ValidateOptionsResult.Fail(errors) : ValidateOptionsResult.Success;
    }
}

/// <summary>
/// Custom validator for RabbitMQ configuration
/// </summary>
/// <remarks>
/// This class provides custom validation for RabbitMQ configuration using a
/// user-provided validation function.
/// </remarks>
internal class CustomRabbitMQConfigurationValidator : IValidateOptions<RabbitMQConfiguration>
{
    private readonly Func<RabbitMQConfiguration, ValidateOptionsResult> _validator;

    /// <summary>
    /// Initializes a new instance of the CustomRabbitMQConfigurationValidator
    /// </summary>
    /// <param name="validator">The validation function</param>
    public CustomRabbitMQConfigurationValidator(Func<RabbitMQConfiguration, ValidateOptionsResult> validator)
    {
        _validator = validator ?? throw new ArgumentNullException(nameof(validator));
    }

    /// <summary>
    /// Validates the RabbitMQ configuration using the custom validator
    /// </summary>
    /// <param name="name">The options name</param>
    /// <param name="options">The configuration options to validate</param>
    /// <returns>Validation result</returns>
    public ValidateOptionsResult Validate(string? name, RabbitMQConfiguration options)
    {
        return _validator(options);
    }
}

/// <summary>
/// Validating options factory that validates options on creation
/// </summary>
/// <typeparam name="TOptions">The options type</typeparam>
internal class ValidatingOptionsFactory<TOptions> : IOptionsFactory<TOptions>
    where TOptions : class
{
    private readonly IEnumerable<IConfigureOptions<TOptions>> _configureOptions;
    private readonly IEnumerable<IPostConfigureOptions<TOptions>> _postConfigureOptions;
    private readonly IEnumerable<IValidateOptions<TOptions>> _validateOptions;

    /// <summary>
    /// Initializes a new instance of the ValidatingOptionsFactory
    /// </summary>
    public ValidatingOptionsFactory(
        IEnumerable<IConfigureOptions<TOptions>> configureOptions,
        IEnumerable<IPostConfigureOptions<TOptions>> postConfigureOptions,
        IEnumerable<IValidateOptions<TOptions>> validateOptions)
    {
        _configureOptions = configureOptions;
        _postConfigureOptions = postConfigureOptions;
        _validateOptions = validateOptions;
    }

    /// <summary>
    /// Creates and validates options
    /// </summary>
    /// <param name="name">The options name</param>
    /// <returns>The validated options</returns>
    public TOptions Create(string name)
    {
        var options = Activator.CreateInstance<TOptions>();

        // Configure options
        foreach (var configure in _configureOptions)
        {
            if (configure is IConfigureNamedOptions<TOptions> namedConfigure)
            {
                namedConfigure.Configure(name, options);
            }
            else
            {
                configure.Configure(options);
            }
        }

        // Post-configure options
        foreach (var postConfigure in _postConfigureOptions)
        {
            postConfigure.PostConfigure(name, options);
        }

        // Validate options
        foreach (var validate in _validateOptions)
        {
            var result = validate.Validate(name, options);
            if (result.Failed)
            {
                throw new OptionsValidationException(name, typeof(TOptions), result.Failures);
            }
        }

        return options;
    }
} 