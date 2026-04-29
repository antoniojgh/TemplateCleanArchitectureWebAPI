using DientesLimpios.Aplicacion.Excepciones;
using DientesLimpios.Aplicacion.Interfaces.Notificaciones;
using DientesLimpios.Aplicacion.Interfaces.Persistencia;
using DientesLimpios.Aplicacion.Interfaces.Repositorios;
using DientesLimpios.Dominio.Entidades;
using DientesLimpios.Dominio.ObjetosDeValor;
using DientesLimpios.Aplicacion.Utilidades.Mediador;
using Microsoft.Extensions.Logging;

namespace DientesLimpios.Aplicacion.CasosdeUso.Citas.Comandos.CrearCita
{
    public class HandlerCrearCita(IRepositorioCitas repositorio, IUnitOfwork unidadDeTrabajo, IServicioNotificaciones servicioNotificaciones, ILogger<HandlerCrearCita> logger) : IRequestHandler<ComandoCrearCita, Guid>
    {
        public async Task<Guid> Handle(ComandoCrearCita request)
        {
            logger.LogInformation("Handling CrearCita for Patient {PatientId} with Dentist {DentistId}", request.PacienteId, request.DentistaId);

            var citaSeSolapa = await repositorio.ExisteCitaSolapada(request.DentistaId, request.FechaInicio, request.FechaFin);

            if (citaSeSolapa)
            {
                throw new ExcepcionDeValidacion("El dentista ya tiene una cita en ese horario");
            }

            var intervaloDeTiempo = new IntervaloDeTiempo(request.FechaInicio, request.FechaFin);
            var cita = new Cita(request.PacienteId, request.DentistaId, request.ConsultorioId, intervaloDeTiempo);

            Guid? id = null;

            try
            {
                var respuesta = await repositorio.Agregar(cita);
                await unidadDeTrabajo.Persistir();
                id = respuesta.Id;

                logger.LogInformation("Cita created successfully with ID: {CitaId}", id);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Database transaction failed for CrearCita");
                await unidadDeTrabajo.Reversar();
                throw;
            }

            try
            {
                var citaDB = await repositorio.ObtenerPorId(id.Value);
                var notificacionDTO = citaDB!.ADto();
                await servicioNotificaciones.EnviarConfirmacionCita(notificacionDTO);

                logger.LogInformation("Confirmation email sent to {Email}", notificacionDTO.Paciente_Email);
            }
            catch (Exception ex)
            {
                // We log as Warning because the appointment exists, but the email failed.
                logger.LogWarning(ex, "Appointment {CitaId} created but failed to send confirmation email.", id);
            }
            return id.Value;
        }

    }
}
