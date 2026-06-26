using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Transport_Management_System.Data;
using Transport_Management_System.Models;
using Route = Transport_Management_System.Models.Route;

namespace Transport_Management_System.Services
{
    public class SearchService : ISearchService
    {
        private readonly AppDbContext _context;

        public SearchService(AppDbContext context)
        {
            _context = context;
        }

        public async Task<GlobalSearchResultsViewModel> SearchAllAsync(string query)
        {
            var results = new GlobalSearchResultsViewModel { Query = query };
            if (string.IsNullOrWhiteSpace(query))
                return results;

            var lowerQuery = query.ToLower().Trim();

            // 1. Search Users
            results.Users = await _context.Users
                .Include(u => u.Role)
                .Where(u => u.UserName.ToLower().Contains(lowerQuery) ||
                            u.Email.ToLower().Contains(lowerQuery) ||
                            (u.Address != null && u.Address.ToLower().Contains(lowerQuery)))
                .Take(20)
                .ToListAsync();

            // 2. Search Drivers
            results.Drivers = await _context.Drivers
                .Include(d => d.AssignedVehicle)
                .Where(d => d.FullName.ToLower().Contains(lowerQuery) ||
                            d.PhoneNumber.ToLower().Contains(lowerQuery) ||
                            (d.Email != null && d.Email.ToLower().Contains(lowerQuery)) ||
                            (d.Address != null && d.Address.ToLower().Contains(lowerQuery)) ||
                            d.LicenseNumber.ToLower().Contains(lowerQuery))
                .Take(20)
                .ToListAsync();

            // 3. Search Vehicles
            results.Vehicles = await _context.Vehicles
                .Where(v => v.VehicleName.ToLower().Contains(lowerQuery) ||
                            v.VehicleNumber.ToLower().Contains(lowerQuery))
                .Take(20)
                .ToListAsync();

            // 4. Search Stations
            results.Stations = await _context.Stations
                .Where(s => s.StationName.ToLower().Contains(lowerQuery) ||
                            s.City.ToLower().Contains(lowerQuery) ||
                            (s.District != null && s.District.ToLower().Contains(lowerQuery)) ||
                            s.Address.ToLower().Contains(lowerQuery))
                .Take(20)
                .ToListAsync();

            // 5. Search Routes
            results.Routes = await _context.Routes
                .Include(r => r.OriginStation)
                .Include(r => r.DestinationStation)
                .Where(r => r.RouteName.ToLower().Contains(lowerQuery) ||
                            r.OriginStation!.StationName.ToLower().Contains(lowerQuery) ||
                            r.DestinationStation!.StationName.ToLower().Contains(lowerQuery) ||
                            r.OriginStation.City.ToLower().Contains(lowerQuery) ||
                            r.DestinationStation.City.ToLower().Contains(lowerQuery))
                .Take(20)
                .ToListAsync();

            // 6. Search Trips
            results.Trips = await _context.Trips
                .Include(t => t.Route)
                    .ThenInclude(r => r!.OriginStation)
                .Include(t => t.Route)
                    .ThenInclude(r => r!.DestinationStation)
                .Include(t => t.Vehicle)
                .Include(t => t.Driver)
                .Where(t => t.TripName.ToLower().Contains(lowerQuery) ||
                            (t.Route != null && t.Route.RouteName.ToLower().Contains(lowerQuery)) ||
                            (t.Vehicle != null && t.Vehicle.VehicleName.ToLower().Contains(lowerQuery)) ||
                            (t.Driver != null && t.Driver.FullName.ToLower().Contains(lowerQuery)) ||
                            (t.Route != null && t.Route.OriginStation!.StationName.ToLower().Contains(lowerQuery)) ||
                            (t.Route != null && t.Route.DestinationStation!.StationName.ToLower().Contains(lowerQuery)))
                .Take(20)
                .ToListAsync();

            // 7. Search Bookings
            results.Bookings = await _context.Bookings
                .Include(b => b.User)
                .Include(b => b.Trip)
                    .ThenInclude(t => t!.Route)
                        .ThenInclude(r => r!.OriginStation)
                .Include(b => b.Trip)
                    .ThenInclude(t => t!.Route)
                        .ThenInclude(r => r!.DestinationStation)
                .Where(b => b.BookingReference.ToLower().Contains(lowerQuery) ||
                            (b.User != null && b.User.UserName.ToLower().Contains(lowerQuery)) ||
                            (b.User != null && b.User.Email.ToLower().Contains(lowerQuery)) ||
                            (b.Trip != null && b.Trip.TripName.ToLower().Contains(lowerQuery)) ||
                            (b.Trip != null && b.Trip.Route != null && b.Trip.Route.OriginStation!.StationName.ToLower().Contains(lowerQuery)) ||
                            (b.Trip != null && b.Trip.Route != null && b.Trip.Route.DestinationStation!.StationName.ToLower().Contains(lowerQuery)))
                .Take(20)
                .ToListAsync();

            // 8. Search Maintenance Records
            results.MaintenanceRecords = await _context.MaintenanceRecords
                .Include(m => m.Vehicle)
                .Where(m => (m.Vehicle != null && m.Vehicle.VehicleName.ToLower().Contains(lowerQuery)) ||
                            (m.Vehicle != null && m.Vehicle.VehicleNumber.ToLower().Contains(lowerQuery)) ||
                            (m.ServiceProvider != null && m.ServiceProvider.ToLower().Contains(lowerQuery)) ||
                            (m.Notes != null && m.Notes.ToLower().Contains(lowerQuery)))
                .Take(20)
                .ToListAsync();

            return results;
        }

