using System;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using DAL.Entities;

namespace DAL.Database
{
    public class AppDbContext : IdentityDbContext<User>
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
        {
        }

        public DbSet<Country> Countries { get; set; }
        public DbSet<Governorate> Governorates { get; set; }
        public DbSet<City> Cities { get; set; }
        public DbSet<UserInterest> UserInterests { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // User Configuration
            modelBuilder.Entity<User>()
     .HasOne(u => u.Country)
     .WithMany(c => c.Users)
     .HasForeignKey(u => u.CountryId)
     .OnDelete(DeleteBehavior.Restrict); 

            modelBuilder.Entity<User>()
                .HasOne(u => u.Governorate)
                .WithMany(g => g.Users)
                .HasForeignKey(u => u.GovernorateId)
                .OnDelete(DeleteBehavior.Restrict); 

            modelBuilder.Entity<User>()
                .HasOne(u => u.City)
                .WithMany()
                .HasForeignKey(u => u.CityId)
                .OnDelete(DeleteBehavior.Restrict); 

            modelBuilder.Entity<User>()
                .HasMany(u => u.UserInterests)
                .WithOne(ui => ui.User)
                .HasForeignKey(ui => ui.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            // Country Configuration
            modelBuilder.Entity<Country>()
                .HasMany(c => c.Governorates)
                .WithOne(g => g.Country)
                .HasForeignKey(g => g.CountryId)
                .OnDelete(DeleteBehavior.Cascade);

            // Governorate Configuration
            modelBuilder.Entity<Governorate>()
                .HasMany(g => g.Cities)
                .WithOne(c => c.Governorate)
                .HasForeignKey(c => c.GovernorateId)
                .OnDelete(DeleteBehavior.Cascade);

            // User Interest Configuration
            modelBuilder.Entity<UserInterest>()
                .HasOne(ui => ui.User)
                .WithMany(u => u.UserInterests)
                .HasForeignKey(ui => ui.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            // Global Query Filter for soft delete
            modelBuilder.Entity<User>()
                .HasQueryFilter(u => !u.IsDeleted);
        }
    }
}
