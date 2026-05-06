using DientesLimpios.Aplicacion.CasosdeUso.Citas.Comandos.CancelarCita;
using DientesLimpios.Aplicacion.Interfaces.Notificaciones;
using DientesLimpios.Aplicacion.Interfaces.Repositorios;
using DientesLimpios.Aplicacion.Interfaces.Repositorios.Modelos;
using DientesLimpios.Aplicacion.Utilidades.Mediador;
using DientesLimpios.Dominio.Comunes.PatronResultados;
using DientesLimpios.Dominio.Enums;
using Microsoft.Extensions.Logging;

namespace DientesLimpios.Aplicacion.CasosdeUso.Citas.Comandos.EnviarRecordatorioCitas
{
    public class HandlerEnviarRecordatorioCitas(IRepositorioCitas repositorio,
                IServicioNotificaciones servicioNotificaciones, ILogger<HandlerEnviarRecordatorioCitas> logger) : IRequestHandler<ComandoEnviarRecordatorioCitas, Result>
    {
        public async Task<Result> Handle(ComandoEnviarRecordatorioCitas request, CancellationToken cancellationToken)
        {
            logger.LogInformation("Enviando recordatorio de citas para el día: {Fecha}", DateTime.UtcNow.Date.AddDays(1));

            var mañana = DateTime.UtcNow.Date.AddDays(1);
            var fechaInicio = mañana;
            var fechaFin = mañana.AddDays(1);

            var filtro = new FiltroCitasDTO
            {
                FechaInicio = fechaInicio,
                FechaFin = fechaFin,
                EstadoCita = EstadoCita.Programada
            };

            var citas = await repositorio.ObtenerFiltrado(filtro);

            foreach (var cita in citas)
            {
                var citaDTO = cita.ADto();
                await servicioNotificaciones.EnviarRecordatorioCita(citaDTO);
            }

            logger.LogInformation("Recordatorio de citas enviado correctamente para {NumeroCitas} citas", citas.Count());

            return Result.Success();
        }
    }
}
