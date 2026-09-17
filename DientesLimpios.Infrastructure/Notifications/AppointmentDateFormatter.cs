using System.Globalization;

namespace DientesLimpios.Infrastructure.Notifications
{
    // Emails are an edge: the only place a stored UTC instant is shown as a local clock time.
    public static class AppointmentDateFormatter
    {
        private static readonly CultureInfo Culture = CultureInfo.GetCultureInfo("en-GB");

        public static string ToClinicTime(DateTime utc, TimeZoneInfo clinicTimeZone) =>
            TimeZoneInfo.ConvertTimeFromUtc(DateTime.SpecifyKind(utc, DateTimeKind.Utc), clinicTimeZone)
                        .ToString("f", Culture);
    }
}
