using System.Globalization;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace ChaoDesi.API.Serialization;

public sealed class EmptyStringToNullableDateTimeConverter : JsonConverter<DateTime?>
{
    public override DateTime? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        if (reader.TokenType == JsonTokenType.Null)
        {
            return null;
        }

        if (reader.TokenType != JsonTokenType.String)
        {
            throw new JsonException("Invalid date value. Expected string or null.");
        }

        var rawValue = reader.GetString();

        if (string.IsNullOrWhiteSpace(rawValue))
        {
            return null;
        }

        if (DateTime.TryParse(rawValue, CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind, out var parsedUtc))
        {
            return parsedUtc;
        }

        if (DateTime.TryParse(rawValue, CultureInfo.CurrentCulture, DateTimeStyles.AssumeLocal, out var parsedLocal))
        {
            return parsedLocal;
        }

        throw new JsonException($"Invalid date value '{rawValue}'. Use ISO date format or null.");
    }

    public override void Write(Utf8JsonWriter writer, DateTime? value, JsonSerializerOptions options)
    {
        if (value.HasValue)
        {
            writer.WriteStringValue(value.Value);
            return;
        }

        writer.WriteNullValue();
    }
}
