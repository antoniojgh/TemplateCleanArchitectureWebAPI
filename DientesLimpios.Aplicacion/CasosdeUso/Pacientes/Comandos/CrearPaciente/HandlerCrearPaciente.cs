using System;
using System.Collections.Generic;
using System.Text;
using DientesLimpios.Aplicacion.Interfaces.Persistencia;
using DientesLimpios.Aplicacion.Interfaces.Repositorios;
using DientesLimpios.Dominio.Entidades;
using DientesLimpios.Dominio.ObjetosDeValor;
using MediatR;

namespace DientesLimpios.Aplicacion.CasosdeUso.Pacientes.Comandos.CrearPaciente
{
    public class HandlerCrearPaciente(IRepositorioPacientes repositorio, IUnitOfwork unidadDeTrabajo) : IRequestHandler<ComandoCrearPaciente, Guid>
    {
        public async Task<Guid> Handle(ComandoCrearPaciente request, CancellationToken cancellationToken)
        {
            var email = new Email(request.Email);
            var paciente = new Paciente(request.Nombre, email);

            try
            {
                var respuesta = await repositorio.Agregar(paciente);
                await unidadDeTrabajo.Persistir();
                return respuesta.Id;
            }
            catch (Exception)
            {
                await unidadDeTrabajo.Reversar();
                throw;
            }
        }
    }
}
