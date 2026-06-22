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

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Configure Trip relationships to avoid multiple cascade path issues
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

            // Configure Booking relationships
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

            // Configure One-to-Many relationship
            modelBuilder.Entity<User>()
                .HasOne(u => u.Role)
                .WithMany(r => r.Users)
                .HasForeignKey(u => u.RoleId)
                .OnDelete(DeleteBehavior.Restrict); // Prevent cascading deletes by default

            // Configure Route and Station relationships to prevent multiple cascade paths
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

            // Seed some default roles
            modelBuilder.Entity<Role>().HasData(
                new Role { Id = 1, RoleName = "Admin", RoleDescription = "Administrator with full access" },
                new Role { Id = 2, RoleName = "User", RoleDescription = "Standard User" }
            );
        }
    }
}
