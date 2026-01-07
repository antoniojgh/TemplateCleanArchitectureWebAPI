using System;
using System.Collections.Generic;
using System.Text;
using DientesLimpios.Aplicacion.Excepciones;
using DientesLimpios.Aplicacion.Interfaces.Persistencia;
using DientesLimpios.Aplicacion.Interfaces.Repositorios;
using DientesLimpios.Dominio.ObjetosDeValor;
using MediatR;

namespace DientesLimpios.Aplicacion.CasosdeUso.Pacientes.Comandos.ActualizarPaciente
{
    public class HandlerActualizarPaciente(IRepositorioPacientes repositorio, IUnitOfwork unidadDeTrabajo) : IRequestHandler<ComandoActualizarPaciente>
    {
        public async Task Handle(ComandoActualizarPaciente request, CancellationToken cancellationToken)
        {
            var paciente = await repositorio.ObtenerPorId(request.Id);

            if (paciente is null)
            {
                throw new ExcepcionNoEncontrado();
            }

            paciente.ActualizarNombre(request.Nombre);
            var email = new Email(request.Email);
            paciente.ActualizarEmail(email);

            try
            {
                await repositorio.Actualizar(paciente);
                await unidadDeTrabajo.Persistir();
            }
            catch (Exception)
            {
                await unidadDeTrabajo.Reversar();
                throw;
            }
        }
    }
}
