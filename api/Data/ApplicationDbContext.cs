using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using api.Data.Seeders;
using api.Models;
using Microsoft.EntityFrameworkCore;

namespace api.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options) { }
        public DbSet<Account> Accounts { get; set; }
        public DbSet<Booking> Bookings { get; set; }
        public DbSet<BookingService> BookingServices { get; set; }
        public DbSet<Field> Fields { get; set; }
        public DbSet<FieldAmenity> FieldAmenities { get; set; }
        public DbSet<FieldDescription> FieldDescriptions { get; set; }
        public DbSet<FieldImage> FieldImages { get; set; }
        public DbSet<FieldPricing> FieldPricings { get; set; }
        public DbSet<Notification> Notifications { get; set; }
        public DbSet<Owner> Owners { get; set; }
        public DbSet<Payment> Payments { get; set; }
        public DbSet<Promotion> Promotions { get; set; }
        public DbSet<RefreshToken> RefreshTokens { get; set; }
        public DbSet<Review> Reviews { get; set; }
        public DbSet<Service> Services { get; set; }
        public DbSet<Sport> Sports { get; set; }
        public DbSet<User> Users { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder); // This is required
            modelBuilder.ApplyConfigurationsFromAssembly(typeof(ApplicationDbContext).Assembly);
            AccountSeeder.Seed(modelBuilder);
            // BookingSeeder.Seed(modelBuilder);
            // BookingServiceSeeder.Seed(modelBuilder);
            FieldSeeder.Seed(modelBuilder);
            // FieldAmenitySeeder.Seed(modelBuilder);
            // FieldDescriptionSeeder.Seed(modelBuilder);
            FieldImageSeeder.Seed(modelBuilder);
            // FieldPricingSeeder.Seed(modelBuilder);
            // NotificationSeeder.Seed(modelBuilder);
            OwnerSeeder.Seed(modelBuilder);
            // PaymentSeeder.Seed(modelBuilder);
            // PromotionSeeder.Seed(modelBuilder);
            // RefreshTokenSeeder.Seed(modelBuilder);
            // ReviewSeeder.Seed(modelBuilder);
            // ServiceSeeder.Seed(modelBuilder);
            SportSeeder.Seed(modelBuilder);
            UserSeeder.Seed(modelBuilder);
        }
    }
}