using DientesLimpios.Aplicacion.Interfaces.Notificaciones;
using DientesLimpios.Aplicacion.Interfaces.Persistencia;
using DientesLimpios.Aplicacion.Interfaces.Repositorios;
using DientesLimpios.Aplicacion.Utilidades.Mediador;
using DientesLimpios.Dominio.Comunes.PatronResultados;
using DientesLimpios.Dominio.Entidades;
using DientesLimpios.Dominio.Errores;
using Microsoft.Extensions.Logging;


namespace DientesLimpios.Aplicacion.CasosdeUso.Citas.Comandos.CrearCita
{
    public class HandlerCrearCita(IRepositorioCitas repositorio, IUnitOfwork unidadDeTrabajo, IServicioNotificaciones servicioNotificaciones, ILogger<HandlerCrearCita> logger) : IRequestHandler<ComandoCrearCita, Result<Guid>>
    {
        public async Task<Result<Guid>> Handle(ComandoCrearCita request, CancellationToken cancellationToken)
        {
            logger.LogInformation(
            "Creando cita para Paciente {PacienteId} con Dentista {DentistaId}",
            request.PacienteId, request.DentistaId);

            var citaSeSolapa = await repositorio.ExisteCitaSolapada(request.DentistaId, request.FechaInicio, request.FechaFin);

            if (citaSeSolapa)
                return Result.Failure<Guid>(DomainErrors.Cita.Solapada);

            // Construct Cita through its factory.
            var citaResult = Cita.Crear(request.PacienteId, request.DentistaId, request.ConsultorioId, request.FechaInicio, request.FechaFin, DateTime.UtcNow);

            if (citaResult.IsFailure)
                return Result.Failure<Guid>(citaResult.Error);

            var cita = citaResult.Value;

            var citaCreada = await repositorio.Agregar(cita);
            await unidadDeTrabajo.Persistir();

            logger.LogInformation("Cita creada correctamente con ID: {CitaId}", citaCreada.Id);

            // Email confirmation — best-effort, must not fail the cita creation.
            try
            {
                var citaDB = await repositorio.ObtenerPorId(citaCreada.Id);
                var notificacionDTO = citaDB!.ADto();

                await servicioNotificaciones.EnviarConfirmacionCita(notificacionDTO);

                logger.LogInformation(
                    "Email de confirmación enviado a {Email}", notificacionDTO.Paciente_Email);
            }
            catch (Exception ex)
            {
                logger.LogWarning(ex,
                    "Cita {CitaId} creada pero falló el envío del email de confirmación.",
                    citaCreada.Id);
            }

            return Result.Success(citaCreada.Id);
        }

    }
}
