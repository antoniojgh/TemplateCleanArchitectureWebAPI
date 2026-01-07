using DientesLimpios.Aplicacion.Interfaces.Repositorios.Modelos;
using DientesLimpios.Dominio.Entidades;

namespace DientesLimpios.Aplicacion.Interfaces.Repositorios
{
    public interface IRepositorioCitas : IRepositorio<Cita>
    {
        Task<bool> ExisteCitaSolapada(Guid dentistaId, DateTime inicio, DateTime fin);

        // Obtiene una cita por su Id, incluyendo las entidades relacionadas y reemplaza el método base
        new Task<Cita?> ObtenerPorId(Guid id);
        Task<IEnumerable<Cita>> ObtenerFiltrado(FiltroCitasDTO filtroCitasDTO);
    }
}
