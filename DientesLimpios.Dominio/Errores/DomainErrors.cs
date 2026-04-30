using DientesLimpios.Dominio.Comunes.PatronResultados;

namespace DientesLimpios.Dominio.Errores
{
    public static class DomainErrors
    {
        public static class General
        {
            public static readonly Error NoEncontrado = new(
                "General.NoEncontrado",
                "El recurso solicitado no fue encontrado.");
        }

        public static class Cita
        {
            public static readonly Error NoEncontrada = new(
                "Cita.NoEncontrada",
                "La cita especificada no fue encontrada.");

            public static readonly Error EnElPasado = new(
                "Cita.EnElPasado",
                "La fecha de inicio no puede ser anterior a la fecha actual.");

            public static readonly Error SoloSePuedenCancelarProgramadas = new(
                "Cita.SoloSePuedenCancelarProgramadas",
                "Solo se pueden cancelar citas programadas.");

            public static readonly Error SoloSePuedenCompletarProgramadas = new(
                "Cita.SoloSePuedenCompletarProgramadas",
                "Solo se pueden completar citas programadas.");

            public static readonly Error Solapada = new(
                "Cita.Solapada",
                "El dentista ya tiene una cita en ese horario.");
        }

        public static class Paciente
        {
            public static readonly Error NoEncontrado = new(
                "Paciente.NoEncontrado",
                "El paciente especificado no fue encontrado.");

            public static readonly Error NombreObligatorio = new(
                "Paciente.NombreObligatorio",
                "El nombre del paciente es obligatorio.");

            public static readonly Error EmailObligatorio = new(
                "Paciente.EmailObligatorio",
                "El email del paciente es obligatorio.");
        }

        public static class Dentista
        {
            public static readonly Error NoEncontrado = new(
                "Dentista.NoEncontrado",
                "El dentista especificado no fue encontrado.");

            public static readonly Error NombreObligatorio = new(
                "Dentista.NombreObligatorio",
                "El nombre del dentista es obligatorio.");

            public static readonly Error EmailObligatorio = new(
                "Dentista.EmailObligatorio",
                "El email del dentista es obligatorio.");
        }

        public static class Consultorio
        {
            public static readonly Error NoEncontrado = new(
                "Consultorio.NoEncontrado",
                "El consultorio especificado no fue encontrado.");

            public static readonly Error NombreObligatorio = new(
                "Consultorio.NombreObligatorio",
                "El nombre del consultorio es obligatorio.");
        }

        public static class IntervaloDeTiempo
        {
            public static readonly Error InicioMayorOIgualAFin = new(
                "IntervaloDeTiempo.InicioMayorOIgualAFin",
                "La hora de inicio debe ser anterior a la hora de fin.");
        }

        public static class Email
        {
            public static readonly Error Vacio = new(
                "Email.Vacio",
                "El email es obligatorio.");

            public static readonly Error FormatoInvalido = new(
                "Email.FormatoInvalido",
                "El email no es válido.");
        }
    }

}
