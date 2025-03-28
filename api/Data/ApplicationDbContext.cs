using Microsoft.EntityFrameworkCore;
using api.Models;
using api.Data.Configurations;

namespace api.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) 
            : base(options) { }

        public DbSet<Account> Accounts { get; set; }
        public DbSet<Booking> Bookings { get; set; }
        public DbSet<BookingService> BookingServices { get; set; }
        public DbSet<Field> Fields { get; set; }
        public DbSet<FieldAmenity> FieldAmenities { get; set; }
        public DbSet<FieldDescription> FieldDescriptions { get; set; }
        public DbSet<FieldImage> FieldImages { get; set; }
        public DbSet<Notification> Notifications { get; set; }
        public DbSet<Owner> Owners { get; set; }
        public DbSet<Payment> Payments { get; set; }
        public DbSet<Promotion> Promotions { get; set; }
        public DbSet<RefreshToken> RefreshTokens { get; set; }
        public DbSet<Review> Reviews { get; set; }
        public DbSet<FieldService> FieldServices { get; set; }
        public DbSet<Sport> Sports { get; set; }
        public DbSet<User> Users { get; set; }
        public DbSet<FavoriteField> FavoriteFields { get; set; }
        public DbSet<SubField> SubFields { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            modelBuilder.ApplyConfigurationsFromAssembly(typeof(ApplicationDbContext).Assembly);
        }
    }
}