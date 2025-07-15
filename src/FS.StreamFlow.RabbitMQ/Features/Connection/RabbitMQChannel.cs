using FS.StreamFlow.Core.Features.Messaging.Models;
using Microsoft.Extensions.Logging;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using RabbitMqChannel = RabbitMQ.Client.IChannel;

namespace FS.StreamFlow.RabbitMQ.Features.Connection;

/// <summary>
/// RabbitMQ implementation of the channel interface.
/// Wraps RabbitMQ.Client.IChannel to provide provider-agnostic channel operations.
/// </summary>
public class RabbitMQChannel : FS.StreamFlow.Core.Features.Messaging.Models.IChannel
{
    private readonly RabbitMqChannel _channel;
    private readonly ILogger _logger;
    private readonly ChannelStatistics _statistics;
    private volatile ChannelState _state = ChannelState.Open;
    private volatile bool _disposed;

    /// <summary>
    /// Gets the channel identifier.
    /// </summary>
    public string Id => _statistics.ChannelId;

    /// <summary>
    /// Gets the current channel state.
    /// </summary>
    public ChannelState State => _state;

    /// <summary>
    /// Gets a value indicating whether the channel is open and ready for operations.
    /// </summary>
    public bool IsOpen => _state == ChannelState.Open && _channel?.IsOpen == true;

    /// <summary>
    /// Gets channel statistics including operation counts and performance metrics.
    /// </summary>
    public ChannelStatistics Statistics => _statistics;

    /// <summary>
    /// Event raised when channel state changes.
    /// </summary>
    public event EventHandler<ChannelStateChangedEventArgs>? StateChanged;

    /// <summary>
    /// Event raised when the channel is closed.
    /// </summary>
    public event EventHandler<ChannelEventArgs>? Closed;

    /// <summary>
    /// Initializes a new instance of the <see cref="RabbitMQChannel"/> class.
    /// </summary>
    /// <param name="channel">The RabbitMQ channel from the new API.</param>
    /// <param name="logger">The logger instance.</param>
    public RabbitMQChannel(RabbitMqChannel channel, ILogger logger)
    {
        _channel = channel ?? throw new ArgumentNullException(nameof(channel));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _statistics = new ChannelStatistics
        {
            ChannelId = Guid.NewGuid().ToString(),
            CreatedAt = DateTimeOffset.UtcNow,
            CurrentState = ChannelState.Open
        };

        // Subscribe to channel events
        _channel.ChannelShutdownAsync += OnChannelShutdownAsync;
    }

    /// <summary>
    /// Closes the channel asynchronously.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    public async Task CloseAsync(CancellationToken cancellationToken = default)
    {
        if (_disposed || _state == ChannelState.Closed)
            return;

        try
        {
            ChangeState(ChannelState.Closing);
            await _channel.CloseAsync(cancellationToken);
            ChangeState(ChannelState.Closed);
            
            Closed?.Invoke(this, new ChannelEventArgs(_statistics.ChannelId, ChannelState.Closed));
            _logger.LogInformation("Channel {ChannelId} closed successfully", _statistics.ChannelId);
        }
        catch (Exception ex)
        {
            ChangeState(ChannelState.Error);
            _logger.LogError(ex, "Error closing channel {ChannelId}", _statistics.ChannelId);
            throw;
        }
    }

    /// <summary>
    /// Performs a basic operation to test channel health.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>True if the channel is healthy, otherwise false.</returns>
    public Task<bool> HealthCheckAsync(CancellationToken cancellationToken = default)
    {
        if (_disposed || _channel == null)
            return Task.FromResult(false);

        try
        {
            // Simple health check - verify the channel is open and responsive
            return Task.FromResult(_channel.IsOpen);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Health check failed for channel {ChannelId}", _statistics.ChannelId);
            return Task.FromResult(false);
        }
    }

    /// <summary>
    /// Gets the underlying RabbitMQ channel for advanced operations.
    /// </summary>
    /// <returns>The RabbitMQ channel.</returns>
    public RabbitMqChannel GetNativeChannel()
    {
        if (_disposed)
            throw new ObjectDisposedException(nameof(RabbitMQChannel));

        return _channel;
    }

    /// <summary>
    /// Releases all resources used by the channel.
    /// </summary>
    public void Dispose()
    {
        if (_disposed)
            return;

        _disposed = true;

        try
        {
            if (_channel != null)
            {
                _channel.ChannelShutdownAsync -= OnChannelShutdownAsync;
                _channel.Dispose();
            }

            ChangeState(ChannelState.Closed);
            _logger.LogInformation("RabbitMQ channel {ChannelId} disposed", _statistics.ChannelId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during channel {ChannelId} disposal", _statistics.ChannelId);
        }
    }

    private void ChangeState(ChannelState newState)
    {
        var previousState = _state;
        _state = newState;
        _statistics.CurrentState = newState;

        if (previousState != newState)
        {
            StateChanged?.Invoke(this, new ChannelStateChangedEventArgs(
                _statistics.ChannelId, previousState, newState));
        }
    }

    private async Task OnChannelShutdownAsync(object? sender, ShutdownEventArgs e)
    {
        _logger.LogWarning("Channel {ChannelId} shutdown: {Reason}", _statistics.ChannelId, e.ReplyText);
        ChangeState(ChannelState.Closed);
        Closed?.Invoke(this, new ChannelEventArgs(_statistics.ChannelId, ChannelState.Closed, e.ReplyText));
        await Task.CompletedTask;
    }
} 