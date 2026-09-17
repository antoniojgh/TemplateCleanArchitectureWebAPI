using System.Text.Json;
using System.Text.Json.Serialization;

namespace DientesLimpios.API.Json
{
    // The API's time contract: every incoming instant is ISO 8601 with an explicit offset or
    // "Z", and every outgoing one is UTC with "Z". Inside the application, DateTime is UTC.
    public sealed class UtcDateTimeJsonConverter : JsonConverter<DateTime>
    {
        public override DateTime Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            // TryGetDateTime reports whether the text carried an offset (Kind != Unspecified).
            // TryGetDateTimeOffset then gives the exact instant, without a detour through the
            // server's own time zone.
            if (reader.TokenType != JsonTokenType.String
                || !reader.TryGetDateTime(out var withKind)
                || withKind.Kind == DateTimeKind.Unspecified
                || !reader.TryGetDateTimeOffset(out var instant))
            {
                throw new JsonException(
                    "Dates must be ISO 8601 with an offset or 'Z', for example 2030-09-01T10:00:00+02:00.");
            }

            return instant.UtcDateTime;
        }

        public override void Write(Utf8JsonWriter writer, DateTime value, JsonSerializerOptions options)
        {
            ArgumentNullException.ThrowIfNull(writer);

            var utc = value.Kind == DateTimeKind.Local
                ? value.ToUniversalTime()
                : DateTime.SpecifyKind(value, DateTimeKind.Utc);

            writer.WriteStringValue(utc);
        }
    }
}