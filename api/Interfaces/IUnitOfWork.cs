using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using api.Models;

namespace api.Interfaces
{
    public interface IUnitOfWork : IDisposable
    {
        IGenericRepository<Account> Accounts { get; }
        IGenericRepository<User> Users { get; }
        IGenericRepository<Owner> Owners { get; }
        IGenericRepository<RefreshToken> RefreshTokens { get; }
        IGenericRepository<FavoriteField> FavoriteFields { get; }
        IGenericRepository<Booking> Bookings { get; }
        IGenericRepository<Field> Fields { get; }
        IGenericRepository<Sport> Sports { get; }
        IGenericRepository<Review> Reviews { get; }
        IGenericRepository<FieldAmenity> FieldAmenities { get; }
        IGenericRepository<FieldService> FieldServices { get; } // Thêm
        IGenericRepository<SubField> SubFields { get; } // Thêm
        IGenericRepository<FieldImage> FieldImages { get; } // Thêm
        IGenericRepository<FieldDescription> FieldDescriptions { get; } // Thêm
        Task<int> SaveChangesAsync();
    }
}