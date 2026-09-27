using System;
using System.Globalization;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace BlazorApp.Shared
{
    /// <summary>
    /// Writes TimeSpans in the standard constant ("c") format, but also reads the legacy
    /// format written by Blazored.LocalStorage 4.x (d\.hh\:mm\:ss\:FFF, e.g. "1.00:00:00:").
    /// </summary>
    public class TimeSpanJsonConverter : JsonConverter<TimeSpan>
    {
        private const string LegacyFormat = @"d\.hh\:mm\:ss\:FFF";

        public override TimeSpan Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            if (reader.TokenType == JsonTokenType.Number)
                return TimeSpan.FromTicks(reader.GetInt64());

            if (reader.TokenType != JsonTokenType.String)
                throw new JsonException($"Unexpected token {reader.TokenType} when parsing TimeSpan.");

            var s = reader.GetString();
            if (string.IsNullOrWhiteSpace(s))
                return TimeSpan.Zero;

            if (TimeSpan.TryParseExact(s, "c", CultureInfo.InvariantCulture, out var result))
                return result;

            if (TimeSpan.TryParseExact(s, LegacyFormat, CultureInfo.InvariantCulture, out result))
                return result;

            if (TimeSpan.TryParse(s, CultureInfo.InvariantCulture, out result))
                return result;

            throw new JsonException($"Unable to parse '{s}' as a TimeSpan.");
        }

        public override void Write(Utf8JsonWriter writer, TimeSpan value, JsonSerializerOptions options)
        {
            writer.WriteStringValue(value.ToString("c", CultureInfo.InvariantCulture));
        }
    }
}
