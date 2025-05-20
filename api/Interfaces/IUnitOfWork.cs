using System;
using System.Threading.Tasks;
using api.Data;
using Microsoft.EntityFrameworkCore.Storage;

namespace api.Interfaces
{
    public interface IUnitOfWork : IDisposable
    {
        ApplicationDbContext Context { get; }
        IGenericRepository<T> Repository<T>() where T : class;
        Task<IDbContextTransaction> BeginTransactionAsync();
        Task CommitTransactionAsync();
        Task RollbackTransactionAsync();
        Task<int> SaveChangesAsync();
    }
}