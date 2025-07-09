using RabbitMQ.Client;

namespace FS.Transport.AMQP.Core.Extensions;

/// <summary>
/// Extension methods for RabbitMQ IModel
/// </summary>
public static class ModelExtensions
{
    /// <summary>
    /// Safely closes a channel/model, ignoring any exceptions
    /// </summary>
    /// <param name="model">Channel/model to close</param>
    public static void SafeClose(this IModel? model)
    {
        if (model == null || model.IsClosed)
            return;

        try
        {
            if (model.IsOpen)
            {
                model.Close();
            }
        }
        catch
        {
            // Ignore exceptions during close
        }
    }

    /// <summary>
    /// Safely disposes a channel/model, ignoring any exceptions
    /// </summary>
    /// <param name="model">Channel/model to dispose</param>
    public static void SafeDispose(this IModel? model)
    {
        if (model == null)
            return;

        try
        {
            model.Dispose();
        }
        catch
        {
            // Ignore exceptions during dispose
        }
    }

    /// <summary>
    /// Checks if the model is usable (not null, open, and not closing)
    /// </summary>
    /// <param name="model">Channel/model to check</param>
    /// <returns>True if the model is usable</returns>
    public static bool IsUsable(this IModel? model)
    {
        return model != null && model.IsOpen && !model.IsClosed;
    }
}