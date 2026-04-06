using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using SmartStorage.Core.Entities;

namespace SmartStorage.Infrastructure.Data
{
    public class ApplicationDbContext : IdentityDbContext<IdentityUser>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        public DbSet<Client> Clients { get; set; }
        public DbSet<StorageUnit> StorageUnits { get; set; }
        public DbSet<Booking> Bookings { get; set; }
        public DbSet<Payment> Payments { get; set; }
        public DbSet<Cartage> Cartages { get; set; }
        public DbSet<Driver> Drivers { get; set; }
        public DbSet<Vehicle> Vehicles { get; set; }
        public DbSet<MaintenanceRecord> MaintenanceRecords { get; set; }
        public DbSet<Asset> Assets { get; set; }
        public DbSet<Bid> Bids { get; set; }
        public DbSet<Contract> Contracts { get; set; }
        public DbSet<Invoice> Invoices { get; set; }
        public DbSet<DeliverySchedule> DeliverySchedules { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Booking configuration
            modelBuilder.Entity<Booking>()
                .HasOne(b => b.Client)
                .WithMany(c => c.Bookings)
                .HasForeignKey(b => b.ClientId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Booking>()
                .HasOne(b => b.StorageUnit)
                .WithMany(s => s.Bookings)
                .HasForeignKey(b => b.StorageUnitId)
                .OnDelete(DeleteBehavior.Restrict);

            // Payment configuration
            modelBuilder.Entity<Payment>()
                .HasOne(p => p.Booking)
                .WithMany(b => b.Payments)
                .HasForeignKey(p => p.BookingId);

            // Cartage configuration
            modelBuilder.Entity<Cartage>()
                .HasOne(c => c.Booking)
                .WithMany(b => b.Cartages)
                .HasForeignKey(c => c.BookingId);

            modelBuilder.Entity<Cartage>()
                .HasOne(c => c.Driver)
                .WithMany(d => d.Cartages)
                .HasForeignKey(c => c.DriverId)
                .OnDelete(DeleteBehavior.SetNull);

            // Driver configuration
            modelBuilder.Entity<Driver>()
                .HasOne(d => d.AssignedVehicle)
                .WithOne()
                .HasForeignKey<Driver>(d => d.AssignedVehicleId)
                .OnDelete(DeleteBehavior.SetNull);

            // Contract configuration
            modelBuilder.Entity<Contract>()
                .HasOne(c => c.Booking)
                .WithMany()
                .HasForeignKey(c => c.BookingId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Contract>()
                .HasOne(c => c.Client)
                .WithMany()
                .HasForeignKey(c => c.ClientId)
                .OnDelete(DeleteBehavior.Restrict);

            // DeliverySchedule configuration
            modelBuilder.Entity<DeliverySchedule>()
                .HasOne(d => d.Booking)
                .WithMany()
                .HasForeignKey(d => d.BookingId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<DeliverySchedule>()
                .HasOne(d => d.Client)
                .WithMany()
                .HasForeignKey(d => d.ClientId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<DeliverySchedule>()
                .HasOne(d => d.AssignedDriver)
                .WithMany()
                .HasForeignKey(d => d.AssignedDriverId)
                .OnDelete(DeleteBehavior.SetNull);

            // Unique constraints
            modelBuilder.Entity<StorageUnit>()
                .HasIndex(s => s.UnitNumber)
                .IsUnique();

            modelBuilder.Entity<Driver>()
                .HasIndex(d => d.LicenseNumber)
                .IsUnique();

            modelBuilder.Entity<Vehicle>()
                .HasIndex(v => v.RegistrationNumber)
                .IsUnique();

            modelBuilder.Entity<Contract>()
                .HasIndex(c => c.ContractNumber)
                .IsUnique();

            modelBuilder.Entity<Invoice>()
                .HasIndex(i => i.InvoiceNumber)
                .IsUnique();

            modelBuilder.Entity<DeliverySchedule>()
                .HasIndex(d => d.ScheduleNumber)
                .IsUnique();

            // Seed initial data
            modelBuilder.Entity<StorageUnit>().HasData(
                new StorageUnit { Id = 1, UnitNumber = "A101", Size = "10x10", MonthlyRate = 100, IsActive = true, Location = "Building A", ClimateControl = "None" },
                new StorageUnit { Id = 2, UnitNumber = "A102", Size = "10x20", MonthlyRate = 180, IsActive = true, Location = "Building A", ClimateControl = "Basic" },
                new StorageUnit { Id = 3, UnitNumber = "B201", Size = "20x20", MonthlyRate = 350, IsActive = true, Location = "Building B", ClimateControl = "Premium" }
            );
        }
    }
}