using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using api.Models;
using Microsoft.EntityFrameworkCore;

namespace api.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
        {
        }

        public DbSet<Account> Accounts { get; set; }
        public DbSet<User> Users { get; set; }
        public DbSet<Owner> Owners { get; set; }
        public DbSet<Sport> Sports { get; set; }
        public DbSet<Field> Fields { get; set; }
        public DbSet<SubField> SubFields { get; set; }
        public DbSet<FieldPricing> FieldPricings { get; set; }
        public DbSet<FieldAmenity> FieldAmenities { get; set; }
        public DbSet<FieldDescription> FieldDescriptions { get; set; }
        public DbSet<FieldImage> FieldImages { get; set; }
        public DbSet<FieldService> FieldServices { get; set; }
        public DbSet<Booking> Bookings { get; set; }
        public DbSet<BookingService> BookingServices { get; set; }
        public DbSet<BookingTimeSlot> BookingTimeSlots { get; set; }
        public DbSet<Payment> Payments { get; set; }
        public DbSet<Promotion> Promotions { get; set; }
        public DbSet<Review> Reviews { get; set; }
        public DbSet<Notification> Notifications { get; set; }
        public DbSet<FavoriteField> FavoriteFields { get; set; }
        public DbSet<SearchHistory> SearchHistories { get; set; }
        public DbSet<RefreshToken> RefreshTokens { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.ApplyConfigurationsFromAssembly(typeof(ApplicationDbContext).Assembly);

            modelBuilder.Entity<Account>()
                .HasIndex(a => a.Email)
                .IsUnique();

            modelBuilder.Entity<Account>()
                .HasIndex(a => a.OAuthId)
                .IsUnique();

            modelBuilder.Entity<Promotion>()
                .HasIndex(p => p.Code)
                .IsUnique();

            modelBuilder.Entity<RefreshToken>()
                .HasIndex(rt => rt.Token)
                .IsUnique();

            modelBuilder.Entity<SubField>()
                .HasIndex(sf => sf.FieldId);

            modelBuilder.Entity<FieldPricing>()
                .HasIndex(fp => new { fp.SubFieldId, fp.StartTime, fp.EndTime });

            modelBuilder.Entity<Field>()
                .HasIndex(f => new { f.Latitude, f.Longitude });

            modelBuilder.Entity<Booking>()
                .HasIndex(b => new { b.SubFieldId, b.BookingDate, b.StartTime, b.EndTime });

            modelBuilder.Entity<FavoriteField>()
                .HasOne(ff => ff.User)
                .WithMany(u => u.FavoriteFields)
                .HasForeignKey(ff => ff.UserId)
                .OnDelete(DeleteBehavior.NoAction);

            modelBuilder.Entity<FavoriteField>()
                .HasOne(ff => ff.Field)
                .WithMany(f => f.FavoriteFields)
                .HasForeignKey(ff => ff.FieldId)
                .OnDelete(DeleteBehavior.NoAction);

            modelBuilder.Entity<Review>()
                .HasOne(r => r.User)
                .WithMany(u => u.Reviews)
                .HasForeignKey(r => r.UserId)
                .OnDelete(DeleteBehavior.NoAction);

            modelBuilder.Entity<Review>()
                .HasOne(r => r.Field)
                .WithMany(f => f.Reviews)
                .HasForeignKey(r => r.FieldId)
                .OnDelete(DeleteBehavior.NoAction);

            modelBuilder.Entity<SearchHistory>()
                .HasOne(sh => sh.User)
                .WithMany(u => u.SearchHistories)
                .HasForeignKey(sh => sh.UserId)
                .OnDelete(DeleteBehavior.NoAction);
        }
    }
}