        public async Task<List<SearchSuggestionDto>> GetSearchSuggestionsAsync(string query)
        {
            var list = new List<SearchSuggestionDto>();
            if (string.IsNullOrWhiteSpace(query) || query.Length < 2)
                return list;

            var lowerQuery = query.ToLower().Trim();

            // Match Vehicles
            var vehicles = await _context.Vehicles
                .Where(v => v.VehicleName.ToLower().Contains(lowerQuery) || v.VehicleNumber.ToLower().Contains(lowerQuery))
                .Take(3)
                .Select(v => new SearchSuggestionDto
                {
                    Title = $"{v.VehicleName} ({v.VehicleNumber})",
                    Category = "Vehicle",
                    Url = $"/Vehicles/Details/{v.VehicleId}"
                })
                .ToListAsync();
            list.AddRange(vehicles);

            // Match Drivers
            var drivers = await _context.Drivers
                .Where(d => d.FullName.ToLower().Contains(lowerQuery) || d.PhoneNumber.ToLower().Contains(lowerQuery))
                .Take(3)
                .Select(d => new SearchSuggestionDto
                {
                    Title = $"{d.FullName} - {d.PhoneNumber}",
                    Category = "Driver",
                    Url = $"/Drivers/Details/{d.DriverId}"
                })
                .ToListAsync();
            list.AddRange(drivers);

            // Match Routes
            var routes = await _context.Routes
                .Where(r => r.RouteName.ToLower().Contains(lowerQuery))
                .Take(3)
                .Select(r => new SearchSuggestionDto
                {
                    Title = r.RouteName,
                    Category = "Route",
                    Url = $"/Routes/Details/{r.RouteId}"
                })
                .ToListAsync();
            list.AddRange(routes);

            // Match Stations
            var stations = await _context.Stations
                .Where(s => s.StationName.ToLower().Contains(lowerQuery) || s.City.ToLower().Contains(lowerQuery))
                .Take(3)
                .Select(s => new SearchSuggestionDto
                {
                    Title = $"{s.StationName} ({s.City})",
                    Category = "Station",
                    Url = $"/Stations/Details/{s.StationId}"
                })
                .ToListAsync();
            list.AddRange(stations);

            // Match Trips
            var trips = await _context.Trips
                .Where(t => t.TripName.ToLower().Contains(lowerQuery))
                .Take(3)
                .Select(t => new SearchSuggestionDto
                {
                    Title = $"{t.TripName} (Dep: {t.DepartureDate.ToString("yyyy-MM-dd")})",
                    Category = "Trip",
                    Url = $"/Trips/Details/{t.TripId}"
                })
                .ToListAsync();
            list.AddRange(trips);

            // Match Bookings
            var bookings = await _context.Bookings
                .Where(b => b.BookingReference.ToLower().Contains(lowerQuery))
                .Take(3)
                .Select(b => new SearchSuggestionDto
                {
                    Title = $"Booking {b.BookingReference}",
                    Category = "Booking",
                    Url = $"/AdminBookings/Details/{b.BookingId}" // or Bookings/Details?
                })
                .ToListAsync();
            list.AddRange(bookings);

            return list;
        }
    }
}
