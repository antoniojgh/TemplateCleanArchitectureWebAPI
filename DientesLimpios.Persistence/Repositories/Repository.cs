using System;
using System.Collections.Generic;
using System.Text;
using DientesLimpios.Application.Interfaces.Repositories;
using Microsoft.EntityFrameworkCore;

namespace DientesLimpios.Persistence.Repositories
{
    public class Repository<T>(DientesLimpiosDbContext context) : IRepository<T> where T : class
    {
        public Task Update(T entity)
        {
            context.Update(entity);
            return Task.CompletedTask;
        }

        public Task<T> Add(T entity)
        {
            context.Add(entity);
            return Task.FromResult(entity);
        }

        public Task Delete(T entity)
        {
            context.Remove(entity);
            return Task.CompletedTask;
        }

        public async Task<T?> GetById(Guid id)
        {
            return await context.Set<T>().FindAsync(id);
        }

        public async Task<IEnumerable<T>> GetAll()
        {
            return await context.Set<T>().ToListAsync();
        }

        public async Task<int> GetTotalRecordCount()
        {
            return await context.Set<T>().CountAsync();
        }
    }
}
