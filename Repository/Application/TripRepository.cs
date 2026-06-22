using Microsoft.EntityFrameworkCore;
using Transport_Management_System.Data;
using Transport_Management_System.Models;
using Transport_Management_System.Repository.Interface;

namespace Transport_Management_System.Repository.Application
{
    public class TripRepository : BaseRepository<Trip>, ITripRepository
    {
        public TripRepository(AppDbContext context) : base(context)
        {
        }

        public override async Task<Trip?> GetByIdAsync(int id)
        {
            return await _context.Trips
                .Include(t => t.Route)
                    .ThenInclude(r => r.OriginStation)
                .Include(t => t.Route)
                    .ThenInclude(r => r.DestinationStation)
                .Include(t => t.Vehicle)
                .Include(t => t.Driver)
                .FirstOrDefaultAsync(t => t.TripId == id);
        }

        public async Task<(IEnumerable<Trip>, int)> GetTripsPagedAsync(
            string? search,
            TripStatus? status,
            DateTime? departureDate,
            int? driverId,
            int? vehicleId,
            int pageNumber,
            int pageSize)
        {
            var query = _context.Trips
                .Include(t => t.Route)
                    .ThenInclude(r => r.OriginStation)
                .Include(t => t.Route)
                    .ThenInclude(r => r.DestinationStation)
                .Include(t => t.Vehicle)
                .Include(t => t.Driver)
                .AsQueryable();

            // Search
            if (!string.IsNullOrWhiteSpace(search))
            {
                search = search.ToLower();
                query = query.Where(t => 
                    t.TripName.ToLower().Contains(search) ||
                    t.Route.RouteName.ToLower().Contains(search) ||
                    t.Vehicle.VehicleNumber.ToLower().Contains(search) ||
                    t.Driver.FullName.ToLower().Contains(search));
            }

            // Filters
            if (status.HasValue)
            {
                query = query.Where(t => t.Status == status.Value);
            }

            if (departureDate.HasValue)
            {
                var date = departureDate.Value.Date;
                query = query.Where(t => t.DepartureDate.Date == date);
            }

            if (driverId.HasValue)
            {
                query = query.Where(t => t.DriverId == driverId.Value);
            }

            if (vehicleId.HasValue)
            {
                query = query.Where(t => t.VehicleId == vehicleId.Value);
            }

            var totalRecords = await query.CountAsync();

            var data = await query
                .OrderByDescending(t => t.DepartureDate)
                .ThenByDescending(t => t.DepartureTime)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return (data, totalRecords);
        }

        public async Task<bool> HasVehicleConflictAsync(int vehicleId, DateTime departure, DateTime arrival, int? excludeTripId = null)
        {
            var existingTrips = await _context.Trips
                .Where(t => t.VehicleId == vehicleId && 
                            t.Status != TripStatus.Cancelled && 
                            t.Status != TripStatus.Completed)
                .ToListAsync();

            if (excludeTripId.HasValue)
            {
                existingTrips = existingTrips.Where(t => t.TripId != excludeTripId.Value).ToList();
            }

            foreach (var trip in existingTrips)
            {
                var tripDeparture = trip.DepartureDate.Date.Add(trip.DepartureTime);
                var tripArrival = trip.EstimatedArrivalTime;

                // Check overlap
                if (departure < tripArrival && arrival > tripDeparture)
                {
                    return true;
                }
            }

            return false;
        }

        public async Task<bool> HasDriverConflictAsync(int driverId, DateTime departure, DateTime arrival, int? excludeTripId = null)
        {
            var existingTrips = await _context.Trips
                .Where(t => t.DriverId == driverId && 
                            t.Status != TripStatus.Cancelled && 
                            t.Status != TripStatus.Completed)
                .ToListAsync();

            if (excludeTripId.HasValue)
            {
                existingTrips = existingTrips.Where(t => t.TripId != excludeTripId.Value).ToList();
            }

            foreach (var trip in existingTrips)
            {
                var tripDeparture = trip.DepartureDate.Date.Add(trip.DepartureTime);
                var tripArrival = trip.EstimatedArrivalTime;

                // Check overlap
                if (departure < tripArrival && arrival > tripDeparture)
                {
                    return true;
                }
            }

            return false;
        }
    }
}
