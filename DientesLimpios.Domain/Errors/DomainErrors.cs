using DientesLimpios.Domain.Common.ResultPattern;

namespace DientesLimpios.Domain.Errors
{
    public static class DomainErrors
    {
        public static class General
        {
            public static readonly Error NotFound = new(
                "General.NotFound",
                "The requested resource was not found.");
        }

        public static class Appointment
        {
            public static readonly Error NotFound = new(
                "Appointment.NotFound",
                "The specified appointment was not found.");

            public static readonly Error InThePast = new(
                "Appointment.InThePast",
                "The start date cannot be earlier than the current date.");

            public static readonly Error OnlyScheduledCanBeCancelled = new(
                "Appointment.OnlyScheduledCanBeCancelled",
                "Only scheduled appointments can be cancelled.");

            public static readonly Error OnlyScheduledCanBeCompleted = new(
                "Appointment.OnlyScheduledCanBeCompleted",
                "Only scheduled appointments can be completed.");

            public static readonly Error Overlapping = new(
                "Appointment.Overlapping",
                "The dentist already has an appointment at that time.");
        }

        public static class Patient
        {
            public static readonly Error NotFound = new(
                "Patient.NotFound",
                "The specified patient was not found.");

            public static readonly Error NameRequired = new(
                "Patient.NameRequired",
                "The patient name is required.");
        }

        public static class Dentist
        {
            public static readonly Error NotFound = new(
                "Dentist.NotFound",
                "The specified dentist was not found.");

            public static readonly Error NameRequired = new(
                "Dentist.NameRequired",
                "The dentist name is required.");
        }

        public static class Office
        {
            public static readonly Error NotFound = new(
                "Office.NotFound",
                "The specified office was not found.");

            public static readonly Error NameRequired = new(
                "Office.NameRequired",
                "The office name is required.");
        }

        public static class TimeInterval
        {
            public static readonly Error StartGreaterThanOrEqualToEnd = new(
                "TimeInterval.StartGreaterThanOrEqualToEnd",
                "The start time must be earlier than the end time.");
        }

        public static class Email
        {
            public static readonly Error Empty = new(
                "Email.Empty",
                "The email is required.");

            public static readonly Error InvalidFormat = new(
                "Email.InvalidFormat",
                "The email format is not valid.");
        }
    }

}
