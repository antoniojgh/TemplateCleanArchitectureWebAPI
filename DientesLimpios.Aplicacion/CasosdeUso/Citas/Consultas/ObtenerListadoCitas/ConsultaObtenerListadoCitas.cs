using DientesLimpios.Aplicacion.Interfaces.Repositorios.Modelos;
using DientesLimpios.Aplicacion.Utilidades.Mediador;

namespace DientesLimpios.Aplicacion.CasosdeUso.Citas.Consultas.ObtenerListadoCitas
{
    public class ConsultaObtenerListadoCitas : FiltroCitasDTO, IRequest<List<CitaListadoDTO>>
    {
    }
}
