using System.Text;

namespace FS.Transport.AMQP.Configuration;

/// <summary>
/// Builder for constructing RabbitMQ AMQP connection strings
/// </summary>
public class ConnectionStringBuilder
{
    private readonly ConnectionSettings _settings;

    public ConnectionStringBuilder(ConnectionSettings settings)
    {
        _settings = settings ?? throw new ArgumentNullException(nameof(settings));
    }

    /// <summary>
    /// Builds an AMQP connection string from the connection settings
    /// </summary>
    /// <returns>AMQP connection string</returns>
    public string Build()
    {
        var protocol = _settings.UseSsl ? "amqps" : "amqp";
        var port = _settings.Port;
        
        // Use default ports if not specified
        if (port == 0)
        {
            port = _settings.UseSsl ? 5671 : 5672;
        }

        var sb = new StringBuilder();
        sb.Append($"{protocol}://");
        
        // Add credentials if provided
        if (!string.IsNullOrEmpty(_settings.UserName))
        {
            sb.Append(Uri.EscapeDataString(_settings.UserName));
            
            if (!string.IsNullOrEmpty(_settings.Password))
            {
                sb.Append($":{Uri.EscapeDataString(_settings.Password)}");
            }
            
            sb.Append("@");
        }
        
        // Add host and port
        sb.Append(_settings.HostName);
        if (port != (_settings.UseSsl ? 5671 : 5672))
        {
            sb.Append($":{port}");
        }
        
        // Add virtual host
        if (!string.IsNullOrEmpty(_settings.VirtualHost) && _settings.VirtualHost != "/")
        {
            sb.Append($"/{Uri.EscapeDataString(_settings.VirtualHost)}");
        }

        return sb.ToString();
    }

    /// <summary>
    /// Builds a connection string with query parameters
    /// </summary>
    /// <param name="includeQueryParameters">Whether to include query parameters</param>
    /// <returns>AMQP connection string with optional query parameters</returns>
    public string Build(bool includeQueryParameters)
    {
        var connectionString = Build();
        
        if (!includeQueryParameters)
            return connectionString;

        var queryParams = new List<string>();
        
        if (_settings.HeartbeatInterval != 60)
        {
            queryParams.Add($"heartbeat={_settings.HeartbeatInterval}");
        }
        
        if (_settings.ConnectionTimeoutMs != 30000)
        {
            queryParams.Add($"connection_timeout={_settings.ConnectionTimeoutMs}");
        }
        
        if (_settings.RequestedChannelMax != 2047)
        {
            queryParams.Add($"channel_max={_settings.RequestedChannelMax}");
        }
        
        if (_settings.RequestedFrameMax != 0)
        {
            queryParams.Add($"frame_max={_settings.RequestedFrameMax}");
        }

        if (queryParams.Any())
        {
            connectionString += "?" + string.Join("&", queryParams);
        }

        return connectionString;
    }

    /// <summary>
    /// Parses an AMQP connection string and returns connection settings
    /// </summary>
    /// <param name="connectionString">AMQP connection string</param>
    /// <returns>Parsed connection settings</returns>
    public static ConnectionSettings Parse(string connectionString)
    {
        if (string.IsNullOrWhiteSpace(connectionString))
            throw new ArgumentException("Connection string cannot be null or empty");

        var uri = new Uri(connectionString);
        var settings = new ConnectionSettings();

        // Parse protocol
        settings.UseSsl = uri.Scheme.Equals("amqps", StringComparison.OrdinalIgnoreCase);
        
        // Parse host and port
        settings.HostName = uri.Host;
        settings.Port = uri.Port != -1 ? uri.Port : (settings.UseSsl ? 5671 : 5672);
        
        // Parse credentials
        if (!string.IsNullOrEmpty(uri.UserInfo))
        {
            var userInfo = uri.UserInfo.Split(':');
            settings.UserName = Uri.UnescapeDataString(userInfo[0]);
            if (userInfo.Length > 1)
            {
                settings.Password = Uri.UnescapeDataString(userInfo[1]);
            }
        }
        
        // Parse virtual host
        if (!string.IsNullOrEmpty(uri.AbsolutePath) && uri.AbsolutePath != "/")
        {
            settings.VirtualHost = Uri.UnescapeDataString(uri.AbsolutePath.TrimStart('/'));
        }
        
        // Parse query parameters
        if (!string.IsNullOrEmpty(uri.Query))
        {
            var queryParams = System.Web.HttpUtility.ParseQueryString(uri.Query);
            
            if (queryParams["heartbeat"] != null && ushort.TryParse(queryParams["heartbeat"], out var heartbeat))
            {
                settings.HeartbeatInterval = heartbeat;
            }
            
            if (queryParams["connection_timeout"] != null && int.TryParse(queryParams["connection_timeout"], out var timeout))
            {
                settings.ConnectionTimeoutMs = timeout;
            }
            
            if (queryParams["channel_max"] != null && ushort.TryParse(queryParams["channel_max"], out var channelMax))
            {
                settings.RequestedChannelMax = channelMax;
            }
            
            if (queryParams["frame_max"] != null && uint.TryParse(queryParams["frame_max"], out var frameMax))
            {
                settings.RequestedFrameMax = frameMax;
            }
        }

        return settings;
    }
}