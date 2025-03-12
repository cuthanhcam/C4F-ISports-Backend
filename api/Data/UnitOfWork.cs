using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using api.Interfaces;
using api.Models;
using api.Repositories;

namespace api.Data
{
    // This class is responsible for managing the repositories and saving changes to the database
    public class UnitOfWork : IUnitOfWork
    {
        private readonly ApplicationDbContext _context;
        public IGenericRepository<Account> Accounts { get; private set; }

        public IGenericRepository<User> Users { get; private set; }

        public IGenericRepository<Owner> Owners { get; private set; }

        public IGenericRepository<RefreshToken> RefreshTokens { get; private set; }

        public UnitOfWork(ApplicationDbContext context)
        {
            _context = context;
            Accounts = new GenericRepository<Account>(_context);
            Users = new GenericRepository<User>(_context);
            Owners = new GenericRepository<Owner>(_context);
            RefreshTokens = new GenericRepository<RefreshToken>(_context);
        }

        public async Task<int> SaveChangesAsync()
        {
            return await _context.SaveChangesAsync();
        }

        public void Dispose()
        {
            _context.Dispose();
        }
    }
}