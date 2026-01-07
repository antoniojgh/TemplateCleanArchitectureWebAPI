using System;
using System.Collections.Generic;
using System.Text;
using MediatR;

namespace DientesLimpios.Aplicacion.CasosdeUso.Pacientes.Comandos.BorrarPaciente
{
    public class ComandoBorrarPaciente : IRequest
    {
        public Guid Id { get; set; }
    }
}
