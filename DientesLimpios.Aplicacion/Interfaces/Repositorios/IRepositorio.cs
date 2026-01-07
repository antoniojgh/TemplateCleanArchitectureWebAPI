using System;
using System.Collections.Generic;
using System.Text;

namespace DientesLimpios.Aplicacion.Interfaces.Repositorios
{
    public interface IRepositorio<T> where T : class
    {
        Task<T?> ObtenerPorId(Guid id);
        Task<IEnumerable<T>> ObtenerTodos();
        Task<int> ObtenerCantidadTotalRegistros();
        Task<T> Agregar(T entidad);
        Task Actualizar(T entidad);
        Task Borrar(T entidad);
    }
}
