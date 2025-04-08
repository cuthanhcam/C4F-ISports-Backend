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
        IGenericRepository<FieldService> FieldServices { get; }
        IGenericRepository<SubField> SubFields { get; }
        IGenericRepository<FieldImage> FieldImages { get; }
        IGenericRepository<FieldDescription> FieldDescriptions { get; }
        IGenericRepository<BookingService> BookingServices { get; }
        Task<int> SaveChangesAsync();
    }
}