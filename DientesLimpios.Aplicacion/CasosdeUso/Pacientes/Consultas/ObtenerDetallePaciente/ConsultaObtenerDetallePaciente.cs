using DientesLimpios.Aplicacion.Utilidades.Mediador;

namespace DientesLimpios.Aplicacion.CasosdeUso.Pacientes.Consultas.ObtenerDetallePaciente
{
    public class ConsultaObtenerDetallePaciente : IRequest<PacienteDetalleDTO>
    {
        public required Guid Id { get; set; }
    }
}
