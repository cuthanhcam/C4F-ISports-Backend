using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using api.Models;

namespace api.Interfaces
{
    // This interface is used to define the Unit of Work pattern
    // The Unit of Work pattern is used to group all the repositories and save changes to the database in a single transaction
    public interface IUnitOfWork : IDisposable
    {
        IGenericRepository<Account> Accounts { get; }
        IGenericRepository<User> Users { get; }
        IGenericRepository<Owner> Owners { get; }
        IGenericRepository<RefreshToken> RefreshTokens { get; }
        Task<int> SaveChangesAsync();
    }
}