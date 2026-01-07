using DientesLimpios.Aplicacion.Interfaces.Repositorios.Modelos;
using MediatR;

namespace DientesLimpios.Aplicacion.CasosdeUso.Citas.Consultas.ObtenerListadoCitas
{
    public class ConsultaObtenerListadoCitas : FiltroCitasDTO, IRequest<List<CitaListadoDTO>>
    {
    }
}
