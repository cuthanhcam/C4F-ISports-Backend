using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace api.Interfaces
{
    // This interface is used to define the generic repository pattern
    // It is used to define the basic CRUD operations for the database
    // The generic repository pattern is used to reduce the amount of code duplication
    public interface IGenericRepository<T> where T : class
    {
        Task<T> GetByIdAsync(int id);
        IQueryable<T> GetAll();
        Task AddAsync(T entity);
        void Update(T entity);
        void Delete(T entity);        
    }
}