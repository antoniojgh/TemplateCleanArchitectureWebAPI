using System;
using System.Collections.Generic;
using System.Text;
using MediatR;

namespace DientesLimpios.Aplicacion.CasosdeUso.Consultorios.Consultas.ObtenerListadoConsultorios
{
    public class ConsultaObtenerListadoConsultorios : IRequest<List<ConsultorioListadoDTO>>
    {
    }
}
