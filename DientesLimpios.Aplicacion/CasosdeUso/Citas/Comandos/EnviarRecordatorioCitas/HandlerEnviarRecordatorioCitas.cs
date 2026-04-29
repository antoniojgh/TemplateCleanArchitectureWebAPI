using DientesLimpios.Aplicacion.Interfaces.Notificaciones;
using DientesLimpios.Aplicacion.Interfaces.Repositorios;
using DientesLimpios.Aplicacion.Interfaces.Repositorios.Modelos;
using DientesLimpios.Dominio.Enums;
using DientesLimpios.Aplicacion.Utilidades.Mediador;

namespace DientesLimpios.Aplicacion.CasosdeUso.Citas.Comandos.EnviarRecordatorioCitas
{
    public class HandlerEnviarRecordatorioCitas(IRepositorioCitas repositorio,
                IServicioNotificaciones servicioNotificaciones) : IRequestHandler<ComandoEnviarRecordatorioCitas>
    {


        public async Task Handle(ComandoEnviarRecordatorioCitas request)
        {
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

        }
    }
}
