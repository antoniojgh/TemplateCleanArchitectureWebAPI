using System;
using System.Collections.Generic;
using System.Text;
using AutoMapper;
using DientesLimpios.Aplicacion.CasosdeUso.Citas.Consultas.ObtenerDetalleCita;
using DientesLimpios.Aplicacion.Excepciones;
using DientesLimpios.Aplicacion.Interfaces.Notificaciones;
using DientesLimpios.Aplicacion.Interfaces.Persistencia;
using DientesLimpios.Aplicacion.Interfaces.Repositorios;
using DientesLimpios.Dominio.Entidades;
using DientesLimpios.Dominio.ObjetosDeValor;
using MediatR;

namespace DientesLimpios.Aplicacion.CasosdeUso.Citas.Comandos.CrearCita
{
    public class HandlerCrearCita(IRepositorioCitas repositorio, IUnitOfwork unidadDeTrabajo, IServicioNotificaciones servicioNotificaciones, IMapper mapper) : IRequestHandler<ComandoCrearCita, Guid>
    {
        public async Task<Guid> Handle(ComandoCrearCita request, CancellationToken cancellationToken)
        {
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
            }
            catch (Exception)
            {
                await unidadDeTrabajo.Reversar();
                throw;
            }

            var citaDB = await repositorio.ObtenerPorId(id.Value);
            var notificacionDTO = mapper.Map<ConfirmacionCitaDTO>(citaDB);
            await servicioNotificaciones.EnviarConfirmacionCita(notificacionDTO);
            return id.Value;
        }

    }
}
