using System.Text.Json;
using System.Text.Json.Serialization;

namespace FS.RabbitMQ.Serialization;

/// <summary>
/// Custom JSON converter for TimeSpan with ISO 8601 duration format
/// </summary>
public class TimeSpanConverter : JsonConverter<TimeSpan>
{
    /// <inheritdoc />
    public override TimeSpan Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        return TimeSpan.Parse(reader.GetString()!);
    }

    /// <inheritdoc />
    public override void Write(Utf8JsonWriter writer, TimeSpan value, JsonSerializerOptions options)
    {
        writer.WriteStringValue(value.ToString("c")); // Constant format
    }
}