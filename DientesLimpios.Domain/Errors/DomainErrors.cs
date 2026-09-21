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

            public static readonly Error ConfirmationAlreadySent = new(
                "Appointment.ConfirmationAlreadySent",
                "The confirmation for this appointment has already been sent.");
        }

        public static class Patient
        {
            public static readonly Error NotFound = new(
                "Patient.NotFound",
                "The specified patient was not found.");

            public static readonly Error NameRequired = new(
                "Patient.NameRequired",
                "The patient name is required.");

            public static readonly Error HasAppointmentsConflict = new(
                "Patient.HasAppointmentsConflict",
                "The patient cannot be deleted because appointments reference them.");
        }

        public static class Dentist
        {
            public static readonly Error NotFound = new(
                "Dentist.NotFound",
                "The specified dentist was not found.");

            public static readonly Error NameRequired = new(
                "Dentist.NameRequired",
                "The dentist name is required.");

            public static readonly Error HasAppointmentsConflict = new(
                "Dentist.HasAppointmentsConflict",
                "The dentist cannot be deleted because appointments reference them.");
        }

        public static class Office
        {
            public static readonly Error NotFound = new(
                "Office.NotFound",
                "The specified office was not found.");

            public static readonly Error NameRequired = new(
                "Office.NameRequired",
                "The office name is required.");

            public static readonly Error HasAppointmentsConflict = new(
                "Office.HasAppointmentsConflict",
                "The office cannot be deleted because appointments reference it.");
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

        public static class Concurrency
        {
            public static readonly Error Conflict = new(
                "Concurrency.Conflict",
                "The record was modified by someone else. Reload it and try again.");
        }
    }

}
