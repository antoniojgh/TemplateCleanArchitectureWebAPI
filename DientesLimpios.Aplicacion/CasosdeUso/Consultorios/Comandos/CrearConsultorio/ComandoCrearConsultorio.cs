using System;
using System.Collections.Generic;
using System.Text;
using MediatR;

namespace DientesLimpios.Aplicacion.CasosdeUso.Consultorios.Comandos.CrearConsultorio
{
    public class ComandoCrearConsultorio : IRequest<Guid>
    {
        public required string Nombre { get; set; }
    }
}
