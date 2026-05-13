using DientesLimpios.Domain.Common.ResultPattern;

namespace DientesLimpios.Domain.Errors
{
    public static class DomainErrors
    {
        public static class General
        {
            public static readonly Error NotFound = new(
                "General.NotFound",
                "El recurso solicitado no fue encontrado.");
        }

        public static class Appointment
        {
            public static readonly Error NotFound = new(
                "Appointment.NotFound",
                "La appointment especificada no fue encontrada.");

            public static readonly Error InThePast = new(
                "Appointment.InThePast",
                "La fecha de start no puede ser anterior a la fecha actual.");

            public static readonly Error OnlyScheduledCanBeCancelled = new(
                "Appointment.OnlyScheduledCanBeCancelled",
                "Solo se pueden cancelar appointments programadas.");

            public static readonly Error OnlyScheduledCanBeCompleted = new(
                "Appointment.OnlyScheduledCanBeCompleted",
                "Solo se pueden completar appointments programadas.");

            public static readonly Error Overlapping = new(
                "Appointment.Overlapping",
                "El dentist ya tiene una appointment en ese horario.");
        }

        public static class Patient
        {
            public static readonly Error NotFound = new(
                "Patient.NotFound",
                "El patient especificado no fue encontrado.");

            public static readonly Error NameRequired = new(
                "Patient.NameRequired",
                "El name del patient es obligatorio.");
        }

        public static class Dentist
        {
            public static readonly Error NotFound = new(
                "Dentist.NotFound",
                "El dentist especificado no fue encontrado.");

            public static readonly Error NameRequired = new(
                "Dentist.NameRequired",
                "El name del dentist es obligatorio.");
        }

        public static class Office
        {
            public static readonly Error NotFound = new(
                "Office.NotFound",
                "El office especificado no fue encontrado.");

            public static readonly Error NameRequired = new(
                "Office.NameRequired",
                "El name del office es obligatorio.");
        }

        public static class TimeInterval
        {
            public static readonly Error StartGreaterThanOrEqualToEnd = new(
                "TimeInterval.StartGreaterThanOrEqualToEnd",
                "La hora de start debe ser anterior a la hora de end.");
        }

        public static class Email
        {
            public static readonly Error Empty = new(
                "Email.Empty",
                "El email es obligatorio.");

            public static readonly Error InvalidFormat = new(
                "Email.InvalidFormat",
                "El email no es válido.");
        }
    }

}
