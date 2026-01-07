using System;
using System.Collections.Generic;
using System.Text;

namespace DientesLimpios.Aplicacion.CasosdeUso.Consultorios.Consultas.ObtenerListadoConsultorios
{
    public class ConsultorioListadoDTO
    {
        public Guid Id { get; set; }
        public required string Nombre { get; set; }
    }
}
