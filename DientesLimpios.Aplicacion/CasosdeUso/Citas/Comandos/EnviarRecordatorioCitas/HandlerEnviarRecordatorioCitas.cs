using System;
using System.Collections.Generic;
using System.Text;
using AutoMapper;
using DientesLimpios.Aplicacion.Interfaces.Notificaciones;
using DientesLimpios.Aplicacion.Interfaces.Repositorios;
using DientesLimpios.Aplicacion.Interfaces.Repositorios.Modelos;
using DientesLimpios.Dominio.Enums;
using MediatR;

namespace DientesLimpios.Aplicacion.CasosdeUso.Citas.Comandos.EnviarRecordatorioCitas
{
    public class HandlerEnviarRecordatorioCitas(IRepositorioCitas repositorio,
                IServicioNotificaciones servicioNotificaciones, IMapper mapper) : IRequestHandler<ComandoEnviarRecordatorioCitas>
    {


        public async Task Handle(ComandoEnviarRecordatorioCitas request, CancellationToken cancellationToken)
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
                var citaDTO = mapper.Map<RecordatorioCitaDTO>(cita);
                await servicioNotificaciones.EnviarRecordatorioCita(citaDTO);
            }

        }
    }
}
