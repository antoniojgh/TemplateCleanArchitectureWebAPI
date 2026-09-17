using System.Text.Json;
using DientesLimpios.API.Json;
using FluentAssertions;

namespace DientesLimpios.Tests.API.Json
{
    public class UtcDateTimeJsonConverterTests
    {
        private static readonly JsonSerializerOptions Options = new(JsonSerializerDefaults.Web)
        {
            Converters = { new UtcDateTimeJsonConverter() }
        };

        private static readonly DateTime EightUtc = new(2030, 9, 1, 8, 0, 0, DateTimeKind.Utc);

        [Theory]
        [InlineData("\"2030-09-01T10:00:00+02:00\"")]
        [InlineData("\"2030-09-01T08:00:00Z\"")]
        public void Read_ValueWithOffset_ReturnsTheUtcInstant(string json)
        {
            // Act
            var value = JsonSerializer.Deserialize<DateTime>(json, Options);

            // Assert
            value.Should().Be(EightUtc);
            value.Kind.Should().Be(DateTimeKind.Utc);
        }

        [Fact]
        public void Read_ValueWithoutOffset_Throws()
        {
            // Act
            var act = () => JsonSerializer.Deserialize<DateTime>("\"2030-09-01T10:00:00\"", Options);

            // Assert
            act.Should().Throw<JsonException>();
        }

        [Fact]
        public void Write_ValueReadFromTheDatabase_EmitsZ()
        {
            // Arrange — datetime2 comes back with Kind=Unspecified.
            var fromDatabase = DateTime.SpecifyKind(EightUtc, DateTimeKind.Unspecified);

            // Act
            var json = JsonSerializer.Serialize(fromDatabase, Options);

            // Assert
            json.Should().Be("\"2030-09-01T08:00:00Z\"");
        }
    }
}