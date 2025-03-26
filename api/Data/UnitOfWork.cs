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
        private bool _disposed;

        public IGenericRepository<Account> Accounts { get; private set; }
        public IGenericRepository<User> Users { get; private set; }
        public IGenericRepository<Owner> Owners { get; private set; }
        public IGenericRepository<RefreshToken> RefreshTokens { get; private set; }
        public IGenericRepository<FavoriteField> FavoriteFields { get; private set; }
        public IGenericRepository<Booking> Bookings { get; private set; }
        public IGenericRepository<Field> Fields { get; private set; }
        public IGenericRepository<Sport> Sports { get; private set; }
        public IGenericRepository<Review> Reviews { get; private set;}
        public IGenericRepository<FieldAmenity> FieldAmenities { get; private set; }

        public UnitOfWork(ApplicationDbContext context)
        {
            _context = context;
            InitializeRepositories();
        }

        private void InitializeRepositories()
        {
            Accounts = new GenericRepository<Account>(_context);
            Users = new GenericRepository<User>(_context);
            Owners = new GenericRepository<Owner>(_context);
            RefreshTokens = new GenericRepository<RefreshToken>(_context);
            FavoriteFields = new GenericRepository<FavoriteField>(_context);
            Bookings = new GenericRepository<Booking>(_context);
            Fields = new GenericRepository<Field>(_context);
            Sports = new GenericRepository<Sport>(_context);
            Reviews = new GenericRepository<Review>(_context);
            FieldAmenities = new GenericRepository<FieldAmenity>(_context);
        }

        public async Task<int> SaveChangesAsync()
        {
            return await _context.SaveChangesAsync();
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!_disposed && disposing)
            {
                _context.Dispose();
                _disposed = true;
            }
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
    }
}