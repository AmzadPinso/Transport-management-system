using Microsoft.EntityFrameworkCore;
using Transport_Management_System.Data;
using Transport_Management_System.Models;
using Transport_Management_System.Repository.Interface;

namespace Transport_Management_System.Repository.Application
{
    public class StationRepository : BaseRepository<Station>, IStationRepository
    {
        public StationRepository(AppDbContext context) : base(context)
        {
        }

        public async Task<(IEnumerable<Station> stations, int totalRecords)> GetStationsPagedAsync(
            string? search,
            bool? isActive,
            int pageNumber,
            int pageSize,
            string sortColumn,
            string sortDirection)
        {
            var query = _context.Stations.AsQueryable();

            if (!string.IsNullOrWhiteSpace(search))
            {
                search = search.Trim().ToLower();
                query = query.Where(s => 
                    s.StationName.ToLower().Contains(search) || 
                    s.City.ToLower().Contains(search) || 
                    (s.District != null && s.District.ToLower().Contains(search)) || 
                    s.Address.ToLower().Contains(search));
            }

            if (isActive.HasValue)
            {
                query = query.Where(s => s.IsActive == isActive.Value);
            }

            var totalRecords = await query.CountAsync();

            query = sortDirection.ToLower() == "asc"
                ? sortColumn.ToLower() switch
                {
                    "name" => query.OrderBy(s => s.StationName),
                    "city" => query.OrderBy(s => s.City),
                    "district" => query.OrderBy(s => s.District),
                    "status" => query.OrderBy(s => s.IsActive),
                    _ => query.OrderBy(s => s.StationId)
                }
                : sortColumn.ToLower() switch
                {
                    "name" => query.OrderByDescending(s => s.StationName),
                    "city" => query.OrderByDescending(s => s.City),
                    "district" => query.OrderByDescending(s => s.District),
                    "status" => query.OrderByDescending(s => s.IsActive),
                    _ => query.OrderByDescending(s => s.StationId)
                };

            var stations = await query
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return (stations, totalRecords);
        }
    }
}
