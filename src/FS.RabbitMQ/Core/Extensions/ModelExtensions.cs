using RabbitMQ.Client;

namespace FS.RabbitMQ.Core.Extensions;

/// <summary>
/// Extension methods for RabbitMQ IChannel
/// </summary>
public static class ModelExtensions
{
    /// <summary>
    /// Safely closes an IChannel, handling any exceptions that may occur
    /// </summary>
    /// <param name="channel">The channel to close</param>
    public static async Task SafeCloseAsync(this IChannel? channel)
    {
        if (channel == null || !channel.IsOpen)
            return;

        try
        {
            await channel.CloseAsync();
        }
        catch (Exception ex)
        {
            // Log the exception but don't rethrow
            // In a production environment, you'd want to use a proper logger
            Console.WriteLine($"Error closing channel: {ex.Message}");
        }
    }

    /// <summary>
    /// Safely disposes a RabbitMQ channel without throwing exceptions
    /// </summary>
    /// <param name="channel">The channel to dispose</param>
    public static void SafeDispose(this IChannel? channel)
    {
        if (channel is null) return;
        
        try
        {
            channel.Dispose();
        }
        catch (Exception ex)
        {
            // Log the exception if logger is available
            Console.WriteLine($"Error disposing channel: {ex.Message}");
        }
    }

    /// <summary>
    /// Checks if a channel is usable (open and not disposed)
    /// </summary>
    /// <param name="channel">The channel to check</param>
    /// <returns>True if the channel is usable, false otherwise</returns>
    public static bool IsUsable(this IChannel? channel)
    {
        return channel is not null && channel.IsOpen;
    }
}