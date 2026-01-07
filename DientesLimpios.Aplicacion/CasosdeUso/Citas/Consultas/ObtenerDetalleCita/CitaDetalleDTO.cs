using System;
using System.Collections.Generic;
using System.Text;

namespace DientesLimpios.Aplicacion.CasosdeUso.Citas.Consultas.ObtenerDetalleCita
{
    public class CitaDetalleDTO
    {
        public required Guid Id { get; set; }
        public required string Paciente { get; set; }
        public required string Dentista { get; set; }
        public required string Consultorio { get; set; }
        public required DateTime FechaInicio { get; set; }
        public required DateTime FechaFin { get; set; }
        public required string EstadoCita { get; set; }

    }
}
