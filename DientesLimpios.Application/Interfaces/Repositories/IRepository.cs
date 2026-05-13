using System;
using System.Collections.Generic;
using System.Text;

namespace DientesLimpios.Application.Interfaces.Repositories
{
    public interface IRepository<T> where T : class
    {
        Task<T?> GetById(Guid id);
        Task<IEnumerable<T>> GetAll();
        Task<int> GetTotalRecordCount();
        Task<T> Add(T entidad);
        Task Update(T entidad);
        Task Delete(T entidad);
    }
}
