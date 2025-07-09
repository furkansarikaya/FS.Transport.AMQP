using System.Text.Json;
using System.Text.Json.Serialization;

namespace FS.RabbitMQ.Serialization;

// Custom JSON converters for better serialization
/// <summary>
/// Custom JSON converter for DateTimeOffset with ISO 8601 format
/// </summary>
public class DateTimeOffsetConverter : JsonConverter<DateTimeOffset>
{
    /// <inheritdoc />
    public override DateTimeOffset Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        return DateTimeOffset.Parse(reader.GetString()!);
    }

    /// <inheritdoc />
    public override void Write(Utf8JsonWriter writer, DateTimeOffset value, JsonSerializerOptions options)
    {
        writer.WriteStringValue(value.ToString("O")); // ISO 8601 format
    }
}