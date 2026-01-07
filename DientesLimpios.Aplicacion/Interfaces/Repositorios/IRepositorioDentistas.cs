using DientesLimpios.Aplicacion.CasosdeUso.Dentistas.Consultas.ObtenerListadoDentistas;
using DientesLimpios.Dominio.Entidades;

namespace DientesLimpios.Aplicacion.Interfaces.Repositorios
{
    public interface IRepositorioDentistas : IRepositorio<Dentista>
    {
        Task<IEnumerable<Dentista>> ObtenerFiltrado(FiltroDentistaDTO filtro);
    }
}
