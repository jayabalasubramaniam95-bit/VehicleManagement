using Microsoft.EntityFrameworkCore;
using VehicleManagement.Models;

namespace VehicleManagement.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        public DbSet<Manufacturer> Manufacturers { get; set; }
        public DbSet<Vehicle> Vehicles { get; set; }
        public DbSet<VehicleCategory> VehicleCategories  { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<Vehicle>()
                .HasOne(v => v.Manufacturer)
                .WithMany(m => m.Vehicles)
                .HasForeignKey(v => v.ManufacturerId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Vehicle>()
                .HasOne(v => v.Category)
                .WithMany(c => c.Vehicles)
                .HasForeignKey(v => v.CategoryId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Vehicle>()
                .Property(v => v.Weight)
                .HasPrecision(10, 2);

            SeedManufacturers(modelBuilder);
            SeedVehicleCategories(modelBuilder);
        }

        private static void SeedManufacturers(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Manufacturer>().HasData(
                new Manufacturer { Id = 1, Name = "Mazda" },
                new Manufacturer { Id = 2, Name = "Mercedes" },
                new Manufacturer { Id = 3, Name = "Honda" },
                new Manufacturer { Id = 4, Name = "Ferrari" },
                new Manufacturer { Id = 5, Name = "Toyota" }
            );
        }

        private static void SeedVehicleCategories(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<VehicleCategory>().HasData(
                new VehicleCategory { Id = 1, Name = "Light", MinWeight = 0, MaxWeight = 500, Size = "light" },
                new VehicleCategory { Id = 2, Name = "Medium", MinWeight = 500, MaxWeight = 2500, Size = "medium" },
                new VehicleCategory { Id = 3, Name = "Heavy", MinWeight = 2500, MaxWeight = null, Size = "heavy" }
            );
        }
    }
}