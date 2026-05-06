using DientesLimpios.Aplicacion.Interfaces.Repositorios.Modelos;
using DientesLimpios.Aplicacion.Utilidades.Mediador;
using DientesLimpios.Dominio.Comunes.PatronResultados;

namespace DientesLimpios.Aplicacion.CasosdeUso.Citas.Consultas.ObtenerListadoCitas
{
    public class ConsultaObtenerListadoCitas : FiltroCitasDTO, IRequest<Result<List<CitaListadoDTO>>>
    {
    }
}
