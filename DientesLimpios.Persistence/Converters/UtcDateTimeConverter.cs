using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace DientesLimpios.Persistence.Converters
{
    // datetime2 has no time zone. Everything written must already be UTC, and everything read
    // is marked UTC, so it compares correctly in memory and serialises with "Z".
    public sealed class UtcDateTimeConverter()
        : ValueConverter<DateTime, DateTime>(
            value => EnsureUtc(value),
            value => DateTime.SpecifyKind(value, DateTimeKind.Utc))
    {
        // A non-UTC value here is a programming error at a boundary, not a business outcome.
        public static DateTime EnsureUtc(DateTime value) =>
            value.Kind == DateTimeKind.Utc
                ? value
                : throw new InvalidOperationException(
                    $"Only UTC DateTime values can be persisted (got Kind={value.Kind}). Convert at the boundary.");
    }
}