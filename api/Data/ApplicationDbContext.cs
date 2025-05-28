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
        public DbSet<Booking> Bookings { get; set; }
        public DbSet<BookingService> BookingServices { get; set; }
        public DbSet<BookingTimeSlot> BookingTimeSlots { get; set; }
        public DbSet<FavoriteField> FavoriteFields { get; set; }
        public DbSet<Field> Fields { get; set; }
        public DbSet<FieldAmenity> FieldAmenities { get; set; }
        public DbSet<FieldDescription> FieldDescriptions { get; set; }
        public DbSet<FieldImage> FieldImages { get; set; }
        public DbSet<FieldPricing> FieldPricings { get; set; }
        public DbSet<FieldService> FieldServices { get; set; }
        public DbSet<Notification> Notifications { get; set; }
        public DbSet<Owner> Owners { get; set; }
        public DbSet<Payment> Payments { get; set; }
        public DbSet<PricingRule> PricingRules { get; set; }
        public DbSet<Promotion> Promotions { get; set; }
        public DbSet<RefreshToken> RefreshTokens { get; set; }
        public DbSet<RefundRequest> RefundRequests { get; set; }
        public DbSet<RescheduleRequest> RescheduleRequests { get; set; }
        public DbSet<Review> Reviews { get; set; }
        public DbSet<SearchHistory> SearchHistories { get; set; }
        public DbSet<Sport> Sports { get; set; }
        public DbSet<SubField> SubFields { get; set; }
        public DbSet<TimeSlot> TimeSlots { get; set; }
        public DbSet<User> Users { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.ApplyConfigurationsFromAssembly(typeof(ApplicationDbContext).Assembly);

            modelBuilder.Entity<Booking>().HasQueryFilter(b => b.DeletedAt == null);
            modelBuilder.Entity<BookingTimeSlot>().HasQueryFilter(bts => bts.DeletedAt == null);
            modelBuilder.Entity<Field>().HasQueryFilter(f => f.DeletedAt == null);
            modelBuilder.Entity<FieldPricing>().HasQueryFilter(fp => fp.DeletedAt == null);
            modelBuilder.Entity<Payment>().HasQueryFilter(p => p.DeletedAt == null);
            modelBuilder.Entity<Promotion>().HasQueryFilter(p => p.DeletedAt == null);
            modelBuilder.Entity<RefundRequest>().HasQueryFilter(rr => rr.DeletedAt == null);
            modelBuilder.Entity<RescheduleRequest>().HasQueryFilter(rr => rr.DeletedAt == null);
            modelBuilder.Entity<SearchHistory>().HasQueryFilter(sh => sh.DeletedAt == null);
            modelBuilder.Entity<User>().HasQueryFilter(u => u.DeletedAt == null);
            modelBuilder.Entity<Owner>().HasQueryFilter(o => o.DeletedAt == null);
            modelBuilder.Entity<Sport>().HasQueryFilter(s => s.DeletedAt == null);
            modelBuilder.Entity<Account>().HasQueryFilter(a => a.DeletedAt == null);
            modelBuilder.Entity<BookingTimeSlot>().HasQueryFilter(bts => bts.DeletedAt == null);
            modelBuilder.Entity<RefundRequest>().HasQueryFilter(rr => rr.DeletedAt == null);
            modelBuilder.Entity<RescheduleRequest>().HasQueryFilter(rr => rr.DeletedAt == null);
                
        }
    }
}