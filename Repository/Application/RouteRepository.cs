using Microsoft.EntityFrameworkCore;
using Transport_Management_System.Data;
using Transport_Management_System.Models;
using Transport_Management_System.Repository.Interface;
using Route = Transport_Management_System.Models.Route;

namespace Transport_Management_System.Repository.Application
{
    public class RouteRepository : BaseRepository<Route>, IRouteRepository
    {
        public RouteRepository(AppDbContext context) : base(context)
        {
        }

        public async Task<(IEnumerable<Route> routes, int totalRecords)> GetRoutesPagedAsync(
            string? search,
            int? originStationId,
            int? destinationStationId,
            RouteStatus? status,
            string? city,
            double? minDistance,
            double? maxDistance,
            int pageNumber,
            int pageSize,
            string sortColumn,
            string sortDirection)
        {
            var query = _context.Routes
                .Include(r => r.OriginStation)
                .Include(r => r.DestinationStation)
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(search))
            {
                search = search.Trim().ToLower();
                query = query.Where(r => 
                    r.RouteName.ToLower().Contains(search) || 
                    r.OriginStation!.StationName.ToLower().Contains(search) || 
                    r.DestinationStation!.StationName.ToLower().Contains(search));
            }

            if (originStationId.HasValue)
            {
                query = query.Where(r => r.OriginStationId == originStationId.Value);
            }

            if (destinationStationId.HasValue)
            {
                query = query.Where(r => r.DestinationStationId == destinationStationId.Value);
            }

            if (status.HasValue)
            {
                query = query.Where(r => r.Status == status.Value);
            }

            if (!string.IsNullOrWhiteSpace(city))
            {
                city = city.Trim().ToLower();
                query = query.Where(r => 
                    r.OriginStation!.City.ToLower() == city || 
                    r.DestinationStation!.City.ToLower() == city);
            }

            if (minDistance.HasValue)
            {
                query = query.Where(r => r.DistanceKm >= minDistance.Value);
            }

            if (maxDistance.HasValue)
            {
                query = query.Where(r => r.DistanceKm <= maxDistance.Value);
            }

            var totalRecords = await query.CountAsync();

            query = sortDirection.ToLower() == "asc"
                ? sortColumn.ToLower() switch
                {
                    "name" => query.OrderBy(r => r.RouteName),
                    "origin" => query.OrderBy(r => r.OriginStation!.StationName),
                    "destination" => query.OrderBy(r => r.DestinationStation!.StationName),
                    "distance" => query.OrderBy(r => r.DistanceKm),
                    "duration" => query.OrderBy(r => r.EstimatedDurationMinutes),
                    "status" => query.OrderBy(r => r.Status),
                    _ => query.OrderBy(r => r.RouteId)
                }
                : sortColumn.ToLower() switch
                {
                    "name" => query.OrderByDescending(r => r.RouteName),
                    "origin" => query.OrderByDescending(r => r.OriginStation!.StationName),
                    "destination" => query.OrderByDescending(r => r.DestinationStation!.StationName),
                    "distance" => query.OrderByDescending(r => r.DistanceKm),
                    "duration" => query.OrderByDescending(r => r.EstimatedDurationMinutes),
                    "status" => query.OrderByDescending(r => r.Status),
                    _ => query.OrderByDescending(r => r.RouteId)
                };

            var routes = await query
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return (routes, totalRecords);
        }

        public async Task<Route?> GetRouteWithDetailsAsync(int id)
        {
            return await _context.Routes
                .Include(r => r.OriginStation)
                .Include(r => r.DestinationStation)
                .Include(r => r.IntermediateStops)
                    .ThenInclude(stop => stop.Station)
                .Include(r => r.PickupPoints)
                    .ThenInclude(pp => pp.Station)
                .Include(r => r.DropOffPoints)
                    .ThenInclude(dp => dp.Station)
                .FirstOrDefaultAsync(r => r.RouteId == id);
        }
    }
}
