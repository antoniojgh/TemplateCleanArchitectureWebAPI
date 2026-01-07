using System;
using System.Collections.Generic;
using System.Text;

namespace DientesLimpios.Aplicacion.CasosdeUso.Consultorios.Consultas.ObtenerDetalleConsultorio
{
    public class ConsultorioDetalleDTO
    {
        public Guid Id { get; set; }
        public required string Nombre { get; set; }
    }
}
