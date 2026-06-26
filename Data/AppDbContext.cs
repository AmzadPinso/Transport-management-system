using Microsoft.EntityFrameworkCore;
using Transport_Management_System.Models;
using Route = Transport_Management_System.Models.Route;

namespace Transport_Management_System.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
        {
        }

        // Existing tables
        public DbSet<User> Users { get; set; }
        public DbSet<Role> Roles { get; set; }
        public DbSet<Vehicle> Vehicles { get; set; }
        public DbSet<Driver> Drivers { get; set; }
        public DbSet<Station> Stations { get; set; }
        public DbSet<Route> Routes { get; set; }
        public DbSet<IntermediateStop> IntermediateStops { get; set; }
        public DbSet<PickupPoint> PickupPoints { get; set; }
        public DbSet<DropOffPoint> DropOffPoints { get; set; }
        public DbSet<Trip> Trips { get; set; }
        public DbSet<Booking> Bookings { get; set; }

        // Week 6 — Smart Features & Maintenance Module
        public DbSet<MaintenanceRecord> MaintenanceRecords { get; set; }
        public DbSet<DriverIssue> DriverIssues { get; set; }
        public DbSet<Expense> Expenses { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // ── Trip relationships ──────────────────────────────────
            modelBuilder.Entity<Trip>()
                .HasOne(t => t.Route)
                .WithMany()
                .HasForeignKey(t => t.RouteId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Trip>()
                .HasOne(t => t.Vehicle)
                .WithMany()
                .HasForeignKey(t => t.VehicleId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Trip>()
                .HasOne(t => t.Driver)
                .WithMany()
                .HasForeignKey(t => t.DriverId)
                .OnDelete(DeleteBehavior.Restrict);

            // Fix decimal precision warning for Trip.TicketPrice
            modelBuilder.Entity<Trip>()
                .Property(t => t.TicketPrice)
                .HasColumnType("decimal(18,2)");

            // ── Booking relationships ───────────────────────────────
            modelBuilder.Entity<Booking>()
                .HasOne(b => b.User)
                .WithMany()
                .HasForeignKey(b => b.UserId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Booking>()
                .HasOne(b => b.Trip)
                .WithMany()
                .HasForeignKey(b => b.TripId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Booking>()
                .HasIndex(b => b.BookingReference)
                .IsUnique();

            // Fix decimal precision warning for Booking.TotalAmount
            modelBuilder.Entity<Booking>()
                .Property(b => b.TotalAmount)
                .HasColumnType("decimal(18,2)");

            // ── User → Role relationship ────────────────────────────
            modelBuilder.Entity<User>()
                .HasOne(u => u.Role)
                .WithMany(r => r.Users)
                .HasForeignKey(u => u.RoleId)
                .OnDelete(DeleteBehavior.Restrict);

            // ── Route & Station relationships ───────────────────────
            modelBuilder.Entity<Route>()
                .HasOne(r => r.OriginStation)
                .WithMany(s => s.OriginRoutes)
                .HasForeignKey(r => r.OriginStationId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Route>()
                .HasOne(r => r.DestinationStation)
                .WithMany(s => s.DestinationRoutes)
                .HasForeignKey(r => r.DestinationStationId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<IntermediateStop>()
                .HasOne(stop => stop.Route)
                .WithMany(r => r.IntermediateStops)
                .HasForeignKey(stop => stop.RouteId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<IntermediateStop>()
                .HasOne(stop => stop.Station)
                .WithMany(s => s.IntermediateStops)
                .HasForeignKey(stop => stop.StationId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<PickupPoint>()
                .HasOne(pp => pp.Route)
                .WithMany(r => r.PickupPoints)
                .HasForeignKey(pp => pp.RouteId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<PickupPoint>()
                .HasOne(pp => pp.Station)
                .WithMany(s => s.PickupPoints)
                .HasForeignKey(pp => pp.StationId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<DropOffPoint>()
                .HasOne(dp => dp.Route)
                .WithMany(r => r.DropOffPoints)
                .HasForeignKey(dp => dp.RouteId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<DropOffPoint>()
                .HasOne(dp => dp.Station)
                .WithMany(s => s.DropOffPoints)
                .HasForeignKey(dp => dp.StationId)
                .OnDelete(DeleteBehavior.Restrict);

            // ── MaintenanceRecord relationships ─────────────────────
            modelBuilder.Entity<MaintenanceRecord>()
                .HasOne(m => m.Vehicle)
                .WithMany()
                .HasForeignKey(m => m.VehicleId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<MaintenanceRecord>()
                .Property(m => m.Cost)
                .HasColumnType("decimal(18,2)");

            // ── DriverIssue relationships ───────────────────────────
            modelBuilder.Entity<DriverIssue>()
                .HasOne(d => d.Driver)
                .WithMany()
                .HasForeignKey(d => d.DriverId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<DriverIssue>()
                .HasOne(d => d.Vehicle)
                .WithMany()
                .HasForeignKey(d => d.VehicleId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<DriverIssue>()
                .HasOne(d => d.ReportedByUser)
                .WithMany()
                .HasForeignKey(d => d.ReportedByUserId)
                .OnDelete(DeleteBehavior.SetNull);

            // ── Expense relationships ───────────────────────────────
            modelBuilder.Entity<Expense>()
                .HasOne(e => e.Vehicle)
                .WithMany()
                .HasForeignKey(e => e.VehicleId)
                .OnDelete(DeleteBehavior.SetNull);

            modelBuilder.Entity<Expense>()
                .HasOne(e => e.CreatedByUser)
                .WithMany()
                .HasForeignKey(e => e.CreatedByUserId)
                .OnDelete(DeleteBehavior.SetNull);

            modelBuilder.Entity<Expense>()
                .Property(e => e.Amount)
                .HasColumnType("decimal(18,2)");

            // ── Seed default roles ──────────────────────────────────
            modelBuilder.Entity<Role>().HasData(
                new Role { Id = 1, RoleName = "Admin", RoleDescription = "Administrator with full access" },
                new Role { Id = 2, RoleName = "User", RoleDescription = "Standard User" }
            );
        }
    }
}

