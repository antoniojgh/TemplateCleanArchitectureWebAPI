using DientesLimpios.Dominio.Comunes;
using DientesLimpios.Dominio.Comunes.PatronResultados;
using DientesLimpios.Dominio.Enums;
using DientesLimpios.Dominio.Errores;
using DientesLimpios.Dominio.Excepciones;
using DientesLimpios.Dominio.ObjetosDeValor;

namespace DientesLimpios.Dominio.Entidades
{
    public class Cita : EntidadAuditable
    {
        public Guid Id { get; private set; }
        public Guid PacienteId { get; private set; }
        public Guid DentistaId { get; private set; }
        public Guid ConsultorioId { get; private set; }
        public EstadoCita Estado { get; private set; }
        public IntervaloDeTiempo IntervaloDeTiempo { get; private set; } = null!;
        public Paciente? Paciente { get; private set; }
        public Dentista? Dentista { get; private set; }
        public Consultorio? Consultorio { get; private set; }

        private Cita() { }   // EF Core

        private Cita(
            Guid pacienteId, Guid dentistaId, Guid consultorioId,
            IntervaloDeTiempo intervaloDeTiempo)
        {
            Id = Guid.CreateVersion7();
            PacienteId = pacienteId;
            DentistaId = dentistaId;
            ConsultorioId = consultorioId;
            IntervaloDeTiempo = intervaloDeTiempo;
            Estado = EstadoCita.Programada;
        }

        public static Result<Cita> Crear(
            Guid pacienteId, Guid dentistaId, Guid consultorioId,
            DateTime fechaInicio, DateTime fechaFin, DateTime ahoraUtc)
        {
            if (fechaInicio < ahoraUtc)
                return Result.Failure<Cita>(DomainErrors.Cita.EnElPasado);

            var intervaloResult = IntervaloDeTiempo.Crear(fechaInicio, fechaFin);
            if (intervaloResult.IsFailure)
                return Result.Failure<Cita>(intervaloResult.Error);

            return Result.Success(new Cita(
                pacienteId, dentistaId, consultorioId, intervaloResult.Value));
        }

        public Result Cancelar()
        {
            if (Estado != EstadoCita.Programada)
                return Result.Failure(DomainErrors.Cita.SoloSePuedenCancelarProgramadas);

            Estado = EstadoCita.Cancelada;
            return Result.Success();
        }

        public Result Completar()
        {
            if (Estado != EstadoCita.Programada)
                return Result.Failure(DomainErrors.Cita.SoloSePuedenCompletarProgramadas);

            Estado = EstadoCita.Completada;
            return Result.Success();
        }
    }

}
